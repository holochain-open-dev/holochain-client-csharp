using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    public partial class HoloNETManager
    {
        private static HoloNETManager _instance = null;
        private const string _holoNETEntryDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        //private const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis-holonet-collection\BUILD\happ\oasis.happ";
        public const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        
        private const string _role_name = "oasis";
        private bool _rebooting = false;
        private bool _adminDisconnected = false;
        // private bool _appDisconnected = false;
        public string InstallinghAppName = "";
        
        private byte[][] _installingAppCellId = null;
        private Avatar _holoNETEntry;
        //private Dictionary<string, AvatarShared> _holoNETEntryShared = new Dictionary<string, AvatarShared>();
        private int _clientsToDisconnect = 0;
        private int _clientsDisconnected = 0;
        
       
        private ushort _appAgentClientPort = 0;
        private bool _removeClientConnectionFromPoolAfterDisconnect = true;
        private bool _initHoloNETEntryDemo = false;
        private bool _showAppsListedInLog = true;

        public HoloNETClient? HoloNETClientAdmin { get; set; }
        public List<HoloNETClient> HoloNETClientAppAgentClients { get; set; } = new List<HoloNETClient>();
        public ObservableCollection<InstalledApp> InstalledApps { get; set; } = new ObservableCollection<InstalledApp>();
        
        //public Dictionary<string, HoloNETObservableCollection<AvatarShared>> HoloNETEntriesShared { get; set; } = new Dictionary<string, HoloNETObservableCollection<AvatarShared>>();
        public HoloNETObservableCollection<Avatar> HoloNETEntries { get; set; }
        
        public InstalledApp CurrentApp { get; set; }
        public AvatarShared CurrentAvatar;
        public bool ShowDetailedLogMessages { get; set; } = false;
        
        //non async state control properties (async version will not need these)
        public dynamic ParamsObjectForZomeCall { get; set; }
        public ClientOperation ClientOperation { get; set; }
        public string HoloNETEntryDemoAppId { get; private set; } = "oasis-holonet-entry-demo-app";
        public string HoloNETCollectionDemoAppId { get; private set; } = "oasis-holonet-collection-demo-app";
        public string InstallinghAppPath { get; private set; } = "";

        public class StatusMessageEventArgs : EventArgs
        {
            public string Message { get; set; }
            public StatusMessageType Type { get; set; } = StatusMessageType.Information;
            public bool ShowSpinner { get; set; } = false;
            public ucHoloNETEntry ucHoloNETEntry { get; set; }
        }

        public class LogMessageEventArgs : EventArgs
        {
            public string Message { get; set; }
        }

        public delegate void StatusMessage(object sender, StatusMessageEventArgs e);
        public event StatusMessage OnStatusMessage;

        public delegate void LogMessage(object sender, LogMessageEventArgs e);
        public event LogMessage OnLogMessage;

        public static HoloNETManager Instance 
        { 
            get
            {
                if (_instance == null)
                    _instance = new HoloNETManager();

                return _instance;
            }
        }

        public void BootHoloNETManager()
        {
            InitHoloNETClientAdmin();
        }

        public void ShowStatusMessage(string message, StatusMessageType type = StatusMessageType.Information, bool showSpinner = false, ucHoloNETEntry ucHoloNETEntry = null)
        {
            OnStatusMessage?.Invoke(this, new StatusMessageEventArgs { Message = message, Type = type, ShowSpinner = showSpinner, ucHoloNETEntry = ucHoloNETEntry });
        }

        public void LogMessage(string message)
        {
            OnLogMessage?.Invoke(this, new LogMessageEventArgs { Message = message });
        }

        public  void Reboot()
        {
            _clientsToDisconnect = HoloNETClientAppAgentClients.Count;
            _rebooting = true;
            CloseAllConnections();
        }

        /// <summary>
        /// This will use both the Admin Connection and AppAgent Connection to check if the demo hApp is ready to be used by either the HoloNET Entry (Internal Connection) or HoloNET Collection (Internal Conmnection) demo (popup).
        /// This is only called if the HoloNET Entry or HoloNET Collection has already been init to check if the connections/hApp status has been changed (either user or system) since the last time they were used (popup opened).
        /// </summary>
        /// <param name="isEntry"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfDemoAppReady(bool isEntry)
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
            ShowStatusMessage($"Checking If Test App {appId} Is Still Installed...", StatusMessageType.Success, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
            //ucHoloNETEntry.ShowStatusMessage($"Checking If Test App {appId} Is Still Installed...", StatusMessageType.Information, true);

            AdminGetAppInfoCallBackEventArgs appInfoResult = await HoloNETClientAdmin.AdminGetAppInfoAsync(appId);

            //If the test app was manually uninstalled by the user then we need to re-init the HoloNET Entry/Collection now...
            if (appInfoResult != null && appInfoResult.AppInfo == null || appInfoResult.IsError)
            {
                LogMessage($"ADMIN: Test App {appId} Is NOT Installed.");
                ShowStatusMessage($"ADMIN App {appId} Is NOT Installed.", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                LogMessage($"APP: Closing HoloNET {item} (And Closing Internal HoloNETClient Connection)...");
                ShowStatusMessage($"Closing HoloNET {item} (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                //ucHoloNETEntry.ShowStatusMessage($"Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true);

                await CloseConnection(isEntry);
            }
            else
            {
                LogMessage($"ADMIN: Test App {appId} Still Installed.");
                ShowStatusMessage($"ADMIN App {appId} Still Installed.", StatusMessageType.Information, false);

                LogMessage($"ADMIN: Checking If Test App {appId} Is Still Running (Enabled)...");
                ShowStatusMessage($"Checking If Test App {appId} Is Still Running (Enabled)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                //ucHoloNETEntry.ShowStatusMessage($"Checking If Test App {appId} Is Still Running (Enabled)...", StatusMessageType.Information, true);

                if (appInfoResult.AppInfo.AppStatus == AppInfoStatusEnum.Running)
                {
                    LogMessage($"ADMIN: Test App {appId} Still Running (Enabled).");
                    ShowStatusMessage($"Test App {appId} Still Running (Enabled).", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                }
                else
                {
                    LogMessage($"ADMIN Test App {appId} NOT Running (Disabled).");
                    ShowStatusMessage($"Test App {appId} NOT Running (Disabled).", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                    LogMessage($"ADMIN: Enabling Test App {appId}...");
                    ShowStatusMessage($"Enabling Test App {appId}...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                    AdminAppEnabledCallBackEventArgs enabledResult = await HoloNETClientAdmin.AdminEnableAppAsync(appId);

                    if (enabledResult != null && !enabledResult.IsError)
                    {
                        //No need to show because the event callback already logs this message.
                        //LogMessage($"ADMIN: Test App {appId} Enabled.");
                        //ShowStatusMessage($"Test App {appId} Enabled.", StatusMessageType.Information, false);
                    }
                    else
                    {
                        LogMessage($"ADMIN: Error Occured Enabling Test App {appId}. Reason: {enabledResult.Message}");
                        ShowStatusMessage($"Error Occured Enabling Test App {appId}. Reason: {enabledResult.Message}", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                        LogMessage($"APP: Closing HoloNET {item} (And Closing Internal HoloNETClient Connection)...");
                        ShowStatusMessage($"Closing HoloNET {item} (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

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
                            ProcessListedApps(await HoloNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All));
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
                            ProcessListedApps(await HoloNETClientAdmin.AdminListAppsAsync(AppStatusFilter.All));
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

        public void CloseAllConnections()
        {
            LogMessage("APP: Disconnecting All HoloNETClient AppAgent WebSockets...");
            ShowStatusMessage($"Disconnecting All HoloNETClient AppAgent WebSockets...", StatusMessageType.Information, true);

            _removeClientConnectionFromPoolAfterDisconnect = true;

            foreach (HoloNETClient client in HoloNETClientAppAgentClients)
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

            if (HoloNETClientAdmin != null)
            {
                if (HoloNETClientAdmin.State == System.Net.WebSockets.WebSocketState.Open)
                    HoloNETClientAdmin.Disconnect();
            }
        }

        public  void ShutdownHoloNETManager()
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

            CloseAllConnections();
        }
    }
}