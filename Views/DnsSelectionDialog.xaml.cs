using System.Windows;
using System.Windows.Input;

namespace WinTweakStudio.Views
{
    public partial class DnsSelectionDialog : Window
    {
        public bool Confirmed { get; private set; }
        public string SelectedProvider { get; private set; } = "Cloudflare";

        public DnsSelectionDialog()
        {
            InitializeComponent();

            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left) DragMove();
            };
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            if (RbGoogle.IsChecked == true) SelectedProvider = "Google";
            else if (RbQuad9.IsChecked == true) SelectedProvider = "Quad9";
            else SelectedProvider = "Cloudflare";

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
