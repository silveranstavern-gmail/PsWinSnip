# PsWinSnip Vision

A Windows snipping tool optimized for creating Instagram-ready screenshots with precise aspect ratio control.

## Problem

Existing Windows snipping tools capture arbitrary regions but don't help with social media formatting. Users must manually crop and resize screenshots to fit Instagram's preferred aspect ratios (1:1, 3:4, 4:5).

## Solution

PsWinSnip captures screenshots with pre-configured Instagram aspect ratios, outputting images already sized for upload without additional cropping.

## Core Features

1. **Aspect Ratio Selection**
   - 1:1 (1080x1080) - Square
   - 3:4 (1080x1350) - Portrait
   - 4:5 (1080x1350) - Instagram portrait
   - Custom (user-defined)

2. **Screen Capture**
   - Full screen capture
   - Drag-to-select region constrained to chosen aspect ratio
   - Live preview of final output during selection

3. **Output**
   - High-quality JPEG output optimized for Instagram
   - Automatic resize/crop to exact Instagram dimensions
   - Save to file or copy to clipboard

## Target Users

- Social media managers
- Content creators
- Anyone who shares screenshots on Instagram

## Tech Stack

- .NET 10 (WPF)
- SkiaSharp for image processing
- Win32 P/Invoke for screen capture