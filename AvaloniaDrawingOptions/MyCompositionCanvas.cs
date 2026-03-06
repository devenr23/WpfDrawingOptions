using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using SkiaSharp;
using System;

namespace AvaloniaDrawingOptions;

/// <summary>
/// Composition API rendering via CompositionCustomVisual + ICustomVisualDrawHandler.
/// The compositor owns the visual on the render thread; draw messages are pushed
/// directly to it, bypassing the UI-thread scene-graph invalidation cycle entirely.
/// Unlike ICustomDrawOperation, the composition visual has its own backing surface
/// so canvas.Clear() is safe and scoped to just this visual.
/// </summary>
public class MyCompositionCanvas : Control
{
    private readonly Random _random = new();
    private CompositionCustomVisual? _visual;
    private bool _sizeToggle;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var compositor = ElementComposition.GetElementVisual(this)?.Compositor;
        if (compositor is null) return;

        _visual = compositor.CreateCustomVisual(new CompositionDrawHandler(_random));
        ElementComposition.SetElementChildVisual(this, _visual);
        UpdateVisualSize();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ElementComposition.SetElementChildVisual(this, null);
        _visual = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty)
        {
            _sizeToggle = false;
            UpdateVisualSize();
        }
    }

    private void UpdateVisualSize()
    {
        if (_visual is not null)
            _visual.Size = new Vector(Bounds.Width, Bounds.Height);
    }

    public void Draw()
    {
        if (_visual is null) return;
        var w = Math.Max(1, (int)Bounds.Width);
        var h = Math.Max(1, (int)Bounds.Height);

        _visual.SendHandlerMessage(new DrawMessage(w, h));

        // Alternate the visual's size by 1px on each call to force a backing-surface
        // resize, which is the only reliable way to trigger the initial OnRender call
        // in Avalonia 11.3. Once OnRender fires, RegisterForNextAnimationFrameUpdate
        // at its end keeps the render loop self-sustaining.
        _sizeToggle = !_sizeToggle;
        _visual.Size = new Vector(w + (_sizeToggle ? 1 : 0), h);
    }

    private sealed record DrawMessage(int Width, int Height);

    private sealed class CompositionDrawHandler : CompositionCustomVisualHandler
    {
        private readonly Random _random;
        private DrawMessage? _pending;

        public CompositionDrawHandler(Random random)
        {
            _random = random;
        }

        public override void OnMessage(object message)
        {
            if (message is DrawMessage dm)
                _pending = dm;
        }

        public override void OnRender(ImmediateDrawingContext context)
        {
            var msg = _pending;
            _pending = null;

            if (msg is not null)
            {
                var leaseFeature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) as ISkiaSharpApiLeaseFeature;
                if (leaseFeature is not null)
                {
                    using var lease = leaseFeature.Lease();
                    var canvas = lease.SkCanvas;

                    //canvas.Clear(SKColors.Transparent);

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
                            _random.Next(msg.Width), _random.Next(msg.Height),
                            _random.Next(msg.Width), _random.Next(msg.Height),
                            paint);
                    }

                    FrameRateMonitor.Instance.DrawCalled();
                }
            }

            RegisterForNextAnimationFrameUpdate();
        }
    }
}
