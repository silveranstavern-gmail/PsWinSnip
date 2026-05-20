# PsWinSnip Implementation Plan

## Overview

A Windows snipping tool that captures screenshots with Instagram-ready aspect ratios (1:1, 3:4, 4:5) using WPF, SkiaSharp, and Win32 P/Invoke.

## Architecture

```
PsWinSnip/
├── App.xaml(.cs)                    # Entry point
├── MainWindow.xaml(.cs)             # Activation window (minimize to tray)
├── Services/
│   ├── ScreenCapture.cs             # Win32 P/Invoke capture
│   ├── ImageProcessor.cs            # SkiaSharp crop/resize/encode
│   └── SettingsService.cs           # User preferences
├── Models/
│   ├── SelectionRect.cs             # Aspect-ratio-aware selection state
│   └── CaptureResult.cs             # Capture output data
├── Views/
│   ├── OverlayWindow.xaml(.cs)      # Fullscreen transparent capture UI
│   └── FloatingToolbar.xaml(.cs)   # Capture toolbar
├── Controls/
│   └── AspectRatioRect.cs           # Resizable selection rectangle
└── Helpers/
    └── Win32Interop.cs              # Native API wrappers
```

---

## Implementation Chunks

### [ ] Chunk 1: Project Foundation & Win32 Interop

**Goal:** Set up folder structure and core Win32 screen capture.

**Files:**
- `Services/ScreenCapture.cs` - BitBlt/DIB capture via P/Invoke
- `Helpers/Win32Interop.cs` - Native method declarations

**Verify:** Console app can capture full screen and save to file.

**Key APIs:**
- `user32.dll` - GetDC, ReleaseDC, BitBlt
- `gdi32.dll` - CreateCompatibleDC, CreateCompatibleBitmap, SelectObject, DeleteObject

---

### [ ] Chunk 2: SelectionRect Model & Aspect Ratio Logic

**Goal:** Core data model with ratio enforcement.

**Files:**
- `Models/SelectionRect.cs` - State: X, Y, Width, Height, AspectRatio
- Logic: Update(), EnforceRatio(), ConstrainToScreen()

**Verify:** Unit tests for ratio math.

---

### [ ] Chunk 3: Basic Overlay Window

**Goal:** Fullscreen transparent borderless window.

**Files:**
- `Views/OverlayWindow.xaml(.cs)` - WindowState.Maximized, AllowsTransparency=true, TopMost, Click-through (IsHitTestVisible=false on background)

**Verify:** Opens fullscreen with dark semi-transparent overlay.

---

### [ ] Chunk 4: Selection Rectangle UI

**Goal:** Draggable rectangle on overlay.

**Files:**
- `Controls/AspectRatioRect.xaml(.cs)` - Border/Rectangle element
- Mouse drag handling (Move, Resize)
- Ratio constraint on resize

**Verify:** Rectangle can be moved and resized while maintaining aspect ratio.

---

### [ ] Chunk 5: Resize Handles (Thumb Controls)

**Goal:** 8-point resize with corner handles.

**Files:**
- `Controls/AspectRatioRect.xaml(.cs)` - Add 8 Thumb elements (corners + edges)

**Verify:** Resize from any edge/corner maintains ratio.

---

### [ ] Chunk 6: Floating Toolbar

**Goal:** Capture/Cancel buttons + ratio presets.

**Files:**
- `Views/FloatingToolbar.xaml(.cs)` - Anchor to selection rect top-right

**UI:**
- Capture button
- 1:1, 3:4, 4:5, Freeform buttons
- Cancel (ESC)

**Verify:** Toolbar follows selection, auto-flips at screen edges.

---

### [ ] Chunk 7: Live Dimensions Display

**Goal:** Show current pixel dimensions in real-time.

**Files:**
- Update `OverlayWindow.xaml.cs` - Add TextBlock overlay showing WxH

**Verify:** Dimensions update as selection changes.

---

### [ ] Chunk 8: SkiaSharp Image Processing

**Goal:** Crop, resize, and encode output image.

**Files:**
- `Services/ImageProcessor.cs` - Crop(), Resize(), EncodeJpeg()

**Instagram targets:**
- 1:1 = 1080x1080
- 3:4 = 1080x1350
- 4:5 = 1080x1350

**Verify:** Save test captures at correct dimensions.

---

### [ ] Chunk 9: Save & Clipboard Output

**Goal:** Save to file or copy to clipboard.

**Files:**
- Update `ImageProcessor.cs` - SaveToFile(), CopyToClipboard()
- Update `OverlayWindow.xaml.cs` - Hook up toolbar buttons

**Verify:**
- Save dialog with filename
- Clipboard paste works in external app

---

### [ ] Chunk 10: System Integration

**Goal:** Global hotkey, minimize to tray, multi-monitor support.

**Files:**
- `MainWindow.xaml(.cs)` - System tray icon, ShowInTaskbar toggle
- `Services/GlobalHotkey.cs` - RegisterHotKey/UnregisterHotKey

**Verify:**
- Ctrl+Shift+S activates overlay from anywhere
- App minimizes to tray when closed
- Works on multi-monitor setup

---

### [ ] Chunk 11: Polish & Edge Cases

**Goal:** Production-ready polish.

**Items:**
- DPI awareness (all coordinates in device pixels)
- Escape to cancel
- Shift = temporarily unlock ratio
- Alt = resize from center
- Minimum size constraints (200x200)
- Handle monitor disconnect during capture
- Settings persistence (last used ratio, save location)

---

## Order of Implementation

| # | Chunk | Complexity | Dependencies |
|---|-------|------------|--------------|
| 1 | Project Foundation | Low | None |
| 2 | SelectionRect Model | Low | None |
| 3 | Overlay Window | Medium | Chunk 1 |
| 4 | Selection Rectangle UI | Medium | Chunk 2, 3 |
| 5 | Resize Handles | Medium | Chunk 4 |
| 6 | Floating Toolbar | Low | Chunk 4 |
| 7 | Dimensions Display | Low | Chunk 4 |
| 8 | SkiaSharp Processing | Medium | Chunk 1 |
| 9 | Save & Clipboard | Low | Chunk 8 |
| 10 | System Integration | Medium | All above |
| 11 | Polish | Low | All above |

Start with **Chunk 1** and build incrementally. Each chunk should be verifiable before moving to the next.