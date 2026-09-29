using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class DiagnosticsViewModel : ViewModelBase
{
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private string _logContent = "Loading...";

    public DiagnosticsViewModel()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += (s, e) => LoadLogs();
        _timer.Start();
        LoadLogs();
    }

    private void LoadLogs()
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDir = Path.Combine(appData, "HexAmbientLight", "Logs");
            var todayFile = Path.Combine(logDir, $"log-{DateTime.Now:yyyyMMdd}.txt");
            
            if (File.Exists(todayFile))
            {
                using var fs = new FileStream(todayFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs);
                LogContent = sr.ReadToEnd();
            }
            else
            {
                LogContent = "No log file for today yet.";
            }
        }
        catch (Exception ex)
        {
            LogContent = "Error reading logs: " + ex.Message;
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadLogs();
    }

    [RelayCommand]
    private void OpenLogsFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logDir = Path.Combine(appData, "HexAmbientLight", "Logs");
        if (Directory.Exists(logDir))
        {
            Process.Start(new ProcessStartInfo { FileName = logDir, UseShellExecute = true });
        }
    }

    [RelayCommand]
    private void CopyDiagnostics()
    {
        System.Windows.Clipboard.SetText(LogContent);
    }
}
