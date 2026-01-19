// MultiSelectDisplay.cs
//  Andrew Baylis
//  Created: 29/11/2025

#region using

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

#endregion

namespace AJBAvalonia;

/// <summary>
/// Specifies how selected items are displayed in the check combo box.
/// </summary>
public enum MultiSelectDisplayTypeEnum
{
    /// <summary>Display individual selected items.</summary>
    SelectedItems,

    /// <summary>Display a summary text (e.g. "X items selected").</summary>
    SummaryText
}

public class MultiSelectDisplay : TemplatedControl
{
    #region Avalonia Properties

    public static readonly StyledProperty<IBrush?> DisplayItemBackgroundProperty = AvaloniaProperty.Register<MultiSelectDisplay, IBrush?>(nameof(DisplayItemBackground));

    public static readonly StyledProperty<IBrush?> DisplayItemBorderBrushProperty = AvaloniaProperty.Register<MultiSelectDisplay, IBrush?>(nameof(DisplayItemBorderBrush));

    public static readonly StyledProperty<Thickness> DisplayItemBorderThicknessProperty =
        AvaloniaProperty.Register<MultiSelectDisplay, Thickness>(nameof(DisplayItemBorderThickness));

    public static readonly StyledProperty<CornerRadius> DisplayItemCornerRadiusProperty =
        AvaloniaProperty.Register<MultiSelectDisplay, CornerRadius>(nameof(DisplayItemCornerRadius));

    public static readonly DirectProperty<MultiSelectDisplay, MultiSelectDisplayTypeEnum> DisplayItemDisplayTypeProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectDisplay, MultiSelectDisplayTypeEnum>(nameof(DisplayItemDisplayType),
                                                                                                o => o.DisplayItemDisplayType,
                                                                                                (o, v) => o.DisplayItemDisplayType = v);

    public static readonly DirectProperty<MultiSelectDisplay, Thickness> DisplayItemMarginProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectDisplay, Thickness>(nameof(DisplayItemMargin), o => o.DisplayItemMargin, (o, v) => o.DisplayItemMargin = v);

    public static readonly StyledProperty<IBinding?> DisplayMemberBindingProperty = AvaloniaProperty.Register<MultiSelectDisplay, IBinding?>(nameof(DisplayMemberBinding));

    public static readonly StyledProperty<string?> DisplayTextFormatStringProperty = AvaloniaProperty.Register<MultiSelectDisplay, string?>(nameof(DisplayTextFormatString));

    public static readonly StyledProperty<string?> DisplayTextProperty = AvaloniaProperty.Register<MultiSelectDisplay, string?>(nameof(DisplayText));

    public static readonly DirectProperty<MultiSelectDisplay, HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectDisplay, HorizontalAlignment>(nameof(HorizontalContentAlignment),
                                                                                 o => o.HorizontalContentAlignment,
                                                                                 (o, v) => o.HorizontalContentAlignment = v);

    public static readonly DirectProperty<MultiSelectDisplay, int> ItemCountProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectDisplay, int>(nameof(ItemCount), o => o.ItemCount, (o, v) => o.ItemCount = v);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = AvaloniaProperty.Register<MultiSelectDisplay, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<MultiSelectDisplay, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly DirectProperty<MultiSelectDisplay, int> MaxItemsBeforeDisplayTextProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectDisplay, int>(nameof(MaxItemsBeforeDisplayText), o => o.MaxItemsBeforeDisplayText, (o, v) => o.MaxItemsBeforeDisplayText = v);

    public static readonly StyledProperty<FontStyle> PlaceholderFontStyleProperty = AvaloniaProperty.Register<MultiSelectDisplay, FontStyle>(nameof(PlaceholderFontStyle));

    public static readonly DirectProperty<MultiSelectDisplay, IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectDisplay, IBrush?>(nameof(PlaceholderForeground), o => o.PlaceholderForeground, (o, v) => o.PlaceholderForeground = v);

    public static readonly StyledProperty<string?> PlaceholderTextProperty = AvaloniaProperty.Register<MultiSelectDisplay, string?>(nameof(PlaceholderText));

    public static readonly DirectProperty<MultiSelectDisplay, bool> ShowDisplayTextProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectDisplay, bool>(nameof(ShowDisplayText), o => o.ShowDisplayText);

    public static readonly DirectProperty<MultiSelectDisplay, VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.RegisterDirect<MultiSelectDisplay, VerticalAlignment>(nameof(VerticalContentAlignment),
                                                                               o => o.VerticalContentAlignment,
                                                                               (o, v) => o.VerticalContentAlignment = v);

    #endregion

    #region Fields

    private const string _displaytextFormat = "{0} items";

    private MultiSelectDisplayTypeEnum _displayItemDisplayType = MultiSelectDisplayTypeEnum.SelectedItems;

    private Thickness _displayItemMargin = new(5, 2);
    private TextBlock? _displaytextBlock;

    private HorizontalAlignment _horizontalContentAlignment = HorizontalAlignment.Center;

    private int _itemCount;
    private ItemsControl? _itemsControl;

    private int _maxItemsBeforeDisplayText;

    private IBrush? _placeholderForeground;

    private bool _showDisplayText;

    private VerticalAlignment _verticalContentAlignment = VerticalAlignment.Center;

    #endregion

    #region Properties

    public IBrush? DisplayItemBackground
    {
        get => GetValue(DisplayItemBackgroundProperty);
        set => SetValue(DisplayItemBackgroundProperty, value);
    }

    public IBrush? DisplayItemBorderBrush
    {
        get => GetValue(DisplayItemBorderBrushProperty);
        set => SetValue(DisplayItemBorderBrushProperty, value);
    }

    public Thickness DisplayItemBorderThickness
    {
        get => GetValue(DisplayItemBorderThicknessProperty);
        set => SetValue(DisplayItemBorderThicknessProperty, value);
    }

    public CornerRadius DisplayItemCornerRadius
    {
        get => GetValue(DisplayItemCornerRadiusProperty);
        set => SetValue(DisplayItemCornerRadiusProperty, value);
    }

    public MultiSelectDisplayTypeEnum DisplayItemDisplayType
    {
        get => _displayItemDisplayType;
        set => SetAndRaise(DisplayItemDisplayTypeProperty, ref _displayItemDisplayType, value);
    }

    public Thickness DisplayItemMargin
    {
        get => _displayItemMargin;
        set => SetAndRaise(DisplayItemMarginProperty, ref _displayItemMargin, value);
    }

    /// <summary>
    ///     Gets or sets the binding used to display item members.
    /// </summary>
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource), AncestorType = typeof(MultiSelectDisplay))]
    public IBinding? DisplayMemberBinding
    {
        get => GetValue(DisplayMemberBindingProperty);
        set => SetValue(DisplayMemberBindingProperty, value);
    }

    public string? DisplayText
    {
        get => GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public string? DisplayTextFormatString
    {
        get => GetValue(DisplayTextFormatStringProperty);
        set => SetValue(DisplayTextFormatStringProperty, value);
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => _horizontalContentAlignment;
        set => SetAndRaise(HorizontalContentAlignmentProperty, ref _horizontalContentAlignment, value);
    }

    public int ItemCount
    {
        get => _itemCount;
        set => SetAndRaise(ItemCountProperty, ref _itemCount, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public int MaxItemsBeforeDisplayText
    {
        get => _maxItemsBeforeDisplayText;
        set => SetAndRaise(MaxItemsBeforeDisplayTextProperty, ref _maxItemsBeforeDisplayText, value);
    }

    public FontStyle PlaceholderFontStyle
    {
        get => GetValue(PlaceholderFontStyleProperty);
        set => SetValue(PlaceholderFontStyleProperty, value);
    }

    public IBrush? PlaceholderForeground
    {
        get => _placeholderForeground;
        set => SetAndRaise(PlaceholderForegroundProperty, ref _placeholderForeground, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public bool ShowDisplayText
    {
        get => _showDisplayText;
        private set => SetAndRaise(ShowDisplayTextProperty, ref _showDisplayText, value);
    }

    public VerticalAlignment VerticalContentAlignment
    {
        get => _verticalContentAlignment;
        set => SetAndRaise(VerticalContentAlignmentProperty, ref _verticalContentAlignment, value);
    }

    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Get<ItemsControl>("PART_ItemsControl");
        _itemsControl.ItemsSource = ItemsSource;
        SetDisplayItemTemplate();
        _displaytextBlock = e.NameScope.Get<TextBlock>("DisplayTextBlock");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        SetItemCount();
        SetDisplayText();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            UpdateItemsSource(change.OldValue, change.NewValue);
            SetItemCount();
        }
        else if (change.Property == ItemTemplateProperty)
        {
            SetDisplayItemTemplate();
            SetDisplayText();
        }
        else if (change.Property == DisplayItemDisplayTypeProperty || change.Property == ItemCountProperty)
        {
            SetDisplayText();
        }
    }

    protected void UpdateItemsSource(object? changeOldValue, object? changeNewValue)
    {
        if (changeOldValue is INotifyCollectionChanged nc)
        {
            nc.CollectionChanged -= ItemsSource_CollectionChanged;
        }
        else if (changeOldValue is INotifyPropertyChanged np)
        {
            np.PropertyChanged -= ItemsSource_PropertyChanged;
        }

        if (changeNewValue is INotifyCollectionChanged ncNew)
        {
            ncNew.CollectionChanged += ItemsSource_CollectionChanged;
        }
        else if (changeNewValue is INotifyPropertyChanged npNew)
        {
            npNew.PropertyChanged += ItemsSource_PropertyChanged;
        }
    }

    #endregion

    #region Private Methods

    private IDataTemplate? GetDisplayItemsTemplate()
    {
        if (ItemTemplate != null)
        {
            return ItemTemplate;
        }

        return new FuncDataTemplate<object?>((_, _) =>
        {
            var border = new Border {BorderBrush = DisplayItemBorderBrush, Background = DisplayItemBackground, CornerRadius = DisplayItemCornerRadius, Margin = DisplayItemMargin};
            var contentPresenter = new ContentControl
            {
                VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(5, 2)
            };
            contentPresenter[!ContentPresenter.ContentProperty] = new Binding();
            contentPresenter.ContentTemplate = GetEffectiveItemTemplate();
            border.Child = contentPresenter;
            return border;
        });
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

    private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetItemCount();
    }

    private void ItemsSource_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SetItemCount();
    }

    private void SetDisplayItemTemplate()
    {
        if (_itemsControl != null)
        {
            _itemsControl.ItemTemplate = GetDisplayItemsTemplate();
        }
    }

    private void SetDisplayText()
    {
        if (DisplayItemDisplayType == MultiSelectDisplayTypeEnum.SelectedItems)
        {
            if (MaxItemsBeforeDisplayText > 0 && ItemCount > MaxItemsBeforeDisplayText)
            {
                ShowDisplayText = true;
                DisplayText = string.Format(CultureInfo.CurrentCulture, DisplayTextFormatString ?? _displaytextFormat, ItemCount);
            }
            else
            {
                ShowDisplayText = false;
            }
        }
        else
        {
            ShowDisplayText = true;
            DisplayText = ItemCount > 0 ? string.Format(CultureInfo.CurrentCulture, DisplayTextFormatString ?? _displaytextFormat, ItemCount) : null;
        }
    }

    private void SetItemCount()
    {
        ItemCount = ItemsSource is ICollection collection ? collection.Count : 0;
    }

    #endregion
}