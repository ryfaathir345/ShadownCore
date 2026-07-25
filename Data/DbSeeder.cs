using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using WinTweakStudio.Models;

namespace WinTweakStudio.Data
{
    public static class DbSeeder
    {
        public static void Seed(SqliteConnection connection)
        {
            var tweaksToSeed = new List<TweakDefinition>();
            tweaksToSeed.AddRange(GetGpuSeedData());
            tweaksToSeed.AddRange(GetCpuSeedData());
            tweaksToSeed.AddRange(GetRamSeedData());
            tweaksToSeed.AddRange(GetNetworkSeedData());
            tweaksToSeed.AddRange(GetWindowsSeedData());
            tweaksToSeed.AddRange(GetServiceSeedData());
            tweaksToSeed.AddRange(GetDebloatSeedData());
            tweaksToSeed.AddRange(GetStorageSeedData());
            tweaksToSeed.AddRange(GetBootPowerSeedData());

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var tweak in tweaksToSeed)
                {
                    var insertCmd = connection.CreateCommand();
                    insertCmd.Transaction = transaction;
                    insertCmd.CommandText = @"
                        INSERT OR REPLACE INTO TweakDefinitions 
                        (Id, Name, Description, Category, SubCategory, RiskLevel, Type, TargetPath, ValueName, DefaultValue, RecommendedValue, RequiresSecurityWarning)
                        VALUES 
                        (@Id, @Name, @Description, @Category, @SubCategory, @RiskLevel, @Type, @TargetPath, @ValueName, @DefaultValue, @RecommendedValue, @RequiresSecurityWarning);
                    ";

                    insertCmd.Parameters.AddWithValue("@Id", tweak.Id);
                    insertCmd.Parameters.AddWithValue("@Name", tweak.Name);
                    insertCmd.Parameters.AddWithValue("@Description", tweak.Description);
                    insertCmd.Parameters.AddWithValue("@Category", tweak.Category.ToString());
                    insertCmd.Parameters.AddWithValue("@SubCategory", tweak.SubCategory);
                    insertCmd.Parameters.AddWithValue("@RiskLevel", tweak.RiskLevel.ToString());
                    insertCmd.Parameters.AddWithValue("@Type", tweak.Type.ToString());
                    insertCmd.Parameters.AddWithValue("@TargetPath", tweak.TargetPath);
                    insertCmd.Parameters.AddWithValue("@ValueName", tweak.ValueName);
                    insertCmd.Parameters.AddWithValue("@DefaultValue", tweak.DefaultValue);
                    insertCmd.Parameters.AddWithValue("@RecommendedValue", tweak.RecommendedValue);
                    insertCmd.Parameters.AddWithValue("@RequiresSecurityWarning", tweak.RequiresSecurityWarning ? 1 : 0);

                    insertCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static List<TweakDefinition> GetGpuSeedData()
        {
            return new List<TweakDefinition>
            {
                // === GPU - General ===
                new TweakDefinition
                {
                    Id = "GPU-GEN-01",
                    Name = "Hardware-Accelerated GPU Scheduling",
                    Description = "Memindahkan manajemen memori GPU dari CPU driver ke GPU scheduler, mengurangi latency sisi CPU. Bisa mengganggu software capture/streaming.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                    ValueName = "HwSchMode",
                    DefaultValue = "1",
                    RecommendedValue = "2"
                },
                new TweakDefinition
                {
                    Id = "GPU-GEN-02",
                    Name = "Disable Fullscreen Optimizations",
                    Description = "Memaksa game pakai exclusive fullscreen asli, mengurangi input lag di beberapa game. Diterapkan per-aplikasi lewat Properties > Compatibility.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-GEN-03",
                    Name = "GPU Priority untuk Games",
                    Description = "Menaikkan prioritas alokasi GPU untuk proses game.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                    ValueName = "GPU Priority",
                    DefaultValue = "2",
                    RecommendedValue = "8"
                },
                new TweakDefinition
                {
                    Id = "GPU-GEN-04",
                    Name = "Scheduling Category untuk Games",
                    Description = "Set task scheduling category game ke High untuk prioritas CPU/GPU lebih baik.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                    ValueName = "Scheduling Category",
                    DefaultValue = "Medium",
                    RecommendedValue = "High"
                },
                new TweakDefinition
                {
                    Id = "GPU-GEN-05",
                    Name = "System Responsiveness",
                    Description = "Menurunkan alokasi CPU untuk background task saat multimedia/game berjalan.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    ValueName = "SystemResponsiveness",
                    DefaultValue = "20",
                    RecommendedValue = "10"
                },
                new TweakDefinition
                {
                    Id = "GPU-GEN-06",
                    Name = "Disable Network Throttling untuk Multimedia",
                    Description = "Menghilangkan throttling bandwidth reservasi untuk proses multimedia/game.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    ValueName = "NetworkThrottlingIndex",
                    DefaultValue = "a",
                    RecommendedValue = "0xffffffff"
                },
                new TweakDefinition
                {
                    Id = "GPU-GEN-07",
                    Name = "Set Default High-Performance GPU (Hybrid Laptop)",
                    Description = "Pastikan Windows memilih dGPU bukan iGPU untuk game pada sistem hybrid GPU.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },

                // === GPU - NVIDIA ===
                new TweakDefinition
                {
                    Id = "GPU-NV-08",
                    Name = "Power Management Mode - Prefer Maximum Performance",
                    Description = "Menghilangkan delay boost clock GPU di awal scene berat, penting untuk 1% lows. Diatur otomatis via NVAPI.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.NvApi,
                    TargetPath = "Power Management",
                    ValueName = "PREFERRED_PSTATE",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-NV-09",
                    Name = "Low Latency Mode - Ultra",
                    Description = "Membatasi pre-rendered frames jadi 1, mengurangi input lag. Diatur otomatis via NVAPI.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.NvApi,
                    TargetPath = "Low Latency",
                    ValueName = "ULTRA_LOW_LATENCY_MODE",
                    DefaultValue = "0",
                    RecommendedValue = "2"
                },
                new TweakDefinition
                {
                    Id = "GPU-NV-10",
                    Name = "Shader Cache Size 10GB",
                    Description = "Mencegah stutter re-compile shader di game open-world besar. Diatur otomatis via NVAPI.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.NvApi,
                    TargetPath = "Shader Cache",
                    ValueName = "PS_SHADERCACHE_MAXSIZE",
                    DefaultValue = "0",
                    RecommendedValue = "10240"
                },
                new TweakDefinition
                {
                    Id = "GPU-NV-11",
                    Name = "Texture Filtering Quality - Performance",
                    Description = "Menurunkan kualitas visual sedikit untuk gain FPS terukur. Diatur otomatis via NVAPI.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.NvApi,
                    TargetPath = "Texture Filtering",
                    ValueName = "PS_TEXFILTER_QUALITY",
                    DefaultValue = "0",
                    RecommendedValue = "10"
                },
                new TweakDefinition
                {
                    Id = "GPU-NV-12",
                    Name = "Threaded Optimization On",
                    Description = "Memungkinkan banyak thread CPU menangani draw call secara paralel. Diatur otomatis via NVAPI.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.NvApi,
                    TargetPath = "Threaded Optimization",
                    ValueName = "OGL_THREADED_OPTIMIZATION",
                    DefaultValue = "2",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-NV-13",
                    Name = "Disable NVIDIA Telemetry Service",
                    Description = "Mematikan service telemetry NVIDIA yang berjalan di background.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "NvTelemetryContainer",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "GPU-NV-14",
                    Name = "Disable Dynamic P-State (Desktop Only)",
                    Description = "Mengunci GPU di P-State performa maksimum, mencegah down-clock dinamis. HANYA untuk desktop PC, wajib pastikan suhu GPU aman sebelum apply.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000",
                    ValueName = "DisableDynamicPstate",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },

                // === GPU - AMD ===
                new TweakDefinition
                {
                    Id = "GPU-AMD-15",
                    Name = "Radeon Anti-Lag / Anti-Lag 2",
                    Description = "Mengurangi input latency dengan mengatur antrian frame CPU-ke-GPU. Diatur otomatis via AMD ADL.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Adl,
                    TargetPath = "Radeon Anti-Lag",
                    ValueName = "AntiLag",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-16",
                    Name = "Image Sharpening 80%",
                    Description = "Mengompensasi softness dari FSR/resolution scaling. Diatur otomatis via AMD ADL.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Adl,
                    TargetPath = "Image Sharpening",
                    ValueName = "Sharpening",
                    DefaultValue = "0",
                    RecommendedValue = "80"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-17",
                    Name = "Disable AMD External Events Utility",
                    Description = "Mengurangi background process jika hotkey AMD tidak dibutuhkan.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "AMDExternalEvents",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-18",
                    Name = "HYPR-RX",
                    Description = "Preset satu-klik gabungan Anti-Lag, Boost, dan frame optimization. Diatur lewat Adrenalin Edition.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-19",
                    Name = "Smart Access Memory (SAM)",
                    Description = "Memberi CPU akses penuh ke VRAM GPU, butuh dukungan motherboard/BIOS + CPU AMD. Diatur lewat BIOS + Adrenalin toggle.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-20",
                    Name = "Radeon Boost",
                    Description = "Menurunkan resolusi render sementara saat gerakan cepat untuk menaikkan FPS. Diatur lewat Adrenalin Edition.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-21",
                    Name = "Radeon Chill",
                    Description = "Membatasi FPS dinamis berdasar gerakan layar untuk hemat daya/panas. Kurang cocok untuk competitive play. Diatur lewat Adrenalin Edition.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-22",
                    Name = "FreeSync + Frame Cap",
                    Description = "Menghilangkan tearing sekaligus menjaga latency rendah. Diatur lewat Adrenalin Edition + Windows Display Settings.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-23",
                    Name = "Wait for Vertical Refresh Off",
                    Description = "Mematikan V-Sync driver-level demi latency lebih rendah. Diatur lewat Adrenalin Edition.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-24",
                    Name = "Tessellation Mode AMD Optimized",
                    Description = "Membatasi level tessellation berlebihan tanpa banyak dampak visual. Diatur lewat Adrenalin Edition.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-25",
                    Name = "Auto Overclock / Rage Mode",
                    Description = "Preset overclock otomatis bawaan driver. Void warranty. Diatur lewat Adrenalin Edition Performance Tuning tab.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-26",
                    Name = "GPU Undervolt Otomatis",
                    Description = "Menurunkan voltage di clock speed yang sama untuk suhu lebih rendah. Butuh testing per-unit. Diatur lewat Adrenalin Edition.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-27",
                    Name = "HBCC Memory Size",
                    Description = "Alokasi system memory tambahan untuk GPU dengan HBM (mis. Radeon VII). Hanya relevan untuk GPU tertentu. Diatur lewat Adrenalin Edition.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-AMD-28",
                    Name = "Disable GPU Energy Driver Service",
                    Description = "Mematikan service GpuEnergyDrv terkait monitoring/estimasi energi GPU.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Service,
                    TargetPath = "GpuEnergyDrv",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },

                // === GPU - Intel ===
                new TweakDefinition
                {
                    Id = "GPU-INT-29",
                    Name = "Increase iGPU Shared VRAM Allocation",
                    Description = "Alokasi RAM lebih besar ke iGPU, berguna untuk game/emulator lama. Nilai keliru bisa sebabkan instabilitas.",
                    Category = TweakCategory.GPU,
                    SubCategory = "Intel",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Intel\GMM",
                    ValueName = "DedicatedSegmentSize",
                    DefaultValue = "0",
                    RecommendedValue = "512"
                },
                new TweakDefinition
                {
                    Id = "GPU-INT-30",
                    Name = "ReBAR (Resizable BAR)",
                    Description = "Wajib untuk performa optimal Arc, gain 30-40% di game yang didukung. HANYA bisa diaktifkan dari BIOS, app hanya cek status dan kasih instruksi.",
                    Category = TweakCategory.GPU,
                    SubCategory = "Intel",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-INT-31",
                    Name = "Intel Graphics Software Performance Tuning",
                    Description = "Menyesuaikan power limit sebelum tuning in-game. Diatur lewat Intel Graphics Software app.",
                    Category = TweakCategory.GPU,
                    SubCategory = "Intel",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "GPU-INT-32",
                    Name = "HwQueuedRenderPacketGroupLimitPerNode",
                    Description = "Mengatur jumlah render packet yang di-queue per node GPU Arc. Efek tidak terdokumentasi resmi, tandai sebagai experimental.",
                    Category = TweakCategory.GPU,
                    SubCategory = "Intel",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000",
                    ValueName = "HwQueuedRenderPacketGroupLimitPerNode",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },

                // === GPU Tweaks Baru 2026 (ShadownTweak & DirectStorage) ===
                new TweakDefinition
                {
                    Id = "GPU-SHA-01",
                    Name = "Shader Cache Optimizer",
                    Description = "Mengoptimalkan batas ukuran shader cache di Windows Registry untuk mengurangi micro-stuttering pada game DX11/DX12/Vulkan.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\System",
                    ValueName = "ShaderCacheMaxSize",
                    DefaultValue = "0",
                    RecommendedValue = "10240"
                },
                new TweakDefinition
                {
                    Id = "GPU-SHA-02",
                    Name = "Optimizations for Windowed Games",
                    Description = "Menghilangkan perbedaan latency antara mode Borderless Windowed dan Exclusive Fullscreen pada Windows 11.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
                    ValueName = "DirectXUserGlobalSettings",
                    DefaultValue = "0",
                    RecommendedValue = "SwapEffectUpgradeEnable=1;"
                },
                new TweakDefinition
                {
                    Id = "GPU-SHA-03",
                    Name = "DXGI Flip Model Upgrade",
                    Description = "Memaksa game legasi menggunakan presentation Flip Model modern untuk input lag terendah.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\SOFTWARE\Microsoft\Direct3D",
                    ValueName = "EnableFlipModel",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-SHA-04",
                    Name = "DirectStorage GPU Cache Enabler",
                    Description = "Mengaktifkan alokasi GPU memory cache untuk teknologi DirectStorage pada NVMe & Modern GPU.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\DirectStorage",
                    ValueName = "UseGPUOptimizedMedia",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-SHA-05",
                    Name = "Auto Super Resolution (Auto SR)",
                    Description = "Mengaktifkan fitur AI Super Resolution bawaan Windows 11 24H2+ untuk peningkatan FPS otomatis tanpa bergantung pada game developer.",
                    Category = TweakCategory.GPU,
                    SubCategory = "General",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\DirectX\UserGpuPreferences",
                    ValueName = "AutoSRGlobalEnable",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-SHA-06",
                    Name = "NVIDIA Maximum Pre-Rendered Frames (Force 1)",
                    Description = "Memaksa antrian frame CPU-ke-GPU di angka 1 untuk meminimalkan keterlambatan input (Input Lag) pada kartu NVIDIA.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.NvApi,
                    TargetPath = "Pre-Rendered Frames",
                    ValueName = "MAX_PRERENDERED_FRAMES",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-SHA-07",
                    Name = "NVIDIA Anisotropic Filtering Optimization",
                    Description = "Mengoptimalkan filter tekstur anisotropik untuk peningkatan performa tanpa mengorbankan ketajaman gambar.",
                    Category = TweakCategory.GPU,
                    SubCategory = "NVIDIA",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.NvApi,
                    TargetPath = "Anisotropic Filtering",
                    ValueName = "AF_OPTIMIZATION",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-SHA-08",
                    Name = "AMD Surface Format Optimization",
                    Description = "Mengizinkan driver AMD mengganti format permukaan tekstur ke format yang lebih cepat di-render oleh GPU Radeon.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Adl,
                    TargetPath = "Surface Format Optimization",
                    ValueName = "SFO_Enable",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "GPU-SHA-09",
                    Name = "AMD Shader Cache Reset & Force Enable",
                    Description = "Memaksa Shader Cache AMD tetap aktif secara permanen untuk mencegah penumpukan cache yang rusak.",
                    Category = TweakCategory.GPU,
                    SubCategory = "AMD",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Adl,
                    TargetPath = "Shader Cache",
                    ValueName = "ShaderCacheMode",
                    DefaultValue = "0",
                    RecommendedValue = "2"
                }
            };
        }

        public static List<TweakDefinition> GetCpuSeedData()
        {
            return new List<TweakDefinition>
            {
                // === SubCategory: Power Plan ===
                new TweakDefinition
                {
                    Id = "CPU-PWR-01",
                    Name = "Ultimate Performance Power Plan",
                    Description = "Power plan tersembunyi Windows, menghilangkan batas minimum processor state dan disable core parking otomatis. Menaikkan respon CPU di skenario CPU-bound.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Power Plan",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Command,
                    TargetPath = "powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "CPU-PWR-02",
                    Name = "Disable Core Parking",
                    Description = "Mencegah Windows 'park' (nonaktifkan sementara) core CPU yang idle. Sangat berguna untuk desktop gaming.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Power Plan",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\0cc5b647-c1df-4637-891a-dec35c318583",
                    ValueName = "Attributes",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "CPU-PWR-03",
                    Name = "Processor Performance Increase/Decrease Threshold",
                    Description = "Mempercepat responsifitas frekuensi clock CPU saat terjadi lonjakan workload dengan meng-expose pengaturan di Power Plan.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Power Plan",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\06cadf0e-46ed-4482-aa12-39f359e200ec",
                    ValueName = "Attributes",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "CPU-PWR-04",
                    Name = "C-States Limit (BIOS)",
                    Description = "Membatasi deep sleep state CPU (C3/C6) agar core lebih responsif. Pengaturan ini diatur langsung melalui BIOS motherboard.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Power Plan",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "CPU-PWR-05",
                    Name = "Disable Processor Idle States Sepenuhnya",
                    Description = "Memaksa semua core CPU tetap di state C0 (fully active). Meningkatkan konsumsi daya dan suhu, disarankan hanya untuk desktop PC.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Power Plan",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\0cc5b647-c1df-4637-891a-dec35c318583",
                    ValueName = "ValueMax",
                    DefaultValue = "100",
                    RecommendedValue = "0"
                },

                // === SubCategory: Scheduling ===
                new TweakDefinition
                {
                    Id = "CPU-SCH-06",
                    Name = "Win32PrioritySeparation",
                    Description = "Mengatur alokasi waktu CPU antara foreground app dan background process. Nilai 26 (0x1A) mengoptimalkan prioritas game foreground.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Scheduling",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl",
                    ValueName = "Win32PrioritySeparation",
                    DefaultValue = "2",
                    RecommendedValue = "26"
                },
                new TweakDefinition
                {
                    Id = "CPU-SCH-07",
                    Name = "Games Task Priority Profile",
                    Description = "Menaikkan prioritas alokasi thread CPU dan penjadwalan multimedia untuk profil tugas Games.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Scheduling",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                    ValueName = "Scheduling Category",
                    DefaultValue = "Medium",
                    RecommendedValue = "High"
                },
                new TweakDefinition
                {
                    Id = "CPU-SCH-08",
                    Name = "System Responsiveness",
                    Description = "Menurunkan persentase alokasi reservasi CPU untuk background task saat game berjalan.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Scheduling",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    ValueName = "SystemResponsiveness",
                    DefaultValue = "20",
                    RecommendedValue = "10"
                },
                new TweakDefinition
                {
                    Id = "CPU-SCH-09",
                    Name = "csrss.exe Priority Boost",
                    Description = "Menaikkan prioritas proses inti Client/Server Runtime Subsystem untuk meminimalkan ketersendatan render GUI.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Scheduling",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions",
                    ValueName = "CpuPriorityClass",
                    DefaultValue = "2",
                    RecommendedValue = "3"
                },
                new TweakDefinition
                {
                    Id = "CPU-SCH-10",
                    Name = "CPU Affinity untuk Multi-CCD Ryzen",
                    Description = "Mengunci proses game hanya pada CCD dengan 3D V-Cache / cache terbesar untuk mencegah latency switching antar CCD.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Scheduling",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },

                // === SubCategory: SMT ===
                new TweakDefinition
                {
                    Id = "CPU-SMT-11",
                    Name = "Disable SMT / Hyper-Threading",
                    Description = "Mematikan multi-threading per core untuk mengurangi latency di game esports tertentu. Diatur via BIOS.",
                    Category = TweakCategory.CPU,
                    SubCategory = "SMT",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },

                // === SubCategory: Background ===
                new TweakDefinition
                {
                    Id = "CPU-BKG-12",
                    Name = "Power Throttling per-App Disable",
                    Description = "Mencegah fitur Windows EcoQoS membatasi resource CPU pada aplikasi yang sedang aktif.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Background",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Power\PowerThrottling",
                    ValueName = "PowerThrottlingOff",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "CPU-BKG-13",
                    Name = "Disable Intel IME Service Overhead",
                    Description = "Panduan mematikan Intel Management Engine Interface di BIOS untuk menghemat siklus CPU background.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Background",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "CPU-BKG-14",
                    Name = "Intel Turbo Boost Max Technology 3.0 Driver",
                    Description = "Menggunakan driver resmi Intel untuk secara otomatis memprioritaskan tugas berat ke core fisik paling cepat.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Background",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },

                // === SubCategory: Security ===
                new TweakDefinition
                {
                    Id = "CPU-SEC-15",
                    Name = "Disable Spectre/Meltdown Mitigations",
                    Description = "Mematikan mitigasi Spectre/Meltdown untuk mengembalikan 5-15% performa CPU. PERINGATAN: Mengorbankan keamanan hardware dari serangan cyber.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Security",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    ValueName = "FeatureSettingsOverride",
                    DefaultValue = "0",
                    RecommendedValue = "3",
                    RequiresSecurityWarning = true
                },
                new TweakDefinition
                {
                    Id = "CPU-SEC-16",
                    Name = "Disable Memory Integrity (Core Isolation / HVCI)",
                    Description = "Mematikan fitur keamanan Core Isolation (HVCI) untuk meningkatkan FPS game. PERINGATAN: Mengorbankan perlindungan kernel dari malware.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Security",
                    RiskLevel = RiskLevel.Advanced,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity",
                    ValueName = "Enabled",
                    DefaultValue = "1",
                    RecommendedValue = "0",
                    RequiresSecurityWarning = true
                },

                // === SubCategory: Monitoring ===
                new TweakDefinition
                {
                    Id = "CPU-MON-17",
                    Name = "CPU Overclock Guidance",
                    Description = "Panduan melakukan tuning voltase dan overclocking menggunakan software resmi vendor (Intel XTU / AMD Ryzen Master) atau BIOS.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Monitoring",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "CPU-MON-18",
                    Name = "Rekomendasi Tool Monitoring Eksternal",
                    Description = "Rekomendasi penggunaan HWiNFO64 untuk pemantauan mendalam terhadap sensor thermal, watt, dan clock speed CPU.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Monitoring",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },

                // === CPU Tweaks Baru 2026 (Ryzen CPPC, Scheduler, Energy Performance Preference) ===
                new TweakDefinition
                {
                    Id = "CPU-SHA-01",
                    Name = "Ryzen CPPC Preferred Cores Optimization",
                    Description = "Memaksa Windows memprioritaskan thread game ke core tercepat (Preferred Cores) pada prosesor AMD Ryzen.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Scheduling",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\893de804-4530-4635-8d6e-0e95c2975854",
                    ValueName = "Attributes",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "CPU-SHA-02",
                    Name = "Energy Performance Preference (EPP 0% Max Performance)",
                    Description = "Memaksa Energy Performance Preference CPU ke nilai 0 (Performance) agar clock speed selalu maksimal tanpa delay latency.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Power Plan",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Command,
                    TargetPath = "powercfg /setactive SCHEME_CURRENT && powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 3b045513-43f7-4407-a9a0-9c329d08773b 0",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "CPU-SHA-03",
                    Name = "Disable Heterogeneous Thread Scheduling Latency",
                    Description = "Mengoptimalkan scheduler thread pada prosesor hybrid (Intel 12th/13th/14th Gen P-Core + E-Core) agar game tidak terlempar ke E-Core.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Scheduling",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\7f24e320-6333-4780-a5d5-b57e9d4e77d1",
                    ValueName = "Attributes",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "CPU-SHA-04",
                    Name = "Disable CPU Parking Unpark Percent Max (100% Unparked)",
                    Description = "Mengunci persentase Unparked CPU Core di angka 100% sehingga seluruh core fisik dan logical aktif penuh.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Power Plan",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Command,
                    TargetPath = "powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 0cc5b647-c1df-4637-891a-dec35c318583 100",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "CPU-SHA-05",
                    Name = "Intel Thermal Velocity Boost (TVB) Response Optimization",
                    Description = "Mengatur batas agresivitas boost clock Intel TVB saat suhu CPU berada di bawah 70°C.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Power Plan",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\45b44565-a664-4d7b-9865-d342d35016c9",
                    ValueName = "Attributes",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "CPU-SHA-06",
                    Name = "System Responsiveness 0% (CPU Reserve for Games)",
                    Description = "Menghilangkan 20% alokasi cadangan CPU bawaan Windows untuk system task, sehingga 100% daya CPU diberikan ke game.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Scheduling",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    ValueName = "SystemResponsiveness",
                    DefaultValue = "20",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "CPU-SHA-07",
                    Name = "Disable TSX (Transactional Synchronization Extensions) Latency",
                    Description = "Menonaktifkan penginstruksian TSX yang dapat menyebabkan sisa komputasi siklus CPU terbuang.",
                    Category = TweakCategory.CPU,
                    SubCategory = "Security",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel",
                    ValueName = "DisableTsx",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                }
            };
        }

        public static List<TweakDefinition> GetRamSeedData()
        {
            return new List<TweakDefinition>
            {
                new TweakDefinition
                {
                    Id = "RAM-MEM-01",
                    Name = "SvcHostSplitThresholdInKB",
                    Description = "Mengatur threshold memori RAM sebelum Windows memisahkan svchost.exe menjadi proses individual. Nilai dihitung otomatis berdasar total RAM sistem untuk meminimalkan overhead.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Memory Management",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control",
                    ValueName = "SvcHostSplitThresholdInKB",
                    DefaultValue = "3800000",
                    RecommendedValue = "Auto"
                },
                new TweakDefinition
                {
                    Id = "RAM-MEM-02",
                    Name = "Disable Superfetch / SysMain",
                    Description = "Mematikan service SysMain yang men-cache aplikasi ke RAM. PERHATIAN (Situational): Dapat membebaskan RAM di SSD cepat, tetapi pada sebagian sistem dapat sedikit memperlambat peluncuran aplikasi pertama kali.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Services",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Service,
                    TargetPath = "SysMain",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "RAM-MEM-03",
                    Name = "Disable Memory Compression",
                    Description = "Mematikan fitur kompresi RAM Windows via PowerShell. Bermanfaat untuk CPU kuat dengan kapasitas RAM besar untuk mengurangi siklus CPU.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Memory Management",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.PowerShell,
                    TargetPath = "Disable-MMAgent -MemoryCompression",
                    ValueName = "-",
                    DefaultValue = "Enable-MMAgent -MemoryCompression",
                    RecommendedValue = "Disable-MMAgent -MemoryCompression"
                },
                new TweakDefinition
                {
                    Id = "RAM-MEM-04",
                    Name = "Clear Page File at Shutdown",
                    Description = "Menghapus isi page file secara bersih setiap kali sistem di-shutdown untuk mencegah kebocoran data sensitif (memperpanjang waktu shutdown).",
                    Category = TweakCategory.RAM,
                    SubCategory = "Security & Pagefile",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    ValueName = "ClearPageFileAtShutdown",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "RAM-MEM-05",
                    Name = "Custom Page File Size (Fixed Size)",
                    Description = "Panduan menetapkan ukuran page file tetap (misal 1.5x kapasitas RAM) untuk mengurangi fragmentasi dan overhead resizing dinamis Windows.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Security & Pagefile",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "RAM-MEM-06",
                    Name = "Prioritas RAM untuk Foreground Program",
                    Description = "Memaksa alokasi sistem cache RAM lebih memprioritaskan aplikasi/game yang sedang berjalan di foreground.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Memory Management",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl",
                    ValueName = "Win32PrioritySeparation",
                    DefaultValue = "2",
                    RecommendedValue = "26"
                },
                new TweakDefinition
                {
                    Id = "RAM-MEM-07",
                    Name = "Disable ReadyBoost Service",
                    Description = "Mematikan service ReadyBoost jika sistem menggunakan SSD modern, menghemat background process overhead.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "ReadyBoost",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "RAM-MEM-08",
                    Name = "Standby Memory List Cleaning",
                    Description = "Membersihkan alokasi memori Standby List (cache RAM dari aplikasi yang sudah ditutup) untuk membebaskan kapasitas RAM aktif secara langsung.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Memory Management",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Command,
                    TargetPath = "ClearStandbyList",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "RAM-MEM-09",
                    Name = "XMP / EXPO Profile Enable",
                    Description = "Memastikan modul RAM berjalan pada frekuensi clock terbaik (XMP Intel / EXPO AMD) melalui BIOS motherboard.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Hardware Speed",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },

                // === RAM Tweaks Baru 2026 ===
                new TweakDefinition
                {
                    Id = "RAM-SHA-01",
                    Name = "Working Set Paging Trim (Force Empty Working Set)",
                    Description = "Memaksa pembersihan working set memori pada proses yang pasif untuk mengembalikan alokasi RAM secara instan.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Memory Management",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Command,
                    TargetPath = "EmptyWorkingSet",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "RAM-SHA-02",
                    Name = "Disable Memory Compression Overhead",
                    Description = "Mematikan kompresi RAM via PowerShell untuk menghemat siklus pemrosesan CPU di PC dengan RAM 16GB+.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Memory Management",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Disable-MMAgent -MemoryCompression",
                    ValueName = "-",
                    DefaultValue = "Enable-MMAgent -MemoryCompression",
                    RecommendedValue = "Disable-MMAgent -MemoryCompression"
                },
                new TweakDefinition
                {
                    Id = "RAM-SHA-03",
                    Name = "Disable Dynamic Pagefile Growth Thrashing",
                    Description = "Mengunci alokasi awal Pagefile agar sama dengan batas maksimumnya untuk mencegah overhead alokasi memori berulang.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Security & Pagefile",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    ValueName = "PagingFilesExemption",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "RAM-SHA-04",
                    Name = "NonPagedPoolLimit Max Optimization",
                    Description = "Mengalokasikan batas maksimum memori NonPaged Pool untuk driver dan kernel.",
                    Category = TweakCategory.RAM,
                    SubCategory = "Memory Management",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    ValueName = "NonPagedPoolLimit",
                    DefaultValue = "0",
                    RecommendedValue = "0"
                }
            };
        }

        public static List<TweakDefinition> GetNetworkSeedData()
        {
            return new List<TweakDefinition>
            {
                new TweakDefinition
                {
                    Id = "NET-LAT-01",
                    Name = "Disable Nagle's Algorithm",
                    Description = "Mencegah TCP membundle paket kecil sebelum dikirim untuk mengurangi latency 5-40ms di game berbasis TCP (WoW, FFXIV). PERHATIAN: Tidak berpengaruh untuk game kompetitif modern berbasis UDP (CS2, Valorant, Apex, Fortnite).",
                    Category = TweakCategory.Network,
                    SubCategory = "DNS & Latency",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces",
                    ValueName = "TcpAckFrequency,TCPNoDelay",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "NET-LAT-02",
                    Name = "Disable Network Throttling Index",
                    Description = "Menghilangkan pembatasan (throttling) throughput jaringan secara otomatis oleh Windows saat aplikasi multimedia/game sedang berjalan.",
                    Category = TweakCategory.Network,
                    SubCategory = "DNS & Latency",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                    ValueName = "NetworkThrottlingIndex",
                    DefaultValue = "10",
                    RecommendedValue = "4294967295"
                },
                new TweakDefinition
                {
                    Id = "NET-DNS-03",
                    Name = "Custom High-Performance DNS",
                    Description = "Mengganti DNS default ISP ke provider ultra-cepat (Cloudflare 1.1.1.1, Google 8.8.8.8, atau Quad9 9.9.9.9) untuk mempercepat resolusi domain dan mengurangi latency.",
                    Category = TweakCategory.Network,
                    SubCategory = "DNS & Latency",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Cloudflare",
                    ValueName = "-",
                    DefaultValue = "DHCP",
                    RecommendedValue = "Cloudflare"
                },
                new TweakDefinition
                {
                    Id = "NET-TCP-04",
                    Name = "TCP Auto-Tuning Level (Normal)",
                    Description = "Memastikan fitur TCP Window Auto-Tuning berada pada level 'Normal' untuk memaksimalkan throughput jaringan di Windows 11 (Jangan di-Disable).",
                    Category = TweakCategory.Network,
                    SubCategory = "DNS & Latency",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Command,
                    TargetPath = "netsh int tcp set global autotuninglevel=normal",
                    ValueName = "-",
                    DefaultValue = "normal",
                    RecommendedValue = "normal"
                },
                new TweakDefinition
                {
                    Id = "NET-ADP-05",
                    Name = "Network Adapter Maximum Performance",
                    Description = "Panduan mematikan fitur hemat daya pada properti lanjutan Network Adapter (Device Manager) untuk mencegah micro-latency spike.",
                    Category = TweakCategory.Network,
                    SubCategory = "Adapter Settings",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "NET-ADP-06",
                    Name = "Disable Large Send Offload (LSO)",
                    Description = "Mematikan Large Send Offload pada adapter jaringan untuk mencegah jitter/latency spike pada beberapa jenis kombinasi driver dan game.",
                    Category = TweakCategory.Network,
                    SubCategory = "Adapter Settings",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.PowerShell,
                    TargetPath = "Disable-NetAdapterLso -Name \"*\" -Confirm:$false",
                    ValueName = "-",
                    DefaultValue = "Enable-NetAdapterLso -Name \"*\" -Confirm:$false",
                    RecommendedValue = "Disable-NetAdapterLso -Name \"*\" -Confirm:$false"
                },
                new TweakDefinition
                {
                    Id = "NET-ADP-07",
                    Name = "Disable Network Adapter Power Saving",
                    Description = "Mencegah Windows mematikan perangkat kartu jaringan secara otomatis untuk menghemat daya yang dapat menyebabkan delay wake-up.",
                    Category = TweakCategory.Network,
                    SubCategory = "Adapter Settings",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Get-NetAdapterPowerManagement | Set-NetAdapterPowerManagement -AllowComputerToTurnOffDevice Disabled",
                    ValueName = "-",
                    DefaultValue = "Enabled",
                    RecommendedValue = "Disabled"
                },
                new TweakDefinition
                {
                    Id = "NET-BWD-08",
                    Name = "QoS Packet Scheduler Reserve Bandwidth Limit",
                    Description = "Menghilangkan alokasi reservasi bandwidth 20% milik Windows QoS sehingga 100% kapasitas bandwidth dapat dipakai seluruhnya oleh aplikasi pengguna.",
                    Category = TweakCategory.Network,
                    SubCategory = "Bandwidth",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\Psched",
                    ValueName = "NonBestEffortLimit",
                    DefaultValue = "20",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "NET-BWD-09",
                    Name = "Disable Windows Update P2P Delivery Optimization",
                    Description = "Mematikan fitur Delivery Optimization agar PC tidak secara diam-diam mengunggah/mengunduh update Windows dari PC lain di internet/LAN.",
                    Category = TweakCategory.Network,
                    SubCategory = "Bandwidth",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config",
                    ValueName = "DODownloadMode",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "NET-IP-10",
                    Name = "Static IP Configuration Guidance",
                    Description = "Panduan mengonfigurasi IP Statis untuk mempercepat koneksi dan negosiasi jaringan. Informasi IP, Subnet Mask, dan Gateway aktif Anda ditampilkan secara langsung di card ini.",
                    Category = TweakCategory.Network,
                    SubCategory = "IP Configuration",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },

                // === Network Tweaks Baru 2026 ===
                new TweakDefinition
                {
                    Id = "NET-SHA-01",
                    Name = "DNS Latency Optimizer (Cloudflare & Google Fast Resolving)",
                    Description = "Mengganti DNS adapter utama secara otomatis ke Cloudflare (1.1.1.1) & Google (8.8.8.8) untuk latensi resolving terendah.",
                    Category = TweakCategory.Network,
                    SubCategory = "DNS & Latency",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Set-DnsClientServerAddress -InterfaceAlias (Get-NetAdapter | Where-Object Status -eq 'Up').Name -ServerAddresses ('1.1.1.1','8.8.8.8')",
                    ValueName = "-",
                    DefaultValue = "DHCP",
                    RecommendedValue = "1.1.1.1"
                },
                new TweakDefinition
                {
                    Id = "NET-SHA-02",
                    Name = "TCP Chimney Offload & RSS Max Queues",
                    Description = "Memaksa pembagian antrean paket jaringan ke seluruh core CPU (Receive Side Scaling).",
                    Category = TweakCategory.Network,
                    SubCategory = "Adapter Settings",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Command,
                    TargetPath = "netsh int tcp set global rss=enabled chimney=enabled",
                    ValueName = "-",
                    DefaultValue = "disabled",
                    RecommendedValue = "enabled"
                },
                new TweakDefinition
                {
                    Id = "NET-SHA-03",
                    Name = "MTU Auto Optimizer (1500 MSS)",
                    Description = "Mengatur ukuran Maximum Transmission Unit (MTU) terbaik untuk menghindari fragmentasi paket di game online.",
                    Category = TweakCategory.Network,
                    SubCategory = "Adapter Settings",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Command,
                    TargetPath = "netsh interface ipv4 set subinterface \"Ethernet\" mtu=1500 store=persistent",
                    ValueName = "-",
                    DefaultValue = "1500",
                    RecommendedValue = "1500"
                },
                new TweakDefinition
                {
                    Id = "NET-SHA-04",
                    Name = "Disable Network Interrupt Moderation Spike",
                    Description = "Mematikan jeda moderasi interupsi kartu jaringan untuk mendapatkan waktu respon paket (ping) secara real-time tanpa penundaan.",
                    Category = TweakCategory.Network,
                    SubCategory = "DNS & Latency",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.PowerShell,
                    TargetPath = "Disable-NetAdapterInterruptModeration -Name \"*\" -Confirm:$false",
                    ValueName = "-",
                    DefaultValue = "Enabled",
                    RecommendedValue = "Disabled"
                }
            };
        }

        public static List<TweakDefinition> GetWindowsSeedData()
        {
            return new List<TweakDefinition>
            {
                new TweakDefinition
                {
                    Id = "WIN-UI-01",
                    Name = "Disable Visual Effects & Animations",
                    Description = "Mematikan animasi jendela, bayangan, dan efek visual Windows untuk menghemat pemakaian resource GPU/CPU.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Control Panel\Desktop",
                    ValueName = "UserPreferencesMask",
                    DefaultValue = "9012078010000000",
                    RecommendedValue = "9012028010000000"
                },
                new TweakDefinition
                {
                    Id = "WIN-UI-02",
                    Name = "MenuShowDelay Reduction",
                    Description = "Mengurangi waktu tunggu (delay) sebelum menu pop-up/context menu muncul dari 400ms menjadi 0ms untuk respon UI yang sangat responsif.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Control Panel\Desktop",
                    ValueName = "MenuShowDelay",
                    DefaultValue = "400",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-UI-03",
                    Name = "AutoEndTasks & Shutdown Timeout Reduction",
                    Description = "Mempercepat Windows dalam mendeteksi dan menghentikan aplikasi tidak merespon (hung app) secara otomatis saat shutdown.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Control Panel\Desktop",
                    ValueName = "AutoEndTasks",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "WIN-SYS-04",
                    Name = "Disable Background Apps (Global)",
                    Description = "Mencegah seluruh aplikasi UWP / Microsoft Store berjalan secara independen di background tanpa dibuka oleh pengguna.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Background Apps",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
                    ValueName = "GlobalUserDisabled",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "WIN-SYS-05",
                    Name = "Disable Microsoft Edge Pre-Launching",
                    Description = "Mematikan proses pre-load Microsoft Edge di background saat startup Windows untuk membebaskan alokasi RAM.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Background Apps",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\MicrosoftEdge\Main",
                    ValueName = "AllowPrelaunch",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-SYS-06",
                    Name = "Disable Windows Ads & Suggestions",
                    Description = "Mematikan rekomendasi aplikasi, iklan, dan saran promosi di Start Menu, Settings, dan Lockscreen.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Background Apps",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                    ValueName = "SystemPaneSuggestionsEnabled",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-SYS-07",
                    Name = "Disable Windows Search Indexing (WSearch)",
                    Description = "Mematikan service indeks pencarian file background (WSearch). PERHATIAN (Situational): Membebaskan I/O disk secara signifikan di SSD/HDD, tetapi pencarian file via File Explorer akan menjadi lebih lambat.",
                    Category = TweakCategory.Windows,
                    SubCategory = "System Services & Power",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Service,
                    TargetPath = "WSearch",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "WIN-GAM-08",
                    Name = "Disable Xbox Game Bar & Game DVR",
                    Description = "Mematikan fitur Xbox Game Bar dan perekaman latar belakang (Game DVR) untuk mencegah masalah overhead CPU/GPU dan konflik overlay.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Gaming & Overlay",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\System\GameConfigStore",
                    ValueName = "GameDVR_Enabled",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-DIAG-09",
                    Name = "Enable Verbose Boot & Shutdown Messages",
                    Description = "Menampilkan pesan detail status teknis saat proses Booting dan Shutdown untuk mempermudah diagnosa jika terjadi masalah delay.",
                    Category = TweakCategory.Windows,
                    SubCategory = "System Diagnostics",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                    ValueName = "VerboseStatus",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "WIN-AI-10",
                    Name = "Disable Windows Copilot & AI Features",
                    Description = "Mematikan fitur Copilot AI bawaan Windows 11 untuk menghentikan proses latar belakang berat di sistem non-NPU.",
                    Category = TweakCategory.Windows,
                    SubCategory = "AI & Telemetry",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot",
                    ValueName = "TurnOffWindowsCopilot",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "WIN-SYS-11",
                    Name = "Disable Notification Center Sync & Toasts",
                    Description = "Mengurangi overhead sinkronisasi notifikasi latar belakang (Phone Link / Push Notifications) yang konsisten berjalan.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Background Apps",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\PushNotifications",
                    ValueName = "ToastEnabled",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-UI-12",
                    Name = "Disable Widgets Panel (Taskbar News & Weather)",
                    Description = "Mematikan panel Widgets berita dan cuaca pada Taskbar yang selalu mengunduh data di background.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\Dsh",
                    ValueName = "AllowNewsAndInterests",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-SYS-13",
                    Name = "Disable Fast Startup (Hybrid Boot)",
                    Description = "Mematikan fitur Fast Startup. PERHATIAN (Situational): Waktu boot ulang mungkin sedikit lebih lama, tetapi memecahkan masalah error driver, hibernasi, dan sinkronisasi hardware.",
                    Category = TweakCategory.Windows,
                    SubCategory = "System Services & Power",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power",
                    ValueName = "HiberbootEnabled",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-AI-14",
                    Name = "Disable Windows Telemetry & Diagnostic Data",
                    Description = "Menurunkan tingkat pengumpulan data telemetri diagnostik latar belakang Windows ke tingkat paling minimum untuk privasi dan efisiensi jaringan.",
                    Category = TweakCategory.Windows,
                    SubCategory = "AI & Telemetry",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                    ValueName = "AllowTelemetry",
                    DefaultValue = "3",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-CTX-15",
                    Name = "Restore Classic Windows 10 Right-Click Context Menu",
                    Description = "Mengembalikan tampilan menu klik kanan klasik (Windows 10 style) tanpa sub-menu 'Show more options' yang memperlambat alur kerja.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32",
                    ValueName = "",
                    DefaultValue = "Default",
                    RecommendedValue = ""
                },
                new TweakDefinition
                {
                    Id = "WIN-EXP-16",
                    Name = "Open File Explorer directly to 'This PC'",
                    Description = "Mengatur File Explorer agar terbuka langsung ke tampilan 'This PC' (Drive C/D) alih-alih halaman Home / Quick Access yang lambat memuat.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                    ValueName = "LaunchTo",
                    DefaultValue = "2",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "WIN-EXP-17",
                    Name = "Show Known File Name Extensions",
                    Description = "Menampilkan ekstensi nama file (.exe, .txt, .dll) secara otomatis untuk meningkatkan keamanan dari malware yang menyamar.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                    ValueName = "HideFileExt",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "WIN-PRI-18",
                    Name = "Disable Advertising ID & Targeted Ads Tracking",
                    Description = "Mematikan ID Pengiklanan unik pengguna di Windows untuk mencegah pelacakan kebiasaan aplikasi dan iklan bertarget.",
                    Category = TweakCategory.Windows,
                    SubCategory = "AI & Telemetry",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
                    ValueName = "Enabled",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },

                // === Windows / Display / Input / System / Gaming / AI Tweaks Baru 2026 ===
                new TweakDefinition
                {
                    Id = "DSP-SHA-01",
                    Name = "Variable Refresh Rate (VRR) Global Enabler",
                    Description = "Mengaktifkan dukungan Variable Refresh Rate (G-Sync/FreeSync) bawaan Windows untuk mengurangi tearing layar tanpa memicu V-Sync lag.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Control Panel\GraphicsDrivers",
                    ValueName = "VarRefreshRate",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "DSP-SHA-02",
                    Name = "HDR Auto Calibration & Latency Mode",
                    Description = "Mengoptimalkan pipeline pemrosesan warna High Dynamic Range (HDR) Windows agar tidak menambah input delay pada monitor gaming.",
                    Category = TweakCategory.Windows,
                    SubCategory = "UI & Performance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\VideoSettings",
                    ValueName = "EnableHDRLatencyOptimization",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "INP-SHA-01",
                    Name = "USB Mouse & Keyboard Polling Rate Driver Optimization",
                    Description = "Menonaktifkan pembatasan frekuensi sampel USB (polling rate) Windows untuk memastikan periferal 1000Hz/4000Hz/8000Hz berjalan optimal.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Gaming & Overlay",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Services\mouclass\Parameters",
                    ValueName = "MouseDataQueueSize",
                    DefaultValue = "100",
                    RecommendedValue = "30"
                },
                new TweakDefinition
                {
                    Id = "INP-SHA-02",
                    Name = "HID Input Latency Buffer Reduction",
                    Description = "Mengurangi ukuran antrian buffer Human Interface Device (HID) untuk merespon klik mouse dan penekanan tombol keyboard secara langsung.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Gaming & Overlay",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Services\kbdclass\Parameters",
                    ValueName = "KeyboardDataQueueSize",
                    DefaultValue = "100",
                    RecommendedValue = "30"
                },
                new TweakDefinition
                {
                    Id = "SYS-SHA-01",
                    Name = "Disable Diagnostic Tracking Service (DiagTrack) Override",
                    Description = "Mematikan paksa pengumpulan logs diagnosa sistem otomatis yang berjalan latar belakang saat game aktif.",
                    Category = TweakCategory.Windows,
                    SubCategory = "AI & Telemetry",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "DiagTrack",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SYS-SHA-02",
                    Name = "Disable Windows Event Log Overhead",
                    Description = "Menurunkan verbositas penulisan event log non-kritis ke disk saat sistem dalam performa gaming.",
                    Category = TweakCategory.Windows,
                    SubCategory = "System Diagnostics",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\WMI\Autologger\EventLog-System",
                    ValueName = "Start",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "GAM-SHA-01",
                    Name = "Game Execution High-Priority Override",
                    Description = "Secara otomatis menetapkan prioritas CPU High (CpuPriorityClass = 3) pada executable game yang terdaftar.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Gaming & Overlay",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\valorant-win64-shipping.exe\PerfOptions",
                    ValueName = "CpuPriorityClass",
                    DefaultValue = "2",
                    RecommendedValue = "3"
                },
                new TweakDefinition
                {
                    Id = "GAM-SHA-02",
                    Name = "Frametime Spike Protection (MMCSS High Priority)",
                    Description = "Memaksa Multimedia Class Scheduler Service (MMCSS) mengalokasikan 95% thread CPU untuk frametime game yang lebih mulus.",
                    Category = TweakCategory.Windows,
                    SubCategory = "Gaming & Overlay",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                    ValueName = "Priority",
                    DefaultValue = "2",
                    RecommendedValue = "6"
                },
                new TweakDefinition
                {
                    Id = "AI-SHA-01",
                    Name = "Smart Hardware Risk Score Analyzer Guidance",
                    Description = "Modul kecerdasan bawaan yang secara dinamis menghitung Risk Score sistem dan merekomendasikan tweak paling aman sesuai komponen PC Anda.",
                    Category = TweakCategory.Windows,
                    SubCategory = "AI & Telemetry",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "AI-SHA-02",
                    Name = "Auto-Profile Adaptation Engine Guidance",
                    Description = "Panduan pengaktifan skenario profil performa otomatis berdasar pendeteksian aplikasi aktif (Gaming, Streaming, Work, Battery).",
                    Category = TweakCategory.Windows,
                    SubCategory = "AI & Telemetry",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                }
            };
        }

        public static List<TweakDefinition> GetServiceSeedData()
        {
            return new List<TweakDefinition>
            {
                new TweakDefinition
                {
                    Id = "SVC-HW-01",
                    Name = "Print Spooler Service (Spooler)",
                    Description = "Layanan sistem untuk mengelola antrean pencetakan printer. Dapat dimatikan jika PC tidak terhubung ke printer fisik maupun virtual.",
                    Category = TweakCategory.Service,
                    SubCategory = "System & Hardware Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "Spooler",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-LEG-02",
                    Name = "Fax Service (Fax)",
                    Description = "Layanan pengiriman dan penerimaan dokumen fax yang hampir tidak pernah digunakan pada PC modern.",
                    Category = TweakCategory.Service,
                    SubCategory = "Legacy & Vendor Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "Fax",
                    ValueName = "Start",
                    DefaultValue = "3",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-SEC-03",
                    Name = "Remote Registry Service (RemoteRegistry)",
                    Description = "Memungkinkan pengguna jarak jauh untuk memodifikasi pengaturan registry Windows. Mematikan layanan ini meningkatkan keamanan sistem.",
                    Category = TweakCategory.Service,
                    SubCategory = "Network & Security Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "RemoteRegistry",
                    ValueName = "Start",
                    DefaultValue = "3",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-TEL-04",
                    Name = "Windows Insider Service (wisvc)",
                    Description = "Layanan pengujian pra-rilis build Windows. Dapat dimatikan jika Anda tidak terdaftar sebagai peserta Windows Insider Program.",
                    Category = TweakCategory.Service,
                    SubCategory = "Telemetry & Diagnostics",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "wisvc",
                    ValueName = "Start",
                    DefaultValue = "3",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-HW-05",
                    Name = "Downloaded Maps Manager (MapsBroker)",
                    Description = "Layanan pengelola peta offline yang jarang digunakan pada perangkat PC desktop atau laptop non-touchscreen.",
                    Category = TweakCategory.Service,
                    SubCategory = "System & Hardware Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "MapsBroker",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-LEG-06",
                    Name = "Retail Demo Service (RetailDemo)",
                    Description = "Layanan yang mengontrol mode demonstrasi toko retail yang sama sekali tidak diperlukan pada komputer penggunaan pribadi.",
                    Category = TweakCategory.Service,
                    SubCategory = "Legacy & Vendor Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "RetailDemo",
                    ValueName = "Start",
                    DefaultValue = "3",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-HW-07",
                    Name = "Touch Keyboard and Handwriting Panel Service (TabletInputService)",
                    Description = "Layanan papan ketik layar dan panel tulisan tangan. Sangat aman dimatikan pada komputer non-touchscreen.",
                    Category = TweakCategory.Service,
                    SubCategory = "System & Hardware Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "TabletInputService",
                    ValueName = "Start",
                    DefaultValue = "3",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-HW-08",
                    Name = "Bluetooth Support Service (bthserv)",
                    Description = "Layanan pendukung perangkat Bluetooth. Dapat dimatikan jika komputer tidak memiliki modul atau perangkat Bluetooth.",
                    Category = TweakCategory.Service,
                    SubCategory = "System & Hardware Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "bthserv",
                    ValueName = "Start",
                    DefaultValue = "3",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-LEG-09",
                    Name = "AMD External Events Utility Service",
                    Description = "Layanan latar belakang driver AMD untuk pintasan tombol dan event listener. Aman dimatikan jika fitur hotkey tidak dipakai.",
                    Category = TweakCategory.Service,
                    SubCategory = "Legacy & Vendor Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "AMD External Events Utility",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-HW-10",
                    Name = "Program Compatibility Assistant Service (PcaSvc)",
                    Description = "Layanan pemantau masalah kompatibilitas aplikasi lama. Dapat diubah dari Automatic menjadi Manual untuk menghemat siklus CPU.",
                    Category = TweakCategory.Service,
                    SubCategory = "System & Hardware Services",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "PcaSvc",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "3"
                },
                new TweakDefinition
                {
                    Id = "SVC-TEL-11",
                    Name = "Connected User Experiences and Telemetry (DiagTrack)",
                    Description = "Layanan telemetri diagnostik utama Windows yang mengirimkan data statistik ke server Microsoft secara berkala.",
                    Category = TweakCategory.Service,
                    SubCategory = "Telemetry & Diagnostics",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Service,
                    TargetPath = "DiagTrack",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "SVC-TEL-12",
                    Name = "Windows Error Reporting Service (WerSvc)",
                    Description = "Layanan pelaporan crash dan kendala aplikasi ke Microsoft. Dapat dimatikan untuk menghentikan pengiriman laporan latar belakang.",
                    Category = TweakCategory.Service,
                    SubCategory = "Telemetry & Diagnostics",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "WerSvc",
                    ValueName = "Start",
                    DefaultValue = "3",
                    RecommendedValue = "4"
                }
            };
        }

        public static List<TweakDefinition> GetDebloatSeedData()
        {
            return new List<TweakDefinition>
            {
                new TweakDefinition
                {
                    Id = "DEB-APP-01",
                    Name = "Uninstall Xbox Companion & Game Services",
                    Description = "Menghapus paket aplikasi Xbox App, Xbox Speech, dan Game Overlay bawaan Windows jika Anda tidak menggunakan Xbox Game Pass / fitur Xbox.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "UWP Apps Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Get-AppxPackage -AllUsers *Microsoft.XboxApp* | Remove-AppxPackage -AllUsers",
                    ValueName = "-",
                    DefaultValue = "Installed",
                    RecommendedValue = "Uninstalled"
                },
                new TweakDefinition
                {
                    Id = "DEB-APP-02",
                    Name = "Uninstall Solitaire Collection & Casual Games",
                    Description = "Menghapus game bawaan Microsoft Solitaire Collection dan promosi game kasual bawaan Windows.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "UWP Apps Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Get-AppxPackage -AllUsers *SolitaireCollection* | Remove-AppxPackage -AllUsers",
                    ValueName = "-",
                    DefaultValue = "Installed",
                    RecommendedValue = "Uninstalled"
                },
                new TweakDefinition
                {
                    Id = "DEB-APP-03",
                    Name = "Uninstall 3D Viewer & Mixed Reality Portal",
                    Description = "Menghapus aplikasi 3D Viewer dan Mixed Reality Portal bawaan yang tidak terpakai.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "UWP Apps Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Get-AppxPackage -AllUsers *Microsoft3DViewer* | Remove-AppxPackage -AllUsers; Get-AppxPackage -AllUsers *MixedReality* | Remove-AppxPackage -AllUsers",
                    ValueName = "-",
                    DefaultValue = "Installed",
                    RecommendedValue = "Uninstalled"
                },
                new TweakDefinition
                {
                    Id = "DEB-APP-04",
                    Name = "Uninstall Get Help & Get Started (Tips)",
                    Description = "Menghapus aplikasi bantuan Get Help dan petunjuk penggunaan Get Started bawaan Windows.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "UWP Apps Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Get-AppxPackage -AllUsers *GetHelp* | Remove-AppxPackage -AllUsers; Get-AppxPackage -AllUsers *Getstarted* | Remove-AppxPackage -AllUsers",
                    ValueName = "-",
                    DefaultValue = "Installed",
                    RecommendedValue = "Uninstalled"
                },
                new TweakDefinition
                {
                    Id = "DEB-APP-05",
                    Name = "Uninstall Feedback Hub",
                    Description = "Menghapus aplikasi Feedback Hub bawaan yang digunakan untuk mengirim masukan pengguna ke Microsoft.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "UWP Apps Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Get-AppxPackage -AllUsers *WindowsFeedbackHub* | Remove-AppxPackage -AllUsers",
                    ValueName = "-",
                    DefaultValue = "Installed",
                    RecommendedValue = "Uninstalled"
                },
                new TweakDefinition
                {
                    Id = "DEB-APP-06",
                    Name = "Uninstall Clipchamp Video Editor",
                    Description = "Menghapus aplikasi video editor bawaan Clipchamp jika Anda telah menggunakan video editor lain (Premiere/DaVinci/CapCut).",
                    Category = TweakCategory.Debloat,
                    SubCategory = "UWP Apps Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Get-AppxPackage -AllUsers *Clipchamp* | Remove-AppxPackage -AllUsers",
                    ValueName = "-",
                    DefaultValue = "Installed",
                    RecommendedValue = "Uninstalled"
                },
                new TweakDefinition
                {
                    Id = "DEB-SYS-07",
                    Name = "Remove OneDrive Sync & Auto-Start",
                    Description = "Mematikan proses auto-start dan sinkronisasi latar belakang OneDrive jika Anda tidak menggunakannya untuk penyimpanan cloud.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "System Apps & Sync",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "taskkill /f /im OneDrive.exe; reg delete \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run\" /v \"OneDrive\" /f",
                    ValueName = "-",
                    DefaultValue = "Enabled",
                    RecommendedValue = "Disabled"
                },
                new TweakDefinition
                {
                    Id = "DEB-SYS-08",
                    Name = "Disable Cortana Remnants",
                    Description = "Mematikan sisa-sisa proses latar belakang asisten virtual Cortana yang sudah tidak aktif.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "System Apps & Sync",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                    ValueName = "AllowCortana",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "DEB-SYS-09",
                    Name = "Disable Teams Consumer Auto-Start & Reinstall",
                    Description = "Menghapus Microsoft Teams versi consumer dan memblokir instalasi ulang otomatis melalui Windows Update.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "System Apps & Sync",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Get-AppxPackage -AllUsers *Teams* | Remove-AppxPackage -AllUsers; reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Teams\" /v \"PreventInstallationFromMSI\" /t REG_DWORD /d 1 /f",
                    ValueName = "-",
                    DefaultValue = "Installed",
                    RecommendedValue = "Blocked"
                },
                new TweakDefinition
                {
                    Id = "DEB-SYS-10",
                    Name = "Disable Suggested Actions (Clipboard AI)",
                    Description = "Mematikan fitur Suggested Actions yang memindai teks clipboard di background untuk memberikan saran otomatis.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "System Apps & Sync",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\SmartAction",
                    ValueName = "SmartActionState",
                    DefaultValue = "1",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "DEB-GUI-11",
                    Name = "Debloat & Restore Script Reference Guidance",
                    Description = "Panduan debloat aman & petunjuk reinstall aplikasi bawaan dari Microsoft Store jika Anda memerlukan kembali aplikasi yang telah dihapus.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "Debloat Guidance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "DEB-BRW-12",
                    Name = "Disable Microsoft Edge Background Mode & Startup Boost",
                    Description = "Mematikan proses latar belakang Microsoft Edge (Startup Boost & Background Extensions) agar RAM dan CPU bebas saat browser tidak digunakan.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "Browser & Vendor Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge\" /v \"BackgroundModeEnabled\" /t REG_DWORD /d 0 /f; reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge\" /v \"StartupBoostEnabled\" /t REG_DWORD /d 0 /f",
                    ValueName = "-",
                    DefaultValue = "Enabled",
                    RecommendedValue = "Disabled"
                },
                new TweakDefinition
                {
                    Id = "DEB-VND-13",
                    Name = "Disable NVIDIA Telemetry & Background Spying",
                    Description = "Mematikan layanan NvTelemetryContainer dan pengumpulan statistik latar belakang NVIDIA driver yang tidak berpengaruh pada performa game.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "Browser & Vendor Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "sc config NvTelemetryContainer start= disabled; net stop NvTelemetryContainer",
                    ValueName = "-",
                    DefaultValue = "Automatic",
                    RecommendedValue = "Disabled"
                },
                new TweakDefinition
                {
                    Id = "DEB-BRW-14",
                    Name = "Disable Google Chrome Background Mode & Update Services",
                    Description = "Mencegah Google Chrome berjalan terus menerus di background setelah jendela ditutup dan mengubah service gupdate menjadi Manual.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "Browser & Vendor Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "reg add \"HKLM\\SOFTWARE\\Policies\\Google\\Chrome\" /v \"BackgroundModeEnabled\" /t REG_DWORD /d 0 /f; sc config gupdate start= demand; sc config gupdatem start= demand",
                    ValueName = "-",
                    DefaultValue = "Enabled",
                    RecommendedValue = "Disabled"
                },
                new TweakDefinition
                {
                    Id = "DEB-BRW-15",
                    Name = "Disable Brave Browser Background Services & Analytics",
                    Description = "Mematikan layanan Brave Update background dan pengumpulan telemetry analytics bawaan Brave Browser.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "Browser & Vendor Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "sc config brave start= demand; sc config bravem start= demand",
                    ValueName = "-",
                    DefaultValue = "Automatic",
                    RecommendedValue = "Manual"
                },
                new TweakDefinition
                {
                    Id = "DEB-VND-16",
                    Name = "Disable Intel & AMD Telemetry Background Services",
                    Description = "Mematikan layanan Intel Computing Improvement Program & AMD External Events telemetri latar belakang.",
                    Category = TweakCategory.Debloat,
                    SubCategory = "Browser & Vendor Debloat",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "sc config IntelTelemetry start= disabled; sc config \"AMD External Events Utility\" start= demand",
                    ValueName = "-",
                    DefaultValue = "Automatic",
                    RecommendedValue = "Disabled"
                }
            };
        }

        public static List<TweakDefinition> GetStorageSeedData()
        {
            return new List<TweakDefinition>
            {
                new TweakDefinition
                {
                    Id = "STO-CLN-01",
                    Name = "Delete Windows Update Cache (SoftwareDistribution)",
                    Description = "Menghentikan service Windows Update sementara dan menghapus berkas cache instalasi lama di folder SoftwareDistribution untuk membebaskan ruang penyimpanan disk.",
                    Category = TweakCategory.Storage,
                    SubCategory = "Cleanup & Maintenance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "net stop wuauserv; net stop UsoSvc; Remove-Item -Path \"C:\\Windows\\SoftwareDistribution\\Download\\*\" -Recurse -Force -ErrorAction SilentlyContinue; net start wuauserv; net start UsoSvc",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "Cleaned"
                },
                new TweakDefinition
                {
                    Id = "STO-SSD-02",
                    Name = "TRIM Enable & Optimization for SSD/NVMe",
                    Description = "Memastikan fitur TRIM aktif dan melakukan re-trim blok penyimpanan SSD/NVMe untuk menjaga performa kecepatan write agar tidak menurun.",
                    Category = TweakCategory.Storage,
                    SubCategory = "SSD & Drive Optimization",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "fsutil behavior set DisableDeleteNotify 0; Optimize-Volume -DriveLetter C -ReTrim -Verbose",
                    ValueName = "-",
                    DefaultValue = "0",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "STO-SYS-03",
                    Name = "Enable Storage Sense Auto-Cleanup",
                    Description = "Mengaktifkan fitur Storage Sense bawaan Windows untuk pembersihan otomatis file sementara dan Recycle Bin yang sudah lapuk.",
                    Category = TweakCategory.Storage,
                    SubCategory = "Cleanup & Maintenance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy",
                    ValueName = "01",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "STO-CLN-04",
                    Name = "Clear System & User Temporary Files (%TEMP%)",
                    Description = "Membersihkan berkas-berkas temporary yang menumpuk dari instalasi aplikasi, ekstensi, dan proses sistem di folder %TEMP% dan Windows\\Temp.",
                    Category = TweakCategory.Storage,
                    SubCategory = "Cleanup & Maintenance",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "Remove-Item -Path \"$env:TEMP\\*\" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item -Path \"C:\\Windows\\Temp\\*\" -Recurse -Force -ErrorAction SilentlyContinue",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "Cleaned"
                },
                new TweakDefinition
                {
                    Id = "STO-SYS-05",
                    Name = "Clean Old Windows System Restore Points",
                    Description = "Menghapus bayangan Restore Point OS Windows versi lama yang menumpuk untuk menghemat hingga puluhan GB. (PERHATIAN: Hanya menghapus restore point sistem Windows bawaan, BUKAN Restore Point WinTweakStudio).",
                    Category = TweakCategory.Storage,
                    SubCategory = "Cleanup & Maintenance",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.PowerShell,
                    TargetPath = "vssadmin delete shadows /for=C: /oldest",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "Cleaned"
                },
                new TweakDefinition
                {
                    Id = "STO-NTFS-06",
                    Name = "Disable 8.3 Short Filename Creation (NTFS)",
                    Description = "Mematikan pembuatan nama file singkat 8.3 legacy pada format NTFS. Mempercepat operasi penelusuran file pada folder dengan puluhan ribu berkas.",
                    Category = TweakCategory.Storage,
                    SubCategory = "NTFS File System",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.PowerShell,
                    TargetPath = "fsutil 8dot3name set 1",
                    ValueName = "-",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "STO-NTFS-07",
                    Name = "Disable Last Access Timestamp Update (NTFS)",
                    Description = "Mencegah Windows menulis ulang stempel waktu \"Last Access\" setiap kali berkas dibaca untuk mengurangi overhead disk write berulang.",
                    Category = TweakCategory.Storage,
                    SubCategory = "NTFS File System",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "fsutil behavior set disablelastaccess 1",
                    ValueName = "-",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "STO-MON-08",
                    Name = "Storage Health & SMART Monitoring Guidance",
                    Description = "Panduan pemantauan indikator kesehatan SSD/NVMe (Remaining Life, Estimated Health Percentage, dan Temperature) untuk mencegah kegagalan hardware drive.",
                    Category = TweakCategory.Storage,
                    SubCategory = "Storage Health & SMART",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },

                // === Storage Tweaks Baru 2026 ===
                new TweakDefinition
                {
                    Id = "STO-SHA-01",
                    Name = "DirectStorage Enabler (UseStorageCache)",
                    Description = "Mengaktifkan alokasi storage cache DirectStorage NVMe untuk kecepatan loading game instan.",
                    Category = TweakCategory.Storage,
                    SubCategory = "SSD & Drive Optimization",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SOFTWARE\Microsoft\DirectStorage",
                    ValueName = "UseStorageCache",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "STO-SHA-02",
                    Name = "Disable Search Indexing Service on SSD",
                    Description = "Mematikan Windows Search Indexer background service pada SSD untuk menghentikan operasi disk-write konstan.",
                    Category = TweakCategory.Storage,
                    SubCategory = "SSD & Drive Optimization",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Service,
                    TargetPath = "WSearch",
                    ValueName = "Start",
                    DefaultValue = "2",
                    RecommendedValue = "4"
                },
                new TweakDefinition
                {
                    Id = "STO-SHA-03",
                    Name = "NVMe Host Memory Buffer (HMB) Optimization",
                    Description = "Mengatur alokasi HMB DRAM-less SSD pada Windows agar menggunakan ukuran buffer paling optimal.",
                    Category = TweakCategory.Storage,
                    SubCategory = "SSD & Drive Optimization",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\StorPort",
                    ValueName = "HmbAllocationPolicy",
                    DefaultValue = "0",
                    RecommendedValue = "2"
                },
                new TweakDefinition
                {
                    Id = "STO-SHA-04",
                    Name = "Disable NTFS Pagefile Thrashing Paging",
                    Description = "Mencegah alokasi Virtual Memory Pagefile mengalami thrashing berulang pada disk.",
                    Category = TweakCategory.Storage,
                    SubCategory = "NTFS File System",
                    RiskLevel = RiskLevel.Moderate,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    ValueName = "PagingFilesExemption",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                }
            };
        }

        public static List<TweakDefinition> GetBootPowerSeedData()
        {
            return new List<TweakDefinition>
            {
                new TweakDefinition
                {
                    Id = "PWR-BOT-01",
                    Name = "Startup Delay Removal (StartupDelayInMSec)",
                    Description = "Menghapus jeda waktu tunggu bawaan Windows (~10 detik) saat startup sehingga aplikasi startup langsung dijalankan seketika.",
                    Category = TweakCategory.BootPower,
                    SubCategory = "Boot & Startup Optimization",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Serialize",
                    ValueName = "StartupDelayInMSec",
                    DefaultValue = "10000",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "PWR-BOT-02",
                    Name = "Reduce Boot Menu Timeout (BCD)",
                    Description = "Mengurangi waktu tunggu menu boot Windows BCD dari 30 detik menjadi 5 detik untuk mempercepat waktu booting OS.",
                    Category = TweakCategory.BootPower,
                    SubCategory = "Boot & Startup Optimization",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "bcdedit /timeout 5",
                    ValueName = "-",
                    DefaultValue = "30",
                    RecommendedValue = "5"
                },
                new TweakDefinition
                {
                    Id = "PWR-BOT-03",
                    Name = "Disable Verbose Boot Logging",
                    Description = "Mematikan pencatatan log boot mendalam jika tidak sedang melakukan pengujian untuk sedikit mempercepat proses boot.",
                    Category = TweakCategory.BootPower,
                    SubCategory = "Boot & Startup Optimization",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "bcdedit /set {current} bootlog no",
                    ValueName = "-",
                    DefaultValue = "yes",
                    RecommendedValue = "no"
                },
                new TweakDefinition
                {
                    Id = "PWR-PWR-04",
                    Name = "Disable USB Selective Suspend",
                    Description = "Mematikan fitur hemat daya pada port USB untuk mencegah masalah input delay atau freeze pada mouse/keyboard gaming saat wake up dari mode idle.",
                    Category = TweakCategory.BootPower,
                    SubCategory = "Power Plan & Energy",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Registry,
                    TargetPath = @"HKLM\SYSTEM\CurrentControlSet\Services\USB",
                    ValueName = "DisableSelectiveSuspend",
                    DefaultValue = "0",
                    RecommendedValue = "1"
                },
                new TweakDefinition
                {
                    Id = "PWR-PWR-05",
                    Name = "Disable Hard Disk Spin-Down (Turn Off Hard Disk = Never)",
                    Description = "Mencegah drive hard disk memasuki mode sleep/spin-down saat idle untuk mengeliminasi jeda freeze saat drive diakses kembali.",
                    Category = TweakCategory.BootPower,
                    SubCategory = "Power Plan & Energy",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "powercfg /change disk-timeout-ac 0; powercfg /change disk-timeout-dc 0",
                    ValueName = "-",
                    DefaultValue = "20",
                    RecommendedValue = "0"
                },
                new TweakDefinition
                {
                    Id = "PWR-PWR-06",
                    Name = "Enable Ultimate Performance Power Plan Preset",
                    Description = "Mengaktifkan dan menerapkan skema daya Ultimate Performance tersembunyi bawaan Windows untuk performa maksimal tanpa pembatasan daya.",
                    Category = TweakCategory.BootPower,
                    SubCategory = "Power Plan & Energy",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.PowerShell,
                    TargetPath = "powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61; powercfg /setactive e9a42b02-d5df-448d-aa00-03f14749eb61",
                    ValueName = "-",
                    DefaultValue = "Balanced",
                    RecommendedValue = "Ultimate Performance"
                },
                new TweakDefinition
                {
                    Id = "PWR-GUI-07",
                    Name = "Startup Application Manager Guidance",
                    Description = "Petunjuk pengelolaan aplikasi startup secara efisien melalui Task Manager atau Settings untuk mengurangi overhead siklus RAM/CPU saat boot.",
                    Category = TweakCategory.BootPower,
                    SubCategory = "Startup Apps & Health",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                },
                new TweakDefinition
                {
                    Id = "PWR-GUI-08",
                    Name = "BIOS Fast Boot & Hardware POST Check Guidance",
                    Description = "Panduan mengaktifkan fitur Fast Boot pada BIOS/UEFI motherboard untuk melewati tes hardware POST sekunder yang tidak diperlukan.",
                    Category = TweakCategory.BootPower,
                    SubCategory = "Startup Apps & Health",
                    RiskLevel = RiskLevel.Safe,
                    Type = TweakType.Guidance,
                    TargetPath = "-",
                    ValueName = "-",
                    DefaultValue = "-",
                    RecommendedValue = "-"
                }
            };
        }
    }
}
