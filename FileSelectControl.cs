// FileSelectControl.cs
// Andrew Baylis
// Created: 20/05/2026

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

#endregion

namespace AJBAvalonia;

public enum FileDialogTypeEnum
{
    FileOpen,
    FileOpenImage,
    FileSave
}

public enum FileDisplayStyleEnum
{
    FullPath,
    ShortenPath,
    FileName
}

public class FileSelectControl : TemplatedControl
{
    #region Static Public

    public static readonly DirectProperty<FileSelectControl, bool> AllowClearProperty =
        AvaloniaProperty.RegisterDirect<FileSelectControl, bool>(nameof(AllowClear), o => o.AllowClear,
            (o, v) => o.AllowClear = v);

    public static readonly StyledProperty<string?> DefaultExtensionProperty =
        AvaloniaProperty.Register<FileSelectControl, string?>(nameof(DefaultExtension));

    public static readonly StyledProperty<string?> DialogFilterProperty =
        AvaloniaProperty.Register<FileSelectControl, string?>(nameof(DialogFilter));

    public static readonly StyledProperty<string?> DialogTitleProperty =
        AvaloniaProperty.Register<FileSelectControl, string?>(nameof(DialogTitle));

    public static readonly StyledProperty<FileDialogTypeEnum> DialogTypeProperty =
        AvaloniaProperty.Register<FileSelectControl, FileDialogTypeEnum>(nameof(DialogType));

    public static readonly StyledProperty<FileDisplayStyleEnum> FileNameDisplayProperty =
        AvaloniaProperty.Register<FileSelectControl, FileDisplayStyleEnum>(nameof(FileNameDisplay));

    public static readonly StyledProperty<string?> FileNameProperty =
        AvaloniaProperty.Register<FileSelectControl, string?>(nameof(FileName), defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<FileSelectControl, HorizontalAlignment>(nameof(HorizontalContentAlignment),
            HorizontalAlignment.Left);

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<FileSelectControl, string?>(nameof(PlaceholderText), "Select file ...");

    public static readonly DirectProperty<FileSelectControl, bool> ShowFileDialogOnTextClickProperty =
        AvaloniaProperty.RegisterDirect<FileSelectControl, bool>(nameof(ShowFileDialogOnTextClick),
            o => o.ShowFileDialogOnTextClick, (o, v) => o.ShowFileDialogOnTextClick = v);

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<FileSelectControl, TextAlignment>(nameof(TextAlignment));

    public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent =
        RoutedEvent.Register<FileSelectControl, TextChangedEventArgs>(nameof(TextChanged), RoutingStrategies.Bubble);

    public static readonly DirectProperty<FileSelectControl, string?> TextProperty =
        AvaloniaProperty.RegisterDirect<FileSelectControl, string?>(nameof(Text), o => o.Text);

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<FileSelectControl, TextWrapping>(nameof(TextWrapping), TextWrapping.Wrap);

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<FileSelectControl, VerticalAlignment>(nameof(VerticalContentAlignment),
            VerticalAlignment.Center);

    #endregion

    #region Private fields

    private bool _allowClear = true;
    private Button? _clearButton;

    private Button? _dropButton;

    private bool _showFileDialogOnTextClick = true;

    private string? _text;

    #endregion

    public FileSelectControl()
    {
        BorderThickness = new Thickness(1);
    }

    #region Public properties

    public bool AllowClear
    {
        get => _allowClear;
        set => SetAndRaise(AllowClearProperty, ref _allowClear, value);
    }

    public string? DefaultExtension
    {
        get => GetValue(DefaultExtensionProperty);
        set => SetValue(DefaultExtensionProperty, value);
    }

    public string? DialogFilter
    {
        get => GetValue(DialogFilterProperty);
        set => SetValue(DialogFilterProperty, value);
    }

    public string? DialogTitle
    {
        get => GetValue(DialogTitleProperty);
        set => SetValue(DialogTitleProperty, value);
    }

    public FileDialogTypeEnum DialogType
    {
        get => GetValue(DialogTypeProperty);
        set => SetValue(DialogTypeProperty, value);
    }

    public string? FileName
    {
        get => GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    public FileDisplayStyleEnum FileNameDisplay
    {
        get => GetValue(FileNameDisplayProperty);
        set => SetValue(FileNameDisplayProperty, value);
    }

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public bool ShowFileDialogOnTextClick
    {
        get => _showFileDialogOnTextClick;
        set => SetAndRaise(ShowFileDialogOnTextClickProperty, ref _showFileDialogOnTextClick, value);
    }

    public string? Text
    {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    #endregion

    #region Public members

    public event EventHandler<TextChangedEventArgs> TextChanged
    {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }

    #endregion

    #region Protected members

    private Border? _textDisplay;
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textDisplay = e.NameScope.Find<Border>("PART_TextDisplay");
        if (_textDisplay != null)
        {
            _textDisplay.AddHandler(PointerPressedEvent, FileNameEditOnPointerPressed,
                RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
        }

        _clearButton = e.NameScope.Find<Button>("PART_ClearButton");
        if (_clearButton != null)
        {
            _clearButton.Click += ClearButtonOnClick;
        }

        _dropButton = e.NameScope.Find<Button>("dropButton");
        _dropButton?.Click += ShowFileDialogExecute;
    }

    //protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    //{
    //    _textDisplay?.AddHandler(PointerPressedEvent, FileNameEditOnPointerPressed,
    //        RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    //}

    //protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    //{
    //    _textDisplay?.RemoveHandler(PointerPressedEvent, FileNameEditOnPointerPressed);
    //}

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FileNameProperty)
        {
            SetDisplayText();
        }
        else if (change.Property == TextProperty && _clearButton != null)
        {
            _clearButton.IsVisible = !string.IsNullOrEmpty(Text) && AllowClear;
        }
    }

    #endregion

    #region Private members

    private void ClearButtonOnClick(object? sender, RoutedEventArgs e)
    {
        FileName = null;
    }

    private void FileNameEditOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ShowFileDialogOnTextClick)
        {
            e.Handled = true;
            ShowFileDialogExecute(sender, e);
        }
    }

    private void SetDisplayText()
    {
        if (!string.IsNullOrEmpty(FileName))
        {
            Text = FileNameDisplay switch
            {
                FileDisplayStyleEnum.FullPath => FileName,
                FileDisplayStyleEnum.ShortenPath => Path.TrimEndingDirectorySeparator(FileName),
                FileDisplayStyleEnum.FileName => Path.GetFileName(FileName),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            Text = null;
        }
        
        Dispatcher.UIThread.Post(() => RaiseEvent(new TextChangedEventArgs(TextChangedEvent)));
    }

    private void ShowFileDialogExecute(object? sender, RoutedEventArgs routedEventArgs)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            var filename = DialogType switch
            {
                FileDialogTypeEnum.FileOpen => await FileDialogExtensions.OpenFileDialog(DialogTitle ?? "Open File",
                    DialogFilter),
                FileDialogTypeEnum.FileOpenImage => await FileDialogExtensions.OpenFileImageDialog(
                    DialogTitle ?? "Open Image File", DialogFilter),
                FileDialogTypeEnum.FileSave => await FileDialogExtensions.SaveFileDialog(DialogTitle ?? "Save File",
                    DefaultExtension, null, true, DialogFilter),
                _ => throw new ArgumentOutOfRangeException(nameof(DialogType))
            };

            if (!string.IsNullOrEmpty(filename))
            {
                FileName = filename.Replace("%20", " ");
            }
        });
    }

    #endregion
}