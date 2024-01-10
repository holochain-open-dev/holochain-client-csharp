using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// //NOTE: EVERY method on HoloNETClient can be called either async or non-async, in these examples we are using a mixture of async and non-async. Normally you would use async because it is less code and easier to follow but we wanted to test and demo both versions (and show how you would use non async as well as async versions)...
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Will init the HoloNET Collection which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
        /// </summary>
        private async Task<bool> InitHoloNETCollection()
        {
            _initHoloNETEntryDemo = true;
            _showAppsListedInLog = false;

            bool initOk = false;
            int port;
            string dnaHash = "";
            string agentPubKey = "";

            (initOk, port, dnaHash, agentPubKey) = await InitDemoApp(_holoNETCollectionDemoAppId, _holoNETCollectionDemoHappPath, ucHoloNETCollectionEntryInternal);

            if (initOk)
            {
                LogMessage($"APP: Creating HoloNET Collection (Creating Internal HoloNETClient & Connecting To Port {port})...");
                ShowStatusMessage($"Creating HoloNET Collection (Creating Internal HoloNETClient & Connecting To Port {port})...", StatusMessageType.Information, true);
                ucHoloNETCollectionEntryInternal.ShowStatusMessage($"Creating Internal HoloNETClient & Connecting To Port {port}...", StatusMessageType.Information, true);

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
                    HolochainConductorAppAgentURI = $"ws://localhost:{port}",
                    InstalledAppId = _holoNETCollectionDemoAppId, //You need to set either the InstalledAppId or the AgentPubKey & DnaHash. You can set all 3 if you wish but only one or the other works fine too (if only the InstalledAppId is set it will look up the AgentPubKey & DnaHash from the conductor).
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
                ProcessListedApps(await _holoNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All));

                ////Set the status to connected in the list of installed apps (this is good example of how you can access the internal HoloNETClient inside the HoloNET Entry).
                //if (_holoNETEntry.HoloNETClient != null && _holoNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                //    SetAppToConnectedStatus(_holoNETCollectionDemoAppId, _holoNETEntry.HoloNETClient.EndPoint.Port, false);

                UpdateNumerOfClientConnections();

                _initHoloNETEntryDemo = false;
                _showAppsListedInLog = true;
                return true;
            }

            _initHoloNETEntryDemo = false;
            _showAppsListedInLog = true;
            return false;
        }

        private void CheckForChangesInHoloNETCollection()
        {
            if (HoloNETEntries != null && HoloNETEntries.IsChanges)
                btnHoloNETCollectionPopupSaveChanges.IsEnabled = true;
            else
                btnHoloNETCollectionPopupSaveChanges.IsEnabled = false;
        }

        private void btnViewHoloNETCollection_Click(object sender, RoutedEventArgs e)
        {
            btnViewHoloNETCollection.IsEnabled = false;
            btnViewHoloNETCollectionInfo.IsEnabled = true;
            stkpnlHoloNETCollectionInternal.Visibility = Visibility.Visible;
            stkpnlInfoHoloNETCollection.Visibility = Visibility.Collapsed;

        }

        private void btnViewHoloNETCollectionInfo_Click(object sender, RoutedEventArgs e)
        {
            btnViewHoloNETCollection.IsEnabled = true;
            btnViewHoloNETCollectionInfo.IsEnabled = false;
            stkpnlHoloNETCollectionInternal.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETCollection.Visibility = Visibility.Visible;
        }

        private void btnHoloNETCollection_Click(object sender, RoutedEventArgs e)
        {
            popupHoloNETCollection.Visibility = Visibility.Visible;

            Dispatcher.InvokeAsync(async () =>
            {
                //This extra check here will not normally be needed in a normal hApp (because you will not have the admin UI allowing you to uninstall, disable or disconnect).
                //But for extra defencive coding to be on the safe side you can of course double check! ;-)
                bool showAlreadyInitMessage = true;

                if (HoloNETEntries != null)
                    showAlreadyInitMessage = await CheckIfDemoAppReady(false);

                //If we intend to re-use an object then we can store it globally so we only need to init once...
                if (HoloNETEntries == null)
                {
                    LogMessage("APP: Initializing HoloNET Collection...");
                    ShowStatusMessage("Initializing HoloNET Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntryInternal);

                    // If the HoloNET Collection is null then we need to init it now...
                    // Will init the HoloNET Collection which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                    await InitHoloNETCollection();
                }
                else if (showAlreadyInitMessage)
                {
                    ShowStatusMessage($"APP: HoloNET Collection Already Initialized.", StatusMessageType.Information, false, ucHoloNETCollectionEntryInternal);
                    LogMessage($"HoloNET Collection Already Initialized..");
                }

                //Non async way.
                //If you use LoadCollection or SaveAllChanges non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...

                //HoloNETEntries.LoadCollection(); //For this OnCollectionLoaded event handler above is required //TODO: Check if this works without waiting for OnInitialized event!

                //Async way.
                if (HoloNETEntries != null)
                {
                    ShowStatusMessage($"APP: Loading HoloNET Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntryInternal);
                    LogMessage($"APP: Loading HoloNET Collection...");

                    //LoadCollectionAsync will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    HoloNETCollectionLoadedResult<Avatar> result = await HoloNETEntries.LoadCollectionAsync(); //No event handlers are needed.

                    if (!result.IsError)
                    {
                        ShowStatusMessage($"APP: HoloNET Collection Loaded.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                        LogMessage($"APP: HoloNET Collection Loaded.");

                        gridDataEntriesInternal.ItemsSource = HoloNETEntries;
                    }
                    else
                    {
                        //ucHoloNETCollectionEntryInternal.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        ShowStatusMessage($"APP: Error Occured Loading HoloNET Collection: {result.Message}", StatusMessageType.Error, false, ucHoloNETCollectionEntryInternal);
                        LogMessage($"APP: Error Occured Loading HoloNET Collection: {result.Message}");
                    }
                }
            });
        }

        private void btnHoloNETCollectionPopupClose_Click(object sender, RoutedEventArgs e)
        {
            popupHoloNETCollection.Visibility = Visibility.Collapsed;
        }

        private void btnHoloNETCollectionPopupRemoveEntryFromCollection_Click(object sender, RoutedEventArgs e)
        {
            Avatar avatar = gridDataEntriesInternal.SelectedItem as Avatar;

            if (avatar != null)
            {
                //btnHoloNETCollectionPopupAddEntryToCollection.IsEnabled = false;
                btnHoloNETCollectionPopupRemoveEntryFromCollection.IsEnabled = true;

                //Remove the item from the list(we could re-load the list from hc here but it is more efficient to just remove it from the in -memory collection).
                int index = -1;
                for (int i = 0; i < HoloNETEntries.Count; i++)
                {
                    if (HoloNETEntries[i] != null && HoloNETEntries[i].Id == avatar.Id)
                    {
                        index = i;
                        break;
                    }
                }

                if (index > -1)
                {
                    HoloNETEntries.RemoveAt(index);
                    gridDataEntriesInternal.ItemsSource = null;
                    gridDataEntriesInternal.ItemsSource = HoloNETEntries;
                    btnHoloNETCollectionPopupRemoveEntryFromCollection.IsEnabled = false;

                    CheckForChangesInHoloNETCollection();
                }
            }
        }

        private void btnHoloNETCollectionPopupAddEntryToCollection_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETCollectionEntryInternal.Validate())
            {
                HoloNETEntries.Add(new Avatar()
                {
                    Id = Guid.NewGuid(),
                    FirstName = ucHoloNETCollectionEntryInternal.txtHoloNETEntryFirstName.Text,
                    LastName = ucHoloNETCollectionEntryInternal.txtHoloNETEntryLastName.Text,
                    DOB = ucHoloNETCollectionEntryInternal.DOBDateTime,
                    Email = ucHoloNETCollectionEntryInternal.txtHoloNETEntryEmail.Text,
                });

                gridDataEntriesInternal.ItemsSource = HoloNETEntries;
                btnHoloNETCollectionPopupSaveChanges.IsEnabled = true;
            }
        }

        private void btnHoloNETCollectionPopupSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                ShowStatusMessage($"Saving Changes For Collection...", StatusMessageType.Information, true);
                LogMessage($"APP: Saving Changes For Collection...");

                //Will save all changes made to the collection (add, remove and updates).
                HoloNETCollectionSavedResult result = await HoloNETEntries.SaveAllChangesAsync();

                if (result != null && !result.IsError)
                {
                    ShowStatusMessage($"Changes Saved For Collection.", StatusMessageType.Success, false);
                    LogMessage($"APP: Changes Saved For Collection.");
                    btnHoloNETCollectionPopupSaveChanges.IsEnabled = false;

                    gridDataEntriesInternal.ItemsSource = null;
                    gridDataEntriesInternal.ItemsSource = HoloNETEntries;

                    ucHoloNETCollectionEntryInternal.FirstName = "";
                    ucHoloNETCollectionEntryInternal.LastName = "";
                    ucHoloNETCollectionEntryInternal.DOB = "";
                    ucHoloNETCollectionEntryInternal.Email = "";
                }
                else
                {
                    ucHoloNETCollectionEntryInternal.ShowStatusMessage(result.Message, StatusMessageType.Error);
                    ShowStatusMessage($"Error Occured Saving Changes For Collection: {result.Message}", StatusMessageType.Error);
                    LogMessage($"APP: Error Occured Saving Changes For Collection: {result.Message}");

                    //TODO:TEMP, REMOVE AFTER!
                    gridDataEntriesInternal.ItemsSource = null;
                    gridDataEntriesInternal.ItemsSource = HoloNETEntries;

                    ucHoloNETCollectionEntryInternal.FirstName = "";
                    ucHoloNETCollectionEntryInternal.LastName = "";
                    ucHoloNETCollectionEntryInternal.DOB = "";
                    ucHoloNETCollectionEntryInternal.Email = "";
                }
            });
        }

        private void gridDataEntriesInternal_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Avatar currentAvatar = gridDataEntriesInternal.SelectedItem as Avatar;

            if (currentAvatar != null)
                btnHoloNETCollectionPopupRemoveEntryFromCollection.IsEnabled = true;
            else
                btnHoloNETCollectionPopupRemoveEntryFromCollection.IsEnabled = false;
        }

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

        private void TxtHoloNETEntryInternalEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChangesInHoloNETCollection();
        }

        private void TxtHoloNETEntryInternalDOB_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChangesInHoloNETCollection();
        }

        private void TxtHoloNETEntryInternalLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChangesInHoloNETCollection();
        }

        private void TxtHoloNETEntryInternalFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChangesInHoloNETCollection();
        }
    }
}