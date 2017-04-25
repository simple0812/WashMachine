using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class Header : UserControl
    {
        public Header()
        {
            this.InitializeComponent();
            var ver = Package.Current.Id.Version;
            txtCurrTime.Text = $"版本号：{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
        }
    }
}
