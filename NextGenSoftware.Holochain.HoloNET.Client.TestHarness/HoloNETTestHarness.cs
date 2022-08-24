using System;
using System.Threading.Tasks;
using NextGenSoftware.Logging;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    public class HoloNETTestHarness
    {
        private static HoloNETClient _holoNETClient = null;

        static async Task Main(string[] args)
        {
            await TestHoloNETClientAsync();
        }

        public static async Task TestHoloNETClientAsync()
        {
            Console.WriteLine("NextGenSoftware.Holochain.HoloNET.Client Test Harness v1.0");
            Console.WriteLine("");
            _holoNETClient = new HoloNETClient("ws://localhost:8888");
            _holoNETClient.WebSocket.Config.NeverTimeOut = true;

            //If no LoggingMode is set then it will default to WarningsErrorsAndInfo.
            _holoNETClient.Config.LoggingMode = LoggingMode.WarningsErrorsInfoAndDebug;
            //_holoNETClient.Config.LoggingMode = LoggingMode.WarningsAndErrors;

            /*
            //holoNETClient.Config.ErrorHandlingBehaviour = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent
            _holoNETClient.Config.AutoStartHolochainConductor = true;
            _holoNETClient.Config.AutoShutdownHolochainConductor = true;
            _holoNETClient.Config.ShutDownALLHolochainConductors = true; //Normally default's to false, but if you want to make sure no holochain processes are left running set this to true.
            _holoNETClient.Config.ShowHolochainConductorWindow = true; //Defaults to false.
           // _holoNETClient.Config.HolochainConductorMode = HolochainConductorModeEnum.UseSystemGlobal;
            _holoNETClient.Config.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded;
            _holoNETClient.Config.HolochainConductorToUse = HolochainConductorEnum.HcDevTool;
            */

            //holoNETClient.Config.FullPathToHolochainAppDNA = @"D:\Dropbox\Our World\OASIS API\NextGenSoftware.Holochain.hApp.OurWorld\our_world\dist\our_world.dna.json";
            _holoNETClient.Config.FullPathToRootHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop");
            _holoNETClient.Config.FullPathToCompiledHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop\workdir\happ");
            //_holoNETClient.Config.FullPathToHapp = string.Concat(Environment.CurrentDirectory, @"\hApps\numbers-hApp");
           

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
            Console.WriteLine("Calling Test Zome...\n");

            //await _holoNETClient.CallZomeFunctionAsync("1", "our_world_core", "test", ZomeCallback, null);
            //await _holoNETClient.CallZomeFunctionAsync("1", "whoami", "whoami", ZomeCallback, null);
            //await _holoNETClient.CallZomeFunctionAsync("1", "whoami", "whoami", ZomeCallback, null);
            await _holoNETClient.CallZomeFunctionAsync("1", "numbers", "add_ten", ZomeCallback, new { number = 10 });

            // Load testing
            //   for (int i = 0; i < 100; i++)
            //     await _holoNETClient.CallZomeFunctionAsync("1", "numbers", "add_ten", ZomeCallback, new { number = 10 });

            //  await _holoNETClient.Disconnect();
        }

        private static void HoloNETClient_OnAppInfoCallBack(object sender, AppInfoCallBackEventArgs e)
        {
            //Console.WriteLine(string.Concat("TEST HARNESS: APPINFO CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id, ", AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash, ", Installed App Id: ", e.InstalledAppId, ", Raw Binary Data: ",  e.RawBinaryData, ", Raw JSON Data: ", e.RawJSONData), LogType.Info);
            //Console.WriteLine(string.Concat("TEST HARNESS: APPINFO CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id, ", AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash, ", Installed App Id: ", e.InstalledAppId, ", Raw Binary Data: ", e.RawBinaryData, $", Raw JSON Data: {e.RawJSONData}"), LogType.Info);
            //Console.WriteLine($"TEST HARNESS: APPINFO CALLBACK EVENT HANDLER: EndPoint: { e.EndPoint}, Id: {e.Id}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, Installed App Id: {e.InstalledAppId}, Raw Binary Data: {e.RawBinaryData}, Raw JSON Data: {e.RawJSONData}", LogType.Info);
            string msg = $"TEST HARNESS: APPINFO CALLBACK EVENT HANDLER: EndPoint: { e.EndPoint}, Id: {e.Id}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, Installed App Id: {e.InstalledAppId}, Raw Binary Data: {e.RawBinaryData}";
            //msg = msg + ", Raw JSON Data: " + e.RawJSONData;
           // msg = string.Concat(msg, ", Raw JSON Data: ", e.RawJSONData);
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
            Console.ReadKey();
        }

        private static void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: ZOME FUNCTION CALLBACK EVENT HANDLER: ", ProcessZomeFunctionCallBackEventArgs(e)));
            Console.WriteLine("");

            _holoNETClient.Disconnect();
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string zomeData = "";

            foreach (string key in args.ZomeReturnData.Keys)
                zomeData = string.Concat(zomeData, key, "=", args.ZomeReturnData[key], "\n");

            //return string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nInstance: ", args.Instance, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nRaw Data: ", args.RawData, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Daya: ", args.RawBinaryData, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\n\nProcessed Zome Return Data:\n", zomeData);
            return string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nRaw Data: ", args.RawData, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Daya: ", args.RawBinaryData, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\n\nProcessed Zome Return Data:\n", zomeData);
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
