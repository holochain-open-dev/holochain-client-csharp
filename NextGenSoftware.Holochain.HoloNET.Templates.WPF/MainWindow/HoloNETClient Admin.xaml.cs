using System;
using System.Threading.Tasks;
using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// //NOTE: EVERY method on HoloNETClient can be called either async or non-async, in these examples we are using a mixture of async and non-async. Normally you would use async because it is less code and easier to follow but we wanted to test and demo both versions (and show how you would use non async as well as async versions)...
    /// </summary>
    public partial class MainWindow : Window
    {
        //private void InitHoloNETClientAdmin()
        //{
        //    HoloNETEntryDNAManager.LoadDNA();

        //    _holoNETClientAdmin = new HoloNETClient();
        //    _holoNETClientAdmin.HoloNETDNA.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded;
        //    //_holoNETClientAdmin.HoloNETDNA.HolochainConductorToUse = HolochainConductorEnum.HcDevTool;
        //    _holoNETClientAdmin.HoloNETDNA.HolochainConductorToUse = HolochainConductorEnum.HolochainProductionConductor;

        //    txtAdminURI.Text = _holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI;
        //    chkAutoStartConductor.IsChecked = _holoNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor;
        //    chkAutoShutdownConductor.IsChecked = _holoNETClientAdmin.HoloNETDNA.AutoShutdownHolochainConductor;
        //    chkShowConductorWindow.IsChecked = _holoNETClientAdmin.HoloNETDNA.ShowHolochainConductorWindow;
        //    txtSecondsToWaitForConductorToStart.Text = _holoNETClientAdmin.HoloNETDNA.SecondsToWaitForHolochainConductorToStart.ToString();
        //    //_holoNETClientAdmin.HoloNETDNA.FullPathToRootHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis";
        //    // //_holoNETClientAdmin.HoloNETDNA.FullPathToRootHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5";
        //    // // _holoNETClientAdmin.HoloNETDNA.FullPathToCompiledHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ";

        //    _holoNETClientAdmin.OnHolochainConductorStarting += _holoNETClientAdmin_OnHolochainConductorStarting;
        //    _holoNETClientAdmin.OnHolochainConductorStarted += _holoNETClientAdmin_OnHolochainConductorStarted;
        //    _holoNETClientAdmin.OnConnected += _holoNETClientAdmin_OnConnected;
        //    _holoNETClientAdmin.OnDataSent += _holoNETClientAdmin_OnDataSent;
        //    _holoNETClientAdmin.OnDataReceived += _holoNETClientAdmin_OnDataReceived;
        //    _holoNETClientAdmin.OnAdminAgentPubKeyGeneratedCallBack += _holoNETClientAdmin_OnAdminAgentPubKeyGeneratedCallBack;
        //    _holoNETClientAdmin.OnAdminAppInstalledCallBack += _holoNETClientAdmin_OnAdminAppInstalledCallBack;
        //    _holoNETClientAdmin.OnAdminAppUninstalledCallBack += _holoNETClientAdmin_OnAdminAppUninstalledCallBack;
        //    _holoNETClientAdmin.OnAdminAppEnabledCallBack += _holoNETClientAdmin_OnAdminAppEnabledCallBack;
        //    _holoNETClientAdmin.OnAdminAppDisabledCallBack += _holoNETClientAdmin_OnAdminAppDisabledCallBack;
        //    _holoNETClientAdmin.OnAdminZomeCallCapabilityGrantedCallBack += _holoNETClientAdmin_OnAdminZomeCallCapabilityGrantedCallBack;
        //    _holoNETClientAdmin.OnAdminAppInterfaceAttachedCallBack += _holoNETClientAdmin_OnAdminAppInterfaceAttachedCallBack;
        //    _holoNETClientAdmin.OnAdminAppsListedCallBack += _holoNETClientAdmin_OnAdminAppsListedCallBack;
        //    _holoNETClientAdmin.OnDisconnected += _holoNETClientAdmin_OnDisconnected;
        //    _holoNETClientAdmin.OnError += _holoNETClientAdmin_OnError;
        //}

        //private void ConnectAdmin()
        //{
        //    _clientsDisconnected = 0;
        //    _clientsToDisconnect = 0;
        //    _rebooting = false;
        //    _adminDisconnected = false;

        //    if (!_holoNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor)
        //    {
        //        LogMessage($"ADMIN: Connecting to Admin WebSocket on endpoint: {_holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI}...");
        //        ShowStatusMessage($"Admin WebSocket Connecting To Endpoint {_holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI}...", StatusMessageType.Information, true);
        //    }

        //    //_holoNETClientAdmin.ConnectAdmin(_holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI);

        //    //If you do not pass a connection string in it will default to HoloNETDNA.HolochainConductorAdminURI
        //    _holoNETClientAdmin.ConnectAdmin();
        //}

        ///// <summary>
        ///// Will init the demo hApp (used by HoloNET Entry and HoloNET Collection), which includes installing and enabling the app, signing credentials & attaching the app interface.
        ///// </summary>
        //private async Task<(bool, int, string, string)> InitDemoApp(string appId, string hAppInstallPath, ucHoloNETEntry ucHoloNETEntry)
        //{
        //    LogMessage($"ADMIN: Checking If App {appId} Is Already Installed...");
        //    ShowStatusMessage($"Checking If App {appId} Is Already Installed...", StatusMessageType.Information, true, ucHoloNETEntry);

        //    AdminGetAppInfoCallBackEventArgs appInfoResult = await _holoNETClientAdmin.AdminGetAppInfoAsync(appId);

        //    if (appInfoResult != null && appInfoResult.AppInfo != null)
        //    {
        //        ShowStatusMessage($"App {appId} Is Already Installed So Uninstalling Now...", StatusMessageType.Information, true, ucHoloNETEntry);
        //        LogMessage($"ADMIN: App {appId} Is Already Installed So Uninstalling Now...");

        //        AdminAppUninstalledCallBackEventArgs uninstallResult = await _holoNETClientAdmin.AdminUninstallAppAsync(appId);

        //        if (uninstallResult != null && uninstallResult.IsError)
        //        {
        //            LogMessage($"ADMIN: Error Uninstalling App {appId}. Reason: {uninstallResult.Message}");
        //            ShowStatusMessage($"Error Uninstalling App {appId}. Reason: {uninstallResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
        //        }
        //        else
        //        {
        //            LogMessage($"ADMIN: Uninstalled App {appId}.");
        //            ShowStatusMessage($"Uninstalled App {appId}.", StatusMessageType.Error, false, ucHoloNETEntry);
        //        }
        //    }
        //    else
        //    {
        //        LogMessage($"ADMIN: {appId} App Not Found.");
        //        ShowStatusMessage($"{appId} App Not Found.", StatusMessageType.Information, true, ucHoloNETEntry);
        //    }


        //    LogMessage($"ADMIN: Generating New AgentPubKey...");
        //    ShowStatusMessage($"Generating New AgentPubKey...", StatusMessageType.Information, true, ucHoloNETEntry);

        //    AdminAgentPubKeyGeneratedCallBackEventArgs agentPubKeyResult = await _holoNETClientAdmin.AdminGenerateAgentPubKeyAsync();

        //    if (agentPubKeyResult != null && !agentPubKeyResult.IsError)
        //    {
        //        LogMessage($"ADMIN: AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}");
        //        ShowStatusMessage($"AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}", StatusMessageType.Success, false, ucHoloNETEntry);

        //        LogMessage($"ADMIN: Installing App {appId}...");
        //        ShowStatusMessage($"Installing App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

        //        AdminAppInstalledCallBackEventArgs installedResult = await _holoNETClientAdmin.AdminInstallAppAsync(appId, hAppInstallPath);

        //        if (installedResult != null && !installedResult.IsError)
        //        {
        //            LogMessage($"ADMIN: {appId} App Installed.");
        //            ShowStatusMessage($"{appId} App Installed.", StatusMessageType.Success, false, ucHoloNETEntry);

        //            LogMessage($"ADMIN: Enabling App {appId}...");
        //            ShowStatusMessage($"Enabling App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

        //            AdminAppEnabledCallBackEventArgs enabledResult = await _holoNETClientAdmin.AdminEnableAppAsync(appId);

        //            if (enabledResult != null && !enabledResult.IsError)
        //            {
        //                LogMessage($"ADMIN: {appId} App Enabled.");
        //                ShowStatusMessage($"{appId} App Enabled.", StatusMessageType.Success, false);

        //                LogMessage($"ADMIN: Signing Credentials (Zome Call Capabilities) For App {appId}...");
        //                ShowStatusMessage($"Signing Credentials (Zome Call Capabilities) For App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

        //                AdminZomeCallCapabilityGrantedCallBackEventArgs signingResult = await _holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(installedResult.CellId, CapGrantAccessType.Unrestricted, GrantedFunctionsType.All);

        //                //Un-comment this line and comment the above one to grant only specefic zome functions.
        //                //_holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(installedResult.CellId, CapGrantAccessType.Assigned, GrantedFunctionsType.Listed, new List<(string, string)>()
        //                //{
        //                //    ("oasis", "create_avatar"),
        //                //    ("oasis", "get_avatar"),
        //                //    ("oasis", "update_avatar")
        //                //});

        //                if (signingResult != null && !signingResult.IsError)
        //                {
        //                    LogMessage($"ADMIN: {appId} App Signing Credentials Authorized.");
        //                    ShowStatusMessage($"{appId} App Signing Credentials Authorized.", StatusMessageType.Success, false, ucHoloNETEntry);

        //                    LogMessage($"ADMIN: Attaching App Interface For App {appId}...");
        //                    ShowStatusMessage($"Attaching App Interface For App {appId}...", StatusMessageType.Information, true, ucHoloNETEntry);

        //                    AdminAppInterfaceAttachedCallBackEventArgs attachedResult = await _holoNETClientAdmin.AdminAttachAppInterfaceAsync();

        //                    if (attachedResult != null && !attachedResult.IsError)
        //                    {
        //                        LogMessage($"ADMIN: {appId} App Interface Attached On Port {attachedResult.Port}.");
        //                        ShowStatusMessage($"{appId} App Interface Attached On Port {attachedResult.Port}.", StatusMessageType.Success, false, ucHoloNETEntry);
        //                        return (true, attachedResult.Port.Value, installedResult.DnaHash, installedResult.AgentPubKey);
        //                    }
        //                    else
        //                    {
        //                        LogMessage($"ADMIN: Error Attaching App Interface For App {appId}. Reason: {attachedResult.Message}");
        //                        ShowStatusMessage($"ADMIN: Error Attaching App Interface For App {appId}. Reason: {attachedResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
        //                    }
        //                }
        //                else
        //                {
        //                    LogMessage($"ADMIN: Error Signing Credentials For App {appId}. Reason: {signingResult.Message}");
        //                    ShowStatusMessage($"ADMIN: Error Signing Credentials For App {appId}. Reason: {signingResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
        //                }
        //            }
        //            else
        //            {
        //                LogMessage($"ADMIN: Error Enabling App {appId}. Reason: {enabledResult.Message}.");
        //                ShowStatusMessage($"ADMIN: Error Enabling App {appId}. Reason: {enabledResult.Message}.", StatusMessageType.Error, false, ucHoloNETEntry);
        //            }
        //        }
        //        else
        //        {
        //            LogMessage($"ADMIN: Error Installing App {appId}. Reason: {installedResult.Message}.");
        //            ShowStatusMessage($"ADMIN: Error Installing App {appId}. Reason: {installedResult.Message}.", StatusMessageType.Error, false, ucHoloNETEntry);
        //        }
        //    }
        //    else
        //    {
        //        LogMessage($"ADMIN: Error Generating AgentPubKey. Reason: {agentPubKeyResult.Message}");
        //        ShowStatusMessage($"Error Generating AgentPubKey. Reason: {agentPubKeyResult.Message}", StatusMessageType.Error, false, ucHoloNETEntry);
        //    }

        //    return (false, 0, "", "");
        //}

        //private void ListHapps()
        //{
        //    if (_showAppsListedInLog)
        //    {
        //        LogMessage("ADMIN: Listing hApps...");
        //        ShowStatusMessage($"Listing hApps...", StatusMessageType.Information, true);
        //    }

        //    _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
        //}

        //private void _holoNETClientAdmin_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        //{
        //    LogMessage("ADMIN: Connected");
        //    ShowStatusMessage($"Admin WebSocket Connected.", StatusMessageType.Success);

        //    //ListHapps();

        //    LogMessage("ADMIN: Generating AgentPubKey For hApp...");
        //    ShowStatusMessage($"Generating AgentPubKey For hApp...");

        //    _holoNETClientAdmin.AdminGenerateAgentPubKey();
        //}

        //private void _holoNETClientAdmin_OnHolochainConductorStarting(object sender, HolochainConductorStartingEventArgs e)
        //{
        //    LogMessage($"ADMIN: Starting Holochain Conductor...");
        //    ShowStatusMessage("Starting Holochain Conductor...", StatusMessageType.Information, true);
        //}

        //private void _holoNETClientAdmin_OnHolochainConductorStarted(object sender, HolochainConductorStartedEventArgs e)
        //{
        //    LogMessage($"ADMIN: Holochain Conductor Started.");
        //    ShowStatusMessage("Holochain Conductor Started.", StatusMessageType.Success);

        //    LogMessage($"ADMIN: Connecting to Admin WebSocket on endpoint: {txtAdminURI.Text}...");
        //    ShowStatusMessage($"Admin WebSocket Connecting To Endpoint {txtAdminURI.Text}...", StatusMessageType.Information, true);
        //}

        //private void _holoNETClientAdmin_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        //{
        //    if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
        //        LogMessage($"ADMIN: Data Sent to EndPoint {e.EndPoint}: {e.RawBinaryDataDecoded} ({e.RawBinaryDataAsString})");
        //}

        //private void _holoNETClientAdmin_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        //{
        //    if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
        //        LogMessage(string.Concat("ADMIN: Data Received for EndPoint: ", e.EndPoint, ": ", e.RawBinaryDataDecoded, "(", e.RawBinaryDataAsString, ")"));
        //}

        //private void _holoNETClientAdmin_OnAdminAgentPubKeyGeneratedCallBack(object sender, AdminAgentPubKeyGeneratedCallBackEventArgs e)
        //{
        //    if (!_initHoloNETEntryDemo)
        //    {
        //        LogMessage($"ADMIN: AgentPubKey Generated for EndPoint: {e.EndPoint} and Id: {e.Id}: AgentPubKey: {e.AgentPubKey}");
        //        ShowStatusMessage($"AgentPubKey Generated.", StatusMessageType.Success);

        //        LogMessage($"ADMIN: Listing hApps...");
        //        ShowStatusMessage($"Listing hApps...", StatusMessageType.Information, true);
        //        _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
        //    }
        //}

        //private void _holoNETClientAdmin_OnAdminAppsListedCallBack(object sender, AdminAppsListedCallBackEventArgs e)
        //{
        //    //if (_showAppsListedInLog)
        //    ProcessListedApps(e);
        //}

        //private void _holoNETClientAdmin_OnAdminAppInstalledCallBack(object sender, AdminAppInstalledCallBackEventArgs e)
        //{
        //    if (!_initHoloNETEntryDemo)
        //    {
        //        CellInfoType cellType = CellInfoType.None;

        //        if (e.IsCallSuccessful && !e.IsError)
        //        {
        //            if (e.AppInfoResponse != null && e.AppInfoResponse.data != null && e.AppInfoResponse.data.cell_info != null && e.AppInfoResponse.data.cell_info.ContainsKey(_role_name) && e.AppInfoResponse.data.cell_info[_role_name] != null && e.AppInfoResponse.data.cell_info[_role_name].Count > 0 && e.AppInfoResponse.data.cell_info[_role_name][0] != null)
        //                cellType = e.AppInfoResponse.data.cell_info[_role_name][0].CellInfoType;

        //            LogMessage($"ADMIN: hApp {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.installed_app_id : "")} Installed for EndPoint: {e.EndPoint} and Id: {e.Id}:, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, Manifest: {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.manifest.name : "")}, CellType: {Enum.GetName(typeof(CellInfoType), cellType)}");
        //            ShowStatusMessage($"hApp {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.installed_app_id : "")} Installed.", StatusMessageType.Success);
        //        }
        //        else
        //        {
        //            LogMessage($"ADMIN hApp NOT Installed! Reason: {e.Message}");
        //            ShowStatusMessage($"hApp NOT Installed! Reason: {e.Message}", StatusMessageType.Error);
        //        }

        //        if (cellType != CellInfoType.Provisioned)
        //            LogMessage("CellType is not Provisioned so aborting...");

        //        else if (e.AppInfoResponse != null && e.AppInfoResponse.data != null)
        //        {
        //            _installingAppCellId = e.CellId;
        //            LogMessage($"ADMIN: Enabling {e.AppInfoResponse.data.installed_app_id} hApp...");
        //            ShowStatusMessage($"Enabling {e.AppInfoResponse.data.installed_app_id} hApp...", StatusMessageType.Information, true);

        //            _holoNETClientAdmin.AdminEnablelApp(e.AppInfoResponse.data.installed_app_id);
        //        }
        //    }
        //}

        //private void _holoNETClientAdmin_OnAdminAppUninstalledCallBack(object sender, AdminAppUninstalledCallBackEventArgs e)
        //{
        //    if (!_initHoloNETEntryDemo)
        //    {
        //        LogMessage($"ADMIN: hApp {e.InstalledAppId} Uninstalled.");
        //        ShowStatusMessage($"hApp {e.InstalledAppId} Uninstalled.", StatusMessageType.Success);
        //        ListHapps();
        //    }
        //}

        //private void _holoNETClientAdmin_OnAdminAppEnabledCallBack(object sender, AdminAppEnabledCallBackEventArgs e)
        //{
        //    if (!_initHoloNETEntryDemo)
        //    {
        //        LogMessage($"ADMIN: hApp {e.InstalledAppId} Enabled.");
        //        ShowStatusMessage($"hApp {e.InstalledAppId} Enabled.", StatusMessageType.Success);

        //        ListHapps();
        //    }
        //}

        //private void _holoNETClientAdmin_OnAdminAppDisabledCallBack(object sender, AdminAppDisabledCallBackEventArgs e)
        //{
        //    LogMessage($"ADMIN: hApp {e.InstalledAppId} Disabled.");
        //    ShowStatusMessage($"hApp {e.InstalledAppId} Disabled.", StatusMessageType.Success);
        //    ListHapps();
        //}

        //private void _holoNETClientAdmin_OnAdminZomeCallCapabilityGrantedCallBack(object sender, AdminZomeCallCapabilityGrantedCallBackEventArgs e)
        //{
        //    if (!_initHoloNETEntryDemo)
        //    {
        //        _installingAppCellId = null;
        //        LogMessage($"ADMIN: Zome Call Capability Granted (Signing Credentials Authorized).");
        //        ShowStatusMessage($"Zome Call Capability Granted (Signing Credentials Authorized).", StatusMessageType.Success);

        //        LogMessage("ADMIN: Attaching App Interface...");
        //        ShowStatusMessage($"Attaching App Interface...", StatusMessageType.Information, true);

        //        //_holoNETClientAdmin.AdminAttachAppInterface(65002);
        //        _holoNETClientAdmin.AdminAttachAppInterface();
        //    }
        //}

        //private void _holoNETClientAdmin_OnAdminAppInterfaceAttachedCallBack(object sender, AdminAppInterfaceAttachedCallBackEventArgs e)
        //{
        //    if (!_initHoloNETEntryDemo)
        //    {
        //        LogMessage($"ADMIN: App Interface Attached On Port {e.Port}.");
        //        ShowStatusMessage("App Interface Attached.");

        //        //Look for the HoloNETClientAppAgent connection that matches the current hApp DnaHash and AgentPubKey (CellId).
        //        bool foundClient = false;
        //        foreach (HoloNETClient client in _holoNETappClients)
        //        {
        //            if (client != null && client.HoloNETDNA.DnaHash == CurrentApp.DnaHash && client.HoloNETDNA.AgentPubKey == CurrentApp.AgentPubKey && client.HoloNETDNA.InstalledAppId == CurrentApp.Name)
        //            {
        //                LogMessage($"APP: Found Exsting Existing HoloNETClient AppAgent WebSocket For AgentPubKey {client.HoloNETDNA.AgentPubKey}, DnaHash {client.HoloNETDNA.DnaHash} And InstalledAppId {client.HoloNETDNA.InstalledAppId} Running On Port {client.EndPoint.Port}.");

        //                if (client.State == System.Net.WebSockets.WebSocketState.Open)
        //                {
        //                    LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port} Is Open.");

        //                    if (client.EndPoint.Port == e.Port)
        //                    {
        //                        LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket Connected On Port {client.EndPoint.Port} So Matched Admin Attached Port.");
        //                        ProcessClientOperation(client);
        //                    }
        //                    else
        //                    {
        //                        LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket Connected On Port {client.EndPoint.Port} but ADMIN Attached To Port {e.Port} So Need To Re-Connect On The New Port...");
        //                        LogMessage($"APP: Disconnecting From HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port}...");

        //                        _appAgentClientPort = e.Port.Value;
        //                        client.Disconnect();
        //                        foundClient = false;
        //                    }
        //                }
        //                else
        //                {
        //                    LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port} Is NOT Open!");
        //                    ShowStatusMessage($"Re-Connecting To HoloNETClient AppAgent WebSocketOn Port {e.Port}...", StatusMessageType.Information, true);
        //                    LogMessage($"APP: Re-Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...");
        //                    client.Connect();
        //                }

        //                foundClient = true;
        //                break;
        //            }
        //        }

        //        //If it doesn't find one then attempt to recycle a stale connection now.
        //        if (!foundClient)
        //        {
        //            LogMessage($"APP: No Existing HoloNETClient AppAgent WebSocket Found Running For AgentPubKey {CurrentApp.AgentPubKey}, DnaHash {CurrentApp.DnaHash} And InstalledAppId {CurrentApp.Name} So Looking For Client To Recycle...");

        //            foreach (HoloNETClient client in _holoNETappClients)
        //            {
        //                if (client.State != System.Net.WebSockets.WebSocketState.Open)
        //                {
        //                    LogMessage($"APP: Found Stale HoloNETClient AppAgent WebSocket So Recycling...");

        //                    //if we find an old or stale client we can recycle it...
        //                    ShowStatusMessage($"Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...", StatusMessageType.Information, true);
        //                    LogMessage($"APP: Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...");
        //                    client.Connect();

        //                    foundClient = true;
        //                    break;
        //                }
        //            }

        //            if (!foundClient)
        //                LogMessage($"APP: No Stale HoloNETClient AppAgent WebSocket Found To Recycle.");
        //        }

        //        //If it still doesn't find one then create a new appAgent connection now...
        //        if (!foundClient)
        //        {
        //            LogMessage($"APP: No Existing HoloNETClient AppAgent WebSocket Found Running For AgentPubKey {CurrentApp.AgentPubKey}, DnaHash {CurrentApp.DnaHash} And InstalledAppId {CurrentApp.Name} So Creating New HoloNETClient AppAgent WebSocket Now...");

        //            HoloNETClient newClient = CreateNewAppAgentClientConnection(e.Port.Value);
        //            LogMessage($"APP: New HoloNETClient AppAgent WebSocket Created.");

        //            ShowStatusMessage($"Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...", StatusMessageType.Information, true);
        //            LogMessage($"APP: Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...");
        //            newClient.Connect();

        //            //Add the new connection the AppAgent Connection Pool.
        //            _holoNETappClients.Add(newClient);
        //        }
        //    }
        //}

        //private void _holoNETClientAdmin_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        //{
        //    LogMessage("ADMIN: Disconnected");
        //    ShowStatusMessage($"Admin WebSocket Disconnected.");
        //    _adminDisconnected = true;

        //    if (_rebooting && _adminDisconnected && _clientsDisconnected == _clientsToDisconnect)
        //    {
        //        if (_holoNETClientAdmin != null)
        //        {
        //            _holoNETClientAdmin.OnHolochainConductorStarting -= _holoNETClientAdmin_OnHolochainConductorStarting;
        //            _holoNETClientAdmin.OnHolochainConductorStarted -= _holoNETClientAdmin_OnHolochainConductorStarted;
        //            _holoNETClientAdmin.OnConnected -= _holoNETClientAdmin_OnConnected;
        //            _holoNETClientAdmin.OnDataSent -= _holoNETClientAdmin_OnDataSent;
        //            _holoNETClientAdmin.OnDataReceived -= _holoNETClientAdmin_OnDataReceived;
        //            _holoNETClientAdmin.OnAdminAgentPubKeyGeneratedCallBack -= _holoNETClientAdmin_OnAdminAgentPubKeyGeneratedCallBack;
        //            _holoNETClientAdmin.OnAdminAppsListedCallBack -= _holoNETClientAdmin_OnAdminAppsListedCallBack;
        //            _holoNETClientAdmin.OnAdminAppInstalledCallBack -= _holoNETClientAdmin_OnAdminAppInstalledCallBack;
        //            _holoNETClientAdmin.OnAdminAppUninstalledCallBack -= _holoNETClientAdmin_OnAdminAppUninstalledCallBack;
        //            _holoNETClientAdmin.OnAdminAppEnabledCallBack -= _holoNETClientAdmin_OnAdminAppEnabledCallBack;
        //            _holoNETClientAdmin.OnAdminAppDisabledCallBack -= _holoNETClientAdmin_OnAdminAppDisabledCallBack;
        //            _holoNETClientAdmin.OnAdminZomeCallCapabilityGrantedCallBack -= _holoNETClientAdmin_OnAdminZomeCallCapabilityGrantedCallBack;
        //            _holoNETClientAdmin.OnAdminAppInterfaceAttachedCallBack -= _holoNETClientAdmin_OnAdminAppInterfaceAttachedCallBack;
        //            _holoNETClientAdmin.OnDisconnected -= _holoNETClientAdmin_OnDisconnected;
        //            _holoNETClientAdmin.OnError -= _holoNETClientAdmin_OnError;
        //            _holoNETClientAdmin = null;
        //        }

        //        InitHoloNETClientAdmin();
        //        ConnectAdmin();
        //    }
        //}

        //private void _holoNETClientAdmin_OnError(object sender, HoloNETErrorEventArgs e)
        //{
        //    LogMessage($"ADMIN: Error occured. Reason: {e.Reason}");

        //    if (e.Reason.Contains("Error occurred in WebSocket.Connect method connecting to"))
        //        ShowStatusMessage($"Error occured connecting to {e.EndPoint}. Please make sure the Holochain Conductor Admin is running on that port and try again.", StatusMessageType.Error);
        //    else
        //        ShowStatusMessage($"Error occured On Admin WebSocket. Reason: {e.Reason}", StatusMessageType.Error);
        //}
    }
}