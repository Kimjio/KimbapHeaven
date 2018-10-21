using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236 에 나와 있습니다.

namespace KimbapHeaven
{
    public sealed partial class TableControl : UserControl
    {
        private TableData TargetTable;
        /// <summary>
        /// 현제 선택된 음식데이터를 가져오거나 설정합니다.
        /// </summary>
        private FoodData SelectedFoodData;
        private int total;

        /// <summary>
        /// 결제되면 발생합니다.
        /// </summary>
        public event PayEventHandler Pay;

        private int TotalPrice
        {
            get
            {
                return total;
            }

            set
            {
                total = value;
                TotalPrice_ValueChanged();
            }
        }


        public TableControl()
        {
            InitializeComponent();

            CloseButton.Click += CloseButton_Click;
            FoodTypePivot.SelectionChanged += FoodTypePivot_SelectionChanged;
        }

        private void TotalPrice_ValueChanged()
        {
            TotalBox.Text = TotalPrice.ToString();
        }

        private void FoodTypePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Pivot) sender).SelectedIndex)
            {
                case 0:
                    FoodGridView.ItemsSource = Utils.GetMenu();
                    break;
                case 1:
                    FoodGridView.ItemsSource = Utils.GetMenu(FoodData.Type.NEW);
                    break;
                case 2:
                    FoodGridView.ItemsSource = Utils.GetMenu(FoodData.Type.KIMBAP);
                    break;
                case 3:
                    FoodGridView.ItemsSource = Utils.GetMenu(FoodData.Type.MEAL);
                    break;
                case 4:
                    FoodGridView.ItemsSource = Utils.GetMenu(FoodData.Type.FLOURBASED);
                    break;
                case 5:
                    FoodGridView.ItemsSource = Utils.GetMenu(FoodData.Type.PORKCUTLET);
                    break;
                case 6:
                    FoodGridView.ItemsSource = Utils.GetMenu(FoodData.Type.SEASON);
                    break;
            }
        }

        public void Show(TableData tableData)
        {
            TargetTable = tableData;
            AddFoodDatas(TargetTable.FoodDatas);
            SetTableIndex(TargetTable.Index);
            SetOrderedTime(TargetTable.OrderedDateTime);
            Visibility = Visibility.Visible;
        }

        private void AddFoodDatas(List<FoodData> foodDatas)
        {
            foreach (FoodData foodData in foodDatas)
            {
                FoodList.Items.Add(foodData);
                TotalPrice += foodData.DefaultPrice;
            }
        }

        public void SetFoodImage(BitmapImage image)
        {
            FoodImage.Source = image;
        }

        public void SetMenuData(List<FoodData> foodDatas)
        {
            FoodGridView.ItemsSource = foodDatas;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void PayButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog content = new ContentDialog()
            {
                Title = "결제 확인",
                Content = FoodList.Items.ToStringList() +
                Environment.NewLine +
                "총 가격: " + TotalPrice,
                PrimaryButtonText = "결제",
                CloseButtonText = "취소"
            };
            ContentDialogResult result = await content.ShowAsync();
            if (result.Equals(ContentDialogResult.Primary))
            {
                Pay?.Invoke(Utils.ConvertListToFoodDataList(FoodList.Items.ToList()), TargetTable);
                TargetTable.OrderedDateTime = DateTime.Now;
                Close();
            }
        }

        public void SetTableIndex(int index)
        {
            TableIndexBox.Text = index.ToString();
        }

        public void SetOrderedTime(DateTime dateTime)
        {
            if (dateTime.Equals(DateTime.MinValue))
                return;
            OrderedTimeBox.Text = dateTime.ToString("d일 tt h:mm");
            OrderedLabel.Visibility = Visibility.Visible;
        }

        public void ClearAll()
        {
            TargetTable = null;
            Pay = null;
            TableIndexBox.Text = TableIndexBox.Text.Remove(0);
            if (OrderedLabel.Visibility == Visibility.Visible)
            {
                OrderedTimeBox.Text = OrderedTimeBox.Text.Remove(0);
                OrderedLabel.Visibility = Visibility.Collapsed;
            }
            Clear();
        }

        private void Clear()
        {
            FoodList.Items.Clear();
            TotalPrice = 0;
            SelectedFoodData = null;
            FoodGridView.SelectedItem = null;
            FoodImage.Source = null;
        }

        public void Close()
        {
            ClearAll();
            Visibility = Visibility.Collapsed;
        }

        private void FoodList_ItemClick(object sender, ItemClickEventArgs e)
        {
            FoodData foodData = (FoodData) e.ClickedItem;
            SelectedFoodData = foodData;
            SetFoodImage(foodData.Image);
        }

        private void FoodGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            FoodData foodData = (FoodData) e.ClickedItem;
            if (!Utils.ContainsFoodDataByName(FoodList.Items, foodData.Name))
            {
                FoodData cloneData = (FoodData) foodData.Clone();
                cloneData.Count = 1;

                FoodList.Items.Add(cloneData);
                FoodList.SelectedItem = cloneData;
                TotalPrice += cloneData.Price;
            }
            SelectedFoodData = (FoodData) FoodList.SelectedItem;
            SetFoodImage(foodData.Image);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFoodData != null)
            {
                SelectedFoodData.Count++;
                SelectedFoodData.Price += SelectedFoodData.DefaultPrice;
                TotalPrice += SelectedFoodData.DefaultPrice;
            }
        }

        private void SubButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFoodData != null)
            {
                if (SelectedFoodData.Count > 1)
                {
                    SelectedFoodData.Count--;
                    SelectedFoodData.Price -= SelectedFoodData.DefaultPrice;
                    TotalPrice -= SelectedFoodData.DefaultPrice;
                }
            }
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFoodData != null)
            {
                FoodList.Items.Remove(SelectedFoodData);
                TotalPrice -= SelectedFoodData.Price;
                SelectedFoodData = null;
                if (!FoodList.Items.Any())
                {
                    FoodImage.Source = null;
                }
            }
        }

        private void ItemCloseButton_Click(object sender, RoutedEventArgs e)
        {
            UserButton button = (UserButton) sender;

            foreach (FoodData foodData in FoodList.Items)
            {
                if (foodData.Name == button.ID)
                {
                    FoodList.Items.Remove(foodData);
                    TotalPrice -= foodData.Price;
                    if (!FoodList.Items.Any())
                    {
                        FoodImage.Source = null;
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// PayEvent 이벤트를 처리할 메서드를 나타냅니다.
    /// </summary>
    /// <param name="foodDatas">추가된 음식 목록입니다.</param>
    /// <param name="tableData">선택된 테이블입니다.</param>
    public delegate void PayEventHandler(List<FoodData> foodDatas, TableData tableData);
}
