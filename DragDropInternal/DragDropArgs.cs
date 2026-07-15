// DragDropArgs.cs
//  Andrew Baylis
//  Created: 20/01/2024

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

#endregion

namespace AJBAvalonia.DragDropInternal;

internal enum TargetPointerHorizontalLocationEnum
{
    Left,
    Middle,
    Right
}
internal enum TargetPointerVerticalLocationEnum
{
    Top,
    Middle,
    Bottom
}

internal class DragDropArgs : RoutedEventArgs
{
    public DragDropArgs(object? sourceData, object? targetData, Control? sourceItem, Control? targetItem, DragEventArgs dragEvent)
    {
        SourceData = sourceData;
        TargetData = targetData;
        SourceItem = sourceItem;
        TargetItem = targetItem;
        DragEvent = dragEvent;
        Source = (dragEvent.DataTransfer as ExDataTransfer)?.AssociatedObject;
    }

    #region Properties

    public DragDropEffects DragEffects
    {
        get => DragEvent.DragEffects;
        set => DragEvent.DragEffects = value;
    }

    public DragEventArgs DragEvent { get; }

    public object? SourceData { get; }

    public Control? SourceItem { get; }

    public object? TargetData { get; }

    public Control? TargetItem { get; }

    #endregion

    #region Public Methods

    public Point DropPositionRelativeToTargetItem()
    {
        return TargetItem != null ? DragEvent.GetPosition(TargetItem) : new Point(0, 0);
    }

    public TargetPointerHorizontalLocationEnum GetHorizontalDropPosition()
    {
        if (TargetItem != null)
        {
            var p = DropPositionRelativeToTargetItem();
            var bnds = TargetItem.Bounds;

            if (p.X < bnds.Width / 3)
            {
                return TargetPointerHorizontalLocationEnum.Left;
            }

            if (p.X > 2 * bnds.Width / 3)
            {
                return TargetPointerHorizontalLocationEnum.Right;
            }
        }

        return TargetPointerHorizontalLocationEnum.Middle;
    }

    public Point GetPosition(Visual relativeTo)
    {
        return DragEvent.GetPosition(relativeTo);
    }

    public TargetPointerVerticalLocationEnum GetVerticalDropPosition()
    {
        if (TargetItem != null)
        {
            var pos = DropPositionRelativeToTargetItem();
            var bounds = TargetItem.Bounds;

            var h = bounds.Height;
            if (pos.Y < h / 3)
            {
                return TargetPointerVerticalLocationEnum.Top;
            }

            if (pos.Y > 2 * h / 3)
            {
                return TargetPointerVerticalLocationEnum.Bottom;
            }
        }

        return TargetPointerVerticalLocationEnum.Middle;
    }

    #endregion
}