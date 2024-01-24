using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography;
using MessagePack;
using Nito.AsyncEx.Synchronous;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETClientAdmin : HoloNETClientBase, IHoloNETClientAdmin
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
        private Dictionary<string, TaskCompletionSource<RegisterDnaCallBackEventArgs>> _taskCompletionRegisterDnaCallBack = new Dictionary<string, TaskCompletionSource<RegisterDnaCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<AppsListedCallBackEventArgs>> _taskCompletionAppsListedCallBack = new Dictionary<string, TaskCompletionSource<AppsListedCallBackEventArgs>>();
        private Dictionary<string, TaskCompletionSource<ListDnasCallBackEventArgs>> _taskCompletionListDnasCallBack = new Dictionary<string, TaskCompletionSource<ListDnasCallBackEventArgs>>();
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


        public delegate void RegisterDnaCallBack(object sender, RegisterDnaCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the dna has been registered via the RegisterDnaAsync/RegisterDna method.
        /// </summary>
        public event RegisterDnaCallBack OnRegisterDnaCallBack;


        public delegate void AppsListedCallBack(object sender, AppsListedCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the ListAppsAsync/ListApps method is called.
        /// </summary>
        public event AppsListedCallBack OnAppsListedCallBack;


        public delegate void ListDnasCallBack(object sender, ListDnasCallBackEventArgs e);

        /// <summary>
        /// Fired when a response is received from the conductor after the ListDnasAsync/ListDnas method is called.
        /// </summary>
        public event ListDnasCallBack OnListDnasCallBack;


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
        public override async Task<HoloNETConnectEventArgs> ConnectAsync(Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
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
        public override HoloNETConnectEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
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
        public override async Task<HoloNETConnectEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            //Is = true;

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
        public override HoloNETConnectEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            return ConnectAsync(holochainConductorURI, ConnectedCallBackMode.UseCallBackEvents, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved).Result;
        }

        /// <summary>
        /// Will init the hApp, which includes installing and enabling the app, signing credentials & attaching the app interface.
        /// </summary>
        //public async Task<InstallEnableSignAndAttachHappEventArgs> InstallEnableSignAndAttachHappAsync(string hAppId, string hAppInstallPath, CapGrantAccessType capGrantAccessType = CapGrantAccessType.Unrestricted, GrantedFunctionsType grantedFunctionsType = GrantedFunctionsType.All, List<(string, string)> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        public async Task<InstallEnableSignAndAttachHappEventArgs> InstallEnableSignAndAttachHappAsync(string hAppId, string hAppInstallPath, CapGrantAccessType capGrantAccessType = CapGrantAccessType.Unrestricted, GrantedFunctionsType grantedFunctionsType = GrantedFunctionsType.All, List<(string, string)> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)
        {
            InstallEnableSignAndAttachHappEventArgs result = new InstallEnableSignAndAttachHappEventArgs();

            if (log)
                Log($"ADMIN: Checking If App {hAppId} Is Already Installed...", LogType.Info, loggingFunction);

            GetAppInfoCallBackEventArgs appInfoResult = await GetAppInfoAsync(hAppId);

            if (appInfoResult != null && appInfoResult.AppInfo != null && uninstallhAppIfAlreadyInstalled)
            {
                if (log)
                    Log($"ADMIN: App {hAppId} Is Already Installed So Uninstalling Now...", LogType.Info);

                AppUninstalledCallBackEventArgs uninstallResult = await UninstallAppAsync(hAppId);

                if (uninstallResult != null && uninstallResult.IsError)
                {
                    if (log)
                        Log($"ADMIN: Error Uninstalling App {hAppId}. Reason: {uninstallResult.Message}", LogType.Info, loggingFunction);
                }
                else
                {
                    if (log)
                        Log($"ADMIN: Uninstalled App {hAppId}.", LogType.Info, loggingFunction);
                }
            }

            if (log)
                Log($"ADMIN: Generating New AgentPubKey...", LogType.Info, loggingFunction);

            result.AgentPubKeyGeneratedCallBackEventArgs = await GenerateAgentPubKeyAsync();

            if (result.AgentPubKeyGeneratedCallBackEventArgs != null && !result.AgentPubKeyGeneratedCallBackEventArgs.IsError)
            {
                if (log)
                {
                    Log($"ADMIN: AgentPubKey Generated Successfully. AgentPubKey: {result.AgentPubKeyGeneratedCallBackEventArgs.AgentPubKey}", LogType.Info, loggingFunction);
                    Log($"ADMIN: Installing App {{appId}}...", LogType.Info, loggingFunction);
                }

                result.IsAgentPubKeyGenerated = true;
                result.AppInstalledCallBackEventArgs = await InstallAppAsync(hAppId, hAppInstallPath, null);

                if (result.AppInstalledCallBackEventArgs != null && !result.AppInstalledCallBackEventArgs.IsError)
                {
                    if (log)
                    {
                        Log($"ADMIN: {hAppId} App Installed.", LogType.Info, loggingFunction);
                        Log($"ADMIN: Enabling App {hAppId}...", LogType.Info, loggingFunction);
                    }

                    result.IsAppInstalled = true;
                    result.AgentPubKey = result.AppInstalledCallBackEventArgs.DnaHash;
                    result.DnaHash = result.AppInstalledCallBackEventArgs.DnaHash;
                    result.CellId = result.AppInstalledCallBackEventArgs.CellId;
                    result.AppStatus = result.AppInstalledCallBackEventArgs.AppInfoResponse.data.AppStatus;
                    result.AppStatusReason = result.AppInstalledCallBackEventArgs.AppInfoResponse.data.AppStatusReason;
                    result.AppManifest = result.AppInstalledCallBackEventArgs.AppInfoResponse.data.manifest;

                    result.AppEnabledCallBackEventArgs = await EnableAppAsync(hAppId);

                    if (result.AppEnabledCallBackEventArgs != null && !result.AppEnabledCallBackEventArgs.IsError)
                    {
                        if (log)
                        {
                            Log($"ADMIN: {hAppId} App Enabled.", LogType.Info, loggingFunction);
                            Log($"ADMIN: Signing Credentials (Zome Call Capabilities) For App {hAppId}...", LogType.Info, loggingFunction);
                        }

                        result.IsAppEnabled = true;
                        result.ZomeCallCapabilityGrantedCallBackEventArgs = await AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(result.AppInstalledCallBackEventArgs.CellId, capGrantAccessType, grantedFunctionsType, grantedFunctions);

                        if (result.ZomeCallCapabilityGrantedCallBackEventArgs != null && !result.ZomeCallCapabilityGrantedCallBackEventArgs.IsError)
                        {
                            if (log)
                            {
                                Log($"ADMIN: {hAppId} App Signing Credentials Authorized.", LogType.Info, loggingFunction);
                                Log($"ADMIN: Attaching App Interface For App {hAppId}...", LogType.Info, loggingFunction);
                            }

                            result.IsAppSigned = true;
                            result.AppInterfaceAttachedCallBackEventArgs = await AttachAppInterfaceAsync();

                            if (result.AppInterfaceAttachedCallBackEventArgs != null && !result.AppInterfaceAttachedCallBackEventArgs.IsError)
                            {
                                result.IsAppAttached = true;
                                result.IsSuccess = true;
                                result.AttachedOnPort = result.AppInterfaceAttachedCallBackEventArgs.Port;

                                if (log)
                                    Log($"ADMIN: {hAppId} App Interface Attached On Port {result.AppInterfaceAttachedCallBackEventArgs.Port}.", LogType.Info, loggingFunction);
                            }
                            else
                            {
                                result.Message = $"ADMIN: Error Attaching App Interface For App {hAppId}. Reason: {result.AppInterfaceAttachedCallBackEventArgs.Message}";
                                result.IsError = true;
                            }
                        }
                        else
                        {
                            result.Message = $"ADMIN: Error Signing Credentials For App {hAppId}. Reason: {result.ZomeCallCapabilityGrantedCallBackEventArgs.Message}";
                            result.IsError = true;
                        }
                    }
                    else
                    {
                        result.Message = $"ADMIN: Error Enabling App {hAppId}. Reason: {result.AppEnabledCallBackEventArgs.Message}";
                        result.IsError = true;
                    }
                }
                else
                {
                    result.Message = $"ADMIN: Error Installing App {hAppId}. Reason: {result.AppInstalledCallBackEventArgs.Message}";
                    result.IsError = true;
                }
            }
            else
            {
                result.Message = $"ADMIN: Error Generating AgentPubKey. Reason: {result.AgentPubKeyGeneratedCallBackEventArgs.Message}";
                result.IsError = true;
            }

            if (log && result.IsError)
                Log(result.Message, LogType.Error, loggingFunction);

            OnInstallEnableSignAndAttachHappCallBack?.Invoke(this, result);
            return result;
        }

        ///// <summary>
        ///// Will init the hApp, which includes installing and enabling the app, signing credentials & attaching the app interface.
        ///// </summary>
        //public InstallEnableSignAndAttachHappEventArgs InstallEnableSignAndAttachHapp(string hAppId, string hAppInstallPath, CapGrantAccessType capGrantAccessType = CapGrantAccessType.Unrestricted, GrantedFunctionsType grantedFunctionsType = GrantedFunctionsType.All, List<(string, string)> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)
        //{
        //    InstallEnableSignAndAttachHappEventArgs result = new InstallEnableSignAndAttachHappEventArgs();

        //    if (log)
        //        Log($"ADMIN: Checking If App {hAppId} Is Already Installed...", LogType.Info, loggingFunction);

        //    GetAppInfoCallBackEventArgs appInfoResult = GetAppInfo(hAppId);

        //    if (appInfoResult != null && appInfoResult.AppInfo != null && uninstallhAppIfAlreadyInstalled)
        //    {
        //        if (log)
        //            Log($"ADMIN: App {hAppId} Is Already Installed So Uninstalling Now...", LogType.Info);

        //        AppUninstalledCallBackEventArgs uninstallResult = UninstallApp(hAppId);

        //        if (uninstallResult != null && uninstallResult.IsError)
        //        {
        //            if (log)
        //                Log($"ADMIN: Error Uninstalling App {hAppId}. Reason: {uninstallResult.Message}", LogType.Info, loggingFunction);
        //        }
        //        else
        //        {
        //            if (log)
        //                Log($"ADMIN: Uninstalled App {hAppId}.", LogType.Info, loggingFunction);
        //        }
        //    }

        //    if (log)
        //        Log($"ADMIN: Generating New AgentPubKey...", LogType.Info, loggingFunction);

        //    result.AgentPubKeyGeneratedCallBackEventArgs = await GenerateAgentPubKeyAsync();

        //    if (result.AgentPubKeyGeneratedCallBackEventArgs != null && !result.AgentPubKeyGeneratedCallBackEventArgs.IsError)
        //    {
        //        if (log)
        //        {
        //            Log($"ADMIN: AgentPubKey Generated Successfully. AgentPubKey: {result.AgentPubKeyGeneratedCallBackEventArgs.AgentPubKey}", LogType.Info, loggingFunction);
        //            Log($"ADMIN: Installing App {{appId}}...", LogType.Info, loggingFunction);
        //        }

        //        result.IsAgentPubKeyGenerated = true;
        //        result.AppInstalledCallBackEventArgs = await InstallAppAsync(hAppId, hAppInstallPath, null);

        //        if (result.AppInstalledCallBackEventArgs != null && !result.AppInstalledCallBackEventArgs.IsError)
        //        {
        //            if (log)
        //            {
        //                Log($"ADMIN: {hAppId} App Installed.", LogType.Info, loggingFunction);
        //                Log($"ADMIN: Enabling App {hAppId}...", LogType.Info, loggingFunction);
        //            }

        //            result.IsAppInstalled = true;
        //            result.AgentPubKey = result.AppInstalledCallBackEventArgs.DnaHash;
        //            result.DnaHash = result.AppInstalledCallBackEventArgs.DnaHash;
        //            result.CellId = result.AppInstalledCallBackEventArgs.CellId;
        //            result.AppStatus = result.AppInstalledCallBackEventArgs.AppInfoResponse.data.AppStatus;
        //            result.AppStatusReason = result.AppInstalledCallBackEventArgs.AppInfoResponse.data.AppStatusReason;
        //            result.AppManifest = result.AppInstalledCallBackEventArgs.AppInfoResponse.data.manifest;

        //            result.AppEnabledCallBackEventArgs = await EnableAppAsync(hAppId);

        //            if (result.AppEnabledCallBackEventArgs != null && !result.AppEnabledCallBackEventArgs.IsError)
        //            {
        //                if (log)
        //                {
        //                    Log($"ADMIN: {hAppId} App Enabled.", LogType.Info, loggingFunction);
        //                    Log($"ADMIN: Signing Credentials (Zome Call Capabilities) For App {hAppId}...", LogType.Info, loggingFunction);
        //                }

        //                result.IsAppEnabled = true;
        //                result.ZomeCallCapabilityGrantedCallBackEventArgs = await AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(result.AppInstalledCallBackEventArgs.CellId, capGrantAccessType, grantedFunctionsType, grantedFunctions);

        //                if (result.ZomeCallCapabilityGrantedCallBackEventArgs != null && !result.ZomeCallCapabilityGrantedCallBackEventArgs.IsError)
        //                {
        //                    if (log)
        //                    {
        //                        Log($"ADMIN: {hAppId} App Signing Credentials Authorized.", LogType.Info, loggingFunction);
        //                        Log($"ADMIN: Attaching App Interface For App {hAppId}...", LogType.Info, loggingFunction);
        //                    }

        //                    result.IsAppSigned = true;
        //                    result.AppInterfaceAttachedCallBackEventArgs = await AttachAppInterfaceAsync();

        //                    if (result.AppInterfaceAttachedCallBackEventArgs != null && !result.AppInterfaceAttachedCallBackEventArgs.IsError)
        //                    {
        //                        result.IsAppAttached = true;
        //                        result.IsSuccess = true;
        //                        result.AttachedOnPort = result.AppInterfaceAttachedCallBackEventArgs.Port;

        //                        if (log)
        //                            Log($"ADMIN: {hAppId} App Interface Attached On Port {result.AppInterfaceAttachedCallBackEventArgs.Port}.", LogType.Info, loggingFunction);
        //                    }
        //                    else
        //                    {
        //                        result.Message = $"ADMIN: Error Attaching App Interface For App {hAppId}. Reason: {result.AppInterfaceAttachedCallBackEventArgs.Message}";
        //                        result.IsError = true;
        //                    }
        //                }
        //                else
        //                {
        //                    result.Message = $"ADMIN: Error Signing Credentials For App {hAppId}. Reason: {result.ZomeCallCapabilityGrantedCallBackEventArgs.Message}";
        //                    result.IsError = true;
        //                }
        //            }
        //            else
        //            {
        //                result.Message = $"ADMIN: Error Enabling App {hAppId}. Reason: {result.AppEnabledCallBackEventArgs.Message}";
        //                result.IsError = true;
        //            }
        //        }
        //        else
        //        {
        //            result.Message = $"ADMIN: Error Installing App {hAppId}. Reason: {result.AppInstalledCallBackEventArgs.Message}";
        //            result.IsError = true;
        //        }
        //    }
        //    else
        //    {
        //        result.Message = $"ADMIN: Error Generating AgentPubKey. Reason: {result.AgentPubKeyGeneratedCallBackEventArgs.Message}";
        //        result.IsError = true;
        //    }

        //    if (log && result.IsError)
        //        Log(result.Message, LogType.Error, loggingFunction);

        //    OnInstallEnableSignAndAttachHappCallBack?.Invoke(this, result);
        //    return result;
        //}

        /// <summary>
        /// Will init the hApp, which includes installing and enabling the app, signing credentials & attaching the app interface.
        /// </summary>
        public InstallEnableSignAndAttachHappEventArgs InstallEnableSignAndAttachHapp(string hAppId, string hAppInstallPath, CapGrantAccessType capGrantAccessType = CapGrantAccessType.Unrestricted, GrantedFunctionsType grantedFunctionsType = GrantedFunctionsType.All, List<(string, string)> grantedFunctions = null, bool uninstallhAppIfAlreadyInstalled = true, bool log = true, Action<string, LogType> loggingFunction = null)
        {
            return InstallEnableSignAndAttachHappAsync(hAppId, hAppInstallPath, capGrantAccessType, grantedFunctionsType, grantedFunctions, uninstallhAppIfAlreadyInstalled, log, loggingFunction).Result;
        }

        public async Task<AgentPubKeyGeneratedCallBackEventArgs> GenerateAgentPubKeyAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, bool updateAgentPubKeyInHoloNETDNA = true, string id = "")
        {
            _updateDnaHashAndAgentPubKey = updateAgentPubKeyInHoloNETDNA;
            return await CallFunctionAsync(HoloNETRequestType.AdminGrantZomeCallCapability, "generate_agent_pub_key", null, _taskCompletionAgentPubKeyGeneratedCallBack, "OnAgentPubKeyGeneratedCallBack", conductorResponseCallBackMode, id);
        }

        //public void GenerateAgentPubKey(bool updateAgentPubKeyInHoloNETDNA = true, string id = "")
        //{
        //    _updateDnaHashAndAgentPubKey = updateAgentPubKeyInHoloNETDNA;
        //    CallFunction(HoloNETRequestType.GenerateAgentPubKey, "generate_agent_pub_key", null, id);
        //}

        public void GenerateAgentPubKey(bool updateAgentPubKeyInHoloNETDNA = true, string id = "")
        {
            //var result = AsyncContext.Run(() => GenerateAgentPubKeyAsync(ConductorResponseCallBackMode.UseCallBackEvents, updateAgentPubKeyInHoloNETDNA, id));
            //GenerateAgentPubKeyAsync(ConductorResponseCallBackMode.UseCallBackEvents, updateAgentPubKeyInHoloNETDNA, id).Wait();
            //GenerateAgentPubKeyAsync(ConductorResponseCallBackMode.UseCallBackEvents, updateAgentPubKeyInHoloNETDNA, id).WaitAsync(new TimeSpan(0, 0, 3));
            //var task = Task.Run(async () => await GenerateAgentPubKeyAsync(ConductorResponseCallBackMode.UseCallBackEvents, updateAgentPubKeyInHoloNETDNA, id));

            //GenerateAgentPubKeyAsync(ConductorResponseCallBackMode.UseCallBackEvents, updateAgentPubKeyInHoloNETDNA, id).RunSynchronously();
            GenerateAgentPubKeyAsync(ConductorResponseCallBackMode.UseCallBackEvents, updateAgentPubKeyInHoloNETDNA, id).WaitAndUnwrapException();
        }

        public async Task<AppInstalledCallBackEventArgs> InstallAppAsync(string installedAppId, string hAppPath, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await InstallAppInternalAsync(installedAppId, hAppPath, null, agentKey, membraneProofs, network_seed, conductorResponseCallBackMode, id);
        }

        public void InstallApp(string agentKey, string installedAppId, string hAppPath, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, string id = null)
        {
            InstallAppInternal(agentKey, installedAppId, hAppPath, null, membraneProofs, network_seed, id);
        }

        public async Task<AppInstalledCallBackEventArgs> InstallAppAsync(string installedAppId, AppBundle appBundle, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await InstallAppInternalAsync(installedAppId, null, appBundle, agentKey, membraneProofs, network_seed, conductorResponseCallBackMode, id);
        }

        public AppInstalledCallBackEventArgs InstallApp(string installedAppId, AppBundle appBundle, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, string id = null)
        {
            return InstallAppInternalAsync(installedAppId, null, appBundle, agentKey, membraneProofs, network_seed, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<AppUninstalledCallBackEventArgs> UninstallAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            if (string.IsNullOrEmpty(id))
                id = GetRequestId();

            _uninstallingAppLookup[id] = installedAppId;

            return await CallFunctionAsync(HoloNETRequestType.AdminUninstallApp, "uninstall_app", new UninstallAppRequest()
            {
                installed_app_id = installedAppId
            }, _taskCompletionAppUninstalledCallBack, "OnAppUninstalledCallBack", conductorResponseCallBackMode, id);
        }

        public AppUninstalledCallBackEventArgs UninstallApp(string installedAppId, string id = null)
        {
            return UninstallAppAsync(installedAppId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<AppEnabledCallBackEventArgs> EnableAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminEnableApp, "enable_app", new EnableAppRequest()
            {
                installed_app_id = installedAppId
            }, _taskCompletionAppEnabledCallBack, "OnAppEnabledCallBack", conductorResponseCallBackMode, id);
        }

        public AppEnabledCallBackEventArgs EnablelApp(string installedAppId, string id = null)
        {
            return EnableAppAsync(installedAppId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<AppDisabledCallBackEventArgs> DisableAppAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            if (string.IsNullOrEmpty(id))
                id = GetRequestId();

            _disablingAppLookup[id] = installedAppId;

            return await CallFunctionAsync(HoloNETRequestType.AdminDisableApp, "disable_app", new EnableAppRequest()
            {
                installed_app_id = installedAppId
            }, _taskCompletionAppDisabledCallBack, "OnAppDisabledCallBack", conductorResponseCallBackMode, id);
        }

        public AppDisabledCallBackEventArgs DisableApp(string installedAppId, string id = null)
        {
            return DisableAppAsync(installedAppId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(string AgentPubKey, string DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<(string, string)> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = "")
        {
            return await AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(GetCellId(DnaHash, AgentPubKey), capGrantAccessType, grantedFunctionsType, functions, conductorResponseCallBackMode, id);
        }

        public ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(string AgentPubKey, string DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<(string, string)> functions = null, string id = "")
        {
            return AuthorizeSigningCredentialsAndGrantZomeCallCapability(GetCellId(DnaHash, AgentPubKey), capGrantAccessType, grantedFunctionsType, functions, id);
        }

        public async Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(byte[] AgentPubKey, byte[] DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<(string, string)> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = "")
        {
            return await AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(GetCellId(DnaHash, AgentPubKey), capGrantAccessType, grantedFunctionsType, functions, conductorResponseCallBackMode, id);
        }

        public ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(byte[] AgentPubKey, byte[] DnaHash, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<(string, string)> functions = null, string id = "")
        {
            return AuthorizeSigningCredentialsAndGrantZomeCallCapability(GetCellId(DnaHash, AgentPubKey), capGrantAccessType, grantedFunctionsType, functions, id);
        }

        public async Task<ZomeCallCapabilityGrantedCallBackEventArgs> AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(byte[][] cellId, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<(string, string)> functions = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = "")
        {
            (ZomeCallCapabilityGrantedCallBackEventArgs args, Dictionary<GrantedFunctionsType, List<(string, string)>> grantedFunctions, byte[] signingKey) = AuthorizeSigningCredentials(cellId, grantedFunctionsType, functions, id);

            if (!args.IsError)
            {
                return await CallFunctionAsync(HoloNETRequestType.AdminGrantZomeCallCapability, "grant_zome_call_capability", CreateGrantZomeCallCapabilityRequest(cellId, capGrantAccessType, grantedFunctions, signingKey),
                _taskCompletionZomeCapabilityGrantedCallBack, "OnZomeCallCapabilityGranted", conductorResponseCallBackMode, id);
            }
            else
                return args;
        }

        public ZomeCallCapabilityGrantedCallBackEventArgs AuthorizeSigningCredentialsAndGrantZomeCallCapability(byte[][] cellId, CapGrantAccessType capGrantAccessType, GrantedFunctionsType grantedFunctionsType, List<(string, string)> functions = null, string id = "")
        {
            (ZomeCallCapabilityGrantedCallBackEventArgs args, Dictionary<GrantedFunctionsType, List<(string, string)>> grantedFunctions, byte[] signingKey) = AuthorizeSigningCredentials(cellId, grantedFunctionsType, functions, id);

            if (!args.IsError)
            {
                CallFunction(HoloNETRequestType.AdminGrantZomeCallCapability, "grant_zome_call_capability", CreateGrantZomeCallCapabilityRequest(cellId, capGrantAccessType, grantedFunctions, signingKey), id);
                return new ZomeCallCapabilityGrantedCallBackEventArgs() { EndPoint = EndPoint, Id = id, Message = "The call has been sent to the conductor.  Please wait for the event 'OnZomeCallCapabilityGranted' to view the response." };
            }
            else
                return args;
        }

        public byte[] GetCapGrantSecret(byte[][] cellId)
        {
            return _signingCredentialsForCell[$"{ConvertHoloHashToString(cellId[1])}:{ConvertHoloHashToString(cellId[0])}"].CapSecret;
        }

        public byte[] GetCapGrantSecret(string agentPubKey, string dnaHash)
        {
            return _signingCredentialsForCell[$"{agentPubKey}:{dnaHash}"].CapSecret;
        }

        public async Task<AppInterfaceAttachedCallBackEventArgs> AttachAppInterfaceAsync(UInt16? port = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminAttachAppInterface, "attach_app_interface", new AttachAppInterfaceRequest()
            {
                port = port
            }, _taskCompletionAppInterfaceAttachedCallBack, "OnAppInterfaceAttachedCallBack", conductorResponseCallBackMode, id);
        }

        public AppInterfaceAttachedCallBackEventArgs AttachAppInterface(UInt16? port = null, string id = null)
        {
            return AttachAppInterfaceAsync(port, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<RegisterDnaCallBackEventArgs> RegisterDnaAsync(string path, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await RegisterDnaAsync(path, null, null, network_seed, properties, conductorResponseCallBackMode, id);
        }

        public RegisterDnaCallBackEventArgs RegisterDna(string path, string network_seed = null, object properties = null, string id = null)
        {
            return RegisterDnaAsync(path, network_seed, properties, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<RegisterDnaCallBackEventArgs> RegisterDnaAsync(byte[] hash, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await RegisterDnaAsync(null, null, hash, network_seed, properties, conductorResponseCallBackMode, id);
        }

        public RegisterDnaCallBackEventArgs RegisterDna(byte[] hash, string network_seed = null, object properties = null, string id = null)
        {
            return RegisterDnaAsync(hash, network_seed, properties, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<RegisterDnaCallBackEventArgs> RegisterDnaAsync(DnaBundle bundle, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await RegisterDnaAsync(null, bundle, null, network_seed, properties, conductorResponseCallBackMode, id);
        }

        public RegisterDnaCallBackEventArgs RegisterDna(DnaBundle bundle, string network_seed = null, object properties = null, string id = null)
        {
            return RegisterDnaAsync(bundle, network_seed, properties, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<GetAppInfoCallBackEventArgs> GetAppInfoAsync(string installedAppId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            GetAppInfoCallBackEventArgs result = new GetAppInfoCallBackEventArgs();
            AppsListedCallBackEventArgs listAppsResult = await ListAppsAsync(AppStatusFilter.All);

            if (listAppsResult != null && !listAppsResult.IsError)
            {
                foreach (AppInfo app in listAppsResult.Apps)
                {
                    if (app.installed_app_id == installedAppId)
                    {
                        result.AppInfo = app;
                        break;
                    }
                }
            }

            if (result.AppInfo == null)
            {
                result.IsError = true;
                result.Message = "App Not Found";
            }

            return result;
        }

        public GetAppInfoCallBackEventArgs GetAppInfo(string installedAppId, string id = null)
        {
            return GetAppInfoAsync(installedAppId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<AppsListedCallBackEventArgs> ListAppsAsync(AppStatusFilter appStatusFilter, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            _updateDnaHashAndAgentPubKey = false;

            return await CallFunctionAsync(HoloNETRequestType.AdminListApps, "list_apps", new ListAppsRequest()
            {
                status_filter = appStatusFilter != AppStatusFilter.All ? appStatusFilter : null
            }, _taskCompletionAppsListedCallBack, "OnListAppsCallBack", conductorResponseCallBackMode, id);
        }

        public AppsListedCallBackEventArgs ListApps(AppStatusFilter appStatusFilter, string id = null)
        {
            return ListAppsAsync(appStatusFilter, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<ListDnasCallBackEventArgs> ListDnasAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminListDnas, "list_dnas", null, _taskCompletionListDnasCallBack, "OnListDnasCallBack", conductorResponseCallBackMode, id);
        }

        public ListDnasCallBackEventArgs ListDnas(string id = null)
        {
            return ListDnasAsync(ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<ListCellIdsCallBackEventArgs> ListCellIdsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminListCellIds, "list_cell_ids", null, _taskCompletionListCellIdsCallBack, "OnListCellIds", conductorResponseCallBackMode, id);
        }

        public ListCellIdsCallBackEventArgs ListCellIds(string id = null)
        {
            return ListCellIdsAsync(ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public async Task<ListAppInterfacesCallBackEventArgs> ListInterfacesAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminListAppInterfaces, "list_app_interfaces", null, _taskCompletionListAppInterfacesCallBack, "OnListAppInterfacesCallBack", conductorResponseCallBackMode, id);
        }

        public ListAppInterfacesCallBackEventArgs ListInterfaces(string id = null)
        {
            return ListInterfacesAsync(ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Dump the full state of the specified cell, including its chain and DHT shard, as JSON.
        /// </summary>
        /// <param name="cellId">The cell id to dump the full state for.</param>
        /// <param name="dHTOpsCursor">The last seen DhtOp RowId, returned in the full dump state. Only DhtOps with RowId greater than the cursor will be returned.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DumpFullStateCallBackEventArgs> DumpFullStateAsync(byte[][] cellId, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminDumpFullState, "dump_full_state", new DumpFullStateRequest()
            {
                cell_id = cellId,
                dht_ops_cursor = dHTOpsCursor
            }, _taskCompletionDumpFullStateCallBack, "OnDumpFullStateCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Dump the full state of the specified cell, including its chain and DHT shard, as JSON.
        /// </summary>
        /// <param name="cellId">The cell id to dump the full state for.</param>
        /// <param name="dHTOpsCursor">The last seen DhtOp RowId, returned in the full dump state. Only DhtOps with RowId greater than the cursor will be returned.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DumpFullStateCallBackEventArgs DumpFullState(byte[][] cellId, int? dHTOpsCursor = null, string id = null)
        {
            return DumpFullStateAsync(cellId, dHTOpsCursor, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Dump the full state of the specified cell, including its chain and DHT shard, as JSON.
        /// </summary>
        /// <param name="agentPubKey">The AgentPubKey for the cell to dump the full state for.</param>
        /// <param name="dnaHash">The DnaHash for the cell to dump the full state for.</param>
        /// <param name="dHTOpsCursor">The last seen DhtOp RowId, returned in the full dump state. Only DhtOps with RowId greater than the cursor will be returned.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DumpFullStateCallBackEventArgs> DumpFullStateAsync(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await DumpFullStateAsync(GetCellId(dnaHash, agentPubKey), dHTOpsCursor, conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Dump the full state of the specified cell, including its chain and DHT shard, as JSON.
        /// </summary>
        /// <param name="agentPubKey">The AgentPubKey for the cell to dump the full state for.</param>
        /// <param name="dnaHash">The DnaHash for the cell to dump the full state for.</param>
        /// <param name="dHTOpsCursor">The last seen DhtOp RowId, returned in the full dump state. Only DhtOps with RowId greater than the cursor will be returned.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DumpFullStateCallBackEventArgs DumpFullState(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, string id = null)
        {
            return DumpFullStateAsync(agentPubKey, dnaHash, dHTOpsCursor, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Dump the full state of the specified cell, including its chain and DHT shard, as JSON. This will dump the state for the current AgentPubKey/DnaHash stored in HoloNETDNA.AgentPubKey & HoloNETDNA.DnaHash. If there it is not stored in the HoloNETDNA it will automatically generate one for you and retrieve from the conductor.
        /// </summary>
        /// <param name="dHTOpsCursor">The last seen DhtOp RowId, returned in the full dump state. Only DhtOps with RowId greater than the cursor will be returned.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DumpFullStateCallBackEventArgs> DumpFullStateAsync(int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await DumpFullStateAsync(await GetCellIdAsync(), dHTOpsCursor, conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Dump the full state of the specified cell, including its chain and DHT shard, as JSON. This will dump the state for the current AgentPubKey/DnaHash stored in HoloNETDNA.AgentPubKey & HoloNETDNA.DnaHash.
        /// </summary>
        /// <param name="dHTOpsCursor">The last seen DhtOp RowId, returned in the full dump state. Only DhtOps with RowId greater than the cursor will be returned.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DumpFullStateCallBackEventArgs DumpFullState(int? dHTOpsCursor = null, string id = null)
        {
            return DumpFullStateAsync(dHTOpsCursor, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Dump the state of the specified cell, including its source chain, as JSON.
        /// </summary>
        /// <param name="cellId">The cell id to dump the full state for.</param>
        /// <param name="dHTOpsCursor">The last seen DhtOp RowId, returned in the full dump state. Only DhtOps with RowId greater than the cursor will be returned.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DumpStateCallBackEventArgs> DumpStateAsync(byte[][] cellId, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminDumpState, "dump_state", new DumpStateRequest()
            {
                cell_id = cellId
            }, _taskCompletionDumpStateCallBack, "OnDumpStateCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Dump the state of the specified cell, including its source chain, as JSON.
        /// </summary>
        /// <param name="cellId">The cell id to dump the full state for.</param>
        /// <param name="dHTOpsCursor">The last seen DhtOp RowId, returned in the full dump state. Only DhtOps with RowId greater than the cursor will be returned.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DumpStateCallBackEventArgs DumpState(byte[][] cellId, int? dHTOpsCursor = null, string id = null)
        {
            return DumpStateAsync(cellId, dHTOpsCursor, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Dump the state of the specified cell, including its source chain, as JSON.
        /// </summary>
        /// <param name="agentPubKey">The AgentPubKey for the cell to dump the full state for.</param>
        /// <param name="dnaHash">The DnaHash for the cell to dump the full state for.</param>
        /// <param name="dHTOpsCursor"></param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DumpStateCallBackEventArgs> DumpStateAsync(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await DumpStateAsync(GetCellId(dnaHash, agentPubKey), dHTOpsCursor, conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Dump the state of the specified cell, including its source chain, as JSON.
        /// </summary>
        /// <param name="agentPubKey">The AgentPubKey for the cell to dump the full state for.</param>
        /// <param name="dnaHash">The DnaHash for the cell to dump the full state for.</param>
        /// <param name="dHTOpsCursor"></param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DumpStateCallBackEventArgs DumpState(string agentPubKey, string dnaHash, int? dHTOpsCursor = null, string id = null)
        {
            return DumpStateAsync(agentPubKey, dnaHash, dHTOpsCursor, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Dump the state of the specified cell, including its source chain, as JSON. This will dump the state for the current AgentPubKey/DnaHash stored in HoloNETDNA.AgentPubKey & HoloNETDNA.DnaHash.
        /// </summary>
        /// <param name="dHTOpsCursor"></param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DumpStateCallBackEventArgs> DumpStateAsync(int? dHTOpsCursor = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await DumpStateAsync(await GetCellIdAsync(), dHTOpsCursor, conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Dump the state of the specified cell, including its source chain, as JSON. This will dump the state for the current AgentPubKey/DnaHash stored in HoloNETDNA.AgentPubKey & HoloNETDNA.DnaHash.
        /// </summary>
        /// <param name="dHTOpsCursor"></param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DumpStateCallBackEventArgs DumpState(int? dHTOpsCursor = null, string id = null)
        {
            return DumpStateAsync(dHTOpsCursor, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }


        /// <summary>
        /// Get the DNA definition for the specified DNA hash.
        /// </summary>
        /// <param name="dnaHash">The hash of the dna.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<GetDnaDefinitionCallBackEventArgs> GetDnaDefinitionAsync(byte[] dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminGetDnaDefinition, "get_dna_definition", new UpdateCoordinatorsRequest()
            {
                dnaHash = dnaHash
            }, _taskCompletionGetDnaDefinitionCallBack, "OnGetDnaDefinitionCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Get the DNA definition for the specified DNA hash.
        /// </summary>
        /// <param name="dnaHash">The hash of the dna.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public GetDnaDefinitionCallBackEventArgs GetDnaDefinition(byte[] dnaHash, string id = null)
        {
            return GetDnaDefinitionAsync(dnaHash, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Get the DNA definition for the specified DNA hash.
        /// </summary>
        /// <param name="dnaHash">The hash of the dna.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<GetDnaDefinitionCallBackEventArgs> GetDnaDefinitionAsync(string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await GetDnaDefinitionAsync(ConvertHoloHashToBytes(dnaHash), conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Get the DNA definition for the specified DNA hash.
        /// </summary>
        /// <param name="dnaHash">The hash of the dna.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public GetDnaDefinitionCallBackEventArgs GetDnaDefinition(string dnaHash, string id = null)
        {
            return GetDnaDefinitionAsync(dnaHash, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<UpdateCoordinatorsCallBackEventArgs> UpdateCoordinatorsAsync(byte[] dnaHash, string path, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await UpdateCoordinatorsAsync(dnaHash, path, null, conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="dnaHash"></param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public UpdateCoordinatorsCallBackEventArgs UpdateCoordinators(byte[] dnaHash, string path, string id = null)
        {
            return UpdateCoordinatorsAsync(dnaHash, path, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<UpdateCoordinatorsCallBackEventArgs> UpdateCoordinatorsAsync(string dnaHash, string path, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await UpdateCoordinatorsAsync(ConvertHoloHashToBytes(dnaHash), path, null, conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="dnaHash"></param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public UpdateCoordinatorsCallBackEventArgs UpdateCoordinators(string dnaHash, string path, string id = null)
        {
            return UpdateCoordinatorsAsync(dnaHash, path, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<UpdateCoordinatorsCallBackEventArgs> UpdateCoordinatorsAsync(byte[] dnaHash, CoordinatorBundle bundle, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await UpdateCoordinatorsAsync(dnaHash, null, bundle, conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="dnaHash"></param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public UpdateCoordinatorsCallBackEventArgs UpdateCoordinators(byte[] dnaHash, CoordinatorBundle bundle, string id = null)
        {
            return UpdateCoordinatorsAsync(dnaHash, bundle, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<UpdateCoordinatorsCallBackEventArgs> UpdateCoordinatorsAsync(string dnaHash, CoordinatorBundle bundle, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await UpdateCoordinatorsAsync(ConvertHoloHashToBytes(dnaHash), null, bundle, conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="dnaHash"></param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public UpdateCoordinatorsCallBackEventArgs UpdateCoordinators(string dnaHash, CoordinatorBundle bundle, string id = null)
        {
            return UpdateCoordinatorsAsync(dnaHash, bundle, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }


        /// <summary>
        /// Request all available info about an agent.
        /// </summary>
        /// <param name="cellId">The cell id to retrive the angent info for.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<GetAgentInfoCallBackEventArgs> GetAgentInfoAsync(byte[][] cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminAgentInfo, "agent_info", new GetAgentInfoRequest()
            {
                cell_id = cellId
            }, _taskCompletionGetAgentInfoCallBack, "OnGetAgentInfoCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Request all available info about an agent.
        /// </summary>
        /// <param name="cellId">The cell id to retrive the angent info for.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public GetAgentInfoCallBackEventArgs GetAgentInfo(byte[][] cellId, string id = null)
        {
            return GetAgentInfoAsync(cellId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Request all available info about an agent.
        /// </summary>
        /// <param name="agentPubKey">The AgentPubKey for the cell to dump the full state for.</param>
        /// <param name="dnaHash">The DnaHash for the cell to dump the full state for.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<GetAgentInfoCallBackEventArgs> GetAgentInfoAsync(string agentPubKey, string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await GetAgentInfoAsync(GetCellId(dnaHash, agentPubKey), conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Request all available info about an agent.
        /// </summary>
        /// <param name="agentPubKey">The AgentPubKey for the cell to dump the full state for.</param>
        /// <param name="dnaHash">The DnaHash for the cell to dump the full state for.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public GetAgentInfoCallBackEventArgs GetAgentInfo(string agentPubKey, string dnaHash, string id = null)
        {
            return GetAgentInfoAsync(agentPubKey, dnaHash, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Request all available info about an agent. This will retreive info for the current AgentPubKey/DnaHash stored in HoloNETDNA.AgentPubKey & HoloNETDNA.DnaHash.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<GetAgentInfoCallBackEventArgs> GetAgentInfoAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await GetAgentInfoAsync(await GetCellIdAsync(), conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Request all available info about an agent. This will retreive info for the current AgentPubKey/DnaHash stored in HoloNETDNA.AgentPubKey & HoloNETDNA.DnaHash.
        /// </summary>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public GetAgentInfoCallBackEventArgs GetAgentInfo(string id = null)
        {
            return GetAgentInfoAsync(ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        ///  Add existing agent(s) to Holochain.
        /// </summary>
        /// <param name="agentInfos">The agentInfo's to add.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<AddAgentInfoCallBackEventArgs> AddAgentInfoAsync(AgentInfo[] agentInfos, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminAddAgentInfo, "add_agent_info", new AddAgentInfoRequest()
            {
                agent_infos = agentInfos
            }, _taskCompletionAddAgentInfoCallBack, "OnAddAgentInfoCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        ///  Add existing agent(s) to Holochain.
        /// </summary>
        /// <param name="agentInfos">The agentInfo's to add.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public AddAgentInfoCallBackEventArgs AddAgentInfo(AgentInfo[] agentInfos, string id = null)
        {
            return AddAgentInfoAsync(agentInfos, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Delete a clone cell that was previously disabled.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="roleName">The clone id (string/rolename).</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DeleteCloneCellCallBackEventArgs> DeleteCloneCellAsync(string appId, string roleName, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminDeleteClonedCell, "delete_clone_cell", new DeleteCloneCellRequest()
            {
                app_id = appId,
                clone_cell_id = roleName
            }, _taskCompletionDeleteCloneCellCallBack, "OnDeleteCloneCellCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        ///  Delete a clone cell that was previously disabled.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="cellId"> The cell id of the cloned cell.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DeleteCloneCellCallBackEventArgs DeleteCloneCell(string appId, string roleName, string id = null)
        {
            return DeleteCloneCellAsync(appId, roleName, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Delete a clone cell that was previously disabled.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="cellId"> The cell id of the cloned cell.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DeleteCloneCellCallBackEventArgs> DeleteCloneCellAsync(string appId, byte[][] cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminDeleteClonedCell, "delete_clone_cell", new DeleteCloneCellRequest()
            {
                app_id = appId,
                clone_cell_id = cellId
            }, _taskCompletionDeleteCloneCellCallBack, "OnDeleteCloneCellCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        ///  Delete a clone cell that was previously disabled.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="cellId"> The cell id of the cloned cell.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DeleteCloneCellCallBackEventArgs DeleteCloneCell(string appId, byte[][] cellId, string id = null)
        {
            return DeleteCloneCellAsync(appId, cellId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Delete a clone cell that was previously disabled.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="agentPubKey">The AgentPubKey for the cell.</param>
        /// <param name="dnaHash">The DnaHash for the cell.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DeleteCloneCellCallBackEventArgs> DeleteCloneCellAsync(string appId, string agentPubKey, string dnaHash, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await DeleteCloneCellAsync(appId, GetCellId(dnaHash, agentPubKey), conductorResponseCallBackMode, id);
        }

        /// <summary>
        ///  Delete a clone cell that was previously disabled.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="agentPubKey">The AgentPubKey for the cell.</param>
        /// <param name="dnaHash">The DnaHash for the cell.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DeleteCloneCellCallBackEventArgs DeleteCloneCell(string appId, string agentPubKey, string dnaHash, string id = null)
        {
            return DeleteCloneCellAsync(appId, agentPubKey, dnaHash, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Delete a clone cell that was previously disabled. This will use the current AgentPubKey/DnaHash stored in HoloNETDNA.AgentPubKey & HoloNETDNA.DnaHash.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DeleteCloneCellCallBackEventArgs> DeleteCloneCellAsync(string appId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await DeleteCloneCellAsync(appId, await GetCellIdAsync(), conductorResponseCallBackMode, id);
        }

        /// <summary>
        ///  Delete a clone cell that was previously disabled. This will use the current AgentPubKey/DnaHash stored in HoloNETDNA.AgentPubKey & HoloNETDNA.DnaHash.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DeleteCloneCellCallBackEventArgs DeleteCloneCell(string appId, string id = null)
        {
            return DeleteCloneCellAsync(appId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Get'the storgage info used by hApps.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<GetStorageInfoCallBackEventArgs> GetStorageInfoAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminStorageInfo, "storage_info", null, _taskCompletionGetStorageInfoCallBack, "OnGetStorageInfoCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Get'the storgage info used by hApps.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="cloneCellId"> The clone id or cell id of the clone cell. Can be RoleName (string) or CellId (byte[][]).</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public GetStorageInfoCallBackEventArgs GetStorageInfo(string id = null)
        {
            return GetStorageInfoAsync(ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Dump the network metrics tracked by kitsune.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<DumpNetworkStatsCallBackEventArgs> DumpNetworkStatsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminDumpNetworkStats, "dump_network_stats", null, _taskCompletionDumpNetworkStatsCallBack, "OnDumpNetworkStatsCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        ///  Dump the network metrics tracked by kitsune.
        /// </summary>
        /// <param name="appId">The app id that the clone cell belongs to.</param>
        /// <param name="cloneCellId"> The clone id or cell id of the clone cell. Can be RoleName (string) or CellId (byte[][]).</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public DumpNetworkStatsCallBackEventArgs DumpNetworkStats(string id = null)
        {
            return DumpNetworkStatsAsync(ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        public override async Task<byte[][]> GetCellIdAsync()
        {
            if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
                await GenerateAgentPubKeyAsync();

            return await base.GetCellIdAsync();
        }

        private async Task<AppInstalledCallBackEventArgs> InstallAppInternalAsync(string installedAppId, string hAppPath = null, AppBundle appBundle = null, string agentKey = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            if (string.IsNullOrEmpty(agentKey))
            {
                if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
                    await GenerateAgentPubKeyAsync();

                agentKey = HoloNETDNA.AgentPubKey;
            }

            if (membraneProofs == null)
                membraneProofs = new Dictionary<string, byte[]>();

            return await CallFunctionAsync(HoloNETRequestType.AdminInstallApp, "install_app", new InstallAppRequest()
            {
                path = hAppPath,
                bundle = appBundle,
                agent_key = ConvertHoloHashToBytes(agentKey),
                installed_app_id = installedAppId,
                membrane_proofs = membraneProofs,
                network_seed = network_seed
            }, _taskCompletionAppInstalledCallBack, "OnAppInstalledCallBack", conductorResponseCallBackMode, id);
        }

        Dictionary<string, string> _installingAppId = new Dictionary<string, string>();

        private void InstallAppInternal(string agentKey, string installedAppId, string hAppPath = null, AppBundle appBundle = null, Dictionary<string, byte[]> membraneProofs = null, string network_seed = null, string id = null)
        {
            if (string.IsNullOrEmpty(agentKey))
            {
                if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
                {
                    //TODO: Later we may want to add the same functionality in the async version to automatically retreive the agentPubKey but for non async version would require a little more work to store the values passed in in a dictionary keyed by id (id would need to be generated first).

                    if (string.IsNullOrEmpty(id))
                        id = GetRequestId();

                    _installingAppId[id] = installedAppId;

                    //    //_installingApp = true;
                    //    //_installedAppId = installedAppId;
                    //    //_hAppPath = hAppPath;
                    //    //_appBundle = appBundle;
                    //    //_membraneProofs
                    //    GenerateAgentPubKey();
                }

                agentKey = HoloNETDNA.AgentPubKey;
            }

            if (!string.IsNullOrEmpty(agentKey))
            {
                if (membraneProofs == null)
                    membraneProofs = new Dictionary<string, byte[]>();

                CallFunction(HoloNETRequestType.AdminInstallApp, "install_app", new InstallAppRequest()
                {
                    path = hAppPath,
                    bundle = appBundle,
                    agent_key = ConvertHoloHashToBytes(agentKey),
                    installed_app_id = installedAppId,
                    membrane_proofs = membraneProofs,
                    network_seed = network_seed
                }, id);
            }
        }

        private async Task<RegisterDnaCallBackEventArgs> RegisterDnaAsync(string path, DnaBundle bundle, byte[] hash, string network_seed = null, object properties = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminRegisterDna, "register_dna", new RegisterDnaRequest()
            {
                path = path,
                bundle = bundle,
                hash = hash,
                modifiers = new DnaModifiers()
                {
                    network_seed = network_seed,
                    properties = properties
                }
            }, _taskCompletionRegisterDnaCallBack, "OnRegisterDnaCallBack", conductorResponseCallBackMode, id);
        }

        private GrantZomeCallCapabilityRequest CreateGrantZomeCallCapabilityRequest(byte[][] cellId, CapGrantAccessType capGrantAccessType, Dictionary<GrantedFunctionsType, List<(string, string)>> grantedFunctions, byte[] signingKey)
        {
            byte[] secret = _signingCredentialsForCell[$"{ConvertHoloHashToString(cellId[1])}:{ConvertHoloHashToString(cellId[0])}"].CapSecret;

            GrantZomeCallCapabilityRequest request = new GrantZomeCallCapabilityRequest()
            {
                cell_id = cellId,
                cap_grant = new ZomeCallCapGrant()
                {
                    tag = "zome-call-signing-key",
                    functions = grantedFunctions,
                }
            };

            switch (capGrantAccessType)
            {
                case CapGrantAccessType.Assigned:
                    {
                        request.cap_grant.access = new CapGrantAccessAssigned()
                        {
                            Assigned = new CapGrantAccessAssignedDetails()
                            {
                                secret = secret,
                                assignees = new byte[1][] { signingKey }
                            }
                        };
                    }
                    break;

                case CapGrantAccessType.Unrestricted:
                    {
                        request.cap_grant.access = new CapGrantAccessUnrestricted()
                        {
                            Unrestricted = null
                        };
                    }
                    break;

                case CapGrantAccessType.Transferable:
                    {
                        request.cap_grant.access = new CapGrantAccessTransferable()
                        {
                            Transferable = new CapGrantAccessTransferableDetails()
                            {
                                secret = secret
                            }
                        };
                    }
                    break;
            }

            return request;
        }

        private (ZomeCallCapabilityGrantedCallBackEventArgs, Dictionary<GrantedFunctionsType, List<(string, string)>>, byte[]) AuthorizeSigningCredentials(byte[][] cellId, GrantedFunctionsType grantedFunctionsType, List<(string, string)> functions = null, string id = "")
        {
            if (cellId == null)
            {
                string msg = "Error occured in AuthorizeSigningCredentialsAsync function. cellId is null.";
                HandleError(msg, null);
                return (new ZomeCallCapabilityGrantedCallBackEventArgs() { IsError = true, EndPoint = EndPoint, Id = id, Message = msg }, null, null);
            }

            if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
            {
                string msg = "Error occured in AuthorizeSigningCredentialsAsync function. HoloNETDNA.AgentPubKey is null. Please set or call GenerateAgentPubKey method.";
                HandleError(msg, null);
                return (new ZomeCallCapabilityGrantedCallBackEventArgs() { IsError = true, EndPoint = EndPoint, Id = id, Message = msg }, null, null);
            }

            if (grantedFunctionsType == GrantedFunctionsType.Listed && functions == null)
            {
                string msg = "Error occured in AuthorizeSigningCredentialsAsync function. GrantedFunctionsType was set to Listed but no functions were passed in.";
                HandleError(msg, null);
                return (new ZomeCallCapabilityGrantedCallBackEventArgs() { IsError = true, EndPoint = EndPoint, Id = id, Message = msg }, null, null);
            }

            Sodium.KeyPair pair = Sodium.PublicKeyAuth.GenerateKeyPair(RandomNumberGenerator.GetBytes(32));
            byte[] DHTLocation = ConvertHoloHashToBytes(HoloNETDNA.AgentPubKey).TakeLast(4).ToArray();
            byte[] signingKey = new byte[] { 132, 32, 36 }.Concat(pair.PublicKey).Concat(DHTLocation).ToArray();

            Dictionary<GrantedFunctionsType, List<(string, string)>> grantedFunctions = new Dictionary<GrantedFunctionsType, List<(string, string)>>();

            if (grantedFunctionsType == GrantedFunctionsType.All)
                grantedFunctions[GrantedFunctionsType.All] = null;
            else
                grantedFunctions[GrantedFunctionsType.Listed] = functions;

            //_signingCredentialsForCell[cellId] = new SigningCredentials()
            //_signingCredentialsForCell[$"{HoloNETDNA.AgentPubKey}:{HoloNETDNA.DnaHash}"] = new SigningCredentials()
            _signingCredentialsForCell[$"{ConvertHoloHashToString(cellId[1])}:{ConvertHoloHashToString(cellId[0])}"] = new SigningCredentials()
            {
                CapSecret = RandomNumberGenerator.GetBytes(64),
                KeyPair = new KeyPair() { PrivateKey = pair.PrivateKey, PublicKey = pair.PublicKey },
                SigningKey = signingKey
            };

            return (new ZomeCallCapabilityGrantedCallBackEventArgs(), grantedFunctions, signingKey);
        }

        /// <summary>
        /// Update the coordinataor zomes.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnDumpFullStateCallBack' event when the conductor responds.   </param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        private async Task<UpdateCoordinatorsCallBackEventArgs> UpdateCoordinatorsAsync(byte[] dnaHash, string path, CoordinatorBundle bundle, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AdminUpdateCoordinators, "update_coordinators", new UpdateCoordinatorsRequest()
            {
                dnaHash = dnaHash,
                path = path,
                bundle = bundle
            }, _taskCompletionUpdateCoordinatorsCallBack, "OnUpdateCoordinatorsCallBack", conductorResponseCallBackMode, id);
        }

        private async Task<T> CallFunctionAsync<T>(HoloNETRequestType requestType, string holochainConductorFunctionName, dynamic holoNETDataDetailed, Dictionary<string, TaskCompletionSource<T>> _taskCompletionCallBack, string eventCallBackName, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null) where T : HoloNETDataReceivedBaseEventArgs, new()
        {
            HoloNETData holoNETData = new HoloNETData()
            {
                type = holochainConductorFunctionName,
                data = holoNETDataDetailed
            };

            if (string.IsNullOrEmpty(id))
                id = GetRequestId();

            _taskCompletionCallBack[id] = new TaskCompletionSource<T> { };
            await SendHoloNETRequestAsync(holoNETData, requestType, id);

            if (conductorResponseCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
            {
                Task<T> returnValue = _taskCompletionCallBack[id].Task;
                return await returnValue;
            }
            else
                return new T() { EndPoint = EndPoint, Id = id, Message = $"conductorResponseCallBackMode is set to UseCallBackEvents so please wait for {eventCallBackName} event for the result." };
        }

        private void CallFunction(HoloNETRequestType requestType, string holochainConductorFunctionName, dynamic holoNETDataDetailed, string id = null)
        {
            HoloNETData holoNETData = new HoloNETData()
            {
                type = holochainConductorFunctionName,
                data = holoNETDataDetailed
            };

            if (string.IsNullOrEmpty(id))
                id = GetRequestId();

            //SendHoloNETRequest(holoNETData, HoloNETRequestType.ZomeCall, id);
            SendHoloNETRequest(holoNETData, requestType, id);
            //return new T() { EndPoint = EndPoint, Id = id, Message = $"conductorResponseCallBackMode is set to UseCallBackEvents so please wait for {eventCallBackName} event for the result." };
        }

        protected override HoloNETResponse ProcessDataReceived(WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            HoloNETResponse response = null;

            try
            {
                response = base.ProcessDataReceived(dataReceivedEventArgs);

                if (!response.IsError)
                {
                    switch (response.HoloNETResponseType)
                    {
                        case HoloNETResponseType.AdminAgentPubKeyGenerated:
                            DecodeAgentPubKeyGeneratedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppInstalled:
                            DecodeAppInstalledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppUninstalled:
                            DecodeAppUninstalledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppEnabled:
                            DecodeAppEnabledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppDisabled:
                            DecodeAppDisabledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminZomeCallCapabilityGranted:
                            DecodeZomeCallCapabilityGrantedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppInterfaceAttached:
                            DecodeAppInterfaceAttachedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppsListed:
                            DecodeAppsListedReceived(response, dataReceivedEventArgs);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in HoloNETClient.ProcessDataReceived method.";
                HandleError(msg, ex);
            }

            return response;
        }

        protected override string ProcessErrorReceivedFromConductor(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string msg = base.ProcessErrorReceivedFromConductor(response, dataReceivedEventArgs);

            if (response != null && response.id > 0 && _requestTypeLookup != null && _requestTypeLookup.ContainsKey(response.id.ToString()))
            {
                switch (_requestTypeLookup[response.id.ToString()])
                {
                    case HoloNETRequestType.AdminGenerateAgentPubKey:
                        RaiseAgentPubKeyGeneratedEvent(ProcessResponeError<AgentPubKeyGeneratedCallBackEventArgs>(response, dataReceivedEventArgs, "GenerateAgentPubKey", msg));
                        break;

                    case HoloNETRequestType.AdminInstallApp:
                        RaiseAppInstalledEvent(ProcessResponeError<AppInstalledCallBackEventArgs>(response, dataReceivedEventArgs, "InstallApp", msg));
                        break;

                    case HoloNETRequestType.AdminUninstallApp:
                        RaiseAppUninstalledEvent(ProcessResponeError<AppUninstalledCallBackEventArgs>(response, dataReceivedEventArgs, "UninstallApp", msg));
                        break;

                    case HoloNETRequestType.AdminEnableApp:
                        RaiseAppEnabledEvent(ProcessResponeError<AppEnabledCallBackEventArgs>(response, dataReceivedEventArgs, "EnableApp", msg));
                        break;

                    case HoloNETRequestType.AdminDisableApp:
                        RaiseAppDisabledEvent(ProcessResponeError<AppDisabledCallBackEventArgs>(response, dataReceivedEventArgs, "DisableApp", msg));
                        break;

                    case HoloNETRequestType.AdminGrantZomeCallCapability:
                        RaiseZomeCallCapabilityGrantedEvent(ProcessResponeError<ZomeCallCapabilityGrantedCallBackEventArgs>(response, dataReceivedEventArgs, "GrantZomeCallCapability", msg));
                        break;

                    case HoloNETRequestType.AdminAttachAppInterface:
                        RaiseAppInterfaceAttachedEvent(ProcessResponeError<AppInterfaceAttachedCallBackEventArgs>(response, dataReceivedEventArgs, "GrantZomeCallCapability", msg));
                        break;

                    case HoloNETRequestType.AdminListApps:
                        RaiseAppsListedEvent(ProcessResponeError<AppsListedCallBackEventArgs>(response, dataReceivedEventArgs, "ListApps", msg));
                        break;
                }
            }

            return msg;
        }

        private void DecodeAgentPubKeyGeneratedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            AgentPubKeyGeneratedCallBackEventArgs args = CreateHoloNETArgs<AgentPubKeyGeneratedCallBackEventArgs>(response, dataReceivedEventArgs);

            try
            {
                Logger.Log("ADMIN AGENT PUB KEY GENERATED DATA DETECTED\n", LogType.Info);
                AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);
                args.AgentPubKey = ConvertHoloHashToString(appResponse.data);
                args.AppResponse = appResponse;

                Logger.Log($"AGENT PUB KEY GENERATED: {args.AgentPubKey}\n", LogType.Info);

                if (_updateDnaHashAndAgentPubKey)
                    HoloNETDNA.AgentPubKey = args.AgentPubKey;
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeAgentPubKeyGeneratedReceived. Reason: {ex}";
                args.IsError = true;
                //args.IsCallSuccessful = false;
                args.Message = msg;
                HandleError(msg, ex);
            }

            RaiseAgentPubKeyGeneratedEvent(args);
        }

        private void DecodeAppInstalledReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            AppInstalledCallBackEventArgs args = new AppInstalledCallBackEventArgs();

            try
            {
                Logger.Log("ADMIN APP INSTALLED DATA DETECTED\n", LogType.Info);

                AppInfoResponse appInfoResponse = MessagePackSerializer.Deserialize<AppInfoResponse>(response.data, messagePackSerializerOptions);
                args = CreateHoloNETArgs<AppInstalledCallBackEventArgs>(response, dataReceivedEventArgs);

                if (appInfoResponse != null)
                {
                    appInfoResponse.data = ProcessAppInfo(appInfoResponse.data, args);
                    args.AppInfoResponse = appInfoResponse;
                }
                else
                {
                    args.Message = "Error occured in HoloNETClient.DecodeAppInfoDataReceived. appInfoResponse is null.";
                    args.IsError = true;
                }
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeAppInstalledReceived. Reason: {ex}";
                args.IsError = true;
               // args.IsCallSuccessful = false;
                args.Message = msg;
                HandleError(msg, ex);
            }

            RaiseAppInstalledEvent(args);
        }

        private void DecodeAppUninstalledReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            AppUninstalledCallBackEventArgs args = new AppUninstalledCallBackEventArgs();
            args.HoloNETResponseType = HoloNETResponseType.AdminAppUninstalled;

            try
            {
                Logger.Log("ADMIN APP UNINSTALLED DATA DETECTED\n", LogType.Info);
                args = CreateHoloNETArgs<AppUninstalledCallBackEventArgs>(response, dataReceivedEventArgs);

                if (_uninstallingAppLookup != null && _uninstallingAppLookup.ContainsKey(response.id.ToString()))
                {
                    args.InstalledAppId = _uninstallingAppLookup[response.id.ToString()];
                    _uninstallingAppLookup.Remove(response.id.ToString());
                }
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeAppUninstalledReceived. Reason: {ex}";
                args.IsError = true;
                args.Message = msg;
                HandleError(msg, ex);
            }

            RaiseAppUninstalledEvent(args);
        }

        private void DecodeAppEnabledReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppEnabledReceived. Reason: ";
            AppEnabledCallBackEventArgs args = CreateHoloNETArgs<AppEnabledCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppEnabled;

            try
            {
                Logger.Log("ADMIN APP ENABLED DATA DETECTED\n", LogType.Info);
                EnableAppResponse enableAppResponse = MessagePackSerializer.Deserialize<EnableAppResponse>(response.data, messagePackSerializerOptions);

                if (enableAppResponse != null)
                {
                    enableAppResponse.data.app = ProcessAppInfo(enableAppResponse.data.app, args);
                    args.AppInfoResponse = new AppInfoResponse() { data = enableAppResponse.data.app };
                    args.Errors = enableAppResponse.data.errors; //TODO: Need to find out what this contains and the correct data structure.
                }
                else
                {
                    string msg = $"{errorMessage} An error occurred deserialzing EnableAppResponse from the Holochain Conductor";
                    args.IsError = true;
                    //args.IsCallSuccessful = false;
                    args.Message = msg;
                    HandleError(msg);
                }
            }
            catch (Exception ex)
            {
                string msg = $"{errorMessage} {ex}";
                args.IsError = true;
                //args.IsCallSuccessful = false;
                args.Message = msg;
                HandleError(msg, ex);
            }

            RaiseAppEnabledEvent(args);
        }

        private void DecodeAppDisabledReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppEnabledReceived. Reason: ";
            AppDisabledCallBackEventArgs args = CreateHoloNETArgs<AppDisabledCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppDisabled;

            try
            {
                Logger.Log("ADMIN APP DISABLED DATA DETECTED\n", LogType.Info);

                if (_disablingAppLookup != null && _disablingAppLookup.ContainsKey(response.id.ToString()))
                {
                    args.InstalledAppId = _disablingAppLookup[response.id.ToString()];
                    _disablingAppLookup.Remove(response.id.ToString());
                }
            }
            catch (Exception ex)
            {
                string msg = $"{errorMessage} {ex}";
                args.IsError = true;
                //args.IsCallSuccessful = false;
                args.Message = msg;
                HandleError(msg, ex);
            }

            RaiseAppDisabledEvent(args);
        }

        private void DecodeZomeCallCapabilityGrantedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeZomeCallCapabilityGrantedReceived. Reason: ";
            ZomeCallCapabilityGrantedCallBackEventArgs args = CreateHoloNETArgs<ZomeCallCapabilityGrantedCallBackEventArgs>(response, dataReceivedEventArgs);

            try
            {
                Logger.Log("ADMIN ZOME CALL CAPABILITY GRANTED\n", LogType.Info);
                args.HoloNETResponseType = HoloNETResponseType.AdminZomeCallCapabilityGranted;
            }
            catch (Exception ex)
            {
                string msg = $"{errorMessage} {ex}";
                args.IsError = true;
                args.Message = msg;
                HandleError(msg, ex);
            }

            RaiseZomeCallCapabilityGrantedEvent(args);
        }

        private void DecodeAppInterfaceAttachedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppInterfaceAttachedReceived. Reason: ";
            AppInterfaceAttachedCallBackEventArgs args = CreateHoloNETArgs<AppInterfaceAttachedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppInterfaceAttached;

            try
            {
                Logger.Log("ADMIN APP INTERFACE ATTACHED\n", LogType.Info);
                //object attachAppInterfaceResponse = MessagePackSerializer.Deserialize<object>(response.data, messagePackSerializerOptions);
                AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);

                if (appResponse != null)
                {
                    args.Port = Convert.ToUInt16(appResponse.data["port"]);

                    //AttachAppInterfaceResponse attachAppInterfaceResponse = MessagePackSerializer.Deserialize<AttachAppInterfaceResponse>(appResponse.data, messagePackSerializerOptions);
                    //attachAppInterfaceResponse.Port = attachAppInterfaceResponse.Port;
                }
                else
                {
                    args.Message = "Error occured in HoloNETClient.DecodeAppInterfaceAttachedReceived. attachAppInterfaceResponse is null.";
                    args.IsError = true;
                    HandleError(args.Message);
                }
            }
            catch (Exception ex)
            {
                string msg = $"{errorMessage} {ex}";
                args.IsError = true;
                args.Message = msg;
                HandleError(msg, ex);
            }

            RaiseAppInterfaceAttachedEvent(args);
        }

        private void DecodeAppsListedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppsListedReceived. Reason: ";
            AppsListedCallBackEventArgs args = CreateHoloNETArgs<AppsListedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppInterfaceAttached;

            try
            {
                Logger.Log("ADMIN APPS LISTED\n", LogType.Info);
                //AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);
                ListAppsResponse appResponse = MessagePackSerializer.Deserialize<ListAppsResponse>(response.data, messagePackSerializerOptions);

                if (appResponse != null)
                {
                    foreach (AppInfo appInfo in appResponse.Apps)
                    {
                        AppInfoCallBackEventArgs appInfoArgs = new AppInfoCallBackEventArgs();
                        AppInfo processedAppInfo = ProcessAppInfo(appInfo, appInfoArgs);

                        if (!appInfoArgs.IsError)
                            args.Apps.Add(processedAppInfo);
                        else
                        {
                            args.IsError = true;
                            args.Message = $"{args.Message} # {appInfoArgs.Message}";
                        }
                    }

                    //foreach (Dictionary<object, object> data in appResponse.data)
                    //{
                    //    AppInfoCallBackEventArgs appInfoArgs = new AppInfoCallBackEventArgs();
                    //    AppInfo appInfo = new AppInfo();
                    //    appInfo.installed_app_id = data["installed_app_id"].ToString();
                    //    //appInfo.agent_pub_key = Convert.ToByte(data["agent_pub_key"]);


                    //    //AppInfo processedAppInfo = ProcessAppInfo(appInfo, appInfoArgs);

                    //    //if (!appInfoArgs.IsError)
                    //    //    args.Apps.Add(processedAppInfo);
                    //    //else
                    //    //{
                    //    //    args.IsError = true;
                    //    //    args.IsCallSuccessful = false;
                    //    //    args.Message = $"{args.Message} # {appInfoArgs.Message}";
                    //    //}
                    //}

                    if (args.IsError)
                        HandleError(args.Message);
                }
                else
                {
                    args.Message = "Error occured in HoloNETClient.DecodeAppsListedReceived. appResponse is null.";
                    args.IsError = true;
                    HandleError(args.Message);
                }
            }
            catch (Exception ex)
            {
                string msg = $"{errorMessage} {ex}";
                args.IsError = true;
                args.Message = msg;
                HandleError(msg, ex);
            }

            RaiseAppsListedEvent(args);
        }

        private void RaiseAgentPubKeyGeneratedEvent(AgentPubKeyGeneratedCallBackEventArgs adminAgentPubKeyGeneratedCallBackEventArgs)
        {
            LogEvent("AgentPubKeyGeneratedCallBack", adminAgentPubKeyGeneratedCallBackEventArgs);
            OnAgentPubKeyGeneratedCallBack?.Invoke(this, adminAgentPubKeyGeneratedCallBackEventArgs);

            if (_taskCompletionAgentPubKeyGeneratedCallBack != null && !string.IsNullOrEmpty(adminAgentPubKeyGeneratedCallBackEventArgs.Id) && _taskCompletionAgentPubKeyGeneratedCallBack.ContainsKey(adminAgentPubKeyGeneratedCallBackEventArgs.Id))
                _taskCompletionAgentPubKeyGeneratedCallBack[adminAgentPubKeyGeneratedCallBackEventArgs.Id].SetResult(adminAgentPubKeyGeneratedCallBackEventArgs);
        }

        private void RaiseAppInstalledEvent(AppInstalledCallBackEventArgs adminAppInstalledCallBackEventArgs)
        {
            LogEvent("AppInstalledCallBack", adminAppInstalledCallBackEventArgs);
            OnAppInstalledCallBack?.Invoke(this, adminAppInstalledCallBackEventArgs);

            if (_taskCompletionAppInstalledCallBack != null && !string.IsNullOrEmpty(adminAppInstalledCallBackEventArgs.Id) && _taskCompletionAppInstalledCallBack.ContainsKey(adminAppInstalledCallBackEventArgs.Id))
                _taskCompletionAppInstalledCallBack[adminAppInstalledCallBackEventArgs.Id].SetResult(adminAppInstalledCallBackEventArgs);
        }

        private void RaiseAppUninstalledEvent(AppUninstalledCallBackEventArgs adminAppUninstalledCallBackEventArgs)
        {
            LogEvent("AppUninstalledCallBack", adminAppUninstalledCallBackEventArgs);
            OnAppUninstalledCallBack?.Invoke(this, adminAppUninstalledCallBackEventArgs);

            if (_taskCompletionAppUninstalledCallBack != null && !string.IsNullOrEmpty(adminAppUninstalledCallBackEventArgs.Id) && _taskCompletionAppUninstalledCallBack.ContainsKey(adminAppUninstalledCallBackEventArgs.Id))
                _taskCompletionAppUninstalledCallBack[adminAppUninstalledCallBackEventArgs.Id].SetResult(adminAppUninstalledCallBackEventArgs);
        }

        private void RaiseAppEnabledEvent(AppEnabledCallBackEventArgs adminAppEnabledCallBackEventArgs)
        {
            LogEvent("AppEnabledCallBack", adminAppEnabledCallBackEventArgs);
            OnAppEnabledCallBack?.Invoke(this, adminAppEnabledCallBackEventArgs);

            if (_taskCompletionAppEnabledCallBack != null && !string.IsNullOrEmpty(adminAppEnabledCallBackEventArgs.Id) && _taskCompletionAppEnabledCallBack.ContainsKey(adminAppEnabledCallBackEventArgs.Id))
                _taskCompletionAppEnabledCallBack[adminAppEnabledCallBackEventArgs.Id].SetResult(adminAppEnabledCallBackEventArgs);
        }

        private void RaiseAppDisabledEvent(AppDisabledCallBackEventArgs adminAppDisabledCallBackEventArgs)
        {
            LogEvent("AppDisabledCallBack", adminAppDisabledCallBackEventArgs);
            OnAppDisabledCallBack?.Invoke(this, adminAppDisabledCallBackEventArgs);

            if (_taskCompletionAppDisabledCallBack != null && !string.IsNullOrEmpty(adminAppDisabledCallBackEventArgs.Id) && _taskCompletionAppDisabledCallBack.ContainsKey(adminAppDisabledCallBackEventArgs.Id))
                _taskCompletionAppDisabledCallBack[adminAppDisabledCallBackEventArgs.Id].SetResult(adminAppDisabledCallBackEventArgs);
        }

        private void RaiseZomeCallCapabilityGrantedEvent(ZomeCallCapabilityGrantedCallBackEventArgs adminZomeCallCapabilityGrantedCallBackEventArgs)
        {
            LogEvent("ZomeCallCapabilityGranted", adminZomeCallCapabilityGrantedCallBackEventArgs);
            OnZomeCallCapabilityGrantedCallBack?.Invoke(this, adminZomeCallCapabilityGrantedCallBackEventArgs);

            if (_taskCompletionZomeCapabilityGrantedCallBack != null && !string.IsNullOrEmpty(adminZomeCallCapabilityGrantedCallBackEventArgs.Id) && _taskCompletionZomeCapabilityGrantedCallBack.ContainsKey(adminZomeCallCapabilityGrantedCallBackEventArgs.Id))
                _taskCompletionZomeCapabilityGrantedCallBack[adminZomeCallCapabilityGrantedCallBackEventArgs.Id].SetResult(adminZomeCallCapabilityGrantedCallBackEventArgs);
        }

        private void RaiseAppInterfaceAttachedEvent(AppInterfaceAttachedCallBackEventArgs adminAppInterfaceAttachedCallBackEventArgs)
        {
            LogEvent("AppInterfaceAttached", adminAppInterfaceAttachedCallBackEventArgs);
            OnAppInterfaceAttachedCallBack?.Invoke(this, adminAppInterfaceAttachedCallBackEventArgs);

            if (_taskCompletionAppInterfaceAttachedCallBack != null && !string.IsNullOrEmpty(adminAppInterfaceAttachedCallBackEventArgs.Id) && _taskCompletionAppInterfaceAttachedCallBack.ContainsKey(adminAppInterfaceAttachedCallBackEventArgs.Id))
                _taskCompletionAppInterfaceAttachedCallBack[adminAppInterfaceAttachedCallBackEventArgs.Id].SetResult(adminAppInterfaceAttachedCallBackEventArgs);
        }

        private void RaiseAppsListedEvent(AppsListedCallBackEventArgs adminAppsListedCallBackEventArgs)
        {
            LogEvent("AppsListed", adminAppsListedCallBackEventArgs);
            OnAppsListedCallBack?.Invoke(this, adminAppsListedCallBackEventArgs);

            if (_taskCompletionAppsListedCallBack != null && !string.IsNullOrEmpty(adminAppsListedCallBackEventArgs.Id) && _taskCompletionAppsListedCallBack.ContainsKey(adminAppsListedCallBackEventArgs.Id))
                _taskCompletionAppsListedCallBack[adminAppsListedCallBackEventArgs.Id].SetResult(adminAppsListedCallBackEventArgs);
        }
    }
}