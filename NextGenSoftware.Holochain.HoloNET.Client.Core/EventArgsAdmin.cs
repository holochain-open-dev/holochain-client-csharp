using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
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