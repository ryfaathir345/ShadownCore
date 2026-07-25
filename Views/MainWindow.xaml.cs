using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WinTweakStudio.Services;

namespace WinTweakStudio.Views
{
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        public bool PromptLogin()
        {
            var licenseService = LicenseService.Instance;
            if (string.IsNullOrEmpty(licenseService.Username))
            {
                // Apply smooth Gaussian Blur effect to MainWindow background
                this.Effect = new System.Windows.Media.Effects.BlurEffect 
                { 
                    Radius = 20, 
                    KernelType = System.Windows.Media.Effects.KernelType.Gaussian 
                };

                var loginWindow = new LoginWindow(licenseService);
                loginWindow.Owner = this;
                loginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                bool? result = loginWindow.ShowDialog();

                // Remove Blur effect after modal closes
                this.Effect = null;

                if (result != true)
                {
                    Application.Current.Shutdown();
                    return false;
                }
            }
            return true;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!PromptLogin()) return;

            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                int useImmersiveDarkMode = 1;
                if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));
                }

                // DWMWA_BORDER_COLOR (34): Set border color to match dark background (#0F0F18 -> COLORREF 0x00180F0F)
                int borderColor = 0x00180F0F;
                DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));

                // DWMWA_CAPTION_COLOR (35): Set caption background color to dark
                int captionColor = 0x00180F0F;
                DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));

                // DWMWA_TEXT_COLOR (36): Set caption text color to white
                int textColor = 0x00FFFFFF;
                DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref textColor, sizeof(int));
            }
        }
    }
}
