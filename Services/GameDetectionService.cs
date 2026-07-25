using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinTweakStudio.Services
{
    public interface IGameDetectionService
    {
        bool IsAutoDetectEnabled { get; set; }
        bool IsGameRunning { get; }
        string DetectedGameName { get; }
        event EventHandler<string>? GameDetected;
        event EventHandler? GameClosed;
        void Start();
        void Stop();
    }

    public class GameDetectionService : ObservableObject, IGameDetectionService
    {
        private readonly IOptimizationProfileService _profileService;
        private readonly DispatcherTimer _timer;

        private static readonly string[] KnownGameExecutableNames = new[]
        {
            "cs2",
            "csgo",
            "valorant-win64-shipping",
            "VALORANT",
            "gta5",
            "GTA5",
            "RDR2",
            "r5apex",
            "dota2",
            "tslgame",
            "javaw",
            "Minecraft",
            "RobloxPlayerBeta",
            "FortniteClient-Win64-Shipping",
            "ModernWarfare2",
            "ModernWarfare3",
            "Overwatch",
            "GenshinImpact",
            "StarRail",
            "ZenlessZoneZero",
            "PUBG",
            "League of Legends",
            "LeagueClient"
        };

        private bool _isAutoDetectEnabled = true;
        public bool IsAutoDetectEnabled
        {
            get => _isAutoDetectEnabled;
            set => SetProperty(ref _isAutoDetectEnabled, value);
        }

        private bool _isGameRunning;
        public bool IsGameRunning
        {
            get => _isGameRunning;
            private set => SetProperty(ref _isGameRunning, value);
        }

        private string _detectedGameName = string.Empty;
        public string DetectedGameName
        {
            get => _detectedGameName;
            private set => SetProperty(ref _detectedGameName, value);
        }

        public event EventHandler<string>? GameDetected;
        public event EventHandler? GameClosed;

        public GameDetectionService(IOptimizationProfileService profileService)
        {
            _profileService = profileService;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _timer.Tick += async (s, e) => await CheckRunningGamesAsync();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private async Task CheckRunningGamesAsync()
        {
            if (!IsAutoDetectEnabled) return;

            await Task.Run(async () =>
            {
                try
                {
                    string? foundGame = null;
                    var allProcesses = Process.GetProcesses();

                    foreach (var procName in KnownGameExecutableNames)
                    {
                        if (allProcesses.Any(p => string.Equals(p.ProcessName, procName, StringComparison.OrdinalIgnoreCase)))
                        {
                            foundGame = procName;
                            break;
                        }
                    }

                    if (foundGame != null)
                    {
                        if (!IsGameRunning || DetectedGameName != foundGame)
                        {
                            IsGameRunning = true;
                            DetectedGameName = foundGame;
                            GameDetected?.Invoke(this, foundGame);

                            // Auto Enable Game Mode Booster!
                            if (!_profileService.IsGameModeActive)
                            {
                                await _profileService.EnableGameModeAsync();
                            }
                        }
                    }
                    else
                    {
                        if (IsGameRunning)
                        {
                            IsGameRunning = false;
                            DetectedGameName = string.Empty;
                            GameClosed?.Invoke(this, EventArgs.Empty);

                            // Auto Disable Game Mode Booster when game closes
                            if (_profileService.IsGameModeActive)
                            {
                                await _profileService.DisableGameModeAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GameDetection Error: {ex.Message}");
                }
            });
        }
    }
}
