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

        public static readonly Dictionary<string, ServerInfo> GameServerList = new()
        {
            ["Valorant"] = new ServerInfo
            {
                DisplayName = "Valorant",
                Region = "Asia-Pacific (Singapore)",
                Host = "dynamodb.ap-southeast-1.amazonaws.com",
                PreferTcp = true,
                TcpPort = 443
            },
            ["CS2"] = new ServerInfo
            {
                DisplayName = "CS2 / CS:GO",
                Region = "Asia (Singapore)",
                Host = "103.10.124.1",
                PreferTcp = true,
                TcpPort = 80
            },
            ["MobileLegends"] = new ServerInfo
            {
                DisplayName = "Mobile Legends",
                Region = "Asia (ID/SG)",
                Host = "8.8.8.8",
                PreferTcp = false,
                TcpPort = 53
            },
            ["Dota2"] = new ServerInfo
            {
                DisplayName = "DOTA 2",
                Region = "SE Asia",
                Host = "103.10.124.1",
                PreferTcp = true,
                TcpPort = 80
            },
            ["ApexLegends"] = new ServerInfo
            {
                DisplayName = "Apex Legends",
                Region = "Singapore",
                Host = "ec2.ap-southeast-1.amazonaws.com",
                PreferTcp = true,
                TcpPort = 443
            },
            ["GTAV"] = new ServerInfo
            {
                DisplayName = "GTA V / Online",
                Region = "Global CDN",
                Host = "rockstargames.com",
                PreferTcp = true,
                TcpPort = 443
            },
            ["Roblox"] = new ServerInfo
            {
                DisplayName = "Roblox",
                Region = "Asia-Pacific",
                Host = "api.roblox.com",
                PreferTcp = true,
                TcpPort = 443
            },
            ["GoogleCloudGaming"] = new ServerInfo
            {
                DisplayName = "Google Cloud Gaming",
                Region = "Asia DNS",
                Host = "8.8.8.8",
                PreferTcp = false,
                TcpPort = 53
            },
            ["CloudflareEdge"] = new ServerInfo
            {
                DisplayName = "Cloudflare Edge",
                Region = "Global Anycast",
                Host = "1.1.1.1",
                PreferTcp = false,
                TcpPort = 53
            }
        };

        public async Task<List<GamePingResult>> TestGamePingServersAsync()
        {
            var tasks = GameServerList.Values.Select(server => PingHostAsync(server));
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<GamePingResult> PingHostAsync(ServerInfo server)
        {
            var result = new GamePingResult
            {
                GameName = server.DisplayName,
                Region = server.Region,
                HostOrIp = server.Host
            };

            var (success, latencyMs) = await CheckServerPing(server);

            if (success)
            {
                result.PingMs = latencyMs;
                if (latencyMs < 45)
                {
                    result.StatusText = $"{latencyMs} ms (Ultra Fast)";
                    result.StatusColor = "#2ECC71"; // Green
                }
                else if (latencyMs < 90)
                {
                    result.StatusText = $"{latencyMs} ms (Good)";
                    result.StatusColor = "#00E5FF"; // Cyan
                }
                else if (latencyMs < 150)
                {
                    result.StatusText = $"{latencyMs} ms (Moderate)";
                    result.StatusColor = "#F39C12"; // Orange
                }
                else
                {
                    result.StatusText = $"{latencyMs} ms (High Ping)";
                    result.StatusColor = "#E74C3C"; // Red
                }
            }
            else
            {
                result.PingMs = -1;
                result.StatusText = "Request Timed Out";
                result.StatusColor = "#E74C3C";
            }

            return result;
        }

        private static async Task<(bool Success, long LatencyMs)> CheckServerPing(ServerInfo server)
        {
            if (server.PreferTcp)
            {
                var tcpRes = await TcpPing(server.Host, server.TcpPort, 1500);
                if (tcpRes.Success) return tcpRes;

                return await IcmpPing(server.Host, 1500);
            }
            else
            {
                var icmpRes = await IcmpPing(server.Host, 1500);
                if (icmpRes.Success) return icmpRes;

                int port = server.TcpPort > 0 ? server.TcpPort : 443;
                return await TcpPing(server.Host, port, 1500);
            }
        }

        private static async Task<(bool Success, long LatencyMs)> IcmpPing(string host, int timeoutMs)
        {
            try
            {
                using var pinger = new Ping();
                var reply = await pinger.SendPingAsync(host, timeoutMs);
                if (reply.Status == IPStatus.Success)
                {
                    return (true, reply.RoundtripTime);
                }
                return (false, 0);
            }
            catch
            {
                return (false, 0);
            }
        }

        private static async Task<(bool Success, long LatencyMs)> TcpPing(string host, int port, int timeoutMs)
        {
            try
            {
                using var client = new TcpClient();
                var sw = System.Diagnostics.Stopwatch.StartNew();

                using var cts = new System.Threading.CancellationTokenSource(timeoutMs);
                var connectTask = client.ConnectAsync(host, port);
                var delayTask = Task.Delay(timeoutMs, cts.Token);

                var completed = await Task.WhenAny(connectTask, delayTask);
                sw.Stop();

                if (completed == connectTask && client.Connected)
                {
                    return (true, sw.ElapsedMilliseconds);
                }

                return (false, 0);
            }
            catch
            {
                return (false, 0);
            }
        }
    }
}
