using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WashMachine.Enums;
using WashMachine.Protocols.Helper;
using WashMachine.Protocols;
using WashMachine.Protocols.Directives;
using WashMachine.Protocols.Enums;
using WashMachine.Libs;
using WashMachine.Models;

namespace WashMachine.Devices
{
    public abstract class DeviceBase 
    {
        public virtual TargetDeviceTypeEnum DeviceType { get; set; }
        public virtual int DeviceId { get; set; }

        protected Timer loopTimer;
        protected readonly IProtocol protocolProvider = ProtocolFactory.Create(ProtocolVersion.V485_1);

        protected AsyncManualResetEvent<DeviceIOResult> startEvent = new AsyncManualResetEvent<DeviceIOResult>();
        protected AsyncManualResetEvent<DeviceIOResult> stopEvent = new AsyncManualResetEvent<DeviceIOResult>();

        public DeviceStatusEnum CurrentStatus { get; set; } = DeviceStatusEnum.Idle;
        protected virtual int IdlePollingInterval => 5 * 1000;
        protected virtual int StartingPollingInterval => 500;
        protected virtual int RunningPollingInterval => 500;
        protected virtual int PausingPollingInterval => 500;

        protected abstract TryStartDirective GenerateTryStartDirective();
        protected abstract bool ValidateDirectiveResult(DirectiveData data);

        public virtual void Initialize()
        {
            try
            {
                loopTimer?.Dispose();
                DirectiveWorker.Instance.SerialPortEvent -= DeviceBase_SerialPortEvent;
                DirectiveWorker.Instance.ErrorEvent -= DeviceBase_ErrorEvent;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Initialize" + e.Message);
            }

            DirectiveWorker.Instance.SerialPortEvent += DeviceBase_SerialPortEvent;
            DirectiveWorker.Instance.ErrorEvent += DeviceBase_ErrorEvent;
        }

        public virtual void Dispose()
        {
            loopTimer?.Dispose();
            DirectiveWorker.Instance.SerialPortEvent -= DeviceBase_SerialPortEvent;
            DirectiveWorker.Instance.ErrorEvent -= DeviceBase_ErrorEvent;
        }

#region 事件
        public delegate void CommunicationEventHandler(object sender, CommunicationEventArgs args);

        public event CommunicationEventHandler CommunicationEvent;
        public event CommunicationEventHandler CommunicationChangeEvent;
        public event Action<BaseDirective> PreCommunicationEvent;
        public event Action<CustomException> CaptureCustomExceptionEvent;

        protected void OnCommunicationEvent(CommunicationEventArgs args)
        {
            CommunicationEvent?.Invoke(this, args);
        }

        protected void OnCommunicationChangeEvent(CommunicationEventArgs args)
        {
            CommunicationChangeEvent?.Invoke(this, args);
        }

        protected void OnPreCommunicationEvent(BaseDirective args)
        {
            PreCommunicationEvent?.Invoke(args);
        }

        protected virtual void OnCaptureCustomExceptionEvent(CustomException obj)
        {
            CaptureCustomExceptionEvent?.Invoke(obj);
        }
#endregion

#region 发送指令

        public virtual async Task<DeviceIOResult> StartAsync()
        {
            try
            {
                Debug.WriteLine($"start {DeviceType}{DeviceId} when CurrentStatus {CurrentStatus}");

//                if (CurrentStatus == DeviceStatusEnum.Running || CurrentStatus == DeviceStatusEnum.Startting)
//                {
//                    return new DeviceIOResult(true);
//                }

                var directive = GenerateTryStartDirective();

                OnPreCommunicationEvent(directive);
                CurrentStatus = DeviceStatusEnum.PreStart;
                SendDirective(directive);
                var p = await startEvent.WaitAsync();
                startEvent.Reset();

                return p;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"start {DeviceType} timeout ->{ex.Message}");
                return new DeviceIOResult(false);
            }
        }

        public virtual async Task<DeviceIOResult> StopAsync()
        {
            Debug.WriteLine($"Stop {DeviceType} {DeviceId} when  CurrentStatus {CurrentStatus}");
            if (CurrentStatus == DeviceStatusEnum.Idle || CurrentStatus == DeviceStatusEnum.Pausing)
            {
                return new DeviceIOResult(true);
            }

            var directive = new TryPauseDirective(DeviceId, DeviceType);
            OnPreCommunicationEvent(directive);

            CurrentStatus = DeviceStatusEnum.PrePause;
            SendDirective(directive);

            var p = await stopEvent.WaitAsync();
            stopEvent.Reset();

            return p;
        }

        protected void SendDirective(BaseDirective directive, int span = 0)
        {
            loopTimer?.Dispose();
            if (span == 0)
            {
                DirectiveWorker.Instance.PrepareDirective(directive);
            }
            else
            {
                loopTimer = new Timer((obj) =>
                {
                    DirectiveWorker.Instance.PrepareDirective(directive);
                }, null, span, -1);
            }
        }

        protected virtual void StartRunningLoop(int directiveId = -1)
        {
            if (CurrentStatus != DeviceStatusEnum.Running && CurrentStatus != DeviceStatusEnum.Startting)
            {
                Debug.WriteLine(
                                $"device{DeviceId} CurrentStatus is {CurrentStatus}, can not send Running Directive");
                return;
            }

            var directive = new RunningDirective(DeviceId, DeviceType);
            if (directiveId == -1)
            {

                OnPreCommunicationEvent(directive);
            }

            SendDirective(directive, CurrentStatus == DeviceStatusEnum.Startting ? StartingPollingInterval : RunningPollingInterval);
        }

        protected virtual void StartPauseLoop(int directiveId = -1)
        {
            if (CurrentStatus != DeviceStatusEnum.Pausing)
            {
                Debug.WriteLine(
                                $"device{DeviceId} CurrentStatus is {CurrentStatus}, can not send Pausing Directive");
                return;
            }

            var directive = new PausingDirective(DeviceId, DeviceType);
            if (directiveId == -1)
            {
                OnPreCommunicationEvent(directive);
            }

            SendDirective(directive, PausingPollingInterval);
        }

        public virtual void StartIdleLoop(int directiveId = -1)
        {
            if (CurrentStatus != DeviceStatusEnum.Idle)
            {
                Debug.WriteLine(
                                $"device{DeviceId} CurrentStatus is {CurrentStatus}, can not send Idle Directive");
                return;
            }
            var directive = new IdleDirective(DeviceId, DeviceType);

            if (directiveId == -1)
            {
                OnPreCommunicationEvent(directive);
            }

            SendDirective(directive, IdlePollingInterval);
        }
        #endregion
        
#region 处理指令反馈
        protected abstract void ProcessRunningDirectiveResult(DirectiveData data, CommunicationEventArgs comEventArgs);
        protected abstract void ProcessTryPauseResult(DirectiveData data, CommunicationEventArgs comEventArgs);
        protected abstract void ProcessPausingResult(DirectiveData data, CommunicationEventArgs comEventArgs);

        protected virtual void ProcessTryStartResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            CurrentStatus = DeviceStatusEnum.Startting;
            comEventArgs.DeviceStatus = DeviceStatusEnum.Startting;
            //记录泵的开始时间
            // 拿到TryStart反馈指令后启动running状态轮询
            OnCommunicationChangeEvent(comEventArgs);
            StartRunningLoop();
        }

        protected virtual void ProcessIdleResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            if (CurrentStatus == DeviceStatusEnum.Idle)
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;
            }
            else
            {
                // 进入普通轮询状态
                CurrentStatus = DeviceStatusEnum.Idle;
                comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;
            }

            StartIdleLoop(data.DirectiveId);
        }

#endregion

        private void DeviceBase_SerialPortEvent(SerialPortEventArgs args)
        {
            if (args.Result?.Data == null || args.Result.Data.DeviceId != DeviceId) return;

            var comEventArgs = new CommunicationEventArgs
            {
                Command = args.Command,
                DirectiveId = args.Result.Data.DirectiveId,
                DeviceType = args.Result.Data.DeviceType,
                DeviceId = args.Result.Data.DeviceId,
                Description = args.Message,
                Data = args.Result.Data
            };

            if (!args.IsSucceed)
            {
                // 错误处理
                OnCaptureCustomExceptionEvent(
                    new CustomException($"device{DeviceId} CurrentStatus is {CurrentStatus},  receive Directive failed",
                        this.GetType().FullName, ExceptionPriority.Unrecoverable));

                return;
            }

            if (!ValidateDirectiveResult(args.Result.Data))
            {
                OnCaptureCustomExceptionEvent(new CustomException(
                    $"DirectiveResult validate failed",
                    this.GetType().FullName, ExceptionPriority.Unrecoverable));

                return;
            }

            var oldStatus = CurrentStatus;

            switch (args.Result.SourceDirectiveType)
            {
                case DirectiveTypeEnum.Idle:
                {
                    if (CurrentStatus == DeviceStatusEnum.Idle)
                    {
                        ProcessIdleResult(args.Result.Data, comEventArgs);
                            Debug.WriteLine(
                                $"device{DeviceId} {oldStatus} status receive Idle feedback convert to {comEventArgs.DeviceStatus}");

                    }
                    else
                    {
                            Debug.WriteLine(
                                $"device{DeviceId} CurrentStatus is {CurrentStatus}, can not receive Idle Directive");
                        return;
                    }

                    break;
                }

                case DirectiveTypeEnum.TryStart:
                {
                    if (CurrentStatus == DeviceStatusEnum.PreStart)
                    {
                        ProcessTryStartResult(args.Result.Data, comEventArgs);
                            Debug.WriteLine(
                                $"device{DeviceId} {oldStatus} status receive TryStart feedback convert to {comEventArgs.DeviceStatus}");
                    }
                    else
                    {
                            Debug.WriteLine(
                                $"device{DeviceId} CurrentStatus is {CurrentStatus}, can not receive TryStart Directive");
                        return;
                    }
                    break;
                }

                case DirectiveTypeEnum.Running:
                {
                    if (CurrentStatus == DeviceStatusEnum.Startting || CurrentStatus == DeviceStatusEnum.Running)
                    {
                        ProcessRunningDirectiveResult(args.Result.Data, comEventArgs);
                            Debug.WriteLine(
                                $"device{DeviceId} {oldStatus} status receive Running feedback convert to {comEventArgs.DeviceStatus}");
                    }
                    else
                    {
                            Debug.WriteLine(
                                $"device{DeviceId} CurrentStatus is {CurrentStatus}, can not receive Running Directive");
                        return;
                    }
                    break;
                }

                case DirectiveTypeEnum.TryPause:
                {
                    if (CurrentStatus == DeviceStatusEnum.PrePause)
                    {
                        ProcessTryPauseResult(args.Result.Data, comEventArgs);
                            Debug.WriteLine(
                                $"device{DeviceId} {oldStatus} status receive TryPause feedback convert to {comEventArgs.DeviceStatus}");
                    }
                    else
                    {
                            Debug.WriteLine(
                                $"device{DeviceId} CurrentStatus is {CurrentStatus}, can not receive TryPause Directive");
                        return;
                    }
                    break;
                }

                case DirectiveTypeEnum.Pausing:
                {
                    if (CurrentStatus == DeviceStatusEnum.Pausing)
                    {
                        ProcessPausingResult(args.Result.Data, comEventArgs);
                            Debug.WriteLine(
                                $"device{DeviceId} {oldStatus}status receive Pausing feedback convert to {comEventArgs.DeviceStatus}");
                    }
                    else
                    {
                            Debug.WriteLine(
                                $"device{DeviceId} CurrentStatus is {CurrentStatus}, can not receive Pausing Directive");
                        return;
                    }
                    break;
                }
            }

            OnCommunicationEvent(comEventArgs);
        }

        private void DeviceBase_ErrorEvent(CustomException ce, BaseDirective directive)
        {
            if (directive?.TargetDeviceId != DeviceId) return;

            OnCaptureCustomExceptionEvent(ce);
        }
    }
}
