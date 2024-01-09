using System.Threading.Tasks;
using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
        /// </summary>
        private async Task<bool> InitHoloNETEntry()
        {
            _initHoloNETEntryDemo = true;
            _showAppsListedInLog = false;

            bool initOk = false;
            int port;
            string dnaHash = "";
            string agentPubKey = "";

            (initOk, port, dnaHash, agentPubKey) = await InitDemoApp(_holoNETEntryDemoAppId, _holoNETEntryDemoHappPath, ucHoloNETEntry);

            if (initOk)
            {
                LogMessage($"APP: Creating HoloNET Entry (Creating Internal HoloNETClient & Connecting To Port {port})...");
                ShowStatusMessage($"Creating HoloNET Entry (Creating Internal HoloNETClient & Connecting To Port {port})...", StatusMessageType.Information, true);
                ucHoloNETEntry.ShowStatusMessage($"Creating Internal HoloNETClient & Connecting To Port {port}...", StatusMessageType.Information, true);

                //If you wanted to load a persisted DNA from disk and then make ammendments to it you would do this:
                //HoloNETDNA dna = HoloNETDNAManager.LoadDNA();
                //dna.HolochainConductorAppAgentURI = $"ws:\\localhost:{attachedResult.Port}";
                //dna.InstalledAppId = _holoNETEntryDemoAppId;
                //dna.AutoStartHolochainConductor = false;
                //dna.AutoStartHolochainConductor = false;
                //_holoNETEntry = new Avatar(dna);

                //If we do not pass in a HoloNETClient it will create it's own internal connection/client
                _holoNETEntry = new Avatar(new HoloNETDNA()
                {
                    HolochainConductorAppAgentURI = $"ws://localhost:{port}",
                    InstalledAppId = _holoNETEntryDemoAppId, //You need to set either the InstalledAppId or the AgentPubKey & DnaHash. You can set all 3 if you wish but only one or the other works fine too (if only the InstalledAppId is set it will look up the AgentPubKey & DnaHash from the conductor).
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
                _holoNETEntry.OnInitialized += _holoNETEntry_OnInitialized;
                _holoNETEntry.OnLoaded += _holoNETEntry_OnLoaded;
                _holoNETEntry.OnClosed += _holoNETEntry_OnClosed;
                _holoNETEntry.OnSaved += _holoNETEntry_OnSaved;
                _holoNETEntry.OnDeleted += _holoNETEntry_OnDeleted;
                _holoNETEntry.OnError += _holoNETEntry_OnError;

                //Will wait until the HoloNET Entry has init (non blocking).
                await _holoNETEntry.WaitTillHoloNETInitializedAsync();

                //Refresh the list of installed hApps.
                ProcessListedApps(await _holoNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All));

                ////Set the status to connected in the list of installed apps (this is good example of how you can access the internal HoloNETClient inside the HoloNET Entry).
                //if (_holoNETEntry.HoloNETClient != null && _holoNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                //    SetAppToConnectedStatus(_holoNETEntryDemoAppId, _holoNETEntry.HoloNETClient.EndPoint.Port, false);

                UpdateNumerOfClientConnections();

                _initHoloNETEntryDemo = false;
                _showAppsListedInLog = true;
                return true;
            }

            _initHoloNETEntryDemo = false;
            _showAppsListedInLog = true;
            return false;
        }

        private void RefreshHoloNETEntryMetaData(HoloNETAuditEntryBase holoNETEntry, ucHoloNETEntryMetaData userControl)
        {
            userControl.DataContext = holoNETEntry;


            //holoNETEntry.Id = Guid.NewGuid();
            //holoNETEntry.IsActive = true;
            //holoNETEntry.Version = 1;
            //holoNETEntry.CreatedBy = "David";
            //holoNETEntry.CreatedDate = DateTime.Now;
            //holoNETEntry.ModifiedBy = "David";
            //holoNETEntry.ModifiedDate = DateTime.Now;
            //holoNETEntry.DeletedBy = "David";
            //holoNETEntry.DeletedDate = DateTime.Now;
            //holoNETEntry.EntryHash = Guid.NewGuid().ToString();

            //userControl.Id = holoNETEntry.Id.ToString();
            //userControl.IsActive = holoNETEntry.IsActive == true ? "true" : "false";
            //userControl.Version = holoNETEntry.Version.ToString();
            //userControl.CreatedBy = holoNETEntry.CreatedBy.ToString();
            //userControl.CreatedDate = holoNETEntry.CreatedDate.ToShortDateString();
            //userControl.ModifiedBy = holoNETEntry.ModifiedBy.ToString();
            //userControl.ModifiedDate = holoNETEntry.ModifiedDate.ToShortDateString();
            //userControl.DeletedBy = holoNETEntry.DeletedBy.ToString();
            //userControl.DeletedDate = holoNETEntry.DeletedDate.ToShortDateString();
            //userControl.EntryHash = holoNETEntry.EntryHash;

            //userControl.EntryHash = holoNETEntry.EntryHash;
            //userControl.PreviousVersionEntryHash = holoNETEntry.PreviousVersionEntryHash;
            //userControl.Hash = holoNETEntry.EntryData.Hash;
            //userControl.ActionSequence = holoNETEntry.EntryData.ActionSequence.ToString();
            //userControl.EntryType = holoNETEntry.EntryData.EntryType;
            //userControl.OriginalActionAddress = holoNETEntry.EntryData.OriginalActionAddress;
            //userControl.OriginalEntryAddress = holoNETEntry.EntryData.OriginalEntryAddress;
            //userControl.Signature = holoNETEntry.EntryData.Signature;
            //userControl.Timestamp = holoNETEntry.EntryData.Timestamp.ToString();
            //userControl.Type = holoNETEntry.EntryData.Type;
        }

        private string GetHoloNETEntryMetaData(HoloNETAuditEntryBase entry)
        {
            return $"EntryHash: {_holoNETEntry.EntryHash}, Created By: {_holoNETEntry.CreatedBy}, Created Date: {_holoNETEntry.CreatedDate}, Modified By: {_holoNETEntry.ModifiedBy}, Modified Date: {_holoNETEntry.ModifiedDate}, Version: {_holoNETEntry.Version}, Previous Version Hash: {_holoNETEntry.PreviousVersionEntryHash}, Id: {_holoNETEntry.Id}, IsActive: {_holoNETEntry.IsActive}, Action Sequence: {_holoNETEntry.EntryData.ActionSequence}, EntryType: {_holoNETEntry.EntryData.EntryType}, Hash: {_holoNETEntry.EntryData.Hash}, OriginalActionAddress: {_holoNETEntry.EntryData.OriginalActionAddress}, OriginalEntryAddress: {_holoNETEntry.EntryData.OriginalEntryAddress}, Signature: {_holoNETEntry.EntryData.Signature}, TimeStamp: {_holoNETEntry.EntryData.Timestamp}, Type: {_holoNETEntry.EntryData.Type}";
        }

        private void ShowHoloNETEntryTab()
        {
            btnViewHoloNETEntry.IsEnabled = false;
            btnViewHoloNETEntryMetaData.IsEnabled = true;
            btnViewHoloNETEntryInfo.IsEnabled = true;
            ucHoloNETEntryMetaData.Visibility = Visibility.Collapsed;
            ucHoloNETEntry.Visibility = Visibility.Visible;
            stkpnlInfoHoloNETEntry.Visibility = Visibility.Collapsed;
        }

        private void ShowHoloNETEntryMetaDataTab()
        {
            btnViewHoloNETEntry.IsEnabled = true;
            btnViewHoloNETEntryMetaData.IsEnabled = false;
            btnViewHoloNETEntryInfo.IsEnabled = true;
            ucHoloNETEntryMetaData.Visibility = Visibility.Visible;
            ucHoloNETEntry.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETEntry.Visibility = Visibility.Collapsed;
        }

        private void ShowHoloNETEntryInfoTab()
        {
            btnViewHoloNETEntry.IsEnabled = true;
            btnViewHoloNETEntryMetaData.IsEnabled = true;
            btnViewHoloNETEntryInfo.IsEnabled = false;
            ucHoloNETEntryMetaData.Visibility = Visibility.Collapsed;
            ucHoloNETEntry.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETEntry.Visibility = Visibility.Visible;
        }

        private void btnViewHoloNETEntry_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntryTab();
        }

        private void btnViewHoloNETEntryMetaData_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntryMetaDataTab();
        }

        private void btnViewHoloNETEntryInfo_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntryInfoTab();
        }

        private void btnHoloNETEntry_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntryTab();
            popupHoloNETEntry.Visibility = Visibility.Visible;

            Dispatcher.InvokeAsync(async () =>
            {
                //This extra check here will not normally be needed in a normal hApp (because you will not have the admin UI allowing you to uninstall, disable or disconnect).
                //But for extra defencive coding to be on the safe side you can of course double check! ;-)
                bool showAlreadyInitMessage = true;

                if (_holoNETEntry != null)
                    showAlreadyInitMessage = await CheckIfDemoAppReady(true);

                //If we intend to re-use an object then we can store it globally so we only need to init once...
                if (_holoNETEntry == null)
                {
                    LogMessage("APP: Initializing HoloNET Entry...");
                    ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true, ucHoloNETEntry);

                    // If the HoloNET Entry is null then we need to init it now...
                    // Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                    await InitHoloNETEntry();
                }
                else if (showAlreadyInitMessage)
                {
                    ShowStatusMessage($"APP: HoloNET Entry Already Initialized.", StatusMessageType.Information, false, ucHoloNETEntry);
                    LogMessage($"HoloNET Entry Already Initialized..");
                }

                //Non async way.
                //If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...

                //ShowStatusMessage($"APP: Loading HoloNET Data Entry...", StatusMessageType.Information, true);
                //LogMessage($"APP: Loading HoloNET Data Entry...");

                //_holoNETEntry.Load(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash); //For this OnLoaded event handler above is required //TODO: Check if this works without waiting for OnInitialized event!


                //TODO: TEMP! REMOVE AFTER!
                RefreshHoloNETEntryMetaData(_holoNETEntry, ucHoloNETEntryMetaData);


                //Async way.
                if (_holoNETEntry != null && HoloNETEntryDNAManager.HoloNETEntryDNA != null && !string.IsNullOrEmpty(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash))
                {
                    ShowStatusMessage($"APP: Loading HoloNET Data Entry...", StatusMessageType.Information, true, ucHoloNETEntry);
                    LogMessage($"APP: Loading HoloNET Data Entry...");

                    //LoadAsync (as well as SaveAsync & DeleteAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await _holoNETEntry.LoadAsync(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash); //No event handlers are needed.

                    if (result.IsCallSuccessful && !result.IsError)
                    {
                        ShowStatusMessage($"APP: HoloNET Entry Loaded.", StatusMessageType.Success, false, ucHoloNETEntry);
                        LogMessage($"APP: HoloNET Entry Loaded. {GetHoloNETEntryMetaData(_holoNETEntry)}");

                        ucHoloNETEntry.DataContext = _holoNETEntry;
                        RefreshHoloNETEntryMetaData(_holoNETEntry, ucHoloNETEntryMetaData);
                    }
                    else
                    {
                        ucHoloNETEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        ShowStatusMessage($"APP: Error Occured Loading Entry: {result.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
                        LogMessage($"APP: Error Occured Loading Entry: {result.Message}");
                    }
                }
            });
        }

        private void btnHoloNETEntryPopupSave_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETEntry.Validate())
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    ///In case it has not been init properly when the popup was opened (it should have been though!)
                    if (_holoNETEntry == null)
                    {
                        LogMessage("APP: Initializing HoloNET Entry...");
                        ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true);

                        // If the HoloNET Entry is null then we need to init it now...
                        // Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                        await InitHoloNETEntry();
                    }

                    //Non async way.
                    //If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
                    //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...

                    //ShowStatusMessage($"APP: Saving HoloNET Data Entry...", StatusMessageType.Information, true);
                    //LogMessage($"APP: Saving HoloNET Data Entry...");

                    //string[] parts = txtHoloNETEntryDOB.Text.Split('/');

                    //_holoNETEntry.FirstName = txtHoloNETEntryFirstName.Text;
                    //_holoNETEntry.LastName = txtHoloNETEntryLastName.Text;
                    //_holoNETEntry.DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                    //_holoNETEntry.Email = txtHoloNETEntryEmail.Text;

                    //_holoNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                    //Async way.
                    if (_holoNETEntry != null)
                    {
                        ShowStatusMessage($"APP: Saving HoloNET Data Entry...", StatusMessageType.Information, true);
                        LogMessage($"APP: Saving HoloNET Data Entry...");

                        _holoNETEntry.FirstName = ucHoloNETEntry.FirstName;
                        _holoNETEntry.LastName = ucHoloNETEntry.LastName;
                        _holoNETEntry.DOB = ucHoloNETEntry.DOBDateTime;
                        _holoNETEntry.Email = ucHoloNETEntry.Email;

                        //SaveAsync (as well as LoadAsync and DeleteAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                        ZomeFunctionCallBackEventArgs result = await _holoNETEntry.SaveAsync(); //No event handlers are needed.

                        if (result.IsCallSuccessful && !result.IsError)
                        {
                            //Persist the entryhash so the next time we start the app we can re-load the entry from the hash.
                            HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = _holoNETEntry.EntryHash;
                            HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = result.Entries[0].EntryHash; //We can also get the entryHash from the callback eventargs.
                            HoloNETEntryDNAManager.SaveDNA();

                            RefreshHoloNETEntryMetaData(_holoNETEntry, ucHoloNETEntryMetaData);

                            ShowStatusMessage($"APP: HoloNET Entry Saved.", StatusMessageType.Success);
                            LogMessage($"APP: HoloNET Entry Saved. {GetHoloNETEntryMetaData(_holoNETEntry)}");
                            popupHoloNETEntry.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            ucHoloNETEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                            ShowStatusMessage($"APP: Error Occured Saving Entry: {result.Message}", StatusMessageType.Error);
                            LogMessage($"APP: Error Occured Saving Entry: {result.Message}");
                        }
                    }
                });
            }
        }

        private void btnHoloNETEntryPopupDelete_Click(object sender, RoutedEventArgs e)
        {
            ucHoloNETEntry.HideStatusMessage();

            Dispatcher.InvokeAsync(async () =>
            {
                ///In case it has not been init properly when the popup was opened (it should have been though!)
                if (_holoNETEntry == null)
                {
                    LogMessage("APP: Initializing HoloNET Entry...");
                    ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true);

                    // If the HoloNET Entry is null then we need to init it now...
                    // Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                    await InitHoloNETEntry();
                }

                //Non async way.
                //If you use Load, Save or Delete non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...

                //ShowStatusMessage($"APP: Deleting HoloNET Data Entry...", StatusMessageType.Information, true);
                //LogMessage($"APP: Deleting HoloNET Data Entry...");

                //_holoNETEntry.Delete(); //For this OnDeleted event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                //Async way.
                if (_holoNETEntry != null)
                {
                    ShowStatusMessage($"APP: Deleting HoloNET Data Entry...", StatusMessageType.Information, true);
                    LogMessage($"APP: Deleting HoloNET Data Entry...");

                    //DeleteAsync (as well as LoadAsync and SaveAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await _holoNETEntry.DeleteAsync(); //No event handlers are needed.

                    if (result.IsCallSuccessful && !result.IsError)
                    {
                        //Persist the entryhash so the next time we start the app we can re-load the entry from the hash.
                        HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = _holoNETEntry.EntryHash;
                        HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarEntryHash = result.Entries[0].EntryHash; //We can also get the entryHash from the callback eventargs.
                        HoloNETEntryDNAManager.SaveDNA();

                        RefreshHoloNETEntryMetaData(_holoNETEntry, ucHoloNETEntryMetaData);

                        ShowStatusMessage($"APP: HoloNET Entry Deleted.", StatusMessageType.Success);
                        LogMessage($"APP: HoloNET Entry Deleted. {GetHoloNETEntryMetaData(_holoNETEntry)}");
                        popupHoloNETEntry.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        ucHoloNETEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        ShowStatusMessage($"APP: Error Occured Deleting Entry: {result.Message}", StatusMessageType.Error);
                        LogMessage($"APP: Error Occured Deleting Entry: {result.Message}");
                    }
                }
            });
        }

        private void btnHoloNETEntryPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            ucHoloNETEntry.HideStatusMessage();
            popupHoloNETEntry.Visibility = Visibility.Hidden;
        }  
    }
}