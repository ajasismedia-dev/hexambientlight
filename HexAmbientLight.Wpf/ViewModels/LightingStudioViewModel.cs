using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexAmbientLight.Core.Services;
using HexAmbientLight.Core.Models;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class LightingStudioViewModel : ViewModelBase, System.IDisposable
{
    private readonly SettingsManager _settingsManager;
    private CancellationTokenSource? _debounceCts;

    [ObservableProperty]
    private string _selectedEffectId = "Static";

    [ObservableProperty]
    private string _primaryColorHex = "#FF00FFFF";

    [ObservableProperty]
    private string _secondaryColorHex = "#FFFF00FF";

    [ObservableProperty]
    private string _tertiaryColorHex = "#FFFFFF00";

    [ObservableProperty]
    private double _speed = 0.5;

    [ObservableProperty]
    private double _intensity = 1.0;

    [ObservableProperty]
    private int _direction = 1;

    public ObservableCollection<string> AvailableEffects { get; } = new()
    {
        "Static", "Breathing", "Pulse", "Rainbow", "ColorWave", "GradientFlow", "Aurora", "Fire", "Cyber"
    };

    public LightingStudioViewModel(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        var s = _settingsManager.AppSettings;

        SelectedEffectId = string.IsNullOrWhiteSpace(s.EffectId) ? "Static" : s.EffectId;
        PrimaryColorHex = string.IsNullOrWhiteSpace(s.EffectPrimaryColorHex) ? "#FF00FFFF" : s.EffectPrimaryColorHex;
        SecondaryColorHex = string.IsNullOrWhiteSpace(s.EffectSecondaryColorHex) ? "#FFFF00FF" : s.EffectSecondaryColorHex;
        TertiaryColorHex = string.IsNullOrWhiteSpace(s.EffectTertiaryColorHex) ? "#FFFFFF00" : s.EffectTertiaryColorHex;
        Speed = s.EffectSpeed;
        Intensity = s.EffectIntensity;
        Direction = s.EffectDirection == 0 ? 1 : s.EffectDirection;
    }

    private void QueueSave()
    {
        _settingsManager.AppSettings.EffectId = SelectedEffectId;
        _settingsManager.AppSettings.EffectPrimaryColorHex = PrimaryColorHex;
        _settingsManager.AppSettings.EffectSecondaryColorHex = SecondaryColorHex;
        _settingsManager.AppSettings.EffectTertiaryColorHex = TertiaryColorHex;
        _settingsManager.AppSettings.EffectSpeed = Speed;
        _settingsManager.AppSettings.EffectIntensity = Intensity;
        _settingsManager.AppSettings.EffectDirection = Direction;

        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        Task.Run(async () => 
        {
            try
            {
                await Task.Delay(400, token);
                if (!token.IsCancellationRequested)
                {
                    await _settingsManager.SaveSettingsAsync();
                }
            }
            catch (TaskCanceledException) { }
        });
    }

    partial void OnSelectedEffectIdChanged(string value) => QueueSave();
    partial void OnPrimaryColorHexChanged(string value) => QueueSave();
    partial void OnSecondaryColorHexChanged(string value) => QueueSave();
    partial void OnTertiaryColorHexChanged(string value) => QueueSave();
    partial void OnSpeedChanged(double value) => QueueSave();
    partial void OnIntensityChanged(double value) => QueueSave();
    partial void OnDirectionChanged(int value) => QueueSave();

    [RelayCommand]
    private void SetQuickColor(string hex)
    {
        SelectedEffectId = "Static";
        PrimaryColorHex = hex;
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _ = _settingsManager.SaveSettingsAsync();
    }
}
