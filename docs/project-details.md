# PsWinSnip Project Details

## Framework & SDK

| Component | Version |
|-----------|---------|
| .NET SDK | 10.0.300 |
| Target Framework | net10.0-windows |
| C# Language | 14 (latest in .NET 10) |

## UI Framework

| Component | Version |
|-----------|---------|
| WPF (Windows Presentation Foundation) | Built-in (.NET 10) |

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| SkiaSharp | 3.119.2 | Image processing (crop, PNG/JPEG encode) |

## Platform

- **OS:** Windows only (Win32 API for screen capture)
- **Architecture:** x64 (primary), supports Arm64
- **Min Windows Version:** Windows 10 (likely, needs testing)

## Build Configuration

```xml
<OutputType>WinExe</OutputType>
<TargetFramework>net10.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

## Notes for Agents

- This is a **WPF desktop application** (not web/Blazor)
- Screen capture uses **Win32 P/Invoke** (user32.dll, gdi32.dll)
- ImageSharp is NOT used (now requires paid license) - use SkiaSharp instead
- Project targets **.NET 10** - ensure any research considers .NET 10 compatibility
- Build with: `dotnet build`
- Run with: `dotnet run` (from PsWinSnip directory)