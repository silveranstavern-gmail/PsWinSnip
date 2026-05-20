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

    public FloatingToolbar()
    {
        InitializeComponent();
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
}