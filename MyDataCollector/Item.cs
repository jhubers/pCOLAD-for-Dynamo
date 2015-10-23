using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MyDataCollector
{
    public class Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string Value { get; set; }
        public Item(string value)
        {
            Value = value;
            NotifyPropertyChanged("Value");
        }
        public bool IsChanged { get;  set; }
        public void SetChanged()
        {
            IsChanged = true;
            NotifyPropertyChanged("IsChanged");
        }
        public void SetSame()
        {
            IsChanged = false;
            NotifyPropertyChanged("IsChanged");
        }
        public override string ToString()
        {
             NotifyPropertyChanged("Value");           
            return Value;

        }
        public override bool Equals(object other)
        {
            var item = other as Item;
            if (item == null)
            {
                NotifyPropertyChanged("Value");
                return false;
            }
            NotifyPropertyChanged("Value");
            return item.Value == Value;
        }
        public override int GetHashCode()
        {
            if (Value == null) return base.GetHashCode();
            return Value.GetHashCode();
        }
    }
}
