// TextBlockEx.cs
// Andrew Baylis
//  Created: 12/10/2024

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

public class TextBlockEx : TextBlock
{
    #region Fields

    private double? _oldPadding;

    #endregion

    #region Properties

    protected override Type StyleKeyOverride => typeof(TextBlock);

    #endregion

    #region Override Methods

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FontStyleProperty)
        {
            if (FontStyle != FontStyle.Normal)
            {
                if (Padding.Right < 2)
                {
                    _oldPadding = Padding.Right;
                    Padding = new Thickness(Padding.Left, Padding.Top, 2, Padding.Bottom);
                }
            }
            else if (_oldPadding != null)
            {
                Padding = new Thickness(Padding.Left, Padding.Top, _oldPadding.Value, Padding.Bottom);
                _oldPadding = null;
            }
        }
    }

    #endregion
}