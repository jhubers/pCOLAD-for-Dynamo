﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace pCOLADnamespace
{
    public class MyImage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private string myImagePath = "";
        public string MyImagePath
        {
            get { return myImagePath; }
            set
            {
                myImagePath = value;
                NotifyPropertyChanged("MyImagePath");
            }
        }
        //constructor
        public MyImage(string ip)
        {
            MyImagePath = ip;
        }
    }
}
