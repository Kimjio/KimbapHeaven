using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace KimbapHeaven
{
    public class FoodData : ICloneable, INotifyPropertyChanged
    {
        public FoodData(Type type, string name, int price, int count, Uri uri)
        {
            FoodType = type;
            Name = name;
            Price = DefaultPrice = price;
            Count = count;
            ImageUri = uri;
        }

        public async Task<FoodData> PreloadImage(int indexCate, int indexFood, int cateMax, int foodMax, Utils.UpdateProgress progress)
        {
            Image = await Utils.ResizedImage(ImageUri, 512, 512, indexCate, indexFood, cateMax, foodMax, progress);
            return this;
        }
        
        public enum Type
        {
            NEW = 31, //신메뉴
            KIMBAP = 25, //김밥
            MEAL = 26, //식사
            FLOURBASED = 27, //분식
            PORKCUTLET = 28, //돈까스
            SEASON = 29, //계절
            UNDIFINED = 0
        }
        public Type FoodType { get; set; }
        public string Name { get; set; }
        private int price;
        public int DefaultPrice { get; }
        public int Price
        {
            get
            {
                return price;
            }
            set
            {
                price = value;
                OnPropertyChanged("Price");
            }
        }
        private int count;
        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                count = value;
                OnPropertyChanged("Count");
            }
        }
        public BitmapImage Image { get; set; }

        private Uri ImageUri;

        public object Clone()
        {
            return MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return Name + " X " + Count;
        }
    }
}
