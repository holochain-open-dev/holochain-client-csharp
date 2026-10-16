using System;
using System.Collections.Generic;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>The outcome of InstallEnableSignAndAttachHappAsync: every step's result, and the installed app's agent, DNA, cell and app interface port.</summary>
    public class InstallEnableSignAndAttachHappEventArgs : CallBackBaseEventArgs
    {
        /// <summary>True when every step worked (the hApp is installed, enabled, signed and attached).</summary>
        public bool IsSuccess { get; set; }
        /// <summary>True when a new agent key was generated (false when an installed app and its agent were kept).</summary>
        public bool IsAgentPubKeyGenerated { get; set; }
        /// <summary>True when the app is installed (newly, or it already was).</summary>
        public bool IsAppInstalled { get; set; }
        /// <summary>True when the app is enabled.</summary>
        public bool IsAppEnabled { get; set; }
        /// <summary>True when signing credentials and the zome-call capability grant were set up.</summary>
        public bool IsAppSigned { get; set; }
        /// <summary>True when an app interface was attached (see AttachedOnPort).</summary>
        public bool IsAppAttached { get; set; }

        /// <summary>The app's agent public key.</summary>
        public string AgentPubKey { get; set; }
        /// <summary>The DNA hash of the app's cell.</summary>
        public string DnaHash { get; set; }
        /// <summary>The app's cell id (DNA hash, agent public key).</summary>
        public byte[][] CellId { get; set; }
        /// <summary>The kind of cell (provisioned, cloned, stem).</summary>
        public CellInfoType CellType { get; set; } = CellInfoType.None;
        /// <summary>The app's status as the conductor reported it.</summary>
        public AppInfoStatusEnum AppStatus { get; set; }
        /// <summary>Why the app is disabled or paused, if it is.</summary>
        public string AppStatusReason { get; set; }
        /// <summary>The app's manifest (roles, DNAs).</summary>
        public IAppManifest AppManifest { get; set; }
        /// <summary>The app interface port to connect app clients to.</summary>
        public UInt16? AttachedOnPort { get; set; }

        /// <summary>The agent key step's raw result.</summary>
        public AgentPubKeyGeneratedCallBackEventArgs AgentPubKeyGeneratedResult { get; set; }
        /// <summary>The install step's raw result.</summary>
        public AppInstalledCallBackEventArgs AppInstalledResult { get; set; }
        /// <summary>The enable step's raw result.</summary>
        public AppEnabledCallBackEventArgs AppEnabledResult { get; set; }
        /// <summary>The signing / capability grant step's raw result.</summary>
        public ZomeCallCapabilityGrantedCallBackEventArgs ZomeCallCapabilityGrantedResult { get; set; }
        /// <summary>The attach-interface step's raw result.</summary>
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

        /// <summary>Each app interface as Holochain 0.7.0 reports it (port, allowed origins, optional app binding).</summary>
        public List<AppInterfaceInfo> AppInterfaces { get; set; } = new List<AppInterfaceInfo>();
    }

    /// <summary>One entry of list_app_interfaces (Holochain 0.7.0 <c>AppInterfaceInfo</c>).</summary>
    public class AppInterfaceInfo
    {
        public ushort Port { get; set; }

        /// <summary>"*" for any origin, otherwise a comma-separated list.</summary>
        public string AllowedOrigins { get; set; }

        /// <summary>Set when the interface is bound to one installed app.</summary>
        public string InstalledAppId { get; set; }
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

        /// <summary>The conductor's FullStateDump as JSON. Always set; DumpedState is best-effort.</summary>
        public string DumpedStateJSON { get; set; }
    }

    public class CoordinatorsUpdatedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
 
    }

    public class AgentInfoReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        //public IAgentInfo AgentInfo { get; set; }
        [System.Obsolete("Holochain 0.7.0 returns encoded agent-info strings; use AgentInfos. Always null.")]
        public AgentInfo AgentInfo { get; set; }

        /// <summary>One encoded agent-info string per agent (AdminResponse::AgentInfo(Vec&lt;String&gt;)).</summary>
        public List<string> AgentInfos { get; set; }
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
        /// kitsune2_api/0.7.0). Will be null if the
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

    // New in Holochain 0.7.0 - Admin API.

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

    // New in Holochain 0.7.0

    public class AdminOpTimingsDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public OpTimingsDump OpTimingsDump { get; set; }
    }

    public class AppOpTimingsDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public OpTimingsDump OpTimingsDump { get; set; }
    }
}