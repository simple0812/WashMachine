using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using WashMachine.Enums;

namespace WashMachine.Protocols.Helper
{
    //缓存串口 避免多次开启和关闭串口
    public class SerialCreater
    {
        Dictionary<SerialEnum, SerialPortHelper> serialHelpers;
        public static readonly SerialCreater Instance = new SerialCreater();

        private SerialCreater()
        {
            this.serialHelpers = new Dictionary<SerialEnum, SerialPortHelper>();
        }

        public SerialPortHelper Create(SerialEnum serial)
        {
            if (!serialHelpers.ContainsKey(serial))
            {
                return null;
            }

            return serialHelpers[serial];
        }

        public async Task Build()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            foreach (var each in dis)
            {
                Debug.WriteLine("portname" + each.Name);
//                if (each.Name.IndexOf("USB", StringComparison.Ordinal) == -1 && each.Name != "MINWINPC") continue;

                var device = await SerialDevice.FromIdAsync(each.Id);
                if (device == null) continue;
                var helper = new SerialPortHelper(device);
                var x = await helper.Open();
                Debug.WriteLine(x);
                if (x != SerialEnum.Unknown)
                {
                    serialHelpers[x] = helper;
                }

            }
        }
    }
}
