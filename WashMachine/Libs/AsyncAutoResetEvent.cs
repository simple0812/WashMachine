using System.Collections.Generic;
using System.Threading.Tasks;

namespace WashMachine.Libs
{
    public class AsyncAutoResetEvent<T> where T: new() 
    {
        // 保存一个成功完成的 Task<TResult>，供重用以提高性能
        private static readonly Task<T> s_completed = Task.FromResult<T>(new T());
        // 等待任务队列
        private readonly Queue<TaskCompletionSource<T>> m_waits = new Queue<TaskCompletionSource<T>>();
        // 用于跟踪 信号到达时可能没有等待者 的情况，将AsyncAutoResetEvent的初始状态设置为有信号
        private bool m_signaled;

        public Task<T> WaitAsync()
        {
            lock (m_waits)
            {
                if (m_signaled)
                {
                    m_signaled = false;
                    return s_completed;
                }
                else
                {
                    var tcs = new TaskCompletionSource<T>();
                    m_waits.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }

        public void Set(T t)
        {
            TaskCompletionSource<T> toRelease = null;
            lock (m_waits)
            {
                if (m_waits.Count > 0)
                    toRelease = m_waits.Dequeue();
                else if (!m_signaled)
                    m_signaled = true;
            }
            toRelease?.SetResult(t);
        }
    }
}
