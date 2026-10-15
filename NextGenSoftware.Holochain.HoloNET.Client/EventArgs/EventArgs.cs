using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETDataReceivedBaseEventArgs : CallBackBaseEventArgsWithDataAndId
    {
        public string Type { get; set; }
        public HoloNETResponseType HoloNETResponseType { get; set; }
    }

    public class HoloNETDataReceivedBaseBaseEventArgs<T> : HoloNETDataReceivedBaseEventArgs
    {
        public T Response { get; set; }
    }

    public class HoloNETConnectedEventArgs : CallBackBaseEventArgs
    {
        public bool IsConnected { get; set; }
    }

    public class HoloNETDisconnectedEventArgs : CallBackBaseEventArgs
    {
        public bool IsDisconnected { get; set; }

        /// <summary>Result of the automatic conductor shutdown that follows a disconnect (WaitForHolochainConductorToDisconnect mode only).</summary>
        public HolochainConductorsShutdownEventArgs HolochainConductorsShutdownResult { get; set; }
    }

    public class HoloNETErrorEventArgs : EventArgs
    {
        public Uri EndPoint { get; set; }
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

        /// <summary>
        /// The zome function's return value as decoded msgpack, whatever its type: null for (), a bool, number or string,
        /// a byte[] hash, a list (e.g. Vec&lt;ActionHash&gt;) or a map. Records / KeyValuePair are also filled for record results.
        /// </summary>
        public object ZomeReturnValue { get; set; }

        public List<Record> Records { get; set; } = new List<Record>();
        public Dictionary<string, string> KeyValuePair { get; set; }
        public string KeyValuePairAsString { get; set; }
    }

    //public class ZomeFunctionCallBackForCollectionEventArgs : HoloNETDataReceivedBaseEventArgs
    //{
    //    public string Zome { get; set; }
    //    public string ZomeFunction { get; set; }
    //    public Dictionary<string, object> ZomeReturnData { get; set; }
    //    public Dictionary<object, object> RawZomeReturnData { get; set; }
    //    public string ZomeReturnHash { get; set; }
    //    public List<EntryData> Entries { get; set; }
    //    public Dictionary<string, string> KeyValuePair { get; set; }
    //    public string KeyValuePairAsString { get; set; }
    //}

    public class AppInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public IAppInfoResponse AppInfoResponse { get; set; }
        public AppInfoStatusEnum AppStatus { get; set; } //TODO: Set
        public string AppStatusReason { get; set; } //TODO: Set
        public IAppManifest AppManifest { get; set; } //TODO: Set
        public string InstalledAppId { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
        public byte[][] CellId { get; set; }
        public CellInfoType CellType { get; set; } //TODO: Set
    }

    public class ReadyForZomeCallsEventArgs : CallBackBaseEventArgs
    {
        public ReadyForZomeCallsEventArgs(Uri endPoint, string dnaHash, string agentPubKey)
        {
            EndPoint = endPoint;
            DnaHash = dnaHash;
            AgentPubKey = agentPubKey;
        }

        public Uri EndPoint { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class HoloNETShutdownEventArgs : CallBackBaseEventArgs
    {
        public HoloNETShutdownEventArgs()
        {

        }

        public HoloNETShutdownEventArgs(Uri endPoint, string dnaHash, string agentPubKey, HolochainConductorsShutdownEventArgs holochainConductorsShutdownEventArgs)
        {
            EndPoint = endPoint;
            DnaHash = dnaHash;
            AgentPubKey = agentPubKey;
            HolochainConductorsShutdownEventArgs = holochainConductorsShutdownEventArgs;
        }

        public HolochainConductorsShutdownEventArgs HolochainConductorsShutdownEventArgs { get; private set; }
        public Uri EndPoint { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
    }


    public class HolochainConductorStartingEventArgs : CallBackBaseEventArgs
    {

    }

    public class HolochainConductorStartedEventArgs : CallBackBaseEventArgs
    {

    }

    public class HolochainConductorsShutdownEventArgs : CallBackBaseEventArgs
    {
        public int NumberOfHolochainExeInstancesShutdown { get; set; }
        public int NumberOfHcExeInstancesShutdown { get; set; }
        public int NumberOfRustcExeInstancesShutdown { get; set; }
        public Uri EndPoint { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
    }

    public class SignalCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string AgentPubKey { get; set; }
        public string DnaHash { get; set; }
        public SignalType SignalType { get; set; }
        public ISignalData RawSignalData { get;set;}
        public Dictionary<string, object> SignalData { get; set; }
        public string SignalDataAsString { get; set; }

        /// <summary>True for a direct signal from another agent (send_direct_signal), false for one emitted by a zome.</summary>
        public bool IsDirectSignal { get; set; }

        /// <summary>Holochain 0.7.1+: the agent who sent a direct signal (null for zome signals and on 0.7.0).</summary>
        public string FromAgentPubKey { get; set; }

        /// <summary>For system signals, the variant (e.g. "successful_countersigning", "restore_complete").</summary>
        public string SystemSignalType { get; set; }
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

    // New in Holochain 0.7.0 - App API.

    public class CloneCellCreatedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public ClonedCellResponse ClonedCell { get; set; }
    }

    public class CloneCellEnabledCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public ClonedCellResponse ClonedCell { get; set; }
    }

    public class CloneCellDisabledCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    public class CountersigningSessionStateReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public CountersigningSessionStateResponse SessionState { get; set; }
    }

    public class CountersigningSessionAbandonedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    public class PublishCountersigningSessionTriggeredCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    /// <summary>Result of the app interface's enable_app (Holochain 0.7: enables the app this connection is authenticated for).</summary>
    public class AppEnabledViaAppInterfaceCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
    }

    /// <summary>Result of the admin interface's dump_conductor_state: the conductor's state as JSON.</summary>
    public class ConductorStateDumpedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string DumpedStateJSON { get; set; }
    }

    /// <summary>Result of send_direct_signal (delivery is best effort; the conductor only confirms it was sent).</summary>
    public class DirectSignalSentCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
    }

    /// <summary>Result of grant_direct_signal_capability (Holochain 0.7.1+): the action hash of the committed grant.</summary>
    public class DirectSignalCapabilityGrantedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public byte[] ActionHash { get; set; }
    }

    public class WasmHostFunctionsListedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public List<string> WasmHostFunctions { get; set; } = new List<string>();
    }

    public class MemproofsProvidedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {

    }

    public class AppPeerMetaInfoReturnedCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public PeerMetaInfoResponse PeerMetaInfo { get; set; }
    }
}