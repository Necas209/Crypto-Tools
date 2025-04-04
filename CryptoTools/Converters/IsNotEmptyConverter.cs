using System;
using Microsoft.UI.Xaml.Data;

namespace CryptoTools.Converters;

public class IsNotEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is > 0;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}