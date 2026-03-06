using System;
using System.Windows;
using System.Windows.Media;

namespace DrawingOptions.Shared;

public class MyCanvas : FrameworkElement
{
    private readonly Random _random = new();

    protected override void OnRender(DrawingContext drawingContext)
    {
        for (int i = 0; i < TestConstants.NumberOfLines; i++)
        {
            var color = new Color
            {
                R = (byte)_random.Next(255),
                G = (byte)_random.Next(255),
                B = (byte)_random.Next(255),
                A = (byte)_random.Next(255)
            };

            var point1 = new Point(_random.Next((int)ActualWidth), _random.Next((int)ActualHeight));
            var point2 = new Point(_random.Next((int)ActualWidth), _random.Next((int)ActualHeight));
            var pen = new Pen(new SolidColorBrush(color), _random.Next(1, 10));

            drawingContext.DrawLine(pen, point1, point2);
        }

        FrameRateMonitor.Instance.DrawCalled();
    }
}