using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WashMachine.Enums;

namespace WashMachine.Models
{
    public class WashFlow
    {
        public int Id { get; set; }
        public FlowEnum  FlowType { get; set; }

        public string Name { get; set; }

        public double WashVolume { get; set; }
        public double WashSpeed { get; set; }
        public DirectionEnum WashPumpDirection { get; set; }

        public double ConcentrateVolume { get; set; }
        public double ConcentrateSpeed { get; set; }
        public DirectionEnum ConcentratePumpDirection { get; set; }
        public int ConcentrateTimes { get; set; }

        public double CollectVolume { get; set; }
        public double CollectSpeed { get; set; }
        public DirectionEnum CollectionPumpDirection { get; set; }
        public int CollectTimes { get; set; }
    }
}
