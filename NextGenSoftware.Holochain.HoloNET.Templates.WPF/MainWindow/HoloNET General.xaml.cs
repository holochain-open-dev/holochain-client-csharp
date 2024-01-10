using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// //NOTE: EVERY method on HoloNETClient can be called either async or non-async, in these examples we are using a mixture of async and non-async. Normally you would use async because it is less code and easier to follow but we wanted to test and demo both versions (and show how you would use non async as well as async versions)...
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// This will use both the Admin Connection and AppAgent Connection to check if the demo hApp is ready to be used by either the HoloNET Entry (Internal Connection) or HoloNET Collection (Internal Conmnection) demo (popup).
        /// This is only called if the HoloNET Entry or HoloNET Collection has already been init to check if the connections/hApp status has been changed (either user or system) since the last time they were used (popup opened).
        /// </summary>
        /// <param name="isEntry"></param>
        /// <returns></returns>
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
    }
}