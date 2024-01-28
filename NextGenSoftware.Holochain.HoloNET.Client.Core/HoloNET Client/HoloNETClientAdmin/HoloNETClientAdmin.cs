using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public partial class HoloNETClientAdmin : HoloNETClientBase//, IHoloNETClientAdmin
    {
        private Dictionary<string, string> _disablingAppLookup = new Dictionary<string, string>();
        private Dictionary<string, string> _uninstallingAppLookup = new Dictionary<string, string>();
        private Dictionary<string, TaskCompletionSource<AgentPubKeyGeneratedCallBackEventArgs>> _taskCompletionAgentPubKeyGeneratedCallBack = new Dictionary<string, TaskCompletionSource<AgentPubKeyGeneratedCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<AppInstalledCallBackEventArgs>> _taskCompletionAppInstalledCallBack = new Dictionary<string, TaskCompletionSource<AppInstalledCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<AppUninstalledCallBackEventArgs>> _taskCompletionAppUninstalledCallBack = new Dictionary<string, TaskCompletionSource<AppUninstalledCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<AppEnabledCallBackEventArgs>> _taskCompletionAppEnabledCallBack = new Dictionary<string, TaskCompletionSource<AppEnabledCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<AppDisabledCallBackEventArgs>> _taskCompletionAppDisabledCallBack = new Dictionary<string, TaskCompletionSource<AppDisabledCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<ZomeCallCapabilityGrantedCallBackEventArgs>> _taskCompletionZomeCapabilityGrantedCallBack = new Dictionary<string, TaskCompletionSource<ZomeCallCapabilityGrantedCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<AppInterfaceAttachedCallBackEventArgs>> _taskCompletionAppInterfaceAttachedCallBack = new Dictionary<string, TaskCompletionSource<AppInterfaceAttachedCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<DnaRegisteredCallBackEventArgs>> _taskCompletionDnaRegisteredCallBack = new Dictionary<string, TaskCompletionSource<DnaRegisteredCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<AppsListedCallBackEventArgs>> _taskCompletionAppsListedCallBack = new Dictionary<string, TaskCompletionSource<AppsListedCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<DnasListedCallBackEventArgs>> _taskCompletionDnasListedCallBack = new Dictionary<string, TaskCompletionSource<DnasListedCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<ListCellIdsCallBackEventArgs>> _taskCompletionListCellIdsCallBack = new Dictionary<string, TaskCompletionSource<ListCellIdsCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<ListAppInterfacesCallBackEventArgs>> _taskCompletionListAppInterfacesCallBack = new Dictionary<string, TaskCompletionSource<ListAppInterfacesCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<DumpFullStateCallBackEventArgs>> _taskCompletionDumpFullStateCallBack = new Dictionary<string, TaskCompletionSource<DumpFullStateCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<DumpStateCallBackEventArgs>> _taskCompletionDumpStateCallBack = new Dictionary<string, TaskCompletionSource<DumpStateCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<GetDnaDefinitionCallBackEventArgs>> _taskCompletionGetDnaDefinitionCallBack = new Dictionary<string, TaskCompletionSource<GetDnaDefinitionCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<UpdateCoordinatorsCallBackEventArgs>> _taskCompletionUpdateCoordinatorsCallBack = new Dictionary<string, TaskCompletionSource<UpdateCoordinatorsCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<GetAgentInfoCallBackEventArgs>> _taskCompletionGetAgentInfoCallBack = new Dictionary<string, TaskCompletionSource<GetAgentInfoCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<AddAgentInfoCallBackEventArgs>> _taskCompletionAddAgentInfoCallBack = new Dictionary<string, TaskCompletionSource<AddAgentInfoCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<DeleteCloneCellCallBackEventArgs>> _taskCompletionDeleteCloneCellCallBack = new Dictionary<string, TaskCompletionSource<DeleteCloneCellCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<GetStorageInfoCallBackEventArgs>> _taskCompletionGetStorageInfoCallBack = new Dictionary<string, TaskCompletionSource<GetStorageInfoCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<DumpNetworkStatsCallBackEventArgs>> _taskCompletionDumpNetworkStatsCallBack = new Dictionary<string, TaskCompletionSource<DumpNetworkStatsCallBackEventArgs>>();

        //Events

        public delegate void InstallEnableSignAttachAndConnectToHappCallBack(object sender, InstallEnableSignAttachAndConnectToHappEventArgs e);

        /// <summary>
        /// Fired when the hApp has been installed, enabled, signed and attached.
        /// </summary>
        public event InstallEnableSignAttachAndConnectToHappCallBack OnInstallEnableSignAttachAndConnectToHappCallBack;


        public delegate void InstallEnableSignAndAttachHappCallBack(object sender, InstallEnableSignAndAttachHappEventArgs e);

        /// <summary>
        /// Fired when the hApp has been installed, enabled, signed and attached.
        /// </summary>
        public event InstallEnableSignAndAttachHappCallBack OnInstallEnableSignAndAttachHappCallBack;




        public delegate void AgentPubKeyGeneratedCallBack(object sender, AgentPubKeyGeneratedCallBackEventArgs e);

        /// <summary>
        /// Fired when the client receives the generated AgentPubKey from the conductor.
        /// </summary>
        public event AgentPubKeyGeneratedCallBack OnAgentPubKeyGeneratedCallBack;



        public delegate void AppInstalledCallBack(object sender, AppInstalledCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after a hApp has been installed via the InstallAppAsyc/InstallApp method.
        /// </summary>
        public event AppInstalledCallBack OnAppInstalledCallBack;


        public delegate void AppUninstalledCallBack(object sender, AppUninstalledCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after a hApp has been uninstalled via the UninstallAppAsyc/UninstallApp method.
        /// </summary>
        public event AppUninstalledCallBack OnAppUninstalledCallBack;


        public delegate void AppEnabledCallBack(object sender, AppEnabledCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after a hApp has been enabled via the EnableAppAsyc/EnableApp method.
        /// </summary>
        public event AppEnabledCallBack OnAppEnabledCallBack;


        public delegate void AppDisabledCallBack(object sender, AppDisabledCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after a hApp has been disabled via the DisableAppAsyc/DisableApp method.
        /// </summary>
        public event AppDisabledCallBack OnAppDisabledCallBack;


        public delegate void ZomeCallCapabilityGrantedCallBack(object sender, ZomeCallCapabilityGrantedCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after a cell has been authorized via the AuthorizeSigningCredentialsForZomeCallsAsync/AuthorizeSigningCredentialsForZomeCalls method.
        /// </summary>
        public event ZomeCallCapabilityGrantedCallBack OnZomeCallCapabilityGrantedCallBack;


        public delegate void AppInterfaceAttachedCallBack(object sender, AppInterfaceAttachedCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the app interface has been attached via the AttachAppInterfaceAsyc/AttachAppInterface method.
        /// </summary>
        public event AppInterfaceAttachedCallBack OnAppInterfaceAttachedCallBack;


        public delegate void DnaRegisteredCallBack(object sender, DnaRegisteredCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the dna has been registered via the RegisterDnaAsync/RegisterDna method.
        /// </summary>
        public event DnaRegisteredCallBack OnDnaRegisteredCallBack;


        public delegate void AppsListedCallBack(object sender, AppsListedCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the ListAppsAsync/ListApps method is called.
        /// </summary>
        public event AppsListedCallBack OnAppsListedCallBack;


        public delegate void DnasListedCallBack(object sender, DnasListedCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the ListDnasAsync/ListDnas method is called.
        /// </summary>
        public event DnasListedCallBack OnDnasListedCallBack;


        public delegate void ListCellIdsCallBack(object sender, ListCellIdsCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the ListCellIdsAsync/ListCellIds method is called.
        /// </summary>
        public event ListCellIdsCallBack OnListCellIdsCallBack;


        public delegate void ListAppInterfacesCallBack(object sender, ListAppInterfacesCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the ListAppInterfacesAsync/ListAppInterfaces method is called.
        /// </summary>
        public event ListAppInterfacesCallBack OnListAppInterfacesCallBack;


        public delegate void DumpFullStateCallBack(object sender, DumpFullStateCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the DumpFullStateAsync/DumpFullState method is called.
        /// </summary>
        public event DumpFullStateCallBack OnDumpFullStateCallBack;


        public delegate void DumpStateCallBack(object sender, DumpStateCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the DumpStateAsync/DumpState method is called.
        /// </summary>
        public event DumpStateCallBack OnDumpStateCallBack;


        public delegate void GetDnaDefinitionCallBack(object sender, GetDnaDefinitionCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor containing the DNA Definition for a DNA Hash after the GetDnaDefinitionAsync/GetDnaDefinition method is called.
        /// </summary>
        public event GetDnaDefinitionCallBack OnGetDnaDefinitionCallBack;


        public delegate void UpdateCoordinatorsCallBack(object sender, UpdateCoordinatorsCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the UpdateCoordinatorsAsync/UpdateCoordinators method is called.
        /// </summary>
        public event UpdateCoordinatorsCallBack OnUpdateCoordinatorsCallBack;


        public delegate void GetAgentInfoCallBack(object sender, GetAgentInfoCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called.
        /// </summary>
        public event GetAgentInfoCallBack OnGetAgentInfoCallBack;


        public delegate void AddAgentInfoCallBack(object sender, AddAgentInfoCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called.
        /// </summary>
        public event AddAgentInfoCallBack OnAddAgentInfoCallBack;


        public delegate void DeleteCloneCellCallBack(object sender, DeleteCloneCellCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called.
        /// </summary>
        public event DeleteCloneCellCallBack OnDeleteCloneCellCallBack;


        public delegate void GetStorageInfoCallBack(object sender, GetStorageInfoCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called.
        /// </summary>
        public event GetStorageInfoCallBack OnGetStorageInfoCallBack;


        public delegate void DumpNetworkStatsCallBack(object sender, DumpNetworkStatsCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor containing the requested Agent Info after the GetAgentInfoAsync/GetAgentInfo method is called.
        /// </summary>
        public event DumpNetworkStatsCallBack OnDumpNetworkStatsCallBack;

        /// <summary>
        /// This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientAdmin(HoloNETDNA holoNETDNA = null) : base(holoNETDNA)
        {

        }

        /// <summary>
        /// This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProvider">The implementation of the ILogProvider interface (custom logProvider).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientAdmin(ILogProvider logProvider, bool alsoUseDefaultLogger = false, HoloNETDNA holoNETDNA = null) : base(logProvider, alsoUseDefaultLogger, holoNETDNA)
        {

        }

        /// <summary>
        /// This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProviders">The implementations of the ILogProvider interface (custom logProviders).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom loggers injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientAdmin(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, HoloNETDNA holoNETDNA = null) : base(logProviders, alsoUseDefaultLogger, holoNETDNA)
        {

        }

        /// <summary>
        /// This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logger">The logger instance to use.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientAdmin(Logger logger, HoloNETDNA holoNETDNA = null) : base(logger, holoNETDNA)
        {

        }

        /// <summary>
        /// This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.
        /// </summary>
        /// <param name="holochainConductorURI">The URI that the Holochain Conductor is running.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the [HoloNETHoloNETDNA](#holonetconfig) once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns></returns>
        public override async Task<HoloNETConnectedEventArgs> ConnectAsync(Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            return await ConnectAsync(holochainConductorURI.AbsoluteUri, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.
        /// </summary>
        /// <param name="holochainConductorURI">The URI that the Holochain Conductor is running.</param>
        ///// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the [HoloNETHoloNETDNA](#holonetconfig) once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns></returns>
        public override HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            return ConnectAsync(holochainConductorURI, ConnectedCallBackMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved).Result;
        }

        /// <summary>
        /// This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.
        /// </summary>
        /// <param name="holochainConductorURI">The URI that the Holochain Conductor is running.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the [HoloNETHoloNETDNA](#holonetconfig) once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns></returns>
        public override async Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            if (string.IsNullOrEmpty(holochainConductorURI))
                holochainConductorURI = HoloNETDNA.HolochainConductorAdminURI;

            return await base.ConnectAsync(holochainConductorURI, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetrieveAgentPubKeyAndDnaHash method to retrieve the AgentPubKey & DnaHash.
        /// </summary>
        /// <param name="holochainConductorURI">The URI that the Holochain Conductor is running.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the [HoloNETHoloNETDNA](#holonetconfig) once it has retrieved the DnaHash & AgentPubKey.</param>
        public override HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            return ConnectAsync(holochainConductorURI, ConnectedCallBackMode.UseCallBackEvents, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved).Result;
        }
    }
}