using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace MyDataCollector
{
    [IsVisibleInDynamoLibrary(false)]
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
        public string myImageFileName {get;set;}
        private string myImagePath = "";
        public string MyImagePath
        {
            get { return myImagePath; }
            set
            {
                myImagePath = value;
                myImageFileName = Path.GetFileName(myImagePath);
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
