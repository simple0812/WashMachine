using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WashMachine.Protocols.SimDirectives;

namespace WashMachine.Protocols.Helper
{
    public class SimWorker
    {
        public static readonly SimWorker Instance = new SimWorker();
        private SerialPortHelper serialPort;
        private BaseSimDirective LastCommand;
        private string receiveCache = "";
        private ConcurrentQueue<CompositeDirective> waitForSendDirectives;
        protected TaskCompletionSource<SimDirectiveResult> cmdEvent;

        private SimWorker()
        {
            waitForSendDirectives = new ConcurrentQueue<CompositeDirective>();

            Task.Run(async () =>
            {
                while (true)
                {
                    CompositeDirective temp = null;
                    if (waitForSendDirectives.TryDequeue(out temp))
                    {
                        if (temp == null) continue;
                        if (temp.DirectiveType == SimDirectiveType.Location)
                        {
                            await SendCommand(new CengWriteDirective());
                            var x = await SendCommand(new CengReadDirective());

                            temp.Handle(x);
                        }
                        else if (temp.DirectiveType == SimDirectiveType.HttpGet)
                        {
                            var directive = temp as HttpCompositeDirective;
                            if (directive == null) continue;
                            await SendCommand(new CregDirective());
                            var x = await SendCommand(new HttpBearerQueryDirective());
                            if (!x.Status)//bearer错误 需要关闭后在打开
                            {
                                await SendCommand(new HttpBearerCloseDirective());
                                await SendCommand(new HttpBearerOpenDirective());
                            }
                            else if (!x.IsExecOk)
                            {
                                await SendCommand(new HttpBearerOpenDirective());
                            }
                            await SendCommand(new HttpInitDirective());
                            await SendCommand(new HttpParaUrlDirective(directive.Url));
                            await SendCommand(new HttpParaCidDirective());
                            await SendCommand(new HttpActionGetDirective());

                            var p = await SendCommand(new HttpReadDirective());
                            await SendCommand(new HttpTermDirective());

                            temp.Handle(p);
                        }
                    }

                    await Task.Delay(10);
                }
            });

            serialPort = SerialCreater.Instance.Create("Silicon Labs CP210x USB to UART Bridge (COM4)");
            serialPort.ReceiveHandler += SerialPort_ReceiveHandler;
        }

        private void SerialPort_ReceiveHandler(byte[] obj)
        {
            Debug.WriteLine(Encoding.UTF8.GetString(obj));
            //需要判断是否包含服务端的回复信息 如：hello 回 world
            var str = Encoding.UTF8.GetString(obj);
            if (str.IndexOf("AT+", StringComparison.Ordinal) == 0)
            {
                receiveCache = str;
            }
            else
            {
                receiveCache += str;
            }

            if (LastCommand == null)
            {
                Debug.WriteLine("LastCommand is null");
                return;
            }

            Debug.WriteLine("LastCommand->" + LastCommand.DirectiveText);
            if (LastCommand?.isEnd(receiveCache) ?? false)
            {
                if (!cmdEvent.Task.IsCompleted && !cmdEvent.Task.IsCanceled)
                {
                    cmdEvent.TrySetResult(LastCommand?.Process(receiveCache));
                }
            }
        }
        
        public void Enqueue(CompositeDirective directive)
        {
            if(waitForSendDirectives.Count < 500)
                waitForSendDirectives.Enqueue(directive);
        }

        public async Task SendData(string msg)
        {
            var x = Encoding.UTF8.GetBytes(msg);
            var p = new CancellationTokenSource();
            await serialPort.Open();
            await serialPort.Send(x.Concat(new byte[] { 0x0D, 0x0A }).ToArray(), p.Token);
        }

        //发送at指令
        public async Task<SimDirectiveResult> SendCommand(BaseSimDirective cmd, int timeout = 2000)
        {
            await Task.Delay(5);//防止指令发送太密集
            cmdEvent = new TaskCompletionSource<SimDirectiveResult>();
            LastCommand = cmd;
            await SendData(cmd.DirectiveText);
            var p = cmdEvent.Task;

            var cancellationToken = new CancellationTokenSource(timeout).Token;
            var cancellationCompletionSource = new TaskCompletionSource<SimDirectiveResult>();

            using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(new SimDirectiveResult(false, "timeout"))))
            {
                var t = await Task.WhenAny(p, cancellationCompletionSource.Task);
                if (p != t)
                {
                    cmdEvent.TrySetResult(new SimDirectiveResult(false, "timeout"));
                }
                receiveCache = "";
                LastCommand = null;
                return t.Result;
            }
           
        }
    }
}
