using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HexAmbientLight.Core.Models;
using Serilog;

using Microsoft.Win32;

namespace HexAmbientLight.Core.Services;

public class ModeOrchestrator : IDisposable
{
    private readonly SettingsManager _settingsManager;
    private readonly WledController _wledController;
    private readonly ScreenCapturer _screenCapturer;
    private readonly Cs2GsiListener _gsiListener;
    private readonly GameEffectEngine _gameEngine;

    private SelectedMode _selectedMode = SelectedMode.Auto;
    private EffectiveMode _effectiveMode = EffectiveMode.Off;
    private EffectiveMode _lastEffectiveMode = EffectiveMode.Off;

    private CancellationTokenSource? _cts;
    private Task? _loopTask;
    private IntPtr _monitorHandle = IntPtr.Zero;
    private bool _isSuspended = false;

    // For manual mode MVP
    public byte ManualColorR { get; set; } = 255;
    public byte ManualColorG { get; set; } = 255;
    public byte ManualColorB { get; set; } = 255;

    public SelectedMode SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (_selectedMode != value)
            {
                _selectedMode = value;
                Log.Information("Selected mode changed to {Mode}", value);
            }
        }
    }

    public EffectiveMode EffectiveMode => _effectiveMode;

    public ModeOrchestrator(
        SettingsManager settingsManager,
        WledController wledController,
        ScreenCapturer screenCapturer,
        Cs2GsiListener gsiListener,
        GameEffectEngine gameEngine)
    {
        _settingsManager = settingsManager;
        _wledController = wledController;
        _screenCapturer = screenCapturer;
        _gsiListener = gsiListener;
        _gameEngine = gameEngine;
        
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Suspend)
        {
            Log.Information("System suspending, stopping DDP and WLED.");
            _isSuspended = true;
            _screenCapturer.StopCapture();
            // Fire and forget
            _ = _wledController.ExitRealtimeModeAsync(WledOnExitBehavior.RestorePreset);
        }
        else if (e.Mode == PowerModes.Resume)
        {
            Log.Information("System resuming, delaying reconnect.");
            // Delay a bit to let network adapters wake up
            Task.Run(async () => {
                await Task.Delay(5000);
                await _wledController.ConnectAndSaveStateAsync();
                _isSuspended = false;
                // Transition logic will naturally restart capture if needed since _lastEffectiveMode might not match
                _lastEffectiveMode = EffectiveMode.Off; // Force a transition
            });
        }
    }

    public void Start(IntPtr hMonitor)
    {
        if (_cts != null) return;

        _monitorHandle = hMonitor;
        _cts = new CancellationTokenSource();
        _loopTask = Task.Run(() => OrchestrateLoop(_cts.Token));
        Log.Information("Mode Orchestrator started.");
    }

        private async Task OrchestrateLoop(CancellationToken token)
    {
        int currentTargetFps = _settingsManager.AppSettings.TargetFps;
        if (currentTargetFps <= 0) currentTargetFps = 30;
        var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000.0 / currentTargetFps));

        while (!token.IsCancellationRequested)
        {
            try
            {
                await periodicTimer.WaitForNextTickAsync(token);

                int newFps = _settingsManager.AppSettings.TargetFps;
                if (newFps > 0 && newFps != currentTargetFps)
                {
                    currentTargetFps = newFps;
                    periodicTimer.Dispose();
                    periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000.0 / currentTargetFps));
                }

                if (!_isSuspended)
                {
                    DetermineEffectiveMode();
                    await HandleModeTransitionsAsync();
                    await ExecuteEffectiveModeAsync();
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception in OrchestrateLoop");
            }
        }
        
        periodicTimer.Dispose();
    }

    private void DetermineEffectiveMode()
    {
        bool hasValidGameData = _settingsManager.AppSettings.Cs2Enabled 
                                && _gsiListener.LatestState != null 
                                && !_gsiListener.IsDataStale();

        switch (_selectedMode)
        {
            case SelectedMode.Auto:
                _effectiveMode = hasValidGameData ? EffectiveMode.Game : EffectiveMode.Ambilight;
                break;
            case SelectedMode.Game:
                _effectiveMode = hasValidGameData ? EffectiveMode.Game : EffectiveMode.Off; // Standby
                break;
            case SelectedMode.Ambilight:
                _effectiveMode = EffectiveMode.Ambilight;
                break;
            case SelectedMode.Manual:
                _effectiveMode = EffectiveMode.Manual;
                break;
            case SelectedMode.Off:
            default:
                _effectiveMode = EffectiveMode.Off;
                break;
        }
    }

    private async Task HandleModeTransitionsAsync()
    {
        if (_effectiveMode != _lastEffectiveMode)
        {
            var ageMs = _gsiListener.GetLastUpdateAgeMs();
            var reason = "State Change";
            if (_selectedMode == SelectedMode.Auto)
            {
                if (_effectiveMode == EffectiveMode.Game) reason = "Valid GSI detected";
                else if (_effectiveMode == EffectiveMode.Ambilight && _lastEffectiveMode == EffectiveMode.Game) reason = "GSI stale/timeout";
                else if (_effectiveMode == EffectiveMode.Ambilight && _lastEffectiveMode == EffectiveMode.Off) reason = "Initial auto fallback";
            }
            
            Log.Information("Transition: SelectedMode={Selected} | Old={Old} | New={New} | Reason={Reason} | GSIAge={Age:F1}ms", 
                _selectedMode, _lastEffectiveMode, _effectiveMode, reason, ageMs);

            // Turn off screen capture if no longer needed
            if (_lastEffectiveMode == EffectiveMode.Ambilight && _effectiveMode != EffectiveMode.Ambilight)
            {
                _screenCapturer.StopCapture();
            }

            // Start screen capture if entering Ambilight
            if (_effectiveMode == EffectiveMode.Ambilight && _monitorHandle != IntPtr.Zero)
            {
                await _screenCapturer.StartCaptureAsync(_monitorHandle);
            }

            // Handle WLED exit if entering Off
            if (_effectiveMode == EffectiveMode.Off && _lastEffectiveMode != EffectiveMode.Off)
            {
                await _wledController.ExitRealtimeModeAsync(WledOnExitBehavior.RestorePreset);
            }

            // Reset Game state if leaving Game mode
                        if (_effectiveMode != _lastEffectiveMode)
            {
                _postProcessor.Reset();
            }

            if (_lastEffectiveMode == EffectiveMode.Game && _effectiveMode != EffectiveMode.Game)
            {
                _gameEngine.ResetState();
            }

            _lastEffectiveMode = _effectiveMode;
        }
    }

        private readonly ImagePostProcessor _postProcessor = new ImagePostProcessor();
    private byte[]? _reusableFrameBuffer;

    private async Task ExecuteEffectiveModeAsync()
    {
        if (_effectiveMode == EffectiveMode.Off) return;

        var layout = _settingsManager.LayoutConfig;
        if (!layout.IsValid)
        {
            return;
        }

        byte[] rgbData = Array.Empty<byte>();

        if (_effectiveMode == EffectiveMode.Ambilight)
        {
            int requiredLength = _screenCapturer.Width * _screenCapturer.Height * 4;
            if (_reusableFrameBuffer == null || _reusableFrameBuffer.Length != requiredLength)
            {
                if (requiredLength > 0) _reusableFrameBuffer = new byte[requiredLength];
            }

            if (_reusableFrameBuffer != null && _screenCapturer.TryGetLatestFrameData(_reusableFrameBuffer))
            {
                rgbData = EdgeColorExtractor.Extract(_reusableFrameBuffer, _screenCapturer.Width, _screenCapturer.Height, layout);
            }
        }
        else if (_effectiveMode == EffectiveMode.Game)
        {
            var state = _gsiListener.LatestState;
            if (state != null)
            {
                rgbData = _gameEngine.Render(state);
            }
        }
        else if (_effectiveMode == EffectiveMode.Manual)
        {
            rgbData = new byte[layout.TotalLeds * 3];
            for (int i = 0; i < layout.TotalLeds; i++)
            {
                rgbData[i * 3] = ManualColorR;
                rgbData[i * 3 + 1] = ManualColorG;
                rgbData[i * 3 + 2] = ManualColorB;
            }
        }

        if (rgbData.Length > 0)
        {
            _postProcessor.ApplyPostProcessing(rgbData, _settingsManager.AppSettings);
            await _wledController.SendDdpFrameAsync(rgbData);
        }
        
    }

    public void Stop()
    {
        _cts?.Cancel();
        try { _loopTask?.Wait(); } catch { }
        _screenCapturer.StopCapture();
        Log.Information("Mode Orchestrator stopped.");
    }

    public void Dispose()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        Stop();
        _cts?.Dispose();
    }
}
