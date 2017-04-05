using System;
using System.Threading.Tasks;
using WashMachine.Protocols.Helper;

namespace WashMachine.Protocols.SimDirectives
{
    public abstract class CompositeDirective
    {
        private int _retryTimes = 0;
        public abstract SimDirectiveType DirectiveType { get; }
        public Action<SimDirectiveResult> SuccessHandler { get; set; }

        public async void Handle(SimDirectiveResult result)
        {
            if (result.IsExecOk && result.Status)
            {
                SuccessHandler?.Invoke(result);
            }

            else if (_retryTimes < 5)
            {
                this._retryTimes++;
                await Task.Delay(500);
                SimWorker.Instance.Enqueue(this);
            }
            else
            {
            }
        }
    }
}
