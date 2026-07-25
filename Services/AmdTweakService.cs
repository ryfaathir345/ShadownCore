using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinTweakStudio.Services
{
    public interface IAmdTweakService
    {
        bool IsAvailable { get; }
        string GetSettingValue(string settingName);
        bool SetSettingValue(string settingName, string recommendedValue);
    }

    public class AmdTweakService : IAmdTweakService
    {
        private const string Atiadlxx = "atiadlxx.dll";
        private const string AdlA = "adl_a.dll";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr ADL_Main_Memory_Alloc(int size);

        [DllImport(Atiadlxx, EntryPoint = "ADL_Main_Control_Create", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Main_Control_Create_64(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);

        [DllImport(Atiadlxx, EntryPoint = "ADL_Main_Control_Destroy", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Main_Control_Destroy_64();

        [DllImport(AdlA, EntryPoint = "ADL_Main_Control_Create", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Main_Control_Create_32(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);

        [DllImport(AdlA, EntryPoint = "ADL_Main_Control_Destroy", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Main_Control_Destroy_32();

        private static readonly ADL_Main_Memory_Alloc AllocDelegate = Main_Memory_Alloc;

        private static IntPtr Main_Memory_Alloc(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        private bool _isAvailable;
        private bool _initAttempted;

        public bool IsAvailable
        {
            get
            {
                EnsureInitialized();
                return _isAvailable;
            }
        }

        private void EnsureInitialized()
        {
            if (_initAttempted) return;
            _initAttempted = true;

            try
            {
                int res = -1;
                try
                {
                    res = ADL_Main_Control_Create_64(AllocDelegate, 1);
                }
                catch
                {
                    try
                    {
                        res = ADL_Main_Control_Create_32(AllocDelegate, 1);
                    }
                    catch { }
                }

                _isAvailable = (res == 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AMD ADL Initialization error: {ex.Message}");
                _isAvailable = false;
            }
        }

        public string GetSettingValue(string settingName)
        {
            if (!IsAvailable) return "N/A";

            if (settingName.Contains("Anti-Lag", StringComparison.OrdinalIgnoreCase))
            {
                return "0"; // Default Off
            }
            if (settingName.Contains("Sharpening", StringComparison.OrdinalIgnoreCase))
            {
                return "0"; // Default Sharpening
            }

            return "0";
        }

        public bool SetSettingValue(string settingName, string recommendedValue)
        {
            if (!IsAvailable) return false;

            try
            {
                Debug.WriteLine($"AMD ADL SetSetting {settingName} -> {recommendedValue}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AMD ADL Error: {ex.Message}");
                return false;
            }
        }
    }
}
