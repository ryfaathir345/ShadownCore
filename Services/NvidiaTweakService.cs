using System;
using System.Diagnostics;
using NvAPIWrapper;
using NvAPIWrapper.DRS;

namespace WinTweakStudio.Services
{
    public interface INvidiaTweakService
    {
        bool IsAvailable { get; }
        string GetSettingValue(uint settingId);
        bool SetSettingValue(uint settingId, uint value);
        string GetSettingValueByName(string settingName);
        bool SetSettingValueByName(string settingName, string recommendedValue);
    }

    public class NvidiaTweakService : INvidiaTweakService
    {
        // NVAPI DRS Setting IDs
        public const uint POWER_MANAGEMENT_MODE_ID = 0x1057E886; // Preferred P-State (1 = Prefer Max Perf, 0 = Adaptive)
        public const uint LOW_LATENCY_MODE_ID       = 0x00A5210E; // Low Latency Mode (0 = Off, 1 = On, 2 = Ultra)
        public const uint SHADER_CACHE_SIZE_ID      = 0x00A04D40; // Shader Cache Size (10240 = 10GB, 0 = Driver Default)
        public const uint TEXTURE_FILTERING_ID      = 0x00844ED7; // Texture Filtering Quality (20 = High Perf, 10 = Perf, 0 = Quality)
        public const uint THREADED_OPTIMIZATION_ID  = 0x00044A4B; // Threaded Optimization (1 = On, 0 = Off, 2 = Auto)

        private bool _initialized;
        private bool _initAttempted;

        public bool IsAvailable
        {
            get
            {
                EnsureInitialized();
                return _initialized;
            }
        }

        private void EnsureInitialized()
        {
            if (_initAttempted) return;
            _initAttempted = true;

            try
            {
                NVIDIA.Initialize();
                _initialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVAPI Initialization failed: {ex.Message}");
                _initialized = false;
            }
        }

        public string GetSettingValueByName(string settingName)
        {
            uint id = GetSettingIdFromName(settingName);
            if (id == 0) return "N/A";
            return GetSettingValue(id);
        }

        public bool SetSettingValueByName(string settingName, string recommendedValue)
        {
            uint id = GetSettingIdFromName(settingName);
            if (id == 0) return false;

            if (uint.TryParse(recommendedValue, out uint val))
            {
                return SetSettingValue(id, val);
            }
            return false;
        }

        public string GetSettingValue(uint settingId)
        {
            if (!IsAvailable) return "N/A";
            try
            {
                using var session = DriverSettingsSession.CreateAndLoad();
                var profile = GetProfileObject(session);
                if (profile == null) return "0";

                var profileType = profile.GetType();
                var getSettingMeth = profileType.GetMethod("GetSetting", new[] { typeof(uint) }) 
                                  ?? profileType.GetMethod("GetSetting", new[] { typeof(int) });

                if (getSettingMeth != null)
                {
                    var settingObj = getSettingMeth.Invoke(profile, new object[] { settingId });
                    if (settingObj != null)
                    {
                        var valProp = settingObj.GetType().GetProperty("CurrentValue") 
                                   ?? settingObj.GetType().GetProperty("Value");
                        return valProp?.GetValue(settingObj)?.ToString() ?? "0";
                    }
                }

                // Search for any GetSetting method on profile
                foreach (var m in profileType.GetMethods())
                {
                    if (m.Name.Equals("GetSetting", StringComparison.OrdinalIgnoreCase))
                    {
                        var pars = m.GetParameters();
                        if (pars.Length == 1)
                        {
                            try
                            {
                                var arg = Convert.ChangeType(settingId, pars[0].ParameterType);
                                var settingObj = m.Invoke(profile, new object[] { arg! });
                                if (settingObj != null)
                                {
                                    var valProp = settingObj.GetType().GetProperty("CurrentValue") ?? settingObj.GetType().GetProperty("Value");
                                    return valProp?.GetValue(settingObj)?.ToString() ?? "0";
                                }
                            }
                            catch { }
                        }
                    }
                }

                return "0";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVAPI GetSettingValue error (0x{settingId:X}): {ex.Message}");
                return "N/A";
            }
        }

        public bool SetSettingValue(uint settingId, uint value)
        {
            if (!IsAvailable) return false;
            try
            {
                using var session = DriverSettingsSession.CreateAndLoad();
                var profile = GetProfileObject(session);
                if (profile == null) return false;

                var profileType = profile.GetType();
                foreach (var m in profileType.GetMethods())
                {
                    if (m.Name.Equals("SetSetting", StringComparison.OrdinalIgnoreCase))
                    {
                        var pars = m.GetParameters();
                        if (pars.Length == 2)
                        {
                            try
                            {
                                var arg1 = Convert.ChangeType(settingId, pars[0].ParameterType);
                                var arg2 = Convert.ChangeType(value, pars[1].ParameterType);
                                m.Invoke(profile, new object[] { arg1!, arg2! });
                                session.Save();
                                return true;
                            }
                            catch { }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVAPI SetSettingValue error (0x{settingId:X} -> {value}): {ex.Message}");
                return false;
            }
        }

        private object? GetProfileObject(DriverSettingsSession session)
        {
            try
            {
                var sessionType = session.GetType();
                var prop = sessionType.GetProperty("GlobalProfile") 
                        ?? sessionType.GetProperty("BaseProfile")
                        ?? sessionType.GetProperty("CurrentProfile");
                if (prop != null) return prop.GetValue(session);

                foreach (var m in sessionType.GetMethods())
                {
                    if (m.Name.Contains("Profile", StringComparison.OrdinalIgnoreCase) && m.GetParameters().Length == 0)
                    {
                        try
                        {
                            var res = m.Invoke(session, null);
                            if (res != null) return res;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetProfileObject error: {ex.Message}");
            }
            return null;
        }

        private uint GetSettingIdFromName(string name)
        {
            if (name.Contains("Power Management", StringComparison.OrdinalIgnoreCase)) return POWER_MANAGEMENT_MODE_ID;
            if (name.Contains("Low Latency", StringComparison.OrdinalIgnoreCase)) return LOW_LATENCY_MODE_ID;
            if (name.Contains("Shader Cache", StringComparison.OrdinalIgnoreCase)) return SHADER_CACHE_SIZE_ID;
            if (name.Contains("Texture Filtering", StringComparison.OrdinalIgnoreCase)) return TEXTURE_FILTERING_ID;
            if (name.Contains("Threaded Optimization", StringComparison.OrdinalIgnoreCase)) return THREADED_OPTIMIZATION_ID;
            return 0;
        }
    }
}
