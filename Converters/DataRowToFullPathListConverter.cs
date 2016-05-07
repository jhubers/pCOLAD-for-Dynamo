using MyDataCollector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace pCOLADnamespace
{
    [ValueConversion(typeof(DataRowView), typeof(List<Image>))]
    class DataRowToFullPathListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<Image> li = new List<Image>();
            if (value == null)
            {
                return li;
            }
            else
            {
                var item = (DataRowView)value;
                //now the row consists only of items, but in the first column we want the list of images
                //which is now a property of Item, so in that case replace the row item by this property
                if (item[0].GetType() == typeof(Item))
                {
                    Item thisItem = (Item)item[0];
                    //to avoid to many images in viewing the History we should check
                    //if HistoryOn is true. But how to access it? Make it static? Yes!
                    //if (pSHARE.historyOn)
                    //{
                    //    //MessageBox.Show("HistoryOn is true...");
                    //    //a list of objects because of the binding
                    //    //because the binding is also used for the images in the first column
                    //    //we use a fake path e.g. put a space in front of myImagePath
                    //    //but it becomes very slow!!!
                    //    Item fakeItem = new Item(thisItem.textValue);                        
                    //    List<MyImage> fakeImageList = new List<MyImage>();
                    //    foreach (MyImage m in thisItem.imageList)
                    //    {
                    //        MyImage nm = new MyImage(" " + m.MyImagePath);
                    //        fakeImageList.Add(nm);
                    //    }
                    //    fakeItem.imageList = fakeImageList;
                    //    return fakeItem.ImageList;
                    //}
                    //else
                    //{
                        //because the binding is to the itemsource of the ItemsControl you have to provide
                        //a list of objects (in this case ImageList consist of MyImage objects), then inside 
                        //the ItemsControl the binding is to a property of these objects (for textblock myImageFileName
                        //for the image myImagePath.
                        return thisItem.ImageList; 
                    }
                //}
                else
                {
                    return item[0];
                }
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
