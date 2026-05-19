// FileSelectControl.cs
// Andrew Baylis
// Created: 19/05/2026

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
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

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<FileSelectControl, HorizontalAlignment>(nameof(HorizontalContentAlignment));

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<FileSelectControl, string?>(nameof(PlaceholderText), "Select file ...");

    public static readonly DirectProperty<FileSelectControl, bool> ShowFileDialogOnTextClickProperty =
        AvaloniaProperty.RegisterDirect<FileSelectControl, bool>(nameof(ShowFileDialogOnTextClick),
            o => o.ShowFileDialogOnTextClick, (o, v) => o.ShowFileDialogOnTextClick = v);

    public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent =
        RoutedEvent.Register<FileSelectControl, TextChangedEventArgs>(nameof(TextChanged), RoutingStrategies.Bubble);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<FileSelectControl, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    #endregion

    #region Private fields

    private Button? _dropButton;

    private TextBox? _fileNameEdit;

    private bool _showFileDialogOnTextClick = true;

    #endregion

    public FileSelectControl()
    {
        BorderThickness = new Thickness(1);
    }

    #region Public properties

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
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _fileNameEdit = e.NameScope.Find<TextBox>("fileNameEdit");
        if (_fileNameEdit != null)
        {
            _fileNameEdit.PlaceholderText = PlaceholderText;
            _fileNameEdit.HorizontalContentAlignment = HorizontalContentAlignment;
        }

        _dropButton = e.NameScope.Find<Button>("dropButton");
        _dropButton?.Click += ShowFileDialogExecute;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        AddHandler(PointerPressedEvent, FileNameEditOnPointerPressed,
            RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RemoveHandler(PointerPressedEvent, FileNameEditOnPointerPressed);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HorizontalContentAlignmentProperty && _fileNameEdit != null)
        {
            _fileNameEdit.HorizontalContentAlignment = HorizontalContentAlignment;
        }
    }

    #endregion

    #region Private members

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
        if (_fileNameEdit != null)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                _fileNameEdit.Text = FileNameDisplay switch
                {
                    FileDisplayStyleEnum.FullPath => Text,
                    FileDisplayStyleEnum.ShortenPath => Path.TrimEndingDirectorySeparator(Text),
                    FileDisplayStyleEnum.FileName => Path.GetFileName(Text),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            else
            {
                _fileNameEdit.Clear();
            }
        }
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
                Text = filename.Replace("%20", " ");
                SetDisplayText();
                RaiseEvent(new TextChangedEventArgs(TextChangedEvent));
            }
        });
    }

    #endregion
}