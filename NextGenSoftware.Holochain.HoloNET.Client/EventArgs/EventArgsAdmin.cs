using System;
using System.Collections.Generic;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
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
        public IAppManifest AppManifest { get; set; }
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
        public IHoloNETClientAppAgent HoloNETClientAppAgent { get; set; }
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
        public IAppResponse AppResponse { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class AppEnabledCallBackEventArgs : AppInfoCallBackEventArgs
    {
        public List<EnableAppError> Errors { get; set; }
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
        public IDnaDefinition DnaDefinition { get; set; }
    }

    public class AppInterfacesListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<ushort> WebSocketPorts { get; set; } = new List<ushort>();
    }

    public class AppsListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<IAppInfo> Apps { get; set; } = new List<IAppInfo>();
    }

    public class DnasListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<byte[]> Dnas { get; set; } = new List<byte[]>();
    }

    public class CellIdsListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<byte[][]> CellIds { get; set; } = new List<byte[][]>();
    }

    public class GetAppInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public IAppInfo AppInfo { get; set; }
    }

    public class StateDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string DumpedStateJSON { get; set; }
    }

    public class FullStateDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public IFullStateDumpedResponse DumpedState { get; set; }
    }

    public class CoordinatorsUpdatedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
 
    }

    public class AgentInfoReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        //public IAgentInfo AgentInfo { get; set; }
        public AgentInfo AgentInfo { get; set; }
    }

    public class AgentInfoAddedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        //public AgentInfo AgentInfo { get; set; }
    }

    public class CloneCellDeletedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    public class StorageInfoReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public IStorageInfoResponse StorageInfoResponse { get; set; }
    }

    public class NetworkMetricsDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string NetworkMetricsDumpJSON { get; set; }
    }

    public class NetworkStatsDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        /// <summary>
        /// Raw JSON escape hatch, as returned directly by the Holochain conductor's
        /// `dump_network_stats` admin call (AdminResponse::NetworkStatsDumped(String)).
        /// Kept for backwards compatibility / in case the typed NetworkStats below fails to
        /// parse a future/older conductor's exact JSON shape.
        /// </summary>
        public string NetworkStatsDumpJSON { get; set; }

        /// <summary>
        /// Typed representation of <see cref="NetworkStatsDumpJSON"/>, parsed into the
        /// kitsune2_api::transport::TransportStats shape (Holochain 0.6.1). Will be null if the
        /// JSON could not be parsed into this shape (see RawJSONData/NetworkStatsDumpJSON for
        /// the raw data in that case).
        /// </summary>
        public DumpNetworkStatsResponse NetworkStats { get; set; }
    }

    public class RecordsGraftedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    public class AdminInterfacesAddedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    // New in Holochain 0.6.1 - Admin API.

    public class ZomeCallCapabilityRevokedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    public class CapabilityGrantsListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<CellCapGrantInfo> CapGrants { get; set; } = new List<CellCapGrantInfo>();
    }

    public class PeerMetaInfoReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public PeerMetaInfoResponse PeerMetaInfo { get; set; }
    }

    public class AppAuthenticationTokenIssuedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppAuthenticationTokenIssuedResponse TokenIssued { get; set; }
    }

    public class AppAuthenticationTokenRevokedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    public class CompatibleCellsReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<AppCompatibleCells> CompatibleCells { get; set; } = new List<AppCompatibleCells>();
    }
}