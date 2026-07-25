using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTweakStudio.Models;
using WinTweakStudio.Services;

namespace WinTweakStudio.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;
        private readonly IDialogService _dialogService;
        private readonly IHardwareMonitorService _hardwareMonitorService;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _activeCategoryName = "Dashboard";

        [ObservableProperty]
        private bool _hasAppliedTweaks;

        public DashboardViewModel DashboardVM { get; }
        public HistoryViewModel HistoryVM { get; }
        public SettingsViewModel SettingsVM { get; }

        public ILicenseService License => LicenseService.Instance;

        public MainViewModel()
        {
            _hardwareMonitorService = new HardwareMonitorService();
            _hardwareMonitorService.Initialize();

            _tweakService = new TweakService();
            _dialogService = new DialogService();

            DashboardVM = new DashboardViewModel(_tweakService, _hardwareMonitorService);
            HistoryVM = new HistoryViewModel(_tweakService, _dialogService);
            SettingsVM = new SettingsViewModel(_tweakService, _dialogService, _hardwareMonitorService);

            // Default to Dashboard
            CurrentView = DashboardVM;
            CheckAppliedTweaks();
        }

        public void CheckAppliedTweaks()
        {
            try
            {
                var logs = Data.DatabaseInitializer.GetAllTweakLogs();
                HasAppliedTweaks = System.Linq.Enumerable.Any(logs, l => !l.IsReverted);
            }
            catch
            {
                HasAppliedTweaks = false;
            }
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            ActiveCategoryName = destination;
            CheckAppliedTweaks();

            if (destination == "Dashboard")
            {
                DashboardVM.LoadStats();
                CurrentView = DashboardVM;
            }
            else if (destination == "History")
            {
                HistoryVM.LoadHistory();
                CurrentView = HistoryVM;
            }
            else if (destination == "Settings")
            {
                CurrentView = SettingsVM;
            }
            else if (System.Enum.TryParse<TweakCategory>(destination == "BootPower" ? "BootPower" : destination, out var cat))
            {
                CurrentView = new CategoryViewModel(cat, _tweakService, _dialogService, _hardwareMonitorService);
            }
            else
            {
                // Fallback
                DashboardVM.LoadStats();
                CurrentView = DashboardVM;
            }
        }

        [RelayCommand]
        private async Task RestartPcAsync()
        {
            bool confirmed = await _dialogService.ShowConfirmationAsync(
                "Restart PC Sekarang?",
                "Sebagian besar perubahan tweak sistem memerlukan restart OS Windows agar dapat berjalan dan terasa efeknya secara penuh.\n\nApakah Anda yakin ingin memuat ulang (restart) komputer Anda sekarang?",
                "SecurityWarning"
            );

            if (confirmed)
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "shutdown.exe",
                        Arguments = "/r /t 0 /f",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch { }
            }
        }

        [RelayCommand]
        private void Logout()
        {
            LicenseService.Instance.Logout();
            var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.PromptLogin();
            }
        }
    }
}
