using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WashMachine.Enums;

namespace WashMachine.Protocols.Directives
{
    public class PausingDirective : BaseDirective
    {

        public override DirectiveTypeEnum DirectiveType => DirectiveTypeEnum.Pausing;
        public override int Priority => 4;

        public PausingDirective(int targetDeviceId, TargetDeviceTypeEnum deviceType = TargetDeviceTypeEnum.Pump)
        {
            this.TargetDeviceId = targetDeviceId;
            this.DeviceType = deviceType;
        }
    }
}
