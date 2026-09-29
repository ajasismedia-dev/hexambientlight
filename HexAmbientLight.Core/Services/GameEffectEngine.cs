using System;
using System.Diagnostics;
using HexAmbientLight.Core.Models;

namespace HexAmbientLight.Core.Services;

public class GameEffectEngine
{
    private readonly SettingsManager _settingsManager;
    private long _bombPlantedTimestamp = 0;
    private bool _isBombPlanted = false;

    // Pulse scheduling
    private double _lastPulseTime = 0;
    private double _nextPulseTime = 0;
    
    // Constants for calibration
    private const double EstimatedBombDuration = 40.0;
    private const double VisualMinimumPulseInterval = 0.15; // Match C4 max frequency
    private const double PulseEnvelopeDuration = 0.150; // 150 ms envelope for a softer pulse

    public GameEffectEngine(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    public void ResetState()
    {
        _isBombPlanted = false;
        _bombPlantedTimestamp = 0;
        _lastPulseTime = 0;
        _nextPulseTime = 0;
    }

    public byte[] Render(Cs2GameState state)
    {
        var layout = _settingsManager.LayoutConfig;
        int totalLeds = layout.TotalLeds;
        if (totalLeds == 0) return Array.Empty<byte>();

        byte[] rgbOutput = new byte[totalLeds * 3];

        UpdateBombState(state);

        if (_isBombPlanted)
        {
            RenderBombEffect(rgbOutput, layout);
            return rgbOutput;
        }

        if (state.IsLocalPlayerDataValid && state.Player?.State != null)
        {
            RenderHealth(rgbOutput, layout, state.Player.State.Health);
            RenderArmor(rgbOutput, layout, state.Player.State.Armor);
            
            bool ammoRendered = false;
            if (state.Player.Weapons != null)
            {
                foreach (var w in state.Player.Weapons.Values)
                {
                    if (w.State == "active")
                    {
                        RenderAmmo(rgbOutput, layout, w.AmmoClip, w.AmmoClipMax, w.Type);
                        ammoRendered = true;
                        break;
                    }
                }
            }
            
            // If no active weapon found (or Weapons was null), fallback neutral state
            if (!ammoRendered)
            {
                RenderAmmo(rgbOutput, layout, 0, 0, "None");
            }
        }

        return rgbOutput;
    }

    private void UpdateBombState(Cs2GameState state)
    {
        var bombStatus = state.Round?.Bomb;

        if (bombStatus == "planted" && !_isBombPlanted)
        {
            _isBombPlanted = true;
            _bombPlantedTimestamp = Stopwatch.GetTimestamp();
            _lastPulseTime = 0;
            _nextPulseTime = 0; // immediate first pulse
        }
        else if (bombStatus == "defused" || bombStatus == "exploded" || state.Round?.Phase == "over" || state.Round?.Phase == "freezetime")
        {
            ResetState();
        }
    }

    private void RenderBombEffect(byte[] rgbOutput, LedLayoutConfig layout)
    {
        double elapsed = Stopwatch.GetElapsedTime(_bombPlantedTimestamp).TotalSeconds;
        double remaining = Math.Max(0, EstimatedBombDuration - elapsed);

        // 1. Color Logic (Kitless defuse threshold)
        byte targetR = 0, targetG = 0, targetB = 0;
        if (remaining > 12.0)
        {
            targetG = 255; // Safe
        }
        else if (remaining > 10.0)
        {
            // Transition from Green (12s) to Red (10s)
            double factor = (12.0 - remaining) / 2.0; 
            targetR = (byte)(255 * factor);
            targetG = (byte)(255 * (1.0 - factor));
        }
        else
        {
            targetR = 255; // Critical
        }

        // 2. Scheduler Logic
        if (elapsed >= _nextPulseTime)
        {
            _lastPulseTime = _nextPulseTime;
            
            // Cadence model based on C4 interval
            double intervalSeconds = 0.15 + 0.85 * (remaining / EstimatedBombDuration);
            intervalSeconds = Math.Clamp(intervalSeconds, 0.15, 1.0);
            
            // Apply visual limits to prevent dangerous strobe
            intervalSeconds = Math.Max(intervalSeconds, VisualMinimumPulseInterval);
            
            // If elapsed jumped way past, catch up
            if (elapsed - _nextPulseTime > intervalSeconds) 
            {
                 _lastPulseTime = elapsed;
            }
            
            _nextPulseTime = _lastPulseTime + intervalSeconds;
        }

        // 3. Envelope Logic
        double intensity = 0.05; // 5% base ambient intensity
        double timeSincePulse = elapsed - _lastPulseTime;

        if (timeSincePulse >= 0 && timeSincePulse <= PulseEnvelopeDuration)
        {
            // Sine ease-in-out envelope
            double peak = Math.Sin((timeSincePulse / PulseEnvelopeDuration) * Math.PI);
            intensity = 0.05 + (0.45 * peak); // Peak at 50% instead of 100%
        }

        byte r = (byte)(targetR * intensity);
        byte g = (byte)(targetG * intensity);
        byte b = (byte)(targetB * intensity);

        FillRegion(rgbOutput, 0, layout.TotalLeds, r, g, b);
    }

    private void RenderHealth(byte[] rgbOutput, LedLayoutConfig layout, int health)
    {
        if (layout.LeftCount == 0) return;
        
        byte r = 0, g = 0, b = 0;
        double pct = Math.Clamp(health / 100.0, 0, 1);
        
        if (pct >= 0.60)
        {
            // 60-100: Green (0, 255, 0)
            r = 0; g = 255; b = 0;
        }
        else if (pct >= 0.30)
        {
            // 30-60: Green (0,255,0) -> Orange (255,128,0)
            double factor = (pct - 0.30) / (0.60 - 0.30); // 0 at 30%, 1 at 60%
            r = (byte)(255 * (1.0 - factor)); // 255 at 30% (Orange), 0 at 60% (Green)
            g = (byte)(128 + 127 * factor); // 128 at 30% (Orange), 255 at 60% (Green)
            b = 0;
        }
        else
        {
            // 0-30: Orange (255,128,0) -> Red (255,0,0)
            double factor = pct / 0.30; // 0 at 0%, 1 at 30%
            r = 255;
            g = (byte)(128 * factor); // 0 at 0% (Red), 128 at 30% (Orange)
            b = 0;
        }

        FillRegion(rgbOutput, 0, layout.LeftCount, r, g, b);
    }

    private void RenderAmmo(byte[] rgbOutput, LedLayoutConfig layout, int ammo, int maxAmmo, string? weaponType)
    {
        if (layout.TopCount == 0) return;
        
        int offset = layout.LeftCount;
        byte r = 0, g = 0, b = 0;

        // Neutral fallback for Knife, C4, Grenades, or invalid max ammo
        if (maxAmmo <= 0 || weaponType == "Knife" || weaponType == "C4" || weaponType == "Grenade")
        {
            r = 30;
            g = 30;
            b = 30; // Dim gray fallback
            FillRegion(rgbOutput, offset, layout.TopCount, r, g, b);
            return;
        }

        double pct = Math.Clamp((double)ammo / maxAmmo, 0, 1);

        if (pct >= 0.60)
        {
            // 60-100: Cyan (0, 255, 255)
            r = 0; g = 255; b = 255;
        }
        else if (pct >= 0.35)
        {
            // 35-60: Cyan (0, 255, 255) -> Yellow (255, 255, 0)
            double factor = (pct - 0.35) / (0.60 - 0.35); // 0 at 35%, 1 at 60%
            r = (byte)(255 * (1.0 - factor)); // Yellow R (255) to Cyan R (0) -> Actually, at 1.0 (60%) R should be 0. So R = 255 * (1 - factor)
            g = 255;
            b = (byte)(255 * factor); // Yellow B (0) to Cyan B (255) -> B = 255 * factor
        }
        else if (pct >= 0.15)
        {
            // 15-35: Yellow (255, 255, 0) -> Orange (255, 128, 0)
            double factor = (pct - 0.15) / (0.35 - 0.15); // 0 at 15%, 1 at 35%
            r = 255;
            // Orange G (128) to Yellow G (255)
            g = (byte)(128 + 127 * factor);
            b = 0;
        }
        else
        {
            // 0-15: Orange (255, 128, 0) -> Red (255, 0, 0)
            double factor = pct / 0.15; // 0 at 0%, 1 at 15%
            r = 255;
            // Red G (0) to Orange G (128)
            g = (byte)(128 * factor);
            b = 0;
        }

        // Entire top zone is lit with the same color
        FillRegion(rgbOutput, offset, layout.TopCount, r, g, b);
    }

    private void RenderArmor(byte[] rgbOutput, LedLayoutConfig layout, int armor)
    {
        if (layout.RightCount == 0) return;
        
        int offset = layout.LeftCount + layout.TopCount;
        byte r = 0, g = 0, b = 0;
        double pct = Math.Clamp(armor / 100.0, 0, 1);

        if (pct >= 0.60)
        {
            // 60-100: Blue (0, 0, 255)
            r = 0; g = 0; b = 255;
        }
        else if (pct >= 0.30)
        {
            // 30-60: Blue (0,0,255) -> Purple/Magenta (255,0,255)
            double factor = (pct - 0.30) / (0.60 - 0.30); // 0 at 30%, 1 at 60%
            r = (byte)(255 * (1.0 - factor)); // 255 at 30% (Magenta), 0 at 60% (Blue)
            g = 0;
            b = 255;
        }
        else
        {
            // 0-30: Purple/Magenta (255,0,255) -> Red (255,0,0)
            double factor = pct / 0.30; // 0 at 0%, 1 at 30%
            r = 255;
            g = 0;
            b = (byte)(255 * factor); // 0 at 0% (Red), 255 at 30% (Magenta)
        }

        FillRegion(rgbOutput, offset, layout.RightCount, r, g, b);
    }

    private DateTime _lastTimerLog = DateTime.MinValue;

    private void FillRegion(byte[] rgbOutput, int offset, int count, byte r, byte g, byte b)
    {
        for (int i = 0; i < count; i++)
        {
            int idx = (offset + i) * 3;
            if (idx + 2 < rgbOutput.Length)
            {
                rgbOutput[idx] = r;
                rgbOutput[idx + 1] = g;
                rgbOutput[idx + 2] = b;
            }
        }
    }
}
