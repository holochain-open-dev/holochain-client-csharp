using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void HoloNETEntries_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Initialized", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                LogMessage($"APP: HoloNET Collection Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
            }
        }

        private void HoloNETEntries_OnHoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Entry Added To Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                LogMessage($"APP: HoloNET Entry Added To Collection.");
            }
        }

        private void HoloNETEntries_OnHoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Entry Removed From Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                LogMessage($"APP: HoloNET Entry Removed From Collection.");
            }
        }

        private void HoloNETEntries_OnCollectionLoaded(object sender, HoloNETCollectionLoadedResult<Avatar> e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Loaded.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                LogMessage($"APP: HoloNET Collection Loaded.");
            }
        }

        private void HoloNETEntries_OnCollectionSaved(object sender, HoloNETCollectionSavedResult e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Saved.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                LogMessage($"APP: HoloNET Collection Saved.");
            }
        }

        private void HoloNETEntries_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Closed.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);

                if (e.HolochainConductorsShutdownEventArgs != null)
                    LogMessage($"APP: HoloNET Collection Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
                else
                    LogMessage($"APP: HoloNET Collection Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
            }
        }

        private void HoloNETEntries_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"Error Occured For HoloNET Collection. Reason: {e.Reason}", StatusMessageType.Error, false, ucHoloNETCollectionEntryInternal);
            LogMessage($"APP: Error Occured For HoloNET Collection. Reason: {e.Reason}");
        }
    }
}