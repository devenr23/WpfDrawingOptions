using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Common.Shared;
using SkiaSharp;
using System;
using System.IO;

namespace AvaloniaDrawingOptions;

/// <summary>
/// Direct Skia via offscreen SKSurface -> image blit. This is a fallback that
/// renders with SkiaSharp into an in-memory PNG and draws it as an Avalonia image.
/// It's not as efficient as a true SKCanvas hook but demonstrates Skia rendering.
/// </summary>
public class MySkiaCanvas : Control
{
    private readonly Random _random = new();

    public void Draw()
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext drawingContext)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var w = Math.Max(1, (int)Bounds.Width);
        var h = Math.Max(1, (int)Bounds.Height);

        using var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul));
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
                IsStroke = true,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            var x1 = _random.Next(w);
            var y1 = _random.Next(h);
            var x2 = _random.Next(w);
            var y2 = _random.Next(h);

            canvas.DrawLine(x1, y1, x2, y2, paint);
        }

        using var img = surface.Snapshot();
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Seek(0, SeekOrigin.Begin);

        using var avaloniaBitmap = new Bitmap(ms);
        var dstRect = new Rect(0, 0, Bounds.Width, Bounds.Height);
        drawingContext.DrawImage(avaloniaBitmap, dstRect);

        FrameRateMonitor.Instance.DrawCalled();
    }
}
