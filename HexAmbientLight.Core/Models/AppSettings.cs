using System;
using System.Text.Json.Serialization;

namespace HexAmbientLight.Core.Models;

public class AppSettings
{
    public int ConfigSchemaVersion { get; set; } = 1;
    
    // General
    public bool StartWithWindows { get; set; } = true;
    public bool CloseToTray { get; set; } = true;
    public int TargetFps { get; set; } = 30; // 15, 30, 45, 60
    
    // Network (WLED)
    public string WledIpAddress { get; set; } = "";
    public int DdpUdpPort { get; set; } = 4048;
    
    // LED Configuration
    public int BrightnessLimit { get; set; } = 255;
    public double Saturation { get; set; } = 1.0;
    public double SmoothingFactor { get; set; } = 0.5; // 0.0 - 1.0
    
    // Game Profile (CS2)
    public bool Cs2Enabled { get; set; } = true;
    public int BombTimerSeconds { get; set; } = 40;
    public bool AutoSwitchToGameProfile { get; set; } = true;
    public int GsiPort { get; set; } = 3000;
    public string GsiAuthToken { get; set; } = "";
    public int GsiTimeoutMs { get; set; } = 5000;

    [JsonIgnore]
    public bool IsWledConfigured => !string.IsNullOrWhiteSpace(WledIpAddress);
}
