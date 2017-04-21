using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WashMachine.Controls;
using WashMachine.Enums;
using WashMachine.Models;
using WashMachine.Services;
using WashMachine.ViewModels;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace WashMachine
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MySettings : Page
    {
        public MySettings()
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

        private async void Btn_OnClick(object sender, RoutedEventArgs e)
        {
            var file = await DownloadsFolder.CreateFileAsync("download.csv", CreationCollisionOption.GenerateUniqueName);
            var strA = new List<string>();
            strA.Add("名称,清洗量,清洗速度,浓缩体积,浓缩速度,浓缩次数,收集量,收集速度,收集次数,开始时间,结束时间");

            strA.AddRange(WashRecordService.Instance.GetList().Select(each => $"{each.Name},{each.WashVolume},{each.WashSpeed}," +
                                                       $"{each.ConcentrateVolume},{each.ConcentrateSpeed},{each.ConcentrateTimes}," +
                                                       $"{each.CollectVolume},{each.CollectSpeed},{each.CollectTimes}," +
                                                       $"{each.StartTime.ToString("yyyy-MM-dd HH:mm:ss")},{each.EndTime.ToString("yyyy-MM-dd HH:mm:ss")}"));

            await FileIO.WriteLinesAsync(file, strA, UnicodeEncoding.Utf8);
            new TopPopup().Show("下载完成,请在下载目录中WashMachine文件夹下查找");
        }
    }
}
