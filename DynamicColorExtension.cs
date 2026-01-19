// DynamicColorExtension.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using Avalonia;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

/// <summary>
///     Use in XAML as {ab:DynamicColor Color=colorValue, LuminancePercent = value, SaturationPercent = value, HuePercent = value}
///     If the colorValue is a {DynamicResource..} then when this changes or if the values change, the color is updated.
///     We could also use the Brush property to set a solidcolorbrush value
/// </summary>
public class DynamicColorExtension : AvaloniaObject
{
    #region Fields

    public static readonly StyledProperty<IBrush?> BrushProperty = AvaloniaProperty.Register<DynamicColorExtension, IBrush?>(nameof(Brush));
    public static readonly StyledProperty<Color?> ColorProperty = AvaloniaProperty.Register<DynamicColorExtension, Color?>(nameof(Color));

    public static readonly StyledProperty<double> HuePercentProperty = AvaloniaProperty.Register<DynamicColorExtension, double>(nameof(HuePercent), 100d);
    public static readonly StyledProperty<double> LuminancePercentProperty = AvaloniaProperty.Register<DynamicColorExtension, double>(nameof(LuminancePercent), 100d);

    public static readonly StyledProperty<double> SaturationPercentProperty = AvaloniaProperty.Register<DynamicColorExtension, double>(nameof(SaturationPercent), 100d);

    public static readonly DirectProperty<DynamicColorExtension, Color> ModifiedColorProperty = AvaloniaProperty.RegisterDirect<DynamicColorExtension, Color>(nameof(ModifiedColor), o => o.ModifiedColor);

    private Color _modifiedColor = Colors.Black;


    public Color ModifiedColor
    {
        get => _modifiedColor;
        private set => SetAndRaise(ModifiedColorProperty, ref _modifiedColor, value);
    }

    #endregion

    #region Properties

    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    ///     Changes the hue value by the percentage. If negative, it adjusts the gap between the hue value and 360
    /// </summary>
    public double HuePercent
    {
        get => GetValue(HuePercentProperty);
        set => SetValue(HuePercentProperty, value);
    }

    /// <summary>
    ///     Changes the luminance value by the percentage. If negative, it adjusts the gap between the luminance and 1.0
    /// </summary>
    public double LuminancePercent
    {
        get => GetValue(LuminancePercentProperty);
        set => SetValue(LuminancePercentProperty, value);
    }

    /// <summary>
    ///     Changes the saturation by the percentage. If negative, it adjusts the gap between the saturation and 1.0
    /// </summary>
    public double SaturationPercent
    {
        get => GetValue(SaturationPercentProperty);
        set => SetValue(SaturationPercentProperty, value);
    }

    #endregion

    #region Override Methods

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BrushProperty)
        {
            if (change.NewValue is ISolidColorBrush s)
            {
                Color = s.Color;
            }
        }
        else if (change.Property == ColorProperty || change.Property == HuePercentProperty || change.Property == LuminancePercentProperty ||
                 change.Property == SaturationPercentProperty)
        {
            DoUpdateColor();
        }

        DoUpdateColor();
    }

    #endregion

    #region Public Methods

    public Color ProvideValue(IServiceProvider serviceProvider)
    {
        //return this.ToBinding();
        return _modifiedColor;
    }

    #endregion

    #region Private Methods

    private void DoUpdateColor()
    {
        if (Color is { A: > 0 })
        {
            ModifiedColor = MakeNewColor(Color.Value);
        }
    }

    private Color MakeNewColor(Color SourceColor)
    {
        var hsl = SourceColor.ToHsl();
        var saturation = hsl.S;
        var luminance = hsl.L;
        var hue = hsl.H;
        if (SaturationPercent < 0)
        {
            saturation = saturation - (1 - saturation) * SaturationPercent / 100d; //- as saturationpercent is negative
        }
        else
        {
            saturation = saturation * SaturationPercent / 100d;
        }

        if (HuePercent < 0)
        {
            hue = hue - (360 - hue) * HuePercent / 100d;
        }
        else
        {
            hue = hue * HuePercent / 100d;
        }

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