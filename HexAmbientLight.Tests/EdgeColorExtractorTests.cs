using System;
using HexAmbientLight.Core.Models;
using HexAmbientLight.Core.Services;
using Xunit;

namespace HexAmbientLight.Tests;

public class EdgeColorExtractorTests
{
    [Fact]
    public void Extract_ReturnsCorrectArraySize()
    {
        var layout = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        var bgra = new byte[100 * 100 * 4];
        
        var result = EdgeColorExtractor.Extract(bgra, 100, 100, layout);
        
        Assert.Equal(40 * 3, result.Length);
    }

        [Fact]
    public void PhysicalMapping_CorrectlyExtractsLedPositions()
    {
        int w = 1000;
        int h = 1000;
        var layout = new LedLayoutConfig 
        { 
            LeftCount = 14, TopCount = 26, RightCount = 14, TotalLeds = 54 
        };
        
        var bgra = new byte[w * h * 4];

        void FillRegion(int xStart, int xEnd, int yStart, int yEnd, byte b, byte g, byte r)
        {
            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    int idx = (y * w + x) * 4;
                    bgra[idx] = b;
                    bgra[idx+1] = g;
                    bgra[idx+2] = r;
                    bgra[idx+3] = 255;
                }
            }
        }

        // Left depth = 30 (0..29). Block h = 1000/14 = 71.
        // LED 0 (Left Bottom): X:0..29, Y: 1000 - 71 = 929..1000. 
        // Bottom overlap is Y > 1000-30=970? Wait, there is no bottom strip! So Y:930..990 is safe.
        FillRegion(10, 20, 940, 960, 0, 0, 255); // Red in LED 0

        // Top depth = 30 (0..29). Block w = 1000/26 = 38.
        // LED 13 (Left Top): X:0..29, Y:0..71. Overlap with Top is Y:0..29.
        // So Y:40..60 is safe for LED 13 only.
        FillRegion(10, 20, 40, 60, 0, 255, 0); // Green in LED 13

        // LED 14 (Top Left): X:0..38, Y:0..29. Overlap with Left is X:0..29.
        // So X:32..37 is safe for LED 14 only.
        FillRegion(32, 37, 10, 20, 255, 0, 0); // Blue in LED 14

        // LED 39 (Top Right): X: 1000-38 = 962..1000, Y:0..29. Overlap with Right is X:970..1000 (1000-30).
        // So X: 963..968 is safe for LED 39 only.
        FillRegion(963, 968, 10, 20, 255, 255, 0); // Cyan in LED 39

        // Right depth = 30 (970..1000). Block h = 71.
        // LED 40 (Right Top): X:970..1000, Y:0..71. Overlap with Top is Y:0..29.
        // So Y:40..60 is safe for LED 40 only.
        FillRegion(980, 990, 40, 60, 255, 0, 255); // Magenta in LED 40

        // LED 53 (Right Bottom): X:970..1000, Y:929..1000.
        FillRegion(980, 990, 940, 960, 0, 255, 255); // Yellow in LED 53

        var result = EdgeColorExtractor.Extract(bgra, w, h, layout);

        Assert.Equal(54 * 3, result.Length);

        // Check LED 0 (Red)
        Assert.True(result[0 * 3] > 0);
        Assert.Equal(0, result[0 * 3 + 1]);
        Assert.Equal(0, result[0 * 3 + 2]);

        // Check LED 13 (Green)
        Assert.Equal(0, result[13 * 3]);
        Assert.True(result[13 * 3 + 1] > 0);
        Assert.Equal(0, result[13 * 3 + 2]);

        // Check LED 14 (Blue)
        Assert.Equal(0, result[14 * 3]);
        Assert.Equal(0, result[14 * 3 + 1]);
        Assert.True(result[14 * 3 + 2] > 0);

        // Check LED 39 (Cyan)
        Assert.Equal(0, result[39 * 3]);
        Assert.True(result[39 * 3 + 1] > 0);
        Assert.True(result[39 * 3 + 2] > 0);

        // Check LED 40 (Magenta)
        Assert.True(result[40 * 3] > 0);
        Assert.Equal(0, result[40 * 3 + 1]);
        Assert.True(result[40 * 3 + 2] > 0);

        // Check LED 53 (Yellow)
        Assert.True(result[53 * 3] > 0);
        Assert.True(result[53 * 3 + 1] > 0);
        Assert.Equal(0, result[53 * 3 + 2]);
    }
}
