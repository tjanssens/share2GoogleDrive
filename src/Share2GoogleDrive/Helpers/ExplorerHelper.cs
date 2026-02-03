using System.Runtime.InteropServices;
using Serilog;

namespace Share2GoogleDrive.Helpers;

/// <summary>
/// Helper for interacting with Windows Explorer to get selected files.
/// </summary>
public static class ExplorerHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    /// <summary>
    /// Gets the currently selected file in Windows Explorer.
    /// </summary>
    public static string? GetSelectedFile()
    {
        try
        {
            // Use Shell32 COM automation to get selected files from Explorer
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType == null) return null;

            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell == null) return null;

            try
            {
                var windows = shell.Windows();
                var foregroundWindow = GetForegroundWindow();

                foreach (var window in windows)
                {
                    try
                    {
                        if (window.HWND == (long)foregroundWindow)
                        {
                            var selectedItems = window.Document.SelectedItems();
                            if (selectedItems.Count > 0)
                            {
                                var item = selectedItems.Item(0);
                                var path = item.Path;

                                // Only return if it's a file (not a folder)
                                if (System.IO.File.Exists(path))
                                {
                                    Log.Debug("Selected file from Explorer: {Path}", path);
                                    return path;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Continue to next window
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(shell);
            }

            return null;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get selected file from Explorer");
            return null;
        }
    }

    /// <summary>
    /// Gets all currently selected files in Windows Explorer.
    /// </summary>
    public static List<string> GetSelectedFiles()
    {
        var files = new List<string>();

        try
        {
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType == null) return files;

            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell == null) return files;

            try
            {
                var windows = shell.Windows();
                var foregroundWindow = GetForegroundWindow();

                foreach (var window in windows)
                {
                    try
                    {
                        if (window.HWND == (long)foregroundWindow)
                        {
                            var selectedItems = window.Document.SelectedItems();
                            foreach (var item in selectedItems)
                            {
                                var path = item.Path;
                                if (System.IO.File.Exists(path))
                                {
                                    files.Add(path);
                                }
                            }
                            break;
                        }
                    }
                    catch
                    {
                        // Continue to next window
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(shell);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get selected files from Explorer");
        }

        return files;
    }
}
