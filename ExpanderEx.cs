// ExpanderEx.cs
// Andrew Baylis
//  Created: 16/09/2024

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

public class ExpanderEx : Expander
{
    #region Fields

    private const string themeFontSize = "ControlContentThemeFontSize";

    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty = AvaloniaProperty.Register<ExpanderEx, IBrush?>(nameof(HeaderBackground));

    public static readonly StyledProperty<FontFamily?> HeaderFontFamilyProperty = AvaloniaProperty.Register<ExpanderEx, FontFamily?>(nameof(HeaderFontFamily));

    public static readonly StyledProperty<double> HeaderFontSizeProperty = AvaloniaProperty.Register<ExpanderEx, double>(nameof(HeaderFontSize), 11d);

    public static readonly StyledProperty<FontStretch>
        HeaderFontStretchProperty = AvaloniaProperty.Register<ExpanderEx, FontStretch>(nameof(HeaderFontStretch), FontStretch.Normal);

    public static readonly StyledProperty<FontStyle> HeaderFontStyleProperty = AvaloniaProperty.Register<ExpanderEx, FontStyle>(nameof(HeaderFontStyle));

    public static readonly StyledProperty<FontWeight> HeaderFontWeightProperty = AvaloniaProperty.Register<ExpanderEx, FontWeight>(nameof(HeaderFontWeight), FontWeight.Normal);

    public static readonly StyledProperty<string?> HeaderTextProperty = AvaloniaProperty.Register<ExpanderEx, string?>(nameof(HeaderText));

    private readonly TextBlock _headerBlock;

    private ToggleButton? _expanderHeaderButton;

    #endregion

    public ExpanderEx()
    {
        if (Application.Current?.Resources.ContainsKey(themeFontSize) == true)
        {
            var d = Application.Current.Resources[themeFontSize];
            if (d is double value)
            {
                HeaderFontSize = value;
            }
        }

        _headerBlock = new TextBlock();
        var b = new Binding(nameof(HeaderText)) {Source = this};
        _headerBlock.Bind(TextBlock.TextProperty, b);
        b = new Binding(nameof(HeaderFontSize)) {Source = this};
        _headerBlock.Bind(TextBlock.FontSizeProperty, b);
        b = new Binding(nameof(HeaderFontWeight)) {Source = this};
        _headerBlock.Bind(TextBlock.FontWeightProperty, b);
        b = new Binding(nameof(HeaderFontStyle)) {Source = this};
        _headerBlock.Bind(TextBlock.FontStyleProperty, b);
        b = new Binding(nameof(HeaderFontFamily)) {Source = this};
        _headerBlock.Bind(TextBlock.FontFamilyProperty, b);
        b = new Binding(nameof(Foreground)) {Source = this};
        _headerBlock.Bind(TextBlock.ForegroundProperty, b);
        b = new Binding(nameof(HeaderFontStretch)) {Source = this};
        _headerBlock.Bind(TextBlock.FontStretchProperty, b);
    }

    #region Properties

    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public FontFamily? HeaderFontFamily
    {
        get => GetValue(HeaderFontFamilyProperty);
        set => SetValue(HeaderFontFamilyProperty, value);
    }

    public double HeaderFontSize
    {
        get => GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    public FontStretch HeaderFontStretch
    {
        get => GetValue(HeaderFontStretchProperty);
        set => SetValue(HeaderFontStretchProperty, value);
    }

    public FontStyle HeaderFontStyle
    {
        get => GetValue(HeaderFontStyleProperty);
        set => SetValue(HeaderFontStyleProperty, value);
    }

    public FontWeight HeaderFontWeight
    {
        get => GetValue(HeaderFontWeightProperty);
        set => SetValue(HeaderFontWeightProperty, value);
    }

    public string? HeaderText
    {
        get => GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(Expander);

    #endregion

    #region Override Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _expanderHeaderButton = e.NameScope.Find<ToggleButton>("ExpanderHeader");
        if (_expanderHeaderButton != null)
        {
            if (Header == null)
            {
                _expanderHeaderButton.Content = _headerBlock;
            }

            if (HeaderBackground != null)
            {
                _expanderHeaderButton.Background = HeaderBackground;
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HeaderBackgroundProperty)
        {
            if (HeaderBackground != null && _expanderHeaderButton != null)
            {
                _expanderHeaderButton.Background = HeaderBackground;
            }
        }
    }

    #endregion
}