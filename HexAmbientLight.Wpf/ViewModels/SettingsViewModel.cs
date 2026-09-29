using CommunityToolkit.Mvvm.ComponentModel;
using HexAmbientLight.Core.Services;
using HexAmbientLight.Core.Models;
using HexAmbientLight.Core.Localization;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsManager _settingsManager;

    [ObservableProperty]
    private string _wledIpAddress;

    [ObservableProperty]
    private string _uiLanguage;

    public string[] AvailableLanguages { get; } = { "en-US", "tr-TR" };

    public SettingsViewModel(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        WledIpAddress = _settingsManager.AppSettings.WledIpAddress ?? "192.168.1.2";
        UiLanguage = _settingsManager.AppSettings.UiLanguage ?? "en-US";
    }

    partial void OnWledIpAddressChanged(string value)
    {
        _settingsManager.AppSettings.WledIpAddress = value;
        _ = _settingsManager.SaveSettingsAsync();
    }

    partial void OnUiLanguageChanged(string value)
    {
        _settingsManager.AppSettings.UiLanguage = value;
        _ = _settingsManager.SaveSettingsAsync();
        LocalizationManager.Instance.CurrentLanguage = value;
    }
}
