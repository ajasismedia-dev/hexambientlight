using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HexAmbientLight.Core.Services;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class ManualViewModel : ViewModelBase
{
    private readonly ModeOrchestrator _orchestrator;

    [ObservableProperty]
    private System.Windows.Media.Color _selectedColor = System.Windows.Media.Color.FromRgb(255, 255, 255);

    [ObservableProperty]
    private byte _red = 255;

    [ObservableProperty]
    private byte _green = 255;

    [ObservableProperty]
    private byte _blue = 255;

    public ManualViewModel(ModeOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    partial void OnRedChanged(byte value) => UpdateColor();
    partial void OnGreenChanged(byte value) => UpdateColor();
    partial void OnBlueChanged(byte value) => UpdateColor();

    private void UpdateColor()
    {
        SelectedColor = System.Windows.Media.Color.FromRgb(Red, Green, Blue);
        _orchestrator.ManualColorR = Red;
        _orchestrator.ManualColorG = Green;
        _orchestrator.ManualColorB = Blue;
    }

    [RelayCommand]
    private void SetPreset(string colorHex)
    {
        if (System.Windows.Media.ColorConverter.ConvertFromString(colorHex) is System.Windows.Media.Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }
    }
}
