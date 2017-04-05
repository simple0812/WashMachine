using System;
using System.Linq;

namespace WashMachine.Protocols.SimDirectives
{
    public class HttpReadDirective: BaseSimDirective
    {
        public override string DirectiveText => "AT+HTTPREAD";

        public override SimDirectiveResult Process(string cmdResult)
        {
            if (!isEnd(cmdResult))
            {
                return new SimDirectiveResult(false, "指令未结束");
            }

            var arr = cmdResult.Split("\r\n".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length < 4)
            {
                return new SimDirectiveResult(false, "指令返回结果未能解析");
            }
            
            if (arr[0] != DirectiveText)
            {
                return new SimDirectiveResult(false, "指令不匹配");
            }

            if (Array.IndexOf(arr, "OK") != -1)
            {
                return new SimDirectiveResult(true, true, arr[2]);
            }
            else
            {
                return new SimDirectiveResult(true, false);
            }
        }
    }
}
