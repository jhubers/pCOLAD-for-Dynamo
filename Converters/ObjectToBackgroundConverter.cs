using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace pCOLADnamespace
{
    [ValueConversion(typeof(object), typeof(SolidColorBrush))]
    public class ObjectToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush b = new SolidColorBrush(Colors.Transparent);
            if (value==null)
            {
                //b = Brushes.Transparent;
                return b;
            }
            //Type t = value.GetType();
            //if (value.GetType()==typeof(Boolean)) //the IsChanged property of Item is bound to DataTemplate x:Key="changedCells"
            //    //but shouldn't you check if field Obstruction is different from oldDataTable!!!
            //    //how can you get the row or the item
            //{
            //    if ((bool)value)
            //    {
            //        b = Brushes.Pink;
            //    }
            //    return b;
            //}
            if (value.GetType()!=typeof(MyDataCollector.Item))
            {
                //b = Brushes.Transparent;
                //would it be possible to check if a comment has changed after typing in the xaml?
                //check e.g. what is the targetType, the parameter, value is the text in the cell
                // targetType is the Brush, parameter is null, maybe find the item with the text
                // but then if some comments the same you get a problem. So use a different converter for the comments!
                // e.g. check when user starts typing with content when leaving the cell
                return b;
            }
            var item = (MyDataCollector.Item)value;
            if (item.IsChanged)
            {
                b = Brushes.Pink;
            }
            return b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
