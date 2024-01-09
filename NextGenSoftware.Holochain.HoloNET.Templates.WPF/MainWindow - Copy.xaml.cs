﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;
using NextGenSoftware.Utilities.ExtentionMethods;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _holoNETEntryDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        //private const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis-holonet-collection\BUILD\happ\oasis.happ";
        private const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _holoNETEntryDemoAppId = "oasis-holonet-entry-demo-app";
        private const string _holoNETCollectionDemoAppId = "oasis-holonet-collection-demo-app";
        private const string _role_name = "oasis";
        private HoloNETClient? _holoNETClientAdmin;
        private List<HoloNETClient> _holoNETappClients = new List<HoloNETClient>();
        private bool _rebooting = false;
        private bool _adminDisconnected = false;
       // private bool _appDisconnected = false;
        private string _installinghAppName = "";
        private string _installinghAppPath = "";
        private byte[][] _installingAppCellId = null;
        dynamic _paramsObject = null;
        private int _clientsToDisconnect = 0;
        private int _clientsDisconnected = 0;
        private Avatar _holoNETEntry;
        private Dictionary<string, AvatarShared> _holoNETEntryShared = new Dictionary<string, AvatarShared>();
        private ClientOperation _clientOperation;
        private ushort _appAgentClientPort = 0;
        private bool _removeClientConnectionFromPoolAfterDisconnect = true;
        private bool _initHoloNETEntryDemo = false;
        private bool _showAppsListedInLog = true;
        private AvatarShared _currentAvatar;

        public ObservableCollection<InstalledApp> InstalledApps { get; set; } = new ObservableCollection<InstalledApp>();

        public Dictionary<string, HoloNETObservableCollection<AvatarShared>> HoloNETEntriesShared { get; set; } = new Dictionary<string, HoloNETObservableCollection<AvatarShared>>();

        public HoloNETObservableCollection<Avatar> HoloNETEntries { get; set; }

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
                HoloNETEntries.OnClosed -= HoloNETEntries_OnClosed;
                HoloNETEntries.OnCollectionLoaded -= HoloNETEntries_OnCollectionLoaded;
                HoloNETEntries.OnCollectionSaved -= HoloNETEntries_OnCollectionSaved;
                HoloNETEntries.OnError -= HoloNETEntries_OnError;
                HoloNETEntries.OnHoloNETEntryAddedToCollection -= HoloNETEntries_OnHoloNETEntryAddedToCollection;
                HoloNETEntries.OnHoloNETEntryRemovedFromCollection -= HoloNETEntries_OnHoloNETEntryRemovedFromCollection;
            }

            if (HoloNETEntriesShared != null)
            {
                foreach (string key in HoloNETEntriesShared.Keys)
                {
                    HoloNETEntriesShared[key].OnInitialized -= HoloNETEntriesShared_OnInitialized;
                    HoloNETEntriesShared[key].OnCollectionLoaded -= HoloNETEntriesShared_OnCollectionLoaded;
                    HoloNETEntriesShared[key].OnCollectionSaved -= HoloNETEntriesShared_OnCollectionSaved;
                    HoloNETEntriesShared[key].OnHoloNETEntryAddedToCollection -= HoloNETEntriesShared_OnHoloNETEntryAddedToCollection;
                    HoloNETEntriesShared[key].OnHoloNETEntryRemovedFromCollection -= HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection;
                    HoloNETEntriesShared[key].OnClosed -= HoloNETEntriesShared_OnClosed;
                    HoloNETEntriesShared[key].OnError -= HoloNETEntriesShared_OnError;
                }
            }

            if (_holoNETEntryShared != null)
            {
                foreach (string key in _holoNETEntryShared.Keys)
                {
                    _holoNETEntryShared[key].OnInitialized += _holoNETEntryShared_OnInitialized;
                    _holoNETEntryShared[key].OnLoaded += _holoNETEntryShared_OnLoaded;
                    _holoNETEntryShared[key].OnClosed += _holoNETEntryShared_OnClosed;
                    _holoNETEntryShared[key].OnSaved += _holoNETEntryShared_OnSaved;
                    _holoNETEntryShared[key].OnDeleted += _holoNETEntryShared_OnDeleted;
                    _holoNETEntryShared[key].OnError += _holoNETEntryShared_OnError;
                }
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

            if (ucHoloNETCollectionEntry != null)
            {
                ucHoloNETCollectionEntry.txtHoloNETEntryFirstName.TextChanged -= TxtHoloNETEntryFirstName_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryLastName.TextChanged -= TxtHoloNETEntryLastName_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryDOB.TextChanged -= TxtHoloNETEntryDOB_TextChanged;
                ucHoloNETCollectionEntry.txtHoloNETEntryEmail.TextChanged -= TxtHoloNETEntryEmail_TextChanged;
            }
            
            CloseAllConnections();
        }

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

        private void TxtHoloNETEntryEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChanges();
        }

        private void TxtHoloNETEntryDOB_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChanges();
        }

        private void TxtHoloNETEntryLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChanges();
        }

        private void TxtHoloNETEntryFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckForChanges();
        }

        private void CheckForChanges()
        {
            if (_currentAvatar != null)
            {
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryFirstName.Text != _currentAvatar.FirstName)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryLastName.Text != _currentAvatar.LastName)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryDOB.Text != _currentAvatar.DOB.ToShortDateString())
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;

                if (ucHoloNETCollectionEntry.txtHoloNETEntryEmail.Text != _currentAvatar.Email)
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = true;
            }
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

        /// <summary>
        /// Will init the HoloNET Entry which includes installing and enabling the app, signing credentials, attaching the app interface, then finally creating and connecting to the internal instance of the HoloNETClient.
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

        private void HoloNETEntries_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Initialized", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                LogMessage($"APP: HoloNET Collection Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
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

        private void HoloNETEntries_OnHoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Entry Added To Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                LogMessage($"APP: HoloNET Entry Added To Collection.");
            }
        }

        private void HoloNETEntries_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"Error Occured For HoloNET Collection. Reason: {e.Reason}", StatusMessageType.Error, false, ucHoloNETCollectionEntryInternal);
            LogMessage($"APP: Error Occured For HoloNET Collection. Reason: {e.Reason}");
        }

        private void HoloNETEntries_OnCollectionSaved(object sender, HoloNETCollectionSavedResult e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Saved.", StatusMessageType.Success, false, ucHoloNETCollectionEntryInternal);
                LogMessage($"APP: HoloNET Collection Saved.");
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

        private void InitHoloNETEntryShared(HoloNETClient client)
        {
            LogMessage("APP: Initializing HoloNET Entry (Shared)...");
            ShowStatusMessage("Initializing HoloNET Entry (Shared)...", StatusMessageType.Information, true);

            if (!_holoNETEntryShared.ContainsKey(CurrentApp.Name) || (_holoNETEntryShared.ContainsKey(CurrentApp.Name) && _holoNETEntryShared[CurrentApp.Name] == null))
            {
                _holoNETEntryShared[CurrentApp.Name] = new AvatarShared(client);

                //If we are using SaveAsync (or LoadAsync) we do not need to worry about any events such as OnSaved if you don't need them.
                _holoNETEntryShared[CurrentApp.Name].OnInitialized += _holoNETEntryShared_OnInitialized;
                _holoNETEntryShared[CurrentApp.Name].OnLoaded += _holoNETEntryShared_OnLoaded;
                _holoNETEntryShared[CurrentApp.Name].OnClosed += _holoNETEntryShared_OnClosed;
                _holoNETEntryShared[CurrentApp.Name].OnSaved += _holoNETEntryShared_OnSaved;
                _holoNETEntryShared[CurrentApp.Name].OnDeleted += _holoNETEntryShared_OnDeleted;
                _holoNETEntryShared[CurrentApp.Name].OnError += _holoNETEntryShared_OnError;
            }
            else
                _holoNETEntryShared[CurrentApp.Name].HoloNETClient = client;
        }

        private void InitHoloNETCollection(HoloNETClient client)
        {
            LogMessage("APP: Initializing HoloNET Collection...");
            ShowStatusMessage("Initializing HoloNET Collection...", StatusMessageType.Information, true);

            if (!HoloNETEntriesShared.ContainsKey(CurrentApp.Name) || (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] == null))
            {
                HoloNETEntriesShared[CurrentApp.Name] = new HoloNETObservableCollection<AvatarShared>("oasis", "load_avatars", "add_avatar", "remove_avatar", client, "update_avatars");
                HoloNETEntriesShared[CurrentApp.Name].OnInitialized += HoloNETEntriesShared_OnInitialized;
                HoloNETEntriesShared[CurrentApp.Name].OnCollectionLoaded += HoloNETEntriesShared_OnCollectionLoaded;
                HoloNETEntriesShared[CurrentApp.Name].OnCollectionSaved += HoloNETEntriesShared_OnCollectionSaved;
                HoloNETEntriesShared[CurrentApp.Name].OnHoloNETEntryAddedToCollection += HoloNETEntriesShared_OnHoloNETEntryAddedToCollection;
                HoloNETEntriesShared[CurrentApp.Name].OnHoloNETEntryRemovedFromCollection += HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection;
                HoloNETEntriesShared[CurrentApp.Name].OnClosed += HoloNETEntriesShared_OnClosed;
                HoloNETEntriesShared[CurrentApp.Name].OnError += HoloNETEntriesShared_OnError;
                HoloNETEntriesShared[CurrentApp.Name].CollectionChanged += HoloNETEntriesShared_CollectionChanged;
            }
            else
                HoloNETEntriesShared[CurrentApp.Name].HoloNETClient = client;
        }

        private void HoloNETEntriesShared_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string msg = $"HoloNET Collection Changed. Action: {Enum.GetName(typeof(NotifyCollectionChangedAction), e.Action)}, New Items: {(e.NewItems != null ? e.NewItems.Count : 0)}, Old Items: {(e.OldItems != null ? e.OldItems.Count : 0)}";
            LogMessage($"APP: {msg}");
            ShowStatusMessage(msg, StatusMessageType.Information, true);
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

        private void LoadCollection(HoloNETClient client)
        {
            if (!HoloNETEntriesShared.ContainsKey(CurrentApp.Name) 
                || (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] == null) 
                || (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] != null && !HoloNETEntriesShared[CurrentApp.Name].IsInitialized))
                    InitHoloNETCollection(client);

            else if (HoloNETEntriesShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                HoloNETEntriesShared[CurrentApp.Name].HoloNETClient = client;

            else
            {
                ShowStatusMessage($"HoloNET Collection Already Initialized.", StatusMessageType.Success, false);
                LogMessage($"APP: HoloNET Collection Already Initialized..");
            }

            if (HoloNETEntriesShared.ContainsKey(CurrentApp.Name) && HoloNETEntriesShared[CurrentApp.Name] != null)
            {
                //ShowStatusMessage($"APP: Loading HoloNET Collection...", StatusMessageType.Information, true);
                //LogMessage($"APP: Loading HoloNET Collection...");

                //Non-async way (you need to use the OnCollectionLoaded event to know when the collection has loaded and receive the data/collection.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //HoloNETEntries[CurrentApp.Name].LoadCollection(); 

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    ShowStatusMessage($"Loading HoloNET Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntry);
                    LogMessage($"APP: Loading HoloNET Collection...");

                    //LoadCollectionAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    HoloNETCollectionLoadedResult<AvatarShared> result = await HoloNETEntriesShared[CurrentApp.Name].LoadCollectionAsync(); //No event handlers are needed but you can still use if you like.
                    HandleHoloNETCollectionLoaded(result);
                });
            }
        }

        private void LoadHoloNETEntryShared(HoloNETClient client)
        {
            if (!_holoNETEntryShared.ContainsKey(CurrentApp.Name)
                || (_holoNETEntryShared.ContainsKey(CurrentApp.Name) && _holoNETEntryShared[CurrentApp.Name] == null)
                || (_holoNETEntryShared.ContainsKey(CurrentApp.Name) && _holoNETEntryShared[CurrentApp.Name] != null && !_holoNETEntryShared[CurrentApp.Name].IsInitialized))
                    InitHoloNETEntryShared(client);

            else if (_holoNETEntryShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                _holoNETEntryShared[CurrentApp.Name].HoloNETClient = client;
            else
            {
                ShowStatusMessage($"HoloNET Entry (Shared) Already Initialized.", StatusMessageType.Success, false);
                LogMessage($"APP: HoloNET Entry (Shared) Already Initialized..");
            }

            if (_holoNETEntryShared.ContainsKey(CurrentApp.Name) && _holoNETEntryShared[CurrentApp.Name] != null)
            {
                //ShowStatusMessage($"APP: Loading HoloNET Entry Shared...", StatusMessageType.Information, true);
                //LogMessage($"APP: Loading HoloNET Entry Shared...");

                //Non-async way (you need to use the OnLoaded event to know when the entry has loaded.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //_holoNETEntryShared[CurrentApp.Name].Load(); 

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    if (!string.IsNullOrEmpty(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash))
                    {
                        ShowStatusMessage($"APP: Loading HoloNET Entry Shared...", StatusMessageType.Information, true, ucHoloNETEntryShared);
                        LogMessage($"Loading HoloNET Entry Shared...");

                        //LoadAsync (as well as all other async methods) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                        ZomeFunctionCallBackEventArgs result = await _holoNETEntryShared[CurrentApp.Name].LoadAsync(HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash); //No event handlers are needed but you can still use if you like.
                        HandleHoloNETEntrySharedLoaded(result);
                    }
                });
            }
        }

        private void AddHoloNETEntryToCollection(HoloNETClient client, string firstName, string lastName, DateTime dob, string email)
        {
            if (HoloNETEntriesShared == null || (HoloNETEntriesShared != null && !HoloNETEntriesShared[CurrentApp.Name].IsInitialized))
                InitHoloNETCollection(client);

            else if (HoloNETEntriesShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                HoloNETEntriesShared[CurrentApp.Name].HoloNETClient = client;

            if (HoloNETEntriesShared != null && HoloNETEntriesShared[CurrentApp.Name].IsInitialized)
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
                    ShowStatusMessage($"Adding HoloNET Entry To Collection...", StatusMessageType.Information, true, ucHoloNETCollectionEntry);
                    LogMessage($"APP: Adding HoloNET Entry To Collection...");

                    //Will add the entry to the collection and then persist the change to the hc/rust/happ code.
                    //We don't need to call Save() on the entry before calling this method because this method will automatically save the entry and then add it to the collection. It can also of course add an existing entry to the collection. The same applies to the SaveCollection method below.
                    ZomeFunctionCallBackEventArgs result = await HoloNETEntriesShared[CurrentApp.Name].AddHoloNETEntryToCollectionAndSaveAsync(new AvatarShared()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        DOB = dob, //new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0])),
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

        private void SaveHoloNETEntryShared(HoloNETClient client, string firstName, string lastName, DateTime dob, string email)
        {
            //If we intend to re-use an object then we can store it globally so we only need to init once...
            if (_holoNETEntryShared[CurrentApp.Name] == null || (_holoNETEntryShared[CurrentApp.Name] != null && !_holoNETEntryShared[CurrentApp.Name].IsInitialized))
                InitHoloNETEntryShared(client);

            else if (_holoNETEntryShared[CurrentApp.Name].HoloNETClient.EndPoint != client.EndPoint)
                _holoNETEntryShared[CurrentApp.Name].HoloNETClient = client;

            if (_holoNETEntryShared != null && _holoNETEntryShared[CurrentApp.Name].IsInitialized)
            {
                _holoNETEntryShared[CurrentApp.Name].FirstName = firstName;
                _holoNETEntryShared[CurrentApp.Name].LastName = lastName;
                _holoNETEntryShared[CurrentApp.Name].DOB = dob; //new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                _holoNETEntryShared[CurrentApp.Name].Email = email;

                //Non async way.
                //If you use Load or Save non-async versions you will need to wait for the OnInitialized event to fire before calling.
                //For non-async methods you will need to wait till the OnInitialized event to raise before calling any other methods such as the one below...
                //_holoNETEntry.Save(); //For this OnSaved event handler above is required (only if you want to see what the result was!) //TODO: Check if this works without waiting for OnInitialized event!

                //Async way.
                Dispatcher.InvokeAsync(async () =>
                {
                    ShowStatusMessage($"Saving HoloNET Data Entry (Shared)...", StatusMessageType.Information, true, ucHoloNETEntryShared);
                    LogMessage($"APP: Saving HoloNET Data Entry (Shared)...");

                    //SaveAsync (as well as LoadAsync) will automatically init and wait for the client to finish connecting and retreiving agentPubKey (if needed) and raising the OnInitialized event.
                    ZomeFunctionCallBackEventArgs result = await _holoNETEntryShared[CurrentApp.Name].SaveAsync(); //No event handlers are needed.
                    HandleHoloNETEntrySharedSaved(result);
                });
            }
        }

        //private void HandleHoloNETEntrySaved(ZomeFunctionCallBackEventArgs result)
        //{
        //    if (result.IsCallSuccessful && !result.IsError)
        //    {
        //        ShowStatusMessage($"APP: HoloNET Entry Saved.", StatusMessageType.Success);
        //        LogMessage($"APP: HoloNET Entry Saved.");
        //        popupHoloNETEntry.Visibility = Visibility.Hidden;
        //    }
        //    else
        //    {
        //        lblHoloNETEntryValidationErrors.Text = result.Message;
        //        lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
        //        ShowStatusMessage($"APP: Error occured saving entry: {result.Message}", StatusMessageType.Error);
        //        LogMessage($"APP: Error occured saving entry: {result.Message}");
        //    }
        //}

        private void HandleHoloNETEntrySharedSaved(ZomeFunctionCallBackEventArgs result)
        {
            //TEMP to test!
            //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(_holoNETEntry);

            if (result.IsCallSuccessful && !result.IsError)
            {
                ShowStatusMessage($"HoloNET Entry (Shared) Saved.", StatusMessageType.Success, false, ucHoloNETEntryShared);
                LogMessage($"APP: HoloNET Entry (Shared) Saved.");

                //Can use either of the lines below to get the EntryHash for the new entry.
                HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = result.Entries[0].EntryHash;
                HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarSharedEntryHash = _holoNETEntryShared[CurrentApp.Name].EntryHash;
                HoloNETEntryDNAManager.SaveDNA();

                //Will add the entry to the collection and then persist the change to the hc/rust/happ code.
                //TODO: Dont think we need to call Save() on the entry before calling this method because this method will automatically save the entry and then add it to the collection. It can also of course add an existing entry to the collection. The same applies to the SaveCollection method below.
                //HoloNETEntries.AddHoloNETEntryToCollectionAndSave(result.Entries[0].EntryDataObject); 

                //Allows you to batch add/remove multiple entries to the collection and then persist the changes to the hc/rust/happ code in one go.
                //HoloNETEntries.Add(_holoNETEntry); //Will only add the entry to the collection in memory (it will NOT persist to hc/rust/happ code until SaveCollection is called.
                //HoloNETEntries.SaveCollection(); //Will look for any changes since the last time this method was called (includes entries added/removed from the collection as well as any changes made to entries themselves). This can invoke multiple events including OnHoloNETEntryAddedToCollection, OnHoloNETEntryRemovedFromCollection & OnHoloNETEntriesUpdated (if any changes were made to the entries themselves))/
            }
            else
            {
                ucHoloNETEntryShared.ShowStatusMessage(result.Message, StatusMessageType.Error);
                ShowStatusMessage($"Error occured saving entry (Shared): {result.Message}", StatusMessageType.Error);
                LogMessage($"APP: Error occured saving entry (Shared): {result.Message}");
            }
        }

        private void HandleHoloNETCollectionLoaded(HoloNETCollectionLoadedResult<AvatarShared> result)
        {
            //TODO: TEMP UNTIL REAL DATA IS RETURNED! REMOVE AFTER!
            if (HoloNETEntriesShared[CurrentApp.Name] != null && HoloNETEntriesShared[CurrentApp.Name].Count == 0) 
            {
                if (CurrentApp.Name == _holoNETCollectionDemoAppId)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        AvatarShared avatar1 = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "David",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar1.MockData();
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar1);
                    }

                    AvatarShared avatar = new AvatarShared()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "James",
                        LastName = "Ellams",
                        Email = "davidellams@hotmail.com",
                        DOB = new DateTime(1980, 4, 11)
                    };

                    //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                    avatar.MockData();
                    HoloNETEntriesShared[CurrentApp.Name].Add(avatar);

                    avatar = new AvatarShared()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Noah",
                        LastName = "Ellams",
                        Email = "davidellams@hotmail.com",
                        DOB = new DateTime(1980, 4, 11)
                    };

                    //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                    avatar.MockData();
                    HoloNETEntriesShared[CurrentApp.Name].Add(avatar);
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        AvatarShared avatar = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "Elba",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar.MockData();
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar);

                        avatar = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "David",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar.MockData();
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar);

                        avatar = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "James",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar.MockData();
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar);

                        avatar = new AvatarShared()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "Noah",
                            LastName = "Ellams",
                            Email = "davidellams@hotmail.com",
                            DOB = new DateTime(1980, 4, 11)
                        };

                        //TODO: REMOVE AFTER, TEMP TILL GET ZOMECALLS WORKING AGAIN!
                        avatar.MockData();
                        HoloNETEntriesShared[CurrentApp.Name].Add(avatar);
                    }
                } 
            }

            gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

            if (!result.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Loaded.", StatusMessageType.Error);
                LogMessage($"APP: HoloNET Collection Loaded.");
                //gridDataEntries.ItemsSource = result.Entries; //Can set it using this or line below.
                gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];
            }
            else
            {
                ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                ShowStatusMessage($"Error Occured Loading HoloNET Collection.", StatusMessageType.Error);
                LogMessage($"APP: Error Occured Loading HoloNET Collection. Reason: {result.Message}");
            }
        }

        private void HandleHoloNETEntrySharedLoaded(ZomeFunctionCallBackEventArgs result)
        {
            //TODO: TEMP, REMOVE AFTER!
            _holoNETEntryShared[CurrentApp.Name].Id = Guid.NewGuid();
            _holoNETEntryShared[CurrentApp.Name].CreatedBy = "David";
            _holoNETEntryShared[CurrentApp.Name].CreatedDate = DateTime.Now;
            _holoNETEntryShared[CurrentApp.Name].EntryHash = Guid.NewGuid().ToString();
            _holoNETEntryShared[CurrentApp.Name].PreviousVersionEntryHash = Guid.NewGuid().ToString();

            ucHoloNETEntryShared.DataContext = _holoNETEntryShared[CurrentApp.Name];
            RefreshHoloNETEntryMetaData(_holoNETEntryShared[CurrentApp.Name], ucHoloNETEntrySharedMetaData);

            if (result.IsCallSuccessful && !result.IsError)
            {
                ShowStatusMessage($"HoloNET Entry Shared Loaded.", StatusMessageType.Success, false, ucHoloNETEntryShared);
                LogMessage($"APP: HoloNET Entry Shared Loaded.");

                ucHoloNETEntryShared.DataContext = _holoNETEntryShared[CurrentApp.Name];
                RefreshHoloNETEntryMetaData(_holoNETEntryShared[CurrentApp.Name], ucHoloNETEntrySharedMetaData);
            }
            else
            {
                ucHoloNETEntryShared.ShowStatusMessage(result.Message, StatusMessageType.Error);
                //ShowStatusMessage($"Error Occured Loading HoloNET Entry Shared.", StatusMessageType.Error, false, ucHoloNETEntryShared);
                LogMessage($"APP: Error Occured Loading HoloNET Entry Shared. Reason: {result.Message}");
            }
        }

        private void HandleHoloNETEntryAddedToCollection(ZomeFunctionCallBackEventArgs result)
        {
            if (result.IsCallSuccessful && !result.IsError)
            {
                gridDataEntries.ItemsSource = null;
                gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

                ucHoloNETCollectionEntry.FirstName = "";
                ucHoloNETCollectionEntry.LastName = "";
                ucHoloNETCollectionEntry.DOB = "";
                ucHoloNETCollectionEntry.Email = "";

                ShowStatusMessage($"HoloNET Entry Added To Collection.", StatusMessageType.Success, false, ucHoloNETCollectionEntry);
                LogMessage($"APP: HoloNET Entry Added To Collection.");
            }
            else
            {
                //TODO: TEMP! REMOVE AFTER!
                gridDataEntries.ItemsSource = null;
                gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

                //TODO: TEMP! REMOVE AFTER!
                ucHoloNETCollectionEntry.FirstName = "";
                ucHoloNETCollectionEntry.LastName = "";
                ucHoloNETCollectionEntry.DOB = "";
                ucHoloNETCollectionEntry.Email = "";

                ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                ShowStatusMessage($"Error Occured Adding HoloNET Entry To Collection.", StatusMessageType.Error);
                LogMessage($"APP: Error Occured Adding HoloNET Entry To Collection. Reason: {result.Message}");
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
            if (!_initHoloNETEntryDemo)
            {
                LogMessage($"ADMIN: hApp {e.InstalledAppId} Uninstalled.");
                ShowStatusMessage($"hApp {e.InstalledAppId} Uninstalled.", StatusMessageType.Success);
                ListHapps();
            }
        }

        private void _holoNETClientAdmin_OnAdminAppsListedCallBack(object sender, AdminAppsListedCallBackEventArgs e)
        {
            //if (_showAppsListedInLog)
                ProcessListedApps(e);
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
                    IsSharedConnection = app.installed_app_id != _holoNETEntryDemoAppId && app.installed_app_id != _holoNETCollectionDemoAppId ? true: false
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


                    //_holoNETappClients[_holoNETappClients.Count - 1].OnConnected += _holoNETClientApp_OnConnected;
                    //_holoNETappClients[_holoNETappClients.Count - 1].OnReadyForZomeCalls += _holoNETClientApp_OnReadyForZomeCalls;
                    //_holoNETappClients[_holoNETappClients.Count - 1].OnZomeFunctionCallBack += _holoNETClientApp_OnZomeFunctionCallBack;
                    //_holoNETappClients[_holoNETappClients.Count - 1].OnDisconnected += _holoNETClientApp_OnDisconnected;
                    //_holoNETappClients[_holoNETappClients.Count - 1].OnDataReceived += _holoNETClientApp_OnDataReceived;
                    //_holoNETappClients[_holoNETappClients.Count - 1].OnDataSent += _holoNETClientApp_OnDataSent;
                    //_holoNETappClients[_holoNETappClients.Count - 1].OnError += _holoNETClientApp_OnError;

                    //UpdateNumerOfClientConnections();
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
                if (_paramsObject != null)
                {
                    LogMessage("APP: Calling Zome Function...");
                    ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                    client.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, _paramsObject);
                    _paramsObject = null;
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

            UpdateNumerOfClientConnections();
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
            if (!_initHoloNETEntryDemo)
            {
                LogMessage($"ADMIN: AgentPubKey Generated for EndPoint: {e.EndPoint} and Id: {e.Id}: AgentPubKey: {e.AgentPubKey}");
                ShowStatusMessage($"AgentPubKey Generated.", StatusMessageType.Success);

                LogMessage($"ADMIN: Listing hApps...");
                ShowStatusMessage($"Listing hApps...", StatusMessageType.Information, true);
                _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
            }
        }

        private void HoloNETClient_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            LogMessage("ADMIN: Disconnected");
            ShowStatusMessage($"Admin WebSocket Disconnected.");
            _adminDisconnected = true;

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
                {
                    LogMessage($"ADMIN hApp NOT Installed! Reason: {e.Message}");
                    ShowStatusMessage($"hApp NOT Installed! Reason: {e.Message}", StatusMessageType.Error);
                }

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

            //ListHapps();

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
            //Process.Start("notepad.exe", "Logs\\HoloNET.log");
            Process.Start("notepad.exe", $"{HoloNETDNAManager.HoloNETDNA.LogPath}\\{HoloNETDNAManager.HoloNETDNA.LogFileName}");
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
            LogMessage("ADMIN: Listing DNAs...");
            ShowStatusMessage($"Listing DNAs...");
            _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
        }

        private void btnListCellIds_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing Cell Ids...");
            ShowStatusMessage($"Listing Cell Ids...", StatusMessageType.Information, true);
            _holoNETClientAdmin.AdminListCellIds();
        }

        private void btnListAttachedInterfaces_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("ADMIN: Listing Attached Interfaces...");
            ShowStatusMessage($"Listing Attached Interfaces...", StatusMessageType.Information, true);
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
            _paramsObject = new ExpandoObject();

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
                        ExpandoObjectHelpers.AddProperty(_paramsObject, paramParts[0], paramParts[1]);
                    }

                    ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

                    if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    {
                        LogMessage("APP: Calling Zome Function...");
                        ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                        result.AppAgentClient.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, _paramsObject);
                        _paramsObject = null;
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
                    txtSecondsToWaitForConductorToStartError.Visibility = Visibility.Visible;
                }
            }
        }

        private void btnPopupInstallhAppOk_Click(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == "")
            {
                lblInstallAppErrorMessage.Text = "Please enter the hApp name";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else if (InputTextBox.Text == _holoNETEntryDemoAppId)
            {
                lblInstallAppErrorMessage.Text = "Sorry that name is reserved for the HoloNETEntry (Inernal Connection) popup.";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else if (InputTextBox.Text == _holoNETCollectionDemoAppId)
            {
                lblInstallAppErrorMessage.Text = "Sorry that name is reserved for the HoloNET Collection (Inernal Connection) popup.";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else
            {
                lblInstallAppErrorMessage.Visibility = Visibility.Collapsed;
                popupInstallhApp.Visibility = Visibility.Collapsed;

                LogMessage($"ADMIN: Generating New AgentPubKey...");
                ShowStatusMessage($"Generating New AgentPubKey...", StatusMessageType.Information, true, ucHoloNETEntry);

                Dispatcher.InvokeAsync(async () =>
                {
                    //We don't normally need to generate a new agentpubkey for each hApp installed if each hApp has a unique DnaHash.
                    //But in this case it allows us to install the same hApp multiple times under different agentPubKeys (AgentPubKey & DnaHash combo must be unique, this is called the cellId).
                    AdminAgentPubKeyGeneratedCallBackEventArgs agentPubKeyResult = await _holoNETClientAdmin.AdminGenerateAgentPubKeyAsync();

                    if (agentPubKeyResult != null && !agentPubKeyResult.IsError)
                    {
                        LogMessage($"ADMIN: AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}");
                        ShowStatusMessage($"AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}", StatusMessageType.Success, false, ucHoloNETEntry);

                        LogMessage($"ADMIN: Installing hApp {InputTextBox.Text} ({_installinghAppPath})...");
                        ShowStatusMessage($"Installing hApp {InputTextBox.Text}...", StatusMessageType.Information, true);

                        //We can use async or non-async versions for every function.
                        _holoNETClientAdmin.AdminInstallApp(_holoNETClientAdmin.HoloNETDNA.AgentPubKey, InputTextBox.Text, _installinghAppPath);
                    }
                });
            }
        }

        private void btnPopupInstallhAppCancel_Click(object sender, RoutedEventArgs e)
        {
            popupInstallhApp.Visibility = Visibility.Collapsed;
            lblInstallAppErrorMessage.Visibility = Visibility.Collapsed;
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
            ShowHoloNETEntrySharedTab();
            ucHoloNETEntryShared.HideStatusMessage();
            ucHoloNETCollectionEntry.HideStatusMessage();
            
            popupDataEntries.Visibility = Visibility.Visible;
            ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                LoadHoloNETEntryShared(result.AppAgentClient);
            else
                _clientOperation = ClientOperation.LoadHoloNETEntryShared;
        }
        

        private void HoloNETEntriesShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Collection Error", StatusMessageType.Error);
            LogMessage($"APP: HoloNET Collection Error: {e.Reason}");
        }

        private void HoloNETEntriesShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Closed", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Collection Closed.");
            }
        }

        private void HoloNETEntriesShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Initialized", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Collection Initialized.");
            }
        }

        private void HoloNETEntriesShared_OnHoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Data Entry Removed From Collection", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Data Entry Removed From Collection: {GetEntryInfo(e)}");
            }
        }

        private void HoloNETEntriesShared_OnHoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Data Entry Added To Collection", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Data Entry Added To Collection: {GetEntryInfo(e)}");
            }
        }

        private void HoloNETEntriesShared_OnCollectionSaved(object sender, HoloNETCollectionSavedResult e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Data Entries Updated (Collection Updated)", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Data Entries Updated (Collection Updated).");
            }
        }

        private void HoloNETEntriesShared_OnCollectionLoaded(object sender, HoloNETCollectionLoadedResult<AvatarShared> e)
        {
            if (!e.IsError)
            {
                ShowStatusMessage($"HoloNET Collection Loaded", StatusMessageType.Success);
                LogMessage($"APP: HoloNET Collection Loaded: {GetEntryInfo(e.ZomeFunctionCallBackEventArgs)}");
            }
        }

        private void btnDataEntriesPopupAddEntryToCollection_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETCollectionEntry.Validate())
            {
                //lblNewEntryValidationErrors.Visibility = Visibility.Hidden;
                ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                {
                    AddHoloNETEntryToCollection(result.AppAgentClient, ucHoloNETCollectionEntry.FirstName, ucHoloNETCollectionEntry.LastName, ucHoloNETCollectionEntry.DOBDateTime, ucHoloNETCollectionEntry.Email);

                    //We could alternatively save the entry first and then add it to the collection afterwards but this is 2 round trips to the conductor whereas AddHoloNETEntryToCollectionAndSave is only 1 and will automatically save the entry before adding it to the collection.
                    //SaveHoloNETEntry(result.AppAgentClient, txtFirstName.Text, txtLastName.Text, txtDOB.Text, txtEmail.Text);
                }
                else
                    _clientOperation = ClientOperation.AddHoloNETEntryToCollection;  
            }
        }

        private void _holoNETEntryShared_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Error", StatusMessageType.Error);
            LogMessage($"APP: HoloNET Data Entry (Shared) Error: {e.Reason}");
        }

        private void _holoNETEntryShared_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Deleted", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Saved", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Saved: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Closed", StatusMessageType.Success);

            if (e.HolochainConductorsShutdownEventArgs != null)
                LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
            else
                LogMessage($"APP: HoloNET Data Entry (Shared) Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
        }

        private void _holoNETEntryShared_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Loaded", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Loaded: {GetEntryInfo(e)}");
        }

        private void _holoNETEntryShared_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry (Shared) Initialized", StatusMessageType.Success);
            LogMessage($"APP: HoloNET Data Entry (Shared) Initialized.");
        }

        private void _holoNETEntry_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            //For non async Save method you would use this callback but with async you can handle it in-line directly after the call to SaveAsync as we do in the btnHoloNETEntryPopupSave_Click event handler.
            //HandleHoloNETEntrySaved(e);
        }

        private void _holoNETEntry_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Loaded", StatusMessageType.Success, false, ucHoloNETEntry);
            LogMessage($"APP: HoloNET Data Entry Loaded: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Initialized", StatusMessageType.Success, false, ucHoloNETEntry);
            LogMessage($"APP: HoloNET Data Entry Initialized: AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}");
        }

        private void _holoNETEntry_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Error", StatusMessageType.Error, false, ucHoloNETEntry);
            LogMessage($"APP: HoloNET Data Entry Error: {e.Reason}");
        }

        private void _holoNETEntry_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            //For non async Delete method you would use this callback but with async you can handle it in-line directly after the call to DeleteAsync as we do in the btnHoloNETEntryPopupDelete_Click event handler.
            //ShowStatusMessage($"APP: HoloNET Data Entry Deleted", StatusMessageType.Success);
            //LogMessage($"APP: HoloNET Data Entry Deleted: {GetEntryInfo(e)}");
        }

        private void _holoNETEntry_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            ShowStatusMessage($"HoloNET Data Entry Closed", StatusMessageType.Success, false, ucHoloNETEntry);
            //LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs != null ? e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown : 0}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");

            if (e.HolochainConductorsShutdownEventArgs != null)
                LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, Number Of Hc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, Number Of Rustc Exe Instances Shutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");
            else
                LogMessage($"APP: HoloNET Data Entry Closed: Number Of Holochain Exe Instances Shutdown: 0, Number Of Hc Exe Instances Shutdown: 0, Number Of Rustc Exe Instances Shutdown: 0");
        }

        private void btnDataEntriesPopupClose_Click(object sender, RoutedEventArgs e)
        {
            //lblNewEntryValidationErrors.Visibility = Visibility.Hidden;
            ucHoloNETCollectionEntry.HideStatusMessage();
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
                //If it is the HoloNETEntry (uses the oasis-app) then get the internal connection from the HoloNET Entry.
                if (app.Name == _holoNETEntryDemoAppId && _holoNETEntry != null && _holoNETEntry.HoloNETClient != null)
                {
                    Dispatcher.InvokeAsync(async () =>
                    {
                        //We can either use async or non-async version.
                        await _holoNETEntry.HoloNETClient.DisconnectAsync();

                        //The async version will wait till it has disconnected so we can refresh the hApp list now without needing to use the OnDisconencted callback.
                        ListHapps();
                    });
                }

                //If it is the HoloNET Collection (uses the oasis-app) then get the internal connection from the HoloNET Collection.
                if (app.Name == _holoNETCollectionDemoAppId && HoloNETEntries != null && HoloNETEntries.HoloNETClient != null)
                {
                    Dispatcher.InvokeAsync(async () =>
                    {
                        //We can either use async or non-async version.
                        await HoloNETEntries.HoloNETClient.DisconnectAsync();

                        //The async version will wait till it has disconnected so we can refresh the hApp list now without needing to use the OnDisconencted callback.
                        ListHapps();
                    });
                }

                //Get the client from the client pool (shared connections).
                HoloNETClient client = GetClient(app.DnaHash, app.AgentPubKey, app.Name);

                if (client != null && client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    Button btnDisconnectClient = sender as Button;

                    if (btnDisconnectClient != null)
                        btnDisconnectClient.IsEnabled = false;

                    _removeClientConnectionFromPoolAfterDisconnect = false;
                    client.Disconnect(); //Non async version (will use OnDisconnected callback to refresh the hApp list (will call ListHapps).
                }
            }
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

        private string GetHoloNETEntryMetaData(HoloNETAuditEntryBase entry)
        {
            return $"EntryHash: {_holoNETEntry.EntryHash}, Created By: {_holoNETEntry.CreatedBy}, Created Date: {_holoNETEntry.CreatedDate}, Modified By: {_holoNETEntry.ModifiedBy}, Modified Date: {_holoNETEntry.ModifiedDate}, Version: {_holoNETEntry.Version}, Previous Version Hash: {_holoNETEntry.PreviousVersionEntryHash}, Id: {_holoNETEntry.Id}, IsActive: {_holoNETEntry.IsActive}, Action Sequence: {_holoNETEntry.EntryData.ActionSequence}, EntryType: {_holoNETEntry.EntryData.EntryType}, Hash: {_holoNETEntry.EntryData.Hash}, OriginalActionAddress: {_holoNETEntry.EntryData.OriginalActionAddress}, OriginalEntryAddress: {_holoNETEntry.EntryData.OriginalEntryAddress}, Signature: {_holoNETEntry.EntryData.Signature}, TimeStamp: {_holoNETEntry.EntryData.Timestamp}, Type: {_holoNETEntry.EntryData.Type}";
        }

        private void btnHoloNETEntryPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            ucHoloNETEntry.HideStatusMessage();
            popupHoloNETEntry.Visibility = Visibility.Hidden;
        }

        private void btnHoloNETEntrySharedPopupSave_Click(object sender, RoutedEventArgs e)
        {
            if (ucHoloNETEntryShared.Validate())
            {
                ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

                if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    SaveHoloNETEntryShared(result.AppAgentClient, ucHoloNETEntryShared.FirstName, ucHoloNETEntryShared.LastName, ucHoloNETEntryShared.DOBDateTime, ucHoloNETEntryShared.Email);
                else
                    _clientOperation = ClientOperation.SaveHoloNETEntryShared;
            }
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

        private void btnShowHoloNETCollection_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETCollectionSharedTab();
        }

        private void btnShowHoloNETEntryShared_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntrySharedTab();
        }

        private void btnHoloNETEntrySharedInfo_Click(object sender, RoutedEventArgs e)
        {
            ShowHoloNETEntrySharedInfoTab();
        }

        private void ShowHoloNETEntrySharedTab()
        {
            btnShowHoloNETCollection.IsEnabled = true;
            btnShowHoloNETEntryShared.IsEnabled = false;
            btnHoloNETEntrySharedInfo.IsEnabled = true;
            stkpnlHoloNETEntryShared.Visibility = Visibility.Visible;
            stkpnlHoloNETCollection.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETEntryShared.Visibility = Visibility.Collapsed;
            btnHoloNETEntrySharedPopupSave.Visibility = Visibility.Visible;
            btnDataEntriesPopupAddEntryToCollection.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupRemoveEntryFromCollection.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupUpdateEntryInCollection.Visibility = Visibility.Collapsed;
        }

        private void ShowHoloNETCollectionSharedTab()
        {
            btnShowHoloNETCollection.IsEnabled = false;
            btnShowHoloNETEntryShared.IsEnabled = true;
            btnHoloNETEntrySharedInfo.IsEnabled = true;
            stkpnlHoloNETEntryShared.Visibility = Visibility.Collapsed;
            stkpnlHoloNETCollection.Visibility = Visibility.Visible;
            stkpnlInfoHoloNETEntryShared.Visibility = Visibility.Collapsed;
            btnHoloNETEntrySharedPopupSave.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupAddEntryToCollection.Visibility = Visibility.Visible;
            btnDataEntriesPopupRemoveEntryFromCollection.Visibility = Visibility.Visible;
            btnDataEntriesPopupUpdateEntryInCollection.Visibility = Visibility.Visible;

            ConnectToAppAgentClientResult result = ConnectToAppAgentClient();

            if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                LoadCollection(result.AppAgentClient);
            else
                _clientOperation = ClientOperation.LoadHoloNETCollection;
        }

        private void ShowHoloNETEntrySharedInfoTab()
        {
            btnShowHoloNETCollection.IsEnabled = true;
            btnShowHoloNETEntryShared.IsEnabled = true;
            btnHoloNETEntrySharedInfo.IsEnabled = false;
            stkpnlHoloNETEntryShared.Visibility = Visibility.Collapsed;
            stkpnlHoloNETCollection.Visibility = Visibility.Collapsed;
            stkpnlInfoHoloNETEntryShared.Visibility = Visibility.Visible;
            btnHoloNETEntrySharedPopupSave.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupAddEntryToCollection.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupRemoveEntryFromCollection.Visibility = Visibility.Collapsed;
            btnDataEntriesPopupUpdateEntryInCollection.Visibility = Visibility.Collapsed;
        }

        private void btnDataEntriesPopupUpdateEntryInCollection_Click(object sender, RoutedEventArgs e)
        {
            //AvatarShared avatar = gridDataEntries.SelectedItem as AvatarShared;

            if (_currentAvatar != null)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                    btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;

                    _currentAvatar.FirstName = ucHoloNETCollectionEntry.FirstName;
                    _currentAvatar.LastName = ucHoloNETCollectionEntry.LastName;
                    _currentAvatar.Email = ucHoloNETCollectionEntry.Email;
                    _currentAvatar.DOB = ucHoloNETCollectionEntry.DOBDateTime;

                    ShowStatusMessage($"Updating HoloNETEntry In Collection...", StatusMessageType.Information, true);
                    LogMessage($"APP: Updating HoloNETEntry In Collection...");

                    HoloNETCollectionSavedResult result = await HoloNETEntriesShared[CurrentApp.Name].SaveAllChangesAsync();

                    if (result != null && !result.IsError)
                    {
                        ShowStatusMessage($"HoloNETEntry Updated In Collection.", StatusMessageType.Success, false);
                        LogMessage($"APP: HoloNETEntry Updated In Collection.");

                        gridDataEntries.ItemsSource = null;
                        gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";
                    }
                    else
                    {
                        ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        ShowStatusMessage($"Error Occured Updating HoloNETEntry In Collection: {result.Message}", StatusMessageType.Error);
                        LogMessage($"APP: Error Occured Updating HoloNETEntry In Collection: {result.Message}");

                        //TODO:TEMP, REMOVE AFTER!
                        gridDataEntries.ItemsSource = null;
                        gridDataEntries.ItemsSource = HoloNETEntriesShared[CurrentApp.Name];

                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";
                    }
                }); 
            }
        }

        private void btnDataEntriesPopupRemoveEntryFromCollection_Click(object sender, RoutedEventArgs e)
        {
            AvatarShared avatar = gridDataEntries.SelectedItem as AvatarShared;

            if (avatar != null)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                    btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;

                    ShowStatusMessage($"Removing HoloNETEntry From Collection...", StatusMessageType.Information, true);
                    LogMessage($"APP: Removing HoloNETEntry From Collection...");

                    ZomeFunctionCallBackEventArgs result = await HoloNETEntriesShared[CurrentApp.Name].RemoveHoloNETEntryFromCollectionAndSaveAsync(avatar);

                    if (result != null && !result.IsError)
                    {
                        ShowStatusMessage($"HoloNETEntry Removed From Collection.", StatusMessageType.Success, false);
                        LogMessage($"APP: HoloNETEntry Removed From Collection.");

                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";

                        //Remove the item from the list (we could re-load the list from hc here but it is more efficient to just remove it from the in-memory collection).
                        //int index = -1;
                        //for (int i = 0; i < HoloNETEntries[CurrentApp.Name].Count; i++)
                        //{
                        //    if (HoloNETEntries[CurrentApp.Name][i] != null && HoloNETEntries[CurrentApp.Name][i].Id == avatar.Id)
                        //    {
                        //        index = i;
                        //        break;
                        //    }
                        //}

                        //if (index > -1)
                        //{
                        //    HoloNETEntries[CurrentApp.Name].RemoveAt(index);
                        //    gridDataEntries.ItemsSource = null;
                        //    gridDataEntries.ItemsSource = HoloNETEntries[CurrentApp.Name];
                        //    btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = false;
                        //    btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
                        //}
                    }
                    else
                    {
                        ucHoloNETCollectionEntry.FirstName = "";
                        ucHoloNETCollectionEntry.LastName = "";
                        ucHoloNETCollectionEntry.DOB = "";
                        ucHoloNETCollectionEntry.Email = "";

                        ucHoloNETCollectionEntry.ShowStatusMessage(result.Message, StatusMessageType.Error);
                        ShowStatusMessage($"Error Occured Removing HoloNETEntry From Collection: {result.Message}", StatusMessageType.Error);
                        LogMessage($"APP: Error Occured Removing HoloNETEntry From Collection: {result.Message}");
                    }
                });
            }
        }

        private void gridDataEntries_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            _currentAvatar = gridDataEntries.SelectedItem as AvatarShared;

            if (_currentAvatar != null)
            {
                btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = true;
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;

                ucHoloNETCollectionEntry.FirstName = _currentAvatar.FirstName;
                ucHoloNETCollectionEntry.LastName = _currentAvatar.LastName;
                ucHoloNETCollectionEntry.DOB = _currentAvatar.DOB.ToShortDateString();
                ucHoloNETCollectionEntry.Email = _currentAvatar.Email;
            }
            else
            {
                btnDataEntriesPopupRemoveEntryFromCollection.IsEnabled = false;
                btnDataEntriesPopupUpdateEntryInCollection.IsEnabled = false;
            }
        }

        //private void gridDataEntries_CurrentCellChanged(object sender, EventArgs e)
        //{
        //    AvatarShared avatar = gridDataEntries.SelectedItem as AvatarShared;
        //}

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

        private void CheckForChangesInHoloNETCollection()
        {
            if (HoloNETEntries != null && HoloNETEntries.IsChanges)
                btnHoloNETCollectionPopupSaveChanges.IsEnabled = true;
            else
                btnHoloNETCollectionPopupSaveChanges.IsEnabled = false;
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
    }
}