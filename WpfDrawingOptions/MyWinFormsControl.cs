using DrawingOptions.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WpfDrawingOptions;

public class MyWinFormsControl : Control
{
	private readonly Random _random = new();
	
	protected override void OnPaint(PaintEventArgs e)
	{
		var graphics = e.Graphics;
		graphics.Clear(Color.White);

		for (int i = 0; i < TestConstants.NumberOfLines; i++)
		{
			var color = Color.FromArgb(_random.Next(255), _random.Next(255), _random.Next(255), _random.Next(255));

			var point1 = new Point(_random.Next(Width), _random.Next(Height));
			var point2 = new Point(_random.Next(Width), _random.Next(Height));
			var pen = new Pen(color, _random.Next(1, 10));

			graphics.DrawLine(pen, point1, point2);
		}

		FrameRateMonitor.Instance.DrawCalled();
	}
}