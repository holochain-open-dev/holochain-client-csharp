using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract class HoloNETEntryBaseClass //: IDisposable
    {
        private Dictionary<string, string> _holochainProperties = new Dictionary<string, string>();
        private bool _disposeOfHoloNETClient = false;

        public HoloNETClient HoloNETClient { get; set; }

        public bool StoreEntryHashInEntry { get; set; } = true;

        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, bool storeEntryHashInEntry = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            HoloNETClient = new HoloNETClient(holochainConductorURI, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour);
            _disposeOfHoloNETClient = true;

            StoreEntryHashInEntry = storeEntryHashInEntry;
            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            Init();
        }

        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETConfig holoNETConfig, bool storeEntryHashInEntry = true)
        {
            HoloNETClient = new HoloNETClient();
            _disposeOfHoloNETClient = true;

            StoreEntryHashInEntry = storeEntryHashInEntry;
            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            if (holoNETConfig != null)
                HoloNETClient.Config = holoNETConfig;

            Init();
        }

        public HoloNETEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETClient holoNETClient, bool storeEntryHashInEntry = true)
        {
            HoloNETClient = holoNETClient;
            StoreEntryHashInEntry = storeEntryHashInEntry;
            ZomeName = zomeName;
            ZomeLoadEntryFunction = zomeLoadEntryFunction;
            ZomeCreateEntryFunction = zomeCreateEntryFunction;
            ZomeUpdateEntryFunction = zomeUpdateEntryFunction;
            ZomeDeleteEntryFunction = zomeDeleteEntryFunction;

            Init();
        }

        public EntryData EntryData { get; set; }

        [HolochainPropertyName("entry_hash")]
        public string EntryHash { get; set; }

        //[HolochainPropertyName("previous_version_entry_hash")]
        public string PreviousVersionEntryHash { get; set; }

        [HolochainPropertyName("version")]
        public int Version { get; set; } = 1;

        public string ZomeName { get; set; }
        public string ZomeLoadEntryFunction { get; set; }
        public string ZomeCreateEntryFunction { get; set; }
        public string ZomeUpdateEntryFunction { get; set; }
        public string ZomeDeleteEntryFunction { get; set; }
        public List<HoloNETAuditEntry> AuditEntries { get; set; } = new List<HoloNETAuditEntry>();

        public async Task<ZomeFunctionCallBackEventArgs> LoadAsync(string entryHash)
        {
            ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeLoadEntryFunction, EntryHash);
            ProcessZomeReturnCall(result);
            return result;
        }

        public ZomeFunctionCallBackEventArgs Load(string entryHash)
        {
            return LoadAsync(entryHash).Result;
        }

        public async Task<ZomeFunctionCallBackEventArgs> LoadAsync()
        {
            return await LoadAsync(EntryHash);
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
        public virtual async Task<ZomeFunctionCallBackEventArgs> SaveAsync()
        {
            dynamic paramsObject = new ExpandoObject();
            dynamic updateParamsObject = new ExpandoObject();
            //object paramsObject = new object();
            Dictionary<string, object> zomeCallProps = new Dictionary<string, object>();
            PropertyInfo[] props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            bool update = false;

           // PreviousVersionEntryHash = EntryHash;

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

                                if (key != "entry_hash")
                                {
                                    if (propInfo.PropertyType == typeof(Guid))
                                        AddProperty(paramsObject, key, value.ToString());

                                    else if (propInfo.PropertyType == typeof(DateTime))
                                        AddProperty(paramsObject, key, value.ToString());

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
                                        AddProperty(paramsObject, key, value);
                                }
                                else if (StoreEntryHashInEntry && key == "entry_hash")
                                {
                                    if (value == null)
                                        AddProperty(paramsObject, key, "");
                                    else
                                        AddProperty(paramsObject, key, value);
                                }
                                
                                if (key == "entry_hash" && value != null)
                                {
                                    //Is an update so we need to include the action_hash for the rust HDK to be able to update the entry...
                                    AddProperty(updateParamsObject, "original_action_hash", HoloNETClient.ConvertHoloHashToBytes(value.ToString()));
                                    update = true;
                                }

                                //else if (key == "entry_hash" && value != null)
                                //{
                                //    //Is an update so we need to include the action_hash for the rust HDK to be able to update the entry...
                                //    AddProperty(paramsObject, "action_hash", HoloNETClient.ConvertHoloHashToBytes(value.ToString()));
                                //}
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

            if (update)
            {
                this.Version++;
                AddProperty(updateParamsObject, "updated_entry", paramsObject);
                AddProperty(updateParamsObject, "version", this.Version);
                //AddProperty(updateParamsObject, "version", this.PreviousVersionEntryHash);

                return await SaveAsync(updateParamsObject);
            }
            else
            {
                AddProperty(paramsObject, "version", this.Version);
                return await SaveAsync(paramsObject);
            }
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
        public virtual async Task<ZomeFunctionCallBackEventArgs> SaveAsync(dynamic paramsObject)
        {
            ZomeFunctionCallBackEventArgs result = null;

            if (string.IsNullOrEmpty(EntryHash))
                result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeCreateEntryFunction, paramsObject);
            else
                result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntryFunction, paramsObject);

            ProcessZomeReturnCall(result);

            return result;
        }

        public ZomeFunctionCallBackEventArgs Save(dynamic paramsObject)
        {
            return SaveAsync(paramsObject).Result;
        }

        public virtual async Task<ZomeFunctionCallBackEventArgs> DeleteAsync()
        {
            return await DeleteAsync(this.EntryHash);
        }

        public virtual async Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash)
        {
            ZomeFunctionCallBackEventArgs result = null;

            if (!string.IsNullOrEmpty(EntryHash))
                result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeDeleteEntryFunction, entryHash);
            else
                result = new ZomeFunctionCallBackEventArgs() { IsError = true, Message = "EntryHash is null, please set before calling this function." };

            return result;
        }

        public virtual ZomeFunctionCallBackEventArgs Delete()
        {
            return DeleteAsync().Result;
        }

        public virtual ZomeFunctionCallBackEventArgs Delete(string entryHash)
        {
            return DeleteAsync(entryHash).Result;
        }

        //public void Dispose()
        //{
        //    if (HoloNETClient != null && _disposeOfHoloNETClient)
        //    {
        //        HoloNETClient.Dispose();

        //        if (HoloNETClient.WebSocket.State == System.Net.WebSockets.WebSocketState.Closed)
        //        {

        //        }
        //        else
        //        {

        //        }

        //        HoloNETClient = null;
        //    }
        //}

        /// <summary>
        /// Will close this HoloNET Entry and then shutdown its internal HoloNET instance (if one was not passed in) and its current connetion to the Holochain Conductor.
        /// You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events to notify the caller.
        /// It will also shutdown the current running Holochain Conductor or all conductors depending on the config/params passed in.
        /// </summary>
        /// <param name="disconnectedCallBackMode"></param>
        /// <param name="shutdownHolochainConductorsMode"></param>
        /// <returns></returns>
        public async Task CloseAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            if (HoloNETClient != null && _disposeOfHoloNETClient)
            {
                await HoloNETClient.ShutdownHoloNETAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);

                if (HoloNETClient.WebSocket.State == System.Net.WebSockets.WebSocketState.Closed)
                {

                }
                else
                {

                }

                HoloNETClient = null;
            }
        }

        /// <summary>
        /// Will close this HoloNET Entry and then shutdown its internal HoloNET instance (if one was not passed in) and its current connetion to the Holochain Conductor.
        /// Unlike the async version, this non async version will not wait until HoloNET disconnects & shutsdown any Holochain Conductors before it returns to the caller. It will later raise the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events. If you wish to wait for HoloNET to disconnect and shutdown the conductors(s) before returning then please use CloseAsync instead.
        /// It will also shutdown the current running Holochain Conductor or all conductors depending on the config/params passed in.
        /// </summary>
        /// <param name="shutdownHolochainConductorsMode"></param>
        public void Close(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            if (HoloNETClient != null && _disposeOfHoloNETClient)
            {
                HoloNETClient.ShutdownHoloNET(shutdownHolochainConductorsMode);

                if (HoloNETClient.WebSocket.State == System.Net.WebSockets.WebSocketState.Closed)
                {

                }
                else
                {

                }

                HoloNETClient = null;
            }
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
            //if ((e.ZomeFunction == ZomeCreateEntryFunction || e.ZomeFunction == ZomeUpdateEntryFunction) && !string.IsNullOrEmpty(e.ZomeReturnHash))
            //    this.EntryHash = e.ZomeReturnHash;

            //if (e.KeyValuePair != null)
            //    HoloNETClient.MapEntryDataObject(this, e.KeyValuePair);
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

        private void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var exDict = expando as IDictionary<string, object>;
            if (exDict.ContainsKey(propertyName))
                exDict[propertyName] = propertyValue;
            else
                exDict.Add(propertyName, propertyValue);
        }

        private void ProcessZomeReturnCall(ZomeFunctionCallBackEventArgs result)
        {
            if (!result.IsError && result.IsCallSuccessful)
            {
                if (result.Entry != null)
                {
                    this.EntryData = result.Entry;

                    if (!string.IsNullOrEmpty(result.Entry.PreviousHash))
                        this.PreviousVersionEntryHash = result.Entry.PreviousHash;
                }

                if (!string.IsNullOrEmpty(result.ZomeReturnHash))
                {
                    if (!string.IsNullOrEmpty(this.EntryHash))
                        this.PreviousVersionEntryHash = this.EntryHash;

                    this.EntryHash = result.ZomeReturnHash;

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
                }

                if (result.KeyValuePair != null)
                {
                    if (result.KeyValuePair.ContainsKey("entry_hash") && string.IsNullOrEmpty(result.KeyValuePair["entry_hash"]))
                        result.KeyValuePair.Remove("entry_hash");

                    HoloNETClient.MapEntryDataObject(this, result.KeyValuePair);
                }
            }
        }
    }
}
