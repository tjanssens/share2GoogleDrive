using System.IO;
using Share2GoogleDrive.Models;
using Serilog;

namespace Share2GoogleDrive.Services;

/// <summary>
/// Service for orchestrating file uploads to Google Drive.
/// </summary>
public interface IUploadService
{
    Task<UploadResult> UploadFileAsync(string filePath, CancellationToken cancellationToken = default);
    event EventHandler<UploadProgressEventArgs>? ProgressChanged;
    event EventHandler<ConflictEventArgs>? ConflictDetected;
}

public class UploadProgressEventArgs : EventArgs
{
    public string FileName { get; init; } = string.Empty;
    public long BytesSent { get; init; }
    public long TotalBytes { get; init; }
    public double ProgressPercentage => TotalBytes > 0 ? (double)BytesSent / TotalBytes * 100 : 0;
}

public class ConflictEventArgs : EventArgs
{
    public string FileName { get; init; } = string.Empty;
    public string ExistingFileId { get; init; } = string.Empty;
    public ConflictResolution Resolution { get; set; } = ConflictResolution.Cancel;
    public TaskCompletionSource<ConflictResolution> ResponseSource { get; } = new();
}

public class UploadService : IUploadService
{
    private readonly IGoogleDriveService _driveService;
    private readonly ISettingsService _settingsService;
    private readonly INotificationService _notificationService;

    public event EventHandler<UploadProgressEventArgs>? ProgressChanged;
    public event EventHandler<ConflictEventArgs>? ConflictDetected;

    public UploadService(
        IGoogleDriveService driveService,
        ISettingsService settingsService,
        INotificationService notificationService)
    {
        _driveService = driveService;
        _settingsService = settingsService;
        _notificationService = notificationService;
    }

    public async Task<UploadResult> UploadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(filePath);
        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            return UploadResult.Failed($"File not found: {filePath}");
        }

        var settings = _settingsService.Settings;
        var folderId = settings.Upload.DefaultFolderId;

        Log.Information("Starting upload of {FileName} ({Size} bytes)", fileName, fileInfo.Length);

        // Show notification if enabled
        if (settings.Upload.ShowProgress)
        {
            _notificationService.ShowUploadStarted(fileName);
        }

        try
        {
            // Check for existing file
            var existingFile = await _driveService.CheckFileExistsAsync(fileName, folderId);

            if (existingFile != null)
            {
                Log.Information("File {FileName} already exists with ID {FileId}", fileName, existingFile.Id);

                // Ask user what to do
                var conflictArgs = new ConflictEventArgs
                {
                    FileName = fileName,
                    ExistingFileId = existingFile.Id
                };

                ConflictDetected?.Invoke(this, conflictArgs);
                var resolution = await conflictArgs.ResponseSource.Task;

                switch (resolution)
                {
                    case ConflictResolution.Replace:
                        var progress = CreateProgressReporter(fileName, fileInfo.Length);
                        var updateResult = await _driveService.UpdateFileAsync(
                            existingFile.Id, filePath, progress, cancellationToken);
                        updateResult.ConflictResolution = ConflictResolution.Replace;
                        await HandleUploadResultAsync(updateResult, settings);
                        return updateResult;

                    case ConflictResolution.KeepBoth:
                        // Upload with modified name
                        var newFileName = GetUniqueFileName(fileName);
                        var tempPath = Path.Combine(Path.GetTempPath(), newFileName);
                        File.Copy(filePath, tempPath, true);
                        try
                        {
                            var progressKeepBoth = CreateProgressReporter(newFileName, fileInfo.Length);
                            var keepBothResult = await _driveService.UploadFileAsync(
                                tempPath, folderId, progressKeepBoth, cancellationToken);
                            keepBothResult.ConflictResolution = ConflictResolution.KeepBoth;
                            await HandleUploadResultAsync(keepBothResult, settings);
                            return keepBothResult;
                        }
                        finally
                        {
                            File.Delete(tempPath);
                        }

                    case ConflictResolution.Cancel:
                    default:
                        return UploadResult.Cancelled();
                }
            }

            // No conflict - normal upload
            var normalProgress = CreateProgressReporter(fileName, fileInfo.Length);
            var result = await _driveService.UploadFileAsync(filePath, folderId, normalProgress, cancellationToken);
            await HandleUploadResultAsync(result, settings);
            return result;
        }
        catch (OperationCanceledException)
        {
            Log.Information("Upload cancelled for {FileName}", fileName);
            return UploadResult.Cancelled();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Upload failed for {FileName}", fileName);
            var errorResult = UploadResult.Failed(ex.Message);
            _notificationService.ShowUploadFailed(fileName, ex.Message);
            return errorResult;
        }
    }

    private IProgress<long> CreateProgressReporter(string fileName, long totalBytes)
    {
        return new Progress<long>(bytesSent =>
        {
            ProgressChanged?.Invoke(this, new UploadProgressEventArgs
            {
                FileName = fileName,
                BytesSent = bytesSent,
                TotalBytes = totalBytes
            });
        });
    }

    private async Task HandleUploadResultAsync(UploadResult result, AppSettings settings)
    {
        if (result.Success)
        {
            if (settings.Upload.NotifyOnComplete)
            {
                _notificationService.ShowUploadCompleted(result.FileName!, result.WebViewLink);
            }

            if (settings.Upload.OpenInBrowserAfterUpload && !string.IsNullOrEmpty(result.WebViewLink))
            {
                _driveService.OpenInBrowser(result.WebViewLink);
            }
        }
        else if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            _notificationService.ShowUploadFailed(result.FileName ?? "Unknown", result.ErrorMessage);
        }

        await Task.CompletedTask;
    }

    private static string GetUniqueFileName(string fileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"{nameWithoutExt}_{timestamp}{extension}";
    }
}
