using HexAmbientLight.Core.Models;
using HexAmbientLight.Core.Services;
using Xunit;

namespace HexAmbientLight.Tests;

public class ImagePostProcessorTests
{
    [Fact]
    public void ApplyPostProcessing_Brightness_ScalesCorrectly()
    {
        var processor = new ImagePostProcessor();
        var settings = new AppSettings { BrightnessLimit = 127, Saturation = 1.0, SmoothingFactor = 0.0 };
        byte[] rgbData = { 255, 100, 0 };

        processor.ApplyPostProcessing(rgbData, settings);

        Assert.Equal(127, rgbData[0]); // 255 * (127/255)
        Assert.Equal(49, rgbData[1]);  // 100 * (127/255) ~ 49.8, clamped to 49 because int cast truncates! Wait Math.Clamp with float to byte might round differently. Actually (byte)Math.Clamp(100 * 127/255f) = (byte)49.803f = 49.
        Assert.Equal(0, rgbData[2]);
    }

    [Fact]
    public void ApplyPostProcessing_Saturation_ZeroMakesGrayscale()
    {
        var processor = new ImagePostProcessor();
        var settings = new AppSettings { BrightnessLimit = 255, Saturation = 0.0, SmoothingFactor = 0.0 };
        byte[] rgbData = { 200, 100, 50 }; // Luminance = 0.299*200 + 0.587*100 + 0.114*50 = 59.8 + 58.7 + 5.7 = 124.2

        processor.ApplyPostProcessing(rgbData, settings);

        Assert.Equal(124, rgbData[0]);
        Assert.Equal(124, rgbData[1]);
        Assert.Equal(124, rgbData[2]);
    }

    [Fact]
    public void ApplyPostProcessing_Smoothing_InterpolatesCorrectly()
    {
        var processor = new ImagePostProcessor();
        var settings = new AppSettings { BrightnessLimit = 255, Saturation = 1.0, SmoothingFactor = 0.5 };
        
        byte[] frame1 = { 0, 0, 0 };
        processor.ApplyPostProcessing(frame1, settings); // First frame is not smoothed

        byte[] frame2 = { 100, 200, 50 };
        processor.ApplyPostProcessing(frame2, settings); // 0*0.5 + 100*0.5 = 50

        Assert.Equal(50, frame2[0]);
        Assert.Equal(100, frame2[1]);
        Assert.Equal(25, frame2[2]);
    }
}
