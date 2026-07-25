using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WinTweakStudio.Services
{
    public interface ITroubleshootService
    {
        Task<bool> FixNetworkAndWinsockAsync();
        Task<bool> FlushDnsCacheAsync();
        Task<bool> FixWindowsUpdateCacheAsync();
        Task<bool> PerformFullSystemFixAsync();
    }

    public class TroubleshootService : ITroubleshootService
    {
        public async Task<bool> FixNetworkAndWinsockAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    RunCmd("netsh winsock reset");
                    RunCmd("netsh int ip reset");
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> FlushDnsCacheAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    RunCmd("ipconfig /flushdns");
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> FixWindowsUpdateCacheAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    RunCmd("net stop wuauserv");
                    RunCmd("net stop bits");
                    RunCmd("Remove-Item -Path 'C:\\Windows\\SoftwareDistribution\\Download\\*' -Recurse -Force -ErrorAction SilentlyContinue", isPowerShell: true);
                    RunCmd("net start wuauserv");
                    RunCmd("net start bits");
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> PerformFullSystemFixAsync()
        {
            bool res1 = await FixNetworkAndWinsockAsync();
            bool res2 = await FlushDnsCacheAsync();
            bool res3 = await FixWindowsUpdateCacheAsync();
            return res1 && res2 && res3;
        }

        private void RunCmd(string command, bool isPowerShell = false)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = isPowerShell ? "powershell.exe" : "cmd.exe",
                    Arguments = isPowerShell ? $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"" : $"/c {command}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(5000);
            }
            catch { }
        }
    }
}
