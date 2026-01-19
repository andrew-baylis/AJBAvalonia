// TextBoxEx.cs
//  Andrew Baylis
//  Created: 10/06/2024

#region using

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

#endregion

namespace AJBAvalonia;

/// <summary>
///     Extended TextBox with additional features like clear button, suffix and select-all behavior on focus.
/// </summary>
public class TextBoxEx : TextBox
{
    #region Avalonia Properties

    public static readonly StyledProperty<bool> AllowClearProperty = AvaloniaProperty.Register<TextBoxEx, bool>(nameof(AllowClear));

    public static readonly StyledProperty<Thickness> InnerLeftPaddingProperty = AvaloniaProperty.Register<TextBoxEx, Thickness>(nameof(InnerLeftPadding));

    public static readonly StyledProperty<bool> SelectAllOnGetFocusProperty = AvaloniaProperty.Register<TextBoxEx, bool>(nameof(SelectAllOnGetFocus));

    public static readonly StyledProperty<bool> ShowPasswordProperty = AvaloniaProperty.Register<TextBoxEx, bool>(nameof(ShowPassword));

    public static readonly StyledProperty<Thickness> SuffixPaddingProperty = AvaloniaProperty.Register<TextBoxEx, Thickness>(nameof(SuffixPadding));

    public static readonly StyledProperty<string?> SuffixProperty = AvaloniaProperty.Register<TextBoxEx, string?>(nameof(Suffix));

    #endregion

    #region Fields

    private bool _focusByPointer;

    private double? _oldPadding;

    #endregion

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextBoxEx" /> class.
    /// </summary>
    public TextBoxEx()
    {
        PointerReleased += OnPointerReleased;
    }

    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether the clear button is enabled.
    /// </summary>
    public bool AllowClear
    {
        get => GetValue(AllowClearProperty);
        set => SetValue(AllowClearProperty, value);
    }

    /// <summary>
    ///     Gets or sets inner left padding to shift content.
    /// </summary>
    public Thickness InnerLeftPadding
    {
        get => GetValue(InnerLeftPaddingProperty);
        set => SetValue(InnerLeftPaddingProperty, value);
    }

    /// <summary>
    ///     Gets or sets whether all text should be selected when the control receives focus.
    /// </summary>
    public bool SelectAllOnGetFocus
    {
        get => GetValue(SelectAllOnGetFocusProperty);
        set => SetValue(SelectAllOnGetFocusProperty, value);
    }

    /// <summary>
    ///     Gets or sets whether a reveal password button is shown when the input is password.
    /// </summary>
    public bool ShowPassword
    {
        get => GetValue(ShowPasswordProperty);
        set => SetValue(ShowPasswordProperty, value);
    }

    /// <summary>
    ///     Gets or sets the suffix text displayed alongside the input.
    /// </summary>
    public string? Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    /// <summary>
    ///     Gets or sets padding applied to the suffix content.
    /// </summary>
    public Thickness SuffixPadding
    {
        get => GetValue(SuffixPaddingProperty);
        set => SetValue(SuffixPaddingProperty, value);
    }

    /// <summary>
    ///     Gets the style key used for theming.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(TextBoxEx);

    #endregion

    #region Events

    /// <summary>
    ///     Raised when the text is cleared.
    /// </summary>
    public event EventHandler? OnClearEvent;

    #endregion

    #region Public Methods

    public T? GetTextAsValue<T>()
    {
        var conv = TypeDescriptor.GetConverter(typeof(T));
        if (conv.CanConvertFrom(typeof(string)))
        {
            try
            {
                var val = conv.ConvertFromString(Text ?? string.Empty);
                if (val == null)
                {
                    return default;
                }

                return (T) val;
            }
            catch
            {
                return default;
            }
        }

        return default;
    }

    public bool ValidateForDataType(Type dataType)
    {
        var conv = TypeDescriptor.GetConverter(dataType);
        if (conv.CanConvertFrom(typeof(string)))
        {
            try
            {
                var val = conv.ConvertFromString(Text ?? string.Empty);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    #endregion

    #region Protected Methods

    /// <summary>
    ///     Applies the template and wires up clear button click handler.
    /// </summary>
    /// <param name="e">Template applied arguments.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var btn = e.NameScope.Find<Button>("PART_ClearButton");
        if (btn != null)
        {
            btn.Focusable = false;
            btn.Click += BtnOnClick;
        }
    }

    /// <summary>
    ///     Invokes the clear text event.
    /// </summary>
    protected virtual void OnClearText()
    {
        OnClearEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Handles when the control receives focus.
    /// </summary>
    /// <param name="e">Got focus event args.</param>
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        _focusByPointer = e.NavigationMethod == NavigationMethod.Pointer;
        if (!_focusByPointer && SelectAllOnGetFocus)
        {
            SelectAll();
        }
    }

    /// <summary>
    ///     Handles keyboard events such as pressing return to move focus to next control.
    /// </summary>
    /// <param name="e">Key event args.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Return)
        {
            //find top parent window
            var v = this.GetVisualParent();
            while (v != null && v is not TopLevel)
            {
                v = v.GetVisualParent();
            }

            if (v != null)
            {
                var f = ((TopLevel) v).FocusManager;
                var current = f?.GetFocusedElement();
                if (current != null)
                {
                    var next = KeyboardNavigationHandler.GetNext(current, NavigationDirection.Next);
                    next?.Focus(NavigationMethod.Tab);
                }
            }
        }
    }

    /// <summary>
    ///     Handles lost focus event to reset pointer focus flag.
    /// </summary>
    /// <param name="e">Routed event args.</param>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        _focusByPointer = false;
    }

    /// <summary>
    ///     Handles property changes to update classes and padding behavior.
    /// </summary>
    /// <param name="change">Change information.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty)
        {
            if (string.IsNullOrEmpty(Text))
            {
                OnClearEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (change.Property == AllowClearProperty)
        {
            Classes.Set("clearButton", AllowClear);
        }
        else if (change.Property == ShowPasswordProperty)
        {
            Classes.Set("revealPasswordButton", ShowPassword);
        }
        else if (change.Property == FontStyleProperty)
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

    /// <summary>
    ///     Handles text input to clear data validation errors.
    /// </summary>
    /// <param name="e">Text input event args.</param>
    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        DataValidationErrors.ClearErrors(this);
    }

    #endregion

    #region Private Methods

    private void BtnOnClick(object? sender, RoutedEventArgs e)
    {
        Clear();
        OnClearText();
        e.Handled = true;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        if (point.Properties.IsLeftButtonPressed && IsFocused && SelectAllOnGetFocus)
        {
            SelectAll();
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased && _focusByPointer && SelectAllOnGetFocus)
        {
            SelectAll();
            _focusByPointer = false;
        }
    }

    #endregion
}