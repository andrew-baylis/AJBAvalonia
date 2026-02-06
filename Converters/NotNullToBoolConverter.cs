// NotNullToBoolConverter.cs
//  Andrew Baylis
//  Created: 06/02/2026

using System.Globalization;
using Avalonia.Data.Converters;

namespace AJBAvalonia.Converters;

public class NotNullToBoolConverter:IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}