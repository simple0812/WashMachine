namespace WashMachine.Protocols.SimDirectives
{
    public class HttpInitDirective: BaseSimDirective
    {
        public override string DirectiveText => "AT+HTTPINIT";
    }
}
