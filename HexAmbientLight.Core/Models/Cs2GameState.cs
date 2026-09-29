using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HexAmbientLight.Core.Models;

public class Cs2GameState
{
    [JsonPropertyName("provider")]
    public Cs2Provider? Provider { get; set; }
    
    [JsonPropertyName("player")]
    public Cs2Player? Player { get; set; }
    
    [JsonPropertyName("round")]
    public Cs2Round? Round { get; set; }

    [JsonPropertyName("map")]
    public Cs2Map? Map { get; set; }

    [JsonPropertyName("phase_countdowns")]
    public Cs2PhaseCountdowns? PhaseCountdowns { get; set; }
    
    [JsonPropertyName("auth")]
    public Cs2Auth? Auth { get; set; }

    [JsonIgnore]
    public bool IsLocalPlayer => Provider?.SteamId != null && Player?.SteamId != null && Provider.SteamId == Player.SteamId;

    [JsonIgnore]
    public bool IsSpectating => !IsLocalPlayer && Player?.SteamId != null;

    [JsonIgnore]
    public bool IsLocalPlayerDataValid => IsLocalPlayer && Player?.State != null;
}

public class Cs2Provider {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("steamid")] public string? SteamId { get; set; }
}

public class Cs2Player {
    [JsonPropertyName("steamid")] public string? SteamId { get; set; }
    [JsonPropertyName("state")] public Cs2PlayerState? State { get; set; }
    [JsonPropertyName("team")] public string? Team { get; set; }
    [JsonPropertyName("weapons")] public Dictionary<string, Cs2Weapon>? Weapons { get; set; }
}

public class Cs2PlayerState {
    [JsonPropertyName("health")] public int Health { get; set; }
    [JsonPropertyName("armor")] public int Armor { get; set; }
}

public class Cs2Weapon {
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("ammo_clip")] public int AmmoClip { get; set; }
    [JsonPropertyName("ammo_clip_max")] public int AmmoClipMax { get; set; }
    [JsonPropertyName("state")] public string? State { get; set; } // active, holstered
}

public class Cs2PhaseCountdowns {
    [JsonPropertyName("phase")] public string? Phase { get; set; } // "live", "freezetime", "bomb", "defuse", "warmup", "over", "timeout"
    [JsonPropertyName("phase_ends_in")] public string? PhaseEndsIn { get; set; } // Sent as string e.g. "115.2" or "10.0"
}

public class Cs2Map {
    [JsonPropertyName("mode")] public string? Mode { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("phase")] public string? Phase { get; set; }
}

public class Cs2Round {
    [JsonPropertyName("phase")] public string? Phase { get; set; }
    [JsonPropertyName("bomb")] public string? Bomb { get; set; } // planted, defused, exploded
}

public class Cs2Auth {
    [JsonPropertyName("token")] public string? Token { get; set; }
}
