// RadioGroup.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;

#endregion

namespace AJBAvalonia;

/// <summary>
/// Layout control that arranges radio buttons in a grid-like pattern with optional custom button.
/// </summary>
public class RadioGroup : Panel
{
    #region Fields

    public static readonly StyledProperty<int> ColumnsProperty = AvaloniaProperty.Register<RadioGroup, int>(nameof(Columns));

    public static readonly DirectProperty<RadioGroup, string?> CustomButtonEditValueProperty =
        AvaloniaProperty.RegisterDirect<RadioGroup, string?>(nameof(CustomButtonEditValue), o => o.CustomButtonEditValue, (o, v) => o.CustomButtonEditValue = v);

    public static readonly DirectProperty<RadioGroup, string?> CustomButtonTextProperty =
        AvaloniaProperty.RegisterDirect<RadioGroup, string?>(nameof(CustomButtonText), o => o.CustomButtonText, (o, v) => o.CustomButtonText = v);

    public static readonly StyledProperty<int> FirstColumnProperty = AvaloniaProperty.Register<RadioGroup, int>(nameof(FirstColumn));

    public static readonly DirectProperty<RadioGroup, bool> HasCustomButtonProperty =
        AvaloniaProperty.RegisterDirect<RadioGroup, bool>(nameof(HasCustomButton), o => o.HasCustomButton, (o, v) => o.HasCustomButton = v);

    public static readonly DirectProperty<RadioGroup, string?> ItemListProperty =
        AvaloniaProperty.RegisterDirect<RadioGroup, string?>(nameof(ItemList), o => o.ItemList, (o, v) => o.ItemList = v);

    public static readonly DirectProperty<RadioGroup, GroupOrientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<RadioGroup, GroupOrientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    public static readonly StyledProperty<Thickness?> RadioButtonMarginProperty = AvaloniaProperty.Register<RadioGroup, Thickness?>(nameof(RadioButtonMargin));

    public static readonly StyledProperty<int> RowsProperty = AvaloniaProperty.Register<RadioGroup, int>(nameof(Rows));

    public static readonly StyledProperty<string?> SelectedItemValueProperty =
        AvaloniaProperty.Register<RadioGroup, string?>(nameof(SelectedItemValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<int> SelectedValueProperty = AvaloniaProperty.Register<RadioGroup, int>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    private readonly string _groupName;
    private int _columns;

    private bool _inClick;

    private string[]? _itemMap;

    private int _rows;

    private RadioButton? customButton;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioGroup"/> class.
    /// </summary>
    public RadioGroup()
    {
        _groupName = Guid.NewGuid().ToString();
    }

    #region Properties

    /// <summary>
    /// Gets or sets the number of columns to arrange buttons into.
    /// </summary>
    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the value entered in the custom button edit field.
    /// </summary>
    public string? CustomButtonEditValue
    {
        get;
        set => SetAndRaise(CustomButtonEditValueProperty, ref field, value);
    } = string.Empty;

    /// <summary>
    /// Gets or sets the text displayed on the custom button.
    /// </summary>
    public string? CustomButtonText
    {
        get;
        set => SetAndRaise(CustomButtonTextProperty, ref field, value);
    } = "Custom";

    /// <summary>
    /// Gets or sets the index of the first column used when arranging children.
    /// </summary>
    public int FirstColumn
    {
        get => GetValue(FirstColumnProperty);
        set => SetValue(FirstColumnProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a custom radio button with an edit field is present.
    /// </summary>
    public bool HasCustomButton
    {
        get;
        set => SetAndRaise(HasCustomButtonProperty, ref field, value);
    }

    /// <summary>
    /// Holds comma delimited text to generate radio buttons from.
    /// </summary>
    [Category("Radio Group Props")]
    [Description("Holds comma delimited text for radiobuttons")]
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
                SetSelectedRadioButton(0);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the list fills vertically or horizontally.
    /// </summary>
    [Category("Radio Group Props")]
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

    /// <summary>
    /// Gets or sets margin applied to the radio buttons.
    /// </summary>
    public Thickness? RadioButtonMargin
    {
        get => GetValue(RadioButtonMarginProperty);
        set => SetValue(RadioButtonMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of rows used for layout.
    /// </summary>
    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the selected item is the custom value.
    /// </summary>
    public bool SelectedIsCustom => customButton?.IsChecked == true;

    /// <summary>
    /// Gets or sets the string representation of the selected item.
    /// </summary>
    public string? SelectedItemValue
    {
        get => GetValue(SelectedItemValueProperty);
        set => SetValue(SelectedItemValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the numeric index of the selected value.
    /// </summary>
    public int SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    #endregion

    #region Override Methods

    /// <summary>
    /// Arranges child elements in the computed grid layout.
    /// </summary>
    /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
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

    /// <summary>
    /// Measures the size required for children and determines desired size.
    /// </summary>
    /// <param name="availableSize">The available size that the parent can give to the child.</param>
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

    /// <summary>
    /// Responds to dependency property changes to update selection and layout.
    /// </summary>
    /// <param name="change">Change information for the property change.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedItemValueProperty)
        {
            SetItemValue((string?) change.NewValue);
        }
        else if (change.Property == SelectedValueProperty && change.NewValue != null)
        {
            SetSelectedRadioButton((int) change.NewValue);
        }
        else if (change.Property == RadioButtonMarginProperty)
        {
            ChangeRadioMargin();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the item text at the given numeric value index.
    /// </summary>
    /// <param name="value">Index of the item.</param>
    /// <returns>The item text or null if index is out of range.</returns>
    public string? GetItemAtValue(int value)
    {
        if (value >= 0 && value < Children.Count)
        {
            switch (Children[value])
            {
                case RadioButton rb:
                    return (string?) rb.Content;
                case Grid when HasCustomButton && customButton != null:
                    return CustomButtonEditValue;
            }
        }

        return null;
    }

    /// <summary>
    /// Loads radio buttons from a list of strings.
    /// </summary>
    /// <param name="list">List of strings for radio button labels.</param>
    public void LoadContent(List<string> list)
    {
        InternalMakeRadioButtons(list);
        InvalidateVisual();
    }

    /// <summary>
    /// Loads radio buttons from an array of strings.
    /// </summary>
    /// <param name="list">Array of strings for radio button labels.</param>
    public void LoadContent(string[] list)
    {
        InternalMakeRadioButtons(list);
        InvalidateVisual();
    }

    #endregion

    #region Private Methods

    private void ChangeRadioMargin()
    {
        var value = RadioButtonMargin ?? new Thickness(0);
        foreach (var item in Children)
        {
            if (item is { } rb)
            {
                rb.Margin = value;
            }
        }

        InvalidateArrange();
    }

    private string GetSelectedItemValue()
    {
        var i = GetSelectedRadioButton();

        if (_itemMap != null && i >= 0 && i < _itemMap.Length)
        {
            return _itemMap[i];
        }

        if (SelectedIsCustom)
        {
            return CustomButtonEditValue ?? string.Empty;
        }

        return string.Empty;
    }

    private int GetSelectedRadioButton()
    {
        var v = 0;
        var cnt = Children.Count;

        while (v < cnt && !IsRBChecked(Children[v]))
        {
            v++;
        }

        if (v >= cnt)
        {
            v = 0;
        }

        return v;
    }

    private void InternalMakeRadioButtons(IEnumerable<string> list)
    {
        Children.Clear();

        foreach (var s in list)
        {
            var rb = new RadioButton {Content = s, GroupName = _groupName, VerticalAlignment = VerticalAlignment.Center};
            rb.Click += (_, _) => RbClickExecute();
            if (RadioButtonMargin != null)
            {
                rb.Margin = RadioButtonMargin.Value;
            }

            Children.Add(rb);
        }

        if (HasCustomButton)
        {
            var customGrid = new Grid {ColumnDefinitions = ColumnDefinitions.Parse("Auto,*")};
            customButton = new RadioButton {Content = CustomButtonText, GroupName = _groupName, VerticalAlignment = VerticalAlignment.Center};
            customButton.Click += (_, _) => RbClickExecute();
            Grid.SetColumn(customButton, 0);
            var edit = new TextBox {VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 5, 0)};
            var b = new Binding(nameof(CustomButtonEditValue), BindingMode.TwoWay) {Source = this};
            edit.Bind(TextBox.TextProperty, b);
            var c = new Binding(nameof(RadioButton.IsChecked)) {Source = customButton};
            edit.Bind(IsEnabledProperty, c);
            Grid.SetColumn(edit, 1);
            customGrid.Children.Add(customButton);
            customGrid.Children.Add(edit);
            if (RadioButtonMargin != null)
            {
                customGrid.Margin = RadioButtonMargin.Value;
            }

            Children.Add(customGrid);
        }
    }

    private bool IsRBChecked(Control item)
    {
        if (item is RadioButton rb)
        {
            return rb.IsChecked == true;
        }

        if (item is Grid && HasCustomButton && customButton != null)
        {
            return customButton.IsChecked == true;
        }

        return false;
    }

    private void RbClickExecute()
    {
        _inClick = true;

        try
        {
            SelectedValue = GetSelectedRadioButton();
            SelectedItemValue = GetSelectedItemValue();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _inClick = false;
        }
    }

    private void SetItemValue(string? value)
    {
        if (_itemMap != null && !string.IsNullOrEmpty(value))
        {
            SetSelectedRadioButton(Array.IndexOf(_itemMap, value));
        }
    }

    private void SetSelectedRadioButton(int value)
    {
        if (!_inClick && value >= 0 && value < Children.Count)
        {
            if (Children[value] is RadioButton rb)
            {
                rb.IsChecked = true;
            }
            else if (HasCustomButton && customButton != null)
            {
                customButton.IsChecked = true;
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