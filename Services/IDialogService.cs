using System.Threading.Tasks;

namespace WinTweakStudio.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message, string warningLevel = "Advanced");
        Task ShowMessageAsync(string title, string message);
        Task<(bool confirmed, string provider)> ShowDnsSelectionAsync();
        Task<(bool confirmed, string adapterGuid)> ShowNagleConfirmationAsync(List<NetworkAdapterInfo> adapters);
    }
}
