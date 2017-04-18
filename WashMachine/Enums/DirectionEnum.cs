using System.Runtime.Serialization;

namespace WashMachine.Enums
{
    public enum DirectionEnum
    {
        [EnumMember(Value = "Out")]
        Out=0,
        [EnumMember(Value = "In")]
        In
    }
}
