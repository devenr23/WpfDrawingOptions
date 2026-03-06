using Avalonia.Controls;
using Avalonia.Interactivity;
using Common.Shared;
using System;

namespace AvaloniaDrawingOptions;

public partial class MainWindow : Window
{
    private readonly BenchmarkManager _benchmark = new(
        ["Drawing Canvas", "Stream Geometry", "Skia (Bitmap)", "Direct Skia", "Skia (WriteableBitmap)", "Composition"]);
    private bool _benchmarkPending;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public FrameRateMonitor FrameRateMonitor => FrameRateMonitor.Instance;

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        FrameRateMonitor.Instance.Start();
        RequestAnimationFrame(OnFrame);
    }

    private void OnRunBenchmarkClicked(object? sender, RoutedEventArgs e)    {
        RunBenchmarkButton.IsEnabled = false;
        DrawingCombo.IsEnabled = false;
        BenchmarkResultsBorder.IsVisible = false;
        BenchmarkStatusText.Text = "Starting…";
        _benchmarkPending = true;
    }

    private async void OnCopyResultsClicked(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
            await clipboard.SetTextAsync(BenchmarkResultsText.Text);
    }

    private void OnFrame(TimeSpan time)
    {
        if (_benchmarkPending)
        {
            _benchmarkPending = false;
            _benchmark.Start(time);
        }

        if (_benchmark.IsRunning)
        {
            var idx = _benchmark.CurrentComboIndex;
            DrawingCombo.SelectedIndex = idx;

            // Set visibility directly — the IsSelected→IsVisible binding resolves
            // asynchronously, so it may not have updated by the time InvalidateVisual()
            // is called below. Setting it explicitly here guarantees Render() fires.
            DrawingCanvasElement.IsVisible       = idx == 1;
            StreamGeometryElement.IsVisible      = idx == 2;
            SkiaElement.IsVisible                = idx == 3;
            DirectSkiaElement.IsVisible          = idx == 4;
            SkiaWriteableBitmapElement.IsVisible = idx == 5;
            CompositionElement.IsVisible         = idx == 6;

            // Capture the drawing area size from the active control once it's laid out.
            // All four controls share the same grid row so the size is the same for all.
            var activeBounds = idx switch
            {
                1 => DrawingCanvasElement.Bounds.Size,
                2 => StreamGeometryElement.Bounds.Size,
                3 => SkiaElement.Bounds.Size,
                4 => DirectSkiaElement.Bounds.Size,
                5 => SkiaWriteableBitmapElement.Bounds.Size,
                6 => CompositionElement.Bounds.Size,
                _ => default
            };
            if (activeBounds.Width > 0)
                _benchmark.DrawingAreaSize = (activeBounds.Width, activeBounds.Height);

            switch (idx)
            {
                case 1: DrawingCanvasElement.InvalidateVisual(); break;
                case 2: StreamGeometryElement.InvalidateVisual(); break;
                case 3: SkiaElement.InvalidateVisual(); break;
                case 4: DirectSkiaElement.InvalidateVisual(); break;
                case 5: SkiaWriteableBitmapElement.InvalidateVisual(); break;
                case 6: CompositionElement.Draw(); break;
            }

            FrameRateMonitor.Instance.FrameRendered();

            var done = _benchmark.OnFrame(
                time,
                FrameRateMonitor.Instance.FrameRate,
                FrameRateMonitor.Instance.DrawRate);

            BenchmarkStatusText.Text = _benchmark.StatusText;

            if (done)
            {
                DrawingCombo.SelectedIndex = 0;
                DrawingCombo.IsEnabled = true;
                RunBenchmarkButton.IsEnabled = true;
                BenchmarkResultsText.Text = _benchmark.ResultsText;
                BenchmarkResultsBorder.IsVisible = true;
            }
        }
        else
        {
            if (UseDrawingCanvas.IsSelected)
                DrawingCanvasElement.InvalidateVisual();
            else if (UseStreamGeometry.IsSelected)
                StreamGeometryElement.InvalidateVisual();
            else if (UseSkiaBitmap.IsSelected)
                SkiaElement.InvalidateVisual();
            else if (UseDirectSkia.IsSelected)
                DirectSkiaElement.InvalidateVisual();
            else if (UseSkiaWriteableBitmap.IsSelected)
                SkiaWriteableBitmapElement.InvalidateVisual();
            else if (UseComposition.IsSelected)
                CompositionElement.Draw();

            FrameRateMonitor.Instance.FrameRendered();
        }

        RequestAnimationFrame(OnFrame);
    }
}
