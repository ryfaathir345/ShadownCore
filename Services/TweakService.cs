using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinTweakStudio.Data;
using WinTweakStudio.Models;

namespace WinTweakStudio.Services
{
    public class TweakService : ITweakService
    {
        private readonly INvidiaTweakService _nvidiaTweakService;
        private readonly IAmdTweakService _amdTweakService;

        public TweakService(INvidiaTweakService? nvidiaTweakService = null, IAmdTweakService? amdTweakService = null)
        {
            _nvidiaTweakService = nvidiaTweakService ?? new NvidiaTweakService();
            _amdTweakService = amdTweakService ?? new AmdTweakService();
        }

        public List<TweakDefinition> GetTweaksByCategory(TweakCategory category)
        {
            var tweaks = DatabaseInitializer.GetTweakDefinitionsByCategory(category);
            if (tweaks.Count == 0)
            {
                tweaks = GetSampleTweaks().Where(t => t.Category == category).ToList();
            }

            var logs = DatabaseInitializer.GetAllTweakLogs();
            foreach (var tweak in tweaks)
            {
                tweak.IsApplied = CheckIsApplied(tweak, logs);
            }
            return tweaks;
        }

        public async Task<bool> ApplyTweakAsync(TweakDefinition tweak)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (tweak.Type == TweakType.Guidance)
                    {
                        // Guidance tweaks do NOT modify system settings and do NOT write to TweakLogs
                        return true;
                    }

                    string oldValue = GetCurrentValue(tweak);
                    long restorePointId = DatabaseInitializer.GetOrCreateActiveRestorePointId();

                    // Step 1: ATOMIC WRITE to TweakLogs BEFORE applying tweak
                    var log = new TweakLog
                    {
                        RestorePointId = restorePointId,
                        TweakName = tweak.Name,
                        Category = tweak.Category.ToString(),
                        Type = tweak.Type.ToString(),
                        TargetPath = tweak.TargetPath,
                        OldValue = oldValue,
                        NewValue = tweak.RecommendedValue,
                        AppliedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        IsReverted = false
                    };

                    DatabaseInitializer.LogTweakApplicationAtomic(log);

                    // Step 2: Apply System Change
                    bool success = ExecuteApply(tweak);
                    if (success)
                    {
                        tweak.IsApplied = true;
                    }
                    return success;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error applying tweak {tweak.Name}: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> RevertTweakAsync(TweakDefinition tweak)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var logs = DatabaseInitializer.GetAllTweakLogs()
                        .Where(l => l.TweakName == tweak.Name && !l.IsReverted)
                        .OrderByDescending(l => l.Id)
                        .ToList();

                    var lastLog = logs.FirstOrDefault();
                    string targetOldValue = lastLog != null ? lastLog.OldValue : tweak.DefaultValue;

                    bool success = ExecuteRevert(tweak, targetOldValue);
                    if (success)
                    {
                        tweak.IsApplied = false;
                        if (lastLog != null)
                        {
                            DatabaseInitializer.MarkLogReverted(lastLog.Id, true);
                        }
                    }
                    return success;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reverting tweak {tweak.Name}: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<bool> UndoLogAsync(TweakLog log)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var tweakDef = GetSampleTweaks().FirstOrDefault(t => t.Name == log.TweakName) ?? new TweakDefinition
                    {
                        Name = log.TweakName,
                        TargetPath = log.TargetPath,
                        Type = Enum.TryParse<TweakType>(log.Type, out var t) ? t : TweakType.Registry
                    };

                    bool success = ExecuteRevert(tweakDef, log.OldValue);
                    if (success)
                    {
                        DatabaseInitializer.MarkLogReverted(log.Id, true);
                    }
                    return success;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> RestoreAllToPointAsync(long restorePointId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var logs = DatabaseInitializer.GetAllTweakLogs()
                        .Where(l => l.RestorePointId == restorePointId && !l.IsReverted)
                        .OrderByDescending(l => l.Id)
                        .ToList();

                    foreach (var log in logs)
                    {
                        var tweakDef = GetSampleTweaks().FirstOrDefault(t => t.Name == log.TweakName) ?? new TweakDefinition
                        {
                            Name = log.TweakName,
                            TargetPath = log.TargetPath,
                            Type = Enum.TryParse<TweakType>(log.Type, out var t) ? t : TweakType.Registry
                        };

                        if (ExecuteRevert(tweakDef, log.OldValue))
                        {
                            DatabaseInitializer.MarkLogReverted(log.Id, true);
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public List<TweakLog> GetTweakHistory()
        {
            return DatabaseInitializer.GetAllTweakLogs();
        }

        public List<RestorePoint> GetRestorePoints()
        {
            return DatabaseInitializer.GetAllRestorePoints();
        }

        public long CreateRestorePoint(string label)
        {
            return DatabaseInitializer.CreateRestorePoint(label);
        }

        private bool CheckIsApplied(TweakDefinition tweak, List<TweakLog> logs)
        {
            if (tweak.Type == TweakType.Guidance)
            {
                return false;
            }

            // 1. Check if TweakLogs has an active (un-reverted) log entry for this tweak
            var activeLog = logs.FirstOrDefault(l => string.Equals(l.TweakName, tweak.Name, StringComparison.OrdinalIgnoreCase) && !l.IsReverted);
            if (activeLog != null)
            {
                return true;
            }

            // 2. Check if current system setting matches RecommendedValue
            string currentVal = GetCurrentValue(tweak);
            if (!string.IsNullOrEmpty(currentVal) && string.Equals(currentVal, tweak.RecommendedValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private string GetCurrentValue(TweakDefinition tweak)
        {
            try
            {
                if (tweak.Type == TweakType.Registry)
                {
                    return ReadRegistryValue(tweak.TargetPath, tweak.ValueName);
                }
                else if (tweak.Type == TweakType.Service)
                {
                    return GetServiceStatus(tweak.TargetPath);
                }
                else if (tweak.Type == TweakType.NvApi)
                {
                    return _nvidiaTweakService.GetSettingValueByName(tweak.TargetPath);
                }
                else if (tweak.Type == TweakType.Adl)
                {
                    return _amdTweakService.GetSettingValue(tweak.TargetPath);
                }
            }
            catch { }

            return tweak.DefaultValue;
        }

        private bool ExecuteApply(TweakDefinition tweak)
        {
            try
            {
                if (tweak.Type == TweakType.Registry)
                {
                    string targetVal = tweak.RecommendedValue;
                    if (string.Equals(tweak.ValueName, "SvcHostSplitThresholdInKB", StringComparison.OrdinalIgnoreCase))
                    {
                        targetVal = GetAutoSvcHostSplitThresholdKb().ToString();
                    }
                    return WriteRegistryValue(tweak.TargetPath, tweak.ValueName, targetVal);
                }
                else if (tweak.Type == TweakType.Service)
                {
                    return SetServiceStartupType(tweak.TargetPath, tweak.RecommendedValue);
                }
                else if (tweak.Type == TweakType.Command)
                {
                    if (tweak.TargetPath == "ClearStandbyList" || tweak.Name.Contains("Standby Memory", StringComparison.OrdinalIgnoreCase))
                    {
                        return ClearStandbyMemory();
                    }
                    return RunCommand(tweak.TargetPath);
                }
                else if (tweak.Type == TweakType.PowerShell)
                {
                    return RunPowerShellCommand(tweak.TargetPath);
                }
                else if (tweak.Type == TweakType.NvApi)
                {
                    return _nvidiaTweakService.SetSettingValueByName(tweak.TargetPath, tweak.RecommendedValue);
                }
                else if (tweak.Type == TweakType.Adl)
                {
                    return _amdTweakService.SetSettingValue(tweak.TargetPath, tweak.RecommendedValue);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ExecuteRevert(TweakDefinition tweak, string targetValue)
        {
            try
            {
                if (tweak.Type == TweakType.Registry)
                {
                    return WriteRegistryValue(tweak.TargetPath, tweak.ValueName, targetValue);
                }
                else if (tweak.Type == TweakType.Service)
                {
                    return SetServiceStartupType(tweak.TargetPath, targetValue);
                }
                else if (tweak.Type == TweakType.Command)
                {
                    return RunCommand(targetValue);
                }
                else if (tweak.Type == TweakType.PowerShell)
                {
                    return RunPowerShellCommand(tweak.DefaultValue);
                }
                else if (tweak.Type == TweakType.NvApi)
                {
                    return _nvidiaTweakService.SetSettingValueByName(tweak.TargetPath, targetValue);
                }
                else if (tweak.Type == TweakType.Adl)
                {
                    return _amdTweakService.SetSettingValue(tweak.TargetPath, targetValue);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwnd);

        public bool ClearStandbyMemory()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var procs = System.Diagnostics.Process.GetProcesses();
                foreach (var proc in procs)
                {
                    try
                    {
                        if (!proc.HasExited)
                        {
                            EmptyWorkingSet(proc.Handle);
                        }
                    }
                    catch { }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private uint GetAutoSvcHostSplitThresholdKb()
        {
            try
            {
                var gcMemoryInfo = GC.GetGCMemoryInfo();
                long totalBytes = gcMemoryInfo.TotalAvailableMemoryBytes;
                if (totalBytes > 0)
                {
                    return (uint)(totalBytes / 1024);
                }
            }
            catch { }

            return 16777216; // 16GB default in KB
        }

        private bool RunPowerShellCommand(string psCommand)
        {
            if (string.IsNullOrWhiteSpace(psCommand) || psCommand == "-") return true;
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                bool exited = proc?.WaitForExit(5000) ?? false;
                if (!exited)
                {
                    try { proc?.Kill(); } catch { }
                    return true;
                }
                return proc?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private bool RunCommand(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine) || commandLine == "-") return true;
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {commandLine}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                bool exited = proc?.WaitForExit(5000) ?? false;
                if (!exited)
                {
                    try { proc?.Kill(); } catch { }
                    return true;
                }
                return proc?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private string ReadRegistryValue(string path, string valueName)
        {
            var (rootKey, subKeyPath) = ParseRegistryPath(path);
            if (rootKey == null) return string.Empty;

            using var key = rootKey.OpenSubKey(subKeyPath, false);
            var val = key?.GetValue(valueName);
            return val?.ToString() ?? string.Empty;
        }

        private bool WriteRegistryValue(string path, string valueName, string value)
        {
            var (rootKey, subKeyPath) = ParseRegistryPath(path);
            if (rootKey == null) return false;

            using var key = rootKey.CreateSubKey(subKeyPath, true);
            if (key == null) return false;

            if (int.TryParse(value, out int intVal))
            {
                key.SetValue(valueName, intVal, RegistryValueKind.DWord);
            }
            else
            {
                key.SetValue(valueName, value, RegistryValueKind.String);
            }
            return true;
        }

        private (RegistryKey? rootKey, string subKeyPath) ParseRegistryPath(string fullPath)
        {
            if (fullPath.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase) || fullPath.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase))
            {
                int index = fullPath.IndexOf('\\');
                return (Registry.CurrentUser, fullPath.Substring(index + 1));
            }
            if (fullPath.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase) || fullPath.StartsWith("HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase))
            {
                int index = fullPath.IndexOf('\\');
                return (Registry.LocalMachine, fullPath.Substring(index + 1));
            }
            return (null, string.Empty);
        }

        private string GetServiceStatus(string serviceName)
        {
            try
            {
                var (rootKey, subKeyPath) = ParseRegistryPath(@"HKLM\SYSTEM\CurrentControlSet\Services\" + serviceName);
                if (rootKey != null)
                {
                    using var key = rootKey.OpenSubKey(subKeyPath, false);
                    var val = key?.GetValue("Start");
                    if (val != null) return val.ToString() ?? "4";
                }

                using var sc = new ServiceController(serviceName);
                return ((int)sc.StartType).ToString();
            }
            catch
            {
                return "4";
            }
        }

        private bool SetServiceStartupType(string serviceName, string startupType)
        {
            try
            {
                string targetVal = "4";
                if (int.TryParse(startupType, out int val))
                {
                    targetVal = val.ToString();
                }
                else if (startupType.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                {
                    targetVal = "4";
                }
                else if (startupType.Equals("Automatic", StringComparison.OrdinalIgnoreCase))
                {
                    targetVal = "2";
                }
                else if (startupType.Equals("Manual", StringComparison.OrdinalIgnoreCase))
                {
                    targetVal = "3";
                }

                return WriteRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\" + serviceName, "Start", targetVal);
            }
            catch
            {
                return false;
            }
        }

        private List<TweakDefinition> GetSampleTweaks()
        {
            return new List<TweakDefinition>
            {
                // GPU
                new TweakDefinition
                {
                    Id = "GPU-01",
                    Name = "Hardware-Accelerated GPU Scheduling (HAGS)",
                    Description = "Reduce latency and improve performance by allowing GPU to manage its own VRAM.",
                    Category = TweakCategory.GPU,
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                    ValueName = "HwSchMode",
                    DefaultValue = "1",
                    RecommendedValue = "2"
                },
                new TweakDefinition
                {
                    Id = "GPU-02",
                    Name = "Disable GPU Energy Driver",
                    Description = "Disables power saving limits on discrete GPU for maximum clock stability.",
                    Category = TweakCategory.GPU,
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm",
                    ValueName = "DisablePowerSavings",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },

                // CPU
                new TweakDefinition
                {
                    Id = "CPU-01",
                    Name = "Disable CPU Core Parking",
                    Description = "Prevents CPU cores from entering deep sleep states to maintain peak responsiveness.",
                    Category = TweakCategory.CPU,
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-825c-42af-8a9d-630d4a7f2c00\0cc5b647-c1df-4596-92c0-d5c741e79860",
                    ValueName = "Attributes",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "CPU-02",
                    Name = "Disable Intel TSX / Spectre Mitigations",
                    Description = "Disables CPU speculative execution mitigations for up to 10% IPC performance boost.",
                    Category = TweakCategory.CPU,
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    ValueName = "FeatureSettingsOverride",
                    DefaultValue = "0",
                    RecommendedValue = "3"
                },

                // RAM
                new TweakDefinition
                {
                    Id = "RAM-01",
                    Name = "Clear PageFile on Shutdown",
                    Description = "Wipes virtual memory file on power down for enhanced privacy and memory cleanliness.",
                    Category = TweakCategory.RAM,
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    ValueName = "ClearPageFileAtShutdown",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "RAM-02",
                    Name = "Disable SysMain (Superfetch)",
                    Description = "Stops constant disk caching and RAM pre-loading for smoother gaming performance.",
                    Category = TweakCategory.RAM,
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Service,
                    TargetPath = "SysMain",
                    DefaultValue = "Automatic",
                    RecommendedValue = "Disabled"
                },

                // Network
                new TweakDefinition
                {
                    Id = "NET-01",
                    Name = "Disable Network Throttling Index",
                    Description = "Disables Windows default network bandwidth throttling for non-multimedia traffic.",
                    Category = TweakCategory.Network,
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    ValueName = "NetworkThrottlingIndex",
                    DefaultValue = "10",
                    RecommendedValue = "ffffffff"
                },

                // Windows
                new TweakDefinition
                {
                    Id = "WIN-01",
                    Name = "Disable Telemetry & Data Collection",
                    Description = "Turns off background telemetry, diagnostic uploads, and feedback requests.",
                    Category = TweakCategory.Windows,
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                    ValueName = "AllowTelemetry",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-02",
                    Name = "Disable Windows Defender Realtime Shield",
                    Description = "Disables Defender background antivirus scanning. Requires third-party protection.",
                    Category = TweakCategory.Windows,
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows Defender",
                    ValueName = "DisableAntiSpyware",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },

                // Service
                new TweakDefinition
                {
                    Id = "SVC-01",
                    Name = "Disable Connected User Experiences (DiagTrack)",
                    Description = "Stops tracking background service logging system usage.",
                    Category = TweakCategory.Service,
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "DiagTrack",
                    DefaultValue = "Automatic",
                    RecommendedValue = "Disabled"
                },

                // Debloat
                new TweakDefinition
                {
                    Id = "DEB-01",
                    Name = "Remove Xbox Game Bar & Services",
                    Description = "Disables background Xbox DVR recording and telemetry overlay services.",
                    Category = TweakCategory.Debloat,
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
                    ValueName = "AppCaptureEnabled",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },

                // Storage
                new TweakDefinition
                {
                    Id = "STO-01",
                    Name = "Disable NTFS Last Access Update",
                    Description = "Disables writing timestamp metadata to drive every time a file is accessed.",
                    Category = TweakCategory.Storage,
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\FileSystem",
                    ValueName = "NtfsDisableLastAccessUpdate",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },

                // BootPower
                new TweakDefinition
                {
                    Id = "PWR-01",
                    Name = "Enable Ultimate Performance Power Plan",
                    Description = "Unlocks hidden Windows Ultimate Performance power plan preset.",
                    Category = TweakCategory.BootPower,
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power",
                    ValueName = "HibernateEnabled",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                }
            };
        }
    }
}
