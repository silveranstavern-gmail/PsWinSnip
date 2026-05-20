# Code Review: PsWinSnip

## Executive Summary

The application is well-structured and follows the planned architecture. Most core functionality is implemented. However, there are several gaps between the documented plan and actual implementation, plus some code quality concerns.

---

## Gap Analysis

### Critical Gaps

| Chunk | Planned Feature | Status | Gap |
|-------|-----------------|--------|-----|
| 6 | FloatingToolbar ratio presets (1:1, 3:4, 4:5, Freeform) | ❌ | Missing - only has Copy/Save/Cancel buttons |
| 5 | 8 resize handles (Thumb controls) | ❌ | Only basic rectangle, no 8-point resize |
| 7 | Live dimensions display (WxH) | ❌ | Not implemented |
| 8 | Instagram target dimensions (1:1=1080x1080, 3:4=1080x1350, 4:5=1080x1350) | ⚠️ | Only scales to 1080 width, doesn't respect ratio-specific heights |

### Partial Implementations

| Chunk | Planned Feature | Status | Notes |
|-------|-----------------|--------|-------|
| 10 | Global hotkey | ✅ | Ctrl+Shift+S works |
| 10 | System tray | ✅ | Implemented with NotifyIcon |
| 11 | Escape to cancel | ✅ | Implemented |
| 11 | Shift = unlock ratio | ❌ | Not implemented |
| 11 | Alt = resize from center | ✅ | Implemented (isCenterAnchored) |

---

## Code Quality Issues

### 1. ImageProcessor.cs:25-26 - Resource Leak
```csharp
SKBitmap cropped = new SKBitmap(cropRect.Width, cropRect.Height);
fullCapture.ExtractSubset(cropped, cropRect);
```
The `cropped` bitmap is created but `ExtractSubset` returns a copy, leaving the original allocated. Should use `using` or verify `ExtractSubset` behavior.

### 2. ImageProcessor.cs:12-37 - Instagram Aspect Ratios Not Enforced
The implementation only scales to 1080px width but doesn't enforce the target heights:
- 1:1 should force 1080x1080
- 3:4 should force 1080x1350
- 4:5 should force 1080x1350

Current logic just preserves the crop's aspect ratio, not the target.

### 3. SettingsService.cs:29,45 - Silent Exception Handling
```csharp
catch { }
```
Silent catch blocks hide errors. Should at least log or use a minimal diagnostic.

### 4. Win32Interop.cs:48 - GetDIBits P/Invoke Signature
```csharp
public static extern int GetDIBits(IntPtr hdc, IntPtr hBitmap, uint startScan, uint scanLines,
    IntPtr lpBits, [In] ref BITMAPINFO lpbi, uint usage);
```
The `lpBits` parameter should use a safe buffer or be carefully handled to avoid GC issues with SkiaSharp's pixel buffer.

### 5. FloatingToolbar.xaml.cs - Missing Ratio Presets
The toolbar should have buttons for 1:1, 3:4, 4:5, and Freeform ratio selection, but only has Save/Copy/Cancel.

### 6. AspectRatioRect.xaml - Missing 8 Resize Handles
The implementation plan specified 8 Thumb controls for corners and edges. Currently just a basic rectangle.

---

## Architecture Observations

### Positive
- Clean separation: Services, Models, Views, Controls, Helpers
- SelectionRect model properly encapsulates ratio logic
- Win32 interop is centralized in helpers
- Proper use of events for communication between components

### Concerns
- **Static classes**: ScreenCapture, ImageProcessor, SettingsService use static methods. This works but limits testability and makes DI harder if needed later.
- **Tight coupling**: OverlayWindow directly manipulates SelectionRect state rather than going through proper view models.
- **Missing interface abstractions**: For a snipping tool, the capture/processing could benefit from interfaces for mocking in tests.

---

## Documentation vs Reality

| Document | Reality |
|----------|---------|
| .NET 10 | ✅ Correct - .NET 10.0.8 released Nov 2025 (LTS), supports VS2026 |
| SkiaSharp 3.119.2 | Correct version |
| 1:1 = 1080x1080 | Not enforced - only scales to 1080 width |
| 3:4, 4:5 = 1080x1350 | Not enforced |

---

## Recommendations

### High Priority
1. **Fix ImageProcessor** to enforce specific Instagram dimensions based on selected ratio
2. **Add ratio preset buttons** to FloatingToolbar
3. **Add 8 resize handles** to AspectRatioRect control
4. **Add live dimensions display** to overlay

### Medium Priority
5. Replace silent catch blocks with proper logging
6. Fix GetDIBits interop for safer memory handling
7. Consider adding unit tests for SelectionRect math

### Low Priority
9. Extract interfaces for testability
10. Add Shift-key ratio unlock
11. Add application icon instead of SystemIcons.Information

---

## Build Check

The project correctly targets `net10.0-windows`. Requires .NET 10 SDK (10.0.300+) and Visual Studio 2026 for best experience.