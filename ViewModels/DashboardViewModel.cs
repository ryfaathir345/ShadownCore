using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTweakStudio.Models;
using WinTweakStudio.Services;

namespace WinTweakStudio.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly IDialogService _dialogService;
        private readonly IOptimizationProfileService _profileService;
        private readonly IGameDetectionService _gameDetectionService;
        private readonly ITroubleshootService _troubleshootService;
        private readonly IDriverScannerService _driverScannerService;
        [ObservableProperty]
        private bool _isAutoRamCleanEnabled;

        partial void OnIsAutoRamCleanEnabledChanged(bool value)
        {
            if (value)
            {
                _autoRamTimer.Start();
            }
            else
            {
                _autoRamTimer.Stop();
            }
        }

        private readonly DispatcherTimer _autoRamTimer;
        private readonly DispatcherTimer _timer;

        public IOptimizationProfileService ProfileService => _profileService;
        public IGameDetectionService GameDetection => _gameDetectionService;

        [ObservableProperty]
        private int _totalTweaksApplied;

        [ObservableProperty]
        private int _totalRestorePoints;

        [ObservableProperty]
        private string _systemStatus = "Optimized & Protected";

        [ObservableProperty]
        private string _osVersion = "Windows 11 Pro 64-bit";

        // CPU Sensors
        [ObservableProperty]
        private string _cpuName = "Detecting CPU...";

        [ObservableProperty]
        private string _cpuTempText = "N/A";

        [ObservableProperty]
        private string _cpuUsageText = "0%";

        [ObservableProperty]
        private string _cpuPowerText = "N/A";

        [ObservableProperty]
        private ObservableCollection<CoreClockInfo> _cpuCoreClocks = new();

        // GPU Sensors
        [ObservableProperty]
        private ObservableCollection<GpuSensorData> _detectedGpus = new();

        [ObservableProperty]
        private string _gpuName = "Detecting GPU...";

        [ObservableProperty]
        private string _gpuTempText = "N/A";

        [ObservableProperty]
        private string _gpuUsageText = "0%";

        [ObservableProperty]
        private string _gpuVramText = "N/A";

        [ObservableProperty]
        private string _gpuPowerText = "N/A";

        [ObservableProperty]
        private string _gpuHybridStatus = "Single GPU";

        // RAM Sensors
        [ObservableProperty]
        private string _ramUsageText = "0 / 0 GB";

        [ObservableProperty]
        private string _ramUsagePercentText = "0%";

        [ObservableProperty]
        private string _ramSpeedText = "N/A";

        // Storage Drives
        [ObservableProperty]
        private ObservableCollection<StorageSensorData> _storageDrives = new();

        // Battery
        [ObservableProperty]
        private bool _isBatteryPresent;

        [ObservableProperty]
        private string _batteryStatusText = "Desktop PC / AC Power";

        [ObservableProperty]
        private string _batteryChargeText = "100% (AC Direct)";

        [ObservableProperty]
        private string _batteryWearLevelText = "N/A (Desktop)";

        // Fan Sensors & Cooling
        [ObservableProperty]
        private ObservableCollection<FanSensorData> _fanSensors = new();

        [ObservableProperty]
        private bool _hasFansDetected;

        [ObservableProperty]
        private string _primaryFanSpeedText = "N/A (Passive Cooling)";

        [ObservableProperty]
        private ObservableCollection<DriverInfo> _driverList = new();

        public DashboardViewModel(ITweakService tweakService, IHardwareMonitorService? hardwareMonitorService = null, IDialogService? dialogService = null, IOptimizationProfileService? profileService = null, IGameDetectionService? gameDetectionService = null, ITroubleshootService? troubleshootService = null, IDriverScannerService? driverScannerService = null)
        {
            _tweakService = tweakService;
            _hardwareMonitorService = hardwareMonitorService ?? new HardwareMonitorService();
            _dialogService = dialogService ?? new DialogService();
            _profileService = profileService ?? new OptimizationProfileService(_tweakService);
            _gameDetectionService = gameDetectionService ?? new GameDetectionService(_profileService);
            _troubleshootService = troubleshootService ?? new TroubleshootService();
            _driverScannerService = driverScannerService ?? new DriverScannerService();

            _gameDetectionService.Start();
            _ = ScanDriversAsync();

            _hardwareMonitorService.Initialize();
            LoadStats();

            _autoRamTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(30)
            };
            _autoRamTimer.Tick += (s, e) => _tweakService.ClearStandbyMemory();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.5)
            };
            _timer.Tick += async (s, e) => await UpdateSensorsAsync();
            _timer.Start();

            // Initial immediate update
            _ = UpdateSensorsAsync();
        }

        [RelayCommand]
        private async Task ToggleGameModeAsync()
        {
            if (!_profileService.IsGameModeActive && !LicenseService.Instance.IsVipOrOwner)
            {
                await _dialogService.ShowMessageAsync(
                    "🔒 FITUR EKSKLUSIF VIP MEMBER",
                    "One-Click Game Mode adalah fitur eksklusif untuk VIP Member dan Owner.\n\nFitur ini secara otomatis mematikan proses background berat, membersihkan standby memory, dan mengaktifkan mode prioritas tinggi GPU/CPU. Silakan aktivasi VIP Key untuk menggunakan fitur ini!"
                );
                return;
            }

            if (_profileService.IsGameModeActive)
            {
                bool success = await _profileService.DisableGameModeAsync();
                if (success)
                {
                    await _dialogService.ShowMessageAsync("Game Mode Off", "Game Mode telah dinonaktifkan. Pengaturan sistem dikembalikan ke normal.");
                }
            }
            else
            {
                bool success = await _profileService.EnableGameModeAsync();
                if (success)
                {
                    await _dialogService.ShowMessageAsync("Game Mode ON 🚀", $"Game Mode Aktif! {_profileService.KilledProcessCount} background process di-kill & CPU/GPU Boost diaktifkan.");
                }
            }
            LoadStats();
        }

        [RelayCommand]
        private async Task SelectProfileAsync(string profileName)
        {
            if (!string.Equals(profileName, "Standard", StringComparison.OrdinalIgnoreCase) && !LicenseService.Instance.IsVipOrOwner)
            {
                await _dialogService.ShowMessageAsync(
                    "🔒 FITUR EKSKLUSIF VIP MEMBER",
                    $"Profil Performa '{profileName}' adalah fitur eksklusif untuk VIP Member dan Owner.\n\nAkun Free User hanya dapat menggunakan profil Standard (Default). Silakan aktivasi VIP Key untuk membuka seluruh profil optimasi!"
                );
                return;
            }

            if (Enum.TryParse<PerformanceProfile>(profileName, out var profile))
            {
                await _profileService.ApplyProfileAsync(profile);
                await _dialogService.ShowMessageAsync("Profile Applied", $"Profil performa '{profileName}' telah diterapkan!");
                LoadStats();
            }
        }

        [RelayCommand]
        private async Task RunFullSystemFixAsync()
        {
            if (!LicenseService.Instance.IsVipOrOwner)
            {
                await _dialogService.ShowMessageAsync(
                    "🔒 FITUR EKSKLUSIF VIP MEMBER",
                    "Full System Troubleshoot & Fix adalah fitur eksklusif untuk VIP Member dan Owner.\n\nSilakan aktivasi VIP Key untuk melakukan perbaikan otomatis pada jaringan, DNS, Winsock, dan Windows Update!"
                );
                return;
            }

            bool success = await _troubleshootService.PerformFullSystemFixAsync();
            if (success)
            {
                await _dialogService.ShowMessageAsync("System Fix Complete 🔧", "Berhasil mereset Winsock, TCP/IP, Flush DNS, dan cache Windows Update! Koneksi dan sistem kembali prima.");
            }
            else
            {
                await _dialogService.ShowMessageAsync("Troubleshoot Warning", "Beberapa langkah perbaikan memerlukan hak akses Administrator.");
            }
        }

        private async Task ScanDriversAsync()
        {
            try
            {
                DriverList.Clear();
                var drivers = await _driverScannerService.ScanDriversAsync();
                foreach (var d in drivers)
                {
                    DriverList.Add(d);
                }
            }
            catch { }
        }

        [RelayCommand]
        private async Task ClearStandbyMemoryAsync()
        {
            if (!LicenseService.Instance.IsVipOrOwner)
            {
                await _dialogService.ShowMessageAsync(
                    "🔒 FITUR EKSKLUSIF VIP MEMBER",
                    "Standby Memory Cleaner adalah fitur eksklusif untuk VIP Member dan Owner.\n\nSilakan aktivasi VIP Key untuk membebaskan alokasi RAM cache secara instan!"
                );
                return;
            }

            bool result = _tweakService.ClearStandbyMemory();
            if (result)
            {
                await _dialogService.ShowMessageAsync("Standby Memory Cleared", "Berhasil membersihkan Standby List RAM dan membebaskan alokasi memori aktif!");
                _ = UpdateSensorsAsync();
            }
            else
            {
                await _dialogService.ShowMessageAsync("Execution Failed", "Gagal membersihkan Standby Memory.");
            }
        }

        public void LoadStats()
        {
            var logs = _tweakService.GetTweakHistory();
            TotalTweaksApplied = logs.Count(l => !l.IsReverted);
            TotalRestorePoints = _tweakService.GetRestorePoints().Count;
        }

        private async Task UpdateSensorsAsync()
        {
            try
            {
                var snapshot = await _hardwareMonitorService.ReadMetricsAsync();

                // CPU
                CpuName = string.IsNullOrWhiteSpace(snapshot.Cpu.Name) ? "Processor" : snapshot.Cpu.Name;
                CpuTempText = snapshot.Cpu.Temperature > 0 ? $"{snapshot.Cpu.Temperature} °C" : "N/A";
                CpuUsageText = $"{snapshot.Cpu.UsagePercent}%";
                CpuPowerText = snapshot.Cpu.PowerWatts > 0 ? $"{snapshot.Cpu.PowerWatts} W" : "N/A";

                CpuCoreClocks.Clear();
                foreach (var clock in snapshot.Cpu.CoreClocks)
                {
                    CpuCoreClocks.Add(clock);
                }

                // GPU
                DetectedGpus.Clear();
                if (snapshot.Gpus.Count > 0)
                {
                    foreach (var gpu in snapshot.Gpus)
                    {
                        DetectedGpus.Add(gpu);
                    }
                }
                else
                {
                    DetectedGpus.Add(new GpuSensorData
                    {
                        Name = "Integrated / Standard Display",
                        Vendor = "Other",
                        IsDiscrete = false,
                        IsActive = true
                    });
                }

                var primaryGpu = snapshot.Gpus.FirstOrDefault(g => g.IsDiscrete) ?? snapshot.Gpus.FirstOrDefault();
                if (primaryGpu != null)
                {
                    GpuName = primaryGpu.Name;
                    GpuTempText = primaryGpu.Temperature > 0 ? $"{primaryGpu.Temperature} °C" : "N/A";
                    GpuUsageText = $"{primaryGpu.UsagePercent}%";
                    GpuPowerText = primaryGpu.PowerWatts > 0 ? $"{primaryGpu.PowerWatts} W" : "N/A";
                    GpuVramText = primaryGpu.TotalVramGB > 0 ? $"{primaryGpu.UsedVramGB} / {primaryGpu.TotalVramGB} GB" : "N/A";

                    if (snapshot.Gpus.Count > 1)
                    {
                        var activeGpu = snapshot.Gpus.FirstOrDefault(g => g.IsActive);
                        GpuHybridStatus = activeGpu != null ? $"Hybrid System ({activeGpu.Name} Active)" : "Hybrid System";
                    }
                    else
                    {
                        GpuHybridStatus = "Single GPU";
                    }
                }
                else
                {
                    GpuName = "Integrated / Standard Display";
                }

                // RAM
                RamUsageText = snapshot.Ram.TotalGB > 0 ? $"{snapshot.Ram.UsedGB} / {snapshot.Ram.TotalGB} GB" : "N/A";
                RamUsagePercentText = $"{snapshot.Ram.UsagePercent}%";
                RamSpeedText = snapshot.Ram.SpeedMHz > 0 ? $"{snapshot.Ram.SpeedMHz} MHz" : "N/A";

                // Storage
                StorageDrives.Clear();
                foreach (var drive in snapshot.StorageDrives)
                {
                    StorageDrives.Add(drive);
                }

                // Battery
                IsBatteryPresent = snapshot.Battery.IsPresent;
                if (IsBatteryPresent)
                {
                    BatteryStatusText = "Laptop Battery Connected";
                    BatteryChargeText = $"{snapshot.Battery.ChargePercent}%";
                    BatteryWearLevelText = snapshot.Battery.WearLevelPercent > 0 ? $"{snapshot.Battery.WearLevelPercent}%" : "100%";
                }
                else
                {
                    BatteryStatusText = "Desktop PC (AC Power Direct)";
                    BatteryChargeText = "100% (AC Direct)";
                    BatteryWearLevelText = "N/A (Desktop)";
                }

                // Fans & Cooling
                FanSensors.Clear();
                if (snapshot.Fans.Count > 0)
                {
                    HasFansDetected = true;
                    foreach (var fan in snapshot.Fans)
                    {
                        FanSensors.Add(fan);
                    }
                    var mainFan = snapshot.Fans.FirstOrDefault(f => f.SpeedRpm > 0) ?? snapshot.Fans.First();
                    PrimaryFanSpeedText = mainFan.SpeedRpm > 0
                        ? $"{mainFan.SpeedRpm:F0} RPM ({mainFan.Name})"
                        : "Vendor EC Protected (Oem Auto)";
                }
                else
                {
                    HasFansDetected = false;
                    PrimaryFanSpeedText = "N/A / Uncontrolled";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dashboard update sensors error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            LoadStats();
            _ = UpdateSensorsAsync();
        }
    }
}
