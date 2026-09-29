using CommunityToolkit.Mvvm.ComponentModel;
using HexAmbientLight.Core.Services;
using System.Threading.Tasks;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class AmbilightViewModel : ViewModelBase
{
    private readonly SettingsManager _settingsManager;

        [ObservableProperty]
    private int _targetFps;

    [ObservableProperty]
    private int _brightness;

    [ObservableProperty]
    private double _saturation;

    [ObservableProperty]
    private int _smoothingPercent;

        public AmbilightViewModel(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        TargetFps = _settingsManager.AppSettings.TargetFps;
        if (TargetFps <= 0) TargetFps = 30;
        
        Brightness = _settingsManager.AppSettings.BrightnessLimit;
        Saturation = _settingsManager.AppSettings.Saturation;
        SmoothingPercent = (int)(_settingsManager.AppSettings.SmoothingFactor * 100);
    }

        partial void OnTargetFpsChanged(int value)
    {
        _settingsManager.AppSettings.TargetFps = value;
        _ = _settingsManager.SaveSettingsAsync();
    }

    partial void OnBrightnessChanged(int value)
    {
        _settingsManager.AppSettings.BrightnessLimit = value;
        _ = _settingsManager.SaveSettingsAsync();
    }

    partial void OnSaturationChanged(double value)
    {
        _settingsManager.AppSettings.Saturation = value;
        _ = _settingsManager.SaveSettingsAsync();
    }

    partial void OnSmoothingPercentChanged(int value)
    {
        _settingsManager.AppSettings.SmoothingFactor = value / 100.0;
        _ = _settingsManager.SaveSettingsAsync();
    }
}