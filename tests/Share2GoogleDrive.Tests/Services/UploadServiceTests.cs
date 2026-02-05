using Moq;
using Share2GoogleDrive.Models;
using Share2GoogleDrive.Services;
using Share2GoogleDrive.Tests.Fixtures;
using Xunit;

namespace Share2GoogleDrive.Tests.Services;

public class UploadServiceTests : IDisposable
{
    private readonly Mock<IGoogleDriveService> _mockDriveService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ITrayIconService> _mockTrayIconService;
    private readonly UploadService _uploadService;
    private readonly List<string> _tempFiles = new();

    public UploadServiceTests()
    {
        _mockDriveService = TestHelpers.CreateMockGoogleDriveService();
        _mockSettingsService = TestHelpers.CreateMockSettingsService();
        _mockNotificationService = TestHelpers.CreateMockNotificationService();
        _mockTrayIconService = TestHelpers.CreateMockTrayIconService();

        _uploadService = new UploadService(
            _mockDriveService.Object,
            _mockSettingsService.Object,
            _mockNotificationService.Object,
            _mockTrayIconService.Object);
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            TestHelpers.SafeDeleteFile(file);
        }
    }

    private string CreateTestFile(string content = "Test content")
    {
        var path = TestHelpers.CreateTempFile(content);
        _tempFiles.Add(path);
        return path;
    }

    #region FileNotFound Tests

    [Fact]
    public async Task UploadFileAsync_FileNotFound_ReturnsFailed()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.txt");

        // Act
        var result = await _uploadService.UploadFileAsync(nonExistentPath);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadFileAsync_FileNotFound_DoesNotCallDriveService()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.txt");

        // Act
        await _uploadService.UploadFileAsync(nonExistentPath);

        // Assert
        _mockDriveService.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<IProgress<long>?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Successful Upload Tests

    [Fact]
    public async Task UploadFileAsync_NoConflict_UploadsSuccessfully()
    {
        // Arrange
        var testFile = CreateTestFile();
        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);

        // Act
        var result = await _uploadService.UploadFileAsync(testFile);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.FileId);
        Assert.NotNull(result.WebViewLink);
    }

    [Fact]
    public async Task UploadFileAsync_NoConflict_CallsUploadOnce()
    {
        // Arrange
        var testFile = CreateTestFile();
        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);

        // Act
        await _uploadService.UploadFileAsync(testFile);

        // Assert
        _mockDriveService.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<IProgress<long>?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Conflict Resolution Tests

    [Fact]
    public async Task UploadFileAsync_ConflictReplace_UpdatesFile()
    {
        // Arrange
        var testFile = CreateTestFile();
        var existingFile = new Google.Apis.Drive.v3.Data.File { Id = "existing-123", Name = Path.GetFileName(testFile) };

        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(existingFile);

        _mockDriveService.Setup(s => s.UpdateFileAsync(
                "existing-123",
                It.IsAny<string>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UploadResult.Successful("existing-123", Path.GetFileName(testFile), "https://drive.google.com/updated"));

        // Setup conflict handler to resolve as Replace
        _uploadService.ConflictDetected += (_, args) =>
        {
            args.ResponseSource.SetResult(ConflictResolution.Replace);
        };

        // Act
        var result = await _uploadService.UploadFileAsync(testFile);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ConflictResolution.Replace, result.ConflictResolution);
        _mockDriveService.Verify(s => s.UpdateFileAsync("existing-123", It.IsAny<string>(), It.IsAny<IProgress<long>?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_ConflictKeepBoth_CreatesUniqueName()
    {
        // Arrange
        var testFile = CreateTestFile();
        var existingFile = new Google.Apis.Drive.v3.Data.File { Id = "existing-123", Name = Path.GetFileName(testFile) };

        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(existingFile);

        // Setup conflict handler to resolve as KeepBoth
        _uploadService.ConflictDetected += (_, args) =>
        {
            args.ResponseSource.SetResult(ConflictResolution.KeepBoth);
        };

        // Act
        var result = await _uploadService.UploadFileAsync(testFile);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ConflictResolution.KeepBoth, result.ConflictResolution);
        // Should upload a new file (not update)
        _mockDriveService.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<IProgress<long>?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_ConflictCancel_ReturnsCancelled()
    {
        // Arrange
        var testFile = CreateTestFile();
        var existingFile = new Google.Apis.Drive.v3.Data.File { Id = "existing-123", Name = Path.GetFileName(testFile) };

        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(existingFile);

        // Setup conflict handler to resolve as Cancel
        _uploadService.ConflictDetected += (_, args) =>
        {
            args.ResponseSource.SetResult(ConflictResolution.Cancel);
        };

        // Act
        var result = await _uploadService.UploadFileAsync(testFile);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cancelled", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task UploadFileAsync_Cancelled_ReturnsCancelledResult()
    {
        // Arrange
        var testFile = CreateTestFile();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);

        _mockDriveService.Setup(s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _uploadService.UploadFileAsync(testFile, cts.Token);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cancelled", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Animation Tests

    [Fact]
    public async Task UploadFileAsync_StartsAndStopsAnimation()
    {
        // Arrange
        var testFile = CreateTestFile();
        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);

        // Act
        await _uploadService.UploadFileAsync(testFile);

        // Assert
        _mockTrayIconService.Verify(s => s.StartUploadAnimation(), Times.Once);
        _mockTrayIconService.Verify(s => s.StopUploadAnimation(), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_OnError_StillStopsAnimation()
    {
        // Arrange
        var testFile = CreateTestFile();
        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);
        _mockDriveService.Setup(s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await _uploadService.UploadFileAsync(testFile);

        // Assert
        _mockTrayIconService.Verify(s => s.StartUploadAnimation(), Times.Once);
        _mockTrayIconService.Verify(s => s.StopUploadAnimation(), Times.Once);
    }

    #endregion

    #region Progress Tests

    [Fact]
    public async Task UploadFileAsync_RaisesProgressChanged()
    {
        // Arrange
        var testFile = CreateTestFile("This is test content for progress reporting.");
        var progressEvents = new List<UploadProgressEventArgs>();

        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);

        // Note: The UploadService creates its own Progress<T> internally,
        // so we can verify that the service sets up progress reporting,
        // but cannot easily capture the events without modifying the implementation.
        // This test verifies the service doesn't throw when progress is available.
        _mockDriveService.Setup(s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UploadResult.Successful("id", "name", "link"));

        _uploadService.ProgressChanged += (_, args) => progressEvents.Add(args);

        // Act
        await _uploadService.UploadFileAsync(testFile);

        // Assert - verify that upload was called with a progress reporter
        _mockDriveService.Verify(s => s.UploadFileAsync(
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsNotNull<IProgress<long>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void UploadProgressEventArgs_CalculatesPercentageCorrectly()
    {
        // Arrange
        var args = new UploadProgressEventArgs
        {
            FileName = "test.txt",
            BytesSent = 50,
            TotalBytes = 100
        };

        // Assert
        Assert.Equal(50.0, args.ProgressPercentage);
    }

    [Fact]
    public void UploadProgressEventArgs_ZeroTotalBytes_ReturnsZeroPercentage()
    {
        // Arrange
        var args = new UploadProgressEventArgs
        {
            FileName = "test.txt",
            BytesSent = 50,
            TotalBytes = 0
        };

        // Assert
        Assert.Equal(0, args.ProgressPercentage);
    }

    #endregion

    #region Notification Tests

    [Fact]
    public async Task UploadFileAsync_WithShowProgressEnabled_ShowsStartedNotification()
    {
        // Arrange
        var testFile = CreateTestFile();
        var settings = TestHelpers.CreateDefaultSettings();
        settings.Upload.ShowProgress = true;
        _mockSettingsService.SetupGet(s => s.Settings).Returns(settings);

        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);

        // Act
        await _uploadService.UploadFileAsync(testFile);

        // Assert
        _mockNotificationService.Verify(s => s.ShowUploadStarted(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WithShowProgressDisabled_DoesNotShowStartedNotification()
    {
        // Arrange
        var testFile = CreateTestFile();
        var settings = TestHelpers.CreateDefaultSettings();
        settings.Upload.ShowProgress = false;
        _mockSettingsService.SetupGet(s => s.Settings).Returns(settings);

        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);

        // Act
        await _uploadService.UploadFileAsync(testFile);

        // Assert
        _mockNotificationService.Verify(s => s.ShowUploadStarted(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadFileAsync_OnError_ShowsFailedNotification()
    {
        // Arrange
        var testFile = CreateTestFile();
        _mockDriveService.Setup(s => s.CheckFileExistsAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync((Google.Apis.Drive.v3.Data.File?)null);
        _mockDriveService.Setup(s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<long>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await _uploadService.UploadFileAsync(testFile);

        // Assert
        _mockNotificationService.Verify(s => s.ShowUploadFailed(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region ConflictEventArgs Tests

    [Fact]
    public void ConflictEventArgs_DefaultResolution_IsCancel()
    {
        // Arrange & Act
        var args = new ConflictEventArgs();

        // Assert
        Assert.Equal(ConflictResolution.Cancel, args.Resolution);
    }

    [Fact]
    public void ConflictEventArgs_HasResponseSource()
    {
        // Arrange & Act
        var args = new ConflictEventArgs();

        // Assert
        Assert.NotNull(args.ResponseSource);
    }

    #endregion
}
