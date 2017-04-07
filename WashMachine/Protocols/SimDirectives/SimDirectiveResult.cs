using System;

namespace WashMachine.Protocols.SimDirectives
{
    public class SimDirectiveResult
    {
        public SimDirectiveResult()
        {
            Status = false;
        }


        public SimDirectiveResult(bool status, string msg ="")
        {
            Status = status;
            Message = msg;
        }

        public SimDirectiveResult(bool status, bool isOk)
        {
            Status = status;
            IsExecOk = isOk;
        }

        public SimDirectiveResult(bool status,bool isOk, Object result,string msg = "")
        {
            Status = status;
            IsExecOk = isOk;
            Result = result;
            Message = msg;
        }

        public bool Status { get; set; } //解析是否成功
        public string Message { get; set; } //解析失败的原因

        public bool IsExecOk { get; set; } //指令是否正确执行
        public object Result { get; set; } //解析的结果
    }
}
