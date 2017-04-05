using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WashMachine.Enums
{
    public enum FlowEnum
    {

        [EnumMember(Value = "simple")]
        Simple = 0,

        [EnumMember(Value = "full")]
        Full = 1
    }
}
