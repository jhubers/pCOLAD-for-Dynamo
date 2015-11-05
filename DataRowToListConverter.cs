using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Data;


namespace pCOLADnamespace
{
    [ValueConversion(typeof(DataRowView), typeof(List<Image>))]
    class DataRowToListConverter : IValueConverter
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
                return item[0];
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
