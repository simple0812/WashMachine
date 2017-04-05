using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WashMachine.Enums;

namespace WashMachine.Protocols.Directives
{
    public class TryStartDirective : BaseDirective
    {
        public override DirectiveTypeEnum DirectiveType => DirectiveTypeEnum.TryStart;
        public override int Priority => 3;

        public double Param1 { get; set; } //FlowRate Speed ...
        public double Param2 { get; set; }   //Volume  Angle ...
        public int Mode { get; set; } //DirectionEnum RockEnum...

        public TryStartDirective(int targetDeviceId, double flowRate, double volume, int direction, TargetDeviceTypeEnum deviceType = TargetDeviceTypeEnum.Pump)
        {
            this.TargetDeviceId = targetDeviceId;
            this.Param1 = flowRate;
            this.Param2 = volume;
            this.Mode = direction;
            this.DeviceType = deviceType;
        }
    }
}
