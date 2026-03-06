using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DrawingOptions.Shared;
using Common.Shared;

namespace WpfDrawingOptions;

public class SharpDxControl : HwndHost
{
	private readonly Factory _factory;
	private WindowRenderTarget? _renderTarget;

	private static readonly Random _random = new();

	public SharpDxControl()
	{
		_factory = new Factory();

		Loaded += OnControlLoaded;
		SizeChanged += OnControlSizeChanged;
	}

	private void OnControlLoaded(object sender, RoutedEventArgs e)
	{
		CreateRenderTarget();
	}

	private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
	{
		CreateRenderTarget();
	}

	private void CreateRenderTarget()
	{
		var properties = new HwndRenderTargetProperties()
		{
			Hwnd = Hwnd,//Handle,
			PixelSize = new Size2((int)ActualWidth, (int)ActualHeight),
			PresentOptions = PresentOptions.None
		};

		_renderTarget = new WindowRenderTarget(_factory, new RenderTargetProperties(), properties);
	}

	private readonly RawColor4 _white = new(1, 1, 1, 1);

	protected override void OnRender(DrawingContext drawingContext)
	{
		var renderTarget = _renderTarget;

		if (renderTarget is null)
			return;

		Draw(renderTarget);
	}

	private void Draw(WindowRenderTarget renderTarget)
	{
		renderTarget.BeginDraw();

		renderTarget.Clear(_white);

		for (int i = 0; i < TestConstants.NumberOfLines; i++)
		{
			var color = new RawColor4((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble(), 1);

			var brush = new SharpDX.Direct2D1.SolidColorBrush(renderTarget, color);
			var point1 = new RawVector2(_random.Next((int)ActualWidth), _random.Next((int)ActualHeight));
			var point2 = new RawVector2(_random.Next((int)ActualWidth), _random.Next((int)ActualHeight));

			var strokeWidth = _random.Next(1, 10);

			renderTarget.DrawLine(point1, point2, brush, strokeWidth);
			brush.Dispose();
		}

		renderTarget.EndDraw();

		FrameRateMonitor.Instance.DrawCalled();
	}

	#region WIN32 COM CRAP

	private const string WindowClass = "HwndWrapper";
	protected IntPtr Hwnd { get; private set; }
	protected override HandleRef BuildWindowCore(HandleRef hwndParent)
	{
		var wndClass = new NativeMethods.WndClassEx();
		wndClass.cbSize = (uint)Marshal.SizeOf(wndClass);
		wndClass.hInstance = NativeMethods.GetModuleHandle(null);
		wndClass.lpfnWndProc = NativeMethods.DefaultWindowProc;
		wndClass.lpszClassName = WindowClass;
		wndClass.hCursor = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW);
		NativeMethods.RegisterClassEx(ref wndClass);

		Hwnd = NativeMethods.CreateWindowEx
		(
			0, WindowClass, "", NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE,
			0, 0, (int)Width, (int)Height, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, 0
		);

		return new HandleRef(this, Hwnd);
	}

	protected override void DestroyWindowCore(HandleRef hwnd)
	{
		_renderTarget?.Dispose();
	}

	internal class NativeMethods
	{
		// ReSharper disable InconsistentNaming
		public const int WS_CHILD = 0x40000000;
		public const int WS_VISIBLE = 0x10000000;

		public const int WM_LBUTTONDOWN = 0x0201;
		public const int WM_LBUTTONUP = 0x0202;
		public const int WM_RBUTTONDOWN = 0x0204;
		public const int WM_RBUTTONUP = 0x0205;

		public const int IDC_ARROW = 32512;

		[StructLayout(LayoutKind.Sequential)]
		public struct WndClassEx
		{
			public uint cbSize;
			public uint style;
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public WndProc lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			public string lpszMenuName;
			public string lpszClassName;
			public IntPtr hIconSm;
		}

		[DllImport("user32.dll")]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public static readonly WndProc DefaultWindowProc = DefWindowProc;

		[DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Auto)]
		public static extern IntPtr CreateWindowEx(
			int exStyle,
			string className,
			string windowName,
			int style,
			int x, int y,
			int width, int height,
			IntPtr hwndParent,
			IntPtr hMenu,
			IntPtr hInstance,
			[MarshalAs(UnmanagedType.AsAny)] object pvParam);

		[DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Auto)]
		public static extern bool DestroyWindow(IntPtr hwnd);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetModuleHandle(string module);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.U2)]
		public static extern short RegisterClassEx([In] ref WndClassEx lpwcx);

		[DllImport("user32.dll")]
		public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
		// ReSharper restore InconsistentNaming
	}

	#endregion
}