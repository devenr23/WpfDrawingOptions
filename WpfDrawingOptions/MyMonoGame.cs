using System;
using DrawingOptions.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;

//using System.Windows.Media;

namespace WpfDrawingOptions;
public class MyMonoGame : WpfGame
{
    private IGraphicsDeviceService _graphicsDeviceManager;
    private WpfKeyboard _keyboard;
    private WpfMouse _mouse;
    private readonly Random _random = new();
    private Texture2D _pixel;

    protected override void Initialize()
    {
        // must be initialized. required by Content loading and rendering (will add itself to the Services)
        // note that MonoGame requires this to be initialized in the constructor, while WpfInterop requires it to
        // be called inside Initialize (before base.Initialize())
        _graphicsDeviceManager = new WpfGraphicsDeviceService(this);

        // wpf and keyboard need reference to the host control in order to receive input
        // this means every WpfGame control will have it's own keyboard & mouse manager which will only react if the mouse is in the control
        _keyboard = new WpfKeyboard(this);
        _mouse = new WpfMouse(this);

        // must be called after the WpfGraphicsDeviceService instance was created
        base.Initialize();

        // content loading now possible
        _pixel = new Texture2D(GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime time)
    {
    }

    protected override void Draw(GameTime time)
    {
        if (!IsVisible)
        {
            return;
        }

        GraphicsDevice.Clear(Color.Khaki);

        var batch = new SpriteBatch(GraphicsDevice);
        batch.Begin();

        for (int i = 0; i < TestConstants.NumberOfLines; i++)
        {
            var color = new Color
            {
                R = (byte)_random.Next(255),
                G = (byte)_random.Next(255),
                B = (byte)_random.Next(255),
                A = (byte)_random.Next(255)
            };

            var point1 = new Vector2(_random.Next((int)ActualWidth), _random.Next((int)ActualHeight));
            var point2 = new Vector2(_random.Next((int)ActualWidth), _random.Next((int)ActualHeight));
            var width = _random.Next(1, 10);

            DrawLine(batch, point1, point2, color, width);
        }

        FrameRateMonitor.Instance.DrawCalled();

        batch.End();
        base.Draw(time);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        Vector2 v = Vector2.Normalize(begin - end);
        float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y)
            angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(_pixel, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}