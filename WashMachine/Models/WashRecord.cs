using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WashMachine.Enums;

namespace WashMachine.Models
{
    public class WashRecord
    {
        public int Id { get; set; }
        public FlowEnum FlowType { get; set; }

        public string Name { get; set; }

        public double WashVolume { get; set; }
        public double WashSpeed { get; set; }

        public double ConcentrateVolume { get; set; }
        public double ConcentrateSpeed { get; set; }
        public int ConcentrateTimes { get; set; }

        public double CollectVolume { get; set; }
        public double CollectSpeed { get; set; }
        public int CollectTimes { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
