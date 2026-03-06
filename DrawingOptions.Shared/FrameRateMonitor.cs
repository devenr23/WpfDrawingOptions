using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DrawingOptions.Shared;

public class FrameRateMonitor : INotifyPropertyChanged
{
    public static FrameRateMonitor Instance { get; } = new FrameRateMonitor();

    private readonly Stopwatch _renderMonitorStopwatch;
    private int _renderCount;

    private readonly Stopwatch _drawMonitorStopwatch;
    private int _drawCount;

    public FrameRateMonitor()
    {
        _renderMonitorStopwatch = new Stopwatch();
        _drawMonitorStopwatch = new Stopwatch();

        Start();
    }

    public void Start()
    {
        _renderMonitorStopwatch.Start();
        _renderCount = 0;

        _drawMonitorStopwatch.Start();
        _drawCount = 0;

        // CompositionTarget.Rendering is reimplemented by Avalonia XPF and fires
        // on every frame that Avalonia's compositor produces.
        CompositionTarget.Rendering += CompositionTargetOnRendering;
    }

    private void CompositionTargetOnRendering(object sender, object e)
    {
        lock (_renderMonitorStopwatch)
        {
            _renderCount++;

            var elapsedSeconds = _renderMonitorStopwatch.ElapsedMilliseconds / 1000.0;
            if (elapsedSeconds > 0.05)
                FrameRate = _renderCount / elapsedSeconds;

            if (elapsedSeconds > 1)
            {
                _renderCount = 0;
                _renderMonitorStopwatch.Restart();
            }
        }
    }

    public void DrawCalled()
    {
        lock (_drawMonitorStopwatch)
        {
            _drawCount++;

            var elapsedSeconds = _drawMonitorStopwatch.ElapsedMilliseconds / 1000.0;
            if (elapsedSeconds > 0.05)
                DrawRate = _drawCount / elapsedSeconds;

            if (elapsedSeconds > 1)
            {
                _drawCount = 0;
                _drawMonitorStopwatch.Restart();
            }
        }

        FirePropertyChanged(nameof(RateInfo));
    }

    public double FrameRate { get; private set; }
    public double DrawRate { get; private set; }

    public string RateInfo => $"Frame Rate: {FrameRate:0.0} - Draw Rate: {DrawRate:0.0}";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void FirePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Stop()
    {
        CompositionTarget.Rendering -= CompositionTargetOnRendering;
        _renderMonitorStopwatch.Stop();
        _drawMonitorStopwatch.Stop();
    }
}
