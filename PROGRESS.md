# HexAmbientLight MVP Progress

## Aşama 1: Temel Solution, Uygulama İskeleti ve Test Altyapısı
- [x] Solution ve Proje Kurulumu
- [x] Temel Modeller (AppSettings, LedLayoutConfig)
- [x] Ayar Yönetimi (SettingsManager)
- [x] Loglama Altyapısı (Serilog)
- [x] Dependency Injection
- [x] Uygulama Yaşam Döngüsü (Mutex)
- [ ] Tray Kabuğu (Aktif NotifyIcon şu an yok)
- [x] Minimal Ana Pencere
- [x] Birim Testleri

## Aşama 2: WLED ve LED Layout (3 Bölgeli)
- [x] JSON API Bağlantısı
- [x] DDP Packetizer (Sequence desteği ile)
- [x] 3 Bölgeli LED Haritası (Sol, Üst, Sağ)
- [x] WLED Oturum Yönetimi

## Aşama 3: Ambilight
- [x] Windows Graphics Capture Motoru
- [x] Staging Texture Read (Tam downsampling henüz yok, doğrudan read yapılıyor)
- [x] 3 Kenar Renk Analizi

## Aşama 4: CS2 Telemetri
- [x] GSI Listener (localhost & token)
- [x] State Çıkarımı (Can, Mermi, Zırh, Spectator Koruması)
- [x] GameEffectEngine (Statik Renk/Bölge Eşleştirmesi)
- [x] Tahmini Bomba Sayacı (Sadece Planted state trigger)

## Aşama 5: Orchestrator & UI
- [x] SelectedMode & EffectiveMode Kontrolü
- [x] WPF UI Kurulumu ve Sidebar Navigation
- [ ] Taskbar Tray ve Sağ Tık Menüsü (İleride eklenecek)
- [ ] Gelişmiş UI Binding'ler

## Aşama 6: Stabilizasyon ve MVP
- [x] GSI Raw JSON ve Log Spam temizliği
- [x] SettingsManager test izolasyonu
- [x] WLED Connection state fix
- [x] Self-contained Publish EXE

## Son Durum (Stabilization Pass)
- WLED bağlantı state hataları (IsConnected güncellemeleri) çözüldü.
- Sidebar Navigation command ve brush'lar düzeltildi.
- İzlenen oyuncunun (spectate) Health/Armor değerleri oyun içi aydınlatmadan ve UI'dan korundu.
- Ayarlar testleri gerçek LocalAppData'ya yazmaktan izole edildi.
- Gereksiz DDP gönderim logları ve GSI JSON dökümü kaldırıldı.
- Ekran yakalama (ScreenCapture) frame kopyalama esnasındaki thread race condition'lar geçici bir lock ile giderildi.
