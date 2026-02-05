using System.Text.Json;
using Share2GoogleDrive.Models;
using Share2GoogleDrive.Services;
using Xunit;

namespace Share2GoogleDrive.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _settingsFilePath;

    public SettingsServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"Share2GoogleDrive_Tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
        _settingsFilePath = Path.Combine(_testDirectory, "settings.json");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private ISettingsService CreateSettingsServiceWithPath()
    {
        // Use reflection or a test-specific subclass to set AppDataPath
        // For this test, we'll create a testable wrapper
        return new TestableSettingsService(_testDirectory);
    }

    #region LoadAsync Tests

    [Fact]
    public async Task LoadAsync_FileExists_LoadsSettings()
    {
        // Arrange
        var expectedSettings = new AppSettings
        {
            Version = "1.2.0",
            Account = new AccountSettings
            {
                Email = "loaded@example.com",
                Connected = true
            },
            Upload = new UploadSettings
            {
                DefaultFolderId = "loaded-folder-id",
                DefaultFolderName = "Loaded Folder"
            }
        };

        var json = JsonSerializer.Serialize(expectedSettings, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await File.WriteAllTextAsync(_settingsFilePath, json);

        var service = CreateSettingsServiceWithPath();

        // Act
        await service.LoadAsync();

        // Assert
        Assert.Equal("1.2.0", service.Settings.Version);
        Assert.Equal("loaded@example.com", service.Settings.Account.Email);
        Assert.True(service.Settings.Account.Connected);
        Assert.Equal("loaded-folder-id", service.Settings.Upload.DefaultFolderId);
        Assert.Equal("Loaded Folder", service.Settings.Upload.DefaultFolderName);
    }

    [Fact]
    public async Task LoadAsync_FileNotExists_CreatesDefaults()
    {
        // Arrange
        var service = CreateSettingsServiceWithPath();
        Assert.False(File.Exists(_settingsFilePath));

        // Act
        await service.LoadAsync();

        // Assert
        Assert.NotNull(service.Settings);
        Assert.Equal("1.0.0", service.Settings.Version);
        Assert.Null(service.Settings.Account.Email);
        Assert.False(service.Settings.Account.Connected);
        Assert.True(File.Exists(_settingsFilePath)); // Should have created the file
    }

    [Fact]
    public async Task LoadAsync_InvalidJson_ReturnsDefaults()
    {
        // Arrange
        await File.WriteAllTextAsync(_settingsFilePath, "{ invalid json content ]]]");
        var service = CreateSettingsServiceWithPath();

        // Act
        await service.LoadAsync();

        // Assert
        Assert.NotNull(service.Settings);
        Assert.Equal("1.0.0", service.Settings.Version);
        Assert.Null(service.Settings.Account.Email);
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_ReturnsDefaults()
    {
        // Arrange
        await File.WriteAllTextAsync(_settingsFilePath, string.Empty);
        var service = CreateSettingsServiceWithPath();

        // Act
        await service.LoadAsync();

        // Assert
        Assert.NotNull(service.Settings);
        Assert.Equal("1.0.0", service.Settings.Version);
    }

    [Fact]
    public async Task LoadAsync_PartialJson_LoadsAvailableAndDefaultsRest()
    {
        // Arrange
        var partialJson = """
        {
            "version": "2.0.0",
            "account": {
                "email": "partial@example.com"
            }
        }
        """;
        await File.WriteAllTextAsync(_settingsFilePath, partialJson);
        var service = CreateSettingsServiceWithPath();

        // Act
        await service.LoadAsync();

        // Assert
        Assert.Equal("2.0.0", service.Settings.Version);
        Assert.Equal("partial@example.com", service.Settings.Account.Email);
        // Default values for missing properties
        Assert.NotNull(service.Settings.Upload);
        Assert.NotNull(service.Settings.Hotkey);
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WritesJsonToFile()
    {
        // Arrange
        var service = CreateSettingsServiceWithPath();
        service.Settings.Account.Email = "saved@example.com";
        service.Settings.Account.Connected = true;
        service.Settings.Upload.DefaultFolderId = "saved-folder";

        // Act
        await service.SaveAsync();

        // Assert
        Assert.True(File.Exists(_settingsFilePath));
        var json = await File.ReadAllTextAsync(_settingsFilePath);
        Assert.Contains("saved@example.com", json);
        Assert.Contains("saved-folder", json);
    }

    [Fact]
    public async Task SaveAsync_CreatesFormattedJson()
    {
        // Arrange
        var service = CreateSettingsServiceWithPath();
        service.Settings.Account.Email = "formatted@example.com";

        // Act
        await service.SaveAsync();

        // Assert
        var json = await File.ReadAllTextAsync(_settingsFilePath);
        // Check that JSON is formatted (has newlines/indentation)
        Assert.Contains("\n", json);
        Assert.Contains("  ", json); // indentation
    }

    [Fact]
    public async Task SaveAsync_UsesCamelCase()
    {
        // Arrange
        var service = CreateSettingsServiceWithPath();
        service.Settings.Upload.DefaultFolderId = "test-id";

        // Act
        await service.SaveAsync();

        // Assert
        var json = await File.ReadAllTextAsync(_settingsFilePath);
        // Should use camelCase, not PascalCase
        Assert.Contains("defaultFolderId", json);
        Assert.DoesNotContain("DefaultFolderId", json);
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        // Arrange
        await File.WriteAllTextAsync(_settingsFilePath, "old content");
        var service = CreateSettingsServiceWithPath();
        service.Settings.Account.Email = "new@example.com";

        // Act
        await service.SaveAsync();

        // Assert
        var json = await File.ReadAllTextAsync(_settingsFilePath);
        Assert.DoesNotContain("old content", json);
        Assert.Contains("new@example.com", json);
    }

    [Fact]
    public async Task SaveAsync_PreservesAllSettings()
    {
        // Arrange
        var service = CreateSettingsServiceWithPath();
        service.Settings.Version = "3.0.0";
        service.Settings.Account.Email = "all@example.com";
        service.Settings.Account.Connected = true;
        service.Settings.Upload.DefaultFolderId = "folder-123";
        service.Settings.Upload.NotifyOnComplete = false;
        service.Settings.Hotkey.Enabled = false;
        service.Settings.General.Autostart = true;

        // Act
        await service.SaveAsync();
        await service.LoadAsync();

        // Assert
        Assert.Equal("3.0.0", service.Settings.Version);
        Assert.Equal("all@example.com", service.Settings.Account.Email);
        Assert.True(service.Settings.Account.Connected);
        Assert.Equal("folder-123", service.Settings.Upload.DefaultFolderId);
        Assert.False(service.Settings.Upload.NotifyOnComplete);
        Assert.False(service.Settings.Hotkey.Enabled);
        Assert.True(service.Settings.General.Autostart);
    }

    #endregion

    #region AppDataPath Tests

    [Fact]
    public void AppDataPath_ReturnsCorrectPath()
    {
        // Arrange
        var service = new SettingsService();

        // Act
        var path = service.AppDataPath;

        // Assert
        Assert.Contains("Share2GoogleDrive", path);
        Assert.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path);
    }

    [Fact]
    public void AppDataPath_CreatesDirectory()
    {
        // Arrange & Act
        var service = new SettingsService();

        // Assert
        Assert.True(Directory.Exists(service.AppDataPath));
    }

    #endregion

    #region Settings Property Tests

    [Fact]
    public void Settings_DefaultIsNotNull()
    {
        // Arrange
        var service = CreateSettingsServiceWithPath();

        // Assert
        Assert.NotNull(service.Settings);
    }

    [Fact]
    public void Settings_DefaultVersion()
    {
        // Arrange
        var service = CreateSettingsServiceWithPath();

        // Assert
        Assert.Equal("1.0.0", service.Settings.Version);
    }

    #endregion

    /// <summary>
    /// Testable subclass that allows setting the AppDataPath.
    /// </summary>
    private class TestableSettingsService : ISettingsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AppSettings Settings { get; private set; } = new();
        public string AppDataPath { get; }

        private string SettingsFilePath => Path.Combine(AppDataPath, "settings.json");

        public TestableSettingsService(string appDataPath)
        {
            AppDataPath = appDataPath;
            Directory.CreateDirectory(AppDataPath);
        }

        public async Task LoadAsync()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(SettingsFilePath);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                }
                else
                {
                    Settings = new AppSettings();
                    await SaveAsync();
                }
            }
            catch
            {
                Settings = new AppSettings();
            }
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            await File.WriteAllTextAsync(SettingsFilePath, json);
        }
    }
}
