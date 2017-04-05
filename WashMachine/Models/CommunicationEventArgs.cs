using System;
using WashMachine.Models;
using WashMachine.Enums;

namespace WashMachine.Models
{
    public class CommunicationEventArgs : EventArgs
    {
        public DeviceStatusEnum DeviceStatus { get; set; }
        public byte[] Command { get; set; }
        public TargetDeviceTypeEnum DeviceType { get; set; }
        public int DeviceId { get; set; }
        public int DirectiveId { get; set; }
        public DirectiveData Data { get; set; }

        public string Description { get; set; }


    }
}
