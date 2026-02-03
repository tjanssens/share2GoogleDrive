using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;

namespace Share2GoogleDrive.Services;

/// <summary>
/// Service for displaying Windows toast notifications.
/// </summary>
public interface INotificationService
{
    void ShowUploadStarted(string fileName);
    void ShowUploadCompleted(string fileName, string? webViewLink);
    void ShowUploadFailed(string fileName, string errorMessage);
    void ShowInfo(string title, string message);
    void ShowError(string title, string message);
}

public class NotificationService : INotificationService
{
    public NotificationService()
    {
        // Handle notification activation (clicking on notification)
        ToastNotificationManagerCompat.OnActivated += e => HandleNotificationActivated(e.Argument);
    }

    private static void HandleNotificationActivated(string argument)
    {
        var args = ToastArguments.Parse(argument);

        if (args.TryGetValue("action", out var action) && action == "open")
        {
            if (args.TryGetValue("url", out var url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to open URL from notification: {Url}", url);
                }
            }
        }
    }

    public void ShowUploadStarted(string fileName)
    {
        try
        {
            new ToastContentBuilder()
                .AddText("Upload Started")
                .AddText($"Uploading {fileName} to Google Drive...")
                .SetToastScenario(ToastScenario.Default)
                .Show();

            Log.Debug("Showed upload started notification for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to show upload started notification");
        }
    }

    public void ShowUploadCompleted(string fileName, string? webViewLink)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddText("Upload Complete")
                .AddText($"{fileName} has been uploaded to Google Drive.");

            if (!string.IsNullOrEmpty(webViewLink))
            {
                builder.AddButton(new ToastButton()
                    .SetContent("Open in Browser")
                    .AddArgument("action", "open")
                    .AddArgument("url", webViewLink));
            }

            builder.Show();

            Log.Debug("Showed upload completed notification for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to show upload completed notification");
        }
    }

    public void ShowUploadFailed(string fileName, string errorMessage)
    {
        try
        {
            new ToastContentBuilder()
                .AddText("Upload Failed")
                .AddText($"Failed to upload {fileName}")
                .AddText(errorMessage)
                .Show();

            Log.Debug("Showed upload failed notification for {FileName}: {Error}", fileName, errorMessage);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to show upload failed notification");
        }
    }

    public void ShowInfo(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to show info notification");
        }
    }

    public void ShowError(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .SetToastScenario(ToastScenario.Default)
                .Show();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to show error notification");
        }
    }

}
