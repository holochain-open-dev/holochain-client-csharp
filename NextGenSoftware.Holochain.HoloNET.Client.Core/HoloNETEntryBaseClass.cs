using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract class HoloNETEntryBaseClass 
    {
        //private TaskCompletionSource<ZomeFunctionCallBackEventArgs> _taskCompletionZomeCallBack = new TaskCompletionSource<ZomeFunctionCallBackEventArgs>();
        private Dictionary<string, string> _holochainProperties = new Dictionary<string, string>();

        public HoloNETClient HoloNETClient { get; set; }

        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(holochainConductorURI, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour);

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            Init();
        }

        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETConfig holoNETConfig)
        {
            HoloNETClient = new HoloNETClient();

            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            Init();
        }

        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETClient holoNETClient)
        {
            HoloNETClient = holoNETClient;
            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            Init();
        }

        [HolochainPropertyName("entry_hash")]
        public string EntryHash { get; set; }

        public string ZomeName { get; set; }
        public string ZomeLoadEntryFunction { get; set; }
        //public string ZomeEntryFunction { get; set; }
        public string ZomeCreateEntryFunction { get; set; }
        public string ZomeUpdateEntryFunction { get; set; }
        public string ZomeDeleteEntryFunction { get; set; }

        public async Task<ZomeFunctionCallBackEventArgs> LoadAsync()
        {
            return await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeLoadEntryFunction, EntryHash);
            //return await _taskCompletionZomeCallBack.Task;
        }

        public ZomeFunctionCallBackEventArgs Load()
        {
            return LoadAsync().Result;
        }

        /// <summary>
        /// Saves the object and will automatically extrct the properties that need saving (contain the HolochainPropertyName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object.
        /// </summary>
        /// <param name="paramsObject"></param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> SaveAsync()
        {
            dynamic paramsObject = new ExpandoObject();
            PropertyInfo[] props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propInfo in props)
            {
                foreach (CustomAttributeData data in propInfo.CustomAttributes)
                {
                    if (data.AttributeType == (typeof(HolochainPropertyName)))
                    {
                        try
                        {
                            if (data.ConstructorArguments.Count > 0 && data.ConstructorArguments[0] != null && data.ConstructorArguments[0].Value != null)
                            {
                                string key = data.ConstructorArguments[0].Value.ToString();

                                //Only include the hash if it is not null.
                                if ((key == "entry_hash" && propInfo.GetValue(this) != null) || key != "entry_hash")
                                    paramsObject.key = propInfo.GetValue(this).ToString();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

            return await SaveAsync(paramsObject);
        }

        public ZomeFunctionCallBackEventArgs Save()
        {
            return SaveAsync().Result;
        }

        /// <summary>
        /// Saves the object using the params object passed in containing the hApp rust properties & their values to save. 
        /// </summary>
        /// <param name="paramsObject"></param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> SaveAsync(dynamic paramsObject)
        {
            if (string.IsNullOrEmpty(EntryHash))
                return await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeCreateEntryFunction, paramsObject);
            else
                return await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntryFunction, paramsObject);

            //return await _taskCompletionZomeCallBack.Task;
        }

        public ZomeFunctionCallBackEventArgs Save(dynamic paramsObject)
        {
            return SaveAsync(paramsObject).Result;
        }

        private void Init()
        {
            HoloNETClient.OnAppInfoCallBack += HoloNETClient_OnAppInfoCallBack;
            HoloNETClient.OnConductorDebugCallBack += HoloNETClient_OnConductorDebugCallBack;
            HoloNETClient.OnConnected += HoloNETClient_OnConnected;
            HoloNETClient.OnDataReceived += HoloNETClient_OnDataReceived;
            HoloNETClient.OnDisconnected += HoloNETClient_OnDisconnected;
            HoloNETClient.OnError += HoloNETClient_OnError;
            HoloNETClient.OnReadyForZomeCalls += HoloNETClient_OnReadyForZomeCalls;
            HoloNETClient.OnSignalsCallBack += HoloNETClient_OnSignalsCallBack;
            HoloNETClient.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;

            if (HoloNETClient.WebSocket.State != System.Net.WebSockets.WebSocketState.Connecting || HoloNETClient.WebSocket.State != System.Net.WebSockets.WebSocketState.Open)
                HoloNETClient.Connect();
        }

        private void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            if ((e.ZomeFunction == ZomeCreateEntryFunction || e.ZomeFunction == ZomeUpdateEntryFunction) && !string.IsNullOrEmpty(e.ZomeReturnHash))
                this.EntryHash = e.ZomeReturnHash;

            HoloNETClient.MapEntryDataObject(this, e.KeyValuePair);
            //_taskCompletionZomeCallBack.SetResult(e);
        }

        private void HoloNETClient_OnSignalsCallBack(object sender, SignalsCallBackEventArgs e)
        {
            
        }

        private void HoloNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            
        }

        private void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            
        }

        private void HoloNETClient_OnDisconnected(object sender, WebSocket.DisconnectedEventArgs e)
        {
            
        }

        private void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            
        }

        private void HoloNETClient_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            
        }

        private void HoloNETClient_OnConductorDebugCallBack(object sender, ConductorDebugCallBackEventArgs e)
        {
            
        }

        private void HoloNETClient_OnAppInfoCallBack(object sender, AppInfoCallBackEventArgs e)
        {
            
        }
    }
}
