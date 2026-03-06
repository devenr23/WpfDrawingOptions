using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Common.Shared;
using SkiaSharp;
using System;

namespace AvaloniaDrawingOptions;

/// <summary>
/// True direct Skia rendering via ICustomDrawOperation + ISkiaSharpApiLease.
/// Hooks directly into Avalonia's Skia render pipeline without any intermediate
/// bitmap or PNG encode step, giving the lowest-overhead path to the GPU.
/// </summary>
public class MyDirectSkiaCanvas : Control
{
    private readonly Random _random = new();

    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        context.Custom(new DirectSkiaDrawOp(new Rect(Bounds.Size), _random));
        FrameRateMonitor.Instance.DrawCalled();
    }

    private sealed class DirectSkiaDrawOp : ICustomDrawOperation
    {
        private readonly Random _random;

        public DirectSkiaDrawOp(Rect bounds, Random random)
        {
            Bounds = bounds;
            _random = random;
        }

        public Rect Bounds { get; }

        public void Dispose() { }

        public bool HitTest(Point p) => false;

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) as ISkiaSharpApiLeaseFeature;
            if (leaseFeature is null) return;

            using var lease = leaseFeature.Lease();
            {
                var canvas = lease.SkCanvas;
                var w = Math.Max(1, (int)Bounds.Width);
                var h = Math.Max(1, (int)Bounds.Height);

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
            }
        }
    }
}
