// AnimatedBrushExtension.cs
//  Andrew Baylis
//  Created: 16/02/2026

#region using

using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#endregion

namespace AJBAvalonia;

public enum AnimationDirection
{
    Forward,
    Backward,
    PingPong
}

public class AnimatedBrushExtension : INotifyPropertyChanged, IDisposable
{
    #region Fields

    private CancellationTokenSource? _animationCancellationTokenSource;

    private Task? _animationTask;

    private int _delayStart;

    private AnimationDirection _direction = AnimationDirection.Forward;

    private TimeSpan _duration = TimeSpan.FromSeconds(1);

    private Easing _easing = new LinearEasing();

    private Color? _endColor;

    private RelativePoint _endPoint = new(0, 1, RelativeUnit.Relative);

    private Color? _midColor;

    private double _midColorWidth = -1;

    private IBrush _modifiedBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops = [new GradientStop(Colors.Red, 0), new GradientStop(Colors.Red, 1)]
    };

    private Orientation _orientation = Orientation.Vertical;
    private double _progress;
    private double _progressInc;
    private double _progressMax;

    private double _progressMin;

    private Color _startColor = Colors.Green;

    private RelativePoint _startPoint = new(0, 0, RelativeUnit.Relative);

    private int _stepMilliseconds = 30;

    #endregion

    #region Properties

    public int DelayStart
    {
        get => _delayStart;
        set => SetField(ref _delayStart, value);
    }

    public AnimationDirection Direction
    {
        get => _direction;
        set => SetField(ref _direction, value);
    }

    public TimeSpan Duration
    {
        get => _duration;
        set => SetField(ref _duration, value);
    }

    public Easing Easing
    {
        get => _easing;
        set => SetField(ref _easing, value);
    }

    public Color? EndColor
    {
        get => _endColor ?? _startColor;
        set => SetField(ref _endColor, value);
    }

    public RelativePoint EndPoint
    {
        get => _endPoint;
        set => SetField(ref _endPoint, value);
    }

    public Color? MidColor
    {
        get => _midColor ?? _startColor;
        set => SetField(ref _midColor, value);
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

            SetField(ref _midColorWidth, value);
        }
    }

    public IBrush ModifiedBrush
    {
        get => _modifiedBrush;
        private set => SetField(ref _modifiedBrush, value);
    }

    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            SetField(ref _orientation, value);
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
        set => SetField(ref _startColor, value);
    }

    public RelativePoint StartPoint
    {
        get => _startPoint;
        set => SetField(ref _startPoint, value);
    }

    public int StepMilliseconds
    {
        get => _stepMilliseconds;
        set
        {
            if (value is >= 10 and < 1000)
            {
                SetField(ref _stepMilliseconds, value);
            }
        }
    }

    #endregion

    #region Public Methods

    public BindingBase GetBinding()
    {
        InitialiseAnimation();
        UpdateBrush(0);
        Run();
        return new Binding(nameof(ModifiedBrush)) { Source = this };
    }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return GetBinding();
    }

    #endregion

    #region Protected Methods

    protected async Task AnimateBrush(CancellationToken cancellationToken)
    {
        //wait for first binding to be made to allow for UI setup to occur, if the animation starts before then it can cause issues with the brush not updating correctly
        while (!cancellationToken.IsCancellationRequested && !CheckInvocationList())
        {
            await Task.Delay(10, cancellationToken); //allows for UI setup to occur
        }

        if (DelayStart > 0)
        {
            await Task.Delay(DelayStart, cancellationToken);
        }

        while (!cancellationToken.IsCancellationRequested && CheckInvocationList())
        {
            var p = CalculatePosition();
            UpdateBrush(p);
            _progress += _progressInc;
            if (_progress < 0)
            {
                if (Direction == AnimationDirection.PingPong)
                {
                    _progress = 0;
                    _progressInc = -_progressInc;
                }
                else
                {
                    _progress = 1;
                }

                if (DelayStart > 0)
                {
                    await Task.Delay(DelayStart, cancellationToken);
                }
            }
            else if (_progress > 1)
            {
                if (Direction == AnimationDirection.PingPong)
                {
                    _progress = 1;
                    _progressInc = -_progressInc;
                }
                else
                {
                    _progress = 0;
                }

                if (DelayStart > 0)
                {
                    await Task.Delay(DelayStart, cancellationToken);
                }
            }

            await Task.Delay(StepMilliseconds, cancellationToken);
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void Run()
    {
        StopAnimation();
        _animationCancellationTokenSource = new CancellationTokenSource();
        _animationTask = Task.Run(async () => { await AnimateBrush(_animationCancellationTokenSource.Token); }).ContinueWith(_ =>
        {
            _animationCancellationTokenSource?.Dispose();
            _animationCancellationTokenSource = null;
        });
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region Private Methods

    private double CalculatePosition()
    {
        return (Easing.Ease(_progress) - _progressMin) / (_progressMax - _progressMin);
    }

    /// <summary>
    ///     used to determine if any controls are still using the brush, if not then the animation can stop to save resources
    /// </summary>
    /// <returns></returns>
    private bool CheckInvocationList()
    {
        return PropertyChanged != null && PropertyChanged.GetInvocationList().Length > 0;
    }

    private void InitialiseAnimation()
    {
        //initialise variables for the animation
        _progressMin = Easing.Ease(0);
        _progressMax = Easing.Ease(1);
        if (_progressMax < _progressMin)
        {
            (_progressMin, _progressMax) = (_progressMax, _progressMin);
        }

        _progress = 0d;
        var steps = (int)(Duration.TotalMilliseconds / StepMilliseconds);
        _progressInc = 1.0 / steps;
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
        GC.SuppressFinalize(this);
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion
}