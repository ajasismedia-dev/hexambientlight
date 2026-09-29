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
        var tempDir = Path.Combine(Path.GetTempPath(), "HexAmbientLight.Tests", Guid.NewGuid().ToString());
        try
        {
            var settingsManager = new SettingsManager(tempDir);
            await settingsManager.InitializeAsync();
            
            Assert.NotNull(settingsManager.AppSettings);
            Assert.NotNull(settingsManager.LayoutConfig);
            Assert.True(settingsManager.AppSettings.ConfigSchemaVersion == 1);
            Assert.True(settingsManager.LayoutConfig.TotalLeds == 54);
            Assert.Equal("192.168.1.2", settingsManager.AppSettings.WledIpAddress);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}
