// Indicator.cs
// Andrew Baylis
// Created: 17/05/2026

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace AJBAvalonia;

public enum IndicatorShapeEnum
{
    Ellipse,
    Rectangle,
    RoundedRectangle
}

public enum IndicatorColorEnum
{
    Red,
    Green,
    Blue,
    Yellow,
    Orange,
    Purple,
    Custom
}

public class Indicator : TemplatedControl
{
    public static readonly DirectProperty<Indicator, double> BorderWidthProperty =
        AvaloniaProperty.RegisterDirect<Indicator, double>(nameof(BorderWidth), o => o.BorderWidth,
            (o, v) => o.BorderWidth = v);

    public static readonly DirectProperty<Indicator, double> CornerRadiusXProperty =
        AvaloniaProperty.RegisterDirect<Indicator, double>(nameof(CornerRadiusX), o => o.CornerRadiusX,
            (o, v) => o.CornerRadiusX = v);

    public static readonly DirectProperty<Indicator, double> CornerRadiusYProperty =
        AvaloniaProperty.RegisterDirect<Indicator, double>(nameof(CornerRadiusY), o => o.CornerRadiusY,
            (o, v) => o.CornerRadiusY = v);

    public static readonly DirectProperty<Indicator, IndicatorColorEnum> IndicatorColorProperty =
        AvaloniaProperty.RegisterDirect<Indicator, IndicatorColorEnum>(nameof(IndicatorColor), o => o.IndicatorColor,
            (o, v) => o.IndicatorColor = v);

    public static readonly DirectProperty<Indicator, IndicatorShapeEnum> IndicatorShapeProperty =
        AvaloniaProperty.RegisterDirect<Indicator, IndicatorShapeEnum>(nameof(IndicatorShape), o => o.IndicatorShape,
            (o, v) => o.IndicatorShape = v);

    public static readonly StyledProperty<IBrush?> SelectedBorderBrushProperty =
        AvaloniaProperty.Register<Indicator, IBrush?>(nameof(SelectedBorderBrush));

    public static readonly StyledProperty<IBrush?> SelectedBrushProperty =
        AvaloniaProperty.Register<Indicator, IBrush?>(nameof(SelectedBrush));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<Indicator, bool>(nameof(IsSelected));

    public static readonly StyledProperty<IBrush?> UnselectedBorderBrushProperty =
        AvaloniaProperty.Register<Indicator, IBrush?>(nameof(UnselectedBorderBrush));

    public static readonly StyledProperty<IBrush?> UnselectedBrushProperty =
        AvaloniaProperty.Register<Indicator, IBrush?>(nameof(UnselectedBrush));

    #region Private fields

    private double _borderWidth;

    private ContentPresenter? _contentPresenter;

    private double _cornerRadiusX = 10d;

    private double _cornerRadiusY = 5d;

    private IndicatorColorEnum _indicatorColor = IndicatorColorEnum.Green;

    private IndicatorShapeEnum _indicatorShape = IndicatorShapeEnum.Ellipse;

    private Shape? _shape;

    #endregion

    #region Public properties

    public double BorderWidth
    {
        get => _borderWidth;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            SetAndRaise(BorderWidthProperty, ref _borderWidth, value);
        }
    }

    public double CornerRadiusX
    {
        get => _cornerRadiusX;
        set
        {
            SetAndRaise(CornerRadiusXProperty, ref _cornerRadiusX, value);
            if (IndicatorShape == IndicatorShapeEnum.RoundedRectangle)
            {
                SetShapeCorners();
            }
        }
    }

    public double CornerRadiusY
    {
        get => _cornerRadiusY;
        set => SetAndRaise(CornerRadiusYProperty, ref _cornerRadiusY, value);
    }

    public IndicatorColorEnum IndicatorColor
    {
        get => _indicatorColor;
        set => SetAndRaise(IndicatorColorProperty, ref _indicatorColor, value);
    }

    public IndicatorShapeEnum IndicatorShape
    {
        get => _indicatorShape;
        set => SetAndRaise(IndicatorShapeProperty, ref _indicatorShape, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public IBrush? SelectedBorderBrush
    {
        get => GetValue(SelectedBorderBrushProperty);
        set => SetValue(SelectedBorderBrushProperty, value);
    }

    public IBrush? SelectedBrush
    {
        get => GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    public IBrush? UnselectedBorderBrush
    {
        get => GetValue(UnselectedBorderBrushProperty);
        set => SetValue(UnselectedBorderBrushProperty, value);
    }

    public IBrush? UnselectedBrush
    {
        get => GetValue(UnselectedBrushProperty);
        set => SetValue(UnselectedBrushProperty, value);
    }

    #endregion

    #region Protected members

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        SetShape();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsSelectedProperty || change.Property == SelectedBrushProperty ||
            change.Property == UnselectedBrushProperty || change.Property == IndicatorColorProperty)
        {
            SetShapeBrush();
        }
        else if (change.Property == IndicatorShapeProperty)
        {
            SetShape();
        }
        else if (change.Property == WidthProperty || change.Property == HeightProperty)
        {
            SetShapeSize();
        }
        else if (change.Property == CornerRadiusXProperty || change.Property == CornerRadiusYProperty)
        {
            SetShapeCorners();
        }
    }

    #endregion

    #region Private members

    private IBrush? MakeBorderBrush()
    {
        switch (IndicatorColor)
        {
            case IndicatorColorEnum.Red:
                return new SolidColorBrush(Colors.DarkRed);
                break;
            case IndicatorColorEnum.Green:
                return new SolidColorBrush(Colors.DarkGreen);
                break;
            case IndicatorColorEnum.Blue:
                return new SolidColorBrush(Colors.DarkBlue);
                break;
            case IndicatorColorEnum.Yellow:
                return new SolidColorBrush(Color.Parse("#FF808000"));
                break;
            case IndicatorColorEnum.Orange:
                return new SolidColorBrush(Colors.DarkOrange);
                break;
            case IndicatorColorEnum.Purple:
                return new SolidColorBrush(Color.Parse("#FF400040"));
                break;
            case IndicatorColorEnum.Custom:
            default:
                return SelectedBorderBrush;
        }
    }

    private IBrush? MakeIndicatorBrush()
    {
        Color? centerColor;
        Color? edgeColor;
        switch (IndicatorColor)
        {
            case IndicatorColorEnum.Green:
                centerColor = Color.Parse("#FF92F16E");
                edgeColor = Colors.Green;
                break;
            case IndicatorColorEnum.Red:
                centerColor = Color.Parse("#FFFFB8B8");
                edgeColor = Colors.Red;
                break;

            case IndicatorColorEnum.Blue:
                centerColor = Color.Parse("#FFC1C1FC");
                edgeColor = Colors.Blue;
                break;
            case IndicatorColorEnum.Yellow:
                centerColor = Color.Parse("#FFFFFFA5");
                edgeColor = Color.Parse("#FFC3C315");
                break;
            case IndicatorColorEnum.Orange:
                centerColor = Color.Parse("#FFFFE9C1");
                edgeColor = Colors.Orange;
                break;
            case IndicatorColorEnum.Purple:
                centerColor = Color.Parse("#FFFC8EFC");
                edgeColor = Colors.Purple;
                break;
            case IndicatorColorEnum.Custom:
            default:
                centerColor = null;
                edgeColor = null;
                break;
        }

        if (centerColor != null && edgeColor != null)
        {
            return new RadialGradientBrush
            {
                GradientStops = [new GradientStop(centerColor.Value, 0), new GradientStop(edgeColor.Value, 1)],
                GradientOrigin = new RelativePoint(0.15, 0.15, RelativeUnit.Relative)
            };
        }

        return SelectedBrush;
    }

    private void SetShape()
    {
        if (_contentPresenter != null)
        {
            // Implementation for setting the shape
            _shape = IndicatorShape switch
            {
                IndicatorShapeEnum.Ellipse => new Ellipse(),
                IndicatorShapeEnum.Rectangle => new Rectangle(),
                IndicatorShapeEnum.RoundedRectangle => new Rectangle(),
                _ => _shape
            };
            SetShapeSize();
            SetShapeCorners();
            SetShapeBrush();
            _contentPresenter.Content = _shape;
        }
    }

    private void SetShapeBrush()
    {
        if (_shape != null)
        {
            _shape.Fill = IsSelected ? MakeIndicatorBrush() : UnselectedBrush;
            _shape.Stroke = IsSelected ? MakeBorderBrush() : UnselectedBorderBrush;
            _shape.StrokeThickness = BorderWidth;
            _shape.Stretch = Stretch.Fill;
        }
    }

    private void SetShapeCorners()
    {
        if (_shape is Rectangle rectangle)
        {
            if (IndicatorShape == IndicatorShapeEnum.RoundedRectangle)
            {
                rectangle.RadiusX = CornerRadiusX;
                rectangle.RadiusY = CornerRadiusY;
            }
            else
            {
                rectangle.RadiusX = 0d;
                rectangle.RadiusY = 0d;
            }
        }
    }

    private void SetShapeSize()
    {
        if (_shape != null)
        {
            _shape.Width = Width;
            _shape.Height = Height;
        }
    }

    #endregion
}