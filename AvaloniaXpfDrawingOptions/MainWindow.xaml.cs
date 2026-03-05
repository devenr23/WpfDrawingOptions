using DrawingOptions.Shared;
using System;
using System.Windows;
using System.Windows.Media;

namespace AvaloniaXpfDrawingOptions;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }

    public FrameRateMonitor FrameRateMonitor => FrameRateMonitor.Instance;

    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        if (UseDrawingVisual.IsSelected)
        {
            DrawingVisualElement.Draw();
        }
        else if (UseDrawingCanvas.IsSelected)
        {
            DrawingCanvasElement.InvalidateVisual();
        }
        else if (UseStreamGeometry.IsSelected)
        {
            StreamGeometryElement.InvalidateVisual();
        }
        else if (UseGeometryDrawing.IsSelected)
        {
            GeometryDrawingElement.InvalidateVisual();
        }
    }
}
