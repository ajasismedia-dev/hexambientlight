using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexAmbientLight.Core.Models;
using HexAmbientLight.Core.Services;
using HexAmbientLight.Core.Localization;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly ModeOrchestrator _orchestrator;
    private readonly WledController _wledController;
    private readonly ScreenCapturer _screenCapturer;
    private readonly Cs2GsiListener _gsiListener;
    private readonly DispatcherTimer _timer;
    private int _healthCheckCounter = 0;

    [ObservableProperty]
    private string _wledStatus = "";

    [ObservableProperty]
    private string _selectedMode = "";

    [ObservableProperty]
    private string _effectiveMode = "";

    [ObservableProperty]
    private int _captureFps = 0;

    [ObservableProperty]
    private int _sendFps = 0;

    [ObservableProperty]
    private string _cs2Status = "";
    
    [ObservableProperty]
    private string _bombState = "";

    [ObservableProperty]
    private string _wledIp = "";

    public DashboardViewModel(
        ModeOrchestrator orchestrator,
        WledController wledController,
        ScreenCapturer screenCapturer,
        Cs2GsiListener gsiListener)
    {
        _orchestrator = orchestrator;
        _wledController = wledController;
        _screenCapturer = screenCapturer;
        _gsiListener = gsiListener;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += async (s, e) => await UpdateStatsAsync();
        _timer.Start();

        LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        _ = UpdateStatsAsync();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Force refresh strings
        _ = UpdateStatsAsync();
    }

    private async Task UpdateStatsAsync()
    {
        _healthCheckCounter++;
        if (_healthCheckCounter >= 4)
        {
            _healthCheckCounter = 0;
            await _wledController.IsAliveAsync(); // updates IsConnected
        }

        var loc = LocalizationManager.Instance;

        WledStatus = _wledController.IsConnected ? loc.GetString("Status.Connected") : loc.GetString("Status.Disconnected");
        WledIp = _wledController.CurrentIp ?? "None";
        
        SelectedMode = loc.GetString("Mode." + _orchestrator.SelectedMode.ToString());
        EffectiveMode = loc.GetString("Mode." + _orchestrator.EffectiveMode.ToString());
        
        CaptureFps = _screenCapturer.CurrentFps;
        SendFps = _wledController.CurrentSendFps;

        bool hasCs2 = _gsiListener.LatestState != null && !_gsiListener.IsDataStale();
        Cs2Status = hasCs2 ? loc.GetString("Status.Connected") : loc.GetString("Status.Disconnected");
        
        if (hasCs2 && _gsiListener.LatestState != null && _gsiListener.LatestState.Round?.Bomb != null)
        {
            BombState = _gsiListener.LatestState.Round?.Bomb ?? loc.GetString("Status.Inactive");
        }
        else
        {
            BombState = loc.GetString("Status.Inactive");
        }
    }

    [RelayCommand]
    private void SetMode(string modeStr)
    {
        if (Enum.TryParse<SelectedMode>(modeStr, out var mode))
        {
            _orchestrator.SelectedMode = mode;
            // Mode string will be updated on next tick
        }
    }

    public void Dispose()
    {
        LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
        _timer.Stop();
    }
}
