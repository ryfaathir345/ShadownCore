using CommunityToolkit.Mvvm.ComponentModel;

namespace WinTweakStudio.Models
{
    public enum RiskLevel
    {
        Safe,
        Moderate,
        Advanced
    }

    public enum TweakCategory
    {
        Dashboard,
        GPU,
        CPU,
        RAM,
        Network,
        Windows,
        Service,
        Debloat,
        Storage,
        BootPower,
        History,
        Settings
    }

    public enum TweakType
    {
        Registry,
        Service,
        Command,
        Guidance,
        NvApi,
        Adl,
        PowerShell
    }

    public partial class TweakDefinition : ObservableObject
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TweakCategory Category { get; set; }
        public string SubCategory { get; set; } = string.Empty;
        public RiskLevel RiskLevel { get; set; }
        public TweakType Type { get; set; }
        public string TargetPath { get; set; } = string.Empty; // Registry path or Service Name
        public string ValueName { get; set; } = string.Empty; // Registry Value Name if applicable
        public string DefaultValue { get; set; } = string.Empty;
        public string RecommendedValue { get; set; } = string.Empty;

        [ObservableProperty]
        private bool _isApplied;

        public bool RequiresSecurityWarning { get; set; }
    }

    public class TweakLog
    {
        public long Id { get; set; }
        public int DisplayIndex { get; set; }
        public long RestorePointId { get; set; }
        public string TweakName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string AppliedAt { get; set; } = string.Empty;
        public bool IsReverted { get; set; }
    }

    public class RestorePoint
    {
        public long Id { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public partial class TweakGroup : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private string _name = string.Empty;

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private string _vendorBrush = "#3B82F6";

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isExpanded;

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isSecurityGroup;

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private bool _isEnabled = true;

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private string _subtitleTag = string.Empty;

        [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<TweakDefinition> _tweaks = new();

        public string CountBadge => $"{Tweaks.Count} tweak{(Tweaks.Count == 1 ? "" : "s")}";
    }
}
