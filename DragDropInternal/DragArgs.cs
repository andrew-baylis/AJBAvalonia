// DragArgs.cs
//  Andrew Baylis
//  Created: 20/01/2024

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

#endregion

namespace AJBAvalonia.DragDropInternal;

internal class DragArgs : RoutedEventArgs
{
    public DragArgs(object? sourceData, Control? sourceItem, IDataTransfer data, PointerPressedEventArgs pointerArgs)
    {
        PointerArgs = pointerArgs;
        SourceData = sourceData;
        SourceItem = sourceItem;
        Data = data;
    }

    #region Properties

    public IDataTransfer Data { get; }

    public PointerPressedEventArgs PointerArgs { get; }

    public object? SourceData { get; }

    public Control? SourceItem { get; }

    #endregion

    #region Public Methods

    public Point GetPosition(Visual? relativeTo)
    {
        return PointerArgs.GetPosition(relativeTo);
    }

    #endregion
}