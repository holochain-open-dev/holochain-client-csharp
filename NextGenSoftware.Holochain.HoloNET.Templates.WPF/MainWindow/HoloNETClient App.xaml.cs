using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        private void _holoNETClientApp_OnError(object sender, HoloNETErrorEventArgs e)
        {
            ShowStatusMessage($"Error occured On App WebSocket. Reason: {e.Reason}.", StatusMessageType.Error);
            LogMessage($"APP: Error occured. Reason: {e.Reason}");
        }
    }
}