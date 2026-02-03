using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using Serilog;

namespace Share2GoogleDrive.Services;

/// <summary>
/// Service for handling Google OAuth2 authentication.
/// </summary>
public interface IGoogleAuthService
{
    Task<UserCredential?> AuthenticateAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAuthenticatedAsync();
    Task SignOutAsync();
    Task<string?> GetUserEmailAsync();
    UserCredential? CurrentCredential { get; }
}

public class GoogleAuthService : IGoogleAuthService
{
    private readonly ISettingsService _settingsService;
    private readonly string _credentialsPath;
    private readonly string _tokenPath;

    private static readonly string[] Scopes =
    {
        DriveService.Scope.DriveFile,
        DriveService.Scope.DriveMetadataReadonly
    };

    public UserCredential? CurrentCredential { get; private set; }

    public GoogleAuthService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _credentialsPath = Path.Combine(_settingsService.AppDataPath, "credentials.json");
        _tokenPath = Path.Combine(_settingsService.AppDataPath, "token");
    }

    public async Task<UserCredential?> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_credentialsPath))
            {
                Log.Warning("credentials.json not found at {Path}", _credentialsPath);
                throw new FileNotFoundException(
                    "Google API credentials not found. Please place credentials.json in the app data folder.",
                    _credentialsPath);
            }

            using var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read);
            var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

            CurrentCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                Scopes,
                "user",
                cancellationToken,
                new FileDataStore(_tokenPath, true));

            if (CurrentCredential.Token.IsStale)
            {
                await CurrentCredential.RefreshTokenAsync(cancellationToken);
            }

            // Update settings with account info
            var email = await GetUserEmailAsync();
            _settingsService.Settings.Account.Email = email;
            _settingsService.Settings.Account.Connected = true;
            await _settingsService.SaveAsync();

            Log.Information("Successfully authenticated as {Email}", email);
            return CurrentCredential;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Authentication failed");
            throw;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            if (CurrentCredential != null && !CurrentCredential.Token.IsStale)
            {
                return true;
            }

            // Try to load existing token
            if (!File.Exists(_credentialsPath))
            {
                return false;
            }

            var tokenFile = Path.Combine(_tokenPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");
            if (!File.Exists(tokenFile))
            {
                return false;
            }

            // Try to authenticate silently
            using var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read);
            var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

            CurrentCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(_tokenPath, true));

            if (CurrentCredential.Token.IsStale)
            {
                await CurrentCredential.RefreshTokenAsync(CancellationToken.None);
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Silent authentication check failed");
            return false;
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            // Delete token files
            if (Directory.Exists(_tokenPath))
            {
                Directory.Delete(_tokenPath, true);
            }

            CurrentCredential = null;

            // Update settings
            _settingsService.Settings.Account.Email = null;
            _settingsService.Settings.Account.Connected = false;
            await _settingsService.SaveAsync();

            Log.Information("User signed out successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to sign out");
            throw;
        }
    }

    public async Task<string?> GetUserEmailAsync()
    {
        if (CurrentCredential == null)
        {
            return null;
        }

        try
        {
            var service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = CurrentCredential,
                ApplicationName = "Share2GoogleDrive"
            });

            var about = await service.About.Get().ExecuteAsync();
            return about.User?.EmailAddress;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get user email");
            return _settingsService.Settings.Account.Email;
        }
    }
}
