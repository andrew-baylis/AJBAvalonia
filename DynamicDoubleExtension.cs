// DynamicDoubleExtension.cs
// Andrew Baylis
//  Created: 15/09/2024

#region using

using Avalonia;

#endregion

namespace AJBAvalonia;

/// <summary>
///     Use in XAML as {ab:DynamicDouble Value=doubleValue, PercentChange = value, IncrementChange = value}
///     If the doubleValue is a {DynamicResource..} then when this changes or if the values change, the number value is
///     updated.
///     PercentChange property takes precedence over the IncrementChange property
/// </summary>
public class DynamicDoubleExtension : AvaloniaObject
{
    #region Fields

    public static readonly DirectProperty<DynamicDoubleExtension, int?> DecimalPlacesProperty =
        AvaloniaProperty.RegisterDirect<DynamicDoubleExtension, int?>(nameof(DecimalPlaces), o => o.DecimalPlaces, (o, v) => o.DecimalPlaces = v);

    public static readonly DirectProperty<DynamicDoubleExtension, double?> IncrementChangeProperty =
        AvaloniaProperty.RegisterDirect<DynamicDoubleExtension, double?>(nameof(IncrementChange), o => o.IncrementChange, (o, v) => o.IncrementChange = v);

    public static readonly DirectProperty<DynamicDoubleExtension, double?> PercentChangeProperty =
        AvaloniaProperty.RegisterDirect<DynamicDoubleExtension, double?>(nameof(PercentChange), o => o.PercentChange, (o, v) => o.PercentChange = v);

    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<DynamicDoubleExtension, double>(nameof(Value));

    #endregion

    #region Properties

    public double AsDouble { get; private set; } = 12d;

    public int? DecimalPlaces
    {
        get;
        set => SetAndRaise(DecimalPlacesProperty, ref field, value);
    }

    public double? IncrementChange
    {
        get;
        set => SetAndRaise(IncrementChangeProperty, ref field, value);
    }

    public double? PercentChange
    {
        get;
        set => SetAndRaise(PercentChangeProperty, ref field, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion

    #region Static Methods

    public static implicit operator double(DynamicDoubleExtension d)
    {
        return d.AsDouble;
    }

    #endregion

    #region Override Methods

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        DoUpdateValue();
    }

    #endregion

    #region Public Methods

    public double ProvideValue(IServiceProvider serviceProvider)
    {
        return AsDouble;
    }

    #endregion

    #region Private Methods

    private void DoUpdateValue()
    {
        if (PercentChange > 0)
        {
            AsDouble = Value * PercentChange.Value;
        }
        else
        {
            AsDouble = Value + IncrementChange.GetValueOrDefault(0);
        }

        if (DecimalPlaces.HasValue)
        {
            AsDouble = Math.Round(AsDouble, DecimalPlaces.Value);
        }
    }

    #endregion
}