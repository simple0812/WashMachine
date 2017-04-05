namespace WashMachine.Protocols.SimDirectives
{
    public class HttpParaCidDirective: BaseSimDirective
    {
        public override string DirectiveText => "AT+HTTPPARA=\"CID\",1";
    }
}
