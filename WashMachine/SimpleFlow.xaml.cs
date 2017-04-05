﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using WashMachine.Enums;
using WashMachine.Libs;
using WashMachine.Models;
using WashMachine.Services;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace WashMachine
{
    public sealed partial class SimpleFlow : Page
    {
        public SimpleFlow()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.DataContext = null;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var flow = e.Parameter as WashFlow;

            this.DataContext = flow ?? App.WashFlow;

            btnStart.IsEnabled = App.Status != SysStatusEnum.Running;
        }

        private async void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            var washFlow = GetWashFlow();
            if (washFlow == null) return;
            this.DataContext = washFlow;

            spEdit.IsHitTestVisible = false;
            spEdit.Background = new SolidColorBrush(Colors.Gray);

            App.Status = SysStatusEnum.Starting;
            btnStart.IsEnabled = false;
            Logic.Instance.pump1.SetParams(washFlow.WashSpeed, washFlow.WashVolume);
            Logic.Instance.pump2.SetParams(washFlow.ConcentrateSpeed, washFlow.ConcentrateVolume);

            for (var i = 0; i < 3; i++)
            {
                txtRet.Text = $"第{i + 1}次加液开始...";
                await Logic.Instance.pump1.StartAsync();
                txtRet.Text = $"第{i + 1}次浓缩开始...";
                await Logic.Instance.pump2.StartAsync();
            }

            App.Status = SysStatusEnum.Completed;
            btnStart.IsEnabled = true;
            Logic.Instance.End();
            txtRet.Text = "操作完成";
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var washFlow = GetWashFlow();
            if (washFlow == null) return;

            string err = "";
            txtRet.Text = WashFlowService.Instance.Save(washFlow, out err) ? "保存成功" : err;
        }

        private async void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            await Logic.Instance.Close();
            App.Status = SysStatusEnum.Discarded;
            btnStart.IsEnabled = true;
            Logic.Instance.End();
        }

        public void initData(DirectiveData data)
        {
            var p = data as PumpDirectiveData;
            if (p == null)
            {
                spEdit.IsHitTestVisible = true;
                spEdit.Background = new SolidColorBrush(Colors.White);
                pbAppend.IsActive = false;
                pbConcentrate.IsActive = false;
                return;
            };

            txtWeight.Text = ((double) p.TimeInterval/10).ToString("#.#");

            txt.Text = $"泵{p.DeviceId}流速{p.FlowRate}重量{p.TimeInterval}-----{p.DirectiveId}---{p.DirectiveType}";

            if (p.DirectiveType == DirectiveTypeEnum.TryStart)
            {
                pbAppend.IsActive = false;
                pbConcentrate.IsActive = false;

                if (p.DeviceId == 1)
                {
                    pbAppend.IsActive = true;
                }
                else if (p.DeviceId == 2)
                {
                    pbConcentrate.IsActive = true;
                }
            }
        }

        private WashFlow GetWashFlow()
        {
            var name = txtName.Text;
            int washSpeed = int.TryParse(txtWashSpeed.Text, out washSpeed) ? washSpeed : -1;
            int washVolume = int.TryParse(txtWashVolume.Text, out washVolume) ? washVolume : -1;
            int conVolume = int.TryParse(txtConVolume.Text, out conVolume) ? conVolume : -1;
            int conSpeed = int.TryParse(txtConSpeed.Text, out conSpeed) ? conSpeed : -1;
            int conTimes = int.TryParse(txtConTimes.Text, out conTimes) ? conTimes : -1;

            if (string.IsNullOrEmpty(name))
            {
                txtRet.Text = "流程名称不能为空";
                return null;
            }

            if (washSpeed <= 0)
            {
                txtRet.Text = "加液速度必须大于0";
                return null;
            }

            if (washVolume <= 0)
            {
                txtRet.Text = "加液量必须大于0";
                return null;
            }
            if (conVolume <= 0)
            {
                txtRet.Text = "浓缩体积必须大于0";
                return null;
            }

            if (conSpeed <= 0)
            {
                txtRet.Text = "浓缩速度必须大于0";
                return null;
            }

            if (conTimes <= 0)
            {
                txtRet.Text = "浓缩次数必须大于0";
                return null;
            }

            var washFlow = new WashFlow();
            washFlow.Name = name;
            washFlow.WashVolume = washVolume;
            washFlow.WashSpeed = washSpeed;
            washFlow.ConcentrateSpeed = conSpeed;
            washFlow.ConcentrateVolume = conVolume;
            washFlow.ConcentrateTimes = conTimes;
            washFlow.FlowType = FlowEnum.Simple;

            var ctx = this.DataContext as WashFlow;

            if (ctx != null)
            {
                washFlow.Id = ctx.Id;
            }

            return washFlow;
        }
    }
}