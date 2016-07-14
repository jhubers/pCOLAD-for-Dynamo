using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MyDataCollector

{
    [IsVisibleInDynamoLibrary(false)]
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
        public string textValue;
        public string oldTextValue;
        public string TextValue 
        {
            get { return textValue; } 
            set
            {
                ////can I find out which cell was changed? And if is a comment set background to red!!! 
                //if (textValue != null && !textValue.Equals(value))
                //{
                //    //works but doesn't turn red immediately. Goes to RowIndex too...So could set a property there
                //    this.SetChanged();                    
                //}
                oldTextValue = textValue;                         
                textValue = value;
                NotifyPropertyChanged("TextValue");
            }
        }
        public Item(string value)
        {
            TextValue = value;
        }
        //public List<string> imageFileNameList { get; set; }
        public List<string> imageFileNameList;
        public List<string> ImageFileNameList
        {
            get { return imageFileNameList; }
            set
            {
                imageFileNameList = value;
            }
        }
        public List<MyImage> imageList;
        public List<MyImage> ImageList
        {
            get { return imageList; }
            set
            {
                imageList = value;
                //NotifyPropertyChanged("ImageList");
            }
        }
        public bool IsChanged { get;  set; }
        public void SetChanged()
        {
            IsChanged = true;
            IsMyChanged = false;
            NotifyPropertyChanged("IsChanged");
        }
        public bool IsMyChanged { get; set; }
        public void SetMyChanged()
        {
            IsMyChanged = true;
            IsChanged = false;
            NotifyPropertyChanged("IsChanged");
        }
        public bool IsDeleted{ get; set; }
        public void SetDeleted()
        {
            IsDeleted = true;
            NotifyPropertyChanged("IsDeleted");
        }

        public void SetSame()
        {
            IsChanged = false;
            IsMyChanged = false;
            NotifyPropertyChanged("IsChanged");
        }
        public override string ToString()
        {
             NotifyPropertyChanged("Value");           
            return textValue;

        }
        public override bool Equals(object other)
        {
            var item = other as Item;
            if (item == null)
            {
                NotifyPropertyChanged("TextValue");
                return false;
            }
            NotifyPropertyChanged("TextValue");
            return item.textValue == textValue;
        }
        public override int GetHashCode()
        {
            if (textValue == null) return base.GetHashCode();
            return textValue.GetHashCode();
        }
    }
}
