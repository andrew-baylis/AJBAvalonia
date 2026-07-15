// ExDataTransfer.cs
//  Andrew Baylis
//  Created: 17/03/2026

#region using

using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

#endregion

namespace AJBAvalonia.DragDropInternal;

internal class ExDataTransfer : IDataTransfer
{
    #region Fields

    private readonly List<CustomDataTransferItem> _items = [];
    private DataFormat[]? _formats;

    #endregion

    public object? AssociatedObject { get; internal set; }

    public object? SourceControlItem { get; internal set; }

    public object? SourceDataContext { get; internal set; }

    #region Public Methods

    public void Add(CustomDataTransferItem item)
    {
        _formats = null;
        _items.Add(item);
    }

    public void Add<T>(string identifier, T data) where T : notnull
    {
        Add(CustomDataTransferItem.Create(identifier, data));
    }

    public void Add<T>(T data) where T : class
    {
        Add(typeof(T).Name, data);
    }

    public void AddBitmap(Bitmap bitmap)
    {
        var c = new CustomDataTransferItem(DataFormat.Bitmap, bitmap);
        Add(c);
    }

    public void AddBitmapWithIdentifier(string identifier, Bitmap bitmap)
    {
        Add(new CustomDataTransferItem(DataFormat.CreateBytesApplicationFormat(identifier), bitmap));
    }

    public void AddFile(IStorageItem file)
    {
        var c = new CustomDataTransferItem(DataFormat.File, file);
        Add(c);
    }

    public void AddFiles(IEnumerable<IStorageItem> files)
    {
        foreach (var file in files)
        {
            AddFile(file);
        }
    }

    public void AddFiles(params IStorageItem[] files)
    {
        foreach (var file in files)
        {
            AddFile(file);
        }
    }

    public void AddText(string text)
    {
        var c = new CustomDataTransferItem(DataFormat.Text, text);
        Add(c);
    }

    public void AddTextWithIdentifier(string identifier, string text)
    {
        Add(new CustomDataTransferItem(DataFormat.CreateStringApplicationFormat(identifier), text));
    }

    public Bitmap? GetBitmap()
    {
        var item = GetItem(DataFormat.Bitmap);
        return item?.TryGetRaw(DataFormat.Bitmap) as Bitmap;
    }

    public Bitmap? GetBitmapByIdentifier(string identifier)
    {
        var item = GetItemByIdentifier(identifier);
        return item?.TryGetRaw(item.Format) as Bitmap;
    }

    public T? GetData<T>(DataFormat format)
    {
        var item = GetItem(format);
        return (T?)item?.TryGetRaw(format);
    }

    public T? GetData<T>()
    {
        var item = GetItemByIdentifier(typeof(T).Name);
        return (T?)item?.TryGetRaw(item.Format);
    }

    public object? GetData(DataFormat format)
    {
        var item = GetItem(format);
        return item?.TryGetRaw(format);
    }

    public T? GetDataByIdentifier<T>(string identifier)
    {
        var item = GetItemByIdentifier(identifier);
        return (T?)item?.TryGetRaw(item.Format);
    }

    public object? GetDataByIdentifier(string identifier)
    {
        var item = GetItemByIdentifier(identifier);
        return item?.TryGetRaw(item.Format);
    }

    public CustomDataTransferItem? GetItem(DataFormat format)
    {
        return _items.FirstOrDefault(item => item.Format == format);
    }

    public CustomDataTransferItem? GetItemByIdentifier(string identifier)
    {
        return _items.FirstOrDefault(item => item.Format.Identifier == identifier);
    }

    public string? GetText()
    {
        var item = GetItem(DataFormat.Text);
        return item?.TryGetRaw(DataFormat.Text) as string;
    }

    public string? GetTextByIdentifier(string identifier)
    {
        var item = GetItemByIdentifier(identifier);
        return item?.TryGetRaw(item.Format) as string;
    }

    #endregion

    #region IDataTransfer Members

    /// <inheritdoc cref="IDataTransferItem.Formats" />
    public IReadOnlyList<DataFormat> Formats
    {
        get
        {
            return _formats ??= GetFormatsCore();

            DataFormat[] GetFormatsCore()
            {
                return _items.SelectMany(item => item.Formats).Distinct().ToArray();
            }
        }
    }

    public IReadOnlyList<IDataTransferItem> Items => _items;

    public void Dispose()
    {
    }

    #endregion
}

public class CustomDataTransferItem : IDataTransferItem
{
    public CustomDataTransferItem(DataFormat format, object data)
    {
        Format = format;
        Data = data;
        Formats = [Format];
    }

    #region Properties

    public object Data { get; }

    public DataFormat Format { get; }

    #endregion

    #region Static Methods

    public static CustomDataTransferItem Create<T>(string identifier, T data) where T : notnull
    {
        var format = DataFormat.CreateBytesApplicationFormat(identifier);
        return new CustomDataTransferItem(format, data);
    }

    #endregion

    #region IDataTransferItem Members

    public IReadOnlyList<DataFormat> Formats { get; }

    public object? TryGetRaw(DataFormat format)
    {
        if (format == Format)
        {
            return Data;
        }

        return null;
    }

    #endregion
}