using System.Windows;
using WinTweakStudio.Data;

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
    }
}
