using System;
using System.Diagnostics;
using DiscordRPC;
using DiscordRPC.Logging;

namespace WinTweakStudio.Services
{
    public interface IDiscordService
    {
        void Initialize();
        void SetStatus(string state, string details);
        void UpdateOptimizationStats(int tweaksApplied, string activeCategory);
        void Shutdown();
        bool IsEnabled { get; set; }
    }

    public class DiscordService : IDiscordService
    {
        private DiscordRpcClient? _client;
        private bool _isEnabled = true;
        private readonly DateTime _startTime = DateTime.UtcNow;
        private string _lastState = "Monitoring Windows System ⚡";
        private string _lastDetails = "WinTweakStudio v1.0 - PC Optimizer";
        private System.Threading.Timer? _heartbeatTimer;

        // Default Client ID untuk WinTweakStudio.
        // TIP UNTUK MONETISASI & MARKETING DISCORD:
        // 1. Buka https://discord.com/developers/applications dan buat New Application bernama "WinTweakStudio".
        // 2. Copy "Application ID" dan salin ke dalam string DefaultClientId di bawah ini.
        // 3. Masuk ke menu "Rich Presence -> Art Assets", upload logo aplikasi dan beri nama kunci "app_icon" & "verified".
        private const string DefaultClientId = "1530888658968641637";

        public static DiscordService Instance { get; } = new DiscordService();

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (_isEnabled)
                {
                    Initialize();
                }
                else
                {
                    Shutdown();
                }
            }
        }

        public DiscordService()
        {
        }

        public void Initialize()
        {
            if (!_isEnabled) return;
            if (_client != null && !_client.IsDisposed) return;

            try
            {
                _client = new DiscordRpcClient(DefaultClientId);
                _client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

                _client.OnReady += (sender, e) =>
                {
                    Debug.WriteLine($"[Discord RPC] Ready from user {e.User.Username}");
                };

                _client.OnPresenceUpdate += (sender, e) =>
                {
                    Debug.WriteLine($"[Discord RPC] Updated: {e.Presence}");
                };

                _client.Initialize();

                // Set Default Presence saat aplikasi pertama kali berjalan
                SetStatus("Monitoring Windows System ⚡", "WinTweakStudio v1.0 - PC Optimizer");
                StartHeartbeat();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Discord RPC] Failed to initialize: {ex.Message}");
            }
        }

        public void SetStatus(string state, string details)
        {
            _lastState = state;
            _lastDetails = details;

            if (!_isEnabled || _client == null || _client.IsDisposed) return;

            try
            {
                SendPresence();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Discord RPC] Failed to set presence: {ex.Message}");
            }
        }

        private void SendPresence()
        {
            if (_client == null || _client.IsDisposed) return;

            _client.SetPresence(new RichPresence()
            {
                Details = _lastDetails,
                State = _lastState,
                Timestamps = new Timestamps(_startTime),
                Assets = new Assets()
                {
                    LargeImageKey = "app_icon", // Nama asset foto di Discord Dev Portal
                    LargeImageText = "WinTweakStudio - Ultimate Windows Optimization Suite",
                    SmallImageKey = "verified",
                    SmallImageText = "Pro Edition Active 👑"
                },
                Buttons = new Button[]
                {
                    new Button() { Label = "🚀 Download / Beli Pro", Url = "https://github.com" },
                    new Button() { Label = "💬 Join Discord Server", Url = "https://discord.gg" }
                }
            });
        }

        private void StartHeartbeat()
        {
            StopHeartbeat();
            // Heartbeat berjalan setiap 25 detik sekali secara background.
            // Penggunaan CPU: ~0.00% (Sangat ringan dan tidak memberatkan PC sama sekali).
            _heartbeatTimer = new System.Threading.Timer(OnHeartbeat, null, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(25));
        }

        private void OnHeartbeat(object? state)
        {
            if (!_isEnabled || _client == null || _client.IsDisposed) return;

            try
            {
                SendPresence();
                Debug.WriteLine("[Discord RPC] Heartbeat refreshed presence to maintain priority.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Discord RPC] Heartbeat failed: {ex.Message}");
            }
        }

        private void StopHeartbeat()
        {
            try
            {
                _heartbeatTimer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _heartbeatTimer?.Dispose();
                _heartbeatTimer = null;
            }
            catch { }
        }

        public void UpdateOptimizationStats(int tweaksApplied, string activeCategory)
        {
            if (!_isEnabled) return;

            string details = $"Active Module: {activeCategory}";
            string state = tweaksApplied > 0
                ? $"⚡ Optimized PC ({tweaksApplied} tweaks active)"
                : "Checking System Health & Performance...";

            SetStatus(state, details);
        }

        public void Shutdown()
        {
            StopHeartbeat();
            try
            {
                if (_client != null && !_client.IsDisposed)
                {
                    _client.ClearPresence();
                    _client.Dispose();
                    _client = null;
                }
            }
            catch { }
        }
    }
}
