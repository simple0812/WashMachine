namespace WashMachine.Protocols.SimDirectives
{
    public class HttpParaUrlDirective: BaseSimDirective
    {
        public override string DirectiveText { get; }

        public HttpParaUrlDirective( string value)
        {
            this.DirectiveText = $"AT+HTTPPARA=\"URL\",\"{value}\"";
        }
    }
}
