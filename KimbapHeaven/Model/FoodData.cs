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
        public FoodData(Type type, string name, int price, int count, Uri uri) : this(type, name, string.Empty, price, count, uri) { }

        public FoodData(Type type, string name, string eanCode, int price, int count, Uri uri)
        {
            FoodType = type;
            Name = name;
            EANCode = eanCode;
            Price = 0;
            DefaultPrice = price;
            Count = count;
            ImageUri = uri;
        }

        public async Task<FoodData> PreloadImage(int indexCate, int indexFood, int cateMax, int foodMax, Utils.UpdateProgress progress)
        {
            Image = await Utils.ResizedImage(ImageUri, indexCate, indexFood, cateMax, foodMax, progress);
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

        public static Type ParseType(string typeStr)
        {
            if (typeStr == null) throw new ArgumentNullException("typeStr == null");

            Type type;

            switch (typeStr.ToLower())
            {
                case "new":
                case "31":
                    type = Type.NEW;
                    break;
                case "kimbap":
                case "25":
                    type = Type.KIMBAP;
                    break;
                case "meal":
                case "26":
                    type = Type.MEAL;
                    break;
                case "flourbased":
                case "27":
                    type = Type.FLOURBASED;
                    break;
                case "porkcutlet":
                case "28":
                    type = Type.PORKCUTLET;
                    break;
                case "season":
                case "29":
                    type = Type.SEASON;
                    break;
                case "undifined":
                case "0":
                    type = Type.UNDIFINED;
                    break;
                default:
                    throw new FormatException();
            }

            return type;
        }

        public static string ToStringTypeKR(Type type)
        {
            string typeStr = string.Empty;

            switch (type)
            {
                case Type.NEW:
                    typeStr = "신메뉴";
                    break;
                case Type.KIMBAP:
                    typeStr = "김밥";
                    break;
                case Type.MEAL:
                    typeStr = "식사";
                    break;
                case Type.FLOURBASED:
                    typeStr = "분식";
                    break;
                case Type.PORKCUTLET:
                    typeStr = "돈까스";
                    break;
                case Type.SEASON:
                    typeStr = "계절";
                    break;
                case Type.UNDIFINED:
                    typeStr = "기타";
                    break;
            }

            return typeStr;
        }

        public Type FoodType { get; set; }
        public string Name { get; set; }
        public int Categorie { get; set; }
        public string EANCode { get; set; }
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

        private readonly Uri ImageUri;

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
