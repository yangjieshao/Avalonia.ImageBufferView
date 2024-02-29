using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.IO;

namespace Avalonia.ImageBufferView;

public partial class ImageBufferView : Control
{
    static ImageBufferView()
    {
        AffectsRender<ImageBufferView>(BitmapProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<ImageBufferView>(BitmapProperty, StretchProperty, StretchDirectionProperty);
        AffectsArrange<ImageBufferView>(BitmapProperty, StretchProperty, StretchDirectionProperty);

        ImageBufferProperty.Changed.AddClassHandler<ImageBufferView>(ImageBufferChanged);
        BitmapProperty.Changed.AddClassHandler<ImageBufferView>(BitmapChanged);
    }

    public static readonly AvaloniaProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImageBufferView, Stretch>(
            nameof(Stretch));

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty)!;
        set => SetValue(StretchProperty, value);
    }

    public static readonly AvaloniaProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<ImageBufferView, StretchDirection>(
            nameof(StretchDirection), StretchDirection.Both);

    public StretchDirection StretchDirection
    {
        get => (StretchDirection)GetValue(StretchDirectionProperty)!;
        set => SetValue(StretchDirectionProperty, value);
    }

    public static readonly AvaloniaProperty<ArraySegment<byte>?> ImageBufferProperty =
        AvaloniaProperty.Register<ImageBufferView, ArraySegment<byte>?>(
            nameof(ImageBuffer));

    /// <summary>
    /// 要渲染的图片的流
    /// </summary>
    public ArraySegment<byte>? ImageBuffer
    {
        get => (ArraySegment<byte>?)GetValue(ImageBufferProperty);
        set => SetValue(ImageBufferProperty, value);
    }

    private static void ImageBufferChanged(ImageBufferView sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is ArraySegment<byte> buffer
            && buffer.Array != null
                && buffer.Array.Length > 0)
        {
            if (!sender._canUpdataBitmap)
            {
                return;
            }
            sender._canUpdataBitmap = false;
            var oldBitmap = sender.Bitmap;
            using var stream = new MemoryStream(buffer.Array);
            sender.Bitmap = new Bitmap(stream);
            oldBitmap?.Dispose();
        }
        else
        {
            var oldBitmap = sender.Bitmap;
            sender.Bitmap = default;
            oldBitmap?.Dispose();
            sender._canUpdataBitmap = true;
        }
    }

    public static readonly AvaloniaProperty<Bitmap?> BitmapProperty =
     AvaloniaProperty.Register<ImageBufferView, Bitmap?>(nameof(Bitmap));

    /// <summary>
    /// 实际渲染的画面
    /// </summary>
    public Bitmap? Bitmap
    {
        get => (Bitmap?)GetValue(BitmapProperty)!;
        set => SetValue(BitmapProperty, value);
    }

    private static void BitmapChanged(ImageBufferView sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Bitmap bitmap)
        {
            sender.SourceSize = bitmap.Size;
        }
        else
        {
            sender.SourceSize = sender.RenderSize;
        }
    }

    /// <summary>
    /// 需要更新渲染画面
    /// </summary>
    private bool _canUpdataBitmap = true;

    public Size RenderSize => Bounds.Size;

    public Size SourceSize { get; private set; }

    protected override Size MeasureOverride(Size constraint)
    {
        return Bitmap is not null ? Stretch.CalculateSize(constraint, SourceSize, StretchDirection) : default;
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        return Bitmap is not null ? Stretch.CalculateSize(arrangeSize, SourceSize) : default;
    }

    public override void Render(DrawingContext drawingContext)
    {
        if (Bitmap is not null)
        {
            var sourceSize = SourceSize;

            var viewPort = new Rect(RenderSize);
            var scale = Stretch.CalculateScaling(RenderSize, sourceSize, StretchDirection);
            var scaledSize = sourceSize * scale;
            var destRect = viewPort
                .CenterRect(new Rect(scaledSize))
                .Intersect(viewPort);
            var sourceRect = new Rect(sourceSize)
                .CenterRect(new Rect(destRect.Size / scale));

            if (Bitmap is not null)
            {
                drawingContext.DrawImage(Bitmap, sourceRect, destRect);
            }
            _canUpdataBitmap = true;
        }

        base.Render(drawingContext);
    }
}