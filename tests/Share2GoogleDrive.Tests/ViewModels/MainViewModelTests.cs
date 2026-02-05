using System.Windows.Input;
using Moq;
using Share2GoogleDrive.Models;
using Share2GoogleDrive.Services;
using Share2GoogleDrive.Tests.Fixtures;
using Share2GoogleDrive.ViewModels;
using Xunit;

namespace Share2GoogleDrive.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<IGoogleAuthService> _mockAuthService;
    private readonly Mock<IHotkeyService> _mockHotkeyService;
    private readonly Mock<IGoogleDriveService> _mockDriveService;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _mockSettingsService = TestHelpers.CreateMockSettingsService();
        _mockAuthService = TestHelpers.CreateMockGoogleAuthService();
        _mockHotkeyService = TestHelpers.CreateMockHotkeyService();
        _mockDriveService = TestHelpers.CreateMockGoogleDriveService();

        _viewModel = new MainViewModel(
            _mockSettingsService.Object,
            _mockAuthService.Object,
            _mockHotkeyService.Object,
            _mockDriveService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Assert
        Assert.False(_viewModel.IsConnected);
        Assert.False(_viewModel.IsLoading);
        Assert.Null(_viewModel.StatusMessage);
    }

    [Fact]
    public void Constructor_SetsDefaultHotkey()
    {
        // Assert
        Assert.Equal(ModifierKeys.Control, _viewModel.HotkeyModifiers);
        Assert.Equal(Key.G, _viewModel.HotkeyKey);
    }

    [Fact]
    public void Constructor_SetsDefaultTargetFolder()
    {
        // Assert
        Assert.Equal("My Drive", _viewModel.TargetFolderName);
    }

    #endregion

    #region InitializeAsync Tests

    [Fact]
    public async Task InitializeAsync_LoadsSettings()
    {
        // Act
        await _viewModel.InitializeAsync();

        // Assert
        _mockSettingsService.Verify(s => s.LoadAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_ChecksAuthStatus()
    {
        // Act
        await _viewModel.InitializeAsync();

        // Assert
        _mockAuthService.Verify(s => s.IsAuthenticatedAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WhenAuthenticated_SetsConnected()
    {
        // Arrange
        _mockAuthService.Setup(s => s.IsAuthenticatedAsync()).ReturnsAsync(true);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.True(_viewModel.IsConnected);
    }

    [Fact]
    public async Task InitializeAsync_WhenNotAuthenticated_SetsDisconnected()
    {
        // Arrange
        _mockAuthService.Setup(s => s.IsAuthenticatedAsync()).ReturnsAsync(false);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.False(_viewModel.IsConnected);
    }

    [Fact]
    public async Task InitializeAsync_LoadsEmailFromSettings()
    {
        // Arrange
        var settings = TestHelpers.CreateDefaultSettings();
        settings.Account.Email = "user@example.com";
        settings.Account.Connected = true;
        _mockSettingsService.SetupGet(s => s.Settings).Returns(settings);
        _mockAuthService.Setup(s => s.IsAuthenticatedAsync()).ReturnsAsync(true);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.Equal("user@example.com", _viewModel.AccountEmail);
    }

    [Fact]
    public async Task InitializeAsync_SetsIsLoadingDuringLoad()
    {
        // Arrange
        var loadingStates = new List<bool>();
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.IsLoading))
            {
                loadingStates.Add(_viewModel.IsLoading);
            }
        };

        // Act
        await _viewModel.InitializeAsync();

        // Assert - should have been set to true then false
        Assert.Contains(true, loadingStates);
        Assert.False(_viewModel.IsLoading); // Final state should be false
    }

    [Fact]
    public async Task InitializeAsync_OnError_SetsErrorMessage()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.LoadAsync()).ThrowsAsync(new Exception("Load failed"));

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.NotNull(_viewModel.StatusMessage);
        Assert.Contains("Game Over", _viewModel.StatusMessage);
    }

    #endregion

    #region SignInAsync Tests

    [Fact]
    public async Task SignInAsync_Success_SetsConnected()
    {
        // Arrange
        _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Google.Apis.Auth.OAuth2.UserCredential?>(null));
        _mockAuthService.Setup(s => s.GetUserEmailAsync())
            .ReturnsAsync("newuser@example.com");

        // Act
        await _viewModel.SignInCommand.ExecuteAsync(null);

        // Assert
        Assert.True(_viewModel.IsConnected);
        Assert.Equal("newuser@example.com", _viewModel.AccountEmail);
    }

    [Fact]
    public async Task SignInAsync_Success_SavesSettings()
    {
        // Arrange
        _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Google.Apis.Auth.OAuth2.UserCredential?>(null));

        // Act
        await _viewModel.SignInCommand.ExecuteAsync(null);

        // Assert
        _mockSettingsService.Verify(s => s.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task SignInAsync_Failure_SetsErrorMessage()
    {
        // Arrange
        _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Auth failed"));

        // Act
        await _viewModel.SignInCommand.ExecuteAsync(null);

        // Assert
        Assert.False(_viewModel.IsConnected);
        Assert.NotNull(_viewModel.StatusMessage);
        Assert.Contains("Mamma Mia", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task SignInAsync_SetsLoadingState()
    {
        // Arrange
        var wasLoading = false;
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.IsLoading) && _viewModel.IsLoading)
            {
                wasLoading = true;
            }
        };

        // Act
        await _viewModel.SignInCommand.ExecuteAsync(null);

        // Assert
        Assert.True(wasLoading);
        Assert.False(_viewModel.IsLoading); // Final state
    }

    #endregion

    #region SignOutAsync Tests

    [Fact]
    public async Task SignOutAsync_ClearsAccount()
    {
        // Arrange
        _viewModel.IsConnected = true;
        _viewModel.AccountEmail = "user@example.com";

        // Act
        await _viewModel.SignOutCommand.ExecuteAsync(null);

        // Assert
        Assert.False(_viewModel.IsConnected);
        Assert.Null(_viewModel.AccountEmail);
    }

    [Fact]
    public async Task SignOutAsync_CallsAuthSignOut()
    {
        // Act
        await _viewModel.SignOutCommand.ExecuteAsync(null);

        // Assert
        _mockAuthService.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_ClearsDriveCache()
    {
        // Act
        await _viewModel.SignOutCommand.ExecuteAsync(null);

        // Assert
        _mockDriveService.Verify(s => s.ClearCache(), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_SavesSettings()
    {
        // Act
        await _viewModel.SignOutCommand.ExecuteAsync(null);

        // Assert
        _mockSettingsService.Verify(s => s.SaveAsync(), Times.Once);
    }

    #endregion

    #region SaveSettingsAsync Tests

    [Fact]
    public async Task SaveSettingsAsync_PersistsToService()
    {
        // Act
        await _viewModel.SaveSettingsCommand.ExecuteAsync(null);

        // Assert
        _mockSettingsService.Verify(s => s.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveSettingsAsync_RegistersHotkey_WhenEnabled()
    {
        // Arrange
        _viewModel.HotkeyEnabled = true;
        _viewModel.HotkeyModifiers = ModifierKeys.Alt;
        _viewModel.HotkeyKey = Key.U;

        // Act
        await _viewModel.SaveSettingsCommand.ExecuteAsync(null);

        // Assert
        _mockHotkeyService.Verify(s => s.Register(ModifierKeys.Alt, Key.U), Times.Once);
    }

    [Fact]
    public async Task SaveSettingsAsync_UnregistersHotkey_WhenDisabled()
    {
        // Arrange
        _viewModel.HotkeyEnabled = false;

        // Act
        await _viewModel.SaveSettingsCommand.ExecuteAsync(null);

        // Assert
        _mockHotkeyService.Verify(s => s.Unregister(), Times.Once);
    }

    [Fact]
    public async Task SaveSettingsAsync_OnSuccess_ShowsSuccessMessage()
    {
        // Act
        await _viewModel.SaveSettingsCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(_viewModel.StatusMessage);
        Assert.Contains("saved", _viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveSettingsAsync_OnFailure_ShowsErrorMessage()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.SaveAsync()).ThrowsAsync(new Exception("Save failed"));

        // Act
        await _viewModel.SaveSettingsCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(_viewModel.StatusMessage);
        Assert.Contains("failed", _viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region ResetHotkey Tests

    [Fact]
    public void ResetHotkey_SetsDefaults()
    {
        // Arrange
        _viewModel.HotkeyModifiers = ModifierKeys.Alt | ModifierKeys.Shift;
        _viewModel.HotkeyKey = Key.X;

        // Act
        _viewModel.ResetHotkeyCommand.Execute(null);

        // Assert
        Assert.Equal(ModifierKeys.Control, _viewModel.HotkeyModifiers);
        Assert.Equal(Key.G, _viewModel.HotkeyKey);
    }

    #endregion

    #region SetTargetFolder Tests

    [Fact]
    public void SetTargetFolder_UpdatesProperties()
    {
        // Act
        _viewModel.SetTargetFolder("folder-456", "New Target Folder");

        // Assert
        Assert.Equal("folder-456", _viewModel.TargetFolderId);
        Assert.Equal("New Target Folder", _viewModel.TargetFolderName);
    }

    [Fact]
    public void SetTargetFolder_WithNull_ClearsFolder()
    {
        // Arrange
        _viewModel.TargetFolderId = "folder-123";
        _viewModel.TargetFolderName = "Old Folder";

        // Act
        _viewModel.SetTargetFolder(null!, "My Drive");

        // Assert
        Assert.Null(_viewModel.TargetFolderId);
        Assert.Equal("My Drive", _viewModel.TargetFolderName);
    }

    #endregion

    #region Property Change Notification Tests

    [Fact]
    public void IsConnected_RaisesPropertyChanged()
    {
        // Arrange
        var raised = false;
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.IsConnected))
            {
                raised = true;
            }
        };

        // Act
        _viewModel.IsConnected = true;

        // Assert
        Assert.True(raised);
    }

    [Fact]
    public void AccountEmail_RaisesPropertyChanged()
    {
        // Arrange
        var raised = false;
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.AccountEmail))
            {
                raised = true;
            }
        };

        // Act
        _viewModel.AccountEmail = "new@example.com";

        // Assert
        Assert.True(raised);
    }

    [Fact]
    public void IsLoading_RaisesPropertyChanged()
    {
        // Arrange
        var raised = false;
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.IsLoading))
            {
                raised = true;
            }
        };

        // Act
        _viewModel.IsLoading = true;

        // Assert
        Assert.True(raised);
    }

    [Fact]
    public void StatusMessage_RaisesPropertyChanged()
    {
        // Arrange
        var raised = false;
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.StatusMessage))
            {
                raised = true;
            }
        };

        // Act
        _viewModel.StatusMessage = "New message";

        // Assert
        Assert.True(raised);
    }

    #endregion

    #region Settings Synchronization Tests

    [Fact]
    public async Task InitializeAsync_LoadsAllSettingsProperties()
    {
        // Arrange
        var settings = new AppSettings
        {
            Account = new AccountSettings { Email = "sync@example.com", Connected = true },
            Upload = new UploadSettings
            {
                DefaultFolderId = "sync-folder",
                DefaultFolderName = "Sync Folder",
                OpenInBrowserAfterUpload = false,
                NotifyOnComplete = false
            },
            Hotkey = new HotkeySettings
            {
                Enabled = false,
                Modifiers = ModifierKeys.Alt,
                Key = Key.S
            },
            General = new GeneralSettings { Autostart = false }
        };
        _mockSettingsService.SetupGet(s => s.Settings).Returns(settings);
        _mockAuthService.Setup(s => s.IsAuthenticatedAsync()).ReturnsAsync(true);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.Equal("sync@example.com", _viewModel.AccountEmail);
        Assert.Equal("sync-folder", _viewModel.TargetFolderId);
        Assert.Equal("Sync Folder", _viewModel.TargetFolderName);
        Assert.False(_viewModel.OpenInBrowserAfterUpload);
        Assert.False(_viewModel.ShowNotifications);
        Assert.False(_viewModel.HotkeyEnabled);
        Assert.Equal(ModifierKeys.Alt, _viewModel.HotkeyModifiers);
        Assert.Equal(Key.S, _viewModel.HotkeyKey);
        // Note: Autostart is checked from Registry, not from settings
        // The ViewModel calls RegistryHelper.IsAutostartEnabled() which cannot be mocked
    }

    #endregion
}
