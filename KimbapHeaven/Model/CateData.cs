using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KimbapHeaven
{
    public class CateData : INotifyPropertyChanged
    {
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        private string name;
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
        private int count;
        public int TotalPrice
        {
            get
            {
                return totalPrice;
            }

            set
            {
                totalPrice = value;
                OnPropertyChanged("TotalPrice");
            }
        }
        private int totalPrice;

        public event PropertyChangedEventHandler PropertyChanged;

        public CateData(string Name, int Count, int TotalPrice)
        {
            this.Name = Name;
            this.Count = Count;
            this.TotalPrice = TotalPrice;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
