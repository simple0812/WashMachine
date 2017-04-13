using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Streams;
using WashMachine.Models;
using WashMachine.Enums;
using WashMachine.Libs;

namespace WashMachine.Protocols.Helper
{
    public sealed class SerialPortHelper 
    {
        public SerialPortStatus Status { get; set; }
        SerialDevice serialPort = null;
        CancellationTokenSource readCancellationTokenSource;
        private string PortName;

        public event Action<byte[]> ReceiveHandler;

        const double WRITE_TIMEOUT = 20;
        const double READ_TIMEOUT = 20;
        const uint BAUD_RATE = 9600;
        const SerialParity SERIAL_PARITY = SerialParity.None;
        const SerialStopBitCount SERIAL_STOP_BIT_COUNT = SerialStopBitCount.One;
        const ushort DATA_BITS = 8;
        const SerialHandshake SERIAL_HANDSHAKE = SerialHandshake.None;

        private DataReader dataReader;

        public SerialPortHelper(string portName = "")
        {
            PortName = portName;
            Status = SerialPortStatus.Initialled;
        }

        public async Task Open()
        {
            if (Status == SerialPortStatus.Initialled)
            {
                try
                {
                    string aqs = SerialDevice.GetDeviceSelector();
                    var dis = await DeviceInformation.FindAllAsync(aqs);
                    foreach (var each in dis)
                    {
                        Debug.WriteLine("portname" + each.Name);
                    }

                    var p = PortName == "" ? dis.FirstOrDefault() : dis.FirstOrDefault(item => item.Name == PortName);
                    if (p == null)
                    {
                        Debug.WriteLine(PortName + " Port is null");
                        return;
                    }

                    serialPort = await SerialDevice.FromIdAsync(p.Id);

                    if (serialPort == null)
                    {
                        Debug.WriteLine("Serial Port is null");
                        return;
                    }

                    serialPort.WriteTimeout = TimeSpan.FromMilliseconds(WRITE_TIMEOUT);
                    serialPort.ReadTimeout = TimeSpan.FromMilliseconds(READ_TIMEOUT);
                    serialPort.BaudRate = BAUD_RATE;
                    serialPort.Parity = SERIAL_PARITY;
                    serialPort.StopBits = SERIAL_STOP_BIT_COUNT;
                    serialPort.DataBits = DATA_BITS;
                    serialPort.Handshake = SERIAL_HANDSHAKE;

                    readCancellationTokenSource = new CancellationTokenSource();

                    listen(1024);
                    Status = SerialPortStatus.Opened;
                    Debug.WriteLine("Serial Port Opened");
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message + "open", this.GetType().FullName, ExceptionPriority.Importance);
                }
            }
        }

        private async void listen(uint bufferLength)
        {
            try
            {
                readCancellationTokenSource.Token.ThrowIfCancellationRequested();

                dataReader = new DataReader(serialPort.InputStream);
                dataReader.InputStreamOptions = InputStreamOptions.Partial;

                if (serialPort != null)
                {
                    while (!readCancellationTokenSource.IsCancellationRequested)
                    {
                        var bytesRead =
                            await dataReader.LoadAsync(bufferLength).AsTask(readCancellationTokenSource.Token);

                        if (bytesRead > 0)
                        {
                            var xdata = new byte[bytesRead];
                            dataReader.ReadBytes(xdata);
                            dataReader.DetachBuffer();
                            LocalLog.Instance.Info("receive ->" + Common.BytesToString(xdata));
                            OnReceiveHandler(xdata);
                        }

                        await Task.Delay(5, readCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("serial listen cancel");
                Status = SerialPortStatus.Initialled;
                serialPort = null;
            }
            catch (Exception)
            {
                Status = SerialPortStatus.Initialled;
                serialPort = null;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.DetachBuffer();
                    dataReader.DetachStream();
                    dataReader.Dispose();
                }
            }
        }

        public async Task Send(byte[] buffer, CancellationToken token)
        {
            if (serialPort == null) return;
            var dataWriter = new DataWriter(serialPort.OutputStream);

            try
            {
                if (buffer.Length != 0)
                {
                    LocalLog.Instance.Info("send ->" + Common.BytesToString(buffer));
                    dataWriter.WriteBytes(buffer);

                    var storeAsyncTask = dataWriter.StoreAsync().AsTask(token);
                    await storeAsyncTask;
                }
            }
            catch (Exception ex)
            {
                Status = SerialPortStatus.Initialled;
                serialPort?.Dispose();
                serialPort = null;
        
                throw new CustomException(ex.Message + "Send", this.GetType().FullName, ExceptionPriority.Importance);
            }
            finally
            {
                dataWriter.DetachBuffer();
                dataWriter.DetachStream();
                dataWriter.Dispose();
            }
        }

        public void Cancel()
        {
            if (readCancellationTokenSource != null)
            {
                if (!readCancellationTokenSource.IsCancellationRequested)
                {
                    readCancellationTokenSource.Cancel();
                }
            }
            Status = SerialPortStatus.Closed;
        }

        public void Close()
        {
            try
            {
                serialPort?.Dispose();
                serialPort = null;
                Status = SerialPortStatus.None;
                Debug.WriteLine("serialport dispose");
            }
            catch (Exception e)
            {
                Debug.WriteLine("serialport close error ->" + e.Message);
            }
        }

        private void OnReceiveHandler(byte[] obj)
        {
            ReceiveHandler?.Invoke(obj);
        }
    }

    public enum SerialPortStatus
    {
        Opened, Opening, Closed, None, Initialled
    }
}
