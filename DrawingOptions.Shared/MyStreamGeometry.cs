using System;
using System.Windows;
using System.Windows.Media;

namespace DrawingOptions.Shared;

public class MyStreamGeometry : FrameworkElement
{
	private readonly Random _random = new();
	
	private static StreamGeometry? _staticGeometry;

    public StreamGeometry MakeConnectedGeometry()
	{
		var geometry = new StreamGeometry();
		var context = geometry.Open();

		var startPoint = new Point(_random.Next((int)ActualWidth), _random.Next((int)ActualHeight));
		context.BeginFigure(startPoint, false, false);

		for (int i = 0; i < TestConstants.NumberOfLines; i++)
		{
			var endPoint = new Point(_random.Next((int)ActualWidth), _random.Next((int)ActualHeight));
			
			context.LineTo(endPoint, true, false);
		}

		context.Close();
		geometry.Freeze();
		return geometry;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		var color = new Color
		{
			R = (byte)_random.Next(255),
			G = (byte)_random.Next(255),
			B = (byte)_random.Next(255),
			A = (byte)_random.Next(255)
		};

		var pen = new Pen(new SolidColorBrush(color), _random.Next(1, 10));

		_staticGeometry ??= MakeConnectedGeometry();

		var geometry = _staticGeometry;
		drawingContext.DrawGeometry(null, pen, geometry);

		FrameRateMonitor.Instance.DrawCalled();
	}
}