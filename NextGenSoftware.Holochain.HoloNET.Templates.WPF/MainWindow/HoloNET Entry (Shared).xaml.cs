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
        private void _holoNETEntryShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Initialized", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Initialized.");
        }

        private void _holoNETEntryShared_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Loaded", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Loaded: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Saved", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Saved: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Deleted", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Closed", StatusMessageType.Success);

            if (e.HolochainConductorsShutdownEventArgs != null)
                LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
            else
                LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
        }

        private void _holoNETEntryShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Error", StatusMessageType.Error);
            LogMessage($"APP: HoloNET Data Entry (Shared) Error: {e.Reason}");
        }
    }
}