using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public interface IHoloNETEntryBaseClass
    {
        EntryData EntryData { get; set; }
        string EntryHash { get; set; }
        HoloNETClient HoloNETClient { get; set; }
        bool IsInitialized { get; }
        bool IsInitializing { get; }
        string PreviousVersionEntryHash { get; set; }
        string ZomeCreateCollectionFunction { get; set; }
        string ZomeCreateEntryFunction { get; set; }
        string ZomeDeleteCollectionFunction { get; set; }
        string ZomeDeleteEntryFunction { get; set; }
        string ZomeLoadCollectionFunction { get; set; }
        string ZomeLoadEntryFunction { get; set; }
        string ZomeName { get; set; }
        string ZomeUpdateCollectionFunction { get; set; }
        string ZomeUpdateEntryFunction { get; set; }

        event HoloNETEntryBaseClass.Closed OnClosed;
        event HoloNETEntryBaseClass.Deleted OnDeleted;
        event HoloNETEntryBaseClass.Error OnError;
        event HoloNETEntryBaseClass.Initialized OnInitialized;
        event HoloNETEntryBaseClass.Loaded OnLoaded;
        event HoloNETEntryBaseClass.Saved OnSaved;

        HoloNETShutdownEventArgs Close(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        Task<HoloNETShutdownEventArgs> CloseAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        ZomeFunctionCallBackEventArgs Delete(bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Delete(object customFieldToDeleteByValue, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Delete(string entryHash, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(object customFieldToDeleteByValue, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        void Initialize(bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task InitializeAsync(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        ZomeFunctionCallBackEventArgs Load(bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Load(object customFieldToLoadByValue, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Load(string entryHash, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(object customFieldToLoadByValue, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(string entryHash, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Save(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Save(dynamic paramsObject, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> SaveAsync(dynamic paramsObject, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ReadyForZomeCallsEventArgs> WaitTillHoloNETInitializedAsync();
    }
}