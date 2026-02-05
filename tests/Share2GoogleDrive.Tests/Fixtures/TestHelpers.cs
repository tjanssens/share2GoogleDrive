using System.Windows.Input;
using Moq;
using Share2GoogleDrive.Models;
using Share2GoogleDrive.Services;

namespace Share2GoogleDrive.Tests.Fixtures;

/// <summary>
/// Helper methods for creating test fixtures and mocks.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a default AppSettings for testing.
    /// </summary>
    public static AppSettings CreateDefaultSettings() => new()
    {
        Version = "1.0.0",
        Account = new AccountSettings
        {
            Email = "test@example.com",
            Connected = true
        },
        Upload = new UploadSettings
        {
            DefaultFolderId = "folder-123",
            DefaultFolderName = "Test Folder",
            ShowProgress = true,
            NotifyOnComplete = true,
            OpenInBrowserAfterUpload = true
        },
        Hotkey = new HotkeySettings
        {
            Enabled = true,
            Modifiers = ModifierKeys.Control,
            Key = Key.G
        },
        General = new GeneralSettings
        {
            Autostart = false,
            MinimizeToTray = true
        }
    };

    /// <summary>
    /// Creates a mock ISettingsService with default settings.
    /// </summary>
    public static Mock<ISettingsService> CreateMockSettingsService(AppSettings? settings = null)
    {
        var mock = new Mock<ISettingsService>();
        settings ??= CreateDefaultSettings();

        mock.SetupGet(s => s.Settings).Returns(settings);
        mock.SetupGet(s => s.AppDataPath).Returns(Path.GetTempPath());
        mock.Setup(s => s.LoadAsync()).Returns(Task.CompletedTask);
        mock.Setup(s => s.SaveAsync()).Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    /// Creates a mock IGoogleDriveService.
    /// </summary>
    public static Mock<IGoogleDriveService> CreateMockGoogleDriveService()
    {
        var mock = new Mock<IGoogleDriveService>();

        mock.Setup(s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string path, string? folder, IProgress<long>? progress, CancellationToken ct) =>
                UploadResult.Successful("file-123", Path.GetFileName(path), "https://drive.google.com/file/123"));

        mock.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);

        mock.Setup(s => s.GetFoldersAsync(It.IsAny<string?>()))
            .ReturnsAsync(new List<DriveFolder>
            {
                new() { Id = "folder-1", Name = "Documents", HasChildren = true },
                new() { Id = "folder-2", Name = "Photos", HasChildren = false }
            });

        mock.Setup(s => s.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((string name, string? parent) => new DriveFolder
            {
                Id = $"new-folder-{Guid.NewGuid():N}",
                Name = name,
                ParentId = parent,
                HasChildren = false
            });

        return mock;
    }

    /// <summary>
    /// Creates a mock IGoogleAuthService.
    /// </summary>
    public static Mock<IGoogleAuthService> CreateMockGoogleAuthService(bool isAuthenticated = true)
    {
        var mock = new Mock<IGoogleAuthService>();

        mock.Setup(s => s.IsAuthenticatedAsync())
            .ReturnsAsync(isAuthenticated);

        mock.Setup(s => s.GetUserEmailAsync())
            .ReturnsAsync(isAuthenticated ? "test@example.com" : null);

        mock.Setup(s => s.SignOutAsync())
            .Returns(Task.CompletedTask);

        return mock;
    }

    /// <summary>
    /// Creates a mock INotificationService.
    /// </summary>
    public static Mock<INotificationService> CreateMockNotificationService()
    {
        var mock = new Mock<INotificationService>();
        return mock;
    }

    /// <summary>
    /// Creates a mock ITrayIconService.
    /// </summary>
    public static Mock<ITrayIconService> CreateMockTrayIconService()
    {
        var mock = new Mock<ITrayIconService>();
        return mock;
    }

    /// <summary>
    /// Creates a mock IHotkeyService.
    /// </summary>
    public static Mock<IHotkeyService> CreateMockHotkeyService()
    {
        var mock = new Mock<IHotkeyService>();

        mock.Setup(s => s.Register(It.IsAny<ModifierKeys>(), It.IsAny<Key>()))
            .Returns(true);

        mock.SetupGet(s => s.IsRegistered)
            .Returns(true);

        return mock;
    }

    /// <summary>
    /// Creates a temporary file with specified content.
    /// </summary>
    public static string CreateTempFile(string content = "Test content", string extension = ".txt")
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}{extension}");
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Creates a temporary directory.
    /// </summary>
    public static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Safely deletes a file if it exists.
    /// </summary>
    public static void SafeDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignore deletion errors in tests
        }
    }

    /// <summary>
    /// Safely deletes a directory if it exists.
    /// </summary>
    public static void SafeDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Ignore deletion errors in tests
        }
    }
}
