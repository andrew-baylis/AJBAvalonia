// SideBySideListSelect.cs
//  Andrew Baylis
//  Created: 01/07/2025

#region using

using System.Collections;
using System.Collections.ObjectModel;
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

#endregion

namespace AJBAvalonia;

/// <summary>
///     Control that presents two lists side-by-side with commands to move items between them.
/// </summary>
public class SideBySideListSelect : TemplatedControl
{
    #region Avalonia Properties

    public static readonly DirectProperty<SideBySideListSelect, bool> AllowCopiesInSelectedProperty =
        AvaloniaProperty.RegisterDirect<SideBySideListSelect, bool>(nameof(AllowCopiesInSelected), o => o.AllowCopiesInSelected, (o, v) => o.AllowCopiesInSelected = v);

    public static readonly StyledProperty<FontStyle> HeaderFontStyleProperty = AvaloniaProperty.Register<SideBySideListSelect, FontStyle>(nameof(HeaderFontStyle));

    public static readonly StyledProperty<FontWeight> HeaderFontWeightProperty = AvaloniaProperty.Register<SideBySideListSelect, FontWeight>(nameof(HeaderFontWeight));

    public static readonly StyledProperty<Thickness?> HeaderMarginProperty = AvaloniaProperty.Register<SideBySideListSelect, Thickness?>(nameof(HeaderMargin));

    public static readonly StyledProperty<string?> LeftHeaderTextProperty = AvaloniaProperty.Register<SideBySideListSelect, string?>(nameof(LeftHeaderText));

    public static readonly StyledProperty<IBrush?> LeftListBoxForegroundProperty = AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(LeftListBoxForeground));

    public static readonly StyledProperty<IDataTemplate?> LeftListTemplateProperty = AvaloniaProperty.Register<SideBySideListSelect, IDataTemplate?>(nameof(LeftListTemplate));

    public static readonly StyledProperty<IBrush?> ListBackgroundProperty = AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(ListBackground));

    public static readonly StyledProperty<IBrush?> ListBoxBorderBrushProperty = AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(ListBoxBorderBrush));

    public static readonly StyledProperty<Thickness?> ListBoxBorderThicknessProperty = AvaloniaProperty.Register<SideBySideListSelect, Thickness?>(nameof(ListBoxBorderThickness));

    public static readonly StyledProperty<SelectionMode> ListSelectionModeProperty = AvaloniaProperty.Register<SideBySideListSelect, SelectionMode>(nameof(ListSelectionMode));

    public static readonly StyledProperty<string?> RightHeaderTextProperty = AvaloniaProperty.Register<SideBySideListSelect, string?>(nameof(RightHeaderText));

    public static readonly StyledProperty<IBrush?> RightListBoxForegroundProperty = AvaloniaProperty.Register<SideBySideListSelect, IBrush?>(nameof(RightListBoxForeground));

    public static readonly StyledProperty<IDataTemplate?> RightListTemplateProperty = AvaloniaProperty.Register<SideBySideListSelect, IDataTemplate?>(nameof(RightListTemplate));

    #endregion

    #region Fields

    private Button? _btnMoveAllLeft;
    private Button? _btnMoveAllRight;
    private Button? _btnMoveLeft;
    private Button? _btnMoveRight;

    private string? _displayMemberPath;

    private ListBox? _leftList;

    private ListBox? _rightList;

    #endregion

    /// <summary>
    ///     Initializes a new instance of the <see cref="SideBySideListSelect" /> class.
    /// </summary>
    public SideBySideListSelect()
    {
        LeftItems = new SortedListCollection<object>();
        LeftItems.CollectionChanged += (s, e) => { CheckCanAddLeftRight(); };
        RightItems = new SortedListCollection<object>();
        RightItems.CollectionChanged += (s, e) => { CheckCanAddRightLeft(); };
    }

    #region Properties

    /// <summary>
    ///     Gets or sets whether copies are allowed when moving items to selected list.
    /// </summary>
    public bool AllowCopiesInSelected
    {
        get;
        set => SetAndRaise(AllowCopiesInSelectedProperty, ref field, value);
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
    public Classes HeaderClasses { get; } = new();

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

    /// <summary>
    ///     Gets the collection of left items.
    /// </summary>
    internal SortedListCollection<object> LeftItems { get; }

    /// <summary>
    ///     Gets the collection of right items.
    /// </summary>
    internal SortedListCollection<object> RightItems { get; }

    #endregion

    #region Events

    /// <summary>
    ///     Raised when the selected collection changes.
    /// </summary>
    public event CollectionChangeEventHandler? SelectedCollectionChanged;

    #endregion

    #region Public Methods

    /// <summary>
    ///     Gets the selected items cast to the requested type.
    /// </summary>
    public IEnumerable<T> GetSelectedItems<T>()
    {
        return RightItems.Cast<T>();
    }

    /// <summary>
    ///     Gets the unselected items cast to the requested type.
    /// </summary>
    public IEnumerable<T> GetUnselectedItems<T>()
    {
        return LeftItems.Cast<T>();
    }

    /// <summary>
    ///     Loads both lists from the provided enumerables.
    /// </summary>
    /// <param name="leftItems">Items for the left list.</param>
    /// <param name="rightItems">Items for the right list.</param>
    /// <param name="descriptionProperty">Optional property name used for display binding.</param>
    public void LoadLists(IEnumerable? leftItems, IEnumerable? rightItems, string? descriptionProperty = null)
    {
        LeftItems.Clear();
        RightItems.Clear();
        DisplayMemberPath = descriptionProperty;

        if (leftItems != null)
        {
            LeftItems.AddRange(leftItems.Cast<object>());
        }

        if (rightItems != null)
        {
            RightItems.AddRange(rightItems.Cast<object>());
        }

        CheckCanAddLeftRight();

        CheckCanAddRightLeft();

        if (!string.IsNullOrEmpty(descriptionProperty))
        {
            DisplayMemberPath = descriptionProperty;
        }
    }

    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var leftHeader = e.NameScope.Find<TextBlock>("LeftHeader");
        var rightHeader = e.NameScope.Find<TextBlock>("RightHeader");
        _leftList = e.NameScope.Find<ListBox>("LeftList");
        _rightList = e.NameScope.Find<ListBox>("RightList");
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

        if (_leftList != null)
        {
            _leftList.ItemsSource = LeftItems;
            _leftList.SelectionChanged += LeftListOnSelectionChanged;
            _leftList.DoubleTapped += LeftListOnDoubleTapped;
        }

        if (_rightList != null)
        {
            _rightList.ItemsSource = RightItems;
            _rightList.SelectionChanged += RightListOnSelectionChanged;
            _rightList.DoubleTapped += RightListOnDoubleTapped;
        }

        _btnMoveLeft = e.NameScope.Find<Button>("btnMoveLeft");
        if (_btnMoveLeft != null)
        {
            _btnMoveLeft.Click += (_, _) => AddLeftToRightExecute();
        }

        _btnMoveAllLeft = e.NameScope.Find<Button>("btnMoveAllLeft");
        if (_btnMoveAllLeft != null)
        {
            _btnMoveAllLeft.Click += (_, _) => AddAllLeftToRightExecute();
        }

        _btnMoveAllRight = e.NameScope.Find<Button>("btnMoveAllRight");
        if (_btnMoveAllRight != null)
        {
            _btnMoveAllRight.Click += (_, _) => AddAllRightToLeftExecute();
        }

        _btnMoveRight = e.NameScope.Find<Button>("btnMoveRight");
        if (_btnMoveRight != null)
        {
            _btnMoveRight.Click += (_, _) => AddRightToLeftExecute();
        }

        InternalSetDisplayBinding();
    }

    #endregion

    #region Private Methods

    private void AddAllLeftToRightExecute()
    {
        var moveList = new List<object>(LeftItems);
        if (!AllowCopiesInSelected)
        {
            LeftItems.Clear();
        }

        RightItems.AddRange(moveList);
        if (SelectedCollectionChanged != null)
        {
            var e = new CollectionChangeEventArgs(CollectionChangeAction.Add, moveList);
            SelectedCollectionChanged(this, e);
        }
    }

    private void AddAllRightToLeftExecute()
    {
        var moveList = new List<object>(RightItems);
        RightItems.Clear();
        if (!AllowCopiesInSelected)
        {
            LeftItems.AddRange(moveList);
        }

        if (SelectedCollectionChanged != null)
        {
            var e = new CollectionChangeEventArgs(CollectionChangeAction.Remove, moveList);
            SelectedCollectionChanged(this, e);
        }
    }

    private void AddLeftToRightExecute()
    {
        if (_leftList?.SelectedItems?.Count > 0)
        {
            var moveList = new List<object>(_leftList.SelectedItems.Cast<object>());
            if (!AllowCopiesInSelected)
            {
                foreach (var item in moveList)
                {
                    LeftItems.Remove(item);
                }
            }

            RightItems.AddRange(moveList);
            if (SelectedCollectionChanged != null)
            {
                var e = new CollectionChangeEventArgs(CollectionChangeAction.Add, moveList);
                SelectedCollectionChanged(this, e);
            }
        }
    }

    private void AddRightToLeftExecute()
    {
        if (_rightList?.SelectedItems?.Count > 0)
        {
            var moveList = new List<object>(_rightList.SelectedItems.Cast<object>());
            foreach (var item in moveList)
            {
                RightItems.Remove(item);
            }

            if (!AllowCopiesInSelected)
            {
                LeftItems.AddRange(moveList);
            }

            if (SelectedCollectionChanged != null)
            {
                var e = new CollectionChangeEventArgs(CollectionChangeAction.Remove, moveList);
                SelectedCollectionChanged(this, e);
            }
        }
    }

    private void CheckCanAddLeftRight()
    {
        if (_btnMoveAllRight != null && _btnMoveRight != null)
        {
            _btnMoveRight.IsEnabled = LeftItems.Count > 0 && _leftList?.SelectedItems?.Count > 0;
            _btnMoveAllRight.IsEnabled = LeftItems.Count > 0;
        }
    }

    private void CheckCanAddRightLeft()
    {
        if (_btnMoveAllLeft != null && _btnMoveLeft != null)
        {
            _btnMoveLeft.IsEnabled = RightItems.Count > 0 && _rightList?.SelectedItems?.Count > 0;
            _btnMoveAllLeft.IsEnabled = RightItems.Count > 0;
        }
    }

    private void InternalSetDisplayBinding()
    {
        if (_leftList != null && _rightList != null)
        {
            if (!string.IsNullOrEmpty(_displayMemberPath))
            {
                _leftList.DisplayMemberBinding = new Binding(_displayMemberPath);
                _rightList.DisplayMemberBinding = new Binding(_displayMemberPath);
            }
            else
            {
                _leftList.DisplayMemberBinding = null;
                _rightList.DisplayMemberBinding = null;
                _leftList.ItemTemplate = LeftListTemplate;
                _rightList.ItemTemplate = RightListTemplate;
            }

            //reset list itemssources
            _leftList.ItemsSource = null;
            _rightList.ItemsSource = null;
            _leftList.ItemsSource = LeftItems;
            _rightList.ItemsSource = RightItems;
        }
    }

    private void LeftListOnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (_leftList?.SelectedItems?.Count > 0)
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
        if (_rightList?.SelectedItems?.Count > 0)
        {
            AddRightToLeftExecute();
        }
    }

    private void RightListOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
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

    #endregion

    #region Nested type: SortedListCollection

    /// <summary>
    ///     A collection that supports sorting, bulk operations and notifies of changes.
    /// </summary>
    public class SortedListCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Fields

        private bool _blockNotifications;
        private IComparer<T>? _sortComparer;
        private string? _sortKey;

        private PropertyInfo? _sortProp;

        #endregion

        #region Properties

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

        #endregion

        #region Public Methods

        /// <summary>
        ///     Adds a range of items to the collection.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            BlockNotifications();
            try
            {
                foreach (var item in items)
                {
                    Items.Add(item);
                }

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

        /// <summary>
        ///     Sorts the collection using the configured comparer or key.
        /// </summary>
        public void Sort()
        {
            if (Items.Count > 1 && CheckSortProp() && _sortComparer != null)
            {
                BlockNotifications();
                try
                {
                    // Perform sorting using the specified property
                    if (_sortProp != null)
                    {
                        ArrayList.Adapter((IList) Items).Sort(Comparer<T>.Create((x, y) => _sortComparer.Compare(x, y)));
                    }
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
            if (Items.Count > 1)
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
            if (Items.Count > 1)
            {
                SortComparer = comparer;
                Sort();
            }
        }

        #endregion

        #region Protected Methods

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionReset();
        }

        /// <summary>
        ///     Called by base class Collection&lt;T&gt; when an item is added to list;
        ///     raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
            if (Items.Count == 0)
            {
                base.InsertItem(index, item);
            }
            else
            {
                if (CheckSortProp() && _sortComparer != null && Items is List<T> itemsList)
                {
                    //insert by BinarySearch

                    var idx = itemsList.BinarySearch(item, _sortComparer);
                    if (idx < 0)
                    {
                        idx = ~idx;
                    }

                    Items.Insert(idx, item);
                }
                else
                {
                    base.InsertItem(index, item);
                }
            }

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionReset();
        }

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
        ///     Called by base class Collection&lt;T&gt; when an item is removed from list;
        ///     raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            var removedItem = this[index];

            base.RemoveItem(index);

            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        /// <summary>
        ///     Called by base class Collection&lt;T&gt; when an item is set in list;
        ///     raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            var originalItem = this[index];
            base.SetItem(index, item);

            OnIndexerPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem, item, index));
        }

        #endregion

        #region Private Methods

        private bool CheckSortProp()
        {
            if (_sortComparer == null)
            {
                if (_sortProp == null && !string.IsNullOrEmpty(SortKey))
                {
                    if (Items.Count == 0)
                    {
                        return false; // No items to check against
                    }

                    var obj = Items[0];

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

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }

    #endregion
}