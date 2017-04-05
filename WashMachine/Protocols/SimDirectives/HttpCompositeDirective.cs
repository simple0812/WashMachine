using System;

namespace WashMachine.Protocols.SimDirectives
{
    public class HttpCompositeDirective:CompositeDirective
    {
        public override SimDirectiveType DirectiveType => SimDirectiveType.HttpGet;
        public string Url { get; set; }

        public HttpCompositeDirective(string url, Action<SimDirectiveResult> cb)
        {
            this.Url = url;
            this.SuccessHandler = cb;
        }
    }
}
