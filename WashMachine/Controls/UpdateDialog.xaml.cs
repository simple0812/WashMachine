using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WashMachine.Libs;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace WashMachine.Controls
{
    public sealed partial class UpdateDialog : ContentDialog
    {
        public UpdateDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var uri = new Uri("appupdatex://xx");

            // Launch the URI.
            var success = await Launcher.LaunchUriAsync(uri);
            if (!success)
            {
                txt.Text = "AppUpdate can not startup";
                Debug.WriteLine("AppUpdate can not startup");
                return;
            };

            await Task.Delay(1000);

            AutoUpdate.Update();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
