using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers
{
    public partial class HoloNETManager
    {
        /// <summary>
        /// Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
        /// </summary>
        public async Task<bool> InitHoloNETEntry()
        {
            bool initOk = false;
            int port;
            string dnaHash = "";
            string agentPubKey = "";

            HoloNETManager.Instance.InitHoloNETEntryDemo = true;
            HoloNETManager.Instance.ShowAppsListedInLog = false;

            (initOk, port, dnaHash, agentPubKey) = await HoloNETManager.Instance.InitDemoApp(HoloNETManager.Instance.HoloNETEntryDemoAppId, HoloNETManager.Instance.HoloNETEntryDemoHappPath);

            if (initOk)
            {
                HoloNETManager.Instance.LogMessage($"APP: Creating HoloNET Entry (Creating Internal HoloNETClient & Connecting To Port {port})...");
                HoloNETManager.Instance.ShowStatusMessage($"Creating HoloNET Entry (Creating Internal HoloNETClient & Connecting To Port {port})...", StatusMessageType.Information, true);
                HoloNETEntryUIManager.CurrentHoloNETEntryUI.ShowStatusMessage($"Creating Internal HoloNETClient & Connecting To Port {port}...", StatusMessageType.Information, true);

                //If you wanted to load a persisted DNA from disk and then make ammendments to it you would do this:
                //HoloNETDNA dna = HoloNETDNAManager.LoadDNA();
                //dna.HolochainConductorAppAgentURI = $"ws:\\localhost:{attachedResult.Port}";
                //dna.InstalledAppId = _holoNETEntryDemoAppId;
                //dna.AutoStartHolochainConductor = false;
                //dna.AutoStartHolochainConductor = false;
                //HoloNETManager.Instance.HoloNETEntry = new Avatar(dna);

                //If we do not pass in a HoloNETClient it will create it's own internal connection/client
                HoloNETManager.Instance.HoloNETEntry = new Avatar(new HoloNETDNA()
                {
                    HolochainConductorAppAgentURI = $"ws://localhost:{port}",
                    InstalledAppId = HoloNETManager.Instance.HoloNETEntryDemoAppId, //You need to set either the InstalledAppId or the AgentPubKey & DnaHash. You can set all 3 if you wish but only one or the other works fine too (if only the InstalledAppId is set it will look up the AgentPubKey & DnaHash from the conductor).
                    AgentPubKey = agentPubKey, //If you only set the AgentPubKey & DnaHash but not the InstalledAppId then it will still work fine.
                    DnaHash = dnaHash,
                    AutoStartHolochainConductor = false, //This defaults to true normally so make sure you set this to false because we can connect to the already running holochain.exe (the one admin is already using).
                    AutoShutdownHolochainConductor = false, //This defaults to true normally so make sure you set this to false.

                    //HolochainConductorToUse = HolochainConductorEnum.HcDevTool,
                    //ShowHolochainConductorWindow = true,

                    //FullPathToRootHappFolder = "E:\\hc\\holochain-holochain-0.1.5\\happs\\oasis",
                    //FullPathToCompiledHappFolder = "E:\\hc\\holochain-holochain-0.1.5\\happs\\oasis\\workdir"
                    //FullPathToRootHappFolder = "C:\\Users\\USER\\holochain-holochain-0.1.5\\happs\\oasis",
                    //FullPathToCompiledHappFolder = "C:\\Users\\USER\\holochain-holochain-0.1.5\\happs\\oasis\\workdir"
                });

                //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                HoloNETManager.Instance.HoloNETEntry.OnInitialized += _holoNETEntry_OnInitialized;
                HoloNETManager.Instance.HoloNETEntry.OnLoaded += _holoNETEntry_OnLoaded;
                HoloNETManager.Instance.HoloNETEntry.OnSaved += _holoNETEntry_OnSaved;
                HoloNETManager.Instance.HoloNETEntry.OnDeleted += _holoNETEntry_OnDeleted;
                HoloNETManager.Instance.HoloNETEntry.OnClosed += _holoNETEntry_OnClosed;
                HoloNETManager.Instance.HoloNETEntry.OnError += _holoNETEntry_OnError;

                //Will wait until the HoloNET Entry has init (non blocking).
                await HoloNETManager.Instance.HoloNETEntry.WaitTillHoloNETInitializedAsync();

                //Refresh the list of installed hApps.
                HoloNETManager.Instance.ProcessListedApps(await HoloNETManager.Instance.HoloNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All));

                ////Set the status to connected in the list of installed apps (this is good example of how you can access the internal HoloNETClient inside the HoloNET Entry).
                //if (HoloNETManager.Instance.HoloNETEntry.HoloNETClient != null && HoloNETManager.Instance.HoloNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                //    SetAppToConnectedStatus(_holoNETEntryDemoAppId, HoloNETManager.Instance.HoloNETEntry.HoloNETClient.EndPoint.Port, false);

                HoloNETManager.Instance.UpdateNumerOfClientConnections();

                HoloNETManager.Instance.InitHoloNETEntryDemo = false;
                HoloNETManager.Instance.ShowAppsListedInLog = true;
                return true;
            }

            HoloNETManager.Instance.InitHoloNETEntryDemo = false;
            HoloNETManager.Instance.ShowAppsListedInLog = true;
            return false;
        }

        private void _holoNETEntry_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry Initialized", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
        }

        private void _holoNETEntry_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry Loaded", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Loaded: {HoloNETManager.Instance.GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            //For non async Save method you would use this callback but with async you can handle it in-line directly after the call to SaveAsync as we do in the btnHoloNETEntryPopupSave_Click event handler.
            //HandleHoloNETEntrySaved(e);
        }

        private void _holoNETEntry_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            //For non async Delete method you would use this callback but with async you can handle it in-line directly after the call to DeleteAsync as we do in the btnHoloNETEntryPopupDelete_Click event handler.
            //HoloNETManager.Instance.ShowStatusMessage($"APP: HoloNET Data Entry Deleted", StatusMessageType.Success);
            //HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry Closed", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            //HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs != null ? e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown : 0}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");

            if (e.HolochainConductorsShutdownEventArgs != null)
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
            else
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
        }

        private void _holoNETEntry_OnError(object sender, HoloNETErrorEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"HoloNET Data Entry Error", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            HoloNETManager.Instance.LogMessage($"APP: HoloNET Data Entry Error: {e.Reason}");
        }
    }
}