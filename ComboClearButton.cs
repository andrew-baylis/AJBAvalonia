// ComboClearButton.cs
//  Andrew Baylis
//  Created: 29/11/2025

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

public class ComboClearButton : Button
{
    #region Avalonia Properties

    public static readonly StyledProperty<double> ClearPathSizeProperty = AvaloniaProperty.Register<ComboClearButton, double>(nameof(ClearPathSize), 10);

    public static readonly StyledProperty<IBrush?> DisabledForegroundProperty = AvaloniaProperty.Register<ComboClearButton, IBrush?>(nameof(DisabledForeground));

    #endregion

    #region Properties

    public double ClearPathSize
    {
        get => GetValue(ClearPathSizeProperty);
        set => SetValue(ClearPathSizeProperty, value);
    }

    public IBrush? DisabledForeground
    {
        get => GetValue(DisabledForegroundProperty);
        set => SetValue(DisabledForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush?> PressedBackgroundProperty = AvaloniaProperty.Register<ComboClearButton, IBrush?>(nameof(PressedBackground));

    public IBrush? PressedBackground
    {
        get => GetValue(PressedBackgroundProperty);
        set => SetValue(PressedBackgroundProperty, value);
    }

    public static readonly StyledProperty<IBrush?> PointerOverBackgroundProperty = AvaloniaProperty.Register<ComboClearButton, IBrush?>(nameof(PointerOverBackground));

    public IBrush? PointerOverBackground
    {
        get => GetValue(PointerOverBackgroundProperty);
        set => SetValue(PointerOverBackgroundProperty, value);
    }

    #endregion
}