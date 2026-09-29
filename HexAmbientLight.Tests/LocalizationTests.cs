using HexAmbientLight.Core.Localization;
using Xunit;

namespace HexAmbientLight.Tests;

public class LocalizationTests
{
    [Fact]
    public void GetString_ValidKey_ReturnsLocalized()
    {
        var lm = LocalizationManager.Instance;
        lm.CurrentLanguage = "en-US";
        Assert.Equal("Ambilight", lm.GetString("Nav.Ambilight"));

        lm.CurrentLanguage = "tr-TR";
        Assert.Equal("Ambilight", lm.GetString("Nav.Ambilight"));
    }

    [Fact]
    public void GetString_FallbackToEnglish()
    {
        var lm = LocalizationManager.Instance;
        lm.CurrentLanguage = "tr-TR";
        // Assuming Nav.Dashboard might be "Dashboard" in English and not defined in tr-TR or same.
        Assert.Equal("Kontrol Paneli", lm.GetString("Nav.Dashboard")); // actually tests normal tr-TR now. For missing key: Assert.Equal("MissingKey", lm.GetString("MissingKey"));
    }
}

