using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using WinTweakStudio.Models;

namespace WinTweakStudio.Services
{
    public enum PerformanceProfile
    {
        Standard,
        Gaming,
        Streaming,
        Work,
        BatterySaver
    }

    public interface IOptimizationProfileService
    {
        bool IsGameModeActive { get; }
        PerformanceProfile CurrentProfile { get; }
        int KilledProcessCount { get; }
        Task<bool> EnableGameModeAsync();
        Task<bool> DisableGameModeAsync();
        Task<bool> ApplyProfileAsync(PerformanceProfile profile);
    }

    public class OptimizationProfileService : ObservableObject, IOptimizationProfileService
    {
        private readonly ITweakService _tweakService;
        private readonly ISoundEffectService _soundService;

        private static readonly string[] AppsToKillInGameMode = new[]
        {
            "OneDrive",
            "SearchHost",
            "GameBarPresenceWriter",
            "WidgetService",
            "XboxGameSave"
        };

        private bool _isGameModeActive;
        public bool IsGameModeActive
        {
            get => _isGameModeActive;
            private set => SetProperty(ref _isGameModeActive, value);
        }

        private PerformanceProfile _currentProfile = PerformanceProfile.Standard;
        public PerformanceProfile CurrentProfile
        {
            get => _currentProfile;
            private set => SetProperty(ref _currentProfile, value);
        }

        private int _killedProcessCount;
        public int KilledProcessCount
        {
            get => _killedProcessCount;
            private set => SetProperty(ref _killedProcessCount, value);
        }

        public OptimizationProfileService(ITweakService tweakService, ISoundEffectService? soundService = null)
        {
            _tweakService = tweakService;
            _soundService = soundService ?? new SoundEffectService();
        }

        private async Task ApplyTweakByIdAsync(string tweakId)
        {
            foreach (TweakCategory cat in Enum.GetValues(typeof(TweakCategory)))
            {
                var tweaks = _tweakService.GetTweaksByCategory(cat);
                var tweak = tweaks.FirstOrDefault(t => t.Id == tweakId);
                if (tweak != null)
                {
                    await _tweakService.ApplyTweakAsync(tweak);
                    break;
                }
            }
        }

        private async Task RevertTweakByIdAsync(string tweakId)
        {
            foreach (TweakCategory cat in Enum.GetValues(typeof(TweakCategory)))
            {
                var tweaks = _tweakService.GetTweaksByCategory(cat);
                var tweak = tweaks.FirstOrDefault(t => t.Id == tweakId);
                if (tweak != null)
                {
                    await _tweakService.RevertTweakAsync(tweak);
                    break;
                }
            }
        }

        public async Task<bool> EnableGameModeAsync()
        {
            try
            {
                int killed = 0;
                foreach (var appName in AppsToKillInGameMode)
                {
                    var processes = Process.GetProcessesByName(appName.Trim());
                    foreach (var proc in processes)
                    {
                        try
                        {
                            proc.Kill();
                            killed++;
                        }
                        catch { }
                    }
                }
                KilledProcessCount = killed;

                // Apply CPU & GPU High Priority registry tweaks via TweakService
                await ApplyTweakByIdAsync("GPU-GEN-03"); // GPU Priority Games
                await ApplyTweakByIdAsync("GPU-GEN-04"); // Scheduling Category Games
                await ApplyTweakByIdAsync("GPU-GEN-05"); // System Responsiveness
                await ApplyTweakByIdAsync("CPU-SCH-06"); // Win32PrioritySeparation 26
                await ApplyTweakByIdAsync("CPU-PWR-01"); // Ultimate Performance Power Plan
                _tweakService.ClearStandbyMemory();

                IsGameModeActive = true;
                CurrentProfile = PerformanceProfile.Gaming;
                _soundService.PlayBoostOn();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnableGameMode Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DisableGameModeAsync()
        {
            try
            {
                await RevertTweakByIdAsync("GPU-GEN-03");
                await RevertTweakByIdAsync("GPU-GEN-04");
                await RevertTweakByIdAsync("GPU-GEN-05");
                await RevertTweakByIdAsync("CPU-SCH-06");

                IsGameModeActive = false;
                CurrentProfile = PerformanceProfile.Standard;
                _soundService.PlayBoostOff();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DisableGameMode Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ApplyProfileAsync(PerformanceProfile profile)
        {
            switch (profile)
            {
                case PerformanceProfile.Gaming:
                    return await EnableGameModeAsync();

                case PerformanceProfile.Streaming:
                    await ApplyTweakByIdAsync("GPU-GEN-03");
                    await ApplyTweakByIdAsync("GPU-GEN-06"); // Network Throttling Disable
                    _tweakService.ClearStandbyMemory();
                    IsGameModeActive = false;
                    CurrentProfile = PerformanceProfile.Streaming;
                    _soundService.PlayProfileSwitch();
                    return true;

                case PerformanceProfile.Work:
                    await Task.Run(() =>
                    {
                        RunCmd("powercfg /setactive 381b4222-f694-41f0-9685-ff5bb260df2e"); // Balanced Plan
                        // Expose & set CPU Boost Mode to Efficient Enabled / Disabled for stable idle
                        RunCmd("powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 be337238-0d82-4146-a960-4f3749d470c7 3"); // Efficient
                        RunCmd("powercfg /setdcvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 be337238-0d82-4146-a960-4f3749d470c7 3");
                        RunCmd("powercfg /setactive SCHEME_CURRENT");
                    });
                    _tweakService.ClearStandbyMemory();
                    IsGameModeActive = false;
                    CurrentProfile = PerformanceProfile.Work;
                    _soundService.PlayProfileSwitch();
                    return true;

                case PerformanceProfile.BatterySaver:
                    await Task.Run(() =>
                    {
                        RunCmd("powercfg /setactive a184414f-3706-4750-b92c-69f44a1df769"); // Power Saver Plan
                        // Completely Disable CPU Aggressive Boost for dead quiet & low clock
                        RunCmd("powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 be337238-0d82-4146-a960-4f3749d470c7 0"); // Disabled
                        RunCmd("powercfg /setdcvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 be337238-0d82-4146-a960-4f3749d470c7 0");
                        // Set Minimum CPU State to 5% & Max CPU State to 50% to force Ryzen down to ~1.5 - 2.2 GHz
                        RunCmd("powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 893de804-4530-4635-8d6e-0e95c2975854 5"); // Min 5%
                        RunCmd("powercfg /setdcvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 893de804-4530-4635-8d6e-0e95c2975854 5");
                        RunCmd("powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc50be8b-7073-4ea8-8e5c-920140d7428a 50"); // Max 50%
                        RunCmd("powercfg /setdcvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc50be8b-7073-4ea8-8e5c-920140d7428a 50");
                        RunCmd("powercfg /setactive SCHEME_CURRENT");
                    });
                    await DisableGameModeAsync();
                    CurrentProfile = PerformanceProfile.BatterySaver;
                    _soundService.PlayProfileSwitch();
                    return true;

                default:
                    await DisableGameModeAsync();
                    return true;
            }
        }

        private void RunCmd(string command)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(3000);
            }
            catch { }
        }
    }
}
