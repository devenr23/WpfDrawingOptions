using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaDrawingOptions.ViewModels;
using Common.Shared;
using System;

namespace AvaloniaDrawingOptions.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainWindowViewModel Vm => (MainWindowViewModel)DataContext!;

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        FrameRateMonitor.Instance.Start();
        RequestAnimationFrame(OnFrame);
    }

    private void OnFrame(TimeSpan time)
    {
        var vm = Vm;

        if (vm.BenchmarkPending)
        {
            vm.BenchmarkPending = false;
            vm.Benchmark.Start(time);
            vm.Benchmark.DrawingAreaSize = (DrawContextElement.Bounds.Width, DrawContextElement.Bounds.Height);
        }

        FrameRateMonitor.Instance.FrameRendered();

        if (vm.Benchmark.IsRunning)
        {
            var done = vm.Benchmark.OnFrame(
                time,
                FrameRateMonitor.Instance.FrameRate,
                FrameRateMonitor.Instance.DrawRate);

            BenchmarkStatusText.Text = vm.Benchmark.StatusText;

            if (done)
            {
                vm.IsRunBenchmarkEnabled = true;
                BenchmarkResultsText.Text = vm.Benchmark.ResultsText;
                vm.IsBenchmarkResultsVisible = true;
            }
        }

        RequestAnimationFrame(OnFrame);
    }

    private async void OnCopyResultsClicked(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
            await clipboard.SetTextAsync(Vm.Benchmark.ResultsText);
    }
}
