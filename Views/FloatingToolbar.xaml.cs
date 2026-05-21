using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PsWinSnip.Views;

public partial class FloatingToolbar : System.Windows.Controls.UserControl
{
    public event EventHandler<double?>? RatioChanged;
    public event EventHandler? CopyRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;
    public event EventHandler? GridToggleRequested;
    public event EventHandler? SettingsRequested;

    public FloatingToolbar()
    {
        InitializeComponent();
    }

    public void SetGridState(bool isEnabled)
    {
        // Adjust BtnGrid Background to reflect whether it is active.
        BtnGrid.Background = isEnabled ? 
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x44, 0xFF, 0xFF, 0xFF)) : 
            System.Windows.Media.Brushes.Transparent;
    }

    private void OnRatioClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn)
        {
            if (double.TryParse(btn.Tag?.ToString(), out double ratio))
            {
                RatioChanged?.Invoke(this, ratio);
            }
            else
            {
                RatioChanged?.Invoke(this, null);
            }
        }
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        CopyRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SaveRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnGridToggleClick(object sender, RoutedEventArgs e)
    {
        GridToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }
}