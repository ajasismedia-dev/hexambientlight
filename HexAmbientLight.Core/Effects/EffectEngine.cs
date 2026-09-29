using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HexAmbientLight.Core.Models;

namespace HexAmbientLight.Core.Effects;

public class EffectEngine
{
    private readonly Dictionary<string, ILightingEffect> _effects = new();
    
    public EffectEngine()
    {
        Register(new StaticEffect());
        Register(new BreathingEffect());
        Register(new PulseEffect());
        Register(new RainbowEffect());
        Register(new ColorWaveEffect());
        Register(new GradientFlowEffect());
        Register(new AuroraEffect());
        Register(new CyberEffect());
        Register(new FireEffect());
    }

    private void Register(ILightingEffect effect)
    {
        _effects[effect.Id] = effect;
    }

    public void Render(AppSettings settings, int totalLeds, Span<byte> outputRgb, double elapsedSeconds, double deltaSeconds)
    {
        var context = new LightingEffectContext
        {
            TotalLeds = totalLeds,
            ElapsedSeconds = elapsedSeconds,
            DeltaSeconds = deltaSeconds,
            PrimaryColor = ColorUtils.ParseHex(settings.EffectPrimaryColorHex),
            SecondaryColor = ColorUtils.ParseHex(settings.EffectSecondaryColorHex),
            TertiaryColor = ColorUtils.ParseHex(settings.EffectTertiaryColorHex),
            Speed = settings.EffectSpeed,
            Intensity = settings.EffectIntensity,
            Direction = settings.EffectDirection == 0 ? 1 : settings.EffectDirection
        };

        string effectId = string.IsNullOrWhiteSpace(settings.EffectId) ? "Static" : settings.EffectId;
        
        if (!_effects.TryGetValue(effectId, out var effect))
        {
            effect = _effects["Static"];
        }

        effect.Render(outputRgb, context);
    }
}
