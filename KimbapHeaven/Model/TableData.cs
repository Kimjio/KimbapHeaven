using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KimbapHeaven
{
    public class TableData : ICloneable, INotifyPropertyChanged
    {
        public TableData(int Index)
        {
            this.Index = Index;
            FoodDatas = new List<FoodData>();
        }

        public DateTime OrderedDateTime { get; set; }

        public int Index { get; }

        private List<FoodData> foodDatas;
        public List<FoodData> FoodDatas {
            get
            {
                return foodDatas;
            }

            set
            {
                foodDatas = value;
                OnPropertyChanged("FoodDatas");
            }
        }

        public void Clear()
        {
            OrderedDateTime = DateTime.MinValue;
            FoodDatas = new List<FoodData>();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
