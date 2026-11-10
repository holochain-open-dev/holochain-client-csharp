using System.Windows.Media;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Interfaces
{
    public interface IInstalledApp
    {
        string AgentPubKey { get; set; }
        Brush CallZomeAndViewDataEntriesButtonsForegroundColour { get; }
        Brush DisabledButtonForegroundColour { get; }
        Brush DisconnectButtonForegroundColour { get; }
        string DnaHash { get; set; }
        Brush EnabledButtonForegroundColour { get; }
        bool IsCallZomeFunctionAndViewDataEntriesEnabled { get; }
        bool IsConnected { get; set; }
        bool IsConnectionInternal { get; }
        bool IsDisabled { get; set; }
        bool IsEnabled { get; set; }
        bool IsSharedConnection { get; set; }
        string Manifest { get; set; }
        string Name { get; set; }
        string Port { get; set; }
        string Status { get; set; }
        string StatusReason { get; set; }
    }
}