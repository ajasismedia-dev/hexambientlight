using System;

namespace HexAmbientLight.Core.Effects;

public struct RgbColor
{
    public byte R, G, B;
    public RgbColor(byte r, byte g, byte b) { R = r; G = g; B = b; }
}

public static class ColorUtils
{
    public static RgbColor HsvToRgb(float h, float s, float v)
    {
        float c = v * s;
        float hPrime = h / 60f;
        float x = c * (1f - Math.Abs((hPrime % 2f) - 1f));
        float m = v - c;

        float r1 = 0, g1 = 0, b1 = 0;
        if (hPrime >= 0 && hPrime < 1) { r1 = c; g1 = x; b1 = 0; }
        else if (hPrime >= 1 && hPrime < 2) { r1 = x; g1 = c; b1 = 0; }
        else if (hPrime >= 2 && hPrime < 3) { r1 = 0; g1 = c; b1 = x; }
        else if (hPrime >= 3 && hPrime < 4) { r1 = 0; g1 = x; b1 = c; }
        else if (hPrime >= 4 && hPrime < 5) { r1 = x; g1 = 0; b1 = c; }
        else if (hPrime >= 5 && hPrime < 6) { r1 = c; g1 = 0; b1 = x; }

        return new RgbColor(
            (byte)((r1 + m) * 255f),
            (byte)((g1 + m) * 255f),
            (byte)((b1 + m) * 255f)
        );
    }

    public static (float H, float S, float V) RgbToHsv(byte r, byte g, byte b)
    {
        float rNorm = r / 255f;
        float gNorm = g / 255f;
        float bNorm = b / 255f;

        float max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
        float min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
        float delta = max - min;

        float h = 0;
        if (delta > 0)
        {
            if (max == rNorm) h = 60f * (((gNorm - bNorm) / delta) % 6f);
            else if (max == gNorm) h = 60f * (((bNorm - rNorm) / delta) + 2f);
            else if (max == bNorm) h = 60f * (((rNorm - gNorm) / delta) + 4f);
        }
        if (h < 0) h += 360f;

        float s = max == 0 ? 0 : delta / max;
        float v = max;

        return (h, s, v);
    }

    public static RgbColor Lerp(RgbColor a, RgbColor b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new RgbColor(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t)
        );
    }
    public static RgbColor ParseHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return new RgbColor(0, 0, 0);
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            return new RgbColor(
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16)
            );
        }
        return new RgbColor(0, 0, 0);
    }
    
    public static string ToHex(RgbColor c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}

public class LightingEffectContext
{
    public int TotalLeds { get; set; }
    public double ElapsedSeconds { get; set; }
    public double DeltaSeconds { get; set; }
    public RgbColor PrimaryColor { get; set; }
    public RgbColor SecondaryColor { get; set; }
    public RgbColor TertiaryColor { get; set; }
    public double Speed { get; set; } = 1.0;
    public double Intensity { get; set; } = 1.0;
    public int Direction { get; set; } = 1;
}

public interface ILightingEffect
{
    string Id { get; }
    string NameKey { get; }
    string DescriptionKey { get; }
    void Render(Span<byte> rgbData, LightingEffectContext context);
}
