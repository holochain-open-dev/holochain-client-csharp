using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Manager.Enums;
using NextGenSoftware.Holochain.HoloNET.Manager.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Manager.Models;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Managers
{
    public partial class HoloNETManager : IHoloNETManager
    {
        /// <summary>
        /// Will init the HoloNET Collection which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
        /// </summary>
        public async Task<bool> InitHoloNETCollectionAsync()
        {
            InitHoloNETEntryDemo = true;
            ShowAppsListedInLog = false;

            LogMessage("APP: Initializing HoloNET Collection (Internal Connection)...");
            ShowStatusMessage("Initializing HoloNET Collection (Internal Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

            InstallEnableSignAndAttachHappEventArgs initDemoResult = await InitDemoApp(HoloNETCollectionDemoAppId, HoloNETEntryDemoHappPath);

            if (initDemoResult != null && !initDemoResult.IsError && initDemoResult.IsSuccess)
            {
                LogMessage($"APP: Creating HoloNET Collection (Internal Connection) (Creating Internal HoloNETClient & Connecting To Port {initDemoResult.AttachedOnPort})...");
                ShowStatusMessage($"Creating HoloNET Collection (Internal Connection) (Creating Internal HoloNETClient & Connecting To Port {initDemoResult.AttachedOnPort})...", StatusMessageType.Information, true);
                HoloNETEntryUIManager.CurrentHoloNETEntryUI.ShowStatusMessage($"Creating Internal HoloNETClient & Connecting To Port {initDemoResult.AttachedOnPort}...", StatusMessageType.Information, true);

                //If you wanted to load a persisted DNA from disk and then make ammendments to it you would do this:
                //HoloNETDNA dna = HoloNETDNAManager.LoadDNA();
                //dna.HolochainConductorAppAgentURI = $"ws:\\localhost:{attachedResult.Port}";
                //dna.InstalledAppId = _holoNETCollectionDemoAppId;
                //dna.AutoStartHolochainConductor = false;
                //dna.AutoStartHolochainConductor = false;
                //_holoNETEntry = new Avatar(dna);

                //If we do not pass in a HoloNETClient it will create it's own internal connection/client
                HoloNETEntries = new HoloNETObservableCollection<Avatar>("oasis", "load_avatars", "add_avatar", "remove_avatar", "bacth_update_collection", true, new HoloNETDNA()
                {
                    HolochainConductorAppAgentURI = $"ws://localhost:{initDemoResult.AttachedOnPort}",
                    InstalledAppId = HoloNETCollectionDemoAppId, //You need to set either the InstalledAppId or the AgentPubKey & DnaHash. You can set all 3 if you wish but only one or the other works fine too (if only the InstalledAppId is set it will look up the AgentPubKey & DnaHash from the conductor).
                    AgentPubKey = initDemoResult.AgentPubKey, //If you only set the AgentPubKey & DnaHash but not the InstalledAppId then it will still work fine.
                    DnaHash = initDemoResult.DnaHash,
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
                HoloNETEntries.OnInitialized += HoloNETEntries_OnInitialized;
                HoloNETEntries.OnClosed += HoloNETEntries_OnClosed;
                HoloNETEntries.OnCollectionLoaded += HoloNETEntries_OnCollectionLoaded;
                HoloNETEntries.OnCollectionSaved += HoloNETEntries_OnCollectionSaved;
                HoloNETEntries.OnError += HoloNETEntries_OnError;
                HoloNETEntries.OnHoloNETEntryAddedToCollection += HoloNETEntries_OnHoloNETEntryAddedToCollection;
                HoloNETEntries.OnHoloNETEntryRemovedFromCollection += HoloNETEntries_OnHoloNETEntryRemovedFromCollection;

                //Will wait until the HoloNET Entry has init (non blocking).
                await HoloNETEntries.WaitTillHoloNETInitializedAsync();

                //Refresh the list of installed hApps.
                ProcessListedApps(await HoloNETClientAdmin.ListAppsAsync(AppStatusFilter.All));

                ////Set the status to connected in the list of installed apps (this is good example of how you can access the internal HoloNETClient inside the HoloNET Entry).
                //if (_holoNETEntry.HoloNETClient != null && _holoNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                //    SetAppToConnectedStatus(_holoNETCollectionDemoAppId, _holoNETEntry.HoloNETClient.EndPoint.Port, false);

                UpdateNumerOfClientConnections();

                InitHoloNETEntryDemo = false;
                ShowAppsListedInLog = true;
                return true;
            }

            InitHoloNETEntryDemo = false;
            ShowAppsListedInLog = true;
            return false;
        }

        public async Task<HoloNETCollectionLoadedResult<Avatar>> LoadCollectionAsync()
        {
            HoloNETCollectionLoadedResult<Avatar> result = new HoloNETCollectionLoadedResult<Avatar>();

            // This extra check here will not normally be needed in a normal hApp (because you will not have the admin UI allowing you to uninstall, disable or disconnect).
            // But for extra defencive coding to be on the safe side you can of course double check! ;-)
            bool showAlreadyInitMessage = true;

            if (HoloNETEntries != null)
                showAlreadyInitMessage = await CheckIfDemoAppReadyAsync(false);

            // If we intend to re-use an object then we can store it globally so we only need to init once...
            // If the HoloNET Collection is null then we need to init it now...
            // Will init the HoloNET Collection which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
            if (HoloNETEntries == null || (HoloNETEntries != null && !HoloNETEntries.IsInitialized))
                await InitHoloNETCollectionAsync();

            else if (showAlreadyInitMessage)
            {
                ShowStatusMessage($"HoloNET Collection (Internal Connection) Already Initialized.", StatusMessageType.Information, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Collection (Internal Connection) Already Initialized..");
            }

            if (HoloNETEntries != null)
            {
                ShowStatusMessage($"Loading HoloNET Collection (Internal Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Loading HoloNET Collection (Internal Connection)...");

                // Non async way.
                // If you use LoadCollection or SaveAllChanges non-async versions you will need to wait for the OnInitialized event to fire before calling.
                // HoloNETEntries.LoadCollection(); //For this OnCollectionLoaded event handler above is required.

                // Async way.
                // LoadCollectionAsync will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                result = await HoloNETEntries.LoadCollectionAsync(); //No event handlers are needed.
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNETCollection (Internal Connection) Failed To Initialize";
            }

            return result;
        }

        public async Task<HoloNETCollectionSavedResult> SaveCollectionAsync()
        {
            HoloNETCollectionSavedResult result = new HoloNETCollectionSavedResult();

            // In case it has not been init properly when the popup was opened (it should have been though!)
            if (HoloNETEntries == null || (HoloNETEntries != null && !HoloNETEntries.IsInitialized))
                await InitHoloNETCollectionAsync();
           
            if (HoloNETEntries != null)
            {
                ShowStatusMessage($"Saving Changes For Collection (Internal Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Saving Changes For Collection (Internal Connection)...");

                // Non async way.
                // If you use LoadCollection or SaveAllChanges non-async versions you will need to wait for the OnInitialized event to fire before calling.
                // HoloNETEntries.SaveAllChanges(); //For this OnCollectionSaved event handler above is required.

                // Async way.
                // Will save all changes made to the collection (add, remove and updates). Will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                // Allows you to batch add/remove multiple entries to the collection and then persist the changes to the hc/rust/happ code in one go.
                // Will look for any changes since the last time this method was called (includes entries added/removed from the collection as well as any changes made to entries themselves).
                result = await HoloNETEntries.SaveAllChangesAsync();
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNETCollection (Internal Connection) Failed To Initialize";
            }

            return result;
        }

        private void HoloNETEntries_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection (Internal Connection) Initialized", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Collection (Internal Connection) Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
            }
        }

        private void HoloNETEntries_OnHoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Entry Added To Collection (Internal Connection).", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Entry Added To Collection (Internal Connection).");
            }
        }

        private void HoloNETEntries_OnHoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Entry Removed From Collection (Internal Connection).", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Entry Removed From Collection (Internal Connection).");
            }
        }

        private void HoloNETEntries_OnCollectionLoaded(object sender, HoloNETCollectionLoadedResult<Avatar> e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection (Internal Connection) Loaded.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Collection (Internal Connection) Loaded.");
            }
        }

        private void HoloNETEntries_OnCollectionSaved(object sender, HoloNETCollectionSavedResult e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection (Internal Connection) Saved.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Collection (Internal Connection) Saved.");
            }
        }

        private void HoloNETEntries_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection (Internal Connection) Closed.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                if (e.HolochainConductorsShutdownEventArgs != null)
                    LogMessage($"APP: HoloNET Collection (Internal Connection) Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
                else
                    LogMessage($"APP: HoloNET Collection (Internal Connection) Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
            }
        }

        private void HoloNETEntries_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"Error Occured For HoloNET Collection (Internal Connection). Reason: {e.Reason}", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: Error Occured For HoloNET Collection (Internal Connection). Reason: {e.Reason}");
        }
    }
}