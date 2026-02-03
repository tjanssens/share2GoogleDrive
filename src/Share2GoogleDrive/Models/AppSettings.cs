using System.Windows.Input;

namespace Share2GoogleDrive.Models;

/// <summary>
/// Application settings model stored in JSON configuration file.
/// </summary>
public class AppSettings
{
    public string Version { get; set; } = "1.0.0";
    public AccountSettings Account { get; set; } = new();
    public UploadSettings Upload { get; set; } = new();
    public HotkeySettings Hotkey { get; set; } = new();
    public GeneralSettings General { get; set; } = new();
}

public class AccountSettings
{
    public string? Email { get; set; }
    public bool Connected { get; set; }
}

public class UploadSettings
{
    public string? DefaultFolderId { get; set; }
    public string DefaultFolderName { get; set; } = "My Drive";
    public bool ShowProgress { get; set; } = true;
    public bool NotifyOnComplete { get; set; } = true;
    public bool OpenInBrowserAfterUpload { get; set; } = true;
}

public class HotkeySettings
{
    public bool Enabled { get; set; } = true;
    public ModifierKeys Modifiers { get; set; } = ModifierKeys.Control;
    public Key Key { get; set; } = Key.G;
}

public class GeneralSettings
{
    public bool Autostart { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
}
