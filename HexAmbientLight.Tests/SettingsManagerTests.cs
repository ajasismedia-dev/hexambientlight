using System;
using System.IO;
using System.Threading.Tasks;
using HexAmbientLight.Core.Services;
using Xunit;

namespace HexAmbientLight.Tests;

public class SettingsManagerTests
{
    [Fact]
    public async Task InitializeAsync_CreatesDefaultSettings_IfNotExist()
    {
        // SettingsManager uses LocalAppData, so this test will actually write to %localappdata%/HexAmbientLight
        // In a real test project we might want to abstract the file system or path, but for MVP verification this is ok.
        var settingsManager = new SettingsManager();
        await settingsManager.InitializeAsync();
        
        Assert.NotNull(settingsManager.AppSettings);
        Assert.NotNull(settingsManager.LayoutConfig);
        Assert.True(settingsManager.AppSettings.ConfigSchemaVersion == 1);
        Assert.True(settingsManager.LayoutConfig.TotalLeds == 54); // Default
    }
}
