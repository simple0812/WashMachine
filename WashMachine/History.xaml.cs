using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using WashMachine.Enums;
using WashMachine.Models;
using WashMachine.ViewModels;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace WashMachine
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class History : Page
    {
        public History()
        {
            this.DataContext = new HistoryViewModel();
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.DataContext = null;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var flow = btn?.Tag as WashFlow;

            if (flow?.FlowType == FlowEnum.Full)
            {
                this.Frame.Navigate(typeof (FullFlow), flow);
            }
            else  if(flow?.FlowType == FlowEnum.Simple)
            {
                this.Frame.Navigate(typeof(SimpleFlow), flow);
            }
        }
    }
}
