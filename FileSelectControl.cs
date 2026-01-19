// FileSelectControl.cs
//  Andrew Baylis
//  Created: 08/01/2026

#region using

using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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
    #region Avalonia Properties

    public static readonly StyledProperty<string?> DefaultExtensionProperty = AvaloniaProperty.Register<FileSelectControl, string?>(nameof(DefaultExtension));

    public static readonly StyledProperty<string?> DialogFilterProperty = AvaloniaProperty.Register<FileSelectControl, string?>(nameof(DialogFilter));
    public static readonly StyledProperty<string?> DialogTitleProperty = AvaloniaProperty.Register<FileSelectControl, string?>(nameof(DialogTitle));

    public static readonly StyledProperty<FileDialogTypeEnum> DialogTypeProperty = AvaloniaProperty.Register<FileSelectControl, FileDialogTypeEnum>(nameof(DialogType));

    public static readonly StyledProperty<FileDisplayStyleEnum> FileNameDisplayProperty =
        AvaloniaProperty.Register<FileSelectControl, FileDisplayStyleEnum>(nameof(FileNameDisplay));

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<FileSelectControl, HorizontalAlignment>(nameof(HorizontalContentAlignment));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<FileSelectControl, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public static readonly StyledProperty<string?> WatermarkTextProperty = AvaloniaProperty.Register<FileSelectControl, string?>(nameof(WatermarkText));

    #endregion

    #region Fields

    public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent =
        RoutedEvent.Register<FileSelectControl, TextChangedEventArgs>(nameof(TextChanged), RoutingStrategies.Bubble);

    private Button? dropButton;

    private TextBox? fileNameEdit;

    #endregion

    public FileSelectControl()
    {
        BorderThickness = new Thickness(1);
    }

    #region Properties

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

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? WatermarkText
    {
        get => GetValue(WatermarkTextProperty);
        set => SetValue(WatermarkTextProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<TextChangedEventArgs> TextChanged
    {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }

    #endregion

    #region Protected Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        fileNameEdit = e.NameScope.Find<TextBox>("fileNameEdit");
        if (fileNameEdit != null)
        {
            fileNameEdit.Watermark = WatermarkText;
            fileNameEdit.HorizontalContentAlignment = HorizontalContentAlignment;
        }

        dropButton = e.NameScope.Find<Button>("dropButton");
        if (dropButton != null)
        {
            dropButton.Click += ShowFileDialogExecute;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HorizontalContentAlignmentProperty && fileNameEdit != null)
        {
            fileNameEdit.HorizontalContentAlignment = HorizontalContentAlignment;
        }
    }

    #endregion

    #region Private Methods

    private void SetDisplayText()
    {
        if (fileNameEdit != null)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                switch (FileNameDisplay)
                {
                    case FileDisplayStyleEnum.FullPath:
                        fileNameEdit.Text = Text;
                        break;
                    case FileDisplayStyleEnum.ShortenPath:
                        fileNameEdit.Text = Path.TrimEndingDirectorySeparator(Text);
                        break;
                    case FileDisplayStyleEnum.FileName:
                        fileNameEdit.Text = Path.GetFileName(Text);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                fileNameEdit.Clear();
            }
        }
    }

    private void ShowFileDialogExecute(object? sender, RoutedEventArgs routedEventArgs)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            string? filename = null;
            switch (DialogType)
            {
                case FileDialogTypeEnum.FileOpen:
                    filename = await FileDialogExtensions.OpenFileDialog(DialogTitle ?? "Open File", DialogFilter);
                    break;
                case FileDialogTypeEnum.FileOpenImage:
                    filename = await FileDialogExtensions.OpenFileImageDialog(DialogTitle ?? "Open Image File", DialogFilter);
                    break;
                case FileDialogTypeEnum.FileSave:
                    filename = await FileDialogExtensions.SaveFileDialog(DialogTitle ?? "Save File", DefaultExtension, true, DialogFilter);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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