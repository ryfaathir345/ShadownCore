using System.Windows;

namespace WinTweakStudio.Views
{
    public partial class ConfirmationDialog : Window
    {
        public bool Confirmed { get; private set; }

        public ConfirmationDialog(string title, string message)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;

            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Left) DragMove();
            };
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
