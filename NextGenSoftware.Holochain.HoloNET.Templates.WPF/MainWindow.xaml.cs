using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Client;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _hcAdminURI = "ws://localhost:56083";
        private const string _hcAppURI = "ws://localhost:8888";
        private const string _oasisHappPath = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
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
            
            //_holoNETClient.Connect();
            _holoNETClient.ConnectAdmin();
        }

        private void _holoNETClient_OnAdminAgentPubKeyGeneratedCallBack(object sender, AdminAgentPubKeyGeneratedCallBackEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN AgentPubKey Generated: EndPoint: {e.EndPoint}, Id: {e.Id},  AgentPubKey: {e.AgentPubKey}, IsError: {e.IsError}, Message: {e.Message}");
            _holoNETClient.AdminInstallApp(e.AgentPubKey, "oasis-app777", _oasisHappPath);
        }

        private void HoloNETClient_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            lstOutput.Items.Add("Disconnected");
        }

        private void HoloNETClient_OnAdminAppInstalledCallBack(object sender, AdminAppInstalledCallBackEventArgs e)
        {
            lstOutput.Items.Add($"ADMIN hApp Installed: EndPoint: {e.EndPoint}, Id: {e.Id}, installed_app_id: {e.AppResponse.installed_app_id}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            lstOutput.Items.Add(string.Concat("Zome CallBack: ", ProcessZomeFunctionCallBackEventArgs(e)));
        }

        private void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            lstOutput.Items.Add(string.Concat("Data Received: EndPoint: ", e.EndPoint, ", Raw JSON Data: ", e.RawJSONData, ", Raw Binary Data: ", e.RawBinaryData, ", Raw Binary Data After MessagePack Decode: ", e.RawBinaryDataAfterMessagePackDecode, ", Raw Binary Data After MessagePack Decode As String: ", e.RawBinaryDataAfterMessagePackDecodeAsString, ", IsError: ", e.IsError, ", Message:", e.Message));
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
            _holoNETClient.AdminGenerateAgentPubKey();
            
            //_holoNETClient.AdminInstallApp("oasis-app", _oasisHappPath);
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";

            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

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
