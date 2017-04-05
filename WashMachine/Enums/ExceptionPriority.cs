namespace WashMachine.Enums
{
    /*
    1.串口异常均为Importance(特别重大)
    2.指令收发异常均为Grave（比较重大）
    3.设备异常均为Biggish（重大） 
    */
    public enum ExceptionPriority
    {
        Normal, //一般
        Biggish, //重大
        Grave, //比较重大
        Importance,//特别重大
        Unrecoverable //未知异常
    }
}
