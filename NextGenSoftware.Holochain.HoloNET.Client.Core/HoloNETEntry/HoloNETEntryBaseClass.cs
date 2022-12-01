﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using NextGenSoftware.Logging;
using NextGenSoftware.Utilities.ExtentionMethods;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract class HoloNETEntryBaseClass //: IDisposable
    {
        private Dictionary<string, string> _holochainProperties = new Dictionary<string, string>();
        private bool _disposeOfHoloNETClient = false;

        public HoloNETClient HoloNETClient { get; set; }

        public bool StoreEntryHashInEntry { get; set; } = true;

        public delegate void Error(object sender, HoloNETErrorEventArgs e);
        public event Error OnError;

        public delegate void Loaded(object sender, ZomeFunctionCallBackEventArgs e);
        public event Loaded OnLoaded;

        public delegate void Saved(object sender, ZomeFunctionCallBackEventArgs e);
        public event Saved OnSaved;

        public delegate void Deleted(object sender, ZomeFunctionCallBackEventArgs e);
        public event Deleted OnDeleted;

        public delegate void Closed(object sender, HoloNETShutdownEventArgs e);
        public event Closed OnClosed;

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

        /// <summary>
        /// Metadata for the Entry.
        /// </summary>
        public EntryData EntryData { get; set; }

        /// <summary>
        /// The Entry hash.
        /// </summary>
        //[HolochainPropertyName("entry_hash")]
        public string EntryHash { get; set; }

        //public string Author { get; set; }

        /// <summary>
        /// The previous entry hash.
        /// </summary>
        //[HolochainPropertyName("previous_version_entry_hash")]
        public string PreviousVersionEntryHash { get; set; }

        /// <summary>
        /// The current version of the entry.
        /// </summary>
        [HolochainPropertyName("version")]
        public int Version { get; set; } = 1;

        /// <summary>
        /// The name of the zome to call the respective ZomeLoadEntryFunction, ZomeCreateEntryFunction, ZomeUpdateEntryFunction & ZomeDeleteEntryFunction.
        /// </summary>
        public string ZomeName { get; set; }

        /// <summary>
        /// The name of the zome function to call to load the entry.
        /// </summary>
        public string ZomeLoadEntryFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to create the entry.
        /// </summary>
        public string ZomeCreateEntryFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to update the entry.
        /// </summary>
        public string ZomeUpdateEntryFunction { get; set; }

        /// <summary>
        /// The name of the zome function to call to delete the entry.
        /// </summary>
        public string ZomeDeleteEntryFunction { get; set; }

        /// <summary>
        /// List of all previous hashes along with the type and datetime.
        /// </summary>
        public List<HoloNETAuditEntry> AuditEntries { get; set; } = new List<HoloNETAuditEntry>();

        /// <summary>
        /// Load's the entry by calling the ZomeLoadEntryFunction.
        /// </summary>
        /// <param name="entryHash"></param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> LoadAsync(string entryHash)
        {
            try
            {
                ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeLoadEntryFunction, EntryHash);
                ProcessZomeReturnCall(result);
                return result;
            }
            catch (Exception ex)
            {
                string msg = "Unknown error occured in LoadAsync method.";
                HandleError(msg, ex);
                return new ZomeFunctionCallBackEventArgs() { IsError = true, Message = $"{msg}. Reason: {ex}" };
            }
        }

        /// <summary>
        /// Load's the entry by calling the ZomeLoadEntryFunction.
        /// </summary>
        /// <param name="entryHash"></param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs Load(string entryHash)
        {
            return LoadAsync(entryHash).Result;
        }

        /// <summary>
        /// Load's the entry by calling the ZomeLoadEntryFunction (it will use the EntryHash property to load from).
        /// </summary>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> LoadAsync()
        {
            return await LoadAsync(EntryHash);
        }

        /// <summary>
        /// Load's the entry by calling the ZomeLoadEntryFunction (it will use the EntryHash property to load from).
        /// </summary>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs Load()
        {
            return LoadAsync().Result;
        }

        /// <summary>
        /// Saves the object and will automatically extrct the properties that need saving (contain the HolochainPropertyName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> SaveAsync()
        {
            try
            {
                dynamic paramsObject = new ExpandoObject();
                dynamic updateParamsObject = new ExpandoObject();
                Dictionary<string, object> zomeCallProps = new Dictionary<string, object>();
                PropertyInfo[] props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                bool update = false;

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
                                            ExpandoObjectHelpers.AddProperty(paramsObject, key, value.ToString());

                                        else if (propInfo.PropertyType == typeof(DateTime))
                                            ExpandoObjectHelpers.AddProperty(paramsObject, key, value.ToString());

                                        else
                                            ExpandoObjectHelpers.AddProperty(paramsObject, key, value);
                                    }
                                    if (key == "entry_hash" && value != null)
                                    {
                                        //Is an update so we need to include the action_hash for the rust HDK to be able to update the entry...
                                        ExpandoObjectHelpers.AddProperty(updateParamsObject, "original_action_hash", HoloNETClient.ConvertHoloHashToBytes(value.ToString()));
                                        update = true;
                                    }
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
                    ExpandoObjectHelpers.AddProperty(updateParamsObject, "updated_entry", paramsObject);
                    ExpandoObjectHelpers.AddProperty(updateParamsObject, "version", this.Version);

                    return await SaveAsync(updateParamsObject);
                }
                else
                {
                    ExpandoObjectHelpers.AddProperty(paramsObject, "version", this.Version);
                    return await SaveAsync(paramsObject);
                }
            }
            catch (Exception ex)
            {
                string msg = "Unknown error occured in SaveAsync method.";
                HandleError(msg, ex);
                return new ZomeFunctionCallBackEventArgs() { IsError = true, Message = $"{msg}. Reason: {ex}" };
            }
        }

        /// <summary>
        /// Saves the object and will automatically extrct the properties that need saving (contain the HolochainPropertyName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object.
        /// </summary>
        /// <returns></returns>
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

            try
            {
                if (string.IsNullOrEmpty(EntryHash))
                    result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeCreateEntryFunction, paramsObject);
                else
                    result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeUpdateEntryFunction, paramsObject);

                ProcessZomeReturnCall(result);
            }
            catch (Exception ex)
            {
                HandleError("Unknown error occured in SaveAsync method.", ex);
            }

            return result;
        }

        /// <summary>
        /// Saves the object using the params object passed in containing the hApp rust properties & their values to save. 
        /// </summary>
        /// <param name="paramsObject"></param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs Save(dynamic paramsObject)
        {
            return SaveAsync(paramsObject).Result;
        }

        /// <summary>
        /// Soft delete's the entry (the previous version can still be retreived). Uses the EntryHash property.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> DeleteAsync()
        {
            return await DeleteAsync(this.EntryHash);
        }

        /// <summary>
        /// Soft delete's the entry (the previous version can still be retreived).
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash)
        {
            ZomeFunctionCallBackEventArgs result = null;

            try
            {
                if (!string.IsNullOrEmpty(EntryHash))
                    result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeDeleteEntryFunction, entryHash);
                else
                    result = new ZomeFunctionCallBackEventArgs() { IsError = true, Message = "EntryHash is null, please set before calling this function." };
            }
            catch (Exception ex)
            {
                HandleError("Unknown error occured in DeleteAsync method.", ex);
            }

            return result;
        }

        /// <summary>
        /// Soft delete's the entry (the previous version can still be retreived).
        /// </summary>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Delete()
        {
            return DeleteAsync().Result;
        }

        /// <summary>
        /// Soft delete's the entry (the previous version can still be retreived).
        /// </summary>
        /// <returns></returns>
        public virtual ZomeFunctionCallBackEventArgs Delete(string entryHash)
        {
            return DeleteAsync(entryHash).Result;
        }

        /// <summary>
        /// Will close this HoloNET Entry and then shutdown its internal HoloNET instance (if one was not passed in) and its current connetion to the Holochain Conductor.
        /// You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events to notify the caller.
        /// It will also shutdown the current running Holochain Conductor or all conductors depending on the config/params passed in.
        /// </summary>
        /// <param name="disconnectedCallBackMode"></param>
        /// <param name="shutdownHolochainConductorsMode"></param>
        /// <returns></returns>
        public async Task<HoloNETShutdownEventArgs> CloseAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            HoloNETShutdownEventArgs returnValue = null;

            try
            {
                if (HoloNETClient != null && _disposeOfHoloNETClient)
                {
                    returnValue = await HoloNETClient.ShutdownHoloNETAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);
                    HoloNETClient = null;
                }

                OnClosed?.Invoke(this, returnValue);
            }
            catch (Exception ex)
            {
                //returnValue = HandleError<HoloNETShutdownEventArgs>("Unknown error occured in CloseAsync method.", ex);

               // returnValue = new HoloNETShutdownEventArgs() { IsError = true, }
            }

            return returnValue;
        }

        /// <summary>
        /// Will close this HoloNET Entry and then shutdown its internal HoloNET instance (if one was not passed in) and its current connetion to the Holochain Conductor.
        /// Unlike the async version, this non async version will not wait until HoloNET disconnects & shutsdown any Holochain Conductors before it returns to the caller. It will later raise the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events. If you wish to wait for HoloNET to disconnect and shutdown the conductors(s) before returning then please use CloseAsync instead.
        /// It will also shutdown the current running Holochain Conductor or all conductors depending on the config/params passed in.
        /// </summary>
        /// <param name="shutdownHolochainConductorsMode"></param>
        public void Close(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            try
            {
                if (HoloNETClient != null && _disposeOfHoloNETClient)
                {
                    HoloNETClient.ShutdownHoloNET(shutdownHolochainConductorsMode);
                    HoloNETClient = null;
                }
            }
            catch (Exception ex)
            {
                HandleError("Unknown error occured in Close method.", ex);
            }
        }

        private void Init()
        {
            try
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
            catch (Exception ex)
            {
                HandleError("Unknown error occured in Init method.", ex);
            }
        }

        private void ProcessZomeReturnCall(ZomeFunctionCallBackEventArgs result)
        {
            try
            {
                if (!result.IsError && result.IsCallSuccessful)
                {
                    //Load
                    if (result.Entry != null)
                    {
                        this.EntryData = result.Entry;

                        if (!string.IsNullOrEmpty(result.Entry.EntryHash))
                            this.EntryHash = result.Entry.EntryHash;

                        if (!string.IsNullOrEmpty(result.Entry.PreviousHash))
                            this.PreviousVersionEntryHash = result.Entry.PreviousHash;

                        //if (!string.IsNullOrEmpty(result.Entry.Author))
                        //    this.Author = result.Entry.Author;
                    }

                    //Create/Updates/Delete
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
                        //if (result.KeyValuePair.ContainsKey("entry_hash") && string.IsNullOrEmpty(result.KeyValuePair["entry_hash"]))
                        //    result.KeyValuePair.Remove("entry_hash");

                        HoloNETClient.MapEntryDataObject(this, result.KeyValuePair);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Unknown error occured in ProcessZomeReturnCall method.", ex);
            }
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

        private T HandleError<T>(string message, Exception exception) where T : CallBackBaseEventArgs, new()
        {
            message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "");
            Logger.Log(message, LogType.Error);

            OnError?.Invoke(this, new HoloNETErrorEventArgs { EndPoint = HoloNETClient.WebSocket.EndPoint, Reason = message, ErrorDetails = exception });
            T returnValue = new T() { IsError = true, Message = message };

            switch (HoloNETClient.Config.ErrorHandlingBehaviour)
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

            return returnValue;
        }
    }
}
