using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETDataReceivedBaseEventArgs : CallBackBaseEventArgsWithId
    {
        //public string RawBinaryDataAsString { get; set; }
        //public string RawBinaryDataDecoded { get; set; }
        public byte[] RawBinaryDataAfterMessagePackDecode { get; set; }
        public string RawBinaryDataAfterMessagePackDecodeAsString { get; set; }
        public string RawBinaryDataAfterMessagePackDecodeDecoded { get; set; }
    }

    public class HoloNETErrorEventArgs : EventArgs
    {
        public string EndPoint { get; set; }
        public string Reason { get; set; }
        public Exception ErrorDetails { get; set; }
    }

    //public class HoloNETDataReceivedEventArgs : DataReceivedEventArgs
    public class HoloNETDataReceivedEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        //public HoloNETDataReceivedEventArgs(string id, string endPoint, bool isCallSuccessful, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult, bool isConductorDebugInfo, byte[] rawBinaryDataAfterMessagePackDecode, string rawBinaryDataAfterMessagePackDecodeAsString) : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        //{
        //    IsConductorDebugInfo = isConductorDebugInfo;
        //    RawBinaryDataAfterMessagePackDecode = rawBinaryDataAfterMessagePackDecode;
        //    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString;
        //}

        //public byte[] RawBinaryDataAfterMessagePackDecode { get; set; }
        //public string RawBinaryDataAfterMessagePackDecodeAsString { get; set; } 
        public bool IsConductorDebugInfo { get; set; }
    }


    public class ZomeFunctionCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public ZomeFunctionCallBackEventArgs() : base()
        {

        }

        //public ZomeFunctionCallBackEventArgs(string id, string endPoint, string zome, string zomeFunction, bool isCallSuccessful, string rawData, Dictionary<object, object> rawZomeReturnData, Dictionary<string, object> zomeReturnData, string zomeReturnHash, Dictionary<string, string> keyValuePair, string keyValuePairAsString, EntryData entry, byte[] rawBinaryData, string rawJSONData, byte[] rawBinaryDataAfterMessagePackDecode, string rawBinaryDataAfterMessagePackDecodeAsString, WebSocketReceiveResult webSocketResult)
        //    : base(id, endPoint, isCallSuccessful, rawBinaryData, rawBinaryDataAfterMessagePackDecode, rawBinaryDataAfterMessagePackDecodeAsString, rawJSONData, webSocketResult)
        //{
        //    Zome = zome;
        //    ZomeFunction = zomeFunction;
        //    RawData = rawData;
        //    RawZomeReturnData = rawZomeReturnData;
        //    ZomeReturnData = zomeReturnData;
        //    ZomeReturnHash = zomeReturnHash;
        //    KeyValuePair = keyValuePair;
        //    KeyValuePairAsString = keyValuePairAsString;
        //    Entry = entry;
        //}

        public string Zome { get; set; }
        public string ZomeFunction { get; set; }
        //public string RawData { get; set; }
        public Dictionary<string, object> ZomeReturnData { get; set; }
        public Dictionary<object, object> RawZomeReturnData { get; set; }
        public string ZomeReturnHash { get; set; }
        public EntryData Entry { get; set; }
        public Dictionary<string, string> KeyValuePair { get; set; }
        public string KeyValuePairAsString { get; set; }
    }

    public class AppInfoCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        //public AppInfoCallBackEventArgs(string id, string endPoint, bool isCallSuccessful, byte[] rawBinaryData, string rawJSONData, string dnaHash, string agentPubKey, string installedAppId, HolonNETAppInfoResponse appInfo, WebSocketReceiveResult webSocketResult)
        //    : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        //{
        //    AppInfo = appInfo;
        //    DnaHash = dnaHash;
        //    AgentPubKey = agentPubKey;
        //    InstalledAppId = installedAppId;
        //}

        public HolonNETAppInfoResponse AppInfo { get; set; }
        public string InstalledAppId { get; set; }
        public string DnaHash { get; set; }
        public string AgentPubKey { get; set; }
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

    public class SignalsCallBackEventArgs : HoloNETDataReceivedBaseEventArgs
    {
        public string AgentPubKey { get; set; }
        public string DnaHash { get; set; }
        public SignalType SignalType { get; set; }
        public HoloNETSignalData RawSignalData { get;set;}
        public Dictionary<string, object> SignalData { get; set; } 
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