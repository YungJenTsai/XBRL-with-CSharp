using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TT.XBRLProcessor
{
    internal struct VoidTypeStruct { }

    public static class TaskExtensions
    {
        #region IEnumerable<Task<T>>.OrderByCompletion

        /* This came out of the EduAsync series of blog posts by Jon Skeet. */

        /// <summary>Returns a sequence of tasks which will be observed to complete with the same set of 
        /// results as the given input tasks, but in the order in which the original tasks complete.</summary>
        public static IEnumerable<Task<T>> OrderByCompletion<T>(this IEnumerable<Task<T>> inputTasks)
        {
            // Copy the input so we know it'll be stable, and we don't evaluate it twice
            var inputTaskList = inputTasks.ToList();

            // Could use Enumerable.Range here, if we wanted...
            var completionSourceList = new List<TaskCompletionSource<T>>(inputTaskList.Count);
            for (int i = 0; i < inputTaskList.Count; i++)
            {
                completionSourceList.Add(new TaskCompletionSource<T>());
            }

            // At any one time, this is "the index of the box we've just filled".
            // It would be nice to make it nextIndex and start with 0, but Interlocked.Increment
            // returns the incremented value...
            int prevIndex = -1;

            // We don't have to create this outside the loop, but it makes it clearer
            // that the continuation is the same for all tasks.
            Action<Task<T>> continuation = completedTask =>
            {
                int index = Interlocked.Increment(ref prevIndex);
                var source = completionSourceList[index];
                PropagateResult(completedTask, source);
            };

            foreach (var inputTask in inputTaskList)
            {
                // TODO: Work out whether TaskScheduler.Default is really the right one to use.
                inputTask.ContinueWith(continuation,
                                       CancellationToken.None,
                                       TaskContinuationOptions.ExecuteSynchronously,
                                       TaskScheduler.Default);
            }

            return completionSourceList.Select(source => source.Task);
        }

        /// <summary>Propagates the status of the given task (which must be 
        /// completed) to a task completion source (which should not be).</summary>
        private static void PropagateResult<T>(Task<T> completedTask,
            TaskCompletionSource<T> completionSource)
        {
            switch (completedTask.Status)
            {
                case TaskStatus.Canceled:
                    completionSource.TrySetCanceled();
                    break;
                case TaskStatus.Faulted:
                    completionSource.TrySetException(completedTask.Exception.InnerExceptions);
                    break;
                case TaskStatus.RanToCompletion:
                    completionSource.TrySetResult(completedTask.Result);
                    break;
                default:
                    // TODO: Work out whether this is really appropriate. Could set
                    // an exception in the completion source, of course...
                    completionSource.TrySetException(new ArgumentException("Task was not completed"));
                    break;
            }
        }

        #endregion

        #region Task.Then

        /* These are from an article by Stephen Toub. */

        public static async Task Then(this Task antecedent, Action continuation)
        {
            await antecedent;
            continuation();
        }

        public static async Task<TNewResult> Then<TNewResult>(this Task antecedent, Func<TNewResult> continuation)
        {
            await antecedent;
            return continuation();
        }

        public static async Task Then<TResult>(this Task<TResult> antecedent, Action<TResult> continuation)
        {
            continuation(await antecedent);
        }

        public static async Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> antecedent, Func<TResult, TNewResult> continuation)
        {
            return continuation(await antecedent);
        }

        public static async Task Then(this Task task, Func<Task> continuation)
        {
            await task;
            await continuation();
        }

        public static async Task<TNewResult> Then<TNewResult>(this Task task, Func<Task<TNewResult>> continuation)
        {
            await task;
            return await continuation();
        }

        public static async Task Then<TResult>(this Task<TResult> task, Func<TResult, Task> continuation)
        {
            await continuation(await task);
        }

        public static async Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task, Func<TResult, Task<TNewResult>> continuation)
        {
            return await continuation(await task);
        }

        #endregion

        #region Task.TimeoutAfter

        /* Task.TimeoutAfter extension methods: Based on sample code by Joe Hoag of Microsoft, from the article at
         * http://blogs.msdn.com/b/pfxteam/archive/2011/11/10/10235834.aspx, Crafting a Task.TimeoutAfter Method.
         * I did not change the original code; only added the overloads that take a TimeSpan argument.
         * 
         * Note that the returned Task on a timeout is set to the faulted state with a TimeoutException, so when
         * calling this you need to catch the TimeoutException, when it times out. */

        public static Task TimeoutAfter(this Task task, TimeSpan timeoutSpan)
        {
            return task.TimeoutAfter((int)timeoutSpan.TotalMilliseconds);
        }

        public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeoutSpan)
        {
            return task.TimeoutAfter((int)timeoutSpan.TotalMilliseconds) as Task<TResult>;
        }

        public static Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            // Short-circuit #1: infinite timeout or task already completed
            if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
            {
                // Either the task has already completed or timeout will never occur.
                // No proxy necessary.
                return task;
            }

            // tcs.Task will be returned as a proxy to the caller
            TaskCompletionSource<VoidTypeStruct> tcs = new TaskCompletionSource<VoidTypeStruct>();

            // Short-circuit #2: zero timeout
            if (millisecondsTimeout == 0)
            {
                // We've already timed out.
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            // Set up a timer to complete after the specified timeout period
            Timer timer = new Timer(state =>
            {
                // Recover your state information
                // Fault our proxy with a TimeoutException
                ((TaskCompletionSource<VoidTypeStruct>)state).TrySetException(new TimeoutException());
            }, tcs, millisecondsTimeout, Timeout.Infinite);

            // Wire up the logic for what happens when source task completes
            task.ContinueWith((antecedent, state) =>
            {
                // Recover our state data
                var tuple = (Tuple<Timer, TaskCompletionSource<VoidTypeStruct>>)state;

                // Cancel the Timer
                tuple.Item1.Dispose();

                // Marshal results to proxy
                MarshalTaskResults(antecedent, tuple.Item2);
            }, Tuple.Create(timer, tcs), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return tcs.Task;
        }

        public static Task TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout)
        {
            // Short-circuit #1: infinite timeout or task already completed
            if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
            {
                // Either the task has already completed or timeout will never occur.
                // No proxy necessary.
                return task;
            }

            // tcs.Task will be returned as a proxy to the caller
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();

            // Short-circuit #2: zero timeout
            if (millisecondsTimeout == 0)
            {
                // We've already timed out.
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            // Set up a timer to complete after the specified timeout period
            Timer timer = new Timer(state =>
            {
                // Recover your state information
                // Fault our proxy with a TimeoutException
                ((TaskCompletionSource<TResult>)state).TrySetException(new TimeoutException());
            }, tcs, millisecondsTimeout, Timeout.Infinite);

            // Wire up the logic for what happens when source task completes
            task.ContinueWith((antecedent, state) =>
            {
                // Recover our state data
                var tuple = (Tuple<Timer, TaskCompletionSource<TResult>>)state;

                // Cancel the Timer
                tuple.Item1.Dispose();

                // Marshal results to proxy
                MarshalTaskResults(antecedent, tuple.Item2);
            }, Tuple.Create(timer, tcs), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return tcs.Task;
        }

        internal static void MarshalTaskResults<TResult>(Task source, TaskCompletionSource<TResult> proxy)
        {
            switch (source.Status)
            {
                case TaskStatus.Faulted:
                    proxy.TrySetException(source.Exception);
                    break;
                case TaskStatus.Canceled:
                    proxy.TrySetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    Task<TResult> castedSource = source as Task<TResult>;
                    proxy.TrySetResult(
                        castedSource == null ? default(TResult) : // source is a Task
                            castedSource.Result);                 // source is a Task<TResult>
                    break;
            }
        }

        #endregion

        #region Task.YieldTo

        public static Task YieldTo(this Task task, TaskScheduler scheduler)
        {
            return task.ContinueWith(antecedent =>
            {
                Task.Yield();
                return antecedent.ConfigureAwait(false);
            }, scheduler);
        }

        #endregion

        #region IEnumerable<T>.ForEachAsync and IEnumerable<T>.ForAsync

        /* Technically, these two should be declared in CollectionExtensions, but this method is used by Zipper.exe.
         * Zipper can add a reference to this file, but not to CollectionExtensions.cs, which references System.Windows.Forms */

        /// <summary>A ForEachAsync implementation. Based on a sample in an article by Stephen Toub,
        /// <a href="http://blogs.msdn.com/b/pfxteam/archive/2012/03/05/10278165.aspx">
        /// Implementing a simple ForEachAsync, part 2</a>.</summary>
        /// <remarks>
        /// I've changed this from calling <b>Task.Run</b> to call <b>Task.Factory.StartNew</b> in order to have it use my 
        /// custom ParallelTaskScheduler rather than the default. (This was intended to be an experimental change, but I 
        /// decided to leave it like this.)</remarks>
        /// Changed on 3/29/2018
        /// Removed the tasks
        public static Task ForEachAsync<T>(this IEnumerable<T> source,
            int maxDegreeOfParallelism,
            Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(maxDegreeOfParallelism)
                select Task.Run(async () =>
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }

        /// <summary>An asynchronous ForAsync implementation.</summary>
        /// <remarks>It simply creates an <b>Enumerable.Range</b> and wraps <b>ForEachAsync</b>.</remarks>
        public static Task ForAsync(int fromInclusive, int toExclusive, int maxDegreeOfParallelism, Func<int, Task> body)
        {
            return Enumerable.Range(
                fromInclusive, toExclusive).
                ForEachAsync(maxDegreeOfParallelism, async i => await body(i));
        }

        #endregion
    }
}
