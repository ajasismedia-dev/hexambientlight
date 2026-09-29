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
        int w = 100;
        int h = 100;
        var layout = new LedLayoutConfig 
        { 
            LeftCount = 14, TopCount = 26, RightCount = 14, TotalLeds = 54 
        };
        
        var bgra = new byte[w * h * 4];

        // Helper to fill a region
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

        int leftBh = h / 14; // 7
        int topBw = w / 26; // 3
        int rightBh = h / 14; // 7

        // LED 0: Left Bottom
        FillRegion(1, 2, 95, 96, 0, 0, 255); // Red

        // LED 13: Left Top
        FillRegion(1, 2, 5, 6, 0, 255, 0); // Green

        // LED 14: Top Left
        FillRegion(1, 2, 1, 2, 255, 0, 0); // Blue

        // LED 39: Top Right
        FillRegion(76, 77, 1, 2, 255, 255, 0); // Cyan

        // LED 40: Right Top
        FillRegion(98, 99, 1, 2, 255, 0, 255); // Magenta

        // LED 53: Right Bottom
        FillRegion(98, 99, 95, 96, 0, 255, 255); // Yellow

        var result = EdgeColorExtractor.Extract(bgra, w, h, layout);

        Assert.Equal(54 * 3, result.Length);

        // Averages will be (255 * 1) / (3*7) for left/right = 255 / 21 = 12
        // For top it will be (255 * 1) / (3*3) = 255 / 9 = 28
        // Let's just assert > 0 instead of 255
        
        // Check LED 0 (Red)
        Assert.True(result[0 * 3] > 0); // R
        Assert.Equal(0, result[0 * 3 + 1]); // G
        Assert.Equal(0, result[0 * 3 + 2]); // B

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
