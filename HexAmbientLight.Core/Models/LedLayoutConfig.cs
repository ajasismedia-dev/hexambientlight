using System;
using System.Text.Json.Serialization;

namespace HexAmbientLight.Core.Models;

public enum LayoutStartRegion
{
    LeftBottom,
    LeftTop,
    TopLeft,
    TopRight,
    RightTop,
    RightBottom
}

public enum LayoutDirection
{
    Clockwise,
    CounterClockwise
}

public class LedLayoutConfig
{
    public int TotalLeds { get; set; } = 54;
    
    // 3-Sided Configuration
    public int LeftCount { get; set; } = 14;
    public int TopCount { get; set; } = 26;
    public int RightCount { get; set; } = 14;
    
    // Physical Setup
    public LayoutStartRegion StartRegion { get; set; } = LayoutStartRegion.LeftBottom;
    public LayoutDirection Direction { get; set; } = LayoutDirection.Clockwise;
    public int StartOffset { get; set; } = 0;
    
    // Gaps (Dead zones in physical corners)
    public int TopLeftGap { get; set; } = 0;
    public int TopRightGap { get; set; } = 0;
    
    // Inversions per region (if physical strip goes backwards relative to screen coordinates)
    public bool InvertLeft { get; set; } = false;
    public bool InvertTop { get; set; } = false;
    public bool InvertRight { get; set; } = false;

    [JsonIgnore]
    public bool IsValid => (LeftCount + TopCount + RightCount) == TotalLeds && TotalLeds > 0;
}
