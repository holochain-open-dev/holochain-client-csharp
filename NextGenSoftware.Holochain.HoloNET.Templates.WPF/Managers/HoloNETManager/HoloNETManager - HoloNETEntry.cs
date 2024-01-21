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
        public async Task<bool> InitHoloNETEntryAsync()
        {
            bool initOk = false;
            int port;
            string dnaHash = "";
            string agentPubKey = "";

            InitHoloNETEntryDemo = true;
            ShowAppsListedInLog = false;

            LogMessage("APP: Initializing HoloNET Entry...");
            ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

            (initOk, port, dnaHash, agentPubKey) = await InitDemoApp(HoloNETEntryDemoAppId, HoloNETEntryDemoHappPath);

            if (initOk)
            {
                LogMessage($"APP: Creating HoloNET Entry (Creating Internal HoloNETClient & Connecting To Port {port})...");
                ShowStatusMessage($"Creating HoloNET Entry (Creating Internal HoloNETClient & Connecting To Port {port})...", StatusMessageType.Information, true);
                HoloNETEntryUIManager.CurrentHoloNETEntryUI.ShowStatusMessage($"Creating Internal HoloNETClient & Connecting To Port {port}...", StatusMessageType.Information, true);

                // If you wanted to load a persisted DNA from disk and then make ammendments to it you would do this:
                //HoloNETDNA dna = HoloNETDNAManager.LoadDNA();
                //dna.HolochainConductorAppAgentURI = $"ws:\\localhost:{attachedResult.Port}";
                //dna.InstalledAppId = _holoNETEntryDemoAppId;
                //dna.AutoStartHolochainConductor = false;
                //dna.AutoStartHolochainConductor = false;
                //HoloNETEntry = new Avatar(dna);

                // If we do not pass in a HoloNETClient it will create it's own internal connection/client
                HoloNETEntry = new Avatar(new HoloNETDNA()
                {
                    HolochainConductorAppAgentURI = $"ws://localhost:{port}",
                    InstalledAppId = HoloNETEntryDemoAppId, // You need to set either the InstalledAppId or the AgentPubKey & DnaHash. You can set all 3 if you wish but only one or the other works fine too (if only the InstalledAppId is set it will look up the AgentPubKey & DnaHash from the conductor).
                    AgentPubKey = agentPubKey, // If you only set the AgentPubKey & DnaHash but not the InstalledAppId then it will still work fine.
                    DnaHash = dnaHash,
                    AutoStartHolochainConductor = false, // This defaults to true normally so make sure you set this to false because we can connect to the already running holochain.exe (the one admin is already using).
                    AutoShutdownHolochainConductor = false, // This defaults to true normally so make sure you set this to false.

                    //HolochainConductorToUse = HolochainConductorEnum.HcDevTool,
                    //ShowHolochainConductorWindow = true,

                    //FullPathToRootHappFolder = "E:\\hc\\holochain-holochain-0.1.5\\happs\\oasis",
                    //FullPathToCompiledHappFolder = "E:\\hc\\holochain-holochain-0.1.5\\happs\\oasis\\workdir"
                    //FullPathToRootHappFolder = "C:\\Users\\USER\\holochain-holochain-0.1.5\\happs\\oasis",
                    //FullPathToCompiledHappFolder = "C:\\Users\\USER\\holochain-holochain-0.1.5\\happs\\oasis\\workdir"
                });

                // If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                HoloNETEntry.OnInitialized += _holoNETEntry_OnInitialized;
                HoloNETEntry.OnLoaded += _holoNETEntry_OnLoaded;
                HoloNETEntry.OnSaved += _holoNETEntry_OnSaved;
                HoloNETEntry.OnDeleted += _holoNETEntry_OnDeleted;
                HoloNETEntry.OnClosed += _holoNETEntry_OnClosed;
                HoloNETEntry.OnError += _holoNETEntry_OnError;

                // Will wait until the HoloNET Entry has init (non blocking).
                await HoloNETEntry.WaitTillHoloNETInitializedAsync();

                // Refresh the list of installed hApps.
                ProcessListedApps(await HoloNETClientAdmin.ListAppsAsync(AppStatusFilter.All));

                // Set the status to connected in the list of installed apps (this is good example of how you can access the internal HoloNETClient inside the HoloNET Entry).
                //if (HoloNETEntry.HoloNETClient != null && HoloNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                //    SetAppToConnectedStatus(_holoNETEntryDemoAppId, HoloNETEntry.HoloNETClient.EndPoint.Port, false);

                UpdateNumerOfClientConnections();

                InitHoloNETEntryDemo = false;
                ShowAppsListedInLog = true;
                return true;
            }

            InitHoloNETEntryDemo = false;
            ShowAppsListedInLog = true;
            return false;
        }

        public async Task<ZomeFunctionCallBackEventArgs> LoadHoloNETEntryAsync()
        {
            ZomeFunctionCallBackEventArgs result = new ZomeFunctionCallBackEventArgs();

            //This extra check here will not normally be needed in a normal hApp (because you will not have the admin UI allowing you to uninstall, disable or disconnect).
            //But for extra defencive coding to be on the safe side you can of course double check! ;-)
            bool showAlreadyInitMessage = true;

            if (HoloNETEntry != null)
                showAlreadyInitMessage = await CheckIfDemoAppReadyAsync(true);

            // If we intend to re-use an object then we can store it globally so we only need to init once...
            // If the HoloNET Entry is null then we need to init it now...
            // Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
            if (HoloNETEntry == null || (HoloNETEntry != null && !HoloNETEntry.IsInitialized))
                await InitHoloNETEntryAsync();

            else if (showAlreadyInitMessage)
            {
                ShowStatusMessage($"HoloNET Entry Already Initialized.", StatusMessageType.Information, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: HoloNET Entry Already Initialized..");
            }

            if (HoloNETEntry != null && HoloNETEntryDNAManager.HoloNETEntryDNA != null && !string.IsNullOrEmpty(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash))
            {
                ShowStatusMessage($"Loading HoloNET Data Entry...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Loading HoloNET Data Entry...");

                //Non async way.
                // If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
                // HoloNETEntry.Load(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash); //For this OnLoaded event handler above is required.

                // Async way.
                //LoadAsync (as well as SaveAsync & DeleteAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                result = await HoloNETEntry.LoadAsync(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash); //No event handlers are needed.
            }
            else
            {
                if (HoloNETEntry == null)
                {
                    result.Message = "HoloNETEntry Failed To Initialize";
                    result.IsError = true;
                }
                else if (HoloNETEntryDNAManager.HoloNETEntryDNA == null)
                {
                    result.Message = "HoloNETEntryDNAManager.HoloNETEntryDNA is null. This is likely caused by a corrupt HoloNETEntryDNA.json file, please delete and retry.";
                    result.IsError = true;
                }
                else
                {
                    result.Message = "AvatarEntryHash in HoloNETEntryDNA is null. The likely cause is that no previous data has been saved for this entry.";
                    result.IsWarning = true;
                }
            }

            return result;
        }

        public async Task<ZomeFunctionCallBackEventArgs> SaveHoloNETEntryAsync()
        {
            ZomeFunctionCallBackEventArgs result = new ZomeFunctionCallBackEventArgs();

            // In case it has not been init properly when the popup was opened (it should have been though!)
            if (HoloNETEntry == null || (HoloNETEntry != null && !HoloNETEntry.IsInitialized))
                await InitHoloNETEntryAsync();

            if (HoloNETEntry != null)
            {
                ShowStatusMessage($"Saving HoloNET Data Entry...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Saving HoloNET Data Entry...");

                HoloNETEntry.FirstName = HoloNETEntryUIManager.CurrentHoloNETEntryUI.FirstName;
                HoloNETEntry.LastName = HoloNETEntryUIManager.CurrentHoloNETEntryUI.LastName;
                HoloNETEntry.DOB = HoloNETEntryUIManager.CurrentHoloNETEntryUI.DOBDateTime;
                HoloNETEntry.Email = HoloNETEntryUIManager.CurrentHoloNETEntryUI.Email;

                // Non async way.
                // If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
                // HoloNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!).

                // Async way.
                // SaveAsync (as well as LoadAsync and DeleteAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                result = await HoloNETEntry.SaveAsync(); //No event handlers are needed.

                if (!result.IsError)
                {
                    //Persist the entryhash so the next time we start the app we can re-load the entry from the hash.
                    HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = HoloNETEntry.EntryHash;
                    HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = result.Entries[0].EntryHash; //We can also get the entryHash from the callback eventargs.
                    HoloNETEntryDNAManager.SaveDNA();
                }
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNETEntry Failed To Initialize";
            }

            return result;
        }
        
        public async Task<ZomeFunctionCallBackEventArgs> DeleteHoloNETEntryAsync()
        {
            ZomeFunctionCallBackEventArgs result = new ZomeFunctionCallBackEventArgs();

            // In case it has not been init properly when the popup was opened (it should have been though!)
            if (HoloNETEntry == null || (HoloNETEntry != null && !HoloNETEntry.IsInitialized))
                await InitHoloNETEntryAsync();

            if (HoloNETEntry != null)
            {
                ShowStatusMessage($"Deleting HoloNET Data Entry...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                LogMessage($"APP: Deleting HoloNET Data Entry...");

                // Non async way.
                // If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
                // HoloNETEntry.Delete(); //For this OnDeleted event handler above is required (only if you want to see what the result was!).

                // Async way.
                // DeleteAsync (as well as LoadAsync and SaveAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                result = await HoloNETEntry.DeleteAsync(); //No event handlers are needed.

                if (!result.IsError)
                {
                    // Persist the entryhash so the next time we start the app we can re-load the entry from the hash.
                    HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = HoloNETEntry.EntryHash;
                    HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = result.Entries[0].EntryHash; //We can also get the entryHash from the callback eventargs.
                    HoloNETEntryDNAManager.SaveDNA(); 
                }
            }
            else
            {
                result.IsError = true;
                result.Message = "HoloNETEntry Failed To Initialize";
            }

            return result;
        }

        private void _holoNETEntry_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Initialized", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Data Entry Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
        }

        private void _holoNETEntry_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Loaded", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Data Entry Loaded: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            // For non async Save method you would use this callback but with async you can handle it in-line directly after the call to SaveAsync as we do in the btnHoloNETEntryPopupSave_Click event handler.
            //HandleHoloNETEntrySaved(e);
        }

        private void _holoNETEntry_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            // For non async Delete method you would use this callback but with async you can handle it in-line directly after the call to DeleteAsync as we do in the btnHoloNETEntryPopupDelete_Click event handler.
            //ShowStatusMessage($"APP: HoloNET Data Entry Deleted", StatusMessageType.Success);
            //LogMessage($"APP: HoloNET Data Entry Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Closed", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            //LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs != null ? e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown : 0}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");

            if (e.HolochainConductorsShutdownEventArgs != null)
                LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
            else
                LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
        }

        private void _holoNETEntry_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Error", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            LogMessage($"APP: HoloNET Data Entry Error: {e.Reason}");
        }
    }
}