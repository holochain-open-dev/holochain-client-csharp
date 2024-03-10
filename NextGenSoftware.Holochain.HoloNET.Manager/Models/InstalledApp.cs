using System.Windows.Media;
using NextGenSoftware.Holochain.HoloNET.Manager.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Models
{
    public class InstalledApp : IInstalledApp
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string StatusReason { get; set; }
        public string Port { get; set; }
        public string Manifest { get; set; }
        public string AgentPubKey { get; set; }
        public string DnaHash { get; set; }
        public byte[][] CellId { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsConnected { get; set; }
        public bool IsSharedConnection { get; set; }

        public bool IsConnectionInternal
        {
            get
            {
                return !IsSharedConnection;
            }
        }

        public bool IsCallZomeFunctionAndViewDataEntriesEnabled
        {
            get
            {
                return IsEnabled && !IsConnectionInternal;
                //return IsEnabled && Name != "oasis-app";
                //return IsEnabled && IsSharedConnection;
            }
        }

        public Brush EnabledButtonForegroundColour
        {
            get
            {
                if (IsEnabled)
                    return Brushes.DarkGray;
                else
                    return Brushes.WhiteSmoke;
            }
        }

        public Brush DisabledButtonForegroundColour
        {
            get
            {
                if (IsDisabled)
                    return Brushes.DarkGray;
                else
                    return Brushes.WhiteSmoke;
            }
        }

        public Brush CallZomeAndViewDataEntriesButtonsForegroundColour
        {
            get
            {
                if (IsCallZomeFunctionAndViewDataEntriesEnabled)
                    return Brushes.WhiteSmoke;
                else
                    return Brushes.DarkGray;
            }
        }

        public Brush DisconnectButtonForegroundColour
        {
            get
            {
                if (IsConnected)
                    return Brushes.WhiteSmoke;
                else
                    return Brushes.DarkGray;
            }
        }
    }
}
