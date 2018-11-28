using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace KimbapHeaven
{
    public sealed partial class StateControl : UserControl
    {
        ObservableCollection<CateData> cateDatas = new ObservableCollection<CateData>();
        ObservableCollection<FoodData> foodDatas = new ObservableCollection<FoodData>();

        public StateControl()
        {
            InitializeComponent();
            Update();
            AllTotal.Text = StateManager.GetAllTotalPrice().ToString();
        }

        public void Update()
        {
            cateDatas.Clear();
            foreach (FoodData.Type type in Utils.FoodTypes)
            {
                cateDatas.Add(new CateData(FoodData.ToStringTypeKR(type), StateManager.GetCateCount(type), StateManager.GetCateTotalPrice(type)));
            }

            foodDatas.Clear();
            foreach (FoodData food in StateManager.GetAllFoodDatas())
            {
                foodDatas.Add(food);
            }

            AllTotal.Text = StateManager.GetAllTotalPrice().ToString();
            CurrentTotal.Text = StateManager.GetCurrentTotalPrice().ToString();
            CardPayCount.Text = StateManager.GetPayTypeCount(StateManager.PayType.Card).ToString();
            CashPayCount.Text = StateManager.GetPayTypeCount(StateManager.PayType.Cash).ToString();
        }
    }
}
