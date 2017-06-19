﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace App.View
{
    public class TrueFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (parameter.GetType().IsArray == false)
                throw new InvalidOperationException("The paramter is not an array");

            var list = (parameter as IEnumerable).Cast<object>().ToList();
            return (bool)value ? list[0] : list[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}