using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;

namespace Avalonia.ImageBufferView;

public partial class ImageBufferView : Control
{
    public static readonly AvaloniaProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImageBufferView, Stretch>(
            nameof(Stretch), Stretch.None);

    public Stretch Stretch
    {
        get => (Stretch)this.GetValue(StretchProperty)!;
        set => this.SetValue(StretchProperty, value);
    }

    public static readonly AvaloniaProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<ImageBufferView, StretchDirection>(
            nameof(StretchDirection), StretchDirection.Both);

    public StretchDirection StretchDirection
    {
        get => (StretchDirection)this.GetValue(StretchDirectionProperty)!;
        set => this.SetValue(StretchDirectionProperty, value);
    }

    static ImageBufferView()
    {
        AffectsRender<ImageBufferView>(ImageBufferProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<ImageBufferView>(ImageBufferProperty, StretchProperty, StretchDirectionProperty);
        AffectsArrange<ImageBufferView>(ImageBufferProperty, StretchProperty, StretchDirectionProperty);
    }

    public static readonly AvaloniaProperty<byte[]> ImageBufferProperty =
        AvaloniaProperty.Register<ImageBufferView, byte[]>(
            nameof(ImageBuffer), coerce: (sender, e) =>
            {
                if (sender is ImageBufferView control)
                {
                    var oldBitmap = control.Bitmap;
                    if (e is byte[] buffer
                    && buffer.Length > 0)
                    {
                        using MemoryStream stream = new (buffer);
                        control.Bitmap = new Bitmap(stream);
                        control.SourceSize = control.Bitmap.Size;
                    }
                    else
                    {
                        control.Bitmap = null;
                        control.SourceSize = control.RenderSize;
                    }
                    oldBitmap?.Dispose();
                }
                return e;
            });

    /// <summary>
    /// 要渲染的图片的流
    /// </summary>
    public byte[]? ImageBuffer
    {
        get => (byte[]?)this.GetValue(ImageBufferProperty);
        set => this.SetValue(ImageBufferProperty, value);
    }

    /// <summary>
    /// 实际渲染的画面
    /// </summary>
    public Bitmap? Bitmap { get; private set; }

    public Size RenderSize => this.Bounds.Size;

    public Size SourceSize { get; private set; }

    protected override Size MeasureOverride(Size constraint)
    {
        if (Bitmap is { })
        {
            return Stretch.CalculateSize(constraint, SourceSize, StretchDirection);
        }
        else
        {
            return default;
        }
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (Bitmap is { })
        {
            return Stretch.CalculateSize(arrangeSize, SourceSize);
        }
        else
        {
            return default;
        }
    }

    public override void Render(DrawingContext drawingContext)
    {
        if (Bitmap is { })
        {
            Size sourceSize = SourceSize;

            var viewPort = new Rect(RenderSize);
            var scale = Stretch.CalculateScaling(RenderSize, sourceSize, StretchDirection);
            var scaledSize = sourceSize * scale;
            var destRect = viewPort
                .CenterRect(new Rect(scaledSize))
                .Intersect(viewPort);
            var sourceRect = new Rect(sourceSize)
                .CenterRect(new Rect(destRect.Size / scale));

            if (Bitmap is { })
            {
                drawingContext.DrawImage(Bitmap, sourceRect, destRect);
            }
        }
        else
        {
            base.Render(drawingContext);
        }
    }
}