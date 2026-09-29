using System;
using HexAmbientLight.Core.Models;

namespace HexAmbientLight.Core.Services;

public class ImagePostProcessor
{
    private byte[]? _previousRgbData;

    public void ApplyPostProcessing(byte[] rgbData, AppSettings settings)
    {
        float brightnessLimit = settings.BrightnessLimit / 255f;
        float saturation = (float)settings.Saturation;
        float smoothing = (float)settings.SmoothingFactor;

        if (_previousRgbData == null || _previousRgbData.Length != rgbData.Length)
        {
            _previousRgbData = new byte[rgbData.Length];
            smoothing = 0; // Don't smooth first frame
        }

        for (int i = 0; i < rgbData.Length; i += 3)
        {
            float r = rgbData[i];
            float g = rgbData[i + 1];
            float b = rgbData[i + 2];

            // 1. Saturation
            if (Math.Abs(saturation - 1.0f) > 0.01f)
            {
                float luminance = 0.299f * r + 0.587f * g + 0.114f * b;
                r = luminance + (r - luminance) * saturation;
                g = luminance + (g - luminance) * saturation;
                b = luminance + (b - luminance) * saturation;
            }

            // 2. Brightness
            r *= brightnessLimit;
            g *= brightnessLimit;
            b *= brightnessLimit;

            // 3. Smoothing
            if (smoothing > 0.0f && smoothing <= 1.0f)
            {
                r = _previousRgbData[i] * smoothing + r * (1.0f - smoothing);
                g = _previousRgbData[i + 1] * smoothing + g * (1.0f - smoothing);
                b = _previousRgbData[i + 2] * smoothing + b * (1.0f - smoothing);
            }

            // Clamp and assign
            rgbData[i] = (byte)Math.Clamp(r, 0, 255);
            rgbData[i + 1] = (byte)Math.Clamp(g, 0, 255);
            rgbData[i + 2] = (byte)Math.Clamp(b, 0, 255);

            _previousRgbData[i] = rgbData[i];
            _previousRgbData[i + 1] = rgbData[i + 1];
            _previousRgbData[i + 2] = rgbData[i + 2];
        }
    }
}
