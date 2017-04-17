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
using WashMachine.Enums;
using WashMachine.Protocols.Directives;
using WashMachine.Protocols.Helper;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace WashMachine
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ManualPage : Page
    {
        public ManualPage()
        {
            this.InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var bi = cbId.SelectedItem as ComboBoxItem;

            if (bi == null) return;
            var btn = sender as Button;
            int id = int.TryParse(bi.Tag.ToString(), out id) ? id : 1;

            int rate = int.TryParse(txtRate.Text, out rate) ? rate : 100;
            int angle = int.TryParse(txtAngel.Text, out angle) ? angle : 1;

            if (btn?.Content == null) return;
            try
            {
                switch (btn.Content.ToString())
                {
                    case "停止":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new TryPauseDirective(id));
                        break;
                    }

                    case "正转":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(id, rate, angle, (int)DirectionEnum.In));
                        break;
                    }

                    case "反转":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(id, rate, angle, (int)DirectionEnum.Out));
                        break;
                    }

                    default:
                        break;
                }

            }
            catch (Exception)
            {
                //
            }
        }
    }
}
