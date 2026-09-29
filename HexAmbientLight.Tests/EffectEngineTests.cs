using HexAmbientLight.Core.Effects;
using Xunit;
using System;

namespace HexAmbientLight.Tests;

public class EffectEngineTests
{
    [Fact]
    public void StaticEffect_FillsCorrectColor()
    {
        var effect = new StaticEffect();
        var context = new LightingEffectContext
        {
            TotalLeds = 2,
            PrimaryColor = new RgbColor(255, 0, 0)
        };
        var buffer = new byte[6];
        
        effect.Render(buffer, context);
        
        Assert.Equal(255, buffer[0]);
        Assert.Equal(0, buffer[1]);
        Assert.Equal(0, buffer[2]);
        Assert.Equal(255, buffer[3]);
        Assert.Equal(0, buffer[4]);
        Assert.Equal(0, buffer[5]);
    }

    [Fact]
    public void BreathingEffect_OscillatesWithTime()
    {
        var effect = new BreathingEffect();
        var context = new LightingEffectContext
        {
            TotalLeds = 1,
            PrimaryColor = new RgbColor(100, 100, 100),
            Intensity = 1.0,
            Speed = 0.5,
            ElapsedSeconds = 0
        };
        var buffer = new byte[3];
        
        // At t=0, phase is 0, wave is 0.5 * (1 + sin(0)) = 0.5
        effect.Render(buffer, context);
        Assert.Equal(50, buffer[0]); // 100 * 0.5 = 50

        // At some positive t, phase increases, wave changes.
        // We just verify it doesn't crash and modifies the buffer
        context.ElapsedSeconds = 0.5;
        effect.Render(buffer, context);
        Assert.NotEqual(50, buffer[0]); // Should have changed
    }

    [Fact]
    public void RainbowEffect_GeneratesColors()
    {
        var effect = new RainbowEffect();
        var context = new LightingEffectContext
        {
            TotalLeds = 4,
            Intensity = 1.0,
            Speed = 0.5,
            ElapsedSeconds = 0
        };
        var buffer = new byte[12];
        
        effect.Render(buffer, context);
        
        // Just verify it produced non-black colors across the strip
        Assert.True(buffer[0] > 0 || buffer[1] > 0 || buffer[2] > 0);
        Assert.True(buffer[9] > 0 || buffer[10] > 0 || buffer[11] > 0);
    }
}
