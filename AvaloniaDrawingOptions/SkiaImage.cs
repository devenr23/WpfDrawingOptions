using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Threading;

namespace AvaloniaDrawingOptions;

public static class TestConstants
{
    // When you change this, don't forget to also change the number in index.html (they aren't linked)
    // Then save, clean, and rebuild all projects
    public const int NumberOfLines = 5_000;
}


public class SkiaImage : Control
{
    private readonly Color _background1 = Color.FromArgb(200, 200, 0, 0);
    private readonly Color _background2 = Color.FromArgb(200, 0, 200, 200); //GreenYellow;
    private readonly Color _background3 = Color.FromArgb(200, 0, 0, 200); //DeepSkyBlue;
    private bool _cancelImageTask = false;
    private readonly Random _random = new();

    // Timer disposable returned by DispatcherTimer.Run
    private IDisposable? _renderTimer;

    static SkiaImage()
    {

    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        DrawCanvas(context, (int)Bounds.Width, (int)Bounds.Height, _background3);
        
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Start timer to invalidate visuals at ~30 FPS.
        // Keep the IDisposable so we can stop it on detach.

        _renderTimer ??= DispatcherTimer.Run(

            () =>
            {
                // Keep running until disposed
                InvalidateVisual();
                return true;
            }, TimeSpan.FromMilliseconds(1000.0 / 120.0));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        // Stop the timer when control is removed
        _renderTimer?.Dispose();
        _renderTimer = null;
    }

    private void DrawCanvas(DrawingContext context, int width, int height, Color background)
    {
        var avaloniaColor = Color.FromArgb(background.A, background.R, background.G, background.B);
        context.DrawRectangle(new SolidColorBrush(avaloniaColor), null, new Rect(0, 0, width, height));

        for (int i = 0; i < TestConstants.NumberOfLines; i++)
        {
            var strokeColor = Color.FromArgb(
                a: (byte)_random.Next(255),
                r: (byte)_random.Next(255),
                g: (byte)_random.Next(255),
                b: (byte)_random.Next(255));

            var StrokeWidth = _random.Next(1, 10);
            var pen = new Pen(new SolidColorBrush(strokeColor), StrokeWidth);

            context.DrawLine(pen,
                    new Point(_random.Next(width), _random.Next(height)),
                    new Point(_random.Next(width), _random.Next(height))
                );
        }

        // FrameRateMonitor.Instance.DrawCalled();
    }

    //private void DrawTask(DrawingContext context)
    //{
    //    const double maxFps = 30;
    //    double minFramePeriod = 1000.0 / maxFps;

    //    Stopwatch stopwatch = Stopwatch.StartNew();
    //    while (!_cancelImageTask)
    //    {
    //        int width = (int)Bounds.Width;
    //        int height = (int)Bounds.Height;

    //        DrawCanvas(context, width, height, _background3);

    //        // FPS limiter
    //        double msToWait = minFramePeriod - stopwatch.ElapsedMilliseconds;
    //        if (msToWait > 0)
    //            Thread.Sleep((int)msToWait);
    //        stopwatch.Restart();
    //    }
    //}

}