using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinTweakStudio.Models;
using WinTweakStudio.Services;

namespace WinTweakStudio.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly ITweakService _tweakService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<TweakLog> _logs = new();

        [ObservableProperty]
        private ObservableCollection<RestorePoint> _restorePoints = new();

        [ObservableProperty]
        private RestorePoint? _selectedRestorePoint;

        public HistoryViewModel(ITweakService tweakService, IDialogService dialogService)
        {
            _tweakService = tweakService;
            _dialogService = dialogService;
            LoadHistory();
        }

        public void LoadHistory()
        {
            Logs.Clear();
            var activeLogs = _tweakService.GetTweakHistory()
                .Where(l => !l.IsReverted)
                .OrderByDescending(l => l.Id)
                .ToList();

            for (int i = 0; i < activeLogs.Count; i++)
            {
                activeLogs[i].DisplayIndex = i + 1;
                Logs.Add(activeLogs[i]);
            }

            RestorePoints.Clear();
            foreach (var point in _tweakService.GetRestorePoints())
            {
                RestorePoints.Add(point);
            }

            if (RestorePoints.Count > 0 && SelectedRestorePoint == null)
            {
                SelectedRestorePoint = RestorePoints[0];
            }
        }

        [RelayCommand]
        private async Task UndoLogAsync(TweakLog log)
        {
            if (log == null || log.IsReverted) return;

            bool success = await _tweakService.UndoLogAsync(log);
            if (success)
            {
                LoadHistory();
                await _dialogService.ShowMessageAsync("Undo Successful", $"Reverted '{log.TweakName}' back to previous value: {log.OldValue}");
            }
            else
            {
                await _dialogService.ShowMessageAsync("Undo Failed", $"Could not revert '{log.TweakName}'.");
            }
        }

        [RelayCommand]
        private async Task RestoreAllAsync()
        {
            if (SelectedRestorePoint == null) return;

            bool confirmed = await _dialogService.ShowConfirmationAsync(
                "Restore System State",
                $"Are you sure you want to revert all applied tweaks under Restore Point #{SelectedRestorePoint.Id} ('{SelectedRestorePoint.Label}')?",
                "Advanced"
            );

            if (!confirmed) return;

            bool success = await _tweakService.RestoreAllToPointAsync(SelectedRestorePoint.Id);
            if (success)
            {
                LoadHistory();
                await _dialogService.ShowMessageAsync("Restore Complete", $"All tweaks under Restore Point #{SelectedRestorePoint.Id} have been reverted.");
            }
            else
            {
                await _dialogService.ShowMessageAsync("Restore Failed", "Some tweaks could not be fully reverted.");
            }
        }

        [RelayCommand]
        private void CreateNewPoint()
        {
            _tweakService.CreateRestorePoint($"Manual Point - {System.DateTime.Now:MMM dd, HH:mm}");
            LoadHistory();
        }
    }
}
