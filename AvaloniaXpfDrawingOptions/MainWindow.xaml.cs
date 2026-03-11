using Common.Shared;
using DrawingOptions.Shared;
using System;
using System.Windows;
using System.Windows.Media;

namespace AvaloniaXpfDrawingOptions;

public partial class MainWindow : Window
{
    private readonly BenchmarkManager _benchmark = new([
        "Drawing Visual", "Drawing Canvas", "Stream Geometry", "GeometryDrawing"
    ]);
    private bool _benchmarkPending;
    // Maps 0-based benchmark option to DrawingCombo index (1-based)
    private static readonly int[] BenchmarkComboIndices = [1, 2, 3, 4];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }

    public FrameRateMonitor FrameRateMonitor => FrameRateMonitor.Instance;

    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        FrameRateMonitor.Instance.FrameRendered();
        var time = ((RenderingEventArgs)e).RenderingTime;

        if (_benchmarkPending)
        {
            _benchmarkPending = false;
            _benchmark.Start(time);
        }

        if (_benchmark.IsRunning)
        {
            var comboIdx = BenchmarkComboIndices[_benchmark.CurrentComboIndex - 1];
            DrawingCombo.SelectedIndex = comboIdx;

            var (areaW, areaH) = comboIdx switch
            {
                1 => (DrawingVisualElement.ActualWidth,  DrawingVisualElement.ActualHeight),
                2 => (DrawingCanvasElement.ActualWidth,  DrawingCanvasElement.ActualHeight),
                3 => (StreamGeometryElement.ActualWidth, StreamGeometryElement.ActualHeight),
                4 => (GeometryDrawingElement.ActualWidth,GeometryDrawingElement.ActualHeight),
                _ => (0.0, 0.0)
            };
            if (areaW > 0)
                _benchmark.DrawingAreaSize = (areaW, areaH);

            switch (comboIdx)
            {
                case 1: DrawingVisualElement.Draw(); break;
                case 2: DrawingCanvasElement.InvalidateVisual(); break;
                case 3: StreamGeometryElement.InvalidateVisual(); break;
                case 4: GeometryDrawingElement.InvalidateVisual(); break;
            }

            var done = _benchmark.OnFrame(time, FrameRateMonitor.Instance.FrameRate, FrameRateMonitor.Instance.DrawRate);
            BenchmarkStatusText.Text = _benchmark.StatusText;

            if (done)
            {
                DrawingCombo.SelectedIndex = 0;
                DrawingCombo.IsEnabled = true;
                RunBenchmarkButton.IsEnabled = true;
                BenchmarkResultsText.Text = _benchmark.ResultsText;
                BenchmarkResultsBorder.Visibility = Visibility.Visible;
            }
        }
        else
        {
            if (UseDrawingVisual.IsSelected)
                DrawingVisualElement.Draw();
            else if (UseDrawingCanvas.IsSelected)
                DrawingCanvasElement.InvalidateVisual();
            else if (UseStreamGeometry.IsSelected)
                StreamGeometryElement.InvalidateVisual();
            else if (UseGeometryDrawing.IsSelected)
                GeometryDrawingElement.InvalidateVisual();
        }
    }

    private void OnRunBenchmarkClicked(object sender, RoutedEventArgs e)
    {
        RunBenchmarkButton.IsEnabled = false;
        DrawingCombo.IsEnabled = false;
        BenchmarkResultsBorder.Visibility = Visibility.Collapsed;
        BenchmarkStatusText.Text = "Starting\u2026";
        _benchmarkPending = true;
    }

    private void OnCopyResultsClicked(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_benchmark.ResultsText);
    }
}
