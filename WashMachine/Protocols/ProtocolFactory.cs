using WashMachine.Protocols.Enums;
using System.Reflection;
using System;
using System.Linq;

namespace WashMachine.Protocols
{
    public class ProtocolFactory
    {

        public static IProtocol Create(ProtocolVersion version)
        {
            VersionListAttribute attrList = typeof(IProtocol).GetTypeInfo().GetCustomAttribute<VersionListAttribute>();
            return (from t in attrList.VersionTypeList let verAttr = t.GetTypeInfo().GetCustomAttribute<VersionAttribute>() where verAttr.Version == version select Type.GetType(t.GetTypeInfo().FullName).GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as IProtocol).FirstOrDefault();
        }

    }
}
