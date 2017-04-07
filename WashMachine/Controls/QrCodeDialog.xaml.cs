using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WashMachine.Libs;
using WashMachine.Models;
using WashMachine.Protocols.Helper;
using WashMachine.Protocols.SimDirectives;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace WashMachine.Controls
{
    public sealed partial class QrCodeDialog : ContentDialog
    {
        private bool isBusy = false;
        public QrCodeDialog()
        {
            this.InitializeComponent();
            txtQr.Focus(FocusState.Pointer);
        }

        private async void TxtQr_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;
                await Task.Delay(1000);
                var box = sender as TextBox;

                if (box != null)
                {
                    ParseQrCode(box.Text);
                    box.Text = "";
                }
                isBusy = false;
            }
        }

        private void ParseQrCode(string str)
        {
            if (str.IndexOf("http", StringComparison.Ordinal) != 0)
            {
                txtRet.Text = "二维码错误";
                return;
            }
            //防止重复扫描
            var x = "http" + str.Split(new string[] { "http" }, StringSplitOptions.RemoveEmptyEntries)[0];
            SimWorker.Instance.Enqueue(new HttpCompositeDirective(x + "&deviceid="+Common.GetUniqueId(), async ret =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        if (!ret.Status)
                        {
                            txtRet.Text = ret.Message;
                            return;
                        }

                        var json = JsonConvert.DeserializeObject<JsonResult>(ret.Result.ToString());
                        if (json.code == "error")
                        {
                            txtRet.Text = json.message;
                            return;
                        }

                        var p = JObject.FromObject(json.result);

                        ApplicationData.Current.LocalSettings.Values["ConsumableSerialNumber"] = p["serialNumber"]?.ToString(); ;
                        ApplicationData.Current.LocalSettings.Values["ConsumableType"] = p["type"]?.ToString();
                        ApplicationData.Current.LocalSettings.Values["ConsumableUsedTimes"] = p["usedTimes"]?.ToString();

                        this.Hide();
                    }
                    catch (Exception e)
                    {
                        txtRet.Text = "未知的异常";
                    }
                   
                });
            }));

          
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
