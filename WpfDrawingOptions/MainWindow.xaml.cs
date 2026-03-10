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
        if (UseSkElement.IsSelected)
        {
            SkiaElement.InvalidateVisual();
        }
        else if (UseSkGl.IsSelected)
        {
            SkGlElement.InvalidateVisual();
        }
        else if (UseDrawingVisual.IsSelected)
        {
            DrawingVisualElement.Draw();
        }
        else if (UseDrawingCanvas.IsSelected)
        {
            DrawingCanvasElement.InvalidateVisual();
        }
        else if (UseStreamGeometry.IsSelected)
        {
            StreamGeometryElement.InvalidateVisual();
        }
        else if (UseGeometryDrawing.IsSelected)
        {
            GeometryDrawingElement.InvalidateVisual();
        }
        else if (UseWindowsForms.IsSelected)
        {
            (WindowsFormsElement.Child as WindowsFormsHost)?.Child.Invalidate();
        }
        else if (UseSharpDX.IsSelected)
        {
            SharpDxControlElement.InvalidateVisual();
        }
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
        var localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        MobiusX.CoreWebView2.SetVirtualHostNameToFolderMapping("demo", localPath, CoreWebView2HostResourceAccessKind.Allow);
        MobiusX.CoreWebView2.Navigate("https://demo/html/index.html");
    }

    private async void UseWebView_Selected2(object sender, RoutedEventArgs e)
    {
        await MobiusY.EnsureCoreWebView2Async(null);
        var localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        MobiusY.CoreWebView2.SetVirtualHostNameToFolderMapping("demo", localPath, CoreWebView2HostResourceAccessKind.Allow);
        MobiusY.CoreWebView2.Navigate("https://demo/html/webgl.html");
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
}