// DialogWindowEx.cs
//  Andrew Baylis
//  Created: 06/02/2026

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

#endregion

namespace AJBAvalonia;

public class DialogWindowEx : Window
{
    #region Avalonia Properties

    public static readonly StyledProperty<HorizontalAlignment> TitleAlignmentProperty = AvaloniaProperty.Register<DialogWindowEx, HorizontalAlignment>(nameof(TitleAlignment));

    public static readonly StyledProperty<double> TitleFontSizeProperty = AvaloniaProperty.Register<DialogWindowEx, double>(nameof(TitleFontSize), 14.0d);

    public static readonly StyledProperty<Bitmap?> CaptionBitmapProperty = AvaloniaProperty.Register<DialogWindowEx, Bitmap?>(nameof(CaptionBitmap));

    public Bitmap? CaptionBitmap
    {
        get => GetValue(CaptionBitmapProperty);
        set => SetValue(CaptionBitmapProperty, value);
    }

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

    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(DialogWindowEx);

    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _closeButton?.Click += CloseButton_Click;

        SetIconFromCaptionBitmap();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CaptionBitmapProperty)
        {
            SetIconFromCaptionBitmap();
        }
    }

    protected void SetIconFromCaptionBitmap()
    {
        if (CaptionBitmap != null)
        {
            using var stream = new MemoryStream();
            CaptionBitmap.Save(stream);
            stream.Position = 0;
            Icon = new WindowIcon(stream);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Forward non-client area events to the custom caption bar
        if (e.Source is Control { Name: "PART_CaptionBorder" })
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