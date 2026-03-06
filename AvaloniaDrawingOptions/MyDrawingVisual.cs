using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace AvaloniaDrawingOptions;

/// <summary>
/// Avalonia has no DrawingVisual concept — all drawing goes through Render().
/// Draw() simply schedules a re-render, making this equivalent to MyCanvas.
/// The "Drawing Visual" slot exists here to show that the WPF pattern has no
/// distinct Avalonia counterpart.
/// </summary>
public class MyDrawingVisual : Control
{
    private readonly Random _random = new();

    public void Draw()
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        for (var i = 0; i < TestConstants.NumberOfLines; i++)
        {
            var color = Color.FromArgb(
                (byte)_random.Next(255),
                (byte)_random.Next(255),
                (byte)_random.Next(255),
                (byte)_random.Next(255));

            var pen = new Pen(new SolidColorBrush(color), _random.Next(1, 10));

            context.DrawLine(pen,
                new Point(_random.Next((int)Bounds.Width), _random.Next((int)Bounds.Height)),
                new Point(_random.Next((int)Bounds.Width), _random.Next((int)Bounds.Height)));
        }

        FrameRateMonitor.Instance.DrawCalled();
    }
}
