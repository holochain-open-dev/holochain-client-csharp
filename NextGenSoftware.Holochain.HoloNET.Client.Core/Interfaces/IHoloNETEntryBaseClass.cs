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
        string ZomeAddToCollectionFunction { get; set; }
        string ZomeCreateEntryFunction { get; set; }
        string ZomeDeleteEntryFunction { get; set; }
        string ZomeLoadCollectionFunction { get; set; }
        string ZomeLoadEntryFunction { get; set; }
        string ZomeName { get; set; }
        string ZomeRemoveFromCollectionFunction { get; set; }
        string ZomeUpdateCollectionFunction { get; set; }
        string ZomeUpdateEntryFunction { get; set; }

        event HoloNETEntryBaseClass.Closed OnClosed;
        event HoloNETEntryBaseClass.CollectionLoaded OnCollectionLoaded;
        event HoloNETEntryBaseClass.CollectionUpdated OnCollectionUpdated;
        event HoloNETEntryBaseClass.Deleted OnDeleted;
        event HoloNETEntryBaseClass.Error OnError;
        event HoloNETEntryBaseClass.Initialized OnInitialized;
        event HoloNETEntryBaseClass.Loaded OnLoaded;
        event HoloNETEntryBaseClass.Saved OnSaved;

        HoloNETShutdownEventArgs Close(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings);
        Task<HoloNETShutdownEventArgs> CloseAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings);
        ZomeFunctionCallBackEventArgs Delete(Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Delete(string entryHash, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Delete(string customFieldToDeleteByValue, string customFieldToDeleteByKey = "", Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string customFieldToDeleteByValue, string customFieldToDeleteByKey = "", Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        void Initialize(bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task InitializeAsync(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        ZomeFunctionCallBackEventArgs Load(Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Load(string entryHash, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Load(string customFieldToLoadByValue, string customFieldToLoadByKey = "", Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(string entryHash, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(string customFieldToLoadByValue, string customFieldToLoadByKey = "", Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Save(Dictionary<string, string> customDataKeyValuePairs = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePairs = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Save(dynamic paramsObject, bool autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams = true, string updatedEntryRustParamName = "updated_entry", string originalEntryHashRustParamName = "original_action_hash", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePairs = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePairs = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> SaveAsync(dynamic paramsObject, bool autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams = true, string updatedEntryRustParamName = "updated_entry", string originalEntryHashRustParamName = "original_action_hash", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ReadyForZomeCallsEventArgs> WaitTillHoloNETInitializedAsync();
    }
}