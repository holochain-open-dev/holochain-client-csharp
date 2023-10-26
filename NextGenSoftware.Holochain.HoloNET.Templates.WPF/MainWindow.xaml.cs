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
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.HoloNETDNA;
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
        private const string _oasisHappPath = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _role_name = "oasis";
        private const string _installed_app_id = "oasis-app7777";
        private HoloNETClient? _holoNETClientAdmin;
        //private HoloNETClient _holoNETClientApp;
        private List<HoloNETClient> _holoNETappClients = new List<HoloNETClient>();
        private bool _rebooting = false;
        private bool _adminDisconnected = false;
        private bool _appDisconnected = false;
        private bool _adminConnected = false;
        private bool _doDemo = false;
        private string _installinghAppName = "";
        private string _installinghAppPath = "";
        private AdminOperation _adminOperation;
        private byte[][] _installingAppCellId = null;
        dynamic paramsObject = null;
        InstalledApp _attachingApp = null;

        public ObservableCollection<InstalledApp> InstalledApps { get; set; } = new ObservableCollection<InstalledApp>();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HoloNETDNAManager.LoadDNA();

            if (HoloNETDNAManager.HoloNETDNA != null)
                txtAdminURI.Text = HoloNETDNAManager.HoloNETDNA.HcAdminURI;

            //if (VisualTreeHelper.GetChildrenCount(lstOutput) > 0)
            //{
            //    Border border = (Border)VisualTreeHelper.GetChild(lstOutput, 0);
            //    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);


            //    ScrollBar scrollbar = (ScrollBar)VisualTreeHelper.GetChild(scrollViewer, 0);
            //    scrollbar.Background = Brushes.Blue;

            //}


            //scrollbar.Foreground = Brushes.Blue;

            //gridHapps.Resources["columnDisabledForeground"] = Brushes.Black;

            //Init();
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_holoNETClientAdmin != null)
            {
                if (_holoNETClientAdmin.State == System.Net.WebSockets.WebSocketState.Open)
                    _holoNETClientAdmin.Disconnect();

                _holoNETClientAdmin = null;
            }

            foreach (HoloNETClient client in _holoNETappClients)
            {
                if (client.State == System.Net.WebSockets.WebSocketState.Open)
                    client.Disconnect();

                //client = null;
            }
        }

        private void Init()
        {
            LogMessage($"ADMIN: Connecting to Admin WebSocket on endpoint: {txtAdminURI.Text}...");
            ShowStatusMessage($"Admin WebSocket Connecting To Endpoint {txtAdminURI.Text}...", StatusMessageType.Information, true);

            _holoNETClientAdmin = new HoloNETClient(txtAdminURI.Text);
            _holoNETClientAdmin.Config.AutoStartHolochainConductor = false;
            _holoNETClientAdmin.Config.ShowHolochainConductorWindow = true;
            _holoNETClientAdmin.Config.AutoShutdownHolochainConductor = false;
            _holoNETClientAdmin.Config.FullPathToRootHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ";
            _holoNETClientAdmin.Config.FullPathToCompiledHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ";
            // _holoNETClientAdmin.Config.LoggingMode = Logging.LoggingMode.WarningsErrorsInfoAndDebug;

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

            _adminOperation = AdminOperation.ListHapps;
            _holoNETClientAdmin.ConnectAdmin();
        }

        private void _holoNETClientAdmin_OnAdminAppDisabledCallBack(object sender, AdminAppDisabledCallBackEventArgs e)
        {
            LogMessage("ADMIN: App Disabled."); //TODO: ADD INSTALL APP ID TO RETURN ARGS TOMORROW! ;-) same with uninstall etc...
            ShowStatusMessage("App Disabled.", StatusMessageType.Success);
            ListHapps();
        }

        private void _holoNETClientAdmin_OnAdminAppUninstalledCallBack(object sender, AdminAppUninstalledCallBackEventArgs e)
        {
            LogMessage($"ADMIN: App Uninstalled.");
            ShowStatusMessage("App Uninstalled.", StatusMessageType.Success);
            ListHapps();
        }

        private void _holoNETClientAdmin_OnAdminAppsListedCallBack(object sender, AdminAppsListedCallBackEventArgs e)
        {
            //lstHapps.Items.Clear();
            string hApps = "";
            InstalledApps.Clear();

            foreach (AppInfo app in e.Apps)
            {
                InstalledApps.Add(new InstalledApp()
                {
                     AgentPubKey = app.AgentPubKey,
                     DnaHash = app.DnaHash,
                     Name = app.installed_app_id,
                     Manifest = $"{ app.manifest.name } v{ app.manifest.manifest_version } { (!string.IsNullOrEmpty(app.manifest.description) ? string.Concat('(', app.manifest.description, ')') : "")}",
                     Status = $"{Enum.GetName(typeof(AppInfoStatusEnum), app.AppStatus)}",
                     StatusReason = app.AppStatusReason,
                     IsEnabled = app.AppStatus == AppInfoStatusEnum.Disabled ? false : true,
                     IsDisabled = app.AppStatus == AppInfoStatusEnum.Disabled ? true : false
                });

                if (hApps != "")
                    hApps = $"{hApps}, ";

                hApps = $"{hApps}{app.installed_app_id}";
            }


            gridHapps.ItemsSource = InstalledApps.OrderBy(x => x.Name);
            LogMessage($"ADMIN: hApps Listed: {hApps}");
            ShowStatusMessage("hApps Listed.", StatusMessageType.Success);
        }

        private void _holoNETClient_OnAdminAppInterfaceAttachedCallBack(object sender, AdminAppInterfaceAttachedCallBackEventArgs e)
        {
            LogMessage($"ADMIN: App Interface Attached On Port {e.Port}.");
            ShowStatusMessage("App Interface Attached.");

            bool foundClient = false;
            foreach (HoloNETClient client in _holoNETappClients)
            {
                if (client != null && client.Config.DnaHash == _attachingApp.DnaHash && client.Config.AgentPubKey == _attachingApp.AgentPubKey && client.Config.InstalledAppId == _attachingApp.Name)
                {
                    LogMessage($"APP: Found Exsting Existing HoloNETClient AppAgent WebSocket For AgentPubKey {client.Config.AgentPubKey}, DnaHash {client.Config.DnaHash} And InstalledAppId {client.Config.InstalledAppId} Running On Port {client.EndPoint.Port}.");

                    if (client.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port} Is Open.");

                        if (client.EndPoint.Port == e.Port)
                        {
                            LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket Connected On Port {client.EndPoint.Port} So Matched Admin Attached Port.");

                            if (paramsObject != null)
                            {
                                LogMessage("APP: Calling Zome Function...");
                                ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                                client.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, paramsObject);
                                paramsObject = null;
                                popupMakeZomeCall.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                LogMessage("ADMIN: Error: Zome paramsObject is null! Please try again");
                                ShowStatusMessage("Error: Zome paramsObject is null! Please try again", StatusMessageType.Error);
                            }
                        }
                        else
                        {
                            LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket Connected On Port {client.EndPoint.Port} but ADMIN Attached To Port {e.Port} So Need To Re-Connect On The New Port...");
                            LogMessage($"APP: Disconnecting From HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port}...");
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
                LogMessage($"APP: No Existing HoloNETClient AppAgent WebSocket Found Running For AgentPubKey {_attachingApp.AgentPubKey}, DnaHash {_attachingApp.DnaHash} And InstalledAppId {_attachingApp.Name} So Looking For Client To Recycle...");

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
                LogMessage($"APP: No Existing HoloNETClient AppAgent WebSocket Found Running For AgentPubKey {_attachingApp.AgentPubKey}, DnaHash {_attachingApp.DnaHash} And InstalledAppId {_attachingApp.Name} So Creating New HoloNETClient AppAgent WebSocket Now...");

                HoloNETClient newClient = new HoloNETClient($"ws://127.0.0.1:{e.Port}");
                newClient.OnConnected += _holoNETClientApp_OnConnected;
                newClient.OnReadyForZomeCalls += _holoNETClientApp_OnReadyForZomeCalls;
                newClient.OnZomeFunctionCallBack += _holoNETClientApp_OnZomeFunctionCallBack;
                newClient.OnDisconnected += _holoNETClientApp_OnDisconnected;
                newClient.OnDataReceived += _holoNETClientApp_OnDataReceived;
                newClient.OnDataSent += _holoNETClientApp_OnDataSent;
                newClient.OnError += _holoNETClientApp_OnError;

                newClient.Config.AutoStartHolochainConductor = false;
                newClient.Config.AutoShutdownHolochainConductor = false;

                newClient.Config.AgentPubKey = _attachingApp.AgentPubKey;
                newClient.Config.DnaHash = _attachingApp.DnaHash;
                newClient.Config.InstalledAppId = _attachingApp.Name;

                LogMessage($"APP: New HoloNETClient AppAgent WebSocket Created.");
                ShowStatusMessage($"Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...", StatusMessageType.Information, true);
                LogMessage($"APP: Connecting To HoloNETClient AppAgent WebSocket On Port {e.Port}...");
                newClient.Connect();

                _holoNETappClients.Add(newClient);
                txtConnections.Text = $"Client Connections: {_holoNETappClients.Count}";
                sbAnimateConnections.Begin();
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

            _appDisconnected = true;

            if (_rebooting && _adminDisconnected && _appDisconnected)
                Init();
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

            HoloNETClient client = GetClient(_attachingApp.DnaHash, _attachingApp.AgentPubKey, _attachingApp.Name);

            if (client != null)
            {
                if (_doDemo)
                {
                    LogMessage("APP: Calling Zome Function...");
                    ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);
                    client.CallZomeFunction("oasis", "create_avatar", null);
                }
                else if (paramsObject != null)
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
            return _holoNETappClients.FirstOrDefault(x => x.Config.DnaHash == dnaHash && x.Config.AgentPubKey == agentPubKey && x.Config.InstalledAppId == installedAppId);
        }

        private void _holoNETClientApp_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            LogMessage("APP: Connected.");
            ShowStatusMessage("App WebSocket Connected.", StatusMessageType.Success);
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
            _installingAppCellId = null;
            LogMessage($"ADMIN: Zome Call Capability Granted (Signing Credentials Authorized).");
            ShowStatusMessage($"Zome Call Capability Granted (Signing Credentials Authorized).", StatusMessageType.Success);
            
            LogMessage("ADMIN: Attaching App Interface...");
            ShowStatusMessage($"Attaching App Interface...", StatusMessageType.Information, true);

            //_holoNETClientAdmin.AdminAttachAppInterface(65002);
            _holoNETClientAdmin.AdminAttachAppInterface();
        }

        private void _holoNETClient_OnAdminAppEnabledCallBack(object sender, AdminAppEnabledCallBackEventArgs e)
        {
            LogMessage($"ADMIN: hApp {e.InstalledAppId} Enabled."); //TODO: FIX InstalledAppId BEING NULL TOMORROW! ;-)
            ShowStatusMessage($"hApp {e.InstalledAppId} Enabled.", StatusMessageType.Success);

            ListHapps();

            //if (_installingAppCellId != null)
            //{
            //    LogMessage($"ADMIN: Authorize Signing Credentials For {e.InstalledAppId} hApp...");
            //    ShowStatusMessage($"Authorize Signing Credentials For {e.InstalledAppId} hApp...", StatusMessageType.Information, true);

            //    //_holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(GrantedFunctionsType.Listed, new List<(string, string)>()
            //    //{
            //    //    ("oasis", "create_avatar"),
            //    //    ("oasis", "get_avatar"),
            //    //    ("oasis", "update_avatar")
            //    //});

            //    _holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(_installingAppCellId, GrantedFunctionsType.All, null);
            //}
        }

        private void _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack(object sender, AdminAgentPubKeyGeneratedCallBackEventArgs e)
        {
            LogMessage($"ADMIN: AgentPubKey Generated for EndPoint: {e.EndPoint} and Id: {e.Id}: AgentPubKey: {e.AgentPubKey}");
            ShowStatusMessage($"AgentPubKey Generated.", StatusMessageType.Success);

            _adminConnected = true;

            switch (_adminOperation)
            {
                case AdminOperation.InstallHapp:
                {
                        LogMessage($"ADMIN: Installing {_installinghAppName} hApp ({_installinghAppPath})...");
                        ShowStatusMessage($"Installing {_installinghAppName} hApp...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminInstallApp(e.AgentPubKey, _installinghAppName, _installinghAppPath);
                }
                break;

                case AdminOperation.ListHapps:
                {
                        LogMessage($"ADMIN: Listing hApps...");
                        ShowStatusMessage($"Listing hApps...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
                }
                break;

                case AdminOperation.ListDNAs:
                {
                        LogMessage($"ADMIN: Listing DNAs...");
                        ShowStatusMessage($"Listing DNAs...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminListDnas();
                }
                break;

                case AdminOperation.ListCellIds:
                {
                        LogMessage($"ADMIN: Listing Cell Ids...");
                        ShowStatusMessage($"Listing Cell Ids...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminListCellIds();
                }
                break;

                case AdminOperation.ListAttachedInterfaces:
                {
                        LogMessage("ADMIN: Listning Attached Interfaces...");
                        ShowStatusMessage($"Listing Attached Interfaces...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminListInterfaces();
                }
                break;   
            }
        }

        private void HoloNETClient_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            LogMessage("ADMIN: Disconnected");
            ShowStatusMessage($"Admin WebSocket Disconnected.");

            _adminDisconnected = true;
            _adminConnected = false;

            if (_rebooting && _adminDisconnected && _appDisconnected)
            {
                _doDemo = true;
                Init();
            }
        }

        private void HoloNETClient_OnAdminAppInstalledCallBack(object sender, AdminAppInstalledCallBackEventArgs e)
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

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            lstOutput.Items.Clear();
        }

        private void btnReboot_Click(object sender, RoutedEventArgs e)
        {
            //LogMessage("Rebooting...");
            //ShowStatusMessage($"Rebooting...");

            //if (_holoNETClientApp != null && _holoNETClientApp.State == System.Net.WebSockets.WebSocketState.Open)
            //{
            //    LogMessage("APP: Disconnecting...");
            //    ShowStatusMessage($"App WebSocket Disconnecting...", StatusMessageType.Information, true);

            //    _appDisconnected = false;
            //    _holoNETClientApp.Disconnect();
            //}
            //else
            //    _appDisconnected = true;

            //LogMessage("ADMIN: Disconnecting...");
            //ShowStatusMessage($"Admin WebSocket Disconnecting...", StatusMessageType.Information, true);

            //_rebooting = true;
            //_adminDisconnected = false;
            //_holoNETClientAdmin.Disconnect();
        }

        private void btnShowLog_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("notepad.exe", "Logs\\HoloNET.log");
        }

        private void btnStartDemo_Click(object sender, RoutedEventArgs e)
        {
            btnStartDemo.IsEnabled = false;
            btnStartDemo.Foreground = new SolidColorBrush(Colors.DarkGray);
            btnReboot.IsEnabled = true;
            btnReboot.Foreground = new SolidColorBrush(Colors.White);

            _doDemo = true;
            //Init();

            LogMessage("ADMIN: Installing OASIS Demo hApp...");
            ShowStatusMessage($"Installing OASIS Demo hApp...", StatusMessageType.Information, true);

            _holoNETClientAdmin.AdminInstallApp(_holoNETClientAdmin.Config.AgentPubKey, _installed_app_id, _oasisHappPath);
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
            _doDemo = false;

            if (_adminConnected)
            {
                LogMessage("ADMIN: Listning DNAs...");
                ShowStatusMessage($"Listning DNAs...");
                _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
            }
            else
            {
                _adminOperation = AdminOperation.ListDNAs;
                Init();
            }
        }

        private void btnListCellIds_Click(object sender, RoutedEventArgs e)
        {
            _doDemo = false;

            if (_adminConnected)
            {
                LogMessage("ADMIN: Listning Cell Ids...");
                ShowStatusMessage($"Listning Cell Ids...", StatusMessageType.Information, true);
                _holoNETClientAdmin.AdminListCellIds();
            }
            else
            {
                _adminOperation = AdminOperation.ListCellIds;
                Init();
            }
        }

        private void btnListAttachedInterfaces_Click(object sender, RoutedEventArgs e)
        {
            _doDemo = false;

            if (_adminConnected)
            {
                LogMessage("ADMIN: Listning Attached Interfaces...");
                ShowStatusMessage($"Listning Attached Interfaces...", StatusMessageType.Information, true);
                _holoNETClientAdmin.AdminListInterfaces();
            }
            else
            {
                _adminOperation = AdminOperation.ListAttachedInterfaces;
                Init();
            }
        }

        private void ListHapps()
        {
            _doDemo = false;

            if (_adminConnected)
            {
                LogMessage("ADMIN: Listning hApps...");
                ShowStatusMessage($"Listning hApps...", StatusMessageType.Information, true);
                _holoNETClientAdmin.AdminListApps(AppStatusFilter.All);
            }
            else
            {
                _adminOperation = AdminOperation.ListHapps;
                Init();
            }
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
                    txtStatus.Foreground= Brushes.LightSalmon;
                    break;
            }

            if (showSpinner)
                spinner.Visibility = Visibility.Visible;
            else
                spinner.Visibility = Visibility.Hidden;

            //lblStatus.SizeChanged

            sbAnimateStatus.Begin();
        }

        private void LogMessage(string message)
        {
            lstOutput.Items.Add(message);
            //lstOutput.SelectedIndex = lstOutput.Items.Count - 1;
            //lstOutput.ScrollIntoView(lstOutput.SelectedItem);

            if (VisualTreeHelper.GetChildrenCount(lstOutput) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(lstOutput, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
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

                    InstalledApp app = gridHapps.SelectedItem as InstalledApp;

                    if (app != null)
                    {
                        HoloNETClient client = GetClient(app.DnaHash, app.AgentPubKey, app.Name);

                        //If we find an existing client then that means it has already been authorized, attached and connected.
                        if (client != null)
                        {
                            LogMessage($"APP: Found Exsting Existing HoloNETClient AppAgent WebSocket For AgentPubKey {client.Config.AgentPubKey}, DnaHash {client.Config.DnaHash} And InstalledAppId {client.Config.InstalledAppId} Running On Port {client.EndPoint.Port}.");
                            
                            if (client.State == System.Net.WebSockets.WebSocketState.Open)
                            {
                                LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port} Is Open.");
                                LogMessage("APP: Calling Zome Function...");
                                ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                                client.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, paramsObject);
                                paramsObject = null;
                                popupMakeZomeCall.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                LogMessage($"APP: Existing HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port} Is NOT Open!");
                                ShowStatusMessage($"Re-Connecting To HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port}...", StatusMessageType.Information, true);
                                LogMessage($"APP: Re-Connecting To HoloNETClient AppAgent WebSocket On Port {client.EndPoint.Port}...");
                                client.Connect();
                            }
                        }
                        else
                        {
                            LogMessage($"ADMIN: No Existing HoloNETClient AppAgent WebSocket Found For AgentPubKey {app.AgentPubKey}, DnaHash {app.DnaHash} And InstalledAppId {app.Name} So New Connection Needs To Be Made...");
                            LogMessage($"ADMIN: Authorizing Signing Credentials For {app.Name} hApp...");
                            ShowStatusMessage($"Authorizing Signing Credentials For {app.Name} hApp...", StatusMessageType.Information, true);

                            //_holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(GrantedFunctionsType.Listed, new List<(string, string)>()
                            //{
                            //    ("oasis", "create_avatar"),
                            //    ("oasis", "get_avatar"),
                            //    ("oasis", "update_avatar")
                            //});

                            _attachingApp = app;
                            _holoNETClientAdmin.AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability(_holoNETClientAdmin.GetCellId(app.DnaHash, app.AgentPubKey), CapGrantAccessType.Unrestricted, GrantedFunctionsType.All, null);
                        }
                    }
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
            if (string.IsNullOrEmpty(txtAdminURI.Text))
                lblAdminURIError.Visibility = Visibility.Visible;

            //else if (!Uri.IsWellFormedUriString(txtAdminURI.Text, UriKind.Absolute))
            //{
            //    //lblAdminURIError.Visibility = Visibility.Visible;
            //}

            else
            {
                lblAdminURIError.Visibility = Visibility.Collapsed;
                popupAdminURI.Visibility = Visibility.Collapsed;

                HoloNETDNAManager.HoloNETDNA.HcAdminURI = txtAdminURI.Text;
                HoloNETDNAManager.SaveDNA();

                Init();
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
                _doDemo = false;

                if (_adminConnected)
                {
                    LogMessage($"ADMIN: Installing hApp {InputTextBox.Text} ({_installinghAppPath})...");
                    ShowStatusMessage($"Installing hApp {InputTextBox.Text}...", StatusMessageType.Information, true);
                    _holoNETClientAdmin.AdminInstallApp(_holoNETClientAdmin.Config.AgentPubKey, InputTextBox.Text, _installinghAppPath);
                }
                else
                {
                    _installinghAppName = InputTextBox.Text;
                    Init();
                }
            }
        }

        private void btnPopupInstallhAppCancel_Click(object sender, RoutedEventArgs e)
        {
            popupInstallhApp.Visibility = Visibility.Collapsed;
            lblEnterName.Visibility = Visibility.Collapsed;
        }
    }
}