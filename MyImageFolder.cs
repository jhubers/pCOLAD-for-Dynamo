using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace pCOLADnamespace
{
    public class MyImageFolder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private string myImageFolderPath = "";
        public string MyImageFolderPath
        {
            get { return myImageFolderPath; }
            set
            {
                myImageFolderPath = value;
                NotifyPropertyChanged("MyImageFolderPath");
            }
        }
        private List<MyImage> myImageList = new List<MyImage>();
        public List<MyImage> MyImageList
        {
            get { return myImageList; }
            set
            {
                myImageList = value;
                NotifyPropertyChanged("MyImageList");
            }
        }
        public MyImageFolder(string fp)
        {
            this.MyImageFolderPath = fp;
        }
    }
}

