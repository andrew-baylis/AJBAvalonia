// CheckComboBoxEx.cs
//  Andrew Baylis
//  Created: 25/11/2025

#region using

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

#endregion

namespace AJBAvalonia;

/// <summary>
///     A combo box control that supports selecting multiple items via checkboxes and displays a summary or the selected
///     items.
/// </summary>
public class CheckComboBoxEx : TemplatedControl
{
    #region Avalonia Properties

    public static readonly StyledProperty<IBrush?> ClearButtonBackgroundProperty =
        AvaloniaProperty.Register<CheckComboBoxEx, IBrush?>(nameof(ClearButtonBackground), Brushes.Transparent);

    public static readonly StyledProperty<IBinding?> DisplayMemberBindingProperty = AvaloniaProperty.Register<CheckComboBoxEx, IBinding?>(nameof(DisplayMemberBinding));

    public static readonly DirectProperty<CheckComboBoxEx, Geometry?> DropGlyphPathProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, Geometry?>(nameof(DropGlyphPath), o => o.DropGlyphPath, (o, v) => o.DropGlyphPath = v);

    public static readonly DirectProperty<CheckComboBoxEx, double?> DropGlyphSizeProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, double?>(nameof(DropGlyphSize), o => o.DropGlyphSize, (o, v) => o.DropGlyphSize = v);

    public static readonly DirectProperty<CheckComboBoxEx, ComboGlyphEnum> GlyphTypeProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, ComboGlyphEnum>(nameof(GlyphType), o => o.GlyphType, (o, v) => o.GlyphType = v);

    public static readonly DirectProperty<CheckComboBoxEx, HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, HorizontalAlignment>(nameof(HorizontalContentAlignment),
                                                                              o => o.HorizontalContentAlignment,
                                                                              (o, v) => o.HorizontalContentAlignment = v);

    public static readonly DirectProperty<CheckComboBoxEx, bool> IsDropDownOpenProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, bool>(nameof(IsDropDownOpen), o => o.IsDropDownOpen, (o, v) => o.IsDropDownOpen = v);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<CheckComboBoxEx, IEnumerable?>(nameof(ItemsSource));

    public static readonly DirectProperty<CheckComboBoxEx, IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, IDataTemplate?>(nameof(ItemTemplate), o => o.ItemTemplate, (o, v) => o.ItemTemplate = v);

    public static readonly DirectProperty<CheckComboBoxEx, int> MaxDropDownHeightProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, int>(nameof(MaxDropDownHeight), o => o.MaxDropDownHeight, (o, v) => o.MaxDropDownHeight = v);

    public static readonly DirectProperty<CheckComboBoxEx, int> MaxSelectedItemsBeforeSummaryTextProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, int>(nameof(MaxSelectedItemsBeforeSummaryText),
                                                              o => o.MaxSelectedItemsBeforeSummaryText,
                                                              (o, v) => o.MaxSelectedItemsBeforeSummaryText = v);

    public static readonly DirectProperty<CheckComboBoxEx, FontStyle> PlaceholderFontStyleProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, FontStyle>(nameof(PlaceholderFontStyle), o => o.PlaceholderFontStyle, (o, v) => o.PlaceholderFontStyle = v);

    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty = AvaloniaProperty.Register<CheckComboBoxEx, IBrush?>(nameof(PlaceholderForeground));

    public static readonly StyledProperty<string?> PlaceholderTextProperty = AvaloniaProperty.Register<CheckComboBoxEx, string?>(nameof(PlaceholderText));

    public static readonly DirectProperty<CheckComboBoxEx, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, int>(nameof(SelectedCount), o => o.SelectedCount);

    public static readonly DirectProperty<CheckComboBoxEx, IBrush?> SelectedItemBackgroundProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, IBrush?>(nameof(SelectedItemBackground), o => o.SelectedItemBackground, (o, v) => o.SelectedItemBackground = v);

    public static readonly DirectProperty<CheckComboBoxEx, IBrush?> SelectedItemBorderBrushProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, IBrush?>(nameof(SelectedItemBorderBrush), o => o.SelectedItemBorderBrush, (o, v) => o.SelectedItemBorderBrush = v);

    public static readonly DirectProperty<CheckComboBoxEx, CornerRadius> SelectedItemCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, CornerRadius>(nameof(SelectedItemCornerRadius), o => o.SelectedItemCornerRadius, (o, v) => o.SelectedItemCornerRadius = v);

    public static readonly DirectProperty<CheckComboBoxEx, Thickness> SelectedItemsBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, Thickness>(nameof(SelectedItemsBorderThickness),
                                                                    o => o.SelectedItemsBorderThickness,
                                                                    (o, v) => o.SelectedItemsBorderThickness = v);

    public static readonly DirectProperty<CheckComboBoxEx, Thickness> SelectedItemsMarginProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, Thickness>(nameof(SelectedItemsMargin), o => o.SelectedItemsMargin, (o, v) => o.SelectedItemsMargin = v);

    public static readonly StyledProperty<IEnumerable?> SelectedItemsProperty = AvaloniaProperty.Register<CheckComboBoxEx, IEnumerable?>(nameof(SelectedItems));

    public static readonly DirectProperty<CheckComboBoxEx, IDataTemplate?> SelectedItemsTemplateProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, IDataTemplate?>(nameof(SelectedItemsTemplate), o => o.SelectedItemsTemplate, (o, v) => o.SelectedItemsTemplate = v);

    public static readonly DirectProperty<CheckComboBoxEx, SelectionMode> SelectionModeProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, SelectionMode>(nameof(SelectionMode), o => o.SelectionMode, (o, v) => o.SelectionMode = v);

    public static readonly StyledProperty<bool> ShowClearButtonProperty = AvaloniaProperty.Register<CheckComboBoxEx, bool>(nameof(ShowClearButton));

    public static readonly DirectProperty<CheckComboBoxEx, MultiSelectDisplayTypeEnum> SummaryDisplayTypeProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, MultiSelectDisplayTypeEnum>(nameof(SummaryDisplayType), o => o.SummaryDisplayType, (o, v) => o.SummaryDisplayType = v);

    public static readonly DirectProperty<CheckComboBoxEx, string?> SummaryTextFormatStringProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, string?>(nameof(SummaryTextFormatString), o => o.SummaryTextFormatString, (o, v) => o.SummaryTextFormatString = v);

    public static readonly DirectProperty<CheckComboBoxEx, VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.RegisterDirect<CheckComboBoxEx, VerticalAlignment>(nameof(VerticalContentAlignment),
                                                                            o => o.VerticalContentAlignment,
                                                                            (o, v) => o.VerticalContentAlignment = v);

    #endregion

    #region Fields

    private const string _comboDrop = "M1939 486 L2029 576 L1024 1581 L19 576 L109 486 L1024 1401 L1939 486 Z";

    private const string _filter = "m 349.34618,456.25731 h 180 v 20 l -70,90 v 100 l -40,-20 v -80 l -70,-90 v -20";

    private const string _glassLeft =
        "m 443.50703,1.4041 c -169.2,13.5 -317,112.6 -392.400005,263.2 -37.8,75.7 -55.6999996,162.9 -50.0999996,244.1 6,85.9 32.2999996,164.6 78.4999996,234.7 105.800005,160.4 297.600005,241.9 487.000005,207 28.7,-5.3 60.3,-14.4 88,-25.2 16.5,-6.4 53.7,-24.5 68.4,-33.3 l 10.4,-6.2 197.1,197.1 197.09997,197.1 76.3,-76.2 76.2,-76.3 -197.1,-197.1 -197.09997,-197.1 6.2,-10.4 c 8.8,-14.7 26.9,-51.9 33.3,-68.4 48.8,-125.1 43.5,-263.9 -14.7,-384 -46.3,-95.6 -121.6,-172 -216.3,-219.4 -77.9,-38.9 -165.9,-56.3 -250.8,-49.6 z m 76,161 c 108.3,14.6 198.7,79.2 246.5,176 15.8,31.9 25.4,63.7 30.7,101.5 2.4,17.6 2.4,61.4 0,79 -11,78.2 -45,143.8 -101.7,196.2 -48.2,44.5 -107.4,71.9 -176,81.5 -17.6,2.4 -61.4,2.4 -79,0 -37.8,-5.3 -69.6,-14.9 -101.5,-30.7 -106.8,-52.7 -174.8,-158.6 -178.2,-277.5 -2.6,-92.1 34,-179.6 101.3,-242.6 52.1,-48.7 117.6,-77.8 191.2,-84.9 13.1,-1.3 52.9,-0.4 66.7,1.5 z";

    private const string _glassRight =
        "m 836.5,1.4041 c 169.2,13.5 317,112.6 392.4,263.2 37.8,75.7 55.7,162.9 50.1,244.1 -6,85.9 -32.3,164.6 -78.5,234.7 -105.8,160.4 -297.6,241.9 -487,207 -28.7,-5.3 -60.3,-14.4 -88,-25.2 -16.5,-6.4 -53.7,-24.5 -68.4,-33.3 l -10.4,-6.2 -197.1,197.1 -197.1,197.1 -76.3,-76.2 -76.2,-76.3 197.1,-197.1 197.1,-197.1 -6.2,-10.4 c -8.8,-14.7 -26.9,-51.9 -33.3,-68.4 -48.8,-125.1 -43.5,-263.9 14.7,-384 46.3,-95.6 121.6,-172 216.3,-219.4 77.9,-38.9 165.9,-56.3 250.8,-49.6 z m -76,161 c -108.3,14.6 -198.7,79.2 -246.5,176 -15.8,31.9 -25.4,63.7 -30.7,101.5 -2.4,17.6 -2.4,61.4 0,79 11,78.2 45,143.8 101.7,196.2 48.2,44.5 107.4,71.9 176,81.5 17.6,2.4 61.4,2.4 79,0 37.8,-5.3 69.6,-14.9 101.5,-30.7 106.8,-52.7 174.8,-158.6 178.2,-277.5 2.6,-92.1 -34,-179.6 -101.3,-242.6 -52.1,-48.7 -117.6,-77.8 -191.2,-84.9 -13.1,-1.3 -52.9,-0.4 -66.7,1.5 z";
    private const string _summarytextFormat = "{0} items selected";

    internal const string pcDropdownOpen = ":dropdownopen";
    internal const string pcPressed = ":pressed";

    private readonly ObservableCollection<CheckComboBoxItem> _checkItems = new();

    private readonly ObservableCollection<object?> _selectedItemsCollection;
    private Border? _borderBackground;
    private ComboClearButton? _clearButton;

    private Geometry? _dropGlyphPath;

    private double? _dropGlyphSize;

    private PathIcon? _dropIcon;

    private ListBox? _dropListbox;

    private IDataTemplate? _itemTemplate;

    private bool _needToSetPropertiesOnLoad;

    private Popup? _popup;

    private IBrush? _selectedItemBackground;

    private IBrush? _selectedItemBorderBrush;

    private CornerRadius _selectedItemCornerRadius = new(5);

    private Thickness _selectedItemsBorderThickness;

    private MultiSelectDisplay? _selectedItemsControl;

    private Thickness _selectedItemsMargin;

    private IDataTemplate? _selectedItemsTemplate;

    private MultiSelectDisplayTypeEnum _summaryDisplayType;

    private string? _summaryTextFormatString;

    #endregion

    /// <summary>
    ///     Initializes a new instance of the <see cref="CheckComboBoxEx" /> class.
    /// </summary>
    public CheckComboBoxEx()
    {
        PlaceholderForeground = Brushes.LightGray;
        PlaceholderFontStyle = FontStyle.Italic;
        _selectedItemsCollection = new ObservableCollection<object?>();
    }

    #region Properties

    /// <summary>
    ///     Gets or sets the background brush for the clear button.
    /// </summary>
    public IBrush? ClearButtonBackground
    {
        get => GetValue(ClearButtonBackgroundProperty);
        set => SetValue(ClearButtonBackgroundProperty, value);
    }

    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource), AncestorType = typeof(CheckComboBoxEx))]
    public IBinding? DisplayMemberBinding
    {
        get => GetValue(DisplayMemberBindingProperty);
        set => SetValue(DisplayMemberBindingProperty, value);
    }

    /// <summary>
    ///     Gets or sets the path geometry used for the drop glyph icon.
    /// </summary>
    public Geometry? DropGlyphPath
    {
        get => _dropGlyphPath;
        set
        {
            SetAndRaise(DropGlyphPathProperty, ref _dropGlyphPath, value);
            UpdateGlyph();
        }
    }

    /// <summary>
    ///     Gets or sets the size used for the drop glyph icon.
    /// </summary>
    public double? DropGlyphSize
    {
        get => _dropGlyphSize;
        set
        {
            SetAndRaise(DropGlyphSizeProperty, ref _dropGlyphSize, value);
            UpdateGlyph();
        }
    }

    /// <summary>
    ///     Gets or sets the glyph type used for the drop icon.
    /// </summary>
    public ComboGlyphEnum GlyphType
    {
        get;
        set
        {
            SetAndRaise(GlyphTypeProperty, ref field, value);
            switch (value)
            {
                case ComboGlyphEnum.Normal:
                    _dropGlyphPath = Geometry.Parse(_comboDrop);
                    _dropGlyphSize = 12;
                    break;
                case ComboGlyphEnum.GlassLeft:
                    _dropGlyphPath = Geometry.Parse(_glassLeft);
                    _dropGlyphSize ??= 14;
                    break;
                case ComboGlyphEnum.GlassRight:
                    _dropGlyphPath = Geometry.Parse(_glassRight);
                    _dropGlyphSize ??= 14;
                    break;
                case ComboGlyphEnum.Filter:
                    _dropGlyphPath = Geometry.Parse(_filter);
                    _dropGlyphSize ??= 14;
                    break;
                case ComboGlyphEnum.Custom:
                    // Custom glyph handling can be added here
                    break;
            }

            UpdateGlyph();
        }
    } = ComboGlyphEnum.Normal;

    /// <summary>
    ///     Gets or sets the horizontal alignment of the content.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get;
        set => SetAndRaise(HorizontalContentAlignmentProperty, ref field, value);
    } = HorizontalAlignment.Stretch;

    /// <summary>
    ///     Gets or sets a value indicating whether the drop-down is open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get;
        set => SetAndRaise(IsDropDownOpenProperty, ref field, value);
    }

    /// <summary>
    ///     Gets or sets the source collection for items in the control.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => _itemTemplate;
        set
        {
            SetAndRaise(ItemTemplateProperty, ref _itemTemplate, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.ItemTemplate = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the maximum height for the dropdown list.
    /// </summary>
    public int MaxDropDownHeight
    {
        get;
        set => SetAndRaise(MaxDropDownHeightProperty, ref field, value);
    } = 1000;

    /// <summary>
    ///     Gets or sets the threshold of selected items before showing summary text.
    /// </summary>
    public int MaxSelectedItemsBeforeSummaryText
    {
        get;
        set
        {
            SetAndRaise(MaxSelectedItemsBeforeSummaryTextProperty, ref field, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.MaxItemsBeforeDisplayText = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the font style used for placeholder text.
    /// </summary>
    public FontStyle PlaceholderFontStyle
    {
        get;
        set => SetAndRaise(PlaceholderFontStyleProperty, ref field, value);
    }

    /// <summary>
    ///     Gets or sets the brush used for placeholder text foreground.
    /// </summary>
    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    /// <summary>
    ///     Gets or sets the placeholder text shown when no selection exists.
    /// </summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    ///     Gets the number of selected items.
    /// </summary>
    public int SelectedCount => _selectedItemsCollection.Count;

    public IBrush? SelectedItemBackground
    {
        get => _selectedItemBackground;
        set
        {
            SetAndRaise(SelectedItemBackgroundProperty, ref _selectedItemBackground, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.DisplayItemBackground = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    public IBrush? SelectedItemBorderBrush
    {
        get => _selectedItemBorderBrush;
        set
        {
            SetAndRaise(SelectedItemBorderBrushProperty, ref _selectedItemBorderBrush, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.DisplayItemBorderBrush = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    public CornerRadius SelectedItemCornerRadius
    {
        get => _selectedItemCornerRadius;
        set
        {
            SetAndRaise(SelectedItemCornerRadiusProperty, ref _selectedItemCornerRadius, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.DisplayItemCornerRadius = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    public IEnumerable? SelectedItems
    {
        get => GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public Thickness SelectedItemsBorderThickness
    {
        get => _selectedItemsBorderThickness;
        set
        {
            SetAndRaise(SelectedItemsBorderThicknessProperty, ref _selectedItemsBorderThickness, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.DisplayItemBorderThickness = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    public Thickness SelectedItemsMargin
    {
        get => _selectedItemsMargin;
        set
        {
            SetAndRaise(SelectedItemsMarginProperty, ref _selectedItemsMargin, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.DisplayItemMargin = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    public IDataTemplate? SelectedItemsTemplate
    {
        get => _selectedItemsTemplate;
        set
        {
            SetAndRaise(SelectedItemsTemplateProperty, ref _selectedItemsTemplate, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.ItemTemplate = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the selection mode used in the dropdown list.
    /// </summary>
    public SelectionMode SelectionMode
    {
        get;
        set => SetAndRaise(SelectionModeProperty, ref field, value);
    } = SelectionMode.Multiple;

    /// <summary>
    ///     Gets or sets a value indicating whether the clear button is shown.
    /// </summary>
    public bool ShowClearButton
    {
        get => GetValue(ShowClearButtonProperty);
        set => SetValue(ShowClearButtonProperty, value);
    }

    public MultiSelectDisplayTypeEnum SummaryDisplayType
    {
        get => _summaryDisplayType;
        set
        {
            SetAndRaise(SummaryDisplayTypeProperty, ref _summaryDisplayType, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.DisplayItemDisplayType = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    public string? SummaryTextFormatString
    {
        get => _summaryTextFormatString;
        set
        {
            SetAndRaise(SummaryTextFormatStringProperty, ref _summaryTextFormatString, value);
            if (_selectedItemsControl != null)
            {
                _selectedItemsControl.DisplayTextFormatString = value;
            }
            else
            {
                _needToSetPropertiesOnLoad = true;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the vertical alignment for the content.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get;
        set => SetAndRaise(VerticalContentAlignmentProperty, ref field, value);
    } = VerticalAlignment.Center;

    #endregion

    #region Events

    /// <summary>
    ///     Occurs when the selection of items changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    #endregion

    #region Public Methods

    public IList GetSelectedItems()
    {
        return _selectedItemsCollection.ToList();
    }

    public void SetSelectedItems(IEnumerable selectedItems)
    {
        var selectionHash = selectedItems.Cast<object>().ToHashSet();
        foreach (var checkitem in _checkItems)
        {
            checkitem.IsSelected = checkitem.ItemData != null && selectionHash.Contains(checkitem.ItemData);
        }

        InternalSetSelectedItems();
    }

    public bool TryGetTotalCount(out int count)
    {
        count = SelectedCount;
        return true;
    }

    #endregion

    #region Protected Methods

    protected virtual void DoClear()
    {
        _selectedItemsCollection.Clear();

        foreach (var checkitem in _checkItems)
        {
            checkitem.IsSelected = false;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Applies the control template and initializes named parts.
    /// </summary>
    /// <param name="e">Template application arguments.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _selectedItemsControl = e.NameScope.Get<MultiSelectDisplay>("PART_ItemDisplay");
        _selectedItemsControl.ItemsSource = _selectedItemsCollection;
        _selectedItemsControl.PlaceholderForeground = PlaceholderForeground;
        _selectedItemsControl.PlaceholderFontStyle = PlaceholderFontStyle;
        _selectedItemsControl.PlaceholderText = PlaceholderText;

        SetMultiDisplayProperties();

        _popup = e.NameScope.Get<Popup>("PART_Popup");
        _popup.Opened += CheckComboBoxExPopupOpened;
        _popup.Closed += CheckComboBoxExPopupClosed;

        _dropListbox = e.NameScope.Get<ListBox>("PART_PopupListBox");
        _dropListbox.SelectionMode = SelectionMode;
        _dropListbox.ItemsSource = _checkItems;

        MakeAndSetListBoxItemTemplate();

        _dropIcon = e.NameScope.Find<PathIcon>("DropDownGlyph");
        if (DropGlyphPath != null && _dropIcon != null)
        {
            _dropIcon.Data = DropGlyphPath;
            if (DropGlyphSize != null)
            {
                _dropIcon.Height = DropGlyphSize.Value;
                _dropIcon.Width = DropGlyphSize.Value;
            }
        }

        _borderBackground = e.NameScope.Find<Border>("PART_Background");

        _clearButton = e.NameScope.Find<ComboClearButton>("PART_ClearButton");
        if (_clearButton != null)
        {
            _clearButton.Click += (_, _) => ClearCommandExecute();
        }
    }

    /// <summary>
    ///     Handles key down events for keyboard interaction with the control.
    /// </summary>
    /// <param name="e">Event arguments describing the key event.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if ((e.Key == Key.F4 && !e.KeyModifiers.HasFlag(KeyModifiers.Alt)) || ((e.Key == Key.Down || e.Key == Key.Up) && e.KeyModifiers.HasFlag(KeyModifiers.Alt)))
        {
            SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Escape)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else if (!IsDropDownOpen && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
            e.Handled = true;
        }
        else if (IsDropDownOpen && e.Key == Key.Tab)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _selectedItemsCollection.Clear();
        if (SelectedItems != null)
        {
            var selectedList = SelectedItems.Cast<object>().ToHashSet();

            foreach (var checkeditem in _checkItems)
            {
                checkeditem.IsSelected = checkeditem.ItemData != null && selectedList.Contains(checkeditem.ItemData);
            }

            InternalSetSelectedItems();

            if (_needToSetPropertiesOnLoad)
            {
                SetMultiDisplayProperties();
            }
        }
    }

    /// <summary>
    ///     Handles pointer pressed events to toggle drop-down open state and visual pressed pseudo-class.
    /// </summary>
    /// <param name="e">Pointer event arguments.</param>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.Handled && e.Source is Visual source)
        {
            if (_popup?.IsInsidePopup(source) == true)
            {
                e.Handled = true;
                return;
            }
        }

        if (IsDropDownOpen)
        {
            // When a drop-down is open with OverlayDismissEventPassThrough enabled and the control
            // is pressed, close the drop-down
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        else
        {
            var point = e.GetCurrentPoint(this).Position;
            var ctl = this.InputHitTest(point);
            if (ReferenceEquals(ctl, _borderBackground))
            {
                PseudoClasses.Set(pcPressed, true);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!e.Handled && e.Source is Visual source)
        {
            if (PseudoClasses.Contains(pcPressed) && !IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
                e.Handled = true;
            }
        }

        PseudoClasses.Set(pcPressed, false);
        base.OnPointerReleased(e);
    }

    /// <summary>
    ///     Called when an Avalonia property changes on this control.
    /// </summary>
    /// <param name="change">The change information for the property change.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedCountProperty)
        {
            PseudoClasses.Set(":empty", SelectedCount == 0);
            CheckClearEnabled();
            DataValidationErrors.ClearErrors(this);
        }
        else if (change.Property == ItemsSourceProperty)
        {
            UpdateItemsSource(change.OldValue, change.NewValue);
        }
        else if (change.Property == DisplayMemberBindingProperty)
        {
            _selectedItemsControl?.DisplayMemberBinding = DisplayMemberBinding;
            MakeAndSetListBoxItemTemplate();
        }
        else if (change.Property == ItemTemplateProperty)
        {
            MakeAndSetListBoxItemTemplate();
        }
        else if (change.Property == SelectionModeProperty && _dropListbox != null)
        {
            _dropListbox.SelectionMode = SelectionMode;
        }
    }

    /// <summary>
    ///     Rebuilds the internal check items when the ItemsSource changes.
    /// </summary>
    protected void UpdateCheckItems()
    {
        _checkItems.Clear();
        if (ItemsSource != null)
        {
            foreach (var item in ItemsSource)
            {
                var cbItem = new CheckComboBoxItem(item);
                _checkItems.Add(cbItem);
            }
        }
    }

    protected virtual void UpdateItemsSource(object? changeOldValue, object? changeNewValue)
    {
        switch (changeOldValue)
        {
            case INotifyCollectionChanged c:
                c.CollectionChanged -= OnItemsSourceCollectionChanged;
                break;
            case INotifyPropertyChanged p:
                p.PropertyChanged -= OnItemsSourcePropertyChanged;
                break;
        }

        switch (changeNewValue)
        {
            case INotifyCollectionChanged coll:
                coll.CollectionChanged += OnItemsSourceCollectionChanged;
                break;
            case INotifyPropertyChanged prop:
                prop.PropertyChanged += OnItemsSourcePropertyChanged;
                break;
        }

        UpdateCheckItems();
    }

    #endregion

    #region Private Methods

    private void CheckClearEnabled()
    {
        if (_clearButton != null)
        {
            _clearButton.IsEnabled = SelectedCount > 0;
        }
    }

    private void CheckComboBoxExPopupClosed(object? sender, EventArgs e)
    {
        InternalSetSelectedItems();
    }

    private void CheckComboBoxExPopupOpened(object? sender, EventArgs e)
    {
        //ensure SelectedItemsCollection is in sync with items that are checked
        foreach (var checkitem in _checkItems)
        {
            checkitem.IsSelected = _selectedItemsCollection.Contains(checkitem.ItemData);
        }
    }

    private void ClearCommandExecute()
    {
        DoClear();
    }

    private IDataTemplate? GetEffectiveItemTemplate()
    {
        if (ItemTemplate is { } itemTemplate)
        {
            return itemTemplate;
        }

        if (DisplayMemberBinding is { } binding)
        {
            return new FuncDataTemplate<object?>((_, _) => new TextBlock {[!TextBlock.TextProperty] = binding});
        }

        return new FuncDataTemplate<object?>((_, _) => new TextBlock {[!TextBlock.TextProperty] = new Binding()});
    }

    private void InternalClearSelectedItems()
    {
        _selectedItemsCollection.Clear();
        (SelectedItems as IList)?.Clear();
        RaisePropertyChanged(SelectedCountProperty, 0, SelectedCount);
        CheckClearEnabled();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void InternalSetSelectedItems()
    {
        //set SelectedItemsCollection and hence the display text
        _selectedItemsCollection.Clear();
        foreach (var checkeditem in _checkItems)
        {
            if (checkeditem.IsSelected)
            {
                _selectedItemsCollection.Add(checkeditem.ItemData);
            }
        }

        if (SelectedItems is IList list)
        {
            list.Clear();
            foreach (var selectedItem in _selectedItemsCollection)
            {
                list.Add(selectedItem);
            }
        }

        RaisePropertyChanged(SelectedCountProperty, 0, SelectedCount);
        CheckClearEnabled();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void MakeAndSetListBoxItemTemplate()
    {
        if (_dropListbox != null)
        {
            _dropListbox.ItemTemplate = new FuncDataTemplate<CheckComboBoxItem?>((_, _) =>
            {
                var checkBox = new CheckBox {VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch};
                checkBox[!ToggleButton.IsCheckedProperty] = new Binding(nameof(CheckComboBoxItem.IsSelected)) {Mode = BindingMode.TwoWay};
                var contentPresenter = new ContentControl
                {
                    VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(5, 2)
                };
                contentPresenter[!ContentPresenter.ContentProperty] = new Binding(nameof(CheckComboBoxItem.ItemData));
                contentPresenter.ContentTemplate = GetEffectiveItemTemplate();
                checkBox.Content = contentPresenter;
                return checkBox;
            });
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCheckItems();
        InternalClearSelectedItems();
    }

    private void OnItemsSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateCheckItems();
        InternalClearSelectedItems();
    }

    private void SetMultiDisplayProperties()
    {
        if (_selectedItemsControl != null)
        {
            _selectedItemsControl.DisplayItemBackground = SelectedItemBackground;
            _selectedItemsControl.DisplayItemBorderBrush = SelectedItemBorderBrush;
            _selectedItemsControl.DisplayItemBorderThickness = SelectedItemsBorderThickness;
            _selectedItemsControl.DisplayItemCornerRadius = SelectedItemCornerRadius;
            _selectedItemsControl.DisplayItemDisplayType = SummaryDisplayType;
            _selectedItemsControl.DisplayItemMargin = SelectedItemsMargin;
            _selectedItemsControl.DisplayMemberBinding = DisplayMemberBinding;
            _selectedItemsControl.DisplayTextFormatString = SummaryTextFormatString;
            _selectedItemsControl.MaxItemsBeforeDisplayText = MaxSelectedItemsBeforeSummaryText;
            _selectedItemsControl.ItemTemplate = ItemTemplate;
            _needToSetPropertiesOnLoad = false;
        }
    }

    private void UpdateGlyph()
    {
        if (IsLoaded && _dropIcon != null)
        {
            if (DropGlyphPath != null)
            {
                _dropIcon.Data = DropGlyphPath;
            }

            if (DropGlyphSize != null)
            {
                _dropIcon.Width = DropGlyphSize.Value;
                _dropIcon.Height = DropGlyphSize.Value;
            }
        }
    }

    #endregion
}

/// <summary>
///     Converter that returns true when the integer value is zero.
/// </summary>
internal class ValueIsZeroConverter : IValueConverter
{
    #region IValueConverter Members

    /// <summary>
    ///     Converts an integer to a boolean indicating if it is zero.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int and 0;
    }

    /// <summary>
    ///     Conversion back is not implemented.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion
}