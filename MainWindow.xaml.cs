using System.Windows;
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

    public MainWindow()
    {
        InitializeComponent();
        InitializeTrayIcon();
        Loaded += OnLoaded;
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
        contextMenu.Items.Add("Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Register Global Hotkey: Ctrl + Shift + S
        var helper = new WindowInteropHelper(this);
        _source = HwndSource.FromHwnd(helper.Handle);
        _source.AddHook(HwndHook);

        Win32Interop.RegisterHotKey(helper.Handle, HotkeyId, Win32Interop.MOD_CONTROL | Win32Interop.MOD_SHIFT, 0x53); // 0x53 = 'S'
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

    private void ShowMainWindow()
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
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
        // Close just hides the window to keep it in tray
        e.Cancel = true;
        this.Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
        _source?.RemoveHook(HwndHook);
        Win32Interop.UnregisterHotKey(new WindowInteropHelper(this).Handle, HotkeyId);
        _notifyIcon?.Dispose();
        base.OnClosed(e);
    }
}