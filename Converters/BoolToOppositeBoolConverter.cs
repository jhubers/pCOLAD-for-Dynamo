﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace pCOLADnamespace
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToOppositeBoolConverter : IValueConverter
    {
        public Boolean TrueValue { get; set; }
        public Boolean FalseValue { get; set; }

        public BoolToOppositeBoolConverter()
        {
            // set defaults
            FalseValue = false;
            TrueValue = true;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
