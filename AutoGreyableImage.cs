// AutoGreyableImage.cs
// Andrew Baylis
//  Created: 20/01/2024

#region using

using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using SkiaSharp;

#endregion

namespace AJBAvalonia;

public class AutoGreyableImage : Control
{
    #region Fields

    public static readonly StyledProperty<double> GreyOpacityProperty = AvaloniaProperty.Register<AutoGreyableImage, double>(nameof(GreyOpacity), 1.0f);

    /// <summary>
    ///     Defines the <see cref="Source" /> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> SourceProperty = AvaloniaProperty.Register<AutoGreyableImage, IImage?>(nameof(Source));

    /// <summary>
    ///     Defines the <see cref="StretchDirection" /> property.
    /// </summary>
    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<AutoGreyableImage, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

    /// <summary>
    ///     Defines the <see cref="Stretch" /> property.
    /// </summary>
    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<AutoGreyableImage, Stretch>(nameof(Stretch), Stretch.Uniform);

    private readonly float[] greyScale =
    {
        0.2126f,
        0.7152f,
        0.0722f,
        0,
        0, // red channel weights
        0.2126f,
        0.7152f,
        0.0722f,
        0,
        0, // green channel weights
        0.2126f,
        0.7152f,
        0.0722f,
        0,
        0, // blue channel weights
        0,
        0,
        0,
        1,
        0 // alpha channel weights
    };

    private Bitmap? _greyImage;

    #endregion

    static AutoGreyableImage()
    {
        AffectsRender<Image>(SourceProperty, StretchProperty, StretchDirectionProperty, IsEnabledProperty, IsEffectivelyEnabledProperty);
        AffectsMeasure<Image>(SourceProperty, StretchProperty, StretchDirectionProperty);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<AutoGreyableImage>(AutomationControlType.Image);
    }

    public AutoGreyableImage()
    {
        Loaded += AutoGreyableImage_Loaded;
    }

    #region Properties

    public double GreyOpacity
    {
        get => GetValue(GreyOpacityProperty);
        set => SetValue(GreyOpacityProperty, value);
    }

    /// <summary>
    ///     Gets or sets the image that will be displayed.
    /// </summary>
    [Content]
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value controlling how the image will be stretched.
    /// </summary>
    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value controlling in what direction the image will be stretched.
    /// </summary>
    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    /// <inheritdoc />
    protected override bool BypassFlowDirectionPolicies => true;

    #endregion

    #region Override Methods

    /// <summary>
    ///     Renders the control.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public override void Render(DrawingContext context)
    {
        var source = IsEffectivelyEnabled ? Source : _greyImage;

        if (source != null && Bounds is {Width: > 0, Height: > 0})
        {
            var viewPort = new Rect(Bounds.Size);
            var sourceSize = source.Size;

            var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
            var scaledSize = sourceSize * scale;
            var destRect = viewPort.CenterRect(new Rect(scaledSize)).Intersect(viewPort);
            var sourceRect = new Rect(sourceSize).CenterRect(new Rect(destRect.Size / scale));

            context.DrawImage(source, sourceRect, destRect);
        }
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = Source;

        if (source != null)
        {
            var sourceSize = source.Size;
            var result = Stretch.CalculateSize(finalSize, sourceSize);
            return result;
        }

        return new Size();
    }

    /// <summary>
    ///     Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var source = Source;
        var result = new Size();

        if (source != null)
        {
            result = Stretch.CalculateSize(availableSize, source.Size, StretchDirection);
        }

        return result;
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new ImageAutomationPeer(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsEnabledProperty || change.Property == IsEffectivelyEnabledProperty)
        {
            InvalidateVisual();
        }

        if (change.Property == SourceProperty || change.Property == GreyOpacityProperty)
        {
            SetUpGreyImage();
            InvalidateArrange();
        }
    }

    #endregion

    #region Protected Methods

    protected Bitmap? MakeGreyScaleImage(Bitmap autoGreyScaleImg, float greyOpacity = 1.0f)
    {
        try
        {
            var matrix = new float[20];
            Array.Copy(greyScale, matrix, 20);
            matrix[18] = greyOpacity;
            using var ms = new MemoryStream();
            autoGreyScaleImg.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            using var bitmap = SKBitmap.Decode(ms);
            var info = new SKImageInfo((int) autoGreyScaleImg.Size.Width, (int) autoGreyScaleImg.Size.Height);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;

            using var paint = new SKPaint();

            // Define a grayscale color filter to apply to the image

            paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrix);

            // redraw the image using the color filter
            canvas.DrawBitmap(bitmap, 0, 0, paint);
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var memoryStream = new MemoryStream(data.ToArray());
            var bm = new Bitmap(memoryStream);
            return bm;
        }
        catch (Exception)
        {
            // nothing
        }

        return null;
    }

    #endregion

    #region Private Methods

    private void AutoGreyableImage_Loaded(object? sender, RoutedEventArgs e)
    {
        SetUpGreyImage();
    }

    private void SetUpGreyImage()
    {
        if (Source is Bitmap image)
        {
            _greyImage = MakeGreyScaleImage(image, (float) GreyOpacity);
        }
        else
        {
            _greyImage = null;
        }
    }

    #endregion
}