using System;
using System.Threading;
using System.Threading.Tasks;

namespace WashMachine.Libs
{
    public class AsyncLock
    {
        private readonly AsyncSemaphore m_semaphore;
        // 缓存Task<Releaser>实例，当访问的锁没有竞争时，可以直接返回从而避免不必要的分配。
        private readonly Task<Releaser> m_releaser;

        public AsyncLock()
        {
            // 信号量为1，用于实现互斥
            m_semaphore = new AsyncSemaphore(1);
            m_releaser = Task.FromResult(new Releaser(this));
        }

        public Task<Releaser> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                m_releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state)
                , this, CancellationToken.None
                , TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;
            internal Releaser(AsyncLock toRelease)
            {
                m_toRelease = toRelease;
            }
            // using块生成的finally块调用IDisposable接口的Dispose()方法
            public void Dispose()
            {
                m_toRelease?.m_semaphore.Release();
            }
        }
    }
}
