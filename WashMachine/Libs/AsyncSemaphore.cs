using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WashMachine.Libs
{
    public class AsyncSemaphore
    {
        // 维护一个已经完成的Task以供重用提高效率
        private static readonly Task s_completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> m_waiters
                                    = new Queue<TaskCompletionSource<bool>>();
        // 跟踪信号量的当前计数，以便我们知道在开始阻塞之前还有多少等待者可以完成
        private int m_currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            m_currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (m_waiters)
            {
                if (m_currentCount > 0)
                {
                    --m_currentCount;
                    return s_completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    m_waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (m_waiters)
            {
                if (m_waiters.Count > 0)
                    toRelease = m_waiters.Dequeue();
                else
                    ++m_currentCount;
            }
            toRelease?.SetResult(true);
        }
    }
}
