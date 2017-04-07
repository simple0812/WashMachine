using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WashMachine.Controls
{
    public sealed partial class QrCodeInfo : UserControl
    {
        public QrCodeInfo()
        {
            this.InitializeComponent();
            Init();
        }

        public void Init()
        {
            var p = ApplicationData.Current.LocalSettings.Values;
            if (p.ContainsKey("ConsumableSerialNumber") && p.ContainsKey("ConsumableType") && p.ContainsKey("ConsumableUsedTimes"))
            {
                txtNo.Text = p["ConsumableSerialNumber"].ToString();
                txtName.Text = p["ConsumableType"].ToString();
                txtTimes.Text = p["ConsumableUsedTimes"].ToString();

                txtNone.Visibility = Visibility.Collapsed;
                spEx.Visibility = Visibility.Visible;
            }
            else
            {
                txtNone.Visibility = Visibility.Visible;
                spEx.Visibility = Visibility.Collapsed;
            }

        }

        public void Clear()
        {
            var p = ApplicationData.Current.LocalSettings.Values;
            if (p.ContainsKey("ConsumableType"))
            {
                p.Remove("ConsumableType");
            }

            if (p.ContainsKey("ConsumableSerialNumber"))
            {
                p.Remove("ConsumableSerialNumber");
            }

            if (p.ContainsKey("ConsumableUsedTimes"))
            {
                p.Remove("ConsumableUsedTimes");
            }

            Init();
        }

        private async void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await new QrCodeDialog().ShowAsync();
            Init();
        }

    }
}
