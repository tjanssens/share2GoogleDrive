using Share2GoogleDrive.Models;
using Xunit;

namespace Share2GoogleDrive.Tests.Models;

public class UploadResultTests
{
    #region Successful Tests

    [Fact]
    public void Successful_SetsCorrectProperties()
    {
        // Arrange
        var fileId = "file-123";
        var fileName = "test.pdf";
        var webViewLink = "https://drive.google.com/file/123";

        // Act
        var result = UploadResult.Successful(fileId, fileName, webViewLink);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(fileId, result.FileId);
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(webViewLink, result.WebViewLink);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ConflictResolution);
    }

    [Fact]
    public void Successful_WithEmptyStrings_StillSucceeds()
    {
        // Act
        var result = UploadResult.Successful(string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.FileId!);
        Assert.Empty(result.FileName!);
        Assert.Empty(result.WebViewLink!);
    }

    #endregion

    #region Failed Tests

    [Fact]
    public void Failed_SetsErrorMessage()
    {
        // Arrange
        var errorMessage = "Upload failed due to network error";

        // Act
        var result = UploadResult.Failed(errorMessage);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Null(result.FileId);
        Assert.Null(result.FileName);
        Assert.Null(result.WebViewLink);
    }

    [Fact]
    public void Failed_WithEmptyErrorMessage_StillFails()
    {
        // Act
        var result = UploadResult.Failed(string.Empty);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.ErrorMessage!);
    }

    [Fact]
    public void Failed_WithDetailedError_PreservesFullMessage()
    {
        // Arrange
        var detailedError = "Network error: Connection refused\nStack trace: at GoogleDriveService.Upload()";

        // Act
        var result = UploadResult.Failed(detailedError);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(detailedError, result.ErrorMessage);
    }

    #endregion

    #region Cancelled Tests

    [Fact]
    public void Cancelled_SetsCancelledState()
    {
        // Act
        var result = UploadResult.Cancelled();

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Upload cancelled by user.", result.ErrorMessage);
        Assert.Null(result.FileId);
        Assert.Null(result.FileName);
        Assert.Null(result.WebViewLink);
    }

    [Fact]
    public void Cancelled_IsDifferentFromFailed()
    {
        // Act
        var cancelledResult = UploadResult.Cancelled();
        var failedResult = UploadResult.Failed("Some error");

        // Assert
        Assert.False(cancelledResult.Success);
        Assert.False(failedResult.Success);
        Assert.NotEqual(cancelledResult.ErrorMessage, failedResult.ErrorMessage);
        Assert.Contains("cancelled", cancelledResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region ConflictResolution Tests

    [Fact]
    public void ConflictResolution_DefaultIsNull()
    {
        // Act
        var result = new UploadResult();

        // Assert
        Assert.Null(result.ConflictResolution);
    }

    [Fact]
    public void ConflictResolution_CanBeSetToReplace()
    {
        // Act
        var result = UploadResult.Successful("id", "name", "link");
        result.ConflictResolution = ConflictResolution.Replace;

        // Assert
        Assert.Equal(ConflictResolution.Replace, result.ConflictResolution);
    }

    [Fact]
    public void ConflictResolution_CanBeSetToKeepBoth()
    {
        // Act
        var result = UploadResult.Successful("id", "name", "link");
        result.ConflictResolution = ConflictResolution.KeepBoth;

        // Assert
        Assert.Equal(ConflictResolution.KeepBoth, result.ConflictResolution);
    }

    [Fact]
    public void ConflictResolution_CanBeSetToCancel()
    {
        // Act
        var result = UploadResult.Successful("id", "name", "link");
        result.ConflictResolution = ConflictResolution.Cancel;

        // Assert
        Assert.Equal(ConflictResolution.Cancel, result.ConflictResolution);
    }

    #endregion

    #region ConflictResolution Enum Tests

    [Fact]
    public void ConflictResolutionEnum_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)ConflictResolution.Replace);
        Assert.Equal(1, (int)ConflictResolution.KeepBoth);
        Assert.Equal(2, (int)ConflictResolution.Cancel);
    }

    [Fact]
    public void ConflictResolutionEnum_HasThreeValues()
    {
        // Act
        var values = Enum.GetValues<ConflictResolution>();

        // Assert
        Assert.Equal(3, values.Length);
    }

    #endregion

    #region DriveFolder Tests

    [Fact]
    public void DriveFolder_DefaultValues_AreCorrect()
    {
        // Act
        var folder = new DriveFolder();

        // Assert
        Assert.Empty(folder.Id);
        Assert.Empty(folder.Name);
        Assert.Null(folder.ParentId);
        Assert.False(folder.HasChildren);
    }

    [Fact]
    public void DriveFolder_CanSetProperties()
    {
        // Act
        var folder = new DriveFolder
        {
            Id = "folder-123",
            Name = "My Documents",
            ParentId = "parent-456",
            HasChildren = true
        };

        // Assert
        Assert.Equal("folder-123", folder.Id);
        Assert.Equal("My Documents", folder.Name);
        Assert.Equal("parent-456", folder.ParentId);
        Assert.True(folder.HasChildren);
    }

    [Fact]
    public void DriveFolder_WithRootFolder_HasNullParent()
    {
        // Act
        var rootFolder = new DriveFolder
        {
            Id = "root",
            Name = "My Drive",
            ParentId = null,
            HasChildren = true
        };

        // Assert
        Assert.Null(rootFolder.ParentId);
    }

    #endregion
}
