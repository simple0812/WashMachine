using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WashMachine.Protocols.Helper
{
    //缓存串口 避免多次开启和关闭串口
    public class SerialCreater
    {
        Dictionary<string, SerialPortHelper> serialHelpers;
        public static readonly SerialCreater Instance = new SerialCreater();

        private SerialCreater()
        {
            this.serialHelpers = new Dictionary<string, SerialPortHelper>();
        }

        public SerialPortHelper Create(string portName = "")
        {
            if (!serialHelpers.ContainsKey(portName))
            {
                serialHelpers[portName] = new SerialPortHelper(portName);
            }

            return serialHelpers[portName];
        }
    }
}
