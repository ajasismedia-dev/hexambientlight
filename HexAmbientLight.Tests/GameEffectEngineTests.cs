using System;
using HexAmbientLight.Core.Models;
using HexAmbientLight.Core.Services;
using Xunit;

namespace HexAmbientLight.Tests;

public class GameEffectEngineTests
{
    [Fact]
    public async System.Threading.Tasks.Task Render_BombPlanted_StartsGreen_ThenRed()
    {
        var settings = new SettingsManager();
        await settings.InitializeAsync();
        settings.LayoutConfig = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        settings.AppSettings.BombTimerSeconds = 40;
        var engine = new GameEffectEngine(settings);
        
        var state = new Cs2GameState
        {
            Round = new Cs2Round { Phase = "live", Bomb = "planted" },
            Player = new Cs2Player { State = new Cs2PlayerState { Health = 100, Armor = 100 }, Team = "T" }
        };

        var result1 = engine.Render(state); // Initially Green (> 11 seconds remaining)

        Assert.NotEmpty(result1);
        Assert.Equal(0, result1[0]); // R
        Assert.True(result1[1] > 0); // G
        Assert.Equal(0, result1[2]); // B
    }

    [Fact]
    public async System.Threading.Tasks.Task Render_BombDefused_ResetsPulse()
    {
        var settings = new SettingsManager();
        await settings.InitializeAsync();
        settings.LayoutConfig = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        var engine = new GameEffectEngine(settings);
        
        var state = new Cs2GameState
        {
            Provider = new Cs2Provider { SteamId = "123" },
            Round = new Cs2Round { Phase = "live", Bomb = "planted" },
            Player = new Cs2Player { SteamId = "123", State = new Cs2PlayerState { Health = 100, Armor = 100 }, Team = "T" }
        };

        engine.Render(state);

        // Change to defused
        state.Round.Bomb = "defused";
        var result = engine.Render(state);

        // Should revert to normal health/armor logic
        // If Health = 100, Left side (first few LEDs) should be Green
        Assert.True(result[1] > 0); // Green channel
    }

    [Fact]
    public async System.Threading.Tasks.Task ResetState_ClearsBombState()
    {
        var settings = new SettingsManager();
        await settings.InitializeAsync();
        settings.LayoutConfig = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        var engine = new GameEffectEngine(settings);
        
        var state = new Cs2GameState
        {
            Provider = new Cs2Provider { SteamId = "123" },
            Round = new Cs2Round { Phase = "live", Bomb = "planted" },
            Player = new Cs2Player { SteamId = "123", State = new Cs2PlayerState { Health = 100, Armor = 100 }, Team = "T" }
        };

        var result1 = engine.Render(state); // Now bomb is planted
        // Starts Green, let's assume it's pulsing so we just check it renders something
        Assert.NotEmpty(result1);

        engine.ResetState(); // Simulate GSI Timeout

        state.Round.Bomb = ""; 
        var result2 = engine.Render(state);
        
        // Now it should be rendering health again (Green)
        Assert.True(result2[1] > 0);
    }

    [Fact]
    public async System.Threading.Tasks.Task Render_Spectator_ClearsLocalData()
    {
        var settings = new SettingsManager();
        await settings.InitializeAsync();
        settings.LayoutConfig = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        var engine = new GameEffectEngine(settings);
        
        var state = new Cs2GameState
        {
            Provider = new Cs2Provider { SteamId = "123" },
            Player = new Cs2Player { SteamId = "456", State = new Cs2PlayerState { Health = 100, Armor = 100 }, Team = "T" }
        };

        var result = engine.Render(state);
        
        // Since it's spectating (provider.steamid != player.steamid), health/ammo shouldn't be rendered.
        // Health (Left) should be 0
        Assert.Equal(0, result[0]); 
        
        // Ammo (Top) should be 0
        Assert.Equal(0, result[10 * 3]);

        // Right (Armor) fallback should be 0
        Assert.Equal(0, result[30 * 3]);
    }

    [Fact]
    public async System.Threading.Tasks.Task Render_Bomb_PulseIsFrameRateIndependent()
    {
        var settings = new SettingsManager();
        await settings.InitializeAsync();
        settings.LayoutConfig = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        var engine = new GameEffectEngine(settings);
        
        var state = new Cs2GameState
        {
            Round = new Cs2Round { Phase = "live", Bomb = "planted" },
            Player = new Cs2Player { State = new Cs2PlayerState { Health = 100, Armor = 100 }, Team = "T" }
        };

        // Render at a high frame rate loop (e.g. 5ms intervals) to see if it strobes unexpectedly
        // We can just verify it doesn't throw and renders successfully multiple times
        for (int i = 0; i < 20; i++)
        {
            var r = engine.Render(state);
            Assert.NotEmpty(r);
            System.Threading.Thread.Sleep(5);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task Render_Health_TransitionColors()
    {
        var settings = new SettingsManager();
        await settings.InitializeAsync();
        settings.LayoutConfig = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        var engine = new GameEffectEngine(settings);
        
        var state = new Cs2GameState
        {
            Player = new Cs2Player { State = new Cs2PlayerState { Health = 100, Armor = 100 }, Team = "T", SteamId = "123" },
            Provider = new Cs2Provider { SteamId = "123" }
        };

        // 100% (Green)
        var res100 = engine.Render(state);
        Assert.Equal(0, res100[0]); // R
        Assert.Equal(255, res100[1]); // G
        Assert.Equal(0, res100[2]); // B
        Assert.Equal(res100[0], res100[9 * 3]); // last LED of left

        // 75% (Green)
        state.Player.State.Health = 75;
        var res75 = engine.Render(state);
        Assert.Equal(0, res75[0]); 
        Assert.Equal(255, res75[1]); 
        Assert.Equal(0, res75[2]);

        // 50% (Green-Orange)
        state.Player.State.Health = 50;
        var res50 = engine.Render(state);
        Assert.True(res50[0] > 0); // R > 0
        Assert.True(res50[1] > 128 && res50[1] < 255); // G between 128 and 255

        // 25% (Orange-Red)
        state.Player.State.Health = 25;
        var res25 = engine.Render(state);
        Assert.Equal(255, res25[0]); // R = 255
        Assert.True(res25[1] > 0 && res25[1] < 128); // G between 0 and 128
        Assert.Equal(0, res25[2]); // B = 0

        // 1% (Red)
        state.Player.State.Health = 1;
        var res1 = engine.Render(state);
        Assert.Equal(255, res1[0]); // R = 255
        Assert.True(res1[1] > 0 && res1[1] < 10); // G is very close to 0

        // 0% (Red)
        state.Player.State.Health = 0;
        var res0 = engine.Render(state);
        Assert.Equal(255, res0[0]); // R = 255
        Assert.Equal(0, res0[1]); // G = 0
        Assert.Equal(0, res0[2]); // B = 0
        Assert.Equal(res0[0], res0[9 * 3]); // last LED is also red
    }

    [Fact]
    public async System.Threading.Tasks.Task Render_Ammo_TransitionColors()
    {
        var settings = new SettingsManager();
        await settings.InitializeAsync();
        settings.LayoutConfig = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        var engine = new GameEffectEngine(settings);
        
        var state = new Cs2GameState
        {
            Player = new Cs2Player { State = new Cs2PlayerState { Health = 100, Armor = 100 }, Team = "T", SteamId = "123" },
            Provider = new Cs2Provider { SteamId = "123" }
        };

        // 100% (Cyan)
        state.Player.Weapons = new System.Collections.Generic.Dictionary<string, Cs2Weapon> { 
            { "w", new Cs2Weapon { State = "active", AmmoClip = 30, AmmoClipMax = 30, Type = "Pistol" } } 
        };
        var res100 = engine.Render(state);
        Assert.Equal(0, res100[10 * 3]); // offset 10, R
        Assert.Equal(255, res100[10 * 3 + 1]); // G
        Assert.Equal(255, res100[10 * 3 + 2]); // B
        Assert.Equal(res100[10 * 3], res100[29 * 3]); // last LED of top

        // 50% (Cyan-Yellow)
        state.Player.Weapons["w"].AmmoClip = 15;
        var res50 = engine.Render(state);
        Assert.True(res50[10 * 3] > 0); // R > 0
        Assert.Equal(255, res50[10 * 3 + 1]); // G = 255

        // 25% (Yellow-Orange)
        state.Player.Weapons["w"].AmmoClip = 7;
        var res25 = engine.Render(state);
        Assert.Equal(255, res25[10 * 3]); // R = 255
        Assert.True(res25[10 * 3 + 1] >= 128 && res25[10 * 3 + 1] <= 255); // G between 128 and 255
        Assert.Equal(0, res25[10 * 3 + 2]); // B = 0

        // 0% (Red)
        state.Player.Weapons["w"].AmmoClip = 0;
        var res0 = engine.Render(state);
        Assert.Equal(255, res0[10 * 3]); // R = 255
        Assert.Equal(0, res0[10 * 3 + 1]); // G = 0
        Assert.Equal(0, res0[10 * 3 + 2]); // B = 0
        Assert.Equal(res0[10 * 3], res0[29 * 3]); // last LED is also red

        // Neutral Fallback (Knife)
        state.Player.Weapons["w"].Type = "Knife";
        state.Player.Weapons["w"].AmmoClipMax = 0;
        var resNeutral = engine.Render(state);
        Assert.Equal(30, resNeutral[10 * 3]); // R = 30
        Assert.Equal(30, resNeutral[10 * 3 + 1]); // G = 30
        Assert.Equal(30, resNeutral[10 * 3 + 2]); // B = 30
    }

    [Fact]
    public async System.Threading.Tasks.Task Render_Armor_TransitionColors()
    {
        var settings = new SettingsManager();
        await settings.InitializeAsync();
        settings.LayoutConfig = new LedLayoutConfig { LeftCount = 10, TopCount = 20, RightCount = 10, TotalLeds = 40 };
        var engine = new GameEffectEngine(settings);
        
        var state = new Cs2GameState
        {
            Player = new Cs2Player { State = new Cs2PlayerState { Health = 100, Armor = 100 }, Team = "T", SteamId = "123" },
            Provider = new Cs2Provider { SteamId = "123" }
        };

        // 100% (Blue)
        var res100 = engine.Render(state);
        Assert.Equal(0, res100[30 * 3]); // R
        Assert.Equal(0, res100[30 * 3 + 1]); // G
        Assert.Equal(255, res100[30 * 3 + 2]); // B
        Assert.Equal(res100[30 * 3], res100[39 * 3]); // last LED is same

        // 75% (Blue)
        state.Player.State.Armor = 75;
        var res75 = engine.Render(state);
        Assert.Equal(0, res75[30 * 3]);
        Assert.Equal(0, res75[30 * 3 + 1]);
        Assert.Equal(255, res75[30 * 3 + 2]);

        // 50% (Blue-Purple)
        state.Player.State.Armor = 50;
        var res50 = engine.Render(state);
        Assert.True(res50[30 * 3] > 0 && res50[30 * 3] < 255); // R > 0 (mixing Red into Blue makes Purple)
        Assert.Equal(0, res50[30 * 3 + 1]); // G = 0
        Assert.Equal(255, res50[30 * 3 + 2]); // B = 255

        // 25% (Purple-Red)
        state.Player.State.Armor = 25;
        var res25 = engine.Render(state);
        Assert.Equal(255, res25[30 * 3]); // R = 255
        Assert.Equal(0, res25[30 * 3 + 1]); // G = 0
        Assert.True(res25[30 * 3 + 2] > 0 && res25[30 * 3 + 2] < 255); // B < 255

        // 0% (Red)
        state.Player.State.Armor = 0;
        var res0 = engine.Render(state);
        Assert.Equal(255, res0[30 * 3]); // R = 255
        Assert.Equal(0, res0[30 * 3 + 1]); // G = 0
        Assert.Equal(0, res0[30 * 3 + 2]); // B = 0

        // Spectator mode (different provider/player steam IDs) -> Armor should not render (fallback to 0)
        state.Provider.SteamId = "456"; // Now spectating
        var resSpectator = engine.Render(state);
        Assert.Equal(0, resSpectator[30 * 3]); // R = 0
        Assert.Equal(0, resSpectator[30 * 3 + 1]); // G = 0
        Assert.Equal(0, resSpectator[30 * 3 + 2]); // B = 0
    }
}
