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
    public class ObjectToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush b = new SolidColorBrush(Colors.Transparent);
            if (value==null)
            {
                //b = Brushes.Transparent;
                return b;
            }
            if (value.GetType()!=typeof(MyDataCollector.Item))
            {
                //b = Brushes.Transparent;
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
