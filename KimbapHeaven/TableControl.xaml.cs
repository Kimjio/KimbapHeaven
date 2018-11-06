using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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

        /*
         코카콜라 - 8801094017200
         펩시콜라 - 8801056070809
        */
        private string receivedBarcodeChars = string.Empty;

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
            Opacity = 0;
            CloseButton.Click += CloseButton_Click;
            FoodTypePivot.SelectionChanged += FoodTypePivot_SelectionChanged;
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;
        }

        //see https://stackoverflow.com/a/15325499
        private static Regex _gtinRegex = new Regex("^(\\d{8}|\\d{12,14})$");

        /// <summary>
        /// 올바른 바코드인지 확인하는 메소드
        /// </summary>
        /// <param name="code">받아온 바코드</param>
        /// <returns>바코드 정상 여부</returns>
        public static bool IsValidGtin(string code)
        {
            if (!(_gtinRegex.IsMatch(code))) return false;
            code = code.PadLeft(14, '0');
            int[] mult = Enumerable.Range(0, 13).Select(i => ((int) (code[i] - '0')) * ((i % 2 == 0) ? 3 : 1)).ToArray();
            int sum = mult.Sum();
            return (10 - (sum % 10)) % 10 == int.Parse(code[13].ToString());
        }

        //바코드기 입력 후 끝에 Enter 키 입력 발생 ("\r\n")
        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {

            if (Visibility == Visibility.Collapsed) return;
            char inputChar = (char) args.KeyCode;
            if (inputChar == '\r')
            {
                if (IsValidGtin(receivedBarcodeChars))
                {
                    Debug.WriteLine("OK: " + receivedBarcodeChars);
                    FoodData foodData = Utils.FindFoodDataByEANCode(receivedBarcodeChars);
                    FoodData listedFood;
                    if ((listedFood = Utils.FindFoodDataByName(FoodList.Items, foodData.Name)) == null)
                    {
                        FoodData cloneData = (FoodData) foodData.Clone();
                        cloneData.Count = 1;

                        FoodList.Items.Add(cloneData);
                        FoodList.SelectedItem = cloneData;
                        TotalPrice += cloneData.DefaultPrice;
                        PayButton.IsEnabled = true;
                    }
                    else
                    {
                        listedFood.Count++;
                        listedFood.Price += listedFood.DefaultPrice;
                        TotalPrice += listedFood.DefaultPrice;
                    }
                    SelectedFoodData = (FoodData) FoodList.SelectedItem;
                    SetFoodImage(foodData.Image);
                }

                receivedBarcodeChars = string.Empty;
            }

            if (char.IsDigit(inputChar))
                receivedBarcodeChars += inputChar;
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
            AddFoodDatas(TargetTable.FoodDatas.Clone());
            SetTableIndex(TargetTable.Index);
            SetOrderedTime(TargetTable.OrderedDateTime);
            PayButton.IsEnabled = FoodList.Items.Any();
            Visibility = Visibility.Visible;
            Opacity = 1;
        }

        private void AddFoodDatas(List<FoodData> foodDatas)
        {
            foreach (FoodData foodData in foodDatas)
            {
                FoodList.Items.Add(foodData);
                TotalPrice += foodData.Price;
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
                Content = FoodList.Items.ToStringWithNewLine() +
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
            //Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;
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
            PayButton.IsEnabled = false;
        }

        public void Close()
        {
            ClearAll();
            Visibility = Visibility.Collapsed;
            Opacity = 0;
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
                PayButton.IsEnabled = true;
            }
            else
            {
                FoodData listedFood = Utils.FindFoodDataByName(FoodList.Items, foodData.Name);
                listedFood.Count++;
                listedFood.Price += listedFood.DefaultPrice;
                TotalPrice += listedFood.DefaultPrice;
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
                    PayButton.IsEnabled = false;
                }
            }
        }

        private void ItemCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button) sender;

            foreach (FoodData foodData in FoodList.Items)
            {
                if (foodData.Name == button.Tag.ToString())
                {
                    FoodList.Items.Remove(foodData);
                    TotalPrice -= foodData.Price;
                    if (!FoodList.Items.Any())
                    {
                        FoodImage.Source = null;
                        PayButton.IsEnabled = false;
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
