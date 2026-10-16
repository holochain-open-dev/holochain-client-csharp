# HoloNET Client API reference

Generated from `NextGenSoftware.Holochain.HoloNET.Client` 4.4.0.0 (every public type and member, with its XML documentation where it has one). For guides and examples see the [documentation index](../README.md).

- **NextGenSoftware.Holochain.HoloNET.Client**: [Action](#nextgensoftware-holochain-holonet-client-action), [HoloNETClientAdmin.AdminInterfacesAddedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-admininterfacesaddedcallback), [AdminInterfacesAddedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-admininterfacesaddedcallbackeventargs), [HoloNETClientAdmin.AdminOpTimingsDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-adminoptimingsdumpedcallback), [AdminOpTimingsDumpedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-adminoptimingsdumpedcallbackeventargs), [HoloNETClientAdmin.AgentInfoAddedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-agentinfoaddedcallback), [AgentInfoAddedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-agentinfoaddedcallbackeventargs), [AgentInfoDump](#nextgensoftware-holochain-holonet-client-agentinfodump), [HoloNETClientAdmin.AgentInfoReturnedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-agentinforeturnedcallback), [AgentInfoReturnedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-agentinforeturnedcallbackeventargs), [AgentPubKeyDnaHash](#nextgensoftware-holochain-holonet-client-agentpubkeydnahash), [HoloNETClientAdmin.AgentPubKeyGeneratedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-agentpubkeygeneratedcallback), [AgentPubKeyGeneratedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-agentpubkeygeneratedcallbackeventargs), [HoloNETClientAppBase.AppAgentInfoReturnedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-appagentinforeturnedcallback), [HoloNETClientAdmin.AppAuthenticationTokenIssuedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appauthenticationtokenissuedcallback), [AppAuthenticationTokenIssuedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appauthenticationtokenissuedcallbackeventargs), [AppAuthenticationTokenIssuedResponse](#nextgensoftware-holochain-holonet-client-appauthenticationtokenissuedresponse), [HoloNETClientAdmin.AppAuthenticationTokenRevokedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appauthenticationtokenrevokedcallback), [AppAuthenticationTokenRevokedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appauthenticationtokenrevokedcallbackeventargs), [AppCompatibleCells](#nextgensoftware-holochain-holonet-client-appcompatiblecells), [HoloNETClientAdmin.AppDisabledCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appdisabledcallback), [AppDisabledCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appdisabledcallbackeventargs), [HoloNETClientAdmin.AppEnabledCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appenabledcallback), [AppEnabledCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appenabledcallbackeventargs), [HoloNETClientAppBase.AppEnabledViaAppInterfaceCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-appenabledviaappinterfacecallback), [AppEnabledViaAppInterfaceCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appenabledviaappinterfacecallbackeventargs), [AppInfo](#nextgensoftware-holochain-holonet-client-appinfo), [HoloNETClientAppBase.AppInfoCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-appinfocallback), [AppInfoCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appinfocallbackeventargs), [AppInfoRequest](#nextgensoftware-holochain-holonet-client-appinforequest), [AppInfoResponse](#nextgensoftware-holochain-holonet-client-appinforesponse), [AppInfoStatusEnum](#nextgensoftware-holochain-holonet-client-appinfostatusenum), [HoloNETClientAdmin.AppInstalledCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appinstalledcallback), [AppInstalledCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appinstalledcallbackeventargs), [HoloNETClientAdmin.AppInterfaceAttachedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appinterfaceattachedcallback), [AppInterfaceAttachedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appinterfaceattachedcallbackeventargs), [AppInterfaceInfo](#nextgensoftware-holochain-holonet-client-appinterfaceinfo), [HoloNETClientAdmin.AppInterfacesListedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appinterfaceslistedcallback), [AppInterfacesListedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appinterfaceslistedcallbackeventargs), [HoloNETClientAppBase.AppNetworkMetricsDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-appnetworkmetricsdumpedcallback), [HoloNETClientAppBase.AppNetworkStatsDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-appnetworkstatsdumpedcallback), [HoloNETClientAppBase.AppOpTimingsDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-appoptimingsdumpedcallback), [AppOpTimingsDumpedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appoptimingsdumpedcallbackeventargs), [HoloNETClientAppBase.AppPeerMetaInfoReturnedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-apppeermetainforeturnedcallback), [AppPeerMetaInfoReturnedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-apppeermetainforeturnedcallbackeventargs), [AppResponse](#nextgensoftware-holochain-holonet-client-appresponse), [HoloNETClientAdmin.AppsListedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appslistedcallback), [AppsListedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appslistedcallbackeventargs), [AppStatusFilter](#nextgensoftware-holochain-holonet-client-appstatusfilter), [AppStatusFilterExtensions](#nextgensoftware-holochain-holonet-client-appstatusfilterextensions), [HoloNETClientAdmin.AppUninstalledCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-appuninstalledcallback), [AppUninstalledCallBackEventArgs](#nextgensoftware-holochain-holonet-client-appuninstalledcallbackeventargs), [AttachAppInterfaceResponse](#nextgensoftware-holochain-holonet-client-attachappinterfaceresponse), [ByteArrayAsIntArrayFormatter](#nextgensoftware-holochain-holonet-client-bytearrayasintarrayformatter), [CacheConfig](#nextgensoftware-holochain-holonet-client-cacheconfig), [CacheEvictionPolicy](#nextgensoftware-holochain-holonet-client-cacheevictionpolicy), [CallZomeOptions](#nextgensoftware-holochain-holonet-client-callzomeoptions), [HoloNETClientAdmin.CapabilityGrantsListedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-capabilitygrantslistedcallback), [CapabilityGrantsListedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-capabilitygrantslistedcallbackeventargs), [CapAccessInfo](#nextgensoftware-holochain-holonet-client-capaccessinfo), [CapGrantAccessType](#nextgensoftware-holochain-holonet-client-capgrantaccesstype), [CapGrantInfo](#nextgensoftware-holochain-holonet-client-capgrantinfo), [CellCapGrantInfo](#nextgensoftware-holochain-holonet-client-cellcapgrantinfo), [CellId](#nextgensoftware-holochain-holonet-client-cellid), [HoloNETClientAdmin.CellIdsListedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-cellidslistedcallback), [CellIdsListedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-cellidslistedcallbackeventargs), [CellInfoType](#nextgensoftware-holochain-holonet-client-cellinfotype), [HoloNETClientAppBase.CloneCellCreatedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-clonecellcreatedcallback), [CloneCellCreatedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-clonecellcreatedcallbackeventargs), [HoloNETClientAdmin.CloneCellDeletedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-clonecelldeletedcallback), [CloneCellDeletedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-clonecelldeletedcallbackeventargs), [HoloNETClientAppBase.CloneCellDisabledCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-clonecelldisabledcallback), [CloneCellDisabledCallBackEventArgs](#nextgensoftware-holochain-holonet-client-clonecelldisabledcallbackeventargs), [HoloNETClientAppBase.CloneCellEnabledCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-clonecellenabledcallback), [CloneCellEnabledCallBackEventArgs](#nextgensoftware-holochain-holonet-client-clonecellenabledcallbackeventargs), [CloneCellId](#nextgensoftware-holochain-holonet-client-clonecellid), [ClonedCellResponse](#nextgensoftware-holochain-holonet-client-clonedcellresponse), [HoloNETClientAdmin.CompatibleCellsReturnedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-compatiblecellsreturnedcallback), [CompatibleCellsReturnedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-compatiblecellsreturnedcallbackeventargs), [ConductorDebugCallBackEventArgs](#nextgensoftware-holochain-holonet-client-conductordebugcallbackeventargs), [ConductorResponseCallBackMode](#nextgensoftware-holochain-holonet-client-conductorresponsecallbackmode), [HoloNETClientAdmin.ConductorStateDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-conductorstatedumpedcallback), [ConductorStateDumpedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-conductorstatedumpedcallbackeventargs), [HoloNETClientBase.Connected](#nextgensoftware-holochain-holonet-client-holonetclientbase-connected), [ConnectedCallBackMode](#nextgensoftware-holochain-holonet-client-connectedcallbackmode), [CoordinatorSource](#nextgensoftware-holochain-holonet-client-coordinatorsource), [HoloNETClientAdmin.CoordinatorsUpdatedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-coordinatorsupdatedcallback), [CoordinatorsUpdatedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-coordinatorsupdatedcallbackeventargs), [CountersigningCellIdRequest](#nextgensoftware-holochain-holonet-client-countersigningcellidrequest), [HoloNETClientAppBase.CountersigningSessionAbandonedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-countersigningsessionabandonedcallback), [CountersigningSessionAbandonedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-countersigningsessionabandonedcallbackeventargs), [CountersigningSessionStateResponse](#nextgensoftware-holochain-holonet-client-countersigningsessionstateresponse), [HoloNETClientAppBase.CountersigningSessionStateReturnedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-countersigningsessionstatereturnedcallback), [CountersigningSessionStateReturnedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-countersigningsessionstatereturnedcallbackeventargs), [CreateCloneCellRequest](#nextgensoftware-holochain-holonet-client-createclonecellrequest), [HoloNETClientBase.DataReceived](#nextgensoftware-holochain-holonet-client-holonetclientbase-datareceived), [HoloNETClientBase.DataSent](#nextgensoftware-holochain-holonet-client-holonetclientbase-datasent), [DesensitizedZomeCallCapGrant](#nextgensoftware-holochain-holonet-client-desensitizedzomecallcapgrant), [DhtOp](#nextgensoftware-holochain-holonet-client-dhtop), [DhtOpsCursor](#nextgensoftware-holochain-holonet-client-dhtopscursor), [HoloNETClientAdmin.DirectSignalCapabilityGrantedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-directsignalcapabilitygrantedcallback), [HoloNETClientAppBase.DirectSignalCapabilityGrantedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-directsignalcapabilitygrantedcallback), [DirectSignalCapabilityGrantedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-directsignalcapabilitygrantedcallbackeventargs), [HoloNETClientAppBase.DirectSignalSentCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-directsignalsentcallback), [DirectSignalSentCallBackEventArgs](#nextgensoftware-holochain-holonet-client-directsignalsentcallbackeventargs), [DisableCloneCellRequest](#nextgensoftware-holochain-holonet-client-disableclonecellrequest), [DisabledAppReasonType](#nextgensoftware-holochain-holonet-client-disabledappreasontype), [HoloNETClientBase.Disconnected](#nextgensoftware-holochain-holonet-client-holonetclientbase-disconnected), [DisconnectedCallBackMode](#nextgensoftware-holochain-holonet-client-disconnectedcallbackmode), [DnaDef](#nextgensoftware-holochain-holonet-client-dnadef), [DnaDefinition](#nextgensoftware-holochain-holonet-client-dnadefinition), [DnaDefinitionResponse](#nextgensoftware-holochain-holonet-client-dnadefinitionresponse), [DnaDefinitionResponseDetail](#nextgensoftware-holochain-holonet-client-dnadefinitionresponsedetail), [HoloNETClientAdmin.DnaDefinitionReturnedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-dnadefinitionreturnedcallback), [DnaDefinitionReturnedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-dnadefinitionreturnedcallbackeventargs), [HoloNETClientAdmin.DnaRegisteredCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-dnaregisteredcallback), [DnaRegisteredCallBackEventArgs](#nextgensoftware-holochain-holonet-client-dnaregisteredcallbackeventargs), [HoloNETClientAdmin.DnasListedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-dnaslistedcallback), [DnasListedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-dnaslistedcallbackeventargs), [DnaStorageBlob](#nextgensoftware-holochain-holonet-client-dnastorageblob), [DnaStorageInfo](#nextgensoftware-holochain-holonet-client-dnastorageinfo), [DumpNetworkStatsResponse](#nextgensoftware-holochain-holonet-client-dumpnetworkstatsresponse), [DumpOpTimingsRequest](#nextgensoftware-holochain-holonet-client-dumpoptimingsrequest), [EmbeddedHolochainVersion](#nextgensoftware-holochain-holonet-client-embeddedholochainversion), [EnableAppError](#nextgensoftware-holochain-holonet-client-enableapperror), [EnableAppResponse](#nextgensoftware-holochain-holonet-client-enableappresponse), [EnableAppResponseDetails](#nextgensoftware-holochain-holonet-client-enableappresponsedetails), [EnableCloneCellRequest](#nextgensoftware-holochain-holonet-client-enableclonecellrequest), [EnforceRequestToResponseIdMatchingBehaviour](#nextgensoftware-holochain-holonet-client-enforcerequesttoresponseidmatchingbehaviour), [HoloNETClientBase.Error](#nextgensoftware-holochain-holonet-client-holonetclientbase-error), [ErrorHandlingBehaviour](#nextgensoftware-holochain-holonet-client-errorhandlingbehaviour), [FullIntegrationStateDump](#nextgensoftware-holochain-holonet-client-fullintegrationstatedump), [HoloNETClientAdmin.FullStateDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-fullstatedumpedcallback), [FullStateDumpedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-fullstatedumpedcallbackeventargs), [FullStateDumpedResponse](#nextgensoftware-holochain-holonet-client-fullstatedumpedresponse), [GetAppInfoCallBackEventArgs](#nextgensoftware-holochain-holonet-client-getappinfocallbackeventargs), [HolochainConductorEnum](#nextgensoftware-holochain-holonet-client-holochainconductorenum), [HolochainConductorModeEnum](#nextgensoftware-holochain-holonet-client-holochainconductormodeenum), [HoloNETClientBase.HolochainConductorsShutdownComplete](#nextgensoftware-holochain-holonet-client-holonetclientbase-holochainconductorsshutdowncomplete), [HolochainConductorsShutdownEventArgs](#nextgensoftware-holochain-holonet-client-holochainconductorsshutdowneventargs), [HoloNETClientBase.HolochainConductorStarted](#nextgensoftware-holochain-holonet-client-holonetclientbase-holochainconductorstarted), [HolochainConductorStartedEventArgs](#nextgensoftware-holochain-holonet-client-holochainconductorstartedeventargs), [HoloNETClientBase.HolochainConductorStarting](#nextgensoftware-holochain-holonet-client-holonetclientbase-holochainconductorstarting), [HolochainConductorStartingEventArgs](#nextgensoftware-holochain-holonet-client-holochainconductorstartingeventargs), [HolochainRustFieldName](#nextgensoftware-holochain-holonet-client-holochainrustfieldname), [HolochainSerialisationWrapper](#nextgensoftware-holochain-holonet-client-holochainserialisationwrapper), [HoloNETAuditEntryType](#nextgensoftware-holochain-holonet-client-holonetauditentrytype), [HoloNETClientAdmin](#nextgensoftware-holochain-holonet-client-holonetclientadmin), [HoloNETClientApp](#nextgensoftware-holochain-holonet-client-holonetclientapp), [HoloNETClientAppAgent](#nextgensoftware-holochain-holonet-client-holonetclientappagent), [HoloNETClientAppBase](#nextgensoftware-holochain-holonet-client-holonetclientappbase), [HoloNETClientBase](#nextgensoftware-holochain-holonet-client-holonetclientbase), [HoloNETConnectedEventArgs](#nextgensoftware-holochain-holonet-client-holonetconnectedeventargs), [HoloNETData](#nextgensoftware-holochain-holonet-client-holonetdata), [HoloNETDataReceivedBaseBaseEventArgs&lt;T&gt;](#nextgensoftware-holochain-holonet-client-holonetdatareceivedbasebaseeventargs-1), [HoloNETDataReceivedBaseEventArgs](#nextgensoftware-holochain-holonet-client-holonetdatareceivedbaseeventargs), [HoloNETDataReceivedEventArgs](#nextgensoftware-holochain-holonet-client-holonetdatareceivedeventargs), [HoloNETDataSentEventArgs](#nextgensoftware-holochain-holonet-client-holonetdatasenteventargs), [HoloNETDisconnectedEventArgs](#nextgensoftware-holochain-holonet-client-holonetdisconnectedeventargs), [HoloNETDNA](#nextgensoftware-holochain-holonet-client-holonetdna), [HoloNETDNAManager](#nextgensoftware-holochain-holonet-client-holonetdnamanager), [HoloNETErrorEventArgs](#nextgensoftware-holochain-holonet-client-holoneterroreventargs), [HoloNETException](#nextgensoftware-holochain-holonet-client-holonetexception), [HoloNETRequest](#nextgensoftware-holochain-holonet-client-holonetrequest), [HoloNETRequestType](#nextgensoftware-holochain-holonet-client-holonetrequesttype), [HoloNETResponse](#nextgensoftware-holochain-holonet-client-holonetresponse), [HoloNETResponseType](#nextgensoftware-holochain-holonet-client-holonetresponsetype), [HoloNETClientBase.HoloNETShutdownComplete](#nextgensoftware-holochain-holonet-client-holonetclientbase-holonetshutdowncomplete), [HoloNETShutdownEventArgs](#nextgensoftware-holochain-holonet-client-holonetshutdowneventargs), [InstalledCell](#nextgensoftware-holochain-holonet-client-installedcell), [HoloNETClientAdmin.InstallEnableSignAndAttachHappCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-installenablesignandattachhappcallback), [InstallEnableSignAndAttachHappEventArgs](#nextgensoftware-holochain-holonet-client-installenablesignandattachhappeventargs), [HoloNETClientAdmin.InstallEnableSignAttachAndConnectToHappCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-installenablesignattachandconnecttohappcallback), [InstallEnableSignAttachAndConnectToHappEventArgs](#nextgensoftware-holochain-holonet-client-installenablesignattachandconnecttohappeventargs), [IZomeCallSigner](#nextgensoftware-holochain-holonet-client-izomecallsigner), [KeystoreConfig](#nextgensoftware-holochain-holonet-client-keystoreconfig), [KeystoreConfigType](#nextgensoftware-holochain-holonet-client-keystoreconfigtype), [Kitsune2Config](#nextgensoftware-holochain-holonet-client-kitsune2config), [ListAppsResponse](#nextgensoftware-holochain-holonet-client-listappsresponse), [HoloNETClientAppBase.MemproofsProvidedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-memproofsprovidedcallback), [MemproofsProvidedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-memproofsprovidedcallbackeventargs), [NetworkConfig](#nextgensoftware-holochain-holonet-client-networkconfig), [HoloNETClientAdmin.NetworkMetricsDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-networkmetricsdumpedcallback), [NetworkMetricsDumpedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-networkmetricsdumpedcallbackeventargs), [HoloNETClientAdmin.NetworkStatsDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-networkstatsdumpedcallback), [NetworkStatsDumpedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-networkstatsdumpedcallbackeventargs), [NewEntryAction](#nextgensoftware-holochain-holonet-client-newentryaction), [OpTimingDump](#nextgensoftware-holochain-holonet-client-optimingdump), [OpTimingsCursor](#nextgensoftware-holochain-holonet-client-optimingscursor), [OpTimingsDump](#nextgensoftware-holochain-holonet-client-optimingsdump), [P2pAgentsDump](#nextgensoftware-holochain-holonet-client-p2pagentsdump), [PausedAppReasonType](#nextgensoftware-holochain-holonet-client-pausedappreasontype), [PeerMetaInfoResponse](#nextgensoftware-holochain-holonet-client-peermetainforesponse), [HoloNETClientAdmin.PeerMetaInfoReturnedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-peermetainforeturnedcallback), [PeerMetaInfoReturnedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-peermetainforeturnedcallbackeventargs), [ProvideMemproofsRequest](#nextgensoftware-holochain-holonet-client-providememproofsrequest), [ProvisioningStrategyType](#nextgensoftware-holochain-holonet-client-provisioningstrategytype), [HoloNETClientAppBase.PublishCountersigningSessionTriggeredCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-publishcountersigningsessiontriggeredcallback), [PublishCountersigningSessionTriggeredCallBackEventArgs](#nextgensoftware-holochain-holonet-client-publishcountersigningsessiontriggeredcallbackeventargs), [QUICConfig](#nextgensoftware-holochain-holonet-client-quicconfig), [HoloNETClientAppBase.ReadyForZomeCalls](#nextgensoftware-holochain-holonet-client-holonetclientappbase-readyforzomecalls), [ReadyForZomeCallsEventArgs](#nextgensoftware-holochain-holonet-client-readyforzomecallseventargs), [Record](#nextgensoftware-holochain-holonet-client-record), [RecordEntry](#nextgensoftware-holochain-holonet-client-recordentry), [HoloNETClientAdmin.RecordsGraftedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-recordsgraftedcallback), [RecordsGraftedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-recordsgraftedcallbackeventargs), [RetrieveAgentPubKeyAndDnaHashMode](#nextgensoftware-holochain-holonet-client-retrieveagentpubkeyanddnahashmode), [ShutdownHolochainConductorsMode](#nextgensoftware-holochain-holonet-client-shutdownholochainconductorsmode), [HoloNETClientAppBase.SignalCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-signalcallback), [SignalCallBackEventArgs](#nextgensoftware-holochain-holonet-client-signalcallbackeventargs), [SignalData](#nextgensoftware-holochain-holonet-client-signaldata), [SignalResponse](#nextgensoftware-holochain-holonet-client-signalresponse), [SignalType](#nextgensoftware-holochain-holonet-client-signaltype), [SignalValue](#nextgensoftware-holochain-holonet-client-signalvalue), [Signature](#nextgensoftware-holochain-holonet-client-signature), [SourceChainCursor](#nextgensoftware-holochain-holonet-client-sourcechaincursor), [SourceChainJsonDump](#nextgensoftware-holochain-holonet-client-sourcechainjsondump), [SourceChainJsonRecord](#nextgensoftware-holochain-holonet-client-sourcechainjsonrecord), [HoloNETClientAdmin.StateDumpedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-statedumpedcallback), [StateDumpedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-statedumpedcallbackeventargs), [StorageInfoResponse](#nextgensoftware-holochain-holonet-client-storageinforesponse), [HoloNETClientAdmin.StorageInfoReturnedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-storageinforeturnedcallback), [StorageInfoReturnedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-storageinforeturnedcallbackeventargs), [TransportConnectionStats](#nextgensoftware-holochain-holonet-client-transportconnectionstats), [WASMConfig](#nextgensoftware-holochain-holonet-client-wasmconfig), [HoloNETClientAppBase.WasmHostFunctionsListedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-wasmhostfunctionslistedcallback), [WasmHostFunctionsListedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-wasmhostfunctionslistedcallbackeventargs), [ZomeCall](#nextgensoftware-holochain-holonet-client-zomecall), [HoloNETClientAdmin.ZomeCallCapabilityGrantedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-zomecallcapabilitygrantedcallback), [ZomeCallCapabilityGrantedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-zomecallcapabilitygrantedcallbackeventargs), [HoloNETClientAdmin.ZomeCallCapabilityRevokedCallBack](#nextgensoftware-holochain-holonet-client-holonetclientadmin-zomecallcapabilityrevokedcallback), [ZomeCallCapabilityRevokedCallBackEventArgs](#nextgensoftware-holochain-holonet-client-zomecallcapabilityrevokedcallbackeventargs), [ZomeCallParamsSigned](#nextgensoftware-holochain-holonet-client-zomecallparamssigned), [ZomeCallSigned](#nextgensoftware-holochain-holonet-client-zomecallsigned), [ZomeCallToSign](#nextgensoftware-holochain-holonet-client-zomecalltosign), [ZomeCallUnsigned](#nextgensoftware-holochain-holonet-client-zomecallunsigned), [ZomeDefinition](#nextgensoftware-holochain-holonet-client-zomedefinition), [HoloNETClientAppBase.ZomeFunctionCallBack](#nextgensoftware-holochain-holonet-client-holonetclientappbase-zomefunctioncallback), [ZomeFunctionCallBackEventArgs](#nextgensoftware-holochain-holonet-client-zomefunctioncallbackeventargs)
- **NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest**: [AppManifest](#nextgensoftware-holochain-holonet-client-data-admin-appmanifest-appmanifest), [AppManifestCurrent](#nextgensoftware-holochain-holonet-client-data-admin-appmanifest-appmanifestcurrent), [Bundeled](#nextgensoftware-holochain-holonet-client-data-admin-appmanifest-bundeled), [Dna](#nextgensoftware-holochain-holonet-client-data-admin-appmanifest-dna), [DnaModifiers](#nextgensoftware-holochain-holonet-client-data-admin-appmanifest-dnamodifiers), [Provisioning](#nextgensoftware-holochain-holonet-client-data-admin-appmanifest-provisioning), [Roles](#nextgensoftware-holochain-holonet-client-data-admin-appmanifest-roles)
- **NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests**: [AddAgentInfoRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-addagentinforequest), [AttachAppInterfaceRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-attachappinterfacerequest), [DeleteCloneCellRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-deleteclonecellrequest), [DisableAppRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-disableapprequest), [DumpFullStateRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-dumpfullstaterequest), [DumpNetworkMetricsRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-dumpnetworkmetricsrequest), [DumpStateRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-dumpstaterequest), [EnableAppRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-enableapprequest), [GetAgentInfoRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-getagentinforequest), [GetCompatibleCellsRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-getcompatiblecellsrequest), [GraftRecordsRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-graftrecordsrequest), [GrantZomeCallCapabilityRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-grantzomecallcapabilityrequest), [InstallAppRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-installapprequest), [IssueAppAuthenticationTokenRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-issueappauthenticationtokenrequest), [ListAppsRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-listappsrequest), [ListCapabilityGrantsRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-listcapabilitygrantsrequest), [ListDnasRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-listdnasrequest), [PeerMetaInfoRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-peermetainforequest), [RegisterDnaRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-registerdnarequest), [RevokeAppAuthenticationTokenRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-revokeappauthenticationtokenrequest), [RevokeZomeCallCapabilityRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-revokezomecallcapabilityrequest), [UninstallAppRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-uninstallapprequest), [UpdateCoordinatorsRequest](#nextgensoftware-holochain-holonet-client-data-admin-requests-updatecoordinatorsrequest)
- **NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects**: [AgentInfo](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-agentinfo), [AgentInfoDetailed](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-agentinfodetailed), [AppBundle](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-appbundle), [CapAccess](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capaccess), [CapGrantAccessAssigned](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccessassigned), [CapGrantAccessAssignedDetails](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccessassigneddetails), [CapGrantAccessTransferable](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccesstransferable), [CapGrantAccessTransferableDetails](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccesstransferabledetails), [CapGrantAccessUnrestricted](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccessunrestricted), [CellInfo](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-cellinfo), [CellInfoFormatter](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-cellinfoformatter), [CoordinatorBundle](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-coordinatorbundle), [CoordinatorManifest](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-coordinatormanifest), [CoordinatorZomeManifest](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-coordinatorzomemanifest), [DnaBundle](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-dnabundle), [DnaManifest](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-dnamanifest), [GrantedFunctions](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-grantedfunctions), [GrantedFunctionsType](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-grantedfunctionstype), [IntegrityManifest](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-integritymanifest), [IntegrityZomeManifest](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-integrityzomemanifest), [KeyPair](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-keypair), [KitsuneAgent](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-kitsuneagent), [KitsuneSignature](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-kitsunesignature), [KitsuneSpace](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-kitsunespace), [RustDuration](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-rustduration), [SigningCredentials](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-signingcredentials), [ZomeCallCapGrant](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-zomecallcapgrant), [ZomeDependency](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-zomedependency), [ZomeManifest](#nextgensoftware-holochain-holonet-client-data-admin-requests-objects-zomemanifest)
- **NextGenSoftware.Holochain.HoloNET.Client.Data.App.Requests**: [AppAuthenticationEnvelope](#nextgensoftware-holochain-holonet-client-data-app-requests-appauthenticationenvelope), [AppAuthenticationRequest](#nextgensoftware-holochain-holonet-client-data-app-requests-appauthenticationrequest)
- **NextGenSoftware.Holochain.HoloNET.Client.Data.App.Responses.Objects**: [AppInfoStatus](#nextgensoftware-holochain-holonet-client-data-app-responses-objects-appinfostatus), [ClonedCell](#nextgensoftware-holochain-holonet-client-data-app-responses-objects-clonedcell), [CloneId](#nextgensoftware-holochain-holonet-client-data-app-responses-objects-cloneid), [CloneIdType](#nextgensoftware-holochain-holonet-client-data-app-responses-objects-cloneidtype), [ProvisionedCell](#nextgensoftware-holochain-holonet-client-data-app-responses-objects-provisionedcell), [StemCell](#nextgensoftware-holochain-holonet-client-data-app-responses-objects-stemcell)
- **NextGenSoftware.Holochain.HoloNET.Client.Enums**: [OptionType](#nextgensoftware-holochain-holonet-client-enums-optiontype)
- **NextGenSoftware.Holochain.HoloNET.Client.Interfaces**: [IAppInfo](#nextgensoftware-holochain-holonet-client-interfaces-iappinfo), [IAppInfoResponse](#nextgensoftware-holochain-holonet-client-interfaces-iappinforesponse), [IAppManifest](#nextgensoftware-holochain-holonet-client-interfaces-iappmanifest), [IAppResponse](#nextgensoftware-holochain-holonet-client-interfaces-iappresponse), [ICell](#nextgensoftware-holochain-holonet-client-interfaces-icell), [IDnaDefinition](#nextgensoftware-holochain-holonet-client-interfaces-idnadefinition), [IDnaDefinitionResponse](#nextgensoftware-holochain-holonet-client-interfaces-idnadefinitionresponse), [IDnaDefinitionResponseDetail](#nextgensoftware-holochain-holonet-client-interfaces-idnadefinitionresponsedetail), [IDnaModifiers](#nextgensoftware-holochain-holonet-client-interfaces-idnamodifiers), [IFullStateDumpedResponse](#nextgensoftware-holochain-holonet-client-interfaces-ifullstatedumpedresponse), [IHoloNETClientAdmin](#nextgensoftware-holochain-holonet-client-interfaces-iholonetclientadmin), [IHoloNETClientAppAgent](#nextgensoftware-holochain-holonet-client-interfaces-iholonetclientappagent), [IHoloNETClientAppBase](#nextgensoftware-holochain-holonet-client-interfaces-iholonetclientappbase), [IHoloNETClientBase](#nextgensoftware-holochain-holonet-client-interfaces-iholonetclientbase), [IHoloNETData](#nextgensoftware-holochain-holonet-client-interfaces-iholonetdata), [IHoloNETDNA](#nextgensoftware-holochain-holonet-client-interfaces-iholonetdna), [IHoloNETRequest](#nextgensoftware-holochain-holonet-client-interfaces-iholonetrequest), [IHoloNETResponse](#nextgensoftware-holochain-holonet-client-interfaces-iholonetresponse), [IRecord](#nextgensoftware-holochain-holonet-client-interfaces-irecord), [ISignalData](#nextgensoftware-holochain-holonet-client-interfaces-isignaldata), [IStorageInfoResponse](#nextgensoftware-holochain-holonet-client-interfaces-istorageinforesponse)

## NextGenSoftware.Holochain.HoloNET.Client

<a id="nextgensoftware-holochain-holonet-client-action"></a>
### Action (struct)

| Member | Kind | Description |
|---|---|---|
| `Dna Dna { get; set; }` | property | The DNA action's payload (the first action of every source chain). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-admininterfacesaddedcallback"></a>
### HoloNETClientAdmin.AdminInterfacesAddedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AdminInterfacesAddedCallBack(object sender, AdminInterfacesAddedCallBackEventArgs e)` (the type of the `OnAdminInterfacesAddedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-admininterfacesaddedcallbackeventargs"></a>
### AdminInterfacesAddedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-adminoptimingsdumpedcallback"></a>
### HoloNETClientAdmin.AdminOpTimingsDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AdminOpTimingsDumpedCallBack(object sender, AdminOpTimingsDumpedCallBackEventArgs e)` (the type of the `OnAdminOpTimingsDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-adminoptimingsdumpedcallbackeventargs"></a>
### AdminOpTimingsDumpedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `OpTimingsDump OpTimingsDump { get; set; }` | property | What the conductor returned for this request (op timings dump). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-agentinfoaddedcallback"></a>
### HoloNETClientAdmin.AgentInfoAddedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AgentInfoAddedCallBack(object sender, AgentInfoAddedCallBackEventArgs e)` (the type of the `OnAgentInfoAddedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-agentinfoaddedcallbackeventargs"></a>
### AgentInfoAddedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-agentinfodump"></a>
### AgentInfoDump (class)

| Member | Kind | Description |
|---|---|---|
| `string dump { get; set; }` | property | Holochain's `dump` field (sent to / received from the conductor as-is). |
| `string kitsune_agent { get; set; }` | property | Holochain's `kitsune_agent` field (sent to / received from the conductor as-is). |
| `string kitsune_space { get; set; }` | property | Holochain's `kitsune_space` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-agentinforeturnedcallback"></a>
### HoloNETClientAdmin.AgentInfoReturnedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AgentInfoReturnedCallBack(object sender, AgentInfoReturnedCallBackEventArgs e)` (the type of the `OnAgentInfoReturnedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-agentinforeturnedcallbackeventargs"></a>
### AgentInfoReturnedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `AgentInfo AgentInfo { get; set; }` | property | What the conductor returned for this request (agent info). |
| `List<string> AgentInfos { get; set; }` | property | One encoded agent-info string per agent (AdminResponse::AgentInfo(Vec<String>)). |

<a id="nextgensoftware-holochain-holonet-client-agentpubkeydnahash"></a>
### AgentPubKeyDnaHash (class)

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-agentpubkeygeneratedcallback"></a>
### HoloNETClientAdmin.AgentPubKeyGeneratedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AgentPubKeyGeneratedCallBack(object sender, AgentPubKeyGeneratedCallBackEventArgs e)` (the type of the `OnAgentPubKeyGeneratedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-agentpubkeygeneratedcallbackeventargs"></a>
### AgentPubKeyGeneratedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `IAppResponse AppResponse { get; set; }` | property | What the conductor returned for this request (app response). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-appagentinforeturnedcallback"></a>
### HoloNETClientAppBase.AppAgentInfoReturnedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppAgentInfoReturnedCallBack(object sender, AgentInfoReturnedCallBackEventArgs e)` (the type of the `OnAppAgentInfoReturnedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appauthenticationtokenissuedcallback"></a>
### HoloNETClientAdmin.AppAuthenticationTokenIssuedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppAuthenticationTokenIssuedCallBack(object sender, AppAuthenticationTokenIssuedCallBackEventArgs e)` (the type of the `OnAppAuthenticationTokenIssuedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appauthenticationtokenissuedcallbackeventargs"></a>
### AppAuthenticationTokenIssuedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `AppAuthenticationTokenIssuedResponse TokenIssued { get; set; }` | property | What the conductor returned for this request (token issued). |

<a id="nextgensoftware-holochain-holonet-client-appauthenticationtokenissuedresponse"></a>
### AppAuthenticationTokenIssuedResponse (class)

Mirrors holochain_conductor_api::admin_interface::AppAuthenticationTokenIssued (Holochain 0.7.0): pub struct AppAuthenticationTokenIssued { pub token: AppAuthenticationToken, // Vec<u8> pub expires_at: Option<Timestamp>, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs

| Member | Kind | Description |
|---|---|---|
| `long? expires_at { get; set; }` | property | Holochain's `expires_at` field (sent to / received from the conductor as-is). |
| `byte[] token { get; set; }` | property | Holochain's `token` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appauthenticationtokenrevokedcallback"></a>
### HoloNETClientAdmin.AppAuthenticationTokenRevokedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppAuthenticationTokenRevokedCallBack(object sender, AppAuthenticationTokenRevokedCallBackEventArgs e)` (the type of the `OnAppAuthenticationTokenRevokedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appauthenticationtokenrevokedcallbackeventargs"></a>
### AppAuthenticationTokenRevokedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-appcompatiblecells"></a>
### AppCompatibleCells (class)

Mirrors holochain_conductor_api::admin_interface::AdminResponse::CompatibleCells (Holochain 0.7.0): #[cfg(feature = "unstable-migration")] pub type CompatibleCells = BTreeSet<(InstalledAppId, BTreeSet<CellId>)>; https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs NOTE: This is gated behind the `unstable-migration` feature flag (unchanged in 0.7.0) and may not be present/enabled on all conductor builds.

| Member | Kind | Description |
|---|---|---|
| `List<CellId> cell_ids { get; set; }` | property | Holochain's `cell_ids` field (sent to / received from the conductor as-is). |
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appdisabledcallback"></a>
### HoloNETClientAdmin.AppDisabledCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppDisabledCallBack(object sender, AppDisabledCallBackEventArgs e)` (the type of the `OnAppDisabledCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appdisabledcallbackeventargs"></a>
### AppDisabledCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string InstalledAppId { get; set; }` | property | The installed app id. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appenabledcallback"></a>
### HoloNETClientAdmin.AppEnabledCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppEnabledCallBack(object sender, AppEnabledCallBackEventArgs e)` (the type of the `OnAppEnabledCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appenabledcallbackeventargs"></a>
### AppEnabledCallBackEventArgs (class)

Inherits / implements: `AppInfoCallBackEventArgs`

| Member | Kind | Description |
|---|---|---|
| `List<EnableAppError> Errors { get; set; }` | property | What the conductor returned for this request (errors). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-appenabledviaappinterfacecallback"></a>
### HoloNETClientAppBase.AppEnabledViaAppInterfaceCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppEnabledViaAppInterfaceCallBack(object sender, AppEnabledViaAppInterfaceCallBackEventArgs e)` (the type of the `OnAppEnabledViaAppInterfaceCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appenabledviaappinterfacecallbackeventargs"></a>
### AppEnabledViaAppInterfaceCallBackEventArgs (class)

Result of the app interface's enable_app (Holochain 0.7: enables the app this connection is authenticated for).

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-appinfo"></a>
### AppInfo (class)

Inherits / implements: `IAppInfo`

| Member | Kind | Description |
|---|---|---|
| `byte[] agent_pub_key { get; set; }` | property | Holochain's `agent_pub_key` field (sent to / received from the conductor as-is). |
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `AppInfoStatusEnum AppStatus { get; }` | property | The app's status as the conductor reports it (enabled, disabled, paused...). |
| `string AppStatusReason { get; }` | property | Why the app is disabled or paused, if it is. |
| `Dictionary<string, List<CellInfo>> cell_info { get; set; }` | property | Holochain's `cell_info` field (sent to / received from the conductor as-is). |
| `byte[][] CellId { get; set; }` | property | The cell id: [DNA hash, agent public key] as raw bytes. |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |
| `AppManifest manifest { get; set; }` | property | Holochain's `manifest` field (sent to / received from the conductor as-is). |
| `Dictionary<string, object> status { get; set; }` | property | Holochain's `status` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-appinfocallback"></a>
### HoloNETClientAppBase.AppInfoCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppInfoCallBack(object sender, AppInfoCallBackEventArgs e)` (the type of the `OnAppInfoCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appinfocallbackeventargs"></a>
### AppInfoCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `IAppInfoResponse AppInfoResponse { get; set; }` | property | The conductor's raw app_info response. |
| `IAppManifest AppManifest { get; set; }` | property | The app's manifest (its roles and DNAs). |
| `AppInfoStatusEnum AppStatus { get; set; }` | property | The app's status as the conductor reports it (enabled, disabled, paused...). |
| `string AppStatusReason { get; set; }` | property | Why the app is disabled or paused, if it is. |
| `byte[][] CellId { get; set; }` | property | The cell id: [DNA hash, agent public key] as raw bytes. |
| `CellInfoType CellType { get; set; }` | property | The kind of cell (provisioned, cloned or stem). |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |
| `string InstalledAppId { get; set; }` | property | The installed app id. |

<a id="nextgensoftware-holochain-holonet-client-appinforequest"></a>
### AppInfoRequest (class)

| Member | Kind | Description |
|---|---|---|
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-appinforesponse"></a>
### AppInfoResponse (class)

Inherits / implements: `IAppInfoResponse`

| Member | Kind | Description |
|---|---|---|
| `AppInfo data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-appinfostatusenum"></a>
### AppInfoStatusEnum (enum)

| Value | Description |
|---|---|
| `Paused` |  |
| `Disabled` |  |
| `Running` |  |
| `AwaitingMemproofs` |  |
| `None` |  |
| `AwaitingRestore` |  |
| `Unrecoverable` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appinstalledcallback"></a>
### HoloNETClientAdmin.AppInstalledCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppInstalledCallBack(object sender, AppInstalledCallBackEventArgs e)` (the type of the `OnAppInstalledCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appinstalledcallbackeventargs"></a>
### AppInstalledCallBackEventArgs (class)

Inherits / implements: `AppInfoCallBackEventArgs`

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appinterfaceattachedcallback"></a>
### HoloNETClientAdmin.AppInterfaceAttachedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppInterfaceAttachedCallBack(object sender, AppInterfaceAttachedCallBackEventArgs e)` (the type of the `OnAppInterfaceAttachedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appinterfaceattachedcallbackeventargs"></a>
### AppInterfaceAttachedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `ushort? Port { get; set; }` | property | What the conductor returned for this request (port). |

<a id="nextgensoftware-holochain-holonet-client-appinterfaceinfo"></a>
### AppInterfaceInfo (class)

One entry of list_app_interfaces (Holochain 0.7.0 `AppInterfaceInfo`).

| Member | Kind | Description |
|---|---|---|
| `string AllowedOrigins { get; set; }` | property | "*" for any origin, otherwise a comma-separated list. |
| `string InstalledAppId { get; set; }` | property | Set when the interface is bound to one installed app. |
| `ushort Port { get; set; }` | property | The app interface's WebSocket port. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appinterfaceslistedcallback"></a>
### HoloNETClientAdmin.AppInterfacesListedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppInterfacesListedCallBack(object sender, AppInterfacesListedCallBackEventArgs e)` (the type of the `OnAppInterfacesListedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appinterfaceslistedcallbackeventargs"></a>
### AppInterfacesListedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `List<AppInterfaceInfo> AppInterfaces { get; set; }` | property | Each app interface as Holochain 0.7.0 reports it (port, allowed origins, optional app binding). |
| `List<ushort> WebSocketPorts { get; set; }` | property | What the conductor returned for this request (web socket ports). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-appnetworkmetricsdumpedcallback"></a>
### HoloNETClientAppBase.AppNetworkMetricsDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppNetworkMetricsDumpedCallBack(object sender, NetworkMetricsDumpedCallBackEventArgs e)` (the type of the `OnAppNetworkMetricsDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-appnetworkstatsdumpedcallback"></a>
### HoloNETClientAppBase.AppNetworkStatsDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppNetworkStatsDumpedCallBack(object sender, NetworkStatsDumpedCallBackEventArgs e)` (the type of the `OnAppNetworkStatsDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-appoptimingsdumpedcallback"></a>
### HoloNETClientAppBase.AppOpTimingsDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppOpTimingsDumpedCallBack(object sender, AppOpTimingsDumpedCallBackEventArgs e)` (the type of the `OnAppOpTimingsDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appoptimingsdumpedcallbackeventargs"></a>
### AppOpTimingsDumpedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `OpTimingsDump OpTimingsDump { get; set; }` | property | What the conductor returned for this request (op timings dump). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-apppeermetainforeturnedcallback"></a>
### HoloNETClientAppBase.AppPeerMetaInfoReturnedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppPeerMetaInfoReturnedCallBack(object sender, AppPeerMetaInfoReturnedCallBackEventArgs e)` (the type of the `OnAppPeerMetaInfoReturnedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-apppeermetainforeturnedcallbackeventargs"></a>
### AppPeerMetaInfoReturnedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `PeerMetaInfoResponse PeerMetaInfo { get; set; }` | property | What the conductor returned for this request (peer meta info). |

<a id="nextgensoftware-holochain-holonet-client-appresponse"></a>
### AppResponse (class)

Inherits / implements: `IAppResponse`

| Member | Kind | Description |
|---|---|---|
| `object data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appslistedcallback"></a>
### HoloNETClientAdmin.AppsListedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppsListedCallBack(object sender, AppsListedCallBackEventArgs e)` (the type of the `OnAppsListedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appslistedcallbackeventargs"></a>
### AppsListedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `List<IAppInfo> Apps { get; set; }` | property | What the conductor returned for this request (apps). |

<a id="nextgensoftware-holochain-holonet-client-appstatusfilter"></a>
### AppStatusFilter (enum)

Mirrors holochain_conductor_api::AppStatusFilter (Holochain 0.7.0): Enabled, Disabled, AwaitingMemproofs, AwaitingRestore, Unrecoverable. Sent on the wire as its snake_case name via ToWireValue(), never as an integer: rmp-serde accepts integers as variant indexes, so the C# ordinal would select the wrong filter.

| Value | Description |
|---|---|
| `Enabled` |  |
| `Disabled` |  |
| `Running` |  |
| `Stopped` |  |
| `Paused` |  |
| `AwaitingMemproofs` |  |
| `All` |  |
| `AwaitingRestore` |  |
| `Unrecoverable` |  |

<a id="nextgensoftware-holochain-holonet-client-appstatusfilterextensions"></a>
### AppStatusFilterExtensions (static class)

| Member | Kind | Description |
|---|---|---|
| `static string ToWireValue(AppStatusFilter filter)` | method | Returns the conductor's wire value, or null for All (no filter). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-appuninstalledcallback"></a>
### HoloNETClientAdmin.AppUninstalledCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void AppUninstalledCallBack(object sender, AppUninstalledCallBackEventArgs e)` (the type of the `OnAppUninstalledCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-appuninstalledcallbackeventargs"></a>
### AppUninstalledCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string InstalledAppId { get; set; }` | property | The installed app id. |

<a id="nextgensoftware-holochain-holonet-client-attachappinterfaceresponse"></a>
### AttachAppInterfaceResponse (class)

| Member | Kind | Description |
|---|---|---|
| `ushort port { get; set; }` | property | Holochain's `port` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-bytearrayasintarrayformatter"></a>
### ByteArrayAsIntArrayFormatter (class)

For Rust Vec<u8> fields without serde_bytes (e.g. AppAuthenticationToken): rmp-serde encodes them as an array of integers, not msgpack bin. Writes an int array; reads either form.

Inherits / implements: `IMessagePackFormatter<byte[]>`, `IMessagePackFormatter`

| Member | Kind | Description |
|---|---|---|
| `byte[] Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)` | method | MessagePack formatter plumbing used by HoloNET's serialiser (you don't call this). |
| `void Serialize(ref MessagePackWriter writer, byte[] value, MessagePackSerializerOptions options)` | method | MessagePack formatter plumbing used by HoloNET's serialiser (you don't call this). |

<a id="nextgensoftware-holochain-holonet-client-cacheconfig"></a>
### CacheConfig (class)

This is a HoloNET-CLIENT-SIDE caching layer config, NOT a mirror of any Holochain Rust conductor struct. Researched holochain_conductor_api's ConductorConfig (https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs) and confirmed there is no generic "cache" concept exposed there at all - the conductor has an internal wasm compilation cache (see WASMConfig.cs in this directory) and SQLite-backed databases, but nothing resembling a configurable client-side response/object cache. This class is therefore honestly relabelled: it is a legitimate, HoloNET-specific local cache configuration (e.g. for caching zome call results or DNA metadata client-side), not a fabricated mirror of a non-existent Rust struct. Its fields are deliberately kept simple (TTL, max size/entries, eviction policy) since this is purely a client-side concern that HoloNET is free to define as it sees fit. The previous version of this file contained several over-elaborate, unused nested configs (warming, persistence, distribution/sharding, HSM-style encryption) that implied integration with non-existent distributed-cache infrastructure; those have been removed as speculative scaffolding pending a real use case.

| Member | Kind | Description |
|---|---|---|
| `int DefaultTTLSeconds { get; set; }` | property | Default time-to-live for cached entries, in seconds. |
| `bool Enabled { get; set; }` | property | Enable the client-side cache. Default is true. |
| `CacheEvictionPolicy EvictionPolicy { get; set; }` | property | Eviction policy applied once MaxEntries is exceeded. |
| `int MaxEntries { get; set; }` | property | Maximum number of entries to retain in the cache before eviction. |

<a id="nextgensoftware-holochain-holonet-client-cacheevictionpolicy"></a>
### CacheEvictionPolicy (enum)

Eviction policies for the HoloNET client-side cache (client concept only, no Rust mirror).

| Value | Description |
|---|---|
| `LRU` | Least Recently Used. |
| `LFU` | Least Frequently Used. |
| `FIFO` | First In, First Out. |

<a id="nextgensoftware-holochain-holonet-client-callzomeoptions"></a>
### CallZomeOptions (class)

Per-call options for CallZomeFunctionAsync. NOTE: Unlike the other classes in this folder, this is NOT a verified 1:1 mirror of a Rust struct - a thorough search of the holochain_zome_types / holochain_conductor_api / holochain_types crates at tag holochain-0.7.0 did not turn up a `CallZomeOptions` struct on the wire protocol. The wire-level request (see `ZomeCallParamsSigned`) has no equivalent "options" field today. This class instead lets HoloNET callers configure client-side behaviour (currently: a per-call request timeout) without having to fall back to the global `RequestTimeoutS` default. If a genuine CallZomeOptions wire struct is added to Holochain in a future release, this class can be extended/mapped to it then.

| Member | Kind | Description |
|---|---|---|
| `CallZomeOptions Default { get; } (static)` | property | The default zome call options. |
| `int? TimeoutSeconds { get; set; }` | property | Per-call override for how long (in seconds) HoloNET should wait for a response to this zome call before timing out. If null, falls back to HoloNETDNA.NetworkConfig.RequestTimeoutS (see NetworkConfig). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-capabilitygrantslistedcallback"></a>
### HoloNETClientAdmin.CapabilityGrantsListedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CapabilityGrantsListedCallBack(object sender, CapabilityGrantsListedCallBackEventArgs e)` (the type of the `OnCapabilityGrantsListedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-capabilitygrantslistedcallbackeventargs"></a>
### CapabilityGrantsListedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `List<CellCapGrantInfo> CapGrants { get; set; }` | property | What the conductor returned for this request (cap grants). |

<a id="nextgensoftware-holochain-holonet-client-capaccessinfo"></a>
### CapAccessInfo (class)

Mirrors holochain_zome_types::capability::CapAccessInfo (Holochain 0.7.0) — the desensitized (secret-free) summary of a CapAccess variant: pub struct CapAccessInfo { pub access_type: String, // "Unrestricted" | "Transferable" | "Assigned" pub assignees: Option<BTreeSet<AgentPubKey>>, // Some only for Assigned variant } https://docs.rs/holochain_zome_types/0.7.0/holochain_zome_types/capability/struct.CapAccessInfo.html

| Member | Kind | Description |
|---|---|---|
| `string access_type { get; set; }` | property | Holochain's `access_type` field (sent to / received from the conductor as-is). |
| `byte[][] assignees { get; set; }` | property | Holochain's `assignees` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-capgrantaccesstype"></a>
### CapGrantAccessType (enum)

| Value | Description |
|---|---|
| `Unrestricted` | No restriction: callable by anyone. |
| `Transferable` | Callable by anyone who can provide the secret (the secret is a randomly generated 64 byte number and is accessible from the GetCapGrantSecret function once the AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability or AdminAuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync function have been called). |
| `Assigned` | Callable by anyone in the list of assignees who possesses the secret (the secret is a randomly generated 64 byte number and is accessible from the GetCapGrantSecret function once the AdminAuthorizeSigningCredentialsAndGrantZomeCallCapability or AdminAuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync function have been called). |

<a id="nextgensoftware-holochain-holonet-client-capgrantinfo"></a>
### CapGrantInfo (class)

Mirrors holochain_zome_types::capability::CapGrantInfo (Holochain 0.7.0): pub struct CapGrantInfo { pub cap_grant: DesensitizedZomeCallCapGrant, pub action_hash: ActionHash, pub created_at: Timestamp, pub revoked_at: Option<Timestamp>, } https://docs.rs/holochain_zome_types/0.7.0/holochain_zome_types/capability/struct.CapGrantInfo.html `DesensitizedZomeCallCapGrant` is verified against holochain_zome_types 0.7.0: tag (String), access (CapAccessInfo), functions (GrantedFunctions).

| Member | Kind | Description |
|---|---|---|
| `byte[] action_hash { get; set; }` | property | Holochain's `action_hash` field (sent to / received from the conductor as-is). |
| `DesensitizedZomeCallCapGrant cap_grant { get; set; }` | property | Holochain's `cap_grant` field (sent to / received from the conductor as-is). |
| `long created_at { get; set; }` | property | Holochain's `created_at` field (sent to / received from the conductor as-is). |
| `long? revoked_at { get; set; }` | property | Holochain's `revoked_at` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-cellcapgrantinfo"></a>
### CellCapGrantInfo (class)

Mirrors holochain_conductor_api::AppCapGrantInfo (Holochain 0.7.0), a tuple struct: `pub struct AppCapGrantInfo(pub Vec<(CellId, Vec<CapGrantInfo>)>);` https://docs.rs/holochain_zome_types/0.7.0/holochain_zome_types/capability/struct.AppCapGrantInfo.html (Vec is used instead of a map because tuple keys are problematic with msgpack encoding.)

| Member | Kind | Description |
|---|---|---|
| `CapGrantInfo[] cap_grants { get; set; }` | property | Holochain's `cap_grants` field (sent to / received from the conductor as-is). |
| `CellId cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-cellid"></a>
### CellId (class)

Mirrors the Rust holochain_zome_types::cell::CellId tuple struct (Holochain 0.7.0): `pub struct CellId(DnaHash, AgentPubKey);` https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_zome_types/src/cell.rs A Rust tuple struct (de)serializes as a positional 2-element array, so this is MessagePack-encoded with positional integer keys (0 = dna_hash, 1 = agent_pub_key) rather than named map keys.

| Member | Kind | Description |
|---|---|---|
| `CellId(byte[] dnaHash, byte[] agentPubKey)` | ctor | A cell id from its DNA hash and agent public key. |
| `byte[] agent_pub_key { get; set; }` | property | Holochain's `agent_pub_key` field (sent to / received from the conductor as-is). |
| `byte[] dna_hash { get; set; }` | property | Holochain's `dna_hash` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-cellidslistedcallback"></a>
### HoloNETClientAdmin.CellIdsListedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CellIdsListedCallBack(object sender, CellIdsListedCallBackEventArgs e)` (the type of the `OnCellIdsListedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-cellidslistedcallbackeventargs"></a>
### CellIdsListedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `List<byte[][]> CellIds { get; set; }` | property | What the conductor returned for this request (cell ids). |

<a id="nextgensoftware-holochain-holonet-client-cellinfotype"></a>
### CellInfoType (enum)

| Value | Description |
|---|---|
| `None` |  |
| `Provisioned` |  |
| `Cloned` |  |
| `Stem` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-clonecellcreatedcallback"></a>
### HoloNETClientAppBase.CloneCellCreatedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CloneCellCreatedCallBack(object sender, CloneCellCreatedCallBackEventArgs e)` (the type of the `OnCloneCellCreatedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-clonecellcreatedcallbackeventargs"></a>
### CloneCellCreatedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `ClonedCellResponse ClonedCell { get; set; }` | property | What the conductor returned for this request (cloned cell). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-clonecelldeletedcallback"></a>
### HoloNETClientAdmin.CloneCellDeletedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CloneCellDeletedCallBack(object sender, CloneCellDeletedCallBackEventArgs e)` (the type of the `OnCloneCellDeletedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-clonecelldeletedcallbackeventargs"></a>
### CloneCellDeletedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-clonecelldisabledcallback"></a>
### HoloNETClientAppBase.CloneCellDisabledCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CloneCellDisabledCallBack(object sender, CloneCellDisabledCallBackEventArgs e)` (the type of the `OnCloneCellDisabledCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-clonecelldisabledcallbackeventargs"></a>
### CloneCellDisabledCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-clonecellenabledcallback"></a>
### HoloNETClientAppBase.CloneCellEnabledCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CloneCellEnabledCallBack(object sender, CloneCellEnabledCallBackEventArgs e)` (the type of the `OnCloneCellEnabledCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-clonecellenabledcallbackeventargs"></a>
### CloneCellEnabledCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `ClonedCellResponse ClonedCell { get; set; }` | property | What the conductor returned for this request (cloned cell). |

<a id="nextgensoftware-holochain-holonet-client-clonecellid"></a>
### CloneCellId (static class)

Builds the wire form of holochain_zome_types::clone::CloneCellId (Holochain 0.7.0). Rust: #[serde(tag = "type", content = "value", rename_all = "snake_case")] Wire: {"type": "clone_id", "value": "role_name.0"} or {"type": "dna_hash", "value": bytes} Pass the result as clone_cell_id on Enable/Disable/DeleteCloneCellRequest.

| Member | Kind | Description |
|---|---|---|
| `static Dictionary<string, object> FromCloneId(string cloneId)` | method | Identify a clone cell by its clone id (e.g. "oasis.0"). |
| `static Dictionary<string, object> FromDnaHash(byte[] dnaHash)` | method | Identify a clone cell by its DNA hash. |
| `static object Normalize(object cloneCellId)` | method | Accepts the legacy inputs callers already pass and returns the tagged wire form: string → clone_id; byte[] → dna_hash; byte[][] CellId → dna_hash of cellId[0] (0.7.0 has no CellId variant; a clone cell's DNA hash identifies it within the app). An already-built dictionary is passed through unchanged. |

<a id="nextgensoftware-holochain-holonet-client-clonedcellresponse"></a>
### ClonedCellResponse (class)

Mirrors holochain_types::app::ClonedCell, returned by AppResponse::CloneCellCreated and AppResponse::CloneCellEnabled (Holochain 0.7.0). Verified against holochain_conductor_api/src/app_interface.rs at holochain-0.7.0: pub struct ClonedCell { pub cell_id: CellId, pub clone_id: CloneId, pub original_dna_hash: DnaHash, pub dna_modifiers: DnaModifiers, pub name: String, pub enabled: bool, } All six fields match. dna_modifiers is kept as dynamic since DnaModifiers contains optional sub-fields (network_seed, properties, origin_time, quantum_time).

| Member | Kind | Description |
|---|---|---|
| `CellId cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `string clone_id { get; set; }` | property | Holochain's `clone_id` field (sent to / received from the conductor as-is). |
| `object dna_modifiers { get; set; }` | property | Holochain's `dna_modifiers` field (sent to / received from the conductor as-is). |
| `bool enabled { get; set; }` | property | Holochain's `enabled` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `byte[] original_dna_hash { get; set; }` | property | Holochain's `original_dna_hash` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-compatiblecellsreturnedcallback"></a>
### HoloNETClientAdmin.CompatibleCellsReturnedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CompatibleCellsReturnedCallBack(object sender, CompatibleCellsReturnedCallBackEventArgs e)` (the type of the `OnCompatibleCellsReturnedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-compatiblecellsreturnedcallbackeventargs"></a>
### CompatibleCellsReturnedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `List<AppCompatibleCells> CompatibleCells { get; set; }` | property | What the conductor returned for this request (compatible cells). |

<a id="nextgensoftware-holochain-holonet-client-conductordebugcallbackeventargs"></a>
### ConductorDebugCallBackEventArgs (class)

Inherits / implements: `CallBackBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string EndPoint { get; set; }` | property | The conductor WebSocket endpoint this came from. |
| `int NumberDelayedValidations { get; set; }` | property | What the conductor returned for this request (number delayed validations). |
| `int NumberHeldAspects { get; set; }` | property | What the conductor returned for this request (number held aspects). |
| `int NumberHeldEntries { get; set; }` | property | What the conductor returned for this request (number held entries). |
| `int NumberPendingValidations { get; set; }` | property | What the conductor returned for this request (number pending validations). |
| `int NumberRunningZomeCalls { get; set; }` | property | What the conductor returned for this request (number running zome calls). |
| `bool Offline { get; set; }` | property | What the conductor returned for this request (offline). |
| `string RawJSONData { get; set; }` | property | The raw response, as JSON (for logging and debugging). |
| `string Type { get; set; }` | property | The conductor message type. |
| `WebSocketReceiveResult WebSocketResult { get; set; }` | property | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please see here for more info. |

<a id="nextgensoftware-holochain-holonet-client-conductorresponsecallbackmode"></a>
### ConductorResponseCallBackMode (enum)

| Value | Description |
|---|---|
| `WaitForHolochainConductorResponse` |  |
| `UseCallBackEvents` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-conductorstatedumpedcallback"></a>
### HoloNETClientAdmin.ConductorStateDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void ConductorStateDumpedCallBack(object sender, ConductorStateDumpedCallBackEventArgs e)` (the type of the `OnConductorStateDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-conductorstatedumpedcallbackeventargs"></a>
### ConductorStateDumpedCallBackEventArgs (class)

Result of the admin interface's dump_conductor_state: the conductor's state as JSON.

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string DumpedStateJSON { get; set; }` | property | What the conductor returned for this request (dumped state json). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-connected"></a>
### HoloNETClientBase.Connected (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void Connected(object sender, ConnectedEventArgs e)` (the type of the `OnConnected` event).

<a id="nextgensoftware-holochain-holonet-client-connectedcallbackmode"></a>
### ConnectedCallBackMode (enum)

| Value | Description |
|---|---|
| `WaitForHolochainConductorToConnect` |  |
| `UseCallBackEvents` |  |

<a id="nextgensoftware-holochain-holonet-client-coordinatorsource"></a>
### CoordinatorSource (enum)

| Value | Description |
|---|---|
| `Path` |  |
| `Bundle` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-coordinatorsupdatedcallback"></a>
### HoloNETClientAdmin.CoordinatorsUpdatedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CoordinatorsUpdatedCallBack(object sender, CoordinatorsUpdatedCallBackEventArgs e)` (the type of the `OnCoordinatorsUpdatedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-coordinatorsupdatedcallbackeventargs"></a>
### CoordinatorsUpdatedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-countersigningcellidrequest"></a>
### CountersigningCellIdRequest (class)

Mirrors the payload of AppRequest::GetCountersigningSessionState, AppRequest::AbandonCountersigningSession and AppRequest::PublishCountersigningSession, all of which take a single `Box<CellId>` (Holochain 0.7.0, gated behind the `unstable-countersigning` feature flag): GetCountersigningSessionState(Box<CellId>) AbandonCountersigningSession(Box<CellId>) PublishCountersigningSession(Box<CellId>) https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs

| Member | Kind | Description |
|---|---|---|
| `CellId cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-countersigningsessionabandonedcallback"></a>
### HoloNETClientAppBase.CountersigningSessionAbandonedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CountersigningSessionAbandonedCallBack(object sender, CountersigningSessionAbandonedCallBackEventArgs e)` (the type of the `OnCountersigningSessionAbandonedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-countersigningsessionabandonedcallbackeventargs"></a>
### CountersigningSessionAbandonedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-countersigningsessionstateresponse"></a>
### CountersigningSessionStateResponse (class)

Response payload for AppResponse::CountersigningSessionState, which wraps `Box<Option<CountersigningSessionState>>` (Holochain 0.7.0). NOTE: `CountersigningSessionState` is an enum with three variants verified against the Holochain 0.7.0 source (holochain_types::countersigning::CountersigningSessionState): Accepted(PreflightRequest) SignaturesCollected { preflight_request, signature_bundles, resolution? } Unknown { preflight_request, resolution, force_abandon, force_publish } The raw state is exposed as `dynamic` here — deserialize precisely from the msgpack payload if you need to inspect individual variant fields.

| Member | Kind | Description |
|---|---|---|
| `object session_state { get; set; }` | property | Holochain's `session_state` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-countersigningsessionstatereturnedcallback"></a>
### HoloNETClientAppBase.CountersigningSessionStateReturnedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void CountersigningSessionStateReturnedCallBack(object sender, CountersigningSessionStateReturnedCallBackEventArgs e)` (the type of the `OnCountersigningSessionStateReturnedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-countersigningsessionstatereturnedcallbackeventargs"></a>
### CountersigningSessionStateReturnedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `CountersigningSessionStateResponse SessionState { get; set; }` | property | What the conductor returned for this request (session state). |

<a id="nextgensoftware-holochain-holonet-client-createclonecellrequest"></a>
### CreateCloneCellRequest (class)

Mirrors holochain_types::app::CreateCloneCellPayload, used as the payload for AppRequest::CreateCloneCell (Holochain 0.7.0): pub struct CreateCloneCellPayload { pub role_name: RoleName, // String pub modifiers: DnaModifiersOpt<YamlProperties>, pub membrane_proof: Option<MembraneProof>, // Option<Arc<SerializedBytes>> pub name: Option<String>, } https://docs.rs/holochain_types/0.7.0/holochain_types/app/struct.CreateCloneCellPayload.html Verified against holochain_types/src/app.rs at holochain-0.7.0 — all four fields match. `DnaModifiersOpt<YamlProperties>` is represented as `dynamic` since its sub-fields (network_seed, properties, origin_time, quantum_time) are each individually Optional; callers can supply a plain anonymous object/dictionary with only the fields they need.

| Member | Kind | Description |
|---|---|---|
| `byte[] membrane_proof { get; set; }` | property | Holochain's `membrane_proof` field (sent to / received from the conductor as-is). |
| `object modifiers { get; set; }` | property | Holochain's `modifiers` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `string role_name { get; set; }` | property | Holochain's `role_name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-datareceived"></a>
### HoloNETClientBase.DataReceived (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void DataReceived(object sender, HoloNETDataReceivedEventArgs e)` (the type of the `OnDataReceived` event).

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-datasent"></a>
### HoloNETClientBase.DataSent (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void DataSent(object sender, HoloNETDataSentEventArgs e)` (the type of the `OnDataSent` event).

<a id="nextgensoftware-holochain-holonet-client-desensitizedzomecallcapgrant"></a>
### DesensitizedZomeCallCapGrant (class)

Mirrors holochain_zome_types::capability::DesensitizedZomeCallCapGrant (Holochain 0.7.0) — the cap grant shape returned by the conductor with secrets removed: pub struct DesensitizedZomeCallCapGrant { pub tag: String, pub access: CapAccessInfo, pub functions: GrantedFunctions, } https://docs.rs/holochain_zome_types/0.7.0/holochain_zome_types/capability/struct.DesensitizedZomeCallCapGrant.html

| Member | Kind | Description |
|---|---|---|
| `CapAccessInfo access { get; set; }` | property | Holochain's `access` field (sent to / received from the conductor as-is). |
| `CapAccessInfo AccessInfo { get; }` | property | Access info on either version. |
| `Dictionary<string, object> capability { get; set; }` | property | Holochain 0.7.1+: what the grant allows, adjacently tagged ("zome_call" or "direct_signal"). |
| `string CapabilityType { get; }` | property | "zome_call" or "direct_signal" (0.7.1+); "zome_call" for 0.7.0 grants. |
| `CapAccessInfo constraint { get; set; }` | property | Holochain 0.7.1+: who may use the grant (0.7.0 sends this as `access`). |
| `Dictionary<string, object> functions { get; set; }` | property | Holochain's `functions` field (sent to / received from the conductor as-is). |
| `Dictionary<string, object> Functions { get; }` | property | Granted functions on either version (null for a direct-signal grant). |
| `string tag { get; set; }` | property | Holochain's `tag` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-dhtop"></a>
### DhtOp (class)

| Member | Kind | Description |
|---|---|---|
| `ValueTuple<Signature, Action, RecordEntry> StoreRecord { get; set; }` | property | A StoreRecord op: the signature, the action and the entry. |

<a id="nextgensoftware-holochain-holonet-client-dhtopscursor"></a>
### DhtOpsCursor (class)

Pagination cursor for dump_full_state (holochain_conductor_api::state_dump::DhtOpsCursor, Holochain 0.7.0 and 0.7.1): pass the previous page's `dht_ops_cursor` to get the next page.

| Member | Kind | Description |
|---|---|---|
| `byte[] hash { get; set; }` | property | Hash of the last op selected (tie-breaks ops sharing a timestamp). |
| `long when_received { get; set; }` | property | Microsecond received timestamp of the last op selected. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-directsignalcapabilitygrantedcallback"></a>
### HoloNETClientAdmin.DirectSignalCapabilityGrantedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void DirectSignalCapabilityGrantedCallBack(object sender, DirectSignalCapabilityGrantedCallBackEventArgs e)` (the type of the `OnDirectSignalCapabilityGrantedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-directsignalcapabilitygrantedcallback"></a>
### HoloNETClientAppBase.DirectSignalCapabilityGrantedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void DirectSignalCapabilityGrantedCallBack(object sender, DirectSignalCapabilityGrantedCallBackEventArgs e)` (the type of the `OnDirectSignalCapabilityGrantedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-directsignalcapabilitygrantedcallbackeventargs"></a>
### DirectSignalCapabilityGrantedCallBackEventArgs (class)

Result of grant_direct_signal_capability (Holochain 0.7.1+): the action hash of the committed grant.

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `byte[] ActionHash { get; set; }` | property | What the conductor returned for this request (action hash). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-directsignalsentcallback"></a>
### HoloNETClientAppBase.DirectSignalSentCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void DirectSignalSentCallBack(object sender, DirectSignalSentCallBackEventArgs e)` (the type of the `OnDirectSignalSentCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-directsignalsentcallbackeventargs"></a>
### DirectSignalSentCallBackEventArgs (class)

Result of send_direct_signal (delivery is best effort; the conductor only confirms it was sent).

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-disableclonecellrequest"></a>
### DisableCloneCellRequest (class)

Mirrors holochain_types::app::DisableCloneCellPayload, used as the payload for AppRequest::DisableCloneCell (Holochain 0.7.0): pub struct DisableCloneCellPayload { pub clone_cell_id: CloneCellId, // enum: CloneId(CloneId) | DnaHash(DnaHash) } https://docs.rs/holochain_types/0.7.0/holochain_types/app/struct.DisableCloneCellPayload.html

| Member | Kind | Description |
|---|---|---|
| `object clone_cell_id { get; set; }` | property | The CloneCellId — either a clone id string (e.g. "role_name.0") or a CellId tuple. Wire (adjacently tagged): {"type": "clone_id", "value": "role.0"} or {"type": "dna_hash", "value": bytes}. Build with CloneCellId.FromCloneId / FromDnaHash. |

<a id="nextgensoftware-holochain-holonet-client-disabledappreasontype"></a>
### DisabledAppReasonType (enum)

| Value | Description |
|---|---|
| `NeverStarted` |  |
| `User` |  |
| `Error` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-disconnected"></a>
### HoloNETClientBase.Disconnected (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void Disconnected(object sender, DisconnectedEventArgs e)` (the type of the `OnDisconnected` event).

<a id="nextgensoftware-holochain-holonet-client-disconnectedcallbackmode"></a>
### DisconnectedCallBackMode (enum)

| Value | Description |
|---|---|
| `WaitForHolochainConductorToDisconnect` |  |
| `UseCallBackEvents` |  |

<a id="nextgensoftware-holochain-holonet-client-dnadef"></a>
### DnaDef (class)

Inherits / implements: `DnaDefinition`, `IDnaDefinition`

<a id="nextgensoftware-holochain-holonet-client-dnadefinition"></a>
### DnaDefinition (class)

Inherits / implements: `IDnaDefinition`

| Member | Kind | Description |
|---|---|---|
| `List<ZomeDefinition> CoordinatorZomes { get; set; }` | property | The DNA's coordinator zomes (hot-swappable with UpdateCoordinatorsAsync). |
| `List<ZomeDefinition> IntegrityZomes { get; set; }` | property | The DNA's integrity zomes (entry/link types and validation; they define the DNA hash). |
| `IDnaModifiers Modifiers { get; set; }` | property | The DNA modifiers (network seed, properties, origin time) that make this DNA's hash. |
| `string Name { get; set; }` | property | The DNA's name. |

<a id="nextgensoftware-holochain-holonet-client-dnadefinitionresponse"></a>
### DnaDefinitionResponse (class)

Inherits / implements: `IDnaDefinitionResponse`

| Member | Kind | Description |
|---|---|---|
| `DnaDefinitionResponseDetail data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-dnadefinitionresponsedetail"></a>
### DnaDefinitionResponseDetail (class)

Inherits / implements: `IDnaDefinitionResponseDetail`

| Member | Kind | Description |
|---|---|---|
| `object coordinator_zomes_raw { get; set; }` | property | Holochain's `coordinator_zomes_raw` field (sent to / received from the conductor as-is). |
| `object integrity_zomes_raw { get; set; }` | property | Holochain's `integrity_zomes_raw` field (sent to / received from the conductor as-is). |
| `object lineage { get; set; }` | property | Holochain 0.7: DNA hashes this DNA declares as its lineage (may be empty). |
| `DnaModifiers modifiers { get; set; }` | property | Holochain's `modifiers` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-dnadefinitionreturnedcallback"></a>
### HoloNETClientAdmin.DnaDefinitionReturnedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void DnaDefinitionReturnedCallBack(object sender, DnaDefinitionReturnedCallBackEventArgs e)` (the type of the `OnDnaDefinitionReturnedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-dnadefinitionreturnedcallbackeventargs"></a>
### DnaDefinitionReturnedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `IDnaDefinition DnaDefinition { get; set; }` | property | What the conductor returned for this request (dna definition). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-dnaregisteredcallback"></a>
### HoloNETClientAdmin.DnaRegisteredCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void DnaRegisteredCallBack(object sender, DnaRegisteredCallBackEventArgs e)` (the type of the `OnDnaRegisteredCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-dnaregisteredcallbackeventargs"></a>
### DnaRegisteredCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `byte[] HoloHash { get; set; }` | property | What the conductor returned for this request (holo hash). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-dnaslistedcallback"></a>
### HoloNETClientAdmin.DnasListedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void DnasListedCallBack(object sender, DnasListedCallBackEventArgs e)` (the type of the `OnDnasListedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-dnaslistedcallbackeventargs"></a>
### DnasListedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `List<byte[]> Dnas { get; set; }` | property | What the conductor returned for this request (dnas). |

<a id="nextgensoftware-holochain-holonet-client-dnastorageblob"></a>
### DnaStorageBlob (class)

| Member | Kind | Description |
|---|---|---|
| `DnaStorageInfo dna { get; set; }` | property | Holochain's `dna` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-dnastorageinfo"></a>
### DnaStorageInfo (class)

| Member | Kind | Description |
|---|---|---|
| `int dht_data_size { get; set; }` | property | Holochain's `dht_data_size` field (sent to / received from the conductor as-is). |
| `int dht_data_size_on_disk { get; set; }` | property | Holochain's `dht_data_size_on_disk` field (sent to / received from the conductor as-is). |
| `string used_by { get; set; }` | property | Holochain's `used_by` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-dumpnetworkstatsresponse"></a>
### DumpNetworkStatsResponse (class)

Typed representation of the JSON returned by the Holochain admin `dump_network_stats` call (AdminResponse::NetworkStatsDumped(String) - the conductor returns a JSON-encoded string here, not a msgpack object, hence this is deserialized with Newtonsoft.Json from NetworkStatsDumpedCallBackEventArgs.NetworkStatsDumpJSON rather than via MessagePack). As of Holochain 0.7.0 this wraps kitsune2_api::transport::TransportStats per connection. https://docs.rs/kitsune2_api/latest/kitsune2_api/struct.TransportStats.html https://docs.rs/kitsune2_api/latest/kitsune2_api/struct.TransportConnectionStats.html

| Member | Kind | Description |
|---|---|---|
| `string Backend { get; set; }` | property | The networking backend that is in use (e.g. "tx5", "mem", "webrtc"). |
| `List<TransportConnectionStats> Connections { get; set; }` | property | The list of current transport connections and their stats. |
| `List<string> PeerUrls { get; set; }` | property | The list of peer urls that this Kitsune2 instance can currently be reached at. |

<a id="nextgensoftware-holochain-holonet-client-dumpoptimingsrequest"></a>
### DumpOpTimingsRequest (class)

Mirrors holochain_conductor_api::admin_interface::AdminRequest::DumpOpTimings and holochain_conductor_api::app_interface::AppRequest::DumpOpTimings. New in Holochain 0.7.0. Reports, for each DHT op the conductor holds for a DNA, when the op was received, when it was integrated, or when validation was abandoned. Results are paginated; pass the cursor from a previous response to continue. https://github.com/holochain/holochain/blob/main-0.7/crates/holochain_conductor_api/src/admin_interface.rs

| Member | Kind | Description |
|---|---|---|
| `OpTimingsCursor cursor { get; set; }` | property | Pagination cursor from a previous DumpOpTimings call; only ops ordered strictly after this cursor are returned. Null starts from the beginning. |
| `byte[] dna_hash { get; set; }` | property | The DNA whose DHT arc to dump op timings for. For the app API, the app must be running a cell of this DNA. |
| `uint? limit { get; set; }` | property | Maximum number of ops to return. Must be greater than zero if set. |

<a id="nextgensoftware-holochain-holonet-client-embeddedholochainversion"></a>
### EmbeddedHolochainVersion (enum)

Which bundled Holochain release HolochainConductorMode.UseEmbedded starts (NextGenSoftware.Holochain.HoloNET.Client.Embedded). hApps must be built for the same release: 0.7.1 needs hdk 0.7.1 / hdi 0.8.1, 0.7.0 needs hdk 0.7.0 / hdi 0.8.0.

| Value | Description |
|---|---|
| `V0_7_1` | Holochain 0.7.1 (default). |
| `V0_7_0` | Holochain 0.7.0. |

<a id="nextgensoftware-holochain-holonet-client-enableapperror"></a>
### EnableAppError (class)

One entry in the errors array returned by enable_app. Wire format: [CellId, error_string] where CellId is [DnaHash, AgentPubKey].

| Member | Kind | Description |
|---|---|---|
| `byte[][] CellId { get; set; }` | property | The cell id: [DNA hash, agent public key] as raw bytes. |
| `string Error { get; set; }` | property | Why a cell could not be enabled. |

<a id="nextgensoftware-holochain-holonet-client-enableappresponse"></a>
### EnableAppResponse (class)

| Member | Kind | Description |
|---|---|---|
| `EnableAppResponseDetails data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-enableappresponsedetails"></a>
### EnableAppResponseDetails (class)

| Member | Kind | Description |
|---|---|---|
| `AppInfo app { get; set; }` | property | Holochain's `app` field (sent to / received from the conductor as-is). |
| `List<EnableAppError> errors { get; set; }` | property | Holochain's `errors` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-enableclonecellrequest"></a>
### EnableCloneCellRequest (class)

Mirrors AppRequest::EnableCloneCell's payload, EnableCloneCellPayload, which per the Holochain 0.7.0 docs is the same shape as DisableCloneCellPayload (a type alias / same CloneCellId wrapper). https://docs.rs/holochain_types/0.7.0/holochain_types/app/type.EnableCloneCellPayload.html

| Member | Kind | Description |
|---|---|---|
| `object clone_cell_id { get; set; }` | property | The CloneCellId — either a clone id string (e.g. "role_name.0") or a CellId tuple. Wire (adjacently tagged): {"type": "clone_id", "value": "role.0"} or {"type": "dna_hash", "value": bytes}. Build with CloneCellId.FromCloneId / FromDnaHash. |

<a id="nextgensoftware-holochain-holonet-client-enforcerequesttoresponseidmatchingbehaviour"></a>
### EnforceRequestToResponseIdMatchingBehaviour (enum)

| Value | Description |
|---|---|
| `Ignore` |  |
| `Warn` |  |
| `Error` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-error"></a>
### HoloNETClientBase.Error (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void Error(object sender, HoloNETErrorEventArgs e)` (the type of the `OnError` event).

<a id="nextgensoftware-holochain-holonet-client-errorhandlingbehaviour"></a>
### ErrorHandlingBehaviour (enum)

| Value | Description |
|---|---|
| `AlwaysThrowExceptionOnError` |  |
| `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` |  |
| `NeverThrowExceptions` |  |

<a id="nextgensoftware-holochain-holonet-client-fullintegrationstatedump"></a>
### FullIntegrationStateDump (class)

| Member | Kind | Description |
|---|---|---|
| `DhtOpsCursor dht_ops_cursor { get; set; }` | property | Holochain's `dht_ops_cursor` field (sent to / received from the conductor as-is). |
| `object[] integrated { get; set; }` | property | Holochain's `integrated` field (sent to / received from the conductor as-is). |
| `object[] integration_limbo { get; set; }` | property | Holochain's `integration_limbo` field (sent to / received from the conductor as-is). |
| `object[] validation_limbo { get; set; }` | property | Holochain's `validation_limbo` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-fullstatedumpedcallback"></a>
### HoloNETClientAdmin.FullStateDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void FullStateDumpedCallBack(object sender, FullStateDumpedCallBackEventArgs e)` (the type of the `OnFullStateDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-fullstatedumpedcallbackeventargs"></a>
### FullStateDumpedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `IFullStateDumpedResponse DumpedState { get; set; }` | property | What the conductor returned for this request (dumped state). |
| `string DumpedStateJSON { get; set; }` | property | The conductor's FullStateDump as JSON. Always set; DumpedState is best-effort. |

<a id="nextgensoftware-holochain-holonet-client-fullstatedumpedresponse"></a>
### FullStateDumpedResponse (class)

Inherits / implements: `IFullStateDumpedResponse`

| Member | Kind | Description |
|---|---|---|
| `FullIntegrationStateDump integration_dump { get; set; }` | property | Holochain's `integration_dump` field (sent to / received from the conductor as-is). |
| `P2pAgentsDump peer_dump { get; set; }` | property | Holochain's `peer_dump` field (sent to / received from the conductor as-is). |
| `SourceChainJsonDump source_chain_dump { get; set; }` | property | Holochain's `source_chain_dump` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-getappinfocallbackeventargs"></a>
### GetAppInfoCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `IAppInfo AppInfo { get; set; }` | property | What the conductor returned for this request (app info). |

<a id="nextgensoftware-holochain-holonet-client-holochainconductorenum"></a>
### HolochainConductorEnum (enum)

| Value | Description |
|---|---|
| `HcDevTool` |  |
| `HolochainProductionConductor` |  |

<a id="nextgensoftware-holochain-holonet-client-holochainconductormodeenum"></a>
### HolochainConductorModeEnum (enum)

| Value | Description |
|---|---|
| `UseExternal` | If this is set you then need to spcecify the full path to the holochain.exe in the 'FullPathToExternalHolochainConductorBinary' setting on the IHoloNETDNA (if 'HolochainConductorToUse' is set to 'HolochainProductionConductor') or the full path to the hc.exe in the 'FullPathToExternalHCToolBinary' setting (if 'HolochainConductorToUse' is set to 'HcDevTool'). |
| `UseEmbedded` | This will use the internal embedded holochain.exe (if 'HolochainConductorToUse' is set to 'HolochainProductionConductor') or hc.exe (if 'HolochainConductorToUse' is set to 'HcDevTool') if the app is using the [NextGenSoftware.Holochain.HoloNET.Client.Embedded](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.Embedded) package, otherwise it will throw an exception. |
| `UseSystemGlobal` | This will use the installed version of holochain.exe on the target machine. (if 'HolochainConductorToUse' is set to 'HolochainProductionConductor') or hc.exe (if 'HolochainConductorToUse' is set to 'HcDevTool'). This is the default option. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-holochainconductorsshutdowncomplete"></a>
### HoloNETClientBase.HolochainConductorsShutdownComplete (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void HolochainConductorsShutdownComplete(object sender, HolochainConductorsShutdownEventArgs e)` (the type of the `OnHolochainConductorsShutdownComplete` event).

<a id="nextgensoftware-holochain-holonet-client-holochainconductorsshutdowneventargs"></a>
### HolochainConductorsShutdownEventArgs (class)

Inherits / implements: `CallBackBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |
| `Uri EndPoint { get; set; }` | property | The conductor WebSocket endpoint this came from. |
| `int NumberOfHcExeInstancesShutdown { get; set; }` | property | The number of hc.exe instances that were shutdown. |
| `int NumberOfHolochainExeInstancesShutdown { get; set; }` | property | The number of holochain.exe instances that were shutdown. |
| `int NumberOfRustcExeInstancesShutdown { get; set; }` | property | The number of rustc.exe instances that were shutdown. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-holochainconductorstarted"></a>
### HoloNETClientBase.HolochainConductorStarted (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void HolochainConductorStarted(object sender, HolochainConductorStartedEventArgs e)` (the type of the `OnHolochainConductorStarted` event).

<a id="nextgensoftware-holochain-holonet-client-holochainconductorstartedeventargs"></a>
### HolochainConductorStartedEventArgs (class)

Inherits / implements: `CallBackBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-holochainconductorstarting"></a>
### HoloNETClientBase.HolochainConductorStarting (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void HolochainConductorStarting(object sender, HolochainConductorStartingEventArgs e)` (the type of the `OnHolochainConductorStarting` event).

<a id="nextgensoftware-holochain-holonet-client-holochainconductorstartingeventargs"></a>
### HolochainConductorStartingEventArgs (class)

Inherits / implements: `CallBackBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-holochainrustfieldname"></a>
### HolochainRustFieldName (class)

Inherits / implements: `Attribute`

| Member | Kind | Description |
|---|---|---|
| `HolochainRustFieldName(string fieldName, bool isEnabled = true)` | ctor | Add this to any property you wish to map to your rust struct/entry in your hApp. |

<a id="nextgensoftware-holochain-holonet-client-holochainserialisationwrapper"></a>
### HolochainSerialisationWrapper (static class)

Implements Holochain 0.7's ZomeCallParams::serialize_and_hash contract in managed code. This is the sole signing path so desktop, Unity and mobile clients sign identical canonical bytes without a platform-specific native serialization wrapper.

<a id="nextgensoftware-holochain-holonet-client-holonetauditentrytype"></a>
### HoloNETAuditEntryType (enum)

| Value | Description |
|---|---|
| `Create` |  |
| `Modify` |  |
| `Delete` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin"></a>
### HoloNETClientAdmin (class)

Inherits / implements: `HoloNETClientBase`, `IHoloNETClientBase`, `IDisposable`, `IHoloNETClientAdmin`

| Member | Kind | Description |
|---|---|---|
| `HoloNETClientAdmin(HoloNETDNA holoNETDNA = null)` | ctor | This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA. |
| `HoloNETClientAdmin(ILogProvider logProvider, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientAdmin(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientAdmin(ILogger logger, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientAdmin.AdminInterfacesAddedCallBack OnAdminInterfacesAddedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.AdminOpTimingsDumpedCallBack OnAdminOpTimingsDumpedCallBack` | event | Fired when a response is received from the conductor after the DumpOpTimingsAsync/DumpOpTimings method is called. |
| `HoloNETClientAdmin.AgentInfoAddedCallBack OnAgentInfoAddedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.AgentInfoReturnedCallBack OnAgentInfoReturnedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.AgentPubKeyGeneratedCallBack OnAgentPubKeyGeneratedCallBack` | event | Fired when the client receives the generated AgentPubKey from the conductor. |
| `HoloNETClientAdmin.AppAuthenticationTokenIssuedCallBack OnAppAuthenticationTokenIssuedCallBack` | event | Fired when a response is received from the conductor after the IssueAppAuthenticationTokenAsync/IssueAppAuthenticationToken method is called. |
| `HoloNETClientAdmin.AppAuthenticationTokenRevokedCallBack OnAppAuthenticationTokenRevokedCallBack` | event | Fired when a response is received from the conductor after the RevokeAppAuthenticationTokenAsync/RevokeAppAuthenticationToken method is called. |
| `HoloNETClientAdmin.AppDisabledCallBack OnAppDisabledCallBack` | event | Fired when a response is received from the conductor after a hApp has been disabled via the DisableAppAsyc/DisableApp method. |
| `HoloNETClientAdmin.AppEnabledCallBack OnAppEnabledCallBack` | event | Fired when a response is received from the conductor after a hApp has been enabled via the EnableAppAsyc/EnableApp method. |
| `HoloNETClientAdmin.AppInstalledCallBack OnAppInstalledCallBack` | event | Fired when a response is received from the conductor after a hApp has been installed via the InstallAppAsyc/InstallApp method. |
| `HoloNETClientAdmin.AppInterfaceAttachedCallBack OnAppInterfaceAttachedCallBack` | event | Fired when a response is received from the conductor after the app interface has been attached via the AttachAppInterfaceAsyc/AttachAppInterface method. |
| `HoloNETClientAdmin.AppInterfacesListedCallBack OnAppInterfacesListedCallBack` | event | Fired when a response is received from the conductor after the ListAppInterfacesAsync/ListAppInterfaces method is called. |
| `HoloNETClientAdmin.AppsListedCallBack OnAppsListedCallBack` | event | Fired when a response is received from the conductor after the ListAppsAsync/ListApps method is called. |
| `HoloNETClientAdmin.AppUninstalledCallBack OnAppUninstalledCallBack` | event | Fired when a response is received from the conductor after a hApp has been uninstalled via the UninstallAppAsyc/UninstallApp method. |
| `HoloNETClientAdmin.CapabilityGrantsListedCallBack OnCapabilityGrantsListedCallBack` | event | Fired when a response is received from the conductor after the ListCapabilityGrantsAsync/ListCapabilityGrants method is called. |
| `HoloNETClientAdmin.CellIdsListedCallBack OnCellIdsListedCallBack` | event | Fired when a response is received from the conductor after the ListCellIdsAsync/ListCellIds method is called. |
| `HoloNETClientAdmin.CloneCellDeletedCallBack OnCloneCellDeletedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.CompatibleCellsReturnedCallBack OnCompatibleCellsReturnedCallBack` | event | Fired when a response is received from the conductor after the GetCompatibleCellsAsync/GetCompatibleCells method is called. |
| `HoloNETClientAdmin.ConductorStateDumpedCallBack OnConductorStateDumpedCallBack` | event | Fired when a response arrives for DumpConductorStateAsync. |
| `HoloNETClientAdmin.CoordinatorsUpdatedCallBack OnCoordinatorsUpdatedCallBack` | event | Fired when a response is received from the conductor after the UpdateCoordinatorsAsync/UpdateCoordinators method is called. |
| `HoloNETClientAdmin.DirectSignalCapabilityGrantedCallBack OnDirectSignalCapabilityGrantedCallBack` | event | Fired when a response arrives for GrantDirectSignalCapabilityAsync. |
| `HoloNETClientAdmin.DnaDefinitionReturnedCallBack OnDnaDefinitionReturnedCallBack` | event | Fired when a response is received from the conductor containing the DNA Definition for a DNA Hash after the GetDnaDefinitionAsync/GetDnaDefinition method is called. |
| `HoloNETClientAdmin.DnaRegisteredCallBack OnDnaRegisteredCallBack` | event | Fired when a response is received from the conductor after the dna has been registered via the RegisterDnaAsync/RegisterDna method. |
| `HoloNETClientAdmin.DnasListedCallBack OnDnasListedCallBack` | event | Fired when a response is received from the conductor after the ListDnasAsync/ListDnas method is called. |
| `HoloNETClientAdmin.FullStateDumpedCallBack OnFullStateDumpedCallBack` | event | Fired when a response is received from the conductor after the DumpFullStateAsync/DumpFullState method is called. |
| `HoloNETClientAdmin.InstallEnableSignAndAttachHappCallBack OnInstallEnableSignAndAttachHappCallBack` | event | Fired when the hApp has been installed, enabled, signed and attached. |
| `HoloNETClientAdmin.InstallEnableSignAttachAndConnectToHappCallBack OnInstallEnableSignAttachAndConnectToHappCallBack` | event | Fired when the hApp has been installed, enabled, signed and attached. |
| `HoloNETClientAdmin.NetworkMetricsDumpedCallBack OnNetworkMetricsDumpedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.NetworkStatsDumpedCallBack OnNetworkStatsDumpedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.PeerMetaInfoReturnedCallBack OnPeerMetaInfoReturnedCallBack` | event | Fired when a response is received from the conductor after the GetPeerMetaInfoAsync/GetPeerMetaInfo method is called. |
| `HoloNETClientAdmin.RecordsGraftedCallBack OnRecordsGraftedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.StateDumpedCallBack OnStateDumpedCallBack` | event | Fired when a response is received from the conductor after the DumpStateAsync/DumpState method is called. |
| `HoloNETClientAdmin.StorageInfoReturnedCallBack OnStorageInfoReturnedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.ZomeCallCapabilityGrantedCallBack OnZomeCallCapabilityGrantedCallBack` | event | Fired when a response is received from the conductor after a cell has been authorized via the AuthorizeSigningCredentialsForZomeCallsAsync/AuthorizeSigningCredentialsForZomeCalls method. |
| `HoloNETClientAdmin.ZomeCallCapabilityRevokedCallBack OnZomeCallCapabilityRevokedCallBack` | event | Fired when a response is received from the conductor after the RevokeZomeCallCapabilityAsync/RevokeZomeCallCapability method is called. |
| `Task<AdminInterfacesAddedCallBackEventArgs> AddAdminInterfacesAsync(IEnumerable<ushort> ports, string allowedOrigins = "*", ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Adds admin websocket interfaces (Holochain 0.7.0 `add_admin_interfaces`). Each port gets a websocket interface accepting ("*" for any origin, or a comma-separated list). |
| `AgentInfoAddedCallBackEventArgs AddAgentInfo(AgentInfo[] agentInfos, string id = null)` | method | Add existing agent(s) to Holochain. |
| `Task<AgentInfoAddedCallBackEventArgs> AddAgentInfoAsync(IEnumerable<string> agentInfos, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Add existing agent(s) to Holochain. |
| `Task<AgentInfoAddedCallBackEventArgs> AddAgentInfoAsync(AgentInfo[] agentInfos, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Add existing agent(s) to Holochain. **(obsolete)** |
| `AppInterfaceAttachedCallBackEventArgs AttachAppInterface(ushort? port = null, string id = null)` | method | Instructs the conductor to open an app WebSocket interface on the given port (or a random port if `null`). The returned event args include the actual port. You then connect a `HoloNETClient` to that port to make zome calls. |
| `Task<AppInterfaceAttachedCallBackEventArgs> AttachAppInterfaceAsync(ushort? port = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Instructs the conductor to open an app WebSocket interface on the given port (or a random port if `null`). The returned event args include the actual port. You then connect a `HoloNETClient` to that port to make zome calls. |
| `ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(byte[][] cellId, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(string AgentPubKey, string DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(byte[] AgentPubKey, byte[] DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(byte[][] cellId, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(string AgentPubKey, string DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(byte[] AgentPubKey, byte[] DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `CloneCellDeletedCallBackEventArgs DeleteCloneCell(string appId, string id = null)` | method | Delete a clone cell that was previously disabled. |
| `CloneCellDeletedCallBackEventArgs DeleteCloneCell(string appId, string roleName, string id = null)` | method | Delete a clone cell that was previously disabled. |
| `CloneCellDeletedCallBackEventArgs DeleteCloneCell(string appId, byte[][] cellId, string id = null)` | method | Delete a clone cell that was previously disabled. |
| `CloneCellDeletedCallBackEventArgs DeleteCloneCell(string appId, string agentPubKey, string dnaHash, string id = null)` | method | Delete a clone cell that was previously disabled. |
| `Task<CloneCellDeletedCallBackEventArgs> DeleteCloneCellAsync(string appId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Delete a clone cell that was previously disabled. |
| `Task<CloneCellDeletedCallBackEventArgs> DeleteCloneCellAsync(string appId, string roleName, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Delete a clone cell that was previously disabled. |
| `Task<CloneCellDeletedCallBackEventArgs> DeleteCloneCellAsync(string appId, byte[][] cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Delete a clone cell that was previously disabled. |
| `Task<CloneCellDeletedCallBackEventArgs> DeleteCloneCellAsync(string appId, string agentPubKey, string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Delete a clone cell that was previously disabled. |
| `AppDisabledCallBackEventArgs DisableApp(string installedAppId, string id = null)` | method | Pauses a hApp without uninstalling it. Useful for maintenance or temporarily suspending an agent. |
| `Task<AppDisabledCallBackEventArgs> DisableAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Pauses a hApp without uninstalling it. Useful for maintenance or temporarily suspending an agent. |
| `ConductorStateDumpedCallBackEventArgs DumpConductorState(string id = null)` | method | The conductor's own state as JSON (dump_conductor_state). |
| `Task<ConductorStateDumpedCallBackEventArgs> DumpConductorStateAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | The conductor's own state as JSON (dump_conductor_state). |
| `FullStateDumpedCallBackEventArgs DumpFullState(int? dHTOpsCursor = null, string id = null)` | method | Dump the full state of the specified cell, including its chain and DHT shard, as JSON. |
| `FullStateDumpedCallBackEventArgs DumpFullState(byte[][] cellId, int? dHTOpsCursor = null, string id = null)` | method | Dump the full state of the specified cell, including its chain and DHT shard, as JSON. |
| `FullStateDumpedCallBackEventArgs DumpFullState(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, string id = null)` | method | Dump the full state of the specified cell, including its chain and DHT shard, as JSON. |
| `Task<FullStateDumpedCallBackEventArgs> DumpFullStateAsync(int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump the full state of the specified cell, including its chain and DHT shard, as JSON. |
| `Task<FullStateDumpedCallBackEventArgs> DumpFullStateAsync(byte[][] cellId, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump the full state of the specified cell, including its chain and DHT shard, as JSON. |
| `Task<FullStateDumpedCallBackEventArgs> DumpFullStateAsync(byte[][] cellId, DhtOpsCursor cursor, uint? limit = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dumps the full state of a cell, a page at a time: pass the previous result's `integration_dump.dht_ops_cursor` as for the next page, and to cap the DHT ops per page. |
| `Task<FullStateDumpedCallBackEventArgs> DumpFullStateAsync(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump the full state of the specified cell, including its chain and DHT shard, as JSON. |
| `NetworkMetricsDumpedCallBackEventArgs DumpNetworkMetrics(string id = null)` | method | Dump the network metrics tracked by kitsune. |
| `Task<NetworkMetricsDumpedCallBackEventArgs> DumpNetworkMetricsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump the network metrics tracked by kitsune. |
| `Task<NetworkMetricsDumpedCallBackEventArgs> DumpNetworkMetricsAsync(byte[] dnaHash, bool includeDhtSummary = false, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Network metrics for one DNA (or all when is null), optionally with the DHT summary. |
| `NetworkStatsDumpedCallBackEventArgs DumpNetworkStats(string id = null)` | method | Dump raw json network statistics from the backend networking lib. |
| `Task<NetworkStatsDumpedCallBackEventArgs> DumpNetworkStatsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump raw json network statistics from the backend networking lib. |
| `AdminOpTimingsDumpedCallBackEventArgs DumpOpTimings(byte[] dnaHash, OpTimingsCursor cursor = null, uint? limit = null, string id = null)` | method | Dump DHT op timing information for a DNA (new in Holochain 0.7.0). |
| `Task<AdminOpTimingsDumpedCallBackEventArgs> DumpOpTimingsAsync(byte[] dnaHash, OpTimingsCursor cursor = null, uint? limit = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump DHT op timing information for a DNA (new in Holochain 0.7.0). Results are paginated; pass the cursor from a previous call to retrieve the next page. |
| `StateDumpedCallBackEventArgs DumpState(int? dHTOpsCursor = null, string id = null)` | method | Dump the state of the specified cell, including its source chain, as JSON. |
| `StateDumpedCallBackEventArgs DumpState(byte[][] cellId, int? dHTOpsCursor = null, string id = null)` | method | Dump the state of the specified cell, including its source chain, as JSON. |
| `StateDumpedCallBackEventArgs DumpState(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, string id = null)` | method | Dump the state of the specified cell, including its source chain, as JSON. |
| `Task<StateDumpedCallBackEventArgs> DumpStateAsync(int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump the state of the specified cell, including its source chain, as JSON. |
| `Task<StateDumpedCallBackEventArgs> DumpStateAsync(byte[][] cellId, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump the state of the specified cell, including its source chain, as JSON. |
| `Task<StateDumpedCallBackEventArgs> DumpStateAsync(byte[][] cellId, object sourceChainCursor, uint? limit = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump the state of the specified cell, including its source chain, as JSON. |
| `Task<StateDumpedCallBackEventArgs> DumpStateAsync(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump the state of the specified cell, including its source chain, as JSON. |
| `Task<AppEnabledCallBackEventArgs> EnableAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Transitions the hApp from `Disabled` → `Enabled` (active). Must be called after `InstallAppAsync` before you can connect an app websocket or make zome calls. |
| `AppEnabledCallBackEventArgs EnablelApp(string installedAppId, string id = null)` | method | Enable an installed app (sync version of EnableAppAsync; the name keeps its original spelling for compatibility). |
| `void GenerateAgentPubKey(bool updateAgentPubKeyInHoloNETDNA = true, string id = "")` | method | Asks the conductor to generate a new agent key pair (Ed25519). Returns the agent public key as both a byte array and a Base64 string. You need this before installing a hApp. |
| `Task<AgentPubKeyGeneratedCallBackEventArgs> GenerateAgentPubKeyAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, bool updateAgentPubKeyInHoloNETDNA = true, string id = "")` | method | Asks the conductor to generate a new agent key pair (Ed25519). Returns the agent public key as both a byte array and a Base64 string. You need this before installing a hApp. |
| `AgentInfoReturnedCallBackEventArgs GetAgentInfo(string id = null)` | method | Request all available info about an agent. |
| `AgentInfoReturnedCallBackEventArgs GetAgentInfo(byte[][] cellId, string id = null)` | method | Request all available info about an agent. |
| `AgentInfoReturnedCallBackEventArgs GetAgentInfo(string agentPubKey, string dnaHash, string id = null)` | method | Request all available info about an agent. |
| `Task<AgentInfoReturnedCallBackEventArgs> GetAgentInfoAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Request all available info about an agent. |
| `Task<AgentInfoReturnedCallBackEventArgs> GetAgentInfoAsync(byte[][] cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Request all available info about an agent. |
| `Task<AgentInfoReturnedCallBackEventArgs> GetAgentInfoAsync(string agentPubKey, string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Request all available info about an agent. |
| `GetAppInfoCallBackEventArgs GetAppInfo(string installedAppId, string id = null)` | method | Returns the full `AppInfo` for the given hApp: status, cell IDs, agent pub key, manifest, and more. |
| `Task<GetAppInfoCallBackEventArgs> GetAppInfoAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the full `AppInfo` for the given hApp: status, cell IDs, agent pub key, manifest, and more. |
| `byte[] GetCapGrantSecret(byte[][] cellId)` | method | The capability secret HoloNET created for this cell's zome-call grant (null if it has none). |
| `byte[] GetCapGrantSecret(string agentPubKey, string dnaHash)` | method | The capability secret HoloNET created for this agent and DNA's zome-call grant (null if it has none). |
| `Task<byte[][]> GetCellIdAsync()` | method | The cell id zome calls go to: HoloNETDNA.CellId, or built from its DnaHash and AgentPubKey (retrieving them from the conductor first if needed). |
| `CompatibleCellsReturnedCallBackEventArgs GetCompatibleCells(byte[] dnaHash, string id = null)` | method | Get the cells that are compatible with (i.e. share a lineage with) the given DnaHash. Gated behind the `unstable-migration` feature flag in Holochain 0.7.0 - may not be available on all conductor builds. |
| `Task<CompatibleCellsReturnedCallBackEventArgs> GetCompatibleCellsAsync(byte[] dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Get the cells that are compatible with (i.e. share a lineage with) the given DnaHash. Gated behind the `unstable-migration` feature flag in Holochain 0.7.0 - may not be available on all conductor builds. |
| `DnaDefinitionReturnedCallBackEventArgs GetDnaDefinition(byte[] dnaHash, string id = null)` | method | Get the DNA definition for the specified DNA hash. |
| `DnaDefinitionReturnedCallBackEventArgs GetDnaDefinition(string dnaHash, string id = null)` | method | Get the DNA definition for the specified DNA hash. |
| `Task<DnaDefinitionReturnedCallBackEventArgs> GetDnaDefinitionAsync(byte[] dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Get the DNA definition for the specified DNA hash. |
| `Task<DnaDefinitionReturnedCallBackEventArgs> GetDnaDefinitionAsync(byte[][] cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Get the DNA definition for the specified cell (Holochain 0.7.0: AdminRequest::GetDnaDefinition(Box<CellId>)). |
| `Task<DnaDefinitionReturnedCallBackEventArgs> GetDnaDefinitionAsync(string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Get the DNA definition for the specified DNA hash. |
| `PeerMetaInfoReturnedCallBackEventArgs GetPeerMetaInfo(string url, List<byte[]> dnaHashes = null, string id = null)` | method | Get meta info about a peer that is known via the networking layer (kitsune2), via the admin interface. |
| `Task<PeerMetaInfoReturnedCallBackEventArgs> GetPeerMetaInfoAsync(string url, List<byte[]> dnaHashes = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Get meta info about a peer that is known via the networking layer (kitsune2), via the admin interface. |
| `StorageInfoReturnedCallBackEventArgs GetStorageInfo(string id = null)` | method | Get'the storgage info used by hApps. |
| `Task<StorageInfoReturnedCallBackEventArgs> GetStorageInfoAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Get'the storgage info used by hApps. |
| `RecordsGraftedCallBackEventArgs GraftRecords(byte[][] cellId, bool validate, object[] records, string id = null)` | method | Dump the network metrics tracked by kitsune. **(obsolete)** |
| `Task<RecordsGraftedCallBackEventArgs> GraftRecordsAsync(byte[][] cellId, bool validate, object[] records, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | "Graft" record's onto the source chain of the specified CellId. **(obsolete)** |
| `DirectSignalCapabilityGrantedCallBackEventArgs GrantDirectSignalCapability(byte[][] cellId, string tag, CapGrantAccessType access = Unrestricted, byte[] secret = null, byte[][] assignees = null, string id = null)` | method | Holochain 0.7.1+: lets other agents send direct signals to (grant_direct_signal_capability). Holochain 0.7.0 doesn't have this request and returns an error. |
| `Task<DirectSignalCapabilityGrantedCallBackEventArgs> GrantDirectSignalCapabilityAsync(byte[][] cellId, string tag, CapGrantAccessType access = Unrestricted, byte[] secret = null, byte[][] assignees = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Holochain 0.7.1+: lets other agents send direct signals to (grant_direct_signal_capability). Holochain 0.7.0 doesn't have this request and returns an error. |
| `void InstallApp(string agentKey, string installedAppId, string hAppPath, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, string id = null)` | method |  **(obsolete)** |
| `AppInstalledCallBackEventArgs InstallApp(string installedAppId, AppBundle appBundle, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, string id = null)` | method |  **(obsolete)** **(obsolete)** |
| `Task<AppInstalledCallBackEventArgs> InstallAppAsync(string installedAppId, string hAppPath, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method |  **(obsolete)** |
| `Task<AppInstalledCallBackEventArgs> InstallAppAsync(string installedAppId, AppBundle appBundle, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method |  **(obsolete)** **(obsolete)** |
| `Task<AppInstalledCallBackEventArgs> InstallAppFromBytesAsync(string installedAppId, byte[] happBytes, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Install a hApp from the raw bytes of a .happ file (AppBundleSource::Bytes). |
| `InstallEnableSignAndAttachHappEventArgs InstallEnableSignAndAttachHapp(string hAppId, string hAppInstallPath, string roleName, CapGrantAccessType capGrantAccessType = Assigned, GrantedFunctionsType grantedFunctionsType = All, List<ValueTuple<string, string>> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)` | method | Composite helper — performs in sequence: `GenerateAgentPubKey` (if no agentKey supplied) → `InstallApp` → `EnableApp` → `AuthorizeSigningCredentials` → `AttachAppInterface`. Returns a single `InstallEnableSignAndAttachHappEventArgs` with the app info, agent pub key, cell IDs, and the attached port. This is the recommended entry point for installing a new hApp. |
| `Task<InstallEnableSignAndAttachHappEventArgs> InstallEnableSignAndAttachHappAsync(string hAppId, string hAppInstallPath, string roleName, CapGrantAccessType capGrantAccessType = Assigned, GrantedFunctionsType grantedFunctionsType = All, List<ValueTuple<string, string>> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)` | method | Composite helper — performs in sequence: `GenerateAgentPubKey` (if no agentKey supplied) → `InstallApp` → `EnableApp` → `AuthorizeSigningCredentials` → `AttachAppInterface`. Returns a single `InstallEnableSignAndAttachHappEventArgs` with the app info, agent pub key, cell IDs, and the attached port. This is the recommended entry point for installing a new hApp. |
| `InstallEnableSignAttachAndConnectToHappEventArgs InstallEnableSignAttachAndConnectToHapp(string hAppId, string hAppInstallPath, string roleName, CapGrantAccessType capGrantAccessType = Assigned, GrantedFunctionsType grantedFunctionsType = All, List<ValueTuple<string, string>> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)` | method | Same as above but also creates and returns a connected `HoloNETClient` instance (app websocket) ready to make zome calls immediately. The most convenient way to go from a `.happ` file to fully connected and ready to call zomes. |
| `Task<InstallEnableSignAttachAndConnectToHappEventArgs> InstallEnableSignAttachAndConnectToHappAsync(string hAppId, string hAppInstallPath, string roleName, CapGrantAccessType capGrantAccessType = Assigned, GrantedFunctionsType grantedFunctionsType = All, List<ValueTuple<string, string>> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)` | method | Same as above but also creates and returns a connected `HoloNETClient` instance (app websocket) ready to make zome calls immediately. The most convenient way to go from a `.happ` file to fully connected and ready to call zomes. |
| `AppAuthenticationTokenIssuedCallBackEventArgs IssueAppAuthenticationToken(string installedAppId, ulong expirySeconds = 0, bool singleUse = false, string id = null)` | method | Issue a new app authentication token for the given installed app, used to authenticate connections on the app interface. |
| `Task<AppAuthenticationTokenIssuedCallBackEventArgs> IssueAppAuthenticationTokenAsync(string installedAppId, ulong expirySeconds = 0, bool singleUse = false, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Issue a new app authentication token for the given installed app, used to authenticate connections on the app interface. |
| `AppsListedCallBackEventArgs ListApps(AppStatusFilter appStatusFilter, string id = null)` | method | Returns all installed hApps matching the filter (`Running`, `Stopped`, `Disabled`, `Paused`, or `All`). |
| `Task<AppsListedCallBackEventArgs> ListAppsAsync(AppStatusFilter appStatusFilter, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns all installed hApps matching the filter (`Running`, `Stopped`, `Disabled`, `Paused`, or `All`). |
| `CapabilityGrantsListedCallBackEventArgs ListCapabilityGrants(string installedAppId, bool includeRevoked = false, string id = null)` | method | List the capability grants for a given installed app. |
| `Task<CapabilityGrantsListedCallBackEventArgs> ListCapabilityGrantsAsync(string installedAppId, bool includeRevoked = false, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | List the capability grants for a given installed app. |
| `CellIdsListedCallBackEventArgs ListCellIds(string id = null)` | method | Returns all cell IDs (agentPubKey + dnaHash pairs) currently held by the conductor. |
| `Task<CellIdsListedCallBackEventArgs> ListCellIdsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns all cell IDs (agentPubKey + dnaHash pairs) currently held by the conductor. |
| `DnasListedCallBackEventArgs ListDnas(string id = null)` | method | Returns the hashes of all DNA bundles registered with the conductor. |
| `Task<DnasListedCallBackEventArgs> ListDnasAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the hashes of all DNA bundles registered with the conductor. |
| `AppInterfacesListedCallBackEventArgs ListInterfaces(string id = null)` | method | Returns all app WebSocket interface ports currently open on the conductor. |
| `Task<AppInterfacesListedCallBackEventArgs> ListInterfacesAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns all app WebSocket interface ports currently open on the conductor. |
| `DnaRegisteredCallBackEventArgs RegisterDna(string path, string network_seed = null, object properties = null, string id = null)` | method |  **(obsolete)** **(obsolete)** |
| `DnaRegisteredCallBackEventArgs RegisterDna(byte[] hash, string network_seed = null, object properties = null, string id = null)` | method |  **(obsolete)** **(obsolete)** |
| `DnaRegisteredCallBackEventArgs RegisterDna(DnaBundle bundle, string network_seed = null, object properties = null, string id = null)` | method |  **(obsolete)** **(obsolete)** |
| `Task<DnaRegisteredCallBackEventArgs> RegisterDnaAsync(string path, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method |  **(obsolete)** **(obsolete)** |
| `Task<DnaRegisteredCallBackEventArgs> RegisterDnaAsync(byte[] hash, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method |  **(obsolete)** **(obsolete)** |
| `Task<DnaRegisteredCallBackEventArgs> RegisterDnaAsync(DnaBundle bundle, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method |  **(obsolete)** **(obsolete)** |
| `AppAuthenticationTokenRevokedCallBackEventArgs RevokeAppAuthenticationToken(byte[] token, string id = null)` | method | Revoke a previously issued app authentication token. |
| `Task<AppAuthenticationTokenRevokedCallBackEventArgs> RevokeAppAuthenticationTokenAsync(byte[] token, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Revoke a previously issued app authentication token. |
| `ZomeCallCapabilityRevokedCallBackEventArgs RevokeZomeCallCapability(byte[] actionHash, CellId cellId, string id = null)` | method | Revoke a previously granted zome call capability. |
| `Task<ZomeCallCapabilityRevokedCallBackEventArgs> RevokeZomeCallCapabilityAsync(byte[] actionHash, CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Revoke a previously granted zome call capability. |
| `AppUninstalledCallBackEventArgs UninstallApp(string installedAppId, string id = null)` | method | Permanently removes a hApp installation. All cells are deleted; the DHT data remains on the network but the local agent is removed. |
| `Task<AppUninstalledCallBackEventArgs> UninstallAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Permanently removes a hApp installation. All cells are deleted; the DHT data remains on the network but the local agent is removed. |
| `CoordinatorsUpdatedCallBackEventArgs UpdateCoordinators(byte[] dnaHash, string path, string id = null)` | method | Update the coordinataor zomes. |
| `CoordinatorsUpdatedCallBackEventArgs UpdateCoordinators(string dnaHash, string path, string id = null)` | method | Update the coordinataor zomes. |
| `CoordinatorsUpdatedCallBackEventArgs UpdateCoordinators(byte[] dnaHash, CoordinatorBundle bundle, string id = null)` | method | Update the coordinataor zomes. |
| `CoordinatorsUpdatedCallBackEventArgs UpdateCoordinators(string dnaHash, CoordinatorBundle bundle, string id = null)` | method | Update the coordinataor zomes. |
| `Task<CoordinatorsUpdatedCallBackEventArgs> UpdateCoordinatorsAsync(byte[] dnaHash, string path, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Update the coordinataor zomes. |
| `Task<CoordinatorsUpdatedCallBackEventArgs> UpdateCoordinatorsAsync(string dnaHash, string path, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Update the coordinataor zomes. |
| `Task<CoordinatorsUpdatedCallBackEventArgs> UpdateCoordinatorsAsync(byte[] dnaHash, CoordinatorBundle bundle, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Update the coordinataor zomes. |
| `Task<CoordinatorsUpdatedCallBackEventArgs> UpdateCoordinatorsAsync(string dnaHash, CoordinatorBundle bundle, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Update the coordinataor zomes. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientapp"></a>
### HoloNETClientApp (class)

Inherits / implements: `HoloNETClientAppBase`, `IHoloNETClientBase`, `IDisposable`, `IHoloNETClientAppBase`

| Member | Kind | Description |
|---|---|---|
| `HoloNETClientApp(IHoloNETDNA holoNETDNA = null)` | ctor | This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA. |
| `HoloNETClientApp(ILogProvider logProvider, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientApp(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientApp(ILogger logger, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappagent"></a>
### HoloNETClientAppAgent (class)

Inherits / implements: `HoloNETClientAppBase`, `IHoloNETClientBase`, `IDisposable`, `IHoloNETClientAppBase`, `IHoloNETClientAppAgent`

| Member | Kind | Description |
|---|---|---|
| `HoloNETClientAppAgent(string installedAppId, string myAgentPubKey, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA. |
| `HoloNETClientAppAgent(string installedAppId, string myAgentPubKey, ILogProvider logProvider, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientAppAgent(string installedAppId, string myAgentPubKey, IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientAppAgent(string installedAppId, string myAgentPubKey, ILogger logger, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETConnectedEventArgs Connect(string installedAppId, string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. Please see the Events section above for more info on how to use this event. |
| `HoloNETConnectedEventArgs Connect(string installedAppId, Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. Please see the Events section above for more info on how to use this event. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string installedAppId, Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. Please see the Events section above for more info on how to use this event. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string installedAppId, string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. Please see the Events section above for more info on how to use this event. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase"></a>
### HoloNETClientAppBase (abstract class)

Inherits / implements: `HoloNETClientBase`, `IHoloNETClientBase`, `IDisposable`, `IHoloNETClientAppBase`

| Member | Kind | Description |
|---|---|---|
| `HoloNETClientAppBase(IHoloNETDNA holoNETDNA = null)` | ctor | This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA. |
| `HoloNETClientAppBase(ILogProvider logProvider, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientAppBase(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientAppBase(ILogger logger, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `AppInfo CachedAppInfo { get; set; }` | property | The last app_info this client received (agent, DNA, cells, status). |
| `bool IsReadyForZomesCalls { get; }` | property | This property is a boolean and will return true when HoloNET is ready for zome calls after the OnReadyForZomeCalls event is raised. |
| `bool RetrievingAgentPubKeyAndDnaHash { get; }` | property | This property is a boolean and will return true when HoloNET is retrieving the AgentPubKey & DnaHash using one of the RetrieveAgentPubKeyAndDnaHash, RetrieveAgentPubKeyAndDnaHashFromSandbox or RetrieveAgentPubKeyAndDnaHashFromConductor methods (by default this will occur automatically after it has connected to the Holochain Conductor). |
| `IZomeCallSigner ZomeCallSigner { get; set; }` | property | Optional external signer. When set, every zome call is signed by it instead of by the local credentials from AuthorizeSigningCredentials (e.g. the shared Android runtime's signZomeCall). |
| `HoloNETClientAppBase.AppAgentInfoReturnedCallBack OnAgentInfoReturnedCallBack` | event | Fired when a response arrives for GetAgentInfoAsync (app interface). |
| `HoloNETClientAppBase.AppEnabledViaAppInterfaceCallBack OnAppEnabledCallBack` | event | Fired when a response arrives for EnableAppAsync (app interface, e.g. after providing membrane proofs). |
| `HoloNETClientAppBase.AppInfoCallBack OnAppInfoCallBack` | event | Fired when the client receives AppInfo from the conductor containing the cell id for the running hApp (which in itself contains the AgentPubKey & DnaHash). It also contains the AppId and other info. |
| `HoloNETClientAppBase.AppOpTimingsDumpedCallBack OnAppOpTimingsDumpedCallBack` | event | Fired when a response is received from the conductor after the DumpOpTimingsAsync/DumpOpTimings method is called on the app interface. |
| `HoloNETClientAppBase.AppPeerMetaInfoReturnedCallBack OnAppPeerMetaInfoReturnedCallBack` | event | Fired when a response is received from the conductor after the GetAppPeerMetaInfoAsync/GetAppPeerMetaInfo method is called. |
| `HoloNETClientAppBase.CloneCellCreatedCallBack OnCloneCellCreatedCallBack` | event | Fired when a response is received from the conductor after the CreateCloneCellAsync/CreateCloneCell method is called. |
| `HoloNETClientAppBase.CloneCellDisabledCallBack OnCloneCellDisabledCallBack` | event | Fired when a response is received from the conductor after the DisableCloneCellAsync/DisableCloneCell method is called. |
| `HoloNETClientAppBase.CloneCellEnabledCallBack OnCloneCellEnabledCallBack` | event | Fired when a response is received from the conductor after the EnableCloneCellAsync/EnableCloneCell method is called. |
| `HoloNETClientAppBase.CountersigningSessionAbandonedCallBack OnCountersigningSessionAbandonedCallBack` | event | Fired when a response is received from the conductor after the AbandonCountersigningSessionAsync/AbandonCountersigningSession method is called. |
| `HoloNETClientAppBase.CountersigningSessionStateReturnedCallBack OnCountersigningSessionStateReturnedCallBack` | event | Fired when a response is received from the conductor after the GetCountersigningSessionStateAsync/GetCountersigningSessionState method is called. |
| `HoloNETClientAppBase.DirectSignalCapabilityGrantedCallBack OnDirectSignalCapabilityGrantedCallBack` | event | Fired when a response arrives for GrantDirectSignalCapabilityAsync. |
| `HoloNETClientAppBase.DirectSignalSentCallBack OnDirectSignalSentCallBack` | event | Fired when a response arrives for SendDirectSignalAsync. |
| `HoloNETClientAppBase.MemproofsProvidedCallBack OnMemproofsProvidedCallBack` | event | Fired when a response is received from the conductor after the ProvideMemproofsAsync/ProvideMemproofs method is called. |
| `HoloNETClientAppBase.AppNetworkMetricsDumpedCallBack OnNetworkMetricsDumpedCallBack` | event | Fired when a response arrives for DumpNetworkMetricsAsync (app interface). |
| `HoloNETClientAppBase.AppNetworkStatsDumpedCallBack OnNetworkStatsDumpedCallBack` | event | Fired when a response arrives for DumpNetworkStatsAsync (app interface). |
| `HoloNETClientAppBase.PublishCountersigningSessionTriggeredCallBack OnPublishCountersigningSessionTriggeredCallBack` | event | Fired when a response is received from the conductor after the PublishCountersigningSessionAsync/PublishCountersigningSession method is called. |
| `HoloNETClientAppBase.ReadyForZomeCalls OnReadyForZomeCalls` | event | Fired when the client has successfully connected and reteived the AgentPubKey & DnaHash, meaning it is ready to make zome calls to the Holochain conductor. |
| `HoloNETClientAppBase.SignalCallBack OnSignalCallBack` | event | Fired when the Holochain conductor sends signals data. |
| `HoloNETClientAppBase.SignalCallBack OnSystemSignalCallBack` | event | System signals from the conductor (successful_countersigning, abandoned_countersigning, restore_complete, ...). OnSignalCallBack only carries app signals; these were previously dropped entirely. |
| `HoloNETClientAppBase.WasmHostFunctionsListedCallBack OnWasmHostFunctionsListedCallBack` | event | Fired when a response is received from the conductor after the ListWasmHostFunctionsAsync/ListWasmHostFunctions method is called. |
| `HoloNETClientAppBase.ZomeFunctionCallBack OnZomeFunctionCallBack` | event | Fired when the Holochain conductor returns the response from a zome function call. This returns the raw data as well as the parsed data returned from the zome function. It also returns the id, zome and zome function that made the call. |
| `CountersigningSessionAbandonedCallBackEventArgs AbandonCountersigningSession(CellId cellId, string id = null)` | method | Abandon an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds. |
| `Task<CountersigningSessionAbandonedCallBackEventArgs> AbandonCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Abandon an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, object entryDataObjectReturnedFromZome = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, object entryDataObjectReturnedFromZome = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, object entryDataObjectReturnedFromZome = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse, CallZomeOptions callZomeOptions = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. |
| `void ClearCache(bool clearPendingRequsts = false)` | method | Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the CallZomeFunction methods if the `cacheData` param was set to true for any of the calls. |
| `HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string installedAppId, Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `static object ConvertEntryValue(string value, Type type)` | method | Converts a decoded entry value (see DecodeRawZomeData, which formats with the invariant culture) to : nullable types, enums (by name or number), Guid, DateTime / DateTimeOffset and every IConvertible type. Unconvertible values throw, and MapEntryDataObjectAsync skips that property. |
| `static byte[] CreateAuthenticationEnvelope(byte[] token)` | method | The authenticate message HoloNET sends first on an app interface, carrying the app authentication token. |
| `CloneCellCreatedCallBackEventArgs CreateCloneCell(string roleName, object modifiers = null, byte[] membraneProof = null, string name = null, string id = null)` | method | Create a new clone cell for the given role. |
| `Task<CloneCellCreatedCallBackEventArgs> CreateCloneCellAsync(string roleName, object modifiers = null, byte[] membraneProof = null, string name = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Create a new clone cell for the given role. |
| `CloneCellDisabledCallBackEventArgs DisableCloneCell(object cloneCellId, string id = null)` | method | Disable a clone cell. |
| `Task<CloneCellDisabledCallBackEventArgs> DisableCloneCellAsync(object cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Disable a clone cell. |
| `NetworkMetricsDumpedCallBackEventArgs DumpNetworkMetrics(byte[] dnaHash = null, bool includeDhtSummary = false, string id = null)` | method | Kitsune network metrics for this app's DNAs (or just ) as JSON (app interface, Holochain 0.7). |
| `Task<NetworkMetricsDumpedCallBackEventArgs> DumpNetworkMetricsAsync(byte[] dnaHash = null, bool includeDhtSummary = false, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Kitsune network metrics for this app's DNAs (or just ) as JSON (app interface, Holochain 0.7). |
| `NetworkStatsDumpedCallBackEventArgs DumpNetworkStats(string id = null)` | method | Transport statistics from the networking backend as JSON (app interface, Holochain 0.7). |
| `Task<NetworkStatsDumpedCallBackEventArgs> DumpNetworkStatsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Transport statistics from the networking backend as JSON (app interface, Holochain 0.7). |
| `AppOpTimingsDumpedCallBackEventArgs DumpOpTimings(byte[] dnaHash, OpTimingsCursor cursor = null, uint? limit = null, string id = null)` | method | Dump DHT op timing information for a DNA via the app interface (new in Holochain 0.7.0). |
| `Task<AppOpTimingsDumpedCallBackEventArgs> DumpOpTimingsAsync(byte[] dnaHash, OpTimingsCursor cursor = null, uint? limit = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump DHT op timing information for a DNA via the app interface (new in Holochain 0.7.0). The app must be running a cell of this DNA. Results are paginated; pass the cursor from a previous call to retrieve the next page. |
| `AppEnabledViaAppInterfaceCallBackEventArgs EnableApp(string id = null)` | method | Enables the app this connection is authenticated for (app interface enable_app, Holochain 0.7), e.g. once deferred memproofs have been provided. |
| `Task<AppEnabledViaAppInterfaceCallBackEventArgs> EnableAppAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Enables the app this connection is authenticated for (app interface enable_app, Holochain 0.7), e.g. once deferred memproofs have been provided. |
| `CloneCellEnabledCallBackEventArgs EnableCloneCell(object cloneCellId, string id = null)` | method | Enable a previously disabled clone cell. |
| `Task<CloneCellEnabledCallBackEventArgs> EnableCloneCellAsync(object cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Enable a previously disabled clone cell. |
| `AgentInfoReturnedCallBackEventArgs GetAgentInfo(IEnumerable<byte[]> dnaHashes = null, string id = null)` | method | List the names of the host functions available to WASM code running in this conductor. |
| `Task<AgentInfoReturnedCallBackEventArgs> GetAgentInfoAsync(IEnumerable<byte[]> dnaHashes = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | List the names of the host functions available to WASM code running in this conductor. |
| `AppInfoCallBackEventArgs GetAppInfo(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse)` | method | This method gets the AppInfo from the Holochain Conductor. |
| `Task<AppInfoCallBackEventArgs> GetAppInfoAsync(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse)` | method | This method gets the AppInfo from the Holochain Conductor. |
| `AppPeerMetaInfoReturnedCallBackEventArgs GetAppPeerMetaInfo(string url, List<byte[]> dnaHashes = null, string id = null)` | method | Get meta info about a peer that is known via the networking layer (kitsune2), via the app interface. |
| `Task<AppPeerMetaInfoReturnedCallBackEventArgs> GetAppPeerMetaInfoAsync(string url, List<byte[]> dnaHashes = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Get meta info about a peer that is known via the networking layer (kitsune2), via the app interface. |
| `Task<byte[][]> GetCellIdAsync()` | method | The cell id zome calls go to: HoloNETDNA.CellId, or built from its DnaHash and AgentPubKey (retrieving them from the conductor first if needed). |
| `CountersigningSessionStateReturnedCallBackEventArgs GetCountersigningSessionState(CellId cellId, string id = null)` | method | Get the state of a countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds. |
| `Task<CountersigningSessionStateReturnedCallBackEventArgs> GetCountersigningSessionStateAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Get the state of a countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds. |
| `DirectSignalCapabilityGrantedCallBackEventArgs GrantDirectSignalCapability(byte[][] cellId, string tag, CapGrantAccessType access = Unrestricted, byte[] secret = null, byte[][] assignees = null, string id = null)` | method | Holochain 0.7.1+: lets other agents send direct signals to (a cell of this app) (grant_direct_signal_capability). Holochain 0.7.0 doesn't have this request and returns an error. |
| `Task<DirectSignalCapabilityGrantedCallBackEventArgs> GrantDirectSignalCapabilityAsync(byte[][] cellId, string tag, CapGrantAccessType access = Unrestricted, byte[] secret = null, byte[][] assignees = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Holochain 0.7.1+: lets other agents send direct signals to (a cell of this app) (grant_direct_signal_capability). Holochain 0.7.0 doesn't have this request and returns an error. |
| `WasmHostFunctionsListedCallBackEventArgs ListWasmHostFunctions(string id = null)` | method | List the names of the host functions available to WASM code running in this conductor. |
| `Task<WasmHostFunctionsListedCallBackEventArgs> ListWasmHostFunctionsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | List the names of the host functions available to WASM code running in this conductor. |
| `object MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true)` | method | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass. |
| `object MapEntryDataObject(object entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true)` | method | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass. |
| `Task<object> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true)` | method | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass. |
| `Task<object> MapEntryDataObjectAsync(object entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true)` | method | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass. |
| `MemproofsProvidedCallBackEventArgs ProvideMemproofs(Dictionary<string, byte[]> membraneProofs, string id = null)` | method | Provide the membrane proofs for this app, used when the app was installed with `allow_deferred_memproofs` and the membrane proofs were not provided at installation time. |
| `Task<MemproofsProvidedCallBackEventArgs> ProvideMemproofsAsync(Dictionary<string, byte[]> membraneProofs, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Provide the membrane proofs for this app, used when the app was installed with `allow_deferred_memproofs` and the membrane proofs were not provided at installation time. |
| `PublishCountersigningSessionTriggeredCallBackEventArgs PublishCountersigningSession(CellId cellId, string id = null)` | method | Publish (force-finalise) an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds. |
| `Task<PublishCountersigningSessionTriggeredCallBackEventArgs> PublishCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Publish (force-finalise) an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds. |
| `AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHash(string installedAppId = null, string roleName = null, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method will retrieve the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retrieving from the Conductor first. It will call RetrieveAgentPubKeyAndDnaHashFromConductor and RetrieveAgentPubKeyAndDnaHashFromSandboxAsync internally. |
| `Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashAsync(string installedAppId = null, string roleName = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method will retrieve the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retrieving from the Conductor first. It will call RetrieveAgentPubKeyAndDnaHashFromConductor and RetrieveAgentPubKeyAndDnaHashFromSandboxAsync internally. |
| `AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromConductor(string installedAppId = null, string roleName = null, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true)` | method | This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retrieved them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retrieve the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetrieveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the \|retrieveAgentPubKeyAndDnaHashFromSandbox method. |
| `Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromConductorAsync(string installedAppId = null, string roleName = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true)` | method | This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retrieved them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retrieve the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetrieveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the \|retrieveAgentPubKeyAndDnaHashFromSandbox method. |
| `AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromSandbox(bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)` | method | This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the OnReadyForZomeCalls event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the \|retrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default). |
| `Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)` | method | This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the OnReadyForZomeCalls event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the \|retrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default). |
| `DirectSignalSentCallBackEventArgs SendDirectSignal(byte[] dnaHash, IEnumerable<byte[]> agents, byte[] signal, byte[] capSecret = null, string id = null)` | method | Sends (opaque bytes) to on the DNA (send_direct_signal). Best effort: the conductor confirms it was sent, not delivered. On Holochain 0.7.1+ a recipient only accepts it if it has a direct-signal grant for the sender (see `GrantDirectSignalCapabilityAsync`); pass that grant's secret as when it has one. Recipients get it as an app signal. |
| `Task<DirectSignalSentCallBackEventArgs> SendDirectSignalAsync(byte[] dnaHash, IEnumerable<byte[]> agents, byte[] signal, byte[] capSecret = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Sends (opaque bytes) to on the DNA (send_direct_signal). Best effort: the conductor confirms it was sent, not delivered. On Holochain 0.7.1+ a recipient only accepts it if it has a direct-signal grant for the sender (see `GrantDirectSignalCapabilityAsync`); pass that grant's secret as when it has one. Recipients get it as an app signal. |
| `static ZomeCallParamsSigned SignZomeCallLocally(ZomeCallToSign call, SigningCredentials credentials)` | method | Signs a zome call with locally held credentials (the default when ZomeCallSigner is unset). Public so a custom signer can delegate to it. |
| `ZomeCallParamsSigned SignZomeCallWithAuthorizedCredentials(ZomeCallToSign call)` | method | Signs with the credentials AuthorizeSigningCredentials stored for the call's cell, or returns null when none have been authorized. |
| `static string ToInvariantString(object value)` | method | Formats a decoded msgpack value with the invariant culture ("null" for null), so ConvertEntryValue round-trips it. |
| `Task<ReadyForZomeCallsEventArgs> WaitTillReadyForZomeCallsAsync()` | method | This method will wait (non blocking) until HoloNET is ready to make zome calls after it has connected to the Holochain Conductor and retrived the AgentPubKey & DnaHash. It will then return to the caller with the AgentPubKey & DnaHash. This method will return the same time the OnReadyForZomeCalls event is raised. Unlike all the other methods, this one only contains an async version because the non async version would block all other threads including any UI ones etc. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase"></a>
### HoloNETClientBase (abstract class)

Inherits / implements: `IHoloNETClientBase`, `IDisposable`

| Member | Kind | Description |
|---|---|---|
| `HoloNETClientBase(IHoloNETDNA holoNETDNA = null)` | ctor | This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA. |
| `HoloNETClientBase(ILogProvider logProvider, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientBase(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `HoloNETClientBase(ILogger logger, IHoloNETDNA holoNETDNA = null)` | ctor | This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA. |
| `Uri EndPoint { get; set; }` | property | This property is a shortcut to the EndPoint property on the WebSocket property. |
| `IHoloNETDNA HoloNETDNA { get; set; }` | property | The HoloNET settings in use (see HoloNETDNA). |
| `bool IsConnecting { get; set; }` | property | True while ConnectAsync is in progress. |
| `bool IsDisconnecting { get; set; }` | property | True while DisconnectAsync is in progress. |
| `bool IsHoloNETDNALoaded { get; }` | property | True once the settings have been loaded (or set). |
| `ILogger Logger { get; set; }` | property | The logger HoloNET writes to (the built-in DefaultLogger unless you pass your own log providers or logger). |
| `WebSocketState State { get; }` | property | This property is a shortcut to the State property on the WebSocket property. |
| `WebSocket WebSocket { get; set; }` | property | This property contains the internal NextGenSoftware.WebSocket (https://www.nuget.org/packages/NextGenSoftware.WebSocket) that HoloNET uses. You can use this property to access the current state of the WebSocket as well as configure more options. |
| `HoloNETClientBase.Connected OnConnected` | event | Fired when the client has successfully connected to the Holochain conductor. |
| `HoloNETClientBase.DataReceived OnDataReceived` | event | Fired when any data is received from the Holochain conductor. This returns the raw data. |
| `HoloNETClientBase.DataSent OnDataSent` | event | Fired when any data is sent to the Holochain conductor. |
| `HoloNETClientBase.Disconnected OnDisconnected` | event | Fired when the client disconnected from the Holochain conductor. |
| `HoloNETClientBase.Error OnError` | event | Fired when an error occurs, check the params for the cause of the error. |
| `HoloNETClientBase.HolochainConductorsShutdownComplete OnHolochainConductorsShutdownComplete` | event | Fired when all Holochain Conductors have been shutdown. |
| `HoloNETClientBase.HolochainConductorStarted OnHolochainConductorStarted` | event | Fired when the Holochain Conductor has been started. |
| `HoloNETClientBase.HolochainConductorStarting OnHolochainConductorStarting` | event | Fired when the Holochain Conductor has been started. |
| `HoloNETClientBase.HoloNETShutdownComplete OnHoloNETShutdownComplete` | event | Fired when HoloNET has completed shutting down (this includes closing all connections and shutting down all Holochain Conductor). |
| `void ClearCache(bool clearPendingRequsts = false)` | method | Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the CallZomeFunction methods if the `cacheData` param was set to true for any of the calls. |
| `HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. Please see the Events section above for more info on how to use this event. |
| `HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. Please see the Events section above for more info on how to use this event. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. Please see the Events section above for more info on how to use this event. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. Please see the Events section above for more info on how to use this event. |
| `byte[] ConvertHoloHashToBytes(string hash)` | method | Utility method to convert a string to base64 encoded bytes (Holochain Conductor format). This is used to convert the AgentPubKey & DnaHash when making a zome call.\| |
| `string ConvertHoloHashToString(byte[] bytes)` | method | Utility method to convert from base64 bytes (Holochain Conductor format) to a friendly C# format. This is used to convert the AgentPubKey & DnaHash retrieved from the Conductor.\| |
| `static T DeserializeResponseValue<T>(byte[] envelope, MessagePackSerializerOptions options)` | method | Decode a conductor response envelope's value as T (MessagePack). |
| `HoloNETDisconnectedEventArgs Disconnect(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownAllHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected event once it is disconnected. |
| `Task<HoloNETDisconnectedEventArgs> DisconnectAsync(DisconnectedCallBackMode disconnectedCallBackMode = WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownAllHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected event once it is disconnected. |
| `void Dispose()` | method | Disconnect and release the WebSocket. |
| `static string EmbeddedHolochainVersionString(EmbeddedHolochainVersion version)` | method | Folder name for a bundled release, e.g. "0.7.1". |
| `byte[][] GetCellId()` | method | Sync version of GetCellIdAsync. |
| `byte[][] GetCellId(string DnaHash, string AgentPubKey)` | method | Build a cell id from a DNA hash and an agent public key (both as HoloHash strings). |
| `byte[][] GetCellId(byte[] DnaHash, byte[] AgentPubKey)` | method | Build a cell id from a DNA hash and an agent public key (raw bytes). |
| `Task<byte[][]> GetCellIdAsync()` | method | The cell id zome calls go to: HoloNETDNA.CellId, or built from its DnaHash and AgentPubKey (retrieving them from the conductor first if needed). |
| `static bool IsEmbeddedHolochainVersionBundled(EmbeddedHolochainVersion version)` | method | True when this build of HoloNET carries that conductor version: the Embedded package carries 0.7.1 and 0.7.0, Embedded.Holochain071 / Embedded.Holochain070 only one, and the lightweight package none. |
| `IHoloNETDNA LoadDNA()` | method | Load the settings from HoloNETDNA.json (HoloNETDNAManager.HoloNETDNAPath). |
| `static byte[] ReadRawValue(byte[] envelope)` | method | Returns the raw msgpack of the envelope's "value" (null when absent). |
| `static string ReadResponseType(byte[] envelope)` | method | Reads only the envelope's "type". Dispatch must not deserialise the whole payload as dynamic: payloads keyed by hashes (e.g. network_metrics_dumped's HashMap<DnaHash, _>) throw under MessagePackSecurity.UntrustedData, and the response was then never routed. |
| `bool SaveDNA()` | method | Save the current settings to HoloNETDNA.json. |
| `void SendHoloNETRequest(HoloNETData holoNETData, HoloNETRequestType requestType, string id = "")` | method | This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor. |
| `void SendHoloNETRequest(byte[] data, HoloNETRequestType requestType, string id = "")` | method | This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor. |
| `Task SendHoloNETRequestAsync(HoloNETData holoNETData, HoloNETRequestType requestType, string id = "")` | method | This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor. |
| `Task SendHoloNETRequestAsync(byte[] data, HoloNETRequestType requestType, string id = "")` | method | This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor. |
| `static string SetConductorConfigAdminPort(string config, int port)` | method | Returns (a conductor-config.yaml) with the first admin interface's port set to . Only the "port:" inside the top-level admin_interfaces block is touched; if that block is null, empty or missing a websocket admin interface is written in the format holochain --create-config generates. |
| `HolochainConductorsShutdownEventArgs ShutDownHolochainConductors(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | Will automatically shutdown the current Holochain Conductor (if the `shutdownHolochainConductorsMode` param is set to `ShutdownCurrentConductorOnly`) or all Holochain Conductors (if the `shutdownHolochainConductorsMode` param is set to `ShutdownAllConductors`). If the `shutdownHolochainConductorsMode` param is set to `UseHoloNETDNASettings` then it will use the `HoloNETClient.HoloNETDNA.AutoShutdownHolochainConductor` and `HoloNETClient.HoloNETDNA.ShutDownALLHolochainConductors` flags to determine which mode to use. The Disconnect method will automatically call this once it has finished disconnecting from the Holochain Conductor. The ShutdownHoloNET will also call this method. |
| `Task<HolochainConductorsShutdownEventArgs> ShutDownHolochainConductorsAsync(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | Will automatically shutdown the current Holochain Conductor (if the `shutdownHolochainConductorsMode` param is set to `ShutdownCurrentConductorOnly`) or all Holochain Conductors (if the `shutdownHolochainConductorsMode` param is set to `ShutdownAllConductors`). If the `shutdownHolochainConductorsMode` param is set to `UseHoloNETDNASettings` then it will use the `HoloNETClient.HoloNETDNA.AutoShutdownHolochainConductor` and `HoloNETClient.HoloNETDNA.ShutDownALLHolochainConductors` flags to determine which mode to use. The Disconnect method will automatically call this once it has finished disconnecting from the Holochain Conductor. The ShutdownHoloNET will also call this method. |
| `HoloNETShutdownEventArgs ShutdownHoloNET(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | This method will shutdown HoloNET by first calling the Disconnect method to disconnect from the Holochain Conductor and then calling the ShutDownHolochainConductors method to shutdown any running Holochain Conductors. This method will then raise the OnHoloNETShutdown event. This method works very similar to the Disconnect method except it also clears the loggers, does any other shutdown tasks necessary and then returns a `HoloNETShutdownEventArgs` object. You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller. |
| `Task<HoloNETShutdownEventArgs> ShutdownHoloNETAsync(DisconnectedCallBackMode disconnectedCallBackMode = WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | This method will shutdown HoloNET by first calling the Disconnect method to disconnect from the Holochain Conductor and then calling the ShutDownHolochainConductors method to shutdown any running Holochain Conductors. This method will then raise the OnHoloNETShutdown event. This method works very similar to the Disconnect method except it also clears the loggers, does any other shutdown tasks necessary and then returns a `HoloNETShutdownEventArgs` object. You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller. |
| `void StartHolochainConductor()` | method | This method will start the Holochain Conducutor using the appropriate settings defined in the HoloNETDNA property. |
| `Task StartHolochainConductorAsync()` | method | This method will start the Holochain Conducutor using the appropriate settings defined in the HoloNETDNA property. |

<a id="nextgensoftware-holochain-holonet-client-holonetconnectedeventargs"></a>
### HoloNETConnectedEventArgs (class)

Inherits / implements: `CallBackBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `bool IsConnected { get; set; }` | property | What the conductor returned for this request (is connected). |

<a id="nextgensoftware-holochain-holonet-client-holonetdata"></a>
### HoloNETData (class)

Inherits / implements: `IHoloNETData`

| Member | Kind | Description |
|---|---|---|
| `object data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetdatareceivedbasebaseeventargs-1"></a>
### HoloNETDataReceivedBaseBaseEventArgs&lt;T&gt; (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `T Response { get; set; }` | property | The decoded conductor response. |

<a id="nextgensoftware-holochain-holonet-client-holonetdatareceivedbaseeventargs"></a>
### HoloNETDataReceivedBaseEventArgs (class)

Inherits / implements: `CallBackBaseEventArgsWithDataAndId`

| Member | Kind | Description |
|---|---|---|
| `HoloNETResponseType HoloNETResponseType { get; set; }` | property | What the conductor returned for this request (holo netresponse type). |
| `string Type { get; set; }` | property | The conductor message type. |

<a id="nextgensoftware-holochain-holonet-client-holonetdatareceivedeventargs"></a>
### HoloNETDataReceivedEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `bool IsConductorDebugInfo { get; set; }` | property | What the conductor returned for this request (is conductor debug info). |

<a id="nextgensoftware-holochain-holonet-client-holonetdatasenteventargs"></a>
### HoloNETDataSentEventArgs (class)

Inherits / implements: `DataSentEventArgs`

<a id="nextgensoftware-holochain-holonet-client-holonetdisconnectedeventargs"></a>
### HoloNETDisconnectedEventArgs (class)

Inherits / implements: `CallBackBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `HolochainConductorsShutdownEventArgs HolochainConductorsShutdownResult { get; set; }` | property | Result of the automatic conductor shutdown that follows a disconnect (WaitForHolochainConductorToDisconnect mode only). |
| `bool IsDisconnected { get; set; }` | property | What the conductor returned for this request (is disconnected). |

<a id="nextgensoftware-holochain-holonet-client-holonetdna"></a>
### HoloNETDNA (class)

Inherits / implements: `IHoloNETDNA`

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; set; }` | property | The AgentPubKey to use for Zome calls. If this is not set then HoloNET will automatically retrieve this along with the DnaHash after it connects (if the Connect method defaults are not overriden). |
| `byte[] AppAuthenticationToken { get; set; }` | property | Holochain 0.7 app-interface authentication token. When supplied, app clients send the mandatory authenticate envelope immediately after the WebSocket opens and before any conductor request. Admin clients never use this value. |
| `bool AutoShutdownHolochainConductor { get; set; }` | property | Set this to true if you wish HoloNET to auto-shutdown the Holochain Conductor after it disconnects. Default is true. |
| `bool AutoStartHolochainConductor { get; set; }` | property | Set this to true if you with HoloNET to auto-start the Holochain Conductor defined in the `FullPathToExternalHolochainConductorBinary parameter if `HolochainConductorToUse` is `Holochain`, otherwise if it`s `Hc` then it will use `FullPathToExternalHCToolBinary`. Default is true. |
| `CacheConfig CacheConfig { get; set; }` | property | HoloNET client-side cache configuration. This is a HoloNET-specific concept, not a mirror of any Holochain Rust struct - see CacheConfig.cs. Not yet consumed by any caching implementation in HoloNET; present in the object graph only, ready for a future client-side cache layer to read from. |
| `byte[][] CellId { get; set; }` | property | THe CellId to use for Zome calls (this contains the DnaHash and AgentPubKey). If this is not set then HoloNET will automatically retrieve this along with the AgentPubKey after it connects (if the Connect method defaults are not overriden). |
| `LoggingMode ConsoleLoggingMode { get; set; }` | property | This passes through to the static LogConfig.ConsoleLoggingMode property in [NextGenSoftware.Logging](https://www.nuget.org/packages/NextGenSoftware.Logging) package. It can be either `WarningsErrorsInfoAndDebug`, `WarningsErrorsAndInfo`, `WarningsAndErrors` or `ErrorsOnly`. |
| `ConsoleColor DebugColour { get; set; }` | property | The colour to use for `Debug` log entries to the console NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `string DnaHash { get; set; }` | property | The DnaHash to use for Zome calls. If this is not set then HoloNET will automatically retrieve this along with the AgentPubKey after it connects (if the Connect method defaults are not overriden). |
| `EmbeddedHolochainVersion EmbeddedHolochainVersion { get; set; }` | property | Which bundled Holochain release to start when `HolochainConductorMode` is `UseEmbedded` (Embedded package only). Defaults to 0.7.1; set V0_7_0 for hApps built against hdk 0.7.0. |
| `EnforceRequestToResponseIdMatchingBehaviour EnforceRequestToResponseIdMatchingBehaviour { get; set; }` | property | Set whether you wish to ignore, warn or error when the response id received does not have a matching request id. If it is Ignore it will do nothing, if it is Warn it will give a warning in the Message property of responses returned but still continue, if it is Error it will give a message in the Message property, set the IsError flag and abort the request. |
| `ConsoleColor ErrorColour { get; set; }` | property | The colour to use for `Error` log entries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `ErrorHandlingBehaviour ErrorHandlingBehaviour { get; set; }` | property | Whether errors only raise OnError (the default) or also throw. |
| `LoggingMode FileLoggingMode { get; set; }` | property | This passes through to the static LogConfig.FileLoggingMode property in [NextGenSoftware.Logging](https://www.nuget.org/packages/NextGenSoftware.Logging) package. It can be either `WarningsErrorsInfoAndDebug`, `WarningsErrorsAndInfo`, `WarningsAndErrors` or `ErrorsOnly`. |
| `string FullPathToCompiledHappFolder { get; set; }` | property | The full path to the compiled hApp that HoloNET will start the Holochain Conductor (currenty uses hc.exe) with and then make zome calls to. |
| `string FullPathToExternalHCToolBinary { get; set; }` | property | The full path to the Holochain Conductor exe (holochain.exe) that HoloNET will auto-start if `HolochainConductorToUse` is set to `Holochain`. |
| `string FullPathToExternalHolochainConductorBinary { get; set; }` | property | This is the Holochain Conductor to use for the auto-start Holochain Conductor feature. It can be either `Holochain` or `Hc`. |
| `string FullPathToRootHappFolder { get; set; }` | property | The full path to the root of the hApp that HoloNET will start the Holochain Conductor (currenty uses hc.exe) with and then make zome calls to. |
| `string HolochainConductorAdminURI { get; set; }` | property | The default URI for the Admin port on the Holochain Conductor. Defaults to ws://localhost:7777. |
| `string HolochainConductorAppAgentURI { get; set; }` | property | The default URI for the AppAgent port on the Holochain Conductor. Defaults to ws://localhost:8888. |
| `string HolochainConductorConfigPath { get; set; }` | property | The Holochain Conductor config path. This defaults to AppData\Roaming\holochain\holochain\config\conductor-config.yml (this is where the conductor will as default install the config file). |
| `HolochainConductorModeEnum HolochainConductorMode { get; set; }` | property | Tells HoloNET how to auto-start the Holochain Conductor. See the documentation at https://github.com/holochain-open-dev/holochain-client-csharp for more info. |
| `HolochainConductorEnum HolochainConductorToUse { get; set; }` | property | This is the Holochain Conductor to use for the auto-start Holochain Conductor feature. It can be either `HolochainProductionConductor` or `HcDevTool`. |
| `int IndentLogMessagesBy { get; set; }` | property | The amount of space to indent the log message by. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `ConsoleColor InfoColour { get; set; }` | property | The colour to use for `Info` log entries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `bool InsertExtraNewLineAfterLogMessage { get; set; }` | property | Set this to true to add additional space after the end of each log entry. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `string InstalledAppId { get; set; }` | property | The Installed AppId that the AppAgent WebSocket will connect to (optional). If this is blank then it will be a generic App WebSocket and can connect to any hApp. |
| `KeystoreConfig KeystoreConfig { get; set; }` | property | Keystore connection configuration, mirroring holochain_conductor_api's KeystoreConfig (DangerTestKeystore / LairServer / LairServerInProc). See KeystoreConfig.cs. Not yet wired through to the actual conductor config file/admin API generation - present in the object graph only; wiring it would require generating/writing the conductor's YAML config or a future admin API call, which is out of scope here. |
| `Kitsune2Config Kitsune2Config { get; set; }` | property | Kitsune2 networking sub-fields of ConductorConfig.network not already covered by NetworkConfig above (bootstrap/relay URLs, target_arc_factor, advanced kitsune2 JSON; signal_url and webrtc_config removed in Holochain 0.7.0). See Kitsune2Config.cs. Not yet wired through to any websocket/conductor call - present in the object graph only. |
| `string LogFileName { get; set; }` | property | The log file name (default is HoloNET.log). NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `string LogPath { get; set; }` | property | The logging path (will defualt to AppData\Roaming\NextGenSoftware\HoloNET\Logs). NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `bool LogToConsole { get; set; }` | property | Set this to true (default) if you wish HoloNET to log to the console. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `bool LogToFile { get; set; }` | property | Set this to true (default) if you wish HoloNET to log a log file. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `int MaxLogFileSize { get; set; }` | property | This is the max file size the log file can be (in bytes) before it creates a new file. The default is 1000000 bytes (1 MB). |
| `NetworkConfig NetworkConfig { get; set; }` | property | Network configuration mirroring Holochain 0.7.0's ConductorConfig.network (holochain_conductor_api::config::conductor::NetworkConfig). As of 0.7.0, request_timeout_s lives here rather than directly on ConductorConfig. See: https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs |
| `int NumberOfRetriesToLogToFile { get; set; }` | property | The number of attempts to attempt to log to the file if the first attempt fails. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `bool OnlyAllowOneHolochainConductorToRunAtATime { get; set; }` | property | Set this to true if you wish HoloNET to allow only ONE Holochain Conductor to run at a time. The default is false. |
| `QUICConfig QUICConfig { get; set; }` | property | QUIC transport configuration. NOT VERIFIED - no client-facing QUIC tuning surface was found in holochain_conductor_api as of 0.7.0; see QUICConfig.cs for details. Kept as a placeholder only; has no effect on conductor behaviour. |
| `int RetryLoggingToFileEverySeconds { get; set; }` | property | The amount of time to wait in seconds between each attempt to log to the file. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `int SecondsToWaitForHolochainConductorToStart { get; set; }` | property | The seconds to wait for the Holochain Conductor to start before attempting to connect to it. |
| `bool ShowColouredLogs { get; set; }` | property | Set this to true to enable coloured logs in the console. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `bool ShowHolochainConductorWindow { get; set; }` | property | Set this to true if you wish HoloNET to show the Holochain Conductor window whilst it is starting it (will be left open until the conductor is automatically shutdown again when HoloNET disconects if `AutoShutdownHolochainConductor` is true.) |
| `bool ShutDownALLHolochainConductors { get; set; }` | property | Set this to true if you wish HoloNET to auto-shutdown ALL Holochain Conductors after it disconnects. Default is false. Set this to true if you wish to make sure there are none left running to prevent memory leaks. You can also of course manually call the ShutDownAllConductors if you wish. |
| `ConsoleColor WarningColour { get; set; }` | property | The colour to use for `Warning` log entries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `WASMConfig WASMConfig { get; set; }` | property | WASM-related configuration. NOT VERIFIED - no client-facing wasm compilation/runtime tuning surface was found in holochain_conductor_api as of 0.7.0; see WASMConfig.cs for details. Kept as a placeholder only; has no effect on conductor behaviour. |

<a id="nextgensoftware-holochain-holonet-client-holonetdnamanager"></a>
### HoloNETDNAManager (static class)

| Member | Kind | Description |
|---|---|---|
| `IHoloNETDNA HoloNETDNA { get; set; } (static)` | property | The HoloNET settings in use (see HoloNETDNA). |
| `bool IsLoaded { get; } (static)` | property | True once the settings have been loaded. |
| `static string HoloNETDNAPath` | field | Where HoloNETDNA.json is read from and saved to. |
| `static IHoloNETDNA LoadDNA()` | method | Load the settings from HoloNETDNA.json (defaults if it doesn't exist). |
| `static IHoloNETDNA LoadDNA(string holoNETDNAPath)` | method | Load the settings from HoloNETDNA.json (defaults if it doesn't exist). |
| `static bool SaveDNA()` | method | Save the settings to HoloNETDNA.json. |
| `static bool SaveDNA(string holoNETDNAPath, IHoloNETDNA holoNETDNA)` | method | Save the settings to HoloNETDNA.json. |

<a id="nextgensoftware-holochain-holonet-client-holoneterroreventargs"></a>
### HoloNETErrorEventArgs (class)

Inherits / implements: `EventArgs`

| Member | Kind | Description |
|---|---|---|
| `Uri EndPoint { get; set; }` | property | The conductor WebSocket endpoint this came from. |
| `Exception ErrorDetails { get; set; }` | property | A more detailed description of the error, this normally includes a stacktrace to help you track down the cause. |
| `string Reason { get; set; }` | property | What the conductor returned for this request (reason). |

<a id="nextgensoftware-holochain-holonet-client-holonetexception"></a>
### HoloNETException (class)

Inherits / implements: `Exception`, `ISerializable`

| Member | Kind | Description |
|---|---|---|
| `HoloNETException(string message)` | ctor | Create the exception with this message (and the inner exception / conductor endpoint, if given). |
| `HoloNETException(string message, Exception innerException)` | ctor | Create the exception with this message (and the inner exception / conductor endpoint, if given). |
| `HoloNETException(string message, Exception innerException, Uri endPoint)` | ctor | Create the exception with this message (and the inner exception / conductor endpoint, if given). |
| `Uri EndPoint { get; set; }` | property | The conductor WebSocket endpoint this came from. |

<a id="nextgensoftware-holochain-holonet-client-holonetrequest"></a>
### HoloNETRequest (class)

Inherits / implements: `IHoloNETRequest`

| Member | Kind | Description |
|---|---|---|
| `byte[] data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `ulong id { get; set; }` | property | Holochain's `id` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetrequesttype"></a>
### HoloNETRequestType (enum)

| Value | Description |
|---|---|
| `ZomeCall` |  |
| `Signal` |  |
| `AppInfo` |  |
| `AdminGenerateAgentPubKey` |  |
| `AdminInstallApp` |  |
| `AdminUninstallApp` |  |
| `AdminEnableApp` |  |
| `AdminDisableApp` |  |
| `AdminGrantZomeCallCapability` |  |
| `AdminAttachAppInterface` |  |
| `AdminListApps` |  |
| `AdminListDnas` |  |
| `AdminListAppInterfaces` |  |
| `AdminListCellIds` |  |
| `AdminRegisterDna` |  |
| `AdminUpdateCoordinators` |  |
| `AdminDumpFullState` |  |
| `AdminDumpState` |  |
| `AdminGetDnaDefinition` |  |
| `AdminAgentInfo` |  |
| `AdminAddAgentInfo` |  |
| `AdminDeleteClonedCell` |  |
| `AdminStorageInfo` |  |
| `AdminDumpNetworkStats` |  |
| `AdminDumpNetworkMetrics` |  |
| `AdminGraftRecords` |  |
| `AdminAddAdminInterfaces` |  |
| `AdminRevokeZomeCallCapability` |  |
| `AdminListCapabilityGrants` |  |
| `AdminPeerMetaInfo` |  |
| `AdminIssueAppAuthenticationToken` |  |
| `AdminRevokeAppAuthenticationToken` |  |
| `AdminGetCompatibleCells` |  |
| `AppCreateCloneCell` |  |
| `AppEnableCloneCell` |  |
| `AppDisableCloneCell` |  |
| `AppGetCountersigningSessionState` |  |
| `AppAbandonCountersigningSession` |  |
| `AppPublishCountersigningSession` |  |
| `AppListWasmHostFunctions` |  |
| `AppSendDirectSignal` |  |
| `AppGrantDirectSignalCapability` |  |
| `AdminGrantDirectSignalCapability` |  |
| `AppProvideMemproofs` |  |
| `AppPeerMetaInfo` |  |
| `AdminDumpOpTimings` |  |
| `AppDumpOpTimings` |  |
| `AdminDumpConductorState` |  |
| `AppAgentInfo` |  |
| `AppDumpNetworkMetrics` |  |
| `AppDumpNetworkStats` |  |
| `AppEnableApp` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetresponse"></a>
### HoloNETResponse (class)

Inherits / implements: `IHoloNETResponse`

| Member | Kind | Description |
|---|---|---|
| `byte[] data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `HoloNETResponseType HoloNETResponseType { get; set; }` | property | Which kind of message this is (zome call response, signal, admin response...). |
| `ulong id { get; set; }` | property | Holochain's `id` field (sent to / received from the conductor as-is). |
| `bool IsError { get; set; }` | property | True when the call or the conductor reported an error (see Message). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetresponsetype"></a>
### HoloNETResponseType (enum)

| Value | Description |
|---|---|
| `ZomeResponse` |  |
| `Signal` |  |
| `AppInfo` |  |
| `AdminAgentPubKeyGenerated` |  |
| `AdminAppInstalled` |  |
| `AdminAppUninstalled` |  |
| `AdminAppEnabled` |  |
| `AdminAppDisabled` |  |
| `AdminZomeCallCapabilityGranted` |  |
| `AdminAppInterfaceAttached` |  |
| `AdminDnaRegistered` |  |
| `AdminDnaDefinitionReturned` |  |
| `AdminAppInterfacesListed` |  |
| `AdminAppsListed` |  |
| `AdminDnasListed` |  |
| `AdminCellIdsListed` |  |
| `AdminAgentInfoReturned` |  |
| `AdminAgentInfoAdded` |  |
| `AdminCoordinatorsUpdated` |  |
| `AdminCloneCellDeleted` |  |
| `AdminStateDumped` |  |
| `AdminFullStateDumped` |  |
| `AdminNetworkMetricsDumped` |  |
| `AdminNetworkStatsDumped` |  |
| `AdminStorageInfoReturned` |  |
| `AdminRecordsGrafted` |  |
| `AdminAdminInterfacesAdded` |  |
| `AdminZomeCallCapabilityRevoked` |  |
| `AdminCapabilityGrantsInfoReturned` |  |
| `AdminPeerMetaInfoReturned` |  |
| `AdminAppAuthenticationTokenIssued` |  |
| `AdminAppAuthenticationTokenRevoked` |  |
| `AdminCompatibleCellsReturned` |  |
| `AppCloneCellCreated` |  |
| `AppCloneCellEnabled` |  |
| `AppCloneCellDisabled` |  |
| `AppCountersigningSessionStateReturned` |  |
| `AppCountersigningSessionAbandoned` |  |
| `AppPublishCountersigningSessionTriggered` |  |
| `AppWasmHostFunctionsListed` |  |
| `AppDirectSignalSent` |  |
| `AppDirectSignalCapabilityGranted` |  |
| `AdminDirectSignalCapabilityGranted` |  |
| `AppMemproofsProvided` |  |
| `AppPeerMetaInfoReturned` |  |
| `AdminOpTimingsDumped` |  |
| `AppOpTimingsDumped` |  |
| `Error` |  |
| `AdminConductorStateDumped` |  |
| `AppAgentInfoReturned` |  |
| `AppNetworkMetricsDumped` |  |
| `AppNetworkStatsDumped` |  |
| `AppEnabledViaAppInterface` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientbase-holonetshutdowncomplete"></a>
### HoloNETClientBase.HoloNETShutdownComplete (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void HoloNETShutdownComplete(object sender, HoloNETShutdownEventArgs e)` (the type of the `OnHoloNETShutdownComplete` event).

<a id="nextgensoftware-holochain-holonet-client-holonetshutdowneventargs"></a>
### HoloNETShutdownEventArgs (class)

Inherits / implements: `CallBackBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `HoloNETShutdownEventArgs(Uri endPoint, string dnaHash, string agentPubKey, HolochainConductorsShutdownEventArgs holochainConductorsShutdownEventArgs)` | ctor | Create the event args with these values (HoloNET raises them; you rarely construct one). |
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |
| `Uri EndPoint { get; set; }` | property | The conductor WebSocket endpoint this came from. |
| `HolochainConductorsShutdownEventArgs HolochainConductorsShutdownEventArgs { get; }` | property | The result of shutting down the conductors HoloNET started. |

<a id="nextgensoftware-holochain-holonet-client-installedcell"></a>
### InstalledCell (class)

| Member | Kind | Description |
|---|---|---|
| `byte[][] cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `string role_id { get; set; }` | property | Holochain's `role_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-installenablesignandattachhappcallback"></a>
### HoloNETClientAdmin.InstallEnableSignAndAttachHappCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void InstallEnableSignAndAttachHappCallBack(object sender, InstallEnableSignAndAttachHappEventArgs e)` (the type of the `OnInstallEnableSignAndAttachHappCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-installenablesignandattachhappeventargs"></a>
### InstallEnableSignAndAttachHappEventArgs (class)

The outcome of InstallEnableSignAndAttachHappAsync: every step's result, and the installed app's agent, DNA, cell and app interface port.

Inherits / implements: `CallBackBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; set; }` | property | The app's agent public key. |
| `AgentPubKeyGeneratedCallBackEventArgs AgentPubKeyGeneratedResult { get; set; }` | property | The agent key step's raw result. |
| `AppEnabledCallBackEventArgs AppEnabledResult { get; set; }` | property | The enable step's raw result. |
| `AppInstalledCallBackEventArgs AppInstalledResult { get; set; }` | property | The install step's raw result. |
| `AppInterfaceAttachedCallBackEventArgs AppInterfaceAttachedResult { get; set; }` | property | The attach-interface step's raw result. |
| `IAppManifest AppManifest { get; set; }` | property | The app's manifest (roles, DNAs). |
| `AppInfoStatusEnum AppStatus { get; set; }` | property | The app's status as the conductor reported it. |
| `string AppStatusReason { get; set; }` | property | Why the app is disabled or paused, if it is. |
| `ushort? AttachedOnPort { get; set; }` | property | The app interface port to connect app clients to. |
| `byte[][] CellId { get; set; }` | property | The app's cell id (DNA hash, agent public key). |
| `CellInfoType CellType { get; set; }` | property | The kind of cell (provisioned, cloned, stem). |
| `string DnaHash { get; set; }` | property | The DNA hash of the app's cell. |
| `bool IsAgentPubKeyGenerated { get; set; }` | property | True when a new agent key was generated (false when an installed app and its agent were kept). |
| `bool IsAppAttached { get; set; }` | property | True when an app interface was attached (see AttachedOnPort). |
| `bool IsAppEnabled { get; set; }` | property | True when the app is enabled. |
| `bool IsAppInstalled { get; set; }` | property | True when the app is installed (newly, or it already was). |
| `bool IsAppSigned { get; set; }` | property | True when signing credentials and the zome-call capability grant were set up. |
| `bool IsSuccess { get; set; }` | property | True when every step worked (the hApp is installed, enabled, signed and attached). |
| `ZomeCallCapabilityGrantedCallBackEventArgs ZomeCallCapabilityGrantedResult { get; set; }` | property | The signing / capability grant step's raw result. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-installenablesignattachandconnecttohappcallback"></a>
### HoloNETClientAdmin.InstallEnableSignAttachAndConnectToHappCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void InstallEnableSignAttachAndConnectToHappCallBack(object sender, InstallEnableSignAttachAndConnectToHappEventArgs e)` (the type of the `OnInstallEnableSignAttachAndConnectToHappCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-installenablesignattachandconnecttohappeventargs"></a>
### InstallEnableSignAttachAndConnectToHappEventArgs (class)

Inherits / implements: `InstallEnableSignAndAttachHappEventArgs`

| Member | Kind | Description |
|---|---|---|
| `IHoloNETClientAppAgent HoloNETClientAppAgent { get; set; }` | property | What the conductor returned for this request (holo netclient app agent). |
| `HoloNETConnectedEventArgs HoloNETConnectedResult { get; set; }` | property | What the conductor returned for this request (holo netconnected result). |
| `bool IsAppConnected { get; set; }` | property | What the conductor returned for this request (is app connected). |

<a id="nextgensoftware-holochain-holonet-client-izomecallsigner"></a>
### IZomeCallSigner (interface)

Signs zome calls on HoloNET's behalf. Set `HoloNETClientAppBase.ZomeCallSigner` when the conductor holds the agent key, e.g. the shared Holochain Android runtime's `signZomeCall` IPC, where a third-party app has no admin access to grant itself a capability. When unset, HoloNET signs locally with the credentials from AuthorizeSigningCredentials.

| Member | Kind | Description |
|---|---|---|
| `Task<ZomeCallParamsSigned> SignZomeCallAsync(ZomeCallToSign call)` | method | Sign a zome call (implement this to sign with an external key store, e.g. a mobile runtime). |

<a id="nextgensoftware-holochain-holonet-client-keystoreconfig"></a>
### KeystoreConfig (class)

Mirrors holochain_conductor_api::config::conductor::keystore_config::KeystoreConfig from Holochain 0.7.0. This defines how the conductor connects to a keystore (lair-keystore), NOT a client-configurable KDF algorithm/keystore-type as a previous, unverified pass on this file invented (KDFAlgorithm = "PBKDF2", Type = KeystoreType.FileSystem, HSM support, key rotation/backup/recovery settings, etc. - none of that exists in the real Rust struct). The real type is a serde tagged enum with exactly three variants: DangerTestKeystore, LairServer { connection_url }, LairServerInProc { lair_root }. There is no KDF, password, key rotation, backup, recovery, or HSM configuration exposed here at all - lair-keystore manages its own passphrase handling via the conductor CLI entrypoint, not via this config struct. Verified against: https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor/keystore_config.rs

| Member | Kind | Description |
|---|---|---|
| `string ConnectionUrl { get; set; }` | property | Only used when Type == LairServer. The "connectionUrl" as defined in the target lair-keystore-config.yaml (also obtainable by running `lair-keystore url`). Mirrors LairServer.connection_url: url2::Url2. |
| `string LairRoot { get; set; }` | property | Only used when Type == LairServerInProc. The "lair_root" path, i.e. the directory containing the lair-keystore-config.yaml file. If null/empty, the conductor defaults to [environment_path]/ks. Mirrors LairServerInProc.lair_root: Option<KeystorePath>. |
| `KeystoreConfigType Type { get; set; }` | property | Which keystore connection mode to use. Mirrors the Rust enum's serde "type" tag (rename_all = "snake_case"): danger_test_keystore, lair_server, lair_server_in_proc. Default mirrors Rust's Default impl: LairServerInProc. |

<a id="nextgensoftware-holochain-holonet-client-keystoreconfigtype"></a>
### KeystoreConfigType (enum)

Mirrors the variants of holochain_conductor_api's KeystoreConfig enum (Holochain 0.7.0).

| Value | Description |
|---|---|
| `DangerTestKeystore` | Uses a test keystore instead of lair, generating publicly accessible private keys. DO NOT USE IN PRODUCTION - mirrors Rust's DangerTestKeystore variant. |
| `LairServer` | Connect to an external lair-keystore process via ConnectionUrl. |
| `LairServerInProc` | Run a lair-keystore server in-process, optionally rooted at LairRoot. |

<a id="nextgensoftware-holochain-holonet-client-kitsune2config"></a>
### Kitsune2Config (class)

Mirrors the Kitsune2-related fields of holochain_conductor_api::config::conductor::NetworkConfig from Holochain 0.7.0. In 0.7.0 the tx5/WebRTC transport was removed entirely; iroh (QUIC) is now the sole network backend. Consequently signal_url and webrtc_config fields were dropped from NetworkConfig, and relay_url is now the primary non-bootstrap server URL. Verified against: https://github.com/holochain/holochain/blob/main-0.7/crates/holochain_conductor_api/src/config/conductor.rs

| Member | Kind | Description |
|---|---|---|
| `string AdvancedJson { get; set; }` | property | Advanced escape hatch for directly configuring kitsune2 (raw JSON), bypassing the higher-level fields above. Mirrors NetworkConfig.advanced: Option<serde_json::Value>. The Rust docs explicitly warn: "use only if you know what you are doing". Not currently wired through to any websocket/conductor call by HoloNET. |
| `string BootstrapUrl { get; set; }` | property | The Kitsune2 bootstrap server to use for WAN discovery. Mirrors NetworkConfig.bootstrap_url: url2::Url2. |
| `string RelayUrl { get; set; }` | property | The iroh relay server address. In Holochain 0.7.0 this replaced signal_url/WebRTC; the iroh bootstrap server now bundles a relay, so a separate relay binary is no longer needed. Mirrors NetworkConfig.relay_url: url2::Url2. |
| `uint TargetArcFactor { get; set; }` | property | The target arc factor applied when receiving hints from kitsune2. Leave at 1 in normal operation; set to 0 for leecher nodes that do not contribute to gossip. Mirrors NetworkConfig.target_arc_factor: u32 (default 1). |

<a id="nextgensoftware-holochain-holonet-client-listappsresponse"></a>
### ListAppsResponse (class)

| Member | Kind | Description |
|---|---|---|
| `AppInfo[] Apps { get; set; }` | property | The installed apps. |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-memproofsprovidedcallback"></a>
### HoloNETClientAppBase.MemproofsProvidedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void MemproofsProvidedCallBack(object sender, MemproofsProvidedCallBackEventArgs e)` (the type of the `OnMemproofsProvidedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-memproofsprovidedcallbackeventargs"></a>
### MemproofsProvidedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-networkconfig"></a>
### NetworkConfig (class)

Mirrors (the relevant part of) holochain_conductor_api::config::conductor::NetworkConfig from Holochain 0.7.0. In 0.5.x and earlier, request_timeout_s lived directly on ConductorConfig; in 0.7.0 it moved under ConductorConfig.network (NetworkConfig). https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs HoloNET uses this client-side to drive the default request/response timeout used for zome calls (see CallZomeOptions.TimeoutSeconds for a per-call override) and other websocket round-trips. It does not attempt to mirror every NetworkConfig field, only the one relevant to client request timeout behaviour.

| Member | Kind | Description |
|---|---|---|
| `int RequestTimeoutS { get; set; }` | property | The amount of time, in seconds, to elapse before a request-response round trip times out. Mirrors Rust's `request_timeout_s: u64` (default 60 seconds in Holochain). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-networkmetricsdumpedcallback"></a>
### HoloNETClientAdmin.NetworkMetricsDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void NetworkMetricsDumpedCallBack(object sender, NetworkMetricsDumpedCallBackEventArgs e)` (the type of the `OnNetworkMetricsDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-networkmetricsdumpedcallbackeventargs"></a>
### NetworkMetricsDumpedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string NetworkMetricsDumpJSON { get; set; }` | property | What the conductor returned for this request (network metrics dump json). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-networkstatsdumpedcallback"></a>
### HoloNETClientAdmin.NetworkStatsDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void NetworkStatsDumpedCallBack(object sender, NetworkStatsDumpedCallBackEventArgs e)` (the type of the `OnNetworkStatsDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-networkstatsdumpedcallbackeventargs"></a>
### NetworkStatsDumpedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `DumpNetworkStatsResponse NetworkStats { get; set; }` | property | Typed representation of `NetworkStatsDumpJSON`, parsed into the kitsune2_api/0.7.0). Will be null if the JSON could not be parsed into this shape (see RawJSONData/NetworkStatsDumpJSON for the raw data in that case). |
| `string NetworkStatsDumpJSON { get; set; }` | property | Raw JSON escape hatch, as returned directly by the Holochain conductor's `dump_network_stats` admin call (AdminResponse::NetworkStatsDumped(String)). Kept for backwards compatibility / in case the typed NetworkStats below fails to parse a future/older conductor's exact JSON shape. |

<a id="nextgensoftware-holochain-holonet-client-newentryaction"></a>
### NewEntryAction (struct)

<a id="nextgensoftware-holochain-holonet-client-optimingdump"></a>
### OpTimingDump (class)

Mirrors holochain_conductor_api::state_dump::OpTimingDump. Lifecycle timings for one DHT op in the conductor's current arc for a DNA.

| Member | Kind | Description |
|---|---|---|
| `long? abandoned_at { get; set; }` | property | Microsecond timestamp when validation was abandoned, or null if not abandoned. |
| `bool? locally_validated { get; set; }` | property | Whether this op was validated locally (true), fetched from the network (false), or validation is pending (null). |
| `byte[] op_hash { get; set; }` | property | The hash of this DHT op. |
| `string validation_status { get; set; }` | property | The validation outcome, or null if validation has not completed. |
| `long? when_integrated { get; set; }` | property | Microsecond timestamp when the op was integrated, or null if not yet integrated. |
| `long when_received { get; set; }` | property | Microsecond timestamp when the op was first received. |

<a id="nextgensoftware-holochain-holonet-client-optimingscursor"></a>
### OpTimingsCursor (class)

Mirrors holochain_conductor_api::state_dump::OpTimingsCursor. Exclusive pagination marker for OpTimingsDump using microsecond timestamps and op hashes to break ties.

| Member | Kind | Description |
|---|---|---|
| `byte[] hash { get; set; }` | property | Hash of the last op in the previous page. |
| `long when_received { get; set; }` | property | Microsecond timestamp of the last op in the previous page. |

<a id="nextgensoftware-holochain-holonet-client-optimingsdump"></a>
### OpTimingsDump (class)

Mirrors holochain_conductor_api::state_dump::OpTimingsDump. One page of op timings for a DNA, ordered by (when_received, op_hash). New in Holochain 0.7.0.

| Member | Kind | Description |
|---|---|---|
| `OpTimingsCursor cursor { get; set; }` | property | Cursor to pass to the next DumpOpTimings call to get the following page. Null when there are no more ops. |
| `List<OpTimingDump> timings { get; set; }` | property | The op timing records in this page. |

<a id="nextgensoftware-holochain-holonet-client-p2pagentsdump"></a>
### P2pAgentsDump (class)

| Member | Kind | Description |
|---|---|---|
| `AgentInfoDump[] peers { get; set; }` | property | Holochain's `peers` field (sent to / received from the conductor as-is). |
| `object[] this_agent { get; set; }` | property | [AgentPubKey bytes, kitsune agent id (base64 string)], or null. |
| `AgentInfoDump this_agent_info { get; set; }` | property | Holochain's `this_agent_info` field (sent to / received from the conductor as-is). |
| `object[] this_dna { get; set; }` | property | [DnaHash bytes, kitsune space id (base64 string)], or null. |

<a id="nextgensoftware-holochain-holonet-client-pausedappreasontype"></a>
### PausedAppReasonType (enum)

| Value | Description |
|---|---|
| `Error` |  |

<a id="nextgensoftware-holochain-holonet-client-peermetainforesponse"></a>
### PeerMetaInfoResponse (class)

Mirrors holochain_conductor_api::admin_interface::AdminResponse::PeerMetaInfo (also used for the equivalent AppResponse::PeerMetaInfo) in Holochain 0.7.0: PeerMetaInfo(BTreeMap<DnaHash, BTreeMap<String, PeerMetaInfo>>) https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs NOTE: The inner `kitsune2_api::PeerMetaInfo` struct's exact field shape could not be located/verified against an authoritative source at the time of writing, so each entry is represented as `dynamic` here rather than guessing its fields. Inspect RawJSONData / the raw msgpack payload if you need its exact contents until this can be confirmed.

| Member | Kind | Description |
|---|---|---|
| `Dictionary<string, Dictionary<string, object>> dna_hash_peer_meta { get; set; }` | property | Outer key = hex/base64-ish string key for the DnaHash, inner dictionary keyed by peer Url string, value = opaque per-peer meta info (see NOTE above). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-peermetainforeturnedcallback"></a>
### HoloNETClientAdmin.PeerMetaInfoReturnedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void PeerMetaInfoReturnedCallBack(object sender, PeerMetaInfoReturnedCallBackEventArgs e)` (the type of the `OnPeerMetaInfoReturnedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-peermetainforeturnedcallbackeventargs"></a>
### PeerMetaInfoReturnedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `PeerMetaInfoResponse PeerMetaInfo { get; set; }` | property | What the conductor returned for this request (peer meta info). |

<a id="nextgensoftware-holochain-holonet-client-providememproofsrequest"></a>
### ProvideMemproofsRequest (class)

Mirrors the payload of AppRequest::ProvideMemproofs(MemproofMap) (Holochain 0.7.0): https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs NOTE: `MemproofMap`'s precise Rust definition could not be conclusively located/verified at the time of writing. Based on its usage as "a membrane proof per role name" elsewhere in the Holochain app installation flow (see InstallAppRequest's `membrane_proofs: HashMap <RoleName, MembraneProof>` for the analogous shape used at install time), it is modelled here as a plain role-name-keyed dictionary of raw membrane proof bytes (MessagePack serializes Dictionary<string, byte[]> as a map natively, matching the Rust map shape). If the real MemproofMap turns out to differ, only this type alias needs updating.

| Member | Kind | Description |
|---|---|---|
| `Dictionary<string, byte[]> MembraneProofs { get; set; }` | property | The membrane proof for each role (role name -> proof bytes). |

<a id="nextgensoftware-holochain-holonet-client-provisioningstrategytype"></a>
### ProvisioningStrategyType (enum)

| Value | Description |
|---|---|
| `Create` |  |
| `CloneOnly` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-publishcountersigningsessiontriggeredcallback"></a>
### HoloNETClientAppBase.PublishCountersigningSessionTriggeredCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void PublishCountersigningSessionTriggeredCallBack(object sender, PublishCountersigningSessionTriggeredCallBackEventArgs e)` (the type of the `OnPublishCountersigningSessionTriggeredCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-publishcountersigningsessiontriggeredcallbackeventargs"></a>
### PublishCountersigningSessionTriggeredCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-quicconfig"></a>
### QUICConfig (class)

NOT VERIFIED / NOT WIRED against any real Holochain 0.7.0 conductor-facing config. Researched holochain_conductor_api::config::conductor::NetworkConfig at https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs and there is NO client-configurable QUIC transport tuning surface (no version, max concurrent streams, congestion window, TLS settings, etc.). Low-level transport details (kitsune2/iroh) are either fixed internally or exposed only as the single opaque `advanced` JSON blob on NetworkConfig (see Kitsune2Config.AdvancedJson in this same directory; webrtc_config and signal_url were removed in Holochain 0.7.0) - not as a structured, per-field QUIC config object like this class previously fabricated (Version = "1", MaxConcurrentStreams, MaxStreamData, congestion window tuning, a nested TLSConfig with certificate file paths, etc.). None of those fields have a real Rust counterpart in the conductor-facing API. This class is kept as a placeholder only so existing references / the HoloNETDNA object graph keep compiling. Its fields below are illustrative/unverified placeholders, not real Holochain config, and are not sent anywhere. Do not rely on them changing conductor behaviour. If real QUIC-level tuning is ever exposed by Holochain, replace this file's contents entirely based on the verified Rust struct at that time.

| Member | Kind | Description |
|---|---|---|
| `bool Enabled { get; set; }` | property | Placeholder only - not backed by any real Holochain conductor config field. Kept false by default to make clear this has no effect. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-readyforzomecalls"></a>
### HoloNETClientAppBase.ReadyForZomeCalls (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void ReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)` (the type of the `OnReadyForZomeCalls` event).

<a id="nextgensoftware-holochain-holonet-client-readyforzomecallseventargs"></a>
### ReadyForZomeCallsEventArgs (class)

Inherits / implements: `CallBackBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `ReadyForZomeCallsEventArgs(Uri endPoint, string dnaHash, string agentPubKey)` | ctor | Create the event args with these values (HoloNET raises them; you rarely construct one). |
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |
| `Uri EndPoint { get; set; }` | property | The conductor WebSocket endpoint this came from. |

<a id="nextgensoftware-holochain-holonet-client-record"></a>
### Record (class)

Inherits / implements: `IRecord`

| Member | Kind | Description |
|---|---|---|
| `string ActionHash { get; set; }` | property | The ActionHash |
| `int ActionSequence { get; set; }` | property | The Action Sequence for this entry. |
| `string Author { get; set; }` | property | The author of the entry (AgentPubKey on their device). |
| `byte[] Bytes { get; set; }` | property | The raw bytes returned from the Holochain Conductor for the entry. |
| `string BytesString { get; set; }` | property | The raw bytes returned from the Holochain Conductor for the entry formatted as a string making logging etc easier. |
| `DateTime DateTime { get; set; }` | property | Standard .NET DateTime converted from the Unix timestamp (returned from the Holochain Conductor). |
| `object EntryDataObject { get; set; }` | property | A dynamic object constructed from the entry data. |
| `string EntryHash { get; set; }` | property | Hash of the entry |
| `Dictionary<string, object> EntryKeyValuePairs { get; set; }` | property | A key/value pair/dictionary containing the entry data itself. |
| `string EntryType { get; set; }` | property | //App/Not sure what other types there are? |
| `string OriginalActionAddress { get; set; }` | property | Is the original Hash for the entry. |
| `string OriginalEntryAddress { get; set; }` | property | Is the original EntryHash for the entry. |
| `string PreviousActionHash { get; set; }` | property | The previous ActionHash. |
| `string Signature { get; set; }` | property | The signature of the entry. |
| `long Timestamp { get; set; }` | property | The Unix timestamp (returned from the Holochain Conductor) |
| `string Type { get; set; }` | property | Create/Update/Delete |

<a id="nextgensoftware-holochain-holonet-client-recordentry"></a>
### RecordEntry (enum)

| Value | Description |
|---|---|
| `Present` |  |
| `Hidden` |  |
| `NA` |  |
| `NotStored` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-recordsgraftedcallback"></a>
### HoloNETClientAdmin.RecordsGraftedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void RecordsGraftedCallBack(object sender, RecordsGraftedCallBackEventArgs e)` (the type of the `OnRecordsGraftedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-recordsgraftedcallbackeventargs"></a>
### RecordsGraftedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-retrieveagentpubkeyanddnahashmode"></a>
### RetrieveAgentPubKeyAndDnaHashMode (enum)

| Value | Description |
|---|---|
| `Wait` |  |
| `UseCallBackEvents` |  |
| `DoNotRetreive` |  |

<a id="nextgensoftware-holochain-holonet-client-shutdownholochainconductorsmode"></a>
### ShutdownHolochainConductorsMode (enum)

| Value | Description |
|---|---|
| `ShutdownCurrentConductorOnly` |  |
| `ShutdownAllConductors` |  |
| `DoNotShutdownAnyConductors` |  |
| `UseHoloNETDNASettings` |  |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-signalcallback"></a>
### HoloNETClientAppBase.SignalCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void SignalCallBack(object sender, SignalCallBackEventArgs e)` (the type of the `OnSignalCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-signalcallbackeventargs"></a>
### SignalCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |
| `string FromAgentPubKey { get; set; }` | property | Holochain 0.7.1+: the agent who sent a direct signal (null for zome signals and on 0.7.0). |
| `bool IsDirectSignal { get; set; }` | property | True for a direct signal from another agent (send_direct_signal), false for one emitted by a zome. |
| `ISignalData RawSignalData { get; set; }` | property | The raw signal: its MessagePack bytes are in Data. |
| `Dictionary<string, object> SignalData { get; set; }` | property | The decoded signal payload as key/value pairs. |
| `string SignalDataAsString { get; set; }` | property | The decoded signal payload as text (for logging). |
| `SignalType SignalType { get; set; }` | property | Whether this is an app signal (emitted by a zome) or a system signal. |
| `string SystemSignalType { get; set; }` | property | For system signals, the variant (e.g. "successful_countersigning", "restore_complete"). |

<a id="nextgensoftware-holochain-holonet-client-signaldata"></a>
### SignalData (class)

Inherits / implements: `ISignalData`

| Member | Kind | Description |
|---|---|---|
| `byte[][] CellData { get; set; }` | property | The cell id the signal came from ([DNA hash, agent public key]). |
| `byte[] Data { get; set; }` | property | The signal payload as MessagePack bytes (what the zome passed to emit_signal). |

<a id="nextgensoftware-holochain-holonet-client-signalresponse"></a>
### SignalResponse (class)

| Member | Kind | Description |
|---|---|---|
| `SignalData App { get; set; }` | property | The app signal. |

<a id="nextgensoftware-holochain-holonet-client-signaltype"></a>
### SignalType (enum)

| Value | Description |
|---|---|
| `App` |  |
| `System` |  |

<a id="nextgensoftware-holochain-holonet-client-signalvalue"></a>
### SignalValue (class)

The "value" of holochain_types::signal::Signal::App / AppDirect (Holochain 0.7.0). Signal is #[serde(tag = "type", content = "value", rename_all = "snake_case")]: {"type": "app", "value": {"cell_id": [dna, agent], "zome_name": "...", "signal": bytes}} {"type": "app_direct", "value": {"cell_id": [dna, agent], "signal": bytes}} {"type": "system", "value": {...}} "signal" holds the zome's own msgpack-encoded payload.

| Member | Kind | Description |
|---|---|---|
| `byte[][] cell_id { get; set; }` | property | CellId tuple: [0] = DnaHash, [1] = AgentPubKey. |
| `byte[] from_agent { get; set; }` | property | Holochain 0.7.1+ app_direct only: the agent who sent the direct signal. |
| `byte[] signal { get; set; }` | property | Holochain's `signal` field (sent to / received from the conductor as-is). |
| `string zome_name { get; set; }` | property | Holochain's `zome_name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-signature"></a>
### Signature (struct)

| Member | Kind | Description |
|---|---|---|
| `Signature(byte[] value)` | ctor | A signature from its 64 raw bytes. |
| `byte[] Value { get; }` | property | The raw signature bytes. |

<a id="nextgensoftware-holochain-holonet-client-sourcechaincursor"></a>
### SourceChainCursor (static class)

Pagination cursor for dump_state (holochain_state_types::SourceChainCursor, an externally tagged enum): start after a sequence number or after an action.

| Member | Kind | Description |
|---|---|---|
| `static object ActionHash(byte[] actionHash)` | method | Start a source chain dump at this action. |
| `static object Sequence(uint actionSeq)` | method | Start a source chain dump at this action sequence number. |

<a id="nextgensoftware-holochain-holonet-client-sourcechainjsondump"></a>
### SourceChainJsonDump (class)

| Member | Kind | Description |
|---|---|---|
| `int published_ops_count { get; set; }` | property | Holochain's `published_ops_count` field (sent to / received from the conductor as-is). |
| `SourceChainJsonRecord[] records { get; set; }` | property | Holochain's `records` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-sourcechainjsonrecord"></a>
### SourceChainJsonRecord (class)

| Member | Kind | Description |
|---|---|---|
| `object action { get; set; }` | property | The action as decoded msgpack (its "type" says which kind). |
| `byte[] action_address { get; set; }` | property | Holochain's `action_address` field (sent to / received from the conductor as-is). |
| `object entry { get; set; }` | property | The entry (tagged by "entry_type"), or null. |
| `byte[] signature { get; set; }` | property | Holochain's `signature` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-statedumpedcallback"></a>
### HoloNETClientAdmin.StateDumpedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void StateDumpedCallBack(object sender, StateDumpedCallBackEventArgs e)` (the type of the `OnStateDumpedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-statedumpedcallbackeventargs"></a>
### StateDumpedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `string DumpedStateJSON { get; set; }` | property | What the conductor returned for this request (dumped state json). |

<a id="nextgensoftware-holochain-holonet-client-storageinforesponse"></a>
### StorageInfoResponse (class)

Inherits / implements: `IStorageInfoResponse`

| Member | Kind | Description |
|---|---|---|
| `DnaStorageBlob[] blobs { get; set; }` | property | Holochain's `blobs` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-storageinforeturnedcallback"></a>
### HoloNETClientAdmin.StorageInfoReturnedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void StorageInfoReturnedCallBack(object sender, StorageInfoReturnedCallBackEventArgs e)` (the type of the `OnStorageInfoReturnedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-storageinforeturnedcallbackeventargs"></a>
### StorageInfoReturnedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `IStorageInfoResponse StorageInfoResponse { get; set; }` | property | What the conductor returned for this request (storage info response). |

<a id="nextgensoftware-holochain-holonet-client-transportconnectionstats"></a>
### TransportConnectionStats (class)

Mirrors kitsune2_api/0.7.0).

| Member | Kind | Description |
|---|---|---|
| `bool IsDirect { get; set; }` | property | True if this connection has successfully upgraded to a direct peer connection. |
| `ulong OpenedAtS { get; set; }` | property | UNIX epoch timestamp in seconds when this connection was opened. |
| `string PubKey { get; set; }` | property | The public key of the remote peer. |
| `ulong RecvBytes { get; set; }` | property | The bytes received on this connection. |
| `ulong RecvMessageCount { get; set; }` | property | The message count received on this connection. |
| `ulong SendBytes { get; set; }` | property | The bytes sent on this connection. |
| `ulong SendMessageCount { get; set; }` | property | The message count sent on this connection. |

<a id="nextgensoftware-holochain-holonet-client-wasmconfig"></a>
### WASMConfig (class)

NOT VERIFIED / NOT WIRED against any real Holochain 0.7.0 conductor-facing config. Researched holochain_conductor_api's config module (https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs and https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor/paths.rs) and found only a `WASM_DIRECTORY` constant ("wasm") - the name of the subdirectory under the conductor's data_root_path where compiled wasm is cached on disk. There is no conductor-facing struct exposing WASM compilation/optimization/profiling/security/ performance tuning to a remote client. The wasmer/witness compilation cache, JIT/AOT behaviour, sandboxing, etc. are entirely internal to the conductor process and not exposed over the admin/app websocket API that HoloNET talks to. A previous, unverified pass on this file invented an entire fabricated object graph (OptimizationLevel, EnableJIT/EnableAOT, congestion-style tuning, a WASMSecurityConfig with allowed/blocked imports, a WASMProfilingConfig, etc.) with no basis in the real API. That has been removed. This class is kept as a placeholder only so existing references / the HoloNETDNA object graph keep compiling. It is not sent anywhere and has no effect on conductor behaviour.

| Member | Kind | Description |
|---|---|---|
| `bool Enabled { get; set; }` | property | Placeholder only - not backed by any real Holochain conductor config field. Kept false by default to make clear this has no effect. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-wasmhostfunctionslistedcallback"></a>
### HoloNETClientAppBase.WasmHostFunctionsListedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void WasmHostFunctionsListedCallBack(object sender, WasmHostFunctionsListedCallBackEventArgs e)` (the type of the `OnWasmHostFunctionsListedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-wasmhostfunctionslistedcallbackeventargs"></a>
### WasmHostFunctionsListedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `List<string> WasmHostFunctions { get; set; }` | property | What the conductor returned for this request (wasm host functions). |

<a id="nextgensoftware-holochain-holonet-client-zomecall"></a>
### ZomeCall (class)

Mirrors the Rust holochain_zome_types::zome_io::ZomeCallParams struct (Holochain 0.7.0): https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_zome_types/src/zome_io.rs pub struct ZomeCallParams { pub provenance: AgentPubKey, pub cell_id: CellId, pub zome_name: ZomeName, pub fn_name: FunctionName, pub cap_secret: Option<CapSecret>, pub payload: ExternIO, pub nonce: Nonce256Bits, pub expires_at: Timestamp, } CellId itself is a Rust tuple struct `CellId(DnaHash, AgentPubKey)`, which (de)serializes as a 2-element array, hence the use of the dedicated `CellId` type below (which is itself MessagePack-encoded with positional keys 0/1 to match the tuple shape) rather than the previous flattened byte[][] / split dna_hash+agent_pub_key fields.

| Member | Kind | Description |
|---|---|---|
| `byte[] cap_secret { get; set; }` | property | Holochain's `cap_secret` field (sent to / received from the conductor as-is). |
| `CellId cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `long expires_at { get; set; }` | property | Holochain's `expires_at` field (sent to / received from the conductor as-is). |
| `string fn_name { get; set; }` | property | Holochain's `fn_name` field (sent to / received from the conductor as-is). |
| `byte[] nonce { get; set; }` | property | Holochain's `nonce` field (sent to / received from the conductor as-is). |
| `byte[] payload { get; set; }` | property | Holochain's `payload` field (sent to / received from the conductor as-is). |
| `byte[] provenance { get; set; }` | property | Holochain's `provenance` field (sent to / received from the conductor as-is). |
| `string zome_name { get; set; }` | property | Holochain's `zome_name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-zomecallcapabilitygrantedcallback"></a>
### HoloNETClientAdmin.ZomeCallCapabilityGrantedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void ZomeCallCapabilityGrantedCallBack(object sender, ZomeCallCapabilityGrantedCallBackEventArgs e)` (the type of the `OnZomeCallCapabilityGrantedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-zomecallcapabilitygrantedcallbackeventargs"></a>
### ZomeCallCapabilityGrantedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-holonetclientadmin-zomecallcapabilityrevokedcallback"></a>
### HoloNETClientAdmin.ZomeCallCapabilityRevokedCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void ZomeCallCapabilityRevokedCallBack(object sender, ZomeCallCapabilityRevokedCallBackEventArgs e)` (the type of the `OnZomeCallCapabilityRevokedCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-zomecallcapabilityrevokedcallbackeventargs"></a>
### ZomeCallCapabilityRevokedCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

<a id="nextgensoftware-holochain-holonet-client-zomecallparamssigned"></a>
### ZomeCallParamsSigned (class)

Mirrors holochain_conductor_api::app_interface::ZomeCallParamsSigned (Holochain 0.7.0). This is the actual struct sent for the `call_zome` AppRequest on the app websocket interface as of 0.7.0: `AppRequest::CallZome(Box<ZomeCallParamsSigned>)`. pub struct ZomeCallParamsSigned { /// Bytes of the serialized zome call payload (holochain_serialized_bytes::encode of /// the unsigned ZomeCallParams/ZomeCall fields). pub bytes: ExternIO, /// Signature over the (sha2-512) hash of `bytes`, signed by the provenance agent key. pub signature: Signature, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs

| Member | Kind | Description |
|---|---|---|
| `ZomeCallParamsSigned(byte[] bytes, byte[] signature)` | ctor | Signed zome call parameters: the serialised call and its signature. |
| `byte[] bytes { get; set; }` | property | Holochain's `bytes` field (sent to / received from the conductor as-is). |
| `byte[] signature { get; set; }` | property | Holochain's `signature` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-zomecallsigned"></a>
### ZomeCallSigned (class)

Inherits / implements: `ZomeCall`

| Member | Kind | Description |
|---|---|---|
| `byte[] signature { get; set; }` | property | Holochain's `signature` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-zomecalltosign"></a>
### ZomeCallToSign (class)

The fields of a zome call before signing. A signer chooses the provenance (agent key) and, when it needs one, the cap secret, then returns the signed call.

| Member | Kind | Description |
|---|---|---|
| `byte[] CellIdAgentPubKey { get; set; }` | property | The agent public key of the cell being called. |
| `byte[] CellIdDnaHash { get; set; }` | property | The DNA hash of the cell being called. |
| `long ExpiresAt { get; set; }` | property | Microseconds since the Unix epoch. |
| `string FnName { get; set; }` | property | The zome function being called. |
| `byte[] Nonce { get; set; }` | property | The call's one-time nonce (replay protection). |
| `byte[] Payload { get; set; }` | property | The msgpack-encoded zome function input. |
| `string ZomeName { get; set; }` | property | This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. You can set this either through this property or through one of the constructors. |

<a id="nextgensoftware-holochain-holonet-client-zomecallunsigned"></a>
### ZomeCallUnsigned (struct)

| Member | Kind | Description |
|---|---|---|
| `byte[] cap_secret` | field | Holochain's `cap_secret` field (sent to / received from the conductor as-is). |
| `byte[] cell_id_agent_pub_key` | field | Holochain's `cell_id_agent_pub_key` field (sent to / received from the conductor as-is). |
| `byte[] cell_id_dna_hash` | field | Holochain's `cell_id_dna_hash` field (sent to / received from the conductor as-is). |
| `long expires_at` | field | Holochain's `expires_at` field (sent to / received from the conductor as-is). |
| `string fn_name` | field | Holochain's `fn_name` field (sent to / received from the conductor as-is). |
| `byte[] nonce` | field | Holochain's `nonce` field (sent to / received from the conductor as-is). |
| `byte[] payload` | field | Holochain's `payload` field (sent to / received from the conductor as-is). |
| `byte[] provenance` | field | Holochain's `provenance` field (sent to / received from the conductor as-is). |
| `string zome_name` | field | Holochain's `zome_name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-zomedefinition"></a>
### ZomeDefinition (class)

| Member | Kind | Description |
|---|---|---|
| `List<string> Dependencies { get; set; }` | property | The integrity zomes this coordinator zome depends on. |
| `byte[] wasm_hash { get; set; }` | property | Holochain's `wasm_hash` field (sent to / received from the conductor as-is). |
| `string ZomeName { get; set; }` | property | This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. You can set this either through this property or through one of the constructors. |

<a id="nextgensoftware-holochain-holonet-client-holonetclientappbase-zomefunctioncallback"></a>
### HoloNETClientAppBase.ZomeFunctionCallBack (delegate)

Inherits / implements: `MulticastDelegate`, `ICloneable`, `ISerializable`

Handler signature: `void ZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)` (the type of the `OnZomeFunctionCallBack` event).

<a id="nextgensoftware-holochain-holonet-client-zomefunctioncallbackeventargs"></a>
### ZomeFunctionCallBackEventArgs (class)

Inherits / implements: `HoloNETDataReceivedBaseEventArgs`

| Member | Kind | Description |
|---|---|---|
| `Dictionary<string, string> KeyValuePair { get; set; }` | property | Contains all of the data returned from the zome call in a simple Dictionary keyvalue pair. The conductor for some reason returns a complex nested structure so is difficult and tedious to get to all the data needed or to quickly view all of it at once. |
| `string KeyValuePairAsString { get; set; }` | property | Contains the same data from KeyValuePair but formatted as a simple string, which can be used for logging, displaying, etc. |
| `Dictionary<object, object> RawZomeReturnData { get; set; }` | property | The raw binary data that the zome function returned. |
| `List<Record> Records { get; set; }` | property | What the conductor returned for this request (records). |
| `string Zome { get; set; }` | property | The zome that made the request. |
| `string ZomeFunction { get; set; }` | property | The zome function that made the request. |
| `Dictionary<string, object> ZomeReturnData { get; set; }` | property | The parsed data that the zome function returned. HoloNET will parse & convert the best it can from the Rust Holochain Conductor format to a C# friendly one such as converting from base64 encoding, etc. |
| `string ZomeReturnHash { get; set; }` | property | The ActionHash returned from a zome call (if it returned one, is null if not). |
| `object ZomeReturnValue { get; set; }` | property | The zome function's return value as decoded msgpack, whatever its type: null for (), a bool, number or string, a byte[] hash, a list (e.g. Vec<ActionHash>) or a map. Records / KeyValuePair are also filled for record results. |

## NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest

<a id="nextgensoftware-holochain-holonet-client-data-admin-appmanifest-appmanifest"></a>
### AppManifest (class)

Inherits / implements: `IAppManifest`

| Member | Kind | Description |
|---|---|---|
| `string description { get; set; }` | property | Holochain's `description` field (sent to / received from the conductor as-is). |
| `string manifest_version { get; set; }` | property | Holochain's `manifest_version` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `Roles[] roles { get; set; }` | property | Holochain's `roles` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-appmanifest-appmanifestcurrent"></a>
### AppManifestCurrent (struct)

| Member | Kind | Description |
|---|---|---|
| `string description { get; set; }` | property | Holochain's `description` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `string roles { get; set; }` | property | Holochain's `roles` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-appmanifest-bundeled"></a>
### Bundeled (struct)

| Member | Kind | Description |
|---|---|---|
| `object _version { get; set; }` | property | Holochain's `_version` field (sent to / received from the conductor as-is). |
| `object clone_limit { get; set; }` | property | Holochain's `clone_limit` field (sent to / received from the conductor as-is). |
| `object installed_hash { get; set; }` | property | Holochain's `installed_hash` field (sent to / received from the conductor as-is). |
| `DnaModifiers modifiers { get; set; }` | property | Holochain's `modifiers` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-appmanifest-dna"></a>
### Dna (struct)

| Member | Kind | Description |
|---|---|---|
| `Bundeled bundeled { get; set; }` | property | Holochain's `bundeled` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-appmanifest-dnamodifiers"></a>
### DnaModifiers (class)

Inherits / implements: `IDnaModifiers`

| Member | Kind | Description |
|---|---|---|
| `string network_seed { get; set; }` | property | Holochain's `network_seed` field (sent to / received from the conductor as-is). |
| `long origin_time { get; set; }` | property | Holochain's `origin_time` field (sent to / received from the conductor as-is). |
| `DateTime OriginTime { get; set; }` | property | Holochain's `origin_time` field (sent to / received from the conductor as-is). |
| `object properties { get; set; }` | property | Holochain's `properties` field (sent to / received from the conductor as-is). |
| `RustDuration quantum_time { get; set; }` | property | Holochain's `quantum_time` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-appmanifest-provisioning"></a>
### Provisioning (class)

| Member | Kind | Description |
|---|---|---|
| `bool deferred { get; set; }` | property | Holochain's `deferred` field (sent to / received from the conductor as-is). |
| `string strategy { get; set; }` | property | Holochain's `strategy` field (sent to / received from the conductor as-is). |
| `ProvisioningStrategyType StrategyType { get; set; }` | property | Holochain's `strategy_type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-appmanifest-roles"></a>
### Roles (struct)

| Member | Kind | Description |
|---|---|---|
| `Dna dna { get; set; }` | property | Holochain's `dna` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `Provisioning provisioning { get; set; }` | property | Holochain's `provisioning` field (sent to / received from the conductor as-is). |

## NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-addagentinforequest"></a>
### AddAgentInfoRequest (class)

| Member | Kind | Description |
|---|---|---|
| `object[] agent_infos { get; set; }` | property | Holochain's `agent_infos` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-attachappinterfacerequest"></a>
### AttachAppInterfaceRequest (class)

Mirrors AdminRequest::AttachAppInterface (Holochain 0.7.0): { port: Option<u16>, danger_bind_addr: Option<String>, allowed_origins: AllowedOrigins, installed_app_id: Option<InstalledAppId> }. allowed_origins is required: the conductor rejects a request without it ("Failed to deserialize request", confirmed against a live 0.7.0 conductor).

| Member | Kind | Description |
|---|---|---|
| `string allowed_origins { get; set; }` | property | "*" for any origin, or a comma-separated list of allowed origins. |
| `string danger_bind_addr { get; set; }` | property | Holochain's `danger_bind_addr` field (sent to / received from the conductor as-is). |
| `string installed_app_id { get; set; }` | property | Restrict the interface to one app, or null for all apps. |
| `ushort? port { get; set; }` | property | Holochain's `port` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-deleteclonecellrequest"></a>
### DeleteCloneCellRequest (class)

| Member | Kind | Description |
|---|---|---|
| `string app_id { get; set; }` | property | The app id that the clone cell belongs to |
| `object clone_cell_id { get; set; }` | property | The CloneCellId identifying the clone cell — either a clone id string (e.g. "role_name.0") or a CellId byte[][] tuple. Wire (adjacently tagged): {"type": "clone_id", "value": "role.0"} or {"type": "dna_hash", "value": bytes}. Build with CloneCellId.FromCloneId / FromDnaHash. |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-disableapprequest"></a>
### DisableAppRequest (class)

| Member | Kind | Description |
|---|---|---|
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-dumpfullstaterequest"></a>
### DumpFullStateRequest (class)

| Member | Kind | Description |
|---|---|---|
| `byte[][] cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `DhtOpsCursor dht_ops_cursor { get; set; }` | property | Holochain's `dht_ops_cursor` field (sent to / received from the conductor as-is). |
| `uint? limit { get; set; }` | property | Maximum number of DHT ops per page (null = conductor default). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-dumpnetworkmetricsrequest"></a>
### DumpNetworkMetricsRequest (class)

Mirrors AdminRequest::DumpNetworkMetrics { dna_hash: Option<DnaHash>, include_dht_summary: bool } (Holochain 0.7.0). Sending no payload is rejected with "Failed to deserialize request".

| Member | Kind | Description |
|---|---|---|
| `byte[] dna_hash { get; set; }` | property | Restrict to one DNA, or null for all DNAs. |
| `bool include_dht_summary { get; set; }` | property | Holochain's `include_dht_summary` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-dumpstaterequest"></a>
### DumpStateRequest (class)

| Member | Kind | Description |
|---|---|---|
| `byte[][] cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `uint? limit { get; set; }` | property | Maximum number of records per page (null = conductor default). |
| `object source_chain_cursor { get; set; }` | property | Holochain 0.7 paging: a `SourceChainCursor` value, or null to start at the beginning. |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-enableapprequest"></a>
### EnableAppRequest (class)

| Member | Kind | Description |
|---|---|---|
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-getagentinforequest"></a>
### GetAgentInfoRequest (class)

Mirrors AdminRequest::AgentInfo { dna_hashes: Option<Vec<DnaHash>> } (Holochain 0.7.0). Null returns agent info for every DNA.

| Member | Kind | Description |
|---|---|---|
| `byte[][] dna_hashes { get; set; }` | property | Holochain's `dna_hashes` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-getcompatiblecellsrequest"></a>
### GetCompatibleCellsRequest (class)

Mirrors holochain_conductor_api::admin_interface::AdminRequest::GetCompatibleCells (Holochain 0.7.0). The payload is a single DnaHash: GetCompatibleCells(DnaHash) https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs NOTE: This variant is gated behind the `unstable-migration` feature flag in Holochain 0.7.0 and so may not be available on all conductor builds.

| Member | Kind | Description |
|---|---|---|
| `byte[] dna_hash { get; set; }` | property | Holochain's `dna_hash` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-graftrecordsrequest"></a>
### GraftRecordsRequest (class)

| Member | Kind | Description |
|---|---|---|
| `byte[][] cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `object[] records { get; set; }` | property | Holochain's `records` field (sent to / received from the conductor as-is). |
| `bool validate { get; set; }` | property | Holochain's `validate` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-grantzomecallcapabilityrequest"></a>
### GrantZomeCallCapabilityRequest (class)

| Member | Kind | Description |
|---|---|---|
| `ZomeCallCapGrant cap_grant { get; set; }` | property | Holochain's `cap_grant` field (sent to / received from the conductor as-is). |
| `byte[][] cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-installapprequest"></a>
### InstallAppRequest (class)

Mirrors holochain_types::app::InstallAppPayload (Holochain 0.7.0): pub struct InstallAppPayload { pub source: AppBundleSource, // {"type": "path" | "bytes", "value": ...} pub agent_key: Option<AgentPubKey>, pub installed_app_id: Option<InstalledAppId>, pub network_seed: Option<NetworkSeed>, pub roles_settings: Option<HashMap<RoleName, RoleSettings>>, pub ignore_genesis_failure: bool, pub restore_from_dht: bool, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_types/src/app.rs

| Member | Kind | Description |
|---|---|---|
| `byte[] agent_key { get; set; }` | property | Holochain's `agent_key` field (sent to / received from the conductor as-is). |
| `bool ignore_genesis_failure { get; set; }` | property | Holochain's `ignore_genesis_failure` field (sent to / received from the conductor as-is). |
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |
| `string network_seed { get; set; }` | property | Holochain's `network_seed` field (sent to / received from the conductor as-is). |
| `bool restore_from_dht { get; set; }` | property | Holochain's `restore_from_dht` field (sent to / received from the conductor as-is). |
| `Dictionary<string, object> roles_settings { get; set; }` | property | Per-role settings; build with InstallAppRequest.RolesSettingsFromMembraneProofs. |
| `Dictionary<string, object> source { get; set; }` | property | Build with InstallAppRequest.SourceFromPath / SourceFromBytes. |
| `static Dictionary<string, object> RolesSettingsFromMembraneProofs(Dictionary<string, byte[]> membraneProofs)` | method | Maps role name → membrane proof to RoleSettings::Provisioned ({"type": "provisioned", "value": {"membrane_proof", "modifiers", "init_properties"}}). |
| `static Dictionary<string, object> SourceFromBytes(byte[] happBytes)` | method | AppBundleSource::Bytes — the raw contents of a .happ file. |
| `static Dictionary<string, object> SourceFromPath(string happPath)` | method | AppBundleSource::Path — a .happ path on the conductor's filesystem. |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-issueappauthenticationtokenrequest"></a>
### IssueAppAuthenticationTokenRequest (class)

Mirrors holochain_conductor_api::admin_interface::IssueAppAuthenticationTokenPayload, used as the payload for AdminRequest::IssueAppAuthenticationToken (Holochain 0.7.0): pub struct IssueAppAuthenticationTokenPayload { pub installed_app_id: InstalledAppId, #[serde(default = "default_expiry_seconds")] pub expiry_seconds: u64, #[serde(default = "default_single_use")] pub single_use: bool, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs

| Member | Kind | Description |
|---|---|---|
| `ulong expiry_seconds { get; set; }` | property | Holochain's `expiry_seconds` field (sent to / received from the conductor as-is). |
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |
| `bool single_use { get; set; }` | property | Holochain's `single_use` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-listappsrequest"></a>
### ListAppsRequest (class)

| Member | Kind | Description |
|---|---|---|
| `string status_filter { get; set; }` | property | Holochain's `status_filter` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-listcapabilitygrantsrequest"></a>
### ListCapabilityGrantsRequest (class)

Mirrors holochain_conductor_api::admin_interface::AdminRequest::ListCapabilityGrants (Holochain 0.7.0): ListCapabilityGrants { installed_app_id: String, include_revoked: bool, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs

| Member | Kind | Description |
|---|---|---|
| `bool include_revoked { get; set; }` | property | Holochain's `include_revoked` field (sent to / received from the conductor as-is). |
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-listdnasrequest"></a>
### ListDnasRequest (class)

| Member | Kind | Description |
|---|---|---|
| `AppStatusFilter status_filter { get; set; }` | property | Holochain's `status_filter` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-peermetainforequest"></a>
### PeerMetaInfoRequest (class)

Mirrors holochain_conductor_api::admin_interface::AdminRequest::PeerMetaInfo (Holochain 0.7.0). Also used by the equivalent AppRequest::PeerMetaInfo variant on the app interface - same shape on both interfaces. PeerMetaInfo { url: Url, dna_hashes: Option<Vec<DnaHash>>, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs

| Member | Kind | Description |
|---|---|---|
| `List<byte[]> dna_hashes { get; set; }` | property | Holochain's `dna_hashes` field (sent to / received from the conductor as-is). |
| `string url { get; set; }` | property | Holochain's `url` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-registerdnarequest"></a>
### RegisterDnaRequest (class)

| Member | Kind | Description |
|---|---|---|
| `DnaBundle bundle { get; set; }` | property | Holochain's `bundle` field (sent to / received from the conductor as-is). |
| `byte[] hash { get; set; }` | property | Holochain's `hash` field (sent to / received from the conductor as-is). |
| `DnaModifiers modifiers { get; set; }` | property | Holochain's `modifiers` field (sent to / received from the conductor as-is). |
| `string path { get; set; }` | property | Holochain's `path` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-revokeappauthenticationtokenrequest"></a>
### RevokeAppAuthenticationTokenRequest (class)

Mirrors holochain_conductor_api::admin_interface::AdminRequest::RevokeAppAuthenticationToken (Holochain 0.7.0). The payload is the raw token itself: `pub type AppAuthenticationToken = Vec<u8>;` RevokeAppAuthenticationToken(AppAuthenticationToken) https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs

| Member | Kind | Description |
|---|---|---|
| `byte[] token { get; set; }` | property | Holochain's `token` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-revokezomecallcapabilityrequest"></a>
### RevokeZomeCallCapabilityRequest (class)

Mirrors holochain_conductor_api::admin_interface::AdminRequest::RevokeZomeCallCapability (Holochain 0.7.0): RevokeZomeCallCapability { action_hash: ActionHash, cell_id: CellId, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs

| Member | Kind | Description |
|---|---|---|
| `byte[] action_hash { get; set; }` | property | Holochain's `action_hash` field (sent to / received from the conductor as-is). |
| `CellId cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-uninstallapprequest"></a>
### UninstallAppRequest (class)

| Member | Kind | Description |
|---|---|---|
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-updatecoordinatorsrequest"></a>
### UpdateCoordinatorsRequest (class)

| Member | Kind | Description |
|---|---|---|
| `CoordinatorBundle bundle { get; set; }` | property | Holochain's `bundle` field (sent to / received from the conductor as-is). |
| `byte[] dnaHash { get; set; }` | property | Holochain's `dna_hash` field (sent to / received from the conductor as-is). |
| `string path { get; set; }` | property | Holochain's `path` field (sent to / received from the conductor as-is). |

## NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-agentinfo"></a>
### AgentInfo (class)

| Member | Kind | Description |
|---|---|---|
| `byte[] agent { get; set; }` | property | Holochain's `agent` field (sent to / received from the conductor as-is). |
| `byte[] agent_info { get; set; }` | property | Holochain's `agent_info` field (sent to / received from the conductor as-is). |
| `byte[] signature { get; set; }` | property | Holochain's `signature` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-agentinfodetailed"></a>
### AgentInfoDetailed (class)

| Member | Kind | Description |
|---|---|---|
| `byte[] agent { get; set; }` | property | Holochain's `agent` field (sent to / received from the conductor as-is). |
| `byte[] agent_info { get; set; }` | property | Holochain's `agent_info` field (sent to / received from the conductor as-is). |
| `byte[] signature { get; set; }` | property | Holochain's `signature` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-appbundle"></a>
### AppBundle (class)

| Member | Kind | Description |
|---|---|---|
| `AppManifest manifest { get; set; }` | property | Holochain's `manifest` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capaccess"></a>
### CapAccess (static class)

Builds the wire form of holochain_integrity_types::capability::CapAccess. Rust: #[serde(tag = "type", content = "value", rename_all = "snake_case")] Wire: {"type": "unrestricted"} {"type": "transferable", "value": {"secret": bytes}} {"type": "assigned", "value": {"secret": bytes, "assignees": [agent_pub_key, ...]}} Verified against holochain-0.7.0 (and 0.6.1, which uses the same tagging). Supersedes CapGrantAccessUnrestricted/Transferable/Assigned, which emitted the externally-tagged {"Unrestricted": ...} form the conductor does not accept.

| Member | Kind | Description |
|---|---|---|
| `static Dictionary<string, object> Assigned(byte[] secret, byte[][] assignees)` | method | A grant only these agents may use, with this secret (what HoloNET uses for zome-call signing). |
| `static Dictionary<string, object> Transferable(byte[] secret)` | method | A grant anyone holding this secret may use. |
| `static Dictionary<string, object> Unrestricted()` | method | A grant anyone may use (Holochain 0.7 rejects this for zome-call signing; fine for e.g. recv_remote_signal). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccessassigned"></a>
### CapGrantAccessAssigned (class)

| Member | Kind | Description |
|---|---|---|
| `CapGrantAccessAssignedDetails Assigned { get; set; }` | property | Holochain's `assigned` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccessassigneddetails"></a>
### CapGrantAccessAssignedDetails (class)

| Member | Kind | Description |
|---|---|---|
| `byte[][] assignees { get; set; }` | property | Holochain's `assignees` field (sent to / received from the conductor as-is). |
| `byte[] secret { get; set; }` | property | Holochain's `secret` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccesstransferable"></a>
### CapGrantAccessTransferable (class)

| Member | Kind | Description |
|---|---|---|
| `CapGrantAccessTransferableDetails Transferable { get; set; }` | property | Holochain's `transferable` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccesstransferabledetails"></a>
### CapGrantAccessTransferableDetails (class)

| Member | Kind | Description |
|---|---|---|
| `byte[] secret { get; set; }` | property | Holochain's `secret` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-capgrantaccessunrestricted"></a>
### CapGrantAccessUnrestricted (class)

| Member | Kind | Description |
|---|---|---|
| `object Unrestricted { get; set; }` | property | Holochain's `unrestricted` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-cellinfo"></a>
### CellInfo (class)

| Member | Kind | Description |
|---|---|---|
| `CellInfoType CellInfoType { get; }` | property | Holochain's `cell_info_type` field (sent to / received from the conductor as-is). |
| `ClonedCell Cloned { get; set; }` | property | Holochain's `cloned` field (sent to / received from the conductor as-is). |
| `ProvisionedCell Provisioned { get; set; }` | property | Holochain's `provisioned` field (sent to / received from the conductor as-is). |
| `StemCell Stem { get; set; }` | property | Holochain's `stem` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-cellinfoformatter"></a>
### CellInfoFormatter (class)

Decodes holochain_conductor_api::CellInfo, which is adjacently tagged: #[serde(tag = "type", content = "value", rename_all = "snake_case")] Wire: {"type": "provisioned" | "cloned" | "stem", "value": {...}} A formatter is needed because the type of "value" depends on "type".

Inherits / implements: `IMessagePackFormatter<CellInfo>`, `IMessagePackFormatter`

| Member | Kind | Description |
|---|---|---|
| `CellInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)` | method | MessagePack formatter plumbing used by HoloNET's serialiser (you don't call this). |
| `void Serialize(ref MessagePackWriter writer, CellInfo value, MessagePackSerializerOptions options)` | method | MessagePack formatter plumbing used by HoloNET's serialiser (you don't call this). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-coordinatorbundle"></a>
### CoordinatorBundle (class)

| Member | Kind | Description |
|---|---|---|
| `CoordinatorManifest manifest { get; set; }` | property | Holochain's `manifest` field (sent to / received from the conductor as-is). |
| `Dictionary<string, byte[]> resources { get; set; }` | property | Holochain's `resources` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-coordinatormanifest"></a>
### CoordinatorManifest (class)

Mirrors holochain_types::dna::dna_manifest::CoordinatorManifest (Holochain 0.7.0). deny_unknown_fields: pub struct CoordinatorManifest { pub zomes: Vec<ZomeManifest>, } Coordinator zomes use the same ZomeManifest as integrity zomes (ZomeDependency objects).

| Member | Kind | Description |
|---|---|---|
| `ZomeManifest[] zomes { get; set; }` | property | Holochain's `zomes` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-coordinatorzomemanifest"></a>
### CoordinatorZomeManifest (class)

One coordinator zome entry inside CoordinatorManifest.zomes (Holochain 0.7.0). Mirrors holochain_types::dna::wasm::CoordinatorZomeManifest: pub struct CoordinatorZomeManifest { pub name: ZomeName, pub dylib: Option<PathBuf>, pub hash: Option<WasmHash>, pub dependencies: Option<Vec<ZomeName>>, // plain strings, NOT ZomeDependency objects pub bundled/path/url: (ZomeLocation), } Note: coordinator zome dependencies are plain ZomeName strings (not wrapped in ZomeDependency), unlike IntegrityZomeManifest which uses ZomeDependency objects.

| Member | Kind | Description |
|---|---|---|
| `string bundled { get; set; }` | property | Expect file to be part of this bundle (ZomeLocation::Bundled). |
| `string[] dependencies { get; set; }` | property | Names of integrity zomes this coordinator depends on. Wire format: plain strings (ZomeName), e.g. ["my_integrity_zome"]. |
| `string dylib { get; set; }` | property | Holochain's `dylib` field (sent to / received from the conductor as-is). |
| `string hash { get; set; }` | property | Holochain's `hash` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `string path { get; set; }` | property | Get file from local filesystem (ZomeLocation::Path). |
| `string url { get; set; }` | property | Get file from URL (ZomeLocation::Url). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-dnabundle"></a>
### DnaBundle (class)

| Member | Kind | Description |
|---|---|---|
| `DnaManifest manifest { get; set; }` | property | Holochain's `manifest` field (sent to / received from the conductor as-is). |
| `Dictionary<string, byte[]> resources { get; set; }` | property | Holochain's `resources` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-dnamanifest"></a>
### DnaManifest (class)

Mirrors holochain_types::dna::DnaManifest (Holochain 0.7.0), an internally tagged enum: #[serde(tag = "manifest_version")] enum DnaManifest { #[serde(rename = "0")] V0(DnaManifestV0) } DnaManifestV0 (deny_unknown_fields) { name: String, integrity: IntegrityManifest, coordinator: CoordinatorManifest, // #[serde(default)] lineage: Vec<DnaHashB64>, // only with the unstable-migration feature } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_types/src/dna/dna_manifest.rs

| Member | Kind | Description |
|---|---|---|
| `CoordinatorManifest coordinator { get; set; }` | property | Holochain's `coordinator` field (sent to / received from the conductor as-is). |
| `IntegrityManifest integrity { get; set; }` | property | Holochain's `integrity` field (sent to / received from the conductor as-is). |
| `string manifest_version { get; set; }` | property | The manifest version tag. Must be "0" for Holochain 0.7.0. |
| `string name { get; set; }` | property | The friendly "name" of a Holochain DNA. |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-grantedfunctions"></a>
### GrantedFunctions (class)

Builds the wire form of holochain_integrity_types::capability::GrantedFunctions. Rust: #[serde(tag = "type", content = "value", rename_all = "snake_case")] Wire: {"type": "all"} or {"type": "listed", "value": [[zome, fn], ...]} Verified against holochain-0.7.0 (and 0.6.1, which uses the same tagging). A dictionary is used so the unit variant omits the "value" key entirely.

| Member | Kind | Description |
|---|---|---|
| `Dictionary<string, object> Functions { get; set; }` | property | Holochain's `functions` field (sent to / received from the conductor as-is). |
| `static GrantedFunctions All()` | method | Grant every zome function. |
| `static GrantedFunctions Listed(List<ValueTuple<string, string>> grants)` | method | Grant only these (zome, function) pairs. |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-grantedfunctionstype"></a>
### GrantedFunctionsType (enum)

| Value | Description |
|---|---|
| `All` |  |
| `Listed` |  |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-integritymanifest"></a>
### IntegrityManifest (class)

Mirrors holochain_types::dna::dna_manifest::IntegrityManifest (Holochain 0.7.0). deny_unknown_fields: pub struct IntegrityManifest { pub network_seed: Option<String>, pub properties: Option<YamlProperties>, pub zomes: Vec<ZomeManifest>, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_types/src/dna/dna_manifest/dna_manifest_v0.rs

| Member | Kind | Description |
|---|---|---|
| `string network_seed { get; set; }` | property | A network seed for uniquifying this DNA. |
| `object properties { get; set; }` | property | Arbitrary DNA properties (YamlProperties = serde_yaml::Value). |
| `ZomeManifest[] zomes { get; set; }` | property | Holochain's `zomes` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-integrityzomemanifest"></a>
### IntegrityZomeManifest (class)

One integrity zome entry inside IntegrityManifest.zomes (Holochain 0.7.0). Mirrors holochain_types::dna::wasm::IntegrityZomeManifest: pub struct IntegrityZomeManifest { pub name: ZomeName, pub dylib: Option<PathBuf>, pub hash: Option<WasmHash>, pub dependencies: Option<Vec<ZomeDependency>>, pub bundled/path/url: (ZomeLocation), pub properties: Option<YamlProperties>, } Note: dependencies here are ZomeDependency objects (name-wrapped), unlike CoordinatorZomeManifest which uses plain string ZomeName dependencies.

| Member | Kind | Description |
|---|---|---|
| `string bundled { get; set; }` | property | Expect file to be part of this bundle (ZomeLocation::Bundled). |
| `ZomeDependency[] dependencies { get; set; }` | property | Holochain's `dependencies` field (sent to / received from the conductor as-is). |
| `string dylib { get; set; }` | property | Holochain's `dylib` field (sent to / received from the conductor as-is). |
| `string hash { get; set; }` | property | Holochain's `hash` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `string path { get; set; }` | property | Get file from local filesystem (ZomeLocation::Path). |
| `object properties { get; set; }` | property | Arbitrary YAML properties for this integrity zome (Option<YamlProperties>). Kept as dynamic because YamlProperties is serde_yaml::Value — any compatible object. |
| `string url { get; set; }` | property | Get file from URL (ZomeLocation::Url). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-keypair"></a>
### KeyPair (class)

| Member | Kind | Description |
|---|---|---|
| `byte[] PrivateKey { get; set; }` | property | Holochain's `private_key` field (sent to / received from the conductor as-is). |
| `byte[] PublicKey { get; set; }` | property | Holochain's `public_key` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-kitsuneagent"></a>
### KitsuneAgent (class)

| Member | Kind | Description |
|---|---|---|
| `KitsuneAgent(byte[] data)` | ctor | A Kitsune (Holochain networking) agent id from its raw bytes. |
| `byte[] Data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `byte[] GetData()` | method | The raw bytes. |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-kitsunesignature"></a>
### KitsuneSignature (class)

| Member | Kind | Description |
|---|---|---|
| `KitsuneSignature(byte[] data)` | ctor | A Kitsune signature from its raw bytes. |
| `byte[] Data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `byte[] GetData()` | method | The raw bytes. |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-kitsunespace"></a>
### KitsuneSpace (class)

| Member | Kind | Description |
|---|---|---|
| `KitsuneSpace(byte[] data)` | ctor | A Kitsune space (a DNA's network) id from its raw bytes. |
| `byte[] Data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `byte[] GetData()` | method | The raw bytes. |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-rustduration"></a>
### RustDuration (struct)

| Member | Kind | Description |
|---|---|---|
| `int nanos { get; set; }` | property | Holochain's `nanos` field (sent to / received from the conductor as-is). |
| `int secs { get; set; }` | property | Holochain's `secs` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-signingcredentials"></a>
### SigningCredentials (class)

| Member | Kind | Description |
|---|---|---|
| `byte[] CapSecret { get; set; }` | property | Holochain's `cap_secret` field (sent to / received from the conductor as-is). |
| `KeyPair KeyPair { get; set; }` | property | Holochain's `key_pair` field (sent to / received from the conductor as-is). |
| `byte[] SigningKey { get; set; }` | property | Holochain's `signing_key` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-zomecallcapgrant"></a>
### ZomeCallCapGrant (class)

| Member | Kind | Description |
|---|---|---|
| `object access { get; set; }` | property | Holochain's `access` field (sent to / received from the conductor as-is). |
| `Dictionary<string, object> functions { get; set; }` | property | Holochain's `functions` field (sent to / received from the conductor as-is). |
| `string tag { get; set; }` | property | Holochain's `tag` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-zomedependency"></a>
### ZomeDependency (class)

| Member | Kind | Description |
|---|---|---|
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-admin-requests-objects-zomemanifest"></a>
### ZomeManifest (class)

Mirrors holochain_types::dna::dna_manifest::ZomeManifest (Holochain 0.7.0), used for both integrity and coordinator zomes. The Rust struct is deny_unknown_fields: pub struct ZomeManifest { pub name: ZomeName, pub hash: Option<WasmHashB64>, pub path: String, pub dependencies: Option<Vec<ZomeDependency>>, } https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_types/src/dna/dna_manifest/dna_manifest_v0.rs

| Member | Kind | Description |
|---|---|---|
| `ZomeDependency[] dependencies { get; set; }` | property | Holochain's `dependencies` field (sent to / received from the conductor as-is). |
| `string hash { get; set; }` | property | Holochain's `hash` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `string path { get; set; }` | property | Path to the zome's .wasm, relative to the manifest. Required. |

## NextGenSoftware.Holochain.HoloNET.Client.Data.App.Requests

<a id="nextgensoftware-holochain-holonet-client-data-app-requests-appauthenticationenvelope"></a>
### AppAuthenticationEnvelope (class)

| Member | Kind | Description |
|---|---|---|
| `byte[] Data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string Type { get; set; }` | property | The conductor message type. |

<a id="nextgensoftware-holochain-holonet-client-data-app-requests-appauthenticationrequest"></a>
### AppAuthenticationRequest (class)

| Member | Kind | Description |
|---|---|---|
| `byte[] Token { get; set; }` | property | Holochain's `token` field (sent to / received from the conductor as-is). |

## NextGenSoftware.Holochain.HoloNET.Client.Data.App.Responses.Objects

<a id="nextgensoftware-holochain-holonet-client-data-app-responses-objects-appinfostatus"></a>
### AppInfoStatus (struct)

| Member | Kind | Description |
|---|---|---|
| `object disabled { get; set; }` | property | Holochain's `disabled` field (sent to / received from the conductor as-is). |
| `object paused { get; set; }` | property | Holochain's `paused` field (sent to / received from the conductor as-is). |
| `object running { get; set; }` | property | Holochain's `running` field (sent to / received from the conductor as-is). |
| `AppInfoStatusEnum Status { get; }` | property | Holochain's `status` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-app-responses-objects-clonedcell"></a>
### ClonedCell (class)

Inherits / implements: `ICell`

| Member | Kind | Description |
|---|---|---|
| `byte[][] cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `string clone_id { get; set; }` | property | Holochain's `clone_id` field (sent to / received from the conductor as-is). |
| `DnaModifiers dna_modifiers { get; set; }` | property | Holochain's `dna_modifiers` field (sent to / received from the conductor as-is). |
| `bool enabled { get; set; }` | property | Holochain's `enabled` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `byte[] original_dna_hash { get; set; }` | property | Holochain's `original_dna_hash` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-app-responses-objects-cloneid"></a>
### CloneId (struct)

| Member | Kind | Description |
|---|---|---|
| `string role_name { get; set; }` | property | Holochain's `role_name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-app-responses-objects-cloneidtype"></a>
### CloneIdType (struct)

| Member | Kind | Description |
|---|---|---|
| `byte[][] cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `string clone_id { get; set; }` | property | Holochain's `clone_id` field (sent to / received from the conductor as-is). |
| `bool enabled { get; set; }` | property | Holochain's `enabled` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `byte[] original_dna_hash { get; set; }` | property | Holochain's `original_dna_hash` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-app-responses-objects-provisionedcell"></a>
### ProvisionedCell (class)

Inherits / implements: `ICell`

| Member | Kind | Description |
|---|---|---|
| `byte[][] cell_id { get; set; }` | property | Holochain's `cell_id` field (sent to / received from the conductor as-is). |
| `DnaModifiers dna_modifiers { get; set; }` | property | Holochain's `dna_modifiers` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-data-app-responses-objects-stemcell"></a>
### StemCell (class)

Inherits / implements: `ICell`

| Member | Kind | Description |
|---|---|---|
| `DnaModifiers dna_modifiers { get; set; }` | property | Holochain's `dna_modifiers` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `byte[] original_dna_hash { get; set; }` | property | Holochain's `original_dna_hash` field (sent to / received from the conductor as-is). |

## NextGenSoftware.Holochain.HoloNET.Client.Enums

<a id="nextgensoftware-holochain-holonet-client-enums-optiontype"></a>
### OptionType (enum)

| Value | Description |
|---|---|
| `None` |  |
| `Some` |  |

## NextGenSoftware.Holochain.HoloNET.Client.Interfaces

<a id="nextgensoftware-holochain-holonet-client-interfaces-iappinfo"></a>
### IAppInfo (interface)

| Member | Kind | Description |
|---|---|---|
| `byte[] agent_pub_key { get; set; }` | property | Holochain's `agent_pub_key` field (sent to / received from the conductor as-is). |
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `AppInfoStatusEnum AppStatus { get; }` | property | The app's status as the conductor reports it (enabled, disabled, paused...). |
| `string AppStatusReason { get; }` | property | Why the app is disabled or paused, if it is. |
| `Dictionary<string, List<CellInfo>> cell_info { get; set; }` | property | Holochain's `cell_info` field (sent to / received from the conductor as-is). |
| `byte[][] CellId { get; set; }` | property | The cell id: [DNA hash, agent public key] as raw bytes. |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |
| `string installed_app_id { get; set; }` | property | Holochain's `installed_app_id` field (sent to / received from the conductor as-is). |
| `AppManifest manifest { get; set; }` | property | Holochain's `manifest` field (sent to / received from the conductor as-is). |
| `Dictionary<string, object> status { get; set; }` | property | Holochain's `status` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iappinforesponse"></a>
### IAppInfoResponse (interface)

| Member | Kind | Description |
|---|---|---|
| `AppInfo data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iappmanifest"></a>
### IAppManifest (interface)

| Member | Kind | Description |
|---|---|---|
| `string description { get; set; }` | property | Holochain's `description` field (sent to / received from the conductor as-is). |
| `string manifest_version { get; set; }` | property | Holochain's `manifest_version` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |
| `Roles[] roles { get; set; }` | property | Holochain's `roles` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iappresponse"></a>
### IAppResponse (interface)

| Member | Kind | Description |
|---|---|---|
| `object data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-icell"></a>
### ICell (interface)

| Member | Kind | Description |
|---|---|---|
| `DnaModifiers dna_modifiers { get; set; }` | property | Holochain's `dna_modifiers` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-idnadefinition"></a>
### IDnaDefinition (interface)

| Member | Kind | Description |
|---|---|---|
| `List<ZomeDefinition> CoordinatorZomes { get; set; }` | property | The DNA's coordinator zomes (hot-swappable with UpdateCoordinatorsAsync). |
| `List<ZomeDefinition> IntegrityZomes { get; set; }` | property | The DNA's integrity zomes (entry/link types and validation; they define the DNA hash). |
| `IDnaModifiers Modifiers { get; set; }` | property | The DNA modifiers (network seed, properties, origin time) that make this DNA's hash. |
| `string Name { get; set; }` | property | The DNA's name. |

<a id="nextgensoftware-holochain-holonet-client-interfaces-idnadefinitionresponse"></a>
### IDnaDefinitionResponse (interface)

| Member | Kind | Description |
|---|---|---|
| `DnaDefinitionResponseDetail data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-idnadefinitionresponsedetail"></a>
### IDnaDefinitionResponseDetail (interface)

| Member | Kind | Description |
|---|---|---|
| `object coordinator_zomes_raw { get; set; }` | property | Holochain's `coordinator_zomes_raw` field (sent to / received from the conductor as-is). |
| `object integrity_zomes_raw { get; set; }` | property | Holochain's `integrity_zomes_raw` field (sent to / received from the conductor as-is). |
| `DnaModifiers modifiers { get; set; }` | property | Holochain's `modifiers` field (sent to / received from the conductor as-is). |
| `string name { get; set; }` | property | Holochain's `name` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-idnamodifiers"></a>
### IDnaModifiers (interface)

| Member | Kind | Description |
|---|---|---|
| `string network_seed { get; set; }` | property | Holochain's `network_seed` field (sent to / received from the conductor as-is). |
| `long origin_time { get; set; }` | property | Holochain's `origin_time` field (sent to / received from the conductor as-is). |
| `DateTime OriginTime { get; set; }` | property | The DNA's origin time (part of what makes its hash). |
| `object properties { get; set; }` | property | Holochain's `properties` field (sent to / received from the conductor as-is). |
| `RustDuration quantum_time { get; set; }` | property | Holochain's `quantum_time` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-ifullstatedumpedresponse"></a>
### IFullStateDumpedResponse (interface)

| Member | Kind | Description |
|---|---|---|
| `FullIntegrationStateDump integration_dump { get; set; }` | property | Holochain's `integration_dump` field (sent to / received from the conductor as-is). |
| `P2pAgentsDump peer_dump { get; set; }` | property | Holochain's `peer_dump` field (sent to / received from the conductor as-is). |
| `SourceChainJsonDump source_chain_dump { get; set; }` | property | Holochain's `source_chain_dump` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iholonetclientadmin"></a>
### IHoloNETClientAdmin (interface)

Inherits / implements: `IHoloNETClientBase`

| Member | Kind | Description |
|---|---|---|
| `HoloNETClientAdmin.AdminInterfacesAddedCallBack OnAdminInterfacesAddedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.AgentInfoAddedCallBack OnAgentInfoAddedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.AgentInfoReturnedCallBack OnAgentInfoReturnedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.AgentPubKeyGeneratedCallBack OnAgentPubKeyGeneratedCallBack` | event | Fired when the client receives the generated AgentPubKey from the conductor. |
| `HoloNETClientAdmin.AppAuthenticationTokenIssuedCallBack OnAppAuthenticationTokenIssuedCallBack` | event | Fired when a response is received from the conductor after the IssueAppAuthenticationTokenAsync/IssueAppAuthenticationToken method is called. |
| `HoloNETClientAdmin.AppAuthenticationTokenRevokedCallBack OnAppAuthenticationTokenRevokedCallBack` | event | Fired when a response is received from the conductor after the RevokeAppAuthenticationTokenAsync/RevokeAppAuthenticationToken method is called. |
| `HoloNETClientAdmin.AppDisabledCallBack OnAppDisabledCallBack` | event | Fired when a response is received from the conductor after a hApp has been disabled via the DisableAppAsyc/DisableApp method. |
| `HoloNETClientAdmin.AppEnabledCallBack OnAppEnabledCallBack` | event | Fired when a response is received from the conductor after a hApp has been enabled via the EnableAppAsyc/EnableApp method. |
| `HoloNETClientAdmin.AppInstalledCallBack OnAppInstalledCallBack` | event | Fired when a response is received from the conductor after a hApp has been installed via the InstallAppAsyc/InstallApp method. |
| `HoloNETClientAdmin.AppInterfaceAttachedCallBack OnAppInterfaceAttachedCallBack` | event | Fired when a response is received from the conductor after the app interface has been attached via the AttachAppInterfaceAsyc/AttachAppInterface method. |
| `HoloNETClientAdmin.AppInterfacesListedCallBack OnAppInterfacesListedCallBack` | event | Fired when a response is received from the conductor after the ListAppInterfacesAsync/ListAppInterfaces method is called. |
| `HoloNETClientAdmin.AppsListedCallBack OnAppsListedCallBack` | event | Fired when a response is received from the conductor after the ListAppsAsync/ListApps method is called. |
| `HoloNETClientAdmin.AppUninstalledCallBack OnAppUninstalledCallBack` | event | Fired when a response is received from the conductor after a hApp has been uninstalled via the UninstallAppAsyc/UninstallApp method. |
| `HoloNETClientAdmin.CapabilityGrantsListedCallBack OnCapabilityGrantsListedCallBack` | event | Fired when a response is received from the conductor after the ListCapabilityGrantsAsync/ListCapabilityGrants method is called. |
| `HoloNETClientAdmin.CellIdsListedCallBack OnCellIdsListedCallBack` | event | Fired when a response is received from the conductor after the ListCellIdsAsync/ListCellIds method is called. |
| `HoloNETClientAdmin.CloneCellDeletedCallBack OnCloneCellDeletedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.CompatibleCellsReturnedCallBack OnCompatibleCellsReturnedCallBack` | event | Fired when a response is received from the conductor after the GetCompatibleCellsAsync/GetCompatibleCells method is called. |
| `HoloNETClientAdmin.CoordinatorsUpdatedCallBack OnCoordinatorsUpdatedCallBack` | event | Fired when a response is received from the conductor after the UpdateCoordinatorsAsync/UpdateCoordinators method is called. |
| `HoloNETClientAdmin.DnaDefinitionReturnedCallBack OnDnaDefinitionReturnedCallBack` | event | Fired when a response is received from the conductor containing the DNA Definition for a DNA Hash after the GetDnaDefinitionAsync/GetDnaDefinition method is called. |
| `HoloNETClientAdmin.DnaRegisteredCallBack OnDnaRegisteredCallBack` | event | Fired when a response is received from the conductor after the dna has been registered via the RegisterDnaAsync/RegisterDna method. |
| `HoloNETClientAdmin.DnasListedCallBack OnDnasListedCallBack` | event | Fired when a response is received from the conductor after the ListDnasAsync/ListDnas method is called. |
| `HoloNETClientAdmin.FullStateDumpedCallBack OnFullStateDumpedCallBack` | event | Fired when a response is received from the conductor after the DumpFullStateAsync/DumpFullState method is called. |
| `HoloNETClientAdmin.InstallEnableSignAndAttachHappCallBack OnInstallEnableSignAndAttachHappCallBack` | event | Fired when the hApp has been installed, enabled, signed and attached. |
| `HoloNETClientAdmin.InstallEnableSignAttachAndConnectToHappCallBack OnInstallEnableSignAttachAndConnectToHappCallBack` | event | Fired when the hApp has been installed, enabled, signed and attached. |
| `HoloNETClientAdmin.NetworkMetricsDumpedCallBack OnNetworkMetricsDumpedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.NetworkStatsDumpedCallBack OnNetworkStatsDumpedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.PeerMetaInfoReturnedCallBack OnPeerMetaInfoReturnedCallBack` | event | Fired when a response is received from the conductor after the GetPeerMetaInfoAsync/GetPeerMetaInfo method is called. |
| `HoloNETClientAdmin.RecordsGraftedCallBack OnRecordsGraftedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.StateDumpedCallBack OnStateDumpedCallBack` | event | Fired when a response is received from the conductor after the DumpStateAsync/DumpState method is called. |
| `HoloNETClientAdmin.StorageInfoReturnedCallBack OnStorageInfoReturnedCallBack` | event | Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called. |
| `HoloNETClientAdmin.ZomeCallCapabilityGrantedCallBack OnZomeCallCapabilityGrantedCallBack` | event | Fired when a response is received from the conductor after a cell has been authorized via the AuthorizeSigningCredentialsForZomeCallsAsync/AuthorizeSigningCredentialsForZomeCalls method. |
| `HoloNETClientAdmin.ZomeCallCapabilityRevokedCallBack OnZomeCallCapabilityRevokedCallBack` | event | Fired when a response is received from the conductor after the RevokeZomeCallCapabilityAsync/RevokeZomeCallCapability method is called. |
| `AgentInfoAddedCallBackEventArgs AddAgentInfo(AgentInfo[] agentInfos, string id = null)` | method | Adds encoded agent infos (Holochain 0.7) to the conductor's peer store, e.g. from another conductor's GetAgentInfo. |
| `Task<AgentInfoAddedCallBackEventArgs> AddAgentInfoAsync(IEnumerable<string> agentInfos, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Adds encoded agent infos (Holochain 0.7) to the conductor's peer store, e.g. from another conductor's GetAgentInfo. |
| `Task<AgentInfoAddedCallBackEventArgs> AddAgentInfoAsync(AgentInfo[] agentInfos, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Adds encoded agent infos (Holochain 0.7) to the conductor's peer store, e.g. from another conductor's GetAgentInfo. |
| `AppInterfaceAttachedCallBackEventArgs AttachAppInterface(ushort? port = null, string id = null)` | method | Instructs the conductor to open an app WebSocket interface on the given port (or a random port if `null`). The returned event args include the actual port. You then connect a `HoloNETClient` to that port to make zome calls. |
| `Task<AppInterfaceAttachedCallBackEventArgs> AttachAppInterfaceAsync(ushort? port = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Instructs the conductor to open an app WebSocket interface on the given port (or a random port if `null`). The returned event args include the actual port. You then connect a `HoloNETClient` to that port to make zome calls. |
| `ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(byte[][] cellId, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(byte[] AgentPubKey, byte[] DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(string AgentPubKey, string DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(byte[][] cellId, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(byte[] AgentPubKey, byte[] DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(string AgentPubKey, string DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<ValueTuple<string, string>> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = "")` | method | Generates a signing key pair and writes a zome-call capability grant into the conductor so the agent can sign zome calls. Call this once after `EnableAppAsync`. Pass `GrantedFunctions.All()` to grant all zomes/functions, or `GrantedFunctions.Listed(...)` to restrict to a specific set. |
| `HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `CloneCellDeletedCallBackEventArgs DeleteCloneCell(string appId, string id = null)` | method | Permanently removes a clone cell from a hApp. The role can be identified by role name, cell ID, or agent/DNA hash pair. |
| `CloneCellDeletedCallBackEventArgs DeleteCloneCell(string appId, byte[][] cellId, string id = null)` | method | Permanently removes a clone cell from a hApp. The role can be identified by role name, cell ID, or agent/DNA hash pair. |
| `CloneCellDeletedCallBackEventArgs DeleteCloneCell(string appId, string roleName, string id = null)` | method | Permanently removes a clone cell from a hApp. The role can be identified by role name, cell ID, or agent/DNA hash pair. |
| `CloneCellDeletedCallBackEventArgs DeleteCloneCell(string appId, string agentPubKey, string dnaHash, string id = null)` | method | Permanently removes a clone cell from a hApp. The role can be identified by role name, cell ID, or agent/DNA hash pair. |
| `Task<CloneCellDeletedCallBackEventArgs> DeleteCloneCellAsync(string appId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Permanently removes a clone cell from a hApp. The role can be identified by role name, cell ID, or agent/DNA hash pair. |
| `Task<CloneCellDeletedCallBackEventArgs> DeleteCloneCellAsync(string appId, byte[][] cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Permanently removes a clone cell from a hApp. The role can be identified by role name, cell ID, or agent/DNA hash pair. |
| `Task<CloneCellDeletedCallBackEventArgs> DeleteCloneCellAsync(string appId, string roleName, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Permanently removes a clone cell from a hApp. The role can be identified by role name, cell ID, or agent/DNA hash pair. |
| `Task<CloneCellDeletedCallBackEventArgs> DeleteCloneCellAsync(string appId, string agentPubKey, string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Permanently removes a clone cell from a hApp. The role can be identified by role name, cell ID, or agent/DNA hash pair. |
| `AppDisabledCallBackEventArgs DisableApp(string installedAppId, string id = null)` | method | Pauses a hApp without uninstalling it. Useful for maintenance or temporarily suspending an agent. |
| `Task<AppDisabledCallBackEventArgs> DisableAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Pauses a hApp without uninstalling it. Useful for maintenance or temporarily suspending an agent. |
| `FullStateDumpedCallBackEventArgs DumpFullState(int? dHTOpsCursor = null, string id = null)` | method | Like `DumpStateAsync` but includes full DHT operations. Supports paging via `dHTOpsCursor`. |
| `FullStateDumpedCallBackEventArgs DumpFullState(byte[][] cellId, int? dHTOpsCursor = null, string id = null)` | method | Like `DumpStateAsync` but includes full DHT operations. Supports paging via `dHTOpsCursor`. |
| `FullStateDumpedCallBackEventArgs DumpFullState(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, string id = null)` | method | Like `DumpStateAsync` but includes full DHT operations. Supports paging via `dHTOpsCursor`. |
| `Task<FullStateDumpedCallBackEventArgs> DumpFullStateAsync(int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Like `DumpStateAsync` but includes full DHT operations. Supports paging via `dHTOpsCursor`. |
| `Task<FullStateDumpedCallBackEventArgs> DumpFullStateAsync(byte[][] cellId, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Like `DumpStateAsync` but includes full DHT operations. Supports paging via `dHTOpsCursor`. |
| `Task<FullStateDumpedCallBackEventArgs> DumpFullStateAsync(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Like `DumpStateAsync` but includes full DHT operations. Supports paging via `dHTOpsCursor`. |
| `NetworkMetricsDumpedCallBackEventArgs DumpNetworkMetrics(string id = null)` | method | Returns detailed timing and throughput metrics from the networking layer. |
| `Task<NetworkMetricsDumpedCallBackEventArgs> DumpNetworkMetricsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns detailed timing and throughput metrics from the networking layer. |
| `NetworkStatsDumpedCallBackEventArgs DumpNetworkStats(string id = null)` | method | Returns JSON statistics from the Kitsune2 P2P networking layer: peer counts, bandwidth, gossip metrics. |
| `Task<NetworkStatsDumpedCallBackEventArgs> DumpNetworkStatsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns JSON statistics from the Kitsune2 P2P networking layer: peer counts, bandwidth, gossip metrics. |
| `StateDumpedCallBackEventArgs DumpState(int? dHTOpsCursor = null, string id = null)` | method | Dumps the source chain and DHT state for the given cell as JSON. Useful for debugging. |
| `StateDumpedCallBackEventArgs DumpState(byte[][] cellId, int? dHTOpsCursor = null, string id = null)` | method | Dumps the source chain and DHT state for the given cell as JSON. Useful for debugging. |
| `StateDumpedCallBackEventArgs DumpState(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, string id = null)` | method | Dumps the source chain and DHT state for the given cell as JSON. Useful for debugging. |
| `Task<StateDumpedCallBackEventArgs> DumpStateAsync(int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dumps the source chain and DHT state for the given cell as JSON. Useful for debugging. |
| `Task<StateDumpedCallBackEventArgs> DumpStateAsync(byte[][] cellId, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dumps the source chain and DHT state for the given cell as JSON. Useful for debugging. |
| `Task<StateDumpedCallBackEventArgs> DumpStateAsync(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dumps the source chain and DHT state for the given cell as JSON. Useful for debugging. |
| `Task<AppEnabledCallBackEventArgs> EnableAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Transitions the hApp from `Disabled` → `Enabled` (active). Must be called after `InstallAppAsync` before you can connect an app websocket or make zome calls. |
| `AppEnabledCallBackEventArgs EnablelApp(string installedAppId, string id = null)` | method | Enable an installed app (sync version of EnableAppAsync; the name keeps its original spelling for compatibility). |
| `void GenerateAgentPubKey(bool updateAgentPubKeyInIHoloNETDNA = true, string id = "")` | method | Asks the conductor to generate a new agent key pair (Ed25519). Returns the agent public key as both a byte array and a Base64 string. You need this before installing a hApp. |
| `Task<AgentPubKeyGeneratedCallBackEventArgs> GenerateAgentPubKeyAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, bool updateAgentPubKeyInIHoloNETDNA = true, string id = "")` | method | Asks the conductor to generate a new agent key pair (Ed25519). Returns the agent public key as both a byte array and a Base64 string. You need this before installing a hApp. |
| `AgentInfoReturnedCallBackEventArgs GetAgentInfo(string id = null)` | method | Returns gossip peer info for the given cell from the local node's perspective. |
| `AgentInfoReturnedCallBackEventArgs GetAgentInfo(byte[][] cellId, string id = null)` | method | Returns gossip peer info for the given cell from the local node's perspective. |
| `AgentInfoReturnedCallBackEventArgs GetAgentInfo(string agentPubKey, string dnaHash, string id = null)` | method | Returns gossip peer info for the given cell from the local node's perspective. |
| `Task<AgentInfoReturnedCallBackEventArgs> GetAgentInfoAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns gossip peer info for the given cell from the local node's perspective. |
| `Task<AgentInfoReturnedCallBackEventArgs> GetAgentInfoAsync(byte[][] cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns gossip peer info for the given cell from the local node's perspective. |
| `Task<AgentInfoReturnedCallBackEventArgs> GetAgentInfoAsync(string agentPubKey, string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns gossip peer info for the given cell from the local node's perspective. |
| `GetAppInfoCallBackEventArgs GetAppInfo(string installedAppId, string id = null)` | method | Returns the full `AppInfo` for the given hApp: status, cell IDs, agent pub key, manifest, and more. |
| `Task<GetAppInfoCallBackEventArgs> GetAppInfoAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the full `AppInfo` for the given hApp: status, cell IDs, agent pub key, manifest, and more. |
| `byte[] GetCapGrantSecret(byte[][] cellId)` | method | The capability secret HoloNET created for this cell's zome-call grant (null if it has none). |
| `byte[] GetCapGrantSecret(string agentPubKey, string dnaHash)` | method | The capability secret HoloNET created for this cell's zome-call grant (null if it has none). |
| `Task<byte[][]> GetCellIdAsync()` | method | The cell id zome calls go to: HoloNETDNA.CellId, or built from its DnaHash and AgentPubKey (retrieving them from the conductor first if needed). |
| `CompatibleCellsReturnedCallBackEventArgs GetCompatibleCells(byte[] dnaHash, string id = null)` | method | Returns cell IDs that share the same integrity DNA hash, indicating they can share data. |
| `Task<CompatibleCellsReturnedCallBackEventArgs> GetCompatibleCellsAsync(byte[] dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns cell IDs that share the same integrity DNA hash, indicating they can share data. |
| `DnaDefinitionReturnedCallBackEventArgs GetDnaDefinition(byte[] dnaHash, string id = null)` | method | Returns the full DNA definition (name, network seed, properties, integrity zomes, coordinator zomes) for the given DNA hash. |
| `DnaDefinitionReturnedCallBackEventArgs GetDnaDefinition(string dnaHash, string id = null)` | method | Returns the full DNA definition (name, network seed, properties, integrity zomes, coordinator zomes) for the given DNA hash. |
| `Task<DnaDefinitionReturnedCallBackEventArgs> GetDnaDefinitionAsync(byte[] dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the full DNA definition (name, network seed, properties, integrity zomes, coordinator zomes) for the given DNA hash. |
| `Task<DnaDefinitionReturnedCallBackEventArgs> GetDnaDefinitionAsync(string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the full DNA definition (name, network seed, properties, integrity zomes, coordinator zomes) for the given DNA hash. |
| `PeerMetaInfoReturnedCallBackEventArgs GetPeerMetaInfo(string url, List<byte[]> dnaHashes = null, string id = null)` | method | Returns the meta info (agent key, URLs, etc.) for a peer identified by its agent URL. |
| `Task<PeerMetaInfoReturnedCallBackEventArgs> GetPeerMetaInfoAsync(string url, List<byte[]> dnaHashes = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the meta info (agent key, URLs, etc.) for a peer identified by its agent URL. |
| `StorageInfoReturnedCallBackEventArgs GetStorageInfo(string id = null)` | method | Returns disk usage information for all cells: source chain size and DHT store size in bytes. |
| `Task<StorageInfoReturnedCallBackEventArgs> GetStorageInfoAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns disk usage information for all cells: source chain size and DHT store size in bytes. |
| `RecordsGraftedCallBackEventArgs GraftRecords(byte[][] cellId, bool validate, object[] records, string id = null)` | method | Inserts pre-existing source chain records into a cell, optionally running validation. Used for migrating or recovering data. |
| `Task<RecordsGraftedCallBackEventArgs> GraftRecordsAsync(byte[][] cellId, bool validate, object[] records, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Inserts pre-existing source chain records into a cell, optionally running validation. Used for migrating or recovering data. |
| `AppInstalledCallBackEventArgs InstallApp(string installedAppId, AppBundle appBundle, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, string id = null)` | method | Installs a hApp from a `.happ` file path or a pre-loaded `AppBundle` object. The `installedAppId` is the human-readable name you give the installation (e.g. `"my-app"`). The app is installed in the disabled state; call `EnableAppAsync` next. |
| `void InstallApp(string agentKey, string installedAppId, string hAppPath, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, string id = null)` | method | Installs a hApp from a `.happ` file path or a pre-loaded `AppBundle` object. The `installedAppId` is the human-readable name you give the installation (e.g. `"my-app"`). The app is installed in the disabled state; call `EnableAppAsync` next. |
| `Task<AppInstalledCallBackEventArgs> InstallAppAsync(string installedAppId, AppBundle appBundle, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Installs a hApp from a `.happ` file path or a pre-loaded `AppBundle` object. The `installedAppId` is the human-readable name you give the installation (e.g. `"my-app"`). The app is installed in the disabled state; call `EnableAppAsync` next. |
| `Task<AppInstalledCallBackEventArgs> InstallAppAsync(string installedAppId, string hAppPath, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Installs a hApp from a `.happ` file path or a pre-loaded `AppBundle` object. The `installedAppId` is the human-readable name you give the installation (e.g. `"my-app"`). The app is installed in the disabled state; call `EnableAppAsync` next. |
| `InstallEnableSignAndAttachHappEventArgs InstallEnableSignAndAttachHapp(string hAppId, string hAppInstallPath, string roleName, CapGrantAccessType capGrantAccessType = Assigned, GrantedFunctionsType grantedFunctionsType = All, List<ValueTuple<string, string>> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)` | method | Composite helper — performs in sequence: `GenerateAgentPubKey` (if no agentKey supplied) → `InstallApp` → `EnableApp` → `AuthorizeSigningCredentials` → `AttachAppInterface`. Returns a single `InstallEnableSignAndAttachHappEventArgs` with the app info, agent pub key, cell IDs, and the attached port. This is the recommended entry point for installing a new hApp. |
| `Task<InstallEnableSignAndAttachHappEventArgs> InstallEnableSignAndAttachHappAsync(string hAppId, string hAppInstallPath, string roleName, CapGrantAccessType capGrantAccessType = Assigned, GrantedFunctionsType grantedFunctionsType = All, List<ValueTuple<string, string>> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)` | method | Composite helper — performs in sequence: `GenerateAgentPubKey` (if no agentKey supplied) → `InstallApp` → `EnableApp` → `AuthorizeSigningCredentials` → `AttachAppInterface`. Returns a single `InstallEnableSignAndAttachHappEventArgs` with the app info, agent pub key, cell IDs, and the attached port. This is the recommended entry point for installing a new hApp. |
| `InstallEnableSignAttachAndConnectToHappEventArgs InstallEnableSignAttachAndConnectToHapp(string hAppId, string hAppInstallPath, string roleName, CapGrantAccessType capGrantAccessType = Assigned, GrantedFunctionsType grantedFunctionsType = All, List<ValueTuple<string, string>> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)` | method | Same as above but also creates and returns a connected `HoloNETClient` instance (app websocket) ready to make zome calls immediately. The most convenient way to go from a `.happ` file to fully connected and ready to call zomes. |
| `Task<InstallEnableSignAttachAndConnectToHappEventArgs> InstallEnableSignAttachAndConnectToHappAsync(string hAppId, string hAppInstallPath, string roleName, CapGrantAccessType capGrantAccessType = Assigned, GrantedFunctionsType grantedFunctionsType = All, List<ValueTuple<string, string>> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)` | method | Same as above but also creates and returns a connected `HoloNETClient` instance (app websocket) ready to make zome calls immediately. The most convenient way to go from a `.happ` file to fully connected and ready to call zomes. |
| `AppAuthenticationTokenIssuedCallBackEventArgs IssueAppAuthenticationToken(string installedAppId, ulong expirySeconds = 0, bool singleUse = false, string id = null)` | method | Issues a one-time authentication token that allows an app WebSocket client to connect to the app interface without knowing the admin credentials. Used when exposing hApps to end-user clients. |
| `Task<AppAuthenticationTokenIssuedCallBackEventArgs> IssueAppAuthenticationTokenAsync(string installedAppId, ulong expirySeconds = 0, bool singleUse = false, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Issues a one-time authentication token that allows an app WebSocket client to connect to the app interface without knowing the admin credentials. Used when exposing hApps to end-user clients. |
| `AppsListedCallBackEventArgs ListApps(AppStatusFilter appStatusFilter, string id = null)` | method | Returns all installed hApps matching the filter (`Running`, `Stopped`, `Disabled`, `Paused`, or `All`). |
| `Task<AppsListedCallBackEventArgs> ListAppsAsync(AppStatusFilter appStatusFilter, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns all installed hApps matching the filter (`Running`, `Stopped`, `Disabled`, `Paused`, or `All`). |
| `CapabilityGrantsListedCallBackEventArgs ListCapabilityGrants(string installedAppId, bool includeRevoked = false, string id = null)` | method | Lists all capability grants for a hApp, with an option to include revoked ones. |
| `Task<CapabilityGrantsListedCallBackEventArgs> ListCapabilityGrantsAsync(string installedAppId, bool includeRevoked = false, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Lists all capability grants for a hApp, with an option to include revoked ones. |
| `CellIdsListedCallBackEventArgs ListCellIds(string id = null)` | method | Returns all cell IDs (agentPubKey + dnaHash pairs) currently held by the conductor. |
| `Task<CellIdsListedCallBackEventArgs> ListCellIdsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns all cell IDs (agentPubKey + dnaHash pairs) currently held by the conductor. |
| `DnasListedCallBackEventArgs ListDnas(string id = null)` | method | Returns the hashes of all DNA bundles registered with the conductor. |
| `Task<DnasListedCallBackEventArgs> ListDnasAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the hashes of all DNA bundles registered with the conductor. |
| `AppInterfacesListedCallBackEventArgs ListInterfaces(string id = null)` | method | Returns all app WebSocket interface ports currently open on the conductor. |
| `Task<AppInterfacesListedCallBackEventArgs> ListInterfacesAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns all app WebSocket interface ports currently open on the conductor. |
| `DnaRegisteredCallBackEventArgs RegisterDna(byte[] hash, string network_seed = null, object properties = null, string id = null)` | method | Registers a DNA with the conductor from a `.dna` file path, hash, or pre-loaded bundle. Optionally provide a `network_seed` to create a separate DHT network for the same DNA (useful for testing isolation). |
| `DnaRegisteredCallBackEventArgs RegisterDna(DnaBundle bundle, string network_seed = null, object properties = null, string id = null)` | method | Registers a DNA with the conductor from a `.dna` file path, hash, or pre-loaded bundle. Optionally provide a `network_seed` to create a separate DHT network for the same DNA (useful for testing isolation). |
| `DnaRegisteredCallBackEventArgs RegisterDna(string path, string network_seed = null, object properties = null, string id = null)` | method | Registers a DNA with the conductor from a `.dna` file path, hash, or pre-loaded bundle. Optionally provide a `network_seed` to create a separate DHT network for the same DNA (useful for testing isolation). |
| `Task<DnaRegisteredCallBackEventArgs> RegisterDnaAsync(byte[] hash, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Registers a DNA with the conductor from a `.dna` file path, hash, or pre-loaded bundle. Optionally provide a `network_seed` to create a separate DHT network for the same DNA (useful for testing isolation). |
| `Task<DnaRegisteredCallBackEventArgs> RegisterDnaAsync(DnaBundle bundle, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Registers a DNA with the conductor from a `.dna` file path, hash, or pre-loaded bundle. Optionally provide a `network_seed` to create a separate DHT network for the same DNA (useful for testing isolation). |
| `Task<DnaRegisteredCallBackEventArgs> RegisterDnaAsync(string path, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Registers a DNA with the conductor from a `.dna` file path, hash, or pre-loaded bundle. Optionally provide a `network_seed` to create a separate DHT network for the same DNA (useful for testing isolation). |
| `AppAuthenticationTokenRevokedCallBackEventArgs RevokeAppAuthenticationToken(byte[] token, string id = null)` | method | Invalidates a previously issued app authentication token. |
| `Task<AppAuthenticationTokenRevokedCallBackEventArgs> RevokeAppAuthenticationTokenAsync(byte[] token, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Invalidates a previously issued app authentication token. |
| `ZomeCallCapabilityRevokedCallBackEventArgs RevokeZomeCallCapability(byte[] actionHash, CellId cellId, string id = null)` | method | Revokes a previously granted capability token identified by its action hash. |
| `Task<ZomeCallCapabilityRevokedCallBackEventArgs> RevokeZomeCallCapabilityAsync(byte[] actionHash, CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Revokes a previously granted capability token identified by its action hash. |
| `AppUninstalledCallBackEventArgs UninstallApp(string installedAppId, string id = null)` | method | Permanently removes a hApp installation. All cells are deleted; the DHT data remains on the network but the local agent is removed. |
| `Task<AppUninstalledCallBackEventArgs> UninstallAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Permanently removes a hApp installation. All cells are deleted; the DHT data remains on the network but the local agent is removed. |
| `CoordinatorsUpdatedCallBackEventArgs UpdateCoordinators(byte[] dnaHash, CoordinatorBundle bundle, string id = null)` | method | Hot-swaps coordinator zome bytecode without touching integrity zomes or DHT data. Enables live upgrades to hApp business logic. |
| `CoordinatorsUpdatedCallBackEventArgs UpdateCoordinators(byte[] dnaHash, string path, string id = null)` | method | Hot-swaps coordinator zome bytecode without touching integrity zomes or DHT data. Enables live upgrades to hApp business logic. |
| `CoordinatorsUpdatedCallBackEventArgs UpdateCoordinators(string dnaHash, CoordinatorBundle bundle, string id = null)` | method | Hot-swaps coordinator zome bytecode without touching integrity zomes or DHT data. Enables live upgrades to hApp business logic. |
| `CoordinatorsUpdatedCallBackEventArgs UpdateCoordinators(string dnaHash, string path, string id = null)` | method | Hot-swaps coordinator zome bytecode without touching integrity zomes or DHT data. Enables live upgrades to hApp business logic. |
| `Task<CoordinatorsUpdatedCallBackEventArgs> UpdateCoordinatorsAsync(byte[] dnaHash, CoordinatorBundle bundle, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Hot-swaps coordinator zome bytecode without touching integrity zomes or DHT data. Enables live upgrades to hApp business logic. |
| `Task<CoordinatorsUpdatedCallBackEventArgs> UpdateCoordinatorsAsync(byte[] dnaHash, string path, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Hot-swaps coordinator zome bytecode without touching integrity zomes or DHT data. Enables live upgrades to hApp business logic. |
| `Task<CoordinatorsUpdatedCallBackEventArgs> UpdateCoordinatorsAsync(string dnaHash, CoordinatorBundle bundle, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Hot-swaps coordinator zome bytecode without touching integrity zomes or DHT data. Enables live upgrades to hApp business logic. |
| `Task<CoordinatorsUpdatedCallBackEventArgs> UpdateCoordinatorsAsync(string dnaHash, string path, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Hot-swaps coordinator zome bytecode without touching integrity zomes or DHT data. Enables live upgrades to hApp business logic. |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iholonetclientappagent"></a>
### IHoloNETClientAppAgent (interface)

Inherits / implements: `IHoloNETClientAppBase`, `IHoloNETClientBase`

| Member | Kind | Description |
|---|---|---|
| `HoloNETConnectedEventArgs Connect(string installedAppId, string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `HoloNETConnectedEventArgs Connect(string installedAppId, Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string installedAppId, string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string installedAppId, Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iholonetclientappbase"></a>
### IHoloNETClientAppBase (interface)

Inherits / implements: `IHoloNETClientBase`

| Member | Kind | Description |
|---|---|---|
| `AppInfo CachedAppInfo { get; set; }` | property | The last app_info this client received (agent, DNA, cells, status). |
| `bool IsReadyForZomesCalls { get; }` | property | This property is a boolean and will return true when HoloNET is ready for zome calls after the OnReadyForZomeCalls event is raised. |
| `bool RetrievingAgentPubKeyAndDnaHash { get; }` | property | This property is a boolean and will return true when HoloNET is retrieving the AgentPubKey & DnaHash using one of the RetrieveAgentPubKeyAndDnaHash, RetrieveAgentPubKeyAndDnaHashFromSandbox or RetrieveAgentPubKeyAndDnaHashFromConductor methods (by default this will occur automatically after it has connected to the Holochain Conductor). |
| `IZomeCallSigner ZomeCallSigner { get; set; }` | property | Optional external zome-call signer; when unset, HoloNET signs with locally authorized credentials. |
| `HoloNETClientAppBase.AppInfoCallBack OnAppInfoCallBack` | event | Fired when the client receives AppInfo from the conductor containing the cell id for the running hApp (which in itself contains the AgentPubKey & DnaHash). It also contains the AppId and other info. |
| `HoloNETClientAppBase.AppPeerMetaInfoReturnedCallBack OnAppPeerMetaInfoReturnedCallBack` | event | Fired when a response is received from the conductor after the GetAppPeerMetaInfoAsync/GetAppPeerMetaInfo method is called. |
| `HoloNETClientAppBase.CloneCellCreatedCallBack OnCloneCellCreatedCallBack` | event | Fired when a clone cell has been created via CreateCloneCellAsync. |
| `HoloNETClientAppBase.CloneCellDisabledCallBack OnCloneCellDisabledCallBack` | event | Fired when a clone cell has been disabled via DisableCloneCellAsync. |
| `HoloNETClientAppBase.CloneCellEnabledCallBack OnCloneCellEnabledCallBack` | event | Fired when a clone cell has been re-enabled via EnableCloneCellAsync. |
| `HoloNETClientAppBase.CountersigningSessionAbandonedCallBack OnCountersigningSessionAbandonedCallBack` | event | Fired when a countersigning session is abandoned via AbandonCountersigningSessionAsync. |
| `HoloNETClientAppBase.CountersigningSessionStateReturnedCallBack OnCountersigningSessionStateReturnedCallBack` | event | Fired when countersigning session state is returned via GetCountersigningSessionStateAsync. |
| `HoloNETClientAppBase.MemproofsProvidedCallBack OnMemproofsProvidedCallBack` | event | Fired when membrane proofs have been submitted via ProvideMemproofsAsync. |
| `HoloNETClientAppBase.PublishCountersigningSessionTriggeredCallBack OnPublishCountersigningSessionTriggeredCallBack` | event | Fired when countersigning publication is triggered via PublishCountersigningSessionAsync. |
| `HoloNETClientAppBase.ReadyForZomeCalls OnReadyForZomeCalls` | event | Fired when the client has successfully connected and reteived the AgentPubKey & DnaHash, meaning it is ready to make zome calls to the Holochain conductor. |
| `HoloNETClientAppBase.SignalCallBack OnSignalCallBack` | event | Fired when the Holochain conductor sends signals data. |
| `HoloNETClientAppBase.SignalCallBack OnSystemSignalCallBack` | event | System signals from the conductor (successful_countersigning, abandoned_countersigning, restore_complete, ...). OnSignalCallBack only carries app signals; these were previously dropped entirely. |
| `HoloNETClientAppBase.WasmHostFunctionsListedCallBack OnWasmHostFunctionsListedCallBack` | event | Fired when the WASM host functions list has been returned via ListWasmHostFunctionsAsync. |
| `HoloNETClientAppBase.ZomeFunctionCallBack OnZomeFunctionCallBack` | event | Fired when the Holochain conductor returns the response from a zome function call. This returns the raw data as well as the parsed data returned from the zome function. It also returns the id, zome and zome function that made the call. |
| `CountersigningSessionAbandonedCallBackEventArgs AbandonCountersigningSession(CellId cellId, string id = null)` | method | Abandons (cancels) an in-progress countersigning session. |
| `Task<CountersigningSessionAbandonedCallBackEventArgs> AbandonCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Abandons (cancels) an in-progress countersigning session. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, object entryDataObjectReturnedFromZome = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, object entryDataObjectReturnedFromZome = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, object entryDataObjectReturnedFromZome = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse, CallZomeOptions callZomeOptions = null)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, object entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, HoloNETClientAppBase.ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = WaitForHolochainConductorResponse)` | method | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnZomeFunctionCallBack event once it has received a response from the Holochain conductor. |
| `void ClearCache(bool clearPendingRequsts = false)` | method | Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the CallZomeFunction methods if the `cacheData` param was set to true for any of the calls. |
| `HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string installedAppId, Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `CloneCellCreatedCallBackEventArgs CreateCloneCell(string roleName, object modifiers = null, byte[] membraneProof = null, string name = null, string id = null)` | method | Creates a new clone cell from an existing role's DNA with optional modifiers. |
| `Task<CloneCellCreatedCallBackEventArgs> CreateCloneCellAsync(string roleName, object modifiers = null, byte[] membraneProof = null, string name = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Creates a new clone cell from an existing role's DNA with optional modifiers. |
| `CloneCellDisabledCallBackEventArgs DisableCloneCell(object cloneCellId, string id = null)` | method | Disables a clone cell without deleting it. |
| `Task<CloneCellDisabledCallBackEventArgs> DisableCloneCellAsync(object cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Disables a clone cell without deleting it. |
| `AppOpTimingsDumpedCallBackEventArgs DumpOpTimings(byte[] dnaHash, OpTimingsCursor cursor = null, uint? limit = null, string id = null)` | method | Dump DHT op timing information for a DNA via the app interface (new in Holochain 0.7.0). |
| `Task<AppOpTimingsDumpedCallBackEventArgs> DumpOpTimingsAsync(byte[] dnaHash, OpTimingsCursor cursor = null, uint? limit = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Dump DHT op timing information for a DNA via the app interface (new in Holochain 0.7.0). The app must be running a cell of this DNA. Results are paginated; pass the cursor from a previous call to retrieve the next page. |
| `CloneCellEnabledCallBackEventArgs EnableCloneCell(object cloneCellId, string id = null)` | method | Re-enables a previously disabled clone cell. |
| `Task<CloneCellEnabledCallBackEventArgs> EnableCloneCellAsync(object cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Re-enables a previously disabled clone cell. |
| `AppInfoCallBackEventArgs GetAppInfo(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse)` | method | Gets AppInfo for the running hApp (role, cell ids, status). |
| `Task<AppInfoCallBackEventArgs> GetAppInfoAsync(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse)` | method | Gets AppInfo for the running hApp (role, cell ids, status). |
| `AppPeerMetaInfoReturnedCallBackEventArgs GetAppPeerMetaInfo(string url, List<byte[]> dnaHashes = null, string id = null)` | method | Returns peer meta info for a given agent URL within the app network. |
| `Task<AppPeerMetaInfoReturnedCallBackEventArgs> GetAppPeerMetaInfoAsync(string url, List<byte[]> dnaHashes = null, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns peer meta info for a given agent URL within the app network. |
| `Task<byte[][]> GetCellIdAsync()` | method | The cell id zome calls go to: HoloNETDNA.CellId, or built from its DnaHash and AgentPubKey (retrieving them from the conductor first if needed). |
| `CountersigningSessionStateReturnedCallBackEventArgs GetCountersigningSessionState(CellId cellId, string id = null)` | method | Returns the current state of an in-progress countersigning session. |
| `Task<CountersigningSessionStateReturnedCallBackEventArgs> GetCountersigningSessionStateAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the current state of an in-progress countersigning session. |
| `WasmHostFunctionsListedCallBackEventArgs ListWasmHostFunctions(string id = null)` | method | Returns the list of host functions exposed to WASM zomes by the conductor. |
| `Task<WasmHostFunctionsListedCallBackEventArgs> ListWasmHostFunctionsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Returns the list of host functions exposed to WASM zomes by the conductor. |
| `object MapEntryDataObject(object entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true)` | method | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass. |
| `object MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true)` | method | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass. |
| `Task<object> MapEntryDataObjectAsync(object entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true)` | method | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass. |
| `Task<object> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true)` | method | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass. |
| `MemproofsProvidedCallBackEventArgs ProvideMemproofs(Dictionary<string, byte[]> membraneProofs, string id = null)` | method | Submits membrane proofs to the conductor for gated DNA networks. |
| `Task<MemproofsProvidedCallBackEventArgs> ProvideMemproofsAsync(Dictionary<string, byte[]> membraneProofs, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Submits membrane proofs to the conductor for gated DNA networks. |
| `PublishCountersigningSessionTriggeredCallBackEventArgs PublishCountersigningSession(CellId cellId, string id = null)` | method | Triggers publication of a completed countersigning session to the DHT. |
| `Task<PublishCountersigningSessionTriggeredCallBackEventArgs> PublishCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = WaitForHolochainConductorResponse, string id = null)` | method | Triggers publication of a completed countersigning session to the DHT. |
| `AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHash(string installedAppId = null, string roleName = null, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method will retrieve the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retrieving from the Conductor first. It will call RetrieveAgentPubKeyAndDnaHashFromConductor and RetrieveAgentPubKeyAndDnaHashFromSandboxAsync internally. |
| `Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashAsync(string installedAppId = null, string roleName = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method will retrieve the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retrieving from the Conductor first. It will call RetrieveAgentPubKeyAndDnaHashFromConductor and RetrieveAgentPubKeyAndDnaHashFromSandboxAsync internally. |
| `AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromConductor(string installedAppId = null, string roleName = null, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true)` | method | This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retrieved them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retrieve the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetrieveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the \|retrieveAgentPubKeyAndDnaHashFromSandbox method. |
| `Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromConductorAsync(string installedAppId = null, string roleName = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true)` | method | This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retrieved them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retrieve the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetrieveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the \|retrieveAgentPubKeyAndDnaHashFromSandbox method. |
| `AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromSandbox(bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)` | method | This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the OnReadyForZomeCalls event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the \|retrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default). |
| `Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)` | method | This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the OnReadyForZomeCalls event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the \|retrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default). |
| `Task<ReadyForZomeCallsEventArgs> WaitTillReadyForZomeCallsAsync()` | method | This method will wait (non blocking) until HoloNET is ready to make zome calls after it has connected to the Holochain Conductor and retrived the AgentPubKey & DnaHash. It will then return to the caller with the AgentPubKey & DnaHash. This method will return the same time the OnReadyForZomeCalls event is raised. Unlike all the other methods, this one only contains an async version because the non async version would block all other threads including any UI ones etc. |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iholonetclientbase"></a>
### IHoloNETClientBase (interface)

| Member | Kind | Description |
|---|---|---|
| `Uri EndPoint { get; set; }` | property | The conductor WebSocket endpoint this came from. |
| `IHoloNETDNA HoloNETDNA { get; set; }` | property | The HoloNET settings in use (see HoloNETDNA). |
| `bool IsConnecting { get; set; }` | property | True while ConnectAsync is in progress. |
| `bool IsDisconnecting { get; set; }` | property | True while DisconnectAsync is in progress. |
| `bool IsHoloNETDNALoaded { get; }` | property | True once the settings have been loaded (or set). |
| `ILogger Logger { get; set; }` | property | The logger HoloNET writes to (the built-in DefaultLogger unless you pass your own log providers or logger). |
| `WebSocketState State { get; }` | property | This property is a shortcut to the State property on the WebSocket property above. |
| `WebSocket WebSocket { get; set; }` | property | This property contains the internal NextGenSoftware WebSocket that HoloNET uses. You can use this property to access the current state of the WebSocket as well as configure more options. |
| `HoloNETClientBase.Connected OnConnected` | event | Fired when the client has successfully connected to the Holochain conductor. |
| `HoloNETClientBase.DataReceived OnDataReceived` | event | Fired when any data is received from the Holochain conductor. This returns the raw data. |
| `HoloNETClientBase.DataSent OnDataSent` | event | Fired when data is sent to the Holochain conductor over the WebSocket. |
| `HoloNETClientBase.Disconnected OnDisconnected` | event | Fired when the client disconnected from the Holochain conductor. |
| `HoloNETClientBase.Error OnError` | event | Fired when an error occurs, check the params for the cause of the error. |
| `HoloNETClientBase.HolochainConductorsShutdownComplete OnHolochainConductorsShutdownComplete` | event | Fired when all Holochain Conductors have been shutdown. |
| `HoloNETClientBase.HolochainConductorStarted OnHolochainConductorStarted` | event | Fired when the Holochain Conductor process has successfully started (Embedded mode). |
| `HoloNETClientBase.HolochainConductorStarting OnHolochainConductorStarting` | event | Fired just before HoloNET starts the Holochain Conductor process (Embedded mode). |
| `HoloNETClientBase.HoloNETShutdownComplete OnHoloNETShutdownComplete` | event | Fired when HoloNET has completed shutting down (this includes closing all connections and shutting down all Holochain Conductor). |
| `void ClearCache(bool clearPendingRequsts = false)` | method | Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the CallZomeFunction methods if the `cacheData` param was set to true for any of the calls. |
| `HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `Task<HoloNETConnectedEventArgs> ConnectAsync(Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateIHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)` | method | This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting. |
| `byte[] ConvertHoloHashToBytes(string hash)` | method | Utility method to convert a string to base64 encoded bytes (Holochain Conductor format). This is used to convert the AgentPubKey & DnaHash when making a zome call. |
| `string ConvertHoloHashToString(byte[] bytes)` | method | Utility method to convert from base64 bytes (Holochain Conductor format) to a friendly C# format. This is used to convert the AgentPubKey & DnaHash retrieved from the Conductor. |
| `HoloNETDisconnectedEventArgs Disconnect(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected once it is disconnected. |
| `Task<HoloNETDisconnectedEventArgs> DisconnectAsync(DisconnectedCallBackMode disconnectedCallBackMode = WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected once it is disconnected. |
| `byte[][] GetCellId()` | method | Build a cell id from a DNA hash and an agent public key (both as HoloHash strings). |
| `byte[][] GetCellId(byte[] DnaHash, byte[] AgentPubKey)` | method | Build a cell id from a DNA hash and an agent public key (both as HoloHash strings). |
| `byte[][] GetCellId(string DnaHash, string AgentPubKey)` | method | Build a cell id from a DNA hash and an agent public key (both as HoloHash strings). |
| `Task<byte[][]> GetCellIdAsync()` | method | The cell id zome calls go to: HoloNETDNA.CellId, or built from its DnaHash and AgentPubKey (retrieving them from the conductor first if needed). |
| `IHoloNETDNA LoadDNA()` | method | Load the settings from HoloNETDNA.json (defaults if it doesn't exist). |
| `bool SaveDNA()` | method | Save the settings to HoloNETDNA.json. |
| `void SendHoloNETRequest(byte[] data, HoloNETRequestType requestType, string id = "")` | method | This method allows you to send your own raw request to holochain. This method raises the OnDataReceived event once it has received a response from the Holochain conductor. Please see the Events section above for more info on how to use this event. You would rarely need to use this and we highly recommend you use the CallZomeFunctionAsync method instead. You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller. |
| `void SendHoloNETRequest(HoloNETData holoNETData, HoloNETRequestType requestType, string id = "")` | method | This method allows you to send your own raw request to holochain. This method raises the OnDataReceived event once it has received a response from the Holochain conductor. Please see the Events section above for more info on how to use this event. You would rarely need to use this and we highly recommend you use the CallZomeFunctionAsync method instead. You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller. |
| `Task SendHoloNETRequestAsync(byte[] data, HoloNETRequestType requestType, string id = "")` | method | This method allows you to send your own raw request to holochain. This method raises the OnDataReceived event once it has received a response from the Holochain conductor. Please see the Events section above for more info on how to use this event. You would rarely need to use this and we highly recommend you use the CallZomeFunctionAsync method instead. You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller. |
| `Task SendHoloNETRequestAsync(HoloNETData holoNETData, HoloNETRequestType requestType, string id = "")` | method | This method allows you to send your own raw request to holochain. This method raises the OnDataReceived event once it has received a response from the Holochain conductor. Please see the Events section above for more info on how to use this event. You would rarely need to use this and we highly recommend you use the CallZomeFunctionAsync method instead. You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller. |
| `HolochainConductorsShutdownEventArgs ShutDownHolochainConductors(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | Will automatically shutdown the current Holochain Conductor (if the `shutdownHolochainConductorsMode` param is set to `ShutdownCurrentConductorOnly`) or all Holochain Conductors (if the `shutdownHolochainConductorsMode` param is set to `ShutdownAllConductors`). If the `shutdownHolochainConductorsMode` param is set to `UseConfigSettings` then it will use the `HoloNETClient.Config.AutoShutdownHolochainConductor` and `HoloNETClient.Config.ShutDownALLHolochainConductors` flags to determine which mode to use. The Disconnect method will automatically call this once it has finished disconnecting from the Holochain Conductor. The ShutdownHoloNET will also call this method. |
| `Task<HolochainConductorsShutdownEventArgs> ShutDownHolochainConductorsAsync(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | Will automatically shutdown the current Holochain Conductor (if the `shutdownHolochainConductorsMode` param is set to `ShutdownCurrentConductorOnly`) or all Holochain Conductors (if the `shutdownHolochainConductorsMode` param is set to `ShutdownAllConductors`). If the `shutdownHolochainConductorsMode` param is set to `UseConfigSettings` then it will use the `HoloNETClient.Config.AutoShutdownHolochainConductor` and `HoloNETClient.Config.ShutDownALLHolochainConductors` flags to determine which mode to use. The Disconnect method will automatically call this once it has finished disconnecting from the Holochain Conductor. The ShutdownHoloNET will also call this method. |
| `HoloNETShutdownEventArgs ShutdownHoloNET(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | This method will shutdown HoloNET by first calling the Disconnect method to disconnect from the Holochain Conductor and then calling the ShutDownHolochainConductors method to shutdown any running Holochain Conductors. This method will then raise the OnHoloNETShutdown event. |
| `Task<HoloNETShutdownEventArgs> ShutdownHoloNETAsync(DisconnectedCallBackMode disconnectedCallBackMode = WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = UseHoloNETDNASettings)` | method | This method will shutdown HoloNET by first calling the Disconnect method to disconnect from the Holochain Conductor and then calling the ShutDownHolochainConductors method to shutdown any running Holochain Conductors. This method will then raise the OnHoloNETShutdown event. |
| `void StartHolochainConductor()` | method | This method will start the Holochain Conducutor using the appropriate settings defined in the HoloNETDNA property. |
| `Task StartHolochainConductorAsync()` | method | This method will start the Holochain Conducutor using the appropriate settings defined in the HoloNETDNA property. |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iholonetdata"></a>
### IHoloNETData (interface)

| Member | Kind | Description |
|---|---|---|
| `object data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iholonetdna"></a>
### IHoloNETDNA (interface)

| Member | Kind | Description |
|---|---|---|
| `string AgentPubKey { get; set; }` | property | The agent public key (HoloHash string) this applies to. |
| `byte[] AppAuthenticationToken { get; set; }` | property | Holochain 0.7 app-interface authentication token. When supplied, app clients send the mandatory authenticate envelope immediately after the WebSocket opens and before any conductor request. Admin clients never use this value. |
| `bool AutoShutdownHolochainConductor { get; set; }` | property | Set this to true if you wish HoloNET to auto-shutdown the Holochain Conductor after it disconnects. Default is true. |
| `bool AutoStartHolochainConductor { get; set; }` | property | Set this to true if you with HoloNET to auto-start the Holochain Conductor defined in the `FullPathToExternalHolochainConductorBinary parameter if `HolochainConductorToUse` is `Holochain`, otherwise if it`s `Hc` then it will use `FullPathToExternalHCToolBinary`. Default is true. |
| `CacheConfig CacheConfig { get; set; }` | property | HoloNET client-side cache configuration (HoloNET-specific concept, not a Rust mirror). Not yet consumed by any caching implementation in HoloNET. |
| `byte[][] CellId { get; set; }` | property | The cell id: [DNA hash, agent public key] as raw bytes. |
| `LoggingMode ConsoleLoggingMode { get; set; }` | property | This passes through to the static LogConfig.ConsoleLoggingMode property in [NextGenSoftware.Logging](https://www.nuget.org/packages/NextGenSoftware.Logging) package. It can be either `WarningsErrorsInfoAndDebug`, `WarningsErrorsAndInfo`, `WarningsAndErrors` or `ErrorsOnly`. |
| `ConsoleColor DebugColour { get; set; }` | property | The colour to use for `Debug` log entries to the console. |
| `string DnaHash { get; set; }` | property | The DNA hash (HoloHash string) this applies to. |
| `EmbeddedHolochainVersion EmbeddedHolochainVersion { get; set; }` | property | Which bundled Holochain release to start when `HolochainConductorMode` is `UseEmbedded` (Embedded package only). Defaults to 0.7.1; set V0_7_0 for hApps built against hdk 0.7.0. |
| `EnforceRequestToResponseIdMatchingBehaviour EnforceRequestToResponseIdMatchingBehaviour { get; set; }` | property | Controls what happens when a conductor response ID doesn't match a pending request. Options: AlwaysError, WarnOnly (default), Ignore. |
| `ConsoleColor ErrorColour { get; set; }` | property | The colour to use for `Error` log entries to the console. |
| `ErrorHandlingBehaviour ErrorHandlingBehaviour { get; set; }` | property | Whether errors only raise OnError (the default) or also throw. |
| `LoggingMode FileLoggingMode { get; set; }` | property | This passes through to the static LogConfig.FileLoggingMode property in [NextGenSoftware.Logging](https://www.nuget.org/packages/NextGenSoftware.Logging) package. It can be either `WarningsErrorsInfoAndDebug`, `WarningsErrorsAndInfo`, `WarningsAndErrors` or `ErrorsOnly`. |
| `string FullPathToCompiledHappFolder { get; set; }` | property | The full path to the compiled hApp that HoloNET will start the Holochain Conductor (currenty uses hc.exe) with and then make zome calls to. |
| `string FullPathToExternalHCToolBinary { get; set; }` | property | The full path to the Holochain Conductor exe (hc.exe) that HoloNET will auto-start if `HolochainConductorToUse` is set to `Hc`. |
| `string FullPathToExternalHolochainConductorBinary { get; set; }` | property | The full path to the Holochain Conductor exe (holochain.exe) that HoloNET will auto-start if `HolochainConductorToUse` is set to `Holochain`. |
| `string FullPathToRootHappFolder { get; set; }` | property | The full path to the root of the hApp that HoloNET will start the Holochain Conductor (currenty uses hc.exe) with and then make zome calls to. |
| `string HolochainConductorAdminURI { get; set; }` | property | The URI for the Admin WebSocket interface on the Holochain Conductor. Defaults to ws://localhost:8000. Used by HoloNETClientAdmin. |
| `string HolochainConductorAppAgentURI { get; set; }` | property | The URI for the App Agent WebSocket interface on the Holochain Conductor. Defaults to ws://localhost:8888. Used by HoloNETClient for zome calls. |
| `string HolochainConductorConfigPath { get; set; }` | property | Path to the Holochain Conductor config file. Defaults to AppData\Roaming\holochain\. Used when auto-starting the conductor. |
| `HolochainConductorModeEnum HolochainConductorMode { get; set; }` | property | Tells HoloNET how to auto-start the Holochain Conductor. It can be one of the following values: `UseExternal` - Will use the hc.exe specified in the `FullPathToExternalHCToolBinary` property if `HolochainConductorToUse` property is set to `Hc`. It will use the holochain.exe specified in the `FullPathToExternalHolochainConductorBinary` property if `HolochainConductorToUse` property is set to `Holochain`. If `HolochainConductorMode` is set to `UseEmbedded` then it will use the embdedded/integrated hc.exe/holochain.exe if the app is using the NextGenSoftware.Holochain.HoloNET.Client.Embedded package, otherwise it will throw an exception. Finally, if `HolochainConductorMode` is set to `UseSystemGlobal` (default), then it will automatically use the installed version of hc.exe & holochain.exe on the target machine. |
| `HolochainConductorEnum HolochainConductorToUse { get; set; }` | property | This is the Holochain Conductor to use for the auto-start Holochain Conductor feature. It can be either `Holochain` or `Hc`. |
| `int IndentLogMessagesBy { get; set; }` | property | The amount of space to indent the log message by. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `ConsoleColor InfoColour { get; set; }` | property | The colour to use for `Info` log entries to the console. |
| `bool InsertExtraNewLineAfterLogMessage { get; set; }` | property | Set this to true to add additional space after the end of each log entry. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `string InstalledAppId { get; set; }` | property | The installed app id. |
| `KeystoreConfig KeystoreConfig { get; set; }` | property | Keystore connection configuration, mirroring holochain_conductor_api's KeystoreConfig. Not yet wired through to actual conductor config generation/admin API - out of scope here. |
| `Kitsune2Config Kitsune2Config { get; set; }` | property | Kitsune2 networking sub-fields of ConductorConfig.network not already covered by NetworkConfig (bootstrap/relay URLs, target_arc_factor, advanced kitsune2 JSON; signal_url and webrtc_config removed in Holochain 0.7.0). Not yet wired through to any websocket/conductor call. |
| `string LogFileName { get; set; }` | property | The name of the log file to create. |
| `string LogPath { get; set; }` | property | The logging path (will defualt to AppData\Roaming\NextGenSoftware\HoloNET\Logs). NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `bool LogToConsole { get; set; }` | property | Set this to true to log to the console. The default is true. |
| `bool LogToFile { get; set; }` | property | Set this to true to log to the file. The default is true. |
| `int MaxLogFileSize { get; set; }` | property | This is the max file size the log file can be (in bytes) before it creates a new file. The default is 1000000 bytes (1 MB). |
| `NetworkConfig NetworkConfig { get; set; }` | property | Network configuration (Holochain 0.7.0 ConductorConfig.network). Currently used by HoloNET to set the default RequestTimeoutS used for zome calls / websocket requests. |
| `int NumberOfRetriesToLogToFile { get; set; }` | property | The number of attempts to attempt to log to the file if the first attempt fails. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `bool OnlyAllowOneHolochainConductorToRunAtATime { get; set; }` | property | Set this to true if you wish HoloNET to allow only ONE Holochain Conductor to run at a time. The default is false. |
| `QUICConfig QUICConfig { get; set; }` | property | QUIC transport configuration. NOT VERIFIED against any real Holochain conductor-facing config - kept as a placeholder only; has no effect on conductor behaviour. |
| `int RetryLoggingToFileEverySeconds { get; set; }` | property | The amount of time to wait in seconds between each attempt to log to the file. NOTE: This is only relevant if the built-in DefaultLogger is used. |
| `int SecondsToWaitForHolochainConductorToStart { get; set; }` | property | The seconds to wait for the Holochain Conductor to start before attempting to connect to it. |
| `bool ShowColouredLogs { get; set; }` | property | Set this to true to enable coloured logs in the console. This default to true. |
| `bool ShowHolochainConductorWindow { get; set; }` | property | Set this to true if you wish HoloNET to show the Holochain Conductor window whilst it is starting it (will be left open until the conductor is automatically shutdown again when HoloNET disconects if `AutoShutdownHolochainConductor` is true.) |
| `bool ShutDownALLHolochainConductors { get; set; }` | property | Set this to true if you wish HoloNET to auto-shutdown ALL Holochain Conductors after it disconnects. Default is false. Set this to true if you wish to make sure there are none left running to prevent memory leaks. You can also of course manually call the ShutDownAllConductors if you wish. |
| `ConsoleColor WarningColour { get; set; }` | property | The colour to use for `Warning` log entries to the console. |
| `WASMConfig WASMConfig { get; set; }` | property | WASM-related configuration. NOT VERIFIED against any real Holochain conductor-facing config - kept as a placeholder only; has no effect on conductor behaviour. |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iholonetrequest"></a>
### IHoloNETRequest (interface)

| Member | Kind | Description |
|---|---|---|
| `byte[] data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `ulong id { get; set; }` | property | Holochain's `id` field (sent to / received from the conductor as-is). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-iholonetresponse"></a>
### IHoloNETResponse (interface)

| Member | Kind | Description |
|---|---|---|
| `byte[] data { get; set; }` | property | Holochain's `data` field (sent to / received from the conductor as-is). |
| `HoloNETResponseType HoloNETResponseType { get; set; }` | property | Which kind of message this is (zome call response, signal, admin response...). |
| `ulong id { get; set; }` | property | Holochain's `id` field (sent to / received from the conductor as-is). |
| `bool IsError { get; set; }` | property | True when the call or the conductor reported an error (see Message). |
| `string type { get; set; }` | property | Holochain's `type` field (sent to / received from the conductor as-is). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-irecord"></a>
### IRecord (interface)

| Member | Kind | Description |
|---|---|---|
| `string ActionHash { get; set; }` | property | The ActionHash |
| `int ActionSequence { get; set; }` | property | The Action Sequence for this entry. |
| `string Author { get; set; }` | property | The author of the entry (AgentPubKey on their device). |
| `byte[] Bytes { get; set; }` | property | The raw bytes returned from the Holochain Conductor for the entry. |
| `string BytesString { get; set; }` | property | The raw bytes returned from the Holochain Conductor for the entry formatted as a string making logging etc easier. |
| `DateTime DateTime { get; set; }` | property | Standard .NET DateTime converted from the Unix timestamp (returned from the Holochain Conductor). |
| `object EntryDataObject { get; set; }` | property | A dynamic object constructed from the entry data. |
| `string EntryHash { get; set; }` | property | This is the hash generated by the Holochain Conductor for the Holochain Entry that the HoloNETEntryBaseClass maps onto. This will be empty until after the first call to the Save method. |
| `Dictionary<string, object> EntryKeyValuePairs { get; set; }` | property | A key/value pair/dictionary containing the entry data itself. |
| `string EntryType { get; set; }` | property | //App/Not sure what other types there are? |
| `string OriginalActionAddress { get; set; }` | property | Is the original Hash for the entry. |
| `string OriginalEntryAddress { get; set; }` | property | Is the original EntryHash for the entry. |
| `string PreviousActionHash { get; set; }` | property | The previous ActionHash. |
| `string Signature { get; set; }` | property | The signature of the entry. |
| `long Timestamp { get; set; }` | property | The Unix timestamp (returned from the Holochain Conductor) |
| `string Type { get; set; }` | property | The conductor message type. |

<a id="nextgensoftware-holochain-holonet-client-interfaces-isignaldata"></a>
### ISignalData (interface)

| Member | Kind | Description |
|---|---|---|
| `byte[][] CellData { get; set; }` | property | The cell id the signal came from ([DNA hash, agent public key]). |
| `byte[] Data { get; set; }` | property | The signal payload as MessagePack bytes (what the zome passed to emit_signal). |

<a id="nextgensoftware-holochain-holonet-client-interfaces-istorageinforesponse"></a>
### IStorageInfoResponse (interface)

| Member | Kind | Description |
|---|---|---|
| `DnaStorageBlob[] blobs { get; set; }` | property | Holochain's `blobs` field (sent to / received from the conductor as-is). |

