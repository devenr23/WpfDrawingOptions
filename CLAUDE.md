# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Purpose

This solution benchmarks 2D drawing performance across multiple UI frameworks and rendering backends (.NET 10). It renders random lines using different techniques and measures frame rates and draw call rates to compare performance.

## Build Commands

```bash
# Build entire solution
dotnet build WpfDrawingOptions.sln

# Build specific project
dotnet build WpfDrawingOptions/WpfDrawingOptions.csproj

# Build release
dotnet build WpfDrawingOptions.sln -c Release

# Run a specific app
dotnet run --project WpfDrawingOptions/WpfDrawingOptions.csproj
dotnet run --project AvaloniaDrawingOptions/AvaloniaDrawingOptions.csproj
dotnet run --project PureAvaloniaDrawingOptions/PureAvaloniaDrawingOptions.csproj
```

**Platform requirement:** `Common.Shared` and `DrawingOptions.Shared` require x64 platform. Use `-p:Platform=x64` if needed.

**Note:** `AvaloniaXpfDrawingOptions` requires a proprietary Xpf license key configured in `nuget.config`. The `Win2D` project is not included in the main solution.

## Solution Architecture

### Projects

| Project | Role |
|---------|------|
| `Common.Shared` | Core benchmarking utilities shared across all apps |
| `DrawingOptions.Shared` | WPF-specific drawing implementations and HTML canvas template |
| `SKGLWpfControl` | Reusable WPF control for OpenGL rendering via SkiaSharp |
| `WpfDrawingOptions` | Main WPF benchmark app |
| `AvaloniaDrawingOptions` | Avalonia benchmark app |
| `PureAvaloniaDrawingOptions` | Avalonia app using MVVM with CommunityToolkit.Mvvm |
| `AvaloniaXpfDrawingOptions` | Avalonia XPF bridge (requires proprietary Xpf.Sdk) |

### Core Components (Common.Shared)

- **`BenchmarkManager`** — Orchestrates automated benchmark cycling: warmup phase → data collection → result aggregation. Controls which drawing method is active.
- **`FrameRateMonitor`** — Singleton tracking real-time frame rate and draw call rate metrics.
- **`TestConstants`** — Configuration values (e.g., `NumberOfLines` controls test complexity).

### Drawing Methods Benchmarked

**WPF (`WpfDrawingOptions`):**
- SkiaSharp (CPU bitmap), SKGLWpfControl (OpenGL), MonoGame WPF Interop, SharpDX (Direct3D), WebView2 (HTML5 Canvas), WPF Canvas, StreamGeometry, GeometryDrawing

**Avalonia (`AvaloniaDrawingOptions`):**
- Drawing Canvas, Stream Geometry, Skia Bitmap, Direct Skia, Skia WriteableBitmap, Composition API

### Benchmark Flow

1. App renders random lines using the active drawing technique
2. `FrameRateMonitor` tracks performance continuously
3. `BenchmarkManager` cycles through each method: warmup → collect → next method
4. Results are aggregated and displayed in real-time
5. Performance data is maintained in `Drawing Performance Comparisons.xlsx`

### Key Patterns

- Each drawing method is encapsulated in its own control class (e.g., `MyCanvas`, `MySkiaCanvas`, `MyDrawingVisual`, `MySkGlWpfControl`)
- WPF apps target `net10.0-windows7.0`; Avalonia apps target `net10.0`
- The shared HTML canvas template lives at `DrawingOptions.Shared/html/index.html` (used by WebView2)
