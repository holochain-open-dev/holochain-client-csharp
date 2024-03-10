using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.Manager.Enums;
using NextGenSoftware.Holochain.HoloNET.Manager.Managers;
using NextGenSoftware.Holochain.HoloNET.Manager.Models;
using NextGenSoftware.Holochain.HoloNET.Manager.Objects;
using NextGenSoftware.Holochain.HoloNET.Manager.UserControls;
using NextGenSoftware.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Interfaces
{
    public interface IHoloNETManager
    {
        ClientOperation ClientOperation { get; set; }
        IInstalledApp CurrentApp { get; set; }
        IAvatarShared CurrentAvatar { get; set; }
        IHoloNETClientAdmin? HoloNETClientAdmin { get; set; }
        List<IHoloNETClientAppAgent> HoloNETClientAppAgentClients { get; set; }
        string HoloNETCollectionDemoAppId { get; }
        HoloNETObservableCollection<Avatar> HoloNETEntries { get; set; }
        Dictionary<string, HoloNETObservableCollection<AvatarShared>> HoloNETEntriesShared { get; set; }
        IAvatar HoloNETEntry { get; set; }
        string HoloNETEntryDemoAppId { get; }
        string HoloNETEntryDemoHappPath { get; }
        Dictionary<string, IAvatarShared> HoloNETEntryShared { get; set; }
        bool InitHoloNETEntryDemo { get; set; }
        ObservableCollection<IInstalledApp> InstalledApps { get; set; }
        InstallingAppParams InstallingAppParams { get; set; }
        int NumberOfClientConnections { get; set; }
        bool ShowAppsListedInLog { get; set; }
        bool ShowDetailedLogMessages { get; set; }
        ZomeCallParams ZomeCallParams { get; set; }

        event HoloNETManager.InstalledAppsChanged OnInstalledAppsChanged;
        event HoloNETManager.Log OnLogMessage;
        event HoloNETManager.NumberOfClientConnectionsChanged OnNumberOfClientConnectionsChanged;
        event HoloNETManager.StatusMessage OnStatusMessage;

        Task<ZomeFunctionCallBackEventArgs> AddHoloNETEntryToCollectionAsync(IHoloNETClientAppAgent client, string firstName, string lastName, DateTime dob, string email);
        void BootHoloNETManager();
        Task<bool> CheckIfDemoAppReadyAsync(bool isEntry);
        void CloseAllConnections();
        Task CloseConnection(bool isEntry);
        Task ConnectAdminAsync();
        ConnectToAppAgentClientResult ConnectToAppAgentClient();
        IHoloNETClientAppAgent CreateNewAppAgentClientConnection(string installedAppId, string agentPubKey, ushort port);
        Task<ZomeFunctionCallBackEventArgs> DeleteHoloNETEntryAsync();
        Task<bool> DisconnectAsync(IInstalledApp app);
        IHoloNETClientAppAgent GetClient(string dnaHash, string agentPubKey, string installedAppId);
        string GetEntryInfo(ZomeFunctionCallBackEventArgs e);
        Task<InstallEnableSignAndAttachHappEventArgs> InitDemoApp(string hAppId, string hAppInstallPath);
        void InitHoloNETClientAdmin();
        void InitHoloNETCollection(IHoloNETClientAppAgent client);
        Task<bool> InitHoloNETCollectionAsync();
        Task<bool> InitHoloNETEntryAsync();
        void InitHoloNETEntryShared(IHoloNETClientAppAgent client);
        Task InstallApp(string hAppName, string hAppPath);
        void ListHapps();
        Task<HoloNETCollectionLoadedResult<Avatar>> LoadCollectionAsync();
        Task<HoloNETCollectionLoadedResult<AvatarShared>> LoadCollectionAsync(IHoloNETClientAppAgent client);
        Task<ZomeFunctionCallBackEventArgs> LoadHoloNETEntryAsync();
        Task<ZomeFunctionCallBackEventArgs> LoadHoloNETEntrySharedAsync(IHoloNETClientAppAgent client);
        void LogMessage(string message);
        Task ProcessClientOperationAsync(IHoloNETClientAppAgent client);
        void ProcessListedApps(AppsListedCallBackEventArgs listedAppsResult);
        void Reboot();
        Task<ZomeFunctionCallBackEventArgs> RemoveHoloNETEntryFromCollectionAsync(IHoloNETClientAppAgent client, IAvatarShared avatar);
        Task<HoloNETCollectionSavedResult> SaveCollectionAsync();
        Task<ZomeFunctionCallBackEventArgs> SaveHoloNETEntryAsync();
        Task<ZomeFunctionCallBackEventArgs> SaveHoloNETEntrySharedAsync(IHoloNETClientAppAgent client, string firstName, string lastName, DateTime dob, string email);
        void SetAppToConnectedStatus(IInstalledApp app, int port, bool isSharedConnection, bool refreshGrid = true);
        void SetAppToConnectedStatus(string appName, int port, bool isSharedConnection, bool refreshGrid = true);
        void SetCurrentAppToConnectedStatus(int port);
        void ShowStatusMessage(string message, StatusMessageType type = StatusMessageType.Information, bool showSpinner = false, ucHoloNETEntry ucHoloNETEntry = null);
        void ShutdownHoloNETManager();
        Task<HoloNETCollectionSavedResult> UpdateHoloNETEntryInCollectionAsync(IHoloNETClientAppAgent client);
        void UpdateNumerOfClientConnections();
        void _holoNETClientAdmin_OnAppDisabledCallBack(object sender, AppDisabledCallBackEventArgs e);
        void _holoNETClientAdmin_OnAppsListedCallBack(object sender, AppsListedCallBackEventArgs e);
        void _holoNETClientAdmin_OnConnected(object sender, ConnectedEventArgs e);
        void _holoNETClientAdmin_OnError(object sender, HoloNETErrorEventArgs e);
        void _holoNETClientAdmin_OnHolochainConductorStarted(object sender, HolochainConductorStartedEventArgs e);
        void _holoNETClientAdmin_OnHolochainConductorStarting(object sender, HolochainConductorStartingEventArgs e);
    }
}