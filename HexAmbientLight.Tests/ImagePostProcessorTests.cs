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

        Assert.Equal(127, rgbData[0]);
        Assert.Equal(49, rgbData[1]);
        Assert.Equal(0, rgbData[2]);
    }

    [Fact]
    public void ApplyPostProcessing_Saturation_ZeroMakesGrayscale()
    {
        var processor = new ImagePostProcessor();
        var settings = new AppSettings { BrightnessLimit = 255, Saturation = 0.0, SmoothingFactor = 0.0 };
        byte[] rgbData = { 200, 100, 50 };

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
        processor.ApplyPostProcessing(frame1, settings);

        byte[] frame2 = { 100, 200, 50 };
        processor.ApplyPostProcessing(frame2, settings);

        Assert.Equal(50, frame2[0]);
        Assert.Equal(100, frame2[1]);
        Assert.Equal(25, frame2[2]);
    }

    [Fact]
    public void ApplyPostProcessing_Smoothing_DoesNotFreezeAtMax()
    {
        var processor = new ImagePostProcessor();
        // Even if user requests 1.0 (100%), it should not completely freeze the output
        var settings = new AppSettings { BrightnessLimit = 255, Saturation = 1.0, SmoothingFactor = 1.0 };
        
        byte[] frame1 = { 0, 0, 0 };
        processor.ApplyPostProcessing(frame1, settings);

        byte[] frame2 = { 100, 100, 100 };
        processor.ApplyPostProcessing(frame2, settings);

        // It should progress towards 100, e.g., 5 at 0.95 smoothing (100 * 0.05 = 5)
        Assert.True(frame2[0] > 0);
        Assert.True(frame2[0] < 100);
    }

    [Fact]
    public void Reset_ClearsSmoothingState()
    {
        var processor = new ImagePostProcessor();
        var settings = new AppSettings { BrightnessLimit = 255, Saturation = 1.0, SmoothingFactor = 0.5 };
        
        byte[] frame1 = { 0, 0, 0 };
        processor.ApplyPostProcessing(frame1, settings);

        processor.Reset();

        // After reset, next frame is treated as first frame (no smoothing)
        byte[] frame2 = { 100, 100, 100 };
        processor.ApplyPostProcessing(frame2, settings);

        Assert.Equal(100, frame2[0]);
    }
}
