using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexAmbientLight.Core.Models;
using HexAmbientLight.Core.Services;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ModeOrchestrator _orchestrator;
    private readonly WledController _wledController;
    private readonly ScreenCapturer _screenCapturer;
    private readonly Cs2GsiListener _gsiListener;
    private readonly DispatcherTimer _timer;
    private int _healthCheckCounter = 0;

    [ObservableProperty]
    private string _wledStatus = "Unknown";

    [ObservableProperty]
    private string _selectedMode = "Auto";

    [ObservableProperty]
    private string _effectiveMode = "Off";

    [ObservableProperty]
    private int _captureFps = 0;

    [ObservableProperty]
    private int _sendFps = 0;

    [ObservableProperty]
    private string _cs2Status = "Disconnected";
    
    [ObservableProperty]
    private string _bombState = "Inactive";

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
    }

    private async Task UpdateStatsAsync()
    {
        _healthCheckCounter++;
        if (_healthCheckCounter >= 4)
        {
            _healthCheckCounter = 0;
            await _wledController.IsAliveAsync(); // updates IsConnected
        }

        WledStatus = _wledController.IsConnected ? "Connected" : "Disconnected";
        WledIp = _wledController.CurrentIp ?? "None";
        
        SelectedMode = _orchestrator.SelectedMode.ToString();
        EffectiveMode = _orchestrator.EffectiveMode.ToString();
        
        CaptureFps = _screenCapturer.CurrentFps;
        SendFps = _wledController.CurrentSendFps;

        bool hasCs2 = _gsiListener.LatestState != null && !_gsiListener.IsDataStale();
        Cs2Status = hasCs2 ? "Connected" : "Disconnected";
        
        if (hasCs2 && _gsiListener.LatestState != null && _gsiListener.LatestState.Round?.Bomb != null)
        {
            BombState = _gsiListener.LatestState.Round?.Bomb ?? "Inactive";
        }
        else
        {
            BombState = "Inactive";
        }
    }

    [RelayCommand]
    private void SetMode(string modeStr)
    {
        if (Enum.TryParse<SelectedMode>(modeStr, out var mode))
        {
            _orchestrator.SelectedMode = mode;
            SelectedMode = mode.ToString();
        }
    }
}
