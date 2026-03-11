using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Shared;

public record BenchmarkResult(string Name, double AvgFrameRate, double AvgDrawRate);

/// <summary>
/// Drives an automated benchmark that cycles through each drawing option,
/// discards a warmup period, collects frame/draw rate samples, then reports averages.
/// </summary>
public class BenchmarkManager
{
    public static readonly TimeSpan WarmupDuration = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan CollectDuration = TimeSpan.FromSeconds(5);

    private readonly string[] _optionNames;
    private readonly Dictionary<string, TimeSpan>? _warmupOverrides;
    private int _currentOption;
    private TimeSpan _phaseStart;
    private bool _inWarmup;
    private readonly List<double> _frameRateSamples = [];
    private readonly List<double> _drawRateSamples = [];
    private readonly List<BenchmarkResult> _results = [];

    public BenchmarkManager(string[] optionNames, Dictionary<string, TimeSpan>? warmupOverrides = null)
    {
        _optionNames = optionNames;
        _warmupOverrides = warmupOverrides;
    }

    private TimeSpan CurrentWarmupDuration =>
        _warmupOverrides?.TryGetValue(_optionNames[_currentOption], out var d) == true ? d : WarmupDuration;

    public bool IsRunning { get; private set; }
    public string StatusText { get; private set; } = "";
    public string ResultsText { get; private set; } = "";
    public (double Width, double Height) DrawingAreaSize { get; set; }

    /// <summary>1-based ComboBox index for the option currently being benchmarked.</summary>
    public int CurrentComboIndex => _currentOption + 1;

    public void Start(TimeSpan currentTime)
    {
        _currentOption = 0;
        _phaseStart = currentTime;
        _inWarmup = true;
        _frameRateSamples.Clear();
        _drawRateSamples.Clear();
        _results.Clear();
        IsRunning = true;
        UpdateStatus();
    }

    /// <summary>
    /// Called every animation frame with the latest rates.
    /// Returns true when all options have been measured.
    /// </summary>
    public bool OnFrame(TimeSpan currentTime, double frameRate, double drawRate)
    {
        if (!IsRunning) return false;

        var elapsed = currentTime - _phaseStart;

        if (_inWarmup)
        {
            if (elapsed >= CurrentWarmupDuration)
            {
                _inWarmup = false;
                _phaseStart = currentTime;
                // Reset so collection samples start clean
                FrameRateMonitor.Instance.Start();
                UpdateStatus();
            }
            return false;
        }

        _frameRateSamples.Add(frameRate);
        _drawRateSamples.Add(drawRate);

        if (currentTime - _phaseStart >= CollectDuration)
        {
            RecordResult();
            _currentOption++;

            if (_currentOption >= _optionNames.Length)
            {
                IsRunning = false;
                StatusText = "";
                BuildResultsText();
                return true;
            }

            _phaseStart = currentTime;
            _inWarmup = true;
            _frameRateSamples.Clear();
            _drawRateSamples.Clear();
            UpdateStatus();
        }

        return false;
    }

    private void RecordResult()
    {
        var avgFrame = _frameRateSamples.Count > 0 ? _frameRateSamples.Average() : 0;
        var avgDraw  = _drawRateSamples.Count  > 0 ? _drawRateSamples.Average()  : 0;
        _results.Add(new BenchmarkResult(_optionNames[_currentOption], avgFrame, avgDraw));
    }

    private void UpdateStatus()
    {
        var phase = _inWarmup ? "warming up" : "measuring";
        StatusText = $"Benchmarking: {_optionNames[_currentOption]} ({phase}\u2026)";
    }

    private void BuildResultsText()
    {
        var size = DrawingAreaSize is { Width: > 0, Height: > 0 }
            ? $" | {DrawingAreaSize.Width:0}×{DrawingAreaSize.Height:0} px"
            : "";
        var header = $"Results — {TestConstants.NumberOfLines:N0} lines{size} | " +
                     $"{CollectDuration.TotalSeconds:0}s per option";
        var rows = _results.Select(r =>
            $"  {r.Name,-16}  Frame: {r.AvgFrameRate,5:0.0}   Draw: {r.AvgDrawRate,5:0.0}");
        ResultsText = header + Environment.NewLine + string.Join(Environment.NewLine, rows);
    }
}
