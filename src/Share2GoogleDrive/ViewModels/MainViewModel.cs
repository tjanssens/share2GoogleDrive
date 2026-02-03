using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Share2GoogleDrive.Models;
using Share2GoogleDrive.Services;
using Share2GoogleDrive.Helpers;
using Serilog;

namespace Share2GoogleDrive.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IGoogleAuthService _authService;
    private readonly IHotkeyService _hotkeyService;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string? _accountEmail;

    [ObservableProperty]
    private string _targetFolderName = "My Drive";

    [ObservableProperty]
    private string? _targetFolderId;

    [ObservableProperty]
    private bool _openInBrowserAfterUpload = true;

    [ObservableProperty]
    private bool _showNotifications = true;

    [ObservableProperty]
    private bool _hotkeyEnabled = true;

    [ObservableProperty]
    private ModifierKeys _hotkeyModifiers = ModifierKeys.Control;

    [ObservableProperty]
    private Key _hotkeyKey = Key.G;

    [ObservableProperty]
    private bool _autostart;

    [ObservableProperty]
    private bool _contextMenuRegistered;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    public MainViewModel(
        ISettingsService settingsService,
        IGoogleAuthService authService,
        IHotkeyService hotkeyService)
    {
        _settingsService = settingsService;
        _authService = authService;
        _hotkeyService = hotkeyService;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading settings...";

        try
        {
            await _settingsService.LoadAsync();
            LoadSettingsToViewModel();

            // Check authentication status
            IsConnected = await _authService.IsAuthenticatedAsync();
            if (IsConnected)
            {
                AccountEmail = _settingsService.Settings.Account.Email;
            }

            // Check context menu status
            ContextMenuRegistered = RegistryHelper.IsContextMenuRegistered();

            // Check autostart status
            Autostart = RegistryHelper.IsAutostartEnabled();

            StatusMessage = null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize MainViewModel");
            StatusMessage = "Failed to load settings";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadSettingsToViewModel()
    {
        var settings = _settingsService.Settings;

        AccountEmail = settings.Account.Email;
        IsConnected = settings.Account.Connected;

        TargetFolderId = settings.Upload.DefaultFolderId;
        TargetFolderName = settings.Upload.DefaultFolderName;
        OpenInBrowserAfterUpload = settings.Upload.OpenInBrowserAfterUpload;
        ShowNotifications = settings.Upload.NotifyOnComplete;

        HotkeyEnabled = settings.Hotkey.Enabled;
        HotkeyModifiers = settings.Hotkey.Modifiers;
        HotkeyKey = settings.Hotkey.Key;

        Autostart = settings.General.Autostart;
    }

    private void SaveViewModelToSettings()
    {
        var settings = _settingsService.Settings;

        settings.Account.Email = AccountEmail;
        settings.Account.Connected = IsConnected;

        settings.Upload.DefaultFolderId = TargetFolderId;
        settings.Upload.DefaultFolderName = TargetFolderName;
        settings.Upload.OpenInBrowserAfterUpload = OpenInBrowserAfterUpload;
        settings.Upload.NotifyOnComplete = ShowNotifications;

        settings.Hotkey.Enabled = HotkeyEnabled;
        settings.Hotkey.Modifiers = HotkeyModifiers;
        settings.Hotkey.Key = HotkeyKey;

        settings.General.Autostart = Autostart;
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        IsLoading = true;
        StatusMessage = "Signing in...";

        try
        {
            await _authService.AuthenticateAsync();
            AccountEmail = await _authService.GetUserEmailAsync();
            IsConnected = true;

            SaveViewModelToSettings();
            await _settingsService.SaveAsync();

            StatusMessage = "Signed in successfully";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Sign in failed");
            StatusMessage = $"Sign in failed: {ex.Message}";
            IsConnected = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        IsLoading = true;
        StatusMessage = "Signing out...";

        try
        {
            await _authService.SignOutAsync();
            AccountEmail = null;
            IsConnected = false;

            SaveViewModelToSettings();
            await _settingsService.SaveAsync();

            StatusMessage = "Signed out successfully";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Sign out failed");
            StatusMessage = $"Sign out failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        IsLoading = true;
        StatusMessage = "Saving settings...";

        try
        {
            SaveViewModelToSettings();
            await _settingsService.SaveAsync();

            // Update hotkey registration
            if (HotkeyEnabled)
            {
                _hotkeyService.Register(HotkeyModifiers, HotkeyKey);
            }
            else
            {
                _hotkeyService.Unregister();
            }

            // Update autostart
            var exePath = Environment.ProcessPath ?? string.Empty;
            if (Autostart)
            {
                RegistryHelper.EnableAutostart(exePath);
            }
            else
            {
                RegistryHelper.DisableAutostart();
            }

            StatusMessage = "Settings saved";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save settings");
            StatusMessage = $"Failed to save: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void RegisterContextMenu()
    {
        var exePath = Environment.ProcessPath ?? string.Empty;
        if (RegistryHelper.RegisterContextMenu(exePath))
        {
            ContextMenuRegistered = true;
            StatusMessage = "Context menu registered";
        }
        else
        {
            StatusMessage = "Failed to register context menu";
        }
    }

    [RelayCommand]
    private void UnregisterContextMenu()
    {
        if (RegistryHelper.UnregisterContextMenu())
        {
            ContextMenuRegistered = false;
            StatusMessage = "Context menu removed";
        }
        else
        {
            StatusMessage = "Failed to remove context menu";
        }
    }

    [RelayCommand]
    private void ResetHotkey()
    {
        HotkeyModifiers = ModifierKeys.Control;
        HotkeyKey = Key.G;
    }

    public void SetTargetFolder(string folderId, string folderName)
    {
        TargetFolderId = folderId;
        TargetFolderName = folderName;
    }
}
