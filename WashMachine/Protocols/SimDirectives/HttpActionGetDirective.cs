using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WashMachine.Protocols.SimDirectives
{
    public class HttpActionGetDirective: BaseSimDirective
    {
        //get方法
        public override string DirectiveText => "AT+HTTPACTION=0";

        public override bool isEnd(string str)
        {
            var regStr = "\\r\\n.*(HTTPACTION)|(ERROR)";
            var reg = new Regex(regStr);
            var m = reg.Match(str);
            if (m.Success)
            {
                return true;
            }

            return false;
        }

        public override SimDirectiveResult Process(string cmdResult)
        {
            if (!isEnd(cmdResult))
            {
                return new SimDirectiveResult(false, "指令未结束");
            }

            var arr = cmdResult.Split("\r\n".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length < 2)
            {
                return new SimDirectiveResult(false, "指令返回结果未能解析");
            }

            if (arr[0] != DirectiveText)
            {
                return new SimDirectiveResult(false, "指令不匹配");
            }

            if (arr.Any(p => p.IndexOf(",200,", StringComparison.Ordinal) >0))
            {
                return new SimDirectiveResult(true, true);
            }
            else
            {
                return new SimDirectiveResult(true, false);
            }
        }
    }
}
