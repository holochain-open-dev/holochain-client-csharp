using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers
{
    public partial class HoloNETManager
    {
        /// <summary>
        /// Will init the HoloNET Collection which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
        /// </summary>
        public async Task<bool> InitHoloNETCollectionAsync()
        {
            HoloNETManager.Instance.InitHoloNETEntryDemo = true;
            HoloNETManager.Instance.ShowAppsListedInLog = false;

            bool initOk = false;
            int port;
            string dnaHash = "";
            string agentPubKey = "";

            (initOk, port, dnaHash, agentPubKey) = await InitDemoApp(HoloNETManager.Instance.HoloNETCollectionDemoAppId, HoloNETManager.Instance.HoloNETEntryDemoHappPath);

            if (initOk)
            {
                HoloNETManager.Instance.LogMessage($"APP: Creating HoloNET Collection (Creating Internal HoloNETClient & Connecting To Port {port})...");
                HoloNETManager.Instance.ShowStatusMessage($"Creating HoloNET Collection (Creating Internal HoloNETClient & Connecting To Port {port})...", StatusMessageType.Information, true);
                HoloNETEntryUIManager.CurrentHoloNETEntryUI.ShowStatusMessage($"Creating Internal HoloNETClient & Connecting To Port {port}...", StatusMessageType.Information, true);

                //If you wanted to load a persisted DNA from disk and then make ammendments to it you would do this:
                //HoloNETDNA dna = HoloNETDNAManager.LoadDNA();
                //dna.HolochainConductorAppAgentURI = $"ws:\\localhost:{attachedResult.Port}";
                //dna.InstalledAppId = _holoNETCollectionDemoAppId;
                //dna.AutoStartHolochainConductor = false;
                //dna.AutoStartHolochainConductor = false;
                //_holoNETEntry = new Avatar(dna);

                //If we do not pass in a HoloNETClient it will create it's own internal connection/client
                HoloNETManager.Instance.HoloNETEntries = new HoloNETObservableCollection<Avatar>("oasis", "load_avatars", "add_avatar", "remove_avatar", "bacth_update_collection", true, new HoloNETDNA()
                {
                    HolochainConductorAppAgentURI = $"ws://localhost:{port}",
                    InstalledAppId = HoloNETManager.Instance.HoloNETCollectionDemoAppId, //You need to set either the InstalledAppId or the AgentPubKey & DnaHash. You can set all 3 if you wish but only one or the other works fine too (if only the InstalledAppId is set it will look up the AgentPubKey & DnaHash from the conductor).
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

                //If we are using LoadCollectionAsync or SaveAllChangesAsync we do not need to worry about any events such as OnSaved if you don't need them.
                HoloNETManager.Instance.HoloNETEntries.OnInitialized += HoloNETEntries_OnInitialized;
                HoloNETManager.Instance.HoloNETEntries.OnClosed += HoloNETEntries_OnClosed;
                HoloNETManager.Instance.HoloNETEntries.OnCollectionLoaded += HoloNETEntries_OnCollectionLoaded;
                HoloNETManager.Instance.HoloNETEntries.OnCollectionSaved += HoloNETEntries_OnCollectionSaved;
                HoloNETManager.Instance.HoloNETEntries.OnError += HoloNETEntries_OnError;
                HoloNETManager.Instance.HoloNETEntries.OnHoloNETEntryAddedToCollection += HoloNETEntries_OnHoloNETEntryAddedToCollection;
                HoloNETManager.Instance.HoloNETEntries.OnHoloNETEntryRemovedFromCollection += HoloNETEntries_OnHoloNETEntryRemovedFromCollection;

                //Will wait until the HoloNET Entry has init (non blocking).
                await HoloNETManager.Instance.HoloNETEntries.WaitTillHoloNETInitializedAsync();

                //Refresh the list of installed hApps.
                HoloNETManager.Instance.ProcessListedApps(await HoloNETManager.Instance.HoloNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All));

                ////Set the status to connected in the list of installed apps (this is good example of how you can access the internal HoloNETClient inside the HoloNET Entry).
                //if (_holoNETEntry.HoloNETClient != null && _holoNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                //    SetAppToConnectedStatus(_holoNETCollectionDemoAppId, _holoNETEntry.HoloNETClient.EndPoint.Port, false);

                HoloNETManager.Instance.UpdateNumerOfClientConnections();

                HoloNETManager.Instance.InitHoloNETEntryDemo = false;
                HoloNETManager.Instance.ShowAppsListedInLog = true;
                return true;
            }

            HoloNETManager.Instance.InitHoloNETEntryDemo = false;
            HoloNETManager.Instance.ShowAppsListedInLog = true;
            return false;
        }

        private void HoloNETEntries_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Initialized", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
            }
        }

        private void HoloNETEntries_OnHoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry Added To Collection.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Added To Collection.");
            }
        }

        private void HoloNETEntries_OnHoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Entry Removed From Collection.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Entry Removed From Collection.");
            }
        }

        private void HoloNETEntries_OnCollectionLoaded(object sender, HoloNETCollectionLoadedResult<Avatar> e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Loaded.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Loaded.");
            }
        }

        private void HoloNETEntries_OnCollectionSaved(object sender, HoloNETCollectionSavedResult e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Saved.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Saved.");
            }
        }

        private void HoloNETEntries_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            if (!e.IsError)
            {
                HoloNETManager.Instance.ShowStatusMessage($"HoloNET Collection Closed.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                if (e.HolochainConductorsShutdownEventArgs != null)
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
                else
                    HoloNETManager.Instance.LogMessage($"APP: HoloNET Collection Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
            }
        }

        private void HoloNETEntries_OnError(object sender, HoloNETErrorEventArgs e)
        {
            HoloNETManager.Instance.ShowStatusMessage($"Error Occured For HoloNET Collection. Reason: {e.Reason}", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            HoloNETManager.Instance.LogMessage($"APP: Error Occured For HoloNET Collection. Reason: {e.Reason}");
        }
    }
}