using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using MessagePack;
using NextGenSoftware.WebSocket;
using NextGenSoftware.Logging;
using NextGenSoftware.Utilities;
using NextGenSoftware.Utilities.ExtentionMethods;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Properties;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract class HoloNETClientBase : IHoloNETClientBase
    {
        private bool _shuttingDownHoloNET = false;
        private TaskCompletionSource<DisconnectedEventArgs> _taskCompletionDisconnected = new TaskCompletionSource<DisconnectedEventArgs>();
        private TaskCompletionSource<HoloNETShutdownEventArgs> _taskCompletionHoloNETShutdown = new TaskCompletionSource<HoloNETShutdownEventArgs>();
        private HoloNETDNA _holoNETDNA = null;
        private Process _conductorProcess = null;
        private int _conductorProcessSessionId = 0;
        private ShutdownHolochainConductorsMode _shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings;

        protected int _currentId = 0;
        protected MessagePackSerializerOptions messagePackSerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
        protected List<string> _pendingRequests = new List<string>();
        protected bool _connectingAsync = false;
        protected bool _updateDnaHashAndAgentPubKey = true;
        protected static Dictionary<string, SigningCredentials> _signingCredentialsForCell = new Dictionary<string, SigningCredentials>();
        protected Dictionary<string, HoloNETRequestType> _requestTypeLookup = new Dictionary<string, HoloNETRequestType>();

        //[DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

        //Events
        public delegate void Connected(object sender, ConnectedEventArgs e);

        /// <summary>
        /// Fired when the client has successfully connected to the Holochain conductor.
        /// </summary>
        public event Connected OnConnected;

        public delegate void Disconnected(object sender, DisconnectedEventArgs e);

        /// <summary>
        /// Fired when the client disconnected from the Holochain conductor.
        /// </summary>
        public event Disconnected OnDisconnected;


        public delegate void HolochainConductorStarting(object sender, HolochainConductorStartingEventArgs e);

        /// <summary>
        /// Fired when the Holochain Conductor has been started.
        /// </summary>
        public event HolochainConductorStarting OnHolochainConductorStarting;


        public delegate void HolochainConductorStarted(object sender, HolochainConductorStartedEventArgs e);

        /// <summary>
        /// Fired when the Holochain Conductor has been started.
        /// </summary>
        public event HolochainConductorStarted OnHolochainConductorStarted;


        public delegate void HolochainConductorsShutdownComplete(object sender, HolochainConductorsShutdownEventArgs e);

        /// <summary>
        /// Fired when all Holochain Conductors have been shutdown.
        /// </summary>
        public event HolochainConductorsShutdownComplete OnHolochainConductorsShutdownComplete;



        public delegate void HoloNETShutdownComplete(object sender, HoloNETShutdownEventArgs e);

        /// <summary>
        /// Fired when HoloNET has completed shutting down (this includes closing all connections and shutting down all Holochain Conductor).
        /// </summary>
        public event HoloNETShutdownComplete OnHoloNETShutdownComplete;


        public delegate void DataReceived(object sender, HoloNETDataReceivedEventArgs e);

        /// <summary>
        /// Fired when any data is received from the Holochain conductor. This returns the raw data.                                     
        /// </summary>
        public event DataReceived OnDataReceived;


        public delegate void DataSent(object sender, HoloNETDataSentEventArgs e);

        /// <summary>
        /// Fired when any data is sent to the Holochain conductor.
        /// </summary>
        public event DataSent OnDataSent;


        public delegate void Error(object sender, HoloNETErrorEventArgs e);

        /// <summary>
        /// Fired when an error occurs, check the params for the cause of the error.
        /// </summary>
        public event Error OnError;

        // Properties

        /// <summary>
        /// This property contains the internal NextGenSoftware.WebSocket (https://www.nuget.org/packages/NextGenSoftware.WebSocket) that HoloNET uses. You can use this property to access the current state of the WebSocket as well as configure more options.
        /// </summary>
        public WebSocket.WebSocket WebSocket { get; set; }

        public Logger Logger { get; set; } = new Logger();

        /// <summary>
        /// This property contains a struct called HoloNETHoloNETDNA containing the sub-properties: AgentPubKey, DnaHash, FullPathToRootHappFolder, FullPathToCompiledHappFolder, HolochainConductorMode, FullPathToExternalHolochainConductorBinary, FullPathToExternalHCToolBinary, SecondsToWaitForHolochainConductorToStart, AutoStartHolochainConductor, ShowHolochainConductorWindow, AutoShutdownHolochainConductor, ShutDownALLHolochainConductors, HolochainConductorToUse, OnlyAllowOneHolochainConductorToRunAtATime, LoggingMode & ErrorHandlingBehaviour.
        /// </summary>
        public HoloNETDNA HoloNETDNA
        {
            get
            {
                if (!HoloNETDNAManager.IsLoaded)
                    HoloNETDNAManager.LoadDNA();

                return HoloNETDNAManager.HoloNETDNA;

                //if (!_holoNETDNAManager.IsLoaded)
                //    _holoNETDNAManager.LoadDNA();

                //return _holoNETDNAManager.HoloNETDNA;
            }
            set
            {
                //_holoNETDNAManager.HoloNETDNA = value;
                HoloNETDNAManager.HoloNETDNA = value;
            }
        }

        public bool IsHoloNETDNALoaded
        {
            get
            {
                return HoloNETDNAManager.IsLoaded;
                //return _holoNETDNAManager.IsLoaded;
                //return _holoNETDNAManager.IsLoaded;
            }
        }

        /// <summary>
        /// This property is a shortcut to the State property on the WebSocket property.
        /// </summary>
        public WebSocketState State
        {
            get
            {
                return WebSocket.State;
            }
        }

        public bool IsConnecting { get; set; }
        public bool IsDisconnecting { get; set; }

        /// <summary>
        /// This property is a shortcut to the EndPoint property on the WebSocket property.
        /// </summary>
        public Uri EndPoint
        {
            get
            {
                if (WebSocket != null)
                    return WebSocket.EndPoint;

                return null;
            }
            set
            {
                if (WebSocket != null)
                    WebSocket.EndPoint = value;
            }
        }


        /// <summary>
        /// This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientBase(HoloNETDNA holoNETDNA = null)
        {
            if (holoNETDNA != null)
                HoloNETDNA = holoNETDNA;

            InitLogger();
            Logger.AddLogProvider(new DefaultLogger(HoloNETDNA.LogToConsole, HoloNETDNA.LogToFile, HoloNETDNA.LogPath, HoloNETDNA.LogFileName, HoloNETDNA.AddAdditionalSpaceAfterEachLogEntry, HoloNETDNA.ShowColouredLogs, HoloNETDNA.DebugColour, HoloNETDNA.InfoColour, HoloNETDNA.WarningColour, HoloNETDNA.ErrorColour, HoloNETDNA.NumberOfRetriesToLogToFile, HoloNETDNA.RetryLoggingToFileEverySeconds));
            Init(new Uri(HoloNETDNA.HolochainConductorAppAgentURI));
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProvider">The implementation of the ILogProvider interface (custom logProvider).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientBase(ILogProvider logProvider, bool alsoUseDefaultLogger = false, HoloNETDNA holoNETDNA = null)
        {
            if (holoNETDNA != null)
                HoloNETDNA = holoNETDNA;

            InitLogger();
            Logger.AddLogProvider(logProvider);

            if (alsoUseDefaultLogger)
                Logger.AddLogProvider(new DefaultLogger(HoloNETDNA.LogToConsole, HoloNETDNA.LogToFile, HoloNETDNA.LogPath, HoloNETDNA.LogFileName, HoloNETDNA.AddAdditionalSpaceAfterEachLogEntry, HoloNETDNA.ShowColouredLogs, HoloNETDNA.DebugColour, HoloNETDNA.InfoColour, HoloNETDNA.WarningColour, HoloNETDNA.ErrorColour, HoloNETDNA.NumberOfRetriesToLogToFile, HoloNETDNA.RetryLoggingToFileEverySeconds));

            Init(new Uri(HoloNETDNA.HolochainConductorAppAgentURI));
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProviders">The implementations of the ILogProvider interface (custom logProviders).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom loggers injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientBase(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, HoloNETDNA holoNETDNA = null)
        {
            if (holoNETDNA != null)
                HoloNETDNA = holoNETDNA;

            InitLogger();
            Logger.AddLogProviders(new List<ILogProvider>(logProviders));

            if (alsoUseDefaultLogger)
                Logger.AddLogProvider(new DefaultLogger(HoloNETDNA.LogToConsole, HoloNETDNA.LogToFile, HoloNETDNA.LogPath, HoloNETDNA.LogFileName, HoloNETDNA.AddAdditionalSpaceAfterEachLogEntry, HoloNETDNA.ShowColouredLogs, HoloNETDNA.DebugColour, HoloNETDNA.InfoColour, HoloNETDNA.WarningColour, HoloNETDNA.ErrorColour, HoloNETDNA.NumberOfRetriesToLogToFile, HoloNETDNA.RetryLoggingToFileEverySeconds));

            Init(new Uri(HoloNETDNA.HolochainConductorAppAgentURI));
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logger">The logger instance to use.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientBase(Logger logger, HoloNETDNA holoNETDNA = null)
        {
            if (holoNETDNA != null)
                HoloNETDNA = holoNETDNA;

            InitLogger(logger);
            Init(new Uri(HoloNETDNA.HolochainConductorAppAgentURI));
        }

        public virtual HoloNETDNA LoadDNA()
        {
            //return _holoNETDNAManager.LoadDNA();
            return HoloNETDNAManager.LoadDNA();
        }

        public virtual bool SaveDNA()
        {
            //return _holoNETDNAManager.SaveDNA();
            return HoloNETDNAManager.SaveDNA();
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
        public virtual async Task<HoloNETConnectEventArgs> ConnectAsync(Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
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
        public virtual HoloNETConnectEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
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
        public virtual async Task<HoloNETConnectEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETConnectEventArgs result = new HoloNETConnectEventArgs();

            try
            {
                if (IsConnecting)
                {
                    result.Message = "HoloNET Is Already Connecting...";
                    result.IsWarning = true;
                    return result;
                }

                IsConnecting = true;

                //if ((string.IsNullOrEmpty(holochainConductorURI) && EndPoint == null) || (EndPoint != null && !EndPoint.IsUnc))
                if (string.IsNullOrEmpty(holochainConductorURI) && EndPoint == null)
                    throw new HoloNETException("ERROR: No Holochain Conductor URI has been specified. Either pass it in through a contructor, set the EndPoint property or as a param to one of the Connect methods.");

                if (Logger.LogProviders.Count == 0)
                    throw new HoloNETException("ERROR: No LogProvider Has Been Specified! Please set a LogProvider with the Logger.AddLogProvider method.");

                if (string.IsNullOrEmpty(holochainConductorURI))
                    holochainConductorURI = EndPoint.AbsoluteUri;

                this.EndPoint = new Uri(holochainConductorURI);

                if (WebSocket.State != WebSocketState.Connecting && WebSocket.State != WebSocketState.Open && WebSocket.State != WebSocketState.Aborted)
                {
                    if (HoloNETDNA.AutoStartHolochainConductor)
                        await StartHolochainConductorAsync();

                    if (connectedCallBackMode == ConnectedCallBackMode.WaitForHolochainConductorToConnect)
                    {
                        _connectingAsync = true;
                        await WebSocket.ConnectAsync(new Uri(holochainConductorURI));

                        if (State == WebSocketState.Open)
                            result.IsConnected = true;
                    }
                    else
                    {
                        _connectingAsync = false;
                        WebSocket.ConnectAsync(new Uri(holochainConductorURI));

                        result.Message = "connectedCallBackMode is set to UseCallBackEvents so please wait for the OnConnected event for the result.";
                        result.IsWarning = true;

                        //if (retrieveAgentPubKeyAndDnaHashMode != RetrieveAgentPubKeyAndDnaHashMode.DoNotRetreive)
                        //    await RetrieveAgentPubKeyAndDnaHashAsync(retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
                    }
                }
            }
            catch (Exception e)
            {
                result.IsError = true;
                result.Message = $"Error occurred in HoloNETClient.Connect method connecting to {WebSocket.EndPoint}. Reason: {e.Message}";
                HandleError(string.Concat("Error occurred in HoloNETClient.Connect method connecting to ", WebSocket.EndPoint), e);
            }

            return result;
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
        public virtual HoloNETConnectEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            return ConnectAsync(holochainConductorURI, ConnectedCallBackMode.UseCallBackEvents, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved).Result;
        }

        /// <summary>
        /// This method will start the Holochain Conducutor using the appropriate settings defined in the HoloNETDNA property.
        /// </summary>
        /// <returns></returns>
        public virtual async Task StartHolochainConductorAsync()
        {
            string fullPathToHcExe = "hc.exe";
            string fullPathToHolochainExe = "holochain.exe";

            try
            {
                // Was used when they were set to Content rather than Embedded.
                string fullPathToEmbeddedHolochainConductorBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\holochain.exe");
                string fullPathToEmbeddedHCToolBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\hc.exe");

                OnHolochainConductorStarting?.Invoke(this, new HolochainConductorStartingEventArgs());
                _conductorProcess = new Process();

                if (string.IsNullOrEmpty(HoloNETDNA.FullPathToExternalHolochainConductorBinary) && HoloNETDNA.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                    throw new ArgumentNullException("FullPathToExternalHolochainConductorBinary", "When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HolochainProductionConductor', FullPathToExternalHolochainConductorBinary cannot be empty.");

                if (string.IsNullOrEmpty(HoloNETDNA.FullPathToExternalHCToolBinary) && HoloNETDNA.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    throw new ArgumentNullException("FullPathToExternalHCToolBinary", "When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HcDevTool', FullPathToExternalHCToolBinary cannot be empty.");

                if (!File.Exists(HoloNETDNA.FullPathToExternalHolochainConductorBinary) && HoloNETDNA.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HolochainProductionConductor', FullPathToExternalHolochainConductorBinary ({HoloNETDNA.FullPathToExternalHolochainConductorBinary}) must point to a valid file.");

                if (!File.Exists(HoloNETDNA.FullPathToExternalHCToolBinary) && HoloNETDNA.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HcDevTool', FullPathToExternalHCToolBinary ({HoloNETDNA.FullPathToExternalHCToolBinary}) must point to a valid file.");

                //if (!File.Exists(fullPathToEmbeddedHolochainConductorBinary) && HoloNETDNA.HolochainConductorMode == HolochainConductorModeEnum.UseEmbedded && HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                //    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseEmbedded' and HolochainConductorToUse is set to 'HolochainProductionConductor', you must ensure the holochain.exe is found here: {fullPathToEmbeddedHolochainConductorBinary}.");

                //if (!File.Exists(fullPathToEmbeddedHCToolBinary) && HoloNETDNA.HolochainConductorMode == HolochainConductorModeEnum.UseEmbedded && HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                //    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseEmbedded' and HolochainConductorToUse is set to 'HcDevTool', you must ensure the hc.exe is found here: {fullPathToEmbeddedHCToolBinary}.");

                if (!string.IsNullOrEmpty(HoloNETDNA.FullPathToRootHappFolder) && !Directory.Exists(HoloNETDNA.FullPathToRootHappFolder) && HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    throw new DirectoryNotFoundException($"The path for HoloNETDNA.FullPathToRootHappFolder ({HoloNETDNA.FullPathToRootHappFolder}) was not found.");

                if (!string.IsNullOrEmpty(HoloNETDNA.FullPathToCompiledHappFolder) && !Directory.Exists(HoloNETDNA.FullPathToCompiledHappFolder) && HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    throw new DirectoryNotFoundException($"The path for HoloNETDNA.FullPathToCompiledHappFolder ({HoloNETDNA.FullPathToCompiledHappFolder}) was not found.");


                if (HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                {
                    switch (HoloNETDNA.HolochainConductorMode)
                    {
                        case HolochainConductorModeEnum.UseExternal:
                            fullPathToHcExe = HoloNETDNA.FullPathToExternalHCToolBinary;
                            break;

                        case HolochainConductorModeEnum.UseEmbedded:
                            {
                                //throw new InvalidOperationException("You must install the Embedded version if you wish to use HolochainConductorMode.UseEmbedded.");

                                //_conductorProcess.StartInfo.FileName = fullPathToEmbeddedHCToolBinary;

                                fullPathToHcExe = Path.Combine(Directory.GetCurrentDirectory(), "hc.exe");

                                if (!File.Exists(fullPathToHcExe))
                                {
                                    using (FileStream fsDst = new FileStream(fullPathToHcExe, FileMode.CreateNew, FileAccess.Write))
                                    {
                                        byte[] bytes = Resources.hc;
                                        fsDst.Write(bytes, 0, bytes.Length);
                                    }
                                }
                            }
                            break;

                        case HolochainConductorModeEnum.UseSystemGlobal:
                            fullPathToHcExe = "hc.exe";
                            break;
                    }
                }
                else
                {
                    switch (HoloNETDNA.HolochainConductorMode)
                    {
                        case HolochainConductorModeEnum.UseExternal:
                            _conductorProcess.StartInfo.FileName = HoloNETDNA.FullPathToExternalHolochainConductorBinary;
                            break;

                        case HolochainConductorModeEnum.UseEmbedded:
                            {
                                //throw new InvalidOperationException("You must install the Embedded version if you wish to use HolochainConductorMode.UseEmbedded.");

                                //_conductorProcess.StartInfo.FileName = fullPathToEmbeddedHolochainConductorBinary;

                                fullPathToHolochainExe = Path.Combine(Directory.GetCurrentDirectory(), "holochain.exe");
                                //fullPathToHolochainExe = Path.Combine(Directory.GetCurrentDirectory(), "HolochainBinaries/beta/holochain.exe");

                                if (!File.Exists(fullPathToHolochainExe))
                                {
                                    using (FileStream fsDst = new FileStream(fullPathToHolochainExe, FileMode.CreateNew, FileAccess.Write))
                                    {
                                        byte[] bytes = Resources.holochain;
                                        fsDst.Write(bytes, 0, bytes.Length);
                                    }
                                }
                            }
                            break;

                        case HolochainConductorModeEnum.UseSystemGlobal:
                            fullPathToHolochainExe = "holochain.exe";
                            break;
                    }
                }

                //Make sure the condctor is not already running
                if (!HoloNETDNA.OnlyAllowOneHolochainConductorToRunAtATime || (HoloNETDNA.OnlyAllowOneHolochainConductorToRunAtATime && !Process.GetProcesses().Any(x => x.ProcessName == _conductorProcess.StartInfo.FileName)))
                {
                    Logger.Log("Starting Holochain Conductor...", LogType.Info, true);

                    if (HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    {
                        _conductorProcess.StartInfo.WorkingDirectory = HoloNETDNA.FullPathToRootHappFolder;
                        //_conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'' | {fullPathToHcExe} s  generate {HoloNETDNA.FullPathToCompiledHappFolder} --piped -f={EndPoint.Port}\"";
                        _conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'' | {fullPathToHcExe} s --piped  generate {HoloNETDNA.FullPathToCompiledHappFolder} --run={EndPoint.Port}\"";
                    }

                    if (HoloNETDNA.ShowHolochainConductorWindow)
                    {
                        _conductorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        _conductorProcess.StartInfo.CreateNoWindow = false;
                    }
                    else
                    {
                        _conductorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        _conductorProcess.StartInfo.CreateNoWindow = true;
                    }


                    _conductorProcess.StartInfo.FileName = "PowerShell.exe";
                    //_conductorProcess.StartInfo.FileName = fullPathToHolochainExe;
                    //_conductorProcess.StartInfo.WorkingDirectory = fullPathToHolochainExe;

                    _conductorProcess.StartInfo.UseShellExecute = true;
                    _conductorProcess.StartInfo.RedirectStandardOutput = false;
                    // _conductorProcess.Start();

                    if (HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    {
                        //_conductorProcess.Start();
                        //await Task.Delay(3000);
                        //_conductorProcess.Close();

                        //_conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'' | {fullPathToHcExe} s --piped -f={EndPoint.Port} run 0\"";
                        ////TrueProcessStart("PowerShell.exe -NoExit -Command \"'' | {fullPathToHcExe} s --piped -f={EndPoint.Port} run 0\"");
                    }
                    else
                    {
                        //If the conductor config file does not exist in the default location then we need to set the conductor up now
                        if (!File.Exists(HoloNETDNA.HolochainConductorConfigPath))
                        {
                            _conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'y' | {fullPathToHolochainExe} --piped -i\"";
                            _conductorProcess.Start();
                            _conductorProcessSessionId = _conductorProcess.Id;

                            await Task.Delay(3000);
                            await ShutDownAllHolochainConductorsAsync();
                            _conductorProcess.Close();
                        }

                        //Once the config file has been created we can update it with the correct port and then launch it.
                        if (File.Exists(HoloNETDNA.HolochainConductorConfigPath))
                        {
                            UpdateHolochainConductorConfigFile();

                            //TrueProcessStart("PowerShell.exe -NoExit -Command \"'' | {fullPathToHolochainExe} --piped\"");
                            _conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'' | {fullPathToHolochainExe} --piped\"";
                        }
                    }

                    _conductorProcess.Start();
                    _conductorProcessSessionId = _conductorProcess.Id;

                    await Task.Delay(HoloNETDNA.SecondsToWaitForHolochainConductorToStart * 1000); // Give the conductor 5 (default) seconds to start up...
                    OnHolochainConductorStarted?.Invoke(this, new HolochainConductorStartedEventArgs());
                }
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.StartConductor method.", ex);
            }
        }

        /// <summary>
        /// This method will start the Holochain Conducutor using the appropriate settings defined in the HoloNETDNA property.
        /// </summary>
        /// <returns></returns>
        public virtual void StartHolochainConductor()
        {
            StartHolochainConductorAsync().Wait();
        }

        //TODO: Try and get this working because _conductorProcess.Id is not set by Windows when using Powershell etc so not able to shut it down correctly.
        /*Modified Process.Start*/
        //public static Process TrueProcessStart(string filename)
        //{
        //    ProcessStartInfo psi;
        //    string ext = System.IO.Path.GetExtension(filename);//get extension

        //    var sb = new StringBuilder(500);//buffer for exe file path
        //    uint size = 500;//buffer size

        //    /*Get associated app*/
        //    uint res = AssocQueryString(AssocF.None, AssocStr.Executable, ext, null, sb, ref size);

        //    if (res != 0)
        //    {
        //        Debug.WriteLine("AssocQueryString returned error: " + res.ToString("X"));
        //        psi = new ProcessStartInfo(filename);//can't get app, use standard method
        //    }
        //    else
        //    {
        //        psi = new ProcessStartInfo(sb.ToString(), filename);
        //    }

        //    return Process.Start(psi);//actually start process
        //}

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        /// <returns></returns>
        public virtual async Task SendHoloNETRequestAsync(HoloNETData holoNETData, HoloNETRequestType requestType, string id = "")
        {
            await SendHoloNETRequestAsync(MessagePackSerializer.Serialize(holoNETData), requestType, id);
        }

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        public virtual void SendHoloNETRequest(HoloNETData holoNETData, HoloNETRequestType requestType, string id = "")
        {
            SendHoloNETRequestAsync(holoNETData, requestType, id);
        }

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        public virtual async Task SendHoloNETRequestAsync(byte[] data, HoloNETRequestType requestType, string id = "")
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    id = GetRequestId();

                HoloNETRequest request = new HoloNETRequest()
                {
                    id = Convert.ToUInt64(id),
                    type = "request",
                    data = data
                };

                if (HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour != EnforceRequestToResponseIdMatchingBehaviour.Ignore)
                    _pendingRequests.Add(id);

                _requestTypeLookup[id] = requestType;

                if (WebSocket.State == WebSocketState.Open)
                {
                    Logger.Log("Sending HoloNET Request to Holochain Conductor...", LogType.Info, true);
                    await WebSocket.SendRawDataAsync(MessagePackSerializer.Serialize(request)); //This is the fastest and most popular .NET MessagePack Serializer.
                    //await WebSocket.UnityWebSocket.Send(MessagePackSerializer.Serialize(request));
                    Logger.Log("HoloNET Request Successfully Sent To Holochain Conductor.", LogType.Info, false);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.SendHoloNETRequest method.", ex);
            }
        }

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        public virtual void SendHoloNETRequest(byte[] data, HoloNETRequestType requestType, string id = "")
        {
            SendHoloNETRequestAsync(data, requestType, id);
        }

        /// <summary>
        /// This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownAllHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected event once it is disconnected.
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        /// <returns></returns>
        public virtual async Task DisconnectAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
        {
            if (IsDisconnecting)
                return;

            IsDisconnecting = true;
            _taskCompletionDisconnected = new TaskCompletionSource<DisconnectedEventArgs>();

            _shutdownHolochainConductorsMode = shutdownHolochainConductorsMode;
            await WebSocket.DisconnectAsync();

            if (disconnectedCallBackMode == DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect)
                await _taskCompletionDisconnected.Task;
        }

        /// <summary>
        /// This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownAllHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected event once it is disconnected.
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        /// <returns></returns>
        public virtual void Disconnect(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
        {
            DisconnectAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);
        }


        /// <summary>
        /// Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the CallZomeFunction methods if the `cacheData` param was set to true for any of the calls.
        /// </summary>
        public virtual void ClearCache(bool clearPendingRequsts = false)
        {
            if (clearPendingRequsts)
                _pendingRequests.Clear();
        }

        /// <summary>
        /// Utility method to convert a string to base64 encoded bytes (Holochain Conductor format). This is used to convert the AgentPubKey & DnaHash when making a zome call.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public virtual byte[] ConvertHoloHashToBytes(string hash)
        {
            try
            {
                return Convert.FromBase64String(hash.Replace('-', '+').Replace('_', '/').Substring(1, hash.Length - 1)); //also remove the u prefix.
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Utility method to convert from base64 bytes (Holochain Conductor format) to a friendly C# format. This is used to convert the AgentPubKey & DnaHash retrieved from the Conductor.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public virtual string ConvertHoloHashToString(byte[] bytes)
        {
            try
            {
                return string.Concat("u", Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_"));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public virtual byte[][] GetCellId(string DnaHash, string AgentPubKey)
        {
            return new byte[2][] { ConvertHoloHashToBytes(DnaHash), ConvertHoloHashToBytes(AgentPubKey) };
        }

        public virtual byte[][] GetCellId(byte[] DnaHash, byte[] AgentPubKey)
        {
            return new byte[2][] { DnaHash, AgentPubKey };
        }

        public virtual async Task<byte[][]> GetCellIdAsync()
        {
            if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
            {
                HandleError("Error occured in GetCellId. HoloNETDNA.AgentPubKey is null, please set before calling this method.", null);
                return null;
            }

            if (string.IsNullOrEmpty(HoloNETDNA.DnaHash))
            {
                HandleError("Error occured in GetCellId. HoloNETDNA.DnaHash is null, please set before calling this method.", null);
                return null;
            }

            return GetCellId(HoloNETDNA.DnaHash, HoloNETDNA.AgentPubKey);
        }

        public virtual byte[][] GetCellId()
        {
            if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
            {
                HandleError("Error occured in GetCellId. HoloNETDNA.AgentPubKey is null, please set before calling this method.", null);
                return null;
            }

            if (string.IsNullOrEmpty(HoloNETDNA.DnaHash))
            {
                HandleError("Error occured in GetCellId. HoloNETDNA.DnaHash is null, please set before calling this method.", null);
                return null;
            }

            return GetCellId(HoloNETDNA.DnaHash, HoloNETDNA.AgentPubKey);
        }

        /// <summary>
        /// This method will shutdown HoloNET by first calling the Disconnect method to disconnect from the Holochain Conductor and then calling the ShutDownHolochainConductors method to shutdown any running Holochain Conductors. This method will then raise the OnHoloNETShutdown event. This method works very similar to the Disconnect method except it also clears the loggers, does any other shutdown tasks necessary and then returns a `HoloNETShutdownEventArgs` object. You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller. 
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the OnDisconnected once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the [ShutDownAllHolochainConductors](#ShutDownAllHolochainConductors) method if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the [ShutDownConductors](#ShutDownConductors) method below for more detail.</param>
        /// <returns></returns>
        public virtual async Task<HoloNETShutdownEventArgs> ShutdownHoloNETAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
        {
            _shuttingDownHoloNET = true;
            _taskCompletionHoloNETShutdown = new TaskCompletionSource<HoloNETShutdownEventArgs>();

            if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
                await DisconnectAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);

            if (disconnectedCallBackMode == DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect)
                return await _taskCompletionHoloNETShutdown.Task;
            else
                return new HoloNETShutdownEventArgs(EndPoint, HoloNETDNA.DnaHash, HoloNETDNA.AgentPubKey, null);
        }

        /// <summary>
        /// This method will shutdown HoloNET by first calling the Disconnect method to disconnect from the Holochain Conductor and then calling the ShutDownHolochainConductors method to shutdown any running Holochain Conductors. This method will then raise the OnHoloNETShutdown event. This method works very similar to the Disconnect method except it also clears the loggers, does any other shutdown tasks necessary and then returns a `HoloNETShutdownEventArgs` object. You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller. 
        /// </summary>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the [ShutDownAllHolochainConductors](#ShutDownAllHolochainConductors) method if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the [ShutDownConductors](#ShutDownConductors) method below for more detail.</param>
        /// <returns></returns>
        public virtual HoloNETShutdownEventArgs ShutdownHoloNET(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
        {
            _shuttingDownHoloNET = true;

            if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
                Disconnect(DisconnectedCallBackMode.UseCallBackEvents, shutdownHolochainConductorsMode);

            return new HoloNETShutdownEventArgs(EndPoint, HoloNETDNA.DnaHash, HoloNETDNA.AgentPubKey, null);
        }

        /// <summary>
        /// Will automatically shutdown the current Holochain Conductor (if the `shutdownHolochainConductorsMode` param is set to `ShutdownCurrentConductorOnly`) or all Holochain Conductors (if the `shutdownHolochainConductorsMode` param is set to `ShutdownAllConductors`). If the `shutdownHolochainConductorsMode` param is set to `UseHoloNETDNASettings` then it will use the `HoloNETClient.HoloNETDNA.AutoShutdownHolochainConductor` and `HoloNETClient.HoloNETDNA.ShutDownALLHolochainConductors` flags to determine which mode to use. The Disconnect method will automatically call this once it has finished disconnecting from the Holochain Conductor. The ShutdownHoloNET will also call this method.
        /// </summary>
        /// <param name="shutdownHolochainConductorsMode">If this flag is set to `ShutdownCurrentConductorOnly` it will shutdown the currently running Holochain Conductor only. If it is set to `ShutdownAllConductors` it will shutdown all running Holochain Conductors. If it is set to `UseHoloNETDNASettings` (default) then it will use the `HoloNETClient.HoloNETDNA.AutoShutdownHolochainConductor` and `HoloNETClient.HoloNETDNA.ShutDownALLHolochainConductors` flags to determine which mode to use.</param>
        /// <returns></returns>
        public virtual async Task<HolochainConductorsShutdownEventArgs> ShutDownHolochainConductorsAsync(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
        {
            HolochainConductorsShutdownEventArgs holochainConductorsShutdownEventArgs = new HolochainConductorsShutdownEventArgs();
            holochainConductorsShutdownEventArgs.AgentPubKey = HoloNETDNA.AgentPubKey;
            holochainConductorsShutdownEventArgs.DnaHash = HoloNETDNA.DnaHash;
            holochainConductorsShutdownEventArgs.EndPoint = EndPoint;

            try
            {
                // Close any conductors down if necessary.
                if ((HoloNETDNA.AutoShutdownHolochainConductor && _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownCurrentConductorOnly
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownAllConductors)
                {
                    if ((HoloNETDNA.ShutDownALLHolochainConductors && _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownAllConductors)
                        holochainConductorsShutdownEventArgs = await ShutDownAllHolochainConductorsAsync();

                    else
                    {
                        Logger.Log("Shutting Down Holochain Conductor...", LogType.Info, true);
                        List<Process> processes = Process.GetProcesses().Where(x => x.ProcessName == "PowerShell.exe" && x.StartInfo.Arguments.Contains("hc")).ToList();

                        foreach (Process process in processes)
                        {
                            if (HoloNETDNA.ShowHolochainConductorWindow)
                            {
                                if (process.SessionId > 0)
                                    process.CloseMainWindow();
                            }

                            if (process.SessionId > 0)
                            {
                                process.Kill();
                                process.Close();

                                // _conductorProcess.WaitForExit();
                                process.Dispose();
                            }

                            if (process.StartInfo.Arguments.Contains("hc.exe"))
                                holochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown = 1;

                            else if (_conductorProcess.StartInfo.Arguments.Contains("holochain.exe"))
                                holochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown = 1;
                        }

                        List<Process> processes2 = Process.GetProcesses().Where(x => x.ProcessName == "hc.exe").ToList();
                        List<Process> processes3 = Process.GetProcesses().Where(x => x.ProcessName == "holochain.exe").ToList();

                        List<Process> processes4 = Process.GetProcesses().ToList();

                        Logger.Log("Holochain Conductor Successfully Shutdown.", LogType.Info);
                    }

                    //else if (_conductorProcess != null)
                    //{
                    //    Logger.Log("Shutting Down Holochain Conductor...", LogType.Info, true);

                    //    if (HoloNETDNA.ShowHolochainConductorWindow)
                    //    {
                    //        if (_conductorProcess.SessionId > 0)
                    //            _conductorProcess.CloseMainWindow();
                    //    }

                    //    if (_conductorProcess.SessionId > 0)
                    //    {
                    //        _conductorProcess.Kill();
                    //        _conductorProcess.Close();

                    //        // _conductorProcess.WaitForExit();
                    //        _conductorProcess.Dispose();
                    //    }

                    //    if (_conductorProcess.StartInfo.FileName.Contains("hc.exe"))
                    //        holochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown = 1;

                    //    else if (_conductorProcess.StartInfo.FileName.Contains("holochain.exe"))
                    //        holochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown = 1;

                    //    Logger.Log("Holochain Conductor Successfully Shutdown.", LogType.Info);
                    //}
                }

                OnHolochainConductorsShutdownComplete?.Invoke(this, holochainConductorsShutdownEventArgs);

                if (_shuttingDownHoloNET)
                {
                    Logger.LogProviders.Clear();

                    HoloNETShutdownEventArgs holoNETShutdownEventArgs = new HoloNETShutdownEventArgs(this.EndPoint, HoloNETDNA.DnaHash, HoloNETDNA.AgentPubKey, holochainConductorsShutdownEventArgs);
                    OnHoloNETShutdownComplete?.Invoke(this, holoNETShutdownEventArgs);
                    _taskCompletionHoloNETShutdown.SetResult(holoNETShutdownEventArgs);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.ShutDownConductorsInternal method.", ex);
            }

            return holochainConductorsShutdownEventArgs;
        }

        /// <summary>
        /// Will automatically shutdown the current Holochain Conductor (if the `shutdownHolochainConductorsMode` param is set to `ShutdownCurrentConductorOnly`) or all Holochain Conductors (if the `shutdownHolochainConductorsMode` param is set to `ShutdownAllConductors`). If the `shutdownHolochainConductorsMode` param is set to `UseHoloNETDNASettings` then it will use the `HoloNETClient.HoloNETDNA.AutoShutdownHolochainConductor` and `HoloNETClient.HoloNETDNA.ShutDownALLHolochainConductors` flags to determine which mode to use. The Disconnect method will automatically call this once it has finished disconnecting from the Holochain Conductor. The ShutdownHoloNET will also call this method.
        /// </summary>
        /// <param name="shutdownHolochainConductorsMode">If this flag is set to `ShutdownCurrentConductorOnly` it will shutdown the currently running Holochain Conductor only. If it is set to `ShutdownAllConductors` it will shutdown all running Holochain Conductors. If it is set to `UseHoloNETDNASettings` (default) then it will use the `HoloNETClient.HoloNETDNA.AutoShutdownHolochainConductor` and `HoloNETClient.HoloNETDNA.ShutDownALLHolochainConductors` flags to determine which mode to use.</param>
        /// <returns></returns>
        public virtual HolochainConductorsShutdownEventArgs ShutDownHolochainConductors(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
        {
            return ShutDownHolochainConductorsAsync(shutdownHolochainConductorsMode).Result;
        }

        protected virtual void Init(Uri holochainConductorURI)
        {
            try
            {
                if (Logger.LogProviders.Count == 0)
                    Logger.AddLogProvider(new DefaultLogger());

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += CurrentDomain_UnhandledException;

                WebSocket = new WebSocket.WebSocket(Logger);
                EndPoint = holochainConductorURI;

                //TODO: Impplemnt IDispoasable to unsubscribe event handlers to prevent memory leaks... 
                WebSocket.OnConnected += WebSocket_OnConnected;
                WebSocket.OnDataSent += WebSocket_OnDataSent;
                WebSocket.OnDataReceived += WebSocket_OnDataReceived;
                WebSocket.OnDisconnected += WebSocket_OnDisconnected;
                WebSocket.OnError += WebSocket_OnError;
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.Init method.", ex);
            }
        }

        protected virtual void InitLogger(Logger logger = null)
        {
            if (logger != null)
                Logger = logger;

            else if (Logger == null)
                Logger = new Logger();
        }

        protected virtual void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleError("CurrentDomain_UnhandledException was raised.", (Exception)e.ExceptionObject);
        }

        protected virtual void WebSocket_OnConnected(object sender, ConnectedEventArgs e)
        {
            try
            {
                IsConnecting = false;
                OnConnected?.Invoke(this, new ConnectedEventArgs { EndPoint = e.EndPoint });
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnConnected method.", ex);
            }
        }

        protected virtual void WebSocket_OnDataSent(object sender, DataSentEventArgs e)
        {
            Logger.Log(string.Concat("EVENT RAISED: DataSent: Raw Binary Data: ", e.RawBinaryDataDecoded, "(", e.RawBinaryDataAsString, ")"), LogType.Info);
            OnDataSent?.Invoke(this, new HoloNETDataSentEventArgs { EndPoint = e.EndPoint, RawBinaryData = e.RawBinaryData, RawBinaryDataAsString = e.RawBinaryDataAsString, RawBinaryDataDecoded = e.RawBinaryDataDecoded });
            //OnDataSent?.Invoke(this, new HoloNETDataSentEventArgs { IsCallSuccessful = e.IsCallSuccessful, EndPoint = e.EndPoint, RawBinaryData = e.RawBinaryData, RawBinaryDataAsString = e.RawBinaryDataAsString, RawBinaryDataDecoded = e.RawBinaryDataDecoded });
        }

        protected virtual void WebSocket_OnDataReceived(object sender, WebSocket.DataReceivedEventArgs e)
        {
            Logger.Log(string.Concat("EVENT RAISED: DataReceived: Raw Binary Data: ", e.RawBinaryDataDecoded.Trim(), "(", e.RawBinaryDataAsString.Trim(), ")"), LogType.Info);
            ProcessDataReceived(e);
        }

        protected virtual void WebSocket_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            try
            {
                if (_pendingRequests.Count > 0)
                    _pendingRequests.Clear();

                if (!_taskCompletionDisconnected.Task.IsCompleted)
                    _taskCompletionDisconnected.SetResult(e);

                //We need to dispose and nullify the socket in case we want to use it to connect again later.
                if (WebSocket.ClientWebSocket != null)
                {
                    WebSocket.ClientWebSocket.Dispose();
                    WebSocket.ClientWebSocket = null;
                }

                IsDisconnecting = false;
                OnDisconnected?.Invoke(this, e);
                ShutDownHolochainConductorsAsync(_shutdownHolochainConductorsMode);
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnDisconnected method.", ex);
            }
        }

        protected virtual void WebSocket_OnError(object sender, WebSocketErrorEventArgs e)
        {
            HandleError(e.Reason, e.ErrorDetails);
        }

        protected virtual string GetRequestId()
        {
            _currentId++;
            return _currentId.ToString();
        }

        protected virtual HoloNETResponse ProcessDataReceived(WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string rawBinaryDataAsString = "";
            string rawBinaryDataDecoded = "";
            HoloNETResponse response = null;

            try
            {
                rawBinaryDataAsString = DataHelper.ConvertBinaryDataToString(dataReceivedEventArgs.RawBinaryData);
                rawBinaryDataDecoded = DataHelper.DecodeBinaryDataAsUTF8(dataReceivedEventArgs.RawBinaryData);

                response = DecodeDataReceived(dataReceivedEventArgs.RawBinaryData, dataReceivedEventArgs);

                if (!response.IsError)
                {
                    switch (response.HoloNETResponseType)
                    {
                        case HoloNETResponseType.Error:
                            ProcessErrorReceivedFromConductor(response, dataReceivedEventArgs);
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

        protected virtual string ProcessErrorReceivedFromConductor(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string msg = $"Error in HoloNETClient.ProcessErrorReceivedFromConductor method. Error received from Holochain Conductor: ";

            if (response != null)
            {
                try
                {
                    AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);

                    if (appResponse != null)
                        msg = $"'{msg}{appResponse.type}: {appResponse.data["type"]}: {appResponse.data["data"]}.'";
                    else
                        msg = $"'{msg}{dataReceivedEventArgs.RawBinaryDataDecoded}'";
                }
                catch (Exception ex)
                {
                    msg = $"'{msg}{dataReceivedEventArgs.RawBinaryDataDecoded}'";
                }
            }
            else
                msg = $"'{msg}{dataReceivedEventArgs.RawBinaryDataDecoded}'";

            HandleError(msg, null);
            return msg;
        }

        protected virtual T ProcessResponeError<T>(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string responseErrorType, string msg) where T : HoloNETDataReceivedBaseEventArgs, new()
        {
            Logger.Log($"{responseErrorType} ERROR", LogType.Error);
            T args = CreateHoloNETArgs<T>(response, dataReceivedEventArgs);
            args.IsError = true;
            //args.IsCallSuccessful = false;
            args.Message = msg;
            return args;
        }

        protected virtual HoloNETResponse DecodeDataReceived(byte[] rawBinaryData, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            HoloNETResponse response = null;
            HoloNETDataReceivedEventArgs holoNETDataReceivedEventArgs = new HoloNETDataReceivedEventArgs();

            try
            {
                string id = "";
                string rawBinaryDataAfterMessagePackDecodeAsString = "";
                string rawBinaryDataAfterMessagePackDecodeDecoded = "";

                response = MessagePackSerializer.Deserialize<HoloNETResponse>(rawBinaryData, messagePackSerializerOptions);
                AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);

                id = response.id.ToString();
                rawBinaryDataAfterMessagePackDecodeAsString = DataHelper.ConvertBinaryDataToString(response.data);
                rawBinaryDataAfterMessagePackDecodeDecoded = DataHelper.DecodeBinaryDataAsUTF8(response.data);

                Logger.Log($"Id: {response.id} Type: {response.type}: {response.type} Internal Type: {appResponse.type}", LogType.Info);
                //Logger.Log(string.Concat("Raw Data Bytes Received After MessagePack Decode: ", rawBinaryDataAfterMessagePackDecodeAsString), LogType.Debug);
                //Logger.Log(string.Concat("Raw Data Bytes Decoded After MessagePack Decode: ", rawBinaryDataAfterMessagePackDecodeDecoded), LogType.Debug);

                switch (appResponse.type)
                {
                    case "zome-response":
                        response.HoloNETResponseType = HoloNETResponseType.ZomeResponse;
                        break;

                    case "signal":
                        response.HoloNETResponseType = HoloNETResponseType.Signal;
                        break;

                    case "app_info":
                        response.HoloNETResponseType = HoloNETResponseType.AppInfo;
                        break;

                    case "agent_pub_key_generated":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAgentPubKeyGenerated;
                        break;

                    case "app_installed":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppInstalled;
                        break;

                    case "app_uninstalled":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppUninstalled;
                        break;

                    case "app_enabled":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppEnabled;
                        break;

                    case "app_disabled":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppDisabled;
                        break;

                    case "zome_call_capability_granted":
                        response.HoloNETResponseType = HoloNETResponseType.AdminZomeCallCapabilityGranted;
                        break;

                    case "app_interface_attached":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppInterfaceAttached;
                        break;

                    case "apps_listed":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppsListed;
                        break;

                    case "error":
                        response.HoloNETResponseType = HoloNETResponseType.Error;
                        break;
                }

                holoNETDataReceivedEventArgs = CreateHoloNETArgs<HoloNETDataReceivedEventArgs>(response, dataReceivedEventArgs);

                if (HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour != EnforceRequestToResponseIdMatchingBehaviour.Ignore
                    && !_pendingRequests.Contains(id))
                {
                    holoNETDataReceivedEventArgs.IsError = HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour == EnforceRequestToResponseIdMatchingBehaviour.Error;
                    //holoNETDataReceivedEventArgs.IsCallSuccessful = false;
                    holoNETDataReceivedEventArgs.Message = $"The id returned in the response ({id}) does not match any pending request.";

                    if (holoNETDataReceivedEventArgs.IsError)
                        holoNETDataReceivedEventArgs.Message = string.Concat(holoNETDataReceivedEventArgs.Message, " HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Error so Aborting Request.");
                }

                _pendingRequests.Remove(id);
                response.IsError = holoNETDataReceivedEventArgs.IsError;
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeDataReceived. Reason: {ex}";
                holoNETDataReceivedEventArgs.IsError = true;
                holoNETDataReceivedEventArgs.Message = msg;
                //holoNETDataReceivedEventArgs.IsCallSuccessful = false;
                HandleError(msg, ex);
            }

            OnDataReceived?.Invoke(this, holoNETDataReceivedEventArgs);
            return response;
        }

        protected virtual AppInfo ProcessAppInfo(AppInfo appInfo, AppInfoCallBackEventArgs args)
        {
            if (appInfo != null)
            {
                string agentPubKey = ConvertHoloHashToString(appInfo.agent_pub_key);
                string agentPubKeyFromCell = "";
                string dnaHash = "";

                if (appInfo.cell_info != null)
                {
                    var first = appInfo.cell_info.First();

                    if (first.Value != null && first.Value.Count > 0 && first.Value[0].Provisioned != null && first.Value[0].Provisioned.cell_id != null && first.Value[0].Provisioned.cell_id.Length == 2 && first.Value[0].Provisioned.cell_id[0] != null && first.Value[0].Provisioned.cell_id[1] != null)
                    {
                        agentPubKeyFromCell = ConvertHoloHashToString(first.Value[0].Provisioned.cell_id[1]);
                        dnaHash = ConvertHoloHashToString(first.Value[0].Provisioned.cell_id[0]);
                        appInfo.CellId = first.Value[0].Provisioned.cell_id;
                        args.CellId = appInfo.CellId;

                        if (agentPubKeyFromCell != agentPubKey)
                        {
                            args.Message = $"Error occured in HoloNETClient.ProcessAppInfo. appInfoResponse.data.agent_pub_key and agentPubKey dervived from cell data do not match! appInfoResponse.data.agent_pub_key = {agentPubKey}, cellData = {agentPubKeyFromCell} ";
                            args.IsError = true;
                        }

                        //Conductor time is in microseconds so we need to multiple it by 10 to get the ticks needed to convert it to a DateTime.
                        //Also UNIX dateime (which hc uses) starts from 1970).
                        first.Value[0].Provisioned.dna_modifiers.OriginTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(first.Value[0].Provisioned.dna_modifiers.origin_time * 10);
                    }
                }

                if (_updateDnaHashAndAgentPubKey)
                {
                    if (!string.IsNullOrEmpty(agentPubKey))
                        HoloNETDNA.AgentPubKey = agentPubKey;

                    if (!string.IsNullOrEmpty(dnaHash))
                        HoloNETDNA.DnaHash = dnaHash;

                    if (args.CellId != null)
                        HoloNETDNA.CellId = args.CellId;
                }

                Logger.Log($"AGENT PUB KEY RETURNED FROM CONDUCTOR: {HoloNETDNA.AgentPubKey}", LogType.Info);
                Logger.Log($"DNA HASH RETURNED FROM CONDUCTOR:: {HoloNETDNA.DnaHash}", LogType.Info);

                args.AgentPubKey = agentPubKey;
                args.DnaHash = dnaHash;
                args.InstalledAppId = appInfo.installed_app_id;
                appInfo.AgentPubKey = agentPubKey;
                appInfo.DnaHash = dnaHash;

                if (appInfo.manifest != null)
                {
                    if (appInfo.manifest.roles != null && appInfo.manifest.roles.Length > 0)
                    {
                        string strategy = appInfo.manifest.roles[0].provisioning.strategy.ToPascalCase();

                        if (appInfo.manifest.roles[0].provisioning != null)
                            appInfo.manifest.roles[0].provisioning.StrategyType = (ProvisioningStrategyType)Enum.Parse(typeof(ProvisioningStrategyType), appInfo.manifest.roles[0].provisioning.strategy.ToPascalCase());
                    }
                }
            }
            else
            {
                args.Message = "Error occured in HoloNETClient.ProcessAppInfo. appInfo is null.";
                args.IsError = true;
            }

            return appInfo;
        }

        protected virtual T CreateHoloNETArgs<T>(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs) where T : HoloNETDataReceivedBaseEventArgs, new()
        {
            return new T()
            {
                EndPoint = dataReceivedEventArgs != null ? dataReceivedEventArgs.EndPoint : null,
                Id = response != null ? response.id.ToString() : "",
                //IsCallSuccessful = true,
                RawBinaryData = dataReceivedEventArgs != null ? dataReceivedEventArgs.RawBinaryData : null,
                RawBinaryDataAsString = dataReceivedEventArgs != null ? dataReceivedEventArgs.RawBinaryDataAsString : "",
                RawBinaryDataDecoded = dataReceivedEventArgs != null ? dataReceivedEventArgs.RawBinaryDataDecoded : "",
                RawJSONData = dataReceivedEventArgs != null ? dataReceivedEventArgs.RawJSONData : "",
                WebSocketResult = dataReceivedEventArgs != null ? dataReceivedEventArgs.WebSocketResult : null
            };
        }

        protected virtual T1 CopyHoloNETArgs<T1>(T1 sourceArgs, T1 targetArgs) where T1 : HoloNETDataReceivedBaseEventArgs
        {
            targetArgs.EndPoint = sourceArgs.EndPoint;
            targetArgs.Exception = sourceArgs.Exception;
            targetArgs.Id = sourceArgs.Id;
            //targetArgs.IsCallSuccessful = sourceArgs.IsCallSuccessful;
            targetArgs.IsError = sourceArgs.IsError;
            targetArgs.Message = sourceArgs.Message;
            targetArgs.RawBinaryData = sourceArgs.RawBinaryData;
            targetArgs.RawBinaryDataDecoded = sourceArgs.RawBinaryDataDecoded;
            targetArgs.RawBinaryDataAsString = sourceArgs.RawBinaryDataAsString;
            targetArgs.RawJSONData = sourceArgs.RawJSONData;
            targetArgs.WebSocketResult = sourceArgs.WebSocketResult;

            return targetArgs;
        }

        protected virtual void UpdateHolochainConductorConfigFile()
        {
            string config = File.ReadAllText(HoloNETDNA.HolochainConductorConfigPath);
            string newConfig = "";

            if (!config.Contains("port"))
                newConfig = config.Replace("admin_interfaces: null", $"admin_interfaces:\n      - driver:\n          type: websocket\n          port: {EndPoint.Port}");
            else
            {
                int portStart = config.IndexOf("port");
                int portEnd = config.IndexOf("\n", portStart);

                newConfig = config.Substring(0, portStart);
                newConfig = string.Concat(newConfig, $"port: {EndPoint.Port}");
                newConfig = string.Concat(newConfig, config.Substring(portEnd));
            }

            File.WriteAllText(HoloNETDNA.HolochainConductorConfigPath, newConfig);
        }

        protected virtual async Task<HolochainConductorsShutdownEventArgs> ShutDownAllHolochainConductorsAsync()
        {
            HolochainConductorsShutdownEventArgs result = new HolochainConductorsShutdownEventArgs();
            result.AgentPubKey = HoloNETDNA.AgentPubKey;
            result.DnaHash = HoloNETDNA.DnaHash;
            result.EndPoint = EndPoint;

            try
            {
                Logger.Log("Shutting Down All Holochain Conductors...", LogType.Info, false);

                foreach (Process process in Process.GetProcessesByName("hc"))
                {
                    if (HoloNETDNA.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                    result.NumberOfHcExeInstancesShutdown++;
                }

                //conductorInfo = new FileInfo(HoloNETDNA.FullPathToExternalHolochainConductorBinary);
                //parts = conductorInfo.Name.Split('.');

                foreach (Process process in Process.GetProcessesByName("holochain"))
                {
                    if (HoloNETDNA.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                    result.NumberOfHolochainExeInstancesShutdown++;
                }

                foreach (Process process in Process.GetProcessesByName("rustc"))
                {
                    if (HoloNETDNA.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                    result.NumberOfRustcExeInstancesShutdown++;
                }

                Logger.Log("All Holochain Conductors Successfully Shutdown.", LogType.Info);
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.ShutDownAllConductors method", ex);
            }

            return result;
        }
   
        protected virtual string GetItemFromCache(string id, Dictionary<string, string> cache)
        {
            return cache.ContainsKey(id) ? cache[id] : null;
        }

        protected virtual void LogEvent(string eventName, HoloNETDataReceivedBaseEventArgs holoNETEvent, string optionalAdditionalLogDetails = "")
        {
            if (!string.IsNullOrEmpty(optionalAdditionalLogDetails))
                optionalAdditionalLogDetails = $", {optionalAdditionalLogDetails}";

            Logger.Log(string.Concat("EVENT RAISED: ", eventName, ": Id: ", holoNETEvent.Id, ", Is Call Successful: ", optionalAdditionalLogDetails, ", Raw Binary Data: ", holoNETEvent.RawBinaryDataDecoded, "(", holoNETEvent.RawBinaryDataAsString, ")"), LogType.Info);
            //Logger.Log(string.Concat("EVENT RAISED: ", eventName, ": Id: ", holoNETEvent.Id, ", Is Call Successful: ", holoNETEvent.IsCallSuccessful ? "True" : "False", optionalAdditionalLogDetails, ", Raw Binary Data: ", holoNETEvent.RawBinaryDataDecoded, "(", holoNETEvent.RawBinaryDataAsString, ")"), LogType.Info);
        }

        protected virtual void Log(string msg, LogType logType, Action<string, LogType> loggingFunction = null)
        {
            if (loggingFunction != null)
                loggingFunction(msg, logType);

            Logger.Log(msg, logType);
        }

        protected virtual void HandleError(string message, Exception exception = null)
        {
            message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "");
            Logger.Log(message, LogType.Error);

            OnError?.Invoke(this, new HoloNETErrorEventArgs { EndPoint = WebSocket.EndPoint, Reason = message, ErrorDetails = exception });

            switch (HoloNETDNA.ErrorHandlingBehaviour)
            {
                case ErrorHandlingBehaviour.AlwaysThrowExceptionOnError:
                    throw new HoloNETException(message, exception, WebSocket.EndPoint);

                case ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent:
                    {
                        if (OnError == null)
                            throw new HoloNETException(message, exception, WebSocket.EndPoint);
                    }
                    break;
            }
        }
    }
}