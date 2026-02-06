// ToolWindowEx.cs
//  Andrew Baylis
//  Created: 06/02/2026

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;

#endregion

namespace AJBAvalonia;

/// <summary>
///     ToolWindow has narrower caption bar, no icon and no system menu.
///     It is designed to be used as a child window of a main application window,
///     such as a dockable panel or a floating tool palette. It provides a more compact and
///     streamlined user interface for tools
/// and utilities that do not require the full functionality of a standard window.
/// </summary>
public class ToolWindowEx : Window
{
    #region Avalonia Properties

    public static readonly StyledProperty<HorizontalAlignment> TitleAlignmentProperty = AvaloniaProperty.Register<ToolWindowEx, HorizontalAlignment>(nameof(TitleAlignment));

    #endregion

    #region Fields

    private Button? _closeButton;

    #endregion

    #region Properties

    public HorizontalAlignment TitleAlignment
    {
        get => GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(ToolWindowEx);

    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (_closeButton != null)
        {
            _closeButton.Click += CloseButton_Click;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Forward non-client area events to the custom caption bar
        if (e.Source is Control {Name: "PART_CaptionBorder"})
        {
            BeginMoveDrag(e);
        }
    }

    #endregion

    #region Private Methods

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(Close);
    }

    #endregion
}