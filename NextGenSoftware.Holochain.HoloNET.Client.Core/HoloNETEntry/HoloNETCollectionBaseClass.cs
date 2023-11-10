using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using NextGenSoftware.Logging;
using NextGenSoftware.Utilities.ExtentionMethods;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract class HoloNETCollectionBaseClass : CollectionBase//, IDisposable
    //, IDisposable
    {
        private bool _disposeOfHoloNETClient = false;

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


        public delegate void CollectionLoaded(object sender, ZomeFunctionCallBackEventArgs e);

        /// <summary>
        /// Fired after the LoadCollection method has finished loading the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadCollectionFunction` or property `ZomeLoadCollectionFunction` and then maps the data returned from the zome call onto your collection.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
        /// </summary>
        public event CollectionLoaded OnCollectionLoaded;


        public delegate void HoloNETEntryAddedToCollection(object sender, ZomeFunctionCallBackEventArgs e);

        /// <summary>
        /// Fired after an HoloNET Entry has been added to the collection. This can be from either calling the AddHoloNETEntryToCollection function or from the SaveCollection function which will automatically add any new entries to the collection. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeAddEntryToCollectionFunction` or property `ZomeAddEntryToCollectionFunction`.
        /// </summary>
        public event HoloNETEntryAddedToCollection OnHoloNETEntryAddedToCollection;


        public delegate void HoloNETEntryRemovedFromCollection(object sender, ZomeFunctionCallBackEventArgs e);

        /// <summary>
        /// Fired after an HoloNET Entry has been removed from the collection. This can be from either calling the RemoveHoloNETEntryFromCollection function or from the SaveCollection function which will automatically remove any old entries from the collection (ones that have been removed in c# client side code but not yet synced with the rust backend code). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeRemoveEntryToCollectionFunction` or property `ZomeRemoveEntryToCollectionFunction`.
        /// </summary>
        public event HoloNETEntryRemovedFromCollection OnHoloNETEntryRemovedFromCollection;


        public delegate void HoloNETEntriesUpdated(object sender, ZomeFunctionCallBackEventArgs e);

        /// <summary>
        /// Fired after the Save method has finished updating/saving all HoloNET entries in this collection. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param 'zomeUpdateEntriesFunction' or property 'ZomeUpdateEntriesFunction'. If this is omitted then it will call the Save method individually on each HoloNET Entry, but if it is not omitted it will update all entries in one batch operation which will be faster than making several round trips to the Holochain Conductor.
        /// </summary>
        public event HoloNETEntriesUpdated OnHoloNETEntriesUpdated;


        public delegate void Closed(object sender, ZomeFunctionCallBackEventArgs e);

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
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="ZomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the ZomeAddEntryToCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>
        /// <param name="ZomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the ZomeRemoveEntryFromCollectionFunction property. This param is optional.</param>
        /// <param name="autoCallInitialize">Set this to true if you wish HoloNETEntryBaseClass to auto-call the Initialize method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the Connect method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
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
        public HoloNETCollectionBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, string ZomeLoadCollectionFunction = "", string ZomeAddEntryToCollectionFunction = "", string ZomeUpdateEntriesFunction = "", string ZomeRemoveEntryFromCollectionFunction = "", bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(holochainConductorURI, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETDNA != null)
                HoloNETClient.HoloNETDNA = holoNETDNA;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// <param name="logger">An implementation of the ILogger interface. [DefaultLogger](#DefaultLogger) is an example of this and is used by the constructor (top one) that does not have ILogger as a param. You can injet in (DI) your own implementations of the ILogger interface using this param.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="ZomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the ZomeAddEntryToCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>
        /// <param name="ZomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the ZomeRemoveEntryFromCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollectionBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, ILogger logger, string ZomeLoadCollectionFunction = "", string ZomeAddEntryToCollectionFunction = "", string ZomeUpdateEntriesFunction = "", string ZomeRemoveEntryFromCollectionFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient(logger, alsoUseDefaultLogger, holochainConductorURI);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETDNA != null)
                HoloNETClient.HoloNETDNA = holoNETDNA;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// <param name="logger">An implementation of the ILogger interface. [DefaultLogger](#DefaultLogger) is an example of this and is used by the constructor (top one) that does not have ILogger as a param. You can injet in (DI) your own implementations of the ILogger interface using this param.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="ZomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the ZomeAddEntryToCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>
        /// <param name="ZomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the ZomeRemoveEntryFromCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
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
        public HoloNETCollectionBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, ILogger logger, string ZomeLoadCollectionFunction = "", string ZomeAddEntryToCollectionFunction = "", string ZomeUpdateEntriesFunction = "", string ZomeRemoveEntryFromCollectionFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(logger, alsoUseDefaultLogger, holochainConductorURI, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETDNA != null)
                HoloNETClient.HoloNETDNA = holoNETDNA;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// <param name="loggers">Allows you to inject in (DI) more than one implementation of the ILogger interface. HoloNET will then log to each logger injected in.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="ZomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the ZomeAddEntryToCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>
        /// <param name="ZomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the ZomeRemoveEntryFromCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollectionBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, IEnumerable<ILogger> loggers, string ZomeLoadCollectionFunction = "", string ZomeAddEntryToCollectionFunction = "", string ZomeUpdateEntriesFunction = "", string ZomeRemoveEntryFromCollectionFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient(loggers, alsoUseDefaultLogger, holochainConductorURI);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETDNA != null)
                HoloNETClient.HoloNETDNA = holoNETDNA;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// <param name="loggers">Allows you to inject in (DI) more than one implementation of the ILogger interface. HoloNET will then log to each logger injected in.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="ZomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the ZomeAddEntryToCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>
        /// <param name="ZomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the ZomeRemoveEntryFromCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holochainConductorURI">The URI of the Holochain Conductor to connect to. Will default to 'ws://localhost:8888'.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
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
        public HoloNETCollectionBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, IEnumerable<ILogger> loggers, string ZomeLoadCollectionFunction = "", string ZomeAddEntryToCollectionFunction = "", string ZomeUpdateEntriesFunction = "", string ZomeRemoveEntryFromCollectionFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(loggers, alsoUseDefaultLogger, holochainConductorURI, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETDNA != null)
                HoloNETClient.HoloNETDNA = holoNETDNA;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="ZomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the ZomeAddEntryToCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>
        /// <param name="ZomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the ZomeRemoveEntryFromCollectionFunction property. This param is optional.</param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollectionBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETDNA holoNETDNA, string ZomeLoadCollectionFunction = "", string ZomeAddEntryToCollectionFunction = "", string ZomeUpdateEntriesFunction = "", string ZomeRemoveEntryFromCollectionFunction = "", bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient();
            _disposeOfHoloNETClient = true;

            //StoreEntryHashInEntry = storeEntryHashInEntry;
            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETDNA != null)
                HoloNETClient.HoloNETDNA = holoNETDNA;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// <param name="holoNETClient">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="ZomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="ZomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the ZomeAddEntryToCollectionFunction property. This param is optional.</param>
        /// <param name="ZomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>
        /// <param name="ZomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the ZomeRemoveEntryFromCollectionFunction property. This param is optional.</param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollectionBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETClient holoNETClient, string ZomeLoadCollectionFunction = "", string ZomeAddEntryToCollectionFunction = "", string ZomeUpdateEntriesFunction = "", string ZomeRemoveEntryFromCollectionFunction = "", bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = holoNETClient;
            //StoreEntryHashInEntry = storeEntryHashInEntry;
            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// The name of the zome to call the respective ZomeLoadEntryFunction, ZomeCreateEntryFunction, ZomeUpdateEntryFunction & ZomeDeleteEntryFunction.
        /// </summary>
        public string ZomeName { get; set; }

        /// <summary>
        /// The name of the zome function to call to load a collection of entries.
        /// </summary>
        public string ZomeLoadCollectionFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to create a collection of entries.
        /// </summary>
        public string ZomeAddEntryToCollectionFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to delete a collection of entries.
        /// </summary>
        public string ZomeRemoveEntryFromCollectionFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to update a collection of entries.
        /// </summary>
        public string ZomeUpdateEntriesFunction { get; set; }


        /// <summary>
        /// This method will Initialize the HoloNETEntryBaseClass along with the internal HoloNET Client and will raise the OnInitialized event once it has finished initializing. This will also call the Connect and RetrieveAgentPubKeyAndDnaHash methods on the HoloNET client. Once the HoloNET client has successfully connected to the Holochain Conductor, retrieved the AgentPubKey & DnaHash & then raised the OnReadyForZomeCalls event it will raise the OnInitialized event. See also the IsInitializing and the IsInitialized properties.
        /// </summary>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public virtual void Initialize(bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            InitializeAsync(ConnectedCallBackMode.UseCallBackEvents, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns></returns>
        public virtual async Task InitializeAsync(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
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
                    await HoloNETClient.ConnectAsync(HoloNETClient.EndPoint, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
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
        /// This method will load the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadCollectionFunction` or property `ZomeLoadCollectionFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionLoaded event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to load.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackForCollectionEventArgs> LoadCollectionAsync(string collectionAnchor, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
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
                return HandleError<ZomeFunctionCallBackForCollectionEventArgs>("Unknown error occurred in LoadCollectionAsync method in HoloNETEntryBaseClass", ex);
            }
        }

        /// <summary>
        /// This method will load the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadCollectionFunction` or property `ZomeLoadCollectionFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionLoaded event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to load.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs LoadCollection(string collectionAnchor, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return LoadCollectionAsync(collectionAnchor).Result;
        }

        /// <summary>
        /// This method will update the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeUpdateCollectionFunction` or property `ZomeUpdateEntriesFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionUpdated event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to update.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> AddHoloNETEntryToCollectionAsync(string collectionAnchor, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntriesFunction, collectionAnchor);
                ProcessZomeReturnCall(result);
                OnLoaded?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in UpdateCollectionAsync method in HoloNETEntryBaseClass", ex);
            }
        }

        /// <summary>
        /// This method will update the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeUpdateCollectionFunction` or property `ZomeUpdateEntriesFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionUpdated event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to update.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs AddHoloNETEntryToCollection(string collectionAnchor, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return AddHoloNETEntryToCollectionAsync(collectionAnchor).Result;
        }

        /// <summary>
        /// This method will update the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeUpdateCollectionFunction` or property `ZomeUpdateEntriesFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionUpdated event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to update.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> RemoveHoloNETEntryFromCollectionAsync(string collectionAnchor, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntriesFunction, collectionAnchor);
                ProcessZomeReturnCall(result);
                OnLoaded?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in UpdateCollectionAsync method in HoloNETEntryBaseClass", ex);
            }
        }

        /// <summary>
        /// This method will update the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeUpdateCollectionFunction` or property `ZomeUpdateEntriesFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionUpdated event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to update.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs RemoveHoloNETEntryFromCollection(string collectionAnchor, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return RemoveHoloNETEntryFromCollectionAsync(collectionAnchor).Result;
        }



        /// <summary>
        /// This method will update the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeUpdateCollectionFunction` or property `ZomeUpdateEntriesFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionUpdated event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to update.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> SaveCollectionAsync(string collectionAnchor, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntriesFunction, collectionAnchor);
                ProcessZomeReturnCall(result);
                OnLoaded?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in UpdateCollectionAsync method in HoloNETEntryBaseClass", ex);
            }
        }

        /// <summary>
        /// This method will update the Holochain Collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeUpdateCollectionFunction` or property `ZomeUpdateEntriesFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionUpdated event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to update.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs SaveCollection(string collectionAnchor, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return SaveCollectionAsync(collectionAnchor).Result;
        }
       

        /// <summary>
        /// Will close this HoloNET Entry and then shutdown its internal HoloNET instance (if one was not passed in) and its current connetion to the Holochain Conductor and then shutdown all running Holochain Conductors (if configured to do so) as well as any other tasks to shut HoloNET down cleanly. This method calls the ShutdownHoloNET method internally. Once it has finished shutting down HoloNET it will raise the OnClosed event.
        /// You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the OnDisconnected, OnHolochainConductorsShutdownComplete & OnHoloNETShutdownComplete events to notify the caller.
        /// </summary>
        /// <param name="disconnectedCallBackMode">If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.</param>
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        /// <returns></returns>
        public virtual async Task<HoloNETShutdownEventArgs> CloseAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
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
        /// <param name="shutdownHolochainConductorsMode">Once it has successfully disconnected it will automatically call the ShutDownAllHolochainConductors method if the `shutdownHolochainConductorsMode` flag (defaults to `UseHoloNETDNASettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the ShutDownConductors method below for more detail.</param>
        public virtual HoloNETShutdownEventArgs Close(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseHoloNETDNASettings)
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
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        protected virtual void ProcessZomeReturnCall(ZomeFunctionCallBackEventArgs result, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
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

                    if (result.KeyValuePair != null && useReflectionToMapKeyValuePairResponseOntoEntryDataObject)
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

            switch (HoloNETClient.HoloNETDNA.ErrorHandlingBehaviour)
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

        private async Task<ZomeFunctionCallBackEventArgs> CallZomeFunction(string zomeFunctionName, string key, string value, string keyDisplayName, string valueDisplayName, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            ZomeFunctionCallBackEventArgs result = null;

            if (!IsInitialized && !IsInitializing)
                await InitializeAsync();

            if (!string.IsNullOrEmpty(value))
            {
                if (customDataKeyValuePairs != null)
                {
                    dynamic paramsObject = new ExpandoObject();

                    if (!string.IsNullOrEmpty(key))
                    {
                        ExpandoObjectHelpers.AddProperty(paramsObject, key, value);

                        foreach (string k in customDataKeyValuePairs.Keys)
                            ExpandoObjectHelpers.AddProperty(paramsObject, k, customDataKeyValuePairs[k]);

                        result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, zomeFunctionName, paramsObject);
                    }
                    else
                        result = new ZomeFunctionCallBackEventArgs() { IsError = true, Message = $"The key {keyDisplayName} is null, please set before calling this function." };
                }
                else
                    result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, zomeFunctionName, value);

                ProcessZomeReturnCall(result, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
            }
            else
                result = new ZomeFunctionCallBackEventArgs() { IsError = true, Message = $"The value {valueDisplayName} is null, please set before calling this function." };

            return result;
        }

        private T HandleError<T>(string message, Exception exception) where T : CallBackBaseEventArgs, new()
        {
            HandleError(message, exception);
            return new T() { IsError = true, Message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "") };
        }
    }
}
