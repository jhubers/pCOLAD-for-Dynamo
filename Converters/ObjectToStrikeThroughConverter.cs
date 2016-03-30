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
    [ValueConversion(typeof(object), typeof(TextDecorations))]
    class ObjectToStrikeThroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var td = TextDecorations.Strikethrough;
            if (value == null)
            {
                return null;
            }
            if (value.GetType() != typeof(MyDataCollector.Item))
            {
                return null;
            }
            var item = (MyDataCollector.Item)value;
            if (item.IsDeleted)
            {
                return TextDecorations.Strikethrough;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
