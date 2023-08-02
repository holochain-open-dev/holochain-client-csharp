using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public interface IHoloNETClient
    {
        HoloNETConfig Config { get; set; }
        string EndPoint { get; }
        bool IsReadyForZomesCalls { get; set; }
        bool RetrievingAgentPubKeyAndDnaHash { get; }
        WebSocketState State { get; }
        WebSocket.WebSocket WebSocket { get; set; }

        event HoloNETClient.AppInfoCallBack OnAppInfoCallBack;
        event HoloNETClient.Connected OnConnected;
        event HoloNETClient.DataReceived OnDataReceived;
        event HoloNETClient.Disconnected OnDisconnected;
        event HoloNETClient.Error OnError;
        event HoloNETClient.HolochainConductorsShutdownComplete OnHolochainConductorsShutdownComplete;
        event HoloNETClient.HoloNETShutdownComplete OnHoloNETShutdownComplete;
        event HoloNETClient.ReadyForZomeCalls OnReadyForZomeCalls;
        event HoloNETClient.SignalCallBack OnSignalCallBack;
        event HoloNETClient.ZomeFunctionCallBack OnZomeFunctionCallBack;

        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClient.ZomeFunctionCallBack callback, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        void ClearCache(bool clearPendingRequsts = false);
        void Connect(bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task ConnectAsync(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        byte[] ConvertHoloHashToBytes(string hash);
        string ConvertHoloHashToString(byte[] bytes);
        void Disconnect(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        Task DisconnectAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        dynamic MapEntryDataObject(dynamic entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true);
        dynamic MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true);
        Task<dynamic> MapEntryDataObjectAsync(dynamic entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true);
        Task<dynamic> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true);
        AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHash(bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashAsync(RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromConductor(bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true);
        Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromConductorAsync(RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true);
        AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromSandbox(bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true);
        Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true);
        void SendHoloNETRequest(HoloNETData holoNETData, string id = "");
        Task SendHoloNETRequestAsync(HoloNETData holoNETData, string id = "");
        HolochainConductorsShutdownEventArgs ShutDownHolochainConductors(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        Task<HolochainConductorsShutdownEventArgs> ShutDownHolochainConductorsAsync(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        HoloNETShutdownEventArgs ShutdownHoloNET(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        Task<HoloNETShutdownEventArgs> ShutdownHoloNETAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        void StartHolochainConductor();
        Task StartHolochainConductorAsync();
        Task<ReadyForZomeCallsEventArgs> WaitTillReadyForZomeCallsAsync();
    }
}