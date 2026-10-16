using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
#if EMBEDDED
//using NextGenSoftware.Holochain.HoloNET.Client.Properties; // the conductors are embedded directly now (see HoloNETClientBase - EmbeddedBinaries.cs)
#endif
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Logging.Interfaces;
using NextGenSoftware.Logging;
using NextGenSoftware.WebSocket;
using System.IO.IsolatedStorage;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientBase : IHoloNETClientBase, IDisposable
    {
        private bool _shuttingDownHoloNET = false;
        private bool _disposed = false;
        private TaskCompletionSource<DisconnectedEventArgs> _taskCompletionDisconnected = new TaskCompletionSource<DisconnectedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        private TaskCompletionSource<HoloNETShutdownEventArgs> _taskCompletionHoloNETShutdown = new TaskCompletionSource<HoloNETShutdownEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        private IHoloNETDNA _holoNETDNA = null;
        private Process _conductorProcess = null;
        private int _conductorProcessSessionId = 0;
        private ShutdownHolochainConductorsMode _shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings;
        private Task<HolochainConductorsShutdownEventArgs> _conductorShutdownTask;
        private HashSet<int> _conductorProcessesBeforeStart = new HashSet<int>();

        // How to fail each call that is waiting for the conductor (request id -> fail with reason). A call used to wait forever
        // if the connection dropped, it was made while disconnected, or its send threw.
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Action<string>> _pendingCompletions = new System.Collections.Concurrent.ConcurrentDictionary<string, Action<string>>();

        /// <summary>Registers how to fail the call waiting on <paramref name="completions"/>[<paramref name="id"/>].</summary>
        protected void TrackPendingCall<T>(string id, Dictionary<string, TaskCompletionSource<T>> completions) where T : HoloNETDataReceivedBaseEventArgs, new()
        {
            _pendingCompletions[id] = reason =>
            {
                if (completions.TryGetValue(id, out TaskCompletionSource<T> completion))
                {
                    completions.Remove(id);
                    completion.TrySetResult(new T { EndPoint = EndPoint, Id = id, IsError = true, Message = reason });
                }
            };
        }

        /// <summary>The call finished normally; stop tracking it.</summary>
        protected void UntrackPendingCall(string id) => _pendingCompletions.TryRemove(id, out _);

        /// <summary>Fails one waiting call (e.g. its send failed).</summary>
        protected void FailPendingCall(string id, string reason)
        {
            if (_pendingCompletions.TryRemove(id, out Action<string> fail))
                fail(reason);
        }

        /// <summary>Fails every waiting call (the connection closed).</summary>
        protected void FailAllPendingCalls(string reason)
        {
            foreach (string id in _pendingCompletions.Keys.ToList())
                FailPendingCall(id, reason);
        }

        /// <summary>Request ids go on the wire as unsigned 64-bit numbers.</summary>
        protected static bool IsValidRequestId(string id) => ulong.TryParse(id, out _);

        /// <summary>
        /// Sends the request for a call registered with <see cref="TrackPendingCall"/>; if it can't be sent (not connected,
        /// invalid id, send error) the call is failed straight away instead of waiting forever.
        /// </summary>
        protected async Task SendTrackedRequestAsync(HoloNETData holoNETData, HoloNETRequestType requestType, string id)
        {
            if (!IsValidRequestId(id))
            {
                FailPendingCall(id, $"Invalid request id '{id}': Holochain request ids are unsigned 64-bit numbers.");
                return;
            }

            if (WebSocket.State != WebSocketState.Open)
            {
                FailPendingCall(id, "Not connected to the Holochain conductor.");
                return;
            }

            try
            {
                //await SendHoloNETRequestAsync(holoNETData, requestType, id);
                if (!await TrySendHoloNETRequestAsync(MessagePackSerializer.Serialize(holoNETData), requestType, id))
                    FailPendingCall(id, "The request could not be sent to the Holochain conductor.");
            }
            catch (Exception ex)
            {
                FailPendingCall(id, $"The request could not be sent: {ex.Message}");
            }
        }
        private string _hcHolochainPath;

        /// <summary>Polls until something accepts TCP connections on <paramref name="endPoint"/>'s port, or the time runs out.</summary>
        protected static async Task<bool> WaitForConductorPortAsync(Uri endPoint, int timeoutMs)
        {
            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    using (var client = new System.Net.Sockets.TcpClient())
                    {
                        var connect = client.ConnectAsync(endPoint.Host == "localhost" ? "127.0.0.1" : endPoint.Host, endPoint.Port);
                        if (await Task.WhenAny(connect, Task.Delay(1000)) == connect && client.Connected)
                            return true;
                    }
                }
                catch { }
                await Task.Delay(250);
            }
            return false;
        }

        private string HcHolochainPathArgument() => string.IsNullOrEmpty(_hcHolochainPath) ? "" : $" -H '{_hcHolochainPath}'";

        protected int _currentId = 0;
        protected MessagePackSerializerOptions messagePackSerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
        protected List<string> _pendingRequests = new List<string>();
        protected bool _connectingAsync = false;
        protected bool _updateDnaHashAndAgentPubKey = true;
        // Shared on purpose (the admin client authorizes credentials that app clients sign with), so it must be thread-safe:
        // concurrent clients writing to a plain Dictionary threw in Dictionary.TryInsert.
        //protected static Dictionary<string, SigningCredentials> _signingCredentialsForCell = new Dictionary<string, SigningCredentials>();
        protected static System.Collections.Concurrent.ConcurrentDictionary<string, SigningCredentials> _signingCredentialsForCell = new System.Collections.Concurrent.ConcurrentDictionary<string, SigningCredentials>();
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

        //public ILogger Logger { get; set; } = new Logger();
        // Whatever logger is set is wrapped (ResilientLogger) so a faulty custom ILogger can't break HoloNET.
        private ILogger _logger = new ResilientLogger(new Logger());
        /// <summary>The logger HoloNET writes to (the built-in DefaultLogger unless you pass your own log providers or logger).</summary>
        public ILogger Logger
        {
            get => _logger;
            set => _logger = value == null || value is ResilientLogger ? value : new ResilientLogger(value);
        }

        /// <summary>
        /// This property contains a struct called HoloNETHoloNETDNA containing the sub-properties: AgentPubKey, DnaHash, FullPathToRootHappFolder, FullPathToCompiledHappFolder, HolochainConductorMode, FullPathToExternalHolochainConductorBinary, FullPathToExternalHCToolBinary, SecondsToWaitForHolochainConductorToStart, AutoStartHolochainConductor, ShowHolochainConductorWindow, AutoShutdownHolochainConductor, ShutDownALLHolochainConductors, HolochainConductorToUse, OnlyAllowOneHolochainConductorToRunAtATime, LoggingMode & ErrorHandlingBehaviour.
        /// </summary>
        /// <remarks>
        /// Each client has its own DNA once one is passed to its constructor or assigned here. Before, every client read and
        /// wrote the single static <see cref="HoloNETDNAManager.HoloNETDNA"/>, so creating one client (e.g. an app agent)
        /// replaced the DNA of every other client in the process (admin URI, InstalledAppId, DnaHash, AgentPubKey...).
        /// A client given no DNA still uses the shared, file-loaded default.
        /// </remarks>
        public IHoloNETDNA HoloNETDNA
        {
            get
            {
                if (_holoNETDNA != null)
                    return _holoNETDNA;

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
                //HoloNETDNAManager.HoloNETDNA = value; // replaced every client's DNA, see remarks
                _holoNETDNA = value;
            }
        }

        /// <summary>True once the settings have been loaded (or set).</summary>
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

        /// <summary>True while ConnectAsync is in progress.</summary>
        public bool IsConnecting { get; set; }
        /// <summary>True while DisconnectAsync is in progress.</summary>
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
        public HoloNETClientBase(IHoloNETDNA holoNETDNA = null)
        {
            if (holoNETDNA != null)
                HoloNETDNA = holoNETDNA;

            InitLogger();
            Logger.AddLogProvider(new DefaultLogProvider(HoloNETDNA.LogToConsole, HoloNETDNA.LogToFile, HoloNETDNA.LogPath, HoloNETDNA.LogFileName, HoloNETDNA.MaxLogFileSize, HoloNETDNA.InsertExtraNewLineAfterLogMessage, HoloNETDNA.IndentLogMessagesBy, HoloNETDNA.ShowColouredLogs, HoloNETDNA.DebugColour, HoloNETDNA.InfoColour, HoloNETDNA.WarningColour, HoloNETDNA.ErrorColour, HoloNETDNA.NumberOfRetriesToLogToFile, HoloNETDNA.RetryLoggingToFileEverySeconds));
            Init(new Uri(HoloNETDNA.HolochainConductorAppAgentURI));
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProvider">The implementation of the ILogProvider interface (custom logProvider).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientBase(ILogProvider logProvider, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)
        {
            if (holoNETDNA != null)
                HoloNETDNA = holoNETDNA;

            InitLogger();
            Logger.AddLogProvider(logProvider);

            if (alsoUseDefaultLogger)
                Logger.AddLogProvider(new DefaultLogProvider(HoloNETDNA.LogToConsole, HoloNETDNA.LogToFile, HoloNETDNA.LogPath, HoloNETDNA.LogFileName, HoloNETDNA.MaxLogFileSize, HoloNETDNA.InsertExtraNewLineAfterLogMessage, HoloNETDNA.IndentLogMessagesBy, HoloNETDNA.ShowColouredLogs, HoloNETDNA.DebugColour, HoloNETDNA.InfoColour, HoloNETDNA.WarningColour, HoloNETDNA.ErrorColour, HoloNETDNA.NumberOfRetriesToLogToFile, HoloNETDNA.RetryLoggingToFileEverySeconds));

            Init(new Uri(HoloNETDNA.HolochainConductorAppAgentURI));
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProviders">The implementations of the ILogProvider interface (custom logProviders).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom loggers injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientBase(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, IHoloNETDNA holoNETDNA = null)
        {
            if (holoNETDNA != null)
                HoloNETDNA = holoNETDNA;

            InitLogger();
            Logger.AddLogProviders(new List<ILogProvider>(logProviders));

            if (alsoUseDefaultLogger)
                Logger.AddLogProvider(new DefaultLogProvider(HoloNETDNA.LogToConsole, HoloNETDNA.LogToFile, HoloNETDNA.LogPath, HoloNETDNA.LogFileName, HoloNETDNA.MaxLogFileSize, HoloNETDNA.InsertExtraNewLineAfterLogMessage, HoloNETDNA.IndentLogMessagesBy, HoloNETDNA.ShowColouredLogs, HoloNETDNA.DebugColour, HoloNETDNA.InfoColour, HoloNETDNA.WarningColour, HoloNETDNA.ErrorColour, HoloNETDNA.NumberOfRetriesToLogToFile, HoloNETDNA.RetryLoggingToFileEverySeconds));

            Init(new Uri(HoloNETDNA.HolochainConductorAppAgentURI));
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logger">The logger instance to use.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientBase(ILogger logger, IHoloNETDNA holoNETDNA = null)
        {
            if (holoNETDNA != null)
                HoloNETDNA = holoNETDNA;

            InitLogger(logger);
            Init(new Uri(HoloNETDNA.HolochainConductorAppAgentURI));
        }

        /// <summary>Load the settings from HoloNETDNA.json (HoloNETDNAManager.HoloNETDNAPath).</summary>
        public virtual IHoloNETDNA LoadDNA()
        {
            //return _holoNETDNAManager.LoadDNA();
            //return HoloNETDNAManager.LoadDNA();
            IHoloNETDNA loaded = HoloNETDNAManager.LoadDNA();
            if (loaded != null)
                _holoNETDNA = loaded;
            return loaded;
        }

        /// <summary>Save the current settings to HoloNETDNA.json.</summary>
        public virtual bool SaveDNA()
        {
            //return _holoNETDNAManager.SaveDNA();
            //return HoloNETDNAManager.SaveDNA(); // saved the shared DNA, not this client's
            return HoloNETDNAManager.SaveDNA(HoloNETDNAManager.HoloNETDNAPath, HoloNETDNA);
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
        public virtual async Task<HoloNETConnectedEventArgs> ConnectAsync(Uri holochainConductorURI, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
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
        public virtual HoloNETConnectedEventArgs Connect(Uri holochainConductorURI, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            //return Task.Run(() => ConnectAsync(holochainConductorURI, ConnectedCallBackMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved)).Result; // returned before the conductor replied
            return Task.Run(() => ConnectAsync(holochainConductorURI, ConnectedCallBackMode.WaitForHolochainConductorToConnect, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved)).GetAwaiter().GetResult();
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
        public virtual async Task<HoloNETConnectedEventArgs> ConnectAsync(string holochainConductorURI = "", ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETConnectedEventArgs result = new HoloNETConnectedEventArgs();

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
                        await WebSocket.ConnectAsync(new Uri(holochainConductorURI), BuildWebSocketHandshakeHeaders());

                        if (State == WebSocketState.Open)
                        {
                            await AuthenticateConnectionAsync();
                            result.IsConnected = true;
                        }
                        else
                        {
                            // A refused / failed connection used to come back with IsConnected = false but no error.
                            result.IsError = true;
                            result.Message = $"Could not connect to the Holochain conductor at {holochainConductorURI} (WebSocket state: {State}).";
                        }
                    }
                    else
                    {
                        _connectingAsync = false;
                        FireAndForget(WebSocket.ConnectAsync(new Uri(holochainConductorURI), BuildWebSocketHandshakeHeaders()), "WebSocket.ConnectAsync");

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
        public virtual HoloNETConnectedEventArgs Connect(string holochainConductorURI = "", bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            //return Task.Run(() => ConnectAsync(holochainConductorURI, ConnectedCallBackMode.UseCallBackEvents, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved)).Result; // returned before the conductor replied
            return Task.Run(() => ConnectAsync(holochainConductorURI, ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode.Wait, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved)).GetAwaiter().GetResult();
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
                            // hc starts holochain: use the external one if given, else one next to hc.exe, else PATH.
                            if (!string.IsNullOrEmpty(HoloNETDNA.FullPathToExternalHolochainConductorBinary) && File.Exists(HoloNETDNA.FullPathToExternalHolochainConductorBinary))
                                _hcHolochainPath = HoloNETDNA.FullPathToExternalHolochainConductorBinary;
                            else if (File.Exists(Path.Combine(Path.GetDirectoryName(fullPathToHcExe) ?? "", "holochain.exe")))
                                _hcHolochainPath = Path.Combine(Path.GetDirectoryName(fullPathToHcExe), "holochain.exe");
                            break;

                        case HolochainConductorModeEnum.UseEmbedded:
                            {
#if !EMBEDDED
                                throw new InvalidOperationException("You must install the NextGenSoftware.Holochain.HoloNET.Client.Embedded NuGet package if you wish to use HolochainConductorMode.UseEmbedded.");
#else
                                //try
                                //{
                                //    fullPathToHolochainExe = Path.Combine(Directory.GetCurrentDirectory(), "hc.exe");
                                //    //fullPathToHolochainExe = Path.Combine(Directory.GetCurrentDirectory(), "HolochainBinaries/beta/holochain.exe");

                                //    if (!File.Exists(fullPathToHolochainExe))
                                //    {
                                //        using (FileStream fsDst = new FileStream(fullPathToHolochainExe, FileMode.CreateNew, FileAccess.Write))
                                //        {
                                //            byte[] bytes = Resources.holochain;
                                //            fsDst.Write(bytes, 0, bytes.Length);
                                //        }
                                //    }
                                //}
                                //catch (Exception ex)
                                //{
                                //    HandleError($"An error occured in HoloNETClientBase.StartHolochainConductorAsync attempting to write the embedded hc.exe to the current working directory {Directory.GetCurrentDirectory()}", ex);

                                    try
                                    {
                                        // Was: set fullPathToHolochainExe and wrote Resources.holochain out as hc.exe.
                                        fullPathToHcExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NextGenSoftware\\HoloNET\\hc.exe");
                                        //ExtractEmbeddedBinary(fullPathToHcExe, Resources.hc);
                                        fullPathToHcExe = EmbeddedBinaryPath("hc.exe");
                                        ExtractEmbeddedBinary(fullPathToHcExe, EmbeddedHc());
                                        // hc launches holochain itself, so the matching bundled conductor has to be there too.
                                        ExtractEmbeddedBinary(EmbeddedBinaryPath("holochain.exe"), EmbeddedHolochain());
                                        _hcHolochainPath = EmbeddedBinaryPath("holochain.exe");
                                    }
                                    catch (Exception ex2)
                                    {
                                        HandleError($"An error occured in HoloNETClientBase.StartHolochainConductorAsync attempting to write the embedded hc.exe to the AppData directory {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\NextGenSoftware\\HoloNET")}", ex2);
                                    }
                            //}
                                break; // inside #else so it isn't unreachable after the throw above (CS0162)
#endif
                                }

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
                            //_conductorProcess.StartInfo.FileName = HoloNETDNA.FullPathToExternalHolochainConductorBinary; // overwritten with PowerShell.exe below, so the external path was never used
                            fullPathToHolochainExe = HoloNETDNA.FullPathToExternalHolochainConductorBinary;
                            break;

                        case HolochainConductorModeEnum.UseEmbedded:
                            {
#if !EMBEDDED
                                throw new InvalidOperationException("You must install the NextGenSoftware.Holochain.HoloNET.Client.Embedded NuGet package if you wish to use HolochainConductorMode.UseEmbedded.");
#else
                                //try
                                //{
                                //    fullPathToHolochainExe = Path.Combine(Directory.GetCurrentDirectory(), "holochain.exe");
                                //    //fullPathToHolochainExe = Path.Combine(Directory.GetCurrentDirectory(), "HolochainBinaries/beta/holochain.exe");

                                //    if (!File.Exists(fullPathToHolochainExe))
                                //    {
                                //        using (FileStream fsDst = new FileStream(fullPathToHolochainExe, FileMode.CreateNew, FileAccess.Write))
                                //        {
                                //            byte[] bytes = Resources.holochain;
                                //            fsDst.Write(bytes, 0, bytes.Length);
                                //        }
                                //    }
                                //}
                                //catch (Exception ex)
                                //{
                                //    HandleError($"An error occured in HoloNETClientBase.StartHolochainConductorAsync attempting to write the embedded holochain.exe to the current working directory {fullPathToHolochainExe}", ex);

                                    try
                                    {
                                        //fullPathToHolochainExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\NextGenSoftware\\HoloNET\\holochain.exe");
                                        //fullPathToHolochainExe = $" { Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) }\\NextGenSoftware\\HoloNET\\holochain.exe";
                                        fullPathToHolochainExe =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NextGenSoftware\\HoloNET\\holochain.exe");
                                        //Logger.Log($"fullPathToHolochainExe={fullPathToHolochainExe}", LogType.Info);

                                        // Re-extracts when the cached copy differs, so an older bundled conductor is replaced.
                                        //ExtractEmbeddedBinary(fullPathToHolochainExe, Resources.holochain);
                                        fullPathToHolochainExe = EmbeddedBinaryPath("holochain.exe");
                                        ExtractEmbeddedBinary(fullPathToHolochainExe, EmbeddedHolochain());
                                    }
                                    catch (Exception ex2)
                                    {
                                        //Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\NextGenSoftware\\HoloNET\\Logs"
                                        //HandleError($"An error occured in HoloNETClientBase.StartHolochainConductorAsync attempting to write the embedded holochain.exe to the AppData directory { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\NextGenSoftware\\HoloNET") }", ex2);
                                        HandleError($"An error occured in HoloNETClientBase.StartHolochainConductorAsync attempting to write the embedded holochain.exe to the IsolatedStorage directory { fullPathToHolochainExe }", ex2);
                                    }
                            //}
                                break; // inside #else so it isn't unreachable after the throw above (CS0162)
#endif
                                }

                        case HolochainConductorModeEnum.UseSystemGlobal:
                            fullPathToHolochainExe = "holochain.exe";
                            break;
                    }
                }

                //Make sure the condctor is not already running
                //if (!HoloNETDNA.OnlyAllowOneHolochainConductorToRunAtATime || (HoloNETDNA.OnlyAllowOneHolochainConductorToRunAtATime && !Process.GetProcesses().Any(x => x.ProcessName == _conductorProcess.StartInfo.FileName)))
                string conductorProcessName = Path.GetFileNameWithoutExtension(HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool ? fullPathToHcExe : fullPathToHolochainExe);
                bool conductorAlreadyRunning = Process.GetProcessesByName(conductorProcessName).Length > 0;

                if (HoloNETDNA.OnlyAllowOneHolochainConductorToRunAtATime && conductorAlreadyRunning)
                    Logger.Log($"A {conductorProcessName} conductor is already running and OnlyAllowOneHolochainConductorToRunAtATime is set, so HoloNET will use it rather than start another.", LogType.Info);

                if (!HoloNETDNA.OnlyAllowOneHolochainConductorToRunAtATime || !conductorAlreadyRunning)
                {
                    Logger.Log("Starting Holochain Conductor...", LogType.Info, true);

                    if (HoloNETDNA.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    {
                        _conductorProcess.StartInfo.WorkingDirectory = HoloNETDNA.FullPathToRootHappFolder;
                        //_conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'' | {fullPathToHcExe} s  generate {HoloNETDNA.FullPathToCompiledHappFolder} --piped -f={EndPoint.Port}\"";
                        //_conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'' | {fullPathToHcExe} s --piped  generate {HoloNETDNA.FullPathToCompiledHappFolder} --run={EndPoint.Port}\"";
                        // `--run=<port>` only sets an *app* port (the admin port was random, so HoloNET couldn't connect);
                        // -f fixes the admin port. hc also needs to know which holochain to launch (-H), and lair runs in-process.
                        _conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'' | & '{fullPathToHcExe}' s --piped -f={EndPoint.Port}{HcHolochainPathArgument()} generate '{HoloNETDNA.FullPathToCompiledHappFolder}' --in-process-lair --run=0\""; // --run needs a value: 0 = one app interface on a free port
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
                        //If the conductor config file does not exist in the default location then we need to set the conductor up now.
                        //NOTE: As of Holochain 0.6.x the old `-i` (interactive init) flag has been removed entirely - it no longer exists on
                        //the holochain.exe CLI (confirmed via `holochain.exe --help` against the real 0.7.0 binary, which only lists
                        //--create-config, not -i). The replacement is `--create-config`, which also no longer accepts a `-c <path>` to
                        //control where it writes the file - it always creates a new randomly-named sandbox directory (and a `.hc` file in
                        //the current working directory pointing at it) and prints a "Created config at <path>" line to stdout. There are
                        //also no interactive passphrase prompts to pipe answers into anymore because the config it generates defaults to
                        //the LairServerInProc keystore, which does not require a passphrase for an unencrypted local sandbox/dev setup.
                        if (!File.Exists(HoloNETDNA.HolochainConductorConfigPath))
                            await CreateHolochainConductorConfigAsync(fullPathToHolochainExe);

                        //Once the config file has been created we can update it with the correct port and then launch it.
                        if (File.Exists(HoloNETDNA.HolochainConductorConfigPath))
                        {
                            UpdateHolochainConductorConfigFile();

                            //TrueProcessStart("PowerShell.exe -NoExit -Command \"'' | {fullPathToHolochainExe} --piped\"");
                            _conductorProcess.StartInfo.Arguments = $"-NoExit -Command \"'' | {fullPathToHolochainExe} --piped -c '{HoloNETDNA.HolochainConductorConfigPath}'\"";
                        }
                        else
                        {
                            // No config (holochain --create-config failed): this used to start the PowerShell host anyway, with no
                            // command, leaving a hidden PowerShell waiting forever and no conductor.
                            ReportError($"Error in HoloNETClient.StartConductor: no conductor config at {HoloNETDNA.HolochainConductorConfigPath}, so the conductor was not started.");
                            return;
                        }
                    }

                    _conductorProcessesBeforeStart = new HashSet<int>(Process.GetProcessesByName("holochain").Concat(Process.GetProcessesByName("hc")).Select(x => x.Id));
                    _conductorProcess.Start();
                    _conductorProcessSessionId = _conductorProcess.Id;

                    //await Task.Delay(HoloNETDNA.SecondsToWaitForHolochainConductorToStart * 1000); // Give the conductor 5 (default) seconds to start up...
                    // Wait until the admin port accepts connections (at most SecondsToWaitForHolochainConductorToStart): a fixed
                    // delay was either too short (hc generate on a slow machine) or wasted time when the conductor was quicker.
                    await WaitForConductorPortAsync(EndPoint, Math.Max(HoloNETDNA.SecondsToWaitForHolochainConductorToStart, 1) * 1000);
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
            //StartHolochainConductorAsync().Wait(); // deadlocks on a UI / Unity synchronisation context
            Task.Run(() => StartHolochainConductorAsync()).GetAwaiter().GetResult();
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
        /// This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownAllHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected event once it is disconnected.
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        /// <returns></returns>
        public virtual async Task<HoloNETDisconnectedEventArgs> DisconnectAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
        {
            HoloNETDisconnectedEventArgs disconnectedEventArgs = new HoloNETDisconnectedEventArgs();

            try
            {
                IsConnecting = false;

                if (IsDisconnecting)
                {
                    disconnectedEventArgs.IsWarning = true;
                    disconnectedEventArgs.Message = "Already Disconnecting...";
                    return disconnectedEventArgs;
                }

                if (State == WebSocketState.Open)
                {
                    IsDisconnecting = true;
                    _taskCompletionDisconnected = new TaskCompletionSource<DisconnectedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

                    _shutdownHolochainConductorsMode = shutdownHolochainConductorsMode;
                    await WebSocket.DisconnectAsync();

                    if (disconnectedCallBackMode == DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect)
                    {
                        await _taskCompletionDisconnected.Task;

                        // The disconnect handler starts the conductor shutdown; wait for it too, so the conductor is really
                        // stopped when this returns (it used to still be running).
                        if (_conductorShutdownTask != null)
                            disconnectedEventArgs.HolochainConductorsShutdownResult = await _conductorShutdownTask;
                    }

                    if (WebSocket.State == WebSocketState.Closed)
                        disconnectedEventArgs.IsDisconnected = true;
                }
                else
                {
                    disconnectedEventArgs.IsWarning = true;
                    disconnectedEventArgs.Message = "HoloNET Is Already Disconnected!";
                    return disconnectedEventArgs;
                }
            }
            catch (Exception e) 
            {
                disconnectedEventArgs.Message = $"Unknown Error Occured in HoloNETClientBase.DisconnectAsync method. Reason: {e}";
                HandleError(disconnectedEventArgs.Message);
            }

            return disconnectedEventArgs;
        }

        /// <summary>
        /// This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownAllHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected event once it is disconnected.
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        /// <returns></returns>
        public virtual HoloNETDisconnectedEventArgs Disconnect(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
        {
            //return Task.Run(() => DisconnectAsync(DisconnectedCallBackMode.UseCallBackEvents, shutdownHolochainConductorsMode)).Result; // returned before the conductor replied
            return Task.Run(() => DisconnectAsync(DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, shutdownHolochainConductorsMode)).GetAwaiter().GetResult();
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
                // return Convert.FromBase64String(hash.Replace('-', '+').Replace('_', '/').Substring(1, hash.Length - 1)); //also remove the u prefix.
                string incoming = hash.Replace('_', '/').Replace('-', '+').Substring(1, hash.Length - 1); //also remove the u prefix.
                //switch (hash.Length % 4)
                //{
                //    case 2: incoming += "=="; break;
                //    case 3: incoming += "="; break;
                //}
                // Padding was worked out from the length *with* the "u" prefix, so Holochain's unpadded base64url strings
                // only decoded for 39-byte hashes (which need none); other lengths came back null.
                incoming = incoming.TrimEnd('=');
                switch (incoming.Length % 4)
                {
                    case 2: incoming += "=="; break;
                    case 3: incoming += "="; break;
                }

                return Convert.FromBase64String(incoming);
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

        /// <summary>Build a cell id from a DNA hash and an agent public key (both as HoloHash strings).</summary>
        public virtual byte[][] GetCellId(string DnaHash, string AgentPubKey)
        {
            return new byte[2][] { ConvertHoloHashToBytes(DnaHash), ConvertHoloHashToBytes(AgentPubKey) };
        }

        /// <summary>Build a cell id from a DNA hash and an agent public key (raw bytes).</summary>
        public virtual byte[][] GetCellId(byte[] DnaHash, byte[] AgentPubKey)
        {
            return new byte[2][] { DnaHash, AgentPubKey };
        }

        /// <summary>The cell id zome calls go to: HoloNETDNA.CellId, or built from its DnaHash and AgentPubKey (retrieving them from the conductor first if needed).</summary>
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

        /// <summary>Sync version of GetCellIdAsync.</summary>
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
            _taskCompletionHoloNETShutdown = new TaskCompletionSource<HoloNETShutdownEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Was "!= Closed || != CloseReceived || != CloseSent" (always true). On a client that was already disconnected,
            // DisconnectAsync returns at once and nothing ever raises OnDisconnected, so the wait below never ended: finish the
            // shutdown here instead (ShutDownHolochainConductorsAsync completes it).
            //if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
            if (WebSocket.State == WebSocketState.Open)
                await DisconnectAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);
            else
            {
                _shutdownHolochainConductorsMode = shutdownHolochainConductorsMode;
                await ShutDownHolochainConductorsAsync(shutdownHolochainConductorsMode);
            }

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

            // Was "!= Closed || != CloseReceived || != CloseSent" (always true); see ShutdownHoloNETAsync.
            //if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
            if (WebSocket.State == WebSocketState.Open)
                Disconnect(shutdownHolochainConductorsMode);

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


                        // Kill by process name — StartInfo of external processes isn't populated on Windows,
                        // so Process.GetProcesses() filtering on StartInfo.Arguments won't work.
                        //List<Process> hcProcesses = Process.GetProcessesByName("hc").ToList();
                        //List<Process> holochainProcesses = Process.GetProcessesByName("holochain").ToList();
                        // "Current conductor only" used to kill every holochain/hc process on the machine. Now only the ones
                        // HoloNET started (any that appeared after it launched its conductor); none if it didn't start one.
                        bool startedOne = _conductorProcess != null;
                        List<Process> hcProcesses = startedOne ? Process.GetProcessesByName("hc").Where(x => !_conductorProcessesBeforeStart.Contains(x.Id)).ToList() : new List<Process>();
                        List<Process> holochainProcesses = startedOne ? Process.GetProcessesByName("holochain").Where(x => !_conductorProcessesBeforeStart.Contains(x.Id)).ToList() : new List<Process>();

                        foreach (Process process in hcProcesses)
                        {
                            try
                            {
                                if (HoloNETDNA.ShowHolochainConductorWindow)
                                    process.CloseMainWindow();
                                process.Kill();
                                process.WaitForExit(3000);
                                process.Dispose();
                                holochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown++;
                            }
                            catch { }
                        }

                        foreach (Process process in holochainProcesses)
                        {
                            try
                            {
                                if (HoloNETDNA.ShowHolochainConductorWindow)
                                    process.CloseMainWindow();
                                process.Kill();
                                process.WaitForExit(3000);
                                process.Dispose();
                                holochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown++;
                            }
                            catch { }
                        }

                        // The conductor runs inside a hidden "PowerShell -NoExit" host that HoloNET started; killing only
                        // holochain/hc left that host running after every shutdown. Stop the host (and its tree) as well.
                        try
                        {
                            if (_conductorProcess != null && !_conductorProcess.HasExited)
                            {
#if NET5_0_OR_GREATER
                                _conductorProcess.Kill(true);
#else
                                _conductorProcess.Kill();
#endif
                                _conductorProcess.WaitForExit(3000);
                            }
                        }
                        catch { }

                        //List<Process> processes2 = Process.GetProcesses().Where(x => x.ProcessName == "hc.exe").ToList();
                        //List<Process> processes3 = Process.GetProcesses().Where(x => x.ProcessName == "holochain.exe").ToList();

                        //List<Process> processes4 = Process.GetProcesses().ToList();

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
                    _taskCompletionHoloNETShutdown.TrySetResult(holoNETShutdownEventArgs);
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
            //return Task.Run(() => ShutDownHolochainConductorsAsync(shutdownHolochainConductorsMode)).Result; // .Result wraps errors in AggregateException
            return Task.Run(() => ShutDownHolochainConductorsAsync(shutdownHolochainConductorsMode)).GetAwaiter().GetResult();
        }

        /// <summary>Disconnect and release the WebSocket.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            // Every client subscribes to AppDomain.UnhandledException in Init; without this a disposed client stayed referenced
            // for the life of the process and kept reacting to every unhandled exception in the app.
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

            if (disposing && WebSocket != null)
            {
                WebSocket.OnConnected -= WebSocket_OnConnected_InCallback;
                WebSocket.OnDataSent -= WebSocket_OnDataSent;
                WebSocket.OnDataReceived -= WebSocket_OnDataReceived_InCallback;
                WebSocket.OnDisconnected -= WebSocket_OnDisconnected_InCallback;
                WebSocket.OnError -= WebSocket_OnError_InCallback;

                if (WebSocket is IDisposable disposableWs)
                    disposableWs.Dispose();
            }
            _conductorProcess?.Dispose();
            _disposed = true;
        }

        protected virtual void Init(Uri holochainConductorURI)
        {
            try
            {
                if (Logger.LogProviders.Count == 0)
                    Logger.AddLogProvider(new DefaultLogProvider());

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += CurrentDomain_UnhandledException;

                WebSocket = new WebSocket.WebSocket(Logger);
                EndPoint = holochainConductorURI;

                WebSocket.OnConnected += WebSocket_OnConnected_InCallback;
                WebSocket.OnDataSent += WebSocket_OnDataSent;
                WebSocket.OnDataReceived += WebSocket_OnDataReceived_InCallback;
                WebSocket.OnDisconnected += WebSocket_OnDisconnected_InCallback;
                WebSocket.OnError += WebSocket_OnError_InCallback;
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.Init method.", ex);
            }
        }

        protected virtual void InitLogger(ILogger logger = null)
        {
            if (logger != null)
                Logger = logger;

            else if (Logger == null)
                Logger = new Logger();
        }

        protected virtual void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //HandleError("CurrentDomain_UnhandledException was raised.", (Exception)e.ExceptionObject); // could throw inside the unhandled-exception handler
            ReportError("CurrentDomain_UnhandledException was raised.", e?.ExceptionObject as Exception);
        }

        // Set while HoloNET is handling something the WebSocket raised (a message, connect, disconnect, error), i.e. on the thread
        // that reads the socket. A sync wrapper that waited for the conductor's reply from here (typically inside one of our
        // events: the Manager calls GenerateAgentPubKey(), ListApps(), EnablelApp()... from its callbacks) blocked the very
        // thread that has to read that reply, so it deadlocked. See RunSync.
        [ThreadStatic] private static int _conductorCallbackDepth;

        /// <summary>True while running on the WebSocket's callback thread (inside a HoloNET event raised for a conductor message).</summary>
        protected static bool IsInConductorCallback => _conductorCallbackDepth > 0;

        /// <summary>
        /// Runs a sync wrapper: waits for the conductor's reply, except inside a conductor callback, where waiting would block the
        /// reply itself; there the request is sent and the result arrives through the matching On...CallBack event instead (how the
        /// sync methods always behaved before they were made to wait).
        /// </summary>
        protected static T RunSync<T>(Func<ConductorResponseCallBackMode, Task<T>> call)
        {
            if (IsInConductorCallback)
                return call(ConductorResponseCallBackMode.UseCallBackEvents).GetAwaiter().GetResult();

            return Task.Run(() => call(ConductorResponseCallBackMode.WaitForHolochainConductorResponse)).GetAwaiter().GetResult();
        }

        private void WebSocket_OnConnected_InCallback(object sender, ConnectedEventArgs e) { _conductorCallbackDepth++; try { WebSocket_OnConnected(sender, e); } finally { _conductorCallbackDepth--; } }
        private void WebSocket_OnDataReceived_InCallback(object sender, WebSocket.DataReceivedEventArgs e) { _conductorCallbackDepth++; try { WebSocket_OnDataReceived(sender, e); } finally { _conductorCallbackDepth--; } }
        private void WebSocket_OnDisconnected_InCallback(object sender, DisconnectedEventArgs e) { _conductorCallbackDepth++; try { WebSocket_OnDisconnected(sender, e); } finally { _conductorCallbackDepth--; } }
        private void WebSocket_OnError_InCallback(object sender, WebSocketErrorEventArgs e) { _conductorCallbackDepth++; try { WebSocket_OnError(sender, e); } finally { _conductorCallbackDepth--; } }

        protected virtual void WebSocket_OnConnected(object sender, ConnectedEventArgs e)
        {
            try
            {
                if (!DeferConnectedNotification)
                    NotifyConnected(e.EndPoint);
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnConnected method.", ex);
            }
        }

        protected virtual bool DeferConnectedNotification => false;

        protected void NotifyConnected(Uri endPoint)
        {
            IsConnecting = false;
            OnConnected?.Invoke(this, new ConnectedEventArgs { EndPoint = endPoint });
        }

        protected virtual void WebSocket_OnDataSent(object sender, DataSentEventArgs e)
        {
            Logger.Log(string.Concat("EVENT RAISED: DataSent: Raw Binary Data: ", e.RawBinaryDataDecoded, "(", e.RawBinaryDataAsString, ")"), LogType.Debug);
            OnDataSent?.Invoke(this, new HoloNETDataSentEventArgs { EndPoint = e.EndPoint, RawBinaryData = e.RawBinaryData, RawBinaryDataAsString = e.RawBinaryDataAsString, RawBinaryDataDecoded = e.RawBinaryDataDecoded });
            //OnDataSent?.Invoke(this, new HoloNETDataSentEventArgs { IsCallSuccessful = e.IsCallSuccessful, EndPoint = e.EndPoint, RawBinaryData = e.RawBinaryData, RawBinaryDataAsString = e.RawBinaryDataAsString, RawBinaryDataDecoded = e.RawBinaryDataDecoded });
        }

        protected virtual void WebSocket_OnDataReceived(object sender, WebSocket.DataReceivedEventArgs e)
        {
            Logger.Log(string.Concat("EVENT RAISED: DataReceived: Raw Binary Data: ", e.RawBinaryDataDecoded != null ? e.RawBinaryDataDecoded.Trim() : "", "(", e.RawBinaryDataAsString != null ? e.RawBinaryDataAsString.Trim() : "", ")"), LogType.Debug);
            //ProcessDataReceived(e);
            IHoloNETResponse response = ProcessDataReceived(e);

            // A reply that wasn't routed to the waiting call (a response type this client doesn't map to that request, e.g. a
            // misbehaving conductor or a type renamed in a future Holochain) left the call waiting forever. Fail it instead.
            // A call that was answered normally has already been completed and removed, so this is a no-op for it.
            if (response != null && response.id != 0 && !string.Equals(response.type, "signal", StringComparison.OrdinalIgnoreCase))
            {
                string responseType = null;
                try { responseType = response.data != null && response.data.Length > 0 ? ReadResponseType(response.data) : null; } catch { }
                FailPendingCall(response.id.ToString(), $"The conductor answered request {response.id} with an unexpected '{responseType}' response.");
            }
        }

        protected virtual void WebSocket_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            FailAllPendingCalls($"The connection to the Holochain conductor closed before it replied. {e?.Reason}".Trim());

            try
            {
                if (_pendingRequests.Count > 0)
                    _pendingRequests.Clear();

                //We need to dispose and nullify the socket in case we want to use it to connect again later.
                //if (WebSocket.ClientWebSocket != null)
                //{
                //    WebSocket.ClientWebSocket.Dispose();
                //    WebSocket.ClientWebSocket = null;
                //}

                IsDisconnecting = false;
                OnDisconnected?.Invoke(this, e);
                //FireAndForget(ShutDownHolochainConductorsAsync(_shutdownHolochainConductorsMode), "ShutDownHolochainConductorsAsync");
                // Started before completing _taskCompletionDisconnected so DisconnectAsync can await it.
                _conductorShutdownTask = ShutDownHolochainConductorsAsync(_shutdownHolochainConductorsMode);
                FireAndForget(_conductorShutdownTask, "ShutDownHolochainConductorsAsync");

                if (!_taskCompletionDisconnected.Task.IsCompleted)
                    _taskCompletionDisconnected.TrySetResult(e);
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnDisconnected method.", ex);
            }
        }

        protected virtual void WebSocket_OnError(object sender, WebSocketErrorEventArgs e)
        {
            IsConnecting = false;
            HandleError(e.Reason, e.ErrorDetails);
        }

        /// <summary>
        /// Builds the extra headers to send on the WebSocket handshake to the Holochain Conductor. As of Holochain
        /// 0.6.x the conductor's admin/app websocket servers reject the handshake with HTTP 400 if no Origin header
        /// is present at all, even when their `allowed_origins` config is set to allow any origin - confirmed directly
        /// against a real Holochain 0.7.0 conductor, since .NET's ClientWebSocket does not send an Origin header by
        /// default (Origin is normally a browser-only concept). We send a generic localhost Origin here since the
        /// conductor's `allowed_origins: Any` setting means the actual value does not need to match anything specific.
        /// </summary>
        protected virtual Dictionary<string, string> BuildWebSocketHandshakeHeaders()
        {
            return new Dictionary<string, string>
            {
                { "Origin", "http://localhost" }
            };
        }

        /// <summary>
        /// App clients override this to perform the Holochain 0.7 authentication exchange before
        /// the connection is reported as usable. Admin clients require no such exchange.
        /// </summary>
        protected virtual Task AuthenticateConnectionAsync() => Task.CompletedTask;

        /// <summary>
        /// Runs `holochain.exe --create-config` to generate a fresh conductor config (this replaces the old, now-removed
        /// `-i` interactive-init flag - see the comment in StartHolochainConductorAsync for details) and copies the
        /// generated file to HoloNETDNA.HolochainConductorConfigPath so subsequent runs can find it at the expected
        /// location. `--create-config` always writes into a new randomly-named sandbox directory under the working
        /// directory it is run from and prints a "Created config at &lt;path&gt;" line to stdout - there is no flag to
        /// control the output path directly, so we parse that line to find where it actually wrote the file.
        /// </summary>
        protected virtual async Task CreateHolochainConductorConfigAsync(string fullPathToHolochainExe)
        {
            string sandboxWorkingDirectory = Path.Combine(Path.GetTempPath(), $"HoloNET_{Guid.NewGuid():N}");
            Directory.CreateDirectory(sandboxWorkingDirectory);

            using (Process createConfigProcess = new Process())
            {
                createConfigProcess.StartInfo.FileName = fullPathToHolochainExe;
                createConfigProcess.StartInfo.Arguments = "--create-config";
                createConfigProcess.StartInfo.WorkingDirectory = sandboxWorkingDirectory;
                createConfigProcess.StartInfo.UseShellExecute = false;
                createConfigProcess.StartInfo.RedirectStandardOutput = true;
                createConfigProcess.StartInfo.RedirectStandardError = true;
                createConfigProcess.StartInfo.CreateNoWindow = true;

                string stdOut = "";
                string stdErr = "";
                createConfigProcess.Start();
                stdOut = await createConfigProcess.StandardOutput.ReadToEndAsync();
                stdErr = await createConfigProcess.StandardError.ReadToEndAsync();
                createConfigProcess.WaitForExit(10000);

                //Checking both streams since this is undocumented CLI behaviour - verified empirically against the real
                //0.7.0 binary that the "Created config at <path>" line currently goes to stdout, but checking stderr
                //too is a cheap safeguard against that changing in a future point release.
                string combinedOutput = string.Concat(stdOut, "\n", stdErr);
                const string marker = "Created config at ";
                int markerIndex = combinedOutput.IndexOf(marker);

                if (markerIndex == -1)
                {
                    HandleError($"Error in HoloNETClient.CreateHolochainConductorConfigAsync method. Could not find the '{marker}' line in the output of 'holochain.exe --create-config'. Output was: {combinedOutput}", null);
                    return;
                }

                int pathStart = markerIndex + marker.Length;
                int pathEnd = combinedOutput.IndexOf("\n", pathStart);
                string generatedConfigPath = combinedOutput.Substring(pathStart, (pathEnd == -1 ? combinedOutput.Length : pathEnd) - pathStart).Trim();

                if (!File.Exists(generatedConfigPath))
                {
                    HandleError($"Error in HoloNETClient.CreateHolochainConductorConfigAsync method. The generated config file could not be found at the path parsed from the output: {generatedConfigPath}", null);
                    return;
                }

                string targetDirectory = Path.GetDirectoryName(HoloNETDNA.HolochainConductorConfigPath);

                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                File.Copy(generatedConfigPath, HoloNETDNA.HolochainConductorConfigPath, true);
            }
        }

        protected virtual void UpdateHolochainConductorConfigFile()
        {
            string config = File.ReadAllText(HoloNETDNA.HolochainConductorConfigPath);
            //string newConfig = "";
            //if (!config.Contains("port"))
            //    newConfig = config.Replace("admin_interfaces: null", $"admin_interfaces:\n      - driver:\n          type: websocket\n          port: {EndPoint.Port}");
            //else
            //{
            //    int portStart = config.IndexOf("port");
            //    int portEnd = config.IndexOf("\n", portStart);
            //    newConfig = config.Substring(0, portStart);
            //    newConfig = string.Concat(newConfig, $"port: {EndPoint.Port}");
            //    newConfig = string.Concat(newConfig, config.Substring(portEnd));
            //}
            // The text "port" also occurs in "report: none" (Holochain 0.7 network section), so with admin_interfaces: null the
            // old code rewrote that line instead and the conductor got no admin port; it only worked when the admin port came first.
            string newConfig = SetConductorConfigAdminPort(config, EndPoint.Port);

            File.WriteAllText(HoloNETDNA.HolochainConductorConfigPath, newConfig);
        }

        /// <summary>
        /// Returns <paramref name="config"/> (a conductor-config.yaml) with the first admin interface's port set to <paramref name="port"/>.
        /// Only the "port:" inside the top-level admin_interfaces block is touched; if that block is null, empty or missing a
        /// websocket admin interface is written in the format holochain --create-config generates.
        /// </summary>
        public static string SetConductorConfigAdminPort(string config, int port)
        {
            string nl = config.Contains("\r\n") ? "\r\n" : "\n";
            List<string> lines = config.Split(new[] { nl }, StringSplitOptions.None).ToList();
            string[] section = { "admin_interfaces:", "- driver:", "    type: websocket", $"    port: {port}", "    danger_bind_addr: null", "    allowed_origins: '*'" };

            int header = lines.FindIndex(l => l.StartsWith("admin_interfaces:"));
            if (header < 0)
            {
                lines.InsertRange(lines.Count > 0 && lines[lines.Count - 1] == "" ? lines.Count - 1 : lines.Count, section);
                return string.Join(nl, lines);
            }

            string inline = lines[header].Substring("admin_interfaces:".Length).Trim();
            if (inline == "null" || inline == "[]" || inline == "~")
            {
                lines.RemoveAt(header);
                lines.InsertRange(header, section);
                return string.Join(nl, lines);
            }

            // The block runs until the next top-level key (a line that doesn't start with space or '-').
            for (int i = header + 1; i < lines.Count && (lines[i].StartsWith(" ") || lines[i].StartsWith("-") || lines[i].Length == 0); i++)
            {
                string trimmed = lines[i].TrimStart(' ', '-');
                if (trimmed.StartsWith("port:"))
                {
                    lines[i] = lines[i].Substring(0, lines[i].IndexOf("port:")) + $"port: {port}";
                    return string.Join(nl, lines);
                }
            }

            // A block without a port (unexpected): replace it with a complete one.
            int end = header + 1;
            while (end < lines.Count && (lines[end].StartsWith(" ") || lines[end].StartsWith("-")))
                end++;
            lines.RemoveRange(header, end - header);
            lines.InsertRange(header, section);
            return string.Join(nl, lines);
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

                // One process that couldn't be killed (already exited, access denied) stopped the whole sweep; each is now
                // stopped independently.
                int Kill(string processName)
                {
                    int killed = 0;
                    foreach (Process process in Process.GetProcessesByName(processName))
                    {
                        try
                        {
                            if (HoloNETDNA.ShowHolochainConductorWindow)
                                process.CloseMainWindow();

                            process.Kill();
                            process.WaitForExit(3000);
                            killed++;
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Could not stop {processName} ({process.Id}): {ex.Message}", LogType.Warning);
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                    return killed;
                }

                //foreach (Process process in Process.GetProcessesByName("hc")) { ... kill ... result.NumberOfHcExeInstancesShutdown++; }
                result.NumberOfHcExeInstancesShutdown = Kill("hc");

                //foreach (Process process in Process.GetProcessesByName("holochain")) { ... kill ... result.NumberOfHolochainExeInstancesShutdown++; }
                result.NumberOfHolochainExeInstancesShutdown = Kill("holochain");

                // This killed every rustc process on the machine, i.e. the user's own Rust/cargo builds. Not a conductor.
                //foreach (Process process in Process.GetProcessesByName("rustc"))
                //{
                //    if (HoloNETDNA.ShowHolochainConductorWindow)
                //        process.CloseMainWindow();
                //    process.Kill();
                //    process.Close();
                //    process.Dispose();
                //    result.NumberOfRustcExeInstancesShutdown++;
                //}

                // The hidden PowerShell host HoloNET started the conductor in was left running.
                try
                {
                    if (_conductorProcess != null && !_conductorProcess.HasExited)
                    {
#if NET5_0_OR_GREATER
                        _conductorProcess.Kill(true);
#else
                        _conductorProcess.Kill();
#endif
                        _conductorProcess.WaitForExit(3000);
                    }
                }
                catch { }

                Logger.Log("All Holochain Conductors Successfully Shutdown.", LogType.Info);
            }
            catch (Exception ex)
            {
                //HandleError("Error occurred in HoloNETClient.ShutDownAllConductors method", ex);
                ReportError("Error occurred in HoloNETClient.ShutDownAllConductors method", ex);
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

            Logger.Log(string.Concat("EVENT RAISED: ", eventName, ": Id: ", holoNETEvent.Id, ", Is Call Successful: ", optionalAdditionalLogDetails, ", Raw Binary Data: ", holoNETEvent.RawBinaryDataDecoded, "(", holoNETEvent.RawBinaryDataAsString, ")"), LogType.Debug);
        }

        protected virtual void Log(string msg, LogType logType, Action<string, LogType> loggingFunction = null)
        {
            if (loggingFunction != null)
                loggingFunction(msg, logType);

            Logger.Log(msg, logType);
        }

        /// <summary>
        /// Logs the error and raises OnError, but never throws. For response handling: throwing there (as HandleError can,
        /// depending on ErrorHandlingBehaviour) runs on the WebSocket receive callback and skips completing the awaiting call.
        /// </summary>
        protected virtual void ReportError(string message, Exception exception = null)
        {
            message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "");
            Logger.Log(message, LogType.Error);
            OnError?.Invoke(this, new HoloNETErrorEventArgs { EndPoint = WebSocket.EndPoint, Reason = message, ErrorDetails = exception });
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

        protected virtual void HandleError(CallBackBaseEventArgs result, string errorMessage, bool log = true, Action<string, LogType> loggingFunction = null)
        {
            result.IsError = true;
            result.Message = errorMessage;
            HandleError(result.Message, null);

            if (log && loggingFunction != null)
                loggingFunction(result.Message, LogType.Error);
        }

        protected virtual void HandleError(HoloNETDataReceivedBaseEventArgs result, string errorMessage, Exception exception = null)
        {
            result.IsError = true;
            result.Message = errorMessage;

            // Decoders call this just before raising `result` to the awaiting caller. Throwing here (the
            // default when nothing subscribes to OnError) skipped that raise and left the call hanging,
            // so report the error on the result instead.
            string message = string.Concat(errorMessage, exception != null ? $". Error Details: {exception}" : "");
            Logger.Log(message, LogType.Error);
            OnError?.Invoke(this, new HoloNETErrorEventArgs { EndPoint = WebSocket.EndPoint, Reason = message, ErrorDetails = exception });
        }

    }
}
