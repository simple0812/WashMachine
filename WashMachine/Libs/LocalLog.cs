using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WashMachine.Libs
{
    public class LocalLog 
    {
        private ConcurrentQueue<string> waitForSendMsg = new ConcurrentQueue<string>();
        public static readonly LocalLog Instance = new LocalLog();

        private LocalLog()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    string data = "";
                    if (waitForSendMsg.TryDequeue(out data))
                    {
                        await WriteToFile(data);
                        await Task.Delay(1);
                    }
                }
            });
        }

        public static async Task WriteToFile(string msg)
        {
            try
            {
                var file = await KnownFolders.PicturesLibrary.CreateFileAsync("logs.txt", CreationCollisionOption.OpenIfExists);
               
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    stream.Seek(0, SeekOrigin.End);
                    using (var sw = new StreamWriter(stream))
                    {
                        sw.WriteLine(msg);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("WriteToFile->" + e.Message);
                //   
            }
        }


        public void Info(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            Enqueue(msg);
        }

        private void Enqueue(string msg)
        {
            waitForSendMsg.Enqueue($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} Info] {msg}");
        }
    }
}
