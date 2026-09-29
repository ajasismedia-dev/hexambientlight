using CommunityToolkit.Mvvm.ComponentModel;
using HexAmbientLight.Core.Services;
using System.Threading.Tasks;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class AmbilightViewModel : ViewModelBase
{
    private readonly SettingsManager _settingsManager;

    [ObservableProperty]
    private int _targetFps;

    public AmbilightViewModel(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        TargetFps = _settingsManager.AppSettings.TargetFps;
        if (TargetFps <= 0) TargetFps = 30;
    }

    partial void OnTargetFpsChanged(int value)
    {
        _settingsManager.AppSettings.TargetFps = value;
        // Fire and forget save
        _ = _settingsManager.SaveSettingsAsync();
    }
}
