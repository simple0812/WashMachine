using WashMachine.Models;

namespace WashMachine.Protocols.V485_1
{
    internal interface IFeedbackResolver
    {
        DirectiveResult ResolveFeedback(byte[] bytes);
    }
}
