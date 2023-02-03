using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NextGenSoftware.CLI.Engine;
using NextGenSoftware.Logging;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class HoloNETTestHarness
    {
        private static HoloNETClient _holoNETClient = null;
        private static TestToRun _testToRun;
        private static int _numberOfZomeCallResponsesReceived = 0;
        private static Dictionary<int, bool> _loadEntryResponseReceived = new Dictionary<int, bool>();
        private static Dictionary<int, bool> _saveEntryResponseReceived = new Dictionary<int, bool>();
        private static int _requestNumber = 10;
        private static Stopwatch _timer = new Stopwatch(); // creating new instance of the stopwatch
        private static ConsoleColor _testHeadingColour = ConsoleColor.Yellow;
        private static AvatarEntryDataObject _avatarEntryDataObject = null;

        static async Task Main(string[] args)
        {
            await TestHoloNETClientAsync(TestToRun.SaveLoadOASISEntryUsingSingleHoloNETAuditEntryBaseClass);
            //await TestHoloNETClientAsync(TestToRun.Signal);
            //await TestHoloNETClientAsync(TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject);
            //await TestHoloNETClientAsync(TestToRun.SaveLoadOASISEntryWithEntryDataObject);
            //await TestHoloNETClientAsync(TestToRun.LoadTestSaveLoadOASISEntry);
            //await TestHoloNETClientAsync(TestToRun.WhoAmI);
        }

        public static async Task TestHoloNETClientAsync(TestToRun testToRun)
        {
            _timer.Start();
            _testToRun = testToRun;
            Console.WriteLine("NextGenSoftware.Holochain.HoloNET.Client Test Harness v2.0.2");
            Console.WriteLine("");

            HoloNETConfig config = new HoloNETConfig()
            {
                LoggingMode = LoggingMode.WarningsErrorsInfoAndDebug,

                //holoNETClient.Config.ErrorHandlingBehaviour = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent
                AutoStartHolochainConductor = true,
                AutoShutdownHolochainConductor = true,
                ShutDownALLHolochainConductors = true, //Normally default's to false, but if you want to make sure no holochain processes are left running set this to true.
                ShowHolochainConductorWindow = true, //Defaults to false.
                HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded,
                HolochainConductorToUse = HolochainConductorEnum.HcDevTool
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
                        config.FullPathToCompiledHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\OASIS-Holochain-hApp\zomes\workdir\happ");
                    }
                    break;

                case TestToRun.WhoAmI:
                case TestToRun.Numbers:
                case TestToRun.LoadTestNumbers:
                    {
                        config.FullPathToRootHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop");
                        config.FullPathToCompiledHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop\workdir\happ");
                    }
                    break;
            }

            //No need to create this when the test is SaveLoadOASISEntryUsingSingleHoloNETBaseClass because it will create its own instance of HoloNETClient internally (in the base class).
            if (_testToRun != TestToRun.SaveLoadOASISEntryUsingSingleHoloNETAuditEntryBaseClass)
            {
                _holoNETClient = new HoloNETClient("ws://localhost:8888");

                //We would normally just set the Config property but in this case we need to share the Config object with multiple HoloNET instances (such as in the SaveLoadOASISEntryUsingSingleHoloNETBaseClass test) 
                _holoNETClient.Config = config;

                _holoNETClient.OnConnected += HoloNETClient_OnConnected;
                _holoNETClient.OnDataReceived += HoloNETClient_OnDataReceived;
                _holoNETClient.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;
                _holoNETClient.OnAppInfoCallBack += HoloNETClient_OnAppInfoCallBack;
                _holoNETClient.OnReadyForZomeCalls += _holoNETClient_OnReadyForZomeCalls;
                _holoNETClient.OnSignalsCallBack += HoloNETClient_OnSignalsCallBack;
                _holoNETClient.OnDisconnected += HoloNETClient_OnDisconnected;
                _holoNETClient.OnError += HoloNETClient_OnError;
                _holoNETClient.OnHolochainConductorsShutdownComplete += _holoNETClient_OnHolochainConductorsShutdownComplete;
                _holoNETClient.OnHoloNETShutdownComplete += _holoNETClient_OnHoloNETShutdownComplete;

                ////Use this if you to manually pass in the AgentPubKey &DnaHash(otherwise it will be automatically queried from the conductor or sandbox).
                //_holoNETClient.Config.AgentPubKey = "YOUR KEY";
                //_holoNETClient.Config.DnaHash = "YOUR HASH";

                //await _holoNETClient.Connect(false, false);
                await _holoNETClient.ConnectAsync();
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

                        //By default the constructor's will auto call the InitializeAsync method but if you wish to call it manually so you can await it and ensure it is connected and AgentPubKey & DnaHash have been retreived you can do so as below:
                        //We need to make sure we call this before anything else to make sure HoloNET is connected to the Conductor and so the AgentPubKey & DnaHash are set.
                        //CLIEngine.ShowWorkingMessage("Initializing Avatar...");
                        //await avatar.InitializeAsync();

                        //Alternatively if you use the defaults and allow the constructor to auto-call InitializeAsync() then you can await the WaitTillHoloNETInitializedAsync method to make sure it is connected and the AgentPubKey & DnaHash have been retreived.
                        //Note that all the CRUD methods LoadAsync, SaveAsync & DeleteAsync will automatically await internally until HoloNET is ready. 
                        //So you only need to use the WaitTillHoloNETInitializedAsync method if you have not used the InitializeAsync method above and if you wish to access the HoloNETClient.Config.AgentPubKey or HoloNETClient.Config.DnaHash properties BEFORE calling any of the CRUD methods.
                        await avatar.WaitTillHoloNETInitializedAsync();
                        CLIEngine.ShowMessage($"AgentPubKey:{avatar.HoloNETClient.Config.AgentPubKey}");
                        CLIEngine.ShowMessage($"DnaHash:{avatar.HoloNETClient.Config.DnaHash}");


                        CLIEngine.ShowWorkingMessage("Saving Avatar...");
                        ZomeFunctionCallBackEventArgs result =  await avatar.SaveAsync();

                        if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                        {
                            CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.SAVE/CREATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                            Console.WriteLine("");
                            ShowAvatarDetails(avatar);

                            avatar = ResetAvatar(avatar);
                            CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY LOAD TEST ***", _testHeadingColour);
                            

                            //string entryHash = avatar.EntryHash;
                            //avatar.Dispose(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            //await avatar.CloseAsync();

                            //avatar = new Avatar(config); //Normally you wouldn't need to create a new instance but we want to verify the data is fully loaded so best to use a new object.
                            //avatar.EntryHash = entryHash;

                            CLIEngine.ShowWorkingMessage("Loading Avatar...");
                            result = await avatar.LoadAsync();

                            if (result.IsCallSuccessful && !result.IsError)
                            {
                                CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                ShowAvatarDetails(avatar);

                                // Now test the update method.
                                CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY SAVE/UPDATE TEST ***", _testHeadingColour);
                                avatar.FirstName = "James";
                                CLIEngine.ShowWorkingMessage("Saving Avatar...");
                                result = await avatar.SaveAsync();

                                if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                {
                                    CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                    ShowAvatarDetails(avatar);

                                    //Now check the update was saved correctly by re-loading it...
                                    CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY LOAD TEST 2 (VERIFYING UPDATE WAS SUCCESSFUL) ***", _testHeadingColour);
                                    CLIEngine.ShowWorkingMessage("Loading Avatar...");

                                    avatar = ResetAvatar(avatar);
                                    result = await avatar.LoadAsync();

                                    if (result.IsCallSuccessful && !result.IsError)
                                    {
                                        CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        ShowAvatarDetails(avatar);

                                        CLIEngine.ShowMessage("*** AVATAR SINGLE HOLONET BASE ENTRY DELETE TEST ***", _testHeadingColour);
                                        CLIEngine.ShowWorkingMessage("Deleting Avatar...");
                                        result = await avatar.DeleteAsync();

                                        if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
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
                        AvatarMultiple avatar = new AvatarMultiple(_holoNETClient)
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

                        if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                        {
                            Console.WriteLine(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONETBASECLASS.SAVE/CREATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                            Console.WriteLine("");
                            ShowAvatarDetails(avatar);
                            string entryHash = avatar.EntryHash;

                            //There is no need to call Dispose on an object sharing a HoloNETClient instance (passed in) because it will only dispose of (disconnect etc) a HoloNETClient if one was NOT passed in because it was internally created and used (like the single use above).
                            //avatar.Dispose(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            avatar = new AvatarMultiple(_holoNETClient); //Normally you wouldn't need to create a new instance but we want to verify the data is fully loaded so best to use a new object.
                            avatar.EntryHash = entryHash;
                            CLIEngine.ShowWorkingMessage("Loading Avatar...");
                            result = await avatar.LoadAsync();

                            if (result.IsCallSuccessful && !result.IsError)
                            {
                                Console.WriteLine(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONETBASECLASS.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                Console.WriteLine("");
                                ShowAvatarDetails(avatar);

                                // Now test the update method.
                                avatar.FirstName = "James";
                                CLIEngine.ShowWorkingMessage("Saving Avatar...");
                                result = await avatar.SaveAsync();

                                if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                {
                                    CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                    ShowAvatarDetails(avatar);

                                    //Now check the update was saved correctly by re-loading it...
                                    CLIEngine.ShowMessage("*** AVATAR MULTIPLE HOLONET BASE ENTRY LOAD TEST 2 (VERIFYING UPDATE WAS SUCCESSFUL) ***", _testHeadingColour);
                                    CLIEngine.ShowWorkingMessage("Loading Avatar...");

                                    avatar = ResetAvatar(avatar);
                                    result = await avatar.LoadAsync();

                                    if (result.IsCallSuccessful && !result.IsError)
                                    {
                                        CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR MULTIPLE HOLONET BASE ENTRY.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        ShowAvatarDetails(avatar);

                                        CLIEngine.ShowMessage("*** AVATAR MULTIPLE HOLONET BASE ENTRY DELETE TEST ***", _testHeadingColour);
                                        CLIEngine.ShowWorkingMessage("Deleting Avatar...");
                                        result = await avatar.DeleteAsync();

                                        if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
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

                        Holon holon = new Holon(_holoNETClient)
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

                        if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                        {
                            Console.WriteLine(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONETBASECLASS.SAVE/CREATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                            Console.WriteLine("");
                            ShowHolonDetails(holon);
                            string entryHash = holon.EntryHash;

                            //There is no need to call Dispose on an object sharing a HoloNETClient instance (passed in) because it will only dispose of (disconnect etc) a HoloNETClient if one was NOT passed in because it was internally created and used (like the single use above).
                            //holon.Dispose(); //Free any resources including disconnecting from the Holochain Conductor, shutting down any running conductors etc...
                            holon = new Holon(_holoNETClient); //Normally you wouldn't need to create a new instance but we want to verify the data is fully loaded so best to use a new object.
                            holon.EntryHash = entryHash;
                            CLIEngine.ShowWorkingMessage("Loading Holon...");
                            result = await holon.LoadAsync();

                            if (result.IsCallSuccessful && !result.IsError)
                            {
                                Console.WriteLine(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONETBASECLASS.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                Console.WriteLine("");
                                ShowHolonDetails(holon);

                                // Now test the update method.
                                holon.Name = "another holon";
                                CLIEngine.ShowWorkingMessage("Saving Holon...");
                                result = await holon.SaveAsync();

                                if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
                                {
                                    CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: AVATAR SINGLE HOLONET BASE ENTRY.SAVE/UPDATE RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                    ShowAvatarDetails(avatar);

                                    //Now check the update was saved correctly by re-loading it...
                                    CLIEngine.ShowMessage("*** HOLON MULTIPLEHOLONET BASE ENTRY LOAD TEST 2 (VERIFYING UPDATE WAS SUCCESSFUL) ***", _testHeadingColour);
                                    CLIEngine.ShowWorkingMessage("Loading Holon...");

                                    holon = ResetHolon(holon);
                                    result = await holon.LoadAsync();

                                    if (result.IsCallSuccessful && !result.IsError)
                                    {
                                        CLIEngine.ShowSuccessMessage(string.Concat("TEST HARNESS: HOLON MULTIPLE HOLONET BASE ENTRY.LOAD RESPONSE: ", ProcessZomeFunctionCallBackEventArgs(result)));
                                        ShowAvatarDetails(avatar);

                                        CLIEngine.ShowMessage("*** HOLON MULTIPLE HOLONET BASE ENTRY DELETE TEST ***", _testHeadingColour);
                                        CLIEngine.ShowWorkingMessage("Deleting Holon...");
                                        result = await avatar.DeleteAsync();

                                        if (result.IsCallSuccessful && !result.IsError && !string.IsNullOrEmpty(result.ZomeReturnHash))
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

        private static void _holoNETClient_OnHoloNETShutdownComplete(object sender, HoloNETShutdownEventArgs e)
        {
            string msg = $"TEST HARNESS: OnHoloNETShutdownComplete, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}";
            
            if (e.HolochainConductorsShutdownEventArgs != null)
                msg = string.Concat(msg, $", NumberOfHcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: { e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: { e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");

            CLIEngine.ShowMessage(msg);
        }

        private static void _holoNETClient_OnHolochainConductorsShutdownComplete(object sender, HolochainConductorsShutdownEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: OnHolochainConductorsShutdownComplete, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}, NumberOfHcExeInstancesShutdown: { e.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: {e.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: {e.NumberOfRustcExeInstancesShutdown}.");
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

        private async static void _holoNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: READY FOR ZOME CALLS EVENT HANDLER: EndPoint: ", e.EndPoint, ", AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash, ", IsError: ", e.IsError, ", Message: ", e.Message));
            Console.WriteLine("");

            switch (_testToRun)
            {
                case TestToRun.WhoAmI:
                    {
                        Console.WriteLine("Calling whoami function on WhoAmI Test Zome...\n");
                        await _holoNETClient.CallZomeFunctionAsync("whoami", "whoami", ZomeCallback, null);
                    }
                    break;

                case TestToRun.Numbers:
                    {
                        Console.WriteLine("Calling add_ten function on Numbers Test Zome...\n");
                        await _holoNETClient.CallZomeFunctionAsync("numbers", "add_ten", ZomeCallback, new { number = 10 });
                    }
                    break;

                case TestToRun.SaveLoadOASISEntryWithEntryDataObject:
                case TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject:
                {
                        Console.WriteLine("Calling create_entry_avatar function on OASIS Test Zome...\n");
                        await _holoNETClient.CallZomeFunctionAsync("oasis", "create_entry_avatar", ZomeCallback, new 
                        { 
                            id = Guid.NewGuid(), 
                            first_name = "David", 
                            last_name = "Ellams", 
                            email = "davidellams@hotmail.com", 
                            dob = "11/07/1980",
                            created_date = DateTime.Now.ToString(),
                            created_by = _holoNETClient.Config.AgentPubKey,
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
                        await _holoNETClient.CallZomeFunctionAsync("oasis", "test_signal_as_string", ZomeCallback, "test signal data");
                        //await _holoNETClient.CallZomeFunctionAsync("oasis", "test_signal_as_int", ZomeCallback, 7);
                        //await _holoNETClient.CallZomeFunctionAsync("oasis", "test_signal_as_int_2", ZomeCallback, 8);
                    }
                    break;

                case TestToRun.LoadTestNumbers:
                    {
                        Console.WriteLine("Calling add_ten function on Numbers Test Zome (Load Testing)...\n");

                        for (int i = 0; i < 100; i++)
                            await _holoNETClient.CallZomeFunctionAsync("numbers", "add_ten", ZomeCallback, new { number = 10 });
                    }
                    break;

                case TestToRun.LoadTestSaveLoadOASISEntry:
                    {
                        Console.WriteLine("Calling create_entry_avatar function on OASIS Test Zome (Load Testing)...\n");

                        for (int i = 0; i < 100; i++)
                        {
                            await _holoNETClient.CallZomeFunctionAsync("oasis", "create_entry_avatar", ZomeCallback, new
                            {
                                id = Guid.NewGuid(),
                                first_name = "David",
                                last_name = "Ellams",
                                email = "davidellams@hotmail.com",
                                dob = "11/07/1980",
                                created_date = DateTime.Now.ToString(),
                                created_by = _holoNETClient.Config.AgentPubKey,
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

        private static void HoloNETClient_OnSignalsCallBack(object sender, SignalsCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: SIGINALS CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id, ", Data: ", e.RawJSONData, ", AgentPubKey =  ", e.AgentPubKey, ", DnaHash = ", e.DnaHash, ", Signal Type: ", Enum.GetName(typeof(SignalType), e.SignalType), ", Signal Data: ", e.SignalDataAsString, ", IsError: ", e.IsError, ", Message: ", e.Message));
            //Console.WriteLine("\nSignal Data:");

            //foreach (string key in e.SignalData.Keys)
            //    Console.WriteLine(string.Concat(key, "=", e.SignalData[key]), "\n");
            
            Console.WriteLine("");
        }

        private static void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage(string.Concat("TEST HARNESS: ERROR EVENT HANDLER: Error Occured. Resason: ", e.Reason, ", EndPoint: ", e.EndPoint, ",Error Details: ", e.ErrorDetails));
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

            //if (!string.IsNullOrEmpty(e.ZomeReturnHash) && e.ZomeFunction == "create_entry_avatar" && (_testToRun == TestToRun.SaveLoadOASISEntry || _testToRun == TestToRun.LoadTestSaveLoadOASISEntry))
            //{
            //    _saveEntryResponseReceived[_requestNumber] = true;
            //    _holoNETClient.CallZomeFunctionAsync(_requestNumber.ToString(), "oasis", "get_entry_avatar", ZomeCallback, e.ZomeReturnHash);
            //}
            //else if (e.ZomeFunction == "get_entry_avatar")
            //{
            //    _loadEntryResponseReceived[_requestNumber] = true;
            //    _numberOfZomeCallResponsesReceived++;
            //}

            //if (_loadEntryResponseReceived.Count >= 100)
            //    _holoNETClient.Disconnect();

            //Console.WriteLine(String.Concat("Current Request Number = ", _requestNumber));
            //Console.WriteLine(String.Concat("Number Of Zome Call Responses Received = ", _numberOfZomeCallResponsesReceived));
            //Console.WriteLine(String.Concat("Save Entry Response Received = ", _saveEntryResponseReceived.Count));
            //Console.WriteLine(String.Concat("Load Entry Response Received = ", _loadEntryResponseReceived.Count));

            if (!string.IsNullOrEmpty(e.ZomeReturnHash) && e.ZomeFunction == "create_entry_avatar" && (_testToRun == TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject || _testToRun == TestToRun.SaveLoadOASISEntryWithEntryDataObject || _testToRun == TestToRun.LoadTestSaveLoadOASISEntry))
            {
                _saveEntryResponseReceived[_requestNumber] = true;

                if (_testToRun == TestToRun.SaveLoadOASISEntryWithTypeOfEntryDataObject)
                    _holoNETClient.CallZomeFunctionAsync("oasis", "get_entry_avatar", ZomeCallback, e.ZomeReturnHash, typeof(AvatarEntryDataObject));

                else if (_testToRun == TestToRun.SaveLoadOASISEntryWithEntryDataObject)
                {
                    _avatarEntryDataObject = new AvatarEntryDataObject();
                    _holoNETClient.CallZomeFunctionAsync("oasis", "get_entry_avatar", ZomeCallback, e.ZomeReturnHash, _avatarEntryDataObject);
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
                //_holoNETClient.ShutdownHoloNETAsync(DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode.ShutdownAllConductors);

                _holoNETClient.Disconnect();
                _holoNETClient.ShutDownHolochainConductors();
               
            }

            //_requestNumber++;
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";
            
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            if (args.Entry != null && args.Entry.EntryDataObject != null)
            {
                AvatarEntryDataObject avatar = args.Entry.EntryDataObject as AvatarEntryDataObject;

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

        private static void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (!e.IsConductorDebugInfo)
            {
                Console.WriteLine(string.Concat("\nTEST HARNESS: DATA RECEIVED EVENT HANDLER: EndPoint: ", e.EndPoint, ", Raw JSON Data: ", e.RawJSONData, ", Raw Binary Data: ", e.RawBinaryData, ", Raw Binary Data After MessagePack Decode: ", e.RawBinaryDataAfterMessagePackDecode, ", Raw Binary Data After MessagePack Decode As String: ", e.RawBinaryDataAfterMessagePackDecodeAsString, ", IsError: ", e.IsError, ", Message:", e.Message));
                Console.WriteLine("");
            }
        }

        private static void HoloNETClient_OnConnected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: CONNECTED CALLBACK: Connected to ", e.EndPoint));
            Console.WriteLine("");
        }
    }
}