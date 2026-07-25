using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using WinTweakStudio.Services;

namespace WinTweakStudio.Views
{
    public partial class NagleConfirmationDialog : Window
    {
        public bool Confirmed { get; private set; }
        public string SelectedAdapterGuid { get; private set; } = string.Empty;

        public NagleConfirmationDialog(List<NetworkAdapterInfo> adapters)
        {
            InitializeComponent();

            CmbAdapters.Items.Add("Semua Adapter Aktif (All Active Interfaces)");
            foreach (var adp in adapters)
            {
                CmbAdapters.Items.Add($"{adp.Name} ({adp.IpAddress})");
            }
            CmbAdapters.SelectedIndex = 0;

            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left) DragMove();
            };
        }

        private void OnCheckboxChanged(object sender, RoutedEventArgs e)
        {
            BtnApply.IsEnabled = ChkUnderstand.IsChecked == true;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            DialogResult = false;
            Close();
        }
    }
}
