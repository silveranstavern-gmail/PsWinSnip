using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using PsWinSnip.Services;
using PsWinSnip.Helpers;
using Forms = System.Windows.Forms;

namespace PsWinSnip;

public partial class MainWindow : Window
{
    private const int HotkeyId = 9000;
    private Forms.NotifyIcon? _notifyIcon;
    private HwndSource? _source;
    private bool _isExitRequested;
    private bool _widthDrivesHeight = true;
    private bool _isUpdatingDimensions = false;

    public MainWindow()
    {
        InitializeComponent();
        InitializeTrayIcon();
        
        // Ensure the window handle is created so hotkeys can be registered
        // even if the window is never shown.
        new WindowInteropHelper(this).EnsureHandle();
        
        RegisterHotkeys();
    }

    private void RegisterHotkeys()
    {
        var helper = new WindowInteropHelper(this);
        _source = HwndSource.FromHwnd(helper.Handle);
        _source.AddHook(HwndHook);

        Win32Interop.RegisterHotKey(helper.Handle, HotkeyId, Win32Interop.MOD_CONTROL | Win32Interop.MOD_SHIFT, 0x53); // 0x53 = 'S'
    }

    private void InitializeTrayIcon()
    {
        _notifyIcon = new Forms.NotifyIcon();
        _notifyIcon.Icon = System.Drawing.SystemIcons.Information; // Use a default icon
        _notifyIcon.Visible = true;
        _notifyIcon.Text = "PsWinSnip";
        _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add("Capture", null, (s, e) => StartCapture());
        contextMenu.Items.Add("Settings", null, (s, e) => ShowMainWindow());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => {
            _isExitRequested = true;
            System.Windows.Application.Current.Shutdown();
        });
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Hotkeys are now registered in the constructor via RegisterHotkeys()
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Win32Interop.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            StartCapture();
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void StartCapture()
    {
        var overlay = new Views.OverlayWindow();
        overlay.Show();
    }

    public void ShowMainWindow()
    {
        LoadSettingsToUI();
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
    }

    private void LoadSettingsToUI()
    {
        _isUpdatingDimensions = true;

        var settings = SettingsService.Load();
        
        // Match aspect ratio
        string ratioTag = settings.LastAspectRatio?.ToString("0.0") ?? "";
        if (settings.LastAspectRatio == 1.0) ratioTag = "1.0";
        else if (settings.LastAspectRatio == 0.8) ratioTag = "0.8";
        else if (settings.LastAspectRatio == 0.75) ratioTag = "0.75";

        foreach (ComboBoxItem item in ComboRatio.Items)
        {
            if (item.Tag?.ToString() == ratioTag)
            {
                ComboRatio.SelectedItem = item;
                break;
            }
        }

        TxtWidth.Text = settings.LastWidth.ToString();
        TxtHeight.Text = settings.LastHeight.ToString();
        ChkRememberPosition.IsChecked = settings.RememberPosition;
        ChkShowGrid.IsChecked = settings.ShowGrid;

        _isUpdatingDimensions = false;
        UpdateRatioConstraints();
    }

    private void OnComboRatioChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateRatioConstraints();
    }

    private void OnToggleDriverClick(object sender, RoutedEventArgs e)
    {
        _widthDrivesHeight = !_widthDrivesHeight;
        UpdateRatioConstraints();
    }

    private void UpdateRatioConstraints()
    {
        if (_isUpdatingDimensions || ComboRatio == null || TxtWidth == null || TxtHeight == null || BtnToggleDriver == null) return;

        bool hasRatio = ComboRatio.SelectedItem is ComboBoxItem item && !string.IsNullOrEmpty(item.Tag?.ToString());
        
        if (hasRatio)
        {
            BtnToggleDriver.Visibility = Visibility.Visible;
            BtnToggleDriver.Content = _widthDrivesHeight ? ">" : "<";
            
            TxtWidth.IsReadOnly = !_widthDrivesHeight;
            TxtWidth.Opacity = _widthDrivesHeight ? 1.0 : 0.5;
            
            TxtHeight.IsReadOnly = _widthDrivesHeight;
            TxtHeight.Opacity = _widthDrivesHeight ? 0.5 : 1.0;

            EnforceRatio();
        }
        else
        {
            BtnToggleDriver.Visibility = Visibility.Collapsed;
            TxtWidth.IsReadOnly = false;
            TxtWidth.Opacity = 1.0;
            TxtHeight.IsReadOnly = false;
            TxtHeight.Opacity = 1.0;
        }
    }

    private void OnDimensionTextChanged(object sender, TextChangedEventArgs e)
    {
        EnforceRatio();
    }

    private void EnforceRatio()
    {
        if (_isUpdatingDimensions || ComboRatio == null || TxtWidth == null || TxtHeight == null) return;

        if (ComboRatio.SelectedItem is ComboBoxItem item && double.TryParse(item.Tag?.ToString(), out double ratio))
        {
            _isUpdatingDimensions = true;

            if (_widthDrivesHeight && double.TryParse(TxtWidth.Text, out double w))
            {
                TxtHeight.Text = Math.Round(w / ratio).ToString();
            }
            else if (!_widthDrivesHeight && double.TryParse(TxtHeight.Text, out double h))
            {
                TxtWidth.Text = Math.Round(h * ratio).ToString();
            }

            _isUpdatingDimensions = false;
        }
    }

    private void OnNumericTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var settings = SettingsService.Load();

        if (ComboRatio.SelectedItem is ComboBoxItem selected && double.TryParse(selected.Tag?.ToString(), out double ratio))
        {
            settings.LastAspectRatio = ratio;
        }
        else
        {
            settings.LastAspectRatio = null;
        }

        if (double.TryParse(TxtWidth.Text, out double w)) settings.LastWidth = Math.Max(200, w);
        if (double.TryParse(TxtHeight.Text, out double h)) settings.LastHeight = Math.Max(200, h);
        
        settings.RememberPosition = ChkRememberPosition.IsChecked ?? true;
        settings.ShowGrid = ChkShowGrid.IsChecked ?? true;

        SettingsService.Save(settings);
        this.Hide();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            this.Hide();
        }
        base.OnStateChanged(e);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExitRequested)
        {
            // Close just hides the window to keep it in tray
            e.Cancel = true;
            this.Hide();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _source?.RemoveHook(HwndHook);
        Win32Interop.UnregisterHotKey(new WindowInteropHelper(this).Handle, HotkeyId);
        _notifyIcon?.Dispose();
        base.OnClosed(e);
    }
}