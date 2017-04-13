using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WashMachine.Enums
{
    public enum SerialEnum
    {
        [EnumMember(Value = "Unknowns")]
        Unknown =0,
        [EnumMember(Value = "LowerComputer")]
        LowerComputer,
        [EnumMember(Value = "Sim")]
        Sim
    }
}
