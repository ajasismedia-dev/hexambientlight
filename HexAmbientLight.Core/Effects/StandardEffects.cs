using System;

namespace HexAmbientLight.Core.Effects;

public class StaticEffect : ILightingEffect
{
    public string Id => "Static";
    public string NameKey => "Effect.Static.Name";
    public string DescriptionKey => "Effect.Static.Desc";

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        for (int i = 0; i < context.TotalLeds; i++)
        {
            rgbData[i * 3] = context.PrimaryColor.R;
            rgbData[i * 3 + 1] = context.PrimaryColor.G;
            rgbData[i * 3 + 2] = context.PrimaryColor.B;
        }
    }
}

public class BreathingEffect : ILightingEffect
{
    public string Id => "Breathing";
    public string NameKey => "Effect.Breathing.Name";
    public string DescriptionKey => "Effect.Breathing.Desc";

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        double hz = 0.1 + (context.Speed * 2.9);
        double phase = (context.ElapsedSeconds * hz * context.Direction) % 1.0;
        if (phase < 0) phase += 1.0;
        
        double minLuma = 1.0 - context.Intensity;
        double wave = minLuma + (1.0 - minLuma) * (0.5 * (1 + Math.Sin(phase * 2 * Math.PI)));

        for (int i = 0; i < context.TotalLeds; i++)
        {
            rgbData[i * 3] = (byte)(context.PrimaryColor.R * wave);
            rgbData[i * 3 + 1] = (byte)(context.PrimaryColor.G * wave);
            rgbData[i * 3 + 2] = (byte)(context.PrimaryColor.B * wave);
        }
    }
}

public class RainbowEffect : ILightingEffect
{
    public string Id => "Rainbow";
    public string NameKey => "Effect.Rainbow.Name";
    public string DescriptionKey => "Effect.Rainbow.Desc";

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        double hz = 0.1 + (context.Speed * 2.0);
        double baseHue = (context.ElapsedSeconds * hz * context.Direction * 360.0) % 360.0;
        if (baseHue < 0) baseHue += 360.0;

        for (int i = 0; i < context.TotalLeds; i++)
        {
            double hueOffset = (double)i / context.TotalLeds * 360.0;
            float h = (float)((baseHue + hueOffset) % 360.0);
            
            var c = ColorUtils.HsvToRgb(h, (float)context.Intensity, 1.0f);
            rgbData[i * 3] = c.R;
            rgbData[i * 3 + 1] = c.G;
            rgbData[i * 3 + 2] = c.B;
        }
    }
}

public class GradientFlowEffect : ILightingEffect
{
    public string Id => "GradientFlow";
    public string NameKey => "Effect.GradientFlow.Name";
    public string DescriptionKey => "Effect.GradientFlow.Desc";

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        double hz = 0.1 + (context.Speed * 1.5);
        double offset = (context.ElapsedSeconds * hz * context.Direction) % 1.0;
        if (offset < 0) offset += 1.0;

        for (int i = 0; i < context.TotalLeds; i++)
        {
            double pos = ((double)i / context.TotalLeds + offset) % 1.0;
            
            float factor = (float)(pos <= 0.5 ? pos * 2 : (1.0 - pos) * 2);
            
            var c = ColorUtils.Lerp(context.PrimaryColor, context.SecondaryColor, factor);
            
            rgbData[i * 3] = (byte)(c.R * context.Intensity);
            rgbData[i * 3 + 1] = (byte)(c.G * context.Intensity);
            rgbData[i * 3 + 2] = (byte)(c.B * context.Intensity);
        }
    }
}

public class PulseEffect : ILightingEffect
{
    public string Id => "Pulse";
    public string NameKey => "Effect.Pulse.Name";
    public string DescriptionKey => "Effect.Pulse.Desc";

    public void Render(Span<byte> rgbData, LightingEffectContext context)
    {
        double hz = 0.5 + (context.Speed * 3.5);
        double phase = (context.ElapsedSeconds * hz * context.Direction) % 1.0;
        if (phase < 0) phase += 1.0;
        
        double wave = Math.Pow(0.5 * (1 + Math.Sin(phase * 2 * Math.PI)), 4);
        double minLuma = 1.0 - context.Intensity;
        wave = minLuma + (1.0 - minLuma) * wave;

        for (int i = 0; i < context.TotalLeds; i++)
        {
            rgbData[i * 3] = (byte)(context.PrimaryColor.R * wave);
            rgbData[i * 3 + 1] = (byte)(context.PrimaryColor.G * wave);
            rgbData[i * 3 + 2] = (byte)(context.PrimaryColor.B * wave);
        }
    }
}
