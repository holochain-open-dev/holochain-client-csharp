
using NextGenSoftware.Logging;
using NextGenSoftware.Utilities.ExtentionMethods;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    //NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties (such as created_date etc) below defined.
    public abstract class HoloNETAuditEntryBaseClass : HoloNETEntryBaseClass, IHoloNETAuditEntryBaseClass
    {
        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETAuditEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the zomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the zomeAddToCollectionFunction property. This param is optional.</param>
        /// <param name="zomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the zomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="zomeRemoveFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the zomeRemoveFromCollectionFunction property. This param is optional.</param>
        /// <param name="isVersionTrackingEnabled">Set this to true if you wish to enable Version Tracking (you will need to make sure your hApp rust code has the version field added to your entry struct).</param>
        /// <param name="isAuditTrackingEnabled">Set this to true if you wish to enable Audit Tracking (the AuditEntries property will be updated every time the entry/object is saved or deleted).</param>
        /// <param name="isAuditAgentCreateModifyDeleteFieldsEnabled">Set this to true if you wish to update the CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedData & DeletedBy properties each time the entry/object is saved or deleted (you will need to make sure your hApp rust code has the created_date, created_by, modified_date, modified_by, deleted_date & deleted_by fields in your entry struct).</param>
        /// <param name="autoCallInitialize">Set this to true if you wish HoloNETAuditEntryBaseClass to auto-call the Initialize method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the Connect method on the HoloNET Client) at a later stage.</param>
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
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, string zomeLoadCollectionFunction = "", string zomeAddToCollectionFunction = "", string zomeUpdateCollectionFunction = "", string zomeRemoveFromCollectionFunction = "", bool isVersionTrackingEnabled = true, bool isAuditTrackingEnabled = true, bool isAuditAgentCreateModifyDeleteFieldsEnabled = true, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, zomeLoadCollectionFunction, zomeAddToCollectionFunction, zomeUpdateCollectionFunction, zomeRemoveFromCollectionFunction, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour)
        {
            IsVersionTrackingEnabled = isVersionTrackingEnabled;
            IsAuditTrackingEnabled = isAuditTrackingEnabled;
            IsAuditAgentCreateModifyDeleteFieldsEnabled = isAuditAgentCreateModifyDeleteFieldsEnabled;
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETAuditEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="logger">An implementation of the ILogger interface. [DefaultLogger](#DefaultLogger) is an example of this and is used by the constructor (top one) that does not have ILogger as a param. You can injet in (DI) your own implementations of the ILogger interface using this param.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the zomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the zomeAddToCollectionFunction property. This param is optional.</param>
        /// <param name="zomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the zomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="zomeRemoveFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the zomeRemoveFromCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="isVersionTrackingEnabled">Set this to true if you wish to enable Version Tracking (you will need to make sure your hApp rust code has the version field added to your entry struct).</param>
        /// <param name="isAuditTrackingEnabled">Set this to true if you wish to enable Audit Tracking (the AuditEntries property will be updated every time the entry/object is saved or deleted).</param>
        /// <param name="isAuditAgentCreateModifyDeleteFieldsEnabled">Set this to true if you wish to update the CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedData & DeletedBy properties each time the entry/object is saved or deleted (you will need to make sure your hApp rust code has the created_date, created_by, modified_date, modified_by, deleted_date & deleted_by fields in your entry struct).</param>
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
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, ILogger logger, string zomeLoadCollectionFunction = "", string zomeAddToCollectionFunction = "", string zomeUpdateCollectionFunction = "", string zomeRemoveFromCollectionFunction = "", bool alsoUseDefaultLogger = false, bool isVersionTrackingEnabled = true, bool isAuditTrackingEnabled = true, bool isAuditAgentCreateModifyDeleteFieldsEnabled = true, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, logger, zomeLoadCollectionFunction, zomeAddToCollectionFunction, zomeUpdateCollectionFunction, zomeRemoveFromCollectionFunction, alsoUseDefaultLogger, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved)
        {
            IsVersionTrackingEnabled = isVersionTrackingEnabled;
            IsAuditTrackingEnabled = isAuditTrackingEnabled;
            IsAuditAgentCreateModifyDeleteFieldsEnabled = isAuditAgentCreateModifyDeleteFieldsEnabled;
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-) 
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETAuditEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="logger">An implementation of the ILogger interface. [DefaultLogger](#DefaultLogger) is an example of this and is used by the constructor (top one) that does not have ILogger as a param. You can injet in (DI) your own implementations of the ILogger interface using this param.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the zomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the zomeAddToCollectionFunction property. This param is optional.</param>
        /// <param name="zomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the zomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="zomeRemoveFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the zomeRemoveFromCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="isVersionTrackingEnabled">Set this to true if you wish to enable Version Tracking (you will need to make sure your hApp rust code has the version field added to your entry struct).</param>
        /// <param name="isAuditTrackingEnabled">Set this to true if you wish to enable Audit Tracking (the AuditEntries property will be updated every time the entry/object is saved or deleted).</param>
        /// <param name="isAuditAgentCreateModifyDeleteFieldsEnabled">Set this to true if you wish to update the CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedData & DeletedBy properties each time the entry/object is saved or deleted (you will need to make sure your hApp rust code has the created_date, created_by, modified_date, modified_by, deleted_date & deleted_by fields in your entry struct).</param>
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
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, ILogger logger, string zomeLoadCollectionFunction = "", string zomeAddToCollectionFunction = "", string zomeUpdateCollectionFunction = "", string zomeRemoveFromCollectionFunction = "", bool alsoUseDefaultLogger = false, bool isVersionTrackingEnabled = true, bool isAuditTrackingEnabled = true, bool isAuditAgentCreateModifyDeleteFieldsEnabled = true, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, logger, zomeLoadCollectionFunction, zomeAddToCollectionFunction, zomeUpdateCollectionFunction, zomeRemoveFromCollectionFunction, alsoUseDefaultLogger, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour)
        {
            IsVersionTrackingEnabled = isVersionTrackingEnabled;
            IsAuditTrackingEnabled = isAuditTrackingEnabled;
            IsAuditAgentCreateModifyDeleteFieldsEnabled = isAuditAgentCreateModifyDeleteFieldsEnabled;
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETAuditEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="loggers">Allows you to inject in (DI) more than one implementation of the ILogger interface. HoloNET will then log to each logger injected in.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the zomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the zomeAddToCollectionFunction property. This param is optional.</param>
        /// <param name="zomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the zomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="zomeRemoveFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the zomeRemoveFromCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="isVersionTrackingEnabled">Set this to true if you wish to enable Version Tracking (you will need to make sure your hApp rust code has the version field added to your entry struct).</param>
        /// <param name="isAuditTrackingEnabled">Set this to true if you wish to enable Audit Tracking (the AuditEntries property will be updated every time the entry/object is saved or deleted).</param>
        /// <param name="isAuditAgentCreateModifyDeleteFieldsEnabled">Set this to true if you wish to update the CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedData & DeletedBy properties each time the entry/object is saved or deleted (you will need to make sure your hApp rust code has the created_date, created_by, modified_date, modified_by, deleted_date & deleted_by fields in your entry struct).</param>
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
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, IEnumerable<ILogger> loggers, string zomeLoadCollectionFunction = "", string zomeAddToCollectionFunction = "", string zomeUpdateCollectionFunction = "", string zomeRemoveFromCollectionFunction = "", bool alsoUseDefaultLogger = false, bool isVersionTrackingEnabled = true, bool isAuditTrackingEnabled = true, bool isAuditAgentCreateModifyDeleteFieldsEnabled = true, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, loggers, zomeLoadCollectionFunction, zomeAddToCollectionFunction, zomeUpdateCollectionFunction, zomeRemoveFromCollectionFunction, alsoUseDefaultLogger, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved)
        {
            IsVersionTrackingEnabled = isVersionTrackingEnabled;
            IsAuditTrackingEnabled = isAuditTrackingEnabled;
            IsAuditAgentCreateModifyDeleteFieldsEnabled = isAuditAgentCreateModifyDeleteFieldsEnabled;
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-) 
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETAuditEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="loggers">Allows you to inject in (DI) more than one implementation of the ILogger interface. HoloNET will then log to each logger injected in.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the zomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the zomeAddToCollectionFunction property. This param is optional.</param>
        /// <param name="zomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the zomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="zomeRemoveFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the zomeRemoveFromCollectionFunction property. This param is optional.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="isVersionTrackingEnabled">Set this to true if you wish to enable Version Tracking (you will need to make sure your hApp rust code has the version field added to your entry struct).</param>
        /// <param name="isAuditTrackingEnabled">Set this to true if you wish to enable Audit Tracking (the AuditEntries property will be updated every time the entry/object is saved or deleted).</param>
        /// <param name="isAuditAgentCreateModifyDeleteFieldsEnabled">Set this to true if you wish to update the CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedData & DeletedBy properties each time the entry/object is saved or deleted (you will need to make sure your hApp rust code has the created_date, created_by, modified_date, modified_by, deleted_date & deleted_by fields in your entry struct).</param>
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
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, IEnumerable<ILogger> loggers, string zomeLoadCollectionFunction = "", string zomeAddToCollectionFunction = "", string zomeUpdateCollectionFunction = "", string zomeRemoveFromCollectionFunction = "", bool alsoUseDefaultLogger = false, bool isVersionTrackingEnabled = true, bool isAuditTrackingEnabled = true, bool isAuditAgentCreateModifyDeleteFieldsEnabled = true, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, loggers, zomeLoadCollectionFunction, zomeAddToCollectionFunction, zomeUpdateCollectionFunction, zomeRemoveFromCollectionFunction, alsoUseDefaultLogger, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour)
        {
            IsVersionTrackingEnabled = isVersionTrackingEnabled;
            IsAuditTrackingEnabled = isAuditTrackingEnabled;
            IsAuditAgentCreateModifyDeleteFieldsEnabled = isAuditAgentCreateModifyDeleteFieldsEnabled;
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-) 
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETAuditEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="holoNETConfig">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the zomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the zomeAddToCollectionFunction property. This param is optional.</param>
        /// <param name="zomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the zomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="zomeRemoveFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the zomeRemoveFromCollectionFunction property. This param is optional.</param>
        /// <param name="isVersionTrackingEnabled">Set this to true if you wish to enable Version Tracking (you will need to make sure your hApp rust code has the version field added to your entry struct).</param>
        /// <param name="isAuditTrackingEnabled">Set this to true if you wish to enable Audit Tracking (the AuditEntries property will be updated every time the entry/object is saved or deleted).</param>
        /// <param name="isAuditAgentCreateModifyDeleteFieldsEnabled">Set this to true if you wish to update the CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedData & DeletedBy properties each time the entry/object is saved or deleted (you will need to make sure your hApp rust code has the created_date, created_by, modified_date, modified_by, deleted_date & deleted_by fields in your entry struct).</param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETConfig holoNETConfig, string zomeLoadCollectionFunction = "", string zomeAddToCollectionFunction = "", string zomeUpdateCollectionFunction = "", string zomeRemoveFromCollectionFunction = "", bool isVersionTrackingEnabled = true, bool isAuditTrackingEnabled = true, bool isAuditAgentCreateModifyDeleteFieldsEnabled = true, bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, holoNETConfig, zomeLoadCollectionFunction, zomeAddToCollectionFunction, zomeUpdateCollectionFunction, zomeRemoveFromCollectionFunction, autoCallInitialize, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved)
        {
            IsVersionTrackingEnabled = isVersionTrackingEnabled;
            IsAuditTrackingEnabled = isAuditTrackingEnabled;
            IsAuditAgentCreateModifyDeleteFieldsEnabled = isAuditAgentCreateModifyDeleteFieldsEnabled;
        }

        /// <summary>
        /// This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations (Load, Save & Delete) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is very similar to HoloNETEntryBaseClass because it extends it by adding auditing capabilities.
        /// NOTE: Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainFieldName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property. See the documentation on GitHub for more info...
        /// NOTE: To use this class you will need to make sure your corresponding rust hApp zome functions/structs have the corresponding properties(such as created_date etc) defined. See the documentation on GitHub for more info...
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-) 
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETAuditEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadEntryFunction">This is the name of the rust zome function in your hApp that will be used to load existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Load method. This also updates the ZomeLoadEntryFunction property.</param>
        /// <param name="zomeCreateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save new Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeCreateEntryFunction property.</param>
        /// <param name="zomeUpdateEntryFunction">This is the name of the rust zome function in your hApp that will be used to save existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Save method. This also updates the ZomeUpdateEntryFunction property.</param>
        /// <param name="zomeDeleteEntryFunction">This is the name of the rust zome function in your hApp that will be used to delete existing Holochain enties that this instance of the HoloNETAuditEntryBaseClass maps onto. This will be used by the Delete method. This also updates the ZomeDeleteEntryFunction property.</param>
        /// <param name="holoNETClient">This is the HoloNETConfig object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the LoadCollection method. This also updates the zomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the AddToCollection method. This also updates the zomeAddToCollectionFunction property. This param is optional.</param>
        /// <param name="zomeUpdateCollectionFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the UpdateCollection method. This also updates the zomeUpdateCollectionFunction property. This param is optional.</param>
        /// <param name="zomeRemoveFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETEntryBaseClass maps onto. This will be used by the RemoveFromCollection method. This also updates the zomeRemoveFromCollectionFunction property. This param is optional.</param>
        /// <param name="isVersionTrackingEnabled">Set this to true if you wish to enable Version Tracking (you will need to make sure your hApp rust code has the version field added to your entry struct).</param>
        /// <param name="isAuditTrackingEnabled">Set this to true if you wish to enable Audit Tracking (the AuditEntries property will be updated every time the entry/object is saved or deleted).</param>
        /// <param name="isAuditAgentCreateModifyDeleteFieldsEnabled">Set this to true if you wish to update the CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedData & DeletedBy properties each time the entry/object is saved or deleted (you will need to make sure your hApp rust code has the created_date, created_by, modified_date, modified_by, deleted_date & deleted_by fields in your entry struct).</param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETConfig once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETClient holoNETClient, string zomeLoadCollectionFunction = "", string zomeAddToCollectionFunction = "", string zomeUpdateCollectionFunction = "", string zomeRemoveFromCollectionFunction = "", bool isVersionTrackingEnabled = true, bool isAuditTrackingEnabled = true, bool isAuditAgentCreateModifyDeleteFieldsEnabled = true, bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, holoNETClient, zomeLoadCollectionFunction, zomeAddToCollectionFunction, zomeUpdateCollectionFunction, zomeRemoveFromCollectionFunction, autoCallInitialize, connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetrieved)
        {
            IsVersionTrackingEnabled = isVersionTrackingEnabled;
            IsAuditTrackingEnabled = isAuditTrackingEnabled;
            IsAuditAgentCreateModifyDeleteFieldsEnabled = isAuditAgentCreateModifyDeleteFieldsEnabled;
        }

        /// <summary>
        /// Set this to true if you wish to enable Version Tracking (you will need to make sure your hApp rust code has the version field added to your entry struct).
        /// </summary>
        public bool IsVersionTrackingEnabled { get; set; }

        /// Set this to true if you wish to enable Audit Tracking (the AuditEntries property will be updated every time the entry/object is saved or deleted).
        public bool IsAuditTrackingEnabled { get; set; }

        /// <summary>
        /// Set this to true if you wish to update the CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, DeletedData & DeletedBy properties each time the entry/object is saved or deleted (you will need to make sure your hApp rust code has the created_date, created_by, modified_date, modified_by, deleted_date & deleted_by fields in your entry struct).
        /// </summary>
        public bool IsAuditAgentCreateModifyDeleteFieldsEnabled { get; set; }

        /// <summary>
        /// GUID Id that is consistent across multiple versions of the entry (each version has a different hash).
        /// </summary>
        [HolochainFieldName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The date the entry was created.
        /// </summary>
        [HolochainFieldName("created_date")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The AgentId who created the entry.
        /// </summary>
        [HolochainFieldName("created_by")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date the entry was last modified.
        /// </summary>
        [HolochainFieldName("modified_date")]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// The AgentId who modifed the entry.
        /// </summary>
        [HolochainFieldName("modified_by")]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The date the entry was soft deleted.
        /// </summary>
        [HolochainFieldName("deleted_date")]
        public DateTime DeletedDate { get; set; }

        /// <summary>
        /// The AgentId who deleted the entry.
        /// </summary>
        [HolochainFieldName("deleted_by")]
        public string DeletedBy { get; set; }

        /// <summary>
        /// Flag showing the whether this entry is active or not.
        /// </summary>
        [HolochainFieldName("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// The current version of the entry.
        /// </summary>
        [HolochainFieldName("version")]
        public int Version { get; set; }

        /// List of all previous hashes along with the type and datetime.
        /// </summary>
        public List<HoloNETAuditEntry> AuditEntries { get; set; } = new List<HoloNETAuditEntry>();

        /// <summary>
        /// This method will load the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp wil need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="entryHash">The hash of the Holochain Entry you wish to load.</param>
        /// <param name="version">This is the version of the Holochain Entry you wish to return. The default is 0 which means the latest will be retreived. NOTE: The version will be added to the customDataKeyValuePairs dictionary with key 'version'.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> LoadAsync(string entryHash, int version = 0, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            if (customDataKeyValuePairs == null)
                customDataKeyValuePairs = new Dictionary<string, string>();

            customDataKeyValuePairs["version"] = version.ToString();
            return await base.LoadAsync(entryHash, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will load the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp wil need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="entryHash">The hash of the Holochain Entry you wish to load.</param>
        /// <param name="version">This is the version of the Holochain Entry you wish to return. The default is 0 which means the latest will be retreived. NOTE: The version will be added to the customDataKeyValuePairs dictionary with key 'version'.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Load(string entryHash, int version = 0, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            if (customDataKeyValuePairs == null)
                customDataKeyValuePairs = new Dictionary<string, string>();

            customDataKeyValuePairs["version"] = version.ToString();
            return base.Load(entryHash, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will load the Holochain entry (it will use the EntryHash property to load from) from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="version">This is the version of the Holochain Entry you wish to return. The default is 0 which means the latest will be retreived. NOTE: The version will be added to the customDataKeyValuePairs dictionary with key 'version'.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> LoadAsync(int version = 0, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            if (customDataKeyValuePairs == null)
                customDataKeyValuePairs = new Dictionary<string, string>();

            customDataKeyValuePairs["version"] = version.ToString();
            return await base.LoadAsync(customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will load the Holochain entry (it will use the EntryHash property to load from) from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="version">This is the version of the Holochain Entry you wish to return. The default is 0 which means the latest will be retreived. NOTE: The version will be added to the customDataKeyValuePairs dictionary with key 'version'.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Load(int version = 0, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            if (customDataKeyValuePairs == null)
                customDataKeyValuePairs = new Dictionary<string, string>();

            customDataKeyValuePairs["version"] = version.ToString();
            return base.Load(customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will load the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customFieldToLoadByValue">The custom field value to load by (if you do not wish to load by the EntryHash).</param>
        /// <param name="customFieldToLoadByKey">The custom field key to load by (if you do not wish to load by the EntryHash). NOTE: This field is only needed if you also pass in a customDataKeyValuePairs dictionary otherwise only the customFieldToLoadByValue is required. If no customDataKeyValuePairs is passed in then this field (customFieldToLoadByKey) will be ignored.</param>
        /// <param name="version">This is the version of the Holochain Entry you wish to return. The default is 0 which means the latest will be retreived. NOTE: The version will be added to the customDataKeyValuePairs dictionary with key 'version'.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the customFieldToLoadByValue will automatically be added to the new params with key customFieldToLoadByKey (make sure your hApp zome function is expecting this name).</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> LoadAsync(string customFieldToLoadByValue, string customFieldToLoadByKey = "", int version = 0, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            if (customDataKeyValuePairs == null)
                customDataKeyValuePairs = new Dictionary<string, string>();

            customDataKeyValuePairs["version"] = version.ToString();
            return await base.LoadAsync(customFieldToLoadByValue, customFieldToLoadByKey, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will load the Holochain entry from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object. It will then raise the OnLoaded event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customFieldToLoadByValue">The custom field value to load by (if you do not wish to load by the EntryHash).</param>
        /// <param name="customFieldToLoadByKey">The custom field key to load by (if you do not wish to load by the EntryHash). NOTE: This field is only needed if you also pass in a customDataKeyValuePairs dictionary otherwise only the customFieldToLoadByValue is required. If no customDataKeyValuePairs is passed in then this field (customFieldToLoadByKey) will be ignored.</param>
        /// <param name="version">This is the version of the Holochain Entry you wish to return. The default is 0 which means the latest will be retreived. NOTE: The version will be added to the customDataKeyValuePairs dictionary with key 'version'.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the customFieldToLoadByValue will automatically be added to the new params with key customFieldToLoadByKey (make sure your hApp zome function is expecting this name).</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Load(string customFieldToLoadByValue, string customFieldToLoadByKey = "", int version = 0, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            if (customDataKeyValuePairs == null)
                customDataKeyValuePairs = new Dictionary<string, string>();

            customDataKeyValuePairs["version"] = version.ToString();
            return base.Load(customFieldToLoadByValue, customFieldToLoadByKey, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// Saves the object and will automatically extrct the properties that need saving (contain the HolochainFieldName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object. 
        /// NOTE: This overload now also allows you to pass in your own params object but it will still dynamically add any properties that have the HolochainFieldName attribute.
        /// </summary>
        /// <param name="customDataKeyValuePair">This is a optional dictionary containing keyvalue pairs of custom data you wish to inject into the params that are sent to the zome function.</param>
        /// <param name="holochainFieldsIsEnabledKeyValuePair">This is a optional dictionary containing keyvalue pairs to allow properties that contain the HolochainFieldName to be omitted from the data sent to the zome function. The key (case senstive) needs to match a property that has the HolochainFieldName attribute.</param>
        /// <param name="cachePropertyInfos">Set this to true if you want HoloNET to cache the property info's for the Entry Data Object (this can reduce the slight overhead used by reflection).</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public override async Task<ZomeFunctionCallBackEventArgs> SaveAsync(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            await ProcessAuditDataAsync();

            if (!IsVersionTrackingEnabled)
                holochainFieldsIsEnabledKeyValuePair["Version"] = false;

            if (!IsAuditAgentCreateModifyDeleteFieldsEnabled)
            {
                holochainFieldsIsEnabledKeyValuePair["CreatedBy"] = false;
                holochainFieldsIsEnabledKeyValuePair["CreatedDate"] = false;
                holochainFieldsIsEnabledKeyValuePair["ModifiedBy"] = false;
                holochainFieldsIsEnabledKeyValuePair["ModifiedDate"] = false;
                holochainFieldsIsEnabledKeyValuePair["DeletedBy"] = false;
                holochainFieldsIsEnabledKeyValuePair["DeletedDate"] = false;
            }

            return await base.SaveAsync(customDataKeyValuePair, holochainFieldsIsEnabledKeyValuePair, cachePropertyInfos, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// Saves the object and will automatically extrct the properties that need saving (contain the HolochainFieldName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object. 
        /// NOTE: This overload now also allows you to pass in your own params object but it will still dynamically add any properties that have the HolochainFieldName attribute.
        /// </summary>
        /// <param name="customDataKeyValuePair">This is a optional dictionary containing keyvalue pairs of custom data you wish to inject into the params that are sent to the zome function.</param>
        /// <param name="holochainFieldsIsEnabledKeyValuePair">This is a optional dictionary containing keyvalue pairs to allow properties that contain the HolochainFieldName to be omitted from the data sent to the zome function. The key (case senstive) needs to match a property that has the HolochainFieldName attribute.</param>
        /// <param name="cachePropertyInfos">Set this to true if you want HoloNET to cache the property info's for the Entry Data Object (this can reduce the slight overhead used by reflection).</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public override ZomeFunctionCallBackEventArgs Save(Dictionary<string, string> customDataKeyValuePair = null, Dictionary<string, bool> holochainFieldsIsEnabledKeyValuePair = null, bool cachePropertyInfos = true, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return SaveAsync(customDataKeyValuePair, holochainFieldsIsEnabledKeyValuePair, cachePropertyInfos, useReflectionToMapKeyValuePairResponseOntoEntryDataObject).Result;
        }

        /// <summary>
        /// This method will save the Holochain entry using the params object passed in containing the hApp rust properties & their values to save to the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeCreateEntryFunction` or property ZomeCreateEntryFunction if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and ZomeUpdateEntryFunction property if it's an existing entry (previously saved object containing a valid value for the EntryHash property). Once it has saved the entry it will then update the EntryHash property with the entry hash returned from the zome call/conductor. The PreviousVersionEntryHash property is also set to the previous EntryHash (if there is one). Once it has finished saving and got a response from the Holochain Conductor it will raise the OnSaved event.
        /// </summary>
        /// <param name="paramsObject">The dynamic data object containing the params you wish to pass to the Create/Update zome function via the CallZomeFunction method. **NOTE:** You do not need to pass this in unless you have a need, if you call one of the overloads that do not have this parameter HoloNETEntryBaseClass will automatically generate this object from any properties in your class that contain the HolochainFieldName attribute.</param>
        /// <param name="autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams">Set this to true if you want HoloNET to auto-generate the updatedEntry object and originalEntryHash params that are passed to the update zome function in your hApp rust code. If this is false then only the paramsObject will be passed to the zome update function and you will need to manually set these object/params yourself. This is an optional param that defaults to true. NOTE: This is set to true for the Save overloads that do not take a paramsobject (use reflection).</param>
        /// <param name="updatedEntryRustParamName">This is the name of the updated entry object param that is in your rust hApp zome update function. This defaults to 'updated_entry'. NOTE: Whatever name that is given here needs to match the same name in your zome update function. NOTE: This param is ignored if the autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams param is false.</param>
        /// <param name="originalEntryHashRustParamName">This is the name of the original entry/action hash param that is in your rust hApp zome update function. This defaults to 'original_action_hash'. NOTE: Whatever name that is given here needs to match the same name in your zome update function. NOTE: This param is ignored if the autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams param is false.</param>
        /// <param name="addAuditInfoToParams">Set this to true (default) to automatically add the additional audit data such as created_date, created_by, modifed_date, modified_by, deleted_date & deleted_by to the paramsObject passed in. This is an optional param.</param>
        /// <param name="addVersionToParams">Set this to true (default) to automatically add the version number to the paramsObject passed in. This is an optional param.</param>
        /// <param name="createdDateRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param CreatedBy (default is created_by). This is an optional param.</param>
        /// <param name="createdByRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param CreatedDate (default is created_date). This is an optional param.</param>
        /// <param name="modifiedDateRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param ModifiedBy (default is modified_by). This is an optional param.</param>
        /// <param name="modifiedByRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param ModifiedDate (default is modified_date). This is an optional param.</param>
        /// <param name="deletedDateRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param DeletedBy (default is deleted_by). This is an optional param.</param>
        /// <param name="deletedByRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param DeletedDate (default is deleted_date). This is an optional param.</param>
        /// <param name="versionRustParamName"><Set this to the name of the rust param in your hApp zome update function for the audit param Version (default is version). This is an optional param.</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> SaveAsync(dynamic paramsObject, bool autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams = true, string updatedEntryRustParamName = "updated_entry", string originalEntryHashRustParamName = "original_action_hash", bool addAuditInfoToParams = true, bool addVersionToParams = true, string createdDateRustParamName = "created_date", string createdByRustParamName = "created_by", string modifiedDateRustParamName = "modified_date", string modifiedByRustParamName = "modified_by", string deletedDateRustParamName = "deleted_date", string deletedByRustParamName = "deleted_by", string versionRustParamName = "version", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            await ProcessAuditDataAsync();
            ProcessAuditRustParams(paramsObject, addAuditInfoToParams, addVersionToParams, createdDateRustParamName, createdByRustParamName, modifiedDateRustParamName, modifiedByRustParamName, deletedDateRustParamName, deletedByRustParamName, versionRustParamName);

            return await base.SaveAsync((object)paramsObject, autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams, updatedEntryRustParamName, originalEntryHashRustParamName, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will save the Holochain entry using the params object passed in containing the hApp rust properties & their values to save to the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeCreateEntryFunction` or property ZomeCreateEntryFunction if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and ZomeUpdateEntryFunction property if it's an existing entry (previously saved object containing a valid value for the EntryHash property). Once it has saved the entry it will then update the EntryHash property with the entry hash returned from the zome call/conductor. The PreviousVersionEntryHash property is also set to the previous EntryHash (if there is one). Once it has finished saving and got a response from the Holochain Conductor it will raise the OnSaved event.
        /// </summary>
        /// <param name="paramsObject">The dynamic data object containing the params you wish to pass to the Create/Update zome function via the CallZomeFunction method. **NOTE:** You do not need to pass this in unless you have a need, if you call one of the overloads that do not have this parameter HoloNETEntryBaseClass will automatically generate this object from any properties in your class that contain the HolochainFieldName attribute.</param>
        /// <param name="autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams">Set this to true if you want HoloNET to auto-generate the updatedEntry object and originalEntryHash params that are passed to the update zome function in your hApp rust code. If this is false then only the paramsObject will be passed to the zome update function and you will need to manually set these object/params yourself. This is an optional param that defaults to true. NOTE: This is set to true for the Save overloads that do not take a paramsobject (use reflection).</param>
        /// <param name="updatedEntryRustParamName">This is the name of the updated entry object param that is in your rust hApp zome update function. This defaults to 'updated_entry'. NOTE: Whatever name that is given here needs to match the same name in your zome update function. NOTE: This param is ignored if the autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams param is false.</param>
        /// <param name="originalEntryHashRustParamName">This is the name of the original entry/action hash param that is in your rust hApp zome update function. This defaults to 'original_action_hash'. NOTE: Whatever name that is given here needs to match the same name in your zome update function. NOTE: This param is ignored if the autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams param is false.</param>
        /// <param name="addAuditInfoToParams">Set this to true (default) to automatically add the additional audit data such as created_date, created_by, modifed_date, modified_by, deleted_date & deleted_by to the paramsObject passed in. This is an optional param.</param>
        /// <param name="addVersionToParams">Set this to true (default) to automatically add the version number to the paramsObject passed in. This is an optional param.</param>
        /// <param name="createdDateRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param CreatedBy (default is created_by). This is an optional param.</param>
        /// <param name="createdByRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param CreatedDate (default is created_date). This is an optional param.</param>
        /// <param name="modifiedDateRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param ModifiedBy (default is modified_by). This is an optional param.</param>
        /// <param name="modifiedByRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param ModifiedDate (default is modified_date). This is an optional param.</param>
        /// <param name="deletedDateRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param DeletedBy (default is deleted_by). This is an optional param.</param>
        /// <param name="deletedByRustParamName">Set this to the name of the rust param in your hApp zome update function for the audit param DeletedDate (default is deleted_date). This is an optional param.</param>
        /// <param name="versionRustParamName"><Set this to the name of the rust param in your hApp zome update function for the audit param Version (default is version). This is an optional param.</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public override ZomeFunctionCallBackEventArgs Save(dynamic paramsObject, bool autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams = true, string updatedEntryRustParamName = "updated_entry", string originalEntryHashRustParamName = "original_action_hash", bool addAuditInfoToParams = true, bool addVersionToParams = true, string createdDateRustParamName = "created_date", string createdByRustParamName = "created_by", string modifiedDateRustParamName = "modified_date", string modifiedByRustParamName = "modified_by", string deletedDateRustParamName = "deleted_date", string deletedByRustParamName = "deleted_by", string versionRustParamName = "version", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            ProcessAuditDataAsync();
            ProcessAuditRustParams(paramsObject, addAuditInfoToParams, addVersionToParams, createdDateRustParamName, createdByRustParamName, modifiedDateRustParamName, modifiedByRustParamName, deletedDateRustParamName, deletedByRustParamName, versionRustParamName);

            return base.Save(paramsObject, autoGeneratUpdatedEntryObjectAndOriginalEntryHashRustParams, updatedEntryRustParamName, originalEntryHashRustParamName, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: This uses the EntryHash property to retrieve the entries hash to soft delete.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public override async Task<ZomeFunctionCallBackEventArgs> DeleteAsync(Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return await DeleteAsync(this.EntryHash, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: This uses the EntryHash property to retrieve the entries hash to soft delete.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// </summary>
        /// <returns></returns>
        public override ZomeFunctionCallBackEventArgs Delete(Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return DeleteAsync(customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject).Result;
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="entryHash">The hash of the Holochain Entry you wish to delete. For the overloads that do not take the entryHash as a paramater it will use the EntryHash property.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public override async Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            if (DeletedDate == DateTime.MinValue)
            {
                DeletedDate = DateTime.Now;

                await WaitTillHoloNETInitializedAsync();
                DeletedBy = this.HoloNETClient.Config.AgentPubKey;
            }

            return await base.DeleteAsync(entryHash, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="entryHash">The hash of the Holochain Entry you wish to delete. For the overloads that do not take the entryHash as a paramater it will use the EntryHash property.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public override ZomeFunctionCallBackEventArgs Delete(string entryHash, Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return DeleteAsync(entryHash, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject).Result;
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customFieldToDeleteByValue">The custom field value to delete by (if you do not wish to delete by the EntryHash).</param>
        /// <param name="customFieldToDeleteByKey">The custom field key to delete by (if you do not wish to delete by the EntryHash). NOTE: This field is only needed if you also pass in a customDataKeyValuePairs dictionary otherwise only the customFieldToLoadByValue is required. If no customDataKeyValuePairs is passed in then this field (customFieldToDeleteByKey) will be ignored.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public override async Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string customFieldToDeleteByValue, string customFieldToDeleteByKey = "", Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            if (DeletedDate == DateTime.MinValue)
            {
                DeletedDate = DateTime.Now;

                await WaitTillHoloNETInitializedAsync();
                DeletedBy = this.HoloNETClient.Config.AgentPubKey;
            }

            return await base.DeleteAsync(customFieldToDeleteByValue, customFieldToDeleteByKey, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
        }

        /// <summary>
        /// This method will soft delete the Holochain entry (the previous version can still be retrieved). This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeDeleteEntryFunction` or property ZomeDeleteEntryFunction. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor and then finally raises the OnDeleted event.
        /// NOTE: The corresponding rust Holochain Entry in your hApp will need to have the same properties contained in your class and have the correct mappings using the HolochainFieldName attribute. Please see HoloNETEntryBaseClass in the documentation/README on the GitHub repo https://github.com/NextGenSoftwareUK/holochain-client-csharp for more info...
        /// </summary>
        /// <param name="customFieldToDeleteByValue">The custom field value to delete by (if you do not wish to delete by the EntryHash).</param>
        /// <param name="customFieldToDeleteByKey">The custom field key to delete by (if you do not wish to delete by the EntryHash). NOTE: This field is only needed if you also pass in a customDataKeyValuePairs dictionary otherwise only the customFieldToLoadByValue is required. If no customDataKeyValuePairs is passed in then this field (customFieldToDeleteByKey) will be ignored.</param>
        /// <param name="customDataKeyValuePairs">This is an optional param where a dictionary containing additional params (KeyValue Pairs) can be passed to the zome function. If this is passed in then the entryHash will automatically be added to the new params with key entry_hash (make sure your hApp zome function is expecting this name)</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        /// <returns></returns>
        public override ZomeFunctionCallBackEventArgs Delete(string customFieldToDeleteByValue, string customFieldToDeleteByKey = "", Dictionary<string, string> customDataKeyValuePairs = null, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return DeleteAsync(customFieldToDeleteByValue, customFieldToDeleteByKey, customDataKeyValuePairs, useReflectionToMapKeyValuePairResponseOntoEntryDataObject).Result;
        }

        /// <summary>
        /// Processes the data returned from the Holochain Conductor/CallZomeFunction method.
        /// </summary>
        /// <param name="result">The data returned from the Holochain Conductor/CallZomeFunction.</param>
        /// <param name="useReflectionToMapKeyValuePairResponseOntoEntryDataObject">This is an optional param, set this to true (default) to map the data returned from the Holochain Conductor onto the Entry Data Object that extends this base class (HoloNETEntryBaseClass or HoloNETAuditEntryBaseClass). This will have a very small performance overhead but means you do not need to do the mapping yourself from the ZomeFunctionCallBackEventArgs.KeyValuePair. </param>
        protected override void ProcessZomeReturnCall(ZomeFunctionCallBackEventArgs result, bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            try
            {
                if (!result.IsError && result.IsCallSuccessful)
                {
                    //Create/Updates/Delete
                    if (IsAuditTrackingEnabled && !string.IsNullOrEmpty(result.ZomeReturnHash))
                    {
                        HoloNETAuditEntry auditEntry = new HoloNETAuditEntry()
                        {
                            DateTime = DateTime.Now,
                            EntryHash = result.ZomeReturnHash
                        };

                        if (result.ZomeFunction == ZomeCreateEntryFunction)
                            auditEntry.Type = HoloNETAuditEntryType.Create;

                        else if (result.ZomeFunction == ZomeUpdateEntryFunction)
                            auditEntry.Type = HoloNETAuditEntryType.Modify;

                        else if (result.ZomeFunction == ZomeDeleteEntryFunction)
                            auditEntry.Type = HoloNETAuditEntryType.Delete;

                        this.AuditEntries.Add(auditEntry);

                        //TODO: Need to persist audit entries to the Rust hApp in it's own audit struct...
                    }
                }

                base.ProcessZomeReturnCall(result, useReflectionToMapKeyValuePairResponseOntoEntryDataObject);
            }
            catch (Exception ex)
            {
                HandleError("Unknown error occurred in ProcessZomeReturnCall method.", ex);
            }
        }

        private async Task ProcessAuditDataAsync()
        {
            if (string.IsNullOrEmpty(EntryHash))
            {
                if (CreatedDate == DateTime.MinValue)
                {
                    CreatedDate = DateTime.Now;

                    await WaitTillHoloNETInitializedAsync();
                    CreatedBy = this.HoloNETClient.Config.AgentPubKey;
                }
            }
            else
            {
                if (ModifiedDate == DateTime.MinValue)
                {
                    ModifiedDate = DateTime.Now;

                    await WaitTillHoloNETInitializedAsync();
                    ModifiedBy = this.HoloNETClient.Config.AgentPubKey;
                }
            }

            if (IsVersionTrackingEnabled)
                this.Version++;
        }

        private dynamic ProcessAuditRustParams(dynamic paramsObject, bool addAuditInfoToParams = true, bool addVersionToParams = true, string createdDateRustParamName = "created_date", string createdByRustParamName = "created_by", string modifiedDateRustParamName = "modified_date", string modifiedByRustParamName = "modified_by", string deletedDateRustParamName = "deleted_date", string deletedByRustParamName = "deleted_by", string versionRustParamName = "version")
        {
            if (addAuditInfoToParams)
            {
                ExpandoObjectHelpers.AddProperty(paramsObject, createdDateRustParamName, CreatedDate.ToShortDateString());
                ExpandoObjectHelpers.AddProperty(paramsObject, createdByRustParamName, CreatedBy);
                ExpandoObjectHelpers.AddProperty(paramsObject, modifiedDateRustParamName, ModifiedDate.ToShortDateString());
                ExpandoObjectHelpers.AddProperty(paramsObject, modifiedByRustParamName, ModifiedBy);
                ExpandoObjectHelpers.AddProperty(paramsObject, deletedDateRustParamName, DeletedDate.ToShortDateString());
                ExpandoObjectHelpers.AddProperty(paramsObject, deletedByRustParamName, DeletedBy);
            }

            if (addVersionToParams)
                ExpandoObjectHelpers.AddProperty(paramsObject, versionRustParamName, Version);

            return paramsObject;
        }
    }
}