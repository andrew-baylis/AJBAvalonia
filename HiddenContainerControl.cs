// HiddenContainerControl.cs
//  Andrew Baylis
//  Created: 25/08/2025

#region using

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

#endregion

namespace AJBAvalonia;

/// <summary>
/// Container that can hide its child from layout and rendering while optionally drawing a background.
/// </summary>
public class HiddenContainerControl : Decorator
{
    #region Avalonia Properties

    /// <summary>
    /// Identifies the Background property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty = AvaloniaProperty.Register<HiddenContainerControl, IBrush?>(nameof(Background));

    /// <summary>
    /// Identifies whether the child is hidden (removed from layout).
    /// </summary>
    public static readonly StyledProperty<bool> IsHiddenProperty = AvaloniaProperty.Register<HiddenContainerControl, bool>(nameof(IsHidden));

    #endregion

    static HiddenContainerControl()
    {
        AffectsMeasure<Decorator>(ChildProperty, PaddingProperty, IsHiddenProperty);
        IsHiddenProperty.Changed.AddClassHandler<HiddenContainerControl>((x, e) => x.HiddenChanged(e));
    }

    #region Properties

    /// <summary>
    /// Gets or sets the background brush used to fill the control.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the child content is hidden from layout and rendering.
    /// </summary>
    public bool IsHidden
    {
        get => GetValue(IsHiddenProperty);
        set => SetValue(IsHiddenProperty, value);
    }

    #endregion

    #region Override Methods

    /// <summary>
    /// Renders the background if set and the child as usual.
    /// </summary>
    /// <param name="context">Drawing context.</param>
    public override void Render(DrawingContext context)
    {
        if (Background != null)
        {
            context.FillRectangle(Background, new Rect(0, 0, Bounds.Width, Bounds.Height));
        }

        base.Render(context);
    }

    /// <summary>
    /// Arranges the child element unless the control is hidden.
    /// </summary>
    /// <param name="finalSize">Final area size.</param>
    protected override Size ArrangeOverride(Size finalSize)
    {
        return !IsHidden ? base.ArrangeOverride(finalSize) : finalSize;
    }

    /// <summary>
    /// Measures the child when not hidden. Temporarily adds the child to the visual tree to measure correctly.
    /// </summary>
    /// <param name="availableSize">Available size for measurement.</param>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child != null && !VisualChildren.Contains(Child))
        {
            VisualChildren.Add(Child);
        }

        try
        {
            return LayoutHelper.MeasureChild(Child, availableSize, Padding);
        }
        finally
        {
            if (IsHidden && Child != null)
            {
                VisualChildren.Remove(Child);
            }
        }
    }

    #endregion

    #region Private Methods

    private void HiddenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    #endregion
}