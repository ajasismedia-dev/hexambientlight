using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HexAmbientLight.Core.Models;
using Serilog;

namespace HexAmbientLight.Core.Services;

public class SettingsManager
{
    private readonly string _settingsDirectory;
    private readonly string _settingsFilePath;
    private readonly string _layoutFilePath;
    
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    
    public AppSettings AppSettings { get; set; } = new();
    public LedLayoutConfig LayoutConfig { get; set; } = new();

        public SettingsManager(string? baseDirectory = null)
    {
        if (baseDirectory != null)
        {
            _settingsDirectory = baseDirectory;
        }
        else
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _settingsDirectory = Path.Combine(appData, "HexAmbientLight");
        }
        _settingsFilePath = Path.Combine(_settingsDirectory, "appsettings.json");
        _layoutFilePath = Path.Combine(_settingsDirectory, "ledlayout.json");
    }

        public async Task InitializeAsync()
    {
        if (!Directory.Exists(_settingsDirectory))
        {
            Directory.CreateDirectory(_settingsDirectory);
            Log.Information("Created settings directory at {Path}", _settingsDirectory);
        }

        await LoadSettingsAsync();
        await LoadLayoutAsync();

        // Apply localization
        if (string.IsNullOrEmpty(AppSettings.UiLanguage) || AppSettings.UiLanguage == "auto")
        {
            var culture = System.Globalization.CultureInfo.InstalledUICulture.Name;
            AppSettings.UiLanguage = culture.StartsWith("tr") ? "tr-TR" : "en-US";
            _ = SaveSettingsAsync();
        }
        HexAmbientLight.Core.Localization.LocalizationManager.Instance.CurrentLanguage = AppSettings.UiLanguage;
    }

    private async Task LoadSettingsAsync()
    {
        Log.Information("SETTINGS INIT START");
        Log.Information("Config path: {Path}", _settingsFilePath);
        Log.Information("Config exists: {Exists}", File.Exists(_settingsFilePath));

        if (!File.Exists(_settingsFilePath))
        {
            AppSettings = new AppSettings { WledIpAddress = "192.168.1.2", UiLanguage = "auto" };
            await SaveSettingsAsync();
        }
        else
        {
            try
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    AppSettings = loaded;
                    if (string.IsNullOrWhiteSpace(AppSettings.WledIpAddress))
                    {
                        AppSettings.WledIpAddress = "192.168.1.2";
                        await SaveSettingsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load AppSettings. Using defaults and backing up corrupted file.");
                BackupCorruptedFile(_settingsFilePath);
                AppSettings = new AppSettings { WledIpAddress = "192.168.1.2", UiLanguage = "auto" };
                await SaveSettingsAsync();
            }
        }

        Log.Information("WledIpAddress: {Ip}", AppSettings.WledIpAddress);
        Log.Information("TargetFps: {Fps}", AppSettings.TargetFps);
        Log.Information("SelectedMode: Auto"); // As requested for log output
        Log.Information("SETTINGS INIT COMPLETE");
    }

    private async Task LoadLayoutAsync()
    {
        if (!File.Exists(_layoutFilePath))
        {
            LayoutConfig = new LedLayoutConfig();
            await SaveLayoutAsync();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_layoutFilePath);
            var loaded = JsonSerializer.Deserialize<LedLayoutConfig>(json);
            if (loaded != null)
            {
                LayoutConfig = loaded;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load LedLayoutConfig. Using defaults and backing up corrupted file.");
            BackupCorruptedFile(_layoutFilePath);
            LayoutConfig = new LedLayoutConfig();
            await SaveLayoutAsync();
        }
    }

    public async Task SaveSettingsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(AppSettings, new JsonSerializerOptions { WriteIndented = true });
            await WriteAtomicallyAsync(_settingsFilePath, json);
            Log.Debug("AppSettings saved successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save AppSettings.");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveLayoutAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(LayoutConfig, new JsonSerializerOptions { WriteIndented = true });
            await WriteAtomicallyAsync(_layoutFilePath, json);
            Log.Debug("LedLayoutConfig saved successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save LedLayoutConfig.");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task WriteAtomicallyAsync(string filePath, string content)
    {
        var tempPath = filePath + ".tmp";
        await File.WriteAllTextAsync(tempPath, content);
        File.Move(tempPath, filePath, overwrite: true);
    }

    private void BackupCorruptedFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var backupPath = filePath + ".bak_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                File.Move(filePath, backupPath);
                Log.Information("Backed up corrupted config file to {BackupPath}", backupPath);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to backup corrupted file: {FilePath}", filePath);
        }
    }
}

