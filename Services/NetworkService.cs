using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;

namespace WinTweakStudio.Services
{
    public class NetworkService : INetworkService
    {
        public List<NetworkAdapterInfo> GetActiveAdapters()
        {
            var list = new List<NetworkAdapterInfo>();
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                                (n.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                                 n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    .ToList();

                foreach (var adapter in adapters)
                {
                    var ipProperties = adapter.GetIPProperties();
                    var ipv4 = ipProperties.UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                    var gateway = ipProperties.GatewayAddresses
                        .FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork);
                    var dnsList = ipProperties.DnsAddresses
                        .Where(d => d.AddressFamily == AddressFamily.InterNetwork)
                        .Select(d => d.ToString())
                        .ToList();

                    list.Add(new NetworkAdapterInfo
                    {
                        Id = adapter.Id,
                        Name = adapter.Name,
                        Description = adapter.Description,
                        IpAddress = ipv4?.Address.ToString() ?? "N/A",
                        SubnetMask = ipv4?.IPv4Mask.ToString() ?? "N/A",
                        Gateway = gateway?.Address.ToString() ?? "N/A",
                        DnsServers = dnsList.Count > 0 ? string.Join(", ", dnsList) : "DHCP / Automatic"
                    });
                }
            }
            catch { }

            return list;
        }

        public NetworkAdapterInfo? GetPrimaryAdapterInfo()
        {
            return GetActiveAdapters().FirstOrDefault();
        }

        public bool ApplyNagleAlgorithm(bool enableTweak, string? adapterGuid = null)
        {
            try
            {
                var targetGuids = new List<string>();
                if (!string.IsNullOrEmpty(adapterGuid))
                {
                    targetGuids.Add(adapterGuid);
                }
                else
                {
                    targetGuids = GetActiveAdapters().Select(a => a.Id).ToList();
                }

                int val = enableTweak ? 1 : 0;

                foreach (var guid in targetGuids)
                {
                    string keyPath = $@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{guid}";
                    using var key = Registry.LocalMachine.CreateSubKey(keyPath);
                    if (key != null)
                    {
                        key.SetValue("TcpAckFrequency", val, RegistryValueKind.DWord);
                        key.SetValue("TCPNoDelay", val, RegistryValueKind.DWord);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetCustomDns(string provider, string? adapterName = null)
        {
            try
            {
                string targetAdapter = adapterName ?? GetPrimaryAdapterInfo()?.Name ?? "Ethernet";
                string primaryDns = "1.1.1.1";
                string secondaryDns = "1.0.0.1";

                if (string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase))
                {
                    primaryDns = "8.8.8.8";
                    secondaryDns = "8.8.4.4";
                }
                else if (string.Equals(provider, "Quad9", StringComparison.OrdinalIgnoreCase))
                {
                    primaryDns = "9.9.9.9";
                    secondaryDns = "149.112.112.112";
                }

                string psCommand = $"Set-DnsClientServerAddress -InterfaceAlias \"{targetAdapter}\" -ServerAddresses (\"{primaryDns}\",\"{secondaryDns}\")";

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                proc?.WaitForExit();
                return proc?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public bool RevertDns(string previousDnsValue, string? adapterName = null)
        {
            try
            {
                string targetAdapter = adapterName ?? GetPrimaryAdapterInfo()?.Name ?? "Ethernet";
                string psCommand;

                if (string.Equals(previousDnsValue, "DHCP", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(previousDnsValue))
                {
                    psCommand = $"Set-DnsClientServerAddress -InterfaceAlias \"{targetAdapter}\" -ResetServerAddresses";
                }
                else
                {
                    var dnsArray = previousDnsValue.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string dnsArgs = string.Join(",", dnsArray.Select(d => $"\"{d}\""));
                    psCommand = $"Set-DnsClientServerAddress -InterfaceAlias \"{targetAdapter}\" -ServerAddresses ({dnsArgs})";
                }

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                proc?.WaitForExit();
                return proc?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public bool IsWindowsHomeEdition()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string edition = key?.GetValue("EditionID")?.ToString() ?? "";
                string productName = key?.GetValue("ProductName")?.ToString() ?? "";

                return edition.Contains("Home", StringComparison.OrdinalIgnoreCase) ||
                       productName.Contains("Home", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<GamePingResult>> TestGamePingServersAsync()
        {
            var servers = new List<(string Game, string Region, string Host)>
            {
                ("Valorant", "Asia-Pacific (Singapore)", "151.106.32.1"),
                ("CS2 / CS:GO", "Asia (Singapore)", "103.10.124.1"),
                ("Mobile Legends", "Asia (ID/SG)", "128.199.200.1"),
                ("DOTA 2", "SE Asia", "103.28.54.1"),
                ("Apex Legends", "Singapore", "203.116.82.1"),
                ("GTA V / Online", "Global CDN", "104.18.22.12"),
                ("Roblox", "Asia-Pacific", "128.116.0.1"),
                ("Google Cloud Gaming", "Asia DNS", "8.8.8.8"),
                ("Cloudflare Edge", "Global Anycast", "1.1.1.1")
            };

            var tasks = servers.Select(s => PingHostAsync(s.Game, s.Region, s.Host));
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<GamePingResult> PingHostAsync(string gameName, string region, string host)
        {
            var result = new GamePingResult
            {
                GameName = gameName,
                Region = region,
                HostOrIp = host
            };

            try
            {
                using var pinger = new Ping();
                var reply = await pinger.SendPingAsync(host, 2000);
                if (reply.Status == IPStatus.Success)
                {
                    result.PingMs = reply.RoundtripTime;
                    if (reply.RoundtripTime < 45)
                    {
                        result.StatusText = $"{reply.RoundtripTime} ms (Ultra Fast)";
                        result.StatusColor = "#2ECC71"; // Green
                    }
                    else if (reply.RoundtripTime < 90)
                    {
                        result.StatusText = $"{reply.RoundtripTime} ms (Good)";
                        result.StatusColor = "#00E5FF"; // Cyan
                    }
                    else if (reply.RoundtripTime < 150)
                    {
                        result.StatusText = $"{reply.RoundtripTime} ms (Moderate)";
                        result.StatusColor = "#F39C12"; // Yellow/Orange
                    }
                    else
                    {
                        result.StatusText = $"{reply.RoundtripTime} ms (High Ping)";
                        result.StatusColor = "#E74C3C"; // Red
                    }
                }
                else
                {
                    result.PingMs = -1;
                    result.StatusText = "Request Timed Out";
                    result.StatusColor = "#E74C3C";
                }
            }
            catch
            {
                result.PingMs = -1;
                result.StatusText = "Connection Failed";
                result.StatusColor = "#E74C3C";
            }

            return result;
        }
    }
}
