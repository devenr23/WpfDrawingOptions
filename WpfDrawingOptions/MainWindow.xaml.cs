using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Common.Shared;
using Microsoft.Web.WebView2.Core;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace WpfDrawingOptions;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly SKColor _background1 = SKColors.MediumVioletRed;
    private readonly SKColor _background2 = SKColors.GreenYellow;
    private readonly SKColor _background3 = SKColors.DeepSkyBlue;
    private readonly SKPaint _paint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke
    };
    private readonly Random _random = new();
    private bool _cancelImageTask;

    private readonly BenchmarkManager _benchmark = new(
        [
            "SkElement", "SKGLWpfControl", "Drawing Visual", "Drawing Canvas",
            "Stream Geometry", "GeometryDrawing", "Windows Forms", "MonoGame", "Sharp DX",
            "Web View", "Web View GL", "Web View GL Pixi"
        ],
        new()
        {
            ["Web View"]      = TimeSpan.FromSeconds(4),
            ["Web View GL"]   = TimeSpan.FromSeconds(4),
            ["Web View GL Pixi"] = TimeSpan.FromSeconds(4),
        });
    private bool _benchmarkPending;
    // Maps 0-based benchmark option to DrawingCombo index (skips SkiaImage=3)
    private static readonly int[] BenchmarkComboIndices = [1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13];

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        SetupWindowsFormsControls();

        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }

    public FrameRateMonitor FrameRateMonitor => FrameRateMonitor.Instance;

    private void SetupWindowsFormsControls()
    {
        var host = new WindowsFormsHost();
        var winFormsControl = new MyWinFormsControl();
        host.Child = winFormsControl;
        WindowsFormsElement.Child = host;
    }

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
                1  => (SkiaElement.ActualWidth,          SkiaElement.ActualHeight),
                2  => (SkGlElement.ActualWidth,           SkGlElement.ActualHeight),
                4  => (DrawingVisualElement.ActualWidth,  DrawingVisualElement.ActualHeight),
                5  => (DrawingCanvasElement.ActualWidth,  DrawingCanvasElement.ActualHeight),
                6  => (StreamGeometryElement.ActualWidth, StreamGeometryElement.ActualHeight),
                7  => (GeometryDrawingElement.ActualWidth,GeometryDrawingElement.ActualHeight),
                8  => (WindowsFormsElement.ActualWidth,   WindowsFormsElement.ActualHeight),
                9  => (MonoGameElement.ActualWidth,       MonoGameElement.ActualHeight),
                10 => (SharpDxControlElement.ActualWidth, SharpDxControlElement.ActualHeight),
                11 => (MobiusX.ActualWidth,               MobiusX.ActualHeight),
                12 => (MobiusY.ActualWidth,               MobiusY.ActualHeight),
                13 => (MobiusZ.ActualWidth,               MobiusZ.ActualHeight),
                _  => (0.0, 0.0)
            };
            if (areaW > 0)
                _benchmark.DrawingAreaSize = (areaW, areaH);

            switch (comboIdx)
            {
                case 1: SkiaElement.InvalidateVisual(); break;
                case 2: SkGlElement.InvalidateVisual(); break;
                case 4: DrawingVisualElement.Draw(); break;
                case 5: DrawingCanvasElement.InvalidateVisual(); break;
                case 6: StreamGeometryElement.InvalidateVisual(); break;
                case 7: GeometryDrawingElement.InvalidateVisual(); break;
                case 8: (WindowsFormsElement.Child as WindowsFormsHost)?.Child.Invalidate(); break;
                // case 9 (MonoGame), 11-13 (WebViews): self-rendering
                case 10: SharpDxControlElement.InvalidateVisual(); break;
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
            if (UseSkElement.IsSelected)
                SkiaElement.InvalidateVisual();
            else if (UseSkGl.IsSelected)
                SkGlElement.InvalidateVisual();
            else if (UseDrawingVisual.IsSelected)
                DrawingVisualElement.Draw();
            else if (UseDrawingCanvas.IsSelected)
                DrawingCanvasElement.InvalidateVisual();
            else if (UseStreamGeometry.IsSelected)
                StreamGeometryElement.InvalidateVisual();
            else if (UseGeometryDrawing.IsSelected)
                GeometryDrawingElement.InvalidateVisual();
            else if (UseWindowsForms.IsSelected)
                (WindowsFormsElement.Child as WindowsFormsHost)?.Child.Invalidate();
            else if (UseSharpDX.IsSelected)
                SharpDxControlElement.InvalidateVisual();
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

    private void DrawCanvas(SKCanvas canvas, int width, int height, SKColor background)
    {
        canvas.Clear(background);

        for (int i = 0; i < TestConstants.NumberOfLines; i++)
        {
            _paint.Color = new SKColor(
                red: (byte)_random.Next(255),
                green: (byte)_random.Next(255),
                blue: (byte)_random.Next(255),
                alpha: (byte)_random.Next(255));

            _paint.StrokeWidth = _random.Next(1, 10);

            canvas.DrawLine(
                x0: _random.Next(width),
                y0: _random.Next(height),
                x1: _random.Next(width),
                y1: _random.Next(height),
                paint: _paint);
        }

        FrameRateMonitor.Instance.DrawCalled();
    }

    private void DrawTask()
    {
        const double maxFps = 30;
        double minFramePeriod = 1000.0 / maxFps;

        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!_cancelImageTask)
        {
            int width = 0;
            int height = 0;

            SkiaImageElement.Dispatcher.Invoke(() =>
            {
                width = (int)SkiaImageElement.ActualWidth;
                height = (int)SkiaImageElement.ActualHeight;
            });

            var bmp = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bmp);

            DrawCanvas(canvas, width, height, _background3);

            SkiaImageElement.Source = SKImage.FromPixels(bmp.PeekPixels());

            SkiaImageElement.Dispatcher.BeginInvoke(() => SkiaImageElement.InvalidateVisual());

            // FPS limiter
            double msToWait = minFramePeriod - stopwatch.ElapsedMilliseconds;
            if (msToWait > 0)
                Thread.Sleep((int)msToWait);
            stopwatch.Restart();
        }
    }

    private void SkGlElement_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
    {
        DrawCanvas(e.Surface.Canvas, e.Info.Width, e.Info.Height, _background2);
    }

    private void SkiaElement_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        DrawCanvas(e.Surface.Canvas, e.Info.Width, e.Info.Height, _background1);
    }

    private void UseSkiaImage_Selected(object sender, RoutedEventArgs e)
    {
        _cancelImageTask = false;
        Task.Run(DrawTask);
    }

    private void UseSkiaImage_Unselected(object sender, RoutedEventArgs e)
    {
        _cancelImageTask = true;
    }

    private async void UseWebView_Selected(object sender, RoutedEventArgs e)
    {
        await MobiusX.EnsureCoreWebView2Async(null);
        MobiusX.CoreWebView2.WebMessageReceived += ReceiveMessageFromJavaScript;
        var localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        MobiusX.CoreWebView2.SetVirtualHostNameToFolderMapping("demo", localPath, CoreWebView2HostResourceAccessKind.Allow);
        MobiusX.CoreWebView2.Navigate("https://demo/html/index.html");
    }

    private async void UseWebView_Selected2(object sender, RoutedEventArgs e)
    {
        await MobiusY.EnsureCoreWebView2Async(null);
        MobiusY.CoreWebView2.WebMessageReceived += ReceiveMessageFromJavaScript;
        var localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        MobiusY.CoreWebView2.SetVirtualHostNameToFolderMapping("demo", localPath, CoreWebView2HostResourceAccessKind.Allow);
        MobiusY.CoreWebView2.Navigate("https://demo/html/webgl.html");
    }

    private async void UseWebView_Selected3(object sender, RoutedEventArgs e)
    {
        await MobiusZ.EnsureCoreWebView2Async(null);
        MobiusZ.CoreWebView2.WebMessageReceived += ReceiveMessageFromJavaScript;
        var localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        MobiusZ.CoreWebView2.SetVirtualHostNameToFolderMapping("demo", localPath, CoreWebView2HostResourceAccessKind.Allow);
        MobiusZ.CoreWebView2.Navigate("https://demo/html/pixi.html");
    }

    private void ReceiveMessageFromJavaScript(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        var message = args.WebMessageAsJson;
        if (message == "\"Draw Called\"")
            FrameRateMonitor.Instance.DrawCalled();
    }

    private void UseWebView_Unselected(object sender, RoutedEventArgs e)
    {
        MobiusX.CoreWebView2.WebMessageReceived -= ReceiveMessageFromJavaScript;
    }
    private void UseWebView_Unselected2(object sender, RoutedEventArgs e)
    {
        MobiusY.CoreWebView2.WebMessageReceived -= ReceiveMessageFromJavaScript;
    }
    private void UseWebView_Unselected3(object sender, RoutedEventArgs e)
    {
        MobiusZ.CoreWebView2.WebMessageReceived -= ReceiveMessageFromJavaScript;
    }
}