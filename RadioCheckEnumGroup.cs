// RadioCheckEnumGroup.cs
// Andrew Baylis
//  Created: 03/01/2025

#region using

using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

#endregion

namespace AJBAvalonia;

public class RadioCheckEnumGroup : Panel
{
    #region Fields

    public static readonly StyledProperty<Thickness?> ButtonMarginProperty = AvaloniaProperty.Register<RadioCheckEnumGroup, Thickness?>(nameof(ButtonMargin));

    public static readonly StyledProperty<int> ColumnsProperty = AvaloniaProperty.Register<RadioCheckEnumGroup, int>(nameof(Columns));

    public static readonly DirectProperty<RadioCheckEnumGroup, Type?> EnumTypeProperty =
        AvaloniaProperty.RegisterDirect<RadioCheckEnumGroup, Type?>(nameof(EnumType), o => o.EnumType, (o, v) => o.EnumType = v);

    public static readonly StyledProperty<int> FirstColumnProperty = AvaloniaProperty.Register<RadioCheckEnumGroup, int>(nameof(FirstColumn));

    public static readonly StyledProperty<GroupOrientation> OrientationProperty = AvaloniaProperty.Register<RadioCheckEnumGroup, GroupOrientation>(nameof(Orientation));

    public static readonly StyledProperty<int> RowsProperty = AvaloniaProperty.Register<RadioCheckEnumGroup, int>(nameof(Rows));

    public static readonly DirectProperty<RadioCheckEnumGroup, object?> SelectedEnumValueProperty =
        AvaloniaProperty.RegisterDirect<RadioCheckEnumGroup, object?>(nameof(SelectedEnumValue),
            o => o.SelectedEnumValue,
            (o, v) => o.SelectedEnumValue = v,
            defaultBindingMode: BindingMode.TwoWay);

    private readonly Dictionary<ToggleButton, (int v, bool multiple)> _lookupControls = new();

    private int _columns;

    private bool _inClickEvent;

    private int _rows;

    private object? _selectedEnumValue;

    #endregion

    static RadioCheckEnumGroup()
    {
        AffectsArrange<RadioCheckEnumGroup>(ButtonMarginProperty, OrientationProperty, FirstColumnProperty, ColumnsProperty, RowsProperty);
        AffectsMeasure<RadioCheckEnumGroup>(RowsProperty, ColumnsProperty, FirstColumnProperty);
    }

    #region Properties

    public Thickness? ButtonMargin
    {
        get => GetValue(ButtonMarginProperty);
        set => SetValue(ButtonMarginProperty, value);
    }

    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public Type? EnumType
    {
        get;
        set => SetAndRaise(EnumTypeProperty, ref field, value);
    }

    public int FirstColumn
    {
        get => GetValue(FirstColumnProperty);
        set => SetValue(FirstColumnProperty, value);
    }

    public GroupOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public object? SelectedEnumValue
    {
        get => GetSelectedValue();
        set
        {
            _selectedEnumValue = value;
            if (IsLoaded && !_inClickEvent)
            {
                SetSelectedValue(value);
            }

            RaisePropertyChanged(SelectedEnumValueProperty, null, null);
        }
    }

    #endregion

    #region Events

    public event EventHandler? SelectionChanged;

    #endregion

    #region Static Methods

    private static int CountBits(int v)
    {
        var value = (uint) v;
        var count = 0;
        while (value != 0)
        {
            count++;
            value &= value - 1;
        }

        return count;
    }

    private static string? GetEnumDescription(object enumerationValue)
    {
        var type = enumerationValue.GetType();

        if (!type.IsEnum)
        {
            throw new ArgumentException(@"EnumerationValue must be of Enum type", nameof(enumerationValue));
        }

        var enumName = enumerationValue.ToString();

        //Tries to find a DescriptionAttribute for a potential friendly name
        //for the enum
        if (!string.IsNullOrEmpty(enumName))
        {
            var memberInfo = type.GetMember(enumName);

            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute) attrs[0]).Description;
                }
            }
        }

        //If we have no description attribute, just return the ToString of the enum
        return enumName;
    }

    #endregion

    #region Override Methods

    protected override Size ArrangeOverride(Size finalSize)
    {
        var x = FirstColumn;
        var y = 0;

        var width = finalSize.Width / _columns;
        var height = finalSize.Height / _rows;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            child.Arrange(new Rect(x * width, y * height, width, height));

            if (Orientation == GroupOrientation.Vertical)
            {
                y++;
                if (y >= _rows)
                {
                    y = 0;
                    x++;
                }
            }
            else
            {
                x++;

                if (x >= _columns)
                {
                    x = 0;
                    y++;
                }
            }
        }

        return finalSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateRowsAndColumns();

        var maxWidth = 0d;
        var maxHeight = 0d;

        var childAvailableSize = new Size(availableSize.Width / _columns, availableSize.Height / _rows);

        foreach (var child in Children)
        {
            child.Measure(childAvailableSize);

            if (child.DesiredSize.Width > maxWidth)
            {
                maxWidth = child.DesiredSize.Width;
            }

            if (child.DesiredSize.Height > maxHeight)
            {
                maxHeight = child.DesiredSize.Height;
            }
        }

        return new Size(maxWidth * _columns, maxHeight * _rows);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetUpEnumItems();
        SetSelectedValue(_selectedEnumValue);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == EnumTypeProperty && IsLoaded)
        {
            SetUpEnumItems();
        }
    }

    #endregion

    #region Public Methods

    public T? GetSelectedValueAsEnum<T>() where T : Enum
    {
        var result = SelectedEnumValue;
        try
        {
            return (T?) result;
        }
        catch (Exception)
        {
            return default;
        }
    }

    #endregion

    #region Private Methods

    private void CbOnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        _inClickEvent = true;
        try
        {
            if (sender is CheckBox checkBox)
            {
                var isChecked = checkBox.IsChecked == true;
                var tag = 0;
                var isMultiple = false;
                if (_lookupControls.TryGetValue(checkBox, out var valueTuple))
                {
                    tag = valueTuple.v;
                    isMultiple = valueTuple.multiple;
                }

                if (isChecked)
                {
                    if (isMultiple)
                    {
                        //set IsChecked for all others who fit in this tag
                        foreach (var childKeyPair in _lookupControls)
                        {
                            if (childKeyPair.Key != checkBox && (childKeyPair.Value.v & tag) == childKeyPair.Value.v)
                            {
                                childKeyPair.Key.IsChecked = true;
                            }
                        }
                    }
                }
                else
                {
                    //uncheck all those that fit
                    foreach (var item in _lookupControls)
                    {
                        if (item.Key != checkBox && (item.Value.v & tag) > 0)
                        {
                            item.Key.IsChecked = false;
                        }
                    }
                }

                //determine tag for all single items
                var singleValue = _lookupControls.Where(item => item.Key.IsChecked == true && !item.Value.multiple).Aggregate(0, (current, item) => current | item.Value.v);

                //now check if other multiples need to be set
                foreach (var lookupControl in _lookupControls)
                {
                    if (lookupControl.Key != checkBox && lookupControl.Value.multiple && (singleValue & lookupControl.Value.v) == lookupControl.Value.v)
                    {
                        lookupControl.Key.IsChecked = true;
                    }
                }
            }

            RaisePropertyChanged(SelectedEnumValueProperty, null, null);
        }
        finally
        {
            _inClickEvent = false;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private object? GetSelectedValue()
    {
        var result = 0;
        if (EnumType != null && IsLoaded)
        {
            foreach (var childKeyPair in _lookupControls)
            {
                var isChecked = childKeyPair.Key.IsChecked == true;
                if (childKeyPair.Key is RadioButton)
                {
                    if (isChecked)
                    {
                        result = childKeyPair.Value.v;
                        break;
                    }
                }
                else
                {
                    if (isChecked)
                    {
                        result |= childKeyPair.Value.v;
                    }
                }
            }
        }

        return result;
    }

    private void rbOnClick(object? sender, RoutedEventArgs e)
    {
        RaisePropertyChanged(SelectedEnumValueProperty, null, null);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetSelectedValue(object? value)
    {
        if (EnumType != null && IsLoaded && !_inClickEvent)
        {
            var intValue = (int) (value ?? 0);
            foreach (var childKeyPair in _lookupControls)
            {
                if (childKeyPair.Key is RadioButton)
                {
                    childKeyPair.Key.IsChecked = childKeyPair.Value.v == intValue;
                }
                else
                {
                    childKeyPair.Key.IsChecked = (childKeyPair.Value.v & intValue) == childKeyPair.Value.v;
                }
            }
        }
    }

    private void SetUpEnumItems()
    {
        Children.Clear();
        _lookupControls.Clear();
        if (EnumType != null)
        {
            var hasFlags = EnumType.GetCustomAttributes().OfType<FlagsAttribute>().FirstOrDefault() != null;
            var rgGroup = Guid.NewGuid().ToString();
            foreach (var enumValue in EnumType.GetEnumValues())
            {
                var s = GetEnumDescription(enumValue);
                var t = (int) enumValue;
                if (hasFlags)
                {
                    var cb = new CheckBox {IsThreeState = false, Content = s, Tag = t};
                    cb.Click += CbOnIsCheckedChanged;
                    if (ButtonMargin != null)
                    {
                        cb.Margin = ButtonMargin.Value;
                    }
                    else
                    {
                        cb.Margin = new Thickness(0);
                    }

                    Children.Add(cb);
                    _lookupControls.Add(cb, (t, CountBits(t) > 1));
                }
                else
                {
                    var rb = new RadioButton {GroupName = rgGroup, Content = s, Tag = t};
                    rb.Click += rbOnClick;
                    if (ButtonMargin != null)
                    {
                        rb.Margin = ButtonMargin.Value;
                    }
                    else
                    {
                        rb.Margin = new Thickness(0);
                    }

                    Children.Add(rb);
                    _lookupControls.Add(rb, (t, false));
                }
            }
        }

        InvalidateArrange();
    }

    private void UpdateRowsAndColumns()
    {
        _rows = Rows;
        _columns = Columns;

        if (FirstColumn >= Columns)
        {
            SetCurrentValue(FirstColumnProperty, 0);
        }

        var itemCount = FirstColumn;

        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                itemCount++;
            }
        }

        if (_rows == 0)
        {
            if (_columns == 0)
            {
                switch (Orientation)
                {
                    case GroupOrientation.Square:
                        _rows = _columns = (int) Math.Ceiling(Math.Sqrt(itemCount));
                        break;
                    case GroupOrientation.Horizontal:
                        _columns = itemCount;
                        _rows = 1;
                        break;
                    case GroupOrientation.Vertical:
                        _columns = 1;
                        _rows = itemCount;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                _rows = Math.DivRem(itemCount, _columns, out var rem);

                if (rem != 0)
                {
                    _rows++;
                }
            }
        }
        else if (_columns == 0)
        {
            _columns = Math.DivRem(itemCount, _rows, out var rem);

            if (rem != 0)
            {
                _columns++;
            }
        }
    }

    #endregion
}