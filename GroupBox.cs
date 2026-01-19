// GroupBox.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

/// <summary>
///     GroupBox Control class with a customizable header background.
/// </summary>
public class GroupBox : HeaderedContentControl
{
    #region Fields

    /// <summary>
    ///     Identifies the HeaderBackground styled property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty = AvaloniaProperty.Register<GroupBox, IBrush?>(nameof(HeaderBackground));

    #endregion

    /// <summary>
    ///     Static constructor to override default metadata for focus and tab navigation.
    /// </summary>
    static GroupBox()
    {
        FocusableProperty.OverrideMetadata<GroupBox>(new StyledPropertyMetadata<bool>(false));
        KeyboardNavigation.TabNavigationProperty.OverrideMetadata<GroupBox>(new StyledPropertyMetadata<KeyboardNavigationMode>(KeyboardNavigationMode.None));
    }

    #region Properties

    /// <summary>
    ///     Gets or sets the brush used for the header background.
    /// </summary>
    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    #endregion
}