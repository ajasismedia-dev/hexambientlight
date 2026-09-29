using HexAmbientLight.Core.Effects;
using Xunit;
using System;

namespace HexAmbientLight.Tests;

public class ColorUtilsTests
{
    [Fact]
    public void ParseHex_ValidHex_ReturnsCorrectColor()
    {
        var c = ColorUtils.ParseHex("#FF0080");
        Assert.Equal(255, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(128, c.B);
    }

    [Fact]
    public void HsvToRgb_Red()
    {
        var c = ColorUtils.HsvToRgb(0, 1.0f, 1.0f);
        Assert.Equal(255, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
    }

    [Fact]
    public void HsvToRgb_Cyan()
    {
        var c = ColorUtils.HsvToRgb(180, 1.0f, 1.0f);
        Assert.Equal(0, c.R);
        Assert.Equal(255, c.G);
        Assert.Equal(255, c.B);
    }

    [Fact]
    public void RgbToHsv_Red()
    {
        var (h, s, v) = ColorUtils.RgbToHsv(255, 0, 0);
        Assert.Equal(0, h);
        Assert.Equal(1.0f, s);
        Assert.Equal(1.0f, v);
    }

    [Fact]
    public void RgbToHsv_Cyan()
    {
        var (h, s, v) = ColorUtils.RgbToHsv(0, 255, 255);
        Assert.Equal(180, h);
        Assert.Equal(1.0f, s);
        Assert.Equal(1.0f, v);
    }
}
