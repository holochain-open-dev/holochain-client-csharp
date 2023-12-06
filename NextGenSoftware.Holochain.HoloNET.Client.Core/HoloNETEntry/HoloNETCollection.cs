using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextGenSoftware.Logging;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    //public abstract class HoloNETCollectionBaseClass : CollectionBase//, IDisposable
    public class HoloNETCollection<T> : List<T> where T : HoloNETEntryBase  // : CollectionBase//, IDisposable
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


        public delegate void Closed(object sender, HoloNETShutdownEventArgs e);

        /// <summary>
        /// Fired after the Close method has finished closing the connection to the Holochain Conductor and has shutdown all running Holochain Conductors (if configured to do so). This method calls the ShutdownHoloNET internally.
        /// </summary>
        public event Closed OnClosed;

        /// <summary>
        /// This is a new class introduced in HoloNET 3 that makes it super easy to manage collections of HoloNETEntries so you do not need to interact with the client directly.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBase class or HoloNETEntryBase class as well as other HoloNETCollection's) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETCollection maps onto. This will be used by the AddHoloNETEntryToCollectionAndSave method. This also updates the ZomeAddEntryToCollectionFunction property.</param>
        /// <param name="zomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the RemoveHoloNETEntryFromCollectionAndSave method. This also updates the ZomeRemoveEntryFromCollectionFunction property.</param>
        /// <param name="zomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETCollection maps onto. This will be used by the SaveCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>        
        /// <param name="autoCallInitialize">Set this to true if you wish HoloNETEntryBaseClass to auto-call the Initialize method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the Connect method on the HoloNET Client) at a later stage.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollection(string zomeName, string zomeLoadCollectionFunction, string zomeAddEntryToCollectionFunction, string zomeRemoveEntryFromCollectionFunction, string zomeUpdateEntriesFunction = "", bool autoCallInitialize = true, HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient(holoNETDNA);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadCollectionFunction = zomeLoadCollectionFunction;
            ZomeAddEntryToCollectionFunction = zomeAddEntryToCollectionFunction;
            ZomeRemoveEntryFromCollectionFunction = zomeRemoveEntryFromCollectionFunction;
            ZomeUpdateEntriesFunction = zomeUpdateEntriesFunction;

            if (holoNETDNA != null)
                HoloNETClient.HoloNETDNA = holoNETDNA;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new class introduced in HoloNET 3 that makes it super easy to manage collections of HoloNETEntries so you do not need to interact with the client directly.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBase class or HoloNETEntryBase class as well as other HoloNETCollection's) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETCollection maps onto. This will be used by the AddHoloNETEntryToCollectionAndSave method. This also updates the ZomeAddEntryToCollectionFunction property.</param>
        /// <param name="zomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the RemoveHoloNETEntryFromCollectionAndSave method. This also updates the ZomeRemoveEntryFromCollectionFunction property.</param>
        /// <param name="logProvider">An implementation of the ILogProvider interface. [DefaultLogger](#DefaultLogger) is an example of this and is used by the constructor (top one) that does not have logProvider as a param. You can injet in (DI) your own implementations of the ILogProvider interface using this param.</param>
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="zomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETCollection maps onto. This will be used by the SaveCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>        
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollection(string zomeName, string zomeLoadCollectionFunction, string zomeAddEntryToCollectionFunction, string zomeRemoveEntryFromCollectionFunction, ILogProvider logProvider, bool alsoUseDefaultLogger = false, string zomeUpdateEntriesFunction = "", bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(logProvider, alsoUseDefaultLogger, holoNETDNA);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadCollectionFunction = zomeLoadCollectionFunction;
            ZomeAddEntryToCollectionFunction = zomeAddEntryToCollectionFunction;
            ZomeRemoveEntryFromCollectionFunction = zomeRemoveEntryFromCollectionFunction;
            ZomeUpdateEntriesFunction = zomeUpdateEntriesFunction;

            if (holoNETDNA != null)
                HoloNETClient.HoloNETDNA = holoNETDNA;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new class introduced in HoloNET 3 that makes it super easy to manage collections of HoloNETEntries so you do not need to interact with the client directly.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBase class or HoloNETEntryBase class as well as other HoloNETCollection's) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETCollection maps onto. This will be used by the AddHoloNETEntryToCollectionAndSave method. This also updates the ZomeAddEntryToCollectionFunction property.</param>
        /// <param name="zomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the RemoveHoloNETEntryFromCollectionAndSave method. This also updates the ZomeRemoveEntryFromCollectionFunction property.</param>
        /// <param name="logProviders">Allows you to inject in (DI) more than one implementation of the ILogProvider interface. HoloNET will then log to each logProvider injected in. </param>
        /// <param name="zomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETCollection maps onto. This will be used by the SaveCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>        
        /// <param name="alsoUseDefaultLogger">Set this to true if you wish HoloNET to also log to the DefaultLogger as well as any custom logger injected in. </param>
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollection(string zomeName, string zomeLoadCollectionFunction, string zomeAddEntryToCollectionFunction, string zomeRemoveEntryFromCollectionFunction, IEnumerable<ILogProvider> logProviders, string zomeUpdateEntriesFunction = "", bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient(logProviders, alsoUseDefaultLogger, holoNETDNA);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadCollectionFunction = zomeLoadCollectionFunction;
            ZomeAddEntryToCollectionFunction = zomeAddEntryToCollectionFunction;
            ZomeRemoveEntryFromCollectionFunction = zomeRemoveEntryFromCollectionFunction;
            ZomeUpdateEntriesFunction = zomeUpdateEntriesFunction;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new class introduced in HoloNET 3 that makes it super easy to manage collections of HoloNETEntries so you do not need to interact with the client directly.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBase class or HoloNETEntryBase class as well as other HoloNETCollection's) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETCollection maps onto. This will be used by the AddHoloNETEntryToCollectionAndSave method. This also updates the ZomeAddEntryToCollectionFunction property.</param>
        /// <param name="zomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the RemoveHoloNETEntryFromCollectionAndSave method. This also updates the ZomeRemoveEntryFromCollectionFunction property.</param>
        /// <param name="logger">Allows you to inject in (DI) more than one implementation of the ILogger interface. HoloNET will then log to each logger injected in.</param>
        /// <param name="zomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETCollection maps onto. This will be used by the SaveCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>        
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="holoNETDNA">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollection(string zomeName, string zomeLoadCollectionFunction, string zomeAddEntryToCollectionFunction, string zomeRemoveEntryFromCollectionFunction, Logger logger, string zomeUpdateEntriesFunction = "", bool autoCallInitialize = true, HoloNETDNA holoNETDNA = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = new HoloNETClient(logger, holoNETDNA);
            _disposeOfHoloNETClient = true;

            ZomeName = zomeName;
            ZomeLoadCollectionFunction = zomeLoadCollectionFunction;
            ZomeAddEntryToCollectionFunction = zomeAddEntryToCollectionFunction;
            ZomeRemoveEntryFromCollectionFunction = zomeRemoveEntryFromCollectionFunction;
            ZomeUpdateEntriesFunction = zomeUpdateEntriesFunction;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a new class introduced in HoloNET 3 that makes it super easy to manage collections of HoloNETEntries so you do not need to interact with the client directly.
        /// It has two main types of constructors, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBase class or HoloNETEntryBase class as well as other HoloNETCollection's) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class. 
        /// NOTE: This is a preview of some of the advanced functionality that will be present in the upcoming .NET HDK Low Code Generator, which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#. HAppy Days! ;-)
        /// </summary>
        /// <param name="zomeName">This is the name of the rust zome in your hApp that this instance of the HoloNETEntryBaseClass maps onto. This will also update the ZomeName property.</param>
        /// <param name="zomeLoadCollectionFunction">This is the name of the rust zome function in your hApp that will be used to load a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the LoadCollection method. This also updates the ZomeLoadCollectionFunction property.</param>
        /// <param name="zomeAddEntryToCollectionFunction">This is the name of the rust zome function in your hApp that will be used to add Holochain Entries to a collection that this instance of the HoloNETCollection maps onto. This will be used by the AddHoloNETEntryToCollectionAndSave method. This also updates the ZomeAddEntryToCollectionFunction property.</param>
        /// <param name="zomeRemoveEntryFromCollectionFunction">This is the name of the rust zome function in your hApp that will be used to remove a Holochain Entry from a Holochain collection that this instance of the HoloNETCollection maps onto. This will be used by the RemoveHoloNETEntryFromCollectionAndSave method. This also updates the ZomeRemoveEntryFromCollectionFunction property.</param>
        /// <param name="holoNETClient">This is the HoloNETDNA object that controls how HoloNET operates. This will be passed into the internally created instance of the HoloNET Client.</param>
        /// <param name="zomeUpdateEntriesFunction">This is the name of the rust zome function in your hApp that will be used to bulk update Holochain Entries in a collection that this instance of the HoloNETCollection maps onto. This will be used by the SaveCollection method. This also updates the ZomeUpdateEntriesFunction property. This param is optional.</param>        
        /// <param name="autoCallInitialize">Set this to true if you wish [HoloNETEntryBaseClass](#HoloNETEntryBaseClass) to auto-call the [Initialize](#Initialize) method when a new instance is created. Set this to false if you do not wish it to do this, you may want to do this manually if you want to initialize (will call the [Connect](#connect) method on the HoloNET Client) at a later stage.</param>
        /// <param name="connectedCallBackMode">If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediately and then call the OnConnected event once it has finished connecting.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then call the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA once it has retrieved the DnaHash & AgentPubKey.</param>
        public HoloNETCollection(string zomeName, string zomeLoadCollectionFunction, string zomeAddEntryToCollectionFunction, string zomeRemoveEntryFromCollectionFunction, HoloNETClient holoNETClient, string zomeUpdateEntriesFunction = "", bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            HoloNETClient = holoNETClient;
            //StoreEntryHashInEntry = storeEntryHashInEntry;
            ZomeName = zomeName;
            ZomeLoadCollectionFunction = zomeLoadCollectionFunction;
            ZomeAddEntryToCollectionFunction = zomeAddEntryToCollectionFunction;
            ZomeRemoveEntryFromCollectionFunction = zomeRemoveEntryFromCollectionFunction;
            ZomeUpdateEntriesFunction = zomeUpdateEntriesFunction;

            if (autoCallInitialize)
                InitializeAsync(connectedCallBackMode, retrieveAgentPubKeyAndDnaHashMode, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved);
        }

        /// <summary>
        /// This is a reference to the internal instance of the HoloNET Client (either the one passed in through a constructor or one created internally.)
        /// </summary>
        public HoloNETClient HoloNETClient { get; set; }

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
        /// The name of the zome function to call to add a HoloNETEntry to a collection.
        /// </summary>
        public string ZomeAddEntryToCollectionFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to remove a HoloNETEntry from a collection.
        /// </summary>
        public string ZomeRemoveEntryFromCollectionFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to update a collection of entries (batch operation).
        /// </summary>
        public string ZomeUpdateEntriesFunction { get; set; }


        /// <summary>
        /// This method will Initialize the HoloNETCollection along with the internal HoloNET Client and will raise the OnInitialized event once it has finished initializing. This will also call the Connect and RetrieveAgentPubKeyAndDnaHash methods on the HoloNET client. Once the HoloNET client has successfully connected to the Holochain Conductor, retrieved the AgentPubKey & DnaHash & then raised the OnReadyForZomeCalls event it will raise the OnInitialized event. See also the IsInitializing and the IsInitialized properties.
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
        /// This method will Initialize the HoloNETCollection along with the internal HoloNET Client and will raise the OnInitialized event once it has finished initializing. This will also call the Connect and RetrieveAgentPubKeyAndDnaHash methods on the HoloNET client. Once the HoloNET client has successfully connected to the Holochain Conductor, retrieved the AgentPubKey & DnaHash & then raised the OnReadyForZomeCalls event it will raise the OnInitialized event. See also the IsInitializing and the IsInitialized properties.
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
                HandleError("Unknown error occurred in InitializeAsync method in HoloNETCollection", ex);
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
        /// This method will load the entries for this collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadCollectionFunction` or property `ZomeLoadCollectionFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionLoaded event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to load from. This is optional.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> LoadCollectionAsync(string collectionAnchor = "", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeLoadCollectionFunction, collectionAnchor);
               //ProcessZomeReturnCall(result);
                OnCollectionLoaded?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in LoadCollectionAsync method in HoloNETCollection", ex);
            }
        }

        /// <summary>
        /// This method will load the entries for this collection from the Holochain Conductor. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param `zomeLoadCollectionFunction` or property `ZomeLoadCollectionFunction` and then maps the data returned from the zome call onto your collection of data objects. It will then raise the OnCollectionLoaded event.
        /// </summary>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to load from. This is optional.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs LoadCollection(string collectionAnchor = "", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return LoadCollectionAsync(collectionAnchor).Result;
        }

        /// <summary>
        /// This method will add the HoloNETEntry to the collection. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param 'zomeAddEntryToCollectionFunction' or property 'ZomeAddEntryToCollectionFunction'. It will then raise the OnHoloNETEntryAddedToCollection event.
        /// </summary>
        /// <param name="holoNETEntry">The HoloNETEntry to add to the collection.</param>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to add the entry to. This is optional.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> AddHoloNETEntryToCollectionAndSaveAsync(T holoNETEntry, string collectionAnchor = "", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                this.Add(holoNETEntry);

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntriesFunction, collectionAnchor);
                //ProcessZomeReturnCall(result);
                OnHoloNETEntryAddedToCollection?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in UpdateCollectionAsync method in HoloNETCollection", ex);
            }
        }

        /// <summary>
        /// This method will add the HoloNETEntry to the collection. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param 'zomeAddEntryToCollectionFunction' or property 'ZomeAddEntryToCollectionFunction'. It will then raise the OnHoloNETEntryAddedToCollection event.
        /// </summary>
        /// <param name="holoNETEntry">The HoloNETEntry to add to the collection.</param>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to add the entry to. This is optional.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs AddHoloNETEntryToCollectionAndSave(T holoNETEntry, string collectionAnchor = "", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return AddHoloNETEntryToCollectionAndSaveAsync(holoNETEntry, collectionAnchor).Result;
        }

        /// <summary>
        /// This method will remnove the HoloNETEntry from the collection. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param 'zomeRemoveEntryFromCollectionFunction' or property 'ZomeRemoveEntryFromCollectionFunction'. It will then raise the OnHoloNETEntryRemovedFromCollection event.
        /// </summary>
        /// <param name="holoNETEntry">The HoloNETEntry to add to the collection.</param>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to remove the entry from. This is optional.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> RemoveHoloNETEntryFromCollectionAndSaveAsync(T holoNETEntry, string collectionAnchor = "", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntriesFunction, collectionAnchor);
                //ProcessZomeReturnCall(result);
                OnHoloNETEntryRemovedFromCollection?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in UpdateCollectionAsync method in HoloNETCollection", ex);
            }
        }

        /// <summary>
        /// This method will remnove the HoloNETEntry from the collection. This calls the CallZomeFunction on the HoloNET client passing in the zome function name specified in the constructor param 'zomeRemoveEntryFromCollectionFunction' or property 'ZomeRemoveEntryFromCollectionFunction'. It will then raise the OnHoloNETEntryRemovedFromCollection event.
        /// </summary>
        /// <param name="holoNETEntry">The HoloNETEntry to add to the collection.</param>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to remove the entry from. This is optional.</param>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs RemoveHoloNETEntryFromCollectionAndSave(T holoNETEntry, string collectionAnchor = "", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return RemoveHoloNETEntryFromCollectionAndSaveAsync(holoNETEntry, collectionAnchor).Result;
        }

        /// <summary>
        /// This method will save all changes made to the collection since the last time this method was called and persist the changes to the Holochain Conductor. This includes any entries that were added or removed to the collection via the Add and Remove methods (in-memory only). If entries were added or removed via the AddHoloNETEntryToCollectionAndSave and RemoveHoloNETEntryFromCollectionAndSave methods then these entries will NOT be added/removed again. If the ZomeUpdateEntriesFunction property has been set or was passed in via one of the constructors it will also batch update any entries that have changed since the last time this method was called. If it was not then it will call the Save method on each entry, which will be slower than updating them as a batch depending on the size of the collection. This can invoke multiple events including OnHoloNETEntryAddedToCollection, OnHoloNETEntryRemovedFromCollection & OnHoloNETEntriesUpdated (if any changes were made to the entries themselves)).
        /// </summary>
        /// <param name="saveChangesMadeToEntries">Set this to true if you wish to update any changes made to the entries themselves. This defaults to true. If the ZomeUpdateEntriesFunction property has been set or was passed in via one of the constructors it will also batch update any entries that have changed since the last time this method was called. If it was not then it will call the Save method on each entry, which will be slower than updating them as a batch depending on the size of the collection.</param>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to update. This is optional.</param>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> SaveAllChangesAsync(bool saveChangesMadeToEntries = true, string collectionAnchor = "", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            try
            {
                if (!IsInitialized && !IsInitializing)
                    await InitializeAsync();

                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntriesFunction, collectionAnchor);
                //ProcessZomeReturnCall(result);
                OnHoloNETEntriesUpdated?.Invoke(this, result);
                return result;
            }
            catch (Exception ex)
            {
                return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occurred in UpdateCollectionAsync method in HoloNETCollection", ex);
            }
        }

        /// <summary>
        /// This method will save all changes made to the collection since the last time this method was called and persist the changes to the Holochain Conductor. This includes any entries that were added or removed to the collection via the Add and Remove methods (in-memory only). If entries were added or removed via the AddHoloNETEntryToCollectionAndSave and RemoveHoloNETEntryFromCollectionAndSave methods then these entries will NOT be added/removed again. If the ZomeUpdateEntriesFunction property has been set or was passed in via one of the constructors it will also batch update any entries that have changed since the last time this method was called. If it was not then it will call the Save method on each entry, which will be slower than updating them as a batch depending on the size of the collection. This can invoke multiple events including OnHoloNETEntryAddedToCollection, OnHoloNETEntryRemovedFromCollection & OnHoloNETEntriesUpdated (if any changes were made to the entries themselves)).
        /// </summary>
        /// <param name="saveChangesMadeToEntries">Set this to true if you wish to update any changes made to the entries themselves. This defaults to true. If the ZomeUpdateEntriesFunction property has been set or was passed in via one of the constructors it will also batch update any entries that have changed since the last time this method was called. If it was not then it will call the Save method on each entry, which will be slower than updating them as a batch depending on the size of the collection.</param>
        /// <param name="collectionAnchor">The anchor of the Holochain Collection you wish to update. This is optional.</param>
        public virtual ZomeFunctionCallBackEventArgs SaveAllChanges(bool saveChangesMadeToEntries = true, string collectionAnchor = "", bool useReflectionToMapKeyValuePairResponseOntoEntryDataObject = true)
        {
            return SaveAllChangesAsync(saveChangesMadeToEntries, collectionAnchor).Result;
        }
       

        /// <summary>
        /// Will close this HoloNETCollection and then shutdown its internal HoloNET instance (if one was not passed in) and its current connection to the Holochain Conductor and then shutdown all running Holochain Conductors (if configured to do so) as well as any other tasks to shut HoloNET down cleanly. This method calls the ShutdownHoloNET method internally. Once it has finished shutting down HoloNET it will raise the OnClosed event.
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
                returnValue = HandleError<HoloNETShutdownEventArgs>("Unknown error occurred in CloseAsync method in HoloNETCollection", ex);
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
                returnValue = HandleError<HoloNETShutdownEventArgs>("Unknown error occurred in Close method in HoloNETCollection", ex);
            }

            return returnValue;
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
            HoloNETClient.Logger.Log(message, LogType.Error);

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

        private T HandleError<T>(string message, Exception exception) where T : CallBackBaseEventArgs, new()
        {
            HandleError(message, exception);
            return new T() { IsError = true, Message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "") };
        }
    }
}