// SystemColorExtension.cs
//  Andrew Baylis
//  Created: 28/11/2025

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

public static class SystemColorExtension
{
    #region Static Methods

    public static double GetColorLuminance(this Color color)
    {
        //if RsRGB <= 0.03928 then R = RsRGB/12.92 else R = ((RsRGB+0.055)/1.055) ^ 2.4
        var r1 = color.R / 255.0;
        var r = r1 <= 0.03928 ? r1 / 12.92 : Math.Pow((r1 + 0.055) / 1.055, 2.4);
        var g1 = color.G / 255.0;
        var g = g1 <= 0.03928 ? g1 / 12.92 : Math.Pow((g1 + 0.055) / 1.055, 2.4);
        var b1 = color.B / 255.0;
        var b = b1 <= 0.03928 ? b1 / 12.92 : Math.Pow((b1 + 0.055) / 1.055, 2.4);
        //L = 0.2126 * R + 0.7152 * G + 0.0722 * B
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    public static IBrush? GetDynamicSystemBrush(string brushName)
    {
        object? obj = null;
        Application.Current?.TryFindResource(brushName, out obj);
        return obj is Color color ? new SolidColorBrush(color) : obj as SolidColorBrush;
    }

    public static Color GetTextContrastColor(Color? color)
    {
        return color?.GetColorLuminance() < 0.179 ? Colors.White : Colors.Black;
    }

    public static void CheckAndSetColor(string key, string keySource, double luminanceValue)
    {
        object? obj = null;
        Application.Current?.TryFindResource(keySource, out obj);
        if (obj is Color systemColor)
        {
            var newSourceColor = MakeNewColor(systemColor, luminanceValue);
            if (Application.Current?.Resources.ContainsKey(key) == true)
            {
                Application.Current.Resources[key] = newSourceColor;
            }
        }
    }

    private static Color MakeNewColor(Color SourceColor, double LuminancePercent)
    {
        var hsl = SourceColor.ToHsl();
        var saturation = hsl.S;
        var luminance = hsl.L;
        var hue = hsl.H;

        if (LuminancePercent < 0)
        {
            luminance = luminance - (1 - luminance) * LuminancePercent / 100d;
        }
        else
        {
            luminance = luminance * LuminancePercent / 100d;
        }

        var lighHSL = HslColor.FromHsl(Math.Clamp(hue, 0, 360d), Math.Clamp(saturation, 0d, 1d), Math.Clamp(luminance, 0, 1d));
        return lighHSL.ToRgb();
    }

    #endregion
}