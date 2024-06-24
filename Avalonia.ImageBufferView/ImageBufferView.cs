using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;

namespace Avalonia.ImageBufferView;

public partial class ImageBufferView : Control
{
    static ImageBufferView()
    {
        AffectsRender<ImageBufferView>(BitmapProperty, StretchProperty, StretchDirectionProperty, DefaultBackgroundProperty);
        AffectsMeasure<ImageBufferView>(BitmapProperty, StretchProperty, StretchDirectionProperty, DefaultBackgroundProperty);
        AffectsArrange<ImageBufferView>(BitmapProperty, StretchProperty, StretchDirectionProperty, DefaultBackgroundProperty);

        ImageBufferProperty.Changed.AddClassHandler<ImageBufferView>(ImageBufferChanged);
        BitmapProperty.Changed.AddClassHandler<ImageBufferView>(BitmapChanged);
    }

    public static readonly AvaloniaProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImageBufferView, Stretch>(nameof(Stretch), Stretch.None);

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty)!;
        set => SetValue(StretchProperty, value);
    }

    public static readonly AvaloniaProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<ImageBufferView, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

    public StretchDirection StretchDirection
    {
        get => (StretchDirection)GetValue(StretchDirectionProperty)!;
        set => SetValue(StretchDirectionProperty, value);
    }

    public static readonly AvaloniaProperty<ArraySegment<byte>?> ImageBufferProperty =
        AvaloniaProperty.Register<ImageBufferView, ArraySegment<byte>?>(nameof(ImageBuffer));

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
        //Debug.WriteLine("ImageBufferChanged");
        if (e.NewValue is ArraySegment<byte> buffer
            && buffer.Array != null
                && buffer.Array.Length > 0)
        {
            if (!sender._canUpdataBitmap)
            {
                return;
            }
            sender._canUpdataBitmap = false;
            using var stream = new MemoryStream(buffer.Array);
            var oldBitmap = sender.Bitmap;
            sender.Bitmap = new Bitmap(stream);
            oldBitmap?.Dispose();
        }
        else
        {
            var oldBitmap = sender.Bitmap;
            sender.Bitmap = default;
            oldBitmap?.Dispose();
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

    public static readonly AvaloniaProperty<IBrush?> DefaultBackgroundProperty =
     AvaloniaProperty.Register<ImageBufferView, IBrush?>(nameof(DefaultBackground));

    public IBrush? DefaultBackground
    {
        get
        {
            return (IBrush?)GetValue(DefaultBackgroundProperty);
        }
        set
        {
            SetValue(DefaultBackgroundProperty, value);
        }
    }

    /// <summary>
    /// 需要更新渲染画面
    /// </summary>
    private bool _canUpdataBitmap = true;

    public Size RenderSize => Bounds.Size;

    public Size SourceSize { get; private set; }

    protected override Size MeasureOverride(Size availableSize)
    {
        return Bitmap is not null ? Stretch.CalculateSize(availableSize, SourceSize, StretchDirection)
                                  : base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return Bitmap is not null ? Stretch.CalculateSize(finalSize, SourceSize)
                                  : base.ArrangeOverride(finalSize);
    }

    public override void Render(DrawingContext drawingContext)
    {
        if (RenderSize.Width > 0.0
            && RenderSize.Height > 0.0)
        {
            if (Bitmap is { })
            {
                var rect = new Rect(RenderSize);
                var size = SourceSize;
                var vector = Stretch.CalculateScaling(RenderSize, size, StretchDirection);
                var size2 = size * vector;

                // Avalonia.Controls.Image 的裁剪逻辑:
                var destRect = rect.CenterRect(new Rect(size2)).Intersect(rect);
                var sourceRect = new Rect(size).CenterRect(new Rect(destRect.Size / vector));

                //// wpf 的裁剪逻辑：
                // var destRect = new Rect(size2).Intersect(rect);
                // var sourceRect = new Rect(destRect.Size / vector);

                if (Bitmap is not null)
                {
                    drawingContext.DrawImage(Bitmap, sourceRect, destRect);
                }

                //var sourceSize = SourceSize;

                //var viewPort = new Rect(RenderSize);
                //var scale = Stretch.CalculateScaling(RenderSize, sourceSize, StretchDirection);
                //var scaledSize = sourceSize * scale;
                //var destRect = viewPort
                //    .CenterRect(new Rect(scaledSize))
                //    .Intersect(viewPort);
                //var sourceRect = new Rect(sourceSize)
                //    .CenterRect(new Rect(destRect.Size / scale));

                //if (Bitmap is not null)
                //{
                //    drawingContext.DrawImage(Bitmap, sourceRect, destRect);
                //}
            }
            else if (DefaultBackground is { })
            {
                Size size = RenderSize;
                drawingContext.FillRectangle(DefaultBackground, new Rect(size));
            }
        }
        if (!_canUpdataBitmap)
        {
            _canUpdataBitmap = true;
        }

        base.Render(drawingContext);
    }
}