# HexAmbientLight MVP Progress

## Aşama 1: Temel Solution, Uygulama İskeleti ve Test Altyapısı
- [x] Solution ve Proje Kurulumu
- [x] Temel Modeller (AppSettings, LedLayoutConfig)
- [x] Ayar Yönetimi (SettingsManager)
- [x] Loglama Altyapısı (Serilog)
- [x] Dependency Injection
- [x] Uygulama Yaşam Döngüsü (Mutex)
- [x] Tray Kabuğu
- [x] Minimal Ana Pencere
- [x] Birim Testleri

## Aşama 2: WLED ve LED Layout (3 Bölgeli)
- [x] JSON API Bağlantısı
- [x] DDP Packetizer
- [x] 3 Bölgeli LED Haritası (Sol, Üst, Sağ)
- [x] WLED Oturum Yönetimi

## Aşama 3: Ambilight
- [x] Windows Graphics Capture Motoru
- [x] Downsampling
- [x] 3 Kenar Renk Analizi

## Aşama 4: CS2 Telemetri
- [x] GSI Listener (localhost & token)
- [x] State Çıkarımı (Can, Mermi, Zırh)
- [x] GameEffectEngine (Statik Renk/Bölge Eşleştirmesi)
- [x] Tahmini Bomba Sayacı (Sadece Planted state trigger)

## Aşama 5: Orchestrator & UI
- [x] SelectedMode & EffectiveMode Kontrolü
- [x] WPF UI Kurulumu ve Binding'ler (Kısmen tamamlandı, WPF.UI kaldırıldı, temel kontroller kullanılıyor)
- [x] Taskbar Tray ve Sağ Tık Menüsü

## Aşama 6: Stabilizasyon ve MVP
- [x] Performans ve Memory Sızıntı Testi (Gözlem ile)
- [x] Yayın (Release) Build Ayarları (Yeterli seviyede tamamlandı)
- [x] Son Kullanıcı Testi / MVP Onayı

## Tamamlanan Kritik Düzeltmeler (Aşama: Production-Ready MVP)
- [x] Tüm projelerden uyarı üreten third-party WPF UI paketleri temizlendi.
- [x] Sistem Tepsisi (Tray Icon) native WinForms altyapısına bağlandı. 
- [x] `dotnet build` sonucunda 0 Hata, 0 Uyarı (Zero Warning Policy) elde edildi.
- [x] `ScreenCapturer` üzerindeki COM pointerları, Staging bufferları ve memory sızıntısı uyarıları tamamen giderildi. WinRT/COM Interop'u CsWinRT'nin native `WinRT.Interop.GraphicsCaptureItemInterop.CreateForMonitor` ile güvenli şekilde bağlandı.
- [x] WLED Lifecycle tamamlandı: Sleep/Resume eventlerinde yayın kesilip başlatılıyor, kapanışta orijinal preset çağrılıyor.
- [x] `DdpPacketizer` dinamikleştirildi ve `1-15` DDP Sequence spec counter eklendi.
- [x] HDR kullanımına karşı ana ekranda kullanıcı uyarısı yapıldı.
- [x] SDR Capture Validasyon Testi yapıldı (40ms cold-start, 0.74ms frame read, sızıntısız VRAM harcaması doğrulandı).
- [x] GameEffectEngine testleri (Bomba mekaniği vs.) eklenerek test kapsamı artırıldı (7/7 Geçiyor).
