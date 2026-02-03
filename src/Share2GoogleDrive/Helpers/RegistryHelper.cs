using Microsoft.Win32;
using Serilog;

namespace Share2GoogleDrive.Helpers;

/// <summary>
/// Helper for managing Windows Registry entries for context menu and autostart.
/// </summary>
public static class RegistryHelper
{
    private const string ContextMenuKeyPath = @"Software\Classes\*\shell\Send2Drive";
    private const string AutostartKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Share2GoogleDrive";

    /// <summary>
    /// Registers the context menu entry for all files.
    /// </summary>
    public static bool RegisterContextMenu(string executablePath, string? iconPath = null)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(ContextMenuKeyPath);
            if (key == null)
            {
                Log.Warning("Failed to create context menu registry key");
                return false;
            }

            key.SetValue("", "Mario Delivery!");
            key.SetValue("Icon", iconPath ?? executablePath);

            using var commandKey = key.CreateSubKey("command");
            commandKey?.SetValue("", $"\"{executablePath}\" \"%1\"");

            Log.Information("Context menu registered successfully");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to register context menu");
            return false;
        }
    }

    /// <summary>
    /// Unregisters the context menu entry.
    /// </summary>
    public static bool UnregisterContextMenu()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(ContextMenuKeyPath, throwOnMissingSubKey: false);
            Log.Information("Context menu unregistered successfully");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to unregister context menu");
            return false;
        }
    }

    /// <summary>
    /// Checks if context menu is registered.
    /// </summary>
    public static bool IsContextMenuRegistered()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(ContextMenuKeyPath);
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Enables autostart on Windows login.
    /// </summary>
    public static bool EnableAutostart(string executablePath)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AutostartKeyPath, writable: true);
            if (key == null)
            {
                Log.Warning("Failed to open autostart registry key");
                return false;
            }

            key.SetValue(AppName, $"\"{executablePath}\" --minimized");
            Log.Information("Autostart enabled");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to enable autostart");
            return false;
        }
    }

    /// <summary>
    /// Disables autostart.
    /// </summary>
    public static bool DisableAutostart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AutostartKeyPath, writable: true);
            key?.DeleteValue(AppName, throwOnMissingValue: false);
            Log.Information("Autostart disabled");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to disable autostart");
            return false;
        }
    }

    /// <summary>
    /// Checks if autostart is enabled.
    /// </summary>
    public static bool IsAutostartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AutostartKeyPath);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }
}
