using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace KimbapHeaven
{
    public sealed partial class SettingsControl : UserControl
    {
        private int seatSize;
        private Utils.ImageQuality imageQuality;

        public SettingsControl()
        {
            InitializeComponent();

            BindSettings();

            #region ResetButton
            ResetButton.Click += async (sender, e) =>
            {
                Utils.ClearFile();
                await CoreApplication.RequestRestartAsync("");
            };
            #endregion

            #region SeatSlider
            SeatSliderText.Text = seatSize.ToString();
            SeatSlider.Value = seatSize;
            SeatSlider.ValueChanged += SeatSlider_ValueChanged;
            #endregion

            #region ImageQuality
            switch (imageQuality)
            {
                case Utils.ImageQuality.High:
                    HighRadio.IsChecked = true;
                    break;

                case Utils.ImageQuality.Middle:
                    MiddleRadio.IsChecked = true;
                    break;

                case Utils.ImageQuality.Low:
                    LowRadio.IsChecked = true;
                    break;
            }
            #endregion
        }

        private void ImageQualityRadio_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton) sender;
            Settings.PutString("ImageQuality", radioButton.Tag.ToString());
        }

        private void BindSettings()
        {
            seatSize = Settings.GetInt("seat_size", 10);
            imageQuality = Utils.GetCurrentImageQuality();
        }

        private void SeatSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Settings.PutInt("seat_size", (int) e.NewValue);
            SeatSliderText.Text = e.NewValue.ToString();
            //ViewModel.Update((int) e.NewValue);
        }
    }
}
