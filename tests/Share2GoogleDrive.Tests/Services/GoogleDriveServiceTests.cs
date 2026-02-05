using Moq;
using Share2GoogleDrive.Models;
using Share2GoogleDrive.Services;
using Share2GoogleDrive.Tests.Fixtures;
using Xunit;

namespace Share2GoogleDrive.Tests.Services;

/// <summary>
/// Tests for GoogleDriveService.
/// Note: Most methods require actual Google API calls, so we test
/// primarily the interface contract and edge cases that can be unit tested.
/// </summary>
public class GoogleDriveServiceTests
{
    private readonly Mock<IGoogleAuthService> _mockAuthService;
    private readonly Mock<ISettingsService> _mockSettingsService;

    public GoogleDriveServiceTests()
    {
        _mockAuthService = TestHelpers.CreateMockGoogleAuthService();
        _mockSettingsService = TestHelpers.CreateMockSettingsService();
    }

    #region Interface Contract Tests

    [Fact]
    public void IGoogleDriveService_HasUploadFileAsync()
    {
        // Verify interface has expected method signature
        var type = typeof(IGoogleDriveService);
        var method = type.GetMethod("UploadFileAsync");

        Assert.NotNull(method);
        Assert.Equal(typeof(Task<UploadResult>), method.ReturnType);
    }

    [Fact]
    public void IGoogleDriveService_HasCheckFileExistsAsync()
    {
        // Verify interface has expected method signature
        var type = typeof(IGoogleDriveService);
        var method = type.GetMethod("CheckFileExistsAsync");

        Assert.NotNull(method);
    }

    [Fact]
    public void IGoogleDriveService_HasUpdateFileAsync()
    {
        var type = typeof(IGoogleDriveService);
        var method = type.GetMethod("UpdateFileAsync");

        Assert.NotNull(method);
        Assert.Equal(typeof(Task<UploadResult>), method.ReturnType);
    }

    [Fact]
    public void IGoogleDriveService_HasGetFoldersAsync()
    {
        var type = typeof(IGoogleDriveService);
        var method = type.GetMethod("GetFoldersAsync");

        Assert.NotNull(method);
        Assert.Equal(typeof(Task<List<DriveFolder>>), method.ReturnType);
    }

    [Fact]
    public void IGoogleDriveService_HasCreateFolderAsync()
    {
        var type = typeof(IGoogleDriveService);
        var method = type.GetMethod("CreateFolderAsync");

        Assert.NotNull(method);
        Assert.Equal(typeof(Task<DriveFolder>), method.ReturnType);
    }

    [Fact]
    public void IGoogleDriveService_HasOpenInBrowser()
    {
        var type = typeof(IGoogleDriveService);
        var method = type.GetMethod("OpenInBrowser");

        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
    }

    [Fact]
    public void IGoogleDriveService_HasClearCache()
    {
        var type = typeof(IGoogleDriveService);
        var method = type.GetMethod("ClearCache");

        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void GoogleDriveService_CanBeConstructed()
    {
        // Act
        var service = new GoogleDriveService(_mockAuthService.Object, _mockSettingsService.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GoogleDriveService_ImplementsInterface()
    {
        // Arrange
        var service = new GoogleDriveService(_mockAuthService.Object, _mockSettingsService.Object);

        // Assert
        Assert.IsAssignableFrom<IGoogleDriveService>(service);
    }

    #endregion

    #region ClearCache Tests

    [Fact]
    public void ClearCache_DisposesService()
    {
        // Arrange
        var service = new GoogleDriveService(_mockAuthService.Object, _mockSettingsService.Object);

        // Act & Assert - should not throw
        service.ClearCache();
    }

    [Fact]
    public void ClearCache_CanBeCalledMultipleTimes()
    {
        // Arrange
        var service = new GoogleDriveService(_mockAuthService.Object, _mockSettingsService.Object);

        // Act & Assert - should not throw
        service.ClearCache();
        service.ClearCache();
        service.ClearCache();
    }

    #endregion

    #region Mock-based Tests

    [Fact]
    public async Task MockGoogleDriveService_UploadFileAsync_ReturnsSuccess()
    {
        // Arrange
        var mockService = TestHelpers.CreateMockGoogleDriveService();

        // Act
        var result = await mockService.Object.UploadFileAsync("test.txt", "folder-123");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.FileId);
    }

    [Fact]
    public async Task MockGoogleDriveService_CheckFileExistsAsync_ReturnsNull_WhenNoConflict()
    {
        // Arrange
        var mockService = TestHelpers.CreateMockGoogleDriveService();

        // Act
        var result = await mockService.Object.CheckFileExistsAsync("test.txt", "folder-123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task MockGoogleDriveService_GetFoldersAsync_ReturnsFolders()
    {
        // Arrange
        var mockService = TestHelpers.CreateMockGoogleDriveService();

        // Act
        var folders = await mockService.Object.GetFoldersAsync();

        // Assert
        Assert.NotNull(folders);
        Assert.NotEmpty(folders);
    }

    [Fact]
    public async Task MockGoogleDriveService_CreateFolderAsync_ReturnsNewFolder()
    {
        // Arrange
        var mockService = TestHelpers.CreateMockGoogleDriveService();

        // Act
        var folder = await mockService.Object.CreateFolderAsync("New Folder", "parent-123");

        // Assert
        Assert.NotNull(folder);
        Assert.Equal("New Folder", folder.Name);
        Assert.Equal("parent-123", folder.ParentId);
        Assert.False(folder.HasChildren);
    }

    #endregion

    #region UploadResult Integration Tests

    [Fact]
    public async Task MockGoogleDriveService_UploadFailed_ReturnsFailedResult()
    {
        // Arrange
        var mockService = new Mock<IGoogleDriveService>();
        mockService.Setup(s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UploadResult.Failed("Network error"));

        // Act
        var result = await mockService.Object.UploadFileAsync("test.txt", "folder-123");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Network error", result.ErrorMessage);
    }

    [Fact]
    public async Task MockGoogleDriveService_UploadCancelled_ReturnsCancelledResult()
    {
        // Arrange
        var mockService = new Mock<IGoogleDriveService>();
        mockService.Setup(s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UploadResult.Cancelled());

        // Act
        var result = await mockService.Object.UploadFileAsync("test.txt", "folder-123");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cancelled", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Authentication Dependency Tests

    [Fact]
    public void GoogleDriveService_AcceptsAuthService()
    {
        // Arrange & Act
        var service = new GoogleDriveService(_mockAuthService.Object, _mockSettingsService.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GoogleDriveService_AcceptsSettingsService()
    {
        // Arrange & Act
        var service = new GoogleDriveService(_mockAuthService.Object, _mockSettingsService.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region Retry Logic Verification (via Mock)

    [Fact]
    public async Task MockGoogleDriveService_RetryOnFailure_MaxRetries()
    {
        // Arrange
        var callCount = 0;
        var mockService = new Mock<IGoogleDriveService>();
        mockService.Setup(s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount < 3)
                {
                    return Task.FromResult(UploadResult.Failed($"Attempt {callCount} failed"));
                }
                return Task.FromResult(UploadResult.Successful("id", "name", "link"));
            });

        // Act - simulate retry logic in calling code
        UploadResult? result = null;
        for (int i = 0; i < 3 && (result == null || !result.Success); i++)
        {
            result = await mockService.Object.UploadFileAsync("test.txt", "folder");
        }

        // Assert
        Assert.True(result!.Success);
        Assert.Equal(3, callCount);
    }

    #endregion
}
