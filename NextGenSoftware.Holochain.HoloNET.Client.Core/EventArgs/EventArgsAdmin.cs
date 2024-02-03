using System;
using System.Collections.Generic;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class InstallEnableSignAndAttachHappEventArgs : CallBackBaseEventArgs
    {
        public bool IsSuccess { get; set; }
        public bool IsAgentPubKeyGenerated { get; set; }
        public bool IsAppInstalled { get; set; }
        public bool IsAppEnabled { get; set; }
        public bool IsAppSigned { get; set; }
        public bool IsAppAttached { get; set; }

        public string AgentPubKey { get; set; }
        public string DnaHash { get; set; }
        public byte[][] CellId { get; set; }
        public CellInfoType CellType { get; set; } = CellInfoType.None;
        public AppInfoStatusEnum AppStatus { get; set; }
        public string AppStatusReason { get; set; }
        public AppManifest AppManifest { get; set; }
        public UInt16? AttachedOnPort { get; set; }

        public AgentPubKeyGeneratedCallBackEventArgs AgentPubKeyGeneratedResult { get; set; }
        public AppInstalledCallBackEventArgs AppInstalledResult { get; set; }
        public AppEnabledCallBackEventArgs AppEnabledResult { get; set; }
        public ZomeCallCapabilityGrantedCallBackEventArgs ZomeCallCapabilityGrantedResult { get; set; }
        public AppInterfaceAttachedCallBackEventArgs AppInterfaceAttachedResult { get; set; }
    }

    public class InstallEnableSignAttachAndConnectToHappEventArgs : InstallEnableSignAndAttachHappEventArgs
    {
        public bool IsAppConnected { get; set; }
        public HoloNETClientAppAgent HoloNETClientAppAgent { get; set; }
        public HoloNETConnectedEventArgs HoloNETConnectedResult { get; set; }
    }

    public class AppInstalledCallBackEventArgs : AppInfoCallBackEventArgs
    {

    }

    public class AppUninstalledCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string InstalledAppId { get; set; }
    }

    public class AgentPubKeyGeneratedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class AppEnabledCallBackEventArgs : AppInfoCallBackEventArgs
    {
        public object Errors { get; set; }
    }

    public class AppDisabledCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string InstalledAppId { get; set; }
    }

    public class ZomeCallCapabilityGrantedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        //public AppResponse AppResponse { get; set; }
    }

    public class AppInterfaceAttachedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public UInt16? Port { get; set; }
    }

    public class DnaRegisteredCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public byte[] HoloHash { get; set; }
    }

    public class DnaDefinitionReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public DnaDefinitionResponse DnaDefinition { get; set; }
    }

    public class AppInterfacesListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<ushort> WebSocketPorts { get; set; } = new List<ushort>();
    }

    public class AppsListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<AppInfo> Apps { get; set; } = new List<AppInfo>();
    }

    public class DnasListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public byte[][] Dnas { get; set; }
    }

    public class CellIdsListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public byte[][][] CellIds { get; set; }
    }

    public class GetAppInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppInfo AppInfo { get; set; }
    }

    public class StateDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string DumpedStateJSON { get; set; }
    }

    public class FullStateDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public FullStateDumpedResponse DumpedState { get; set; }
    }

    public class CoordinatorsUpdatedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
 
    }

    public class AgentInfoReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AgentInfo AgentInfo { get; set; }
    }

    public class AgentInfoAddedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        //public AgentInfo AgentInfo { get; set; }
    }

    public class CloneCellDeletedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    public class GetStorageInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }


    public class NetworkMetricsDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string NetworkMetricsDumpJSON { get; set; }
    }

    public class DumpNetworkStatsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }
}