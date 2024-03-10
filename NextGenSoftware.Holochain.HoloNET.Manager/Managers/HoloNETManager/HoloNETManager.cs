using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Manager.Enums;
using NextGenSoftware.Holochain.HoloNET.Manager.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Manager.Models;
using NextGenSoftware.Holochain.HoloNET.Manager.Objects;
using NextGenSoftware.Holochain.HoloNET.Manager.UserControls;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Managers
{
    public partial class HoloNETManager : IHoloNETManager
    {
        private static IHoloNETManager _instance = null;
        private const string _holoNETEntryDemoAppId = "oasis-holonet-entry-demo-app";
        private const string _holoNETCollectionDemoAppId = "oasis-holonet-collection-demo-app";
        private const string _holoNETEntryDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _holoNETCollectionDemoHappPath = @"E:\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _role_name = "oasis";
        private bool _rebooting = false;
        private bool _adminDisconnected = false;
        private byte[][] _installingAppCellId = null;
        private int _clientsToDisconnect = 0;
        private int _clientsDisconnected = 0;
        private ushort _appAgentClientPort = 0;
        private bool _removeClientConnectionFromPoolAfterDisconnect = true;

        public delegate void StatusMessage(object sender, StatusMessageEventArgs e);
        public event StatusMessage OnStatusMessage;

        public delegate void Log(object sender, LogMessageEventArgs e);
        public event Log OnLogMessage;

        public delegate void NumberOfClientConnectionsChanged(object sender, NumberOfClientConnectionsEventArgs e);
        public event NumberOfClientConnectionsChanged OnNumberOfClientConnectionsChanged;

        public delegate void InstalledAppsChanged(object sender, InstalledAppsEventArgs e);
        public event InstalledAppsChanged OnInstalledAppsChanged;

        public IHoloNETClientAdmin? HoloNETClientAdmin { get; set; }
        public List<IHoloNETClientAppAgent> HoloNETClientAppAgentClients { get; set; } = new List<IHoloNETClientAppAgent>();
        public ObservableCollection<IInstalledApp> InstalledApps { get; set; } = new ObservableCollection<IInstalledApp>();

        //This HoloNETEntry and HoloNETCollection use their own interal HoloNETClient (AppAgent) connection.
        public IAvatar HoloNETEntry { get; set; }
        public HoloNETObservableCollection<Avatar> HoloNETEntries { get; set; }

        //This HoloNETEntry and HoloNETCollection use a shared HoloNETClient (AppAgent) connection for the given hApp from the HoloNETClientAppAgentClients connection pool.
        public Dictionary<string, IAvatarShared> HoloNETEntryShared { get; set; } = new Dictionary<string, IAvatarShared>();
        public Dictionary<string, HoloNETObservableCollection<AvatarShared>> HoloNETEntriesShared { get; set; } = new Dictionary<string, HoloNETObservableCollection<AvatarShared>>();

        public string HoloNETEntryDemoHappPath { get; private set; } = _holoNETEntryDemoHappPath;
        public string HoloNETEntryDemoAppId { get; private set; } = _holoNETEntryDemoAppId;
        public string HoloNETCollectionDemoAppId { get; private set; } = _holoNETCollectionDemoAppId;
        public IInstalledApp CurrentApp { get; set; }
        public IAvatarShared CurrentAvatar { get; set; }
        public bool ShowDetailedLogMessages { get; set; } = false;
        public bool InitHoloNETEntryDemo { get; set; } = false;
        public bool ShowAppsListedInLog { get; set; } = true;
        public int NumberOfClientConnections { get; set; }

        //non-async state control properties (async version will not need these)
        public ClientOperation ClientOperation { get; set; }
        public ZomeCallParams ZomeCallParams { get; set; } = new ZomeCallParams();
        public InstallingAppParams InstallingAppParams { get; set; } = new InstallingAppParams();

        public static IHoloNETManager Instance
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

        public void Reboot()
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
        public async Task<bool> CheckIfDemoAppReadyAsync(bool isEntry)
        {
            //Supress event callbacks for the shared connections.
            ShowAppsListedInLog = false;
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

            GetAppInfoCallBackEventArgs appInfoResult = await HoloNETClientAdmin.GetAppInfoAsync(appId);

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

                    AppEnabledCallBackEventArgs enabledResult = await HoloNETClientAdmin.EnableAppAsync(appId);

                    if (enabledResult != null && !enabledResult.IsError)
                    {
                        //No need to show because the event callback already logs this message.
                        //Log($"ADMIN: Test App {appId} Enabled.");
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
                ShowStatusMessage($"Checking If Test App {appId} HoloNETClient WebSocket Connection Is Open...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                if (isEntry)
                {
                    if (HoloNETEntry != null && HoloNETEntry.HoloNETClient != null && HoloNETEntry.HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                    {
                        LogMessage($"APP: Test App {appId} HoloNETClient WebSocket Connection Is Not Open!");
                        ShowStatusMessage($"Test App {appId} HoloNETClient WebSocket Connection Is Not Open!", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                        LogMessage($"APP: Opening HoloNETClient WebSocket Connection On Port {HoloNETEntry.HoloNETClient.EndPoint.Port}...");
                        ShowStatusMessage($"Opening HoloNETClient WebSocket Connection On Port {HoloNETEntry.HoloNETClient.EndPoint.Port}...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                        await HoloNETEntry.HoloNETClient.ConnectAsync();

                        if (HoloNETEntry != null && HoloNETEntry.HoloNETClient != null && HoloNETEntry.HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                        {
                            LogMessage($"APP: Failed To Open Connection!");
                            ShowStatusMessage($"Failed To Open Connection!", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                            LogMessage($"APP: Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...");
                            ShowStatusMessage($"Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                            await CloseConnection(isEntry);
                        }
                        else
                        {
                            LogMessage($"APP: HoloNETClient WebSocket Connection Opened On Port {HoloNETEntry.HoloNETClient.EndPoint.Port}.");
                            ShowStatusMessage($"HoloNETClient WebSocket Connection Opened On Port {HoloNETEntry.HoloNETClient.EndPoint.Port}.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                            //Will wait until the HoloNET Entry has init (non blocking).
                            await HoloNETEntry.WaitTillHoloNETInitializedAsync();

                            //Refresh the list of installed hApps.
                            ProcessListedApps(await HoloNETClientAdmin.ListAppsAsync(AppStatusFilter.All));

                            //Update number of connections UI.
                            UpdateNumerOfClientConnections();
                        }
                    }
                    else
                    {
                        LogMessage($"APP: HoloNETClient WebSocket Connection Is Open On Port {HoloNETEntry.HoloNETClient.EndPoint.Port}.");
                        ShowStatusMessage($"HoloNETClient WebSocket Connection Is Open On Port {HoloNETEntry.HoloNETClient.EndPoint.Port}.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                    }
                }
                else
                {
                    if (HoloNETEntries != null && HoloNETEntries.HoloNETClient != null && HoloNETEntries.HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                    {
                        LogMessage($"APP: Test App {appId} HoloNETClient WebSocket Connection Is Not Open!");
                        ShowStatusMessage($"Test App {appId} HoloNETClient WebSocket Connection Is Not Open!", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                        LogMessage($"APP: Opening HoloNETClient WebSocket Connection On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}...");
                        ShowStatusMessage($"Opening HoloNETClient WebSocket Connection On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                        await HoloNETEntries.HoloNETClient.ConnectAsync();

                        if (HoloNETEntries != null && HoloNETEntries.HoloNETClient != null && HoloNETEntries.HoloNETClient.State != System.Net.WebSockets.WebSocketState.Open)
                        {
                            LogMessage($"APP: Failed To Open Connection!");
                            ShowStatusMessage($"Failed To Open Connection!", StatusMessageType.Error, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                            LogMessage($"APP: Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...");
                            ShowStatusMessage($"Closing HoloNET Entry (And Closing Internal HoloNETClient Connection)...", StatusMessageType.Information, true, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                            await CloseConnection(isEntry);
                        }
                        else
                        {
                            LogMessage($"APP: HoloNETClient WebSocket Connection Opened On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}.");
                            ShowStatusMessage($"HoloNETClient WebSocket Connection Opened On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);

                            //Will wait until the HoloNET Collection has init (non blocking).
                            await HoloNETEntries.WaitTillHoloNETInitializedAsync();

                            //Refresh the list of installed hApps.
                            ProcessListedApps(await HoloNETClientAdmin.ListAppsAsync(AppStatusFilter.All));

                            //Update number of connections UI.
                            UpdateNumerOfClientConnections();
                        }
                    }
                    else
                    {
                        LogMessage($"APP: HoloNETClient WebSocket Connection Is Open On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}.");
                        ShowStatusMessage($"HoloNETClient WebSocket Connection Is Open On Port {HoloNETEntries.HoloNETClient.EndPoint.Port}.", StatusMessageType.Success, false, HoloNETEntryUIManager.CurrentHoloNETEntryUI);
                    }
                }
            }

            ShowAppsListedInLog = true;
            //ucHoloNETEntry.HideMessage(); //Hide any messages that were shown.

            return showAlreadyInitMessage;
        }

        public void CloseAllConnections()
        {
            LogMessage("APP: Disconnecting All HoloNETClient AppAgent WebSockets...");
            ShowStatusMessage($"Disconnecting All HoloNETClient AppAgent WebSockets...", StatusMessageType.Information, true);

            _removeClientConnectionFromPoolAfterDisconnect = true;

            foreach (HoloNETClientAppAgent client in HoloNETClientAppAgentClients)
            {
                if (client.State == System.Net.WebSockets.WebSocketState.Open)
                    client.Disconnect();

                //client = null;
            }

            if (HoloNETEntry != null)
            {
                HoloNETEntry.Close();
                HoloNETEntry = null;
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

        public void ShutdownHoloNETManager()
        {
            if (HoloNETEntry != null)
            {
                HoloNETEntry.OnInitialized -= _holoNETEntry_OnInitialized;
                HoloNETEntry.OnLoaded -= _holoNETEntry_OnLoaded;
                HoloNETEntry.OnSaved -= _holoNETEntry_OnSaved;
                HoloNETEntry.OnDeleted -= _holoNETEntry_OnDeleted;
                HoloNETEntry.OnClosed -= _holoNETEntry_OnClosed;
                HoloNETEntry.OnError -= _holoNETEntry_OnError;
            }

            if (HoloNETEntries != null)
            {
                HoloNETEntries.OnClosed -= HoloNETEntries_OnClosed;
                HoloNETEntries.OnCollectionLoaded -= HoloNETEntries_OnCollectionLoaded;
                HoloNETEntries.OnCollectionSaved -= HoloNETEntries_OnCollectionSaved;
                HoloNETEntries.OnError -= HoloNETEntries_OnError;
                HoloNETEntries.OnHoloNETEntryAddedToCollection -= HoloNETEntries_OnHoloNETEntryAddedToCollection;
                HoloNETEntries.OnHoloNETEntryRemovedFromCollection -= HoloNETEntries_OnHoloNETEntryRemovedFromCollection;
            }

            if (HoloNETEntryShared != null)
            {
                foreach (string key in HoloNETEntryShared.Keys)
                {
                    HoloNETEntryShared[key].OnInitialized += _holoNETEntryShared_OnInitialized;
                    HoloNETEntryShared[key].OnLoaded += _holoNETEntryShared_OnLoaded;
                    HoloNETEntryShared[key].OnClosed += _holoNETEntryShared_OnClosed;
                    HoloNETEntryShared[key].OnSaved += _holoNETEntryShared_OnSaved;
                    HoloNETEntryShared[key].OnDeleted += _holoNETEntryShared_OnDeleted;
                    HoloNETEntryShared[key].OnError += _holoNETEntryShared_OnError;
                }
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

            CloseAllConnections();
        }

        public void ShowStatusMessage(string message, StatusMessageType type = StatusMessageType.Information, bool showSpinner = false, ucHoloNETEntry ucHoloNETEntry = null)
        {
            OnStatusMessage?.Invoke(this, new StatusMessageEventArgs { Message = message, Type = type, ShowSpinner = showSpinner, ucHoloNETEntry = ucHoloNETEntry });
        }

        public void LogMessage(string message)
        {
            OnLogMessage?.Invoke(this, new LogMessageEventArgs { Message = message });
        }
    }
}