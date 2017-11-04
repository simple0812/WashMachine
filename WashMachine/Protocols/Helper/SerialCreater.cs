using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

        public async Task<SerialPortHelper> Create(SerialEnum serial)
        {
            if (serialHelpers.ContainsKey(serial))
            {
                var p = serialHelpers[serial];

                if(p.serialPort != null)
                    return serialHelpers[serial];

                serialHelpers.Remove(serial);
            }

            Debug.WriteLine("start build");
            await Build();
            Debug.WriteLine("finish build");

            if (serialHelpers.ContainsKey(serial))
            {
                return serialHelpers[serial];
            }

            return null;
        }

        public async Task Build()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            foreach (var each in dis)
            {
                Debug.WriteLine("portname:" + each.Name);
                if (each.Name.IndexOf("USB", StringComparison.Ordinal) == -1 
                    && each.Name != "MINWINPC" 
                    && each.Name.IndexOf("Virtual Serial Port", StringComparison.Ordinal) == -1) continue;

                var device = await SerialDevice.FromIdAsync(each.Id);
                if (device == null)
                {
                    Debug.WriteLine(each.Name + "is null");
                    continue;
                }

                var helper = new SerialPortHelper(device);

                var cancellationToken = new CancellationTokenSource(3 * 1000).Token;
                var cancellationCompletionSource = new TaskCompletionSource<SerialEnum>();

                using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(SerialEnum.Unknown)))
                {
                    var task = await Task.WhenAny(helper.Open(), cancellationCompletionSource.Task);

                    Debug.WriteLine("open:" + task.Result);
                    if (task.Result != SerialEnum.Unknown)
                    {
                        serialHelpers[task.Result] = helper;
                    }
                }
            }
        }
    }
}
