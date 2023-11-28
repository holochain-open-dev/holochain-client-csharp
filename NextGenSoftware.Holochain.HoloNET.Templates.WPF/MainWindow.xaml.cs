using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Utilities.ExtentionMethods;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _hcAdminURI = "ws://localhost:52542";
        private const string _hcAppURI = "ws://localhost:888888";
        //private const string _oasisHappPath = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _oasisHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _role_name = "oasis";
        private const string _installed_app_id = "oasis-app";
        private HoloNETClient? _holoNETClientAdmin;
        private List<HoloNETClient> _holoNETappClients = new List<HoloNETClient>();
        private bool _rebooting = false;
        private bool _adminDisconnected = false;
        private bool _appDisconnected = false;
        private string _installinghAppName = "";
        private string _installinghAppPath = "";
        private byte[][] _installingAppCellId = null;
        dynamic paramsObject = null;
        private int _clientsToDisconnect = 0;
        private int _clientsDisconnected = 0;
        private Avatar _holoNETEntry;
        private AvatarShared _holoNETEntryShared;
        private ClientOperation _clientOperation;
        private ushort _appAgentClientPort = 0;
        private bool _removeClientConnectionFromPoolAfterDisconnect = true;
        private bool _initHoloNETEntryDemo = false; 

        public ObservableCollection<InstalledApp> InstalledApps { get; set; } = new ObservableCollection<InstalledApp>();
        //public ObservableCollection<AvatarMultiple> HoloNETEntries { get; set; } = new ObservableCollection<AvatarMultiple>();
        //public AvatarCollection HoloNETEntries { get; set; } = new AvatarCollection();
        public HoloNETCollection<AvatarShared> HoloNETEntries { get; set; }

        public InstalledApp CurrentApp { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (HoloNETEntries != null)
            {
                HoloNETEntries.OnInitialized -= HoloNETEntries_OnInitialized;
                HoloNETEntries.OnCollectionLoaded -= HoloNETEntries_OnCollectionLoaded;
                HoloNETEntries.OnHoloNETEntriesUpdated -= HoloNETEntries_OnHoloNETEntriesUpdated;
                HoloNETEntries.OnHoloNETEntryAddedToCollection -= HoloNETEntries_OnHoloNETEntryAddedToCollection;
                HoloNETEntries.OnHoloNETEntryRemovedFromCollection -= HoloNETEntries_OnHoloNETEntryRemovedFromCollection;
                HoloNETEntries.OnClosed -= HoloNETEntries_OnClosed;
                HoloNETEntries.OnError -= HoloNETEntries_OnError;
            }

            if (_holoNETEntry != null)
            {
                _holoNETEntry.OnInitialized -= _holoNETEntry_OnInitialized;
                _holoNETEntry.OnLoaded -= _holoNETEntry_OnLoaded;
                _holoNETEntry.OnSaved -= _holoNETEntry_OnSaved;
                _holoNETEntry.OnDeleted -= _holoNETEntry_OnDeleted;
                _holoNETEntry.OnClosed -= _holoNETEntry_OnClosed;
                _holoNETEntry.OnError -= _holoNETEntry_OnError;
            }

            CloseAllConnections();
        }

        private void Init()
        {
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
            _holoNETClientAdmin.OnConnected += HoloNETClient_OnConnected;
            _holoNETClientAdmin.OnError += HoloNETClient_OnError;
            _holoNETClientAdmin.OnDataSent += _holoNETClient_OnDataSent;
            _holoNETClientAdmin.OnDataReceived += HoloNETClient_OnDataReceived;
            _holoNETClientAdmin.OnDisconnected += HoloNETClient_OnDisconnected;
            _holoNETClientAdmin.OnAdminAgentPubKeyGeneratedCallBack += _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack;
            _holoNETClientAdmin.OnAdminAppInstalledCallBack += HoloNETClient_OnAdminAppInstalledCallBack;
            _holoNETClientAdmin.OnAdminAppUninstalledCallBack += _holoNETClientAdmin_OnAdminAppUninstalledCallBack;
            _holoNETClientAdmin.OnAdminAppEnabledCallBack += _holoNETClient_OnAdminAppEnabledCallBack;
            _holoNETClientAdmin.OnAdminAppDisabledCallBack += _holoNETClientAdmin_OnAdminAppDisabledCallBack;
            _holoNETClientAdmin.OnAdminZomeCallCapabilityGrantedCallBack += _holoNETClient_OnAdminZomeCallCapabilityGrantedCallBack;
            _holoNETClientAdmin.OnAdminAppInterfaceAttachedCallBack += _holoNETClient_OnAdminAppInterfaceAttachedCallBack;
            _holoNETClientAdmin.OnAdminAppsListedCallBack += _holoNETClientAdmin_OnAdminAppsListedCallBack;
        }

        private HoloNETClient CreateNewAppAgentClientConnection(ushort port)
        {
            HoloNETClient newClient = new HoloNETClient($"ws://127.0.0.1:{port}");
            newClient.OnConnected += _holoNETClientApp_OnConnected;
            newClient.OnReadyForZomeCalls += _holoNETClientApp_OnReadyForZomeCalls;
            newClient.OnZomeFunctionCallBack += _holoNETClientApp_OnZomeFunctionCallBack;
            newClient.OnDisconnected += _holoNETClientApp_OnDisconnected;
            newClient.OnDataReceived += _holoNETClientApp_OnDataReceived;
            newClient.OnDataSent += _holoNETClientApp_OnDataSent;
            newClient.OnError += _holoNETClientApp_OnError;

            newClient.HoloNETDNA.AutoStartHolochainConductor = false;
            newClient.HoloNETDNA.AutoShutdownHolochainConductor = false;

            newClient.HoloNETDNA.AgentPubKey = CurrentApp.AgentPubKey;
            newClient.HoloNETDNA.DnaHash = CurrentApp.DnaHash;
            newClient.HoloNETDNA.InstalledAppId = CurrentApp.Name;

            return newClient;
        }

        /// <summary>
        /// Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
        /// </summary>
        private void InitHoloNETEntry()
        {
            Dispatcher.InvokeAsync(async () =>
            {
                _initHoloNETEntryDemo = true;

                //TODO: Implement this tomorrow! ;-)
                //_holoNETClientAdmin.AdminGetAppInfoAsync();

                AdminAppsListedCallBackEventArgs listAppsResult = await _holoNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All);

                if (listAppsResult != null && !listAppsResult.IsError) 
                {
                    foreach (AppInfo app in listAppsResult.Apps)
                    {
                        if (app.installed_app_id == _installed_app_id)
                        {
                            AdminAppUninstalledCallBackEventArgs uninstallResult = await _holoNETClientAdmin.AdminUninstallAppAsync(_installed_app_id);
                            
                            if (uninstallResult != null && uninstallResult.IsError)
                            {
                                LogMessage($"ADMIM: Error uninstalling app {_installed_app_id}.");
                                ShowStatusMessage($"ADMIM: Error uninstalling app {_installed_app_id}.", StatusMessageType.Error);
                            }

                            break;
                        }
                    }
                }

                AdminAppInstalledCallBackEventArgs installedResult = await _holoNETClientAdmin.AdminInstallAppAsync(_installed_app_id, _oasisHappPath);

                if (installedResult != null && !installedResult.IsError)
                {
                    AdminAppEnabledCallBackEventArgs enabledResult = await _holoNETClientAdmin.AdminEnableAppAsync(_installed_app_id);

                    if (enabledResult != null && !enabledResult.IsError)
                    {
                        AdminZomeCallCapabilityGrantedCallBackEventArgs signingResult = await _holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(installedResult.CellId, CapGrantAccessType.Unrestricted, GrantedFunctionsType.All);

                        //_holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(GrantedFunctionsType.Listed, new List<(string, string)>()
                        //{
                        //    ("oasis", "create_avatar"),
                        //    ("oasis", "get_avatar"),
                        //    ("oasis", "update_avatar")
                        //});

                        if (signingResult != null && !signingResult.IsError)
                        {
                            AdminAppInterfaceAttachedCallBackEventArgs attachedResult = await _holoNETClientAdmin.AdminAttachAppInterfaceAsync();

                            if (attachedResult != null && !attachedResult.IsError)
                            {
                                //If we do not pass in a HoloNETClient it will create it's own internal connection/client
                                _holoNETEntry = new Avatar(new HoloNETDNA()
                                {
                                    HolochainConductorAppAgentURI = $"ws:\\localhost:{attachedResult.Port}",
                                    HolochainConductorToUse = HolochainConductorEnum.HcDevTool,
                                    ShowHolochainConductorWindow = true,
                                    FullPathToRootHappFolder = "E:\\hc\\holochain-holochain-0.1.5\\happs\\oasis",
                                    FullPathToCompiledHappFolder = "E:\\hc\\holochain-holochain-0.1.5\\happs\\oasis\\workdir"
                                    //FullPathToRootHappFolder = "C:\\Users\\USER\\holochain-holochain-0.1.5\\happs\\oasis",
                                    //FullPathToCompiledHappFolder = "C:\\Users\\USER\\holochain-holochain-0.1.5\\happs\\oasis\\workdir"
                                });

                                ////If we do not pass in a HoloNETClient it will create it's own internal connection/client
                                //_holoNETEntry = new Avatar(new HoloNETDNA()
                                //{
                                //    HolochainConductorToUse = HolochainConductorEnum.HcDevTool,
                                //    ShowHolochainConductorWindow = true,
                                //    FullPathToRootHappFolder = "E:\\hc\\holochain-holochain-0.1.5\\happs\\oasis",
                                //    FullPathToCompiledHappFolder = "E:\\hc\\holochain-holochain-0.1.5\\happs\\oasis\\workdir"
                                //    //FullPathToRootHappFolder = "C:\\Users\\USER\\holochain-holochain-0.1.5\\happs\\oasis",
                                //    //FullPathToCompiledHappFolder = "C:\\Users\\USER\\holochain-holochain-0.1.5\\happs\\oasis\\workdir"
                                //});

                                //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                                _holoNETEntry.OnInitialized += _holoNETEntry_OnInitialized;
                                _holoNETEntry.OnLoaded += _holoNETEntry_OnLoaded;
                                _holoNETEntry.OnClosed += _holoNETEntry_OnClosed;
                                _holoNETEntry.OnSaved += _holoNETEntry_OnSaved;
                                _holoNETEntry.OnDeleted += _holoNETEntry_OnDeleted;
                                _holoNETEntry.OnError += _holoNETEntry_OnError;

                                _initHoloNETEntryDemo = false;
                                SaveHoloNETEntry();
                            }
                        }
                    }
                }
            });
        }

        private void InitHoloNETEntry(HoloNETClient client)
        {
            LogMessage("APP: Initializing HoloNET Entry (Shared)...");
            ShowStatusMessage("Initializing HoloNET Entry (Shared)...", StatusMessageType.Information, true);

            if (_holoNETEntry == null)
            {
                _holoNETEntryShared = new AvatarShared(client);

                //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                _holoNETEntryShared.OnInitialized += _holoNETEntryShared_OnInitialized;
                _holoNETEntryShared.OnLoaded += _holoNETEntryShared_OnLoaded;
                _holoNETEntryShared.OnClosed += _holoNETEntryShared_OnClosed;
                _holoNETEntryShared.OnSaved += _holoNETEntryShared_OnSaved;
                _holoNETEntryShared.OnDeleted += _holoNETEntryShared_OnDeleted;
                _holoNETEntryShared.OnError += _holoNETEntryShared_OnError;
            }
            else
                _holoNETEntryShared.HoloNETClient = client;
        }

        private void InitHoloNETCollection(HoloNETClient client)
        {
            LogMessage("APP: Initializing HoloNET Collection...");
            ShowStatusMessage("Initializing HoloNET Collection...", StatusMessageType.Information, true);

            if (HoloNETEntries == null)
            {
                HoloNETEntries = new HoloNETCollection<AvatarShared>("oasis", "load_avatars", "add_avatar", "remove_avatar", client, "update_avatars");
                HoloNETEntries.OnInitialized += HoloNETEntries_OnInitialized;
                HoloNETEntries.OnCollectionLoaded += HoloNETEntries_OnCollectionLoaded;
                HoloNETEntries.OnHoloNETEntriesUpdated += HoloNETEntries_OnHoloNETEntriesUpdated;
                HoloNETEntries.OnHoloNETEntryAddedToCollection += HoloNETEntries_OnHoloNETEntryAddedToCollection;
                HoloNETEntries.OnHoloNETEntryRemovedFromCollection += HoloNETEntries_OnHoloNETEntryRemovedFromCollection;
                HoloNETEntries.OnClosed += HoloNETEntries_OnClosed;
                HoloNETEntries.OnError += HoloNETEntries_OnError;
            }
            else
                HoloNETEntries.HoloNETClient = client;
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
            LogMessage("ADMIN: Listning hApps...");
            ShowStatusMessage($"Listning hApps...", StatusMessageType.Information, true);
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
                        if (paramsObject != null)
                        {
                            LogMessage("APP: Calling Zome Function...");
                            ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                            client.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, paramsObject);
                            paramsObject = null;
                            //popupMakeZomeCall.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            LogMessage("ADMIN: Error: Zome paramsObject is null! Please try again");
                            ShowStatusMessage("Error: Zome paramsObject is null! Please try again", StatusMessageType.Error);
                        }
                        _clientOperation = ClientOperation.None;
                    }
                    break;

                case ClientOperation.SaveHoloNETEntry:
                    {
                        SaveHoloNETEntryShared(client, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);
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
                        AddHoloNETEntryToCollection(client, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);
                        _clientOperation = ClientOperation.None;
                    }
                    break;
            }
        }

        private void LoadCollection(HoloNETClient client)
        {
            if (HoloNETEntries == null || (HoloNETEntries != null && !HoloNETEntries.IsInitialized))
                InitHoloNETCollection(client);

            else if (HoloNETEntries.HoloNETClient.EndPoint != client.EndPoint)
                HoloNETEntries.HoloNETClient = client;

            if (HoloNETEntries != null && HoloNETEntries.IsInitialized)
            {
                //ShowStatusMessage($"APP: Loading HoloNET Collection...", StatusMessageType.Information, true);
                //LogMessage($"APP: Loading HoloNET Collection...");

                //Non-async way (you need to use the OnCollectionLoaded event to know when the collection has loaded and receive the data/collection.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //HoloNETEntries.LoadCollection(); 

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    ShowStatusMessage($"APP: Loading HoloNET Collection...", StatusMessageType.Information, true);
                    LogMessage($"APP: Loading HoloNET Collection...");

                    //LoadCollectionAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await HoloNETEntries.LoadCollectionAsync(); //No event handlers are needed but you can still use if you like.
                    HandleHoloNETCollectionLoaded(result);
                });
            }
        }

        private void AddHoloNETEntryToCollection(HoloNETClient client, string firstName, string lastName, string dob, string email)
        {
            if (HoloNETEntries == null || (HoloNETEntries != null && !HoloNETEntries.IsInitialized))
                InitHoloNETCollection(client);

            else if (HoloNETEntries.HoloNETClient.EndPoint != client.EndPoint)
                HoloNETEntries.HoloNETClient = client;

            string[] parts = dob.Split('/');

            if (HoloNETEntries != null && HoloNETEntries.IsInitialized)
            {
                //ShowStatusMessage($"APP: Adding HoloNET Entry To Collection...", StatusMessageType.Information, true);
                //LogMessage($"APP: Adding HoloNET Entry To Collection...");

                //Non-async way (you need to use the OnCollectionLoaded event to know when the collection has loaded and receive the data/collection.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(new AvatarMultiple()
                //{
                //    FirstName = firstName,
                //    LastName = lastName,
                //    DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                //    Email = email
                //});

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    ShowStatusMessage($"APP: Adding HoloNET Entry To Collection...", StatusMessageType.Information, true);
                    LogMessage($"APP: Adding HoloNET Entry To Collection...");

                    //Will add the entry to the collection and then persist the change to the hc/rust/happ code.
                    //We don't need to call Save() on the entry before calling this method because this method will automatically save the entry and then add it to the collection. It can also of course add an existing entry to the collection. The same applies to the SaveCollection method below.
                    ZomeFunctionCallBackEventArgs result = await HoloNETEntries.AddHoloNETEntryToCollectionAndSaveAsync(new AvatarShared()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                        Email = email
                    });

                    HandleHoloNETEntryAddedToCollection(result);

                    //Allows you to batch add/remove multiple entries to the collection and then persist the changes to the hc/rust/happ code in one go.
                    //Will only add the entry to the collection in memory (it will NOT persist to hc/rust/happ code until SaveCollection is called.
                    //HoloNETEntries.Add(new AvatarMultiple()
                    //{
                    //    FirstName = firstName,
                    //    LastName = lastName,
                    //    DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
                    //    Email = email
                    //});

                    //Will look for any changes since the last time this method was called (includes entries added/removed from the collection as well as any changes made to entries themselves). This can invoke multiple events including OnHoloNETEntryAddedToCollection, OnHoloNETEntryRemovedFromCollection & OnHoloNETEntriesUpdated (if any changes were made to the entries themselves))/
                    //HoloNETEntries.SaveCollection(); 
                });
            }
        }

        private void SaveHoloNETEntryShared(HoloNETClient client, string firstName, string lastName, string dob, string email)
        {
            //If we intend to re-use an object then we can store it globally so we only need to init once...
            if (_holoNETEntryShared == null || (_holoNETEntryShared != null && !_holoNETEntryShared.IsInitialized))
                InitHoloNETEntry(client);

            else if (_holoNETEntry.HoloNETClient.EndPoint != client.EndPoint)
                _holoNETEntryShared.HoloNETClient = client;

            if (_holoNETEntryShared != null && _holoNETEntryShared.IsInitialized)
            {
                //ShowStatusMessage($"APP: Saving HoloNET Data Entry...", StatusMessageType.Information, true);
                //LogMessage($"APP: Saving HoloNET Data Entry...");

                string[] parts = dob.Split('/');

                _holoNETEntryShared.FirstName = firstName;
                _holoNETEntryShared.LastName = lastName;
                _holoNETEntryShared.DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                _holoNETEntryShared.Email = email;

                //Non async way.
                //If you use Load or Save non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //_holoNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    ShowStatusMessage($"APP: Saving HoloNET Data Entry (Shared)...", StatusMessageType.Information, true);
                    LogMessage($"APP: Saving HoloNET Data Entry (Shared)...");

                    //SaveAsync (as well as LoadAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await _holoNETEntryShared.SaveAsync(); //No event handlers are needed.
                    HandleHoloNETEntrySharedSaved(result);
                });
            }
        }

        private void HandleHoloNETEntrySaved(ZomeFunctionCallBackEventArgs result)
        {
            if (result.IsCallSuccessful && !result.IsError)
            {
                ShowStatusMessage($"APP: HoloNET Entry Saved.", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Entry Saved.");
                popupHoloNETEntry.Visibility = Visibility.Hidden;
            }
            else
            {
                lblHoloNETEntryValidationErrors.Text = result.Message;
                ShowStatusMessage($"APP: Error occured saving entry: {result.Message}", StatusMessageType.Error);
                LogMessage($"APP: Error occured saving entry: {result.Message}");
            }
        }

        private void HandleHoloNETEntrySharedSaved(ZomeFunctionCallBackEventArgs result)
        {
            //TEMP to test!
            //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(_holoNETEntry);

            if (result.IsCallSuccessful && !result.IsError)
            {
                ShowStatusMessage($"APP: HoloNET Entry (Shared) Saved.", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Entry (Shared) Saved.");
                //popupHoloNETEntry.Visibility = Visibility.Hidden;

                //Will add the entry to the collection and then persist the change to the hc/rust/happ code.
                //TODO: Dont think we need to call Save() on the entry before calling this method because this method will automatically save the entry and then add it to the collection. It can also of course add an existing entry to the collection. The same applies to the SaveCollection method below.
                //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(result.Entries[0].EntryDataObject); 

                //Allows you to batch add/remove multiple entries to the collection and then persist the changes to the hc/rust/happ code in one go.
                //HoloNETEntries.Add(_holoNETEntry); //Will only add the entry to the collection in memory (it will NOT persist to hc/rust/happ code until SaveCollection is called.
                //HoloNETEntries.SaveCollection(); //Will look for any changes since the last time this method was called (includes entries added/removed from the collection as well as any changes made to entries themselves). This can invoke multiple events including OnHoloNETEntryAddedToCollection, OnHoloNETEntryRemovedFromCollection & OnHoloNETEntriesUpdated (if any changes were made to the entries themselves))/
            }
            else
            {
                lblHoloNETEntryValidationErrors.Text = result.Message;
                ShowStatusMessage($"APP: Error occured saving entry (Shared): {result.Message}", StatusMessageType.Error);
                LogMessage($"APP: Error occured saving entry (Shared): {result.Message}");
            }
        }

        private void HandleHoloNETCollectionLoaded(ZomeFunctionCallBackEventArgs result)
        {
            if (result.IsCallSuccessful && !result.IsError)
            {
                ShowStatusMessage($"APP: HoloNET Collection Loaded.", StatusMessageType.Error);
                LogMessage($"APP: HoloNET Collection Loaded.");
                gridDataEntries.ItemsSource = result.Entries;
            }
            else
            {
                lblNewEntryValidationErrors.Text = result.Message;
                ShowStatusMessage($"APP: Error Occured Loading HoloNET Collection.", StatusMessageType.Error);
                LogMessage($"APP: Error Occured Loading HoloNET Collection. Reason: {result.Message}");
            }
        }

        private void HandleHoloNETEntryAddedToCollection(ZomeFunctionCallBackEventArgs result)
        {
            if (result.IsCallSuccessful && !result.IsError)
            {
                ShowStatusMessage($"APP: HoloNET Entry Added To Collection.", StatusMessageType.Error);
                LogMessage($"APP: HoloNET Entry Added To Collection.");
            }
            else
            {
                lblNewEntryValidationErrors.Text = result.Message;
                ShowStatusMessage($"APP: Error Occured Adding HoloNET Entry To Collection.", StatusMessageType.Error);
                LogMessage($"APP: Error Occured Adding HoloNET Entry To Collection. Reason: {result.Message}");
            }
        }

        private string GetEntryInfo(ZomeFunctionCallBackEventArgs e)
        {
            return $"DateTime: {e.Entries[0].DateTime}, Author: {e.Entries[0].Author}, ActionSequence: {e.Entries[0].ActionSequence}, Signature: {e.Entries[0].Signature}, Type: {e.Entries[0].Type}, Hash: {e.Entries[0].Hash}, Previous Hash: {e.Entries[0].PreviousHash}, OriginalActionAddress: {e.Entries[0].OriginalActionAddress}, OriginalEntryAddress: {e.Entries[0].OriginalEntryAddress}";
        }

        private void ShowStatusMessage(string message, StatusMessageType type = StatusMessageType.Information, bool showSpinner = false)
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
            txtConnections.Text = $"Client Connections: {_holoNETappClients.Count}";
            sbAnimateConnections.Begin();
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";

            //result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\n\nRaw Binary Data: ", args.RawBinaryDataDecoded, "(", args.RawBinaryDataAsString, ")\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            //if (args.Entry != null && args.Entry.EntryDataObject != null)
            //{
            //    AvatarEntryDataObject avatar = args.Entry.EntryDataObject as AvatarEntryDataObject;

            //    if (avatar != null)
            //        result = BuildEntryDataObjectMessage(avatar, "Entry.EntryDataObject", result);
            //}

            //if (_avatarEntryDataObject != null)
            //    result = BuildEntryDataObjectMessage(_avatarEntryDataObject, "Global.EntryDataObject", result);

            return result;
        }

        private void SetCurrentAppToConnectedStatus(int port)
        {
            CurrentApp = InstalledApps.FirstOrDefault(x => x.Name == CurrentApp.Name && x.DnaHash == CurrentApp.DnaHash && x.AgentPubKey == CurrentApp.AgentPubKey);
            SetAppToConnectedStatus(CurrentApp, port);
        }

        private void SetAppToConnectedStatus(InstalledApp app, int port, bool refreshGrid = true)
        {
            if (app != null)
            {
                app.IsConnected = true;
                app.Status = "Running (Connected)";
                app.StatusReason = "AppAgent Client Connected";
                app.Port = port.ToString();

                if (refreshGrid)
                    gridHapps.ItemsSource = InstalledApps.OrderBy(x => x.Name);
            }
        }

        private void _holoNETClientAdmin_OnHolochainConductorStarting(object sender, HolochainConductorStartingEventArgs e)
        {
            LogMessage($"ADMIN: Starting Holochain Conductor...");
            ShowStatusMessage("Starting Holochain Conductor...", StatusMessageType.Information, true);
        }

        private void _holoNETClientAdmin_OnHolochainConductorStarted(object sender, HolochainConductorStartedEventArgs e)
        {
            LogMessage($"ADMIN: Holochain Conductor Started.");
            ShowStatusMessage("Holochain Conductor Started.", StatusMessageType.Success);

            LogMessage($"ADMIN: Connecting to Admin WebSocket on endpoint: {txtAdminURI.Text}...");
            ShowStatusMessage($"Admin WebSocket Connecting To Endpoint {txtAdminURI.Text}...", StatusMessageType.Information, true);
        }

        private void _holoNETClientAdmin_OnAdminAppDisabledCallBack(object sender, AdminAppDisabledCallBackEventArgs e)
        {
            LogMessage($"ADMIN: hApp {e.InstalledAppId} Disabled.");
            ShowStatusMessage($"hApp {e.InstalledAppId} Disabled.", StatusMessageType.Success);
            ListHapps();
        }

        private void _holoNETClientAdmin_OnAdminAppUninstalledCallBack(object sender, AdminAppUninstalledCallBackEventArgs e)
        {
            LogMessage($"ADMIN: hApp {e.InstalledAppId} Uninstalled.");
            ShowStatusMessage($"hApp {e.InstalledAppId} Uninstalled.", StatusMessageType.Success);
            ListHapps();
        }

        private void _holoNETClientAdmin_OnAdminAppsListedCallBack(object sender, AdminAppsListedCallBackEventArgs e)
        {
            string hApps = "";
            InstalledApps.Clear();

            foreach (AppInfo app in e.Apps)
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
                    IsDisabled = app.AppStatus == AppInfoStatusEnum.Disabled ? true : false
                };

                if (app.AppStatus == AppInfoStatusEnum.Running)
                {
                    HoloNETClient client = GetClient(installedApp.DnaHash, installedApp.AgentPubKey, installedApp.Name);

                    if (client != null && client.State == System.Net.WebSockets.WebSocketState.Open)
                        SetAppToConnectedStatus(installedApp, client.EndPoint.Port, false);
                }

                InstalledApps.Add(installedApp);

                if (hApps != "")
                    hApps = $"{hApps}, ";

                hApps = $"{hApps}{app.installed_app_id}";
            }

            gridHapps.ItemsSource = InstalledApps.OrderBy(x => x.Name);
            LogMessage($"ADMIN: hApps Listed: {hApps}");
            ShowStatusMessage("hApps Listed.", StatusMessageType.Success);        }

        private void _holoNETClient_OnAdminAppInterfaceAttachedCallBack(object sender, AdminAppInterfaceAttachedCallBackEventArgs e)
        {
            if (!_initHoloNETEntryDemo)
            {
                LogMessage($"ADMIN: App Interface Attached On Port {e.Port}.");
                ShowStatusMessage("App Interface Attached.");

                bool foundClient = false;
                foreach (HoloNETClient client in _holoNETappClients)
                {
                    if (client != null && client.HoloNETDNA.DnaHash == CurrentApp.DnaHash && client.HoloNETDNA.AgentPubKey == CurrentApp.AgentPubKey && client.HoloNETDNA.InstalledAppId == CurrentApp.Name)
                    {
                        LogMessage($"APP: Found Exsting Existing HoloNETClient AppAgent WebSocket For AgentPubKey {client.HoloNETDNA.AgentPubKey}, DnaHash {client.HoloNETDNA.DnaHash} And InstalledAppId {client.HoloNETDNA.InstalledAppId} Running On Port {client.EndPoint.Port}.");

                        if (client.State == System.Net.WebSockets.WebSocketState.Open)
                        {
                            LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port} Is Open.");

                            if (client.EndPoint.Port == e.Port)
                            {
                                LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket Connected On Port {client.EndPoint.Port} So Matched Admin Attached Port.");
                                ProcessClientOperation(client);
                            }
                            else
                            {
                                LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket Connected On Port {client.EndPoint.Port} but ADMIN Attached To Port {e.Port} So Need To Re-Connect On The New Port...");
                                LogMessage($"APP: Disconnecting From HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port}...");

                                _appAgentClientPort = e.Port.Value;
                                client.Disconnect();
                                foundClient = false;
                            }
                        }
                        else
                        {
                            LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port} Is NOT Open!");
                            ShowStatusMessage($"Re-Connecting To HoloNETClient AppAgent WebSocketOn Port {e.Port}...", StatusMessageType.Information, true);
                            LogMessage($"APP: Re-Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...");
                            client.Connect();
                        }

                        foundClient = true;
                        break;
                    }
                }

                if (!foundClient)
                {
                    LogMessage($"APP: No Existing HoloNETClient AppAgent WebSocket Found Running For AgentPubKey {CurrentApp.AgentPubKey}, DnaHash {CurrentApp.DnaHash} And InstalledAppId {CurrentApp.Name} So Looking For Client To Recycle...");

                    foreach (HoloNETClient client in _holoNETappClients)
                    {
                        if (client.State != System.Net.WebSockets.WebSocketState.Open)
                        {
                            LogMessage($"APP: Found Stale HoloNETClient AppAgent WebSocket So Recycling...");

                            //if we find an old or stale client we can recycle it...
                            ShowStatusMessage($"Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...", StatusMessageType.Information, true);
                            LogMessage($"APP: Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...");
                            client.Connect();

                            foundClient = true;
                            break;
                        }
                    }

                    if (!foundClient)
                        LogMessage($"APP: No Stale HoloNETClient AppAgent WebSocket Found To Recycle.");
                }

                if (!foundClient)
                {
                    LogMessage($"APP: No Existing HoloNETClient AppAgent WebSocket Found Running For AgentPubKey {CurrentApp.AgentPubKey}, DnaHash {CurrentApp.DnaHash} And InstalledAppId {CurrentApp.Name} So Creating New HoloNETClient AppAgent WebSocket Now...");

                    HoloNETClient newClient = CreateNewAppAgentClientConnection(e.Port.Value);
                    LogMessage($"APP: New HoloNETClient AppAgent WebSocket Created.");

                    ShowStatusMessage($"Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...", StatusMessageType.Information, true);
                    LogMessage($"APP: Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...");
                    newClient.Connect();

                    _holoNETappClients.Add(newClient);
                    UpdateNumerOfClientConnections();
                }
            }
        }

        private void _holoNETClientApp_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"Error occured On App WebSocket. Reason: {e.Reason}.", StatusMessageType.Error);
            LogMessage($"APP: Error occured. Reason: {e.Reason}");
        }

        private void _holoNETClientApp_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        {
            if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
                LogMessage($"APP: Data Sent to EndPoint {e.EndPoint}: {e.RawBinaryDataDecoded} ({e.RawBinaryDataAsString})");
        }

        private void _holoNETClientApp_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
                LogMessage(string.Concat("APP: Data Received for EndPoint: ", e.EndPoint, ": ", e.RawBinaryDataDecoded, "(", e.RawBinaryDataAsString, ")"));
        }

        private void _holoNETClientApp_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            LogMessage("APP: Disconnected");
            ShowStatusMessage("App WebSocket Disconnected.");

            HoloNETClient client = sender as HoloNETClient;

            if (client != null)
            {
                if (_clientOperation == ClientOperation.None && _removeClientConnectionFromPoolAfterDisconnect)
                {
                    client.OnConnected -= _holoNETClientApp_OnConnected;
                    client.OnReadyForZomeCalls -= _holoNETClientApp_OnReadyForZomeCalls;
                    client.OnZomeFunctionCallBack -= _holoNETClientApp_OnZomeFunctionCallBack;
                    client.OnDisconnected -= _holoNETClientApp_OnDisconnected;
                    client.OnDataReceived -= _holoNETClientApp_OnDataReceived;
                    client.OnDataSent -= _holoNETClientApp_OnDataSent;
                    client.OnError -= _holoNETClientApp_OnError;

                    _holoNETappClients.Remove(client);
                    UpdateNumerOfClientConnections();
                }

                else if (_appAgentClientPort > 0)
                {
                    //If there was a pending client request (such as calling a zome call or init a holonet entry) then re-connect on the correct port now...
                    LogMessage($"APP: Re-Connecting To AppAgent Client WebSocket: ws://127.0.0.1:{_appAgentClientPort}");
                    ShowStatusMessage($"Re-Connecting To AppAgent Client WebSocket: ws://127.0.0.1:{_appAgentClientPort}", StatusMessageType.Information, true);
                    client.Connect($"ws://127.0.0.1:{_appAgentClientPort}");
                    _appAgentClientPort = 0;
                }

                if (!_rebooting)
                    ListHapps();
            }

            _clientsDisconnected++;

            if (_clientsDisconnected == _clientsToDisconnect && _rebooting && _adminDisconnected)
                ConnectAdmin();
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

            HoloNETClient client = GetClient(CurrentApp.DnaHash, CurrentApp.AgentPubKey, CurrentApp.Name);

            if (client != null)
            {
                if (paramsObject != null)
                {
                    LogMessage("APP: Calling Zome Function...");
                    ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                    client.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, paramsObject);
                    paramsObject = null;
                    popupMakeZomeCall.Visibility = Visibility.Collapsed;
                }
            }    
        }

        private HoloNETClient GetClient(string dnaHash, string agentPubKey, string installedAppId)
        {
            return _holoNETappClients.FirstOrDefault(x => x.HoloNETDNA.DnaHash == dnaHash && x.HoloNETDNA.AgentPubKey == agentPubKey && x.HoloNETDNA.InstalledAppId == installedAppId);
        }

        private void _holoNETClientApp_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            LogMessage($"APP: AppAgent Client WebSocket Connected To {e.EndPoint.AbsoluteUri}");
            ShowStatusMessage($"APP: AppAgent Client WebSocket Connected To {e.EndPoint.AbsoluteUri}", StatusMessageType.Success);
            SetCurrentAppToConnectedStatus(e.EndPoint.Port);

            HoloNETClient client = sender as HoloNETClient;
            
            if (client != null)
                ProcessClientOperation(client);
        }

        private void _holoNETClient_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        {
            if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
                LogMessage($"ADMIN: Data Sent to EndPoint {e.EndPoint}: {e.RawBinaryDataDecoded} ({e.RawBinaryDataAsString})");
        }

        private void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
                LogMessage(string.Concat("ADMIN: Data Received for EndPoint: ", e.EndPoint, ": ", e.RawBinaryDataDecoded, "(", e.RawBinaryDataAsString, ")"));
        }

        private void _holoNETClient_OnAdminZomeCallCapabilityGrantedCallBack(object sender, AdminZomeCallCapabilityGrantedCallBackEventArgs e)
        {
            if (!_initHoloNETEntryDemo)
            {
                _installingAppCellId = null;
                LogMessage($"ADMIN: Zome Call Capability Granted (Signing Credentials Authorized).");
                ShowStatusMessage($"Zome Call Capability Granted (Signing Credentials Authorized).", StatusMessageType.Success);

                LogMessage("ADMIN: Attaching App Interface...");
                ShowStatusMessage($"Attaching App Interface...", StatusMessageType.Information, true);

                //_holoNETClientAdmin.AdminAttachAppInterface(65002);
                _holoNETClientAdmin.AdminAttachAppInterface();
            }
        }

        private void _holoNETClient_OnAdminAppEnabledCallBack(object sender, AdminAppEnabledCallBackEventArgs e)
        {
            if (!_initHoloNETEntryDemo)
            {
                LogMessage($"ADMIN: hApp {e.InstalledAppId} Enabled.");
                ShowStatusMessage($"hApp {e.InstalledAppId} Enabled.", StatusMessageType.Success);

                ListHapps();
            }
        }

        private void _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack(object sender, AdminAgentPubKeyGeneratedCallBackEventArgs e)
        {
            LogMessage($"ADMIN: AgentPubKey Generated for EndPoint: {e.EndPoint} and Id: {e.Id}: AgentPubKey: {e.AgentPubKey}");
            ShowStatusMessage($"AgentPubKey Generated.", StatusMessageType.Success);

           // _adminConnected = true;

            LogMessage($"ADMIN: Listing hApps...");
            ShowStatusMessage($"Listing hApps...", StatusMessageType.Information, true);
            _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
        }

        private void HoloNETClient_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            LogMessage("ADMIN: Disconnected");
            ShowStatusMessage($"Admin WebSocket Disconnected.");

            _adminDisconnected = true;
           // _adminConnected = false;

            if (_rebooting && _adminDisconnected && _clientsDisconnected == _clientsToDisconnect)
            {
                if (_holoNETClientAdmin != null)
                {
                    _holoNETClientAdmin.OnHolochainConductorStarting -= _holoNETClientAdmin_OnHolochainConductorStarting;
                    _holoNETClientAdmin.OnHolochainConductorStarted -= _holoNETClientAdmin_OnHolochainConductorStarted;
                    _holoNETClientAdmin.OnConnected -= HoloNETClient_OnConnected;
                    _holoNETClientAdmin.OnError -= HoloNETClient_OnError;
                    _holoNETClientAdmin.OnDataSent -= _holoNETClient_OnDataSent;
                    _holoNETClientAdmin.OnDataReceived -= HoloNETClient_OnDataReceived;
                    _holoNETClientAdmin.OnDisconnected -= HoloNETClient_OnDisconnected;
                    _holoNETClientAdmin.OnAdminAgentPubKeyGeneratedCallBack -= _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack;
                    _holoNETClientAdmin.OnAdminAppInstalledCallBack -= HoloNETClient_OnAdminAppInstalledCallBack;
                    _holoNETClientAdmin.OnAdminAppUninstalledCallBack -= _holoNETClientAdmin_OnAdminAppUninstalledCallBack;
                    _holoNETClientAdmin.OnAdminAppEnabledCallBack -= _holoNETClient_OnAdminAppEnabledCallBack;
                    _holoNETClientAdmin.OnAdminAppDisabledCallBack -= _holoNETClientAdmin_OnAdminAppDisabledCallBack;
                    _holoNETClientAdmin.OnAdminZomeCallCapabilityGrantedCallBack -= _holoNETClient_OnAdminZomeCallCapabilityGrantedCallBack;
                    _holoNETClientAdmin.OnAdminAppInterfaceAttachedCallBack -= _holoNETClient_OnAdminAppInterfaceAttachedCallBack;
                    _holoNETClientAdmin.OnAdminAppsListedCallBack -= _holoNETClientAdmin_OnAdminAppsListedCallBack;
                    _holoNETClientAdmin = null;
                }

                Init();
                ConnectAdmin();
            }
        }

        private void HoloNETClient_OnAdminAppInstalledCallBack(object sender, AdminAppInstalledCallBackEventArgs e)
        {
            if (!_initHoloNETEntryDemo)
            {
                CellInfoType cellType = CellInfoType.None;

                if (e.IsCallSuccessful && !e.IsError)
                {
                    if (e.AppInfoResponse != null && e.AppInfoResponse.data != null && e.AppInfoResponse.data.cell_info != null && e.AppInfoResponse.data.cell_info.ContainsKey(_role_name) && e.AppInfoResponse.data.cell_info[_role_name] != null && e.AppInfoResponse.data.cell_info[_role_name].Count > 0 && e.AppInfoResponse.data.cell_info[_role_name][0] != null)
                        cellType = e.AppInfoResponse.data.cell_info[_role_name][0].CellInfoType;

                    LogMessage($"ADMIN: hApp {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.installed_app_id : "")} Installed for EndPoint: {e.EndPoint} and Id: {e.Id}:, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, Manifest: {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.manifest.name : "")}, CellType: {Enum.GetName(typeof(CellInfoType), cellType)}");
                    ShowStatusMessage($"hApp {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.installed_app_id : "")} Installed.", StatusMessageType.Success);
                }
                else
                    LogMessage($"ADMIN hApp NOT Installed! Reason: {e.Message}");
                ShowStatusMessage($"hApp NOT Installed! Reason: {e.Message}", StatusMessageType.Error);

                if (cellType != CellInfoType.Provisioned)
                    LogMessage("CellType is not Provisioned so aborting...");

                else if (e.AppInfoResponse != null && e.AppInfoResponse.data != null)
                {
                    _installingAppCellId = e.CellId;
                    LogMessage($"ADMIN: Enabling {e.AppInfoResponse.data.installed_app_id} hApp...");
                    ShowStatusMessage($"Enabling {e.AppInfoResponse.data.installed_app_id} hApp...", StatusMessageType.Information, true);

                    _holoNETClientAdmin.AdminEnablelApp(e.AppInfoResponse.data.installed_app_id);
                }
            }
        }

        private void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            LogMessage($"ADMIN: Error occured. Reason: {e.Reason}");

            if (e.Reason.Contains("Error occurred in WebSocket.Connect method connecting to"))
                ShowStatusMessage($"Error occured connecting to {e.EndPoint}. Please make sure the Holochain Conductor Admin is running on that port and try again.", StatusMessageType.Error);
            else
                ShowStatusMessage($"Error occured On Admin WebSocket. Reason: {e.Reason}" , StatusMessageType.Error);
        }


        private void HoloNETClient_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            LogMessage("ADMIN: Connected");
            ShowStatusMessage($"Admin WebSocket Connected.", StatusMessageType.Success);

            LogMessage("ADMIN: Generating AgentPubKey For hApp...");
            ShowStatusMessage($"Generating AgentPubKey For hApp...");

            _holoNETClientAdmin.AdminGenerateAgentPubKey();
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            lstOutput.Items.Clear();
        }

        private void btnReboot_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Rebooting...");
            ShowStatusMessage($"Rebooting...");

            _clientsToDisconnect = _holoNETappClients.Count;
            _rebooting = true;
            CloseAllConnections();
        }

        private void btnShowLog_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("notepad.exe", "Logs\\HoloNET.log");
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "hApp files (*.hApp)|*.hApp";

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName)) 
                {
                    _installinghAppPath = openFileDialog.FileName;
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    string[] parts = fileInfo.Name.Split('.');
                    _installinghAppName = parts[0];
                    InputTextBox.Text = _installinghAppName;
                    popupInstallhApp.Visibility = Visibility.Visible;
                }
            }
        }

        private void btnRefreshInstalledhApps_Click(object sender, RoutedEventArgs e)
        {
            ListHapps();
        }

        private void btnListDNAs_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listning DNAs...");
            ShowStatusMessage($"Listning DNAs...");
            _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
        }

        private void btnListCellIds_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listning Cell Ids...");
            ShowStatusMessage($"Listning Cell Ids...", StatusMessageType.Information, true);
            _holoNETClientAdmin.AdminListCellIds();
        }

        private void btnListAttachedInterfaces_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listning Attached Interfaces...");
            ShowStatusMessage($"Listning Attached Interfaces...", StatusMessageType.Information, true);
            _holoNETClientAdmin.AdminListInterfaces();
        }

        private void btnEnableApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Enabling {app.Name} hApp...");
                        ShowStatusMessage($"Enabling {app.Name} hApp...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminEnablelApp(app.Name);
                    }
                }
            }
        }

        private void btnDisableApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Disabling {app.Name} hApp...");
                        ShowStatusMessage($"Disabling {app.Name} hApp...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminDisableApp(app.Name);
                    }
                }
            }
        }

        private void btnUninstallApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Uninstalling {app.Name} hApp...");
                        ShowStatusMessage($"Uninstalling {app.Name} hApp...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminUninstallApp(app.Name);
                    }
                }
            }
        }

        private void btnCallZomeFunction_Click(object sender, RoutedEventArgs e)
        {
            popupMakeZomeCall.Visibility = Visibility.Visible;
        }

        private void btnCallZomeFunctionPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            popupMakeZomeCall.Visibility = Visibility.Collapsed;
            lblCallZomeFunctionErrors.Visibility = Visibility.Collapsed;
        }

        private void btnCallZomeFunctionPopupOk_Click(object sender, RoutedEventArgs e)
        {
            paramsObject = new ExpandoObject();

            if (txtZomeName.Text == "")
            {
                lblCallZomeFunctionErrors.Text = "Please enter the zome name.";
                lblCallZomeFunctionErrors.Visibility = Visibility.Visible;
                txtZomeName.Focus();
            }
            
            else if (txtZomeFunction.Text == "")
            {
                lblCallZomeFunctionErrors.Text = "Please enter the zome function name.";
                lblCallZomeFunctionErrors.Visibility = Visibility.Visible;
                txtZomeFunction.Focus();
            }

            else if (txtZomeParams.Text == "")
            {
                lblCallZomeFunctionErrors.Text = "Please enter the zome params.";
                lblCallZomeFunctionErrors.Visibility = Visibility.Visible;
                txtZomeParams.Focus();
            }
            else
            {
                lblCallZomeFunctionErrors.Visibility = Visibility.Collapsed;

                try
                {
                    string[] parts = txtZomeParams.Text.Split(';');

                    foreach (string param in parts)
                    {
                        string[] paramParts = param.Split('=');
                        ExpandoObjectHelpers.AddProperty(paramsObject, paramParts[0], paramParts[1]);
                    }

                    ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

                    if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    {
                        LogMessage("APP: Calling Zome Function...");
                        ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                        result.AppAgentClient.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, paramsObject);
                        paramsObject = null;
                    }
                    else
                        _clientOperation = ClientOperation.CallZomeFunction;

                    popupMakeZomeCall.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex) 
                {
                    lblCallZomeFunctionErrors.Text = "The zome params are incorrect, they need to be in the format 'param1=1;param2=2;param3=3'";
                    lblCallZomeFunctionErrors.Visibility = Visibility.Visible;
                    txtZomeParams.Focus();
                }
            }
        }

        private void btnAdminURIPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            popupAdminURI.Visibility = Visibility.Collapsed;
            Application.Current.Shutdown();
        }

        private void btnAdminURIPopupOK_Click(object sender, RoutedEventArgs e)
        {
            txtSecondsToWaitForConductorToStartError.Visibility = Visibility.Hidden;

            if (string.IsNullOrEmpty(txtAdminURI.Text))
                lblAdminURIError.Visibility = Visibility.Visible;

            //else if (!Uri.IsWellFormedUriString(txtAdminURI.Text, UriKind.Absolute))
            //{
            //    //lblAdminURIError.Visibility = Visibility.Visible;
            //}

            else
            {
                try
                {
                    int number = Convert.ToInt32(txtSecondsToWaitForConductorToStart.Text);

                    lblAdminURIError.Visibility = Visibility.Collapsed;
                    popupAdminURI.Visibility = Visibility.Collapsed;

                    _holoNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI = txtAdminURI.Text;
                    _holoNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor = chkAutoStartConductor.IsChecked.Value;
                    _holoNETClientAdmin.HoloNETDNA.AutoShutdownHolochainConductor = chkAutoShutdownConductor.IsChecked.Value;
                    _holoNETClientAdmin.HoloNETDNA.ShowHolochainConductorWindow = chkShowConductorWindow.IsChecked.Value;
                    _holoNETClientAdmin.HoloNETDNA.SecondsToWaitForHolochainConductorToStart = Convert.ToInt32(txtSecondsToWaitForConductorToStart.Text);
                    _holoNETClientAdmin.SaveDNA();

                    ConnectAdmin();
                }
                catch (Exception ex)
                {
                    //popupSecondsToWaitForConductorToStart.IsOpen = true;
                    txtSecondsToWaitForConductorToStartError.Visibility = Visibility.Visible;
                }
            }
        }

        private void btnPopupInstallhAppOk_Click(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == "")
                lblEnterName.Visibility = Visibility.Visible;
            else
            {
                lblEnterName.Visibility = Visibility.Collapsed;
                popupInstallhApp.Visibility = Visibility.Collapsed;

                LogMessage($"ADMIN: Installing hApp {InputTextBox.Text} ({_installinghAppPath})...");
                ShowStatusMessage($"Installing hApp {InputTextBox.Text}...", StatusMessageType.Information, true);
                _holoNETClientAdmin.AdminInstallApp(_holoNETClientAdmin.HoloNETDNA.AgentPubKey, InputTextBox.Text, _installinghAppPath);
            }
        }

        private void btnPopupInstallhAppCancel_Click(object sender, RoutedEventArgs e)
        {
            popupInstallhApp.Visibility = Visibility.Collapsed;
            lblEnterName.Visibility = Visibility.Collapsed;
        }

        private void chkAutoStartConductor_Checked(object sender, RoutedEventArgs e)
        {
            if (chkAutoShutdownConductor != null)
                chkAutoShutdownConductor.IsEnabled = true;
                
            if (chkShowConductorWindow != null)
                chkShowConductorWindow.IsEnabled = true;

            if (txtSecondsToWaitForConductorToStart != null)
                txtSecondsToWaitForConductorToStart.IsEnabled = true;

            if (lblSecondsToWaitForConductorToStart != null)
            {
                lblSecondsToWaitForConductorToStart.IsEnabled = true;
                lblSecondsToWaitForConductorToStart.Foreground = Brushes.WhiteSmoke;
            }
        }

        private void chkAutoStartConductor_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkAutoShutdownConductor != null)
                chkAutoShutdownConductor.IsEnabled = false;

            if (chkShowConductorWindow != null)
                chkShowConductorWindow.IsEnabled = false;

            if (txtSecondsToWaitForConductorToStart != null)
                txtSecondsToWaitForConductorToStart.IsEnabled = false;

            if (lblSecondsToWaitForConductorToStart != null)
            {
                lblSecondsToWaitForConductorToStart.IsEnabled = false;
                lblSecondsToWaitForConductorToStart.Foreground = Brushes.Gray;
            }

            if (txtSecondsToWaitForConductorToStartError != null)
                txtSecondsToWaitForConductorToStartError.Visibility = Visibility.Hidden;
        }

        private void btnViewDataEntries_Click(object sender, RoutedEventArgs e)
        {
            popupDataEntries.Visibility = Visibility.Visible;
            ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                LoadCollection(result.AppAgentClient);
            else
                _clientOperation = ClientOperation.LoadHoloNETCollection;
        }
        

        private void HoloNETEntries_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Collection Error", StatusMessageType.Error);
            LogMessage($"APP: HoloNET Data Entry Collection Error: {e.Reason}");
        }

        private void HoloNETEntries_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Collection Closed", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Collection Closed.");
        }

        private void HoloNETEntries_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Initialized", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Initialized.");
        }

        private void HoloNETEntries_OnHoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Removed From Collection", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Removed From Collection: {GetEntryInfo(e)}");
        }

        private void HoloNETEntries_OnHoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Added To Collection", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Added To Collection: {GetEntryInfo(e)}");
        }

        private void HoloNETEntries_OnHoloNETEntriesUpdated(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entries Updated (Collection Updated)", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entries Updated (Collection Updated).");
        }

        private void HoloNETEntries_OnCollectionLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Collection Loaded", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data EntryCollection Loaded: {GetEntryInfo(e)}");
        }

        private void btnDataEntriesPopupOk_Click(object sender, RoutedEventArgs e)
        {
            DateTime dob;

            if (string.IsNullOrEmpty(txtFirstName.Text))
            {
                lblNewEntryValidationErrors.Text = "Enter the first name.";
                lblNewEntryValidationErrors.Visibility = Visibility.Visible;
                txtFirstName.Focus();
            }

            else if (string.IsNullOrEmpty(txtLastName.Text))
            {
                lblNewEntryValidationErrors.Text = "Enter the last name.";
                lblNewEntryValidationErrors.Visibility = Visibility.Visible;
                txtLastName.Focus();
            }

            else if (string.IsNullOrEmpty(txtDOB.Text))
            {
                lblNewEntryValidationErrors.Text = "Enter the DOB.";
                lblNewEntryValidationErrors.Visibility = Visibility.Visible;
                txtDOB.Focus();
            }

            //else if (!DateTime.TryParse(txtDOB.Text, out dob))
            //{
            //    lblNewEntryValidationErrors.Text = "Enter a valid DOB.";
            //    lblNewEntryValidationErrors.Visibility = Visibility.Visible;
            //    txtDOB.Focus();
            //}

            else if (string.IsNullOrEmpty(txtEmail.Text))
            {
                lblNewEntryValidationErrors.Text = "Enter the Email.";
                lblNewEntryValidationErrors.Visibility = Visibility.Visible;
                txtEmail.Focus();
            }
            else
            {
                lblNewEntryValidationErrors.Visibility = Visibility.Hidden;
                ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                {
                    AddHoloNETEntryToCollection(result.AppAgentClient, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);

                    //We could alternatively save the entry first and then add it to the collection afterwards but this is 2 round trips to the conductor whereas AddHoloNETEntryToCollectionAndSave is only 1 and will automatically save the entry before adding it to the collection.
                    //SaveHoloNETEntry(result.AppAgentClient, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);
                }
                else
                    _clientOperation = ClientOperation.AddHoloNETEntryToCollection;  
            }
        }

        private void _holoNETEntryShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry (Shared) Error", StatusMessageType.Error);
            LogMessage($"APP: HoloNET Data Entry (Shared) Error: {e.Reason}");
        }

        private void _holoNETEntryShared_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry (Shared) Deleted", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry (Shared) Saved", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Saved: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry (Shared) Closed", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
        }

        private void _holoNETEntryShared_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry (Shared) Loaded", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Loaded: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry (Shared) Initialized", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Initialized.");
        }

        private void _holoNETEntry_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            //Only needed for non-async version.
            //HandleHoloNETEntrySaved(e);
        }

        private void _holoNETEntry_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Loaded", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Loaded: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Initialized", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
        }

        private void _holoNETEntry_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Error", StatusMessageType.Error);
            LogMessage($"APP: HoloNET Data Entry Error: {e.Reason}");
        }

        private void _holoNETEntry_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Deleted", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnCollectionUpdated(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Collection Updated", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Collection Updated");
        }

        private void _holoNETEntry_OnCollectionLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Collection Loaded", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Collection Loaded");
        }

        private void _holoNETEntry_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"APP: HoloNET Data Entry Closed", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
        }

        private void btnDataEntriesPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            lblNewEntryValidationErrors.Visibility = Visibility.Hidden;
            popupDataEntries.Visibility = Visibility.Collapsed;
        }

        private void gridLog_LayoutUpdated(object sender, EventArgs e)
        {
            if (gridLog.ActualHeight - 60 > 0)
                lstOutput.Height = gridLog.ActualHeight - 60;
        }

        private void btnDisconnectClient_Click(object sender, RoutedEventArgs e)
        {
            InstalledApp app = gridHapps.SelectedItem as InstalledApp;

            if (app != null)
            {
                HoloNETClient client = GetClient(app.DnaHash, app.AgentPubKey, app.Name);

                if (client != null && client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    Button btnDisconnectClient = sender as Button;

                    if (btnDisconnectClient != null)
                        btnDisconnectClient.IsEnabled = false;

                    _removeClientConnectionFromPoolAfterDisconnect = false;
                    client.Disconnect();
                }
            }
        }

        private void btnHoloNETEntry_Click(object sender, RoutedEventArgs e)
        {
            popupHoloNETEntry.Visibility = Visibility.Visible;
        }

        private void btnHoloNETEntryPopupOk_Click(object sender, RoutedEventArgs e)
        {
            DateTime dob = DateTime.MinValue;

            if (string.IsNullOrEmpty(txtHoloNETEntryFirstName.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the first name.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtHoloNETEntryFirstName.Focus();
            }

            else if (string.IsNullOrEmpty(txtHoloNETEntryLastName.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the last name.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtHoloNETEntryLastName.Focus();
            }

            else if (string.IsNullOrEmpty(txtHoloNETEntryDOB.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the DOB.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtHoloNETEntryDOB.Focus();
            }

            //else if (!DateTime.TryParse(txtDOB.Text, out dob))
            //{
            //    lblHoloNETEntryValidationErrors.Text = "Enter a valid DOB.";
            //    lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
            //    txtDOB.Focus();
            //}

            else if (string.IsNullOrEmpty(txtHoloNETEntryEmail.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the Email.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtHoloNETEntryEmail.Focus();
            }
            else
            {
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Hidden;

                //If we intend to re - use an object then we can store it globally so we only need to init once...
                if (_holoNETEntry == null || (_holoNETEntry != null && !_holoNETEntry.IsInitialized))
                {
                    LogMessage("APP: Initializing HoloNET Entry...");
                    ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true);

                    // If the HoloNET Entry is null then we need to init it now...
                    // Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
                    if (_holoNETEntry == null)
                        InitHoloNETEntry();
                    else
                    {
                        _holoNETEntry.Initialize(); //Will connect to the internally created instance of the HoloNETClient.
                        SaveHoloNETEntry();
                    }
                }
                else
                    SaveHoloNETEntry();
            }
        }

        private void SaveHoloNETEntry()
        {
            //Non async way.
            //If you use Load or Save non-async versions you will need to wait for the OnInitialized event to fire before calling.
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
            Dispatcher.InvokeAsync(async () =>
            {
                if (_holoNETEntry != null && _holoNETEntry.IsInitialized)
                {
                    ShowStatusMessage($"APP: Saving HoloNET Data Entry...", StatusMessageType.Information, true);
                    LogMessage($"APP: Saving HoloNET Data Entry...");

                    string[] parts = txtHoloNETEntryDOB.Text.Split('/');

                    _holoNETEntry.FirstName = txtHoloNETEntryFirstName.Text;
                    _holoNETEntry.LastName = txtHoloNETEntryLastName.Text;
                    _holoNETEntry.DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                    _holoNETEntry.Email = txtHoloNETEntryEmail.Text;

                    //SaveAsync (as well as LoadAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await _holoNETEntry.SaveAsync(); //No event handlers are needed.
                    HandleHoloNETEntrySaved(result);
                }
            });
        }

        private void btnHoloNETEntryPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            lblHoloNETEntryValidationErrors.Visibility = Visibility.Hidden;
            popupHoloNETEntry.Visibility = Visibility.Hidden;
        }

        private void btnHoloNETEntrySharedPopupOk_Click(object sender, RoutedEventArgs e)
        {
            DateTime dob;

            if (string.IsNullOrEmpty(txtEntryFirstName.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the first name.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtEntryFirstName.Focus();
            }

            else if (string.IsNullOrEmpty(txtEntryLastName.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the last name.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtEntryLastName.Focus();
            }

            else if (string.IsNullOrEmpty(txtEntryDOB.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the DOB.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtEntryDOB.Focus();
            }

            //else if (!DateTime.TryParse(txtDOB.Text, out dob))
            //{
            //    lblHoloNETEntryValidationErrors.Text = "Enter a valid DOB.";
            //    lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
            //    txtDOB.Focus();
            //}

            else if (string.IsNullOrEmpty(txtEntryEmail.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the Email.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtEntryEmail.Focus();
            }
            else
            {
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Hidden;

                //If we intend to re-use an object then we can store it globally so we only need to init once...
                //if (_holoNETEntry == null || (_holoNETEntry != null && !_holoNETEntry.IsInitialized))
                //{
                //    LogMessage("APP: Initializing HoloNET Entry...");
                //    ShowStatusMessage("Initializing HoloNET Entry...", StatusMessageType.Information, true);

                //    if (_holoNETEntry == null)
                //    {
                //        _holoNETEntry = new AvatarMultiple();

                //        //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                //        _holoNETEntry.OnInitialized += _holoNETEntry_OnInitialized;
                //        _holoNETEntry.OnLoaded += _holoNETEntry_OnLoaded;
                //        _holoNETEntry.OnClosed += _holoNETEntry_OnClosed;
                //        _holoNETEntry.OnSaved += _holoNETEntry_OnSaved;
                //        _holoNETEntry.OnDeleted += _holoNETEntry_OnDeleted;
                //        _holoNETEntry.OnError += _holoNETEntry_OnError;
                //    }
                //}


                ////If we intend to re-use an object then we can store it globally so we only need to init once...
                //if (_holoNETEntry == null || (_holoNETEntry != null && !_holoNETEntry.IsInitialized))
                //    InitHoloNETEntry(client);

                //else if (_holoNETEntry.HoloNETClient.EndPoint != client.EndPoint)
                //    _holoNETEntry.HoloNETClient = client;

                //if (_holoNETEntry != null && _holoNETEntry.IsInitialized)
                //{
                //    //ShowStatusMessage($"APP: Saving HoloNET Data Entry...", StatusMessageType.Information, true);
                //    //LogMessage($"APP: Saving HoloNET Data Entry...");

                //    string[] parts = dob.Split('/');

                //    _holoNETEntry.FirstName = firstName;
                //    _holoNETEntry.LastName = lastName;
                //    _holoNETEntry.DOB = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                //    _holoNETEntry.Email = email;

                //    //Non async way.
                //    //If you use Load or Save non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //    //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //    //_holoNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                //    //Async way.
                //    Dispatcher.InvokeAsync(async () =>
                //    {
                //        ShowStatusMessage($"APP: Saving HoloNET Data Entry...", StatusMessageType.Information, true);
                //        LogMessage($"APP: Saving HoloNET Data Entry...");

                //        //SaveAsync (as well as LoadAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                //        ZomeFunctionCallBackEventArgs result = await _holoNETEntry.SaveAsync(); //No event handlers are needed.
                //        HandleHoloNETEntrySaved(result);
                //    });
                //}

                ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    SaveHoloNETEntryShared(result.AppAgentClient, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);
                else
                    _clientOperation = ClientOperation.SaveHoloNETEntry;
            }
        }
    }
}