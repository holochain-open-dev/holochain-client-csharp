using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void Init()
        {
            ucHoloNETCollectionEntry.txtHoloNETEntryFirstName.TextChanged += TxtHoloNETEntryFirstName_TextChanged;
            ucHoloNETCollectionEntry.txtHoloNETEntryLastName.TextChanged += TxtHoloNETEntryLastName_TextChanged;
            ucHoloNETCollectionEntry.txtHoloNETEntryDOB.TextChanged += TxtHoloNETEntryDOB_TextChanged;
            ucHoloNETCollectionEntry.txtHoloNETEntryEmail.TextChanged += TxtHoloNETEntryEmail_TextChanged;

            ucHoloNETCollectionEntryInternal.txtHoloNETEntryFirstName.TextChanged += TxtHoloNETEntryInternalFirstName_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryLastName.TextChanged += TxtHoloNETEntryInternalLastName_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryDOB.TextChanged += TxtHoloNETEntryInternalDOB_TextChanged;
            ucHoloNETCollectionEntryInternal.txtHoloNETEntryEmail.TextChanged += TxtHoloNETEntryInternalEmail_TextChanged;


            HoloNETEntryDNAManager.LoadDNA();

            _holoNETClientAdmin = new HoloNETClient();
            _holoNETClientAdmin.HoloNETDNA.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded;
            //_holoNETClientAdmin.HoloNETDNA.HolochainConductorToUse = HolochainConductorEnum.HcDevTool;
            _holoNETClientAdmin.HoloNETDNA.HolochainConductorToUse = HolochainConductorEnum.HolochainProductionConductor;

            txtAdminURI.Text = _holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI;
            chkAutoStartConductor.IsChecked = _holoNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor;
            chkAutoShutdownConductor.IsChecked = _holoNETClientAdmin.HoloNETDNA.AutoShutdownHolochainConductor;
            chkShowConductorWindow.IsChecked = _holoNETClientAdmin.HoloNETDNA.ShowHolochainConductorWindow;
            txtSecondsToWaitForConductorToStart.Text = _holoNETClientAdmin.HoloNETDNA.SecondsToWaitForHolochainConductorToStart.ToString();
            //_holoNETClientAdmin.HoloNETDNA.FullPathToRootHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis";
            // //_holoNETClientAdmin.HoloNETDNA.FullPathToRootHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5";
            // // _holoNETClientAdmin.HoloNETDNA.FullPathToCompiledHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ";

            _holoNETClientAdmin.OnHolochainConductorStarting += _holoNETClientAdmin_OnHolochainConductorStarting;
            _holoNETClientAdmin.OnHolochainConductorStarted += _holoNETClientAdmin_OnHolochainConductorStarted;
            _holoNETClientAdmin.OnConnected += _holoNETClientAdmin_OnConnected;
            _holoNETClientAdmin.OnError += _holoNETClientAdmin_OnError;
            _holoNETClientAdmin.OnDataSent += _holoNETClientAdmin_OnDataSent;
            _holoNETClientAdmin.OnDataReceived += _holoNETClientAdmin_OnDataReceived;
            _holoNETClientAdmin.OnDisconnected += _holoNETClientAdmin_OnDisconnected;
            _holoNETClientAdmin.OnAdminAgentPubKeyGeneratedCallBack += _holoNETClientAdmin_OnAdminAgentPubKeyGeneratedCallBack;
            _holoNETClientAdmin.OnAdminAppInstalledCallBack += _holoNETClientAdmin_OnAdminAppInstalledCallBack;
            _holoNETClientAdmin.OnAdminAppUninstalledCallBack += _holoNETClientAdmin_OnAdminAppUninstalledCallBack;
            _holoNETClientAdmin.OnAdminAppEnabledCallBack += _holoNETClientAdmin_OnAdminAppEnabledCallBack;
            _holoNETClientAdmin.OnAdminAppDisabledCallBack += _holoNETClientAdmin_OnAdminAppDisabledCallBack;
            _holoNETClientAdmin.OnAdminZomeCallCapabilityGrantedCallBack += _holoNETClientAdmin_OnAdminZomeCallCapabilityGrantedCallBack;
            _holoNETClientAdmin.OnAdminAppInterfaceAttachedCallBack += _holoNETClientAdmin_OnAdminAppInterfaceAttachedCallBack;
            _holoNETClientAdmin.OnAdminAppsListedCallBack += _holoNETClientAdmin_OnAdminAppsListedCallBack;
        }

       

        private HoloNETClient CreateNewAppAgentClientConnection(ushort port)
        {
            //_test[CurrentApp.Name] = new HoloNETClient(new HoloNETDNA()
            //{
            //    HolochainConductorAppAgentURI = $"ws://127.0.0.1:{port}",
            //    AgentPubKey = CurrentApp.AgentPubKey,
            //    DnaHash = CurrentApp.DnaHash,
            //    InstalledAppId = CurrentApp.Name,
            //    AutoStartHolochainConductor = false,
            //    AutoShutdownHolochainConductor = false
            //});

            ////_test[_holoNETappClients.Count - 1].OnConnected += _holoNETClientApp_OnConnected;
            ////_test[_holoNETappClients.Count - 1].OnReadyForZomeCalls += _holoNETClientApp_OnReadyForZomeCalls;
            ////_test[_holoNETappClients.Count - 1].OnZomeFunctionCallBack += _holoNETClientApp_OnZomeFunctionCallBack;
            ////_test[_holoNETappClients.Count - 1].OnDisconnected += _holoNETClientApp_OnDisconnected;
            ////_holoNETappClients[_holoNETappClients.Count - 1].OnDataReceived += _holoNETClientApp_OnDataReceived;
            ////_holoNETappClients[_holoNETappClients.Count - 1].OnDataSent += _holoNETClientApp_OnDataSent;
            ////_holoNETappClients[_holoNETappClients.Count - 1].OnError += _holoNETClientApp_OnError;

            //return _test[CurrentApp.Name];


            //_holoNETappClients.Add(new HoloNETClient(new HoloNETDNA()
            //{
            //    HolochainConductorAppAgentURI = $"ws://127.0.0.1:{port}",
            //    AgentPubKey = CurrentApp.AgentPubKey,
            //    DnaHash = CurrentApp.DnaHash,
            //    InstalledAppId = CurrentApp.Name,
            //    AutoStartHolochainConductor = false,
            //    AutoShutdownHolochainConductor = false
            //}));

            //_holoNETappClients[_holoNETappClients.Count - 1].OnConnected += _holoNETClientApp_OnConnected;
            //_holoNETappClients[_holoNETappClients.Count - 1].OnReadyForZomeCalls += _holoNETClientApp_OnReadyForZomeCalls;
            //_holoNETappClients[_holoNETappClients.Count - 1].OnZomeFunctionCallBack += _holoNETClientApp_OnZomeFunctionCallBack;
            //_holoNETappClients[_holoNETappClients.Count - 1].OnDisconnected += _holoNETClientApp_OnDisconnected;
            //_holoNETappClients[_holoNETappClients.Count - 1].OnDataReceived += _holoNETClientApp_OnDataReceived;
            //_holoNETappClients[_holoNETappClients.Count - 1].OnDataSent += _holoNETClientApp_OnDataSent;
            //_holoNETappClients[_holoNETappClients.Count - 1].OnError += _holoNETClientApp_OnError;

            //return _holoNETappClients[_holoNETappClients.Count - 1];

            //If you do not pass a HoloNETDNA in then it will use the one persisted to disk from the previous run if SaveDNA was called(if there is no saved DNA then it will use the defaults).
            HoloNETClient newClient = new HoloNETClient(new HoloNETDNA()
            {
                HolochainConductorAppAgentURI = $"ws://127.0.0.1:{port}"
            });

            newClient.OnConnected += _holoNETClientApp_OnConnected;
            newClient.OnReadyForZomeCalls += _holoNETClientApp_OnReadyForZomeCalls;
            newClient.OnZomeFunctionCallBack += _holoNETClientApp_OnZomeFunctionCallBack;
            newClient.OnDisconnected += _holoNETClientApp_OnDisconnected;
            newClient.OnDataReceived += _holoNETClientApp_OnDataReceived;
            newClient.OnDataSent += _holoNETClientApp_OnDataSent;
            newClient.OnError += _holoNETClientApp_OnError;

            //You can pass the HoloNETDNA in via the constructor as we have above or you can set it via the property below:
            newClient.HoloNETDNA.AutoStartHolochainConductor = false;
            newClient.HoloNETDNA.AutoShutdownHolochainConductor = false;

            //You need to set either the InstalledAppId or the AgentPubKey & DnaHash. You can set all 3 if you wish but only one or the other works fine too (if only the InstalledAppId is set it will look up the AgentPubKey & DnaHash from the conductor). 
            newClient.HoloNETDNA.AgentPubKey = CurrentApp.AgentPubKey; //If you only set the AgentPubKey & DnaHash but not the InstalledAppId then it will still work fine.
            newClient.HoloNETDNA.DnaHash = CurrentApp.DnaHash;
            newClient.HoloNETDNA.InstalledAppId = CurrentApp.Name;

            return newClient;
        }

        /// <summary>
        /// Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
        /// </summary>
        private async Task<(bool, int, string, string)> InitDemoApp(string appId, string hAppInstallPath, ucHoloNETEntry ucHoloNETEntry)
        {
            LogMessage($"ADMIN: Checking If App {appId} Is Already Installed...");
            ShowStatusMessage($"Checking If App {appId} Is Already Installed...", StatusMessageType.Information, true, ucHoloNETEntry);

            AdminGetAppInfoCallBackEventArgs appInfoResult = await _holoNETClientAdmin.AdminGetAppInfoAsync(appId);

            if (appInfoResult != null && appInfoResult.AppInfo != null)
            {
                ShowStatusMessage($"App {appId} Is Already Installed So Uninstalling Now...", StatusMessageType.Information, true, ucHoloNETEntry);
                LogMessage($"ADMIN: App {appId} Is Already Installed So Uninstalling Now...");

                AdminAppUninstalledCallBackEventArgs uninstallResult = await _holoNETClientAdmin.AdminUninstallAppAsync(appId);

                if (uninstallResult != null && uninstallResult.IsError)
                {
                    LogMessage($"ADMIN: Error Uninstalling App {appId}. Reason: {uninstallResult.Message}");
                    ShowStatusMessage($"Error Uninstalling App {appId}. Reason: {uninstallResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
                }
                else
                {
                    LogMessage($"ADMIN: Uninstalled App {appId}.");
                    ShowStatusMessage($"Uninstalled App {appId}.", StatusMessageType.Error, false, ucHoloNETEntry);
                }
            }
            else
            {
                LogMessage($"ADMIN: {appId} App Not Found.");
                ShowStatusMessage($"{appId} App Not Found.", StatusMessageType.Information, true, ucHoloNETEntry);
            }


            LogMessage($"ADMIN: Generating New AgentPubKey...");
            ShowStatusMessage($"Generating New AgentPubKey...", StatusMessageType.Information, true, ucHoloNETEntry);

            AdminAgentPubKeyGeneratedCallBackEventArgs agentPubKeyResult = await _holoNETClientAdmin.AdminGenerateAgentPubKeyAsync();

            if (agentPubKeyResult != null && !agentPubKeyResult.IsError)
            {
                LogMessage($"ADMIN: AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}");
                ShowStatusMessage($"AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}", StatusMessageType.Success, false, ucHoloNETEntry);

                LogMessage($"ADMIN: Installing App {appId}...");
                ShowStatusMessage($"Installing App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

                AdminAppInstalledCallBackEventArgs installedResult = await _holoNETClientAdmin.AdminInstallAppAsync(appId, hAppInstallPath);

                if (installedResult != null && !installedResult.IsError)
                {
                    LogMessage($"ADMIN: {appId} App Installed.");
                    ShowStatusMessage($"{appId} App Installed.", StatusMessageType.Success, false, ucHoloNETEntry);

                    LogMessage($"ADMIN: Enabling App {appId}...");
                    ShowStatusMessage($"Enabling App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

                    AdminAppEnabledCallBackEventArgs enabledResult = await _holoNETClientAdmin.AdminEnableAppAsync(appId);

                    if (enabledResult != null && !enabledResult.IsError)
                    {
                        LogMessage($"ADMIN: {appId} App Enabled.");
                        ShowStatusMessage($"{appId} App Enabled.", StatusMessageType.Success, false);

                        LogMessage($"ADMIN: Signing Credentials (Zome Call Capabilities) For App {appId}...");
                        ShowStatusMessage($"Signing Credentials (Zome Call Capabilities) For App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

                        AdminZomeCallCapabilityGrantedCallBackEventArgs signingResult = await _holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(installedResult.CellId, CapGrantAccessType.Unrestricted, GrantedFunctionsType.All);

                        //Un-comment this line and comment the above one to grant only specefic zome functions.
                        //_holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(installedResult.CellId, CapGrantAccessType.Assigned, GrantedFunctionsType.Listed, new List<(string, string)>()
                        //{
                        //    ("oasis", "create_avatar"),
                        //    ("oasis", "get_avatar"),
                        //    ("oasis", "update_avatar")
                        //});

                        if (signingResult != null && !signingResult.IsError)
                        {
                            LogMessage($"ADMIN: {appId} App Signing Credentials Authorized.");
                            ShowStatusMessage($"{appId} App Signing Credentials Authorized.", StatusMessageType.Success, false, ucHoloNETEntry);

                            LogMessage($"ADMIN: Attaching App Interface For App {appId}...");
                            ShowStatusMessage($"Attaching App Interface For App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

                            AdminAppInterfaceAttachedCallBackEventArgs attachedResult = await _holoNETClientAdmin.AdminAttachAppInterfaceAsync();

                            if (attachedResult != null && !attachedResult.IsError)
                            {
                                LogMessage($"ADMIN: {appId} App Interface Attached On Port {attachedResult.Port}.");
                                ShowStatusMessage($"{appId} App Interface Attached On Port {attachedResult.Port}.", StatusMessageType.Success, false, ucHoloNETEntry);
                                return (true, attachedResult.Port.Value, installedResult.DnaHash, installedResult.AgentPubKey);
                            }
                            else
                            {
                                LogMessage($"ADMIN: Error Attaching App Interface For App {appId}. Reason: {attachedResult.Message}");
                                ShowStatusMessage($"ADMIN: Error Attaching App Interface For App {appId}. Reason: {attachedResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
                            }
                        }
                        else
                        {
                            LogMessage($"ADMIN: Error Signing Credentials For App {appId}. Reason: {signingResult.Message}");
                            ShowStatusMessage($"ADMIN: Error Signing Credentials For App {appId}. Reason: {signingResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
                        }
                    }
                    else
                    {
                        LogMessage($"ADMIN: Error Enabling App {appId}. Reason: {enabledResult.Message}.");
                        ShowStatusMessage($"ADMIN: Error Enabling App {appId}. Reason: {enabledResult.Message}.", StatusMessageType.Error, false, ucHoloNETEntry);
                    }
                }
                else
                {
                    LogMessage($"ADMIN: Error Installing App {appId}. Reason: {installedResult.Message}.");
                    ShowStatusMessage($"ADMIN: Error Installing App {appId}. Reason: {installedResult.Message}.", StatusMessageType.Error, false, ucHoloNETEntry);
                }
            }
            else
            {
                LogMessage($"ADMIN: Error Generating AgentPubKey. Reason: {agentPubKeyResult.Message}");
                ShowStatusMessage($"Error Generating AgentPubKey. Reason: {agentPubKeyResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
            }

            return (false, 0, "", "");
        }


        private void ConnectAdmin()
        {
            _clientsDisconnected = 0;
            _clientsToDisconnect = 0;
            _rebooting = false;
            _adminDisconnected = false;

            if (!_holoNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor)
            {
                LogMessage($"ADMIN: Connecting to Admin WebSocket on endpoint: {_holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI}...");
                ShowStatusMessage($"Admin WebSocket Connecting To Endpoint {_holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI}...", StatusMessageType.Information, true);
            }

            _holoNETClientAdmin.ConnectAdmin(_holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI);
        }

        private void ListHapps()
        {
            if (_showAppsListedInLog)
            {
                LogMessage("ADMIN: Listing hApps...");
                ShowStatusMessage($"Listing hApps...", StatusMessageType.Information, true);
            }

            _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
        }

        private void CloseAllConnections()
        {
            LogMessage("APP: Disconnecting All HoloNETClient AppAgent WebSockets...");
            ShowStatusMessage($"Disconnecting All HoloNETClient AppAgent WebSockets...", StatusMessageType.Information, true);

            _removeClientConnectionFromPoolAfterDisconnect = true;

            foreach (HoloNETClient client in _holoNETappClients)
            {
                if (client.State == System.Net.WebSockets.WebSocketState.Open)
                    client.Disconnect();

                //client = null;
            }

            if (_holoNETEntry != null)
            {
                _holoNETEntry.Close();
                _holoNETEntry = null;
            }

            if (HoloNETEntries != null)
            {
                HoloNETEntries.Close();
                HoloNETEntries = null;
            }

            LogMessage("ADMIN: Disconnecting...");
            ShowStatusMessage($"Disconnecting Admin WebSocket...", StatusMessageType.Information, true);

            _adminDisconnected = false;

            if (_holoNETClientAdmin != null)
            {
                if (_holoNETClientAdmin.State == System.Net.WebSockets.WebSocketState.Open)
                    _holoNETClientAdmin.Disconnect();
            }
        }

        private ConnectToAppAgentClientResult ConnectToAppAgentClient()
        {
            ConnectToAppAgentClientResult result = new ConnectToAppAgentClientResult();
            CurrentApp = gridHapps.SelectedItem as InstalledApp;

            if (CurrentApp != null)
            {
                result.AppAgentClient = GetClient(CurrentApp.DnaHash, CurrentApp.AgentPubKey, CurrentApp.Name);

                //If we find an existing client then that means it has already been authorized, attached and connected.
                if (result.AppAgentClient != null)
                {
                    LogMessage($"APP: Found Existing HoloNETClient AppAgent WebSocket For AgentPubKey {result.AppAgentClient.HoloNETDNA.AgentPubKey}, DnaHash {result.AppAgentClient.HoloNETDNA.DnaHash} And InstalledAppId {result.AppAgentClient.HoloNETDNA.InstalledAppId} Running On Port {result.AppAgentClient.EndPoint.Port}.");

                    if (result.AppAgentClient.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {result.AppAgentClient.EndPoint.Port} Is Open.");
                        result.ResponseType = ConnectToAppAgentClientResponseType.Connected;
                    }
                    else
                    {
                        LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {result.AppAgentClient.EndPoint.Port} Is NOT Open!");
                        ShowStatusMessage($"Re-Connecting To HoloNETClient AppAgent WebSocket On Port {result.AppAgentClient.EndPoint.Port}...", StatusMessageType.Information, true);
                        LogMessage($"APP: Re-Connecting To HoloNETClient AppAgent WebSocket On Port {result.AppAgentClient.EndPoint.Port}...");

                        result.AppAgentClient.Connect();
                        result.ResponseType = ConnectToAppAgentClientResponseType.Connecting;
                    }
                }
                else
                {
                    LogMessage($"ADMIN: No Existing HoloNETClient AppAgent WebSocket Found For AgentPubKey {CurrentApp.AgentPubKey}, DnaHash {CurrentApp.DnaHash} And InstalledAppId {CurrentApp.Name} So New Connection Needs To Be Made...");
                    LogMessage($"ADMIN: Authorizing Signing Credentials For {CurrentApp.Name} hApp...");
                    ShowStatusMessage($"Authorizing Signing Credentials For {CurrentApp.Name} hApp...", StatusMessageType.Information, true);

                    //_holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(GrantedFunctionsType.Listed, new List<(string, string)>()
                    //{
                    //    ("oasis", "create_avatar"),
                    //    ("oasis", "get_avatar"),
                    //    ("oasis", "update_avatar")
                    //});

                    _holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(_holoNETClientAdmin.GetCellId(CurrentApp.DnaHash, CurrentApp.AgentPubKey), CapGrantAccessType.Unrestricted, GrantedFunctionsType.All, null);
                    result.ResponseType = ConnectToAppAgentClientResponseType.GrantingZomeCapabilities;
                }
            }
            else
            {
                LogMessage("APP: CurrentApp Not Found!");
                ShowStatusMessage("CurrentApp Not Found!", StatusMessageType.Error, true);
                result.ResponseType = ConnectToAppAgentClientResponseType.CurrentAppNotFound;
            }

            return result;
        }

        /// <summary>
        /// Process the client operation which was queued before the HoloNET AppAgent was connected. This is only needed in non-async code, async code is much simplier and less! ;-)
        /// </summary>
        /// <param name="client"></param>
        private void ProcessClientOperation(HoloNETClient client)
        {
            switch (_clientOperation)
            {
                case ClientOperation.CallZomeFunction:
                    {
                        if (_paramsObject != null)
                        {
                            LogMessage("APP: Calling Zome Function...");
                            ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                            client.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, _paramsObject);
                            _paramsObject = null;
                        }
                        else
                        {
                            LogMessage("ADMIN: Error: Zome paramsObject is null! Please try again");
                            ShowStatusMessage("Error: Zome paramsObject is null! Please try again", StatusMessageType.Error);
                        }
                        _clientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.LoadHoloNETEntryShared:
                    {
                        LoadHoloNETEntryShared(client);
                        _clientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.SaveHoloNETEntryShared:
                    {
                        SaveHoloNETEntryShared(client, ucHoloNETCollectionEntry.FirstName, ucHoloNETCollectionEntry.LastName, ucHoloNETCollectionEntry.DOBDateTime, ucHoloNETCollectionEntry.Email);
                        _clientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.LoadHoloNETCollection:
                    {
                        LoadCollection(client);
                        _clientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.AddHoloNETEntryToCollection:
                    {
                        AddHoloNETEntryToCollection(client, ucHoloNETCollectionEntry.FirstName, ucHoloNETCollectionEntry.LastName, ucHoloNETCollectionEntry.DOBDateTime, ucHoloNETCollectionEntry.Email);
                        _clientOperation = ClientOperation.None;
                    }
                    break;
            }
        }  

        private string GetEntryInfo(ZomeFunctionCallBackEventArgs e)
        {
            if (e != null && e.Entries.Count > 0)
                return $"DateTime: {e.Entries[0].DateTime}, Author: {e.Entries[0].Author}, ActionSequence: {e.Entries[0].ActionSequence}, Signature: {e.Entries[0].Signature}, Type: {e.Entries[0].Type}, Hash: {e.Entries[0].Hash}, Previous Hash: {e.Entries[0].PreviousHash}, OriginalActionAddress: {e.Entries[0].OriginalActionAddress}, OriginalEntryAddress: {e.Entries[0].OriginalEntryAddress}";
            else
                return "";
        }

        private void ShowStatusMessage(string message, StatusMessageType type = StatusMessageType.Information, bool showSpinner = false, ucHoloNETEntry ucHoloNETEntry = null)
        {
            txtStatus.Text = $"{message}";

            switch (type)
            {
                case StatusMessageType.Success:
                    txtStatus.Foreground = Brushes.LightGreen;
                    break;

                case StatusMessageType.Information:
                    txtStatus.Foreground = Brushes.White;
                    break;

                case StatusMessageType.Error:
                    txtStatus.Foreground = Brushes.LightSalmon;
                    break;
            }

            if (showSpinner)
                spinner.Visibility = Visibility.Visible;
            else
                spinner.Visibility = Visibility.Hidden;

            if (ucHoloNETEntry != null)
                ucHoloNETEntry.ShowStatusMessage(message, type, showSpinner);

            sbAnimateStatus.Begin();
        }

        private void LogMessage(string message)
        {
            lstOutput.Items.Add(message);

            if (VisualTreeHelper.GetChildrenCount(lstOutput) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(lstOutput, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
        private void UpdateNumerOfClientConnections()
        {
            //First count the number of shared connections.
            //int noConnections = _holoNETappClients.Count;
            int noConnections = 0;

            foreach (HoloNETClient client in _holoNETappClients)
            {
                if (client.State == System.Net.WebSockets.WebSocketState.Open)
                    noConnections++;
            }

            //If the HoloNETEntry that is using its own internal connection is connected then count this too.
            if (_holoNETEntry != null && _holoNETEntry.HoloNETClient != null && _holoNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                noConnections++;

            //If the HoloNET Collection that is using its own internal connection is connected then count this too.
            if (HoloNETEntries != null && HoloNETEntries.HoloNETClient != null && HoloNETEntries.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                noConnections++;

            txtConnections.Text = $"Client Connections: {noConnections}";
            sbAnimateConnections.Begin();
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = string.Concat("EndPoint: ", args.EndPoint, ", Id: ", args.Id, ", Zome: ", args.Zome, ", ZomeFunction: ", args.ZomeFunction, ", ZomeReturnData: ", args.ZomeReturnData, ", ZomeReturnHash: ", args.ZomeReturnHash, ", Raw Zome Return Data: ", args.RawZomeReturnData, ", Raw Binary Data: ", args.RawBinaryDataDecoded, "(", args.RawBinaryDataAsString, "), IsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", ", IsError: ", args.IsError ? "true" : "false", ", Message: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, ", Processed Zome Return Data: ", args.KeyValuePairAsString);

            return result;
        }

        private void SetCurrentAppToConnectedStatus(int port)
        {
            CurrentApp = InstalledApps.FirstOrDefault(x => x.Name == CurrentApp.Name && x.DnaHash == CurrentApp.DnaHash && x.AgentPubKey == CurrentApp.AgentPubKey);
            SetAppToConnectedStatus(CurrentApp, port, true);
        }

        private void SetAppToConnectedStatus(InstalledApp app, int port, bool isSharedConnection, bool refreshGrid = true)
        {
            if (app != null)
            {
                app.IsSharedConnection = isSharedConnection;
                app.IsConnected = true;
                app.Status = "Running (Connected)";
                app.StatusReason = "AppAgent Client Connected";
                app.Port = port.ToString();

                if (refreshGrid)
                    gridHapps.ItemsSource = InstalledApps.OrderBy(x => x.Name);
            }
        }

        private void SetAppToConnectedStatus(string appName, int port, bool isSharedConnection, bool refreshGrid = true)
        {
            SetAppToConnectedStatus(InstalledApps.FirstOrDefault(x => x.Name == appName), port, isSharedConnection, refreshGrid);
        }

        private void ProcessListedApps(AdminAppsListedCallBackEventArgs listedAppsResult)
        {
            string hApps = "";
            InstalledApps.Clear();

            foreach (AppInfo app in listedAppsResult.Apps)
            {
                InstalledApp installedApp = new InstalledApp()
                {
                    AgentPubKey = app.AgentPubKey,
                    DnaHash = app.DnaHash,
                    Name = app.installed_app_id,
                    Manifest = $"{app.manifest.name} v{app.manifest.manifest_version} {(!string.IsNullOrEmpty(app.manifest.description) ? string.Concat('(', app.manifest.description, ')') : "")}",
                    Status = $"{Enum.GetName(typeof(AppInfoStatusEnum), app.AppStatus)}",
                    StatusReason = app.AppStatusReason,
                    IsEnabled = app.AppStatus == AppInfoStatusEnum.Disabled ? false : true,
                    IsDisabled = app.AppStatus == AppInfoStatusEnum.Disabled ? true : false,
                    IsSharedConnection = app.installed_app_id != _holoNETEntryDemoAppId && app.installed_app_id != _holoNETCollectionDemoAppId ? true : false
                };

                if (app.AppStatus == AppInfoStatusEnum.Running)
                {
                    HoloNETClient client = GetClient(installedApp.DnaHash, installedApp.AgentPubKey, installedApp.Name);

                    if (client != null && client.State == System.Net.WebSockets.WebSocketState.Open)
                        SetAppToConnectedStatus(installedApp, client.EndPoint.Port, true, false);
                }

                InstalledApps.Add(installedApp);

                if (hApps != "")
                    hApps = $"{hApps}, ";

                hApps = $"{hApps}{app.installed_app_id}";
            }

            //Set the status to connected in the list of installed apps (this is good example of how you can access the internal HoloNETClient inside the HoloNET Entry).
            //This HoloNETEntry uses its own internal HoloNETClient connection so it will not be shared with CallZomeFunction or View Data Entries popups (it is technically possible to share it if you wanted to but it is not recommended because it is controlled by the HoloNETEntry and when it is closed it will automatically close the connection too).
            if (_holoNETEntry != null && _holoNETEntry.HoloNETClient != null && _holoNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                SetAppToConnectedStatus(_holoNETEntryDemoAppId, _holoNETEntry.HoloNETClient.EndPoint.Port, false);

            if (HoloNETEntries != null && HoloNETEntries.HoloNETClient != null && HoloNETEntries.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                SetAppToConnectedStatus(_holoNETCollectionDemoAppId, HoloNETEntries.HoloNETClient.EndPoint.Port, false);

            gridHapps.ItemsSource = InstalledApps.OrderBy(x => x.Name);

            if (_showAppsListedInLog)
            {
                LogMessage($"ADMIN: hApps Listed: {hApps}");
                ShowStatusMessage("hApps Listed.", StatusMessageType.Success);
            }
        }

        private HoloNETClient GetClient(string dnaHash, string agentPubKey, string installedAppId)
        {
            return _holoNETappClients.FirstOrDefault(x => x.HoloNETDNA.DnaHash == dnaHash && x.HoloNETDNA.AgentPubKey == agentPubKey && x.HoloNETDNA.InstalledAppId == installedAppId);
        }

        private async Task<bool> CheckIfDemoAppReady(bool isEntry)
        {
            //Supress event callbacks for the shared connections.
            _showAppsListedInLog = false;
            bool showAlreadyInitMessage = false;
            string item = "";
            string appId = "";

            if (isEntry)
            {
                item = "Entry";
                appId = _holoNETEntryDemoAppId;
            }
            else
            {
                item = "Collection";
                appId = _holoNETCollectionDemoAppId;
            }

            LogMessage($"APP: HoloNET {item} Already Initialized.");
            ShowStatusMessage($"HoloNET {item} Already Initialized.", StatusMessageType.Information, false);

            LogMessage($"ADMIN: Checking If Test App {appId} Is Still Installed...");
            ShowStatusMessage($"Checking If Test App {appId} Is Still Installed...", StatusMessageType.Success, true, ucHoloNETEntry);
            //ucHoloNETEntry.ShowStatusMessage($"Checking If Test App {appId} Is Still Installed...", StatusMessageType.Information, true);

            AdminGetAppInfoCallBackEventArgs appInfoResult = await _holoNETClientAdmin.AdminGetAppInfoAsync(appId);

            //If the test app was manually uninstalled by the user then we need to re-init the HoloNET Entry/Collection now...
            if (appInfoResult != null && appInfoResult.AppInfo == null || appInfoResult.IsError)
            {
                LogMessage($"ADMIN: Test App {appId} Is NOT Installed.");
                ShowStatusMessage($"ADMIN App {appId} Is NOT Installed.", StatusMessageType.Error, false, ucHoloNETEntry);

                LogMessage($"APP: Closing HoloNET {item} (And Closing Internal HoloNETClient Connection)...");
                ShowStatusMessage($"Closing HoloNET {item} (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true, ucHoloNETEntry);
                //ucHoloNETEntry.ShowStatusMessage($"Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true);

                await CloseConnection(isEntry);
            }
            else
            {
                LogMessage($"ADMIN: Test App {appId} Still Installed.");
                ShowStatusMessage($"ADMIN App {appId} Still Installed.", StatusMessageType.Information, false);

                LogMessage($"ADMIN: Checking If Test App {appId} Is Still Running (Enabled)...");
                ShowStatusMessage($"Checking If Test App {appId} Is Still Running (Enabled)...", StatusMessageType.Information, true, ucHoloNETEntry);
                //ucHoloNETEntry.ShowStatusMessage($"Checking If Test App {appId} Is Still Running (Enabled)...", StatusMessageType.Information, true);

                if (appInfoResult.AppInfo.AppStatus == AppInfoStatusEnum.Running)
                {
                    LogMessage($"ADMIN: Test App {appId} Still Running (Enabled).");
                    ShowStatusMessage($"Test App {appId} Still Running (Enabled).", StatusMessageType.Success, false, ucHoloNETEntry);
                }
                else
                {
                    LogMessage($"ADMIN Test App {appId} NOT Running (Disabled).");
                    ShowStatusMessage($"Test App {appId} NOT Running (Disabled).", StatusMessageType.Error, false, ucHoloNETEntry);

                    LogMessage($"ADMIN: Enabling Test App {appId}...");
                    ShowStatusMessage($"Enabling Test App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

                    AdminAppEnabledCallBackEventArgs enabledResult = await _holoNETClientAdmin.AdminEnableAppAsync(appId);

                    if (enabledResult != null && !enabledResult.IsError)
                    {
                        //No need to show because the event callback already logs this message.
                        //LogMessage($"ADMIN: Test App {appId} Enabled.");
                        //ShowStatusMessage($"Test App {appId} Enabled.", StatusMessageType.Information, false);
                    }
                    else
                    {
                        LogMessage($"ADMIN: Error Occured Enabling Test App {appId}. Reason: {enabledResult.Message}");
                        ShowStatusMessage($"Error Occured Enabling Test App {appId}. Reason: {enabledResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);

                        LogMessage($"APP: Closing HoloNET {item} (And Closing Internal HoloNETClient Connection)...");
                        ShowStatusMessage($"Closing HoloNET {item} (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true, ucHoloNETEntry);

                        await CloseConnection(isEntry);
                    }
                }

                LogMessage($"APP: Checking If Test App {appId} HoloNETClient WebSocket Connection Is Open...");
                ShowStatusMessage($"Checking If Test App {appId} HoloNETClient WebSocket Connection Is Open...", StatusMessageType.Information, true, ucHoloNETEntry);

                if (isEntry)
                {
                    if (_holoNETEntry != null && _holoNETEntry.HoloNETClient != null && _holoNETEntry.HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                    {
                        LogMessage($"APP: Test App {appId} HoloNETClient WebSocket Connection Is Not Open!");
                        ShowStatusMessage($"Test App {appId} HoloNETClient WebSocket Connection Is Not Open!", StatusMessageType.Error, false, ucHoloNETEntry);

                        LogMessage($"APP: Opening HoloNETClient WebSocket Connection On Port {_holoNETEntry.HoloNETClient.EndPoint.Port}...");
                        ShowStatusMessage($"Opening HoloNETClient WebSocket Connection On Port {_holoNETEntry.HoloNETClient.EndPoint.Port}...", StatusMessageType.Information, true, ucHoloNETEntry);

                        await _holoNETEntry.HoloNETClient.ConnectAsync();

                        if (_holoNETEntry != null && _holoNETEntry.HoloNETClient != null && _holoNETEntry.HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                        {
                            LogMessage($"APP: Failed To Open Connection!");
                            ShowStatusMessage($"Failed To Open Connection!", StatusMessageType.Error, false, ucHoloNETEntry);

                            LogMessage($"APP: Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...");
                            ShowStatusMessage($"Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true, ucHoloNETEntry);

                            await CloseConnection(isEntry);
                        }
                        else
                        {
                            LogMessage($"APP: HoloNETClient WebSocket Connection Opened On Port {_holoNETEntry.HoloNETClient.EndPoint.Port}.");
                            ShowStatusMessage($"HoloNETClient WebSocket Connection Opened On Port {_holoNETEntry.HoloNETClient.EndPoint.Port}.", StatusMessageType.Success, false, ucHoloNETEntry);

                            //Will wait until the HoloNET Entry has init (non blocking).
                            await _holoNETEntry.WaitTillHoloNETInitializedAsync();

                            //Refresh the list of installed hApps.
                            ProcessListedApps(await _holoNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All));
                        }
                    }
                    else
                    {
                        LogMessage($"APP: HoloNETClient WebSocket Connection Is Open On Port {_holoNETEntry.HoloNETClient.EndPoint.Port}.");
                        ShowStatusMessage($"HoloNETClient WebSocket Connection Is Open On Port {_holoNETEntry.HoloNETClient.EndPoint.Port}.", StatusMessageType.Success, false, ucHoloNETEntry);
                    }
                }
                else
                {
                    if (HoloNETEntries != null && HoloNETEntries.HoloNETClient != null && HoloNETEntries.HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                    {
                        LogMessage($"APP: Test App {appId} HoloNETClient WebSocket Connection Is Not Open!");
                        ShowStatusMessage($"Test App {appId} HoloNETClient WebSocket Connection Is Not Open!", StatusMessageType.Error, false, ucHoloNETCollectionEntryInternal);

                        LogMessage($"APP: Opening HoloNETClient WebSocket Connection On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}...");
                        ShowStatusMessage($"Opening HoloNETClient WebSocket Connection On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}...", StatusMessageType.Information, true, ucHoloNETCollectionEntryInternal);

                        await HoloNETEntries.HoloNETClient.ConnectAsync();

                        if (HoloNETEntries != null && HoloNETEntries.HoloNETClient != null && HoloNETEntries.HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                        {
                            LogMessage($"APP: Failed To Open Connection!");
                            ShowStatusMessage($"Failed To Open Connection!", StatusMessageType.Error, false, ucHoloNETEntry);

                            LogMessage($"APP: Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...");
                            ShowStatusMessage($"Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true, ucHoloNETCollectionEntryInternal);

                            await CloseConnection(isEntry);
                        }
                        else
                        {
                            LogMessage($"APP: HoloNETClient WebSocket Connection Opened On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}.");
                            ShowStatusMessage($"HoloNETClient WebSocket Connection Opened On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);

                            //Will wait until the HoloNET Collection has init (non blocking).
                            await HoloNETEntries.WaitTillHoloNETInitializedAsync();

                            //Refresh the list of installed hApps.
                            ProcessListedApps(await _holoNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All));
                        }
                    }
                    else
                    {
                        LogMessage($"APP: HoloNETClient WebSocket Connection Is Open On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}.");
                        ShowStatusMessage($"HoloNETClient WebSocket Connection Is Open On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                    }
                }
            }

            _showAppsListedInLog = true;
            //ucHoloNETEntry.HideMessage(); //Hide any messages that were shown.

            return showAlreadyInitMessage;
        }

        private async Task CloseConnection(bool isEntry)
        {
            if (isEntry)
            {
                await _holoNETEntry.CloseAsync(); //Will close the internal HoloNETClient connection.
                _holoNETEntry = null;
            }
            else
            {
                await HoloNETEntries.CloseAsync(); //Will close the internal HoloNETClient connection.
                HoloNETEntries = null;
            }
        }
    }
}