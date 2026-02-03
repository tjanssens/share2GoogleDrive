using System.Drawing;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Serilog;

namespace Share2GoogleDrive.Services;

/// <summary>
/// Service for managing the system tray icon.
/// </summary>
public interface ITrayIconService : IDisposable
{
    void Initialize();
    void ShowBalloon(string title, string message, BalloonIcon icon = BalloonIcon.Info);
    event EventHandler? SettingsRequested;
    event EventHandler? ExitRequested;
}

public class TrayIconService : ITrayIconService
{
    private TaskbarIcon? _trayIcon;

    public event EventHandler? SettingsRequested;
    public event EventHandler? ExitRequested;

    public void Initialize()
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _trayIcon = new TaskbarIcon
                {
                    ToolTipText = "Share2GoogleDrive",
                    Icon = LoadIcon(),
                    ContextMenu = CreateContextMenu()
                };

                _trayIcon.TrayMouseDoubleClick += OnTrayDoubleClick;
            });

            Log.Information("Tray icon initialized");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize tray icon");
            throw;
        }
    }

    private Icon LoadIcon()
    {
        try
        {
            // Try to load embedded icon
            var resourceStream = Application.GetResourceStream(
                new Uri("pack://application:,,,/Resources/Icons/app.ico"));

            if (resourceStream != null)
            {
                return new Icon(resourceStream.Stream);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load custom icon, using default");
        }

        // Create a simple default icon
        return SystemIcons.Application;
    }

    private System.Windows.Controls.ContextMenu CreateContextMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();

        var settingsItem = new System.Windows.Controls.MenuItem { Header = "Settings" };
        settingsItem.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(settingsItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnTrayDoubleClick(object sender, RoutedEventArgs e)
    {
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ShowBalloon(string title, string message, BalloonIcon icon = BalloonIcon.Info)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _trayIcon?.ShowBalloonTip(title, message, icon);
        });
    }

    public void Dispose()
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
        });
    }
}
