using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HexAmbientLight.Wpf.Controls;

public partial class HsvColorPicker : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty SelectedColorHexProperty =
        DependencyProperty.Register(nameof(SelectedColorHex), typeof(string), typeof(HsvColorPicker),
            new FrameworkPropertyMetadata("#FFFF0000", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorHexChanged));

    public string SelectedColorHex
    {
        get => (string)GetValue(SelectedColorHexProperty);
        set => SetValue(SelectedColorHexProperty, value);
    }

    private bool _isUpdating = false;

    public HsvColorPicker()
    {
        InitializeComponent();
        HueSlider.ValueChanged += (s, e) => UpdateColorFromSliders();
        SaturationSlider.ValueChanged += (s, e) => UpdateColorFromSliders();
        HexInput.TextChanged += (s, e) => UpdateColorFromHex();
    }

    private static void OnSelectedColorHexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HsvColorPicker picker && !picker._isUpdating)
        {
            picker.ParseAndSetHex((string)e.NewValue);
        }
    }

    private void ParseAndSetHex(string hex)
    {
        try
        {
            if (string.IsNullOrEmpty(hex)) return;
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            _isUpdating = true;
            ColorPreview.Background = new SolidColorBrush(color);
            HexInput.Text = hex;
            
            // Simple RGB to HSV
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double diff = max - min;
            
            double h = 0;
            if (max == min) h = 0;
            else if (max == r) h = (60 * ((g - b) / diff) + 360) % 360;
            else if (max == g) h = (60 * ((b - r) / diff) + 120) % 360;
            else if (max == b) h = (60 * ((r - g) / diff) + 240) % 360;
            
            double s = max == 0 ? 0 : diff / max;

            HueSlider.Value = h;
            SaturationSlider.Value = s;
            _isUpdating = false;
        }
        catch { }
    }

    private void UpdateColorFromSliders()
    {
        if (_isUpdating) return;
        _isUpdating = true;
        
        double h = HueSlider.Value;
        double s = SaturationSlider.Value;
        double v = 1.0; // Keep value at 1.0 for ambient light, we have global brightness

        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;
        
        double r = 0, g = 0, b = 0;
        if (h >= 0 && h < 60) { r = c; g = x; b = 0; }
        else if (h >= 60 && h < 120) { r = x; g = c; b = 0; }
        else if (h >= 120 && h < 180) { r = 0; g = c; b = x; }
        else if (h >= 180 && h < 240) { r = 0; g = x; b = c; }
        else if (h >= 240 && h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        byte R = (byte)((r + m) * 255);
        byte G = (byte)((g + m) * 255);
        byte B = (byte)((b + m) * 255);

        string hex = $"#FF{R:X2}{G:X2}{B:X2}";
        SelectedColorHex = hex;
        ColorPreview.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(R, G, B));
        HexInput.Text = hex;
        
        _isUpdating = false;
    }

    private void UpdateColorFromHex()
    {
        if (_isUpdating) return;
        string hex = HexInput.Text;
        if (hex.Length == 7 || hex.Length == 9)
        {
            ParseAndSetHex(hex);
            SelectedColorHex = hex;
        }
    }
}



