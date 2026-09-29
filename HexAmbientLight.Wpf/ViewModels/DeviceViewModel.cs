using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexAmbientLight.Core.Models;
using HexAmbientLight.Core.Services;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class DeviceViewModel : ViewModelBase
{
    private readonly SettingsManager _settingsManager;

    [ObservableProperty]
    private string _wledIp;

    [ObservableProperty]
    private int _leftCount;

    [ObservableProperty]
    private int _topCount;

    [ObservableProperty]
    private int _rightCount;

    [ObservableProperty]
    private int _totalCount;

    public DeviceViewModel(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        WledIp = _settingsManager.AppSettings.WledIpAddress;
        
        var layout = _settingsManager.LayoutConfig;
        LeftCount = layout.LeftCount;
        TopCount = layout.TopCount;
        RightCount = layout.RightCount;
        TotalCount = layout.TotalLeds;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        _settingsManager.AppSettings.WledIpAddress = WledIp;
        // Layout updates not supported in MVP
        await _settingsManager.SaveSettingsAsync();
    }
}
