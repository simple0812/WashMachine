using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using WashMachine.Enums;
using WashMachine.Libs;
using WashMachine.Models;

namespace WashMachine.Protocols.Helper
{
    public sealed class SerialPortHelper 
    {
        public SerialPortStatus Status { get; set; }
        SerialDevice serialPort = null;
        CancellationTokenSource readCancellationTokenSource;

        public event Action<byte[]> ReceiveHandler;

        const double WRITE_TIMEOUT = 20;
        const double READ_TIMEOUT = 20;
        const uint BAUD_RATE = 9600;
        const SerialParity SERIAL_PARITY = SerialParity.None;
        const SerialStopBitCount SERIAL_STOP_BIT_COUNT = SerialStopBitCount.One;
        const ushort DATA_BITS = 8;
        const SerialHandshake SERIAL_HANDSHAKE = SerialHandshake.None;

        private DataReader dataReader;

        public SerialPortHelper(SerialDevice device)
        {
            serialPort = device;
            Status = SerialPortStatus.Initialled;
        }

        public async Task<SerialEnum> Open( )
        {
            if (Status == SerialPortStatus.Initialled)
            {
                try
                {
                    serialPort.WriteTimeout = TimeSpan.FromMilliseconds(WRITE_TIMEOUT);
                    serialPort.ReadTimeout = TimeSpan.FromMilliseconds(READ_TIMEOUT);
                    serialPort.BaudRate = BAUD_RATE;
                    serialPort.Parity = SERIAL_PARITY;
                    serialPort.StopBits = SERIAL_STOP_BIT_COUNT;
                    serialPort.DataBits = DATA_BITS;
                    serialPort.Handshake = SERIAL_HANDSHAKE;

                    readCancellationTokenSource = new CancellationTokenSource();
                    var ping = await Ping(new byte[] { 0x01, 0x04, 0x00, 0x00, 0x00, 0x18, 0xf0 });
                    Debug.WriteLine("PC ping" + ping);
                    //改为
                    if (!string.IsNullOrEmpty(ping) && ping !="ff")
                    {
                        listen(1024);
                        Status = SerialPortStatus.Opened;
                        return SerialEnum.LowerComputer;
                    }

                    ping = await Ping(Encoding.UTF8.GetBytes("AT+CCID").Concat(new byte[] { 0x0D, 0x0A }).ToArray());
                    Debug.WriteLine("SIM ping" + ping);
                    if (!string.IsNullOrEmpty(ping))
                    {
                        listen(1024);
                        Status = SerialPortStatus.Opened;
                        return SerialEnum.Sim;
                    }
                    Status = SerialPortStatus.Initialled;
                    serialPort.Dispose();//该行会阻塞代码,导致不能正常返回
                    return SerialEnum.Unknown;
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message + "open", this.GetType().FullName, ExceptionPriority.Importance);
                }
               
            }

            return SerialEnum.Unknown;
        }

        private async Task<string> Ping(byte[] buffer)
        {
            var dataWriter = new DataWriter(serialPort.OutputStream);
            var reader = new DataReader(serialPort.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;
            var source = new CancellationTokenSource(1000);
            try
            {
                dataWriter.WriteBytes(buffer);
                await dataWriter.StoreAsync().AsTask(source.Token); ;
                var bytesRead =
                    await reader.LoadAsync(1024).AsTask(source.Token);
                var xdata = new byte[bytesRead];
                reader.ReadBytes(xdata);

                return Common.BytesToString(xdata);

            }
            catch (Exception e)
            {
                Debug.WriteLine("abc->" + e.Message);
            }
            finally
            {
                dataWriter.DetachBuffer();
                dataWriter.DetachStream();
                dataWriter.Dispose();

                reader.DetachBuffer();
                reader.DetachStream();
                reader.Dispose();
            }

            return "";
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
                            LocalLog.Instance.Info("receive ->" + Common.BytesToString(xdata) + "<- end");
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
                    LocalLog.Instance.Info("send ->" + Common.BytesToString(buffer) +"<- end");
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
