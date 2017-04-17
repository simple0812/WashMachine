using WashMachine.Models;
using WashMachine.Protocols.Directives;
using WashMachine.Enums;

namespace WashMachine.Devices
{
    public sealed class PumpDevice : DeviceBase
    {
        public double FlowRate { get; set; } = 10;
        public double Volume { get; set; } = 20;
        public DirectionEnum Direction { get; set; }


        public PumpDevice(int deviceId = 0x01, DirectionEnum direction = DirectionEnum.In)
        {
            Initialize();

            DeviceType = TargetDeviceTypeEnum.Pump;

            DeviceId = deviceId;
            Direction = direction;
        }

        public PumpDevice SetParams(double flowRate, double volume, DirectionEnum direction )
        {
            FlowRate = flowRate;
            Volume = volume;
            Direction = direction;

            return this;
        }

        protected override TryStartDirective GenerateTryStartDirective()
        {
            return new TryStartDirective(DeviceId, FlowRate, Volume, (int)Direction);
        }


        protected override bool ValidateDirectiveResult(DirectiveData data)
        {
            var ret = data as PumpDirectiveData;
            return null != ret;
        }

        protected override void ProcessRunningDirectiveResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            var ret = data as PumpDirectiveData;

            if (CurrentStatus == DeviceStatusEnum.Startting)
            {
                if (ret?.FlowRate > 0)
                {
                    CurrentStatus = DeviceStatusEnum.Running;
                    comEventArgs.DeviceStatus = DeviceStatusEnum.Running;

                    OnCommunicationChangeEvent(comEventArgs);
                    StartRunningLoop();
                }
                else
                {
                    comEventArgs.DeviceStatus = CurrentStatus;
                    StartRunningLoop(data.DirectiveId);
                }
            }
            else if (CurrentStatus == DeviceStatusEnum.Running)
            {
                if (ret != null && ret.FlowRate <= 0)
                {
                    CurrentStatus = DeviceStatusEnum.Idle;
                    comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;

                    var diResult = new DeviceIOResult(true, comEventArgs);
                    startEvent.Set(diResult);
                }
                else
                {
                    comEventArgs.DeviceStatus = DeviceStatusEnum.Running;
                    StartRunningLoop(data.DirectiveId);
                }
            }
            else
            {
                StartRunningLoop();
            }
        }

        protected override void ProcessTryPauseResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            CurrentStatus = DeviceStatusEnum.Pausing;
            comEventArgs.DeviceStatus = DeviceStatusEnum.Pausing;
            OnCommunicationChangeEvent(comEventArgs);
            StartPauseLoop();
        }

        protected override void ProcessPausingResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            var ret = data as PumpDirectiveData;

            if (ret != null && ret.FlowRate <= 0)
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;
                comEventArgs.Description = IdleDesc.Paused.ToString(); ;

                CurrentStatus = DeviceStatusEnum.Idle;

                stopEvent.Set(new DeviceIOResult(true));

                OnCommunicationChangeEvent(comEventArgs);
            }
            else
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Pausing;
                StartPauseLoop(data.DirectiveId);
            }
        }
    }
}
