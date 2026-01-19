// ColorLightnessExtension.cs
// Andrew Baylis
//  Created: 20/01/2024

using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AJBAvalonia;

public class ColorLightnessExtension : MarkupExtension
{
    #region Properties

    public double Ratio { get; set; } = 1.25;

    public Color SourceColor { get; set; }

    #endregion

    #region Override Methods

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var hsl = SourceColor.ToHsl();
        var lightness = hsl.L * Ratio;
        var lighHSL = HslColor.FromHsl(hsl.H, hsl.S, Math.Clamp(lightness, 0, 1));
        var lightColor = lighHSL.ToRgb();

        var provideValueTarget = (IProvideValueTarget?) serviceProvider.GetService(typeof(IProvideValueTarget));
        var propertyType = (provideValueTarget?.TargetProperty as AvaloniaProperty)?.PropertyType;
        if (propertyType == typeof(IBrush))
        {
            return new ImmutableSolidColorBrush(lightColor);
        }

        return lightColor;
    }

    #endregion
}