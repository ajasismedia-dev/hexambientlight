using CommunityToolkit.Mvvm.ComponentModel;
using HexAmbientLight.Core.Models;
using HexAmbientLight.Core.Services;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsManager _settingsManager;

    [ObservableProperty]
    private bool _closeToTray;

    [ObservableProperty]
    private bool _startWithWindows;

    public SettingsViewModel(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        CloseToTray = _settingsManager.AppSettings.CloseToTray;
        // StartWithWindows logic typically uses registry or startup folder. Skipping full implementation for UI shell, just placeholder.
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        _settingsManager.AppSettings.CloseToTray = CloseToTray;
        await _settingsManager.SaveSettingsAsync();
    }
}
