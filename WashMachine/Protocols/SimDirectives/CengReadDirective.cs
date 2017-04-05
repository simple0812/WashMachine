using System;
using System.Diagnostics;
using System.Linq;
using WashMachine.Models;

namespace WashMachine.Protocols.SimDirectives
{
    public class CengReadDirective: BaseSimDirective
    {
        public override string DirectiveText => "AT+CENG?";

        public override SimDirectiveResult Process(string cmdResult)
        {
            if (!isEnd(cmdResult))
            {
                return new SimDirectiveResult(false, "指令未结束");
            }

            var arr = cmdResult.Split("\r\n".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length <= 2)
            {
                return new SimDirectiveResult(false, "指令返回结果未能解析");
            }
            var ret = arr.FirstOrDefault(each => each.IndexOf("+CENG: 0", StringComparison.Ordinal) == 0);
            Debug.WriteLine(ret);
            if (string.IsNullOrEmpty(ret))
            {
                return new SimDirectiveResult(false, "指令解析失败");
            }

            if (arr[0] != DirectiveText)
            {
                return new SimDirectiveResult(false, "指令不匹配");
            }

            var cnet = new CnetScan().Resovlex(ret);
            Debug.WriteLine(cnet.Cellid +"," + cnet.Lac);
            

            if (Array.IndexOf(arr, "OK") != -1)
            {
                return new SimDirectiveResult(true, true, cnet);
            }
            else
            {
                return new SimDirectiveResult(true, false);
            }
        }
    }
}
