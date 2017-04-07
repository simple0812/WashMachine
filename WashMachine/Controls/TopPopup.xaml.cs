using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class TopPopup : UserControl
    {
        public TopPopup()
        {
            this.InitializeComponent();
            sbAlert.Completed += SbAlert_Completed;
        }

        private void SbAlert_Completed(object sender, object e)
        {
            pp.IsOpen = false;
            sbAlert.Completed -= SbAlert_Completed;
        }

        public void Show(string text)
        {
            pp.IsOpen = true;

            tbAlert.Text = text;
            sbAlert.Begin();
        }
    }
}
