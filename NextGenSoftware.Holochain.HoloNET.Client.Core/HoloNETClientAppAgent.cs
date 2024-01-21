using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using MessagePack;
using Blake2Fast;
using NextGenSoftware.WebSocket;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETClientAppAgent : HoloNETClientBase, IHoloNETClientAppAgent
    {
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
        private TaskCompletionSource<AgentPubKeyDnaHash> _taskCompletionAgentPubKeyAndDnaHashRetrieved = new TaskCompletionSource<AgentPubKeyDnaHash>();
        private Dictionary<string, TaskCompletionSource<ZomeFunctionCallBackEventArgs>> _taskCompletionZomeCallBack = new Dictionary<string, TaskCompletionSource<ZomeFunctionCallBackEventArgs>>();
        private Dictionary<string, PropertyInfo[]> _dictPropertyInfos = new Dictionary<string, PropertyInfo[]>();
        private TaskCompletionSource<ReadyForZomeCallsEventArgs> _taskCompletionReadyForZomeCalls = new TaskCompletionSource<ReadyForZomeCallsEventArgs>();

        //Events

        public delegate void ZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e);

        /// <summary>
        /// Fired when the Holochain conductor returns the response from a zome function call. This returns the raw data as well as the parsed data returned from the zome function. It also returns the id, zome and zome function that made the call.
        /// </summary>
        public event ZomeFunctionCallBack OnZomeFunctionCallBack;

        public delegate void SignalCallBack(object sender, SignalCallBackEventArgs e);

        /// <summary>
        /// Fired when the Holochain conductor sends signals data. 
        /// </summary>
        public event SignalCallBack OnSignalCallBack;

        //public delegate void ConductorDebugCallBack(object sender, ConductorDebugCallBackEventArgs e);
        //public event ConductorDebugCallBack OnConductorDebugCallBack;

        public delegate void AppInfoCallBack(object sender, AppInfoCallBackEventArgs e);

        /// <summary>
        /// Fired when the client receives AppInfo from the conductor containing the cell id for the running hApp (which in itself contains the AgentPubKey & DnaHash). It also contains the AppId and other info.
        /// </summary>
        public event AppInfoCallBack OnAppInfoCallBack;


        public delegate void ReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e);

        /// <summary>
        /// Fired when the client has successfully connected and reteived the AgentPubKey & DnaHash, meaning it is ready to make zome calls to the Holochain conductor.
        /// </summary>
        public event ReadyForZomeCalls OnReadyForZomeCalls;

        // Properties

        /// <summary>
        /// This property is a boolean and will return true when HoloNET is ready for zome calls after the OnReadyForZomeCalls event is raised.
        /// </summary>
        public bool IsReadyForZomesCalls { get; private set; }

        /// <summary>
        /// This property is a boolean and will return true when HoloNET is retrieving the AgentPubKey & DnaHash using one of the RetrieveAgentPubKeyAndDnaHash, RetrieveAgentPubKeyAndDnaHashFromSandbox or RetrieveAgentPubKeyAndDnaHashFromConductor methods (by default this will occur automatically after it has connected to the Holochain Conductor).
        /// </summary>
        public bool RetrievingAgentPubKeyAndDnaHash { get; private set; }


        /// <summary>
        /// This constructor uses the built-in DefaultLogger and the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientAppAgent(HoloNETDNA holoNETDNA = null) : base(holoNETDNA)
        {

        }

        /// <summary>
        /// This constructor allows you to inject in (DI) your own implementation (logProvider) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProvider">The implementation of the ILogProvider interface (custom logProvider).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientAppAgent(ILogProvider logProvider, bool alsoUseDefaultLogger = false, HoloNETDNA holoNETDNA = null) : base(logProvider, alsoUseDefaultLogger, holoNETDNA)
        {

        }

        /// <summary>
        /// This constructor allows you to inject in (DI) multiple implementations (logProviders) of the ILogProvider interface, which will be added to the Logger.LogProviders collection. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logProviders">The implementations of the ILogProvider interface (custom logProviders).</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom loggers injected in.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientAppAgent(IEnumerable<ILogProvider> logProviders, bool alsoUseDefaultLogger = false, HoloNETDNA holoNETDNA = null) : base(logProviders, alsoUseDefaultLogger, holoNETDNA)
        {

        }

        /// <summary>
        /// This constructor allows you to inject in (DI) a Logger instance (which could contain multiple logProviders). This will then override the default Logger found on the Logger property. This Logger instance is also passed to the WebSocket library. HoloNET will then log to each of these logProviders contained within the Logger. It will also use the settings contained in the HoloNETDNA.
        /// </summary>
        /// <param name="logger">The logger instance to use.</param>
        /// <param name="holoNETDNA">The HoloNETDNA you wish to use for this connection (optional). If this is not passed in then it will use the default HoloNETDNA defined in the HoloNETDNA property.</param>
        public HoloNETClientAppAgent(Logger logger, HoloNETDNA holoNETDNA = null) : base(logger, holoNETDNA)
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
            _getAgentPubKeyAndDnaHashFromConductor = retrieveAgentPubKeyAndDnaHashFromConductor;
            _taskCompletionReadyForZomeCalls = new TaskCompletionSource<ReadyForZomeCallsEventArgs>();
            _taskCompletionAgentPubKeyAndDnaHashRetrieved = new TaskCompletionSource<AgentPubKeyDnaHash>(); //TODO: Need to init all of these in the code base for each call! ;-)

            if (string.IsNullOrEmpty(holochainConductorURI))
                holochainConductorURI = HoloNETDNA.HolochainConductorAppAgentURI;

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
        /// This method will wait (non blocking) until HoloNET is ready to make zome calls after it has connected to the Holochain Conductor and retrived the AgentPubKey & DnaHash. It will then return to the caller with the AgentPubKey & DnaHash. This method will return the same time the OnReadyForZomeCalls event is raised. Unlike all the other methods, this one only contains an async version because the non async version would block all other threads including any UI ones etc.
        /// </summary>
        /// <returns></returns>
        public async Task<ReadyForZomeCallsEventArgs> WaitTillReadyForZomeCallsAsync()
        {
            return await _taskCompletionReadyForZomeCalls.Task;
        }

        /// <summary>
        /// This method will retrieve the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retrieving from the Conductor first. It will call RetrieveAgentPubKeyAndDnaHashFromConductor and RetrieveAgentPubKeyAndDnaHashFromSandboxAsync internally.
        /// </summary>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHash(bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            return RetrieveAgentPubKeyAndDnaHashAsync(RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved).Result;
        }

        /// <summary>
        /// This method will retrieve the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retrieving from the Conductor first. It will call RetrieveAgentPubKeyAndDnaHashFromConductor and RetrieveAgentPubKeyAndDnaHashFromSandboxAsync internally.
        /// </summary>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then raise the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashAsync(RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            if (RetrievingAgentPubKeyAndDnaHash)
                return null;

            RetrievingAgentPubKeyAndDnaHash = true;

            //Try to first get from the conductor.
            if (retrieveAgentPubKeyAndDnaHashFromConductor)
                return await RetrieveAgentPubKeyAndDnaHashFromConductorAsync(null, retrieveAgentPubKeyAndDnaHashMode, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails);

            else if (retrieveAgentPubKeyAndDnaHashFromSandbox)
            {
                AgentPubKeyDnaHash agentPubKeyDnaHash = await RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails);

                if (agentPubKeyDnaHash != null)
                    RetrievingAgentPubKeyAndDnaHash = false;
            }

            return new AgentPubKeyDnaHash() { AgentPubKey = HoloNETDNA.AgentPubKey, DnaHash = HoloNETDNA.DnaHash };
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default).
        /// </summary>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)
        {
            try
            {
                if (RetrievingAgentPubKeyAndDnaHash)
                    return null;

                RetrievingAgentPubKeyAndDnaHash = true;
                Logger.Log("Attempting To Retrieve AgentPubKey & DnaHash From hc sandbox...", LogType.Info, true);

                if (string.IsNullOrEmpty(HoloNETDNA.FullPathToExternalHCToolBinary))
                    HoloNETDNA.FullPathToExternalHCToolBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\hc.exe"); //default to the current path

                Process pProcess = new Process();
                pProcess.StartInfo.WorkingDirectory = HoloNETDNA.FullPathToRootHappFolder;
                pProcess.StartInfo.FileName = "hc";
                //pProcess.StartInfo.FileName = HoloNETDNA.FullPathToExternalHCToolBinary; //TODO: Need to get this working later (think currently has a version conflict with keylairstone? But not urgent because AgentPubKey & DnaHash are retrieved from Conductor anyway.
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
                    if (updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved)
                    {
                        HoloNETDNA.AgentPubKey = agentPubKey;
                        HoloNETDNA.DnaHash = dnaHash;
                        HoloNETDNA.CellId = new byte[2][] { ConvertHoloHashToBytes(dnaHash), ConvertHoloHashToBytes(agentPubKey) };
                    }

                    Logger.Log("AgentPubKey & DnaHash successfully retrieved from hc sandbox.", LogType.Info, false);

                    if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(HoloNETDNA.AgentPubKey) && !string.IsNullOrEmpty(HoloNETDNA.DnaHash))
                        SetReadyForZomeCalls();

                    return new AgentPubKeyDnaHash() { DnaHash = dnaHash, AgentPubKey = agentPubKey };
                }
                else if (automaticallyAttemptToRetrieveFromConductorIfSandBoxFails)
                    await RetrieveAgentPubKeyAndDnaHashFromConductorAsync();
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.retrieveAgentPubKeyAndDnaHashFromSandbox method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default).
        /// </summary>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromSandbox(bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)
        {
            return RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails).Result;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retrieved them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retrieve the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetrieveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromSandbox method. 
        /// </summary>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then raise the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromConductorAsync(string installedAppId = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true)
        {
            try
            {
                //if (RetrievingAgentPubKeyAndDnaHash)
                //    return null;

                if (string.IsNullOrEmpty(installedAppId))
                {
                    if (!string.IsNullOrEmpty(HoloNETDNA.InstalledAppId))
                        installedAppId = HoloNETDNA.InstalledAppId;
                    else
                        installedAppId = "test-app";
                }

                _automaticallyAttemptToGetFromSandboxIfConductorFails = automaticallyAttemptToRetrieveFromSandBoxIfConductorFails;
                RetrievingAgentPubKeyAndDnaHash = true;
                _updateDnaHashAndAgentPubKey = updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved;
                Logger.Log("Attempting To Retrieve AgentPubKey & DnaHash from Holochain Conductor...", LogType.Info, true);

                HoloNETData holoNETData = new HoloNETData()
                {
                    type = "app_info",
                    data = new AppInfoRequest()
                    {
                        installed_app_id = installedAppId
                    }
                };

                await SendHoloNETRequestAsync(holoNETData, HoloNETRequestType.AppInfo);

                if (retrieveAgentPubKeyAndDnaHashMode == RetrieveAgentPubKeyAndDnaHashMode.Wait)
                    return await _taskCompletionAgentPubKeyAndDnaHashRetrieved.Task;
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.retrieveAgentPubKeyAndDnaHashFromConductor method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retrieved them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retrieve the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetrieveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromSandbox method. 
        /// </summary>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromConductor(string installedAppId = null, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true)
        {
            return RetrieveAgentPubKeyAndDnaHashFromConductorAsync(installedAppId, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails).Result;
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
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
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
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
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
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The id of the HoloNET request sent to the Holochain Conductor. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. If this id is not passed used by calling one of the alternative overloads then HoloNET will automatically generate a unique id.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return await CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return await CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.  
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.   
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.    
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.    
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.     
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.      
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            try
            {
                _taskCompletionZomeCallBack[id] = new TaskCompletionSource<ZomeFunctionCallBackEventArgs>();

                //if (WebSocket.State == WebSocketState.Closed || WebSocket.State == WebSocketState.None)
                if (WebSocket.State != WebSocketState.Open && WebSocket.State != WebSocketState.Connecting)
                    await ConnectAsync();

                if (!IsReadyForZomesCalls)
                {
                    if (zomeResultCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
                        await WaitTillReadyForZomeCallsAsync();
                    else
                        return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "HoloNET is not ready to make zome calls yet, please wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                }

                if (string.IsNullOrEmpty(HoloNETDNA.DnaHash))
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "The DnaHash cannot be empty, please either set manually in the HoloNETDNA.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                //throw new InvalidOperationException("The DnaHash cannot be empty, please either set manually in the HoloNETDNA.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call.");

                if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "The AgentPubKey cannot be empty, please either set manually in the HoloNETDNA.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                //throw new InvalidOperationException("The AgentPubKey cannot be empty, please either set manually in the HoloNETDNA.AgentPubKey property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call.");

                Logger.Log($"Calling Zome Function {function} on Zome {zome} with Id {id} On Holochain Conductor...", LogType.Info, true);

                _cacheZomeReturnDataLookup[id] = cachReturnData;

                if (cachReturnData)
                {
                    if (_zomeReturnDataLookup.ContainsKey(id))
                    {
                        Logger.Log("Caching Enabled so returning data from cache...", LogType.Info);
                        Logger.Log(string.Concat("Id: ", _zomeReturnDataLookup[id].Id, ", Zome: ", _zomeReturnDataLookup[id].Zome, ", Zome Function: ", _zomeReturnDataLookup[id].ZomeFunction, ", Raw Zome Return Data: ", _zomeReturnDataLookup[id].RawZomeReturnData, ", Zome Return Data: ", _zomeReturnDataLookup[id].ZomeReturnData, ", JSON Raw Data: ", _zomeReturnDataLookup[id].RawJSONData), LogType.Info);
                        //Logger.Log(string.Concat("Id: ", _zomeReturnDataLookup[id].Id, ", Zome: ", _zomeReturnDataLookup[id].Zome, ", Zome Function: ", _zomeReturnDataLookup[id].ZomeFunction, ", Is Zome Call Successful: ", _zomeReturnDataLookup[id].IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", _zomeReturnDataLookup[id].RawZomeReturnData, ", Zome Return Data: ", _zomeReturnDataLookup[id].ZomeReturnData, ", JSON Raw Data: ", _zomeReturnDataLookup[id].RawJSONData), LogType.Info);

                        if (callback != null)
                            callback.DynamicInvoke(this, _zomeReturnDataLookup[id]);

                        OnZomeFunctionCallBack?.Invoke(this, _zomeReturnDataLookup[id]);
                        _taskCompletionZomeCallBack[id].SetResult(_zomeReturnDataLookup[id]);

                        Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                        _taskCompletionZomeCallBack.Remove(id);

                        return await returnValue;
                    }
                }

                if (matchIdToZomeFuncInCallback || HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour != EnforceRequestToResponseIdMatchingBehaviour.Ignore)
                {
                    _zomeLookup[id] = zome;
                    _funcLookup[id] = function;
                }

                if (callback != null)
                    _callbackLookup[id] = callback;

                //_responseMustHaveMatchingRequestLookup[id] = responseMustHaveMatchingRequest;

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

                //TODO: Also be good to implement same functionality in js client where it will error if there is no matching request to a response (id etc). I think we can make this optional param for these method overloads like responseMustHaveMatchingRequest which defaults to true.

                //byte[][] cellId = await GetCellIdAsync();
                string cellId = $"{HoloNETDNA.AgentPubKey}:{HoloNETDNA.DnaHash}";

                // global using BandPass = (int Min, int Max); //TODO: Need to upgrade to C#12 (.NET 8) before names tuple aliases will work...

                if (_signingCredentialsForCell.ContainsKey(cellId) && _signingCredentialsForCell[cellId] != null)
                {
                    ZomeCall payload = new ZomeCall()
                    {
                        cap_secret = _signingCredentialsForCell[cellId].CapSecret,
                        cell_id = new byte[2][] { ConvertHoloHashToBytes(HoloNETDNA.DnaHash), ConvertHoloHashToBytes(HoloNETDNA.AgentPubKey) },
                        //cell_id = new CellId() { agent_pubkey = ConvertHoloHashToBytes(HoloNETDNA.AgentPubKey), dna_hash = ConvertHoloHashToBytes(HoloNETDNA.DnaHash) },
                        //cell_id = (ConvertHoloHashToBytes(HoloNETDNA.DnaHash), ConvertHoloHashToBytes(HoloNETDNA.AgentPubKey)),
                        fn_name = function,
                        zome_name = zome,
                        payload = MessagePackSerializer.Serialize(paramsObject),
                        provenance = ConvertHoloHashToBytes(HoloNETDNA.AgentPubKey),
                        nonce = RandomNumberGenerator.GetBytes(32),
                        expires_at = DateTime.Now.AddMinutes(5).Ticks / 10 //DateTime.Now.AddMinutes(5).ToBinary(), //Conductor expects it in microseconds.
                    };

                    byte[] hash = Blake2b.ComputeHash(MessagePackSerializer.Serialize(payload));
                    //var sig = Ed25519.Sign(hash, _signingCredentialsForCell[cellId].KeyPair.PrivateKey);
                    //var sig = Sodium.PublicKeyAuth.Sign(hash, _signingCredentialsForCell[cellId].KeyPair.PrivateKey).Take(64).ToArray();
                    var sig = Sodium.PublicKeyAuth.SignDetached(hash, _signingCredentialsForCell[cellId].KeyPair.PrivateKey);

                    ZomeCallSigned signedPayload = new ZomeCallSigned()
                    {
                        cap_secret = payload.cap_secret,
                        cell_id = payload.cell_id,
                        fn_name = payload.fn_name,
                        zome_name = payload.zome_name,
                        payload = payload.payload,
                        provenance = payload.provenance,
                        nonce = payload.nonce,
                        expires_at = payload.expires_at,
                        signature = sig
                    };

                    HoloNETData holoNETData = new HoloNETData()
                    {
                        type = "call_zome",
                        data = signedPayload
                    };

                    await SendHoloNETRequestAsync(holoNETData, HoloNETRequestType.ZomeCall, id);

                    if (zomeResultCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
                    {
                        Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                        return await returnValue;
                    }
                    else
                        return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, Message = "ZomeResultCallBackMode is set to UseCallBackEvents so please wait for OnZomeFunctionCallBack event for zome result." };
                }
                else
                {
                    string msg = $"Error occurred in HoloNETClient.CallZomeFunctionAsync method: Cannot sign zome call when no signing credentials have been authorized for the cell (AgentPubKey: {HoloNETDNA.AgentPubKey}, DnaHash: {HoloNETDNA.DnaHash}).";
                    HandleError(msg, null);
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, Message = msg, IsError = true };
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.CallZomeFunctionAsync method.", ex);
                return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = $"Error occurred in HoloNETClient.CallZomeFunctionAsync method. Details: {ex}", Excception = ex };
            }
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The id of the HoloNET request sent to the Holochain Conductor. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. If this id is not passed used by calling one of the alternative overloads then HoloNET will automatically generate a unique id.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.  
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.    
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.     
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null)
        {
            _currentId++;
            return CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, ConductorResponseCallBackMode.UseCallBackEvents);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents).Result;
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return CallZomeFunction(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false)
        {
            return CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents).Result;
        }

        /// <summary>
        /// This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass.
        /// </summary>
        /// <param name="entryDataObjectType">The type of the data object to map the KeyValuePairs returned from the Holochain Conductor onto.</param>
        /// <param name="keyValuePairs">The KeyValuePairs returned from the Holochain Conductor (after they have been decoded by an internal function called `DecodeRawZomeData`) that will be mapped onto the data object.</param>
        /// <param name="cacheEntryDataObjectPropertyInfo">Set this to true if you want HoloNET to cache the property info for the Entry Data Object passed in (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public async Task<dynamic> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true)
        {
            return await MapEntryDataObjectAsync(Activator.CreateInstance(entryDataObjectType), keyValuePairs, cacheEntryDataObjectPropertyInfo);
        }

        /// <summary>
        /// This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass.
        /// </summary>
        /// <param name="entryDataObjectType">The type of the data object to map the KeyValuePairs returned from the Holochain Conductor onto.</param>
        /// <param name="keyValuePairs">The KeyValuePairs returned from the Holochain Conductor (after they have been decoded by an internal function called `DecodeRawZomeData`) that will be mapped onto the data object.</param>
        /// <param name="cacheEntryDataObjectPropertyInfo">Set this to true if you want HoloNET to cache the property info for the Entry Data Object passed in (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public dynamic MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true)
        {
            return MapEntryDataObjectAsync(entryDataObjectType, keyValuePairs, cacheEntryDataObjectPropertyInfo).Result;
        }

        /// <summary>
        /// This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass.
        /// </summary>
        /// <param name="entryDataObject">The dynamic data object to map the KeyValuePairs returned from the Holochain Conductor onto.</param>
        /// <param name="keyValuePairs">The KeyValuePairs returned from the Holochain Conductor (after they have been decoded by an internal function called `DecodeRawZomeData`) that will be mapped onto the data object.</param>
        /// <param name="cacheEntryDataObjectPropertyInfos">Set this to true if you want HoloNET to cache the property info's for the Entry Data Object passed in (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public async Task<dynamic> MapEntryDataObjectAsync(dynamic entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true)
        {
            try
            {
                PropertyInfo[] props = null;

                if (keyValuePairs != null && entryDataObject != null)
                {
                    Type type = entryDataObject.GetType();
                    string typeKey = $"{type.AssemblyQualifiedName}.{type.FullName}";

                    if (cacheEntryDataObjectPropertyInfos && _dictPropertyInfos.ContainsKey(typeKey))
                        props = _dictPropertyInfos[typeKey];
                    else
                    {
                        //Cache the props to reduce overhead of reflection.
                        props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        if (cacheEntryDataObjectPropertyInfos)
                            _dictPropertyInfos[typeKey] = props;
                    }

                    foreach (PropertyInfo propInfo in props)
                    {
                        foreach (CustomAttributeData data in propInfo.CustomAttributes)
                        {
                            if (data.AttributeType == (typeof(HolochainRustFieldName)))
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

        /// <summary>
        /// This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass.
        /// </summary>
        /// <param name="entryDataObject">The dynamic data object to map the KeyValuePairs returned from the Holochain Conductor onto.</param>
        /// <param name="keyValuePairs">The KeyValuePairs returned from the Holochain Conductor (after they have been decoded by an internal function called `DecodeRawZomeData`) that will be mapped onto the data object.</param>
        /// <param name="cacheEntryDataObjectPropertyInfos">Set this to true if you want HoloNET to cache the property info's for the Entry Data Object passed in (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public dynamic MapEntryDataObject(dynamic entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true)
        {
            return MapEntryDataObjectAsync(entryDataObject, keyValuePairs, cacheEntryDataObjectPropertyInfos).Result;
        }

        /// <summary>
        /// Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the CallZomeFunction methods if the `cacheData` param was set to true for any of the calls.
        /// </summary>
        public override void ClearCache(bool clearPendingRequsts = false)
        {
            _zomeReturnDataLookup.Clear();
            _cacheZomeReturnDataLookup.Clear();
            _dictPropertyInfos.Clear();

            if (clearPendingRequsts)
            {
                _zomeLookup.Clear();
                _funcLookup.Clear();
            }

            base.ClearCache(clearPendingRequsts);
        }

        public override async Task<byte[][]> GetCellIdAsync()
        {
            if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
                await RetrieveAgentPubKeyAndDnaHashAsync();

            return await base.GetCellIdAsync();
        }

        protected override void WebSocket_OnConnected(object sender, ConnectedEventArgs e)
        {
            try
            {
                //If the AgentPubKey & DnaHash have already been retrieved from the hc sandbox command then raise the OnReadyForZomeCalls event.
                if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(HoloNETDNA.AgentPubKey) && !string.IsNullOrEmpty(HoloNETDNA.DnaHash))
                    SetReadyForZomeCalls();

                //Otherwise, if the retrieveAgentPubKeyAndDnaHashFromConductor param was set to true when calling the Connect method, retrieve them now...
                else if (_getAgentPubKeyAndDnaHashFromConductor)
                {
                    if (_connectingAsync)
                        RetrieveAgentPubKeyAndDnaHashFromConductorAsync();
                    else
                        RetrieveAgentPubKeyAndDnaHashFromConductorAsync(null, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents);
                }

                base.WebSocket_OnConnected(sender, e);
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnConnected method.", ex);
            }
        }

        protected override void WebSocket_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            try
            {
                if (_pendingRequests.Count > 0)
                {
                    string requests = "";

                    foreach (string id in _pendingRequests)
                        requests = string.Concat(requests, ", id: ", id, "| zome: ", _zomeLookup[id], "| function: ", _funcLookup[id]);

                    e.Reason = $"Error Occured. The WebSocket closed with the following requests still in progress: {requests}. Reason: {e.Reason}";

                    _pendingRequests.Clear();
                    HandleError(e.Reason, null);
                }

                base.WebSocket_OnDisconnected(sender, e);
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnDisconnected method.", ex);
            }
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
                        case HoloNETResponseType.AppInfo:
                            DecodeAppInfoDataReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.Signal:
                            DecodeSignalDataReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.ZomeResponse:
                            DecodeZomeDataReceived(response, dataReceivedEventArgs);
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

            if (response != null && response.type != null && response.type.ToUpper() == "SIGNAL")
                RaiseSignalReceivedEvent(ProcessResponeError<SignalCallBackEventArgs>(response, dataReceivedEventArgs, "Signal", msg));

            if (response != null && response.id > 0 && _requestTypeLookup != null && _requestTypeLookup.ContainsKey(response.id.ToString()))
            {
                switch (_requestTypeLookup[response.id.ToString()])
                {
                    case HoloNETRequestType.ZomeCall:
                        {
                            ZomeFunctionCallBackEventArgs args = ProcessResponeError<ZomeFunctionCallBackEventArgs>(response, dataReceivedEventArgs, "ZomeCall", msg);
                            args.Zome = GetItemFromCache(response != null ? response.id.ToString() : "", _zomeLookup);
                            args.ZomeFunction = GetItemFromCache(response != null ? response.id.ToString() : "", _funcLookup);
                            RaiseZomeDataReceivedEvent(args);
                        }
                        break;

                    case HoloNETRequestType.AppInfo:
                        RaiseAppInfoReceivedEvent(ProcessResponeError<AppInfoCallBackEventArgs>(response, dataReceivedEventArgs, "AppInfo", msg));
                        break;

                    case HoloNETRequestType.Signal:
                        RaiseSignalReceivedEvent(ProcessResponeError<SignalCallBackEventArgs>(response, dataReceivedEventArgs, "Signal", msg));
                        break;
                }
            }

            return msg;
        }

        private void DecodeAppInfoDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs();
            AppInfoResponse appInfoResponse = null;

            try
            {
                Logger.Log("APP INFO RESPONSE DATA DETECTED\n", LogType.Info);
                appInfoResponse = MessagePackSerializer.Deserialize<AppInfoResponse>(response.data, messagePackSerializerOptions);
                args = CreateHoloNETArgs<AppInfoCallBackEventArgs>(response, dataReceivedEventArgs);

                if (appInfoResponse != null)
                {
                    appInfoResponse.data = ProcessAppInfo(appInfoResponse.data, args);
                    args.AppInfoResponse = appInfoResponse;
                }
                else
                {
                    args.Message = "Error occured in HoloNETClient.DecodeAppInfoDataReceived. appInfoResponse is null.";
                    args.IsError = true;
                    //args.IsCallSuccessful = false;
                    HandleError(args.Message);
                }
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeAppInfoDataReceived. Reason: {ex}";
                args.IsError = true;
                //args.IsCallSuccessful = false;
                args.Message = msg;
                HandleError(msg, ex);
            }

            //If either the AgentPubKey or DnaHash is empty then attempt to get from the sandbox cmd.
            if (!args.IsError)
            {
                if (!string.IsNullOrEmpty(HoloNETDNA.AgentPubKey) && !string.IsNullOrEmpty(HoloNETDNA.DnaHash))
                    SetReadyForZomeCalls();

                else if (_automaticallyAttemptToGetFromSandboxIfConductorFails)
                    RetrieveAgentPubKeyAndDnaHashFromSandbox();
            }

            RaiseAppInfoReceivedEvent(args);
        }

        private void DecodeSignalDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            SignalCallBackEventArgs signalCallBackEventArgs = new SignalCallBackEventArgs();

            try
            {
                Logger.Log("SIGNAL DATA DETECTED\n", LogType.Info);

                SignalResponse appResponse = MessagePackSerializer.Deserialize<SignalResponse>(response.data, messagePackSerializerOptions);
                Dictionary<string, object> signalDataDecoded = new Dictionary<string, object>();
                SignalType signalType = SignalType.App;
                string agentPublicKey = "";
                string dnaHash = "";
                string signalDataAsString = "";

                if (appResponse != null)
                {
                    agentPublicKey = ConvertHoloHashToString(appResponse.App.CellData[0]);
                    dnaHash = ConvertHoloHashToString(appResponse.App.CellData[1]);
                    Dictionary<object, byte[]> signalData = MessagePackSerializer.Deserialize<Dictionary<object, byte[]>>(appResponse.App.Data, messagePackSerializerOptions);

                    foreach (object key in signalData.Keys)
                    {
                        signalDataDecoded[key.ToString()] = MessagePackSerializer.Deserialize<object>(signalData[key]);
                        signalDataAsString = string.Concat(signalDataAsString, key.ToString(), "=", signalDataDecoded[key.ToString()], ",");
                    }

                    signalDataAsString = signalDataAsString.Substring(0, signalDataAsString.Length - 1);
                }
                else
                    signalType = SignalType.System;

                signalCallBackEventArgs = CreateHoloNETArgs<SignalCallBackEventArgs>(response, dataReceivedEventArgs);
                signalCallBackEventArgs.DnaHash = dnaHash;
                signalCallBackEventArgs.AgentPubKey = agentPublicKey;
                signalCallBackEventArgs.RawSignalData = appResponse.App;
                signalCallBackEventArgs.SignalData = signalDataDecoded;
                signalCallBackEventArgs.SignalDataAsString = signalDataAsString;
                signalCallBackEventArgs.SignalType = signalType; //TODO: Need to test for System SignalType... Not even sure if we want to raise this event for System signals? (the js client ignores them currently).
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeSignalDataReceived. Reason: {ex}";
                signalCallBackEventArgs.IsError = true;
                //signalCallBackEventArgs.IsCallSuccessful = false;
                signalCallBackEventArgs.Message = msg;
                HandleError(msg, ex);
            }

            RaiseSignalReceivedEvent(signalCallBackEventArgs);
        }

        private void DecodeZomeDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            Logger.Log("ZOME RESPONSE DATA DETECTED\n", LogType.Info);
            AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);
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

                //if (entryData.EntryDataObject.GetType() == typeof(HoloNETEntryBase))
                //{
                //    entryData.EntryDataObject.OrginalEntry = entryData.EntryDataObject;
                //    entryData.EntryDataObject.OrginalDataKeyValuePairs = entryData.EntryKeyValuePairs;
                //    entryData.EntryDataObject.OrginalKeyValuePairs = keyValuePairs;
                //}

                Logger.Log($"Decoded Data:\n{keyValuePairsAsString}", LogType.Info);

                zomeFunctionCallBackArgs = CreateHoloNETArgs<ZomeFunctionCallBackEventArgs>(response, dataReceivedEventArgs);
                zomeFunctionCallBackArgs.Zome = GetItemFromCache(id, _zomeLookup);
                zomeFunctionCallBackArgs.ZomeFunction = GetItemFromCache(id, _funcLookup);
                zomeFunctionCallBackArgs.RawZomeReturnData = rawAppResponseData;
                zomeFunctionCallBackArgs.KeyValuePair = keyValuePairs;
                zomeFunctionCallBackArgs.KeyValuePairAsString = keyValuePairsAsString;
                zomeFunctionCallBackArgs.Entries[0] = entryData; //TODO: Need to add support for multiple entries ASAP!
            }
            catch (Exception ex)
            {
                try
                {
                    object rawAppResponseData = MessagePackSerializer.Deserialize<object>(appResponse.data, messagePackSerializerOptions);
                    byte[] holoHash = rawAppResponseData as byte[];

                    zomeFunctionCallBackArgs = CreateHoloNETArgs<ZomeFunctionCallBackEventArgs>(response, dataReceivedEventArgs);
                    zomeFunctionCallBackArgs.Zome = GetItemFromCache(id, _zomeLookup);
                    zomeFunctionCallBackArgs.ZomeFunction = GetItemFromCache(id, _funcLookup);

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
                        //zomeFunctionCallBackArgs.IsCallSuccessful = false;
                        zomeFunctionCallBackArgs.Message = msg;
                        HandleError(msg, null);
                    }
                }
                catch (Exception ex2)
                {
                    string msg = $"An unknown error occurred in HoloNETClient.DecodeZomeDataReceived. Reason: {ex2}";
                    zomeFunctionCallBackArgs.IsError = true;
                    //zomeFunctionCallBackArgs.IsCallSuccessful = false;
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
                                    entryData.EntryKeyValuePairs = decodedEntry;
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
            LogEvent("SignalCallBack", signalCallBackEventArgs);
            Logger.Log(string.Concat("AgentPubKey: ", signalCallBackEventArgs.AgentPubKey, ", DnaHash: ", signalCallBackEventArgs.DnaHash, ", Signal Type: ", Enum.GetName(typeof(SignalType), signalCallBackEventArgs.SignalType), ", Signal Data: ", signalCallBackEventArgs.SignalDataAsString, "\n"), LogType.Info);
            OnSignalCallBack?.Invoke(this, signalCallBackEventArgs);
        }

        private void RaiseAppInfoReceivedEvent(AppInfoCallBackEventArgs appInfoCallBackEventArgs)
        {
            LogEvent("AppInfoCallBack", appInfoCallBackEventArgs, string.Concat("AgentPubKey: ", appInfoCallBackEventArgs.AgentPubKey, ", DnaHash: ", appInfoCallBackEventArgs.DnaHash, ", Installed App Id: ", appInfoCallBackEventArgs.InstalledAppId));
            OnAppInfoCallBack?.Invoke(this, appInfoCallBackEventArgs);
        }

        private void RaiseZomeDataReceivedEvent(ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs)
        {
            LogEvent("ZomeFunctionCallBack", zomeFunctionCallBackArgs, string.Concat("Zome: ", zomeFunctionCallBackArgs.Zome, ", Zome Function: ", zomeFunctionCallBackArgs.ZomeFunction, ", Raw Zome Return Data: ", zomeFunctionCallBackArgs.RawZomeReturnData, ", Zome Return Data: ", zomeFunctionCallBackArgs.ZomeReturnData, ", Zome Return Hash: ", zomeFunctionCallBackArgs.ZomeReturnHash));

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
            RetrievingAgentPubKeyAndDnaHash = false;
            _taskCompletionAgentPubKeyAndDnaHashRetrieved.SetResult(new AgentPubKeyDnaHash() { AgentPubKey = HoloNETDNA.AgentPubKey, DnaHash = HoloNETDNA.DnaHash });

            IsReadyForZomesCalls = true;
            ReadyForZomeCallsEventArgs eventArgs = new ReadyForZomeCallsEventArgs(EndPoint, HoloNETDNA.DnaHash, HoloNETDNA.AgentPubKey);
            OnReadyForZomeCalls?.Invoke(this, eventArgs);
            _taskCompletionReadyForZomeCalls.SetResult(eventArgs);
        }
    }
}