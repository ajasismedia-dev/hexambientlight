using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HexAmbientLight.Core.Models;

public class WledStateResponse
{
    [JsonPropertyName("on")]
    public bool On { get; set; }
    
    [JsonPropertyName("bri")]
    public int Brightness { get; set; }
    
    [JsonPropertyName("ps")]
    public int Preset { get; set; }
}

public class WledStateRequest
{
    [JsonPropertyName("on")]
    public bool? On { get; set; }
    
    [JsonPropertyName("bri")]
    public int? Brightness { get; set; }
    
    [JsonPropertyName("ps")]
    public int? Preset { get; set; }
    
    [JsonPropertyName("live")]
    public bool? Live { get; set; } // Set to true for realtime mode override
}
