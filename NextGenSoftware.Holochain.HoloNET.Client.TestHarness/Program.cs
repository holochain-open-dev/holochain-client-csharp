using System;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client.Core;
using NextGenSoftware.Holochain.HoloNET.Client.Desktop;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    class Program
    {
        private static HoloNETClient _holoNETClient = null;

        static async Task Main(string[] args)
        {
            Console.WriteLine("NextGenSoftware.Holochain.HoloNET.Client Test Harness v1.4");
            Console.WriteLine("");
            await TestHoloNETClient();
            Console.ReadKey();
        }

        private static async Task TestHoloNETClient()
        {
            _holoNETClient = new HoloNETClient("ws://localhost:8888", HolochainVersion.RSM);

            // holoNETClient.HolochainVersion = HolochainVersion.RSM;
            _holoNETClient.WebSocket.Config.NeverTimeOut = true;
            //holoNETClient.Config.ErrorHandlingBehaviour = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent
            _holoNETClient.Config.AutoStartConductor = false;
            _holoNETClient.Config.AutoShutdownConductor = false;
            //holoNETClient.Config.FullPathToHolochainAppDNA = @"D:\Dropbox\Our World\OASIS API\NextGenSoftware.Holochain.hApp.OurWorld\our_world\dist\our_world.dna.json";
            _holoNETClient.Config.FullPathToHapp = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop");

            _holoNETClient.OnConnected += HoloNETClient_OnConnected;
            _holoNETClient.OnDataReceived += HoloNETClient_OnDataReceived;
            _holoNETClient.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;
            _holoNETClient.OnAppInfoCallBack += HoloNETClient_OnAppInfoCallBack;
            _holoNETClient.OnReadyForZomeCalls += _holoNETClient_OnReadyForZomeCalls;
            _holoNETClient.OnSignalsCallBack += HoloNETClient_OnSignalsCallBack;
            _holoNETClient.OnDisconnected += HoloNETClient_OnDisconnected;
            _holoNETClient.OnError += HoloNETClient_OnError;
            _holoNETClient.OnConductorDebugCallBack += HoloNETClient_OnConductorDebugCallBack;

            await _holoNETClient.Connect();
        }

        private static void _holoNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: READY FOR ZOME CALLS EVENT HANDLER: AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash));
            Console.WriteLine("");
            Console.WriteLine("Calling Test Zome...\n");

            //await holoNETClient.CallZomeFunctionAsync("1", "test-instance", "our_world_core", "test", ZomeCallback, null);
            _holoNETClient.CallZomeFunctionAsync("1", "test-instance", "whoami", "whoami", ZomeCallback, null);
            //await holoNETClient.CallZomeFunctionAsync("1", "test-instance", "numbers", "add_ten", ZomeCallback, new { number = 10 });

            //await holoNETClient.CallZomeFunctionAsync("1", "test-instance", "our_world_core", "test", ZomeCallback, new { message = new { content = "blah!" } });
            //await holoNETClient.CallZomeFunctionAsync("2", "test-instance", "our_world_core", "test2", ZomeCallback, new { _message = "blah!" });

            // await holoNETClient.CallZomeFunctionAsync("2", "test-instance", "our_world_core", "save_Avatar", ZomeCallback, new { address = "" });
            //await holoNETClient.CallZomeFunctionAsync("2", "test-instance", "our_world_core", "load_Avatar", ZomeCallback, new { address = "" });

            // Load testing
            //   for (int i = 0; i < 100; i++)
            //     await holoNETClient.CallZomeFunctionAsync(i.ToString(), "test-instance", "our_world_core", "test", ZomeCallback, new { message = new { content = "blah!" } });

            //  await holoNETClient.Disconnect();
        }

        private static void HoloNETClient_OnAppInfoCallBack(object sender, AppInfoCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: APPINFO CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id, ", AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash, ", Installed App Id: ", e.InstalledAppId, ", Raw Binary Data: ",  e.RawBinaryData, ", Raw JSON Data: ", e.RawJSONData), LogType.Info);
            Console.WriteLine("");
        }

        private static void HoloNETClient_OnConductorDebugCallBack(object sender, ConductorDebugCallBackEventArgs e)
        {
          //  Console.WriteLine(string.Concat("OnConductorDebugCallBack: EndPoint: ", e.EndPoint, ", Data: ", e.RawJSONData, ", NumberDelayedValidations: ", e.NumberDelayedValidations, ", NumberHeldAspects: ", e.NumberHeldAspects, ", NumberHeldEntries: ", e.NumberHeldEntries, ", NumberPendingValidations: ", e.NumberPendingValidations, ", NumberRunningZomeCalls: ", e.NumberRunningZomeCalls, ", Offline: ", e.Offline, ", Type: ", e.Type));
          //  Console.WriteLine("");
        }

        private static void HoloNETClient_OnSignalsCallBack(object sender, SignalsCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: SIGINALS CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id , ", Data: ", e.RawJSONData, "Name: ", e.Name, "SignalType: ", Enum.GetName(typeof(SignalsCallBackEventArgs.SignalTypes), e.SignalType), "Arguments: ", e.Arguments));
            Console.WriteLine("");
        }

        private static void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: ERROR EVENT HANDLER: Error Occured. Resason: ", e.Reason, ", EndPoint: ", e.EndPoint));
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
        }

        private static void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: ZOME FUNCTION CALLBACK EVENT HANDLER: ", ProcessZomeFunctionCallBackEventArgs(e)));
            Console.WriteLine("");
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string zomeData = "";

            foreach (string key in args.ZomeReturnData.Keys)
                zomeData = string.Concat(zomeData, key, "=", args.ZomeReturnData[key], "\n");

            return string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nInstance: ", args.Instance, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nRaw Data: ", args.RawData, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Daya: ", args.RawBinaryData, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\n\nProcessed Zome Return Data:\n", zomeData);
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
