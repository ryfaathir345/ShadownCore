using System.Collections.Generic;
using System.Threading.Tasks;
using WinTweakStudio.Models;

namespace WinTweakStudio.Services
{
    public interface ITweakService
    {
        List<TweakDefinition> GetTweaksByCategory(TweakCategory category);
        Task<bool> ApplyTweakAsync(TweakDefinition tweak);
        Task<bool> RevertTweakAsync(TweakDefinition tweak);
        Task<bool> UndoLogAsync(TweakLog log);
        Task<bool> RestoreAllToPointAsync(long restorePointId);
        List<TweakLog> GetTweakHistory();
        List<RestorePoint> GetRestorePoints();
        long CreateRestorePoint(string label);
        bool ClearStandbyMemory();
    }
}
