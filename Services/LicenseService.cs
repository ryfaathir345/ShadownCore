using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinTweakStudio.Services
{
    public enum UserRole
    {
        Free,
        VIP,
        Owner
    }

    public interface ILicenseService
    {
        string Username { get; }
        UserRole CurrentRole { get; }
        string LicenseKey { get; }
        bool IsVipOrOwner { get; }
        bool IsOwner { get; }
        string RoleBadgeText { get; }
        string RoleBadgeColor { get; }
        bool RegisterFreeUser(string username);
        Task<bool> ActivateLicenseAsync(string username, string key);
        string GenerateKey(UserRole role);
        Task SyncOnlineLicenseAsync();
        void Logout();
    }

    public class LicenseService : ObservableObject, ILicenseService
    {
        private static readonly string LicenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.dat");
        private static readonly string DatabaseUrl = "https://raw.githubusercontent.com/Prosnatics/WinTweakStudio-Auth/main/users.json";

        private static LicenseService? _instance;
        public static LicenseService Instance => _instance ??= new LicenseService();

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            private set => SetProperty(ref _username, value);
        }

        private UserRole _currentRole = UserRole.Free;
        public UserRole CurrentRole
        {
            get => _currentRole;
            private set
            {
                if (SetProperty(ref _currentRole, value))
                {
                    OnPropertyChanged(nameof(IsVipOrOwner));
                    OnPropertyChanged(nameof(IsOwner));
                    OnPropertyChanged(nameof(RoleBadgeText));
                    OnPropertyChanged(nameof(RoleBadgeColor));
                }
            }
        }

        private string _licenseKey = string.Empty;
        public string LicenseKey
        {
            get => _licenseKey;
            private set => SetProperty(ref _licenseKey, value);
        }

        public bool IsVipOrOwner => CurrentRole == UserRole.VIP || CurrentRole == UserRole.Owner;
        public bool IsOwner => CurrentRole == UserRole.Owner;

        public string RoleBadgeText => CurrentRole switch
        {
            UserRole.VIP => "VIP MEMBER",
            UserRole.Owner => "OWNER / DEVELOPER",
            _ => "FREE USER"
        };

        public string RoleBadgeColor => CurrentRole switch
        {
            UserRole.VIP => "#00E5FF",
            UserRole.Owner => "#FFD700",
            _ => "#9CA3AF"
        };

        public LicenseService()
        {
            _instance = this;
            LoadSavedLicense();
        }

        private static bool IsOwnerUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            string u = username.Trim();
            return string.Equals(u, "Ryfaathir", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(u, "Ryfaathir345", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(u, "ShadownCore", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(u, "Shadown Core", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(u, "ShadowCore", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(u, "Owner", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(u, "Developer", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(u, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadSavedLicense()
        {
            try
            {
                if (File.Exists(LicenseFilePath))
                {
                    string base64Content = File.ReadAllText(LicenseFilePath).Trim();
                    string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64Content));
                    string[] parts = decoded.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        Username = parts[0];
                        string key = parts[1];

                        // Perform offline validation first
                        ValidateOffline(Username, key);
                        
                        // Async sync online in background
                        Task.Run(() => SyncOnlineLicenseAsync());
                    }
                    else if (parts.Length == 1 && !string.IsNullOrWhiteSpace(parts[0]))
                    {
                        Username = parts[0];
                        if (IsOwnerUsername(Username))
                        {
                            CurrentRole = UserRole.Owner;
                            LicenseKey = "OWNER-AUTHOR-DEV-KEY";
                        }
                        else
                        {
                            CurrentRole = UserRole.Free;
                            LicenseKey = string.Empty;
                        }
                    }
                }
            }
            catch
            {
                Username = string.Empty;
                CurrentRole = UserRole.Free;
            }
        }

        private void ValidateOffline(string username, string key)
        {
            if (string.IsNullOrWhiteSpace(username)) return;

            string cleanKey = key?.Trim().ToUpper() ?? string.Empty;
            if (IsOwnerUsername(username) || cleanKey.StartsWith("OWNER-") || cleanKey == "OWNER-SECRET-KEY-999")
            {
                CurrentRole = UserRole.Owner;
                LicenseKey = string.IsNullOrEmpty(cleanKey) ? "OWNER-AUTHOR-DEV-KEY" : cleanKey;
            }
            else if (cleanKey.StartsWith("VIP-") || cleanKey.Length >= 16)
            {
                CurrentRole = UserRole.VIP;
                LicenseKey = cleanKey;
            }
            else
            {
                CurrentRole = UserRole.Free;
                LicenseKey = string.Empty;
            }
        }

        public bool RegisterFreeUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            Username = username.Trim();
            if (IsOwnerUsername(Username))
            {
                CurrentRole = UserRole.Owner;
                LicenseKey = "OWNER-AUTHOR-DEV-KEY";
            }
            else
            {
                CurrentRole = UserRole.Free;
                LicenseKey = string.Empty;
            }
            SaveLicense(Username, LicenseKey);
            return true;
        }

        public async Task<bool> ActivateLicenseAsync(string username, string key)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            string u = username.Trim();
            string k = key?.Trim().ToUpper() ?? string.Empty;

            if (IsOwnerUsername(u))
            {
                Username = u;
                CurrentRole = UserRole.Owner;
                LicenseKey = string.IsNullOrEmpty(k) ? "OWNER-AUTHOR-DEV-KEY" : k;
                SaveLicense(u, LicenseKey);
                return true;
            }

            if (string.IsNullOrWhiteSpace(k)) return false;

            // Try online validation first
            bool onlineResult = await ValidateOnlineAsync(u, k);
            if (onlineResult)
            {
                SaveLicense(u, k);
                return true;
            }

            // Fallback: Offline validation
            if (k.StartsWith("OWNER-") || k == "OWNER-SECRET-KEY-999" || k.StartsWith("VIP-"))
            {
                Username = u;
                ValidateOffline(u, k);
                SaveLicense(u, k);
                return true;
            }

            return false;
        }

        private async Task<bool> ValidateOnlineAsync(string username, string key)
        {
            if (IsOwnerUsername(username))
            {
                CurrentRole = UserRole.Owner;
                Username = username;
                if (string.IsNullOrEmpty(LicenseKey)) LicenseKey = "OWNER-AUTHOR-DEV-KEY";
                return true;
            }
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetStringAsync(DatabaseUrl);
                
                using var doc = JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("users", out var usersArray))
                {
                    foreach (var userElement in usersArray.EnumerateArray())
                    {
                        string? name = userElement.GetProperty("name").GetString();
                        string? userKey = userElement.GetProperty("key").GetString();
                        string? role = userElement.GetProperty("role").GetString();

                        if (string.Equals(name, username, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(userKey, key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.Equals(role, "OWNER", StringComparison.OrdinalIgnoreCase))
                            {
                                CurrentRole = UserRole.Owner;
                            }
                            else if (string.Equals(role, "VIP", StringComparison.OrdinalIgnoreCase))
                            {
                                CurrentRole = UserRole.VIP;
                            }
                            else
                            {
                                CurrentRole = UserRole.Free;
                            }
                            Username = username;
                            LicenseKey = key;
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // Network issue or repo does not exist yet
            }
            return false;
        }

        public async Task SyncOnlineLicenseAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(LicenseKey)) return;
            await ValidateOnlineAsync(Username, LicenseKey);
        }

        private void SaveLicense(string username, string key)
        {
            try
            {
                string raw = $"{username}:{key}";
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                File.WriteAllText(LicenseFilePath, base64);
            }
            catch { }
        }

        public void Logout()
        {
            Username = string.Empty;
            CurrentRole = UserRole.Free;
            LicenseKey = string.Empty;
            try
            {
                if (File.Exists(LicenseFilePath))
                {
                    File.Delete(LicenseFilePath);
                }
            }
            catch { }
        }

        public string GenerateKey(UserRole role)
        {
            string prefix = role == UserRole.Owner ? "OWNER" : "VIP";
            string randomStr = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
            return $"{prefix}-{randomStr.Substring(0, 4)}-{randomStr.Substring(4, 4)}-{randomStr.Substring(8, 4)}-{randomStr.Substring(12, 4)}";
        }
    }
}
