using System;
using System.Collections.Generic;
using System.Globalization;

namespace HexAmbientLight.Core.Localization;

public class LocalizationManager
{
    private static LocalizationManager? _instance;
    public static LocalizationManager Instance => _instance ??= new LocalizationManager();

    private string _currentLanguage = "en-US";
    public string CurrentLanguage 
    { 
        get => _currentLanguage; 
        set 
        { 
            if (_currentLanguage != value) 
            { 
                _currentLanguage = value; 
                LanguageChanged?.Invoke(this, EventArgs.Empty); 
            } 
        } 
    }

    public event EventHandler? LanguageChanged;

    private readonly Dictionary<string, Dictionary<string, string>> _resources = new();

    private LocalizationManager()
    {
        InitializeResources();
    }

    public string GetString(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (_resources.TryGetValue(_currentLanguage, out var langDict) && langDict.TryGetValue(key, out var val))
        {
            return val;
        }

        // Fallback to en-US
        if (_resources.TryGetValue("en-US", out var fallbackDict) && fallbackDict.TryGetValue(key, out var fallbackVal))
        {
            return fallbackVal;
        }

        // Missing key
        return key; // return key itself so it doesn't crash or look completely broken
    }

    private void InitializeResources()
    {
        _resources["en-US"] = new Dictionary<string, string>
        {
            // Navigation
            { "Nav.Dashboard", "Dashboard" },
            { "Nav.Ambilight", "Ambilight" },
            { "Nav.LightingStudio", "Lighting Studio" },
            { "Nav.Game", "Game (CS2)" },
            { "Nav.Device", "Device" },
            { "Nav.Settings", "Settings" },
            { "Nav.Diagnostics", "Diagnostics" },

            // Lighting Studio
            { "Studio.Title", "Lighting Studio" },
            { "Studio.Subtitle", "Create your own ambient lighting scene" },
            { "Studio.Effects", "Effects" },
            { "Studio.Colors", "Colors" },
            { "Studio.PrimaryColor", "Primary Color" },
            { "Studio.SecondaryColor", "Secondary Color" },
            { "Studio.AccentColor", "Accent Color" },
            { "Studio.Animation", "Animation" },
            { "Studio.Speed", "Speed" },
            { "Studio.Intensity", "Intensity" },
            { "Studio.Direction", "Direction" },
            { "Studio.Forward", "Forward" },
            { "Studio.Reverse", "Reverse" },
            { "Studio.Output", "Output" },
            { "Studio.QuickColors", "Quick Colors" },
            { "Studio.LivePreview", "Live Preview" },

            // Effects
            { "Effect.Static.Name", "Static" },
            { "Effect.Static.Desc", "Solid single color across all LEDs." },
            { "Effect.Breathing.Name", "Breathing" },
            { "Effect.Breathing.Desc", "Smooth fade in and out." },
            { "Effect.Pulse.Name", "Pulse" },
            { "Effect.Pulse.Desc", "Short distinct flashes of color." },
            { "Effect.Rainbow.Name", "Rainbow" },
            { "Effect.Rainbow.Desc", "Moving spectrum of all colors." },
            { "Effect.ColorWave.Name", "Color Wave" },
            { "Effect.ColorWave.Desc", "Wave of primary and secondary colors." },
            { "Effect.GradientFlow.Name", "Gradient Flow" },
            { "Effect.GradientFlow.Desc", "Smooth flowing gradient." },
            { "Effect.Aurora.Name", "Aurora" },
            { "Effect.Aurora.Desc", "Organic movement between three tones." },
            { "Effect.Fire.Name", "Fire" },
            { "Effect.Fire.Desc", "Flickering flame effect." },
            { "Effect.Cyber.Name", "Cyber" },
            { "Effect.Cyber.Desc", "Neon cyan and purple pulse." },

            // Common / Settings
            { "Common.Language", "Language" },
            { "Common.SaveSettings", "Save Settings" },
            { "Settings.Brightness", "Brightness Limit" },
            { "Settings.Saturation", "Saturation" },
            { "Settings.Smoothing", "Smoothing" },
            { "Settings.TargetFps", "Target FPS" },
            { "Settings.StartWithWindows", "Start with Windows (Coming Soon)" },
            { "Settings.CloseToTray", "Close to Tray (Coming Soon)" },
            { "Settings.WledIp", "WLED Device IP" },

            // Modes / Status
            { "Mode.Auto", "Auto" },
            { "Mode.Ambilight", "Ambilight" },
            { "Mode.LightingStudio", "Lighting Studio" },
            { "Mode.Off", "Off" },
            { "Mode.Game", "Game" },
            { "Status.Connected", "Connected" },
            { "Status.Disconnected", "Disconnected" },
            { "Status.Active", "Active" },
            { "Status.Inactive", "Inactive" },
            { "Common.HDRWarning", "HDR Warning" },
            { "Common.HDRWarningText", "HDR is not fully supported yet. Please disable HDR in Windows settings when using Ambilight mode for accurate colors." },
            { "Ambilight.Desc", "Screen capture and LED mapping" },
            { "Settings.Title", "Settings" },
            { "Settings.Desc", "Configure application preferences" }
        };

        _resources["tr-TR"] = new Dictionary<string, string>
        {
            // Navigation
            { "Nav.Dashboard", "Kontrol Paneli" },
            { "Nav.Ambilight", "Ambilight" },
            { "Nav.LightingStudio", "Aydınlatma Stüdyosu" },
            { "Nav.Game", "Oyun (CS2)" },
            { "Nav.Device", "Cihaz" },
            { "Nav.Settings", "Ayarlar" },
            { "Nav.Diagnostics", "Tanılama" },

            // Lighting Studio
            { "Studio.Title", "Aydınlatma Stüdyosu" },
            { "Studio.Subtitle", "Kendi ortam aydınlatma sahnenizi oluşturun" },
            { "Studio.Effects", "Efektler" },
            { "Studio.Colors", "Renkler" },
            { "Studio.PrimaryColor", "Ana Renk" },
            { "Studio.SecondaryColor", "İkincil Renk" },
            { "Studio.AccentColor", "Vurgu Rengi" },
            { "Studio.Animation", "Animasyon" },
            { "Studio.Speed", "Hız" },
            { "Studio.Intensity", "Yoğunluk" },
            { "Studio.Direction", "Yön" },
            { "Studio.Forward", "İleri" },
            { "Studio.Reverse", "Ters" },
            { "Studio.Output", "Çıktı" },
            { "Studio.QuickColors", "Hızlı Renkler" },
            { "Studio.LivePreview", "Canlı Önizleme" },

            // Effects
            { "Effect.Static.Name", "Sabit" },
            { "Effect.Static.Desc", "Tüm LED'lerde sabit tek renk." },
            { "Effect.Breathing.Name", "Nefes" },
            { "Effect.Breathing.Desc", "Yumuşak şekilde yanıp sönme." },
            { "Effect.Pulse.Name", "Vuruş" },
            { "Effect.Pulse.Desc", "Kısa ve belirgin renk parlamaları." },
            { "Effect.Rainbow.Name", "Gökkuşağı" },
            { "Effect.Rainbow.Desc", "Tüm renklerin hareketli tayfı." },
            { "Effect.ColorWave.Name", "Renk Dalgası" },
            { "Effect.ColorWave.Desc", "Ana ve ikincil renk dalgası." },
            { "Effect.GradientFlow.Name", "Gradyan Akışı" },
            { "Effect.GradientFlow.Desc", "Yumuşak akan geçişli renkler." },
            { "Effect.Aurora.Name", "Aurora" },
            { "Effect.Aurora.Desc", "Üç ton arasında organik hareket." },
            { "Effect.Fire.Name", "Ateş" },
            { "Effect.Fire.Desc", "Titreşen alev efekti." },
            { "Effect.Cyber.Name", "Siber" },
            { "Effect.Cyber.Desc", "Neon camgöbeği ve mor atışları." },

            // Common / Settings
            { "Common.Language", "Dil / Language" },
            { "Common.SaveSettings", "Ayarları Kaydet" },
            { "Settings.Brightness", "Parlaklık Sınırı" },
            { "Settings.Saturation", "Doygunluk" },
            { "Settings.Smoothing", "Yumuşatma (Smoothing)" },
            { "Settings.TargetFps", "Hedef FPS" },
            { "Settings.StartWithWindows", "Windows ile Başla (Yakında)" },
            { "Settings.CloseToTray", "Tepsiye Küçült (Yakında)" },
            { "Settings.WledIp", "WLED Cihaz IP" },

            // Modes / Status
            { "Mode.Auto", "Otomatik" },
            { "Mode.Ambilight", "Ambilight" },
            { "Mode.LightingStudio", "Aydınlatma Stüdyosu" },
            { "Mode.Off", "Kapalı" },
            { "Mode.Game", "Oyun" },
            { "Status.Connected", "Bağlı" },
            { "Status.Disconnected", "Bağlantı Yok" },
            { "Status.Active", "Aktif" },
            { "Status.Inactive", "İnaktif" },
            { "Common.HDRWarning", "HDR Uyarısı" },
            { "Common.HDRWarningText", "HDR tam olarak desteklenmemektedir. Doğru renkler için Ambilight modunu kullanırken Windows ayarlarından HDR'ı kapatın." },
            { "Ambilight.Desc", "Ekran yakalama ve LED eşleme" },
            { "Settings.Title", "Ayarlar" },
            { "Settings.Desc", "Uygulama tercihlerini yapılandırın" }
        };
    }
}

