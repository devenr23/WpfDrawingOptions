using Common.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaDrawingOptions.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public FrameRateMonitor FrameRateMonitor => FrameRateMonitor.Instance;

    public BenchmarkManager Benchmark { get; } = new(["Drawing Canvas"]);

    [ObservableProperty]
    private bool _isBenchmarkResultsVisible;

    [ObservableProperty]
    private bool _isRunBenchmarkEnabled = true;

    public bool BenchmarkPending { get; set; }

    [RelayCommand]
    private void RunBenchmark()
    {
        IsBenchmarkResultsVisible = false;
        IsRunBenchmarkEnabled = false;
        BenchmarkPending = true;
    }
}
