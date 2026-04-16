// AnimatedLinearBrush.cs
//  Andrew Baylis
//  Created: 15/02/2026

#region using

using System.Reflection;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

#endregion

namespace AJBAvalonia;

public enum AnimationDirection
{
    Forward,
    Backward,
    PingPong
}

public class AnimatedLinearBrushExtension : AvaloniaObject, IDisposable
{
    #region Avalonia Properties

    public static readonly DirectProperty<AnimatedLinearBrushExtension, int> DelayStartProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, int>(nameof(DelayStart), o => o.DelayStart, (o, v) => o.DelayStart = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, AnimationDirection> DirectionProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, AnimationDirection>(nameof(Direction), o => o.Direction, (o, v) => o.Direction = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, TimeSpan>(nameof(Duration), o => o.Duration, (o, v) => o.Duration = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, Easing> EasingProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, Easing>(nameof(Easing), o => o.Easing, (o, v) => o.Easing = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, Color?> EndColorProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, Color?>(nameof(EndColor), o => o.EndColor, (o, v) => o.EndColor = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, RelativePoint> EndPointProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, RelativePoint>(nameof(EndPoint), o => o.EndPoint, (o, v) => o.EndPoint = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, Color?> MidColorProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, Color?>(nameof(MidColor), o => o.MidColor, (o, v) => o.MidColor = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, double> MidColorWidthProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, double>(nameof(MidColorWidth), o => o.MidColorWidth, (o, v) => o.MidColorWidth = v);
    public static readonly DirectProperty<AnimatedLinearBrushExtension, IBrush> ModifiedBrushProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, IBrush>(nameof(ModifiedBrush), o => o.ModifiedBrush);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, Orientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, Color> StartColorProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, Color>(nameof(StartColor), o => o.StartColor, (o, v) => o.StartColor = v);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, RelativePoint> StartPointProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, RelativePoint>(nameof(StartPoint), o => o.StartPoint);

    public static readonly DirectProperty<AnimatedLinearBrushExtension, int> StepMillisecondsProperty =
        AvaloniaProperty.RegisterDirect<AnimatedLinearBrushExtension, int>(nameof(StepMilliseconds), o => o.StepMilliseconds, (o, v) => o.StepMilliseconds = v);

    #endregion

    #region Fields

    PropertyInfo? _hasObserversPropertyInfo;

    public AnimatedLinearBrushExtension()
    {
        _hasObserversPropertyInfo = ModifiedBrushProperty.Changed.GetType().GetProperty("HasObservers",BindingFlags.Public|BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private CancellationTokenSource? _animationCancellationTokenSource = new();

    private Task? _animationTask;

    private int _delayStart;

    private AnimationDirection _direction = AnimationDirection.Forward;

    private TimeSpan _duration = TimeSpan.FromSeconds(1);

    private Easing _easing = new LinearEasing();

    private Color? _endColor;

    private RelativePoint _endPoint = new(0, 1, RelativeUnit.Relative);

    private Color? _midColor;

    private double _midColorWidth = -1;


    WeakReference<IBinding>? _bindingRef;

    private IBrush _modifiedBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops = [new GradientStop(Colors.Red, 0), new GradientStop(Colors.Red, 1)]
    };

    private Orientation _orientation = Orientation.Vertical;

    private Color _startColor = Colors.Green;

    private RelativePoint _startPoint = new(0, 0, RelativeUnit.Relative);

    private int _stepMilliseconds = 30;

    #endregion

    #region Properties

    public int DelayStart
    {
        get => _delayStart;
        set => SetAndRaise(DelayStartProperty, ref _delayStart, value);
    }

    public AnimationDirection Direction
    {
        get => _direction;
        set => SetAndRaise(DirectionProperty, ref _direction, value);
    }

    public TimeSpan Duration
    {
        get => _duration;
        set => SetAndRaise(DurationProperty, ref _duration, value);
    }

    public Easing Easing
    {
        get => _easing;
        set => SetAndRaise(EasingProperty, ref _easing, value);
    }

    public Color? EndColor
    {
        get => _endColor ?? _startColor;
        set => SetAndRaise(EndColorProperty, ref _endColor, value);
    }

    public RelativePoint EndPoint
    {
        get => _endPoint;
        set => SetAndRaise(EndPointProperty, ref _endPoint, value);
    }

    public Color? MidColor
    {
        get => _midColor ?? _startColor;
        set => SetAndRaise(MidColorProperty, ref _midColor, value);
    }

    public double MidColorWidth
    {
        get => _midColorWidth;
        set
        {
            if (value is < 0 or > 1)
            {
                value = -1;
            }

            SetAndRaise(MidColorWidthProperty, ref _midColorWidth, value);
        }
    }

    public IBrush ModifiedBrush
    {
        get => _modifiedBrush;
        private set => SetAndRaise(ModifiedBrushProperty, ref _modifiedBrush, value);
    }

    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            SetAndRaise(OrientationProperty, ref _orientation, value);
            if (value == Orientation.Horizontal)
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative);
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative);
            }
            else
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative);
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative);
            }
        }
    }

    public Color StartColor
    {
        get => _startColor;
        set => SetAndRaise(StartColorProperty, ref _startColor, value);
    }

    public RelativePoint StartPoint
    {
        get => _startPoint;
        private set => SetAndRaise(StartPointProperty, ref _startPoint, value);
    }

    public int StepMilliseconds
    {
        get => _stepMilliseconds;
        set
        {
            if (value is >= 10 and < 1000)
            {
                SetAndRaise(StepMillisecondsProperty, ref _stepMilliseconds, value);
            }
        }
    }

    private IDisposable? _timerDisposable;
    #endregion

    #region Public Methods

    public IBinding GetBinding()
    {
        InitialiseAnimation();
        UpdateBrush(0);
        Run();
        var b= new Binding(nameof(ModifiedBrush)) {Source = this};
        _bindingRef = new WeakReference<IBinding>(b);
        return b;
    }

    public IBinding ProvideValue(IServiceProvider serviceProvider)
    {
        return GetBinding();
    }

    #endregion

    #region Protected Methods

    private double CalculatePosition()
    {
       return (Easing.Ease(progress) - pMin) / (pMax - pMin);
    }

    private double pMin;
    private double pMax;
    private double progress;
    private double progressInc;

    //protected void Run()
    //{
    //    _timerDisposable?.Dispose(); //stops any running timer
    //    _timerDisposable = null;

    //    InitialiseAnimation();

    //    DispatcherTimer.Run(DoUpdateBrush, TimeSpan.FromMilliseconds(StepMilliseconds));
    //}

    private void InitialiseAnimation()
    {
        //initialise variables for the animation
        pMin = Easing.Ease(0);
        pMax = Easing.Ease(1);
        if (pMax < pMin)
        {
            (pMin, pMax) = (pMax, pMin);
        }

        progress = 0d;
        var steps = (int)(Duration.TotalMilliseconds / StepMilliseconds);
        progressInc = 1.0 / steps;
    }

    bool BindingIsAlive()
    {
        //var b= _bindingRef?.TryGetTarget(out _) == true;

        //return b;
        return _hasObserversPropertyInfo?.GetValue(ModifiedBrushProperty.Changed) is bool hasObservers && hasObservers;
    }

    private bool DoUpdateBrush()
    {
        var p = CalculatePosition();
        UpdateBrush(p);
        progress += progressInc;
        if (progress < 0)
        {
            if (Direction == AnimationDirection.PingPong)
            {
                progress = 0;
                progressInc = -progressInc;
            }
            else
            {
                progress = 1;
            }
        }
        else if (progress > 1)
        {
            if (Direction == AnimationDirection.PingPong)
            {
                progress = 1;
                progressInc = -progressInc;
            }
            else
            {
                progress = 0;
            }
        }
        return BindingIsAlive(); //keep the timer running as long as the binding is alive
    }

    protected void Run()
    {
        StopAnimation();
        _animationCancellationTokenSource = new CancellationTokenSource();
        _animationTask = Task.Run(async () => { await AnimateBrush(_animationCancellationTokenSource.Token); }).ContinueWith(t =>
        {
            _animationTask = null;
            _animationCancellationTokenSource?.Dispose();
            _animationCancellationTokenSource = null;
        });
    }

    #endregion

    #region Private Methods

    private async Task AnimateBrush(CancellationToken cancellationToken)
    {
        if (DelayStart > 0)
        {
            await Task.Delay(DelayStart, cancellationToken);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var p = CalculatePosition();
            UpdateBrush(p);
            progress += progressInc;
            if (progress < 0)
            {
                if (Direction == AnimationDirection.PingPong)
                {
                    progress = 0;
                    progressInc = -progressInc;
                }
                else
                {
                    progress = 1;
                }

                if (DelayStart > 0)
                {
                    await Task.Delay(DelayStart, cancellationToken);
                }
            }
            else if (progress > 1)
            {
                if (Direction == AnimationDirection.PingPong)
                {
                    progress = 1;
                    progressInc = -progressInc;
                }
                else
                {
                    progress = 0;
                }

                if (DelayStart > 0)
                {
                    await Task.Delay(DelayStart, cancellationToken);
                }
            }

            await Task.Delay(StepMilliseconds, cancellationToken);

        }
    }

    private void StopAnimation()
    {
        _animationCancellationTokenSource?.Cancel();
        _animationTask?.Wait();
        _animationTask = null;
        _animationCancellationTokenSource?.Dispose();
        _animationCancellationTokenSource = null;
    }

    private void UpdateBrush(double p)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (MidColorWidth > 0)
            {
                ModifiedBrush = new LinearGradientBrush
                {
                    StartPoint = _startPoint,
                    EndPoint = _endPoint,
                    GradientStops =
                    [
                        new GradientStop(StartColor, 0),
                        new GradientStop(StartColor, p - MidColorWidth),
                        new GradientStop(MidColor ?? StartColor, p),
                        new GradientStop(EndColor ?? StartColor, p + MidColorWidth),
                        new GradientStop(EndColor ?? StartColor, 1)
                    ]
                };
            }
            else
            {
                ModifiedBrush = new LinearGradientBrush
                {
                    StartPoint = _startPoint,
                    EndPoint = _endPoint,
                    GradientStops =
                    [
                        new GradientStop(StartColor, 0),
                        new GradientStop(MidColor ?? StartColor, p),
                        new GradientStop(EndColor ?? StartColor, 1)
                    ]
                };
            }
        });
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        StopAnimation();
    }

    #endregion
}