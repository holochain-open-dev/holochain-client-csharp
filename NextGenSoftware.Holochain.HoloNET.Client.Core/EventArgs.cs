using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETDataReceivedBaseEventArgs : CallBackBaseEventArgsWithDataAndId
    {
        public string Type { get; set; }
        public HoloNETResponseType HoloNETResponseType { get; set; }
    }

    public class HoloNETErrorEventArgs : EventArgs
    {
        public string EndPoint { get; set; }
        public string Reason { get; set; }
        public Exception ErrorDetails { get; set; }
    }

    public class HoloNETDataReceivedEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public bool IsConductorDebugInfo { get; set; }
    }

    public class HoloNETDataSentEventArgs : DataSentEventArgs
    {

    }

    public class ZomeFunctionCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public ZomeFunctionCallBackEventArgs() : base()
        {

        }

        public string Zome { get; set; }
        public string ZomeFunction { get; set; }
        public Dictionary<string, object> ZomeReturnData { get; set; }
        public Dictionary<object, object> RawZomeReturnData { get; set; }
        public string ZomeReturnHash { get; set; }
        public EntryData Entry { get; set; }
        public Dictionary<string, string> KeyValuePair { get; set; }
        public string KeyValuePairAsString { get; set; }
    }

    public class AppInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppInfoResponse AppInfoResponse { get; set; }
        public string InstalledAppId { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class AdminAppInstalledCallBackEventArgs : AppInfoCallBackEventArgs
    {

    }

    public class AdminAgentPubKeyGeneratedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class AdminAppEnabledCallBackEventArgs : AppInfoCallBackEventArgs
    {
        public object Errors { get; set; }
    }

    public class AdminAppDisabledCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminZomeCallCapabilityGrantedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        //public AppResponse AppResponse { get; set; }
    }

    public class AdminAppInterfaceAttachedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public UInt16? Port { get; set; }
    }

    public class AdminRegisterDnaCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminListAppsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminListDnasCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminListCellIdsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminListAppInterfacesCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminDumpStateCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminDumpFullStateCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminGetDnaDefinitionCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminUpdateCoordinatorsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminGetAgentInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminAddAgentInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminDeleteCloneCellCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminGetStorageInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class AdminDumpNetworkStatsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public AppResponse AppResponse { get; set; }
    }

    public class ReadyForZomeCallsEventArgs : CallBackBaseEventArgs
    {
        public ReadyForZomeCallsEventArgs(string endPoint, string dnaHash, string agentPubKey)
        {
            EndPoint = endPoint;
            DnaHash = dnaHash;
            AgentPubKey = agentPubKey;
        }

        public string EndPoint { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class HoloNETShutdownEventArgs : CallBackBaseEventArgs
    {
        public HoloNETShutdownEventArgs()
        {

        }

        public HoloNETShutdownEventArgs(string endPoint, string dnaHash, string agentPubKey, HolochainConductorsShutdownEventArgs holochainConductorsShutdownEventArgs)
        {
            EndPoint = endPoint;
            DnaHash = dnaHash;
            AgentPubKey = agentPubKey;
            HolochainConductorsShutdownEventArgs = holochainConductorsShutdownEventArgs;
        }

        public HolochainConductorsShutdownEventArgs HolochainConductorsShutdownEventArgs { get; private set; }
        public string EndPoint { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class HolochainConductorsShutdownEventArgs : CallBackBaseEventArgs
    {
        public int NumberOfHolochainExeInstancesShutdown { get; set; }
        public int NumberOfHcExeInstancesShutdown { get; set; }
        public int NumberOfRustcExeInstancesShutdown { get; set; }
        public string EndPoint { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class SignalCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string AgentPubKey { get; set; }
        public string DnaHash { get; set; }
        public SignalType SignalType { get; set; }
        public SignalData RawSignalData { get;set;}
        public Dictionary<string, object> SignalData { get; set; }
        public string SignalDataAsString { get; set; }
    }

    public class ConductorDebugCallBackEventArgs : CallBackBaseEventArgs
    {
        public string Type { get; set; }
        public int NumberHeldEntries { get; set; }
        public int NumberHeldAspects { get; set; }
        public int NumberPendingValidations { get; set; }
        public int NumberDelayedValidations { get; set; }
        public int NumberRunningZomeCalls { get; set; }
        public bool Offline { get; set; }
        public string EndPoint { get; set; }
        public string RawJSONData { get; set; }
        public WebSocketReceiveResult WebSocketResult { get; set; }
    }
}