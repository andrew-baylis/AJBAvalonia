// NumericTextBox.cs
//  Andrew Baylis
//  Created: 20/01/2024

#region using

using System.Globalization;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

#endregion

namespace AJBAvalonia;

/// <summary>
///     TextBox control specialized for numeric input with formatting and bounds checking.
/// </summary>
public class NumericTextBox : TextBoxEx
{
    #region Avalonia Properties

    public static readonly DirectProperty<NumericTextBox, bool> CheckMaxMinValuesProperty =
        AvaloniaProperty.RegisterDirect<NumericTextBox, bool>(nameof(CheckMaxMinValues), o => o.CheckMaxMinValues, (o, v) => o.CheckMaxMinValues = v);

    public static readonly StyledProperty<bool> ClearToNullProperty = AvaloniaProperty.Register<NumericTextBox, bool>(nameof(ClearToNull));

    public static readonly StyledProperty<object?> MaximumValueProperty = AvaloniaProperty.Register<NumericTextBox, object?>(nameof(MaximumValue));

    public static readonly StyledProperty<object?> MinimumValueProperty = AvaloniaProperty.Register<NumericTextBox, object?>(nameof(MinimumValue));

    public static readonly StyledProperty<string?> NumberDisplayFormatProperty = AvaloniaProperty.Register<NumericTextBox, string?>(nameof(NumberDisplayFormat));

    public static readonly DirectProperty<NumericTextBox, TextEntryEnum> NumberEntryTypeProperty =
        AvaloniaProperty.RegisterDirect<NumericTextBox, TextEntryEnum>(nameof(NumberEntryType), o => o.NumberEntryType, (o, v) => o.NumberEntryType = v);

    public static readonly DirectProperty<NumericTextBox, bool> NumericValueImmediateUpdateProperty =
        AvaloniaProperty.RegisterDirect<NumericTextBox, bool>(nameof(NumericValueImmediateUpdate), o => o.NumericValueImmediateUpdate, (o, v) => o.NumericValueImmediateUpdate = v);

    public static readonly StyledProperty<object?> NumericValueProperty =
        AvaloniaProperty.Register<NumericTextBox, object?>(nameof(NumericValue), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    #endregion

    #region Fields

    private readonly StringBuilder _inputBuffer = new(100);

    //private readonly Regex anyCurrency = new(@"^[+-]?\p{Sc}?(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex anyDouble = new(@"^[+-]?(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex anyInteger = new(@"^[+-]?[0-9]?$", RegexOptions.Compiled);
    //private readonly Regex positiveCurrency = new(@"^\p{Sc}?(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex positiveDouble = new(@"^(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex positiveInteger = new(@"^[0-9]?$", RegexOptions.Compiled);

    //private readonly Regex anyCurrency = new(@"^[+-]?\p{Sc}?(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex anyDouble = new(@"^[+-]?(\d+|\d{1,3}(,\d{3})*)(\.\d*)?$", RegexOptions.Compiled);
    //private readonly Regex anyInteger = new(@"^[+-]?\d*$", RegexOptions.Compiled);
    //private readonly Regex positiveCurrency = new(@"^\p{Sc}?(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex positiveDouble = new(@"^(\d*(\.\d*)?)$", RegexOptions.Compiled);
    //private readonly Regex positiveInteger = new(@"^\d*$", RegexOptions.Compiled);
    //private readonly Regex time12hrClock = new(@"^(?<hour>1[0-2]|0?[0-9])(?::(?<minute>[0-5]?[0-9])?(?::(?<seconds>[0-5]?[0-9])?)?)?\s*(?<ampm>[AaPp][Mm]?)?$$",
    //                                           RegexOptions.Compiled);
    //private readonly Regex time24hrClock = new(@"^(?<hour>2[0-3]|1[0-2]|0?[0-9])(?::(?<minute>[0-5]?[0-9])?(?::(?<seconds>[0-5]?[0-9])?)?)?$", RegexOptions.Compiled);

    private bool _inChangeNumericValue;

    private TextPresenter? _presenter;

    #endregion

    static NumericTextBox()
    {
        AffectsRender<NumericTextBox>(CaretIndexProperty);
    }

    #region Properties

    /// <summary>
    ///     Gets or sets whether the control enforces maximum and minimum value checks.
    /// </summary>
    public bool CheckMaxMinValues
    {
        get;
        set => SetAndRaise(CheckMaxMinValuesProperty, ref field, value);
    }

    /// <summary>
    ///     Gets or sets whether clearing sets the value to null.
    /// </summary>
    public bool ClearToNull
    {
        get => GetValue(ClearToNullProperty);
        set => SetValue(ClearToNullProperty, value);
    }

    /// <summary>
    ///     Gets or sets the maximum allowed value.
    /// </summary>
    public object? MaximumValue
    {
        get => GetValue(MaximumValueProperty);
        set => SetValue(MaximumValueProperty, value);
    }

    /// <summary>
    ///     Gets or sets the minimum allowed value.
    /// </summary>
    public object? MinimumValue
    {
        get => GetValue(MinimumValueProperty);
        set => SetValue(MinimumValueProperty, value);
    }

    /// <summary>
    ///     Gets or sets the display format string for the numeric value.
    /// </summary>
    public string? NumberDisplayFormat
    {
        get => GetValue(NumberDisplayFormatProperty);
        set => SetValue(NumberDisplayFormatProperty, value);
    }

    /// <summary>
    ///     Gets or sets the numeric entry type used to validate text.
    /// </summary>
    public TextEntryEnum NumberEntryType
    {
        get;
        set => SetAndRaise(NumberEntryTypeProperty, ref field, value);
    } = TextEntryEnum.AnyDouble;

    /// <summary>
    ///     Gets or sets the numeric value represented by the control.
    /// </summary>
    public object? NumericValue
    {
        get => GetValue(NumericValueProperty);
        set => SetValue(NumericValueProperty, value);
    }

    /// <summary>
    ///     Immediately updates the numeric value on a change to the text, otherwise wait until Enter or lost focus.
    /// </summary>
    public bool NumericValueImmediateUpdate
    {
        get;
        set => SetAndRaise(NumericValueImmediateUpdateProperty, ref field, value);
    } = true;

    /// <summary>
    ///     Gets the style key used for theming.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(TextBoxEx);

    #endregion

    #region Protected Methods

    protected override void OnClearText()
    {
        base.OnClearText();
        NumericValue = GetDefaultValue();
        CaretIndex = Text?.Length ?? 0;
        _presenter?.MoveCaretToTextPosition(CaretIndex);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        //reformat so no commas
        SetTextFromNumericValue(true);
        base.OnGotFocus(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            SetNumericValueFromText();
        }

        base.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        foreach (var child in this.GetVisualDescendants())
        {
            if (child is TextPresenter presenter)
            {
                _presenter = presenter;
                break;
            }
        }

        //if check max min, check if NumericValue is allowed
        if (CheckMaxMinValues)
        {
            NumericValue = GetValueBoundedByMaxMin(NumericValue); //checks bounds, modifies value before returning
        }

        CaretIndex = Text?.Length ?? 0;
        _presenter?.MoveCaretToTextPosition(CaretIndex);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        if (CheckMaxMinValues)
        {
            NumericValue = DoCheckMaxMin(); //checks bounds, modifies value before returning
        }
        else if (!NumericValueImmediateUpdate)
        {
            SetNumericValueFromText();
        }

        SetTextFromNumericValue(false); //reformat
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AllowClearProperty)
        {
            Classes.Set("clearButton", AllowClear);
        }
        else if (change.Property == NumericValueProperty && !_inChangeNumericValue)
        {
            SetTextFromNumericValue(IsFocused);
        }
        else if (change.Property == TextProperty && !_inChangeNumericValue && NumericValueImmediateUpdate)
        {
            SetNumericValueFromText();
        }
        else if (change.Property == NumberDisplayFormatProperty && !_inChangeNumericValue)
        {
            SetTextFromNumericValue(IsFocused);
        }
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        DataValidationErrors.ClearErrors(this);
        if (!e.Handled)
        {
            CheckText(e.Text);
            e.Handled = true;
        }
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        if (property == NumericValueProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
        else
        {
            base.UpdateDataValidation(property, state, error);
        }
    }

    #endregion

    #region Private Methods

    private void CheckText(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        _inputBuffer.Clear();
        var currentText = Text ?? string.Empty;
        var selectionLength = Math.Abs(SelectionStart - SelectionEnd);
        var newLength = input.Length + currentText.Length - selectionLength;

        if (MaxLength > 0 && newLength > MaxLength)
        {
            input = input.Remove(Math.Max(0, input.Length - (newLength - MaxLength)));
        }

        if (!string.IsNullOrEmpty(input))
        {
            _inputBuffer.Append(currentText);

            var caretIndex = CaretIndex;

            if (selectionLength != 0)
            {
                var (start, _) = GetSelectionRange();

                _inputBuffer.Remove(start, selectionLength);

                caretIndex = start;
            }

            _inputBuffer.Insert(caretIndex, input);

            var text = _inputBuffer.ToString();
            if (IsTextAllowed(text))
            {
                Text = text; //calls onpropertychanged
                ClearSelection();

                caretIndex += input.Length;

                CaretIndex = caretIndex;
            }
        }
    }

    private object? ConvertTextToNumber()
    {
        switch (NumberEntryType)
        {
            case TextEntryEnum.PositiveInteger:
            case TextEntryEnum.AnyInteger:

                if (int.TryParse(Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
                {
                    return result;
                }

                return 0;
            case TextEntryEnum.PositiveDouble:
            case TextEntryEnum.AnyDouble:
            case TextEntryEnum.PositiveCurrency:
            case TextEntryEnum.AnyCurrency:
                if (double.TryParse(Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var r))
                {
                    return r;
                }

                return 0d;
            case TextEntryEnum.Time12Hr:
            {
                var time = TimeSpan.Zero;
                if (!string.IsNullOrEmpty(Text))
                {
                    var m = RegExExtensions.time12hrClock.Match(Text);
                    var h = m.Groups["hour"].Value;
                    var min = m.Groups["minute"].Value;
                    var s = m.Groups["seconds"].Value;
                    var a = m.Groups["ampm"].Value;
                    if (!int.TryParse(h, out var hour))
                    {
                        hour = 0;
                    }

                    if (!int.TryParse(min, out var minute))
                    {
                        minute = 0;
                    }

                    if (!int.TryParse(s, out var second))
                    {
                        second = 0;
                    }

                    if (a.Length > 0 && a.ToUpper()[0] == 'P')
                    {
                        hour += 12;
                    }

                    time = new TimeSpan(hour, minute, second);
                }

                if (GetNumericValueBoundType() == typeof(DateTime))
                {
                    return DateTime.MinValue.Add(time);
                }

                return time;
            }

            case TextEntryEnum.Time24Hr:
            {
                var time = TimeSpan.Zero;
                if (!string.IsNullOrEmpty(Text))
                {
                    var m = RegExExtensions.time24hrClock.Match(Text);
                    var h = m.Groups["hour"].Value;
                    var min = m.Groups["minute"].Value;
                    var s = m.Groups["seconds"].Value;

                    if (!int.TryParse(h, out var hour))
                    {
                        hour = 0;
                    }

                    if (!int.TryParse(min, out var minute))
                    {
                        minute = 0;
                    }

                    if (!int.TryParse(s, out var second))
                    {
                        second = 0;
                    }

                    time = new TimeSpan(hour, minute, second);
                }

                if (GetNumericValueBoundType() == typeof(DateTime))
                {
                    return DateTime.MinValue.Add(time);
                }

                return time;
            }

            case TextEntryEnum.AnyText:
                return Text;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private object? DoCheckMaxMin()
    {
        //checks the text value as NumericValue may not yet be set
        var value = ConvertTextToNumber();
        return GetValueBoundedByMaxMin(value);
    }

    private object? GetDefaultValue()
    {
        if (ClearToNull && IsValueBoundTypeNullable())
        {
            return null;
        }

        if (CheckMaxMinValues && MinimumValue != null)
        {
            return MinimumValue;
        }

        return NumberEntryType switch
        {
            TextEntryEnum.PositiveInteger => 0,
            TextEntryEnum.AnyInteger => 0,
            TextEntryEnum.PositiveDouble => 0d,
            TextEntryEnum.AnyDouble => 0d,
            TextEntryEnum.PositiveCurrency => 0d,
            TextEntryEnum.AnyCurrency => 0d,
            TextEntryEnum.Time12Hr => TimeSpan.Zero,
            TextEntryEnum.Time24Hr => TimeSpan.Zero,
            _ => null
        };
    }

    private Type? GetNumericValueBoundType()
    {
        var b = BindingOperations.GetBindingExpressionBase(this, NumericValueProperty);
        if (b != null)
        {
            var sourcetypeProp = b.GetType().GetProperty("SourceType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (sourcetypeProp != null)
            {
                return sourcetypeProp.GetValue(b) as Type;
            }
        }

        return null;
    }

    private Type? GetNumericValueUnderlyingType()
    {
        var type = GetNumericValueBoundType();
        if (type is {IsGenericType: true} && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Nullable.GetUnderlyingType(type);
        }

        return type;
    }

    private (int start, int end) GetSelectionRange()
    {
        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;

        return (Math.Min(selectionStart, selectionEnd), Math.Max(selectionStart, selectionEnd));
    }

    private object? GetValueBoundedByMaxMin(object? value)
    {
        switch (NumberEntryType)
        {
            case TextEntryEnum.Time12Hr:
            case TextEntryEnum.Time24Hr:
            {
                if (value is TimeSpan ts)
                {
                    if (MaximumValue is TimeSpan max && ts > max)
                    {
                        return max;
                    }

                    if (MinimumValue is TimeSpan min && ts < min)
                    {
                        return min;
                    }
                }
                else if (value is DateTime dt)
                {
                    if (MaximumValue is DateTime max && dt > max)
                    {
                        return max;
                    }

                    if (MinimumValue is DateTime min && dt < min)
                    {
                        return min;
                    }
                }
            }
                break;
            case TextEntryEnum.AnyText:
            {
                if (MaximumValue is string maxs && string.CompareOrdinal(value?.ToString(), maxs) > 0)
                {
                    return maxs;
                }

                if (MinimumValue is string mins && string.CompareOrdinal(value?.ToString(), mins) < 0)
                {
                    return mins;
                }
            }
                break;
            default:
            {
                var d = string.Format(CultureInfo.CurrentCulture, "{0:G}", value);
                if (double.TryParse(d, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
                {
                    var maxs = string.Format(CultureInfo.CurrentCulture, "{0:G}", MaximumValue);
                    if (double.TryParse(maxs, NumberStyles.Any, CultureInfo.CurrentCulture, out var max) && result > max)
                    {
                        return max;
                    }

                    var mins = string.Format(CultureInfo.CurrentCulture, "{0:G}", MinimumValue);
                    if (double.TryParse(mins, NumberStyles.Any, CultureInfo.CurrentCulture, out var min) && result < min)
                    {
                        return min;
                    }
                }
            }
                break;
        }

        return value;
    }

    private bool IsTextAllowed(string? s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            var accept = NumberEntryType switch
            {
                TextEntryEnum.AnyInteger => RegExExtensions.anyInteger.IsMatch(s),
                TextEntryEnum.AnyDouble => RegExExtensions.anyDouble.IsMatch(s),
                TextEntryEnum.AnyCurrency => RegExExtensions.anyCurrency.IsMatch(s),
                TextEntryEnum.PositiveCurrency => RegExExtensions.positiveCurrency.IsMatch(s),
                TextEntryEnum.PositiveDouble => RegExExtensions.positiveDouble.IsMatch(s),
                TextEntryEnum.PositiveInteger => RegExExtensions.positiveInteger.IsMatch(s),
                TextEntryEnum.Time12Hr => RegExExtensions.time12hrClock.IsMatch(s),
                TextEntryEnum.Time24Hr => RegExExtensions.time24hrClock.IsMatch(s),
                _ => true
            };

            return accept;
        }

        return true;
    }

    private bool IsValueBoundTypeNullable()
    {
        var type = GetNumericValueBoundType();
        if (type is {IsGenericType: true} && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return true;
        }

        return false;
    }

    private void SetNumericValueFromText()
    {
        _inChangeNumericValue = true;
        try
        {
            NumericValue = ConvertTextToNumber();
        }
        finally
        {
            _inChangeNumericValue = false;
        }
    }

    private void SetTextFromNumericValue(bool isFocused)
    {
        _inChangeNumericValue = true;
        try
        {
            if (NumberDisplayFormat != null && !isFocused)
            {
                var fmt = NumberDisplayFormat;
                if (!fmt.Contains('{'))
                {
                    fmt = $"{{0:{fmt}}}";
                }

                Text = string.Format(CultureInfo.CurrentCulture, fmt, NumericValue);
            }
            else
            {
                var value = NumericValue;
                if (NumericValue is TimeSpan ts)
                {
                    value = DateTime.Today.Add(ts);
                }

                if (value is DateTime date)
                {
                    var fmt = NumberEntryType switch
                    {
                        TextEntryEnum.Time12Hr => "h:mm:ss tt",
                        TextEntryEnum.Time24Hr => "HH:mm:ss",
                        _ => "h:mm:ss tt"
                    };
                    if (date.Second == 0)
                    {
                        fmt = NumberEntryType switch
                        {
                            TextEntryEnum.Time12Hr => "h:mm tt",
                            TextEntryEnum.Time24Hr => "HH:mm",
                            _ => "h:mm tt"
                        };
                    }

                    Text = date.ToString(fmt, CultureInfo.CurrentCulture);
                }
                else
                {
                    Text = string.Format(CultureInfo.CurrentCulture, @"{0}", value);
                }
            }
        }
        finally
        {
            _inChangeNumericValue = false;
            ClearSelection();
            CaretIndex = Text?.Length ?? 0;
            _presenter?.MoveCaretToTextPosition(CaretIndex);
        }
    }

    #endregion

    //#region Nested type: DecimalDoubleConverter

    ///// <summary>
    ///// Converter between decimal and double types for data binding.
    ///// </summary>
    //public class DecimalDoubleConverter : IBindingTypeConverter
    //{
    //    #region IBindingTypeConverter Members

    //    public int GetAffinityForObjects(Type fromType, Type toType)
    //    {
    //        if ((fromType == typeof(decimal) && toType == typeof(double)) || (fromType == typeof(double) && toType == typeof(decimal)))
    //        {
    //            return 100;
    //        }

    //        return 0;
    //    }

    //    public bool TryConvert(object? from, Type toType, object? conversionHint, [UnscopedRef] out object? result)
    //    {
    //        if (from != null)
    //        {
    //            try
    //            {
    //                var t = from.GetType();
    //                if (t == typeof(decimal) && toType == typeof(double))
    //                {
    //                    var dec = (decimal) from;
    //                    result = (double) dec;
    //                    return true;
    //                }

    //                if (t == typeof(double) && toType == typeof(decimal))
    //                {
    //                    var doub = (double) from;
    //                    result = (decimal) doub;
    //                    return true;
    //                }
    //            }
    //            catch (Exception)
    //            {
    //                result = null;
    //                return false;
    //            }
    //        }

    //        result = null;
    //        return false;
    //    }

    //    #endregion
    //}

    //#endregion
}