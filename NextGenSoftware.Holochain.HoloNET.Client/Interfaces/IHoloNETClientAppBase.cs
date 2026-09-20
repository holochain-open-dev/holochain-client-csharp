using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client.Interfaces
{
    public interface IHoloNETClientAppBase : IHoloNETClientBase
    {
        AppInfo CachedAppInfo { get; set; }
        bool IsReadyForZomesCalls { get; }
        bool RetrievingAgentPubKeyAndDnaHash { get; }

        event HoloNETClientAppBase.AppInfoCallBack OnAppInfoCallBack;
        event HoloNETClientAppBase.ReadyForZomeCalls OnReadyForZomeCalls;
        event HoloNETClientAppBase.SignalCallBack OnSignalCallBack;
        event HoloNETClientAppBase.ZomeFunctionCallBack OnZomeFunctionCallBack;

        // New in Holochain 0.7.0 - App API.
        event HoloNETClientAppBase.CloneCellCreatedCallBack OnCloneCellCreatedCallBack;
        event HoloNETClientAppBase.CloneCellEnabledCallBack OnCloneCellEnabledCallBack;
        event HoloNETClientAppBase.CloneCellDisabledCallBack OnCloneCellDisabledCallBack;
        event HoloNETClientAppBase.CountersigningSessionStateReturnedCallBack OnCountersigningSessionStateReturnedCallBack;
        event HoloNETClientAppBase.CountersigningSessionAbandonedCallBack OnCountersigningSessionAbandonedCallBack;
        event HoloNETClientAppBase.PublishCountersigningSessionTriggeredCallBack OnPublishCountersigningSessionTriggeredCallBack;
        event HoloNETClientAppBase.WasmHostFunctionsListedCallBack OnWasmHostFunctionsListedCallBack;
        event HoloNETClientAppBase.MemproofsProvidedCallBack OnMemproofsProvidedCallBack;
        event HoloNETClientAppBase.AppPeerMetaInfoReturnedCallBack OnAppPeerMetaInfoReturnedCallBack;

        CloneCellCreatedCallBackEventArgs CreateCloneCell(string roleName, dynamic modifiers = null, byte[] membraneProof = null, string name = null, string id = null);
        Task<CloneCellCreatedCallBackEventArgs> CreateCloneCellAsync(string roleName, dynamic modifiers = null, byte[] membraneProof = null, string name = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);
        CloneCellEnabledCallBackEventArgs EnableCloneCell(dynamic cloneCellId, string id = null);
        Task<CloneCellEnabledCallBackEventArgs> EnableCloneCellAsync(dynamic cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);
        CloneCellDisabledCallBackEventArgs DisableCloneCell(dynamic cloneCellId, string id = null);
        Task<CloneCellDisabledCallBackEventArgs> DisableCloneCellAsync(dynamic cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);
        CountersigningSessionStateReturnedCallBackEventArgs GetCountersigningSessionState(CellId cellId, string id = null);
        Task<CountersigningSessionStateReturnedCallBackEventArgs> GetCountersigningSessionStateAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);
        CountersigningSessionAbandonedCallBackEventArgs AbandonCountersigningSession(CellId cellId, string id = null);
        Task<CountersigningSessionAbandonedCallBackEventArgs> AbandonCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);
        PublishCountersigningSessionTriggeredCallBackEventArgs PublishCountersigningSession(CellId cellId, string id = null);
        Task<PublishCountersigningSessionTriggeredCallBackEventArgs> PublishCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);
        WasmHostFunctionsListedCallBackEventArgs ListWasmHostFunctions(string id = null);
        Task<WasmHostFunctionsListedCallBackEventArgs> ListWasmHostFunctionsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);
        MemproofsProvidedCallBackEventArgs ProvideMemproofs(Dictionary<string, byte[]> membraneProofs, string id = null);
        Task<MemproofsProvidedCallBackEventArgs> ProvideMemproofsAsync(Dictionary<string, byte[]> membraneProofs, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);
        AppPeerMetaInfoReturnedCallBackEventArgs GetAppPeerMetaInfo(string url, List<byte[]> dnaHashes = null, string id = null);
        Task<AppPeerMetaInfoReturnedCallBackEventArgs> GetAppPeerMetaInfoAsync(string url, List<byte[]> dnaHashes = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null);

        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false);
        ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, CallZomeOptions callZomeOptions = null);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        void ClearCache(bool clearPendingRequsts = false);
        HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task<HoloNETConnectedEventArgs> ConnectAsync(string installedAppId, Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task<AppInfoCallBackEventArgs> GetAppInfo(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<AppInfoCallBackEventArgs> GetAppInfoAsync(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        Task<byte[][]> GetCellIdAsync();
        dynamic MapEntryDataObject(dynamic entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true);
        dynamic MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true);
        Task<dynamic> MapEntryDataObjectAsync(dynamic entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true);
        Task<dynamic> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true);
        AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHash(string installedAppId = null, string roleName = null, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashAsync(string installedAppId = null, string roleName = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromConductor(string installedAppId = null, string roleName = null, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true);
        Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromConductorAsync(string installedAppId = null, string roleName = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true);
        AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromSandbox(bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true);
        Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true);
        Task<ReadyForZomeCallsEventArgs> WaitTillReadyForZomeCallsAsync();
    }
}