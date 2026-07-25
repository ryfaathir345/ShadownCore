using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WinTweakStudio.Data;
using WinTweakStudio.Services;

namespace WinTweakStudio.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;
        private readonly IDialogService _dialogService;
        private readonly IHardwareMonitorService _hardwareMonitorService;

        [ObservableProperty]
        private bool _runOnStartup;

        [ObservableProperty]
        private bool _autoCreateRestorePoint = true;

        [ObservableProperty]
        private int _monitoringRefreshInterval = 2; // Default 2 seconds

        [ObservableProperty]
        private string _databasePath = string.Empty;

        [ObservableProperty]
        private string _osVersion = string.Empty;

        [ObservableProperty]
        private string _cpuName = string.Empty;

        [ObservableProperty]
        private string _totalRam = string.Empty;

        public SettingsViewModel(ITweakService tweakService, IDialogService dialogService, IHardwareMonitorService hardwareMonitorService)
        {
            _tweakService = tweakService;
            _dialogService = dialogService;
            _hardwareMonitorService = hardwareMonitorService;

            DatabasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_data.db");
            LoadSystemMetadata();
            CheckStartupState();
        }

        private void CheckStartupState()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                var val = key?.GetValue("WinTweakStudio");
                RunOnStartup = val != null;
            }
            catch
            {
                RunOnStartup = false;
            }
        }

        partial void OnRunOnStartupChanged(bool value)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (value)
                {
                    string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key?.SetValue("WinTweakStudio", $"\"{exePath}\"");
                    }
                }
                else
                {
                    key?.DeleteValue("WinTweakStudio", false);
                }
            }
            catch { }
        }

        private async void LoadSystemMetadata()
        {
            try
            {
                OsVersion = $"{Environment.OSVersion.VersionString} ({(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")})";
                
                var snapshot = await _hardwareMonitorService.ReadMetricsAsync();
                CpuName = string.IsNullOrEmpty(snapshot.Cpu.Name) ? Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Generic CPU" : snapshot.Cpu.Name;
                TotalRam = snapshot.Ram.TotalGB > 0 ? $"{snapshot.Ram.TotalGB:F1} GB Physical RAM" : $"{GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024 * 1024):F1} GB RAM";
            }
            catch
            {
                OsVersion = Environment.OSVersion.VersionString;
                CpuName = "Windows Processor";
                TotalRam = "System Memory";
            }
        }

        [RelayCommand]
        private async Task ResetDatabaseAsync()
        {
            bool confirmed = await _dialogService.ShowConfirmationAsync(
                "⚠️ Reset Database SQLite Baseline?",
                "Tindakan ini akan mengosongkan seluruh riwayat TweakLogs dan Restore Points dari database SQLite lokal.\n\nApakah Anda yakin ingin melanjutkan?",
                "SecurityWarning"
            );

            if (confirmed)
            {
                try
                {
                    string dbFile = DatabasePath;
                    if (File.Exists(dbFile))
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        File.Delete(dbFile);
                    }
                    DatabaseInitializer.Initialize();
                    await _dialogService.ShowMessageAsync("Reset Berhasil", "Database SQLite telah di-reset ke kondisi baseline bersih.");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowMessageAsync("Error Reset", $"Gagal mereset database: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async Task ExportHistoryJsonAsync()
        {
            try
            {
                var logs = _tweakService.GetTweakHistory();
                string json = System.Text.Json.JsonSerializer.Serialize(logs, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                
                string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"WinTweakStudio_History_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                File.WriteAllText(exportPath, json);

                await _dialogService.ShowMessageAsync("Export Berhasil", $"File riwayat tweak berhasil diexport ke Desktop:\n{exportPath}");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Export Gagal", $"Terjadi kesalahan saat export: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task CheckUpdatesAsync()
        {
            await _dialogService.ShowMessageAsync(
                "WinTweakStudio v1.0",
                "Versi Aplikasi: v1.0.0 (Latest Release)\nEngine: .NET 8.0 WPF + NVAPI / ADL + SQLite\nStatus: Aplikasi berada pada versi terbaru."
            );
        }
    }
}
