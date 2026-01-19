// CheckComboBoxItem.cs
//  Andrew Baylis
//  Created: 26/11/2025

#region using

using Avalonia;

#endregion

namespace AJBAvalonia;

/// <summary>
/// Represents an item in the check combo box, wrapping the original item data and selection state.
/// </summary>
public class CheckComboBoxItem(object? itemData) : AvaloniaObject
{
    #region Avalonia Properties

    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<CheckComboBoxItem, bool>(nameof(IsSelected));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the item is selected.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Gets the wrapped item data for this check item.
    /// </summary>
    public object? ItemData { get; } = itemData;

    #endregion
}