Input:

> Research the best libraries I can leverage for my snipping app that are free, maintained, and powerful enough to allow me to specify size constraints and ratios. I think I want a visible overlay that with a movable tray in the top right that contains a capture button, and 3 or 4 quick ratio locks that snap the capture area into the desired ratio and allow you to move it and resize it while maintaining the ratio, and clicking capture saves the file.

Output:

Given your stack and vision for [SkiaSharp](https://github.com/mono/SkiaSharp?utm_source=chatgpt.com) + WPF + Win32 capture, I would not pull in a heavyweight cropping framework. Your app is already narrowly scoped and the interaction model you described is specific enough that a custom overlay layer will likely be cleaner, faster, and easier to maintain than adapting a general-purpose image editor.

Your current architecture from the uploaded docs is already aligned with the right approach:

- WPF for transparent overlay windows
- Win32 for capture
- SkiaSharp for crop/resize/export
- Fixed-ratio interactive selection region
- Instagram-oriented presets (1:1, 3:4, 4:5)

The strongest libraries/components for this are:

1. [SkiaSharp](https://github.com/mono/SkiaSharp?utm_source=chatgpt.com) — keep this as your rendering + export engine

This should remain your core graphics dependency.

Why:

- MIT licensed
- Very mature
- GPU-capable if needed later
- Excellent resize/crop quality
- Cross-platform future option
- Active maintenance resumed/co-maintained in 2026 ([Reddit](https://www.reddit.com/r/unoplatform/comments/1syaf57/uno_platform_is_now_comaintaining_skiasharp_plus/?utm_source=chatgpt.com))

Most importantly: SkiaSharp already supports the exact crop rectangle workflow you need, including aspect-ratio constrained resizing. Microsoft’s own sample demonstrates:

- draggable crop rectangle
- corner handles
- locked aspect ratio
- resize constraints
- extracting cropped bitmap ([Mono](https://mono.github.io/SkiaSharp/docs/bitmaps/cropping.html?utm_source=chatgpt.com))

That sample is almost a direct prototype for your interaction model.

I would adapt that design into:

- a fullscreen transparent WPF overlay window
- one custom “SelectionRect” model
- optional Skia overlay renderer
- capture toolbar floating near the rect

Notably, the Microsoft sample already includes:

- `MoveCorner`
- ratio locking
- resize boundaries
- snapping logic
- minimum sizes ([Mono](https://mono.github.io/SkiaSharp/docs/bitmaps/cropping.html?utm_source=chatgpt.com))

So instead of searching for a “crop library,” you can extract the interaction math from that sample and modernize it for WPF desktop.

1. [Screenshot.NET](https://github.com/gilyes/Screenshot.NET?utm_source=chatgpt.com) — useful reference implementation, not necessarily a dependency

This is worth studying for:

- region selection UX
- overlay behavior
- input handling
- capture workflow

But I probably would not depend on it directly because:

- you already have Win32 capture plans
- your ratio-lock behavior is specialized
- your UX differs from generic snipping tools

Still, it can save time for:

- ESC cancel handling
- multi-monitor coordinate normalization
- DPI-awareness edge cases
- overlay activation quirks ([GitHub](https://github.com/gilyes/Screenshot.NET?utm_source=chatgpt.com))

1. Native WPF `Thumb` controls — probably your best resize-handle solution

This is the most underrated option.

You do not actually need a graphics-heavy cropper component.

A very solid architecture is:

- transparent borderless fullscreen WPF window
- Canvas root
- dark translucent backdrop
- selection rectangle element
- 8 `Thumb` controls for resize handles
- drag logic on center region
- ratio constraint logic in code

Why this is better:

- native WPF hit-testing
- no weird input interop
- easier accessibility
- cleaner resize semantics
- simpler cursor handling
- excellent performance

This is how many desktop design tools are implemented internally.

Your ratio logic becomes straightforward:

```csharp
height = width / aspectRatio;
```

or:

```csharp
width = height * aspectRatio;
```

depending on active drag edge/corner.

The key is:

- one source-of-truth rect
- all handles mutate that rect
- render updates from the rect only

1. WinUI/Avalonia libraries: probably not worth it

You may see recommendations for:

- Cropper.Blazor
- ImageCropper libraries
- Avalonia crop tools
- WinUI Community Toolkit

I would avoid all of them for this app.

Reasons:

- extra abstraction
- harder overlay integration
- less control over fullscreen transparent behavior
- many are image-editor oriented rather than screen-region oriented
- ratio-locking often becomes awkward

Your app is fundamentally:
“interactive geometry over a live desktop surface.”

That is closer to:

- selection tools
- CAD interaction
- design software overlays

than traditional image editors.

Recommended architecture

I think this is the cleanest structure for PsWinSnip:

```text
OverlayWindow
 ├── Dimmed background
 ├── SelectionRectangle
 │    ├── Resize thumbs
 │    ├── Ratio enforcement
 │    ├── Move behavior
 │    └── Bounds constraints
 ├── FloatingToolbar
 │    ├── Capture button
 │    ├── Ratio presets
 │    ├── Copy
 │    └── Cancel
 └── Live dimensions label
```

For your floating toolbar:

- anchor it to top-right of the selection rect
- auto-flip to inside/top-left if near screen edge
- use “AlwaysOnTop within overlay”

The UX you described is good. I would keep it minimal:

- Capture
- 1:1
- 4:5
- 3:4
- Freeform

Possibly:

- Shift = temporarily unlock ratio
- Alt = resize from center

That gives it a “professional tool” feel without complexity.

What I would personally use in your exact app

Core:

- WPF
- SkiaSharp
- Win32 capture APIs
- Native WPF Thumb controls

No third-party crop UI framework.

Why:

- smallest dependency surface
- easiest maintenance
- best control
- simplest debugging
- easiest DPI handling
- easiest transparent-overlay behavior

The hardest parts of your app are not cropping. They are:

- multi-monitor DPI correctness
- overlay rendering performance
- coordinate translation
- screen-edge behavior
- transparent click handling
- smooth resize interaction

Libraries do not help much there.

One architectural suggestion

Do not couple the selection rectangle to the rendering system.

Instead:

```csharp
record SelectionRect(
    double X,
    double Y,
    double Width,
    double Height,
    double AspectRatio
);
```

Then:

- overlay renders from state
- toolbar reads from state
- capture uses state
- handles mutate state

This keeps the entire app predictable.

Your uploaded vision document is already scoped correctly. The most important thing now is not finding a massive library — it is implementing a polished interaction model with a very small, stable core.