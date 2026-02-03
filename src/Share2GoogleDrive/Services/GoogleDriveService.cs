using System.Diagnostics;
using System.IO;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Share2GoogleDrive.Models;
using Serilog;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace Share2GoogleDrive.Services;

/// <summary>
/// Service for interacting with Google Drive API.
/// </summary>
public interface IGoogleDriveService
{
    Task<UploadResult> UploadFileAsync(string filePath, string? folderId, IProgress<long>? progress = null, CancellationToken cancellationToken = default);
    Task<DriveFile?> CheckFileExistsAsync(string fileName, string? folderId);
    Task<UploadResult> UpdateFileAsync(string fileId, string filePath, IProgress<long>? progress = null, CancellationToken cancellationToken = default);
    Task<List<DriveFolder>> GetFoldersAsync(string? parentId = null);
    Task<DriveFolder> CreateFolderAsync(string name, string? parentId = null);
    void OpenInBrowser(string webViewLink);
    void ClearCache();
}

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IGoogleAuthService _authService;
    private readonly ISettingsService _settingsService;
    private DriveService? _driveService;

    private const int MaxRetries = 3;
    private const int RetryDelayMs = 1000;

    public GoogleDriveService(IGoogleAuthService authService, ISettingsService settingsService)
    {
        _authService = authService;
        _settingsService = settingsService;
    }

    private async Task<DriveService> GetDriveServiceAsync()
    {
        if (_driveService != null)
        {
            return _driveService;
        }

        var credential = _authService.CurrentCredential;
        if (credential == null)
        {
            credential = await _authService.AuthenticateAsync();
        }

        if (credential == null)
        {
            throw new InvalidOperationException("Not authenticated with Google Drive.");
        }

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Share2GoogleDrive"
        });

        return _driveService;
    }

    public async Task<UploadResult> UploadFileAsync(
        string filePath,
        string? folderId,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(filePath);
        Log.Information("Starting upload of {FileName} to folder {FolderId}", fileName, folderId ?? "root");

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var service = await GetDriveServiceAsync();

                var fileMetadata = new DriveFile
                {
                    Name = fileName,
                    Parents = !string.IsNullOrEmpty(folderId) ? new List<string> { folderId } : null
                };

                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var mimeType = GetMimeType(filePath);

                var request = service.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id, name, webViewLink";

                if (progress != null)
                {
                    request.ProgressChanged += upload =>
                    {
                        if (upload.Status == UploadStatus.Uploading)
                        {
                            progress.Report(upload.BytesSent);
                        }
                    };
                }

                var result = await request.UploadAsync(cancellationToken);

                if (result.Status == UploadStatus.Failed)
                {
                    throw result.Exception ?? new Exception("Upload failed with unknown error");
                }

                var uploadedFile = request.ResponseBody;
                Log.Information("Successfully uploaded {FileName} with ID {FileId}", fileName, uploadedFile.Id);

                return UploadResult.Successful(uploadedFile.Id, uploadedFile.Name, uploadedFile.WebViewLink);
            }
            catch (OperationCanceledException)
            {
                Log.Information("Upload cancelled by user");
                return UploadResult.Cancelled();
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                Log.Warning(ex, "Upload attempt {Attempt} failed, retrying...", attempt);
                await Task.Delay(RetryDelayMs * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Upload failed after {MaxRetries} attempts", MaxRetries);
                return UploadResult.Failed($"Upload failed: {ex.Message}");
            }
        }

        return UploadResult.Failed("Upload failed after maximum retries");
    }

    public async Task<DriveFile?> CheckFileExistsAsync(string fileName, string? folderId)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            var query = $"name = '{EscapeQuery(fileName)}' and trashed = false";
            if (!string.IsNullOrEmpty(folderId))
            {
                query += $" and '{folderId}' in parents";
            }

            var request = service.Files.List();
            request.Q = query;
            request.Fields = "files(id, name, webViewLink)";

            var result = await request.ExecuteAsync();
            return result.Files.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to check if file exists");
            return null;
        }
    }

    public async Task<UploadResult> UpdateFileAsync(
        string fileId,
        string filePath,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(filePath);
        Log.Information("Updating file {FileId} with {FileName}", fileId, fileName);

        try
        {
            var service = await GetDriveServiceAsync();

            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var mimeType = GetMimeType(filePath);

            var request = service.Files.Update(new DriveFile(), fileId, stream, mimeType);
            request.Fields = "id, name, webViewLink";

            if (progress != null)
            {
                request.ProgressChanged += upload =>
                {
                    if (upload.Status == UploadStatus.Uploading)
                    {
                        progress.Report(upload.BytesSent);
                    }
                };
            }

            var result = await request.UploadAsync(cancellationToken);

            if (result.Status == UploadStatus.Failed)
            {
                throw result.Exception ?? new Exception("Update failed with unknown error");
            }

            var updatedFile = request.ResponseBody;
            Log.Information("Successfully updated {FileName} with ID {FileId}", fileName, updatedFile.Id);

            return UploadResult.Successful(updatedFile.Id, updatedFile.Name, updatedFile.WebViewLink);
        }
        catch (OperationCanceledException)
        {
            return UploadResult.Cancelled();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update file");
            return UploadResult.Failed($"Update failed: {ex.Message}");
        }
    }

    public async Task<List<DriveFolder>> GetFoldersAsync(string? parentId = null)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            var query = "mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            if (!string.IsNullOrEmpty(parentId))
            {
                query += $" and '{parentId}' in parents";
            }
            else
            {
                query += " and 'root' in parents";
            }

            var request = service.Files.List();
            request.Q = query;
            request.Fields = "files(id, name, parents)";
            request.OrderBy = "name";

            var result = await request.ExecuteAsync();

            var folders = new List<DriveFolder>();
            foreach (var file in result.Files)
            {
                // Check if folder has children
                var hasChildren = await HasSubfoldersAsync(service, file.Id);
                folders.Add(new DriveFolder
                {
                    Id = file.Id,
                    Name = file.Name,
                    ParentId = file.Parents?.FirstOrDefault(),
                    HasChildren = hasChildren
                });
            }

            return folders;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get folders");
            throw;
        }
    }

    private async Task<bool> HasSubfoldersAsync(DriveService service, string folderId)
    {
        var request = service.Files.List();
        request.Q = $"mimeType = 'application/vnd.google-apps.folder' and trashed = false and '{folderId}' in parents";
        request.Fields = "files(id)";
        request.PageSize = 1;

        var result = await request.ExecuteAsync();
        return result.Files.Count > 0;
    }

    public async Task<DriveFolder> CreateFolderAsync(string name, string? parentId = null)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            var folderMetadata = new DriveFile
            {
                Name = name,
                MimeType = "application/vnd.google-apps.folder",
                Parents = !string.IsNullOrEmpty(parentId) ? new List<string> { parentId } : null
            };

            var request = service.Files.Create(folderMetadata);
            request.Fields = "id, name, parents";

            var folder = await request.ExecuteAsync();

            Log.Information("Created folder {FolderName} with ID {FolderId}", name, folder.Id);

            return new DriveFolder
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentId = folder.Parents?.FirstOrDefault(),
                HasChildren = false
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create folder");
            throw;
        }
    }

    public void OpenInBrowser(string webViewLink)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = webViewLink,
                UseShellExecute = true
            });
            Log.Information("Opened file in browser: {Link}", webViewLink);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file in browser");
        }
    }

    public void ClearCache()
    {
        _driveService?.Dispose();
        _driveService = null;
        Log.Information("Drive service cache cleared");
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".html" => "text/html",
            ".htm" => "text/html",
            ".xml" => "application/xml",
            ".json" => "application/json",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".mp3" => "audio/mpeg",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream"
        };
    }

    private static string EscapeQuery(string value)
    {
        return value.Replace("\\", "\\\\").Replace("'", "\\'");
    }
}
