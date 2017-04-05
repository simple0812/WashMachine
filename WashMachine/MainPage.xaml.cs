using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WashMachine.Libs;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace WashMachine
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int _preSelectNavigation = -1;
        bool _ignoreNavigation = false;
       
        public MainPage()
        {
            this.InitializeComponent();
            mainNavigationList.SelectedIndex = 1;
            Logic.Instance.CommunicationHandler += Instance_CommunicationHandler;
        }

        private async void Instance_CommunicationHandler(object arg1, Models.CommunicationEventArgs arg2)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,  () =>
            {
                if (mainFrame.Content == null) return;

                //操作流程结束或者取消了
                if (arg1 == null && arg2?.Description == "end")
                {
                    mainNavigationList.IsEnabled = true;
                    var type = mainFrame.Content.GetType();

                    if (type == typeof (SimpleFlow))
                    {
                        var x = mainFrame.Content as SimpleFlow;
                        x?.initData(null);
                    }

                    if (type == typeof (FullFlow))
                    {
                        var x = mainFrame.Content as FullFlow;
                        x?.initData(null);
                    }
                }
                else
                {
                    if (mainNavigationList.IsEnabled)
                        mainNavigationList.IsEnabled = false;

                    var type = mainFrame.Content.GetType();

                    if (type == typeof(SimpleFlow))
                    {
                        var x = mainFrame.Content as SimpleFlow;
                        x?.initData(arg2?.Data);
                    }

                    if (type == typeof(FullFlow))
                    {
                        var x = mainFrame.Content as FullFlow;
                        x?.initData(arg2?.Data);
                    }
                }
                
            });
        }

        private void ListBoxItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var tapped_item = sender as ListBoxItem;
            if (tapped_item?.Tag != null && tapped_item.Tag.ToString().Equals("0")) //汉堡按钮
            {
                mainSplitView.IsPaneOpen = !mainSplitView.IsPaneOpen;
            }
        }
        
        private void mainNavigationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ignoreNavigation)
            {
                _ignoreNavigation = false;
                return;
            }
            var tapped_item = mainNavigationList.SelectedItems[0] as ListBoxItem;
            if (tapped_item?.Tag != null && tapped_item.Tag.ToString().Equals("1")) //简单流程
            {
                mainSplitView.IsPaneOpen = false;
                _preSelectNavigation = mainNavigationList.SelectedIndex;
                mainFrame.Navigate(typeof(SimpleFlow));
            }
            if (tapped_item?.Tag != null && tapped_item.Tag.ToString().Equals("2")) //完整流程
            {
                mainSplitView.IsPaneOpen = false;
                _preSelectNavigation = mainNavigationList.SelectedIndex;
                mainFrame.Navigate(typeof(FullFlow));
            }

            if (tapped_item?.Tag != null && tapped_item.Tag.ToString().Equals("3")) //流程列表
            {
                mainSplitView.IsPaneOpen = false;
                _preSelectNavigation = mainNavigationList.SelectedIndex;
                mainFrame.Navigate(typeof(History));
            }
        }

        public void ShowNavigationBar(bool show)
        {
            mainSplitView.DisplayMode = show ? SplitViewDisplayMode.CompactOverlay : SplitViewDisplayMode.Overlay;
        }
        
       
        public void ShowNavigationBarOneTime()
        {
            mainSplitView.IsPaneOpen = true;
        }

        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            var type = e.Content.GetType();
            if (type == typeof (FullFlow))
            {
                mainNavigationList.SelectedIndex = 2;
            }
            if (type == typeof(SimpleFlow))
            {
                mainNavigationList.SelectedIndex = 1;
            }
            if (type == typeof(History))
            {
                mainNavigationList.SelectedIndex = 3;
            }
        }
    }
}
