using System.Drawing;
using System.Windows;
using System.Windows.Threading;
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
    void StartUploadAnimation();
    void StopUploadAnimation();
    event EventHandler? SettingsRequested;
    event EventHandler? ExitRequested;
}

public class TrayIconService : ITrayIconService
{
    private TaskbarIcon? _trayIcon;
    private Icon? _staticIcon;
    private Icon[]? _animationFrames;
    private DispatcherTimer? _animationTimer;
    private int _currentFrame;
    private bool _isAnimating;

    public event EventHandler? SettingsRequested;
    public event EventHandler? ExitRequested;

    public void Initialize()
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _staticIcon = LoadIcon("app.ico");
                _animationFrames = LoadAnimationFrames();

                _trayIcon = new TaskbarIcon
                {
                    ToolTipText = "It's-a me, Mario! Ready to deliver your files! 🍄",
                    Icon = _staticIcon,
                    ContextMenu = CreateContextMenu()
                };

                _trayIcon.TrayMouseDoubleClick += OnTrayDoubleClick;

                // Setup animation timer
                _animationTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(150)
                };
                _animationTimer.Tick += OnAnimationTick;
            });

            Log.Information("Tray icon initialized");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize tray icon");
            throw;
        }
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (_trayIcon == null || _animationFrames == null || _animationFrames.Length == 0)
            return;

        _currentFrame = (_currentFrame + 1) % _animationFrames.Length;
        _trayIcon.Icon = _animationFrames[_currentFrame];
    }

    public void StartUploadAnimation()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_isAnimating || _animationFrames == null || _animationFrames.Length == 0)
                return;

            _isAnimating = true;
            _currentFrame = 0;
            _animationTimer?.Start();
            Log.Debug("Upload animation started");
        });
    }

    public void StopUploadAnimation()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!_isAnimating)
                return;

            _isAnimating = false;
            _animationTimer?.Stop();
            if (_trayIcon != null && _staticIcon != null)
            {
                _trayIcon.Icon = _staticIcon;
            }
            Log.Debug("Upload animation stopped");
        });
    }

    private Icon LoadIcon(string iconName)
    {
        try
        {
            var resourceStream = Application.GetResourceStream(
                new Uri($"pack://application:,,,/Resources/Icons/{iconName}"));

            if (resourceStream != null)
            {
                return new Icon(resourceStream.Stream);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load icon {IconName}, using default", iconName);
        }

        return SystemIcons.Application;
    }

    private Icon[] LoadAnimationFrames()
    {
        var frames = new List<Icon>();

        for (int i = 1; i <= 6; i++)
        {
            try
            {
                var resourceStream = Application.GetResourceStream(
                    new Uri($"pack://application:,,,/Resources/Icons/walk_{i}.ico"));

                if (resourceStream != null)
                {
                    frames.Add(new Icon(resourceStream.Stream));
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load animation frame {Frame}", i);
            }
        }

        Log.Information("Loaded {Count} animation frames", frames.Count);
        return frames.ToArray();
    }

    private System.Windows.Controls.ContextMenu CreateContextMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();

        var settingsItem = new System.Windows.Controls.MenuItem { Header = "⚙️ Warp Zone (Settings)" };
        settingsItem.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(settingsItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "🚪 Exit the Castle" };
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
