// SideBySideListSelect.cs
// Andrew Baylis
// Created: 01/06/2026

#region using

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;

#endregion

namespace AJBAvalonia;

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

    public static readonly StyledProperty<Thickness?> HeaderMarginProperty =
        AvaloniaProperty.Register<SideBySideListSelect, Thickness?>(nameof(HeaderMargin));

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

    public static readonly StyledProperty<IBrush?> ListBackgroundProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(ListBackground));

    public static readonly StyledProperty<IBrush?> ListBoxBorderBrushProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(ListBoxBorderBrush));

    public static readonly StyledProperty<Thickness?> ListBoxBorderThicknessProperty =
        AvaloniaProperty.Register<SideBySideListSelect, Thickness?>(nameof(ListBoxBorderThickness));

    public static readonly StyledProperty<SelectionMode> ListSelectionModeProperty =
        AvaloniaProperty.Register<SideBySideListSelect, SelectionMode>(nameof(ListSelectionMode));

    public static readonly StyledProperty<string?> RightHeaderTextProperty =
        AvaloniaProperty.Register<SideBySideListSelect, string?>(nameof(RightHeaderText));

    public static readonly StyledProperty<IBrush?> RightListBoxForegroundProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(RightListBoxForeground));

    public static readonly StyledProperty<IDataTemplate?> RightListTemplateProperty =
        AvaloniaProperty.Register<SideBySideListSelect, IDataTemplate?>(nameof(RightListTemplate));

    public static readonly DirectProperty<SideBySideListSelect, IEnumerable?> SelectedItemsProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, IEnumerable?>(nameof(SelectedItems), o => o.SelectedItems,
            (o, v) => o.SelectedItems = v, enableDataValidation: true);

    #endregion

    #region Private fields

    private Button? _btnMoveAllLeft;
    private Button? _btnMoveAllRight;
    private Button? _btnMoveLeft;
    private Button? _btnMoveRight;

    private BindingBase? _displayMemberBinding;

    private string? _displayMemberPath;

    private bool _inSelectedItemsChange;

    private bool _isLeftToRight = true;

    private IEnumerable? _itemsSource;

    private IEnumerable? _selectedItems;

    #endregion

    #region Public properties

    /// <summary>
    ///     Gets or sets whether copies are allowed when moving items to selected list.
    /// </summary>
    public bool AllowCopiesInSelected
    {
        get;
        set => SetAndRaise(AllowCopiesInSelectedProperty, ref field, value);
    }

    [AssignBinding]
    [InheritDataTypeFromItems("LeftSource", AncestorType = typeof(SideBySideListSelect))]
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
    public Thickness? HeaderMargin
    {
        get => GetValue(HeaderMarginProperty);
        set => SetValue(HeaderMarginProperty, value);
    }

    public bool IsLeftToRight
    {
        get => _isLeftToRight;
        set => SetAndRaise(IsLeftToRightProperty, ref _isLeftToRight, value);
    }

    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set
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
    public IBrush? ListBackground
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
    public Thickness? ListBoxBorderThickness
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

    #endregion

    #region Protected properties

    protected ListBox? LeftListBox { get; private set; }

    protected ListBox? RightListBox { get; private set; }

    #endregion

    #region Internal properties

    /// <summary>
    ///     Gets the collection of left items.
    /// </summary>
    internal SortedFilterListCollection<object> LeftItems { get; } = [];

    /// <summary>
    ///     Gets the collection of right items.
    /// </summary>
    internal SortedFilterListCollection<object> RightItems { get; } = [];

    #endregion

    #region Events

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

    /// <summary>
    ///     Raised when the selected collection changes.
    /// </summary>
    public event CollectionChangeEventHandler? SelectedCollectionChanged;

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

    #region Public members

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

    public void SetFilterLeftList(Func<object, bool>? filter)
    {
        LeftItems.SetFilter(filter);
    }

    public void SetFilterRightList(Func<object, bool>? filter)
    {
        RightItems.SetFilter(filter);
    }

    #endregion

    #region Protected members

    protected void AddLeftToRightExecute()
    {
        if (LeftListBox?.SelectedItems?.Count > 0)
        {
            var moveList = new List<object>(LeftListBox.SelectedItems.Cast<object>());

            if (!AllowCopiesInSelected || !IsLeftToRight)
            {
                LeftItems.RemoveRange(moveList);
            }

            if (IsLeftToRight || !AllowCopiesInSelected)
            {
                RightItems.AddRange(moveList);
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

            if (!AllowCopiesInSelected || IsLeftToRight)
            {
                RightItems.RemoveRange(moveList);
            }

            if (!IsLeftToRight || !AllowCopiesInSelected)
            {
                LeftItems.AddRange(moveList);
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
                if (HeaderMargin != null)
                {
                    leftHeader.Margin = HeaderMargin.Value;
                }
            }

            if (rightHeader != null)
            {
                rightHeader.Classes.Clear();
                rightHeader.Classes.AddRange(HeaderClasses);
                if (HeaderMargin != null)
                {
                    rightHeader.Margin = HeaderMargin.Value;
                }
            }
        }

        if (LeftListBox != null)
        {
            LeftListBox.ItemsSource = LeftItems;
            LeftListBox.SelectionChanged += LeftListOnSelectionChanged;
            LeftListBox.DoubleTapped += LeftListOnDoubleTapped;
        }

        if (RightListBox != null)
        {
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
    }

    #endregion

    #region Internal members

    internal void AddAllLeftToRightExecute()
    {
        var moveList = new List<object>(LeftItems);

        if (!AllowCopiesInSelected || !IsLeftToRight)
        {
            LeftItems.Clear();
        }

        if (IsLeftToRight || !AllowCopiesInSelected)
        {
            RightItems.AddRange(moveList);
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

        if (IsLeftToRight || !AllowCopiesInSelected)
        {
            RightItems.Clear();
        }

        if (!AllowCopiesInSelected || !IsLeftToRight)
        {
            LeftItems.AddRange(moveList);
        }

        UpdateSelectedItems();

        if (SelectedCollectionChanged != null)
        {
            var e = new CollectionChangeEventArgs(CollectionChangeAction.Remove, moveList);
            SelectedCollectionChanged(this, e);
        }
    }

    #endregion

    #region Private members

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
        if (SelectedItems is not IList list)
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
    public class SortedFilterListCollection<T> : ICollection<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Private fields

        private readonly List<T> _items = new();

        private bool _blockNotifications;
        private Func<object, bool>? _filter;
        private IComparer<T>? _sortComparer;
        private string? _sortKey;

        private PropertyInfo? _sortProp;

        #endregion

        #region Public properties

        public int Count => _items.Count;

        public int FilteredCount => _items.Count(item => item != null && (_filter == null || _filter(item)));

        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;

        public T this[int index]
        {
            get => _items[index];
            set => SetItem(index, value);
        }

        /// <summary>
        ///     Gets or sets the comparer used for sorting.
        /// </summary>
        public IComparer<T>? SortComparer
        {
            get => _sortComparer;
            set
            {
                _sortProp = null;
                _sortKey = null;
                _sortComparer = value;
            }
        }

        /// <summary>
        ///     Gets or sets the key (property name) used to sort items.
        /// </summary>
        public string? SortKey
        {
            get => _sortKey;
            set
            {
                _sortKey = value;
                _sortProp = null; // Reset the property info to force re-evaluation
                _sortComparer = null;
            }
        }

        public object SyncRoot => this;

        #endregion

        #region Private properties

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

        #region Public members

        /// <summary>
        ///     Adds a range of items to the collection.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            BlockNotifications();
            try
            {
                _items.AddRange(items);

                Sort();
            }
            finally
            {
                EnableNotifications();
            }
        }

        /// <summary>
        ///     Blocks change notifications until <see cref="EnableNotifications" /> is called.
        /// </summary>
        public void BlockNotifications()
        {
            _blockNotifications = true;
        }

        /// <summary>
        ///     Re-enables notifications and raises a reset event.
        /// </summary>
        public void EnableNotifications()
        {
            _blockNotifications = false;
            OnCollectionReset();
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            BlockNotifications();
            try
            {
                foreach (var item in items)
                {
                    _items.Remove(item);
                }

                Sort();
            }
            finally
            {
                EnableNotifications();
            }
        }

        public void SetFilter(Func<object, bool>? filter)
        {
            _filter = filter;
            OnCollectionReset();
        }

        /// <summary>
        ///     Sorts the collection using the configured comparer or key.
        /// </summary>
        public void Sort()
        {
            if (_items.Count > 1 && CheckSortProp() && _sortComparer != null)
            {
                BlockNotifications();
                try
                {
                    // Perform sorting using the specified property

                    _items.Sort(Comparer<T>.Create((x, y) => _sortComparer.Compare(x, y)));
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
            if (_items.Count > 1)
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
            if (_items.Count > 1)
            {
                SortComparer = comparer;
                Sort();
            }
        }

        #endregion

        #region Protected members

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_blockNotifications)
            {
                CollectionChanged?.Invoke(this, e);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Called by base class Collection&lt;T&gt; when an item is set in list;
        ///     raises a CollectionChanged event to any listeners.
        /// </summary>
        protected void SetItem(int index, T item)
        {
            var originalItem = this[index];
            _items[index] = item;

            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                originalItem, item, index));
        }

        #endregion

        #region Private members

        private bool CheckSortProp()
        {
            if (_sortComparer == null)
            {
                if (_sortProp == null && !string.IsNullOrEmpty(SortKey))
                {
                    if (_items.Count == 0)
                    {
                        return false; // No items to check against
                    }

                    var obj = _items[0];

                    if (obj != null)
                    {
                        _sortProp = obj.GetType().GetProperty(SortKey, BindingFlags.Public | BindingFlags.Instance);
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
            }

            return _sortComparer != null;
        }

        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCountPropertyChanged()
        {
            OnPropertyChanged(nameof(Count));
        }

        private void OnIndexerPropertyChanged()
        {
            OnPropertyChanged("Item[]");
        }

        #endregion

        #region Implementing ICollection

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        #endregion

        #region Implementing ICollection<T>

        public void Add(T item)
        {
            _items.Add(item);
            Sort();
            OnCountPropertyChanged();
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _items.Count - 1));
        }

        public void Clear()
        {
            _items.Clear();
            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionReset();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var result = _items.Remove(item);
            if (result)
            {
                Sort();
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, -1));
            }

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
            foreach (var item in _items)
            {
                if (item != null && (_filter == null || _filter(item)))
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region Implementing IList

        public int Add(object? value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object? value)
        {
            if (value is T item)
            {
                return _items.Contains(item);
            }

            return false;
        }

        public int IndexOf(object? value)
        {
            if (value is T item)
            {
                return _items.IndexOf(item);
            }

            return -1;
        }

        public void Insert(int index, object? value)
        {
            if (value is T item)
            {
                _items.Insert(index, item);
                Sort();
                OnCountPropertyChanged();
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
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
            var item = _items[index];
            _items.RemoveAt(index);
            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, -1));
        }

        #endregion
    }
}