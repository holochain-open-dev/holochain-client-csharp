using System;
using System.Collections.Generic;
using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _hcAdminURI = "ws://localhost:53888";
        private const string _hcAppURI = "ws://localhost:888888";
        private const string _oasisHappPath = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _role_name = "oasis";
        private const string _installed_app_id = "oasis-app777";
        private HoloNETClient _holoNETClientApp;
        private HoloNETClient _holoNETClientAdmin = new HoloNETClient(_hcAdminURI);
        private bool _rebooting = false;
        private bool _adminDisconnected = false;
        private bool _appDisconnected = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _holoNETClientAdmin.Config.AutoStartHolochainConductor = false;
            _holoNETClientAdmin.Config.AutoShutdownHolochainConductor = false;

            _holoNETClientAdmin.OnConnected += HoloNETClient_OnConnected;
            _holoNETClientAdmin.OnError += HoloNETClient_OnError;
            _holoNETClientAdmin.OnDataSent += _holoNETClient_OnDataSent;
            _holoNETClientAdmin.OnDataReceived += HoloNETClient_OnDataReceived;
            _holoNETClientAdmin.OnDisconnected += HoloNETClient_OnDisconnected;
            _holoNETClientAdmin.OnAdminAgentPubKeyGeneratedCallBack += _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack;
            _holoNETClientAdmin.OnAdminAppInstalledCallBack += HoloNETClient_OnAdminAppInstalledCallBack;
            _holoNETClientAdmin.OnAdminAppEnabledCallBack += _holoNETClient_OnAdminAppEnabledCallBack;
            _holoNETClientAdmin.OnAdminZomeCallCapabilityGrantedCallBack += _holoNETClient_OnAdminZomeCallCapabilityGrantedCallBack;
            _holoNETClientAdmin.OnAdminAppInterfaceAttachedCallBack += _holoNETClient_OnAdminAppInterfaceAttachedCallBack;

            Init();
        }

        private void _holoNETClient_OnAdminAppInterfaceAttachedCallBack(object sender, AdminAppInterfaceAttachedCallBackEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN App Interface Attached On Port {e.Port}.");
            lstOutput.Items.Add($"Connecting To AppAgent HoloNETClient On Port{e.Port}...");

            _holoNETClientApp = new HoloNETClient($"ws://127.0.0.1:{e.Port}");
            _holoNETClientApp.OnConnected += _holoNETClientApp_OnConnected;
            _holoNETClientApp.OnReadyForZomeCalls += _holoNETClientApp_OnReadyForZomeCalls;
            _holoNETClientApp.OnZomeFunctionCallBack += _holoNETClientApp_OnZomeFunctionCallBack;
            _holoNETClientApp.OnDisconnected += _holoNETClientApp_OnDisconnected;
            _holoNETClientApp.OnDataReceived += _holoNETClientApp_OnDataReceived;
            _holoNETClientApp.OnDataSent += _holoNETClientApp_OnDataSent;
            _holoNETClientApp.OnError += _holoNETClientApp_OnError;
        }

        private void _holoNETClientApp_OnError(object sender, HoloNETErrorEventArgs e)
        {
            lstOutput.Items.Add($"APP: Error occured. Reason: {e.Reason}");
        }

        private void _holoNETClientApp_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        {
            if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
                lstOutput.Items.Add($"APP: Data Sent to EndPoint {e.EndPoint}: {e.RawBinaryDataDecoded} ({e.RawBinaryDataAsString})");
        }

        private void _holoNETClientApp_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
                lstOutput.Items.Add(string.Concat("APP: Data Received for EndPoint: ", e.EndPoint, ": ", e.RawBinaryDataDecoded, "(", e.RawBinaryDataAsString, ")"));
        }

        private void _holoNETClientApp_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            lstOutput.Items.Add("APP: Disconnected");
            _appDisconnected = true;

            if (_rebooting && _adminDisconnected && _appDisconnected)
                Init();
        }

        private void _holoNETClientApp_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            lstOutput.Items.Add(string.Concat("APP: Zome CallBack: ", ProcessZomeFunctionCallBackEventArgs(e)));
        }

        private void _holoNETClientApp_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            lstOutput.Items.Add("APP: Ready For Zome Calls.");
            lstOutput.Items.Add("APP: Calling Zome Function...");

            _holoNETClientApp.CallZomeFunction("oasis", "create_avatar", null);
        }

        private void _holoNETClientApp_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            lstOutput.Items.Add("APP: Connected.");
        }

        private void Init()
        {
            lstOutput.Items.Add("Booting...");
            lstOutput.Items.Add("ADMIN: Connecting...");
            _holoNETClientAdmin.ConnectAdmin();
            //_holoNETClientAdmin.Connect();
        }

        private void _holoNETClient_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        {
            if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
                lstOutput.Items.Add($"ADMIN: Data Sent to EndPoint {e.EndPoint}: {e.RawBinaryDataDecoded} ({e.RawBinaryDataAsString})");
        }

        private void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (chkShowDetailedMessages.IsChecked.HasValue && chkShowDetailedMessages.IsChecked.Value)
                lstOutput.Items.Add(string.Concat("ADMIN: Data Received for EndPoint: ", e.EndPoint, ": ", e.RawBinaryDataDecoded, "(", e.RawBinaryDataAsString, ")"));
        }

        private void _holoNETClient_OnAdminZomeCallCapabilityGrantedCallBack(object sender, AdminZomeCallCapabilityGrantedCallBackEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN: Zome Call Capability Granted (Signing Credentials Authorized).");
            lstOutput.Items.Add("ADMIN: Attaching App Interface...");

            //_holoNETClientAdmin.AdminAttachAppInterface(65001);
            _holoNETClientAdmin.AdminAttachAppInterface();
        }

        private void _holoNETClient_OnAdminAppEnabledCallBack(object sender, AdminAppEnabledCallBackEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN: hApp Enabled.");
            lstOutput.Items.Add("ADMIN: Authorize Signing Credentials For hApp...");
            _holoNETClientAdmin.AdminAuthorizeSigningCredentialsForZomeCalls(GrantedFunctionsType.Listed, new List<(string, string)>()
            {
                ("oasis", "create_avatar"),
                ("oasis", "get_avatar"),
                ("oasis", "update_avatar")
            });

            //_holoNETClientAdmin.AdminAuthorizeSigningCredentials(GrantedFunctionsType.All, null);
        }

        private void _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack(object sender, AdminAgentPubKeyGeneratedCallBackEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN: AgentPubKey Generated for EndPoint: {e.EndPoint} and Id: {e.Id}: AgentPubKey: {e.AgentPubKey}");
            lstOutput.Items.Add("ADMIN: Installing hApp...");
            _holoNETClientAdmin.AdminInstallApp(e.AgentPubKey, _installed_app_id, _oasisHappPath);
        }

        private void HoloNETClient_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            lstOutput.Items.Add("ADMIN: Disconnected");
            _adminDisconnected = true;

            if (_rebooting && _adminDisconnected && _appDisconnected)
                Init();
        }

        private void HoloNETClient_OnAdminAppInstalledCallBack(object sender, AdminAppInstalledCallBackEventArgs e)
        {
            CellInfoType cellType = CellInfoType.None;

            if (e.IsCallSuccessful && !e.IsError)
            {
                if (e.AppInfoResponse != null && e.AppInfoResponse.data != null && e.AppInfoResponse.data.cell_info != null && e.AppInfoResponse.data.cell_info.ContainsKey(_role_name) && e.AppInfoResponse.data.cell_info[_role_name] != null && e.AppInfoResponse.data.cell_info[_role_name].Count > 0 && e.AppInfoResponse.data.cell_info[_role_name][0] != null)
                    cellType = e.AppInfoResponse.data.cell_info[_role_name][0].CellInfoType;

                lstOutput.Items.Add($"ADMIN: hApp Installed for EndPoint: {e.EndPoint} and Id: {e.Id}: installed_app_id: {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.installed_app_id : "")}, AgentPubKey: {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.agent_pub_key : "")}, Manifest: {(e.AppInfoResponse != null && e.AppInfoResponse.data != null ? e.AppInfoResponse.data.manifest.name : "")}, CellType: {Enum.GetName(typeof(CellInfoType), cellType)}");
            }
            else
                lstOutput.Items.Add($"ADMIN hApp NOT Installed! Reason: {e.Message}");

            if (cellType != CellInfoType.Provisioned)
                lstOutput.Items.Add("CellType is not Provisioned so aborting...");
            else
            {
                lstOutput.Items.Add("ADMIN: Enabling hApp...");
                _holoNETClientAdmin.AdminEnablelApp(_installed_app_id);
            }
        }

        private void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            //lblOutput.Content = $"Error occured. Reason: {e.Reason}";
            lstOutput.Items.Add($"ADMIN: Error occured. Reason: {e.Reason}");
        }


        private void HoloNETClient_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            lstOutput.Items.Add("ADMIN: Connected");
            lstOutput.Items.Add("ADMIN: Generating AgentPubKey For hApp...");
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
            lstOutput.Items.Add("Rebooting...");

            if (_holoNETClientApp != null && _holoNETClientApp.State == System.Net.WebSockets.WebSocketState.Open)
            {
                lstOutput.Items.Add("APP: Disconneting...");
                _appDisconnected = false;
                _holoNETClientApp.Disconnect();
            }
            else
                _appDisconnected = true;

            lstOutput.Items.Add("ADMIN: Disconneting...");
            _rebooting = true;
            _adminDisconnected = false;
            _holoNETClientAdmin.Disconnect();
        }
    }
}

