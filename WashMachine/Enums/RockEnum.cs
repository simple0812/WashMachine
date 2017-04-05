using System.Runtime.Serialization;

namespace WashMachine.Enums
{
    public enum RockEnum
    {
        [EnumMember(Value = "Normal")]
        Normal = 0,
        [EnumMember(Value = "Rotate")]
        Rotate
    }
}
