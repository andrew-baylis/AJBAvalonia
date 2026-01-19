// CheckBoxGroup.cs
//  Andrew Baylis
//  Created: 19/02/2025

#region using

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;

#endregion

namespace AJBAvalonia;

public class CheckBoxGroup : Panel
{
    #region Avalonia Properties

    public static readonly StyledProperty<Thickness?> CheckBoxMarginProperty = AvaloniaProperty.Register<CheckBoxGroup, Thickness?>(nameof(CheckBoxButtonMargin));
    public static readonly StyledProperty<int> ColumnsProperty = AvaloniaProperty.Register<CheckBoxGroup, int>(nameof(Columns));

    public static readonly StyledProperty<int> FirstColumnProperty = AvaloniaProperty.Register<CheckBoxGroup, int>(nameof(FirstColumn));

    public static readonly DirectProperty<CheckBoxGroup, string?> ItemListProperty =
        AvaloniaProperty.RegisterDirect<CheckBoxGroup, string?>(nameof(ItemList), o => o.ItemList, (o, v) => o.ItemList = v);

    public static readonly DirectProperty<CheckBoxGroup, GroupOrientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<CheckBoxGroup, GroupOrientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    public static readonly StyledProperty<int> RowsProperty = AvaloniaProperty.Register<CheckBoxGroup, int>(nameof(Rows));

    public static readonly StyledProperty<string?> SelectedItemValueProperty =
        AvaloniaProperty.Register<CheckBoxGroup, string?>(nameof(SelectedItemValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<int> SelectedValueProperty = AvaloniaProperty.Register<CheckBoxGroup, int>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    #endregion

    #region Fields

    private int _columns;

    private bool _inClick;

    private string[]? _itemMap;

    private int _rows;

    #endregion

    #region Properties

    public Thickness? CheckBoxButtonMargin
    {
        get => GetValue(CheckBoxMarginProperty);
        set => SetValue(CheckBoxMarginProperty, value);
    }

    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public int FirstColumn
    {
        get => GetValue(FirstColumnProperty);
        set => SetValue(FirstColumnProperty, value);
    }

    [Category("Check Group Props")]
    [Description("Holds comma delimited text for checkboxes")]
    public string? ItemList
    {
        get;
        set
        {
            SetAndRaise(ItemListProperty, ref field, value);
            if (!string.IsNullOrEmpty(field))
            {
                _itemMap = field.Split(',', StringSplitOptions.TrimEntries);
                LoadContent(_itemMap);
                ClearAllCheckBoxes();
            }
        }
    }

    [Category("Check Group Props")]
    [Description("Whether the list fills vertically or horizontally")]
    public GroupOrientation Orientation
    {
        get;
        set
        {
            SetAndRaise(OrientationProperty, ref field, value);
            InvalidateArrange();
        }
    } = GroupOrientation.Horizontal;

    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public string? SelectedItemValue
    {
        get => GetValue(SelectedItemValueProperty);
        set => SetValue(SelectedItemValueProperty, value);
    }

    public int SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler? SelectionChanged;

    #endregion

    #region Public Methods

    public string? GetItemAtValue(int value)
    {
        if (value >= 0 && value < Children.Count && Children[value] is CheckBox cb)
        {
            return (string?) cb.Content;
        }

        return null;
    }

    public void LoadContent(List<string> list)
    {
        InternalMakeCheckBoxes(list);
        InvalidateVisual();
    }

    public void LoadContent(string[] list)
    {
        InternalMakeCheckBoxes(list);
        InvalidateVisual();
    }

    #endregion

    #region Protected Methods

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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedItemValueProperty)
        {
            SetItemValue((string?) change.NewValue);
        }
        else if (change.Property == SelectedValueProperty && change.NewValue != null)
        {
            SetSelectedCheckBoxes((int) change.NewValue);
        }
        else if (change.Property == CheckBoxMarginProperty)
        {
            ChangeCheckBoxMargin();
        }
    }

    #endregion

    #region Private Methods

    private void CbClickExecute()
    {
        _inClick = true;

        try
        {
            SelectedValue = GetSelectedCheckBoxes();
            SelectedItemValue = GetSelectedItemValue();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _inClick = false;
        }
    }

    private void ChangeCheckBoxMargin()
    {
        var value = CheckBoxButtonMargin ?? new Thickness(0);
        foreach (var item in Children)
        {
            if (item is { } rb)
            {
                rb.Margin = value;
            }
        }

        InvalidateArrange();
    }

    private void ClearAllCheckBoxes()
    {
        foreach (var cb in Children)
        {
            if (cb is CheckBox c)
            {
                c.IsChecked = false;
            }
        }
    }

    private int GetSelectedCheckBoxes()
    {
        var mask = 1;
        var cnt = Children.Count;
        var result = 0;
        for (var v = 0; v < cnt; v++)
        {
            if (Children[v] is CheckBox {IsChecked: true})
            {
                result |= mask;
            }

            mask = mask << 1;
        }

        return result;
    }

    private string GetSelectedItemValue()
    {
        var cnt = Children.Count;
        var resultList = new List<string>();
        if (_itemMap != null && _itemMap.Length >= cnt)
        {
            for (var v = 0; v < cnt; v++)
            {
                if (Children[v] is CheckBox {IsChecked: true})
                {
                    resultList.Add(_itemMap[v]);
                }
            }

            return string.Join(',', resultList);
        }

        return string.Empty;
    }

    private void InternalMakeCheckBoxes(IEnumerable<string> list)
    {
        Children.Clear();

        foreach (var s in list)
        {
            var rb = new CheckBox {Content = s, VerticalAlignment = VerticalAlignment.Center, IsThreeState = false};
            rb.Click += (_, _) => CbClickExecute();
            if (CheckBoxButtonMargin != null)
            {
                rb.Margin = CheckBoxButtonMargin.Value;
            }

            Children.Add(rb);
        }
    }

    private void SetItemValue(string? value)
    {
        if (!_inClick)
        {
            ClearAllCheckBoxes();
            if (_itemMap != null && !string.IsNullOrEmpty(value))
            {
                var splits = value.Split(',');
                foreach (var s in splits)
                {
                    var idx = Array.IndexOf(_itemMap, s);
                    if (idx >= 0)
                    {
                        SetSelectedCheckBox(idx);
                    }
                }

                SetSelectedCheckBox(Array.IndexOf(_itemMap, value));
            }
        }
    }

    private void SetSelectedCheckBox(int value)
    {
        if (!_inClick && value >= 0 && value < Children.Count)
        {
            if (Children[value] is CheckBox cb)
            {
                cb.IsChecked = true;
            }
        }
    }

    private void SetSelectedCheckBoxes(int value)
    {
        if (!_inClick)
        {
            ClearAllCheckBoxes();
            var mask = 1;
            var cnt = Children.Count;
            for (var v = 0; v < cnt; v++)
            {
                if ((value & mask) != 0)
                {
                    SetSelectedCheckBox(v);
                }

                mask = mask << 1;
            }
        }
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