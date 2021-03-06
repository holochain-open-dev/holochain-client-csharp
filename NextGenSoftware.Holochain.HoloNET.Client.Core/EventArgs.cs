using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client.Core
{
    public class HoloNETErrorEventArgs : EventArgs
    {
        public string EndPoint { get; set; }
        public string Reason { get; set; }
        public Exception ErrorDetails { get; set; }
    }

    public class HoloNETDataReceivedEventArgs : DataReceivedEventArgs
    {
        public bool IsConductorDebugInfo { get; set; }
    }


    public class ZomeFunctionCallBackEventArgs : CallBackBaseEventArgs
    {
        public ZomeFunctionCallBackEventArgs(string id, string endPoint, string instance, string zome, string zomeFunction, bool isCallSuccessful, string rawData, Dictionary<object, object> rawZomeReturnData, Dictionary<string, object> zomeReturnData, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult)
            : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        {
            Instance = instance;
            Zome = zome;
            ZomeFunction = zomeFunction;
            RawData = rawData;
            RawZomeReturnData = rawZomeReturnData;
            ZomeReturnData = zomeReturnData;
        }

        public string Instance { get; private set; }
        public string Zome { get; private set; }
        public string ZomeFunction { get; private set; }
        public string RawData { get; private set; }
        public Dictionary<string, object> ZomeReturnData { get; private set; }
        public Dictionary<object, object> RawZomeReturnData { get; private set; }
    }

    public class AppInfoCallBackEventArgs : CallBackBaseEventArgs
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
        public ReadyForZomeCallsEventArgs(string dnaHash, string agentPubKey)
        {
            DnaHash = dnaHash;
            AgentPubKey = agentPubKey;
        }

        public HolonNETAppInfoResponse AppInfo { get; private set; }
        public string InstalledAppId { get; private set; }
        public string DnaHash { get; private set; }
        public string AgentPubKey { get; private set; }
    }

    public class SignalsCallBackEventArgs : CallBackBaseEventArgs
    {
        public SignalsCallBackEventArgs(string id, string endPoint, bool isCallSuccessful, SignalTypes signalType, string name, JToken args, byte[] rawBinaryData, string rawJSONData, WebSocketReceiveResult webSocketResult)
            : base(id, endPoint, isCallSuccessful, rawBinaryData, rawJSONData, webSocketResult)
        {
            this.SignalType = signalType;
            this.Name = name;
            this.Arguments = args;
        }

        public enum SignalTypes
        {
            User
        }

        //TODO: Check Signals Return Data And Add Properties Here
       public SignalTypes SignalType { get; set; }
       public string Name { get; set; }
       public JToken Arguments { get; set; }
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