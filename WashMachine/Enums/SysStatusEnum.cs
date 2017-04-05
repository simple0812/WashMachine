using System.Runtime.Serialization;

namespace WashMachine.Enums
{
    [DataContract]
    public enum SysStatusEnum
    {
        [EnumMember(Value = "Unknown")]
        Unknown = 0,

        [EnumMember(Value = "Ready")]
        Ready = 1,

        [EnumMember(Value = "Starting")]
        Starting = 2,

        [EnumMember(Value = "Running")]
        Running = 3,

        [EnumMember(Value = "Pausing")]
        Pausing = 4,

        [EnumMember(Value = "Paused")]
        Paused = 5,

        [EnumMember(Value = "Completed")]
        Completed = 6,

        [EnumMember(Value = "Discarding")]
        Discarding = 7,

        [EnumMember(Value = "Discarded")]
        Discarded = 8,
    }
}
