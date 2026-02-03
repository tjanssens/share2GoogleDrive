using System.IO;
using System.Windows;
using Share2GoogleDrive.Helpers;
using Share2GoogleDrive.Models;
using Share2GoogleDrive.Services;
using Share2GoogleDrive.ViewModels;
using Share2GoogleDrive.Views;
using Serilog;

namespace Share2GoogleDrive;

public partial class App : Application
{
    private ISettingsService _settingsService = null!;
    private IGoogleAuthService _authService = null!;
    private IGoogleDriveService _driveService = null!;
    private IUploadService _uploadService = null!;
    private IHotkeyService _hotkeyService = null!;
    private INotificationService _notificationService = null!;
    private ITrayIconService _trayIconService = null!;

    private MainWindow? _mainWindow;

    // Public properties for access from other components
    public IGoogleDriveService DriveService => _driveService;
    public IUploadService UploadService => _uploadService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize logging
        InitializeLogging();

        Log.Information("Share2GoogleDrive starting...");

        try
        {
            // Initialize services
            await InitializeServicesAsync();

            // Check if started with file argument (from context menu)
            if (e.Args.Length > 0 && File.Exists(e.Args[0]))
            {
                await HandleFileUploadAsync(e.Args[0]);

                // If just uploading a file, exit after upload unless already running
                if (!IsAlreadyRunning())
                {
                    Shutdown();
                    return;
                }
            }

            // Check if started minimized
            bool startMinimized = e.Args.Contains("--minimized");

            // Initialize tray icon
            _trayIconService.Initialize();
            _trayIconService.SettingsRequested += OnSettingsRequested;
            _trayIconService.ExitRequested += OnExitRequested;

            // Register hotkey if enabled
            if (_settingsService.Settings.Hotkey.Enabled)
            {
                var settings = _settingsService.Settings.Hotkey;
                _hotkeyService.Register(settings.Modifiers, settings.Key);
            }

            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            // Show main window if not minimized
            if (!startMinimized)
            {
                ShowMainWindow();
            }

            Log.Information("Share2GoogleDrive started successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to start Share2GoogleDrive");
            MessageBox.Show($"Failed to start application: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void InitializeLogging()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Share2GoogleDrive");

        Directory.CreateDirectory(appDataPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(appDataPath, "logs", "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();
    }

    private async Task InitializeServicesAsync()
    {
        // Create services
        _settingsService = new SettingsService();
        await _settingsService.LoadAsync();

        _authService = new GoogleAuthService(_settingsService);
        _driveService = new GoogleDriveService(_authService, _settingsService);
        _notificationService = new NotificationService();
        _uploadService = new UploadService(_driveService, _settingsService, _notificationService);
        _hotkeyService = new HotkeyService();
        _trayIconService = new TrayIconService();

        // Subscribe to conflict events
        _uploadService.ConflictDetected += OnConflictDetected;
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null)
        {
            var viewModel = new MainViewModel(_settingsService, _authService, _hotkeyService);
            _mainWindow = new MainWindow(viewModel);
        }

        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void OnSettingsRequested(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(ShowMainWindow);
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        Log.Information("Exit requested");
        Shutdown();
    }

    private async void OnHotkeyPressed(object? sender, EventArgs e)
    {
        Log.Debug("Hotkey pressed, checking for selected file");

        var selectedFile = ExplorerHelper.GetSelectedFile();
        if (string.IsNullOrEmpty(selectedFile))
        {
            _notificationService.ShowInfo("No File Selected",
                "Please select a file in Windows Explorer first.");
            return;
        }

        await HandleFileUploadAsync(selectedFile);
    }

    private async Task HandleFileUploadAsync(string filePath)
    {
        Log.Information("Starting upload for file: {FilePath}", filePath);

        try
        {
            // Check if authenticated
            if (!await _authService.IsAuthenticatedAsync())
            {
                var result = MessageBox.Show(
                    "You need to sign in to Google Drive first. Would you like to sign in now?",
                    "Authentication Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _authService.AuthenticateAsync();
                }
                else
                {
                    return;
                }
            }

            await _uploadService.UploadFileAsync(filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Upload failed for {FilePath}", filePath);
            _notificationService.ShowUploadFailed(Path.GetFileName(filePath), ex.Message);
        }
    }

    private void OnConflictDetected(object? sender, ConflictEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var dialog = new ConflictDialog(e.FileName);
            dialog.ShowDialog();
            e.ResponseSource.SetResult(dialog.Result);
        });
    }

    private bool IsAlreadyRunning()
    {
        // Check if another instance is running by checking the tray icon service
        return _trayIconService != null;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Share2GoogleDrive shutting down");

        _hotkeyService?.Dispose();
        _trayIconService?.Dispose();

        Log.CloseAndFlush();

        base.OnExit(e);
    }
}
