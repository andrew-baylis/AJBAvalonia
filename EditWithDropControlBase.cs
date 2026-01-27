// EditWithDropControlBase.cs
//  Andrew Baylis
//  Created: 10/06/2024

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

/// <summary>
///     Base control providing an editor with a drop-down flyout.
/// </summary>
public abstract class EditWithDropControlBase : TemplatedControl
{
    #region Avalonia Properties

    public static readonly StyledProperty<bool> AddShadowProperty = AvaloniaProperty.Register<EditWithDropControlBase, bool>(nameof(AddShadow));

    public static readonly StyledProperty<bool> AllowClearProperty = AvaloniaProperty.Register<EditWithDropControlBase, bool>(nameof(AllowClear), true);

    public static readonly DirectProperty<EditWithDropControlBase, bool> CloseDropDownOnClickOutsideProperty =
        AvaloniaProperty.RegisterDirect<EditWithDropControlBase, bool>(nameof(CloseDropDownOnClickOutside),
                                                                       o => o.CloseDropDownOnClickOutside,
                                                                       (o, v) => o.CloseDropDownOnClickOutside = v);

    public static readonly StyledProperty<bool> ConstrainPopupWidthProperty = AvaloniaProperty.Register<EditWithDropControlBase, bool>(nameof(ConstrainPopupWidth));

    public static readonly DirectProperty<EditWithDropControlBase, Geometry?> DropGlyphPathProperty =
        AvaloniaProperty.RegisterDirect<EditWithDropControlBase, Geometry?>(nameof(DropGlyphPath), o => o.DropGlyphPath, (o, v) => o.DropGlyphPath = v);

    public static readonly DirectProperty<EditWithDropControlBase, double?> DropGlyphSizeProperty =
        AvaloniaProperty.RegisterDirect<EditWithDropControlBase, double?>(nameof(DropGlyphSize), o => o.DropGlyphSize, (o, v) => o.DropGlyphSize = v);

    public static readonly StyledProperty<string?> EditorValueProperty =
        AvaloniaProperty.Register<EditWithDropControlBase, string?>(nameof(EditorValue), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public static readonly StyledProperty<bool> EditReadOnlyProperty = AvaloniaProperty.Register<EditWithDropControlBase, bool>(nameof(EditReadOnly));

    public static readonly StyledProperty<PlacementMode> FlyoutPlacementProperty =
        AvaloniaProperty.Register<EditWithDropControlBase, PlacementMode>(nameof(FlyoutPlacement), PlacementMode.BottomEdgeAlignedLeft);

    public static readonly DirectProperty<EditWithDropControlBase, ComboGlyphEnum> GlyphTypeProperty =
        AvaloniaProperty.RegisterDirect<EditWithDropControlBase, ComboGlyphEnum>(nameof(GlyphType), o => o.GlyphType, (o, v) => o.GlyphType = v);

    public static readonly StyledProperty<IBrush?> PopUpBackgroundProperty =
        AvaloniaProperty.Register<EditWithDropControlBase, IBrush?>(nameof(PopUpBackground), Brushes.WhiteSmoke);

    public static readonly StyledProperty<IBrush?> PopupBorderBrushProperty = AvaloniaProperty.Register<EditWithDropControlBase, IBrush?>(nameof(PopupBorderBrush));

    public static readonly StyledProperty<Thickness> PopupBorderThicknessProperty = AvaloniaProperty.Register<EditWithDropControlBase, Thickness>(nameof(PopupBorderThickness));

    public static readonly StyledProperty<Thickness> PopupPaddingProperty = AvaloniaProperty.Register<EditWithDropControlBase, Thickness>(nameof(PopupPadding));

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<EditWithDropControlBase, VerticalAlignment>(nameof(VerticalContentAlignment), VerticalAlignment.Center);

    #endregion

    #region Fields

    private const string _comboDrop = "M1939 486 L2029 576 L1024 1581 L19 576 L109 486 L1024 1401 L1939 486 Z";

    private const string _filter = "m 349.34618,456.25731 h 180 v 20 l -70,90 v 100 l -40,-20 v -80 l -70,-90 v -20";

    private const string _glassLeft =
        "m 443.50703,1.4041 c -169.2,13.5 -317,112.6 -392.400005,263.2 -37.8,75.7 -55.6999996,162.9 -50.0999996,244.1 6,85.9 32.2999996,164.6 78.4999996,234.7 105.800005,160.4 297.600005,241.9 487.000005,207 28.7,-5.3 60.3,-14.4 88,-25.2 16.5,-6.4 53.7,-24.5 68.4,-33.3 l 10.4,-6.2 197.1,197.1 197.09997,197.1 76.3,-76.2 76.2,-76.3 -197.1,-197.1 -197.09997,-197.1 6.2,-10.4 c 8.8,-14.7 26.9,-51.9 33.3,-68.4 48.8,-125.1 43.5,-263.9 -14.7,-384 -46.3,-95.6 -121.6,-172 -216.3,-219.4 -77.9,-38.9 -165.9,-56.3 -250.8,-49.6 z m 76,161 c 108.3,14.6 198.7,79.2 246.5,176 15.8,31.9 25.4,63.7 30.7,101.5 2.4,17.6 2.4,61.4 0,79 -11,78.2 -45,143.8 -101.7,196.2 -48.2,44.5 -107.4,71.9 -176,81.5 -17.6,2.4 -61.4,2.4 -79,0 -37.8,-5.3 -69.6,-14.9 -101.5,-30.7 -106.8,-52.7 -174.8,-158.6 -178.2,-277.5 -2.6,-92.1 34,-179.6 101.3,-242.6 52.1,-48.7 117.6,-77.8 191.2,-84.9 13.1,-1.3 52.9,-0.4 66.7,1.5 z";

    private const string _glassRight =
        "m 836.5,1.4041 c 169.2,13.5 317,112.6 392.4,263.2 37.8,75.7 55.7,162.9 50.1,244.1 -6,85.9 -32.3,164.6 -78.5,234.7 -105.8,160.4 -297.6,241.9 -487,207 -28.7,-5.3 -60.3,-14.4 -88,-25.2 -16.5,-6.4 -53.7,-24.5 -68.4,-33.3 l -10.4,-6.2 -197.1,197.1 -197.1,197.1 -76.3,-76.2 -76.2,-76.3 197.1,-197.1 197.1,-197.1 -6.2,-10.4 c -8.8,-14.7 -26.9,-51.9 -33.3,-68.4 -48.8,-125.1 -43.5,-263.9 14.7,-384 46.3,-95.6 121.6,-172 216.3,-219.4 77.9,-38.9 165.9,-56.3 250.8,-49.6 z m -76,161 c -108.3,14.6 -198.7,79.2 -246.5,176 -15.8,31.9 -25.4,63.7 -30.7,101.5 -2.4,17.6 -2.4,61.4 0,79 11,78.2 45,143.8 101.7,196.2 48.2,44.5 107.4,71.9 176,81.5 17.6,2.4 61.4,2.4 79,0 37.8,-5.3 69.6,-14.9 101.5,-30.7 106.8,-52.7 174.8,-158.6 178.2,-277.5 2.6,-92.1 -34,-179.6 -101.3,-242.6 -52.1,-48.7 -117.6,-77.8 -191.2,-84.9 -13.1,-1.3 -52.9,-0.4 -66.7,1.5 z";

    private readonly Popup _dropPopup;
    private readonly Border _popBorder;

    private bool _closeDropDownOnClickOutside = true;

    private Border? _displayBorder;

    private DropDownButton? _dropButton;

    private Geometry? _dropGlyphPath;

    private double? _dropGlyphSize;

    private PathIcon? _dropIcon;

    private TextBoxEx? _editBox;

    private IPointer? _pointer;

    #endregion

    protected EditWithDropControlBase()
    {
        _dropPopup = new Popup();
        _popBorder = new Border();
        _dropPopup.Child = _popBorder;
        _dropPopup.Opened += DropPopupOnOpened;
        _dropPopup.Closed += DropPopupOnClosed;
        _dropPopup.Closed += DropPopupOnClosed;
    }

    #region Properties

    /// <summary>
    ///     Gets or sets whether the popup should add a drop shadow.
    /// </summary>
    public bool AddShadow
    {
        get => GetValue(AddShadowProperty);
        set => SetValue(AddShadowProperty, value);
    }

    /// <summary>
    ///     Gets or sets whether the embedded editor allows clearing its text.
    /// </summary>
    public bool AllowClear
    {
        get => GetValue(AllowClearProperty);
        set => SetValue(AllowClearProperty, value);
    }

    /// <summary>
    ///     Gets or sets whether clicking outside closes the popup when not light-dismiss.
    /// </summary>
    public bool CloseDropDownOnClickOutside
    {
        get => _closeDropDownOnClickOutside;
        set => SetAndRaise(CloseDropDownOnClickOutsideProperty, ref _closeDropDownOnClickOutside, value);
    }

    /// <summary>
    ///     Gets or sets whether the popup width should be constrained to the control width.
    /// </summary>
    public bool ConstrainPopupWidth
    {
        get => GetValue(ConstrainPopupWidthProperty);
        set => SetValue(ConstrainPopupWidthProperty, value);
    }

    /// <summary>
    ///     Gets or sets the geometry used for the drop glyph icon.
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
    ///     Gets or sets the editor value bound to the embedded editor.
    /// </summary>
    public string? EditorValue
    {
        get => GetValue(EditorValueProperty);
        set => SetValue(EditorValueProperty, value);
    }

    /// <summary>
    ///     Gets or sets whether the embedded editor is read-only.
    /// </summary>
    public bool EditReadOnly
    {
        get => GetValue(EditReadOnlyProperty);
        set => SetValue(EditReadOnlyProperty, value);
    }

    /// <summary>
    ///     Gets or sets the placement mode used for the popup.
    /// </summary>
    public PlacementMode FlyoutPlacement
    {
        get => GetValue(FlyoutPlacementProperty);
        set => SetValue(FlyoutPlacementProperty, value);
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
    }

    /// <summary>
    ///     Gets or sets the popup background brush.
    /// </summary>
    public IBrush? PopUpBackground
    {
        get => GetValue(PopUpBackgroundProperty);
        set => SetValue(PopUpBackgroundProperty, value);
    }

    /// <summary>
    ///     Gets or sets the popup border brush.
    /// </summary>
    public IBrush? PopupBorderBrush
    {
        get => GetValue(PopupBorderBrushProperty);
        set => SetValue(PopupBorderBrushProperty, value);
    }

    /// <summary>
    ///     Gets or sets the popup border thickness.
    /// </summary>
    public Thickness PopupBorderThickness
    {
        get => GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }

    /// <summary>
    ///     Gets or sets padding inside the popup border.
    /// </summary>
    public Thickness PopupPadding
    {
        get => GetValue(PopupPaddingProperty);
        set => SetValue(PopupPaddingProperty, value);
    }

    /// <summary>
    ///     Gets or sets the vertical alignment for the control's content.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    ///     Gets or sets the control used as the internal content of the popup.
    /// </summary>
    protected Control? InternalDropControl
    {
        get;
        set
        {
            field = value;
            _popBorder.Child = field;
        }
    }

    #endregion

    #region Events

    /// <summary>
    ///     Raised when the embedded editor is cleared.
    /// </summary>
    public event EventHandler? OnClearEvent;

    /// <summary>
    ///     Raised when the popup is closed.
    /// </summary>
    public event EventHandler? OnPopupClose;

    /// <summary>
    ///     Raised when the popup is opened.
    /// </summary>
    public event EventHandler? OnPopupOpen;

    #endregion

    #region Public Methods

    /// <summary>
    ///     Closes the drop-down popup.
    /// </summary>
    public void CloseDropDown()
    {
        _pointer?.Capture(null);
        _dropPopup.IsOpen = false;
    }

    #endregion

    #region Protected Methods

    /// <summary>
    ///     Applies the control template and initializes named template parts.
    /// </summary>
    /// <param name="e">Template applied arguments.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _editBox = e.NameScope.Find("PART_textEdit") as TextBoxEx;
        _displayBorder = e.NameScope.Find("PART_displayBorder") as Border;
        if (_editBox != null)
        {
            _editBox.AllowClear = AllowClear;
            _editBox.OnClearEvent += EditBoxOnOnClearEvent;
            _editBox.IsReadOnly = EditReadOnly;

            _popBorder.Background = PopUpBackground ?? Brushes.WhiteSmoke;
            _popBorder.BorderBrush = PopupBorderBrush;
            _popBorder.BorderThickness = PopupBorderThickness;
            _popBorder.Padding = PopupPadding;

            _dropPopup.Placement = FlyoutPlacement;
            _dropPopup.WindowManagerAddShadowHint = AddShadow;
            _dropPopup.IsLightDismissEnabled = CloseDropDownOnClickOutside;
        }

        SetFlyoutParent();

        _dropButton = e.NameScope.Find<DropDownButton>("PART_dropButton");
        if (_dropButton != null)
        {
            _dropButton.Click += DropDownButton_OnClick;
            _dropButton.PointerEntered += DropButtonOnPointerEntered;
        }
    }

    /// <summary>
    ///     Handles control loaded event to prepare glyph icon.
    /// </summary>
    /// <param name="e">Routed event args.</param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DropGlyphPath != null && _dropButton != null)
        {
            _dropIcon ??= _dropButton.GetTemplateChildren().OfType<PathIcon>().FirstOrDefault();
            UpdateGlyph();
        }
    }

    /// <summary>
    ///     Responds to property changes to update popup or editor state.
    /// </summary>
    /// <param name="change">Change information.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PopUpBackgroundProperty)
        {
            _popBorder.Background = PopUpBackground;
        }
        else if (change.Property == PopupBorderBrushProperty)
        {
            _popBorder.BorderBrush = PopupBorderBrush;
        }
        else if (change.Property == PopupBorderThicknessProperty)
        {
            _popBorder.BorderThickness = PopupBorderThickness;
        }
        else if (change.Property == PopupPaddingProperty)
        {
            _popBorder.Padding = PopupPadding;
        }
        else if (change.Property == FlyoutPlacementProperty)
        {
            _dropPopup.Placement = FlyoutPlacement;
        }
        else if (change.Property == AddShadowProperty)
        {
            _dropPopup.WindowManagerAddShadowHint = AddShadow;
        }
        else if (change.Property == EditReadOnlyProperty)
        {
            if (_editBox != null)
            {
                _editBox.IsReadOnly = EditReadOnly;
                SetFlyoutParent();
            }
        }
        else if (change.Property == AllowClearProperty)
        {
            if (_editBox != null)
            {
                _editBox.AllowClear = AllowClear;
            }
        }
        else if (change.Property == CloseDropDownOnClickOutsideProperty)
        {
            _dropPopup.IsLightDismissEnabled = CloseDropDownOnClickOutside;
        }
    }

    #endregion

    #region Private Methods

    private void DoOpenFlyout()
    {
        if (_editBox != null)
        {
            if (InternalDropControl != null)
            {
                if (ConstrainPopupWidth)
                {
                    InternalDropControl.Width = Bounds.Width - PopupPadding.Left - PopupPadding.Right - PopupBorderThickness.Left - PopupBorderThickness.Right -
                                                BorderThickness.Left - BorderThickness.Right;
                }
                else
                {
                    InternalDropControl.Width = double.NaN;
                    InternalDropControl.MinWidth = _editBox.Bounds.Width - PopupPadding.Left - PopupPadding.Right - PopupBorderThickness.Left - PopupBorderThickness.Right -
                                                   BorderThickness.Left - BorderThickness.Right;
                }

                _dropPopup.IsOpen = true;
            }
        }
    }

    private void DropButtonOnPointerEntered(object? sender, PointerEventArgs e)
    {
        _pointer = e.Pointer;
    }

    private void DropDownButton_OnClick(object? sender, RoutedEventArgs e)
    {
        DoOpenFlyout();
    }

    private async void DropPopupOnClosed(object? sender, EventArgs e)
    {
        OnPopupClose?.Invoke(this, e);
    }

    private void DropPopupOnOpened(object? sender, EventArgs e)
    {
        OnPopupOpen?.Invoke(this, e);
    }

    private void EditBoxOnOnClearEvent(object? sender, EventArgs e)
    {
        if (AllowClear)
        {
            OnClearEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetFlyoutParent()
    {
        if (EditReadOnly)
        {
            _dropPopup.PlacementTarget = _displayBorder;
            ((ISetLogicalParent) _dropPopup).SetParent(_displayBorder);
        }
        else
        {
            _dropPopup.PlacementTarget = _editBox;
            ((ISetLogicalParent) _dropPopup).SetParent(_editBox);
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