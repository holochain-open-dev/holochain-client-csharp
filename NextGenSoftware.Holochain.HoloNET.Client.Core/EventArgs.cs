using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETErrorEventArgs : EventArgs
    {
        public string EndPoint { get; set; }
        public string Reason { get; set; }
        public Exception ErrorDetails { get; set; }
    }

    public class HoloNETDataReceivedEventArgs : DataReceivedEventArgs
    {
        public HoloNETDataReceivedEventArgs(string id, string endPoint, bool isCallSuccessful, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult, bool isConductorDebugInfo) : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        {
            IsConductorDebugInfo = isConductorDebugInfo;
        }

        public bool IsConductorDebugInfo { get; set; }
    }


    public class ZomeFunctionCallBackEventArgs : CallBackBaseEventArgsWithId
    {
        public ZomeFunctionCallBackEventArgs() : base()
        {

        }

        public ZomeFunctionCallBackEventArgs(string id, string endPoint, string zome, string zomeFunction, bool isCallSuccessful, string rawData, Dictionary<object, object> rawZomeReturnData, Dictionary<string, object> zomeReturnData, string zomeReturnHash, Dictionary<string, string> keyValuePair, string keyValuePairAsString, EntryData entry, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult)
            : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        {
            Zome = zome;
            ZomeFunction = zomeFunction;
            RawData = rawData;
            RawZomeReturnData = rawZomeReturnData;
            ZomeReturnData = zomeReturnData;
            ZomeReturnHash = zomeReturnHash;
            KeyValuePair = keyValuePair;
            KeyValuePairAsString = keyValuePairAsString;
            Entry = entry;
        }

        public string Zome { get; set; }
        public string ZomeFunction { get; set; }
        public string RawData { get; set; }
        public Dictionary<string, object> ZomeReturnData { get; set; }
        public Dictionary<object, object> RawZomeReturnData { get; set; }
        public string ZomeReturnHash { get; set; }
        public EntryData Entry { get; set; }
        public Dictionary<string, string> KeyValuePair { get; set; }
        public string KeyValuePairAsString { get; set; }
    }

    public class AppInfoCallBackEventArgs : CallBackBaseEventArgsWithId
    {
        public AppInfoCallBackEventArgs(string id, string endPoint, bool isCallSuccessful, byte[] rawBinaryData, string rawJSONData, string dnaHash, string agentPubKey, string installedAppId, HolonNETAppInfoResponse appInfo, WebSocketReceiveResult webSocketResult)
            : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        {
            AppInfo = appInfo;
            DnaHash = dnaHash;
            AgentPubKey = agentPubKey;
            InstalledAppId = installedAppId;
        }

        public HolonNETAppInfoResponse AppInfo { get; private set; }
        public string InstalledAppId { get; private set; }
        public string DnaHash { get; private set; }
        public string AgentPubKey { get; private set; }
    }

    public class ReadyForZomeCallsEventArgs
    {
        public ReadyForZomeCallsEventArgs(string endPoint, string dnaHash, string agentPubKey)
        {
            EndPoint = endPoint;
            DnaHash = dnaHash;
            AgentPubKey = agentPubKey;
        }

        public string EndPoint { get; private set; }
        public string DnaHash { get; private set; }
        public string AgentPubKey { get; private set; }
    }

    public class SignalsCallBackEventArgs : CallBackBaseEventArgsWithId
    {
        public SignalsCallBackEventArgs(string id, string endPoint, bool isCallSuccessful, SignalTypes signalType, string name, JToken signalData, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult)
            : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        {
            this.SignalType = signalType;
            this.Name = name;
            this.SignalData = signalData;
        }

        public enum SignalTypes
        {
            User,
            Admin
        }

        //TODO: Check Signals Return Data And Add Properties Here
       public SignalTypes SignalType { get; set; }
       public string Name { get; set; }
       public JToken SignalData { get; set; }
    }

    public class ConductorDebugCallBackEventArgs 
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