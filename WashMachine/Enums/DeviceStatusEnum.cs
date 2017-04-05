namespace WashMachine.Enums
{
    public enum DeviceStatusEnum
    {
        Running = 0,
        Idle,
        PreStart,
        Startting, // 收到TryStart反馈
        PrePause,
        Pausing, // 收到TryPause反馈
        Error
    }
}
