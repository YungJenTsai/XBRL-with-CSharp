using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>An async semaphore-like implementation. This was written by Stephen Toub of MSFT.
    /// <a href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx">Building Async
    /// Coordination Primitives, Part 5: AsyncSemaphore</a> From: Parallel Programming with .NET" blog.</summary>
    public class AsyncSemaphore
    {
        private int currentCount;

        private readonly Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();

        private readonly static Task completed = Task.FromResult(true);

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
            currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (waiters)
            {
                if (currentCount > 0)
                {
                    --currentCount;
                    return completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (waiters)
            {
                if (waiters.Count > 0)
                    toRelease = waiters.Dequeue();
                else
                    ++currentCount;
            }
            if (toRelease != null)
                toRelease.SetResult(true);
        }
    }
}
