using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WashMachine.Enums;
using WashMachine.Libs;
using WashMachine.Models;
using WashMachine.Protocols.Directives;
using WashMachine.Protocols.Enums;

namespace WashMachine.Protocols.Helper
{
    public sealed class DirectiveWorker : IDisposable
    {
        private ConcurrentQueue<WaitForSend> waitForSendDirectives;
        private List<byte> DirtyDirective; 
        private ConcurrentDictionary<int, WaitForFeedBack> waitForFeedbackDirectives;
        private CancellationTokenSource cancelTokenSource;

        private IProtocol protocolProvider = ProtocolFactory.Create(ProtocolVersion.V485_1);

        private static object _locker = new object();
        private bool _isSort = true;
        private static object parse_locker = new object();
        private bool isRetry = true;
        private SerialPortHelper serialPort;

        private DirectiveWorker()
        {
            
            Init();
           
        }

        public void Init()
        {
            waitForSendDirectives = new ConcurrentQueue<WaitForSend>();
            DirtyDirective = new List<byte>();
            waitForFeedbackDirectives = new ConcurrentDictionary<int, WaitForFeedBack>();

            cancelTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                await DispatchDirective(cancelTokenSource.Token);
            });

            Task.Run(async () =>
            {
                await RetrySend(cancelTokenSource.Token);
            });
        }

        private void SpHelper_ReceiveHandler(byte[] data)
        {
            parseResultAndNotify(data);
        }

        public void SetIsRtry(bool retry)
        {
            isRetry = retry;
        }

        private static volatile DirectiveWorker _instance = null; 
        private static readonly object instance_locker = new object();
        public static DirectiveWorker Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (instance_locker)
                    {
                        if (_instance == null)
                            _instance = new DirectiveWorker();
                    }
                }
                return _instance;
            }
        }

        public delegate void SerialPortEventHandler(SerialPortEventArgs args);
        public event SerialPortEventHandler SerialPortEvent;
        public event Action<CustomException, BaseDirective> ErrorEvent;

        public void OnErrorEvent(CustomException args, BaseDirective directive)
        {
            ErrorEvent?.Invoke(args, directive);
            Dispose();
        }

        public void OnSerialPortEvent(SerialPortEventArgs args)
        {
            SerialPortEvent?.Invoke(args);
        }

        //指令排序：优先按照超时时间多少排列(比如大于3秒为超时，然后根据超过的时间量排序) 然后按照指令优先级排列
        public void PrepareDirective(BaseDirective item, int reSendTimes = 0)
        {
            lock (_locker)
            {
                _isSort = false;
                if(waitForSendDirectives.Count <= 100)
                    waitForSendDirectives.Enqueue(new WaitForSend(DateTime.Now, item, reSendTimes));
            }   
        }

        private async Task DispatchDirective(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                while (!cancelTokenSource.IsCancellationRequested)
                {
                    WaitForSend tuple = null;
                    bool isout = false;
                    lock (_locker)
                    {
                        if (!_isSort)
                        {
                            var list = waitForSendDirectives.ToList();
                            var now = DateTime.Now;
                            for (int i = 0, len = list.Count; i < len; i++)
                            {
                                var span = (now - list[i].CreatedAt).TotalMilliseconds;
                                list[i].TimeOut = span > 1000 ? span : 0 ;
                            }

                            waitForSendDirectives =
                                new ConcurrentQueue<WaitForSend>(
                                    list.OrderByDescending(p => p.RetrySendTimes).ThenByDescending(p => p.TimeOut).ThenBy(p => p.Directive.Priority));

                            _isSort = true;
                        }

                        if (waitForFeedbackDirectives.Count == 0)
                        {
                            isout = waitForSendDirectives.TryDequeue(out tuple);
                        }
                    }

                    if (isout && tuple != null)
                    {
                        var item = tuple.Directive;
                        try
                        {
                            await Send(item, tuple.RetrySendTimes);
                        }
                        catch (CustomException ce)
                        {
                            Debug.WriteLine("..........");
                            OnErrorEvent(ce, item);
                        }
                        catch (TaskCanceledException)
                        {
                            Debug.WriteLine("feedbackSource cancel");
                            OnErrorEvent(new CustomException($"10重试发送指令失败", this.GetType().FullName,
                                ExceptionPriority.Unrecoverable), item);
                            return;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(".........XXXXXXXXXXX.");
                            OnErrorEvent(new CustomException(ex.Message, this.GetType().FullName,
                                ExceptionPriority.Unrecoverable), item);
                            return;
                        }
                    }

                    await Task.Delay(50, token);
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("DispatchDirective cancel");
            }
        }

        

        public async Task Send(BaseDirective item, int  reSendTimes = 0)
        {
            try
            {
                waitForFeedbackDirectives.TryAdd(item.DirectiveId, new WaitForFeedBack(DateTime.Now, item, reSendTimes));

                var directiveData = protocolProvider.GenerateDirectiveBuffer(item);
                //第一次加载
                if (serialPort == null)
                {
                    serialPort = await getSerialPort();
                }

                if (serialPort != null)
                {
                    // 运行过程中串口断开了
                    if (serialPort.serialPort == null)
                    {
                        serialPort.ReceiveHandler -= SpHelper_ReceiveHandler;
                        serialPort = await getSerialPort();
                    }

                    if (waitForFeedbackDirectives.ContainsKey(item.DirectiveId) && serialPort != null)
                        await serialPort.Send(directiveData, cancelTokenSource.Token);
                }
            }
            catch (CustomException e)
            {
                Debug.WriteLine("send error->" + e.Message );
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("send cancel");
            }
            catch (Exception e)
            {
                OnErrorEvent(new CustomException(e.Message + "Send", this.GetType().FullName,
                    ExceptionPriority.Unrecoverable), item);
            }
        }

        private async Task<SerialPortHelper> getSerialPort()
        {
            serialPort = await SerialCreater.Instance.Create(SerialEnum.LowerComputer);
            if (serialPort != null)
            {
                serialPort.ReceiveHandler += SpHelper_ReceiveHandler;
            }
            else
            {
                Debug.WriteLine("LowerComputer is null");

            }

            return serialPort;
        }

        private void HandleFeedback(DirectiveResult recvData, byte[] directiveBytes)
        {

            WaitForFeedBack metadata = null;

            if (!waitForFeedbackDirectives.ContainsKey(recvData.Data.DirectiveId))
            {
                Debug.WriteLine($"waitForFeedbackDirectives not ContainsKey {recvData.Data.DirectiveId}");
                return;
            }

            var feedback = waitForFeedbackDirectives[recvData.Data.DirectiveId];

            //修正同一个指令id，发送和反馈对应的设备不同或者类型不同
            if (feedback?.Directive == null || feedback.Directive.TargetDeviceId != recvData.Data.DeviceId
                || feedback.Directive.DeviceType != recvData.Data.DeviceType)
            {
                Debug.WriteLine("send and feedback not match");
                return;
            }

            if (waitForFeedbackDirectives.TryRemove(recvData.Data.DirectiveId, out metadata) && null != metadata)
            {
                var args = new SerialPortEventArgs
                {
                    IsSucceed = true,
                    Result = recvData,
                    Command = directiveBytes
                };

                OnSerialPortEvent(args);
            }
            else
            {
                Debug.WriteLine($"waitForFeedbackDirectives TryRemove {recvData.Data.DirectiveId} failed");
            }

        }

        //只重发成功发送的指令
        private async Task RetrySend(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                while (!cancelTokenSource.IsCancellationRequested)
                {
                    if (waitForFeedbackDirectives.Count != 0)
                    {
                        var waitForFeedback = waitForFeedbackDirectives.Values.FirstOrDefault();
                        if (waitForFeedback != null)
                        {
                            var now = DateTime.Now;
                            if ((now - waitForFeedback.CreatedAt).TotalMilliseconds >= 10)
                            {
                                var p =
                                    Common.BytesToString(
                                        protocolProvider.GenerateDirectiveBuffer(waitForFeedback.Directive));


                                if (waitForFeedback.RetrySendTimes < 10)
                                {

                                    Debug.WriteLine(
                                            $"{waitForFeedback.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss.fff")}<-timeout->{now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

                                    WaitForFeedBack metadata = null;

                                    if (isRetry)
                                    {
                                        Debug.WriteLine(
                                                $"{waitForFeedbackDirectives.Count}count,{waitForFeedback.RetrySendTimes}times retry ->{p}<- retry end");

                                        PrepareDirective(waitForFeedback.Directive, ++waitForFeedback.RetrySendTimes);
                                    }
                                    else
                                    {
                                        OnSerialPortEvent(new SerialPortEventArgs() {IsSucceed = false, Message = waitForFeedback.Directive.TargetDeviceId.ToString() });
                                    }

                                    waitForFeedbackDirectives.TryRemove(waitForFeedback.Directive.DirectiveId,
                                        out metadata);
                                }
                                else
                                {
                                    Debug.WriteLine(
                                            $"{waitForFeedbackDirectives.Count}count,{p}, {waitForFeedback.RetrySendTimes}times retry failed");

                                    OnErrorEvent(new CustomException($"10重试发送指令失败", this.GetType().FullName,
                                            ExceptionPriority.Unrecoverable), waitForFeedback.Directive);
                                    return;

                                }
                            }
                        }

                    }

                    await Task.Delay(1500, token);
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("RetrySend cancel");
            }
        }

        private byte[] GetRawDirective(byte[] b)
        {
            if (b.Length <= 2) return b;
            var len = ((DirectiveTypeEnum)b[1]).GetFeedbackLength();

            return b.Skip(0).Take(len).ToArray();
        }

        //解析指令，如果成功则清空DirtyData
        //如果失败则先判断是否上一次是否也解析失败 如果没有则将该次指令保存到DirtyData
        //如果有则将该次指令与DirtyData合并在一起解析 如果仍然失败 则清空DirtyData
        private void parseResultAndNotify(byte[] b)
        {
            lock (parse_locker)
            {
                var recvData = protocolProvider.ResolveFeedback(GetRawDirective(b));

                if (null == recvData || !recvData.Status)
                {
                    if (DirtyDirective.Any())
                    {
                        DirtyDirective.AddRange(b);
                        var newBytes = DirtyDirective.ToArray();
                        var newData = protocolProvider.ResolveFeedback(GetRawDirective(newBytes));
                        if (null == newData || !newData.Status)
                        {
                            //如果DirtyDirective不是有效的指令则清空
                            if (DirtyDirective.Count > 2)
                            {
                                var len = ((DirectiveTypeEnum)DirtyDirective[1]).GetFeedbackLength();
                                if (len == 0 || len <= DirtyDirective.Count)
                                {
                                    DirtyDirective.Clear();
                                    Debug.WriteLine(".....recvData is error.....");
                                    DirtyDirective.AddRange(b);
                                }
                            }
                        }
                        else
                        {
                            DirtyDirective.Clear();
                            HandleFeedback(newData, newBytes);
                        }
                    }
                    else
                    {
                        DirtyDirective.AddRange(b);
                    }
                }
                else
                {
                    DirtyDirective.Clear();
                    HandleFeedback(recvData, b);
                }
            }
        }

        public void Cancel()
        {
            if (cancelTokenSource != null)
            {
                if (!cancelTokenSource.IsCancellationRequested)
                {
                    cancelTokenSource.Cancel();
                }
            }

            if (serialPort != null)
            {
                serialPort.ReceiveHandler -= SpHelper_ReceiveHandler;
            }
            
            waitForFeedbackDirectives.Clear();
        }

        public void Dispose()
        {
            Cancel();
            _instance = null;
        }
    }

    public class SerialPortEventArgs : EventArgs
    {
        public bool IsSucceed { get; set; }
        public DirectiveResult Result { get; set; }
        public string Message { get; set; }
        public byte[] Command { get; set; }
    }

    public class WaitForFeedBack
    {
        public DateTime CreatedAt { get; set; }
        public BaseDirective Directive { get; set; }
        public bool IsSendSuccess { get; set; } = false;
        public int RetrySendTimes { get; set; } = 0;

        public WaitForFeedBack(DateTime time, BaseDirective directive, int reSendTimes)
        {
            CreatedAt = time;
            Directive = directive;
            RetrySendTimes = reSendTimes;
        }
    }

    public class WaitForSend
    {
        public DateTime CreatedAt { get; set; }
        public BaseDirective Directive { get; set; }
        public double TimeOut { get; set; } = 0;
        public int RetrySendTimes { get; set; } = 0;

        public WaitForSend(DateTime time, BaseDirective directive, int reSendTimes = 0)
        {
            CreatedAt = time;
            Directive = directive;
            RetrySendTimes = reSendTimes;
        }
    }
}
