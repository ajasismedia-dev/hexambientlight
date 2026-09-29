using CommunityToolkit.Mvvm.ComponentModel;
using HexAmbientLight.Core.Services;
using System.Threading;
using System.Threading.Tasks;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class AmbilightViewModel : ViewModelBase, System.IDisposable
{
    private readonly SettingsManager _settingsManager;
    private CancellationTokenSource? _debounceCts;

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

    private void QueueSave()
    {
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

    partial void OnTargetFpsChanged(int value)
    {
        _settingsManager.AppSettings.TargetFps = value;
        QueueSave();
    }

    partial void OnBrightnessChanged(int value)
    {
        _settingsManager.AppSettings.BrightnessLimit = value;
        QueueSave();
    }

    partial void OnSaturationChanged(double value)
    {
        _settingsManager.AppSettings.Saturation = value;
        QueueSave();
    }

    partial void OnSmoothingPercentChanged(int value)
    {
        _settingsManager.AppSettings.SmoothingFactor = value / 100.0;
        QueueSave();
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        // Fire and forget final save on dispose if any pending changes happened
        _ = _settingsManager.SaveSettingsAsync();
        
    }
}
