using System;
using System.Windows;
using System.Windows.Input;
using WinTweakStudio.Services;

namespace WinTweakStudio.Views
{
    public partial class LoginWindow : Window
    {
        private readonly ILicenseService _licenseService;

        public LoginWindow(ILicenseService licenseService)
        {
            InitializeComponent();
            _licenseService = licenseService;
            MouseDown += LoginWindow_MouseDown;
        }

        private void LoginWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnFree_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Silakan masukkan Username / Nickname Anda untuk memulai.");
                return;
            }

            bool success = _licenseService.RegisterFreeUser(username);
            if (success)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError("Gagal mendaftarkan user. Silakan coba lagi.");
            }
        }

        private async void BtnVip_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string key = TxtVipKey.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Silakan masukkan Username / Nickname Anda.");
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                ShowError("Silakan masukkan VIP Activation Key Anda.");
                return;
            }

            BtnVip.IsEnabled = false;
            BtnFree.IsEnabled = false;
            LblStatus.Visibility = Visibility.Collapsed;

            try
            {
                bool success = await _licenseService.ActivateLicenseAsync(username, key);
                if (success)
                {
                    MessageBox.Show($"Selamat, {username}! Akun VIP Anda telah berhasil diaktifkan.", "Aktivasi Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError("Username atau Key tidak valid. Jika Anda offline, silakan hubungi Owner.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Terjadi kesalahan: {ex.Message}");
            }
            finally
            {
                BtnVip.IsEnabled = true;
                BtnFree.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            LblStatus.Text = message;
            LblStatus.Visibility = Visibility.Visible;
        }
    }
}
