using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HexAmbientLight.Core.Services;
using HexAmbientLight.Core.Models;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private readonly Cs2GsiListener _gsiListener;
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private string _cs2Status = "Disconnected";
    
    [ObservableProperty]
    private string _bombState = "Inactive";

    [ObservableProperty]
    private int _health = 0;

    [ObservableProperty]
    private int _armor = 0;

    public GameViewModel(Cs2GsiListener gsiListener)
    {
        _gsiListener = gsiListener;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (s, e) => UpdateStats();
        _timer.Start();
    }

    private void UpdateStats()
    {
        var state = _gsiListener.LatestState;
        bool hasCs2 = state != null && !_gsiListener.IsDataStale();
        Cs2Status = hasCs2 ? "Connected" : "Disconnected";
        
        if (hasCs2 && state != null)
        {
            BombState = state.Round?.Bomb != null ? state.Round.Bomb : "Inactive";
            Health = state.Player?.State?.Health ?? 0;
            Armor = state.Player?.State?.Armor ?? 0;
        }
        else
        {
            BombState = "Inactive";
            Health = 0;
            Armor = 0;
        }
    }
}
