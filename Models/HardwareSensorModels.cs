using System.Collections.Generic;

namespace WinTweakStudio.Models
{
    public class CoreClockInfo
    {
        public string CoreName { get; set; } = string.Empty;
        public double ClockSpeedMHz { get; set; }
    }

    public class CpuSensorData
    {
        public string Name { get; set; } = "CPU";
        public double Temperature { get; set; }
        public double UsagePercent { get; set; }
        public double PowerWatts { get; set; }
        public List<CoreClockInfo> CoreClocks { get; set; } = new();
    }

    public class GpuSensorData
    {
        public string Name { get; set; } = "GPU";
        public string Vendor { get; set; } = "Other"; // "NVIDIA", "AMD", "Intel"
        public double Temperature { get; set; }
        public double UsagePercent { get; set; }
        public double UsedVramGB { get; set; }
        public double TotalVramGB { get; set; }
        public double PowerWatts { get; set; }
        public bool IsDiscrete { get; set; }
        public bool IsActive { get; set; }

        public string GpuTypeBadge => IsDiscrete ? "dGPU" : "iGPU";
        public string VramFormatted => TotalVramGB > 0 ? $"{UsedVramGB:F1} / {TotalVramGB:F1} GB" : "N/A";
    }

    public class RamSensorData
    {
        public double UsedGB { get; set; }
        public double TotalGB { get; set; }
        public double UsagePercent { get; set; }
        public double SpeedMHz { get; set; }
    }

    public class StorageSensorData
    {
        public string Name { get; set; } = string.Empty;
        public string DriveType { get; set; } = "SSD/HDD";
        public double Temperature { get; set; }
        public double UsagePercent { get; set; }
        public double HealthPercent { get; set; } = 100;
        public bool HasTemperature { get; set; }
        public bool HasHealth { get; set; }
    }

    public class BatterySensorData
    {
        public bool IsPresent { get; set; }
        public double ChargePercent { get; set; }
        public double WearLevelPercent { get; set; }
        public double CycleCount { get; set; }
        public string Status { get; set; } = "N/A";
    }

    public class HardwareMetricsSnapshot
    {
        public CpuSensorData Cpu { get; set; } = new();
        public List<GpuSensorData> Gpus { get; set; } = new();
        public RamSensorData Ram { get; set; } = new();
        public List<StorageSensorData> StorageDrives { get; set; } = new();
        public BatterySensorData Battery { get; set; } = new();

        public bool IsXmpDisabled { get; set; }
        public string XmpWarningMessage { get; set; } = string.Empty;

        public bool HasNvidia => Gpus.Any(g => string.Equals(g.Vendor, "NVIDIA", System.StringComparison.OrdinalIgnoreCase));
        public bool HasAmd => Gpus.Any(g => string.Equals(g.Vendor, "AMD", System.StringComparison.OrdinalIgnoreCase));
        public bool HasIntel => Gpus.Any(g => string.Equals(g.Vendor, "Intel", System.StringComparison.OrdinalIgnoreCase));
    }
}
