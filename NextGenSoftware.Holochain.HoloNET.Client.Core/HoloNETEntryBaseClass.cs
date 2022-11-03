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
            //object paramsObject = new object();
            Dictionary<string, object> zomeCallProps = new Dictionary<string, object>();
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
                                object value = propInfo.GetValue(this);

                                //Only include the hash if it is not null.
                                if ((key == "entry_hash" && value != null) || key != "entry_hash")
                                {
                                    if (propInfo.PropertyType == typeof(Guid))
                                        propInfo.SetValue(paramsObject, value.ToString());

                                    else if (propInfo.PropertyType == typeof(DateTime))
                                        propInfo.SetValue(paramsObject, value.ToString());

                                    /*
                                    else if (propInfo.PropertyType == typeof(bool))
                                        propInfo.SetValue(paramsObject, Convert.ToBoolean(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(int))
                                        propInfo.SetValue(paramsObject, Convert.ToInt32(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(long))
                                        propInfo.SetValue(paramsObject, Convert.ToInt64(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(float))
                                        propInfo.SetValue(paramsObject, Convert.ToDouble(keyValuePairs[key])); //TODO: Check if this is right?! :)

                                    else if (propInfo.PropertyType == typeof(double))
                                        propInfo.SetValue(paramsObject, Convert.ToDouble(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(decimal))
                                        propInfo.SetValue(paramsObject, Convert.ToDecimal(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(UInt16))
                                        propInfo.SetValue(paramsObject, Convert.ToUInt16(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(UInt32))
                                        propInfo.SetValue(paramsObject, Convert.ToUInt32(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(UInt64))
                                        propInfo.SetValue(paramsObject, Convert.ToUInt64(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(Single))
                                        propInfo.SetValue(paramsObject, Convert.ToSingle(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(char))
                                        propInfo.SetValue(paramsObject, Convert.ToChar(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(byte))
                                        propInfo.SetValue(paramsObject, Convert.ToByte(keyValuePairs[key]));

                                    else if (propInfo.PropertyType == typeof(sbyte))
                                        propInfo.SetValue(paramsObject, Convert.ToSByte(keyValuePairs[key]));
                                    */

                                    else
                                        propInfo.SetValue(paramsObject, value);
                                }
                                    //AddProperty(paramsObject, key, value);
                                    //paramsObject.Add(key, value);
                                    //zomeCallProps[key] = value;
                                    //propInfo.SetValue(paramsObject, value);
                                    //paramsObject.key = propInfo.GetValue(this).ToString();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

            //dynamic zomeCallParams = new HoloNETEntryDynamicParams(zomeCallProps);
            //return await SaveAsync(zomeCallParams);

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

        public void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var exDict = expando as IDictionary<string, object>;
            if (exDict.ContainsKey(propertyName))
                exDict[propertyName] = propertyValue;
            else
                exDict.Add(propertyName, propertyValue);
        }
    }
}
