using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System.Profile;

namespace WashMachine.Libs
{
    public static class Common
    {
        public static string BytesToString(byte[] bytes)
        {
            return bytes.Aggregate("", (current, t) => current + (Convert.ToString(t, 16).PadLeft(2, '0') + ",")).TrimEnd(',');
        }



        public static async Task WriteToFile(string msg)
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync("log", CreationCollisionOption.OpenIfExists);
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
                Debug.WriteLine("WriteToFile->" + e.Message );
                //   
            }
        }

        public static void SetData(string key, object value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public static string GetData(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return ApplicationData.Current.LocalSettings.Values[key].ToString();
            }

            return "";
        }

        public static string GetUniqueId()
        {
            HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
            return CryptographyHelper.Md5Encrypt(token.Id);
        }
    }
}
