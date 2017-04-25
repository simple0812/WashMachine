using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WashMachine.Models;

namespace WashMachine.Libs
{
    public static class AutoUpdate
    {
        private static int version;
        private static string path;
        private static AppServiceConnection updaterService;

        public static int GetCurrentVersion()
        {
            var ver = Package.Current.Id.Version;

            return ver.Major* 1000000 + ver.Minor*10000 + ver.Build*100 + ver.Revision;
        }

        public static async Task<bool> CheckUpdate()
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = new Uri($"http://{App.SERVER_ADDR}:{App.SERVER_PORT}/api/version");
                var response = await client.GetAsync(uri);
                if (response.EnsureSuccessStatusCode().StatusCode.ToString().ToLower() == "ok")
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var x = JsonConvert.DeserializeObject<JsonResult>(responseBody);
                    if (x.code != "success")
                    {
                        return false;
                    }

                    var p = JObject.FromObject(x.result);
                    int lastVersion = int.TryParse(p["ver"].ToString(), out lastVersion) ? lastVersion : 0;

                    path = p["path"].ToString();

                    return lastVersion > GetCurrentVersion();
                }

                return false;
            }
        }

        public static async void Update()
        {
            if (updaterService == null)
            {
                updaterService = new AppServiceConnection();
                updaterService.AppServiceName = "net.hoekstraonline.appupdater";
                updaterService.PackageFamilyName = "189703c0-3a5c-47dd-a12f-af8b2ed78d2d_wbwwzgv8bqypp";

                var status = await updaterService.OpenAsync();
                if (status != AppServiceConnectionStatus.Success)
                {
                    LocalLog.Instance.Info("appservice connect error");
                    updaterService = null;
                    return;
                }
                else
                {
                    LocalLog.Instance.Info("appservice connect success");
                }
            }
            try
            {
                Uri updatePackage = new Uri($"http://{App.SERVER_ADDR}:{App.SERVER_PORT}/" + path);
                var message = new ValueSet();
                message.Add("PackageFamilyName", Windows.ApplicationModel.Package.Current.Id.FamilyName);
                message.Add("PackageLocation", updatePackage.ToString());

                AppServiceResponse response = await updaterService.SendMessageAsync(message);

                if (response.Status == AppServiceResponseStatus.Success)
                {
                    LocalLog.Instance.Info("appservice connect xxx");
                }
            }
            catch (Exception)
            {
                //
            }
        }
    }
}
