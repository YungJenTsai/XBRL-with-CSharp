using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>An async lock implementation. This was written by Stephen Toub of MSFT.
    /// <a href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx">
    /// Building Async Coordination Primitives, Part 6: AsyncLock</a>
    /// From: "Parallel Programming with .NET" blog.</summary>
    public class AsyncLock
    {
        private readonly AsyncSemaphore semaphore;

        private readonly Task<Releaser> releaser;

        public AsyncLock()
        {
            semaphore = new AsyncSemaphore(1);
            releaser = Task.FromResult(new Releaser(this));
        }

        protected internal AsyncSemaphore Semaphore
        {
            get { return semaphore; }
        }

        public Task<Releaser> LockAsync()
        {
            var wait = semaphore.WaitAsync();
            return wait.IsCompleted ?
                releaser :
                wait.ContinueWith((_) => new Releaser((AsyncLock)this),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }

    public struct Releaser : IDisposable
    {
        internal Releaser(AsyncLock toRelease)
        {
            this.toRelease = toRelease;
        }

        private readonly AsyncLock toRelease;

        public void Dispose()
        {
            if (toRelease != null)
                toRelease.Semaphore.Release();
        }
    }
}
