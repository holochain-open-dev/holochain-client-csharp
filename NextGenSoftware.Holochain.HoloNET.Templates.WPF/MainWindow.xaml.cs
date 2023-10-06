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
        private const string _hcAdminURI = "ws://localhost:60236";
        private const string _hcAppURI = "ws://localhost:8888";
        private const string _oasisHappPath = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _role_name = "oasis";
        private const string _installed_app_id = "oasis-app7";
        private HoloNETClient _holoNETClient = new HoloNETClient(_hcAdminURI);

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //HoloNETClient _holoNETClient = new HoloNETClient();
            //HoloNETClient _holoNETClient = new HoloNETClient(_hcAdminURI);
            _holoNETClient.Config.AutoStartHolochainConductor = false;
            _holoNETClient.Config.AutoShutdownHolochainConductor = false;

            _holoNETClient.OnConnected += HoloNETClient_OnConnected;
            _holoNETClient.OnReadyForZomeCalls += HoloNETClient_OnReadyForZomeCalls;
            _holoNETClient.OnError += HoloNETClient_OnError;
            _holoNETClient.OnDataReceived += HoloNETClient_OnDataReceived;
            _holoNETClient.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;
            _holoNETClient.OnDisconnected += HoloNETClient_OnDisconnected;
            _holoNETClient.OnAdminAgentPubKeyGeneratedCallBack += _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack;
            _holoNETClient.OnAdminAppInstalledCallBack += HoloNETClient_OnAdminAppInstalledCallBack;
            _holoNETClient.OnAdminAppEnabledCallBack += _holoNETClient_OnAdminAppEnabledCallBack;
            _holoNETClient.OnAdminSigningCredentialsAuthorized += _holoNETClient_OnAdminSigningCredentialsAuthorized;

            //_holoNETClient.Connect();

            lstOutput.Items.Add("ADMIN: Connecting...");
            _holoNETClient.ConnectAdmin();
        }

        private void _holoNETClient_OnAdminSigningCredentialsAuthorized(object sender, AdminSigningCredentialsAuthorizedEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN: Signing Credentials Autorized.");
        }

        private void _holoNETClient_OnAdminAppEnabledCallBack(object sender, AdminAppEnabledCallBackEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN: hApp Enabled.");
            lstOutput.Items.Add("ADMIN: Authorize Signing Credentials For hApp...");
            _holoNETClient.AdminAuthorizeSigningCredentials(GrantedFunctionsType.Listed, new List<(string, string)>()
            {
                ("oasis", "create_avatar"),
                ("oasis", "get_avatar"),
                ("oasis", "update_avatar")
            });
        }

        private void _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack(object sender, AdminAgentPubKeyGeneratedCallBackEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN: AgentPubKey Generated: EndPoint: {e.EndPoint}, Id: {e.Id},  AgentPubKey: {e.AgentPubKey}, IsError: {e.IsError}, Message: {e.Message}");
            lstOutput.Items.Add("ADMIN: Installing hApp...");
            _holoNETClient.AdminInstallApp(e.AgentPubKey, _installed_app_id, _oasisHappPath);
        }

        private void HoloNETClient_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            lstOutput.Items.Add("Disconnected");
        }

        private void HoloNETClient_OnAdminAppInstalledCallBack(object sender, AdminAppInstalledCallBackEventArgs e)
        {
            CellInfoType cellType = CellInfoType.None;

            if (e.AppInfoResponse != null && e.AppInfoResponse.data != null && e.AppInfoResponse.data.cell_info != null && e.AppInfoResponse.data.cell_info.ContainsKey(_role_name) && e.AppInfoResponse.data.cell_info[_role_name] != null && e.AppInfoResponse.data.cell_info[_role_name].Count > 0 && e.AppInfoResponse.data.cell_info[_role_name][0] != null)
                cellType = e.AppInfoResponse.data.cell_info[_role_name][0].CellInfoType;

            lstOutput.Items.Add($"ADMIN: hApp Installed: EndPoint: {e.EndPoint}, Id: {e.Id}, installed_app_id: {(e.AppInfoResponse.data != null ? e.AppInfoResponse.data.installed_app_id : "")}, AgentPubKey: {(e.AppInfoResponse.data != null ? e.AppInfoResponse.data.agent_pub_key : "")}, Manifest: {(e.AppInfoResponse.data != null ? e.AppInfoResponse.data.manifest.name : "")}, CellType: {Enum.GetName(typeof(CellInfoType), cellType)}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");

            if (cellType != CellInfoType.Provisioned)
                lstOutput.Items.Add("CellType is not Provisioned so aborting...");
            else
            {
                lstOutput.Items.Add("ADMIN: Enabling hApp...");
                _holoNETClient.AdminEnablelApp(_installed_app_id);
            }
        }

        private void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            lstOutput.Items.Add(string.Concat("Zome CallBack: ", ProcessZomeFunctionCallBackEventArgs(e)));
        }

        private void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            //lstOutput.Items.Add(string.Concat("Data Received: EndPoint: ", e.EndPoint, ", Raw JSON Data: ", e.RawJSONData, ", Raw Binary Data: ", e.RawBinaryData, ", Raw Binary Data After MessagePack Decode: ", e.RawBinaryDataAfterMessagePackDecode, ", Raw Binary Data After MessagePack Decode As String: ", e.RawBinaryDataAfterMessagePackDecodeAsString, ", IsError: ", e.IsError, ", Message:", e.Message));
            lstOutput.Items.Add(string.Concat("Data Received: EndPoint: ", e.EndPoint, ", Raw Binary Data: ", e.RawBinaryDataAsString, ", Raw Binary Data Decoded: ", e.RawBinaryDataDecoded, ", IsError: ", e.IsError, ", Message:", e.Message));
        }

        private void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            //lblOutput.Content = $"Error occured. Reason: {e.Reason}";
            lstOutput.Items.Add($"Error occured. Reason: {e.Reason}");
        }

        private void HoloNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            //lblOutput.Content = "Ready";
            lstOutput.Items.Add("Ready");
        }

        private void HoloNETClient_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            // lblOutput.Content = "Connected";
            lstOutput.Items.Add("Connected");
            lstOutput.Items.Add("ADMIN: Generating AgentPubKey hApp...");
            _holoNETClient.AdminGenerateAgentPubKey();
            
            //_holoNETClient.AdminInstallApp("oasis-app", _oasisHappPath);
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";

            //result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

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
    }
}
