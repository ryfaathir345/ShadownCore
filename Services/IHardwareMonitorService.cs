using System;
using System.Threading.Tasks;
using WinTweakStudio.Models;

namespace WinTweakStudio.Services
{
    public interface IHardwareMonitorService : IDisposable
    {
        void Initialize();
        Task<HardwareMetricsSnapshot> ReadMetricsAsync();
    }
}
