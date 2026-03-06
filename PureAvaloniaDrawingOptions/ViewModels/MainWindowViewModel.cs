using Common.Shared;

namespace AvaloniaDrawingOptions.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public FrameRateMonitor FrameRateMonitor => FrameRateMonitor.Instance;
}
