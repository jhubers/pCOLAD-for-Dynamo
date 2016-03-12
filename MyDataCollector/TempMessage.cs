using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDataCollector
{
    public class TempMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public string messageString;
        public string MessageString
        {
            get { return messageString; }
            set
            {
                messageString = value;
                NotifyPropertyChanged("MessageString");
            }
        }
        public TempMessage()
        {
            //messageString = "Default message...";
        }
    }
}
