using System;
using WashMachine.Models;
using WashMachine.Protocols.Directives;
using WashMachine.Protocols.Enums;

namespace WashMachine.Protocols
{
    [VersionList(new Type[] { typeof(V485_1.V485_1) })]
    public interface IProtocol
    {
        byte[] GenerateDirectiveBuffer(BaseDirective directive);
        DirectiveResult ResolveFeedback(byte[] directive);
    }
}
