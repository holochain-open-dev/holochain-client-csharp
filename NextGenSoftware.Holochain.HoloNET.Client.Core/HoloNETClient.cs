using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MessagePack;
using NextGenSoftware.WebSocket;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Properties;
using NextGenSoftware.Utilities;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETClient //: IDisposable //, IAsyncDisposable
    {
        private bool _shuttingDownHoloNET = false;
        private bool _getAgentPubKeyAndDnaHashFromConductor;
        private bool _automaticallyAttemptToGetFromSandboxIfConductorFails;
        private bool _updateDnaHashAndAgentPubKey = true;
        private Dictionary<string, string> _zomeLookup = new Dictionary<string, string>();
        private Dictionary<string, string> _funcLookup = new Dictionary<string, string>();
        private Dictionary<string, Type> _entryDataObjectTypeLookup = new Dictionary<string, Type>();
        private Dictionary<string, dynamic> _entryDataObjectLookup = new Dictionary<string, dynamic>();
        private Dictionary<string, ZomeFunctionCallBack> _callbackLookup = new Dictionary<string, ZomeFunctionCallBack>();
        private Dictionary<string, ZomeFunctionCallBackEventArgs> _zomeReturnDataLookup = new Dictionary<string, ZomeFunctionCallBackEventArgs>();
        private Dictionary<string, bool> _cacheZomeReturnDataLookup = new Dictionary<string, bool>();
        private Dictionary<string, TaskCompletionSource<ZomeFunctionCallBackEventArgs>> _taskCompletionZomeCallBack = new Dictionary<string, TaskCompletionSource<ZomeFunctionCallBackEventArgs>>();
        private TaskCompletionSource<AgentPubKeyDnaHash> _taskCompletionAgentPubKeyAndDnaHashRetreived = new TaskCompletionSource<AgentPubKeyDnaHash>();
        //private TaskCompletionSource<AgentPubKeyDnaHash> _taskCompletionAgentPubKeyAndDnaHashRetreivedFromConductor = new TaskCompletionSource<AgentPubKeyDnaHash>();
        private TaskCompletionSource<ReadyForZomeCallsEventArgs> _taskCompletionReadyForZomeCalls = new TaskCompletionSource<ReadyForZomeCallsEventArgs>();
        private TaskCompletionSource<DisconnectedEventArgs> _taskCompletionDisconnected = new TaskCompletionSource<DisconnectedEventArgs>();
        private TaskCompletionSource<HoloNETShutdownEventArgs> _taskCompletionHoloNETShutdown = new TaskCompletionSource<HoloNETShutdownEventArgs>();
        private int _currentId = 0;
        private HoloNETConfig _config = null;
        private Process _conductorProcess = null;
        private ShutdownHolochainConductorsMode _shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings;
        private MessagePackSerializerOptions messagePackSerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        //Events
        public delegate void Connected(object sender, ConnectedEventArgs e);
        public event Connected OnConnected;

        public delegate void Disconnected(object sender, DisconnectedEventArgs e);
        public event Disconnected OnDisconnected;

        public delegate void HolochainConductorsShutdownComplete(object sender, HolochainConductorsShutdownEventArgs e);
        public event HolochainConductorsShutdownComplete OnHolochainConductorsShutdownComplete;

        public delegate void HoloNETShutdownComplete(object sender, HoloNETShutdownEventArgs e);
        public event HoloNETShutdownComplete OnHoloNETShutdownComplete;

        public delegate void DataReceived(object sender, HoloNETDataReceivedEventArgs e);
        public event DataReceived OnDataReceived;

        public delegate void ZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e);
        public event ZomeFunctionCallBack OnZomeFunctionCallBack;

        public delegate void SignalCallBack(object sender, SignalCallBackEventArgs e);
        public event SignalCallBack OnSignalCallBack;

        //public delegate void ConductorDebugCallBack(object sender, ConductorDebugCallBackEventArgs e);
        //public event ConductorDebugCallBack OnConductorDebugCallBack;

        public delegate void AppInfoCallBack(object sender, AppInfoCallBackEventArgs e);
        public event AppInfoCallBack OnAppInfoCallBack;

        public delegate void ReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e);
        public event ReadyForZomeCalls OnReadyForZomeCalls;

        public delegate void Error(object sender, HoloNETErrorEventArgs e);
        public event Error OnError;

        // Properties

        /// <summary>
        /// This property contains the internal NextGenSoftware.WebSocket (https://www.nuget.org/packages/NextGenSoftware.WebSocket) that HoloNET uses. You can use this property to access the current state of the WebSocket as well as configure more options.
        /// </summary>
        public WebSocket.WebSocket WebSocket { get; set; }

        /// <summary>
        /// This property contains a struct called HoloNETConfig containing the sub-properties: AgentPubKey, DnaHash, FullPathToRootHappFolder, FullPathToCompiledHappFolder, HolochainConductorMode, FullPathToExternalHolochainConductorBinary, FullPathToExternalHCToolBinary, SecondsToWaitForHolochainConductorToStart, AutoStartHolochainConductor, ShowHolochainConductorWindow, AutoShutdownHolochainConductor, ShutDownALLHolochainConductors, HolochainConductorToUse, OnlyAllowOneHolochainConductorToRunAtATime, LoggingMode & ErrorHandlingBehaviour.
        /// </summary>
        public HoloNETConfig Config
        {
            get
            {
                if (_config == null)
                    _config = new HoloNETConfig();

                return _config;
            }
            set
            {
                _config = value;
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

        /// <summary>
        /// This property is a shortcut to the EndPoint property on the WebSocket property.
        /// </summary>
        public string EndPoint
        {
            get
            {
                if (WebSocket != null)
                    return WebSocket.EndPoint;

                return "";
            }
        }

        /// <summary>
        /// This property is a boolean and will return true when HoloNET is ready for zome calls after the OnReadyForZomeCalls event is raised.
        /// </summary>
        public bool IsReadyForZomesCalls { get; set; }

        /// <summary>
        /// This property is a boolean and will return true when HoloNET is retreiving the AgentPubKey & DnaHash using one of the RetreiveAgentPubKeyAndDnaHash, RetreiveAgentPubKeyAndDnaHashFromSandbox or RetreiveAgentPubKeyAndDnaHashFromConductor methods (by default this will occur automativally after it has connected to the Holochain Conductor).
        /// </summary>
        public bool RetreivingAgentPubKeyAndDnaHash { get; private set; }

        /// <summary>
        /// This constructor uses the built-in DefaultLogger and allows various options to be configured for it.
        /// </summary>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="logToConsole">Set this to true (default) if you wish HoloNET to log to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logToFile">Set this to true (default) if you wish HoloNET to log a log file. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="releativePathToLogFolder">The relative path to the log folder to log to. Will default to a sub-directory called `Logs` within the current working directory. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logFileName">The name of the file to log to. Will default to `HoloNET.log`. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="addAdditionalSpaceAfterEachLogEntry">Set this to true to add additional space after each log entry. The default is false. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="showColouredLogs">Set this to true to enable coloured logs in the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="debugColour">The colour to use for `Debug` log enries to the console NOTE: This is only relevant if the built-in DefaultLogger is used..</param>
        /// <param name="infoColour">The colour to use for `Info` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="warningColour">The colour to use for `Warning` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="errorColour">The colour to use for `Error` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        public HoloNETClient(string holochainConductorURI = "ws://localhost:8888", bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            Logger.Loggers.Add(new DefaultLogger(logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour));
            Init(holochainConductorURI);
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) your own implementation (logger) of the ILogger interface.
        /// </summary>
        /// <param name="logger">The implementation of the ILogger interface (custom logger).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        public HoloNETClient(ILogger logger, bool alsoUseDefaultLogger = false, string holochainConductorURI = "ws://localhost:8888")
        {
            Logger.Loggers.Add(logger);

            if (alsoUseDefaultLogger)
                Logger.Loggers.Add(new DefaultLogger());

            Init(holochainConductorURI);
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) your own implementation (logger) of the ILogger interface.
        /// </summary>
        /// <param name="logger">The implementation of the ILogger interface (custom logger).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="logToConsole">Set this to true (default) if you wish HoloNET to log to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logToFile">Set this to true (default) if you wish HoloNET to log a log file. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="releativePathToLogFolder">The relative path to the log folder to log to. Will default to a sub-directory called `Logs` within the current working directory. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logFileName">The name of the file to log to. Will default to `HoloNET.log`. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="addAdditionalSpaceAfterEachLogEntry">Set this to true to add additional space after each log entry. The default is false. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="showColouredLogs">Set this to true to enable coloured logs in the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="debugColour">The colour to use for `Debug` log enries to the console NOTE: This is only relevant if the built-in DefaultLogger is used..</param>
        /// <param name="infoColour">The colour to use for `Info` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="warningColour">The colour to use for `Warning` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="errorColour">The colour to use for `Error` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        public HoloNETClient(ILogger logger, bool alsoUseDefaultLogger = false, string holochainConductorURI = "ws://localhost:8888", bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            Logger.Loggers.Add(logger);

            if (alsoUseDefaultLogger)
                Logger.Loggers.Add(new DefaultLogger(logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour));

            Init(holochainConductorURI);
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) multiple implementations (logger) of the ILogger interface. HoloNET will then log to each of these loggers.
        /// </summary>
        /// <param name="loggers">The implementations of the ILogger interface (custom loggers).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom loggers injected in.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        public HoloNETClient(IEnumerable<ILogger> loggers, bool alsoUseDefaultLogger = false, string holochainConductorURI = "ws://localhost:8888")
        {
            Logger.Loggers = new List<ILogger>(loggers);

            if (alsoUseDefaultLogger)
                Logger.Loggers.Add(new DefaultLogger());

            Init(holochainConductorURI);
        }

        /// <summary>
        /// This constructor allows you to inject in (DI) multiple implementations (logger) of the ILogger interface. HoloNET will then log to each of these loggers.
        /// </summary>
        /// <param name="loggers">The implementations of the ILogger interface (custom loggers).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom loggers injected in.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="logToConsole">Set this to true (default) if you wish HoloNET to log to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logToFile">Set this to true (default) if you wish HoloNET to log a log file. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="releativePathToLogFolder">The relative path to the log folder to log to. Will default to a sub-directory called `Logs` within the current working directory. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logFileName">The name of the file to log to. Will default to `HoloNET.log`. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="addAdditionalSpaceAfterEachLogEntry">Set this to true to add additional space after each log entry. The default is false. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="showColouredLogs">Set this to true to enable coloured logs in the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="debugColour">The colour to use for `Debug` log enries to the console NOTE: This is only relevant if the built-in DefaultLogger is used..</param>
        /// <param name="infoColour">The colour to use for `Info` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="warningColour">The colour to use for `Warning` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="errorColour">The colour to use for `Error` log enries to the console. NOTE: This is only relevant if the built-in DefaultLogger is used.</param>
        public HoloNETClient(IEnumerable<ILogger> loggers, bool alsoUseDefaultLogger = false, string holochainConductorURI = "ws://localhost:8888", bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            Logger.Loggers = new List<ILogger>(loggers);

            if (alsoUseDefaultLogger)
                Logger.Loggers.Add(new DefaultLogger(logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour));

            Init(holochainConductorURI);
        }

        /// <summary>
        /// This method will wait (non blocking) until HoloNET is ready to make zome calls after it has connected to the Holochain Conductor and retrived the AgentPubKey & DnaHash. It will then return to the caller with the AgentPubKey & DnaHash. This method will return the same time the OnReadyForZomeCalls event is raised. Unlike all the other methods, this one only contains an async version because the non async version would block all other threads including any UI ones etc.
        /// </summary>
        /// <returns></returns>
        public async Task<ReadyForZomeCallsEventArgs> WaitTillReadyForZomeCallsAsync()
        {
            return await _taskCompletionReadyForZomeCalls.Task;
        }

        /// <summary>
        /// This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetreiveAgentPubKeyAndDnaHash method to retrive the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediatley and then call the OnConnected event once it has finished connecting.
        /// </summary>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediatley and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retreiveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retreiving the AgentPubKey & DnaHash before returning, otherwise it will return immediatley and then call the OnReadyForZomeCalls event once it has finished retreiving the DnaHash & AgentPubKey.</param>
        /// <param name="retreiveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retreive the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retreiveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retreive the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetreiveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetreiveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetreived">Set this to true (default) to automatically update the [HoloNETConfig](#holonetconfig) once it has retreived the DnaHash & AgentPubKey.</param>
        /// <returns></returns>
        public async Task ConnectAsync(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true)
        {
            try
            {
                _getAgentPubKeyAndDnaHashFromConductor = retreiveAgentPubKeyAndDnaHashFromConductor;

                if (Logger.Loggers.Count == 0)
                    throw new HoloNETException("ERROR: No Logger Has Been Specified! Please set a Logger with the Logger.Loggers Property.");

                if (WebSocket.State != WebSocketState.Connecting && WebSocket.State != WebSocketState.Open && WebSocket.State != WebSocketState.Aborted)
                {
                    if (Config.AutoStartHolochainConductor)
                        await StartHolochainConductorAsync();

                    if (connectedCallBackMode == ConnectedCallBackMode.WaitForHolochainConductorToConnect)
                        await WebSocket.Connect();
                    else
                    {
                        WebSocket.Connect();
                        await RetreiveAgentPubKeyAndDnaHashAsync(retreiveAgentPubKeyAndDnaHashMode, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(string.Concat("Error occured in HoloNETClient.Connect method connecting to ", WebSocket.EndPoint), e);
            }
        }

        /// <summary>
        /// This method simply connects to the Holochain conductor. It raises the OnConnected event once it is has successfully established a connection. It then calls the RetreiveAgentPubKeyAndDnaHash method to retrive the AgentPubKey & DnaHash.
        /// </summary>
        /// <param name="retreiveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retreive the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retreiveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retreive the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetreiveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetreiveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetreived">Set this to true (default) to automatically update the [HoloNETConfig](#holonetconfig) once it has retreived the DnaHash & AgentPubKey.</param>
        public void Connect(bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true)
        {
            ConnectAsync(ConnectedCallBackMode.UseCallBackEvents, RetreiveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived);
        }

        /// <summary>
        /// This method will start the Holochain Conducutor using the approprtiate settings defined in the Config property.
        /// </summary>
        /// <returns></returns>
        public async Task StartHolochainConductorAsync()
        {
            try
            {
                // Was used when they were set to Content rather than Embedded.
                //string fullPathToEmbeddedHolochainConductorBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\holochain.exe");
                //string fullPathToEmbeddedHCToolBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\hc.exe");

                _conductorProcess = new Process();

                if (string.IsNullOrEmpty(Config.FullPathToExternalHolochainConductorBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && Config.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                    throw new ArgumentNullException("FullPathToExternalHolochainConductorBinary", "When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HolochainProductionConductor', FullPathToExternalHolochainConductorBinary cannot be empty.");

                if (string.IsNullOrEmpty(Config.FullPathToExternalHCToolBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    throw new ArgumentNullException("FullPathToExternalHCToolBinary", "When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HcDevTool', FullPathToExternalHCToolBinary cannot be empty.");

                if (!File.Exists(Config.FullPathToExternalHolochainConductorBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && Config.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HolochainProductionConductor', FullPathToExternalHolochainConductorBinary ({Config.FullPathToExternalHolochainConductorBinary}) must point to a valid file.");

                if (!File.Exists(Config.FullPathToExternalHCToolBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HcDevTool', FullPathToExternalHCToolBinary ({Config.FullPathToExternalHCToolBinary}) must point to a valid file.");

                //if (!File.Exists(fullPathToEmbeddedHolochainConductorBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseEmbedded && Config.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                //    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseEmbedded' and HolochainConductorToUse is set to 'HolochainProductionConductor', you must ensure the holochain.exe is found here: {fullPathToEmbeddedHolochainConductorBinary}.");

                //if (!File.Exists(fullPathToEmbeddedHCToolBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseEmbedded && Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                //    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseEmbedded' and HolochainConductorToUse is set to 'HcDevTool', you must ensure the hc.exe is found here: {fullPathToEmbeddedHCToolBinary}.");

                if (!Directory.Exists(Config.FullPathToRootHappFolder))
                    throw new DirectoryNotFoundException($"The path for Config.FullPathToRootHappFolder ({Config.FullPathToRootHappFolder}) was not found.");

                if (!Directory.Exists(Config.FullPathToCompiledHappFolder))
                    throw new DirectoryNotFoundException($"The path for Config.FullPathToCompiledHappFolder ({Config.FullPathToCompiledHappFolder}) was not found.");


                if (Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                {
                    switch (Config.HolochainConductorMode)
                    {
                        case HolochainConductorModeEnum.UseExternal:
                            _conductorProcess.StartInfo.FileName = Config.FullPathToExternalHCToolBinary;
                            break;

                        case HolochainConductorModeEnum.UseEmbedded:
                            {
                                //throw new InvalidOperationException("You must install the Embedded version if you wish to use HolochainConductorMode.UseEmbedded.");

                                //_conductorProcess.StartInfo.FileName = fullPathToEmbeddedHCToolBinary;

                                string hcPath = Path.Combine(Directory.GetCurrentDirectory(), "hc.exe");

                                if (!File.Exists(hcPath))
                                {
                                    using (FileStream fsDst = new FileStream(hcPath, FileMode.CreateNew, FileAccess.Write))
                                    {
                                        byte[] bytes = Resources.hc;
                                        fsDst.Write(bytes, 0, bytes.Length);
                                    }
                                }

                                _conductorProcess.StartInfo.FileName = hcPath;
                            }
                            break;

                        case HolochainConductorModeEnum.UseSystemGlobal:
                            _conductorProcess.StartInfo.FileName = "hc.exe";
                            break;
                    }
                }
                else
                {
                    switch (Config.HolochainConductorMode)
                    {
                        case HolochainConductorModeEnum.UseExternal:
                            _conductorProcess.StartInfo.FileName = Config.FullPathToExternalHolochainConductorBinary;
                            break;

                        case HolochainConductorModeEnum.UseEmbedded:
                            {
                                //throw new InvalidOperationException("You must install the Embedded version if you wish to use HolochainConductorMode.UseEmbedded.");

                                //_conductorProcess.StartInfo.FileName = fullPathToEmbeddedHolochainConductorBinary;

                                string holochainPath = Path.Combine(Directory.GetCurrentDirectory(), "holochain.exe");

                                if (!File.Exists(holochainPath))
                                {
                                    using (FileStream fsDst = new FileStream(holochainPath, FileMode.CreateNew, FileAccess.Write))
                                    {
                                        byte[] bytes = Resources.holochain;
                                        fsDst.Write(bytes, 0, bytes.Length);
                                    }
                                }

                                _conductorProcess.StartInfo.FileName = holochainPath;
                            }
                            break;

                        case HolochainConductorModeEnum.UseSystemGlobal:
                            _conductorProcess.StartInfo.FileName = "holochain.exe";
                            break;
                    }
                }

                //Make sure the condctor is not already running
                if (!Config.OnlyAllowOneHolochainConductorToRunAtATime || (Config.OnlyAllowOneHolochainConductorToRunAtATime && !Process.GetProcesses().Any(x => x.ProcessName == _conductorProcess.StartInfo.FileName)))
                {
                    Logger.Log("Starting Holochain Conductor...", LogType.Info, true);
                    _conductorProcess.StartInfo.WorkingDirectory = Config.FullPathToRootHappFolder;

                    if (Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    {
                        //_conductorProcess.StartInfo.Arguments = "sandbox run 0";
                        _conductorProcess.StartInfo.Arguments = $"hc sandbox generate {Config.FullPathToCompiledHappFolder}";
                    }

                    _conductorProcess.StartInfo.UseShellExecute = true;
                    _conductorProcess.StartInfo.RedirectStandardOutput = false;

                    if (Config.ShowHolochainConductorWindow)
                    {
                        _conductorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        _conductorProcess.StartInfo.CreateNoWindow = false;
                    }
                    else
                    {
                        _conductorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        _conductorProcess.StartInfo.CreateNoWindow = true;
                    }

                    _conductorProcess.Start();

                    if (Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    {
                        await Task.Delay(3000);
                        _conductorProcess.Close();
                        _conductorProcess.StartInfo.Arguments = "sandbox run 0";
                        _conductorProcess.Start();
                    }

                    await Task.Delay(Config.SecondsToWaitForHolochainConductorToStart * 1000); // Give the conductor 7 (default) seconds to start up...
                }
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.StartConductor method.", ex);
            }
        }

        /// <summary>
        /// This method will start the Holochain Conducutor using the approprtiate settings defined in the Config property.
        /// </summary>
        /// <returns></returns>
        public void StartHolochainConductor()
        {
            StartHolochainConductorAsync();
        }

        /// <summary>
        /// This method will retrive the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retreiving from the Conductor first. It will call RetreiveAgentPubKeyAndDnaHashFromConductor and RetreiveAgentPubKeyAndDnaHashFromSandboxAsync internally.
        /// </summary>
        /// <param name="retreiveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retreive the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retreiveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retreive the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetreiveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetreiveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetreived">Set this to true (default) to automatically update the Config property once it has retreived the DnaHash & AgentPubKey.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetreiveAgentPubKeyAndDnaHash(bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true)
        {
            return RetreiveAgentPubKeyAndDnaHashAsync(RetreiveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived).Result;
        }

        /// <summary>
        /// This method will retrive the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retreiving from the Conductor first. It will call RetreiveAgentPubKeyAndDnaHashFromConductor and RetreiveAgentPubKeyAndDnaHashFromSandboxAsync internally.
        /// </summary>
        /// <param name="retreiveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retreiving the AgentPubKey & DnaHash before returning, otherwise it will return immediatley and then raise the OnReadyForZomeCalls event once it has finished retreiving the DnaHash & AgentPubKey.</param>
        /// <param name="retreiveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retreive the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retreiveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retreive the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetreiveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetreiveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetreived">Set this to true (default) to automatically update the Config property once it has retreived the DnaHash & AgentPubKey.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetreiveAgentPubKeyAndDnaHashAsync(RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true)
        {
            if (RetreivingAgentPubKeyAndDnaHash)
                return null;

            RetreivingAgentPubKeyAndDnaHash = true;

            //Try to first get from the conductor.
            if (retreiveAgentPubKeyAndDnaHashFromConductor)
                return await RetreiveAgentPubKeyAndDnaHashFromConductorAsync(retreiveAgentPubKeyAndDnaHashMode, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails);

            else if (retreiveAgentPubKeyAndDnaHashFromSandbox)
            {
                AgentPubKeyDnaHash agentPubKeyDnaHash = await RetreiveAgentPubKeyAndDnaHashFromSandboxAsync(updateConfigWithAgentPubKeyAndDnaHashOnceRetreived, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails);

                if (agentPubKeyDnaHash != null)
                    RetreivingAgentPubKeyAndDnaHash = false;
            }

            return new AgentPubKeyDnaHash() { AgentPubKey = Config.AgentPubKey, DnaHash = Config.DnaHash };
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retreives them and the WebSocket has connected to the Holochain Conductor. If it fails to retreive the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetreiveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the RetreiveAgentPubKeyAndDnaHashFromConductor method to attempt to retreive them directly from the conductor (default).
        /// </summary>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetreived">Set this to true (default) to automatically update the Config property once it has retreived the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetreiveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetreiveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true)
        {
            try
            {
                if (RetreivingAgentPubKeyAndDnaHash)
                    return null;

                RetreivingAgentPubKeyAndDnaHash = true;
                Logger.Log("Attempting To Retreive AgentPubKey & DnaHash From hc sandbox...", LogType.Info, true);

                if (string.IsNullOrEmpty(Config.FullPathToExternalHCToolBinary))
                    Config.FullPathToExternalHCToolBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\hc.exe"); //default to the current path

                Process pProcess = new Process();
                pProcess.StartInfo.WorkingDirectory = Config.FullPathToRootHappFolder;
                pProcess.StartInfo.FileName = "hc";
                //pProcess.StartInfo.FileName = Config.FullPathToExternalHCToolBinary; //TODO: Need to get this working later (think currently has a version conflict with keylairstone? But not urgent because AgentPubKey & DnaHash are retreived from Conductor anyway.
                pProcess.StartInfo.Arguments = "sandbox call list-cells";
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                pProcess.StartInfo.CreateNoWindow = false;
                pProcess.Start();

                string output = pProcess.StandardOutput.ReadToEnd();
                int dnaStart = output.IndexOf("DnaHash") + 8;
                int dnaEnd = output.IndexOf(")", dnaStart);
                int agentStart = output.IndexOf("AgentPubKey") + 12;
                int agentEnd = output.IndexOf(")", agentStart);

                string dnaHash = output.Substring(dnaStart, dnaEnd - dnaStart);
                string agentPubKey = output.Substring(agentStart, agentEnd - agentStart);

                if (!string.IsNullOrEmpty(dnaHash) && !string.IsNullOrEmpty(agentPubKey))
                {
                    if (updateConfigWithAgentPubKeyAndDnaHashOnceRetreived)
                    {
                        Config.AgentPubKey = agentPubKey;
                        Config.DnaHash = dnaHash;
                    }

                    Logger.Log("AgentPubKey & DnaHash successfully retreived from hc sandbox.", LogType.Info, false);

                    if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                        SetReadyForZomeCalls();

                    return new AgentPubKeyDnaHash() { DnaHash = dnaHash, AgentPubKey = agentPubKey };
                }
                else if (automaticallyAttemptToRetreiveFromConductorIfSandBoxFails)
                    await RetreiveAgentPubKeyAndDnaHashFromConductorAsync();
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.retreiveAgentPubKeyAndDnaHashFromSandbox method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retreives them and the WebSocket has connected to the Holochain Conductor. If it fails to retreive the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetreiveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the RetreiveAgentPubKeyAndDnaHashFromConductor method to attempt to retreive them directly from the conductor (default).
        /// </summary>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetreived">Set this to true (default) to automatically update the Config property once it has retreived the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetreiveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetreiveAgentPubKeyAndDnaHashFromSandbox(bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true)
        {
            return RetreiveAgentPubKeyAndDnaHashFromSandboxAsync(updateConfigWithAgentPubKeyAndDnaHashOnceRetreived, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails).Result;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retreived them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retreive the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetreiveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the RetreiveAgentPubKeyAndDnaHashFromSandbox method. 
        /// </summary>
        /// <param name="retreiveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retreiving the AgentPubKey & DnaHash before returning, otherwise it will return immediatley and then raise the OnReadyForZomeCalls event once it has finished retreiving the DnaHash & AgentPubKey.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetreived">Set this to true (default) to automatically update the Config property once it has retreived the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetreiveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetreiveAgentPubKeyAndDnaHashFromConductorAsync(RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true)
        {
            try
            {
                if (RetreivingAgentPubKeyAndDnaHash)
                    return null;

                _automaticallyAttemptToGetFromSandboxIfConductorFails = automaticallyAttemptToRetreiveFromSandBoxIfConductorFails;
                RetreivingAgentPubKeyAndDnaHash = true;
                _updateDnaHashAndAgentPubKey = updateConfigWithAgentPubKeyAndDnaHashOnceRetreived;
                Logger.Log("Attempting To Retreive AgentPubKey & DnaHash from Holochain Conductor...", LogType.Info, true);

                HoloNETData holoNETData = new HoloNETData()
                {
                    type = "app_info",
                    data = new HoloNETDataAppInfoCall()
                    {
                        installed_app_id = "test-app"
                    }
                };

                await SendHoloNETRequestAsync(_currentId.ToString(), holoNETData);
                _currentId++;

                if (retreiveAgentPubKeyAndDnaHashMode == RetreiveAgentPubKeyAndDnaHashMode.Wait)
                    return await _taskCompletionAgentPubKeyAndDnaHashRetreived.Task;
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.retreiveAgentPubKeyAndDnaHashFromConductor method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retreived them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retreive the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetreiveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the RetreiveAgentPubKeyAndDnaHashFromSandbox method. 
        /// </summary>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetreived">Set this to true (default) to automatically update the Config property once it has retreived the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetreiveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetreiveAgentPubKeyAndDnaHashFromConductor(bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true)
        {
            return RetreiveAgentPubKeyAndDnaHashFromConductorAsync(RetreiveAgentPubKeyAndDnaHashMode.UseCallBackEvents, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails).Result;
        }

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        /// <returns></returns>
        public async Task SendHoloNETRequestAsync(string id, HoloNETData holoNETData)
        {
            try
            {
                HoloNETRequest request = new HoloNETRequest()
                {
                    id = Convert.ToUInt64(id),
                    type = "Request",
                    data = MessagePackSerializer.Serialize(holoNETData)
                };

                if (WebSocket.State == WebSocketState.Open)
                {
                    Logger.Log("Sending HoloNET Request to Holochain Conductor...", LogType.Info, true);
                    await WebSocket.SendRawDataAsync(MessagePackSerializer.Serialize(request)); //This is the fastest and most popular .NET MessagePack Serializer.
                    Logger.Log("HoloNET Request Successfully Sent To Holochain Conductor.", LogType.Info, false);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.SendHoloNETRequest method.", ex);
            }
        }

        /// <summary>
        /// This method allows you to send your own raw request to holochain. This method raises the OnDataRecived event once it has received a response from the Holochain conductor.
        /// </summary>
        /// <param name="id">The id of the request to send to the Holochain Conductor. This will be matched to the id in the response received from the Holochain Conductor.</param>
        /// <param name="holoNETData">The raw data packet you wish to send to the Holochain conductor.</param>
        public void SendHoloNETRequest(string id, HoloNETData holoNETData)
        {
            SendHoloNETRequestAsync(id, holoNETData);
        }

        /// <summary>
        /// This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownAllHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected event once it is disconnected.
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconneced it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        /// <returns></returns>
        public async Task DisconnectAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            _shutdownHolochainConductorsMode = shutdownHolochainConductorsMode;
            await WebSocket.Disconnect();

            if (disconnectedCallBackMode == DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect)
                await _taskCompletionDisconnected.Task;
        }

        /// <summary>
        /// This method disconnects the client from the Holochain conductor. It raises the OnDisconnected event once it is has successfully disconnected. It will then automatically call the ShutDownAllHolochainConductors method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the OnDisconnected event once it is disconnected.
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconneced it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        /// <returns></returns>
        public void Disconnect(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            DisconnectAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, false, ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediatley once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediatley once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediatley once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediatley once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return await CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return await CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            try
            {
                _taskCompletionZomeCallBack[id] = new TaskCompletionSource<ZomeFunctionCallBackEventArgs>();

                if (WebSocket.State == WebSocketState.Closed || WebSocket.State == WebSocketState.None)
                    await ConnectAsync();

                await _taskCompletionReadyForZomeCalls.Task;

                if (string.IsNullOrEmpty(Config.DnaHash))
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "The DnaHash cannot be empty, please either set manually in the Config.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                    //throw new InvalidOperationException("The DnaHash cannot be empty, please either set manually in the Config.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call.");

                if (string.IsNullOrEmpty(Config.AgentPubKey))
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "The AgentPubKey cannot be empty, please either set manually in the Config.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                    //throw new InvalidOperationException("The AgentPubKey cannot be empty, please either set manually in the Config.AgentPubKey property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call.");

                Logger.Log($"Calling Zome Function {function} on Zome {zome} with Id {id} On Holochain Conductor...", LogType.Info, true);

                _cacheZomeReturnDataLookup[id] = cachReturnData;

                if (cachReturnData)
                {
                    if (_zomeReturnDataLookup.ContainsKey(id))
                    {
                        Logger.Log("Caching Enabled so returning data from cache...", LogType.Info);
                        Logger.Log(string.Concat("Id: ", _zomeReturnDataLookup[id].Id, ", Zome: ", _zomeReturnDataLookup[id].Zome, ", Zome Function: ", _zomeReturnDataLookup[id].ZomeFunction, ", Is Zome Call Successful: ", _zomeReturnDataLookup[id].IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", _zomeReturnDataLookup[id].RawZomeReturnData, ", Zome Return Data: ", _zomeReturnDataLookup[id].ZomeReturnData, ", JSON Raw Data: ", _zomeReturnDataLookup[id].RawJSONData), LogType.Info);

                        if (callback != null)
                            callback.DynamicInvoke(this, _zomeReturnDataLookup[id]);

                        OnZomeFunctionCallBack?.Invoke(this, _zomeReturnDataLookup[id]);
                        _taskCompletionZomeCallBack[id].SetResult(_zomeReturnDataLookup[id]);

                        Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                        _taskCompletionZomeCallBack.Remove(id);

                        return await returnValue;
                    }
                }

                if (matchIdToInstanceZomeFuncInCallback)
                {
                    _zomeLookup[id] = zome;
                    _funcLookup[id] = function;
                }

                if (callback != null)
                    _callbackLookup[id] = callback;

                try
                {
                    if (paramsObject != null && paramsObject.GetType() == typeof(string))
                    {
                        byte[] holoHash = null;
                        holoHash = ConvertHoloHashToBytes(paramsObject.ToString());

                        if (holoHash != null)
                            paramsObject = holoHash;
                    }
                }
                catch (Exception ex)
                {

                }

                HoloNETData holoNETData = new HoloNETData()
                {
                    type = "zome_call",
                    data = new HoloNETDataZomeCall()
                    {
                        cell_id = new byte[2][] { ConvertHoloHashToBytes(Config.DnaHash), ConvertHoloHashToBytes(Config.AgentPubKey) },
                        fn_name = function,
                        zome_name = zome,
                        payload = MessagePackSerializer.Serialize(paramsObject),
                        provenance = ConvertHoloHashToBytes(Config.AgentPubKey),
                        cap = null
                    }
                };

                await SendHoloNETRequestAsync(id, holoNETData);

                if (zomeResultCallBackMode == ZomeResultCallBackMode.WaitForHolochainConductorResponse)
                {
                    Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                    //_taskCompletionZomeCallBack.Remove(id);
                    return await returnValue;
                }
                else
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, Message = "ZomeResultCallBackMode is set to UseCallBackEvents so please wait for OnZomeFunctionCallBack event for zome result." };
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.CallZomeFunctionAsync method.", ex);
                return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = $"Error occured in HoloNETClient.CallZomeFunctionAsync method. Details: {ex}", Excception = ex };
            }
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, false, ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, false, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunction(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return CallZomeFunction(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode).Result;
        }

        public void ClearCache()
        {
            _zomeReturnDataLookup.Clear();
            _cacheZomeReturnDataLookup.Clear();
        }

        public byte[] ConvertHoloHashToBytes(string hash)
        {
            return Convert.FromBase64String(hash.Replace('-', '+').Replace('_', '/').Substring(1, hash.Length - 1)); //also remove the u prefix.

            //Span<byte> output = null;
            //int bytesWritten = 0;

            //if (Convert.TryFromBase64String(hash.Replace('-', '+').Replace('_', '/').Substring(1, hash.Length - 1), output, out bytesWritten)) //also remove the u prefix.
            //    return output.ToArray();
            //else
            //    return null;
        }

        public string ConvertHoloHashToString(byte[] bytes)
        {
            return string.Concat("u", Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_"));
        }

        public async Task<dynamic> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs)
        {
            return await MapEntryDataObjectAsync(Activator.CreateInstance(entryDataObjectType), keyValuePairs);
        }

        public dynamic MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs)
        {
            return MapEntryDataObjectAsync(entryDataObjectType, keyValuePairs).Result;
        }

        public async Task<dynamic> MapEntryDataObjectAsync(dynamic entryDataObject, Dictionary<string, string> keyValuePairs)
        {
            try
            {
                if (keyValuePairs != null && entryDataObject != null)
                {
                    PropertyInfo[] props = entryDataObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (PropertyInfo propInfo in props)
                    {
                        foreach (CustomAttributeData data in propInfo.CustomAttributes)
                        {
                            if (data.AttributeType == (typeof(HolochainPropertyName)))
                            {
                                try
                                {
                                    if (data.ConstructorArguments.Count > 0 && data.ConstructorArguments[0] != null && data.ConstructorArguments[0].Value != null)
                                    {
                                        string key = data.ConstructorArguments[0].Value.ToString();

                                        if (propInfo.PropertyType == typeof(Guid))
                                            propInfo.SetValue(entryDataObject, new Guid(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(bool))
                                            propInfo.SetValue(entryDataObject, Convert.ToBoolean(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(DateTime))
                                            propInfo.SetValue(entryDataObject, Convert.ToDateTime(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(int))
                                            propInfo.SetValue(entryDataObject, Convert.ToInt32(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(long))
                                            propInfo.SetValue(entryDataObject, Convert.ToInt64(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(float))
                                            propInfo.SetValue(entryDataObject, Convert.ToDouble(keyValuePairs[key])); //TODO: Check if this is right?! :)

                                        else if (propInfo.PropertyType == typeof(double))
                                            propInfo.SetValue(entryDataObject, Convert.ToDouble(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(decimal))
                                            propInfo.SetValue(entryDataObject, Convert.ToDecimal(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(UInt16))
                                            propInfo.SetValue(entryDataObject, Convert.ToUInt16(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(UInt32))
                                            propInfo.SetValue(entryDataObject, Convert.ToUInt32(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(UInt64))
                                            propInfo.SetValue(entryDataObject, Convert.ToUInt64(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(Single))
                                            propInfo.SetValue(entryDataObject, Convert.ToSingle(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(char))
                                            propInfo.SetValue(entryDataObject, Convert.ToChar(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(byte))
                                            propInfo.SetValue(entryDataObject, Convert.ToByte(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(sbyte))
                                            propInfo.SetValue(entryDataObject, Convert.ToSByte(keyValuePairs[key]));

                                        else
                                            propInfo.SetValue(entryDataObject, keyValuePairs[key]);

                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return entryDataObject;
        }

        public dynamic MapEntryDataObject(dynamic entryDataObject, Dictionary<string, string> keyValuePairs)
        {
            return MapEntryDataObjectAsync(entryDataObject, keyValuePairs).Result;
        }

        private async Task<HolochainConductorsShutdownEventArgs> ShutDownAllHolochainConductorsAsync()
        {
            HolochainConductorsShutdownEventArgs result = new HolochainConductorsShutdownEventArgs();
            result.AgentPubKey = Config.AgentPubKey;
            result.DnaHash = Config.DnaHash;
            result.EndPoint = EndPoint;

            try
            {
                Logger.Log("Shutting Down All Holochain Conductors...", LogType.Info, false);

                foreach (Process process in Process.GetProcessesByName("hc"))
                {
                    if (Config.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                    result.NumberOfHcExeInstancesShutdown++;
                }

                //conductorInfo = new FileInfo(Config.FullPathToExternalHolochainConductorBinary);
                //parts = conductorInfo.Name.Split('.');

                foreach (Process process in Process.GetProcessesByName("holochain"))
                {
                    if (Config.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                    result.NumberOfHolochainExeInstancesShutdown++;
                }

                foreach (Process process in Process.GetProcessesByName("rustc"))
                {
                    if (Config.ShowHolochainConductorWindow)
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
                HandleError("Error occured in HoloNETClient.ShutDownAllConductors method", ex);
            }

            return result;
        }

        ///// <summary>
        ///// Shutsdown all running Holochain Conductors.
        ///// </summary>
        //public HolochainConductorsShutdownEventArgs ShutDownAllHolochainConductors()
        //{
        //    return ShutDownAllHolochainConductorsAsync().Result;
        //}

        //public void Dispose()
        //{
        //    Shutdown();
        //}

        //public ValueTask DisposeAsync()
        //{
        //    if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
        //        await DisconnectAsync();

        //    Logger.Loggers.Clear();
        //}

        /// <summary>
        /// Will shutdown HoloNET and its current connetion to the Holochain Conductor.
        /// You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events to notify the caller.
        /// It will also shutdown the current running Holochain Conductor or all conductors depending on the config/params passed in.
        /// </summary>
        /// <param name="disconnectedCallBackMode"></param>
        /// <param name="shutdownHolochainConductorsMode"></param>
        /// <returns></returns>
        public async Task<HoloNETShutdownEventArgs> ShutdownHoloNETAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            _shuttingDownHoloNET = true;

            if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
                await DisconnectAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);

            if (disconnectedCallBackMode == DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect)
                return await _taskCompletionHoloNETShutdown.Task;
            else
                return new HoloNETShutdownEventArgs(EndPoint, Config.DnaHash, Config.AgentPubKey, null);
        }

        /// <summary>
        /// Will shutdown HoloNET and its current connetion to the Holochain Conductor.
        /// Unlike the async version, this non async version will not wait until HoloNET disconnects & shutsdown any Holochain Conductors before it returns to the caller. It will later raise the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events. If you wish to wait for HoloNET to disconnect and shutdown the conductors(s) before returning then please use ShutdownAsync instead.
        /// It will also shutdown the current running Holochain Conductor or all conductors depending on the config/params passed in.
        /// </summary>
        /// <param name="shutdownHolochainConductorsMode"></param>
        public HoloNETShutdownEventArgs ShutdownHoloNET(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            _shuttingDownHoloNET = true;

            if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
                Disconnect(DisconnectedCallBackMode.UseCallBackEvents, shutdownHolochainConductorsMode);

            return new HoloNETShutdownEventArgs(EndPoint, Config.DnaHash, Config.AgentPubKey, null);
        }

        public async Task<HolochainConductorsShutdownEventArgs> ShutDownHolochainConductorsAsync(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            HolochainConductorsShutdownEventArgs holochainConductorsShutdownEventArgs = new HolochainConductorsShutdownEventArgs();
            holochainConductorsShutdownEventArgs.AgentPubKey = Config.AgentPubKey;
            holochainConductorsShutdownEventArgs.DnaHash = Config.DnaHash;
            holochainConductorsShutdownEventArgs.EndPoint = EndPoint;

            try
            {
                // Close any conductors down if necessary.
                if ((Config.AutoShutdownHolochainConductor && _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.UseConfigSettings)
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownCurrentConductorOnly
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownAllConductors)
                {
                    if ((Config.ShutDownALLHolochainConductors && _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.UseConfigSettings)
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownAllConductors)
                        holochainConductorsShutdownEventArgs = await ShutDownAllHolochainConductorsAsync();

                    else if (_conductorProcess != null)
                    {
                        Logger.Log("Shutting Down Holochain Conductor...", LogType.Info, true);

                        if (Config.ShowHolochainConductorWindow)
                            _conductorProcess.CloseMainWindow();

                        _conductorProcess.Kill();
                        _conductorProcess.Close();

                        // _conductorProcess.WaitForExit();
                        _conductorProcess.Dispose();

                        if (_conductorProcess.StartInfo.FileName.Contains("hc.exe"))
                            holochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown = 1;

                        else if (_conductorProcess.StartInfo.FileName.Contains("holochain.exe"))
                            holochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown = 1;

                        Logger.Log("Holochain Conductor Successfully Shutdown.", LogType.Info);
                    }
                }

                OnHolochainConductorsShutdownComplete?.Invoke(this, holochainConductorsShutdownEventArgs);

                if (_shuttingDownHoloNET)
                {
                    Logger.Loggers.Clear();

                    HoloNETShutdownEventArgs holoNETShutdownEventArgs = new HoloNETShutdownEventArgs(this.EndPoint, Config.DnaHash, Config.AgentPubKey, holochainConductorsShutdownEventArgs);
                    OnHoloNETShutdownComplete?.Invoke(this, holoNETShutdownEventArgs);
                    _taskCompletionHoloNETShutdown.SetResult(holoNETShutdownEventArgs);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.ShutDownConductorsInternal method.", ex);
            }

            return holochainConductorsShutdownEventArgs;
        }

        public HolochainConductorsShutdownEventArgs ShutDownHolochainConductors(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            return ShutDownHolochainConductorsAsync(shutdownHolochainConductorsMode).Result;
        }

        private void Init(string holochainConductorURI)
        {
            try
            {
                WebSocket = new WebSocket.WebSocket(holochainConductorURI, Logger.Loggers);

                //TODO: Impplemnt IDispoasable to unsubscribe event handlers to prevent memory leaks... 
                WebSocket.OnConnected += WebSocket_OnConnected;
                WebSocket.OnDataReceived += WebSocket_OnDataReceived;
                WebSocket.OnDisconnected += WebSocket_OnDisconnected;
                WebSocket.OnError += WebSocket_OnError;
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.Init method.", ex);
            }
        }

        private void WebSocket_OnConnected(object sender, ConnectedEventArgs e)
        {
            try
            {
                OnConnected?.Invoke(this, new ConnectedEventArgs { EndPoint = e.EndPoint });

                //If the AgentPubKey & DnaHash have already been retreived from the hc sandbox command then raise the OnReadyForZomeCalls event.
                if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                    SetReadyForZomeCalls();

                //Otherwise, if the retreiveAgentPubKeyAndDnaHashFromConductor param was set to true when calling the Connect method, retreive them now...
                else if (_getAgentPubKeyAndDnaHashFromConductor) //TODO: Might not need to do this now because it is called in the Connect method (need to test this).
                    RetreiveAgentPubKeyAndDnaHashFromConductorAsync();
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnConnected method.", ex);
            }
        }


        private void WebSocket_OnError(object sender, WebSocketErrorEventArgs e)
        {
            HandleError(e.Reason, e.ErrorDetails);
        }

        private void WebSocket_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            try
            {
                if (!_taskCompletionDisconnected.Task.IsCompleted)
                    _taskCompletionDisconnected.SetResult(e);

                OnDisconnected?.Invoke(this, e);
                ShutDownHolochainConductorsAsync(_shutdownHolochainConductorsMode);
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnDisconnected method.", ex);
            }
        }

        private void WebSocket_OnDataReceived(object sender, WebSocket.DataReceivedEventArgs e)
        {
            ProcessDataReceived(e);
        }

        private void ProcessDataReceived(WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string rawBinaryDataAsString = "";
            string rawBinaryDataDecoded = "";
            string rawBinaryDataAfterMessagePackDecodeAsString = "";
            string rawBinaryDataAfterMessagePackDecodeDecoded = "";

            try
            {
                rawBinaryDataAsString = DataHelper.ConvertBinaryDataToString(dataReceivedEventArgs.RawBinaryData);
                rawBinaryDataDecoded = DataHelper.DecodeBinaryDataAsUTF8(dataReceivedEventArgs.RawBinaryData);

                Logger.Log("DATA RECEIVED", LogType.Info);
                Logger.Log("Raw Data Received: " + rawBinaryDataAsString, LogType.Debug);
                Logger.Log("Raw Data Decoded: " + rawBinaryDataDecoded, LogType.Debug);

                if (rawBinaryDataDecoded.Contains("error"))
                    ProcessErrorReceivedFromConductor(dataReceivedEventArgs, rawBinaryDataDecoded, rawBinaryDataAsString, rawBinaryDataAfterMessagePackDecodeAsString, rawBinaryDataAfterMessagePackDecodeDecoded);
                else
                {
                    HoloNETResponse response = DecodeDataReceived(dataReceivedEventArgs.RawBinaryData, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded);

                    if (rawBinaryDataDecoded.ToUpper().Contains("APP_INFO"))
                        DecodeAppInfoDataReceived(response, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded, rawBinaryDataAfterMessagePackDecodeAsString, rawBinaryDataAfterMessagePackDecodeDecoded);

                    else if (response.type.ToUpper() == "SIGNAL")
                        DecodeSignalDataReceived(response, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded, rawBinaryDataAfterMessagePackDecodeAsString, rawBinaryDataAfterMessagePackDecodeDecoded);
                    else
                        DecodeZomeDataReceived(response, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded, rawBinaryDataAfterMessagePackDecodeAsString, rawBinaryDataAfterMessagePackDecodeDecoded);
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in HoloNETClient.ProcessDataReceived method.";
                HandleError(msg, ex);
            }
        }

        private void ProcessErrorReceivedFromConductor(WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataDecoded, string rawBinaryDataAsString, string rawBinaryDataAfterMessagePackDecodeAsString, string rawBinaryDataAfterMessagePackDecodeDecoded)
        {
            string msg = $"Error in HoloNETClient.ProcessErrorReceivedFromConductor method. Error received from Holochain Conductor: {rawBinaryDataDecoded}";
            HandleError(msg, null);

            HoloNETResponse response = DecodeDataReceived(dataReceivedEventArgs.RawBinaryData, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded);

            if (rawBinaryDataDecoded.Contains("app_info"))
            {
                AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs()
                {
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    Id = response != null ? response.id.ToString() : null,
                    IsCallSuccessful = true,
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };

                RaiseAppInfoReceivedEvent(args);
            }
            else if (response != null)
            {
                if (response.type.ToUpper() == "SIGNAL")
                {
                    SignalCallBackEventArgs signalCallBackEventArgs = new SignalCallBackEventArgs()
                    {
                        Id = response != null ? response.id.ToString() : null,
                        EndPoint = dataReceivedEventArgs.EndPoint,
                        IsCallSuccessful = false,
                        IsError = true,
                        Message = msg,
                        RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                        RawBinaryDataAsString = rawBinaryDataAsString,
                        RawBinaryDataDecoded = rawBinaryDataDecoded,
                        RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                        RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                        RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                        RawJSONData = dataReceivedEventArgs.RawJSONData,
                        WebSocketResult = dataReceivedEventArgs.WebSocketResult
                    };

                    RaiseSignalReceivedEvent(signalCallBackEventArgs);
                }
            }
            else
            {
                string id = "";

                if (response != null)
                    id = response.id.ToString();

                ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs
                {
                    Id = id,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    IsError = true,
                    Message = msg,
                    Zome = GetItemFromCache(id, _zomeLookup),
                    ZomeFunction = GetItemFromCache(id, _funcLookup),
                    IsCallSuccessful = false,
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };

                RaiseZomeDataReceivedEvent(zomeFunctionCallBackArgs);
            }
        }

        private HoloNETResponse DecodeDataReceived(byte[] rawBinaryData, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataAsString, string rawBinaryDataDecoded)
        {
            HoloNETResponse response = null;
            HoloNETDataReceivedEventArgs holoNETDataReceivedEventArgs = new HoloNETDataReceivedEventArgs();

            try
            {
                string id = "";
                string rawBinaryDataAfterMessagePackDecodeAsString = "";
                string rawBinaryDataAfterMessagePackDecodeDecoded = "";

                response = MessagePackSerializer.Deserialize<HoloNETResponse>(rawBinaryData, messagePackSerializerOptions);

                id = response.id.ToString();
                rawBinaryDataAfterMessagePackDecodeAsString = DataHelper.ConvertBinaryDataToString(response.data);
                rawBinaryDataAfterMessagePackDecodeDecoded = DataHelper.DecodeBinaryDataAsUTF8(response.data);

                Logger.Log("Response", LogType.Info);
                Logger.Log($"Id: {response.id}", LogType.Info);
                Logger.Log($"Type: {response.type}", LogType.Info);
                Logger.Log(String.Concat("Raw Data Bytes Received After MessagePack Decode: ", rawBinaryDataAfterMessagePackDecodeAsString), LogType.Debug);
                Logger.Log(String.Concat("Raw Data Bytes Decoded After MessagePack Decode: ", rawBinaryDataAfterMessagePackDecodeDecoded), LogType.Debug);

                holoNETDataReceivedEventArgs = new HoloNETDataReceivedEventArgs()
                {
                    Id = id,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    IsCallSuccessful = true,
                    RawBinaryData = rawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occured in HoloNETClient.DecodeDataReceived. Reason: {ex}";
                holoNETDataReceivedEventArgs.IsError = true;
                holoNETDataReceivedEventArgs.Message = msg;
                HandleError(msg, ex);
            }

            OnDataReceived?.Invoke(this, holoNETDataReceivedEventArgs);
            return response;
        }

        private void DecodeAppInfoDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataAsString, string rawBinaryDataDecoded, string rawBinaryDataAfterMessagePackDecodeAsString, string rawBinaryDataAfterMessagePackDecodeDecoded)
        {
            AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs();

            try
            {
                Logger.Log("APP INFO RESPONSE DATA DETECTED\n", LogType.Info);
                HolonNETAppInfoResponse appInfoResponse = MessagePackSerializer.Deserialize<HolonNETAppInfoResponse>(response.data, messagePackSerializerOptions);

                string dnaHash = ConvertHoloHashToString(appInfoResponse.data.cell_data[0].cell_id[0]);
                string agentPubKey = ConvertHoloHashToString(appInfoResponse.data.cell_data[0].cell_id[1]);

                if (_updateDnaHashAndAgentPubKey)
                {
                    Config.AgentPubKey = agentPubKey;
                    Config.DnaHash = dnaHash;
                }

                Logger.Log($"AGENT PUB KEY RETURNED FROM CONDUCTOR: {Config.AgentPubKey}", LogType.Info);
                Logger.Log($"DNA HASH RETURNED FROM CONDUCTOR:: {Config.DnaHash}", LogType.Info);

                args = new AppInfoCallBackEventArgs()
                {
                    AgentPubKey = agentPubKey,
                    DnaHash = dnaHash,
                    AppInfo = appInfoResponse,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    Id = response.id.ToString(),
                    InstalledAppId = appInfoResponse.data.installed_app_id,
                    IsCallSuccessful = true,
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occured in HoloNETClient.DecodeAppInfoDataReceived. Reason: {ex}";
                args.IsError = true;
                args.Message = msg;
                HandleError(msg, ex);
            }

            //If either the AgentPubKey or DnaHash is empty then attempt to get from the sandbox cmd.
            if (!args.IsError)
            {
                if (!string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                    SetReadyForZomeCalls();

                else if (_automaticallyAttemptToGetFromSandboxIfConductorFails)
                    RetreiveAgentPubKeyAndDnaHashFromSandbox();
            }

            RaiseAppInfoReceivedEvent(args);
        }

        private void DecodeSignalDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataAsString, string rawBinaryDataDecoded, string rawBinaryDataAfterMessagePackDecodeAsString, string rawBinaryDataAfterMessagePackDecodeDecoded)
        {
            SignalCallBackEventArgs signalCallBackEventArgs = new SignalCallBackEventArgs();

            try
            {
                Logger.Log("SIGNAL DATA DETECTED\n", LogType.Info);

                HoloNETSignalResponse appResponse = MessagePackSerializer.Deserialize<HoloNETSignalResponse>(response.data, messagePackSerializerOptions);
                Dictionary<string, object> signalDataDecoded = new Dictionary<string, object>();
                SignalType signalType = SignalType.App;
                string agentPublicKey = "";
                string dnaHash = "";
                string signalDataAsString = "";

                if (appResponse != null)
                {
                    agentPublicKey = ConvertHoloHashToString(appResponse.App.CellData[0]);
                    dnaHash = ConvertHoloHashToString(appResponse.App.CellData[1]);
                    Dictionary<object, byte[]> signalData = MessagePackSerializer.Deserialize<Dictionary<object, byte[]>>(appResponse.App.SignalData, messagePackSerializerOptions);

                    foreach (object key in signalData.Keys)
                    {
                        signalDataDecoded[key.ToString()] = MessagePackSerializer.Deserialize<object>(signalData[key]);
                        signalDataAsString = string.Concat(signalDataAsString, key.ToString(), "=", signalDataDecoded[key.ToString()], ",");
                    }

                    signalDataAsString = signalDataAsString.Substring(0, signalDataAsString.Length - 1);
                }
                else
                    signalType = SignalType.System;

                signalCallBackEventArgs = new SignalCallBackEventArgs()
                {
                    Id = response != null ? response.id.ToString() : null,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    IsCallSuccessful = true,
                    AgentPubKey = agentPublicKey,
                    DnaHash = dnaHash,
                    RawSignalData = appResponse.App,
                    SignalData = signalDataDecoded,
                    SignalDataAsString = signalDataAsString,
                    SignalType = signalType, //TODO: Need to test for System SignalType... Not even sure if we want to raise this event for System signals? (the js client ignores them currently).
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occured in HoloNETClient.DecodeSignalDataReceived. Reason: {ex}";
                signalCallBackEventArgs.IsError = true;
                signalCallBackEventArgs.Message = msg;
                HandleError(msg, ex);
            }

            RaiseSignalReceivedEvent(signalCallBackEventArgs);
        }

        private void DecodeZomeDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataAsString, string rawBinaryDataDecoded, string rawBinaryDataAfterMessagePackDecodeAsString, string rawBinaryDataAfterMessagePackDecodeDecoded)
        {
            Logger.Log("ZOME RESPONSE DATA DETECTED\n", LogType.Info);
            HolonNETAppResponse appResponse = MessagePackSerializer.Deserialize<HolonNETAppResponse>(response.data, messagePackSerializerOptions);
            string id = response.id.ToString();
            ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs();

            try
            {
                Dictionary<object, object> rawAppResponseData = MessagePackSerializer.Deserialize<Dictionary<object, object>>(appResponse.data, messagePackSerializerOptions);
                Dictionary<string, object> appResponseData = new Dictionary<string, object>();
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                string keyValuePairsAsString = "";
                EntryData entryData = null;
                
                (appResponseData, keyValuePairs, keyValuePairsAsString, entryData) = DecodeRawZomeData(rawAppResponseData, appResponseData, keyValuePairs, keyValuePairsAsString);

                if (_entryDataObjectTypeLookup.ContainsKey(id) && _entryDataObjectTypeLookup[id] != null)
                    entryData.EntryDataObject = MapEntryDataObject(_entryDataObjectTypeLookup[id], keyValuePairs);

                else if (_entryDataObjectLookup.ContainsKey(id) && _entryDataObjectLookup[id] != null)
                    entryData.EntryDataObject = MapEntryDataObject(_entryDataObjectLookup[id], keyValuePairs);


                Logger.Log($"Decoded Data:\n{keyValuePairsAsString}", LogType.Info);
                //Logger.Log($"appResponse.data as string: {DataHelper.ConvertBinaryDataToString(appResponse.data)}", LogType.Debug);
                //Logger.Log($"appResponse.data decoded: {DataHelper.DecodeBinaryData(appResponse.data)}", LogType.Debug);

                zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs
                {
                    Id = id,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    Zome = GetItemFromCache(id, _zomeLookup),
                    ZomeFunction = GetItemFromCache(id, _funcLookup),
                    IsCallSuccessful = true,
                    RawZomeReturnData = rawAppResponseData,
                    ZomeReturnData = appResponseData,
                    KeyValuePair = keyValuePairs,
                    KeyValuePairAsString = keyValuePairsAsString,
                    Entry = entryData,
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };
            }
            catch (Exception ex)
            {
                try
                {
                    object rawAppResponseData = MessagePackSerializer.Deserialize<object>(appResponse.data, messagePackSerializerOptions);
                    byte[] holoHash = rawAppResponseData as byte[];

                    zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs
                    {
                        Id = id,
                        EndPoint = dataReceivedEventArgs.EndPoint,
                        Zome = GetItemFromCache(id, _zomeLookup),
                        ZomeFunction = GetItemFromCache(id, _funcLookup),
                        IsCallSuccessful = true,
                        RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                        RawBinaryDataAsString = rawBinaryDataAsString,
                        RawBinaryDataDecoded = rawBinaryDataDecoded,
                        RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                        RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                        RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                        RawJSONData = dataReceivedEventArgs.RawJSONData,
                        WebSocketResult = dataReceivedEventArgs.WebSocketResult
                    };

                    if (holoHash != null)
                    {
                        string hash = ConvertHoloHashToString(holoHash);
                        Logger.Log($"Decoded Data:\nHoloHash: {hash}", LogType.Info);
                        zomeFunctionCallBackArgs.ZomeReturnHash = hash;
                    }
                    else
                    {
                        string msg = $"An unknown response was received from the conductor for type 'Response' (Zome Response). Response Received: {rawAppResponseData}";

                        zomeFunctionCallBackArgs.IsError = true;
                        zomeFunctionCallBackArgs.IsCallSuccessful = false;
                        zomeFunctionCallBackArgs.Message = msg;
                        HandleError(msg, null);
                    }
                }
                catch (Exception ex2)
                {
                    string msg = $"An unknown error occured in HoloNETClient.DecodeZomeDataReceived. Reason: {ex2}";
                    zomeFunctionCallBackArgs.IsError = true;
                    zomeFunctionCallBackArgs.Message = msg;
                    HandleError(msg, ex2);
                }
            }

            RaiseZomeDataReceivedEvent(zomeFunctionCallBackArgs);
        }

        private (Dictionary<string, object>, Dictionary<string, string> keyValuePair, string keyValuePairAsString, EntryData entry) DecodeRawZomeData(Dictionary<object, object> rawAppResponseData, Dictionary<string, object> appResponseData, Dictionary<string, string> keyValuePair, string keyValuePairAsString, EntryData entryData = null)
        {
            string value = "";
            var options = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

            if (entryData == null)
                entryData = new EntryData();

            try
            {
                foreach (string key in rawAppResponseData.Keys)
                {
                    try
                    {
                        value = "";
                        byte[] bytes = rawAppResponseData[key] as byte[];

                        if (bytes != null)
                        {
                            if (key == "entry")
                            {
                                string byteString = "";

                                for (int i = 0; i < bytes.Length; i++)
                                    byteString = string.Concat(byteString, bytes[i], ",");

                                byteString = byteString.Substring(0, byteString.Length - 1);

                                Dictionary<object, object> entry = MessagePackSerializer.Deserialize<Dictionary<object, object>>(bytes, options);
                                Dictionary<string, object> decodedEntry = new Dictionary<string, object>();

                                if (entry != null)
                                {
                                    foreach (object entryKey in entry.Keys)
                                    {
                                        decodedEntry[entryKey.ToString()] = entry[entryKey].ToString();
                                        keyValuePair[entryKey.ToString()] = entry[entryKey].ToString();
                                        keyValuePairAsString = string.Concat(keyValuePairAsString, entryKey.ToString(), "=", entry[entryKey].ToString(), "\n");
                                    }

                                    entryData.Bytes = bytes;
                                    entryData.BytesString = byteString;
                                    entryData.Entry = decodedEntry;
                                    appResponseData[key] = entryData;
                                }
                            }
                            else
                                value = ConvertHoloHashToString(bytes);
                        }
                        else
                        {
                            Dictionary<object, object> dict = rawAppResponseData[key] as Dictionary<object, object>;

                            if (dict != null)
                            {
                                Dictionary<string, object> tempDict = new Dictionary<string, object>();
                                (tempDict, keyValuePair, keyValuePairAsString, entryData) = DecodeRawZomeData(dict, tempDict, keyValuePair, keyValuePairAsString, entryData);
                                appResponseData[key] = tempDict;
                            }
                            else if (rawAppResponseData[key] != null)
                                value = rawAppResponseData[key].ToString();
                        }

                        if (!string.IsNullOrEmpty(value))
                        {
                            keyValuePairAsString = string.Concat(keyValuePairAsString, key, "=", value, "\n");
                            keyValuePair[key] = value;
                            appResponseData[key] = value;

                            try
                            {
                                switch (key)
                                {
                                    case "hash":
                                        entryData.Hash = value;
                                        break;

                                    case "entry_hash":
                                        entryData.EntryHash = value;
                                        break;

                                    case "prev_action":
                                        entryData.PreviousHash = value;
                                        break;

                                    case "signature":
                                        entryData.Signature = value;
                                        break;

                                    case "action_seq":
                                        entryData.ActionSequence = Convert.ToInt32(value);
                                        break;

                                    case "author":
                                        entryData.Author = value;
                                        break;

                                    case "original_action_address":
                                        entryData.OriginalActionAddress = value;
                                        break;

                                    case "original_entry_address":
                                        entryData.OriginalEntryAddress = value;
                                        break;

                                    case "timestamp":
                                        {
                                            entryData.Timestamp = Convert.ToInt64(value);
                                            long time = entryData.Timestamp / 1000; // Divide by 1,000 because we need milliseconds, not microseconds.
                                            entryData.DateTime = DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime.AddHours(-5).AddMinutes(1);
                                        }
                                        break;

                                    case "type":
                                        entryData.Type = value;
                                        break;

                                    case "entry_type":
                                        entryData.EntryType = value;
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                HandleError("Error in HoloNETClient.DecodeZomeReturnData method.", ex);
                            } 
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError("Error in HoloNETClient.DecodeZomeReturnData method.", ex);
                    }
                 }
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.DecodeZomeReturnData method.", ex);
            }

            return (appResponseData, keyValuePair, keyValuePairAsString, entryData);
        }

        private void RaiseSignalReceivedEvent(SignalCallBackEventArgs signalCallBackEventArgs)
        {
            //TODO: Copy Test Harness logging into appropriate logs in this method for log/debug logtypes... (espcially decoded data)
            Logger.Log(string.Concat("Id: ", signalCallBackEventArgs.Id, ", Is Call Successful: ", signalCallBackEventArgs.IsCallSuccessful ? "True" : "False", ", AgentPubKey: ", signalCallBackEventArgs.AgentPubKey, ", DnaHash: ", signalCallBackEventArgs.DnaHash, " Raw Binary Data: ", signalCallBackEventArgs.RawBinaryData, ", Raw JSON Data: ", signalCallBackEventArgs.RawJSONData, ", Signal Type: ", Enum.GetName(typeof(SignalType), signalCallBackEventArgs.SignalType), ", Signal Data: ", signalCallBackEventArgs.SignalDataAsString, "\n"), LogType.Info);
            OnSignalCallBack?.Invoke(this, signalCallBackEventArgs);
        }

        private void RaiseAppInfoReceivedEvent(AppInfoCallBackEventArgs appInfoCallBackEventArgs)
        {
            Logger.Log(string.Concat("Id: ", appInfoCallBackEventArgs.Id, ", Is Call Successful: ", appInfoCallBackEventArgs.IsCallSuccessful ? "True" : "False", ", AgentPubKey: ", appInfoCallBackEventArgs.AgentPubKey, ", DnaHash: ", appInfoCallBackEventArgs.DnaHash, ", Installed App Id: ", appInfoCallBackEventArgs.InstalledAppId, ", Raw Binary Data: ", appInfoCallBackEventArgs.RawBinaryData, ", Raw JSON Data: ", appInfoCallBackEventArgs.RawJSONData, "\n"), LogType.Info);
            OnAppInfoCallBack?.Invoke(this, appInfoCallBackEventArgs);
        }

        private void RaiseZomeDataReceivedEvent(ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs)
        {
            Logger.Log(string.Concat("Id: ", zomeFunctionCallBackArgs.Id, ", Zome: ", zomeFunctionCallBackArgs.Zome, ", Zome Function: ", zomeFunctionCallBackArgs.ZomeFunction, ", Is Zome Call Successful: ", zomeFunctionCallBackArgs.IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", zomeFunctionCallBackArgs.RawZomeReturnData, ", Zome Return Data: ", zomeFunctionCallBackArgs.ZomeReturnData, ", Zome Return Hash: ", zomeFunctionCallBackArgs.ZomeReturnHash, ", Raw Binary Data: ", zomeFunctionCallBackArgs.RawBinaryData, ", Raw Binary Data As String: ", zomeFunctionCallBackArgs.RawBinaryDataAsString, "Raw Binary Data Decoded: ", zomeFunctionCallBackArgs.RawBinaryDataDecoded, ", Raw Binary Data After MessagePack Decode: ", zomeFunctionCallBackArgs.RawBinaryDataAfterMessagePackDecode, ",  Raw Binary Data After MessagePack Decode As String: ", zomeFunctionCallBackArgs.RawBinaryDataAfterMessagePackDecodeAsString, ", Raw Binary Data Decoded After MessagePack Decode: ", zomeFunctionCallBackArgs.RawBinaryDataAfterMessagePackDecodeDecoded, ", Raw JSON Data: ", zomeFunctionCallBackArgs.RawJSONData), LogType.Info);

            if (_callbackLookup.ContainsKey(zomeFunctionCallBackArgs.Id) && _callbackLookup[zomeFunctionCallBackArgs.Id] != null)
                _callbackLookup[zomeFunctionCallBackArgs.Id].DynamicInvoke(this, zomeFunctionCallBackArgs);

            if (_taskCompletionZomeCallBack.ContainsKey(zomeFunctionCallBackArgs.Id) && _taskCompletionZomeCallBack[zomeFunctionCallBackArgs.Id] != null)
                _taskCompletionZomeCallBack[zomeFunctionCallBackArgs.Id].SetResult(zomeFunctionCallBackArgs);

            OnZomeFunctionCallBack?.Invoke(this, zomeFunctionCallBackArgs);

            // If the zome call requested for this to be cached then stick it in the cache.
            if (_cacheZomeReturnDataLookup.ContainsKey(zomeFunctionCallBackArgs.Id) && _cacheZomeReturnDataLookup[zomeFunctionCallBackArgs.Id])
                _zomeReturnDataLookup[zomeFunctionCallBackArgs.Id] = zomeFunctionCallBackArgs;

            _zomeLookup.Remove(zomeFunctionCallBackArgs.Id);
            _funcLookup.Remove(zomeFunctionCallBackArgs.Id);
            _callbackLookup.Remove(zomeFunctionCallBackArgs.Id);
            _entryDataObjectTypeLookup.Remove(zomeFunctionCallBackArgs.Id);
            _entryDataObjectLookup.Remove(zomeFunctionCallBackArgs.Id);
            _cacheZomeReturnDataLookup.Remove(zomeFunctionCallBackArgs.Id);
            _taskCompletionZomeCallBack.Remove(zomeFunctionCallBackArgs.Id);
        }

        private void SetReadyForZomeCalls()
        {
            RetreivingAgentPubKeyAndDnaHash = false;
            _taskCompletionAgentPubKeyAndDnaHashRetreived.SetResult(new AgentPubKeyDnaHash() { AgentPubKey = Config.AgentPubKey, DnaHash = Config.DnaHash });
            
            IsReadyForZomesCalls = true;
            ReadyForZomeCallsEventArgs eventArgs = new ReadyForZomeCallsEventArgs(EndPoint, Config.DnaHash, Config.AgentPubKey);
            OnReadyForZomeCalls?.Invoke(this, eventArgs);
            _taskCompletionReadyForZomeCalls.SetResult(eventArgs);            
        }

        private string GetItemFromCache(string id, Dictionary<string, string> cache)
        {
            return cache.ContainsKey(id) ? cache[id] : null;
        }

        private void HandleError(string message, Exception exception)
        {
            message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "");
            Logger.Log(message, LogType.Error);

            OnError?.Invoke(this, new HoloNETErrorEventArgs { EndPoint = WebSocket.EndPoint, Reason = message, ErrorDetails = exception });

            switch (Config.ErrorHandlingBehaviour)
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