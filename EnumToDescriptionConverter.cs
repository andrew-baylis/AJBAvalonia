// EnumToDescriptionConverter.cs
//  Andrew Baylis
//  Created: 23/08/2025

#region using

using System.ComponentModel;
using System.Globalization;
using Avalonia.Data.Converters;

#endregion

namespace AJBAvalonia;

public class EnumToDescriptionConverter : IValueConverter
{
    #region Private Methods

    private string GetEnumDescription(object enumerationValue)
    {
        var type = enumerationValue.GetType();

        if (!type.IsEnum)
        {
            throw new ArgumentException(@"EnumerationValue must be of Enum type", nameof(enumerationValue));
        }

        var enumString = enumerationValue.ToString();
        if (!string.IsNullOrEmpty(enumString))
        {
            var memberInfo = type.GetMember(enumString);

            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute) attrs[0]).Description;
                }
            }
        }

        //If we have no description attribute, just return the ToString of the enum
        return enumString ?? string.Empty;
    }

    #endregion

    #region IValueConverter Members

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum e)
        {
            return GetEnumDescription(e);
        }

        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion
}