namespace HexAmbientLight.Core.Models;

public enum SelectedMode
{
    Auto,
    Ambilight,
    Game,
    Manual,
    Off
}

public enum EffectiveMode
{
    Off,
    Ambilight,
    Game,
    Manual
}

public enum WledOnExitBehavior
{
    TurnOff,
    LeaveAsIs,
    RestorePreset
}
