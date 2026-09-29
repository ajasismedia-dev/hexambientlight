using System;
using HexAmbientLight.Core.Models;

namespace HexAmbientLight.Core.Services;

public class EdgeColorExtractor
{
    private const int EdgeThicknessPercentage = 3; // e.g., 3% of screen size for block depth

    public static byte[] Extract(
        ReadOnlySpan<byte> bgraData,
        int width,
        int height,
        LedLayoutConfig layout)
    {
        int totalLeds = layout.TotalLeds;
        if (totalLeds == 0) return Array.Empty<byte>();

        byte[] rgbOutput = new byte[totalLeds * 3];

        int depthX = Math.Max(1, (width * EdgeThicknessPercentage) / 100);
        int depthY = Math.Max(1, (height * EdgeThicknessPercentage) / 100);

        int ledIndex = 0;

        // If clockwise: Left (bottom->top), Top (left->right), Right (top->bottom)
        // Adjust based on user offset/reverse later. For now, strict Clockwise sequence.

        // Left Edge
        if (layout.LeftCount > 0)
        {
            int blockHeight = height / layout.LeftCount;
            for (int i = 0; i < layout.LeftCount; i++)
            {
                // Bottom to top
                int yStart = height - ((i + 1) * blockHeight);
                int yEnd = height - (i * blockHeight);
                int xStart = 0;
                int xEnd = depthX;

                var (r, g, b) = AverageBlock(bgraData, width, xStart, xEnd, yStart, yEnd);
                
                rgbOutput[ledIndex * 3] = r;
                rgbOutput[ledIndex * 3 + 1] = g;
                rgbOutput[ledIndex * 3 + 2] = b;
                ledIndex++;
            }
        }

        // Top Edge
        if (layout.TopCount > 0)
        {
            int blockWidth = width / layout.TopCount;
            for (int i = 0; i < layout.TopCount; i++)
            {
                // Left to right
                int xStart = i * blockWidth;
                int xEnd = (i + 1) * blockWidth;
                int yStart = 0;
                int yEnd = depthY;

                var (r, g, b) = AverageBlock(bgraData, width, xStart, xEnd, yStart, yEnd);
                
                rgbOutput[ledIndex * 3] = r;
                rgbOutput[ledIndex * 3 + 1] = g;
                rgbOutput[ledIndex * 3 + 2] = b;
                ledIndex++;
            }
        }

        // Right Edge
        if (layout.RightCount > 0)
        {
            int blockHeight = height / layout.RightCount;
            for (int i = 0; i < layout.RightCount; i++)
            {
                // Top to bottom
                int yStart = i * blockHeight;
                int yEnd = (i + 1) * blockHeight;
                int xStart = width - depthX;
                int xEnd = width;

                var (r, g, b) = AverageBlock(bgraData, width, xStart, xEnd, yStart, yEnd);
                
                rgbOutput[ledIndex * 3] = r;
                rgbOutput[ledIndex * 3 + 1] = g;
                rgbOutput[ledIndex * 3 + 2] = b;
                ledIndex++;
            }
        }

        return rgbOutput;
    }

    private static (byte r, byte g, byte b) AverageBlock(
        ReadOnlySpan<byte> bgra, int strideWidth, 
        int xStart, int xEnd, int yStart, int yEnd)
    {
        long sumR = 0, sumG = 0, sumB = 0;
        int count = 0;

        for (int y = yStart; y < yEnd; y++)
        {
            int rowOffset = y * strideWidth * 4;
            for (int x = xStart; x < xEnd; x++)
            {
                int pixelOffset = rowOffset + (x * 4);
                
                sumB += bgra[pixelOffset];
                sumG += bgra[pixelOffset + 1];
                sumR += bgra[pixelOffset + 2];
                // bgra[pixelOffset + 3] is Alpha, ignore
                count++;
            }
        }

        if (count == 0) return (0, 0, 0);

        return (
            (byte)(sumR / count),
            (byte)(sumG / count),
            (byte)(sumB / count)
        );
    }
}
