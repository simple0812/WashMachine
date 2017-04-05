using System.Threading;
using System.Threading.Tasks;

namespace WashMachine.Libs
{
    public class AsyncManualResetEvent<T>
    {
        private volatile TaskCompletionSource<T> m_tcs = new TaskCompletionSource<T>();

        public Task<T> WaitAsync() { return m_tcs.Task; }

        public void Set(T t)
        {
            var tcs = m_tcs;
            Task.Factory.StartNew(s => ((TaskCompletionSource<T>)s).TrySetResult(t), tcs
               , CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            tcs.Task.Wait();
        }

        public void Reset()
        {
            while (true)
            {
                var tcs = m_tcs;
                // 短逻辑单元 确保如果当前Task已经完成就切换一个新的Task。
                if (!tcs.Task.IsCompleted ||
                    Interlocked.CompareExchange(ref m_tcs, new TaskCompletionSource<T>(), tcs) == tcs)
                    return;
            }
        }
    }
}
