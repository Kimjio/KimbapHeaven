using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace KimbapHeaven
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 메인 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public TableViewModel ViewModel { get; }

        public MainPage()
        {
            InitializeComponent();
            
            #region TitleBar
            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            formattableTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            formattableTitleBar.ButtonForegroundColor = Colors.Black;

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            TitleBar.Height = 32;

            Window.Current.Activated += Current_Activated;

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            #endregion

            #region Timer
            DispatcherTimer dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            dispatcherTimer.Tick += Timer_Tick;
            dispatcherTimer.Start();
            #endregion

            ViewModel = new TableViewModel();
        }

        #region Timer Method
        private void Timer_Tick(object sender, object e)
        {
            Time.Text = DateTime.Now.ToString("tt h:mm");
        }
        #endregion

        #region TitleBar Methods
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                MainTitleBar.Opacity = 1;
            }
            else
            {
                MainTitleBar.Opacity = 0.5;
            }
        }

        void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar titleBar, object args)
        {
            TitleBar.Visibility = titleBar.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            TitleBar.Height = sender.Height;
        }
        #endregion

        #region ItemOnClick
        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Enter 입력 시 Visible 이면 넘어가기
            if (TableControlPanel.Visibility != Visibility.Visible)
            {
                TableData tableData = (TableData) e.ClickedItem;
                TableControlPanel.Pay += TableControlPanel_Pay;
                TableControlPanel.Order += TableControlPanel_Order;
                TableControlPanel.Canceled += TableControlPanel_Canceled;

                TableControlPanel.Show(tableData);
            }
        }

        #endregion

        private void TableControlPanel_Order(List<FoodData> foodDatas, TableData tableData)
        {
            tableData.FoodDatas = foodDatas;
        }

        private void TableControlPanel_Canceled(TableData tableData)
        {
            tableData.Clear();
        }

        private void TableControlPanel_Pay(TableData tableData)
        {
            tableData.Clear();
            StateControlPanel.Update();
        }
    }

    public class TableViewModel
    {
        public List<TableData> TableDatas;

        public TableViewModel() : this(Settings.GetInt("seat_size", 10)) { }

        public TableViewModel(int seatSize)
        {
            TableDatas = new List<TableData>();
            for (int i = 1; i <= seatSize; i++)
            {
                TableData tableData = new TableData(i);
                TableDatas.Add(tableData);
            }
        }
    }
}
