using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Common.Shared;
using SkiaSharp;
using System;

namespace AvaloniaDrawingOptions;

/// <summary>
/// Skia rendering directly into a WriteableBitmap pixel buffer — no PNG encode/decode.
/// SKSurface.Create wraps the WriteableBitmap's locked memory pointer, so Skia writes
/// straight into the bitmap. Sits between Skia (Bitmap) and Direct Skia in overhead:
/// no PNG cost, but still an extra CPU blit to get pixels onto the GPU.
/// </summary>
public class MySkiaWriteableBitmapCanvas : Control
{
    private readonly Random _random = new();
    private WriteableBitmap? _bitmap;
    private int _bitmapW, _bitmapH;

    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var w = Math.Max(1, (int)Bounds.Width);
        var h = Math.Max(1, (int)Bounds.Height);

        if (_bitmap is null || _bitmapW != w || _bitmapH != h)
        {
            _bitmap?.Dispose();
            _bitmap = new WriteableBitmap(
                new PixelSize(w, h),
                new Vector(96, 96),
                PixelFormats.Bgra8888,
                AlphaFormat.Premul);
            _bitmapW = w;
            _bitmapH = h;
        }

        using (var fb = _bitmap.Lock())
        {
            using var surface = SKSurface.Create(
                new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul),
                fb.Address,
                fb.RowBytes);

            if (surface is null) return;

            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            for (int i = 0; i < TestConstants.NumberOfLines; i++)
            {
                var color = new SKColor(
                    (byte)_random.Next(255),
                    (byte)_random.Next(255),
                    (byte)_random.Next(255),
                    (byte)_random.Next(255));

                using var paint = new SKPaint
                {
                    Color = color,
                    StrokeWidth = _random.Next(1, 10),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };

                canvas.DrawLine(
                    _random.Next(w), _random.Next(h),
                    _random.Next(w), _random.Next(h),
                    paint);
            }

            canvas.Flush();
        }

        context.DrawImage(_bitmap, new Rect(0, 0, w, h));
        FrameRateMonitor.Instance.DrawCalled();
    }
}
