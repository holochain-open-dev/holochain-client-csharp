using System;
using System.Collections.Generic;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;
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
        public AppInfoStatusEnum AppStatus { get; set; }
        public string AppStatusReason { get; set; }
        public AppManifest AppManifest { get; set; }
        public UInt16? AttachedOnPort { get; set; }

        public AgentPubKeyGeneratedCallBackEventArgs AgentPubKeyGeneratedCallBackEventArgs { get; set; }
        public AppInstalledCallBackEventArgs AppInstalledCallBackEventArgs { get; set; }
        public AppEnabledCallBackEventArgs AppEnabledCallBackEventArgs { get; set; }
        public ZomeCallCapabilityGrantedCallBackEventArgs ZomeCallCapabilityGrantedCallBackEventArgs { get; set; }
        public AppInterfaceAttachedCallBackEventArgs AppInterfaceAttachedCallBackEventArgs { get; set; }
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

    public class RegisterDnaCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AppsListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<AppInfo> Apps { get; set; } = new List<AppInfo>();
    }

    public class GetAppInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppInfo AppInfo { get; set; }
    }

    public class ListDnasCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class ListCellIdsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class ListAppInterfacesCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class DumpStateCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class DumpFullStateCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class GetDnaDefinitionCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class UpdateCoordinatorsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class GetAgentInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AddAgentInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class DeleteCloneCellCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class GetStorageInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class DumpNetworkStatsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }
}