using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WashMachine.Protocols.SimDirectives
{
    public abstract class BaseSimDirective
    {
        public bool IsOk { get; set; }

        public abstract string DirectiveText { get; }

        public virtual SimDirectiveResult Process(string cmdResult)
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

            if (Array.IndexOf(arr,"OK") != -1)
            {
                return new SimDirectiveResult(true, true);
            }
            else
            {
                return new SimDirectiveResult(true, false);
            }
        }

        public virtual bool isEnd(string str)
        {
            var regStr = "\\r\\n.*(OK)|(ERROR)|(>)";
            var reg = new Regex(regStr);
            var m = reg.Match(str);
            if (m.Success)
            {
                return true;
            }

            return false;
        }
    }
}
