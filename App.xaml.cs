using System.Configuration;
using System.Data;
using System.Windows;

namespace PsWinSnip;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Instantiate the MainWindow to register hotkeys and set up the tray icon
        var mainWindow = new MainWindow();
        this.MainWindow = mainWindow;

        // Do not call mainWindow.Show() here so it remains hidden in the tray on start.
    }
}

