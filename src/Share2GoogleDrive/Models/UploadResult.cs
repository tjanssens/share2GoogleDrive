namespace Share2GoogleDrive.Models;

/// <summary>
/// Result of a file upload operation.
/// </summary>
public class UploadResult
{
    public bool Success { get; set; }
    public string? FileId { get; set; }
    public string? FileName { get; set; }
    public string? WebViewLink { get; set; }
    public string? ErrorMessage { get; set; }
    public ConflictResolution? ConflictResolution { get; set; }

    public static UploadResult Successful(string fileId, string fileName, string webViewLink) => new()
    {
        Success = true,
        FileId = fileId,
        FileName = fileName,
        WebViewLink = webViewLink
    };

    public static UploadResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };

    public static UploadResult Cancelled() => new()
    {
        Success = false,
        ErrorMessage = "Upload cancelled by user."
    };
}

/// <summary>
/// Options for resolving file conflicts.
/// </summary>
public enum ConflictResolution
{
    Replace,
    KeepBoth,
    Cancel
}

/// <summary>
/// Represents a folder in Google Drive.
/// </summary>
public class DriveFolder
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public bool HasChildren { get; set; }
}
