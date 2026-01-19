// EditWithDropControl.axaml.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

#endregion

namespace AJBAvalonia;

/// <summary>
///     Represents a control that combines a text box and a drop-down control.
/// </summary>
/// <remarks>
///     The drop-down control is given by the <see cref="DropControl" /> property.
///     The text box displays the selected item and allows further editing.
/// </remarks>
public class EditWithDropControl : EditWithDropControlBase
{
    #region Fields

    public static readonly StyledProperty<Control?> DropControlProperty = AvaloniaProperty.Register<EditWithDropControl, Control?>(nameof(DropControl));

    #endregion

    #region Properties

    public Control? DropControl
    {
        get => GetValue(DropControlProperty);
        set => SetValue(DropControlProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(EditWithDropControlBase); //use the template ControlTheme for the Base control

    #endregion

    #region Override Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        InternalDropControl = DropControl;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DropControlProperty)
        {
            InternalDropControl = DropControl;
        }
    }

    #endregion
}