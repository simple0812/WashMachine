namespace WashMachine.Models
{
    public class DeviceIOResult
    {
        public DeviceIOResult()
        {
            Status = false;
        }

        public DeviceIOResult(bool status)
        {
            Status = status;
        }

        public DeviceIOResult(bool status, object code)
        {
            Status = status;
            Code = code;
        }

        public bool Status { get; set; }
        public object Code { get; set; }

    }
}
