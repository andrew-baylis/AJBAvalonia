// SideBySideListSelect.cs
// Andrew Baylis
// Created: 14/07/2026

#region using

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using AJBAvalonia.DragDropInternal;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

#endregion

namespace AJBAvalonia;

public class FilterItemsEventArgs : EventArgs
{
    public FilterItemsEventArgs(object? item)
    {
        Item = item;
        IsSelected = true;
    }

    #region Public properties

    public bool IsSelected { get; set; }
    public object? Item { get; }

    #endregion
}

public delegate bool FilterFuncDelegate(object? item);

/// <summary>
///     Control that presents two lists side-by-side with commands to move items between them.
/// </summary>
public class SideBySideListSelect : TemplatedControl
{
    #region Static Public

    public static readonly DirectProperty<SideBySideListSelect, bool> AllowCopiesInSelectedProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, bool>(nameof(AllowCopiesInSelected),
            o => o.AllowCopiesInSelected, (o, v) => o.AllowCopiesInSelected = v);

    public static readonly DirectProperty<SideBySideListSelect, BindingBase?> DisplayMemberBindingProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, BindingBase?>(nameof(DisplayMemberBinding),
            o => o.DisplayMemberBinding, (o, v) => o.DisplayMemberBinding = v);

    public static readonly StyledProperty<FontStyle> HeaderFontStyleProperty =
        AvaloniaProperty.Register<SideBySideListSelect, FontStyle>(nameof(HeaderFontStyle));

    public static readonly StyledProperty<FontWeight> HeaderFontWeightProperty =
        AvaloniaProperty.Register<SideBySideListSelect, FontWeight>(nameof(HeaderFontWeight));

    public static readonly StyledProperty<Thickness> HeaderMarginProperty =
        AvaloniaProperty.Register<SideBySideListSelect, Thickness>(nameof(HeaderMargin));

    public static readonly DirectProperty<SideBySideListSelect, bool> IsLeftToRightProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, bool>(nameof(IsLeftToRight), o => o.IsLeftToRight,
            (o, v) => o.IsLeftToRight = v);

    public static readonly DirectProperty<SideBySideListSelect, IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, IEnumerable?>(nameof(ItemsSource), o => o.ItemsSource,
            (o, v) => o.ItemsSource = v);

    public static readonly StyledProperty<string?> LeftHeaderTextProperty =
        AvaloniaProperty.Register<SideBySideListSelect, string?>(nameof(LeftHeaderText));

    public static readonly StyledProperty<IBrush?> LeftListBoxForegroundProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(LeftListBoxForeground));

    public static readonly StyledProperty<IDataTemplate?> LeftListTemplateProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IDataTemplate?>(nameof(LeftListTemplate));

    public static readonly StyledProperty<IBrush> ListBackgroundProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IBrush>(nameof(ListBackground), Brushes.LightGray);

    public static readonly StyledProperty<IBrush?> ListBoxBorderBrushProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(ListBoxBorderBrush));

    public static readonly StyledProperty<Thickness> ListBoxBorderThicknessProperty =
        AvaloniaProperty.Register<SideBySideListSelect, Thickness>(nameof(ListBoxBorderThickness));

    public static readonly StyledProperty<SelectionMode> ListSelectionModeProperty =
        AvaloniaProperty.Register<SideBySideListSelect, SelectionMode>(nameof(ListSelectionMode));

    public static readonly DirectProperty<SideBySideListSelect, bool> RefreshFilterProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, bool>(nameof(RefreshFilter), o => o.RefreshFilter,
            (o, v) => o.RefreshFilter = v);

    public static readonly StyledProperty<string?> RightHeaderTextProperty =
        AvaloniaProperty.Register<SideBySideListSelect, string?>(nameof(RightHeaderText));

    public static readonly StyledProperty<IBrush?> RightListBoxForegroundProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(RightListBoxForeground));

    public static readonly StyledProperty<IDataTemplate?> RightListTemplateProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IDataTemplate?>(nameof(RightListTemplate));

    public static readonly DirectProperty<SideBySideListSelect, IEnumerable?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, IEnumerable?>(nameof(SelectedItems), o => o.SelectedItems,
            (o, v) => o.SelectedItems = v, enableDataValidation: true);

    public static readonly DirectProperty<SideBySideListSelect, BindingBase?> SortKeyBindingProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, BindingBase?>(nameof(SortKeyBinding),
            o => o.SortKeyBinding, (o, v) => o.SortKeyBinding = v);

    #endregion

    #region Private fields

    private bool _allowCopiesInSelected;

    private Button? _btnMoveAllLeft;
    private Button? _btnMoveAllRight;
    private Button? _btnMoveLeft;
    private Button? _btnMoveRight;

    private BindingBase? _displayMemberBinding;

    private string? _displayMemberPath;

    // add fields
    private bool _inSelectedItemsChange;

    private bool _isLeftToRight = true;

    private IEnumerable? _itemsSource;

    private MinimalDragDrop? _leftDragDrop;

    private bool _refreshFilter;
    private MinimalDragDrop? _rightDragDrop;

    private IEnumerable? _selectedItems;

    private BindingBase? _sortKeyBinding;

    private int idx;

    #endregion

    #region Public properties

    public bool AllowCopiesInSelected
    {
        get => _allowCopiesInSelected;
        set => SetAndRaise(AllowCopiesInSelectedProperty, ref _allowCopiesInSelected, value);
    }

    [AssignBinding]
    [InheritDataTypeFromItems("ItemsSource", AncestorType = typeof(SideBySideListSelect))]
    public BindingBase? DisplayMemberBinding
    {
        get => _displayMemberBinding;
        set
        {
            SetAndRaise(DisplayMemberBindingProperty, ref _displayMemberBinding, value);
            if (_displayMemberBinding is ReflectionBinding b)
            {
                DisplayMemberPath = b.Path;
            }
            else if (_displayMemberBinding is CompiledBinding cb)
            {
                DisplayMemberPath = cb.Path?.ToString();
            }
        }
    }

    /// <summary>
    ///     Gets or sets the member path to use for item display.
    /// </summary>
    public string? DisplayMemberPath
    {
        get => _displayMemberPath;
        set
        {
            _displayMemberPath = value;
            InternalSetDisplayBinding();
        }
    }

    /// <summary>
    ///     Gets classes applied to headers for styling.
    /// </summary>
    public Classes HeaderClasses { get; } = [];

    /// <summary>
    ///     Gets or sets the header font style.
    /// </summary>
    public FontStyle HeaderFontStyle
    {
        get => GetValue(HeaderFontStyleProperty);
        set => SetValue(HeaderFontStyleProperty, value);
    }

    /// <summary>
    ///     Gets or sets the header font weight.
    /// </summary>
    public FontWeight HeaderFontWeight
    {
        get => GetValue(HeaderFontWeightProperty);
        set => SetValue(HeaderFontWeightProperty, value);
    }

    /// <summary>
    ///     Gets or sets the header margin.
    /// </summary>
    public Thickness HeaderMargin
    {
        get => GetValue(HeaderMarginProperty);
        set => SetValue(HeaderMarginProperty, value);
    }

    public bool IsLeftToRight
    {
        get => _isLeftToRight;
        set
        {
            if (SetAndRaise(IsLeftToRightProperty, ref _isLeftToRight, value))
            {
                if (IsLeftToRight)
                {
                    LeftItems.SetFilter(InternalFilterItems);
                }
                else
                {
                    RightItems.SetFilter(InternalFilterItems);
                }
            }
        }
    }

    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set
        {
            if (!ReferenceEquals(_itemsSource, value))
            {
                if (_itemsSource is INotifyCollectionChanged col)
                {
                    col.CollectionChanged -= ItemsSourceCollectionChanged;
                }

                SetAndRaise(ItemsSourceProperty, ref _itemsSource, value);
                ReloadLists();
                if (_itemsSource is INotifyCollectionChanged col1)
                {
                    col1.CollectionChanged += ItemsSourceCollectionChanged;
                }
            }
        }
    }

    /// <summary>
    ///     Gets or sets the left list header text.
    /// </summary>
    public string? LeftHeaderText
    {
        get => GetValue(LeftHeaderTextProperty);
        set => SetValue(LeftHeaderTextProperty, value);
    }

    /// <summary>
    ///     Gets or sets the foreground brush for the left list box.
    /// </summary>
    public IBrush? LeftListBoxForeground
    {
        get => GetValue(LeftListBoxForegroundProperty);
        set => SetValue(LeftListBoxForegroundProperty, value);
    }

    /// <summary>
    ///     Gets or sets a template for the left list items.
    /// </summary>
    public IDataTemplate? LeftListTemplate
    {
        get => GetValue(LeftListTemplateProperty);
        set => SetValue(LeftListTemplateProperty, value);
    }

    /// <summary>
    ///     Gets or sets the background brush for the lists.
    /// </summary>
    public IBrush ListBackground
    {
        get => GetValue(ListBackgroundProperty);
        set => SetValue(ListBackgroundProperty, value);
    }

    /// <summary>
    ///     Gets or sets the border brush for the list boxes.
    /// </summary>
    public IBrush? ListBoxBorderBrush
    {
        get => GetValue(ListBoxBorderBrushProperty);
        set => SetValue(ListBoxBorderBrushProperty, value);
    }

    /// <summary>
    ///     Gets or sets the border thickness for the list boxes.
    /// </summary>
    public Thickness ListBoxBorderThickness
    {
        get => GetValue(ListBoxBorderThicknessProperty);
        set => SetValue(ListBoxBorderThicknessProperty, value);
    }

    /// <summary>
    ///     Gets or sets the selection mode used for the lists.
    /// </summary>
    public SelectionMode ListSelectionMode
    {
        get => GetValue(ListSelectionModeProperty);
        set => SetValue(ListSelectionModeProperty, value);
    }

    /// <summary>
    ///     Gets or sets the comparer used to sort both lists.
    /// </summary>
    public IComparer<object>? ListSortComparer
    {
        get => LeftItems.SortComparer;
        set
        {
            LeftItems.SortComparer = value;
            RightItems.SortComparer = value;
            SortLists();
        }
    }

    /// <summary>
    ///     Gets or sets the key used to sort both lists.
    /// </summary>
    public string? ListSortKey
    {
        get => LeftItems.SortKey;
        set
        {
            LeftItems.SortKey = value;
            RightItems.SortKey = value;
            SortLists();
        }
    }

    public bool RefreshFilter
    {
        get => _refreshFilter;
        set
        {
            SetAndRaise(RefreshFilterProperty, ref _refreshFilter, value);
            _refreshFilter = false;
            LeftItems.RefreshFilter();
            RightItems.RefreshFilter();
        }
    }

    /// <summary>
    ///     Gets or sets the right list header text.
    /// </summary>
    public string? RightHeaderText
    {
        get => GetValue(RightHeaderTextProperty);
        set => SetValue(RightHeaderTextProperty, value);
    }

    /// <summary>
    ///     Gets or sets the foreground brush for the right list box.
    /// </summary>
    public IBrush? RightListBoxForeground
    {
        get => GetValue(RightListBoxForegroundProperty);
        set => SetValue(RightListBoxForegroundProperty, value);
    }

    /// <summary>
    ///     Gets or sets a template for the right list items.
    /// </summary>
    public IDataTemplate? RightListTemplate
    {
        get => GetValue(RightListTemplateProperty);
        set => SetValue(RightListTemplateProperty, value);
    }

    public IEnumerable? SelectedItems
    {
        get => _selectedItems;
        set
        {
            if (!ReferenceEquals(_selectedItems, value))
            {
                if (_selectedItems is INotifyCollectionChanged col)
                {
                    col.CollectionChanged -= SelectedItemsCollectionChanged;
                }

                SetAndRaise(SelectedItemsProperty, ref _selectedItems, value);

                ReloadLists();
                if (_selectedItems is INotifyCollectionChanged col1)
                {
                    col1.CollectionChanged += SelectedItemsCollectionChanged;
                }
            }
        }
    }

    [AssignBinding]
    [InheritDataTypeFromItems("ItemsSource", AncestorType = typeof(SideBySideListSelect))]
    public BindingBase? SortKeyBinding
    {
        get => _sortKeyBinding;
        set
        {
            if (SetAndRaise(SortKeyBindingProperty, ref _sortKeyBinding, value))
            {
                if (_sortKeyBinding is ReflectionBinding b)
                {
                    ListSortKey = b.Path;
                }
                else if (_sortKeyBinding is CompiledBinding cb)
                {
                    ListSortKey = cb.Path?.ToString();
                }
                else
                {
                    ListSortKey = null;
                }
            }
        }
    }

    #endregion

    #region Protected properties

    protected ListBox? LeftListBox { get; private set; }

    protected ListBox? RightListBox { get; private set; }

    #endregion

    #region Internal properties

    /// <summary>
    ///     Gets the collection of left items.
    /// </summary>
    internal FilteredObservableCollection<object> LeftItems { get; } = [];

    /// <summary>
    ///     Gets the collection of right items.
    /// </summary>
    internal FilteredObservableCollection<object> RightItems { get; } = [];

    #endregion

    #region Events

    public event EventHandler<FilterItemsEventArgs>? FilterItemsEvent;

    /// <summary>
    ///     Raised when the selected collection changes.
    /// </summary>
    public event CollectionChangeEventHandler? SelectedCollectionChanged;

    private void ItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReloadItemsSourceList();
    }

    private void LeftListOnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (LeftListBox?.SelectedItems?.Count > 0)
        {
            AddLeftToRightExecute();
        }
    }

    private void LeftListOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        CheckCanAddLeftRight();
    }

    private void ListBoxOnOnDragOver(object? sender, DragDropArgs e)
    {
        if (sender is ListBox && !ReferenceEquals(e.SourceItem, e.TargetItem))
        {
            e.DragEffects = AllowCopiesInSelected ? DragDropEffects.Copy : DragDropEffects.Move;
            e.Handled = true;
            return;
        }

        e.DragEffects = DragDropEffects.None;
        e.Handled = true;
    }

    private void ListBoxOnOnDrop(object? sender, DragDropArgs e)
    {
        if (ReferenceEquals(e.SourceItem, e.TargetItem))
        {
            return;
        }

        if (ReferenceEquals(e.TargetItem, LeftListBox))
        {
            AddRightToLeftExecute();
        }
        else if (ReferenceEquals(e.TargetItem, RightListBox))
        {
            AddLeftToRightExecute();
        }

        e.Handled = true;
    }

    private void RightListOnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (RightListBox?.SelectedItems?.Count > 0)
        {
            AddRightToLeftExecute();
        }
    }

    private void RightListOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        CheckCanAddRightLeft();
    }

    private void SelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_inSelectedItemsChange)
        {
            _inSelectedItemsChange = true;
            try
            {
                // Handle the collection change event
                ReloadSelectedItemsList();
            }
            finally
            {
                _inSelectedItemsChange = false;
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///     Gets the selected items cast to the requested type.
    /// </summary>
    public IEnumerable<T> GetSelectedItems<T>()
    {
        return IsLeftToRight ? RightItems.Cast<T>() : LeftItems.Cast<T>();
    }

    /// <summary>
    ///     Gets the unselected items cast to the requested type.
    /// </summary>
    public IEnumerable<T> GetUnselectedItems<T>()
    {
        return IsLeftToRight ? LeftItems.Cast<T>() : RightItems.Cast<T>();
    }

    public void SetLeftFilter(FilterFuncDelegate? filter)
    {
        if (filter == null && IsLeftToRight)
        {
            filter = InternalFilterItems;
        }

        LeftItems.SetFilter(filter);
    }

    public void SetRightFilter(FilterFuncDelegate? filter)
    {
        if (filter == null && !IsLeftToRight)
        {
            filter = InternalFilterItems;
        }

        LeftItems.SetFilter(filter);
    }

    #endregion

    #region Protected Methods

    protected void AddLeftToRightExecute()
    {
        if (LeftListBox?.SelectedItems?.Count > 0)
        {
            var moveList = new List<object>(LeftListBox.SelectedItems.Cast<object>());

            if (IsLeftToRight)
            {
                //left is source list, right is selected list
                RightItems.AddRange(moveList);
                if (!AllowCopiesInSelected)
                {
                    LeftItems.RemoveRange(moveList);
                }
            }
            else
            {
                //left is selected list, right is source list
                LeftItems.RemoveRange(moveList);
                if (!AllowCopiesInSelected)
                {
                    RightItems.AddRange(moveList);
                }
            }

            UpdateSelectedItems();

            if (SelectedCollectionChanged != null)
            {
                var e = new CollectionChangeEventArgs(CollectionChangeAction.Add, moveList);
                SelectedCollectionChanged(this, e);
            }
        }
    }

    protected void AddRightToLeftExecute()
    {
        if (RightListBox?.SelectedItems?.Count > 0)
        {
            var moveList = new List<object>(RightListBox.SelectedItems.Cast<object>());

            if (IsLeftToRight)
            {
                //left is source list, right is selected list
                RightItems.RemoveRange(moveList);
                if (!AllowCopiesInSelected)
                {
                    LeftItems.AddRange(moveList);
                }
            }
            else
            {
                //left is selected list, right is source list
                LeftItems.AddRange(moveList);
                if (!AllowCopiesInSelected)
                {
                    RightItems.RemoveRange(moveList);
                }
            }

            UpdateSelectedItems();

            if (SelectedCollectionChanged != null)
            {
                var e = new CollectionChangeEventArgs(CollectionChangeAction.Remove, moveList);
                SelectedCollectionChanged(this, e);
            }
        }
    }

    protected bool CanMoveAllLeftRight()
    {
        return LeftItems.Count > 0;
    }

    protected bool CanMoveAllRightLeft()
    {
        return RightItems.Count > 0;
    }

    protected bool CanMoveLeftRight()
    {
        return LeftItems.Count > 0 && LeftListBox?.SelectedItems?.Count > 0;
    }

    protected bool CanMoveRightLeft()
    {
        return RightItems.Count > 0 && RightListBox?.SelectedItems?.Count > 0;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var leftHeader = e.NameScope.Find<TextBlock>("LeftHeader");
        var rightHeader = e.NameScope.Find<TextBlock>("RightHeader");
        LeftListBox = e.NameScope.Find<ListBox>("LeftList");
        RightListBox = e.NameScope.Find<ListBox>("RightList");

        if (HeaderClasses.Count != 0)
        {
            if (leftHeader != null)
            {
                leftHeader.Classes.Clear();
                leftHeader.Classes.AddRange(HeaderClasses);
            }

            if (rightHeader != null)
            {
                rightHeader.Classes.Clear();
                rightHeader.Classes.AddRange(HeaderClasses);
            }
        }

        if (LeftListBox != null)
        {
            _leftDragDrop = AttachDragDrop(LeftListBox);
            LeftListBox.ItemsSource = LeftItems;
            LeftListBox.SelectionChanged += LeftListOnSelectionChanged;
            LeftListBox.DoubleTapped += LeftListOnDoubleTapped;
        }

        if (RightListBox != null)
        {
            _rightDragDrop = AttachDragDrop(RightListBox);
            RightListBox.ItemsSource = RightItems;
            RightListBox.SelectionChanged += RightListOnSelectionChanged;
            RightListBox.DoubleTapped += RightListOnDoubleTapped;
        }

        _btnMoveLeft = e.NameScope.Find<Button>("btnMoveLeft");
        _btnMoveLeft?.Click += (_, _) => AddRightToLeftExecute();

        _btnMoveAllLeft = e.NameScope.Find<Button>("btnMoveAllLeft");
        _btnMoveAllLeft?.Click += (_, _) => AddAllRightToLeftExecute();

        _btnMoveAllRight = e.NameScope.Find<Button>("btnMoveAllRight");
        _btnMoveAllRight?.Click += (_, _) => AddAllLeftToRightExecute();

        _btnMoveRight = e.NameScope.Find<Button>("btnMoveRight");
        _btnMoveRight?.Click += (_, _) => AddLeftToRightExecute();

        InternalSetDisplayBinding();

        if (IsLeftToRight)
        {
            LeftItems.SetFilter(InternalFilterItems);
        }
        else
        {
            RightItems.SetFilter(InternalFilterItems);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        CheckCanAddLeftRight();
        CheckCanAddRightLeft();
    }

    #endregion

    #region Internal Methods

    internal void AddAllLeftToRightExecute()
    {
        var moveList = new List<object>(LeftItems);

        if (IsLeftToRight)
        {
            //right is selected list, left is source list
            RightItems.AddRange(moveList);
            if (!AllowCopiesInSelected)
            {
                //remove all items from left list if not allowing duplicates in selected
                LeftItems.Clear();
            }
        }
        else
        {
            //right is source list, left is selected list
            LeftItems.Clear();
            if (!AllowCopiesInSelected)
            {
                //if no duplicates, right list will have fewer items
                RightItems.AddRange(moveList);
            }
        }

        UpdateSelectedItems();
        if (SelectedCollectionChanged != null)
        {
            var e = new CollectionChangeEventArgs(CollectionChangeAction.Add, moveList);
            SelectedCollectionChanged(this, e);
        }
    }

    internal void AddAllRightToLeftExecute()
    {
        var moveList = new List<object>(RightItems);

        if (IsLeftToRight)
        {
            //right is selected list, left is source list
            RightItems.Clear();
            if (!AllowCopiesInSelected)
            {
                LeftItems.AddRange(moveList);
            }
        }
        else
        {
            //right is source list, left is selected list
            LeftItems.AddRange(moveList);
            if (!AllowCopiesInSelected)
            {
                RightItems.Clear();
            }
        }

        UpdateSelectedItems();

        if (SelectedCollectionChanged != null)
        {
            var e = new CollectionChangeEventArgs(CollectionChangeAction.Remove, moveList);
            SelectedCollectionChanged(this, e);
        }
    }

    #endregion

    #region Private Methods

    private MinimalDragDrop AttachDragDrop(ListBox box)
    {
        var result = new MinimalDragDrop(box);
        result.IsDragSource = true;
        result.IsDropTarget = true;
        result.OnDragOver += ListBoxOnOnDragOver;
        result.OnDrop += ListBoxOnOnDrop;
        return result;
    }

    private void CheckCanAddLeftRight()
    {
        if (_btnMoveAllRight != null && _btnMoveRight != null)
        {
            _btnMoveRight.IsEnabled = CanMoveLeftRight();
            _btnMoveAllRight.IsEnabled = CanMoveAllLeftRight();
        }
    }

    private void CheckCanAddRightLeft()
    {
        if (_btnMoveAllLeft != null && _btnMoveLeft != null)
        {
            _btnMoveLeft.IsEnabled = CanMoveRightLeft();
            _btnMoveAllLeft.IsEnabled = CanMoveAllRightLeft();
        }
    }

    private static IDataTransfer CreateDragData()
    {
        var item = new DataTransferItem();
        item.SetText("SideBySideListSelectItem");

        var data = new DataTransfer();
        data.Add(item);
        return data;
    }

    private static DragDropEffects GetDragEffects(PointerPressedEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return DragDropEffects.Copy;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return DragDropEffects.Link;
        }

        return DragDropEffects.Move;
    }

    private bool InternalFilterItems(object? item)
    {
        if (FilterItemsEvent != null)
        {
            var e = new FilterItemsEventArgs(item);
            FilterItemsEvent.Invoke(this, e);
            return e.IsSelected;
        }

        return true;
    }

    private void InternalSetDisplayBinding()
    {
        if (LeftListBox != null && RightListBox != null)
        {
            if (!string.IsNullOrEmpty(_displayMemberPath))
            {
                LeftListBox.DisplayMemberBinding = new Binding(_displayMemberPath);
                RightListBox.DisplayMemberBinding = new Binding(_displayMemberPath);
            }
            else
            {
                LeftListBox.DisplayMemberBinding = null;
                RightListBox.DisplayMemberBinding = null;
                LeftListBox.ItemTemplate = LeftListTemplate;
                RightListBox.ItemTemplate = RightListTemplate;
            }

            //reset list itemssources
            LeftListBox.ItemsSource = null;
            RightListBox.ItemsSource = null;
            LeftListBox.ItemsSource = LeftItems;
            RightListBox.ItemsSource = RightItems;
        }
    }

    private void ReloadItemsSourceList()
    {
        if (IsLeftToRight)
        {
            LeftItems.Clear();
            if (ItemsSource != null)
            {
                LeftItems.AddRange(ItemsSource.Cast<object>());
            }
        }
        else
        {
            RightItems.Clear();
            if (ItemsSource != null)
            {
                RightItems.AddRange(ItemsSource.Cast<object>());
            }
        }

        CheckCanAddLeftRight();
        CheckCanAddRightLeft();
    }

    private void ReloadLists()
    {
        LeftItems.Clear();
        RightItems.Clear();

        if (IsLeftToRight)
        {
            if (ItemsSource != null)
            {
                LeftItems.AddRange(ItemsSource.Cast<object>());
            }

            if (SelectedItems != null)
            {
                RightItems.AddRange(SelectedItems.Cast<object>());
            }
        }
        else
        {
            if (ItemsSource != null)
            {
                RightItems.AddRange(ItemsSource.Cast<object>());
            }

            if (SelectedItems != null)
            {
                LeftItems.AddRange(SelectedItems.Cast<object>());
            }
        }

        CheckCanAddLeftRight();
        CheckCanAddRightLeft();
    }

    private void ReloadSelectedItemsList()
    {
        if (IsLeftToRight)
        {
            RightItems.Clear();
            if (SelectedItems != null)
            {
                RightItems.AddRange(SelectedItems.Cast<object>());
            }
        }
        else
        {
            LeftItems.Clear();
            if (SelectedItems != null)
            {
                LeftItems.AddRange(SelectedItems.Cast<object>());
            }
        }

        CheckCanAddLeftRight();
        CheckCanAddRightLeft();
    }

    private void SortLeftList()
    {
        LeftItems.Sort();
    }

    private void SortLists()
    {
        SortLeftList();
        SortRightList();
    }

    private void SortRightList()
    {
        RightItems.Sort();
    }

    private void UpdateSelectedItems()
    {
        if (_selectedItems is not IList list)
        {
            return;
        }

        _inSelectedItemsChange = true;
        try
        {
            var selectedList = new List<object>(IsLeftToRight ? RightItems : LeftItems);
            var removeList = new List<object>();
            foreach (var item in list)
            {
                if (!selectedList.Remove(item))
                {
                    removeList.Add(item);
                }
            }

            foreach (var r in removeList)
            {
                list.Remove(r);
            }

            foreach (var a in selectedList)
            {
                list.Add(a);
            }
        }
        finally
        {
            _inSelectedItemsChange = false;
        }
    }

    #endregion

    /// <summary>
    ///     A collection that supports sorting, bulk operations and notifies of changes.
    /// </summary>
    internal class FilteredObservableCollection<T> : IList, IList<T>, IEnumerable<T>, IEnumerable,
        IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Private fields

        private readonly List<T> _filteredList;

        private readonly List<T> _sourceList;

        private int _blockCount;

        private FilterFuncDelegate? _filter;

        private IComparer<T>? _sortComparer;
        private string? _sortKey;
        private PropertyInfo? _sortProp;

        #endregion

        public FilteredObservableCollection()
        {
            _sourceList = [];
            _filteredList = [];
        }

        public FilteredObservableCollection(IEnumerable<T> collection)
        {
            _sourceList = new List<T>(collection);
            _filteredList = new List<T>(_sourceList);
        }

        public FilteredObservableCollection(int capacity)
        {
            if (capacity < 4)
            {
                capacity = 4;
            }

            _sourceList = new List<T>(capacity);
            _filteredList = new List<T>(capacity);
        }

        #region Public properties

        public int Count => Filter == null ? _sourceList.Count : _filteredList.Count;

        public FilterFuncDelegate? Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                RebuildFilteredList();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Count));
            }
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public T this[int index]
        {
            get => _filter == null ? _sourceList[index] : _filteredList[index];
            set
            {
                if (_filter == null)
                {
                    _sourceList[index] = value;
                }
                else
                {
                    var oldItem = _filteredList[index];
                    _filteredList[index] = value;
                    var idx = _sourceList.IndexOf(oldItem);
                    if (idx >= 0)
                    {
                        _sourceList[idx] = value;
                    }
                }
            }
        }

        public IComparer<T>? SortComparer
        {
            get => _sortComparer;
            set
            {
                _sortProp = null;
                _sortKey = null;
                _sortComparer = value;
                Sort();
                OnPropertyChanged();
            }
        }

        public string? SortKey
        {
            get => _sortKey;
            set
            {
                _sortKey = value;
                _sortProp = null; // Reset the property info to force re-evaluation
                _sortComparer = null;
                Sort();
                OnPropertyChanged();
            }
        }

        public object SyncRoot { get; } = new();

        #endregion

        #region Private properties

        int ICollection.Count => Filter == null ? _sourceList.Count : _filteredList.Count;

        int IReadOnlyCollection<T>.Count => Filter == null ? _sourceList.Count : _filteredList.Count;

        bool ICollection<T>.IsReadOnly => false;

        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is T item)
                {
                    this[index] = item;
                }
            }
        }

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Public Methods

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                InternalAddItem(item);
            }

            NotifyListChanges();
        }

        public void BlockNotifications()
        {
            _blockCount++;
        }

        public void EnableNotifications()
        {
            _blockCount--;
            if (_blockCount <= 0)
            {
                _blockCount = 0;
                NotifyListChanges();
            }
        }

        public void RefreshFilter()
        {
            RebuildFilteredList();
            NotifyListChanges();
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _sourceList.Remove(item);
            }

            RebuildFilteredList();
            NotifyListChanges();
        }

        public void RemoveRange(int index, int count)
        {
            _sourceList.RemoveRange(index, count);
            RebuildFilteredList();
            NotifyListChanges();
        }

        public void ReplaceWithRange(IEnumerable<T> items)
        {
            BlockNotifications();
            try
            {
                _sourceList.Clear();
                _sourceList.AddRange(items);
                RebuildFilteredList();
                Sort();
            }
            finally
            {
                EnableNotifications();
            }
        }

        public void SetFilter(FilterFuncDelegate? filter)
        {
            _filter = filter;
            RebuildFilteredList();
            NotifyListChanges();
        }

        /// <summary>
        ///     Sorts the collection using the configured comparer or key.
        /// </summary>
        public void Sort()
        {
            if (_sourceList.Count > 1 && CheckSortProp() && _sortComparer != null)
            {
                BlockNotifications();
                try
                {
                    // Perform sorting using the specified property

                    _sourceList.Sort(Comparer<T>.Create((x, y) => _sortComparer.Compare(x, y)));
                    _filteredList.Sort(Comparer<T>.Create((x, y) => _sortComparer.Compare(x, y)));
                }
                finally
                {
                    EnableNotifications();
                }
            }
        }

        /// <summary>
        ///     Sorts by a property name.
        /// </summary>
        public void Sort(string propertyName)
        {
            if (_sourceList.Count > 1)
            {
                SortKey = propertyName;
                Sort();
            }
        }

        /// <summary>
        ///     Sorts using a provided comparer.
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            if (_sourceList.Count > 1)
            {
                SortComparer = comparer;
                Sort();
            }
        }

        #endregion

        #region Private Methods

        private bool CheckFilter(T item)
        {
            if (_filter != null)
            {
                return _filter(item);
            }

            return true;
        }

        private bool CheckSortProp()
        {
            if (_sortComparer == null)
            {
                if (_sortProp == null && !string.IsNullOrEmpty(SortKey))
                {
                    _sortProp = typeof(T).GetProperty(SortKey, BindingFlags.Public | BindingFlags.Instance);
                    if (_sortProp != null)
                    {
                        _sortComparer = Comparer<T>.Create((x, y) =>
                        {
                            var xValue = _sortProp?.GetValue(x);
                            var yValue = _sortProp?.GetValue(y);
                            return Comparer<object>.Default.Compare(xValue, yValue);
                        });
                    }
                }
            }

            return _sortComparer != null;
        }

        private void InternalAddItem(T item)
        {
            if (_sortComparer != null)
            {
                var idx = _sourceList.BinarySearch(item, _sortComparer);
                if (idx < 0)
                {
                    idx = ~idx;
                }

                _sourceList.Insert(idx, item);
                if (CheckFilter(item))
                {
                    idx = _filteredList.BinarySearch(item, _sortComparer);
                    if (idx < 0)
                    {
                        idx = ~idx;
                    }

                    _filteredList.Insert(idx, item);
                }
            }
            else
            {
                _sourceList.Add(item);
                if (CheckFilter(item))
                {
                    _filteredList.Add(item);
                }
            }
        }

        private bool InternalRemove(T item)
        {
            var result = _sourceList.Remove(item);
            if (CheckFilter(item) && result)
            {
                _filteredList.Remove(item);
            }

            return result;
        }

        private void NotifyListChanges()
        {
            OnCountChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_blockCount <= 0)
            {
                CollectionChanged?.Invoke(this, e);
            }
        }

        private void OnCountChanged()
        {
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Items[]");
        }

        private void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            if (_blockCount <= 0)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        private void RebuildFilteredList()
        {
            _filteredList.Clear();
            foreach (var item in _sourceList)
            {
                if (CheckFilter(item))
                {
                    _filteredList.Add(item);
                }
            }
        }

        #endregion

        #region Implementing ICollection

        public void CopyTo(Array array, int index)
        {
            if (array is T[] arr)
            {
                if (Filter == null)
                {
                    _sourceList.CopyTo(arr, index);
                }
                else
                {
                    _filteredList.CopyTo(arr, index);
                }
            }
        }

        #endregion

        #region Implementing ICollection<T>

        public void Add(T item)
        {
            InternalAddItem(item);
            OnCountChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public bool Contains(T item)
        {
            return _sourceList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (Filter == null)
            {
                _sourceList.CopyTo(array, arrayIndex);
            }
            else
            {
                _filteredList.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            var result = InternalRemove(item);
            OnCountChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            return result;
        }

        #endregion

        #region Implementing IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementing IEnumerable<T>

        public IEnumerator<T> GetEnumerator()
        {
            return _filter == null ? _sourceList.GetEnumerator() : _filteredList.GetEnumerator();
        }

        #endregion

        #region Implementing IList

        public int Add(object? value)
        {
            if (value is T item)
            {
                Add(item);
            }

            return 0;
        }

        public void Clear()
        {
            _sourceList.Clear();
            _filteredList.Clear();
            NotifyListChanges();
        }

        public bool Contains(object? value)
        {
            if (value is T item)
            {
                return Contains(item);
            }

            return false;
        }

        public int IndexOf(object? value)
        {
            if (value is T item)
            {
                return IndexOf(item);
            }

            return -1;
        }

        public void Insert(int index, object? value)
        {
            if (value is T item)
            {
                Add(item);
            }
        }

        public void Remove(object? value)
        {
            if (value is T item)
            {
                Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region Implementing IList<T>

        public int IndexOf(T item)
        {
            if (_sortComparer != null)
            {
                return _filteredList.BinarySearch(item, _sortComparer);
            }

            return _filteredList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Add(item);
        }

        #endregion
    }
}