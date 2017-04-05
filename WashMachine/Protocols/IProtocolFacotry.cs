using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WashMachine.Protocols
{
    public interface IProtocolFacotry
    {
        IProtocol Create();
    }
}
