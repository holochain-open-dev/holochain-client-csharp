
namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public enum HoloNETRequestType
    {
        ZomeCall,
        Signal,
        AppInfo,
        AdminGenerateAgentPubKey,
        AdminInstallApp,
        AdminUninstallApp,
        AdminEnableApp,
        AdminDisableApp,
        AdminGrantZomeCallCapability,
        AdminAttachAppInterface,
        AdminListApps,
        AdminListDnas,
        AdminListAppInterfaces,
        AdminListCellIds,
        AdminRegisterDna,
        AdminUpdateCoordinators,
        AdminDumpFullState,
        AdminDumpState,
        AdminGetDnaDefinition,
        AdminAgentInfo,
        AdminAddAgentInfo,
        AdminDeleteClonedCell,
        AdminStorageInfo,
        AdminDumpNetworkStats,
        AdminDumpNetworkMetrics,
        AdminGraftRecords,
        AdminAddAdminInterfaces,

        // New in Holochain 0.7.0 - Admin API (verified against
        // holochain_conductor_api::admin_interface::AdminRequest at tag holochain-0.7.0)
        AdminRevokeZomeCallCapability,
        AdminListCapabilityGrants,
        AdminPeerMetaInfo,
        AdminIssueAppAuthenticationToken,
        AdminRevokeAppAuthenticationToken,
        AdminGetCompatibleCells,

        // New in Holochain 0.7.0 - App API (verified against
        // holochain_conductor_api::app_interface::AppRequest at tag holochain-0.7.0)
        AppCreateCloneCell,
        AppEnableCloneCell,
        AppDisableCloneCell,
        AppGetCountersigningSessionState,
        AppAbandonCountersigningSession,
        AppPublishCountersigningSession,
        AppListWasmHostFunctions,
        AppProvideMemproofs,
        AppPeerMetaInfo,

        // New in Holochain 0.7.0 - paginated DHT op timing inspection
        AdminDumpOpTimings,
        AppDumpOpTimings,
    }
}