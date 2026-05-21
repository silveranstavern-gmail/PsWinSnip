using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using SkiaSharp;
using PsWinSnip.Models;
using PsWinSnip.Helpers;
using PsWinSnip.Services;

namespace PsWinSnip.Views;

public partial class OverlayWindow : Window
{
    public event EventHandler? CancelRequested;

    private SelectionRect _selection = new();
    private Rect _virtualScreenBounds;

    public SelectionRect Selection => _selection;

    public OverlayWindow()
    {
        InitializeComponent();
        InitializeVirtualScreen();
        InitializeSelection();
        KeyDown += OnKeyDown;
    }

    private void InitializeVirtualScreen()
    {
        // Get bounds in WPF device-independent pixels (DIPs)
        double x = SystemParameters.VirtualScreenLeft;
        double y = SystemParameters.VirtualScreenTop;
        double width = SystemParameters.VirtualScreenWidth;
        double height = SystemParameters.VirtualScreenHeight;

        _virtualScreenBounds = new Rect(x, y, width, height);

        Left = x;
        Top = y;
        Width = width;
        Height = height;

        WholeScreenGeometry.Rect = new Rect(0, 0, width, height);
    }

    private void InitializeSelection()
    {
        var settings = SettingsService.Load();

        double width = settings.LastWidth;
        double height = settings.LastHeight;
        
        // Defensive: Clamp dimensions to screen bounds in case of monitor disconnection
        if (width > _virtualScreenBounds.Width) width = _virtualScreenBounds.Width;
        if (height > _virtualScreenBounds.Height) height = _virtualScreenBounds.Height;

        double x, y;

        if (settings.RememberPosition && settings.LastX.HasValue && settings.LastY.HasValue)
        {
            x = settings.LastX.Value;
            y = settings.LastY.Value;
        }
        else
        {
            var cursor = System.Windows.Forms.Cursor.Position;
            var screen = System.Windows.Forms.Screen.FromPoint(cursor);
            var dpi = VisualTreeHelper.GetDpi(this);

            double screenDipsX = screen.Bounds.X / dpi.DpiScaleX;
            double screenDipsY = screen.Bounds.Y / dpi.DpiScaleY;
            double screenDipsWidth = screen.Bounds.Width / dpi.DpiScaleX;
            double screenDipsHeight = screen.Bounds.Height / dpi.DpiScaleY;
            
            double screenLocalX = screenDipsX - _virtualScreenBounds.X;
            double screenLocalY = screenDipsY - _virtualScreenBounds.Y;

            x = screenLocalX + (screenDipsWidth - width) / 2;
            y = screenLocalY + (screenDipsHeight - height) / 2;
        }

        // Ensure within bounds
        if (x + width > _virtualScreenBounds.Width) x = Math.Max(0, _virtualScreenBounds.Width - width);
        if (y + height > _virtualScreenBounds.Height) y = Math.Max(0, _virtualScreenBounds.Height - height);
        if (x < 0) x = 0;
        if (y < 0) y = 0;

        _selection = new SelectionRect(x, y, width, height, settings.LastAspectRatio);

        SelectionControl.Initialize(_selection);
        SelectionControl.Visibility = Visibility.Visible;
        SelectionControl.SetGridVisibility(settings.ShowGrid);
        Toolbar.SetGridState(settings.ShowGrid);

        SelectionControl.SelectionChanged += (s, e) => {
            UpdateHole();
            UpdateToolbarPosition();
        };

        Toolbar.Visibility = Visibility.Visible;
        Toolbar.RatioChanged += (s, ratio) => {
            _selection.SetRatio(ratio);
            SelectionControl.UpdateUI();
            UpdateHole();
            UpdateToolbarPosition();

            settings.LastAspectRatio = ratio;
            SettingsService.Save(settings);
        };
        Toolbar.GridToggleRequested += (s, e) => {
            settings.ShowGrid = !settings.ShowGrid;
            SelectionControl.SetGridVisibility(settings.ShowGrid);
            Toolbar.SetGridState(settings.ShowGrid);
            SettingsService.Save(settings);
        };
        Toolbar.SettingsRequested += (s, e) => {
            Close();
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWin)
            {
                mainWin.ShowMainWindow();
            }
        };
        Toolbar.CopyRequested += (s, e) => ExecuteCapture(true);
        Toolbar.SaveRequested += (s, e) => ExecuteCapture(false);
        Toolbar.CancelRequested += (s, e) => Close();

        UpdateHole();
        UpdateToolbarPosition();
    }

    private void UpdateToolbarPosition()
    {
        Toolbar.UpdateLayout();

        // Position toolbar at the top right of the selection
        double x = _selection.X + _selection.Width - Toolbar.ActualWidth;
        double y = _selection.Y - Toolbar.ActualHeight - 10;

        // If it goes off the top, put it inside or below
        if (y < 0)
        {
            y = _selection.Y + 10;
        }

        // If it goes off the left, align to left
        if (x < 0)
        {
            x = _selection.X;
        }

        // Clamp to window bounds using logical bounds (ActualWidth is 0 during constructor)
        double boundsWidth = ActualWidth > 0 ? ActualWidth : _virtualScreenBounds.Width;
        double boundsHeight = ActualHeight > 0 ? ActualHeight : _virtualScreenBounds.Height;

        x = Math.Max(0, Math.Min(x, boundsWidth - Toolbar.ActualWidth));
        y = Math.Max(0, Math.Min(y, boundsHeight - Toolbar.ActualHeight));

        Canvas.SetLeft(Toolbar, x);
        Canvas.SetTop(Toolbar, y);
    }

    private void UpdateHole()
    {
        SelectionGeometry.Rect = _selection.ToRect();
    }

    private void OnBackgroundClick(object sender, MouseButtonEventArgs e)
    {
        // Removed click-to-recenter behavior to avoid accidental movement
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
            Close();
        }
        else if (e.Key == Key.Enter)
        {
            ExecuteCapture(false); // Default to save for now
        }
        else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            ExecuteCapture(true); // Copy to clipboard
        }
    }

    private Rect GetSelectionInScreenPixels()
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        var rect = _selection.ToRect();

        // Convert WPF DIPs to physical pixels
        double x = rect.X * dpi.DpiScaleX;
        double y = rect.Y * dpi.DpiScaleY;
        double width = rect.Width * dpi.DpiScaleX;
        double height = rect.Height * dpi.DpiScaleY;

        return new Rect(x, y, width, height);
    }

    public async void ExecuteCapture(bool copyToClipboard)
    {
        // Save state before hiding/closing
        var settings = SettingsService.Load();
        settings.LastWidth = _selection.Width;
        settings.LastHeight = _selection.Height;
        if (settings.RememberPosition)
        {
            settings.LastX = _selection.X;
            settings.LastY = _selection.Y;
        }
        SettingsService.Save(settings);

        // Hide the overlay temporarily to capture the screen underneath
        Hide();
        
        // Wait for the window to actually hide and the UI to settle
        await System.Threading.Tasks.Task.Delay(150);

        try
        {
            using var fullBitmap = ScreenCapture.CaptureFullScreen();
            var physicalRect = GetSelectionInScreenPixels();
            using var processed = ImageProcessor.CropCapture(fullBitmap, physicalRect);

            if (copyToClipboard)
            {
                ImageProcessor.CopyToClipboard(processed);
                System.Windows.MessageBox.Show("Captured and copied to clipboard!", "PsWinSnip");
            }
            else
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg",
                    FileName = $"PsWinSnip_{DateTime.Now:yyyyMMdd_HHmmss}.png",
                    DefaultExt = ".png"
                };

                if (dialog.ShowDialog() == true)
                {
                    // Use PNG if requested or by default
                    if (dialog.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        using var image = SKImage.FromBitmap(processed);
                        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                        using var stream = File.Create(dialog.FileName);
                        data.SaveTo(stream);
                    }
                    else
                    {
                        ScreenCapture.SaveBitmap(processed, dialog.FileName);
                    }
                    System.Windows.MessageBox.Show($"Saved to:\n{dialog.FileName}", "PsWinSnip");
                }
            }
        }
        finally
        {
            Close();
        }
    }

    public void UpdateSelection(SelectionRect selection)
    {
        _selection = selection;
        // In Chunk 4 we will sync this to the UI
    }

    public void SetRatio(double? ratio)
    {
        _selection.SetRatio(ratio);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        Focus();
    }

    public Rect GetCurrentBounds() => new Rect(Left, Top, ActualWidth, ActualHeight);
}