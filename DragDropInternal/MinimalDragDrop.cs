// MinimalDragDrop.cs
// Andrew Baylis
// Created: 14/07/2026

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AJBAvalonia.DragDropInternal;

internal class MinimalDragDrop
{
    #region Static Protected

    protected const string SourceControlItem = "SourceControlItem";

    protected const string SourceDataContext = "SourceDataContext";

    #endregion

    #region Private fields

    private readonly Control _target;

    private bool _captured;
    private Point _dragStartPoint;
    private bool _lock;
    private DragArgs? _triggerEvent;

    #endregion

    public MinimalDragDrop(Control target)
    {
        _target = target;
        _target.AttachedToVisualTree += TargetOnAttachedToVisualTree;
        _target.DetachedFromVisualTree += TargetOnDetachedFromVisualTree;
    }

    #region Public properties

    public double HorizontalDragThreshold { get; set; } = 4d;

    public bool IsDragSource { get; set; }

    public bool IsDropTarget { get; set; }

    public double VerticalDragThreshold { get; set; } = 4d;

    #endregion

    #region Events

    public event EventHandler<DragArgs>? OnAfterDrag;

    public event EventHandler<DragAllowArgs>? OnAllowDrag;

    public event EventHandler<DragArgs>? OnBeforeDrag;

    public event EventHandler<DragDropArgs>? OnDragEnter;

    public event EventHandler<DragDropArgs>? OnDragLeave;

    public event EventHandler<DragDropArgs>? OnDragOver;

    public event EventHandler<DragDropArgs>? OnDrop;

    protected void _target_CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
        _captured = false;
    }

    protected async void _target_PointerMoved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(_target).Properties;
        if (_captured && properties.IsLeftButtonPressed && _triggerEvent is not null)
        {
            var point = e.GetPosition(null);
            var diff = _dragStartPoint - point;
            var horizontalDragThreshold = HorizontalDragThreshold;
            var verticalDragThreshold = VerticalDragThreshold;

            if (Math.Abs(diff.X) > horizontalDragThreshold || Math.Abs(diff.Y) > verticalDragThreshold)
            {
                if (_lock)
                {
                    _lock = false;
                }
                else
                {
                    return;
                }

                OnBeforeDrag?.Invoke(sender, _triggerEvent);
                if (!_triggerEvent.Handled)
                {
                    if (!_triggerEvent.Handled)
                    {
                        await DoDragDropAsync(_triggerEvent.PointerArgs, _triggerEvent.Data);

                        OnAfterDrag?.Invoke(sender, _triggerEvent);
                    }
                }

                _triggerEvent = null;
            }
        }
    }

    protected void _target_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(_target).Properties;
        if (properties.IsLeftButtonPressed && DoAllowDragStart(e))
        {
            if (e.Source is Control control && IsSourceChild(control))
            {
                _dragStartPoint = e.GetPosition(null);
                var d = MakeDragDropData(e);
                _triggerEvent = MakeDragArgs(e, d);
                _lock = true;
                _captured = true;
            }
        }
    }

    protected void _target_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_captured)
        {
            if (e.InitialPressMouseButton == MouseButton.Left && _triggerEvent is not null)
            {
                Released();
            }

            _captured = false;
        }
    }

    protected void DragEnter(object? sender, DragEventArgs e)
    {
        var args = MakeDragDropArgs(e);
        OnDragEnter?.Invoke(sender, args);
    }

    protected void DragLeave(object? sender, DragEventArgs e)
    {
        var args = MakeDragDropArgs(e);
        OnDragLeave?.Invoke(sender, args);
    }

    protected void DragOver(object? sender, DragEventArgs e)
    {
        var args = MakeDragDropArgs(e);
        OnDragOver?.Invoke(sender, args);
    }

    protected void Drop(object? sender, DragEventArgs e)
    {
        var args = MakeDragDropArgs(e);
        OnDrop?.Invoke(sender, args);
    }

    private void TargetOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (IsDragSource)
        {
            _target.AddHandler(InputElement.PointerPressedEvent, _target_PointerPressed,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            _target.AddHandler(InputElement.PointerReleasedEvent, _target_PointerReleased,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            _target.AddHandler(InputElement.PointerMovedEvent, _target_PointerMoved,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            _target.AddHandler(InputElement.PointerCaptureLostEvent, _target_CaptureLost,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        }

        if (IsDropTarget)
        {
            DragDrop.SetAllowDrop(_target, true);

            _target.AddHandler(DragDrop.DragEnterEvent, DragEnter, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            _target.AddHandler(DragDrop.DragLeaveEvent, DragLeave, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            _target.AddHandler(DragDrop.DragOverEvent, DragOver, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            _target.AddHandler(DragDrop.DropEvent, Drop, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        }
    }

    /// <inheritdoc />
    private void TargetOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (IsDragSource)
        {
            _target.RemoveHandler(InputElement.PointerPressedEvent, _target_PointerPressed);
            _target.RemoveHandler(InputElement.PointerReleasedEvent, _target_PointerReleased);
            _target.RemoveHandler(InputElement.PointerMovedEvent, _target_PointerMoved);
            _target.RemoveHandler(InputElement.PointerCaptureLostEvent, _target_CaptureLost);
        }

        if (IsDropTarget)
        {
            DragDrop.SetAllowDrop(_target, false);

            _target.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
            _target.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
            _target.RemoveHandler(DragDrop.DragOverEvent, DragOver);
            _target.RemoveHandler(DragDrop.DropEvent, Drop);
        }
    }

    #endregion

    #region Public Methods

    public static Control? GetSourceControlItem(DragEventArgs e)
    {
        if (e.DataTransfer is ExDataTransfer customTransfer)
        {
            return customTransfer.GetDataByIdentifier<Control>(SourceControlItem);
        }

        return null;
        // return e.DataTransfer.GetLocalObject(SourceControlItem) as Control;
    }

    public static Control? GetSourceControlItem(DragDropArgs e)
    {
        return GetSourceControlItem(e.DragEvent);
    }

    public static object? GetSourceDataContext(DragEventArgs e)
    {
        if (e.DataTransfer is ExDataTransfer customTransfer)
        {
            return customTransfer.GetDataByIdentifier<Control>(SourceDataContext);
        }

        return null;
        //return e.DataTransfer.GetLocalObject(SourceDataContext);
    }

    public static object? GetSourceDataContext(DragDropArgs e)
    {
        return GetSourceDataContext(e.DragEvent);
    }

    #endregion

    #region Protected Methods

    protected virtual bool DoAllowDragStart(PointerPressedEventArgs e)
    {
        if (OnAllowDrag != null)
        {
            var args = new DragAllowArgs(e);
            OnAllowDrag.Invoke(this, args);
            return args.Allow;
        }

        return true;
    }

    protected Control? GetSourceControlItem(PointerPressedEventArgs e)
    {
        return _target;
    }

    protected virtual object? GetSourceData(Control? sourceItem)
    {
        return sourceItem?.DataContext;
    }

    protected Control? GetTargetControlItem(DragEventArgs e)
    {
        return _target;
    }

    protected virtual object? GetTargetData(Control? targetItem)
    {
        return targetItem?.DataContext;
    }

    protected virtual DragArgs MakeDragArgs(PointerPressedEventArgs e, IDataTransfer data)
    {
        var sourceItem = (data as ExDataTransfer)?.SourceControlItem as Control ?? GetSourceControlItem(e);
        var sourceData = GetSourceData(sourceItem);
        e.Source = _target;
        return new DragArgs(sourceData, sourceItem, data, e);
    }

    protected virtual DragDropArgs MakeDragDropArgs(DragEventArgs e)
    {
        var targetItem = GetTargetControlItem(e);
        var targetData = GetTargetData(targetItem);
        var sourceItem = (e.DataTransfer as ExDataTransfer)?.SourceControlItem as Control ?? GetSourceControlItem(e);
        var sourceData = GetSourceData(sourceItem);
        return new DragDropArgs(sourceData, targetData, sourceItem, targetItem, e);
    }

    protected virtual ExDataTransfer MakeDragDropData(PointerPressedEventArgs e)
    {
        var d = new ExDataTransfer();

        var srcItem = GetSourceControlItem(e);
        d.SourceControlItem = srcItem;

        d.SourceDataContext = srcItem?.DataContext;

        d.AssociatedObject = _target;

        return d;
    }

    #endregion

    #region Private Methods

    private async Task DoDragDropAsync(PointerPressedEventArgs e, IDataTransfer data)
    {
        var effect = DragDropEffects.None;

            effect |= DragDropEffects.Move| DragDropEffects.Copy;

        await DragDrop.DoDragDropAsync(e, data, effect);
    }

    private bool IsSourceChild(Control source)
    {
        Visual? p = source;
        while (p != null && p != _target)
        {
            p = p.GetVisualParent();
        }

        return p != null;
    }

    private void Released()
    {
        _triggerEvent = null;
        _lock = false;
    }

    #endregion
}