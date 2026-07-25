using System.Windows;
using System.Windows.Input;

namespace WinTweakStudio.Views
{
    public partial class MessageDialog : Window
    {
        public MessageDialog(string title, string message, string badgeText = "SUCCESS")
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            TxtBadge.Text = badgeText.ToUpper();

            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left) DragMove();
            };
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
