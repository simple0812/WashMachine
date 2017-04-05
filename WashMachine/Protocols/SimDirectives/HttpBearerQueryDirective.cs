using System;
using System.Linq;

namespace WashMachine.Protocols.SimDirectives
{
    public class HttpBearerQueryDirective : BaseSimDirective
    {
        public override string DirectiveText => "AT+SAPBR=2,1";

        public override SimDirectiveResult Process(string cmdResult)
        {
            if (!isEnd(cmdResult))
            {
                return new SimDirectiveResult(false, "指令未结束");
            }

            var arr = cmdResult.Split("\r\n".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length != 3)
            {
                return new SimDirectiveResult(false, "指令返回结果未能解析");
            }

            if (arr[0] != DirectiveText)
            {
                return new SimDirectiveResult(false, "指令不匹配");
            }

            //特殊处理 如果此处为error 需要关闭bearer后重新链接
            if (Array.IndexOf(arr, "OK") == -1)
            {
                return new SimDirectiveResult(false, "指令执行失败");
            }

            if(arr.Any(x => x.IndexOf("0.0.0.0", StringComparison.Ordinal) > 0))
            {
                return new SimDirectiveResult(true, false);//bearer未连接
            }

            return new SimDirectiveResult(true, true);//已经连接
        }
    }
}
