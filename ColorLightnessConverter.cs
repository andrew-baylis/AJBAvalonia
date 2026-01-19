// ColorLightnessConverter.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;

#endregion

namespace AJBAvalonia;

/// <summary>
/// create a resource with this as the converter - either set the Ratio here or in the ConverterParameter in the binding
/// </summary>
public class ColorLightnessConverter : IValueConverter
{
    #region Properties

    public double Ratio { get; set; } = 1d;

    #endregion

    #region Private Methods

    private Color MakeNewColor(Color source)
    {
        var hsl = source.ToHsl();
        var lightness = hsl.L * Math.Clamp(Ratio, 0, 10);
        var lighHSL = HslColor.FromHsl(hsl.H, hsl.S, Math.Clamp(lightness, 0, 1));
        return lighHSL.ToRgb();
    }

    #endregion

    #region IValueConverter Members

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is double d)
        {
            Ratio = d;
        }

        Color? newColor = null;
        if (value is Color c)
        {
            newColor = MakeNewColor(c);
        }
        else if (value is ISolidColorBrush s)
        {
            newColor = MakeNewColor(s.Color);
        }

        if (newColor != null)
        {
            if (targetType == typeof(IBrush))
            {
                return new ImmutableSolidColorBrush(newColor.Value);
            }

            if (targetType == typeof(Color))
            {
                return newColor.Value;
            }
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion
}