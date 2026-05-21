# PsWinSnip Vision

A Windows snipping tool optimized for precise aspect ratio control and pixel-perfect screen cropping.

## Problem

Existing Windows snipping tools capture arbitrary regions but don't help with specific aspect ratio requirements. Users must manually crop screenshots to fit specific ratios for design, presentation, or social media.

## Solution

PsWinSnip captures screenshots with pre-configured aspect ratios, outputting images that match the selection exactly without unnecessary resizing or scaling.

## Core Features

1. **Aspect Ratio Selection**
   - 1:1 - Square
   - 3:4 - Portrait
   - 4:5 - Vertical
   - Free (user-defined)

2. **Screen Capture**
   - Full screen capture
   - Drag-to-select region constrained to chosen aspect ratio
   - Accurate pixel-level selection

3. **Output**
   - High-quality PNG/JPEG output preserving original pixels
   - Precise crop to the selected area
   - Save to file or copy to clipboard

## Target Users

- Designers and Developers
- Content creators
- Anyone who needs precise aspect-ratio screen crops

## Tech Stack

- .NET 10 (WPF)
- SkiaSharp for image processing
- Win32 P/Invoke for screen capture