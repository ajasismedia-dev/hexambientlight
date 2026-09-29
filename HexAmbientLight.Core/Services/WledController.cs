using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using HexAmbientLight.Core.Models;
using Serilog;

namespace HexAmbientLight.Core.Services;

public class WledController : IDisposable
{
    private readonly SettingsManager _settingsManager;
    private readonly HttpClient _httpClient;
    private readonly UdpClient _udpClient;
    
    private WledStateResponse? _savedState;
    private bool _isRealtimeActive;
    private int _frameCount;
    private DateTime _lastFpsTime = DateTime.UtcNow;
    private bool _firstFrameSent;
    public bool IsConnected { get; private set; }
    public string? CurrentIp => _settingsManager.AppSettings.WledIpAddress;
    public int CurrentSendFps { get; set; }

    public WledController(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        _httpClient = new HttpClient();
        // Short timeout for health checks
        _httpClient.Timeout = TimeSpan.FromSeconds(3);
        _udpClient = new UdpClient();
    }

    public async Task<bool> IsAliveAsync()
    {
        var ip = _settingsManager.AppSettings.WledIpAddress;
        if (string.IsNullOrWhiteSpace(ip)) { IsConnected = false; return false; } try { var response = await _httpClient.GetAsync($"http://{ip}/json/state"); IsConnected = response.IsSuccessStatusCode; return IsConnected; } catch { IsConnected = false; return false; }
    }

    public async Task ConnectAndSaveStateAsync()
    {
        var ip = _settingsManager.AppSettings.WledIpAddress;
        if (string.IsNullOrWhiteSpace(ip)) { IsConnected = false; return; } try { _savedState = await _httpClient.GetFromJsonAsync<WledStateResponse>($"http://{ip}/json/state"); IsConnected = true; Log.Information("Saved current WLED state."); } catch (Exception ex) { IsConnected = false; Log.Warning(ex, "Failed to save WLED state. Device might be offline."); }
    }

    public async Task SendDdpFrameAsync(ReadOnlyMemory<byte> rgbData)
    {
        var ip = _settingsManager.AppSettings.WledIpAddress;
        var port = _settingsManager.AppSettings.DdpUdpPort;
        if (string.IsNullOrWhiteSpace(ip) || rgbData.Length == 0) return;

        _isRealtimeActive = true;
        var packets = DdpPacketizer.Packetize(rgbData.Span);

        _frameCount++;
        var now = DateTime.UtcNow;
        if ((now - _lastFpsTime).TotalSeconds >= 1.0)
        {
            CurrentSendFps = _frameCount;
            _frameCount = 0;
            _lastFpsTime = now;
        }

        if (!_firstFrameSent)
        {
            _firstFrameSent = true;
            Log.Information("First LED frame generated. DDP destination: {Ip}, LED count: {Count}", ip, rgbData.Length / 3);
        }
        
        try
        {
            foreach (var packet in packets)
            {
                await _udpClient.SendAsync(packet, packet.Length, ip, port);
            }
        }
        catch (Exception ex)
        {
            // Do not spam log per frame, maybe add throttling later
            Log.Debug(ex, "Failed to send DDP packet.");
        }
    }

    public async Task ExitRealtimeModeAsync(WledOnExitBehavior behavior)
    {
        if (!_isRealtimeActive) return;
        _isRealtimeActive = false;

        var ip = _settingsManager.AppSettings.WledIpAddress;
        if (string.IsNullOrWhiteSpace(ip)) return;

        var request = new WledStateRequest { Live = false }; // Exit realtime override

        switch (behavior)
        {
            case WledOnExitBehavior.TurnOff:
                request.On = false;
                break;
            case WledOnExitBehavior.RestorePreset:
                if (_savedState != null)
                {
                    request.On = _savedState.On;
                    request.Brightness = _savedState.Brightness;
                    request.Preset = _savedState.Preset > 0 ? _savedState.Preset : null;
                }
                break;
            case WledOnExitBehavior.LeaveAsIs:
                // Just let it return to whatever it was before live mode implicitly
                break;
        }

        try
        {
            var content = JsonContent.Create(request);
            await _httpClient.PostAsync($"http://{ip}/json/state", content);
            Log.Information("Exited realtime mode on WLED.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to send exit command to WLED.");
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _udpClient.Dispose();
    }
}


