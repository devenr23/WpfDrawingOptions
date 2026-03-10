using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Common.Shared;
using System;

namespace AvaloniaDrawingOptions;

/// <summary>
/// Avalonia equivalent of the WPF MyCanvas — overrides Render() and draws directly
/// into the DrawingContext each frame (immediate mode).
/// Optimized with pre-allocated pen/brush pools to eliminate per-frame allocations.
/// </summary>
public class MyCanvas : Control
{
    private readonly Random _random = new();
    
    // Pre-allocated pools to eliminate per-frame allocations
    private readonly SolidColorBrush[] _brushPool;
    private readonly Pen[] _penPool;
    private const int PoolSize = 256; // Pool of pre-created pens/brushes

    public MyCanvas()
    {
        // Pre-create a pool of random brushes
        _brushPool = new SolidColorBrush[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            var color = Color.FromArgb(
                (byte)_random.Next(255),
                (byte)_random.Next(255),
                (byte)_random.Next(255),
                (byte)_random.Next(255));
            _brushPool[i] = new SolidColorBrush(color);
        }

        // Pre-create a pool of random pens using the brushes
        _penPool = new Pen[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            _penPool[i] = new Pen(_brushPool[i], _random.Next(1, 10));
        }
    }

    public override void Render(DrawingContext drawingContext)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        int width = (int)Bounds.Width;
        int height = (int)Bounds.Height;

        for (int i = 0; i < TestConstants.NumberOfLines; i++)
        {
            // Select random pre-created pen from pool (no allocation!)
            var pen = _penPool[_random.Next(PoolSize)];
            
            // Reuse local variables to minimize struct overhead
            var point1 = new Point(_random.Next(width), _random.Next(height));
            var point2 = new Point(_random.Next(width), _random.Next(height));

            drawingContext.DrawLine(pen, point1, point2);
        }

        FrameRateMonitor.Instance.DrawCalled();
    }
}
