using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        // Get bounds in device pixels
        int x = Win32Interop.GetSystemMetrics(Win32Interop.SM_XVIRTUALSCREEN);
        int y = Win32Interop.GetSystemMetrics(Win32Interop.SM_YVIRTUALSCREEN);
        int width = Win32Interop.GetSystemMetrics(Win32Interop.SM_CXVIRTUALSCREEN);
        int height = Win32Interop.GetSystemMetrics(Win32Interop.SM_CYVIRTUALSCREEN);

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
        _selection = new SelectionRect(100, 100, 400, 400, settings.LastAspectRatio);
        
        SelectionControl.Initialize(_selection);
        SelectionControl.Visibility = Visibility.Visible;
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

            // Save the ratio preference
            settings.LastAspectRatio = ratio;
            SettingsService.Save(settings);
        };
        Toolbar.CopyRequested += (s, e) => ExecuteCapture(true);
        Toolbar.SaveRequested += (s, e) => ExecuteCapture(false);
        Toolbar.CancelRequested += (s, e) => Close();

        UpdateHole();
        UpdateToolbarPosition();
    }

    private void UpdateToolbarPosition()
    {
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

        Canvas.SetLeft(Toolbar, x);
        Canvas.SetTop(Toolbar, y);
    }

    private void UpdateHole()
    {
        SelectionGeometry.Rect = _selection.ToRect();
    }

    private void OnBackgroundClick(object sender, MouseButtonEventArgs e)
    {
        // For now, let's allow moving the selection to the click point
        System.Windows.Point pos = e.GetPosition(SelectionCanvas);
        _selection.X = pos.X - _selection.Width / 2;
        _selection.Y = pos.Y - _selection.Height / 2;
        
        Rect bounds = new Rect(0, 0, SelectionCanvas.ActualWidth, SelectionCanvas.ActualHeight);
        _selection.Move(0, 0, bounds); // Just to trigger constraints

        SelectionControl.UpdateUI();
        UpdateHole();
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

    public async void ExecuteCapture(bool copyToClipboard)
    {
        // Hide the overlay temporarily to capture the screen underneath
        Hide();
        
        // Wait for the window to actually hide and the UI to settle
        await System.Threading.Tasks.Task.Delay(100);

        try
        {
            using var fullBitmap = ScreenCapture.CaptureFullScreen();
            using var processed = ImageProcessor.ProcessCapture(fullBitmap, _selection.ToRect());

            if (copyToClipboard)
            {
                ImageProcessor.CopyToClipboard(processed);
                System.Windows.MessageBox.Show("Captured and copied to clipboard!", "PsWinSnip");
            }
            else
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JPEG Image|*.jpg",
                    FileName = $"PsWinSnip_{DateTime.Now:yyyyMMdd_HHmmss}.jpg",
                    DefaultExt = ".jpg"
                };

                if (dialog.ShowDialog() == true)
                {
                    ScreenCapture.SaveBitmap(processed, dialog.FileName);
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