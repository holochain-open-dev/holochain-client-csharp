
namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public enum HoloNETResponseType
    {
        ZomeResponse,
        Signal,
        AppInfo,
        AdminAgentPubKeyGenerated,
        AdminAppInstalled,
        AdminAppUninstalled,
        AdminAppEnabled,
        AdminAppDisabled,
        AdminZomeCallCapabilityGranted,
        AdminAppInterfaceAttached,
        AdminDnaRegistered,
        AdminDnaDefinitionReturned,
        AdminAppInterfacesListed,
        AdminAppsListed,
        AdminDnasListed,
        AdminCellIdsListed,
        AdminAgentInfoReturned,
        AdminAgentInfoAdded,
        AdminCoordinatorsUpdated,
        AdminCloneCellDeleted,
        AdminStateDumped,
        AdminFullStateDumped,
        AdminNetworkMetricsDumped,
        AdminNetworkStatsDumped,
        AdminStorageInfoReturned,
        AdminRecordsGrafted,
        AdminAdminInterfacesAdded,

        // New in Holochain 0.7.0 - Admin API
        AdminZomeCallCapabilityRevoked,
        AdminCapabilityGrantsInfoReturned,
        AdminPeerMetaInfoReturned,
        AdminAppAuthenticationTokenIssued,
        AdminAppAuthenticationTokenRevoked,
        AdminCompatibleCellsReturned,

        // New in Holochain 0.7.0 - App API
        AppCloneCellCreated,
        AppCloneCellEnabled,
        AppCloneCellDisabled,
        AppCountersigningSessionStateReturned,
        AppCountersigningSessionAbandoned,
        AppPublishCountersigningSessionTriggered,
        AppWasmHostFunctionsListed,
        AppMemproofsProvided,
        AppPeerMetaInfoReturned,

        // New in Holochain 0.7.0 - paginated DHT op timing inspection
        AdminOpTimingsDumped,
        AppOpTimingsDumped,

        Error
    }
}