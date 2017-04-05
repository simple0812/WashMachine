namespace WashMachine.Protocols.SimDirectives
{
    public class HttpBearerOpenDirective : BaseSimDirective
    {
        public override string DirectiveText => "AT+SAPBR=1,1";
    }
}
