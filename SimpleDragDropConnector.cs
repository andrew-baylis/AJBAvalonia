// SimpleDragDropConnector.cs
// Andrew Baylis
// Created: 31/05/2026

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AJBAvalonia;

public class SimpleDragDropConnector
{
    #region Private fields

    private bool _captured;
    private Point _dragStartPoint;
    private bool _handlersConnected;
    private double _horizontalDragThreshold;
    private bool _lock;
    private PointerPressedEventArgs? _triggerEvent;
    private double _verticalDragThreshold;

    #endregion

    public SimpleDragDropConnector(Control associatedObject, string dragDropIdentifier,
        double horizontalDragThreshold = 4, double verticalDragThreshold = 4)
    {
        AssociatedObject = associatedObject;
        DragDropIdentifier = dragDropIdentifier;
        _horizontalDragThreshold = horizontalDragThreshold;
        _verticalDragThreshold = verticalDragThreshold;
    }

    #region Public properties

    public Control AssociatedObject { get; }

    public string DragDropIdentifier { get; }

    /// <summary>
    /// </summary>
    public double HorizontalDragThreshold
    {
        get => _horizontalDragThreshold;
        set
        {
            if (value > 0)
            {
                _horizontalDragThreshold = value;
            }
        }
    }

    public bool IsDragSource { get; set; }

    public bool IsDropTarget { get; set; }

    /// <summary>
    /// </summary>
    public double VerticalDragThreshold
    {
        get => _verticalDragThreshold;
        set
        {
            if (value > 0)
            {
                _verticalDragThreshold = value;
            }
        }
    }

    #endregion

    #region Events

    protected void AssociatedObject_CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
        _captured = false;
    }

    protected async Task AssociatedObject_PointerMoved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
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
                    await DoDragDropAsync(_triggerEvent, MakeData());

                    OnAfterDrag?.Invoke(sender, _triggerEvent);
                }

                _triggerEvent = null;
            }
        }
    }

    protected void AssociatedObject_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed)
        {
            if (e.Source is Visual control && IsSourceChild(control))
            {
                _dragStartPoint = e.GetPosition(null);
                _triggerEvent = e;
                _lock = true;
                _captured = true;
            }
        }
    }

    protected void AssociatedObject_PointerReleased(object? sender, PointerReleasedEventArgs e)
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
        OnDragEnter?.Invoke(sender, e);
    }

    protected void DragLeave(object? sender, DragEventArgs e)
    {
        OnDragLeave?.Invoke(sender, e);
    }

    protected void DragOver(object? sender, DragEventArgs e)
    {
        OnDragOver?.Invoke(sender, e);
    }

    protected void Drop(object? sender, DragEventArgs e)
    {
        OnDrop?.Invoke(sender, e);
    }

    public event EventHandler<PointerPressedEventArgs>? OnAfterDrag;

    public event EventHandler<PointerPressedEventArgs>? OnBeforeDrag;

    public event EventHandler<DragEventArgs>? OnDragEnter;

    public event EventHandler<DragEventArgs>? OnDragLeave;

    public event EventHandler<DragEventArgs>? OnDragOver;

    public event EventHandler<DragEventArgs>? OnDrop;

    #endregion

    #region Public members

    public void ConnectHandlers()
    {
        if (_handlersConnected)
        {
            return;
        }

        if (IsDragSource)
        {
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, AssociatedObject_PointerPressed,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, AssociatedObject_PointerReleased,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent, AssociatedObject_PointerMoved,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, AssociatedObject_CaptureLost,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        }

        if (IsDropTarget)
        {
            DragDrop.SetAllowDrop(AssociatedObject, true);

            AssociatedObject.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AssociatedObject.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            AssociatedObject.AddHandler(DragDrop.DragOverEvent, DragOver);
            AssociatedObject.AddHandler(DragDrop.DropEvent, Drop);
        }

        _handlersConnected = true;
    }

    public void DisconnectHandlers()
    {
        if (!_handlersConnected)
        {
            return;
        }

        if (IsDragSource)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, AssociatedObject_PointerPressed);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, AssociatedObject_PointerReleased);
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, AssociatedObject_PointerMoved);
            AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, AssociatedObject_CaptureLost);
        }

        if (IsDropTarget)
        {
            DragDrop.SetAllowDrop(AssociatedObject, false);

            AssociatedObject.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
            AssociatedObject.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
            AssociatedObject.RemoveHandler(DragDrop.DragOverEvent, DragOver);
            AssociatedObject.RemoveHandler(DragDrop.DropEvent, Drop);
        }

        _handlersConnected = false;
    }

    public bool IsDragSourceChild(DragEventArgs args)
    {
        if (args.Source is Visual visual)
        {
            return IsSourceChild(visual);
        }

        return false;
    }

    #endregion

    #region Private members

    private async Task DoDragDropAsync(PointerPressedEventArgs e, IDataTransfer data)
    {
        var effect = DragDropEffects.None;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            effect |= DragDropEffects.Link;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            effect |= DragDropEffects.Move;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            effect |= DragDropEffects.Copy;
        }
        else
        {
            effect |= DragDropEffects.Move;
        }

        await DragDrop.DoDragDropAsync(e, data, effect);
    }

    private bool IsSourceChild(Visual source)
    {
        var p = source;
        while (p != null && p != AssociatedObject)
        {
            p = p.GetVisualParent();
        }

        return p != null;
    }

    private IDataTransfer MakeData()
    {
        var df = new DataTransferItem();
        df.SetText(DragDropIdentifier);
        var result = new DataTransfer();
        result.Add(df);
        return result;
    }

    private void Released()
    {
        _triggerEvent = null;
        _lock = false;
    }

    #endregion
}