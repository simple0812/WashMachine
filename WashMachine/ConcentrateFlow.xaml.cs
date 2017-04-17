using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WashMachine.Controls;
using WashMachine.Enums;
using WashMachine.Libs;
using WashMachine.Models;
using WashMachine.Services;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace WashMachine
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ConcentrateFlow : Page
    {
        public ConcentrateFlow()
        {
            this.InitializeComponent();
        }

        private async void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
//            var p = ApplicationData.Current.LocalSettings.Values;
//            if (!(p.ContainsKey("ConsumableSerialNumber") && p.ContainsKey("ConsumableType") &&
//                p.ContainsKey("ConsumableUsedTimes")))
//            {
//                new TopPopup().Show("请扫描二维码添加耗材");
//                return;
//            }

            int conVolume = int.TryParse(txtConVolume.Text, out conVolume) ? conVolume : -1;
            int conSpeed = int.TryParse(txtConSpeed.Text, out conSpeed) ? conSpeed : -1;
            var dir = tsPump2.IsOn ? DirectionEnum.Out : DirectionEnum.In;

            if (conVolume <= 0)
            {
                new TopPopup().Show("浓缩体积必须大于0");
                return ;
            }

            if (conSpeed <= 0)
            {
                new TopPopup().Show("浓缩速度必须大于0");
                return ;
            }

            spEdit.IsHitTestVisible = false;
            spEdit.Background = new SolidColorBrush(Colors.Gray);

            App.Status = SysStatusEnum.Starting;
            btnStart.IsEnabled = false;

            await Logic.Instance.pump1.SetParams(conSpeed, conVolume, dir).StartAsync();

            App.Status = SysStatusEnum.Completed;
            btnStart.IsEnabled = true;
            Logic.Instance.End();
            txtRet.Text = "操作完成";
        }

        private async void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            await Logic.Instance.Close();
            App.Status = SysStatusEnum.Discarded;
            Logic.Instance.End();
        }

        public void initData(DirectiveData data)
        {
            var p = data as PumpDirectiveData;
            if (p == null)
            {
                btnStart.IsEnabled = true;
                spEdit.IsHitTestVisible = true;
                spEdit.Background = new SolidColorBrush(Colors.White);
                pbConcentrate.IsActive = false;
                qrCtrl.Clear();
                return;
            };

            txtWeight.Text = ((double)p.TimeInterval / 10).ToString("#.#");

            txt.Text = $"泵{p.DeviceId}流速{p.FlowRate}重量{p.TimeInterval}-----{p.DirectiveId}---{p.DirectiveType}";

            if (p.DirectiveType == DirectiveTypeEnum.TryStart)
            {
                pbConcentrate.IsActive = p.DeviceId == 1;
            }
        }
    }
}
