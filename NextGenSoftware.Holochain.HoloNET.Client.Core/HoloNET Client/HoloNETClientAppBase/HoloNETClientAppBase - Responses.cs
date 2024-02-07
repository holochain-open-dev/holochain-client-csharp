﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using MessagePack;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientAppBase : HoloNETClientBase
    {
        /// <summary>
        /// This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass.
        /// </summary>
        /// <param name="entryDataObjectType">The type of the data object to map the KeyValuePairs returned from the Holochain Conductor onto.</param>
        /// <param name="keyValuePairs">The KeyValuePairs returned from the Holochain Conductor (after they have been decoded by an internal function called `DecodeRawZomeData`) that will be mapped onto the data object.</param>
        /// <param name="cacheEntryDataObjectPropertyInfo">Set this to true if you want HoloNET to cache the property info for the Entry Data Object passed in (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public async Task<dynamic> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true)
        {
            return await MapEntryDataObjectAsync(Activator.CreateInstance(entryDataObjectType), keyValuePairs, cacheEntryDataObjectPropertyInfo);
        }

        /// <summary>
        /// This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass.
        /// </summary>
        /// <param name="entryDataObjectType">The type of the data object to map the KeyValuePairs returned from the Holochain Conductor onto.</param>
        /// <param name="keyValuePairs">The KeyValuePairs returned from the Holochain Conductor (after they have been decoded by an internal function called `DecodeRawZomeData`) that will be mapped onto the data object.</param>
        /// <param name="cacheEntryDataObjectPropertyInfo">Set this to true if you want HoloNET to cache the property info for the Entry Data Object passed in (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public dynamic MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfo = true)
        {
            return MapEntryDataObjectAsync(entryDataObjectType, keyValuePairs, cacheEntryDataObjectPropertyInfo).Result;
        }

        /// <summary>
        /// This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass.
        /// </summary>
        /// <param name="entryDataObject">The dynamic data object to map the KeyValuePairs returned from the Holochain Conductor onto.</param>
        /// <param name="keyValuePairs">The KeyValuePairs returned from the Holochain Conductor (after they have been decoded by an internal function called `DecodeRawZomeData`) that will be mapped onto the data object.</param>
        /// <param name="cacheEntryDataObjectPropertyInfos">Set this to true if you want HoloNET to cache the property info's for the Entry Data Object passed in (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public async Task<dynamic> MapEntryDataObjectAsync(dynamic entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true)
        {
            try
            {
                PropertyInfo[] props = null;

                if (keyValuePairs != null && entryDataObject != null)
                {
                    Type type = entryDataObject.GetType();
                    string typeKey = $"{type.AssemblyQualifiedName}.{type.FullName}";

                    if (cacheEntryDataObjectPropertyInfos && _dictPropertyInfos.ContainsKey(typeKey))
                        props = _dictPropertyInfos[typeKey];
                    else
                    {
                        //Cache the props to reduce overhead of reflection.
                        props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        if (cacheEntryDataObjectPropertyInfos)
                            _dictPropertyInfos[typeKey] = props;
                    }

                    foreach (PropertyInfo propInfo in props)
                    {
                        foreach (CustomAttributeData data in propInfo.CustomAttributes)
                        {
                            if (data.AttributeType == (typeof(HolochainRustFieldName)))
                            {
                                try
                                {
                                    if (data.ConstructorArguments.Count > 0 && data.ConstructorArguments[0] != null && data.ConstructorArguments[0].Value != null)
                                    {
                                        string key = data.ConstructorArguments[0].Value.ToString();

                                        if (propInfo.PropertyType == typeof(Guid))
                                            propInfo.SetValue(entryDataObject, new Guid(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(bool))
                                            propInfo.SetValue(entryDataObject, Convert.ToBoolean(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(DateTime))
                                            propInfo.SetValue(entryDataObject, Convert.ToDateTime(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(int))
                                            propInfo.SetValue(entryDataObject, Convert.ToInt32(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(long))
                                            propInfo.SetValue(entryDataObject, Convert.ToInt64(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(float))
                                            propInfo.SetValue(entryDataObject, Convert.ToDouble(keyValuePairs[key])); //TODO: Check if this is right?! :)

                                        else if (propInfo.PropertyType == typeof(double))
                                            propInfo.SetValue(entryDataObject, Convert.ToDouble(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(decimal))
                                            propInfo.SetValue(entryDataObject, Convert.ToDecimal(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(UInt16))
                                            propInfo.SetValue(entryDataObject, Convert.ToUInt16(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(UInt32))
                                            propInfo.SetValue(entryDataObject, Convert.ToUInt32(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(UInt64))
                                            propInfo.SetValue(entryDataObject, Convert.ToUInt64(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(Single))
                                            propInfo.SetValue(entryDataObject, Convert.ToSingle(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(char))
                                            propInfo.SetValue(entryDataObject, Convert.ToChar(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(byte))
                                            propInfo.SetValue(entryDataObject, Convert.ToByte(keyValuePairs[key]));

                                        else if (propInfo.PropertyType == typeof(sbyte))
                                            propInfo.SetValue(entryDataObject, Convert.ToSByte(keyValuePairs[key]));

                                        else
                                            propInfo.SetValue(entryDataObject, keyValuePairs[key]);

                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return entryDataObject;
        }

        /// <summary>
        /// This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the CallZomeFunction method. Alternatively the type of the data object can be passed in, for which an instance of it will be created. Either way the now mapped and populated data object is then returned in the `ZomeFunctionCallBackEventArgs.EntryData.EntryDataObject` property during the OnZomeFunctionCallBack event. Please see OnZomeFunctionCallBack for more info. This method is called internally but can also be called manually and is used by the HoloNETEntryBaseClass and HoloNETAuditEntryBaseClass.
        /// </summary>
        /// <param name="entryDataObject">The dynamic data object to map the KeyValuePairs returned from the Holochain Conductor onto.</param>
        /// <param name="keyValuePairs">The KeyValuePairs returned from the Holochain Conductor (after they have been decoded by an internal function called `DecodeRawZomeData`) that will be mapped onto the data object.</param>
        /// <param name="cacheEntryDataObjectPropertyInfos">Set this to true if you want HoloNET to cache the property info's for the Entry Data Object passed in (this can reduce the slight overhead used by reflection).</param>
        /// <returns></returns>
        public dynamic MapEntryDataObject(dynamic entryDataObject, Dictionary<string, string> keyValuePairs, bool cacheEntryDataObjectPropertyInfos = true)
        {
            return MapEntryDataObjectAsync(entryDataObject, keyValuePairs, cacheEntryDataObjectPropertyInfos).Result;
        }

        protected override HoloNETResponse ProcessDataReceived(WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            HoloNETResponse response = null;

            try
            {
                response = base.ProcessDataReceived(dataReceivedEventArgs);

                if (!response.IsError)
                {
                    switch (response.HoloNETResponseType)
                    {
                        case HoloNETResponseType.AppInfo:
                            DecodeAppInfoDataReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.Signal:
                            DecodeSignalDataReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.ZomeResponse:
                            DecodeZomeDataReceived(response, dataReceivedEventArgs);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in HoloNETClient.ProcessDataReceived method.";
                HandleError(msg, ex);
            }

            return response;
        }

        protected override string ProcessErrorReceivedFromConductor(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string msg = base.ProcessErrorReceivedFromConductor(response, dataReceivedEventArgs);

            if (response != null && response.type != null && response.type.ToUpper() == "SIGNAL")
                RaiseSignalReceivedEvent(ProcessResponeError<SignalCallBackEventArgs>(response, dataReceivedEventArgs, "Signal", msg));

            if (response != null && response.id > 0 && _requestTypeLookup != null && _requestTypeLookup.ContainsKey(response.id.ToString()))
            {
                switch (_requestTypeLookup[response.id.ToString()])
                {
                    case HoloNETRequestType.ZomeCall:
                        {
                            ZomeFunctionCallBackEventArgs args = ProcessResponeError<ZomeFunctionCallBackEventArgs>(response, dataReceivedEventArgs, "ZomeCall", msg);
                            args.Zome = GetItemFromCache(response != null ? response.id.ToString() : "", _zomeLookup);
                            args.ZomeFunction = GetItemFromCache(response != null ? response.id.ToString() : "", _funcLookup);
                            RaiseZomeDataReceivedEvent(args);
                        }
                        break;

                    case HoloNETRequestType.AppInfo:
                        RaiseAppInfoReceivedEvent(ProcessResponeError<AppInfoCallBackEventArgs>(response, dataReceivedEventArgs, "AppInfo", msg));
                        break;

                    case HoloNETRequestType.Signal:
                        RaiseSignalReceivedEvent(ProcessResponeError<SignalCallBackEventArgs>(response, dataReceivedEventArgs, "Signal", msg));
                        break;
                }
            }

            return msg;
        }

        private void DecodeAppInfoDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs();
            AppInfoResponse appInfoResponse = null;

            try
            {
                Logger.Log("APP INFO RESPONSE DATA DETECTED\n", LogType.Info);
                appInfoResponse = MessagePackSerializer.Deserialize<AppInfoResponse>(response.data, messagePackSerializerOptions);
                args = CreateHoloNETArgs<AppInfoCallBackEventArgs>(response, dataReceivedEventArgs);

                if (appInfoResponse != null)
                {
                    appInfoResponse.data = ProcessAppInfo(appInfoResponse.data, args);
                    args.AppInfoResponse = appInfoResponse;
                }
                else
                {
                    args.Message = "Error occured in HoloNETClient.DecodeAppInfoDataReceived. appInfoResponse is null.";
                    args.IsError = true;
                    //args.IsCallSuccessful = false;
                    HandleError(args.Message);
                }
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeAppInfoDataReceived. Reason: {ex}";
                args.IsError = true;
                //args.IsCallSuccessful = false;
                args.Message = msg;
                HandleError(msg, ex);
            }

            //If either the AgentPubKey or DnaHash is empty then attempt to get from the sandbox cmd.
            if (!args.IsError)
            {
                if (!string.IsNullOrEmpty(HoloNETDNA.AgentPubKey) && !string.IsNullOrEmpty(HoloNETDNA.DnaHash))
                    SetReadyForZomeCalls();

                else if (_automaticallyAttemptToGetFromSandboxIfConductorFails)
                    RetrieveAgentPubKeyAndDnaHashFromSandbox();
            }

            RaiseAppInfoReceivedEvent(args);
        }

        private void DecodeSignalDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            SignalCallBackEventArgs signalCallBackEventArgs = new SignalCallBackEventArgs();

            try
            {
                Logger.Log("SIGNAL DATA DETECTED\n", LogType.Info);

                SignalResponse appResponse = MessagePackSerializer.Deserialize<SignalResponse>(response.data, messagePackSerializerOptions);
                Dictionary<string, object> signalDataDecoded = new Dictionary<string, object>();
                SignalType signalType = SignalType.App;
                string agentPublicKey = "";
                string dnaHash = "";
                string signalDataAsString = "";

                if (appResponse != null)
                {
                    agentPublicKey = ConvertHoloHashToString(appResponse.App.CellData[0]);
                    dnaHash = ConvertHoloHashToString(appResponse.App.CellData[1]);
                    Dictionary<object, byte[]> signalData = MessagePackSerializer.Deserialize<Dictionary<object, byte[]>>(appResponse.App.Data, messagePackSerializerOptions);

                    foreach (object key in signalData.Keys)
                    {
                        signalDataDecoded[key.ToString()] = MessagePackSerializer.Deserialize<object>(signalData[key]);
                        signalDataAsString = string.Concat(signalDataAsString, key.ToString(), "=", signalDataDecoded[key.ToString()], ",");
                    }

                    signalDataAsString = signalDataAsString.Substring(0, signalDataAsString.Length - 1);
                }
                else
                    signalType = SignalType.System;

                signalCallBackEventArgs = CreateHoloNETArgs<SignalCallBackEventArgs>(response, dataReceivedEventArgs);
                signalCallBackEventArgs.DnaHash = dnaHash;
                signalCallBackEventArgs.AgentPubKey = agentPublicKey;
                signalCallBackEventArgs.RawSignalData = appResponse.App;
                signalCallBackEventArgs.SignalData = signalDataDecoded;
                signalCallBackEventArgs.SignalDataAsString = signalDataAsString;
                signalCallBackEventArgs.SignalType = signalType; //TODO: Need to test for System SignalType... Not even sure if we want to raise this event for System signals? (the js client ignores them currently).
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeSignalDataReceived. Reason: {ex}";
                signalCallBackEventArgs.IsError = true;
                //signalCallBackEventArgs.IsCallSuccessful = false;
                signalCallBackEventArgs.Message = msg;
                HandleError(msg, ex);
            }

            RaiseSignalReceivedEvent(signalCallBackEventArgs);
        }

        private void DecodeZomeDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            Logger.Log("ZOME RESPONSE DATA DETECTED\n", LogType.Info);
            AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);
            string id = response.id.ToString();
            ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs();

            try
            {
                Dictionary<object, object> rawAppResponseData = MessagePackSerializer.Deserialize<Dictionary<object, object>>(appResponse.data, messagePackSerializerOptions);
                Dictionary<string, object> appResponseData = new Dictionary<string, object>();
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                string keyValuePairsAsString = "";
                EntryData entryData = null;

                (appResponseData, keyValuePairs, keyValuePairsAsString, entryData) = DecodeRawZomeData(rawAppResponseData, appResponseData, keyValuePairs, keyValuePairsAsString);

                if (_entryDataObjectTypeLookup.ContainsKey(id) && _entryDataObjectTypeLookup[id] != null)
                    entryData.EntryDataObject = MapEntryDataObject(_entryDataObjectTypeLookup[id], keyValuePairs);

                else if (_entryDataObjectLookup.ContainsKey(id) && _entryDataObjectLookup[id] != null)
                    entryData.EntryDataObject = MapEntryDataObject(_entryDataObjectLookup[id], keyValuePairs);

                //if (entryData.EntryDataObject.GetType() == typeof(HoloNETEntryBase))
                //{
                //    entryData.EntryDataObject.OrginalEntry = entryData.EntryDataObject;
                //    entryData.EntryDataObject.OrginalDataKeyValuePairs = entryData.EntryKeyValuePairs;
                //    entryData.EntryDataObject.OrginalKeyValuePairs = keyValuePairs;
                //}

                Logger.Log($"Decoded Data:\n{keyValuePairsAsString}", LogType.Info);

                zomeFunctionCallBackArgs = CreateHoloNETArgs<ZomeFunctionCallBackEventArgs>(response, dataReceivedEventArgs);
                zomeFunctionCallBackArgs.Zome = GetItemFromCache(id, _zomeLookup);
                zomeFunctionCallBackArgs.ZomeFunction = GetItemFromCache(id, _funcLookup);
                zomeFunctionCallBackArgs.RawZomeReturnData = rawAppResponseData;
                zomeFunctionCallBackArgs.KeyValuePair = keyValuePairs;
                zomeFunctionCallBackArgs.KeyValuePairAsString = keyValuePairsAsString;
                zomeFunctionCallBackArgs.Entries[0] = entryData; //TODO: Need to add support for multiple entries ASAP!
            }
            catch (Exception ex)
            {
                try
                {
                    object rawAppResponseData = MessagePackSerializer.Deserialize<object>(appResponse.data, messagePackSerializerOptions);
                    byte[] holoHash = rawAppResponseData as byte[];

                    zomeFunctionCallBackArgs = CreateHoloNETArgs<ZomeFunctionCallBackEventArgs>(response, dataReceivedEventArgs);
                    zomeFunctionCallBackArgs.Zome = GetItemFromCache(id, _zomeLookup);
                    zomeFunctionCallBackArgs.ZomeFunction = GetItemFromCache(id, _funcLookup);

                    if (holoHash != null)
                    {
                        string hash = ConvertHoloHashToString(holoHash);
                        Logger.Log($"Decoded Data:\nHoloHash: {hash}", LogType.Info);
                        zomeFunctionCallBackArgs.ZomeReturnHash = hash;
                    }
                    else
                    {
                        string msg = $"An unknown response was received from the conductor for type 'Response' (Zome Response). Response Received: {rawAppResponseData}";

                        zomeFunctionCallBackArgs.IsError = true;
                        //zomeFunctionCallBackArgs.IsCallSuccessful = false;
                        zomeFunctionCallBackArgs.Message = msg;
                        HandleError(msg, null);
                    }
                }
                catch (Exception ex2)
                {
                    string msg = $"An unknown error occurred in HoloNETClient.DecodeZomeDataReceived. Reason: {ex2}";
                    zomeFunctionCallBackArgs.IsError = true;
                    //zomeFunctionCallBackArgs.IsCallSuccessful = false;
                    zomeFunctionCallBackArgs.Message = msg;
                    HandleError(msg, ex2);
                }
            }

            RaiseZomeDataReceivedEvent(zomeFunctionCallBackArgs);
        }

        private (Dictionary<string, object>, Dictionary<string, string> keyValuePair, string keyValuePairAsString, EntryData entry) DecodeRawZomeData(Dictionary<object, object> rawAppResponseData, Dictionary<string, object> appResponseData, Dictionary<string, string> keyValuePair, string keyValuePairAsString, EntryData entryData = null)
        {
            string value = "";
            var options = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

            if (entryData == null)
                entryData = new EntryData();

            try
            {
                foreach (string key in rawAppResponseData.Keys)
                {
                    try
                    {
                        value = "";
                        byte[] bytes = rawAppResponseData[key] as byte[];

                        if (bytes != null)
                        {
                            if (key == "entry")
                            {
                                string byteString = "";

                                for (int i = 0; i < bytes.Length; i++)
                                    byteString = string.Concat(byteString, bytes[i], ",");

                                byteString = byteString.Substring(0, byteString.Length - 1);

                                Dictionary<object, object> entry = MessagePackSerializer.Deserialize<Dictionary<object, object>>(bytes, options);
                                Dictionary<string, object> decodedEntry = new Dictionary<string, object>();

                                if (entry != null)
                                {
                                    foreach (object entryKey in entry.Keys)
                                    {
                                        decodedEntry[entryKey.ToString()] = entry[entryKey].ToString();
                                        keyValuePair[entryKey.ToString()] = entry[entryKey].ToString();
                                        keyValuePairAsString = string.Concat(keyValuePairAsString, entryKey.ToString(), "=", entry[entryKey].ToString(), "\n");
                                    }

                                    entryData.Bytes = bytes;
                                    entryData.BytesString = byteString;
                                    entryData.EntryKeyValuePairs = decodedEntry;
                                    appResponseData[key] = entryData;
                                }
                            }
                            else
                                value = ConvertHoloHashToString(bytes);
                        }
                        else
                        {
                            Dictionary<object, object> dict = rawAppResponseData[key] as Dictionary<object, object>;

                            if (dict != null)
                            {
                                Dictionary<string, object> tempDict = new Dictionary<string, object>();
                                (tempDict, keyValuePair, keyValuePairAsString, entryData) = DecodeRawZomeData(dict, tempDict, keyValuePair, keyValuePairAsString, entryData);
                                appResponseData[key] = tempDict;
                            }
                            else if (rawAppResponseData[key] != null)
                                value = rawAppResponseData[key].ToString();
                        }

                        if (!string.IsNullOrEmpty(value))
                        {
                            keyValuePairAsString = string.Concat(keyValuePairAsString, key, "=", value, "\n");
                            keyValuePair[key] = value;
                            appResponseData[key] = value;

                            try
                            {
                                switch (key)
                                {
                                    case "hash":
                                        entryData.Hash = value;
                                        break;

                                    case "entry_hash":
                                        entryData.EntryHash = value;
                                        break;

                                    case "prev_action":
                                        entryData.PreviousHash = value;
                                        break;

                                    case "signature":
                                        entryData.Signature = value;
                                        break;

                                    case "action_seq":
                                        entryData.ActionSequence = Convert.ToInt32(value);
                                        break;

                                    case "author":
                                        entryData.Author = value;
                                        break;

                                    case "original_action_address":
                                        entryData.OriginalActionAddress = value;
                                        break;

                                    case "original_entry_address":
                                        entryData.OriginalEntryAddress = value;
                                        break;

                                    case "timestamp":
                                        {
                                            entryData.Timestamp = Convert.ToInt64(value);
                                            long time = entryData.Timestamp / 1000; // Divide by 1,000 because we need milliseconds, not microseconds.
                                            entryData.DateTime = DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime.AddHours(-5).AddMinutes(1);
                                        }
                                        break;

                                    case "type":
                                        entryData.Type = value;
                                        break;

                                    case "entry_type":
                                        entryData.EntryType = value;
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                HandleError("Error in HoloNETClient.DecodeZomeReturnData method.", ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError("Error in HoloNETClient.DecodeZomeReturnData method.", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.DecodeZomeReturnData method.", ex);
            }

            return (appResponseData, keyValuePair, keyValuePairAsString, entryData);
        }

        private void RaiseSignalReceivedEvent(SignalCallBackEventArgs signalCallBackEventArgs)
        {
            LogEvent("SignalCallBack", signalCallBackEventArgs);
            Logger.Log(string.Concat("AgentPubKey: ", signalCallBackEventArgs.AgentPubKey, ", DnaHash: ", signalCallBackEventArgs.DnaHash, ", Signal Type: ", Enum.GetName(typeof(SignalType), signalCallBackEventArgs.SignalType), ", Signal Data: ", signalCallBackEventArgs.SignalDataAsString, "\n"), LogType.Info);
            OnSignalCallBack?.Invoke(this, signalCallBackEventArgs);
        }

        private void RaiseAppInfoReceivedEvent(AppInfoCallBackEventArgs appInfoCallBackEventArgs)
        {
            LogEvent("AppInfoCallBack", appInfoCallBackEventArgs, string.Concat("AgentPubKey: ", appInfoCallBackEventArgs.AgentPubKey, ", DnaHash: ", appInfoCallBackEventArgs.DnaHash, ", Installed App Id: ", appInfoCallBackEventArgs.InstalledAppId));
            OnAppInfoCallBack?.Invoke(this, appInfoCallBackEventArgs);
        }

        private void RaiseZomeDataReceivedEvent(ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs)
        {
            LogEvent("ZomeFunctionCallBack", zomeFunctionCallBackArgs, string.Concat("Zome: ", zomeFunctionCallBackArgs.Zome, ", Zome Function: ", zomeFunctionCallBackArgs.ZomeFunction, ", Raw Zome Return Data: ", zomeFunctionCallBackArgs.RawZomeReturnData, ", Zome Return Data: ", zomeFunctionCallBackArgs.ZomeReturnData, ", Zome Return Hash: ", zomeFunctionCallBackArgs.ZomeReturnHash));

            if (_callbackLookup.ContainsKey(zomeFunctionCallBackArgs.Id) && _callbackLookup[zomeFunctionCallBackArgs.Id] != null)
                _callbackLookup[zomeFunctionCallBackArgs.Id].DynamicInvoke(this, zomeFunctionCallBackArgs);

            if (_taskCompletionZomeCallBack.ContainsKey(zomeFunctionCallBackArgs.Id) && _taskCompletionZomeCallBack[zomeFunctionCallBackArgs.Id] != null)
                _taskCompletionZomeCallBack[zomeFunctionCallBackArgs.Id].SetResult(zomeFunctionCallBackArgs);

            OnZomeFunctionCallBack?.Invoke(this, zomeFunctionCallBackArgs);

            // If the zome call requested for this to be cached then stick it in the cache.
            if (_cacheZomeReturnDataLookup.ContainsKey(zomeFunctionCallBackArgs.Id) && _cacheZomeReturnDataLookup[zomeFunctionCallBackArgs.Id])
                _zomeReturnDataLookup[zomeFunctionCallBackArgs.Id] = zomeFunctionCallBackArgs;

            _zomeLookup.Remove(zomeFunctionCallBackArgs.Id);
            _funcLookup.Remove(zomeFunctionCallBackArgs.Id);
            _callbackLookup.Remove(zomeFunctionCallBackArgs.Id);
            _entryDataObjectTypeLookup.Remove(zomeFunctionCallBackArgs.Id);
            _entryDataObjectLookup.Remove(zomeFunctionCallBackArgs.Id);
            _cacheZomeReturnDataLookup.Remove(zomeFunctionCallBackArgs.Id);
            _taskCompletionZomeCallBack.Remove(zomeFunctionCallBackArgs.Id);
        }

        private void SetReadyForZomeCalls()
        {
            RetrievingAgentPubKeyAndDnaHash = false;
            _taskCompletionAgentPubKeyAndDnaHashRetrieved.SetResult(new AgentPubKeyDnaHash() { AgentPubKey = HoloNETDNA.AgentPubKey, DnaHash = HoloNETDNA.DnaHash });

            IsReadyForZomesCalls = true;
            ReadyForZomeCallsEventArgs eventArgs = new ReadyForZomeCallsEventArgs(EndPoint, HoloNETDNA.DnaHash, HoloNETDNA.AgentPubKey);
            OnReadyForZomeCalls?.Invoke(this, eventArgs);
            _taskCompletionReadyForZomeCalls.SetResult(eventArgs);
        }
    }
}