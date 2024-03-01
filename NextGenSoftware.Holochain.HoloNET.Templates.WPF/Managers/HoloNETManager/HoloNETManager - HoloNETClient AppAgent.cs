using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers
{
    public partial class HoloNETManager : IHoloNETManager
    {
        public IHoloNETClientAppAgent CreateNewAppAgentClientConnection(string installedAppId, string agentPubKey, ushort port)
        {
            //_test[CurrentApp.Name] = new HoloNETClientAppAgent(new HoloNETDNA()
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


            //_holoNETappClients.Add(new HoloNETClientAppAgent(new HoloNETDNA()
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

            // If you do not pass a HoloNETDNA in then it will use the one persisted to disk from the previous run if SaveDNA was called(if there is no saved DNA then it will use the defaults).
            IHoloNETClientAppAgent newClient = new HoloNETClientAppAgent(installedAppId, agentPubKey, new HoloNETDNA()
            {
                HolochainConductorAppAgentURI = $"ws://127.0.0.1:{port}"
            });

            newClient.OnConnected += _holoNETClientApp_OnConnected;
            newClient.OnDataSent += _holoNETClientApp_OnDataSent;
            newClient.OnDataReceived += _holoNETClientApp_OnDataReceived;
            newClient.OnReadyForZomeCalls += _holoNETClientApp_OnReadyForZomeCalls;
            newClient.OnZomeFunctionCallBack += _holoNETClientApp_OnZomeFunctionCallBack;
            newClient.OnDisconnected += _holoNETClientApp_OnDisconnected;
            newClient.OnError += _holoNETClientApp_OnError;

            // You can pass the HoloNETDNA in via the constructor as we have above or you can set it via the property below:
            newClient.HoloNETDNA.AutoStartHolochainConductor = false;
            newClient.HoloNETDNA.AutoShutdownHolochainConductor = false;

            // You need to set either the InstalledAppId or the AgentPubKey & DnaHash. You can set all 3 if you wish but only one or the other works fine too (if only the InstalledAppId is set it will look up the AgentPubKey & DnaHash from the conductor). 
            newClient.HoloNETDNA.AgentPubKey = CurrentApp.AgentPubKey; //If you only set the AgentPubKey & DnaHash but not the InstalledAppId then it will still work fine.
            newClient.HoloNETDNA.DnaHash = CurrentApp.DnaHash;
            newClient.HoloNETDNA.InstalledAppId = CurrentApp.Name;

            return newClient;
        }

        /// <summary>
        /// Attempt to find an existing appAgent HoloNET Client connection from the connection pool that matches the current hApp's DnaHash & AgentPubKey (CellId). If it doesn't find one then it will begin the process of creating a new one by getting the admin connection to authorize the hApp...
        /// </summary>
        /// <returns></returns>
        public ConnectToAppAgentClientResult ConnectToAppAgentClient()
        {
            ConnectToAppAgentClientResult result = new ConnectToAppAgentClientResult();

            if (CurrentApp != null)
            {
                result.AppAgentClient = GetClient(CurrentApp.DnaHash, CurrentApp.AgentPubKey, CurrentApp.Name);

                // If we find an existing client then that means it has already been authorized, attached and connected.
                if (result.AppAgentClient != null)
                {
                    LogMessage($"APP: Found Existing HoloNETClientAppAgent AppAgent WebSocket For AgentPubKey {result.AppAgentClient.HoloNETDNA.AgentPubKey}, DnaHash {result.AppAgentClient.HoloNETDNA.DnaHash} And InstalledAppId {result.AppAgentClient.HoloNETDNA.InstalledAppId} Running On Port {result.AppAgentClient.EndPoint.Port}.");

                    if (result.AppAgentClient.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        LogMessage($"APP: Existing HoloNETClientAppAgent AppAgent WebSocket On Port {result.AppAgentClient.EndPoint.Port} Is Open.");
                        result.ResponseType = ConnectToAppAgentClientResponseType.Connected;
                    }
                    else
                    {
                        LogMessage($"APP: Existing HoloNETClientAppAgent AppAgent WebSocket On Port {result.AppAgentClient.EndPoint.Port} Is NOT Open!");
                        ShowStatusMessage($"Re-Connecting To HoloNETClientAppAgent AppAgent WebSocket On Port {result.AppAgentClient.EndPoint.Port}...", StatusMessageType.Information, true);
                        LogMessage($"APP: Re-Connecting To HoloNETClientAppAgent AppAgent WebSocket On Port {result.AppAgentClient.EndPoint.Port}...");

                        result.AppAgentClient.Connect();
                        result.ResponseType = ConnectToAppAgentClientResponseType.Connecting;
                    }
                }
                else
                {
                    LogMessage($"ADMIN: No Existing HoloNETClientAppAgent AppAgent WebSocket Found For AgentPubKey {CurrentApp.AgentPubKey}, DnaHash {CurrentApp.DnaHash} And InstalledAppId {CurrentApp.Name} So New Connection Needs To Be Made...");
                    LogMessage($"ADMIN: Authorizing Signing Credentials For {CurrentApp.Name} hApp...");
                    ShowStatusMessage($"Authorizing Signing Credentials For {CurrentApp.Name} hApp...", StatusMessageType.Information, true);

                    //_holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(GrantedFunctionsType.Listed, new List<(string, string)>()
                    //{
                    //    ("oasis", "create_avatar"),
                    //    ("oasis", "get_avatar"),
                    //    ("oasis", "update_avatar")
                    //});

                    // NOTE: EVERY method on HoloNETClientAppAgent can be called either async or non-async, in these examples we are using non-async. Normally you would use async because it is less code and easier to follow but we wanted to test and demo both versions (and show how you would use non async as well as async versions)...
                    HoloNETClientAdmin.AuthorizeSigningCredentialsAndGrantZomeCallCapability(HoloNETClientAdmin.GetCellId(CurrentApp.DnaHash, CurrentApp.AgentPubKey), CapGrantAccessType.Unrestricted, GrantedFunctionsType.All, null);
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

        public IHoloNETClientAppAgent GetClient(string dnaHash, string agentPubKey, string installedAppId)
        {
            return HoloNETClientAppAgentClients.FirstOrDefault(x => x.HoloNETDNA.DnaHash == dnaHash && x.HoloNETDNA.AgentPubKey == agentPubKey && x.HoloNETDNA.InstalledAppId == installedAppId);
        }

        /// <summary>
        /// Process the client operation which was queued before the HoloNET AppAgent was connected. This is only needed in non-async code, async code is much simplier and less! ;-)
        /// </summary>
        /// <param name="client"></param>
        public async Task ProcessClientOperationAsync(IHoloNETClientAppAgent client)
        {
            switch (ClientOperation)
            {
                case ClientOperation.CallZomeFunction:
                    {
                        if (ZomeCallParams.ParamsObjectForZomeCall != null)
                        {
                            LogMessage("APP: Calling Zome Function...");
                            ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                            client.CallZomeFunction(ZomeCallParams.ZomeName, ZomeCallParams.ZomeFunction, ZomeCallParams.ParamsObjectForZomeCall);
                            ZomeCallParams.ParamsObjectForZomeCall = null;
                        }
                        else
                        {
                            LogMessage("ADMIN: Error: Zome paramsObject is null! Please try again");
                            ShowStatusMessage("Error: Zome paramsObject is null! Please try again", StatusMessageType.Error);
                        }
                        ClientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.LoadHoloNETEntryShared:
                    {
                        await LoadHoloNETEntrySharedAsync(client);
                        ClientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.SaveHoloNETEntryShared:
                    {
                        await SaveHoloNETEntrySharedAsync(client, HoloNETEntryUIManager.CurrentHoloNETEntryUI.FirstName, HoloNETEntryUIManager.CurrentHoloNETEntryUI.LastName, HoloNETEntryUIManager.CurrentHoloNETEntryUI.DOBDateTime, HoloNETEntryUIManager.CurrentHoloNETEntryUI.Email);
                        ClientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.LoadHoloNETCollectionShared:
                    {
                        await LoadCollectionAsync(client);
                        ClientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.AddHoloNETEntryToCollectionShared:
                    {
                        await AddHoloNETEntryToCollectionAsync(client, HoloNETEntryUIManager.CurrentHoloNETEntryUI.FirstName, HoloNETEntryUIManager.CurrentHoloNETEntryUI.LastName, HoloNETEntryUIManager.CurrentHoloNETEntryUI.DOBDateTime, HoloNETEntryUIManager.CurrentHoloNETEntryUI.Email);
                        ClientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.RemoveHoloNETEntryFromCollectionShared:
                    {
                        await RemoveHoloNETEntryFromCollectionAsync(client, CurrentAvatar);
                        ClientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.UpdateHoloNETEntryInCollectionShared:
                    {
                        await UpdateHoloNETEntryInCollectionAsync(client);
                        ClientOperation = ClientOperation.None;
                    }
                    break;
            }
        }

        public void UpdateNumerOfClientConnections()
        {
            //First count the number of shared connections in the connection pool.
            NumberOfClientConnections = 0;

            foreach (IHoloNETClientAppAgent client in HoloNETClientAppAgentClients)
            {
                if (client.State == System.Net.WebSockets.WebSocketState.Open)
                    NumberOfClientConnections++;
            }

            //If the HoloNETEntry that is using its own internal connection is connected then count this too.
            if (HoloNETEntry != null && HoloNETEntry.HoloNETClient != null && HoloNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                NumberOfClientConnections++;

            //If the HoloNET Collection that is using its own internal connection is connected then count this too.
            if (HoloNETEntries != null && HoloNETEntries.HoloNETClient != null && HoloNETEntries.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                NumberOfClientConnections++;

            OnNumberOfClientConnectionsChanged?.Invoke(this, new NumberOfClientConnectionsEventArgs() { NumberOfConnections = NumberOfClientConnections });

            //txtConnections.Text = $"Client Connections: {noConnections}";
            //sbAnimateConnections.Begin();
        }

        public void SetCurrentAppToConnectedStatus(int port)
        {
            CurrentApp = InstalledApps.FirstOrDefault(x => x.Name == CurrentApp.Name && x.DnaHash == CurrentApp.DnaHash && x.AgentPubKey == CurrentApp.AgentPubKey);
            SetAppToConnectedStatus(CurrentApp, port, true);
        }

        public void SetAppToConnectedStatus(IInstalledApp app, int port, bool isSharedConnection, bool refreshGrid = true)
        {
            if (app != null)
            {
                app.IsSharedConnection = isSharedConnection;
                app.IsConnected = true;
                app.Status = "Running (Connected)";
                app.StatusReason = "AppAgent Client Connected";
                app.Port = port.ToString();

                //TODO: Because InstalledApps is an observableCollection we shouldn't need to raise an event like this but for some unknown reason the grid bound to it does not refresh!
                if (refreshGrid)
                    OnInstalledAppsChanged?.Invoke(this, new InstalledAppsEventArgs { InstalledApps = InstalledApps.OrderBy(x => x.Name).ToList() });
            }
        }

        public void SetAppToConnectedStatus(string appName, int port, bool isSharedConnection, bool refreshGrid = true)
        {
            SetAppToConnectedStatus(InstalledApps.FirstOrDefault(x => x.Name == appName), port, isSharedConnection, refreshGrid);
        }

        public void ProcessListedApps(AppsListedCallBackEventArgs listedAppsResult)
        {
            string hApps = "";
            InstalledApps.Clear();

            foreach (AppInfo app in listedAppsResult.Apps.OrderBy(x => x.installed_app_id))
            {
                IInstalledApp installedApp = new InstalledApp()
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
                    IHoloNETClientAppAgent client = GetClient(installedApp.DnaHash, installedApp.AgentPubKey, installedApp.Name);

                    if (client != null && client.State == System.Net.WebSockets.WebSocketState.Open)
                        SetAppToConnectedStatus(installedApp, client.EndPoint.Port, true, false);
                }

                InstalledApps.Add(installedApp);

                //if (hApps != "")
                //    hApps = $"{hApps}";

                hApps = $"{hApps}{app.installed_app_id}\n";
            }

            //Set the status to connected in the list of installed apps (this is good example of how you can access the internal HoloNETClientAppAgent inside the HoloNET Entry).
            //This HoloNETEntry uses its own internal HoloNETClientAppAgent connection so it will not be shared with CallZomeFunction or View Data Entries popups (it is technically possible to share it if you wanted to but it is not recommended because it is controlled by the HoloNETEntry and when it is closed it will automatically close the connection too).
            if (HoloNETEntry != null && HoloNETEntry.HoloNETClient != null && HoloNETEntry.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                SetAppToConnectedStatus(_holoNETEntryDemoAppId, HoloNETEntry.HoloNETClient.EndPoint.Port, false);

            if (HoloNETEntries != null && HoloNETEntries.HoloNETClient != null && HoloNETEntries.HoloNETClient.State == System.Net.WebSockets.WebSocketState.Open)
                SetAppToConnectedStatus(_holoNETCollectionDemoAppId, HoloNETEntries.HoloNETClient.EndPoint.Port, false);

            //TODO: Because InstalledApps is an observableCollection we shouldn't need to raise an event like this but for some unknown reason the grid bound to it does not refresh!
            OnInstalledAppsChanged?.Invoke(this, new InstalledAppsEventArgs { InstalledApps = InstalledApps.OrderBy(x => x.Name).ToList() });

            if (ShowAppsListedInLog)
            {
                LogMessage($"ADMIN: hApps Listed: \n{hApps}");
                ShowStatusMessage("hApps Listed.", StatusMessageType.Success);
            }
        }

        public static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = string.Concat("EndPoint: ", args.EndPoint, ", Id: ", args.Id, ", Zome: ", args.Zome, ", ZomeFunction: ", args.ZomeFunction, ", ZomeReturnData: ", args.ZomeReturnData, ", ZomeReturnHash: ", args.ZomeReturnHash, ", Raw Zome Return Data: ", args.RawZomeReturnData, ", Raw Binary Data: ", args.RawBinaryDataDecoded, "(", args.RawBinaryDataAsString, "), IsError: ", args.IsError ? "true" : "false", ", Message: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, ", Processed Zome Return Data: ", args.KeyValuePairAsString);

            return result;
        }

        public string GetEntryInfo(ZomeFunctionCallBackEventArgs e)
        {
            if (e != null && e.Entries.Count > 0)
                return $"DateTime: {e.Entries[0].DateTime}, Author: {e.Entries[0].Author}, ActionSequence: {e.Entries[0].ActionSequence}, Signature: {e.Entries[0].Signature}, Type: {e.Entries[0].Type}, Hash: {e.Entries[0].Hash}, Previous Hash: {e.Entries[0].PreviousHash}, OriginalActionAddress: {e.Entries[0].OriginalActionAddress}, OriginalEntryAddress: {e.Entries[0].OriginalEntryAddress}";
            else
                return "";
        }

        public async Task CloseConnection(bool isEntry)
        {
            if (isEntry)
            {
                await HoloNETEntry.CloseAsync(); //Will close the internal HoloNETClientAppAgent connection.
                HoloNETEntry = null;
            }
            else
            {
                await HoloNETEntries.CloseAsync(); //Will close the internal HoloNETClientAppAgent connection.
                HoloNETEntries = null;
            }
        }

        public async Task<bool> DisconnectAsync(IInstalledApp app)
        {
            if (app != null)
            {
                //If it is the HoloNETEntry (uses the oasis-app) then get the internal connection from the HoloNET Entry.
                if (app.Name == _holoNETEntryDemoAppId && HoloNETEntry != null && HoloNETEntry.HoloNETClient != null)
                {
                    await HoloNETEntry.HoloNETClient.DisconnectAsync();
                    UpdateNumerOfClientConnections();
                    
                    //The async version will wait till it has disconnected so we can refresh the hApp list now without needing to use the OnDisconencted callback.
                    ListHapps();
                }

                //If it is the HoloNET Collection (uses the oasis-app) then get the internal connection from the HoloNET Collection.
                if (app.Name == _holoNETCollectionDemoAppId && HoloNETEntries != null && HoloNETEntries.HoloNETClient != null)
                {
                    //We can either use async or non-async version.
                    await HoloNETEntries.HoloNETClient.DisconnectAsync();
                    UpdateNumerOfClientConnections();

                    //The async version will wait till it has disconnected so we can refresh the hApp list now without needing to use the OnDisconencted callback.
                    ListHapps();
                }

                //Get the client from the client pool (shared connections).
                IHoloNETClientAppAgent client = GetClient(app.DnaHash, app.AgentPubKey, app.Name);

                if (client != null && client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    _removeClientConnectionFromPoolAfterDisconnect = false;
                    client.Disconnect(); //Non async version (will use OnDisconnected callback to refresh the hApp list (will call ListHapps).
                    return true;
                }
            }

            return false;
        }

        private void _holoNETClientApp_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            LogMessage($"APP: AppAgent Client WebSocket Connected To {e.EndPoint.AbsoluteUri}");
            ShowStatusMessage($"AppAgent Client WebSocket Connected To {e.EndPoint.AbsoluteUri}", StatusMessageType.Success);
            SetCurrentAppToConnectedStatus(e.EndPoint.Port);

            UpdateNumerOfClientConnections();
            ListHapps();

            HoloNETClientAppAgent client = sender as HoloNETClientAppAgent;

            if (client != null)
                Dispatcher.CurrentDispatcher.InvokeAsync(async () => await ProcessClientOperationAsync(client));
        }


        private void _holoNETClientApp_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        {
            if (ShowDetailedLogMessages)
                LogMessage($"APP: Data Sent to EndPoint {e.EndPoint}: {e.RawBinaryDataDecoded} ({e.RawBinaryDataAsString})");
        }

        private void _holoNETClientApp_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (ShowDetailedLogMessages)
                LogMessage(string.Concat("APP: Data Received for EndPoint: ", e.EndPoint, ": ", e.RawBinaryDataDecoded, "(", e.RawBinaryDataAsString, ")"));
        }

        private void _holoNETClientApp_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            LogMessage(string.Concat("APP: Zome CallBack: ", ProcessZomeFunctionCallBackEventArgs(e)));
            ShowStatusMessage("Zome Callback.", StatusMessageType.Success);
        }

        private void _holoNETClientApp_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            LogMessage("APP: Ready For Zome Calls.");
            ShowStatusMessage("Ready For Zome Calls.", StatusMessageType.Success);

            IHoloNETClientAppAgent client = GetClient(CurrentApp.DnaHash, CurrentApp.AgentPubKey, CurrentApp.Name);

            //For the async version we wouldn't need to do any of this so would be a lot less code and be much simplier! ;-)
            if (client != null)
            {
                if (ZomeCallParams.ParamsObjectForZomeCall != null)
                {
                    LogMessage("APP: Calling Zome Function...");
                    ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                    client.CallZomeFunction(ZomeCallParams.ZomeName, ZomeCallParams.ZomeFunction, ZomeCallParams.ParamsObjectForZomeCall);
                    ZomeCallParams.ParamsObjectForZomeCall = null;

                    //popupMakeZomeCall.Visibility = Visibility.Collapsed;
                    PopupManager.CurrentPopup.Visibility = Visibility.Collapsed;
                    PopupManager.CurrentPopup = null;
                }
            }
        }

        private void _holoNETClientApp_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            LogMessage("APP: WebSocket Disconnected");
            ShowStatusMessage("App WebSocket Disconnected.");

            IHoloNETClientAppAgent client = sender as HoloNETClientAppAgent;

            if (client != null)
            {
                if (ClientOperation == ClientOperation.None && _removeClientConnectionFromPoolAfterDisconnect)
                {
                    client.OnConnected -= _holoNETClientApp_OnConnected;
                    client.OnReadyForZomeCalls -= _holoNETClientApp_OnReadyForZomeCalls;
                    client.OnZomeFunctionCallBack -= _holoNETClientApp_OnZomeFunctionCallBack;
                    client.OnDisconnected -= _holoNETClientApp_OnDisconnected;
                    client.OnDataReceived -= _holoNETClientApp_OnDataReceived;
                    client.OnDataSent -= _holoNETClientApp_OnDataSent;
                    client.OnError -= _holoNETClientApp_OnError;

                    HoloNETClientAppAgentClients.Remove(client);
                }
                else if (_appAgentClientPort > 0)
                {
                    //If there was a pending client request (such as calling a zome call or init a holonet entry) then re-connect on the correct port now...
                    LogMessage($"APP: Re-Connecting To AppAgent Client WebSocket: ws://127.0.0.1:{_appAgentClientPort}");
                    ShowStatusMessage($"Re-Connecting To AppAgent Client WebSocket: ws://127.0.0.1:{_appAgentClientPort}", StatusMessageType.Information, true);
                    client.Connect($"ws://127.0.0.1:{_appAgentClientPort}");
                    _appAgentClientPort = 0;
                }

                UpdateNumerOfClientConnections();

                if (!_rebooting)
                    ListHapps();
            }

            _clientsDisconnected++;

            if (_clientsDisconnected == _clientsToDisconnect && _rebooting && _adminDisconnected)
                Dispatcher.CurrentDispatcher.InvokeAsync(async () => { await ConnectAdminAsync(); });
        }

        private void _holoNETClientApp_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"Error occured On App WebSocket. Reason: {e.Reason}.", StatusMessageType.Error);
            LogMessage($"APP: Error occured. Reason: {e.Reason}");
        }
    }
}