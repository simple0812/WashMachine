namespace WashMachine.Protocols.SimDirectives
{
    public class CregDirective:BaseSimDirective
    {
        public override string DirectiveText => "AT+CREG?";
    }
}
