using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using NextGenSoftware.CLI.Engine;
using NextGenSoftware.WebSocket;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class HoloNETTestHarness
    {
        private static IHoloNETClientAppAgent _holoNETClientAppAgent = null;
        private static IHoloNETClientAdmin _holoNETClientAdmin = null;
        private static TestToRun _testToRun;
        private static int _numberOfZomeCallResponsesReceived = 0;
        private static Dictionary<int, bool> _loadEntryResponseReceived = new Dictionary<int, bool>();
        private static Dictionary<int, bool> _saveEntryResponseReceived = new Dictionary<int, bool>();
        private static int _requestNumber = 10;
        private static Stopwatch _timer = new Stopwatch(); // creating new instance of the stopwatch
        private static ConsoleColor _testHeadingColour = ConsoleColor.Yellow;
        private static AvatarEntryDataObject _avatarEntryDataObject = null;
        private const string _hcAdminURI = "ws://localhost:65464";
        private const string _hcAppURI = "ws://localhost:8888";
        //private const string _oasisHappPath = @"\hApps\oasis\zomes\workdir\happ";
        private const string _oasisHappId = "oasis";
        private const string _oasisRoleName = "oasis";
        private const string _oasisHappPath = @"E:\Code\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\happ\oasis.happ";
        private const string _numbersHappPath = @"\hApps\happ-build-tutorial-develop\workdir\happ";
        private const string _oasisHappFolder = @"C:\Users\USER\holochain-holochain-0.1.5\happs\oasis\BUILD\happ";
        private const string _oasisDnaPath = @"E:\Code\hc\holochain-holochain-0.1.5\happs\oasis\BUILD\dna\oasis.dna";
        private const string _oasisDnaHash = ""; //TODO Generate Hash here!

        static async Task Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
                

            //  await TestHoloNETClientAsync(TestToRun.AdminGenerateAgentPubKey);
            await TestHoloNETClientAsync(TestToRun.AdminInstallApp);
            //await TestHoloNETClientAsync(TestToRun.AdminAuthorizeSigningCredentials);
            //await TestHoloNETClientAsync(TestToRun.SaveLoadOASISEntryUsingSingleHoloNETAuditEntryBaseClass);
            //await TestHoloNETClientAsync(TestToRun.Signal);
            //await TestHoloNETClientAsync(TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject);
            //await TestHoloNETClientAsync(TestToRun.SaveLoadOASISEntryWithEntryDataObject);
            //await TestHoloNETClientAsync(TestToRun.LoadTestSaveLoadOASISEntry);
            //await TestHoloNETClientAsync(TestToRun.WhoAmI);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"CurrentDomain_UnhandledException: UNKNOWN ERROR OCCURED! ERROR DETAILS: {e.ExceptionObject.ToString()}");
        }

        public static async Task TestHoloNETClientAsync(TestToRun testToRun)
        {
            _timer.Start();
            _testToRun = testToRun;
            Console.WriteLine("NextGenSoftware.Holochain.HoloNET.Client Test Harness v3.0.1");
            Console.WriteLine("");
            bool isAdmin = false;

            HoloNETDNA config = new HoloNETDNA()
            {
                AutoStartHolochainConductor = true,
                AutoShutdownHolochainConductor = true,
                ShutDownALLHolochainConductors = true, //Normally default's to false, but if you want to make sure no holochain processes are left running set this to true.
                ShowHolochainConductorWindow = true, //Defaults to false.
                HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded,
                HolochainConductorToUse = HolochainConductorEnum.HolochainProductionConductor
            };

            switch (testToRun)
            {
                case TestToRun.SaveLoadOASISEntryWithEntryDataObject:
                case TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject:
                case TestToRun.SaveLoadOASISEntryUsingMultipleHoloNETAuditEntryBaseClasses:
                case TestToRun.SaveLoadOASISEntryUsingSingleHoloNETAuditEntryBaseClass:
                case TestToRun.LoadTestSaveLoadOASISEntry:
                case TestToRun.Signal:
                    {
                        config.FullPathToRootHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\OASIS-Holochain-hApp");
                        config.FullPathToCompiledHappFolder = string.Concat(Environment.CurrentDirectory, _oasisHappFolder);
                    }
                    break;

                case TestToRun.WhoAmI:
                case TestToRun.Numbers:
                case TestToRun.LoadTestNumbers:
                    {
                        config.FullPathToRootHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop");
                        config.FullPathToCompiledHappFolder = string.Concat(Environment.CurrentDirectory, _numbersHappPath);
                    }
                    break;
            }

            //No need to create this when the test is SaveLoadOASISEntryUsingSingleHoloNETBaseClass because it will create its own instance of HoloNETClient internally (in the base class).
            if (_testToRun != TestToRun.SaveLoadOASISEntryUsingSingleHoloNETAuditEntryBaseClass)
            {
                switch (_testToRun)
                {
                    case TestToRun.AdminAuthorizeSigningCredentials:
                    case TestToRun.AdminGenerateAgentPubKey:
                    case TestToRun.AdminInstallApp:
                    case TestToRun.AdminEnableApp:
                    case TestToRun.AdminDisableApp:
                    case TestToRun.AdminAttachAppInterface:
                    case TestToRun.AdminRegisterDna:
                    case TestToRun.AdminListApps:
                    case TestToRun.AdminListDnas:
                    case TestToRun.AdminListCellIds:
                    case TestToRun.AdminListInterfaces:
                    case TestToRun.AdminDumpFullState:
                    case TestToRun.AdminDumpState:
                    case TestToRun.AdminGetDnaDefinition:
                    case TestToRun.AdminUpdateCoordinators:
                    case TestToRun.AdminGetAgentInfo:
                    case TestToRun.AdminAddAgentInfo:
                    case TestToRun.AdminDeleteCloneCell:
                    case TestToRun.AdminGetStorageInfo:
                    case TestToRun.AdminDumpNetworkStats:
                        _holoNETClientAdmin = new HoloNETClientAdmin();
                        isAdmin = true;
                        break;

                    default:
                        _holoNETClientAdmin = new HoloNETClientAdmin();
                        break;
                }

                //We would normally just set the Config property but in this case we need to share the Config object with multiple HoloNET instances (such as in the SaveLoadOASISEntryUsingSingleHoloNETBaseClass test) 
                _holoNETClientAdmin.HoloNETDNA = config;

                _holoNETClientAdmin.OnConnected += _holoNETClientAdmin_OnConnected;
                _holoNETClientAdmin.OnDataReceived += _holoNETClientAdmin_OnDataReceived;
                _holoNETClientAdmin.OnDataSent += _holoNETClientAdmin_OnDataSent;
                _holoNETClientAdmin.OnError += _holoNETClientAdmin_OnError;
                _holoNETClientAdmin.OnDisconnected += _holoNETClientAdmin_OnDisconnected;
                _holoNETClientAdmin.OnHolochainConductorStarting += _holoNETClientAdmin_OnHolochainConductorStarting;
                _holoNETClientAdmin.OnHolochainConductorStarted += _holoNETClientAdmin_OnHolochainConductorStarted;
                _holoNETClientAdmin.OnHolochainConductorsShutdownComplete += _holoNETClientAdmin_OnHolochainConductorsShutdownComplete;
                _holoNETClientAdmin.OnHoloNETShutdownComplete += _holoNETClientAdmin_OnHoloNETShutdownComplete;
                _holoNETClientAdmin.OnAgentPubKeyGeneratedCallBack += _holoNETClientAdmin_OnAgentPubKeyGeneratedCallBack;
                _holoNETClientAdmin.OnAppInstalledCallBack += _holoNETClientAdmin_OnAppInstalledCallBack;
                _holoNETClientAdmin.OnAppUninstalledCallBack += _holoNETClientAdmin_OnAppUninstalledCallBack;
                _holoNETClientAdmin.OnAppEnabledCallBack += _holoNETClientAdmin_OnAppEnabledCallBack;
                _holoNETClientAdmin.OnAppDisabledCallBack += _holoNETClientAdmin_OnAppDisabledCallBack;
                _holoNETClientAdmin.OnAppInterfaceAttachedCallBack += _holoNETClientAdmin_OnAppInterfaceAttachedCallBack;
                _holoNETClientAdmin.OnZomeCallCapabilityGrantedCallBack += _holoNETClientAdmin_OnZomeCallCapabilityGranted;
                _holoNETClientAdmin.OnDnaRegisteredCallBack += _holoNETClientAdmin_OnDnaRegisteredCallBack;
                _holoNETClientAdmin.OnAppsListedCallBack += _holoNETClientAdmin_OnListAppsCallBack;
                _holoNETClientAdmin.OnDnasListedCallBack += _holoNETClientAdmin_OnDnasListedCallBack;
                _holoNETClientAdmin.OnCellIdsListedCallBack += _holoNETClientAdmin_OnCellIdsListedCallBack;
                _holoNETClientAdmin.OnAppInterfacesListedCallBack += _holoNETClientAdmin_OnAppInterfacesListedCallBack;
                _holoNETClientAdmin.OnAdminInterfacesAddedCallBack += _holoNETClientAdmin_OnInterfacesAddedCallBack;
                _holoNETClientAdmin.OnDnaDefinitionReturnedCallBack += _holoNETClientAdmin_OnDnaDefinitionReturnedCallBack;
                _holoNETClientAdmin.OnCoordinatorsUpdatedCallBack += _holoNETClientAdmin_OnCoordinatorsUpdatedCallBack;
                _holoNETClientAdmin.OnAgentInfoReturnedCallBack += _holoNETClientAdmin_OnAgentInfoReturnedCallBack;
                _holoNETClientAdmin.OnAgentInfoAddedCallBack += _holoNETClientAdmin_OnAgentInfoAddedCallBack;
                _holoNETClientAdmin.OnCloneCellDeletedCallBack += _holoNETClientAdmin_OnCloneCellDeletedCallBack;
                _holoNETClientAdmin.OnStorageInfoReturnedCallBack += _holoNETClientAdmin_OnStorageInfoReturnedCallBack;
                _holoNETClientAdmin.OnNetworkStatsDumpedCallBack += _holoNETClientAdmin_OnNetworkStatsDumpedCallBack;
                _holoNETClientAdmin.OnNetworkMetricsDumpedCallBack += _holoNETClientAdmin_OnNetworkMetricsDumpedCallBack;
                _holoNETClientAdmin.OnFullStateDumpedCallBack += _holoNETClientAdmin_OnFullStateDumpedCallBack;
                _holoNETClientAdmin.OnStateDumpedCallBack += _holoNETClientAdmin_OnStateDumpedCallBack;
                _holoNETClientAdmin.OnInstallEnableSignAndAttachHappCallBack += _holoNETClientAdmin_OnInstallEnableSignAndAttachHappCallBack;
                _holoNETClientAdmin.OnInstallEnableSignAttachAndConnectToHappCallBack += _holoNETClientAdmin_OnInstallEnableSignAttachAndConnectToHappCallBack;
                _holoNETClientAdmin.OnRecordsGraftedCallBack += _holoNETClientAdmin_OnRecordsGraftedCallBack;

                ////Use this if you wish to manually pass in the AgentPubKey & DnaHash(otherwise it will be automatically queried from the conductor or sandbox).
                //_holoNETClientAdmin.Config.AgentPubKey = "YOUR KEY";
                //_holoNETClientAdmin.Config.DnaHash = "YOUR HASH";
                //await _holoNETClientAdmin.Connect(false, false);

                HoloNETConnectedEventArgs connectedResult = await _holoNETClientAdmin.ConnectAsync();

                if (connectedResult != null && !connectedResult.IsError && connectedResult.IsConnected)
                {
                    if (!isAdmin)
                    {
                        InstallEnableSignAttachAndConnectToHappEventArgs installResult = await _holoNETClientAdmin.InstallEnableSignAttachAndConnectToHappAsync(_oasisHappId, _oasisHappPath, _oasisRoleName);

                        if (installResult != null && !installResult.IsError && installResult.IsAppConnected)
                        {
                            _holoNETClientAppAgent = installResult.HoloNETClientAppAgent;
                            _holoNETClientAppAgent.OnConnected += _holoNETClientAppAgent_OnConnected;
                            _holoNETClientAppAgent.OnDataSent += _holoNETClientAppAgent_OnDataSent;
                            _holoNETClientAppAgent.OnDataReceived += HoloNETClient_OnDataReceived;
                            _holoNETClientAppAgent.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;
                            _holoNETClientAppAgent.OnAppInfoCallBack += HoloNETClient_OnAppInfoCallBack;
                            _holoNETClientAppAgent.OnReadyForZomeCalls += _holoNETClient_OnReadyForZomeCalls;
                            _holoNETClientAppAgent.OnSignalCallBack += HoloNETClient_OnSignalCallBack;
                            _holoNETClientAppAgent.OnDisconnected += HoloNETClient_OnDisconnected;
                            _holoNETClientAppAgent.OnError += HoloNETClient_OnError;
                        }
                    }
                } 
            }

            switch (testToRun)
            {
                case TestToRun.SaveLoadOASISEntryUsingSingleHoloNETAuditEntryBaseClass:
                    {
                        CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY SAVE/CREATE TEST ***", _testHeadingColour);

                        Avatar avatar = new Avatar(config)
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "David",
                            LastName = "Ellams",
                            DOB = Convert.ToDateTime("11/07/1980"),
                            Email = "davidellams@hotmail.com",
                            IsActive = true
                        };

                        avatar.OnLoaded += Avatar_OnLoaded;
                        avatar.OnSaved += Avatar_OnSaved;
                        avatar.OnDeleted += Avatar_OnDeleted;
                        avatar.OnError += Avatar_OnError;
                        avatar.OnClosed += Avatar_OnClosed;
                        avatar.OnInitialized += Avatar_OnInitialized;

                        //By default the constructor's will auto call the InitializeAsync method but if you wish to call it manually so you can await it and ensure it is connected and AgentPubKey & DnaHash have been retrieved you can do so as below:
                        //We need to make sure we call this before anything else to make sure HoloNET is connected to the Conductor and so the AgentPubKey & DnaHash are set.
                        //CLIEngine.ShowWorkingMessage("Initializing Avatar...");
                        //await avatar.InitializeAsync();

                        //Alternatively if you use the defaults and allow the constructor to auto-call InitializeAsync() then you can await the WaitTillHoloNETInitializedAsync method to make sure it is connected and the AgentPubKey & DnaHash have been retrieved.
                        //Note that all the CRUD methods LoadAsync, SaveAsync & DeleteAsync will automatically await internally until HoloNET is ready. 
                        //So you only need to use the WaitTillHoloNETInitializedAsync method if you have not used the InitializeAsync method above and if you wish to access the HoloNETClient.Config.AgentPubKey or HoloNETClient.Config.DnaHash properties BEFORE calling any of the CRUD methods.
                        await avatar.WaitTillHoloNETInitializedAsync();
                        CLIEngine.ShowMessage($"AgentPubKey:{avatar.HoloNETClient.HoloNETDNA.AgentPubKey}");
                        CLIEngine.ShowMessage($"DnaHash:{avatar.HoloNETClient.HoloNETDNA.DnaHash}");


                        CLIEngine.ShowWorkingMessage("Saving Avatar...");
                        ZomeFunctionCallBackEventArgs result =  await avatar.SaveAsync(new Dictionary<string, string>() 
                        { 
                            { "custom field 1", "custom value 1" }, 
                            { "custom field 2", "custom value 2" } 
                        });

                        //List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>() 
                        //{ 
                        //    new KeyValuePair<string, string>("bob", "harper"),
                        //    new KeyValuePair<string, string>("david", "ellams")
                        //};

                        //dynamic params = new dynamic() { "david" = "ellams" };

                        //await avatar.LoadAsync(true, new { david = ellams, bob = harper });

                        if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                        {
                            CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.SAVE/CREATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                            Console.WriteLine("");
                            ShowAvatarDetails(avatar);

                            Avatar avatar2 = new Avatar(config)
                            {
                                Id = Guid.NewGuid(),
                                FirstName = "David",
                                LastName = "Ellams",
                                DOB = Convert.ToDateTime("11/07/1980"),
                                Email = "davidellams@hotmail.com",
                                IsActive = true
                            };

                            await avatar2.SaveAsync();

                            avatar = ResetAvatar(avatar);
                            CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY LOAD TEST ***", _testHeadingColour);
                            

                            //string entryHash = avatar.EntryHash;
                            //avatar.Dispose(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            //await avatar.CloseAsync();

                            //avatar = new Avatar(config); //Normally you wouldn't need to create a new instance but we want to verify the data is fully loaded so best to use a new object.
                            //avatar.EntryHash = entryHash;

                            CLIEngine.ShowWorkingMessage("Loading Avatar...");
                            result = await avatar.LoadAsync();

                            if (!result.IsError)
                            {
                                CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                ShowAvatarDetails(avatar);

                                // Now test the update method.
                                CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY SAVE/UPDATE TEST ***", _testHeadingColour);
                                avatar.FirstName = "James";
                                CLIEngine.ShowWorkingMessage("Saving Avatar...");
                                result = await avatar.SaveAsync();

                                if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                {
                                    CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                    ShowAvatarDetails(avatar);

                                    //Now check the update was saved correctly by re-loading it...
                                    CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY LOAD TEST 2 (VERIFYING UPDATE WAS SUCCESSFUL) ***", _testHeadingColour);
                                    CLIEngine.ShowWorkingMessage("Loading Avatar...");

                                    avatar = ResetAvatar(avatar);
                                    result = await avatar.LoadAsync();

                                    if (!result.IsError)
                                    {
                                        CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        ShowAvatarDetails(avatar);

                                        CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY DELETE TEST ***", _testHeadingColour);
                                        CLIEngine.ShowWorkingMessage("Deleting Avatar...");
                                        result = await avatar.DeleteAsync();

                                        if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                            CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.DELETE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        else
                                            CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.DELETE RESPONSE: AN ERROR OCCURED: ", result.Message));
                                    }
                                    else
                                        CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.LOAD RESPONSE: AN ERROR OCCURED: ", result.Message));
                                }
                                else
                                    CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: AN ERROR OCCURED: ", result.Message));
                            
                            
                            }
                            else
                                CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.LOAD RESPONSE: AN ERROR OCCURED: ", result.Message));
                        }
                        else
                            CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.SAVE/CREATE RESPONSE: AN ERROR OCCURED: ", result.Message));

                        if (avatar != null)
                        {
                            await avatar.CloseAsync(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            //avatar.Dispose(); //There originally use to be a Dispose method but we need it to be async so it waits for the HoloNET to disconnect etc. The AsyncDispose pattern was also not a good fit for what we needed and is overly complicated. We may look at implementing this in a later version...

                            avatar.OnLoaded -= Avatar_OnLoaded;
                            avatar.OnSaved -= Avatar_OnSaved;
                            avatar.OnDeleted -= Avatar_OnDeleted;
                            avatar.OnError -= Avatar_OnError;
                            avatar.OnClosed -= Avatar_OnClosed;
                            avatar.OnInitialized -= Avatar_OnInitialized;
                        }
                    }
                    break;

                case TestToRun.SaveLoadOASISEntryUsingMultipleHoloNETAuditEntryBaseClasses:
                    {
                        //In this example we are passing in an existing instance of the HoloNET client so it along with the connection to the holochain condutor is shared amongst the objects/classes that inherit from HoloNETEntryBaseClass.
                        AvatarMultiple avatar = new AvatarMultiple(_holoNETClientAppAgent)
                        {
                            Id = Guid.NewGuid(),
                            FirstName = "David",
                            LastName = "Ellams",
                            DOB = Convert.ToDateTime("11/07/1980"),
                            Email = "davidellams@hotmail.com",
                            IsActive = true
                        };

                        avatar.OnLoaded += Avatar_OnLoaded;
                        avatar.OnSaved += Avatar_OnSaved;
                        avatar.OnDeleted += Avatar_OnDeleted;
                        avatar.OnError += Avatar_OnError;
                        avatar.OnClosed += Avatar_OnClosed;
                        avatar.OnInitialized += Avatar_OnInitialized;

                        CLIEngine.ShowWorkingMessage("Saving Avatar...");
                        ZomeFunctionCallBackEventArgs result = await avatar.SaveAsync();

                        if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                        {
                            Console.WriteLine(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONETBASECLASS.SAVE/CREATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                            Console.WriteLine("");
                            ShowAvatarDetails(avatar);
                            string entryHash = avatar.EntryHash;

                            //There is no need to call Dispose on an object sharing a HoloNETClient instance (passed in) because it will only dispose of (disconnect etc) a HoloNETClient if one was NOT passed in because it was internally created and used (like the single use above).
                            //avatar.Dispose(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            avatar = new AvatarMultiple(_holoNETClientAppAgent); //Normally you wouldn't need to create a new instance but we want to verify the data is fully loaded so best to use a new object.
                            avatar.EntryHash = entryHash;
                            CLIEngine.ShowWorkingMessage("Loading Avatar...");
                            result = await avatar.LoadAsync();

                            if (!result.IsError)
                            {
                                Console.WriteLine(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONETBASECLASS.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                Console.WriteLine("");
                                ShowAvatarDetails(avatar);

                                // Now test the update method.
                                avatar.FirstName = "James";
                                CLIEngine.ShowWorkingMessage("Saving Avatar...");
                                result = await avatar.SaveAsync();

                                if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                {
                                    CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                    ShowAvatarDetails(avatar);

                                    //Now check the update was saved correctly by re-loading it...
                                    CLIEngine.ShowMessage("*** AVATAR MULTIPLE HOLONET BASE ENTRY LOAD TEST 2 (VERIFYING UPDATE WAS SUCCESSFUL) ***", _testHeadingColour);
                                    CLIEngine.ShowWorkingMessage("Loading Avatar...");

                                    avatar = ResetAvatar(avatar);
                                    result = await avatar.LoadAsync();

                                    if (!result.IsError)
                                    {
                                        CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONET BASE ENTRY.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        ShowAvatarDetails(avatar);

                                        CLIEngine.ShowMessage("*** AVATAR MULTIPLE HOLONET BASE ENTRY DELETE TEST ***", _testHeadingColour);
                                        CLIEngine.ShowWorkingMessage("Deleting Avatar...");
                                        result = await avatar.DeleteAsync();

                                        if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                            CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONET BASE ENTRY.DELETE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        else
                                            CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONET BASE ENTRY.DELETE RESPONSE: AN ERROR OCCURED: ", result.Message));
                                    }
                                    else
                                        CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONET BASE ENTRY.LOAD RESPONSE: AN ERROR OCCURED: ", result.Message));
                                }
                                else
                                    CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: AN ERROR OCCURED: ", result.Message));
                            }
                            else
                            {
                                Console.WriteLine(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONETBASECLASS.LOAD RESPONSE: AN ERROR OCCURED: ", result.Message));
                                Console.WriteLine("");
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONETBASECLASS.SAVE/CREATE RESPONSE: AN ERROR OCCURED: ", result.Message));
                            Console.WriteLine("");
                        }

                        Holon holon = new Holon(_holoNETClientAppAgent)
                        {
                            Id = Guid.NewGuid(),
                            ParentId = Guid.NewGuid(),
                            Name = "Test holon",
                            Description = "Test description",
                            Type = "Park",
                            IsActive = true
                        };

                        holon.OnLoaded += Holon_OnLoaded;
                        holon.OnSaved += Holon_OnSaved;
                        holon.OnDeleted += Holon_OnDeleted;
                        holon.OnError += Holon_OnError;
                        holon.OnClosed += Holon_OnClosed;
                        holon.OnInitialized += Holon_OnInitialized;

                        CLIEngine.ShowWorkingMessage("Saving Holon...");
                        result = await holon.SaveAsync();

                        if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                        {
                            Console.WriteLine(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONETBASECLASS.SAVE/CREATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                            Console.WriteLine("");
                            ShowHolonDetails(holon);
                            string entryHash = holon.EntryHash;

                            //There is no need to call Dispose on an object sharing a HoloNETClient instance (passed in) because it will only dispose of (disconnect etc) a HoloNETClient if one was NOT passed in because it was internally created and used (like the single use above).
                            //holon.Dispose(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            holon = new Holon(_holoNETClientAppAgent); //Normally you wouldn't need to create a new instance but we want to verify the data is fully loaded so best to use a new object.
                            holon.EntryHash = entryHash;
                            CLIEngine.ShowWorkingMessage("Loading Holon...");
                            result = await holon.LoadAsync();

                            if (!result.IsError)
                            {
                                Console.WriteLine(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONETBASECLASS.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                Console.WriteLine("");
                                ShowHolonDetails(holon);

                                // Now test the update method.
                                holon.Name = "another holon";
                                CLIEngine.ShowWorkingMessage("Saving Holon...");
                                result = await holon.SaveAsync();

                                if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                {
                                    CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                    ShowAvatarDetails(avatar);

                                    //Now check the update was saved correctly by re-loading it...
                                    CLIEngine.ShowMessage("*** HOLON MULTIPLEHOLONET BASE ENTRY LOAD TEST 2 (VERIFYING UPDATE WAS SUCCESSFUL) ***", _testHeadingColour);
                                    CLIEngine.ShowWorkingMessage("Loading Holon...");

                                    holon = ResetHolon(holon);
                                    result = await holon.LoadAsync();

                                    if (!result.IsError)
                                    {
                                        CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONET BASE ENTRY.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        ShowAvatarDetails(avatar);

                                        CLIEngine.ShowMessage("*** HOLON MULTIPLE HOLONET BASE ENTRY DELETE TEST ***", _testHeadingColour);
                                        CLIEngine.ShowWorkingMessage("Deleting Holon...");
                                        result = await avatar.DeleteAsync();

                                        if (!result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                            CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONET BASE ENTRY.DELETE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        else
                                            CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: HOLON MULTIPLEHOLONET BASE ENTRY.DELETE RESPONSE: AN ERROR OCCURED: ", result.Message));
                                    }
                                    else
                                        CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONET BASE ENTRY.LOAD RESPONSE: AN ERROR OCCURED: ", result.Message));
                                }
                                else
                                    CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: AN ERROR OCCURED: ", result.Message));
                            }
                            else
                            {
                                Console.WriteLine(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONETBASECLASS.LOAD RESPONSE: AN ERROR OCCURED: ", result.Message));
                                Console.WriteLine("");
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONETBASECLASS.SAVE/CREATE RESPONSE: AN ERROR OCCURED: ", result.Message));
                            Console.WriteLine("");
                        }

                        // There is no need to call Close on an object sharing a HoloNETClient instance (passed in) because it will only dispose of (disconnect etc) a HoloNETClient if one was NOT passed in because it was internally created and used (like the single use above).
                        // UPDATE: There is now a need to still call Close so it unsubscribes the OnError event handler created for the shared HoloNETClient instance.
                        if (avatar != null)
                        {
                            await avatar.CloseAsync(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            // avatar.Dispose(); //There originally use to be a Dispose method but we need it to be async so it waits for the HoloNET to disconnect etc. The AsyncDispose pattern was also not a good fit for what we needed and is overly complicated. We may look at implementing this in a later version...

                            avatar.OnLoaded -= Avatar_OnLoaded;
                            avatar.OnSaved -= Avatar_OnSaved;
                            avatar.OnDeleted -= Avatar_OnDeleted;
                            avatar.OnError -= Avatar_OnError;
                            avatar.OnClosed -= Avatar_OnClosed;
                            avatar.OnInitialized -= Avatar_OnInitialized;
                        }

                        if (holon != null)
                        {
                            await holon.CloseAsync(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            // holon.Dispose(); 

                            holon.OnLoaded -= Holon_OnLoaded;
                            holon.OnSaved -= Holon_OnSaved;
                            holon.OnDeleted -= Holon_OnDeleted;
                            holon.OnError -= Holon_OnError;
                            holon.OnClosed -= Holon_OnClosed;
                            holon.OnInitialized -= Holon_OnInitialized;
                        }
                    }
                    break;
            }
        }

        private async static void _holoNETClientAdmin_OnConnected(object sender, ConnectedEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnConnected, EndPoint: {e.EndPoint}");

            Console.WriteLine(string.Concat("TEST HARNESS: CONNECTED CALLBACK: Connected to ", e.EndPoint));
            Console.WriteLine("");

            switch (_testToRun)
            {
                case TestToRun.AdminGenerateAgentPubKey:
                    {
                        Console.WriteLine("Calling AdminGenerateAgentPubKeyAsync function on Admin API...\n");
                        await _holoNETClientAdmin.GenerateAgentPubKeyAsync();
                    }
                    break;

                case TestToRun.AdminInstallApp:
                    {
                        Console.WriteLine("Calling AdminInstallAppAsync function on Admin API...\n");
                        await _holoNETClientAdmin.InstallAppAsync("oasis-app4", _oasisHappPath);
                    }
                    break;

                case TestToRun.AdminEnableApp:
                    {
                        Console.WriteLine("Calling AdminEnableAppAsync function on Admin API...\n");
                        await _holoNETClientAdmin.EnableAppAsync("test-app");
                    }
                    break;

                case TestToRun.AdminDisableApp:
                    {
                        Console.WriteLine("Calling AdminDisableAppAsync function on Admin API...\n");
                        await _holoNETClientAdmin.DisableAppAsync("test-app");
                    }
                    break;

                case TestToRun.AdminAuthorizeSigningCredentials:
                    {
                        Console.WriteLine("Calling AdminAuthorizeSigningCredentialsAsync function on Admin API...\n");
                        await _holoNETClientAdmin.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(_holoNETClientAppAgent.HoloNETDNA.CellId, CapGrantAccessType.Assigned, GrantedFunctionsType.Listed, new List<(string, string)>()
                        {
                            ("zome1", "function1"),
                            ("zome2", "function2")
                        });
                    }
                    break;

                case TestToRun.AdminAttachAppInterface:
                    {
                        Console.WriteLine("Calling AdminAttachAppInterfaceAsync function on Admin API...\n");
                        await _holoNETClientAdmin.AttachAppInterfaceAsync(777);
                    }
                    break;

                case TestToRun.AdminRegisterDna:
                    {
                        Console.WriteLine("Calling AdminRegisterDna function on Admin API...\n");
                        await _holoNETClientAdmin.RegisterDnaAsync(_oasisDnaPath);

                        await _holoNETClientAdmin.RegisterDnaAsync(new DnaBundle()
                        {
                            manifest = new DnaManifest()
                            {
                                manifest_version = "1",
                                name = "oasis-test",
                                network_seed = "1",
                                properties = "test props",
                                zomes = new ZomeManifest[]
                                {
                                    new ZomeManifest()
                                    {
                                         //bundled = "", //Can ONLY be one of bundled, path or url.
                                         path = _oasisDnaPath, //Can ONLY be one of bundled, path or url.
                                         //url = "", //Can ONLY be one of bundled, path or url.
                                         name = "OASIS Test",
                                         hash = "",
                                         dependencies = new ZomeDependency[]{ new ZomeDependency(){ name = "oasis"} }
                                    }
                                }

                            },
                            resources = new Dictionary<string, byte[]>()
                        });
                    }
                    break;

                case TestToRun.AdminListApps:
                    {
                        Console.WriteLine("Calling AdminListApps function on Admin API...\n");
                        await _holoNETClientAdmin.ListAppsAsync(AppStatusFilter.Running);
                    }
                    break;

                case TestToRun.AdminListDnas:
                    {
                        Console.WriteLine("Calling AdminListDnas function on Admin API...\n");
                        await _holoNETClientAdmin.ListDnasAsync();
                    }
                    break;

                case TestToRun.AdminListCellIds:
                    {
                        Console.WriteLine("Calling AdminListCellIds function on Admin API...\n");
                        await _holoNETClientAdmin.ListCellIdsAsync();
                    }
                    break;

                case TestToRun.AdminListInterfaces:
                    {
                        Console.WriteLine("Calling AdminListInterfaces function on Admin API...\n");
                        await _holoNETClientAdmin.ListInterfacesAsync();
                    }
                    break;

                case TestToRun.AdminDumpFullState:
                    {
                        Console.WriteLine("Calling AdminDumpFullState function on Admin API...\n");
                        await _holoNETClientAdmin.DumpFullStateAsync();
                    }
                    break;

                case TestToRun.AdminDumpState:
                    {
                        Console.WriteLine("Calling AdminDumpState function on Admin API...\n");
                        await _holoNETClientAdmin.DumpStateAsync();
                    }
                    break;

                case TestToRun.AdminGetDnaDefinition:
                    {
                        Console.WriteLine("Calling AdminGetDnaDefinition function on Admin API...\n");
                        await _holoNETClientAdmin.GetDnaDefinitionAsync(_oasisDnaHash);
                    }
                    break;

                case TestToRun.AdminUpdateCoordinators:
                    {
                        Console.WriteLine("Calling AdminUpdateCoordinators function on Admin API...\n");
                        await _holoNETClientAdmin.UpdateCoordinatorsAsync(_oasisDnaHash, _oasisDnaPath);
                    }
                    break;

                case TestToRun.AdminGetAgentInfo:
                    {
                        Console.WriteLine("Calling AdminGetAgentInfo function on Admin API...\n");
                        await _holoNETClientAdmin.GetAgentInfoAsync();
                    }
                    break;

                case TestToRun.AdminAddAgentInfo:
                    {
                        Console.WriteLine("Calling AdminAddAgentInfo function on Admin API...\n");
                        await _holoNETClientAdmin.AddAgentInfoAsync(new AgentInfo[]
                        {
                            new AgentInfo
                            {
                                agent = new byte[] { },
                                signature = new byte[] { },
                                agent_info = new byte[] { },
                            }
                            //new AgentInfo
                            //{
                            //    agent = new KitsuneAgent(new byte[] { }),
                            //    space = new KitsuneSpace(new byte[] { }),
                            //    signature = new KitsuneSignature(new byte[] { }),
                            //    encoded_bytes = new byte[] {},
                            //    expires_at_ms = 0,
                            //    signed_at_ms= 0,
                            //    url_list = new string[] { "http://holochain.org", "http://holo-net.com"}
                            //}
                        });
                    }
                    break;

                case TestToRun.AdminDeleteCloneCell:
                    {
                        Console.WriteLine("Calling AdminDeleteCloneCell function on Admin API...\n");
                        await _holoNETClientAdmin.DeleteCloneCellAsync("oasis");
                    }
                    break;

                case TestToRun.AdminGetStorageInfo:
                    {
                        Console.WriteLine("Calling AdminGetStorageInfo function on Admin API...\n");
                        await _holoNETClientAdmin.GetStorageInfoAsync();
                    }
                    break;

                case TestToRun.AdminDumpNetworkStats:
                    {
                        Console.WriteLine("Calling AdminDumpNetworkStats function on Admin API...\n");
                        await _holoNETClientAdmin.DumpNetworkStatsAsync();
                    }
                    break;
            }
        }

        private static void _holoNETClientAdmin_OnRecordsGraftedCallBack(object sender, RecordsGraftedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnRecordsGraftedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnNetworkMetricsDumpedCallBack(object sender, NetworkMetricsDumpedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnNetworkMetricsDumpedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnInstallEnableSignAttachAndConnectToHappCallBack(object sender, InstallEnableSignAttachAndConnectToHappEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnInstallEnableSignAttachAndConnectToHappCallBack, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, InstalledAppId: {e.AppInstalledResult.InstalledAppId}, AttachedOnPort: {e.AttachedOnPort}, AppStatusReason: {e.AppStatusReason}");
        }

        private static void _holoNETClientAdmin_OnInstallEnableSignAndAttachHappCallBack(object sender, InstallEnableSignAndAttachHappEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnInstallEnableSignAndAttachHappCallBack, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, InstalledAppId: {e.AppInstalledResult.InstalledAppId}, AttachedOnPort: {e.AttachedOnPort}, AppStatusReason: {e.AppStatusReason}");
        }

        private static void _holoNETClientAdmin_OnHolochainConductorStarting(object sender, HolochainConductorStartingEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnHolochainConductorStarting");
        }

        private static void _holoNETClientAdmin_OnHolochainConductorStarted(object sender, HolochainConductorStartedEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnHolochainConductorStarted");
        }

        private static void _holoNETClientAdmin_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnDisconnected, EndPoint: {e.EndPoint}, Reason: {e.Reason}");
        }

        private static void _holoNETClientAdmin_OnError(object sender, HoloNETErrorEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnError, EndPoint: {e.EndPoint}, Reason: {e.Reason}, ErrorDetails: {e.ErrorDetails}");
        }

        private static void _holoNETClientAdmin_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnDataSent, EndPoint: {e.EndPoint}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnDataReceived, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnInterfacesAddedCallBack(object sender, AdminInterfacesAddedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnAdminInterfacesAddedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnAppUninstalledCallBack(object sender, AppUninstalledCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnAppUninstalledCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnNetworkStatsDumpedCallBack(object sender, NetworkStatsDumpedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnNetworkStatsDumpedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnStorageInfoReturnedCallBack(object sender, StorageInfoReturnedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnStorageInfoReturnedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnCloneCellDeletedCallBack(object sender, CloneCellDeletedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnCloneCellDeletedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnAgentInfoAddedCallBack(object sender, AgentInfoAddedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnAgentInfoAddedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnAgentInfoReturnedCallBack(object sender, AgentInfoReturnedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnAgentInfoReturnedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnCoordinatorsUpdatedCallBack(object sender, CoordinatorsUpdatedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnCoordinatorsUpdatedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnDnaDefinitionReturnedCallBack(object sender, DnaDefinitionReturnedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnDnaDefinitionReturnedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnStateDumpedCallBack(object sender, StateDumpedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnStateDumpedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnFullStateDumpedCallBack(object sender, FullStateDumpedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnFullStateDumpedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnAppInterfacesListedCallBack(object sender, AppInterfacesListedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnAppInterfacesListedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnCellIdsListedCallBack(object sender, CellIdsListedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnCellIdsListedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnDnasListedCallBack(object sender, DnasListedCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAdmin_OnDnasListedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void _holoNETClientAdmin_OnDnaRegisteredCallBack(object sender, DnaRegisteredCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: _holoNETClientAdmin_OnDnaRegisteredCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}";
            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnZomeCallCapabilityGranted(object sender, ZomeCallCapabilityGrantedCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: _holoNETClientAdmin_OnZomeCallCapabilityGranted, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}";
            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnListAppsCallBack(object sender, AppsListedCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: _holoNETClientAdmin_OnListAppsCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}";
            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnAppInterfaceAttachedCallBack(object sender, AppInterfaceAttachedCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: _holoNETClientAdmin_OnAppInterfaceAttachedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}";
            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnAppDisabledCallBack(object sender, AppDisabledCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: _holoNETClientAdmin_OnAppDisabledCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}";
            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnAppEnabledCallBack(object sender, AppEnabledCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: _holoNETClientAdmin_OnAppEnabledCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}";
            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnAppInstalledCallBack(object sender, AppInstalledCallBackEventArgs e)
        {
            //string msg = $"TEST HARNESS: _holoNETClient_OnAdminAppInstalledCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data: {e.RawBinaryData}, Raw Binary Data As String: {e.RawBinaryDataAsString}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}";
            string msg = $"TEST HARNESS: _holoNETClientAdmin_OnAppInstalledCallBack, EndPoint: {e.EndPoint}, Id: {e.Id}, Raw Binary Data Decoded: {e.RawBinaryDataDecoded}, IsError: {e.IsError}, Message: {e.Message}";
            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnAgentPubKeyGeneratedCallBack(object sender, AgentPubKeyGeneratedCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: _holoNETClientAdmin_OnAgentPubKeyGeneratedCallBack, EndPoint: {e.EndPoint}, Id: {e.Id},  AgentPubKey: {e.AgentPubKey}, IsError: {e.IsError}, Message: {e.Message}";
            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnHoloNETShutdownComplete(object sender, HoloNETShutdownEventArgs e)
        {
            string msg = $"TEST HARNESS: OnHoloNETShutdownComplete, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}";
            
            if (e.HolochainConductorsShutdownEventArgs != null)
                msg = string.Concat(msg, $", NumberOfHcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: { e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: { e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");

            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClientAdmin_OnHolochainConductorsShutdownComplete(object sender, HolochainConductorsShutdownEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: OnHolochainConductorsShutdownComplete, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}, NumberOfHcExeInstancesShutdown: { e.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: {e.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: {e.NumberOfRustcExeInstancesShutdown}.");
        }



        private static void _holoNETClientAppAgent_OnConnected(object sender, ConnectedEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: _holoNETClientAppAgent_OnConnected, EndPoint: {e.EndPoint}");
        }

        private static void _holoNETClientAppAgent_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        {
            CLIEngine.ShowMessage(string.Concat("\nTEST HARNESS: DATA SENT EVENT HANDLER: EndPoint: ", e.EndPoint, ", Raw JSON Data: ", e.RawJSONData, ", Raw Binary Data: ", e.RawBinaryData, ", IsError: ", e.IsError, ", Message:", e.Message));
        }

        private static void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (!e.IsConductorDebugInfo)
                CLIEngine.ShowMessage(string.Concat("\nTEST HARNESS: DATA RECEIVED EVENT HANDLER: EndPoint: ", e.EndPoint, ", Raw JSON Data: ", e.RawJSONData, ", Raw Binary Data: ", e.RawBinaryData, ", IsError: ", e.IsError, ", Message:", e.Message));
        }

        private async static void _holoNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: READY FOR ZOME CALLS EVENT HANDLER: EndPoint: ", e.EndPoint, ", AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash, ", IsError: ", e.IsError, ", Message: ", e.Message));
            Console.WriteLine("");

            switch (_testToRun)
            {
                case TestToRun.WhoAmI:
                    {
                        Console.WriteLine("Calling whoami function on WhoAmI Test Zome...\n");
                        await _holoNETClientAppAgent.CallZomeFunctionAsync("whoami", "whoami", ZomeCallback, null);
                    }
                    break;

                case TestToRun.Numbers:
                    {
                        Console.WriteLine("Calling add_ten function on Numbers Test Zome...\n");
                        await _holoNETClientAppAgent.CallZomeFunctionAsync("numbers", "add_ten", ZomeCallback, new { number = 10 });
                    }
                    break;

                case TestToRun.SaveLoadOASISEntryWithEntryDataObject:
                case TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject:
                {
                        Console.WriteLine("Calling create_entry_avatar function on OASIS Test Zome...\n");
                        await _holoNETClientAppAgent.CallZomeFunctionAsync("oasis", "create_entry_avatar", ZomeCallback, new 
                        { 
                            id = Guid.NewGuid(), 
                            first_name = "David", 
                            last_name = "Ellams", 
                            email = "davidellams@superland.com", 
                            dob = "11/07/1980",
                            created_date = DateTime.Now.ToString(),
                            created_by = _holoNETClientAppAgent.HoloNETDNA.AgentPubKey,
                            modified_date = "",
                            modified_by = "",
                            deleted_date = "",
                            deleted_by = "",
                            is_active = true,
                            version = 1
                        });
                    }
                    break;

                case TestToRun.Signal:
                    {
                        Console.WriteLine("Calling test_signal function on OASIS Test Zome...\n");
                        await _holoNETClientAppAgent.CallZomeFunctionAsync("oasis", "test_signal_as_string", ZomeCallback, "test signal data");
                        //await _holoNETClientAppAgent.CallZomeFunctionAsync("oasis", "test_signal_as_int", ZomeCallback, 7);
                        //await _holoNETClientAppAgent.CallZomeFunctionAsync("oasis", "test_signal_as_int_2", ZomeCallback, 8);
                    }
                    break;

                case TestToRun.LoadTestNumbers:
                    {
                        Console.WriteLine("Calling add_ten function on Numbers Test Zome (Load Testing)...\n");

                        for (int i = 0; i < 100; i++)
                            await _holoNETClientAppAgent.CallZomeFunctionAsync("numbers", "add_ten", ZomeCallback, new { number = 10 });
                    }
                    break;

                case TestToRun.LoadTestSaveLoadOASISEntry:
                    {
                        Console.WriteLine("Calling create_entry_avatar function on OASIS Test Zome (Load Testing)...\n");

                        for (int i = 0; i < 100; i++)
                        {
                            await _holoNETClientAppAgent.CallZomeFunctionAsync("oasis", "create_entry_avatar", ZomeCallback, new
                            {
                                id = Guid.NewGuid(),
                                first_name = "David",
                                last_name = "Ellams",
                                email = "davidellams@hotmail.com",
                                dob = "11/07/1980",
                                created_date = DateTime.Now.ToString(),
                                created_by = _holoNETClientAppAgent.HoloNETDNA.AgentPubKey,
                                modified_date = "",
                                modified_by = "",
                                deleted_date = "",
                                deleted_by = "",
                                is_active = true,
                                version = 1
                            });
                        }
                    }
                    break;
            }
        }

        private static void HoloNETClient_OnAppInfoCallBack(object sender, AppInfoCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: APPINFO CALLBACK EVENT HANDLER: EndPoint: { e.EndPoint}, Id: {e.Id}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, Installed App Id: {e.InstalledAppId}, Raw Binary Data: {e.RawBinaryData}, IsError: {e.IsError}, Message: {e.Message}";
            Console.WriteLine(string.Concat(msg, ", Raw JSON Data: ", e.RawJSONData));
            Console.WriteLine("");
        }

        private static void HoloNETClient_OnConductorDebugCallBack(object sender, ConductorDebugCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("OnConductorDebugCallBack: EndPoint: ", e.EndPoint, ", Data: ", e.RawJSONData, ", NumberDelayedValidations: ", e.NumberDelayedValidations, ", NumberHeldAspects: ", e.NumberHeldAspects, ", NumberHeldEntries: ", e.NumberHeldEntries, ", NumberPendingValidations: ", e.NumberPendingValidations, ", NumberRunningZomeCalls: ", e.NumberRunningZomeCalls, ", Offline: ", e.Offline, ", Type: ", e.Type));
            Console.WriteLine("");
        }

        private static void HoloNETClient_OnSignalCallBack(object sender, SignalCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: SIGINALS CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id, ", Data: ", e.RawJSONData, ", AgentPubKey =  ", e.AgentPubKey, ", DnaHash = ", e.DnaHash, ", Signal Type: ", Enum.GetName(typeof(SignalType), e.SignalType), ", Signal Data: ", e.SignalDataAsString, ", IsError: ", e.IsError, ", Message: ", e.Message));
            //Console.WriteLine("\nSignal Data:");

            //foreach (string key in e.SignalData.Keys)
            //    Console.WriteLine(string.Concat(key, "=", e.SignalData[key]), "\n");
            
            Console.WriteLine("");
        }

        private static void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: ERROR EVENT HANDLER: Error Occurred. Resason: ", e.Reason, ", EndPoint: ", e.EndPoint, ",Error Details: ", e.ErrorDetails));
            Console.WriteLine("");
        }

        private static void ZomeCallback(object sender, ZomeFunctionCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("\nTEST HARNESS: ZOME CALLBACK DELEGATE EVENT HANDLER: ", ProcessZomeFunctionCallBackEventArgs(e)));
            Console.WriteLine("");
        }

        private static void HoloNETClient_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: DISCONNECTED CALL BACK: Disconnected from ", e.EndPoint, ". Resason: ", e.Reason));
            Console.WriteLine("");

            //if (_testToRun == TestToRun.LoadTestNumbers || _testToRun == TestToRun.LoadTestSaveLoadOASISEntry)
            //{
            //    TimeSpan timeSpan = _endTime.Subtract(_startTime);
            //    Console.WriteLine($"Test Complete: Time Took: {timeSpan}");
            //}

            Console.ReadKey();
        }

        private static void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            bool disconect = false;
            Console.WriteLine(string.Concat("TEST HARNESS: ZOME FUNCTION CALLBACK EVENT HANDLER: ", ProcessZomeFunctionCallBackEventArgs(e)));
            Console.WriteLine("");

            if (!string.IsNullOrEmpty(e.ZomeReturnHash) && e.ZomeFunction == "create_entry_avatar" && (_testToRun == TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject || _testToRun == TestToRun.SaveLoadOASISEntryWithEntryDataObject || _testToRun == TestToRun.LoadTestSaveLoadOASISEntry))
            {
                _saveEntryResponseReceived[_requestNumber] = true;

                if (_testToRun == TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject)
                    _holoNETClientAppAgent.CallZomeFunctionAsync("oasis", "get_entry_avatar", ZomeCallback, e.ZomeReturnHash, typeof(AvatarEntryDataObject));

                else if (_testToRun == TestToRun.SaveLoadOASISEntryWithEntryDataObject)
                {
                    _avatarEntryDataObject = new AvatarEntryDataObject();
                    _holoNETClientAppAgent.CallZomeFunctionAsync("oasis", "get_entry_avatar", ZomeCallback, e.ZomeReturnHash, _avatarEntryDataObject);
                }
            }
            else
            {
                //TODO: Need to make tests and results more accurate... (in future version)...
                if (((_testToRun == TestToRun.LoadTestNumbers
                    || _testToRun == TestToRun.LoadTestSaveLoadOASISEntry)
                    && _numberOfZomeCallResponsesReceived >= 96)
                    || (_testToRun != TestToRun.LoadTestNumbers
                    && _testToRun != TestToRun.LoadTestSaveLoadOASISEntry))
                {
                    if (_testToRun == TestToRun.LoadTestNumbers || _testToRun == TestToRun.LoadTestSaveLoadOASISEntry)
                    {
                        _timer.Stop();
                        Console.WriteLine($"Test Complete: Time Took: {_timer.Elapsed.Minutes} minute(s) and {_timer.Elapsed.Seconds} second(s).");
                    }

                    disconect = true;
                }
                else
                    _numberOfZomeCallResponsesReceived++;
            }

            Console.WriteLine(String.Concat("Number Of Zome Call Responses Received = ", _numberOfZomeCallResponsesReceived));

            if (disconect)
            {
                Console.WriteLine("");
                //_holoNETClientAppAgent.ShutdownHoloNETAsync(DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode.ShutdownAllConductors);

                _holoNETClientAppAgent.Disconnect();
                _holoNETClientAppAgent.ShutDownHolochainConductors();
               
            }

            //_requestNumber++;
        }

        private static void Holon_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Holon_OnInitialized, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void Avatar_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Avatar_OnInitialized, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void Holon_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Holon_OnClosed, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, NumberOfHcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void Holon_OnError(object sender, HoloNETErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"TEST HARNESS: Holon_OnError, EndPoint: {e.EndPoint}, Reason: {e.Reason}");
        }

        private static void Holon_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Holon_OnDeleted: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static void Holon_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Holon_OnSaved: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static void Holon_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Holon_OnLoaded: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static void Avatar_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Avatar_OnClosed, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, NumberOfHcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}, IsError: {e.IsError}, Message: {e.Message}");
        }

        private static void Avatar_OnError(object sender, HoloNETErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"TEST HARNESS: Avatar_OnError, EndPoint: {e.EndPoint}, Reason: {e.Reason}");
        }

        private static void Avatar_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Avatar_OnDeleted: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static void Avatar_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Avatar_OnSaved: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static void Avatar_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: Avatar_OnLoaded: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";
            //result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            if (args.Records != null && args.Records[0] != null)
            {
                AvatarEntryDataObject avatar = args.Records[0].EntryDataObject as AvatarEntryDataObject;

                if (avatar != null)
                    result = BuildEntryDataObjectMessage(avatar, "Entry.EntryDataObject", result);
            }
            
            if (_avatarEntryDataObject != null)
                result = BuildEntryDataObjectMessage(_avatarEntryDataObject, "Global.EntryDataObject", result);

            return result;
        }

        private static string BuildEntryDataObjectMessage(AvatarEntryDataObject avatar, string header, string message)
        {
            message = string.Concat(message, "\n\n", header, ".FirstName: ", avatar.FirstName);
            message = string.Concat(message, "\n", header, ".LastName: ", avatar.LastName);
            message = string.Concat(message, "\n", header, ".Email: ", avatar.Email);
            message = string.Concat(message, "\n", header, ".DOB: ", avatar.DOB);

            return message;
        }

        private static Avatar ResetAvatar(Avatar avatar)
        {
            avatar.Id = Guid.Empty;
            avatar.FirstName = "";
            avatar.LastName = "";
            avatar.Email = "";
            avatar.DOB = DateTime.MinValue;
            avatar.CreatedBy = "";
            avatar.CreatedDate = DateTime.MinValue;
            avatar.ModifiedBy = "";
            avatar.ModifiedDate = DateTime.MinValue;
            avatar.DeletedBy = "";
            avatar.DeletedDate = DateTime.MinValue;

            return avatar;
        }

        private static AvatarMultiple ResetAvatar(AvatarMultiple avatar)
        {
            avatar.Id = Guid.Empty;
            avatar.FirstName = "";
            avatar.LastName = "";
            avatar.Email = "";
            avatar.DOB = DateTime.MinValue;
            avatar.CreatedBy = "";
            avatar.CreatedDate = DateTime.MinValue;
            avatar.ModifiedBy = "";
            avatar.ModifiedDate = DateTime.MinValue;
            avatar.DeletedBy = "";
            avatar.DeletedDate = DateTime.MinValue;

            return avatar;
        }

        private static Holon ResetHolon(Holon holon)
        {
            holon.Id = Guid.Empty;
            holon.ParentId = Guid.Empty;
            holon.Name = "";
            holon.Description = "";
            holon.CreatedBy = "";
            holon.CreatedDate = DateTime.MinValue;
            holon.ModifiedBy = "";
            holon.ModifiedDate = DateTime.MinValue;
            holon.DeletedBy = "";
            holon.DeletedDate = DateTime.MinValue;

            return holon;
        }

        private static void ShowAvatarDetails(Avatar avatar)
        {
            Console.WriteLine(string.Concat("Avatar.EntryHash = ", avatar.EntryHash));
            Console.WriteLine(string.Concat("Avatar.Id = ", avatar.Id.ToString()));
            Console.WriteLine(string.Concat("Avatar.FirstName = ", avatar.FirstName.ToString()));
            Console.WriteLine(string.Concat("Avatar.LastName = ", avatar.LastName.ToString()));
            Console.WriteLine(string.Concat("Avatar.Email = ", avatar.Email.ToString()));
            Console.WriteLine(string.Concat("Avatar.DOB = ", avatar.DOB.ToString()));
            Console.WriteLine(string.Concat("Avatar.CreatedDate = ", avatar.CreatedDate.ToString()));
            Console.WriteLine(string.Concat("Avatar.CreatedBy = ", avatar.CreatedBy));
            Console.WriteLine(string.Concat("Avatar.ModifiedDate = ", avatar.ModifiedDate.ToString()));
            Console.WriteLine(string.Concat("Avatar.ModifiedBy = ", avatar.ModifiedBy));
            Console.WriteLine(string.Concat("Avatar.DeletedDate = ", avatar.DeletedDate.ToString()));
            Console.WriteLine(string.Concat("Avatar.DeletedBy = ", avatar.DeletedBy));
            Console.WriteLine(string.Concat("Avatar.IsActive = ", avatar.IsActive.ToString()));
        }

        private static void ShowAvatarDetails(AvatarMultiple avatar)
        {
            Console.WriteLine(string.Concat("Avatar.EntryHash = ", avatar.EntryHash));
            Console.WriteLine(string.Concat("Avatar.Id = ", avatar.Id.ToString()));
            Console.WriteLine(string.Concat("Avatar.FirstName = ", avatar.FirstName.ToString()));
            Console.WriteLine(string.Concat("Avatar.LastName = ", avatar.LastName.ToString()));
            Console.WriteLine(string.Concat("Avatar.Email = ", avatar.Email.ToString()));
            Console.WriteLine(string.Concat("Avatar.DOB = ", avatar.DOB.ToString()));
            Console.WriteLine(string.Concat("Avatar.CreatedDate = ", avatar.CreatedDate.ToString()));
            Console.WriteLine(string.Concat("Avatar.CreatedBy = ", avatar.CreatedBy));
            Console.WriteLine(string.Concat("Avatar.ModifiedDate = ", avatar.ModifiedDate.ToString()));
            Console.WriteLine(string.Concat("Avatar.ModifiedBy = ", avatar.ModifiedBy));
            Console.WriteLine(string.Concat("Avatar.DeletedDate = ", avatar.DeletedDate.ToString()));
            Console.WriteLine(string.Concat("Avatar.DeletedBy = ", avatar.DeletedBy));
            Console.WriteLine(string.Concat("Avatar.IsActive = ", avatar.IsActive.ToString()));
        }

        private static void ShowHolonDetails(Holon holon)
        {
            Console.WriteLine(string.Concat("Holon.EntryHash = ", holon.EntryHash));
            Console.WriteLine(string.Concat("Holon.Id = ", holon.Id.ToString()));
            Console.WriteLine(string.Concat("Holon.ParentId = ", holon.ParentId.ToString()));
            Console.WriteLine(string.Concat("Holon.Name = ", holon.Name));
            Console.WriteLine(string.Concat("Holon.Description = ", holon.Description));
            Console.WriteLine(string.Concat("Holon.CreatedDate = ", holon.CreatedDate.ToString()));
            Console.WriteLine(string.Concat("Holon.CreatedBy = ", holon.CreatedBy));
            Console.WriteLine(string.Concat("Holon.ModifiedDate = ", holon.ModifiedDate.ToString()));
            Console.WriteLine(string.Concat("Holon.ModifiedBy = ", holon.ModifiedBy));
            Console.WriteLine(string.Concat("Holon.DeletedDate = ", holon.DeletedDate.ToString()));
            Console.WriteLine(string.Concat("Holon.DeletedBy = ", holon.DeletedBy));
            Console.WriteLine(string.Concat("Holon.IsActive = ", holon.IsActive.ToString()));
        }
    }
}