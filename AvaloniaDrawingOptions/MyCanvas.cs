using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Common.Shared;
using System;

namespace AvaloniaDrawingOptions;

/// <summary>
/// Avalonia equivalent of the WPF MyCanvas — overrides Render() and draws directly
/// into the DrawingContext each frame (immediate mode).
/// </summary>
public class MyCanvas : Control
{
    private readonly Random _random = new();

    public override void Render(DrawingContext drawingContext)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        for (int i = 0; i < TestConstants.NumberOfLines; i++)
        {
            var color = Color.FromArgb(
                (byte)_random.Next(255),
                (byte)_random.Next(255),
                (byte)_random.Next(255),
                (byte)_random.Next(255));

            var point1 = new Point(_random.Next((int)Bounds.Width), _random.Next((int)Bounds.Height));
            var point2 = new Point(_random.Next((int)Bounds.Width), _random.Next((int)Bounds.Height));
            var pen = new Pen(new SolidColorBrush(color), _random.Next(1, 10));

            drawingContext.DrawLine(pen, point1, point2);
        }

        FrameRateMonitor.Instance.DrawCalled();
    }
}
