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
        private void _holoNETEntry_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Initialized", StatusMessageType.Success, false, ucHoloNETEntry);
            LogMessage($"APP: HoloNET Data Entry Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
        }

        private void _holoNETEntry_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Loaded", StatusMessageType.Success, false, ucHoloNETEntry);
            LogMessage($"APP: HoloNET Data Entry Loaded: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            //For non async Save method you would use this callback but with async you can handle it in-line directly after the call to SaveAsync as we do in the btnHoloNETEntryPopupSave_Click event handler.
            //HandleHoloNETEntrySaved(e);
        }

        private void _holoNETEntry_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            //For non async Delete method you would use this callback but with async you can handle it in-line directly after the call to DeleteAsync as we do in the btnHoloNETEntryPopupDelete_Click event handler.
            //ShowStatusMessage($"APP: HoloNET Data Entry Deleted", StatusMessageType.Success);
            //LogMessage($"APP: HoloNET Data Entry Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Closed", StatusMessageType.Success, false, ucHoloNETEntry);
            //LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs != null ? e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown : 0}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");

            if (e.HolochainConductorsShutdownEventArgs != null)
                LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
            else
                LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
        }

        private void _holoNETEntry_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Error", StatusMessageType.Error, false, ucHoloNETEntry);
            LogMessage($"APP: HoloNET Data Entry Error: {e.Reason}");
        } 
    }
}