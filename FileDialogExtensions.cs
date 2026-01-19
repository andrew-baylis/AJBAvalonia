// FileDialogExtensions.cs
//  Andrew Baylis
//  Created: 02/03/2024

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

#endregion

namespace AJBAvalonia;

public static class FileDialogExtensions
{
    #region Static Methods

    public static async Task<string> OpenFileDialog(string title, string? filters = null, string? startFolder = null)
    {
        var storageProvider = GetStorageProvider();
        return await OpenFileDialog(storageProvider, title, filters, startFolder);
    }

    public static async Task<string> OpenFileDialog(Control source, string title, string? filters = null, string? startFolder = null)
    {
        var storageProvider = GetStorageProvider(source);
        return await OpenFileDialog(storageProvider, title, filters, startFolder);
    }

    public static async Task<string> OpenFileImageDialog(string title, string? filters = null, string? startFolder = null)
    {
        if (string.IsNullOrEmpty(filters))
        {
            filters = @"All Image types|*.bmp,*gif,*.png,*.jpg,*.jpeg|Bitmap|*.bmp|GIF|*.gif|JPEG|*.jpg,*.jpeg|PNG|*.png|All files|*.*";
        }

        return await OpenFileDialog(title, filters, startFolder);
    }

    public static async Task<string> OpenFileImageDialog(Control source, string title, string? filters = null, string? startFolder = null)
    {
        if (string.IsNullOrEmpty(filters))
        {
            filters = @"All Image types|*.bmp,*gif,*.png,*.jpg,*.jpeg|Bitmap|*.bmp|GIF|*.gif|JPEG|*.jpg,*.jpeg|PNG|*.png|All files|*.*";
        }

        return await OpenFileDialog(source, title, filters, startFolder);
    }

    public static async Task<string> OpenFolderDialog(string title, string? startFolder = null)
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider is {CanPickFolder: true})
        {
            var startUri = !string.IsNullOrEmpty(startFolder) ? await storageProvider.TryGetFolderFromPathAsync(startFolder) : null;
            var files = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {Title = title, AllowMultiple = false, SuggestedStartLocation = startUri});

            return files[0].Path.AbsolutePath;
        }

        return string.Empty;
    }

    public static async Task<string> SaveFileDialog(string title, string? defaultExtension, bool? showOverwritePrompt = true, string? filters = null, string? startFolder = null)
    {
        var storageProvider = GetStorageProvider();
        return await SaveFileDialog(storageProvider, title, defaultExtension, showOverwritePrompt, filters, startFolder);
    }

    public static async Task<string> SaveFileDialog(Control source, string title, string? defaultExtension, bool? showOverwritePrompt = true, string? filters = null,
        string? startFolder = null)
    {
        var storageProvider = GetStorageProvider(source);
        return await SaveFileDialog(storageProvider, title, defaultExtension, showOverwritePrompt, filters, startFolder);
    }

    private static IStorageProvider? GetStorageProvider(Control source)
    {
        var topLevel = TopLevel.GetTopLevel(source);
        return topLevel?.StorageProvider;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        return Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime {MainWindow: not null} desktopLifetime => GetStorageProvider(desktopLifetime.MainWindow),
            ISingleViewApplicationLifetime {MainView: not null} singleViewPlatform => GetStorageProvider(singleViewPlatform.MainView),
            _ => null
        };
    }

    private static async Task<string> OpenFileDialog(IStorageProvider? storageProvider, string title, string? filters = null, string? startFolder = null)
    {
        if (storageProvider is {CanOpen: true})
        {
            var filterList = new List<FilePickerFileType>();
            if (!string.IsNullOrEmpty(filters))
            {
                var splitFilters = filters.Split('|');
                var i = 0;
                while (i < splitFilters.Length)
                {
                    var text = splitFilters[i++];
                    if (i < splitFilters.Length)
                    {
                        var filter = splitFilters[i++];
                        var filtersplit = filter.Split(';', ',');
                        filterList.Add(new FilePickerFileType(text) {Patterns = filtersplit});
                    }
                }
            }

            if (!filterList.Any())
            {
                filterList.Add(new FilePickerFileType("All Files") {Patterns = new[] {"*.*"}});
            }

            var startUri = !string.IsNullOrEmpty(startFolder) ? await storageProvider.TryGetFolderFromPathAsync(startFolder) : null;
            var files = await Dispatcher.UIThread.InvokeAsync(async () => await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = title,
                FileTypeFilter = filterList,
                SuggestedStartLocation = startUri
            }));
            //var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            //{
            //    AllowMultiple = false, Title = title, FileTypeFilter = filterList, SuggestedStartLocation = startUri
            //});
            if (files.Any())
            {
                return files[0].Path.AbsolutePath;
            }
        }

        return string.Empty;
    }

    private static async Task<string> SaveFileDialog(IStorageProvider? storageProvider, string title, string? defaultExtension, bool? showOverwritePrompt = true,
        string? filters = null, string? startFolder = null)
    {
        if (storageProvider is {CanSave: true})
        {
            var filterList = new List<FilePickerFileType>();
            if (!string.IsNullOrEmpty(filters))
            {
                var splitFilters = filters.Split('|');
                var i = 0;
                while (i < splitFilters.Length)
                {
                    var text = splitFilters[i++];
                    if (i < splitFilters.Length)
                    {
                        var filter = splitFilters[i++];
                        filterList.Add(new FilePickerFileType(text) {Patterns = new[] {filter}});
                    }
                }
            }

            if (!filterList.Any())
            {
                filterList.Add(new FilePickerFileType("All Files") {Patterns = new[] {"*.*"}});
            }

            var startUri = !string.IsNullOrEmpty(startFolder) ? await storageProvider.TryGetFolderFromPathAsync(startFolder) : null;
            var files = await Dispatcher.UIThread.InvokeAsync(async () => await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                DefaultExtension = defaultExtension,
                Title = title,
                FileTypeChoices = filterList,
                ShowOverwritePrompt = showOverwritePrompt,
                SuggestedStartLocation = startUri
            }));
            if (files != null)
            {
                return files.Path.AbsolutePath;
            }
        }

        return string.Empty;
    }

    #endregion
}