using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using SkiaSharp;

namespace AvaloniaDrawingOptions.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }


    //public override void Render(DrawingContext context)
    //{
    //    base.Render(context);

        
    //    context.DrawLine(new Pen(Brushes.Red, 2),
    //        new Point(0, 0),
    //        new Point(Bounds.Width, Bounds.Height));

    //    context.DrawEllipse(Brushes.Blue, null,
    //        new Point(Bounds.Width / 2, Bounds.Height / 2),
    //        40, 40);
    //}


    //private void DrawTask()
    //{
    //    const double maxFps = 30;
    //    double minFramePeriod = 1000.0 / maxFps;

    //    Stopwatch stopwatch = Stopwatch.StartNew();
    //    while (!_cancelImageTask)
    //    {
    //        int width = 0;
    //        int height = 0;

    //        Dispatcher.UIThread.Post(() =>
    //        {
    //            width = (int)SkiaImageElement.Bounds.Width;
    //            height = (int)SkiaImageElement.Bounds.Height;
    //        }, DispatcherPriority.Send);

    //        if (width <= 0 || height <= 0)
    //        {
    //            Thread.Sleep(100);
    //            continue;
    //        }

    //        Dispatcher.UIThread.Post(() =>
    //        {
    //            SkiaImageElement.InvalidateVisual();
    //        });

    //        // FPS limiter
    //        double msToWait = minFramePeriod - stopwatch.ElapsedMilliseconds;
    //        if (msToWait > 0)
    //            Thread.Sleep((int)msToWait);
    //        stopwatch.Restart();
    //    }
    //}
}