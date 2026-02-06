// DynamicContrastBrushExtension.cs
//  Andrew Baylis
//  Created: 06/02/2026

#region using

using Avalonia;
using Avalonia.Data;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

/// <summary>
///     Use in XAML as {ab:DynamicContrastBrush Brush= value}
///     If the brushValue is a {DynamicResource..} then when this changes or if the values change, the color is updated.
/// </summary>
public class DynamicContrastBrushExtension : AvaloniaObject
{
    #region Avalonia Properties

    public static readonly StyledProperty<IBrush?> BrushProperty = AvaloniaProperty.Register<DynamicContrastBrushExtension, IBrush?>(nameof(Brush));

    public static readonly DirectProperty<DynamicContrastBrushExtension, IBrush?> ModifiedBrushProperty =
        AvaloniaProperty.RegisterDirect<DynamicContrastBrushExtension, IBrush?>(nameof(ModifiedBrush), o => o.ModifiedBrush);

    #endregion

    #region Fields

    private IBrush? _modifiedBrush = new SolidColorBrush(Colors.Black);

    #endregion

    #region Properties

    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public IBrush? ModifiedBrush
    {
        get => _modifiedBrush;
        private set => SetAndRaise(ModifiedBrushProperty, ref _modifiedBrush, value);
    }

    #endregion

    #region Public Methods

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding(nameof(ModifiedBrush)) {Source = this};
    }

    #endregion

    #region Protected Methods

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BrushProperty)
        {
            UpdateModifiedBrush();
        }
    }

    #endregion

    #region Private Methods

    private void UpdateModifiedBrush()
    {
        if (Brush is ISolidColorBrush solidColorBrush)
        {
            ModifiedBrush = solidColorBrush.GetTextContrastBrush();
        }
    }

    #endregion
}