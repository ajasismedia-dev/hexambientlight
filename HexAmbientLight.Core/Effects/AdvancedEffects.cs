using System;

namespace HexAmbientLight.Core.Effects;

public class ColorWaveEffect : ILightingEffect
{
    public string Id => "ColorWave";
    public string NameKey => "Effect.ColorWave.Name";
    public string DescriptionKey => "Effect.ColorWave.Desc";

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        double hz = 0.5 + (context.Speed * 2.5);
        double offset = (context.ElapsedSeconds * hz * context.Direction) % 1.0;
        if (offset < 0) offset += 1.0;

        for (int i = 0; i < context.TotalLeds; i++)
        {
            double pos = ((double)i / context.TotalLeds + offset) % 1.0;
            float factor = (float)Math.Pow(Math.Sin(pos * Math.PI), 2);
            
            var c = ColorUtils.Lerp(context.SecondaryColor, context.PrimaryColor, factor);
            
            rgbData[i * 3] = (byte)(c.R * context.Intensity);
            rgbData[i * 3 + 1] = (byte)(c.G * context.Intensity);
            rgbData[i * 3 + 2] = (byte)(c.B * context.Intensity);
        }
    }
}

public class AuroraEffect : ILightingEffect
{
    public string Id => "Aurora";
    public string NameKey => "Effect.Aurora.Name";
    public string DescriptionKey => "Effect.Aurora.Desc";

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        double hz1 = 0.1 + (context.Speed * 0.4);
        double hz2 = 0.15 + (context.Speed * 0.6);
        double hz3 = 0.2 + (context.Speed * 0.8);

        double t = context.ElapsedSeconds * context.Direction;

        for (int i = 0; i < context.TotalLeds; i++)
        {
            double norm = (double)i / context.TotalLeds;
            
            double w1 = Math.Sin(t * hz1 + norm * Math.PI * 2) * 0.5 + 0.5;
            double w2 = Math.Sin(t * hz2 - norm * Math.PI * 4) * 0.5 + 0.5;
            double w3 = Math.Sin(t * hz3 + norm * Math.PI * 6) * 0.5 + 0.5;

            double sum = w1 + w2 + w3;
            if (sum == 0) sum = 1;

            w1 /= sum;
            w2 /= sum;
            w3 /= sum;

            double r = context.PrimaryColor.R * w1 + context.SecondaryColor.R * w2 + context.TertiaryColor.R * w3;
            double g = context.PrimaryColor.G * w1 + context.SecondaryColor.G * w2 + context.TertiaryColor.G * w3;
            double b = context.PrimaryColor.B * w1 + context.SecondaryColor.B * w2 + context.TertiaryColor.B * w3;

            rgbData[i * 3] = (byte)(Math.Clamp(r * context.Intensity, 0, 255));
            rgbData[i * 3 + 1] = (byte)(Math.Clamp(g * context.Intensity, 0, 255));
            rgbData[i * 3 + 2] = (byte)(Math.Clamp(b * context.Intensity, 0, 255));
        }
    }
}

public class CyberEffect : ILightingEffect
{
    public string Id => "Cyber";
    public string NameKey => "Effect.Cyber.Name";
    public string DescriptionKey => "Effect.Cyber.Desc";

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        double hz = 0.2 + (context.Speed * 1.5);
        double t = context.ElapsedSeconds * hz * context.Direction;

        for (int i = 0; i < context.TotalLeds; i++)
        {
            double norm = (double)i / context.TotalLeds;
            
            double wave = Math.Sin(t * Math.PI * 2 + norm * Math.PI * 4);
            double pulse = Math.Pow(Math.Sin(t * Math.PI * 1.5 - norm * Math.PI * 2), 8);

            float factor = (float)(wave * 0.5 + 0.5);
            var c = ColorUtils.Lerp(context.PrimaryColor, context.SecondaryColor, factor);
            
            double pr = c.R + pulse * 255;
            double pg = c.G + pulse * 255;
            double pb = c.B + pulse * 255;

            rgbData[i * 3] = (byte)Math.Clamp(pr * context.Intensity, 0, 255);
            rgbData[i * 3 + 1] = (byte)Math.Clamp(pg * context.Intensity, 0, 255);
            rgbData[i * 3 + 2] = (byte)Math.Clamp(pb * context.Intensity, 0, 255);
        }
    }
}

public class FireEffect : ILightingEffect
{
    public string Id => "Fire";
    public string NameKey => "Effect.Fire.Name";
    public string DescriptionKey => "Effect.Fire.Desc";
    
    private double Noise(double x)
    {
        return Math.Abs(Math.Sin(x * 12.9898) * 43758.5453) % 1.0;
    }

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        double hz = 2.0 + (context.Speed * 4.0);
        double t = context.ElapsedSeconds * hz;

        for (int i = 0; i < context.TotalLeds; i++)
        {
            double pos = (double)i / context.TotalLeds;
            
            double n1 = Noise(pos * 10 + Math.Floor(t));
            double n2 = Noise(pos * 20 - Math.Floor(t * 1.5));
            double blend = t % 1.0;
            
            double noise = n1 * (1 - blend) + n2 * blend;
            
            float factor = (float)noise;
            RgbColor c;
            if (factor < 0.5)
            {
                c = ColorUtils.Lerp(context.TertiaryColor, context.PrimaryColor, factor * 2f);
            }
            else
            {
                c = ColorUtils.Lerp(context.PrimaryColor, context.SecondaryColor, (factor - 0.5f) * 2f);
            }

            rgbData[i * 3] = (byte)(c.R * context.Intensity);
            rgbData[i * 3 + 1] = (byte)(c.G * context.Intensity);
            rgbData[i * 3 + 2] = (byte)(c.B * context.Intensity);
        }
    }
}
