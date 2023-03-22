using System;
using System.Globalization;
using System.Windows.Data;

namespace CryptoTools.Converters;

[ValueConversion(typeof(int), typeof(bool))]
public class IntToBoolValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is > 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}