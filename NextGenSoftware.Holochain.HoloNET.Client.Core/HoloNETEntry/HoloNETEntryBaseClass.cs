using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using NextGenSoftware.Logging;
using NextGenSoftware.Utilities.ExtentionMethods;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract class HoloNETEntryBaseClass //: IDisposable
    {
        private Dictionary<string, string> _holochainProperties = new Dictionary<string, string>();
        private bool _disposeOfHoloNETClient = false;
        //private PropertyInfo[] _propInfoCache = null;
        private static Dictionary<string, PropertyInfo[]> _dictPropertyInfos = new Dictionary<string, PropertyInfo[]>();

        public delegate void Error(object sender, HoloNETErrorEventArgs e);

        /// <summary>
        /// Fired when there is an error either in HoloNETEntryBaseClass or the HoloNET client itself.  
        /// </summary>
        public event Error OnError;

        public delegate void Initialized(object sender, ReadyForZomeCallsEventArgs e);

        /// <summary>
        /// Fired after the Initialize method has finished initializing the HoloNET client. This will also call the Connect and RetrieveAgentPubKeyAndDnaHash methods on the HoloNET client. This event is then fired when the HoloNET client has successfully connected to the Holochain Conductor, retrieved the AgentPubKey & DnaHash & then fired the OnReadyForZomeCalls event. See also the IsInitializing and the IsInitialized properties.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
        /// </summary>
        public event Initialized OnInitialized;

        public delegate void Loaded(object sender, ZomeFunctionCallBackEventArgs e);

        /// <summary>
        /// Fired after the Load method has finished loading the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
        /// </summary>
        public event Loaded OnLoaded;

        public delegate void Saved(object sender, ZomeFunctionCallBackEventArgs e);

        /// <summary>
        /// Fired after the Save method has finished saving the Holochain entry to the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeCreateEntryFunction` or property ZomeCreateEntryFunction if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and ZomeUpdateEntryFunction property if it's an existing entry (previously saved object containing a valid value for the EntryHash property). Once it has saved the entry it will then update the EntryHash property with the entry hash returned from the zome call/conductor. The PreviousVersionEntryHash property is also set to the previous EntryHash (if there is one).
        /// </summary>
        public event Saved OnSaved;

        public delegate void Deleted(object sender, ZomeFunctionCallBackEventArgs e);

        /// <summary>
        /// Fired after the Delete method has finished deleting the Holochain entry and has received a response from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property `ZomeDeleteEntryFunction`. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor.
        /// </summary>
        public event Deleted OnDeleted;

        public delegate void Closed(object sender, HoloNETShutdownEventArgs e);

        /// <summary>
        /// Fired after the Close method has finished closing the connection to the Holochain Conductor and has shutdown all running Holochain Conductors (if configured to do so). This method calls the ShutdownHoloNET internally.
        /// </summary>
        public event Closed OnClosed;

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="ZomeCreateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to create a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the CreateCollection method. This also updates the ZomeCreateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to update a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeDeleteCollectionFunction">This is the name of the rust zome function in your hApp that will be used to delete a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the DeleteCollection method. This also updates the ZomeDeleteCollectionFunction property. This param is optional.</param>
        /// <param name="autoCallInitialize">Set this to true if you wish HoloNETEntryBaseClass to auto-call the Initialize method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the Connect method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETConfig">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="logToConsole">Set this to true (default) if you wish HoloNET to log to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logToFile">Set this to true (default) if you wish HoloNET to log a log file. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="releativePathToLogFolder">The relative path to the log folder to log to. Will default to a sub-directory called `Logs` within the current working directory. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logFileName">The name of the file to log to. Will default to `HoloNET.log`. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="addAdditionalSpaceAfterEachLogEntry">Set this to true to add additional space after each log entry. The default is false. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="showColouredLogs">Set this to true to enable coloured logs in the console. This default to true. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="debugColour">The colour to use for `Debug` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="infoColour">The colour to use for `Info` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="warningColour">The colour to use for `Warning` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="errorColour">The colour to use for `Error` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, string ZomeLoadCollectionFunction, string ZomeCreateCollectionFunction = "", string ZomeUpdateCollectionFunction = "", string ZomeDeleteCollectionFunction = "", bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(holochainConductorURI, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="logger">An implementation of the ILogger interface. [DefaultLogger](#DefaultLogger) is an example of this and is used by the constructor (top one) that does not have ILogger as a param. You can injet in (DI) your own implementations of the ILogger interface using this param.</param>
        /// <param name="ZomeCreateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to create a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the CreateCollection method. This also updates the ZomeCreateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to update a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeDeleteCollectionFunction">This is the name of the rust zome function in your hApp that will be used to delete a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the DeleteCollection method. This also updates the ZomeDeleteCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETConfig">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, string ZomeLoadCollectionFunction, ILogger logger, string ZomeCreateCollectionFunction = "", string ZomeUpdateCollectionFunction = "", string ZomeDeleteCollectionFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient(logger, alsoUseDefaultLogger, holochainConductorURI);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-) 
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="logger">An implementation of the ILogger interface. [DefaultLogger](#DefaultLogger) is an example of this and is used by the constructor (top one) that does not have ILogger as a param. You can injet in (DI) your own implementations of the ILogger interface using this param.</param>
        /// <param name="ZomeCreateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to create a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the CreateCollection method. This also updates the ZomeCreateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to update a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeDeleteCollectionFunction">This is the name of the rust zome function in your hApp that will be used to delete a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the DeleteCollection method. This also updates the ZomeDeleteCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETConfig">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="logToConsole">Set this to true (default) if you wish HoloNET to log to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logToFile">Set this to true (default) if you wish HoloNET to log a log file. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="releativePathToLogFolder">The relative path to the log folder to log to. Will default to a sub-directory called `Logs` within the current working directory. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logFileName">The name of the file to log to. Will default to `HoloNET.log`. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="addAdditionalSpaceAfterEachLogEntry">Set this to true to add additional space after each log entry. The default is false. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="showColouredLogs">Set this to true to enable coloured logs in the console. This default to true. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="debugColour">The colour to use for `Debug` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="infoColour">The colour to use for `Info` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="warningColour">The colour to use for `Warning` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="errorColour">The colour to use for `Error` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, ILogger logger, string ZomeLoadCollectionFunction, string ZomeCreateCollectionFunction = "", string ZomeUpdateCollectionFunction = "", string ZomeDeleteCollectionFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(logger, alsoUseDefaultLogger, holochainConductorURI, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="loggers">Allows you to inject in (DI) more than one implementation of the ILogger interface. HoloNET will then log to each logger injected in.</param>
        /// <param name="ZomeCreateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to create a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the CreateCollection method. This also updates the ZomeCreateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to update a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeDeleteCollectionFunction">This is the name of the rust zome function in your hApp that will be used to delete a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the DeleteCollection method. This also updates the ZomeDeleteCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETConfig">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, IEnumerable<ILogger> loggers, string ZomeLoadCollectionFunction, string ZomeCreateCollectionFunction = "", string ZomeUpdateCollectionFunction = "", string ZomeDeleteCollectionFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient(loggers, alsoUseDefaultLogger, holochainConductorURI);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-) 
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="loggers">Allows you to inject in (DI) more than one implementation of the ILogger interface. HoloNET will then log to each logger injected in.</param>
        /// <param name="ZomeCreateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to create a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the CreateCollection method. This also updates the ZomeCreateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to update a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeDeleteCollectionFunction">This is the name of the rust zome function in your hApp that will be used to delete a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the DeleteCollection method. This also updates the ZomeDeleteCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETConfig">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="logToConsole">Set this to true (default) if you wish HoloNET to log to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logToFile">Set this to true (default) if you wish HoloNET to log a log file. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="releativePathToLogFolder">The relative path to the log folder to log to. Will default to a sub-directory called `Logs` within the current working directory. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="logFileName">The name of the file to log to. Will default to `HoloNET.log`. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="addAdditionalSpaceAfterEachLogEntry">Set this to true to add additional space after each log entry. The default is false. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="showColouredLogs">Set this to true to enable coloured logs in the console. This default to true. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="debugColour">The colour to use for `Debug` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="infoColour">The colour to use for `Info` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="warningColour">The colour to use for `Warning` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        /// <param name="errorColour">The colour to use for `Error` log entries to the console. **NOTE**: This is only relevant if the built-in DefaultLogger is used.</param>
        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, string ZomeLoadCollectionFunction, IEnumerable<ILogger> loggers, string ZomeCreateCollectionFunction = "", string ZomeUpdateCollectionFunction = "", string ZomeDeleteCollectionFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(loggers, alsoUseDefaultLogger, holochainConductorURI, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-) 
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="holoNETConfig">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="ZomeCreateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to create a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the CreateCollection method. This also updates the ZomeCreateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to update a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeDeleteCollectionFunction">This is the name of the rust zome function in your hApp that will be used to delete a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the DeleteCollection method. This also updates the ZomeDeleteCollectionFunction property. This param is optional.</param>        
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, string ZomeLoadCollectionFunction, HoloNETConfig holoNETConfig, string ZomeCreateCollectionFunction = "", string ZomeUpdateCollectionFunction = "", string ZomeDeleteCollectionFunction = "", bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient();
            _disposeOfHoloNETClient = true;

            //StoreEntryHashInEntry = storeEntryHashInEntry;
            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-) 
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="holoNETClient">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="ZomeCreateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to create a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the CreateCollection method. This also updates the ZomeCreateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to update a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeDeleteCollectionFunction">This is the name of the rust zome function in your hApp that will be used to delete a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the DeleteCollection method. This also updates the ZomeDeleteCollectionFunction property. This param is optional.</param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, string ZomeLoadCollectionFunction, HoloNETClient holoNETClient, string ZomeCreateCollectionFunction = "", string ZomeUpdateCollectionFunction = "", string ZomeDeleteCollectionFunction = "", bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = holoNETClient;
            //StoreEntryHashInEntry = storeEntryHashInEntry;
            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a reference to the internal instance of the HoloNET Client (either the one passed in through a constructor or one created internally.)
        /// </summary>
        public HoloNETClient HoloNETClient { get; set; }

        //public bool StoreEntryHashInEntry { get; set; } = true;

        /// <summary>
        /// This will return true whilst HoloNETEntryBaseClass and it's internal HoloNET client is initializing. The Initialize method will begin the initialization process. This will also call the Connect and RetrieveAgentPubKeyAndDnaHash methods on the HoloNET client. Once the HoloNET client has successfully connected to the Holochain Conductor, retrieved the AgentPubKey & DnaHash & then raised the OnReadyForZomeCalls event it will raise the OnInitialized event. See also the IsInitialized property.
        /// </summary>
        public bool IsInitializing { get; private set; }

        /// <summary>
        /// This will return true once HoloNETEntryBaseClass and it's internal HoloNET client have finished initializing and the OnInitialized event has been raised. See also the IsInitializing property and the Initialize method.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return HoloNETClient != null ? HoloNETClient.IsReadyForZomesCalls : false;
            }
        }

        /// <summary>
        /// Metadata for the Entry.
        /// </summary>
        public EntryData EntryData { get; set; }

        /// <summary>
        /// The Entry hash.
        /// </summary>
        //[HolochainFieldName("entry_hash")]
        public string EntryHash { get; set; }

        //public string Author { get; set; }

        /// <summary>
        /// The previous entry hash.
        /// </summary>
        //[HolochainFieldName("previous_version_entry_hash")]
        public string PreviousVersionEntryHash { get; set; }

        ///// <summary>
        ///// The current version of the entry.
        ///// </summary>
        //[HolochainFieldName("version")]
        //public int Version { get; set; } = 1;

        /// <summary>
        /// The name of the zome to call the respective ZomeLoadEntryFunction, ZomeCreateEntryFunction, ZomeUpdateEntryFunction & ZomeDeleteEntryFunction.
        /// </summary>
        public string ZomeName { get; set; }

        /// <summary>
        /// The name of the zome function to call to load the entry.
        /// </summary>
        public string ZomeLoadEntryFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to create the entry.
        /// </summary>
        public string ZomeCreateEntryFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to update the entry.
        /// </summary>
        public string ZomeUpdateEntryFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to delete the entry.
        /// </summary>
        public string ZomeDeleteEntryFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to load a collection of entries.
        /// </summary>
        public string ZomeLoadCollectionFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to create a collection of entries.
        /// </summary>
        public string ZomeCreateCollectionFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to update a collection of entries.
        /// </summary>
        public string ZomeUpdateCollectionFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to delete a collection of entries.
        /// </summary>
        public string ZomeDeleteCollectionFunction { get; set; }

        /// <summary>
        ///// List of all previous hashes along with the type and datetime.
        ///// </summary>
        //public List<HoloNETAuditEntry> AuditEntries { get; set; } = new List<HoloNETAuditEntry>();

        /// <summary>
        /// This method will load the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="entryHash">The hash of the Holochain Entry you wish to load.</param>
        /// <param name="useReflectionToMapOntoEntryDataObject">Set this to true to map the data returned from the Holochain Conductor onto</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> LoadAsync(string entryHash, bool useReflectionToMapOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeLoadEntryFunction, EntryHash);
                ProcessZomeReturnCall(result, useReflectionToMapOntoEntryDataObject);
                OnLoaded?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in LoadAsync method.", ex);
            }
        }

        /// <summary>
        /// This method will load the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="entryHash">The hash of the Holochain Entry you wish to load.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Load(string entryHash, bool useReflectionToMapOntoEntryDataObject = true)
        {
            return LoadAsync(entryHash, useReflectionToMapOntoEntryDataObject).Result;
        }

        /// <summary>
        /// This method will load the Holochain entry (it will use the EntryHash property to load from) from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> LoadAsync(bool useReflectionToMapOntoEntryDataObject = true)
        {
            return await LoadAsync(EntryHash, useReflectionToMapOntoEntryDataObject);
        }

        /// <summary>
        /// This method will load the Holochain entry (it will use the EntryHash property to load from) from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Load(bool useReflectionToMapOntoEntryDataObject = true)
        {
            return LoadAsync(useReflectionToMapOntoEntryDataObject).Result;
        }

        /// <summary>
        /// This method will load the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customFieldToLoadByValue">The custom field value to load by (if you do not wish to load by the EntryHash).</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> LoadAsync(object customFieldToLoadByValue, bool useReflectionToMapOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeLoadEntryFunction, customFieldToLoadByValue);
                ProcessZomeReturnCall(result, useReflectionToMapOntoEntryDataObject);
                OnLoaded?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in LoadAsync method.", ex);
            }
        }

        /// <summary>
        /// This method will load the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customFieldToLoadByValue">The custom field value to load by (if you do not wish to load by the EntryHash).</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Load(object customFieldToLoadByValue, bool useReflectionToMapOntoEntryDataObject = true)
        {
            return LoadAsync(customFieldToLoadByValue, useReflectionToMapOntoEntryDataObject).Result;
        }

        /// <summary>
        /// This method will load the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadCollectionFunction` or property `ZomeLoadCollectionFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnLoaded event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to load.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> LoadCollectionAsync(string collectionAnchor)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeLoadCollectionFunction, collectionAnchor);
                ProcessZomeReturnCall(result);
                OnLoaded?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in LoadCollectionAsync method in HoloNETEntryBaseClass", ex);
            }
        }

        /// <summary>
        /// This method will load the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadCollectionFunction` or property `ZomeLoadCollectionFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnLoaded event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to load.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs LoadCollection(string collectionAnchor)
        {
            return LoadCollectionAsync(collectionAnchor).Result;
        }

        /// <summary>
        /// This method will save the Holochain entry to the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeCreateEntryFunction` or property ZomeCreateEntryFunction if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and ZomeUpdateEntryFunction property if it's an existing entry (previously saved object containing a valid value for the EntryHash property). Once it has saved the entry it will then update the EntryHash property with the entry hash returned from the zome call/conductor. The PreviousVersionEntryHash property is also set to the previous EntryHash (if there is one). Once it has finished saving and got a response from the Holochain Conductor it will raise the OnSaved event.
        /// NOTE: This will automatically extrct the properties that need saving (contain the HolochainFieldName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customDataKeyValuePair">This is a optional dictionary containing keyvalue pairs of custom data you wish to inject into the params that are sent to the zome function.</param>
        /// <param name="holochainFieldsIsEnabledKeyValuePair">This is a optional dictionary containing keyvalue pairs to allow properties that contain the HolochainFieldName to be omitted from the data sent to the zome function. The key (case senstive) needs to match a property that has the HolochainFieldName attribute.</param>
        /// <param name="cachePropertyInfos">Set this to true if you want HoloNET to cache the property info's for the Entry Data Object (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                dynamic paramsObject = new ExpandoObject();
                dynamic updateParamsObject = new ExpandoObject();
                PropertyInfo[] props = null;
                Dictionary<string, object> zomeCallProps = new Dictionary<string, object>();
                Type type = GetType();
                string typeKey = $"{type.AssemblyQualifiedName}.{type.FullName}";

                if (cachePropertyInfos && _dictPropertyInfos.ContainsKey(typeKey))
                    props = _dictPropertyInfos[typeKey];
                else
                {
                    //Cache the props to reduce overhead of reflection.
                    props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    
                    if (cachePropertyInfos)
                        _dictPropertyInfos[typeKey] = props;
                }

                foreach (PropertyInfo propInfo in props)
                {
                    foreach (CustomAttributeData data in propInfo.CustomAttributes)
                    {
                        if (data.AttributeType == (typeof(HolochainFieldName)))
                        {
                            try
                            {
                                if (data.ConstructorArguments.Count > 0 && data.ConstructorArguments[0].Value != null)
                                {
                                    string key = data.ConstructorArguments[0].Value.ToString();
                                    bool? isEnabled = data.ConstructorArguments[1].Value as bool?;
                                    object value = propInfo.GetValue(this);

                                    if ((isEnabled.HasValue && !isEnabled.Value) || (holochainFieldsIsEnabledKeyValuePair != null && holochainFieldsIsEnabledKeyValuePair.ContainsKey(propInfo.Name) && !holochainFieldsIsEnabledKeyValuePair[propInfo.Name]))
                                        break;

                                    if (key != "entry_hash")
                                    {
                                        if (propInfo.PropertyType == typeof(Guid))
                                            ExpandoObjectHelpers.AddProperty(paramsObject, key, value.ToString());

                                        else if (propInfo.PropertyType == typeof(DateTime))
                                            ExpandoObjectHelpers.AddProperty(paramsObject, key, value.ToString());

                                        else
                                        {
                                            //For some reason ExpandoObject doesn't set null properties so for strings we will set it as a empty string.
                                            if (propInfo.PropertyType == typeof(string) && value == null)
                                                value = "";

                                            ExpandoObjectHelpers.AddProperty(paramsObject, key, value);
                                        }
                                    }
                                    //if (key == "entry_hash" && value != null)
                                    //{
                                    //    //Is an update so we need to include the action_hash for the rust HDK to be able to update the entry...
                                    //    ExpandoObjectHelpers.AddProperty(updateParamsObject, "original_action_hash", HoloNETClient.ConvertHoloHashToBytes(value.ToString()));
                                    //    update = true;
                                    //}
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }

                if (customDataKeyValuePair != null)
                {
                    foreach (string key in customDataKeyValuePair.Keys)
                        ExpandoObjectHelpers.AddProperty(paramsObject, key, customDataKeyValuePair[key]);
                }

                //Update
                if (!string.IsNullOrEmpty(EntryHash))
                {
                    ExpandoObjectHelpers.AddProperty(updateParamsObject, "original_action_hash", HoloNETClient.ConvertHoloHashToBytes(EntryHash));
                    //ExpandoObjectHelpers.AddProperty(updateParamsObject, "original_action_hash", EntryHash);
                    ExpandoObjectHelpers.AddProperty(updateParamsObject, "updated_entry", paramsObject);
                    return await SaveAsync(updateParamsObject);
                }
                else
                    //Create
                    return await SaveAsync(paramsObject);
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in SaveAsync method.", ex);
            }
        }

        /// <summary>
        /// This method will save the Holochain entry to the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeCreateEntryFunction` or property ZomeCreateEntryFunction if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and ZomeUpdateEntryFunction property if it's an existing entry (previously saved object containing a valid value for the EntryHash property). Once it has saved the entry it will then update the EntryHash property with the entry hash returned from the zome call/conductor. The PreviousVersionEntryHash property is also set to the previous EntryHash (if there is one). Once it has finished saving and got a response from the Holochain Conductor it will raise the OnSaved event.
        /// NOTE: This will automatically extrct the properties that need saving (contain the HolochainFieldName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customDataKeyValuePair">This is a optional dictionary containing keyvalue pairs of custom data you wish to inject into the params that are sent to the zome function.</param>
        /// <param name="holochainFieldsIsEnabledKeyValuePair">This is a optional dictionary containing keyvalue pairs to allow properties that contain the HolochainFieldName to be omitted from the data sent to the zome function. The key (case senstive) needs to match a property that has the HolochainFieldName attribute.</param>
        /// <param name="cachePropertyInfos">Set this to true if you want HoloNET to cache the property info's for the Entry Data Object (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Save(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true)
        {
            return SaveAsync(customDataKeyValuePair, holochainFieldsIsEnabledKeyValuePair, cachePropertyInfos).Result;
        }

        /// <summary>
        /// This method will save the Holochain entry using the params object passed in containing the hApp rust properties & their values to save to the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeCreateEntryFunction` or property ZomeCreateEntryFunction if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and ZomeUpdateEntryFunction property if it's an existing entry (previously saved object containing a valid value for the EntryHash property). Once it has saved the entry it will then update the EntryHash property with the entry hash returned from the zome call/conductor. The PreviousVersionEntryHash property is also set to the previous EntryHash (if there is one). Once it has finished saving and got a response from the Holochain Conductor it will raise the OnSaved event.
        /// </summary>
        /// <param name="paramsObject">The dynamic data object containing the params you wish to pass to the Create/Update zome function via the CallZomeFunction method. **NOTE:** You do not need to pass this in unless you have a need, if you call one of the overloads that do not have this parameter HoloNETEntryBaseClass will automatically generate this object from any properties in your class that contain the HolochainFieldName attribute.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> SaveAsync(dynamic paramsObject)
        {
            ZomeFunctionCallBackEventArgs result = null;

            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                if (string.IsNullOrEmpty(EntryHash))
                    result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeCreateEntryFunction, paramsObject);
                else
                    result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntryFunction, paramsObject);

                ProcessZomeReturnCall(result);
                OnSaved?.Invoke(this, result);
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in SaveAsync method.", ex);
            }

            return result;
        }

        /// <summary>
        /// This method will save the Holochain entry using the params object passed in containing the hApp rust properties & their values to save to the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeCreateEntryFunction` or property ZomeCreateEntryFunction if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and ZomeUpdateEntryFunction property if it's an existing entry (previously saved object containing a valid value for the EntryHash property). Once it has saved the entry it will then update the EntryHash property with the entry hash returned from the zome call/conductor. The PreviousVersionEntryHash property is also set to the previous EntryHash (if there is one). Once it has finished saving and got a response from the Holochain Conductor it will raise the OnSaved event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="paramsObject">The dynamic data object containing the params you wish to pass to the Create/Update zome function via the CallZomeFunction method. **NOTE:** You do not need to pass this in unless you have a need, if you call one of the overloads that do not have this parameter HoloNETEntryBaseClass will automatically generate this object from any properties in your class that contain the HolochainFieldName attribute.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Save(dynamic paramsObject)
        {
            return SaveAsync(paramsObject).Result;
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: This uses the EntryHash property to retrieve the entries hash to soft delete.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> DeleteAsync()
        {
            return await DeleteAsync(this.EntryHash);
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: This uses the EntryHash property to retrieve the entries hash to soft delete.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Delete()
        {
            return DeleteAsync().Result;
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="entryHash">The hash of the Holochain Entry you wish to delete. For the overloads that do not take the entryHash as a paramater it will use the EntryHash property.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash)
        {
            ZomeFunctionCallBackEventArgs result = null;

            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                if (!string.IsNullOrEmpty(EntryHash))
                    result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeDeleteEntryFunction, entryHash);
                else
                    result = new ZomeFunctionCallBackEventArgs() { IsError = true, Message = "EntryHash is null, please set before calling this function." };
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in DeleteAsync method.", ex);
            }

            return result;
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="entryHash">The hash of the Holochain Entry you wish to delete. For the overloads that do not take the entryHash as a paramater it will use the EntryHash property.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Delete(string entryHash)
        {
            return DeleteAsync(entryHash).Result;
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customFieldToLoadByValue">The custom field value to delete by (if you do not wish to delete by the EntryHash).</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> DeleteAsync(object customFieldToLoadByValue)
        {
            ZomeFunctionCallBackEventArgs result = null;

            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                if (!string.IsNullOrEmpty(EntryHash))
                    result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeDeleteEntryFunction, customFieldToLoadByValue);
                else
                    result = new ZomeFunctionCallBackEventArgs() { IsError = true, Message = "EntryHash is null, please set before calling this function." };
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in DeleteAsync method.", ex);
            }

            return result;
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customFieldToLoadByValue">The custom field value to delete by (if you do not wish to delete by the EntryHash).</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Delete(object customFieldToLoadByValue)
        {
            return DeleteAsync(customFieldToLoadByValue).Result;
        }

        /// <summary>
        /// Will close this HoloNET Entry and then shutdown its internal HoloNET instance (if one was not passed in) and its current connetion to the Holochain Conductor and then shutdown all running Holochain Conductors (if configured to do so) as well as any other tasks to shut HoloNET down cleanly. This method calls the ShutdownHoloNET method internally. Once it has finished shutting down HoloNET it will raise the OnClosed event.
        /// You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller.
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        /// <returns></returns>
        public virtual async Task<HoloNETShutdownEventArgs> CloseAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            HoloNETShutdownEventArgs returnValue = null;

            try
            {
                if (HoloNETClient != null && _disposeOfHoloNETClient)
                {
                    returnValue = await HoloNETClient.ShutdownHoloNETAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);
                    UnsubscribeEvents();
                    HoloNETClient = null;
                }

                OnClosed?.Invoke(this, returnValue);
            }
            catch (Exception ex)
            {
                returnValue = HandleError<HoloNETShutdownEventArgs>("Unknown error occurred in CloseAsync method.", ex);
            }

            return returnValue;
        }

        /// <summary>
        /// Will close this HoloNET Entry and then shutdown its internal HoloNET instance (if one was not passed in) and its current connetion to the Holochain Conductor and then shutdown all running Holochain Conductors (if configured to do so) as well as any other tasks to shut HoloNET down cleanly. This method calls the ShutdownHoloNET method internally. Once it has finished shutting down HoloNET it will raise the OnClosed event.
        /// Unlike the async version, this non async version will not wait until HoloNET disconnects & shutsdown any Holochain Conductors before it returns to the caller. It will later raise the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events. If you wish to wait for HoloNET to disconnect and shutdown the conductors(s) before returning then please use CloseAsync instead. It will also not contain any Holochain conductor shutdown stats and the HolochainConductorsShutdownEventArgs property will be null (Only the CloseAsync version contains this info).
        /// </summary>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        public virtual HoloNETShutdownEventArgs Close(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            HoloNETShutdownEventArgs returnValue = null;

            try
            {
                if (HoloNETClient != null && _disposeOfHoloNETClient)
                {
                    returnValue = HoloNETClient.ShutdownHoloNET(shutdownHolochainConductorsMode);
                    UnsubscribeEvents();
                    HoloNETClient = null;
                }

                OnClosed?.Invoke(this, returnValue);
            }
            catch (Exception ex)
            {
                returnValue = HandleError<HoloNETShutdownEventArgs>("Unknown error occurred in Close method.", ex);
            }

            return returnValue;
        }

        /// <summary>
        /// This method will Initialize the HoloNETEntryBaseClass along with the internal HoloNET Client and will raise the OnInitialized event once it has finished initializing. This will also call the Connect and RetrieveAgentPubKeyAndDnaHash methods on the HoloNET client. Once the HoloNET client has successfully connected to the Holochain Conductor, retrieved the AgentPubKey & DnaHash & then raised the OnReadyForZomeCalls event it will raise the OnInitialized event. See also the IsInitializing and the IsInitialized properties.
        /// </summary>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        public virtual void Initialize(bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            InitializeAsync(ConnectedCallBackMode.UseCallBackEvents, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This method will Initialize the HoloNETEntryBaseClass along with the internal HoloNET Client and will raise the OnInitialized event once it has finished initializing. This will also call the Connect and RetrieveAgentPubKeyAndDnaHash methods on the HoloNET client. Once the HoloNET client has successfully connected to the Holochain Conductor, retrieved the AgentPubKey & DnaHash & then raised the OnReadyForZomeCalls event it will raise the OnInitialized event. See also the IsInitializing and the IsInitialized properties.
        /// </summary>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns></returns>
        public virtual async Task InitializeAsync(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            try
            {
                if (IsInitialized)
                    return;
                    //throw new InvalidOperationException("The HoloNET Client has already been initialized.");

                if (IsInitializing)
                    return;

                IsInitializing = true;

                HoloNETClient.OnError += HoloNETClient_OnError;
                HoloNETClient.OnReadyForZomeCalls += HoloNETClient_OnReadyForZomeCalls;

                if (HoloNETClient.WebSocket.State != System.Net.WebSockets.WebSocketState.Connecting || HoloNETClient.WebSocket.State != System.Net.WebSockets.WebSocketState.Open)
                    await HoloNETClient.ConnectAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved);
            }
            catch (Exception ex)
            {
                HandleError("Unknown error occurred in InitializeAsync method.", ex);
            }
        }

        /// <summary>
        /// This mehod will call the WaitTillReadyForZomeCallsAsync method on the HoloNET Client. 
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ReadyForZomeCallsEventArgs> WaitTillHoloNETInitializedAsync()
        {
            if (!IsInitialized && !IsInitializing)
                await InitializeAsync();

            return await HoloNETClient.WaitTillReadyForZomeCallsAsync();
        }

        /// <summary>
        /// Clears the PropertyInfo cache used during the Save methods that use reflection to dynamically build the params for the zome function (save/update).
        /// </summary>
        public static void ClearCache()
        {
            _dictPropertyInfos.Clear();
        }

        /// <summary>
        /// Processes the data returned from the Holochain Conductor/CallZomeFunction method.
        /// </summary>
        /// <param name="result">The data returned from the Holochain Conductor/CallZomeFunction.</param>
        protected virtual void ProcessZomeReturnCall(ZomeFunctionCallBackEventArgs result, bool useReflectionToMapOntoEntryDataObject = true)
        {
            try
            {
                if (!result.IsError && result.IsCallSuccessful)
                {
                    //Load
                    if (result.Entry != null)
                    {
                        this.EntryData = result.Entry;

                        if (!string.IsNullOrEmpty(result.Entry.EntryHash))
                            this.EntryHash = result.Entry.EntryHash;

                        if (!string.IsNullOrEmpty(result.Entry.PreviousHash))
                            this.PreviousVersionEntryHash = result.Entry.PreviousHash;

                        //if (!string.IsNullOrEmpty(result.Entry.Author))
                        //    this.Author = result.Entry.Author;
                    }

                    //Create/Updates/Delete
                    if (!string.IsNullOrEmpty(result.ZomeReturnHash))
                    {
                        if (!string.IsNullOrEmpty(this.EntryHash))
                            this.PreviousVersionEntryHash = this.EntryHash;

                        this.EntryHash = result.ZomeReturnHash;
                    }

                    if (result.KeyValuePair != null && useReflectionToMapOntoEntryDataObject)
                    {
                        //if (result.KeyValuePair.ContainsKey("entry_hash") && string.IsNullOrEmpty(result.KeyValuePair["entry_hash"]))
                        //    result.KeyValuePair.Remove("entry_hash");

                        HoloNETClient.MapEntryDataObject(this, result.KeyValuePair);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Unknown error occurred in ProcessZomeReturnCall method.", ex);
            }
        }

        private void UnsubscribeEvents()
        {
            HoloNETClient.OnError -= HoloNETClient_OnError;
            HoloNETClient.OnReadyForZomeCalls -= HoloNETClient_OnReadyForZomeCalls;
        }

        private void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            OnError?.Invoke(this, e);
        }

        private void HoloNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            IsInitializing = false;
            OnInitialized?.Invoke(this, e);
        }

        protected void HandleError(string message, Exception exception)
        {
            message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "");
            Logger.Log(message, LogType.Error);

            OnError?.Invoke(this, new HoloNETErrorEventArgs { EndPoint = HoloNETClient.WebSocket.EndPoint, Reason = message, ErrorDetails = exception });

            switch (HoloNETClient.Config.ErrorHandlingBehaviour)
            {
                case ErrorHandlingBehaviour.AlwaysThrowExceptionOnError:
                    throw new HoloNETException(message, exception, HoloNETClient.WebSocket.EndPoint);

                case ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent:
                    {
                        if (OnError == null)
                            throw new HoloNETException(message, exception, HoloNETClient.WebSocket.EndPoint);
                    }
                    break;
            }
        }

        private T HandleError<T>(string message, Exception exception) where T : CallBackBaseEventArgs, new()
        {
            HandleError(message, exception);
            return new T() { IsError = true, Message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "") };
        }
    }
}
