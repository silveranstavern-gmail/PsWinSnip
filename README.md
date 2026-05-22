# PsWinSnip

PsWinSnip is a lightweight Windows desktop snipping utility optimized for precise aspect ratio control and pixel-perfect screen cropping. 

Unlike standard screen-capture utilities that only capture arbitrary regions, PsWinSnip is designed for developers, designers, and content creators who need to crop images to exact dimensions and aspect ratios (e.g., for social media, UI designs, or document layouts) without manual post-processing, scaling, or resizing.

---

## Features

- **Constrained Aspect Ratios:** Lock capture constraints to popular presets:
  - `1:1` (Square)
  - `4:5` (Vertical / Social Media)
  - `3:4` (Portrait)
  - `Free` (Custom freehand selection)
- **Dynamic Dimension Driver:** Choose whether the width drives the height or vice versa when adjusting capture constraints, allowing fine-grained numeric precision.
- **Global Hotkey:** Trigger a capture instantly from anywhere using `Ctrl + Shift + S`.
- **System Tray Integration:** Runs discreetly in the background, allowing you to trigger captures or access settings from the tray icon.
- **Pixel-Perfect Copy:** Auto-crops selections directly to your clipboard in high-quality PNG format.
- **High-Performance Backend:** Leverages **SkiaSharp** for ultra-fast, modern image processing and encoding.

---

## Technical Specifications

- **Framework:** .NET 10.0 (WPF & Windows Forms Hybrid)
- **Language:** C# 14
- **Graphics & Encoding:** SkiaSharp
- **Target OS:** Windows 10 / 11 (uses Win32 API P/Invoke for screen-capture operations)

---

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or higher.
- Windows 10 (Build 17763) or newer.

### Build and Run

1. Clone or download the repository.
2. Open your terminal at the project root directory.
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

Upon running, PsWinSnip will initialize as a tray icon in your Windows Taskbar. Double-click the tray icon to configure settings (such as dimension presets and grid display toggles) or press `Ctrl + Shift + S` to capture.

### Publishing Standalone Executables

To package the application as a standalone, single-file executable for 64-bit Windows (which bundles the .NET runtime and native libraries, making it fully self-contained and ready to run without separate installations):

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The output single executable will be generated at `bin/Release/net10.0-windows/win-x64/publish/PsWinSnip.exe`.

---

## License

This project is licensed under the **Mozilla Public License Version 2.0 (MPL-2.0)**. For the full license terms, please see the [LICENSE](LICENSE) file.

---

## Support & Donation

If you find PsWinSnip useful and would like to support me and the development of other apps and potential maintenance of this one, you can do so through the following links:

- **Support Hub & Contact:** [Pondering Silver Support](https://ponderingsilver.com/support/)
- **Direct Support via Stripe:** [Donate on Stripe](https://donate.stripe.com/eVa4hvd8sfNx0Pm4gg)
- **Direct Support via PayPal:** [Donate on PayPal](https://www.paypal.com/donate/?business=5XBKNVSRQXEYU&no_recurring=0&currency_code=USD)

Support is always appreciated and many thanks are given.
