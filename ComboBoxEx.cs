// ComboBoxEx.cs
//  Andrew Baylis
//  Created: 20/01/2024

#region using

using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

#endregion

namespace AJBAvalonia;

/// <summary>
///     Extended ComboBox with additional features such as clear button and customizable glyph.
/// </summary>
public class ComboBoxEx : ComboBox
{
    #region Avalonia Properties

    public static readonly StyledProperty<IBrush?> ClearButtonBackgroundProperty =
        AvaloniaProperty.Register<ComboBoxEx, IBrush?>(nameof(ClearButtonBackground), Brushes.Transparent);

    public static readonly DirectProperty<ComboBoxEx, Geometry?> DropGlyphPathProperty =
        AvaloniaProperty.RegisterDirect<ComboBoxEx, Geometry?>(nameof(DropGlyphPath), o => o.DropGlyphPath, (o, v) => o.DropGlyphPath = v);

    public static readonly DirectProperty<ComboBoxEx, double?> DropGlyphSizeProperty =
        AvaloniaProperty.RegisterDirect<ComboBoxEx, double?>(nameof(DropGlyphSize), o => o.DropGlyphSize, (o, v) => o.DropGlyphSize = v);

    public static readonly DirectProperty<ComboBoxEx, ComboGlyphEnum> GlyphTypeProperty =
        AvaloniaProperty.RegisterDirect<ComboBoxEx, ComboGlyphEnum>(nameof(GlyphType), o => o.GlyphType, (o, v) => o.GlyphType = v);

    public static readonly DirectProperty<ComboBoxEx, FontStyle> PlaceholderFontStyleProperty =
        AvaloniaProperty.RegisterDirect<ComboBoxEx, FontStyle>(nameof(PlaceholderFontStyle), o => o.PlaceholderFontStyle, (o, v) => o.PlaceholderFontStyle = v);

    public static readonly StyledProperty<bool> ShowClearButtonProperty = AvaloniaProperty.Register<ComboBoxEx, bool>(nameof(ShowClearButton));

    public static readonly DirectProperty<ComboBoxEx, string?> TextSearchPropertyNameProperty =
        AvaloniaProperty.RegisterDirect<ComboBoxEx, string?>(nameof(TextSearchPropertyName), o => o.TextSearchPropertyName, (o, v) => o.TextSearchPropertyName = v);

    #endregion

    #region Fields

    private const string _comboDrop = "M1939 486 L2029 576 L1024 1581 L19 576 L109 486 L1024 1401 L1939 486 Z";

    private const string _filter = "m 349.34618,456.25731 h 180 v 20 l -70,90 v 100 l -40,-20 v -80 l -70,-90 v -20";

    private const string _glassLeft =
        "m 443.50703,1.4041 c -169.2,13.5 -317,112.6 -392.400005,263.2 -37.8,75.7 -55.6999996,162.9 -50.0999996,244.1 6,85.9 32.2999996,164.6 78.4999996,234.7 105.800005,160.4 297.600005,241.9 487.000005,207 28.7,-5.3 60.3,-14.4 88,-25.2 16.5,-6.4 53.7,-24.5 68.4,-33.3 l 10.4,-6.2 197.1,197.1 197.09997,197.1 76.3,-76.2 76.2,-76.3 -197.1,-197.1 -197.09997,-197.1 6.2,-10.4 c 8.8,-14.7 26.9,-51.9 33.3,-68.4 48.8,-125.1 43.5,-263.9 -14.7,-384 -46.3,-95.6 -121.6,-172 -216.3,-219.4 -77.9,-38.9 -165.9,-56.3 -250.8,-49.6 z m 76,161 c 108.3,14.6 198.7,79.2 246.5,176 15.8,31.9 25.4,63.7 30.7,101.5 2.4,17.6 2.4,61.4 0,79 -11,78.2 -45,143.8 -101.7,196.2 -48.2,44.5 -107.4,71.9 -176,81.5 -17.6,2.4 -61.4,2.4 -79,0 -37.8,-5.3 -69.6,-14.9 -101.5,-30.7 -106.8,-52.7 -174.8,-158.6 -178.2,-277.5 -2.6,-92.1 34,-179.6 101.3,-242.6 52.1,-48.7 117.6,-77.8 191.2,-84.9 13.1,-1.3 52.9,-0.4 66.7,1.5 z";

    private const string _glassRight =
        "m 836.5,1.4041 c 169.2,13.5 317,112.6 392.4,263.2 37.8,75.7 55.7,162.9 50.1,244.1 -6,85.9 -32.3,164.6 -78.5,234.7 -105.8,160.4 -297.6,241.9 -487,207 -28.7,-5.3 -60.3,-14.4 -88,-25.2 -16.5,-6.4 -53.7,-24.5 -68.4,-33.3 l -10.4,-6.2 -197.1,197.1 -197.1,197.1 -76.3,-76.2 -76.2,-76.3 197.1,-197.1 197.1,-197.1 -6.2,-10.4 c -8.8,-14.7 -26.9,-51.9 -33.3,-68.4 -48.8,-125.1 -43.5,-263.9 14.7,-384 46.3,-95.6 121.6,-172 216.3,-219.4 77.9,-38.9 165.9,-56.3 250.8,-49.6 z m -76,161 c -108.3,14.6 -198.7,79.2 -246.5,176 -15.8,31.9 -25.4,63.7 -30.7,101.5 -2.4,17.6 -2.4,61.4 0,79 11,78.2 45,143.8 101.7,196.2 48.2,44.5 107.4,71.9 176,81.5 17.6,2.4 61.4,2.4 79,0 37.8,-5.3 69.6,-14.9 101.5,-30.7 106.8,-52.7 174.8,-158.6 178.2,-277.5 2.6,-92.1 -34,-179.6 -101.3,-242.6 -52.1,-48.7 -117.6,-77.8 -191.2,-84.9 -13.1,-1.3 -52.9,-0.4 -66.7,1.5 z";

    protected ContentControl? _comboBoxSelectionContentPresenter;
    private ComboClearButton? _clearButton;

    private Geometry? _dropGlyphPath;

    private double? _dropGlyphSize;

    private PathIcon? _dropIcon;

    private IPointer? _pointer;

    private string? _txtSearchTerm;
    private DispatcherTimer? _txtSearchTimer;

    #endregion

    static ComboBoxEx()
    {
        SelectedValueProperty.OverrideMetadata<ComboBoxEx>(new StyledPropertyMetadata<object?>(defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComboBoxEx" /> class.
    /// </summary>
    public ComboBoxEx()
    {
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

    /// <summary>
    ///     Gets or sets the geometry for the drop glyph icon.
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
    ///     Gets or sets the size of the drop glyph icon.
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
    ///     Gets or sets the placeholder font style.
    /// </summary>
    public FontStyle PlaceholderFontStyle
    {
        get;
        set => SetAndRaise(PlaceholderFontStyleProperty, ref field, value);
    } = FontStyle.Normal;

    /// <summary>
    ///     Gets or sets a value indicating whether the clear button is shown.
    /// </summary>
    public bool ShowClearButton
    {
        get => GetValue(ShowClearButtonProperty);
        set => SetValue(ShowClearButtonProperty, value);
    }

    /// <summary>
    ///     Gets or sets the property name used for text search.
    /// </summary>
    public string? TextSearchPropertyName
    {
        get;
        set => SetAndRaise(TextSearchPropertyNameProperty, ref field, value);
    }

    #endregion

    #region Protected Methods

    protected virtual void DoClear()
    {
        Clear();
    }

    /// <summary>
    ///     Applies the control template and locates named template parts.
    /// </summary>
    /// <param name="e">Template application arguments.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _comboBoxSelectionContentPresenter = e.NameScope.Find<ContentControl>("ContentPresenter");
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

        _clearButton = e.NameScope.Find<ComboClearButton>("PART_ClearButton");
        if (_clearButton != null)
        {
            _clearButton.Click += (_, _) => DoClear();
        }
    }

    /// <summary>
    ///     Handles control load event to set up the selection presenter template.
    /// </summary>
    /// <param name="e">Routed event args.</param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_comboBoxSelectionContentPresenter != null)
        {
            var temp = GetEffectiveSelectedItemTemplate();
            if (temp != null)
            {
                _comboBoxSelectionContentPresenter.ContentTemplate = temp;
            }
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _pointer = e.Pointer;
    }

    /// <summary>
    ///     Called when an Avalonia property changes on this control.
    /// </summary>
    /// <param name="change">The change information for the property change.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SelectedIndexProperty)
        {
            _pointer?.Capture(null);
            PseudoClasses.Set(":empty", SelectedIndex < 0);
            CheckCanClear();
            DataValidationErrors.ClearErrors(this);
        }
        else if (change.Property == ItemsSourceProperty)
        {
            UpdateItemsSource(change.OldValue, change.NewValue);
        }

        base.OnPropertyChanged(change);
    }

    /// <summary>
    ///     Handles text input for performing a text search within items when enabled.
    /// </summary>
    /// <param name="e">Text input event arguments.</param>
    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (!e.Handled)
        {
            if (!IsTextSearchEnabled)
            {
                return;
            }

            StopTextSearchTimer();

            _txtSearchTerm += e.Text;

            PropertyInfo? searchProp = null;

            bool MatchItem(object? item)
            {
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(TextSearchPropertyName) && searchProp == null)
                    {
                        searchProp = item.GetType().GetProperty(TextSearchPropertyName);
                    }

                    if (searchProp != null)
                    {
                        var searchText = searchProp.GetValue(item)?.ToString();
                        if (searchText?.StartsWith(_txtSearchTerm, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return item.ToString()?.StartsWith(_txtSearchTerm, StringComparison.OrdinalIgnoreCase) == true;
                    }
                }

                return false;
            }

            var item = Items.FirstOrDefault(MatchItem);

            if (item != null)
            {
                SelectedIndex = Items.IndexOf(item);
            }

            StartTextSearchTimer();

            e.Handled = true;
        }

        base.OnTextInput(e);
    }

    /// <summary>
    ///     Updates data validation state when bindings report validation for properties.
    /// </summary>
    /// <param name="property">The property being validated.</param>
    /// <param name="state">The validation state.</param>
    /// <param name="error">Validation error if any.</param>
    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        if (property == SelectedValueProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
        else
        {
            base.UpdateDataValidation(property, state, error);
        }
    }

    /// <summary>
    ///     Subscribes and unsubscribes to item source change notifications.
    /// </summary>
    /// <param name="changeOldValue">Old items source value.</param>
    /// <param name="changeNewValue">New items source value.</param>
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
    }

    #endregion

    #region Private Methods

    private void CheckCanClear()
    {
        if (_clearButton != null)
        {
            _clearButton.IsEnabled = SelectedIndex >= 0;
        }
    }

    private void ClearCommandExecute()
    {
        DoClear();
    }

    private IDataTemplate? GetEffectiveSelectedItemTemplate()
    {
        if (SelectionBoxItemTemplate != null)
        {
            return SelectionBoxItemTemplate;
        }

        if (ItemTemplate != null)
        {
            return ItemTemplate;
        }

        //if (DisplayMemberBinding is { } binding)
        //{
        //    return new FuncDataTemplate<object?>((_, _) => new TextBlock {[!TextBlock.TextProperty] = binding});
        //}

        return null;
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var obj = SelectedValue;
        SelectedValue = null;
        SelectedValue = obj; //ensure changes to selected value get properly propagated
    }

    private void OnItemsSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var obj = SelectedValue;
        SelectedValue = null;
        SelectedValue = obj; //ensure changes to selected value get properly propagated
    }

    private void StartTextSearchTimer()
    {
        _txtSearchTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
        _txtSearchTimer.Tick += TextSearchTimer_Tick;
        _txtSearchTimer.Start();
    }

    private void StopTextSearchTimer()
    {
        if (_txtSearchTimer == null)
        {
            return;
        }

        _txtSearchTimer.Tick -= TextSearchTimer_Tick;
        _txtSearchTimer.Stop();

        _txtSearchTimer = null;
    }

    private void TextSearchTimer_Tick(object? sender, EventArgs e)
    {
        _txtSearchTerm = string.Empty;
        StopTextSearchTimer();
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