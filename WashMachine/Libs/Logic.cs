using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WashMachine.Devices;
using WashMachine.Enums;
using WashMachine.Models;
using WashMachine.Protocols.Directives;
using WashMachine.Protocols.Helper;

namespace WashMachine.Libs
{
    public sealed class Logic
    {
        public  PumpDevice pump1 = new PumpDevice(1);
        public  PumpDevice pump2 = new PumpDevice(2, DirectionEnum.Out);
        public  PumpDevice pump3 = new PumpDevice(3, DirectionEnum.Out);
        public  WashFlow WashFlow { get; set; }

        public static readonly Logic Instance = new Logic();

        public event Action<object, CommunicationEventArgs> CommunicationHandler;

        private Logic()
        {
            AttactchEventHanlder(pump1);
            AttactchEventHanlder(pump2);
            AttactchEventHanlder(pump3);
        }

        public async Task Close()
        {
            try
            {
                var task = StopPumps();
                var cancellationToken = new CancellationTokenSource(5*1000).Token;
                var cancellationCompletionSource = new TaskCompletionSource<bool>();

                using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(true)))
                {
                    if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                    {
                        Dispose();
                    }
                }

                await task;
                Dispose();

            }
            catch (Exception)
            {
                //
            }
            finally
            {
                pump1 = new PumpDevice(1);
                pump2 = new PumpDevice(2, DirectionEnum.Out);
                pump3 = new PumpDevice(3, DirectionEnum.Out);
                AttactchEventHanlder(pump1);
                AttactchEventHanlder(pump2);
                AttactchEventHanlder(pump3);
            }
          
        }

        public async Task<DeviceIOResult[]> StopPumps()
        {
            var list = new List<Task<DeviceIOResult>>();

            list.Add(pump1.StopAsync());
            list.Add(pump2.StopAsync());
            list.Add(pump3.StopAsync());

            return await Task.WhenAll(list);
        }

        public void Dispose()
        {
            try
            {
                pump1.Dispose();
                pump2.Dispose();
                pump3.Dispose();

                DirectiveWorker.Instance.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine("IotCommunicationService dispose error->" + e.Message);
            }
        }

        public void DetactchEventHanlder(PumpDevice device)
        {
            try
            {
                device.CommunicationEvent -= Communication;
                device.PreCommunicationEvent -= PreCommunicationStatusChange;
                device.CommunicationChangeEvent -= CommunicationChange;
                device.CaptureCustomExceptionEvent -= CaptureCustomExceptionEvent;
            }
            catch (Exception)
            {
               //
            }
        }

        public void AttactchEventHanlder(PumpDevice device)
        {
            DetactchEventHanlder(device);

            device.CommunicationEvent += Communication;
            device.PreCommunicationEvent += PreCommunicationStatusChange;
            device.CommunicationChangeEvent += CommunicationChange;
            device.CaptureCustomExceptionEvent += CaptureCustomExceptionEvent;
        }

        private void CaptureCustomExceptionEvent(CustomException obj)
        {
            //service.OnErrorEvent(obj);
        }

        private void PreCommunicationStatusChange(BaseDirective directive)
        {
            //发送开始命令或者暂停命令前
        }

        //监听触发状态改变的指令
        private void CommunicationChange(object sender, CommunicationEventArgs e)
        {
            if (e == null) return;
            if (e.DeviceStatus == DeviceStatusEnum.Idle)
            {
                App.Status = SysStatusEnum.Discarded;
            }
            else
            {
                if (App.Status == SysStatusEnum.Starting)
                {
                    App.Status = SysStatusEnum.Running;
                }
            }
           
            Debug.WriteLine($"****************************{e.DeviceId}->{e.DeviceStatus}***********************");
        }

        //监听每一条指令
        private void Communication(object sender, CommunicationEventArgs e)
        {
            OnCommunicationHandler(sender, e);
        }

        private void OnCommunicationHandler(object arg1, CommunicationEventArgs arg2)
        {
            CommunicationHandler?.Invoke(arg1, arg2);
        }

        public void End()
        {
            CommunicationHandler?.Invoke(null, new CommunicationEventArgs() {Description = "end"});
        }
    }
}
