using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>This came from a blog post by Stephen Toub:
    /// <a href="http://blogs.msdn.com/b/pfxteam/archive/2013/01/13/cooperatively-pausing-async-methods.aspx#">
    /// Cooperatively pausing async methods</a></summary>
    public class PauseTokenSource
    {
        internal static readonly Task s_runningTask = Task.FromResult(true);

        private volatile TaskCompletionSource<bool> m_paused;

        public bool IsPaused
        {
            get { return m_paused != null; }
            set
            {
                if (value)
                {
#pragma warning disable 420
                    Interlocked.CompareExchange(
                        ref m_paused, new TaskCompletionSource<bool>(), null);
#pragma warning restore 420
                }
                else
                {
                    while (true)
                    {
                        var tcs = m_paused;
                        if (tcs == null) return;
#pragma warning disable 420
                        if (Interlocked.CompareExchange(ref m_paused, null, tcs) == tcs)
                        {
                            tcs.SetResult(true);
                            break;
                        }
#pragma warning restore 420
                    }
                }
            }
        }

        public PauseToken Token { get { return new PauseToken(this); } }

        internal Task WaitWhilePausedAsync()
        {
            var cur = m_paused;
            return cur != null ? cur.Task : s_runningTask;
        }
    }

    public struct PauseToken
    {
        private readonly PauseTokenSource m_source;

        internal PauseToken(PauseTokenSource source)
        {
            m_source = source;
        }

        public bool IsPaused { get { return m_source != null && m_source.IsPaused; } }

        public Task WaitWhilePausedAsync()
        {
            return IsPaused ?
                m_source.WaitWhilePausedAsync() :
                PauseTokenSource.s_runningTask;
        }
    }
}
