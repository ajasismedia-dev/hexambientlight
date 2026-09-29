using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using HexAmbientLight.Core.Services;
using HexAmbientLight.Core.Models;
using HexAmbientLight.Wpf.ViewModels;

namespace HexAmbientLight.Wpf;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        base.OnStartup(e);

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logDir = Path.Combine(appData, "HexAmbientLight", "Logs");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(logDir, "log-.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("BOOT 01 Host built");
            _host = Host.CreateDefaultBuilder(e.Args)
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<SettingsManager>();
                    services.AddSingleton<WledController>();
                    services.AddSingleton<ScreenCapturer>();
                    services.AddSingleton<Cs2GsiListener>();
                    services.AddSingleton<GameEffectEngine>();
                    services.AddSingleton<ModeOrchestrator>();
                    
                    services.AddSingleton<DashboardViewModel>();
                    services.AddSingleton<AmbilightViewModel>();
                    services.AddSingleton<GameViewModel>();
                    services.AddSingleton<ManualViewModel>();
                    services.AddSingleton<DeviceViewModel>();
                    services.AddSingleton<SettingsViewModel>();
                    services.AddSingleton<DiagnosticsViewModel>();
                    services.AddSingleton<MainViewModel>();
                    
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            Log.Information("BOOT 02 Settings initialized");
            var settingsManager = _host.Services.GetRequiredService<SettingsManager>();
            await settingsManager.InitializeAsync();

            Log.Information("BOOT 03 Resolving MainWindow");
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            
            Log.Information("BOOT 04 MainWindow resolved");
            this.MainWindow = mainWindow;
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.ShowInTaskbar = true;
            
            bool runtimeStarted = false;

            mainWindow.ContentRendered += async (_, _) =>
            {
                Log.Information("BOOT 07 ContentRendered");
                if (runtimeStarted) return;
                runtimeStarted = true;
                await InitializeRuntimeAsync(mainWindow);
            };

            Log.Information("BOOT 05 Calling Show");
            mainWindow.Show();
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;
            
            Log.Information("BOOT 06 MainWindow shown");
            mainWindow.Activate();
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "UI_STARTUP_CRASH.txt"), ex.ToString());
            System.Windows.MessageBox.Show(ex.ToString(), "HexAmbientLight UI Startup Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            throw;
        }
    }

    private async Task InitializeRuntimeAsync(MainWindow mainWindow)
    {
        Log.Information("RUNTIME 01 Host Start");
        try { await _host!.StartAsync(); } catch (Exception ex) { Log.Error(ex, "RUNTIME 01 ERROR"); }

        Log.Information("RUNTIME 02 GSI Start");
        try { _host!.Services.GetRequiredService<Cs2GsiListener>().Start(); } catch (Exception ex) { Log.Error(ex, "RUNTIME 02 ERROR"); }

        Log.Information("RUNTIME 03 WLED Connect START");
        try 
        { 
            var wledController = _host!.Services.GetRequiredService<WledController>();
            await wledController.ConnectAndSaveStateAsync(); 
            Log.Information("RUNTIME 04 WLED Connect RESULT: Connected={Connected}, IP={Ip}", wledController.IsConnected, wledController.CurrentIp);
        } 
        catch (Exception ex) 
        { 
            Log.Error(ex, "RUNTIME 04 WLED Connect RESULT ERROR"); 
        }

        Log.Information("RUNTIME 05 Monitor Handle");
        IntPtr monitorHandle = IntPtr.Zero;
        try 
        {
            var interopHelper = new System.Windows.Interop.WindowInteropHelper(mainWindow);
            monitorHandle = MonitorFromWindow(interopHelper.Handle, 2); // MONITOR_DEFAULTTONEAREST
        } 
        catch (Exception ex) { Log.Error(ex, "RUNTIME 05 ERROR"); }

        Log.Information("RUNTIME 06 ModeOrchestrator Start");
        try { _host!.Services.GetRequiredService<ModeOrchestrator>().Start(monitorHandle); } catch (Exception ex) { Log.Error(ex, "RUNTIME 06 ERROR"); }

        // RUNTIME 07 is inside ModeOrchestrator typically, but we can log here that we called start
        Log.Information("RUNTIME 08 Runtime Ready");
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            try { _host.Services.GetRequiredService<ModeOrchestrator>().Stop(); } catch {}
            try { _host.Services.GetRequiredService<Cs2GsiListener>().Stop(); } catch {}
            try { 
                var wledController = _host.Services.GetRequiredService<WledController>();
                await wledController.ExitRealtimeModeAsync(WledOnExitBehavior.RestorePreset);
            } catch {}
            try { await _host.StopAsync(TimeSpan.FromSeconds(2)); } catch {}
            try { _host.Dispose(); } catch {}
        }
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
