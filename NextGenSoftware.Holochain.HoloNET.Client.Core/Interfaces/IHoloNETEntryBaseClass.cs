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
        event HoloNETEntryBaseClass.Deleted OnDeleted;
        event HoloNETEntryBaseClass.Error OnError;
        event HoloNETEntryBaseClass.Initialized OnInitialized;
        event HoloNETEntryBaseClass.Loaded OnLoaded;
        event HoloNETEntryBaseClass.Saved OnSaved;

        HoloNETShutdownEventArgs Close(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        Task<HoloNETShutdownEventArgs> CloseAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings);
        ZomeFunctionCallBackEventArgs Delete(bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        ZomeFunctionCallBackEventArgs Delete(object customFieldToDeleteByValue, string customFieldToDeleteByName = "defaultKey", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        ZomeFunctionCallBackEventArgs Delete(string entryHash, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(object customFieldToDeleteByValue, string customFieldToDeleteByName = "defaultKey", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        void Initialize(bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        Task InitializeAsync(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true);
        ZomeFunctionCallBackEventArgs Load(bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        ZomeFunctionCallBackEventArgs Load(object customFieldToLoadByValue, string customFieldToLoadByName = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        ZomeFunctionCallBackEventArgs Load(string entryHash, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(object customFieldToLoadByValue, string customFieldToLoadByName = "defaultKey", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        Task<ZomeFunctionCallBackEventArgs> LoadAsync(string entryHash, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true, dynamic additionalParams = null);
        ZomeFunctionCallBackEventArgs Save(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        ZomeFunctionCallBackEventArgs Save(dynamic paramsObject, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ZomeFunctionCallBackEventArgs> SaveAsync(dynamic paramsObject, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true);
        Task<ReadyForZomeCallsEventArgs> WaitTillHoloNETInitializedAsync();
    }
}