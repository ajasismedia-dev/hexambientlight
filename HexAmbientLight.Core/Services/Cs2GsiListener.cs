using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HexAmbientLight.Core.Models;
using Serilog;

namespace HexAmbientLight.Core.Services;

public class Cs2GsiListener : IDisposable
{
    private readonly SettingsManager _settingsManager;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _listenerTask;
    private long _lastUpdateTimestamp = 0;

    public Cs2GameState? LatestState { get; private set; }

    public event EventHandler<Cs2GameState>? StateUpdated;

    public Cs2GsiListener(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    public void Start()
    {
        if (_listener != null && _listener.IsListening) return;

        var port = _settingsManager.AppSettings.GsiPort;
        if (port <= 0) port = 3000;

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        
        try
        {
            _listener.Start();
            _cts = new CancellationTokenSource();
            _listenerTask = Task.Run(() => ListenAsync(_cts.Token));
            Log.Information("CS2 GSI Listener started on port {Port}", port);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start CS2 GSI Listener on port {Port}. Make sure run as admin isn't required for this port.", port);
        }
    }

    private async Task ListenAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _listener != null && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync().WaitAsync(token);
                _ = ProcessRequestAsync(context);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error while accepting GSI request.");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.HttpMethod == "POST")
        {
            try
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                var json = await reader.ReadToEndAsync();
                
                // Debug log phase_countdowns specifically

                var state = JsonSerializer.Deserialize<Cs2GameState>(json);

                if (state != null)
                {
                    var configuredToken = _settingsManager.AppSettings.GsiAuthToken;
                    if (!string.IsNullOrWhiteSpace(configuredToken))
                    {
                        if (state.Auth?.Token != configuredToken)
                        {
                            Log.Warning("GSI request received with invalid auth token.");
                            response.StatusCode = 403;
                            response.Close();
                            return;
                        }
                    }

                    _lastUpdateTimestamp = Stopwatch.GetTimestamp();

                    LatestState = state;
                    StateUpdated?.Invoke(this, state);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to parse GSI payload.");
            }
        }

        response.StatusCode = 200;
        response.Close();
    }

    public bool IsDataStale()
    {
        var timeoutMs = _settingsManager.AppSettings.GsiTimeoutMs;
        if (timeoutMs <= 0) timeoutMs = 5000;
        
        if (_lastUpdateTimestamp == 0) return true;
        return Stopwatch.GetElapsedTime(_lastUpdateTimestamp).TotalMilliseconds > timeoutMs;
    }

    public double GetLastUpdateAgeMs()
    {
        if (_lastUpdateTimestamp == 0) return -1;
        return Stopwatch.GetElapsedTime(_lastUpdateTimestamp).TotalMilliseconds;
    }

    public void Stop()
    {
        if (_listener == null) return;
        
        _cts?.Cancel();
        _listener.Stop();
        _listener.Close();
        _listener = null;
        
        try { _listenerTask?.Wait(); } catch { /* Ignore */ }
        
        Log.Information("CS2 GSI Listener stopped.");
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}
