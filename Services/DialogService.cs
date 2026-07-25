using System.Threading.Tasks;
using System.Windows;
using WinTweakStudio.Views;

namespace WinTweakStudio.Services
{
    public class DialogService : IDialogService
    {
        public Task<bool> ShowConfirmationAsync(string title, string message, string warningLevel = "Advanced")
        {
            var tcs = new TaskCompletionSource<bool>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new ConfirmationDialog(title, message)
                {
                    Owner = Application.Current.MainWindow
                };
                bool? result = dialog.ShowDialog();
                tcs.SetResult(result == true);
            });

            return tcs.Task;
        }

        public Task ShowMessageAsync(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new MessageDialog(title, message)
                {
                    Owner = Application.Current.MainWindow
                };
                dialog.ShowDialog();
                tcs.SetResult(true);
            });

            return tcs.Task;
        }

        public Task<(bool confirmed, string provider)> ShowDnsSelectionAsync()
        {
            var tcs = new TaskCompletionSource<(bool, string)>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new DnsSelectionDialog
                {
                    Owner = Application.Current.MainWindow
                };
                bool? result = dialog.ShowDialog();
                tcs.SetResult((result == true && dialog.Confirmed, dialog.SelectedProvider));
            });

            return tcs.Task;
        }

        public Task<(bool confirmed, string adapterGuid)> ShowNagleConfirmationAsync(List<NetworkAdapterInfo> adapters)
        {
            var tcs = new TaskCompletionSource<(bool, string)>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new NagleConfirmationDialog(adapters)
                {
                    Owner = Application.Current.MainWindow
                };
                bool? result = dialog.ShowDialog();
                tcs.SetResult((result == true && dialog.Confirmed, dialog.SelectedAdapterGuid));
            });

            return tcs.Task;
        }
    }
}
