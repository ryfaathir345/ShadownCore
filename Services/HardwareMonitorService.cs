using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using WinTweakStudio.Models;

namespace WinTweakStudio.Services
{
    public class HardwareUpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
            {
                subHardware.Accept(this);
            }
        }

        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }

    public class HardwareMonitorService : IHardwareMonitorService
    {
        private Computer? _computer;
        private readonly HardwareUpdateVisitor _updateVisitor = new();
        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsStorageEnabled = true,
                    IsMotherboardEnabled = true,
                    IsBatteryEnabled = true,
                    IsControllerEnabled = true
                };

                _computer.Open();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HardwareMonitorService initialization error: {ex.Message}");
                _isInitialized = false;
            }
        }

        public Task<HardwareMetricsSnapshot> ReadMetricsAsync()
        {
            return Task.Run(() =>
            {
                var snapshot = new HardwareMetricsSnapshot();

                if (!_isInitialized || _computer == null)
                {
                    CheckWmiBattery(snapshot.Battery);
                    return snapshot;
                }

                try
                {
                    _computer.Accept(_updateVisitor);

                    foreach (IHardware hardware in _computer.Hardware)
                    {
                        switch (hardware.HardwareType)
                        {
                            case HardwareType.Cpu:
                                ReadCpuData(hardware, snapshot.Cpu);
                                break;
                            case HardwareType.GpuNvidia:
                            case HardwareType.GpuAmd:
                            case HardwareType.GpuIntel:
                                var gpuData = ReadGpuData(hardware);
                                if (gpuData != null) snapshot.Gpus.Add(gpuData);
                                break;
                            case HardwareType.Memory:
                                ReadRamData(hardware, snapshot.Ram);
                                break;
                            case HardwareType.Storage:
                                var storageData = ReadStorageData(hardware);
                                if (storageData != null) snapshot.StorageDrives.Add(storageData);
                                break;
                            case HardwareType.Battery:
                                ReadBatteryData(hardware, snapshot.Battery);
                                break;
                        }

                        // Collect Fan sensors unconditionally from all hardware & sub-hardware nodes
                        CollectFanSensors(hardware, snapshot.Fans, GetHardwareLocationLabel(hardware));
                    }

                    if (snapshot.Gpus.Count == 0)
                    {
                        CheckWmiGpus(snapshot.Gpus);
                    }

                    if (!snapshot.Battery.IsPresent)
                    {
                        CheckWmiBattery(snapshot.Battery);
                    }

                    // Check WMI & Thermal estimation if fans are 0 RPM or missing
                    CheckWmiFans(snapshot);

                    CheckWmiRamXmp(snapshot);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading hardware sensors: {ex.Message}");
                    CheckWmiGpus(snapshot.Gpus);
                    CheckWmiBattery(snapshot.Battery);
                    CheckWmiFans(snapshot);
                    CheckWmiRamXmp(snapshot);
                }

                return snapshot;
            });
        }

        private void CheckWmiRamXmp(HardwareMetricsSnapshot snapshot)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Speed, ConfiguredClockSpeed FROM Win32_PhysicalMemory");
                uint maxRatedSpeed = 0;
                uint minConfiguredSpeed = uint.MaxValue;

                foreach (var obj in searcher.Get())
                {
                    if (obj["Speed"] != null && uint.TryParse(obj["Speed"].ToString(), out uint speed))
                    {
                        if (speed > maxRatedSpeed) maxRatedSpeed = speed;
                    }
                    if (obj["ConfiguredClockSpeed"] != null && uint.TryParse(obj["ConfiguredClockSpeed"].ToString(), out uint cfgSpeed) && cfgSpeed > 0)
                    {
                        if (cfgSpeed < minConfiguredSpeed) minConfiguredSpeed = cfgSpeed;
                    }
                }

                if (maxRatedSpeed > 0 && minConfiguredSpeed < uint.MaxValue)
                {
                    if (maxRatedSpeed > minConfiguredSpeed + 200)
                    {
                        snapshot.IsXmpDisabled = true;
                        snapshot.XmpWarningMessage = $"⚠ XMP/EXPO Belum Aktif (RAM Running: {minConfiguredSpeed} MHz < Rated: {maxRatedSpeed} MHz)";
                    }
                }
            }
            catch { }
        }

        private void ReadCpuData(IHardware hardware, CpuSensorData cpu)
        {
            cpu.Name = string.IsNullOrWhiteSpace(hardware.Name) ? "CPU" : hardware.Name;

            var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && (s.Name.Contains("Package") || s.Name.Contains("Core")));
            if (tempSensor?.Value != null) cpu.Temperature = Math.Round(tempSensor.Value.Value, 1);

            var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && (s.Name.Contains("Total") || s.Name.Equals("CPU Core", StringComparison.OrdinalIgnoreCase)));
            if (loadSensor?.Value != null) cpu.UsagePercent = Math.Round(loadSensor.Value.Value, 1);

            var powerSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Power && (s.Name.Contains("Package") || s.Name.Contains("CPU")));
            if (powerSensor?.Value != null) cpu.PowerWatts = Math.Round(powerSensor.Value.Value, 1);

            cpu.CoreClocks.Clear();
            var clockSensors = hardware.Sensors.Where(s => s.SensorType == SensorType.Clock && s.Name.Contains("Core #")).Take(8);
            foreach (var clock in clockSensors)
            {
                if (clock.Value != null)
                {
                    cpu.CoreClocks.Add(new CoreClockInfo
                    {
                        CoreName = clock.Name,
                        ClockSpeedMHz = Math.Round(clock.Value.Value, 0)
                    });
                }
            }
        }

        private GpuSensorData ReadGpuData(IHardware hardware)
        {
            string name = string.IsNullOrWhiteSpace(hardware.Name) ? "GPU" : hardware.Name;
            string vendor = "Other";

            if (hardware.HardwareType == HardwareType.GpuNvidia || name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) || name.Contains("GeForce", StringComparison.OrdinalIgnoreCase))
            {
                vendor = "NVIDIA";
            }
            else if (hardware.HardwareType == HardwareType.GpuAmd || name.Contains("AMD", StringComparison.OrdinalIgnoreCase) || name.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
            {
                vendor = "AMD";
            }
            else if (hardware.HardwareType == HardwareType.GpuIntel || name.Contains("Intel", StringComparison.OrdinalIgnoreCase) || name.Contains("Arc", StringComparison.OrdinalIgnoreCase) || name.Contains("HD Graphics", StringComparison.OrdinalIgnoreCase) || name.Contains("UHD", StringComparison.OrdinalIgnoreCase) || name.Contains("Iris", StringComparison.OrdinalIgnoreCase))
            {
                vendor = "Intel";
            }

            var gpu = new GpuSensorData
            {
                Name = name,
                Vendor = vendor,
                IsDiscrete = vendor == "NVIDIA" || (vendor == "AMD" && !name.Contains("Radeon Graphics", StringComparison.OrdinalIgnoreCase))
            };

            var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && (s.Name.Contains("Core") || s.Name.Contains("GPU")));
            if (tempSensor?.Value != null) gpu.Temperature = Math.Round(tempSensor.Value.Value, 1);

            var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && (s.Name.Contains("Core") || s.Name.Contains("GPU")));
            if (loadSensor?.Value != null) gpu.UsagePercent = Math.Round(loadSensor.Value.Value, 1);

            var powerSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Power);
            if (powerSensor?.Value != null) gpu.PowerWatts = Math.Round(powerSensor.Value.Value, 1);

            var usedVram = hardware.Sensors.FirstOrDefault(s => (s.SensorType == SensorType.SmallData || s.SensorType == SensorType.Data) && s.Name.Contains("Memory Used"));
            if (usedVram?.Value != null) gpu.UsedVramGB = Math.Round(usedVram.Value.Value / 1024.0, 2);

            var totalVram = hardware.Sensors.FirstOrDefault(s => (s.SensorType == SensorType.SmallData || s.SensorType == SensorType.Data) && s.Name.Contains("Memory Total"));
            if (totalVram?.Value != null) gpu.TotalVramGB = Math.Round(totalVram.Value.Value / 1024.0, 2);

            gpu.IsActive = gpu.UsagePercent > 0.5 || gpu.PowerWatts > 2.0;

            return gpu;
        }

        private void CheckWmiGpus(List<GpuSensorData> gpus)
        {
            if (gpus.Count > 0) return;
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
                foreach (var obj in searcher.Get())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    string vendor = "Other";
                    if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) || name.Contains("GeForce", StringComparison.OrdinalIgnoreCase)) vendor = "NVIDIA";
                    else if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase) || name.Contains("Radeon", StringComparison.OrdinalIgnoreCase)) vendor = "AMD";
                    else if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase) || name.Contains("HD Graphics", StringComparison.OrdinalIgnoreCase) || name.Contains("UHD", StringComparison.OrdinalIgnoreCase) || name.Contains("Iris", StringComparison.OrdinalIgnoreCase) || name.Contains("Arc", StringComparison.OrdinalIgnoreCase)) vendor = "Intel";

                    bool isDiscrete = vendor == "NVIDIA" || (vendor == "AMD" && !name.Contains("Radeon Graphics", StringComparison.OrdinalIgnoreCase));

                    double ramGB = 0;
                    if (obj["AdapterRAM"] != null && long.TryParse(obj["AdapterRAM"].ToString(), out long bytes))
                    {
                        ramGB = Math.Round(bytes / (1024.0 * 1024.0 * 1024.0), 2);
                    }

                    gpus.Add(new GpuSensorData
                    {
                        Name = name,
                        Vendor = vendor,
                        IsDiscrete = isDiscrete,
                        TotalVramGB = ramGB,
                        IsActive = isDiscrete
                    });
                }
            }
            catch { }
        }

        private void ReadRamData(IHardware hardware, RamSensorData ram)
        {
            var usedMem = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name.Contains("Memory Used"));
            var availMem = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name.Contains("Memory Available"));
            var loadMem = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("Memory"));
            var clockMem = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Clock && s.Name.Contains("Memory"));

            if (usedMem?.Value != null) ram.UsedGB = Math.Round(usedMem.Value.Value, 1);
            if (availMem?.Value != null && usedMem?.Value != null) ram.TotalGB = Math.Round(usedMem.Value.Value + availMem.Value.Value, 1);
            if (loadMem?.Value != null) ram.UsagePercent = Math.Round(loadMem.Value.Value, 1);
            if (clockMem?.Value != null) ram.SpeedMHz = Math.Round(clockMem.Value.Value, 0);

            // Fallback for Total RAM if unavailable from sensors
            if (ram.TotalGB <= 0)
            {
                try
                {
                    var gcMemoryInfo = GC.GetGCMemoryInfo();
                    ram.TotalGB = Math.Round(gcMemoryInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0 * 1024.0), 1);
                }
                catch { }
            }
        }

        private StorageSensorData ReadStorageData(IHardware hardware)
        {
            var storage = new StorageSensorData
            {
                Name = string.IsNullOrWhiteSpace(hardware.Name) ? "Storage Drive" : hardware.Name,
                DriveType = hardware.Name.Contains("NVMe", StringComparison.OrdinalIgnoreCase) ? "NVMe SSD" :
                            hardware.Name.Contains("SSD", StringComparison.OrdinalIgnoreCase) ? "SATA SSD" : "HDD/SSD"
            };

            var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
            if (tempSensor?.Value != null)
            {
                storage.Temperature = Math.Round(tempSensor.Value.Value, 1);
                storage.HasTemperature = true;
            }

            var loadSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && (s.Name.Contains("Used Space") || s.Name.Contains("Total")));
            if (loadSensor?.Value != null) storage.UsagePercent = Math.Round(loadSensor.Value.Value, 1);

            var healthSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Level && (s.Name.Contains("Remaining Life") || s.Name.Contains("Health") || s.Name.Contains("Wear")));
            if (healthSensor?.Value != null)
            {
                storage.HealthPercent = Math.Round(healthSensor.Value.Value, 0);
                storage.HasHealth = true;
            }

            return storage;
        }

        private void ReadBatteryData(IHardware hardware, BatterySensorData battery)
        {
            battery.IsPresent = true;

            var chargeSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Level && s.Name.Contains("Charge"));
            if (chargeSensor?.Value != null) battery.ChargePercent = Math.Round(chargeSensor.Value.Value, 1);

            var degradationSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Level && (s.Name.Contains("Degradation") || s.Name.Contains("Wear")));
            if (degradationSensor?.Value != null)
            {
                battery.WearLevelPercent = Math.Round(100.0 - degradationSensor.Value.Value, 1);
            }
            else
            {
                battery.WearLevelPercent = 100.0;
            }

            battery.Status = battery.ChargePercent > 0 ? $"{battery.ChargePercent}%" : "Connected";
        }

        private void CheckWmiBattery(BatterySensorData battery)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
                using var collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    battery.IsPresent = true;
                    if (obj["EstimatedChargeRemaining"] != null)
                    {
                        battery.ChargePercent = Convert.ToDouble(obj["EstimatedChargeRemaining"]);
                    }
                    if (obj["DesignCapacity"] != null && obj["FullChargeCapacity"] != null)
                    {
                        double design = Convert.ToDouble(obj["DesignCapacity"]);
                        double full = Convert.ToDouble(obj["FullChargeCapacity"]);
                        if (design > 0)
                        {
                            battery.WearLevelPercent = Math.Round((full / design) * 100.0, 1);
                        }
                    }
                    else
                    {
                        battery.WearLevelPercent = 100.0;
                    }
                    battery.Status = "Battery Present";
                    break;
                }
            }
            catch { }
        }

        private string GetHardwareLocationLabel(IHardware hardware)
        {
            return hardware.HardwareType switch
            {
                HardwareType.Cpu => "CPU",
                HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel => "GPU",
                HardwareType.Motherboard or HardwareType.SuperIO => "Motherboard / EC",
                HardwareType.EmbeddedController => "Embedded Controller (EC)",
                _ => hardware.HardwareType.ToString()
            };
        }

        private void CollectFanSensors(IHardware hardware, List<FanSensorData> fans, string location)
        {
            try
            {
                var fanSensors = hardware.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
                var controlSensors = hardware.Sensors.Where(s => s.SensorType == SensorType.Control).ToList();

                foreach (var fanSensor in fanSensors)
                {
                    double rpm = fanSensor.Value.HasValue ? Math.Round(fanSensor.Value.Value, 0) : 0;
                    string fanName = string.IsNullOrWhiteSpace(fanSensor.Name) ? $"{location} Fan" : fanSensor.Name;

                    // Match control percent if available
                    double? ctrlPercent = null;
                    var matchingCtrl = controlSensors.FirstOrDefault(c => c.Name.Contains(fanSensor.Name, StringComparison.OrdinalIgnoreCase) || c.Index == fanSensor.Index);
                    if (matchingCtrl?.Value != null)
                    {
                        ctrlPercent = Math.Round(matchingCtrl.Value.Value, 0);
                    }

                    var existing = fans.FirstOrDefault(f => f.Name.Equals(fanName, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        if (rpm > 0 || existing.SpeedRpm == 0) existing.SpeedRpm = rpm;
                        if (ctrlPercent.HasValue) existing.ControlPercent = ctrlPercent;
                    }
                    else
                    {
                        fans.Add(new FanSensorData
                        {
                            Name = fanName,
                            SpeedRpm = rpm,
                            ControlPercent = ctrlPercent,
                            Location = location
                        });
                    }
                }

                // Check Control sensors that might be fans (e.g. Fan PWM % without RPM)
                foreach (var ctrlSensor in controlSensors)
                {
                    if (ctrlSensor.Name.Contains("Fan", StringComparison.OrdinalIgnoreCase) || ctrlSensor.Name.Contains("Cooling", StringComparison.OrdinalIgnoreCase))
                    {
                        string ctrlName = ctrlSensor.Name;
                        var existing = fans.FirstOrDefault(f => f.Name.Equals(ctrlName, StringComparison.OrdinalIgnoreCase));
                        double? percent = ctrlSensor.Value.HasValue ? Math.Round(ctrlSensor.Value.Value, 0) : null;

                        if (existing != null)
                        {
                            if (percent.HasValue) existing.ControlPercent = percent;
                        }
                        else
                        {
                            fans.Add(new FanSensorData
                            {
                                Name = ctrlName,
                                SpeedRpm = 0,
                                ControlPercent = percent,
                                Location = location
                            });
                        }
                    }
                }

                // Check SubHardware (e.g. SuperIO / EC chips under Motherboard)
                foreach (IHardware subHardware in hardware.SubHardware)
                {
                    CollectFanSensors(subHardware, fans, $"{location} (Sub)");
                }
            }
            catch { }
        }

        private void CheckWmiFans(HardwareMetricsSnapshot snapshot)
        {
            var fans = snapshot.Fans;
            try
            {
                // Query standard Win32_Fan
                using (var searcher = new ManagementObjectSearcher("SELECT Name, DesiredSpeed FROM Win32_Fan"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString() ?? "System Fan";
                        double speed = 0;
                        if (obj["DesiredSpeed"] != null && double.TryParse(obj["DesiredSpeed"].ToString(), out double rpm))
                        {
                            speed = rpm;
                        }

                        if (!fans.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                        {
                            fans.Add(new FanSensorData
                            {
                                Name = name,
                                SpeedRpm = speed,
                                Location = "WMI Fan"
                            });
                        }
                    }
                }

                // Dual Fan Laptop Fallback: Ensure both CPU Fan and GPU Fan exist
                bool hasCpuFan = fans.Any(f => f.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase));
                bool hasGpuFan = fans.Any(f => f.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase));

                if (!hasCpuFan)
                {
                    fans.Insert(0, new FanSensorData
                    {
                        Name = "CPU Fan",
                        SpeedRpm = 0,
                        ControlPercent = null,
                        Location = "CPU / Laptop EC",
                        IsHardwareSensor = false
                    });
                }

                if (!hasGpuFan)
                {
                    fans.Add(new FanSensorData
                    {
                        Name = "GPU Fan",
                        SpeedRpm = 0,
                        ControlPercent = null,
                        Location = "dGPU / Laptop EC",
                        IsHardwareSensor = false
                    });
                }

                // Mark whether fan sensor returned exact hardware RPM from physical sensors
                foreach (var fan in fans)
                {
                    if (fan.SpeedRpm > 0)
                    {
                        fan.IsHardwareSensor = true;
                    }
                }
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                _computer?.Close();
                _computer = null;
            }
            catch { }
        }
    }
}
