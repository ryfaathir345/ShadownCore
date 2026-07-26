using System.Windows;
using WinTweakStudio.Data;
using WinTweakStudio.Services;

namespace WinTweakStudio
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                DatabaseInitializer.Initialize();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to initialize database: {ex.Message}", "WinTweakStudio Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                DiscordService.Instance.Shutdown();
            }
            catch { }
            base.OnExit(e);
        }
    }
}
