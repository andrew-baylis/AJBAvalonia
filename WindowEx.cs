// WindowEx.cs
//  Andrew Baylis
//  Created: 06/02/2026

#region using

using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

#endregion

namespace AJBAvalonia;

public class WindowEx : Window
{

    #region Protected Methods

    protected override Type StyleKeyOverride => typeof(WindowEx);

    private Button? _closeButton;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Forward non-client area events to the custom caption bar
        if (e.Source is Control {Name: "PART_CaptionBorder"})
        {
            BeginMoveDrag(e);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (_closeButton != null)
        {
            _closeButton.Click += CloseButton_Click;
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(Close);
    }

    #endregion
}