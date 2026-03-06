using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Common.Shared;
using System;

namespace AvaloniaDrawingOptions;

/// <summary>
/// Avalonia equivalent of the WPF MyStreamGeometry — builds a StreamGeometry once
/// and redraws it each frame with a new random pen color.
/// </summary>
public class MyStreamGeometry : Control
{
    private readonly Random _random = new();
    private StreamGeometry? _staticGeometry;
    private Size _lastSize;

    public StreamGeometry MakeConnectedGeometry()
    {
        var geometry = new StreamGeometry();
        using var context = geometry.Open();

        var startPoint = new Point(_random.Next((int)Bounds.Width), _random.Next((int)Bounds.Height));
        context.BeginFigure(startPoint, isFilled: false);

        for (int i = 0; i < TestConstants.NumberOfLines; i++)
        {
            var endPoint = new Point(_random.Next((int)Bounds.Width), _random.Next((int)Bounds.Height));
            context.LineTo(endPoint);
        }

        context.EndFigure(isClosed: false);
        return geometry;
    }

    public override void Render(DrawingContext drawingContext)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        if (Bounds.Size != _lastSize)
        {
            _staticGeometry = null;
            _lastSize = Bounds.Size;
        }

        var color = Color.FromArgb(
            (byte)_random.Next(255),
            (byte)_random.Next(255),
            (byte)_random.Next(255),
            (byte)_random.Next(255));

        var pen = new Pen(new SolidColorBrush(color), _random.Next(1, 10));

        _staticGeometry ??= MakeConnectedGeometry();

        drawingContext.DrawGeometry(null, pen, _staticGeometry);

        FrameRateMonitor.Instance.DrawCalled();
    }
}
