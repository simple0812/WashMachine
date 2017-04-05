using System.Runtime.Serialization;

namespace WashMachine.Enums
{
    public enum IdleDesc
    {

        [EnumMember(Value = "start")]
        Start = 0,

        [EnumMember(Value = "completed")]
        Completed = 1,

        [EnumMember(Value = "paused")]
        Paused = 2,

        [EnumMember(Value = "cultivation finished")]
        Finished = 3,
    }
}
