using System.IO;
using System.Text.Json;
using Share2GoogleDrive.Models;
using Serilog;

namespace Share2GoogleDrive.Services;

/// <summary>
/// Service for managing application settings.
/// </summary>
public interface ISettingsService
{
    AppSettings Settings { get; }
    Task LoadAsync();
    Task SaveAsync();
    string AppDataPath { get; }
}

public class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettings Settings { get; private set; } = new();

    public string AppDataPath { get; }
    private string SettingsFilePath => Path.Combine(AppDataPath, "settings.json");

    public SettingsService()
    {
        AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Share2GoogleDrive");

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
                Log.Information("Settings loaded from {Path}", SettingsFilePath);
            }
            else
            {
                Settings = new AppSettings();
                await SaveAsync();
                Log.Information("Created default settings at {Path}", SettingsFilePath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load settings, using defaults");
            Settings = new AppSettings();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            await File.WriteAllTextAsync(SettingsFilePath, json);
            Log.Information("Settings saved to {Path}", SettingsFilePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save settings");
            throw;
        }
    }
}
