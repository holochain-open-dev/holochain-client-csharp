using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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


        static async Task Main(string[] args)
        {
            await TestHoloNETClientAsync(TestToRun.SaveLoadOASISEntry);
        }

        public static async Task TestHoloNETClientAsync(TestToRun testToRun)
        {
            _timer.Start();
            _testToRun = testToRun;
            Console.WriteLine("NextGenSoftware.Holochain.HoloNET.Client Test Harness v1.2");
            Console.WriteLine("");
            _holoNETClient = new HoloNETClient("ws://localhost:8888");
            _holoNETClient.WebSocket.Config.NeverTimeOut = true;

            _holoNETClient.Config.LoggingMode = LoggingMode.WarningsErrorsInfoAndDebug;
            
            //holoNETClient.Config.ErrorHandlingBehaviour = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent
            _holoNETClient.Config.AutoStartHolochainConductor = false;
            _holoNETClient.Config.AutoShutdownHolochainConductor = false;
            _holoNETClient.Config.ShutDownALLHolochainConductors = false; //Normally default's to false, but if you want to make sure no holochain processes are left running set this to true.
            _holoNETClient.Config.ShowHolochainConductorWindow = false; //Defaults to false.
            _holoNETClient.Config.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded;
            _holoNETClient.Config.HolochainConductorToUse = HolochainConductorEnum.HcDevTool;
           
            switch (testToRun)
            {
                case TestToRun.SaveLoadOASISEntry:
                case TestToRun.LoadTestSaveLoadOASISEntry:
                {
                        _holoNETClient.Config.FullPathToRootHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\OASIS-Holochain-hApp");
                        _holoNETClient.Config.FullPathToCompiledHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\OASIS-Holochain-hApp\zomes\workdir\happ");
                    }break;

                case TestToRun.WhoAmI:
                case TestToRun.Numbers:
                case TestToRun.LoadTestNumbers:
                    {
                        _holoNETClient.Config.FullPathToRootHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop");
                        _holoNETClient.Config.FullPathToCompiledHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop\workdir\happ");
                    }
                    break;
            }

            _holoNETClient.OnConnected += HoloNETClient_OnConnected;
            _holoNETClient.OnDataReceived += HoloNETClient_OnDataReceived;
            _holoNETClient.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;
            _holoNETClient.OnAppInfoCallBack += HoloNETClient_OnAppInfoCallBack;
            _holoNETClient.OnReadyForZomeCalls += _holoNETClient_OnReadyForZomeCalls;
            _holoNETClient.OnSignalsCallBack += HoloNETClient_OnSignalsCallBack;
            _holoNETClient.OnDisconnected += HoloNETClient_OnDisconnected;
            _holoNETClient.OnError += HoloNETClient_OnError;
            _holoNETClient.OnConductorDebugCallBack += HoloNETClient_OnConductorDebugCallBack;

            ////Use this if you to manually pass in the AgentPubKey &DnaHash(otherwise it will be automatically queried from the conductor or sandbox).
            //_holoNETClient.Config.AgentPubKey = "YOUR KEY";
            //_holoNETClient.Config.DnaHash = "YOUR HASH";

            //await _holoNETClient.Connect(false, false);
            await _holoNETClient.Connect();
        }

        private async static void _holoNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: READY FOR ZOME CALLS EVENT HANDLER: EndPoint: ", e.EndPoint, ", AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash));
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

                case TestToRun.SaveLoadOASISEntry:
                    {
                        Console.WriteLine("Calling create_entry_avatar function on OASIS Test Zome...\n");
                        await _holoNETClient.CallZomeFunctionAsync("oasis", "create_entry_avatar", ZomeCallback, new { id = 1, first_name = "David", last_name = "Ellams", email = "davidellams@hotmail.com", dob = "11/04/1980" });
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
                            await _holoNETClient.CallZomeFunctionAsync("oasis", "create_entry_avatar", ZomeCallback, new { id = 1, first_name = "David", last_name = "Ellams", email = "davidellams@hotmail.com", dob = "11/04/1980" });
                    }
                    break;
            }
        }

        private static void HoloNETClient_OnAppInfoCallBack(object sender, AppInfoCallBackEventArgs e)
        {
            string msg = $"TEST HARNESS: APPINFO CALLBACK EVENT HANDLER: EndPoint: { e.EndPoint}, Id: {e.Id}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, Installed App Id: {e.InstalledAppId}, Raw Binary Data: {e.RawBinaryData}";
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
            Console.WriteLine(string.Concat("TEST HARNESS: SIGINALS CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id , ", Data: ", e.RawJSONData, "Name: ", e.Name, "SignalType: ", Enum.GetName(typeof(SignalsCallBackEventArgs.SignalTypes), e.SignalType), "Arguments: ", e.SignalData));
            Console.WriteLine("");
        }

        private static void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: ERROR EVENT HANDLER: Error Occured. Resason: ", e.Reason, ", EndPoint: ", e.EndPoint, ",Error Details: ", e.ErrorDetails));
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

            if (!string.IsNullOrEmpty(e.ZomeReturnHash) && e.ZomeFunction == "create_entry_avatar" && (_testToRun == TestToRun.SaveLoadOASISEntry || _testToRun == TestToRun.LoadTestSaveLoadOASISEntry))
            {
                _saveEntryResponseReceived[_requestNumber] = true;
                _holoNETClient.CallZomeFunctionAsync("oasis", "get_entry_avatar", ZomeCallback, e.ZomeReturnHash, typeof(Avatar));
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
                _holoNETClient.Disconnect();
            }

            //_requestNumber++;
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";
            
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nRaw Data: ", args.RawData, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Daya: ", args.RawBinaryData, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false");

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            return result;
        }

        private static void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (!e.IsConductorDebugInfo)
            {
                Console.WriteLine(string.Concat("\nTEST HARNESS: DATA RECEIVED EVENT HANDLER: EndPoint: ", e.EndPoint, ", Raw JSON Data: ", e.RawJSONData, ", Raw Binary Data: ", e.RawBinaryData));
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