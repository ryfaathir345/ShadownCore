using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinTweakStudio.Services
{
    public class NetworkAdapterInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IpAddress { get; set; } = "N/A";
        public string SubnetMask { get; set; } = "N/A";
        public string Gateway { get; set; } = "N/A";
        public string DnsServers { get; set; } = "DHCP / Automatic";
    }

    public class ServerInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public bool PreferTcp { get; set; }
        public int TcpPort { get; set; }
    }

    public class GamePingResult
    {
        public string GameName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string HostOrIp { get; set; } = string.Empty;
        public long PingMs { get; set; } = -1;
        public string StatusText { get; set; } = "Testing...";
        public string StatusColor { get; set; } = "#9CA3AF";
    }

    public interface INetworkService
    {
        List<NetworkAdapterInfo> GetActiveAdapters();
        bool ApplyNagleAlgorithm(bool enableTweak, string? adapterGuid = null);
        bool SetCustomDns(string provider, string? adapterName = null);
        bool RevertDns(string previousDnsValue, string? adapterName = null);
        NetworkAdapterInfo? GetPrimaryAdapterInfo();
        bool IsWindowsHomeEdition();
        Task<List<GamePingResult>> TestGamePingServersAsync();
        Task<GamePingResult> PingHostAsync(ServerInfo server);
    }
}
