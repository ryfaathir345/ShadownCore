using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTweakStudio.Models;
using WinTweakStudio.Services;

namespace WinTweakStudio.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;
        private readonly IDialogService _dialogService;
        private readonly IHardwareMonitorService? _hardwareMonitorService;
        private readonly INetworkService _networkService;

        [ObservableProperty]
        private TweakCategory _category;

        [ObservableProperty]
        private string _categoryTitle = string.Empty;

        [ObservableProperty]
        private ObservableCollection<TweakDefinition> _tweaks = new();

        [ObservableProperty]
        private ObservableCollection<TweakGroup> _tweakGroups = new();

        [ObservableProperty]
        private ObservableCollection<GamePingResult> _gamePingResults = new();

        [ObservableProperty]
        private bool _isPinging;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        partial void OnSearchQueryChanged(string value)
        {
            FilterTweaks();
        }

        public CategoryViewModel(TweakCategory category, ITweakService tweakService, IDialogService dialogService, IHardwareMonitorService? hardwareMonitorService = null, INetworkService? networkService = null)
        {
            _category = category;
            CategoryTitle = category.ToString();
            _tweakService = tweakService;
            _dialogService = dialogService;
            _hardwareMonitorService = hardwareMonitorService;
            _networkService = networkService ?? new NetworkService();
            _ = LoadTweaksAsync();
        }

        public void LoadTweaks()
        {
            _ = LoadTweaksAsync();
        }

        public async Task LoadTweaksAsync()
        {
            var list = _tweakService.GetTweaksByCategory(Category);

            // Filter out Tweak #10 (Multi-CCD Ryzen) if current CPU is not multi-CCD
            if (Category == TweakCategory.CPU)
            {
                bool isMultiCcd = await IsMultiCcdCpuAsync();
                if (!isMultiCcd)
                {
                    list.RemoveAll(t => t.Id == "CPU-SCH-10" || t.Name.Contains("Multi-CCD", System.StringComparison.OrdinalIgnoreCase));
                }
            }

            // Handle RAM category XMP detection badge update
            if (Category == TweakCategory.RAM && _hardwareMonitorService != null)
            {
                var snapshot = await _hardwareMonitorService.ReadMetricsAsync();
                if (snapshot.IsXmpDisabled)
                {
                    var xmpTweak = list.FirstOrDefault(t => t.Id == "RAM-MEM-09" || t.Name.Contains("XMP", System.StringComparison.OrdinalIgnoreCase));
                    if (xmpTweak != null && !xmpTweak.Description.Contains("⚠ XMP/EXPO Belum Aktif"))
                    {
                        xmpTweak.Description = $"{xmpTweak.Description}\n\n{snapshot.XmpWarningMessage}";
                    }
                }
            }

            // Handle Network category IP readout for Static IP Guidance (Item 10)
            if (Category == TweakCategory.Network)
            {
                var ipTweak = list.FirstOrDefault(t => t.Id == "NET-IP-10" || t.Name.Contains("Static IP", System.StringComparison.OrdinalIgnoreCase));
                var activeAdapter = _networkService.GetPrimaryAdapterInfo();
                if (ipTweak != null && activeAdapter != null && !ipTweak.Description.Contains("INFORMASI IP ADAPTER SAAT INI"))
                {
                    ipTweak.Description = $"{ipTweak.Description}\n\nINFORMASI IP ADAPTER SAAT INI ({activeAdapter.Name}):\n• IP Address: {activeAdapter.IpAddress}\n• Subnet Mask: {activeAdapter.SubnetMask}\n• Default Gateway: {activeAdapter.Gateway}\n• DNS Servers: {activeAdapter.DnsServers}";
                }

                _ = RunPingTestAsync();
            }

            // Handle Service category hardware relevance detection
            if (Category == TweakCategory.Service)
            {
                // Bluetooth Service (bthserv)
                var btTweak = list.FirstOrDefault(t => t.Id == "SVC-HW-08" || t.Name.Contains("Bluetooth", System.StringComparison.OrdinalIgnoreCase));
                if (btTweak != null && !btTweak.Description.Contains("Hardware Bluetooth"))
                {
                    btTweak.Description = $"{btTweak.Description}\n\n💡 CATATAN RELEVANSI: Aman dimatikan jika PC Anda tidak menggunakan adapter/modul Bluetooth.";
                }

                // Touch Keyboard Service (TabletInputService)
                var touchTweak = list.FirstOrDefault(t => t.Id == "SVC-HW-07" || t.Name.Contains("Touch Keyboard", System.StringComparison.OrdinalIgnoreCase));
                if (touchTweak != null && !touchTweak.Description.Contains("Perangkat Non-Touchscreen"))
                {
                    touchTweak.Description = $"{touchTweak.Description}\n\n💡 CATATAN RELEVANSI: Rekomendasi Disabled untuk perangkat PC desktop & laptop non-touchscreen.";
                }

                // Print Spooler Service (Spooler)
                var spoolTweak = list.FirstOrDefault(t => t.Id == "SVC-HW-01" || t.Name.Contains("Print Spooler", System.StringComparison.OrdinalIgnoreCase));
                if (spoolTweak != null && !spoolTweak.Description.Contains("Printer"))
                {
                    spoolTweak.Description = $"{spoolTweak.Description}\n\n💡 CATATAN RELEVANSI: Matikan jika Anda tidak menggunakan printer fisik atau dokumen virtual PDF printer.";
                }
            }

            Tweaks.Clear();
            foreach (var item in list)
            {
                Tweaks.Add(item);
            }

            var groupOrder = Category switch
            {
                TweakCategory.CPU => new List<string> { "Power Plan", "Scheduling", "SMT", "Background", "Security", "Monitoring" },
                TweakCategory.RAM => new List<string> { "Memory Management", "Services", "Security & Pagefile", "Hardware Speed" },
                TweakCategory.Network => new List<string> { "DNS & Latency", "Adapter Settings", "Bandwidth", "IP Configuration" },
                TweakCategory.Windows => new List<string> { "UI & Performance", "Background Apps", "Gaming & Overlay", "AI & Telemetry", "System Services & Power", "System Diagnostics" },
                TweakCategory.Service => new List<string> { "System & Hardware Services", "Telemetry & Diagnostics", "Network & Security Services", "Legacy & Vendor Services" },
                TweakCategory.Debloat => new List<string> { "UWP Apps Debloat", "Browser & Vendor Debloat", "System Apps & Sync", "Debloat Guidance" },
                TweakCategory.Storage => new List<string> { "Cleanup & Maintenance", "SSD & Drive Optimization", "NTFS File System", "Storage Health & SMART" },
                TweakCategory.BootPower => new List<string> { "Boot & Startup Optimization", "Power Plan & Energy", "Startup Apps & Health" },
                _ => new List<string> { "General", "NVIDIA", "AMD", "Intel" }
            };

            var groupedDict = list
                .GroupBy(t => string.IsNullOrWhiteSpace(t.SubCategory) ? "General" : t.SubCategory)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allKeys = groupOrder.Where(k => groupedDict.ContainsKey(k))
                .Concat(groupedDict.Keys.Where(k => !groupOrder.Contains(k)))
                .ToList();

            HardwareMetricsSnapshot? gpuMetrics = null;
            if (Category == TweakCategory.GPU && _hardwareMonitorService != null)
            {
                gpuMetrics = await _hardwareMonitorService.ReadMetricsAsync();
            }

            var existingStates = TweakGroups.ToDictionary(g => g.Name, g => g.IsExpanded);

            TweakGroups.Clear();
            foreach (var key in allKeys)
            {
                var groupTweaks = groupedDict[key];
                bool isGroupEnabled = true;
                string subtitleTag = "";
                bool isExpanded = false;

                if (Category == TweakCategory.GPU && gpuMetrics != null)
                {
                    if (key.Equals("NVIDIA", System.StringComparison.OrdinalIgnoreCase))
                    {
                        isGroupEnabled = gpuMetrics.HasNvidia;
                        isExpanded = gpuMetrics.HasNvidia;
                        subtitleTag = gpuMetrics.HasNvidia ? "" : "(Tidak terdeteksi di sistem Anda)";
                    }
                    else if (key.Equals("AMD", System.StringComparison.OrdinalIgnoreCase))
                    {
                        isGroupEnabled = gpuMetrics.HasAmd;
                        isExpanded = gpuMetrics.HasAmd;
                        subtitleTag = gpuMetrics.HasAmd ? "" : "(Tidak terdeteksi di sistem Anda)";
                    }
                    else if (key.Equals("Intel", System.StringComparison.OrdinalIgnoreCase))
                    {
                        isGroupEnabled = gpuMetrics.HasIntel;
                        isExpanded = gpuMetrics.HasIntel;
                        subtitleTag = gpuMetrics.HasIntel ? "" : "(Tidak terdeteksi di sistem Anda)";
                    }
                    else // "General"
                    {
                        isGroupEnabled = true;
                        isExpanded = true;
                        subtitleTag = "";
                    }
                }
                else if (Category == TweakCategory.CPU)
                {
                    isExpanded = key.Equals("Power Plan", System.StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    isExpanded = true;
                }

                if (!isGroupEnabled)
                {
                    isExpanded = false;
                }
                else if (existingStates.TryGetValue(key, out var wasExpanded))
                {
                    isExpanded = wasExpanded;
                }

                bool isSecurity = key.Equals("Security", System.StringComparison.OrdinalIgnoreCase);

                var tweakGroup = new TweakGroup
                {
                    Name = key,
                    VendorBrush = isSecurity ? "#EF4444" : GetVendorBrush(key),
                    IsExpanded = isExpanded,
                    IsSecurityGroup = isSecurity,
                    IsEnabled = isGroupEnabled,
                    SubtitleTag = subtitleTag
                };

                foreach (var tw in groupTweaks)
                {
                    tweakGroup.Tweaks.Add(tw);
                }

                TweakGroups.Add(tweakGroup);
            }
        }

        private void FilterTweaks()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                foreach (var group in TweakGroups)
                {
                    group.IsExpanded = true;
                }
                return;
            }

            string query = SearchQuery.Trim().ToLowerInvariant();
            foreach (var group in TweakGroups)
            {
                bool hasMatch = group.Tweaks.Any(t => t.Name.ToLowerInvariant().Contains(query) || t.Description.ToLowerInvariant().Contains(query));
                group.IsExpanded = hasMatch;
            }
        }

        private async Task<bool> IsMultiCcdCpuAsync()
        {
            if (_hardwareMonitorService == null) return false;
            try
            {
                var metrics = await _hardwareMonitorService.ReadMetricsAsync();
                string cpuName = metrics.Cpu.Name.ToUpperInvariant();
                string[] multiCcdKeywords = new[] { "7950X3D", "7900X3D", "5950X", "5900X", "3950X", "3900X", "7950X", "7900X" };
                return multiCcdKeywords.Any(k => cpuName.Contains(k));
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> DetectGpuVendorAsync()
        {
            if (_hardwareMonitorService == null) return string.Empty;
            try
            {
                var metrics = await _hardwareMonitorService.ReadMetricsAsync();
                var primaryGpu = metrics.Gpus.FirstOrDefault(g => g.IsDiscrete) ?? metrics.Gpus.FirstOrDefault();
                if (primaryGpu != null)
                {
                    string name = primaryGpu.Name.ToUpperInvariant();
                    if (name.Contains("NVIDIA") || name.Contains("GEFORCE")) return "NVIDIA";
                    if (name.Contains("AMD") || name.Contains("RADEON")) return "AMD";
                    if (name.Contains("INTEL") || name.Contains("ARC")) return "Intel";
                }
            }
            catch { }
            return string.Empty;
        }

        private string GetVendorBrush(string name)
        {
            return name.ToUpperInvariant() switch
            {
                "NVIDIA" => "#76B900",
                "AMD" => "#EF4444",
                "INTEL" => "#0071C5",
                "SECURITY" => "#EF4444",
                "POWER PLAN" => "#3B82F6",
                "SCHEDULING" => "#8B5CF6",
                "SMT" => "#EC4899",
                "BACKGROUND" => "#10B981",
                "MONITORING" => "#F59E0B",
                "DNS & LATENCY" => "#00E5FF",
                "ADAPTER SETTINGS" => "#3B82F6",
                "BANDWIDTH" => "#10B981",
                "IP CONFIGURATION" => "#F59E0B",
                "UI & PERFORMANCE" => "#3B82F6",
                "BACKGROUND APPS" => "#10B981",
                "GAMING & OVERLAY" => "#8B5CF6",
                "AI & TELEMETRY" => "#EC4899",
                "SYSTEM SERVICES & POWER" => "#F59E0B",
                "SYSTEM DIAGNOSTICS" => "#6366F1",
                "SYSTEM & HARDWARE SERVICES" => "#3B82F6",
                "TELEMETRY & DIAGNOSTICS" => "#EC4899",
                "NETWORK & SECURITY SERVICES" => "#10B981",
                "LEGACY & VENDOR SERVICES" => "#F59E0B",
                "UWP APPS DEBLOAT" => "#EF4444",
                "BROWSER & VENDOR DEBLOAT" => "#8B5CF6",
                "SYSTEM APPS & SYNC" => "#3B82F6",
                "DEBLOAT GUIDANCE" => "#10B981",
                "CLEANUP & MAINTENANCE" => "#10B981",
                "SSD & DRIVE OPTIMIZATION" => "#3B82F6",
                "NTFS FILE SYSTEM" => "#8B5CF6",
                "STORAGE HEALTH & SMART" => "#F59E0B",
                "BOOT & STARTUP OPTIMIZATION" => "#3B82F6",
                "POWER PLAN & ENERGY" => "#F59E0B",
                "STARTUP APPS & HEALTH" => "#10B981",
                _ => "#3B82F6"
            };
        }

        [RelayCommand]
        private async Task ApplyTweakAsync(TweakDefinition tweak)
        {
            if (tweak == null) return;

            if (!LicenseService.Instance.IsVipOrOwner && tweak.IsVipExclusive)
            {
                await _dialogService.ShowMessageAsync(
                    "🔒 FITUR EKSKLUSIF VIP MEMBER",
                    $"Tweak '{tweak.Name}' (Kategori: {tweak.Category} - {tweak.SubCategory}) adalah fitur eksklusif untuk VIP Member dan Owner.\n\nAkun Free User dibatasi pada ~40% tweak dasar (66 tweak standar). Silakan aktivasi VIP Key untuk membuka seluruh 169+ tweak performa lanjutan dan fitur khusus!"
                );
                return;
            }

            // Special Handler: Item 1 Disable Nagle's Algorithm (NET-LAT-01)
            if (tweak.Id == "NET-LAT-01" || tweak.Name.Contains("Nagle", System.StringComparison.OrdinalIgnoreCase))
            {
                var activeAdapters = _networkService.GetActiveAdapters();
                var (confirmed, adapterGuid) = await _dialogService.ShowNagleConfirmationAsync(activeAdapters);
                if (!confirmed) return;

                bool applySuccess = _networkService.ApplyNagleAlgorithm(true, adapterGuid);
                if (applySuccess)
                {
                    await _tweakService.ApplyTweakAsync(tweak);
                    LoadTweaks();
                    await _dialogService.ShowMessageAsync("Tweak Applied", "Berhasil mematikan Nagle's Algorithm pada adapter jaringan.");
                }
                else
                {
                    await _dialogService.ShowMessageAsync("Error", "Gagal memodifikasi registry Nagle's Algorithm.");
                }
                return;
            }

            // Special Handler: Item 3 Custom DNS (NET-DNS-03)
            if (tweak.Id == "NET-DNS-03" || tweak.Name.Contains("Custom High-Performance DNS", System.StringComparison.OrdinalIgnoreCase))
            {
                var (confirmed, selectedProvider) = await _dialogService.ShowDnsSelectionAsync();
                if (!confirmed) return;

                tweak.RecommendedValue = selectedProvider;
                bool dnsSuccess = _networkService.SetCustomDns(selectedProvider);
                if (dnsSuccess)
                {
                    await _tweakService.ApplyTweakAsync(tweak);
                    LoadTweaks();
                    await _dialogService.ShowMessageAsync("DNS Applied", $"Berhasil memperbarui DNS ke provider {selectedProvider}.");
                }
                else
                {
                    await _dialogService.ShowMessageAsync("Error", $"Gagal mengubah DNS ke {selectedProvider}. Pastikan Administrator privileges.");
                }
                return;
            }

            // Handle Security Warning confirmation dialog
            if (tweak.RequiresSecurityWarning)
            {
                bool warningConfirmed = await _dialogService.ShowConfirmationAsync(
                    "⚠️ PERINGATAN KEAMANAN SISTEM",
                    $"PERHATIAN: Tweak '{tweak.Name}' mematikan fitur perlindungan keamanan penting Windows ({tweak.Description}).\n\nHal ini berpotensi meningkatkan performa tetapi mengorbankan proteksi terhadap malware dan eksploitasi hardware.\n\nApakah Anda memahami risiko ini dan tetap ingin mengaktifkan tweak ini?",
                    "SecurityWarning"
                );

                if (!warningConfirmed) return;
            }

            // Handle Guidance Tweak: display manual instructions only
            if (tweak.Type == TweakType.Guidance)
            {
                await _dialogService.ShowMessageAsync(
                    $"Instruksi Manual: {tweak.Name}",
                    $"{tweak.Description}\n\nPengaturan ini dilakukan secara manual sesuai petunjuk di atas."
                );
                return;
            }

            // Intel device-specific path disclaimer for tweaks #29 and #32
            if (tweak.Id == "GPU-INT-29" || tweak.Id == "GPU-INT-32" || 
                tweak.Name.Contains("Shared VRAM", System.StringComparison.OrdinalIgnoreCase) || 
                tweak.Name.Contains("HwQueuedRenderPacketGroupLimitPerNode", System.StringComparison.OrdinalIgnoreCase))
            {
                bool pathConfirmed = await _dialogService.ShowConfirmationAsync(
                    "Disclaimer Device Path Intel",
                    $"PERHATIAN: Path registry untuk tweak '{tweak.Name}' bergantung pada Device ID GPU Intel yang terpasang pada system Anda. Mohon verifikasi path registry secara manual sebelum melanjutkan.\n\nApakah Anda yakin ingin melanjutkan?",
                    "Warning"
                );
                if (!pathConfirmed) return;
            }

            // Advanced level risk requirement: MUST show confirmation dialog
            if (tweak.RiskLevel == RiskLevel.Advanced)
            {
                bool confirmed = await _dialogService.ShowConfirmationAsync(
                    $"Apply Advanced Tweak: {tweak.Name}?",
                    $"Modifying '{tweak.Name}' affects deep system settings ({tweak.TargetPath}). A snapshot of your current settings will be saved to SQLite before applying. Do you wish to continue?",
                    "Advanced"
                );

                if (!confirmed) return;
            }

            // 1. Instantly set IsApplied = true so card turns gray immediately
            tweak.IsApplied = true;
            NotifyMainVmCheckApplied();

            // 2. Open dialog immediately without waiting for background tasks
            var dialogTask = _dialogService.ShowMessageAsync("Tweak Applied", $"Berhasil menerapkan tweak '{tweak.Name}'. Baseline & status telah disimpan ke Riwayat.");

            // 3. Process actual system change asynchronously in background
            _ = Task.Run(async () =>
            {
                bool success = await _tweakService.ApplyTweakAsync(tweak);
                if (!success)
                {
                    App.Current.Dispatcher.Invoke(() => 
                    {
                        tweak.IsApplied = false;
                        NotifyMainVmCheckApplied();
                    });
                }
            });

            await dialogTask;
        }

        private void NotifyMainVmCheckApplied()
        {
            try
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (App.Current.MainWindow?.DataContext is MainViewModel mainVm)
                    {
                        mainVm.CheckAppliedTweaks();
                    }
                });
            }
            catch { }
        }

        [RelayCommand]
        private async Task RevertTweakAsync(TweakDefinition tweak)
        {
            if (tweak == null) return;

            // Special Handler: Revert Item 3 Custom DNS
            if (tweak.Id == "NET-DNS-03" || tweak.Name.Contains("Custom High-Performance DNS", System.StringComparison.OrdinalIgnoreCase))
            {
                bool revertSuccess = _networkService.RevertDns("DHCP");
                if (revertSuccess)
                {
                    tweak.IsApplied = false;
                    await _tweakService.RevertTweakAsync(tweak);
                    NotifyMainVmCheckApplied();
                    await _dialogService.ShowMessageAsync("DNS Reverted", "Berhasil mengembalikan DNS ke pengaturan Otomatis (DHCP).");
                }
                else
                {
                    await _dialogService.ShowMessageAsync("Error", "Gagal mengembalikan DNS ke DHCP.");
                }
                return;
            }

            // 1. Instantly set IsApplied = false so card reactivates immediately
            tweak.IsApplied = false;
            NotifyMainVmCheckApplied();

            // 2. Open dialog immediately
            var dialogTask = _dialogService.ShowMessageAsync("Tweak Reverted", $"Berhasil mengembalikan tweak '{tweak.Name}' ke pengaturan awal.");

            // 3. Process revert asynchronously in background
            _ = Task.Run(async () =>
            {
                bool success = await _tweakService.RevertTweakAsync(tweak);
                if (!success)
                {
                    App.Current.Dispatcher.Invoke(() => 
                    {
                        tweak.IsApplied = true;
                        NotifyMainVmCheckApplied();
                    });
                }
            });

            await dialogTask;
        }

        [RelayCommand]
        private async Task RunPingTestAsync()
        {
            if (!LicenseService.Instance.IsVipOrOwner)
            {
                await _dialogService.ShowMessageAsync(
                    "🔒 FITUR EKSKLUSIF VIP MEMBER",
                    "Game Server Ping Monitor adalah fitur eksklusif untuk VIP Member dan Owner.\n\nSilakan aktivasi VIP Key Anda untuk menguji latency real-time ke server game dan cloud utama!"
                );
                return;
            }

            if (IsPinging) return;
            IsPinging = true;

            try
            {
                GamePingResults.Clear();
                var results = await _networkService.TestGamePingServersAsync();
                foreach (var res in results)
                {
                    GamePingResults.Add(res);
                }
            }
            finally
            {
                IsPinging = false;
            }
        }
    }
}
