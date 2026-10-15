using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using MessagePack;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientAppBase : HoloNETClientBase, IHoloNETClientAppBase
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
            //return Task.Run(() => MapEntryDataObjectAsync(entryDataObjectType, keyValuePairs, cacheEntryDataObjectPropertyInfo)).Result; // .Result wraps errors in AggregateException
            return Task.Run(() => MapEntryDataObjectAsync(entryDataObjectType, keyValuePairs, cacheEntryDataObjectPropertyInfo)).GetAwaiter().GetResult();
        }

        /// <summary>Formats a decoded msgpack value with the invariant culture ("null" for null), so ConvertEntryValue round-trips it.</summary>
        public static string ToInvariantString(object value) =>
            value == null ? "null" : value is IFormattable formattable ? formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture) : value.ToString();

        /// <summary>
        /// Converts a decoded entry value (see DecodeRawZomeData, which formats with the invariant culture) to
        /// <paramref name="type"/>: nullable types, enums (by name or number), Guid, DateTime / DateTimeOffset and every
        /// IConvertible type. Unconvertible values throw, and MapEntryDataObjectAsync skips that property.
        /// </summary>
        public static object ConvertEntryValue(string value, Type type)
        {
            Type target = Nullable.GetUnderlyingType(type) ?? type;

            if (value == null || (value == "null" && target != typeof(string)))
                return type.IsValueType && Nullable.GetUnderlyingType(type) == null ? Activator.CreateInstance(type) : null;

            if (target == typeof(string))
                return value;

            if (target == typeof(Guid))
                return Guid.Parse(value);

            if (target == typeof(DateTime))
                return DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);

            if (target == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

            if (target.IsEnum)
                return Enum.Parse(target, value, true);

            return Convert.ChangeType(value, target, System.Globalization.CultureInfo.InvariantCulture);
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

                                        // The chain above set float properties from a double (rejected by reflection), and failed for nullable
                                        // and enum properties and with non-English number formats; every failure was silently skipped.
//                                        if (propInfo.PropertyType == typeof(Guid))
//                                            propInfo.SetValue(entryDataObject, new Guid(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(bool))
//                                            propInfo.SetValue(entryDataObject, Convert.ToBoolean(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(DateTime))
//                                            propInfo.SetValue(entryDataObject, Convert.ToDateTime(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(int))
//                                            propInfo.SetValue(entryDataObject, Convert.ToInt32(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(long))
//                                            propInfo.SetValue(entryDataObject, Convert.ToInt64(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(float))
//                                            propInfo.SetValue(entryDataObject, Convert.ToDouble(keyValuePairs[key])); //TODO: Check if this is right?! :)

//                                        else if (propInfo.PropertyType == typeof(double))
//                                            propInfo.SetValue(entryDataObject, Convert.ToDouble(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(decimal))
//                                            propInfo.SetValue(entryDataObject, Convert.ToDecimal(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(UInt16))
//                                            propInfo.SetValue(entryDataObject, Convert.ToUInt16(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(UInt32))
//                                            propInfo.SetValue(entryDataObject, Convert.ToUInt32(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(UInt64))
//                                            propInfo.SetValue(entryDataObject, Convert.ToUInt64(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(Single))
//                                            propInfo.SetValue(entryDataObject, Convert.ToSingle(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(char))
//                                            propInfo.SetValue(entryDataObject, Convert.ToChar(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(byte))
//                                            propInfo.SetValue(entryDataObject, Convert.ToByte(keyValuePairs[key]));

//                                        else if (propInfo.PropertyType == typeof(sbyte))
//                                            propInfo.SetValue(entryDataObject, Convert.ToSByte(keyValuePairs[key]));

//                                        else
//                                            propInfo.SetValue(entryDataObject, keyValuePairs[key]);
                                        if (keyValuePairs.TryGetValue(key, out string rawValue))
                                            propInfo.SetValue(entryDataObject, ConvertEntryValue(rawValue, propInfo.PropertyType));

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
            //return Task.Run(() => MapEntryDataObjectAsync(entryDataObject, keyValuePairs, cacheEntryDataObjectPropertyInfos)).Result; // .Result wraps errors in AggregateException
            //return Task.Run(() => MapEntryDataObjectAsync(entryDataObject, keyValuePairs, cacheEntryDataObjectPropertyInfos)).GetAwaiter().GetResult();
            // With a dynamic argument the lambda returned the Task itself, so the "mapped object" was a Task<object>.
            object entry = entryDataObject;
            return Task.Run(() => (Task<dynamic>)MapEntryDataObjectAsync(entry, keyValuePairs, cacheEntryDataObjectPropertyInfos)).GetAwaiter().GetResult();
        }

        protected override IHoloNETResponse ProcessDataReceived(WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            IHoloNETResponse response = null;

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

                        case HoloNETResponseType.AppCloneCellCreated:
                            DecodeCloneCellCreatedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppCloneCellEnabled:
                            DecodeCloneCellEnabledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppCloneCellDisabled:
                            DecodeCloneCellDisabledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppCountersigningSessionStateReturned:
                            DecodeCountersigningSessionStateReturnedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppCountersigningSessionAbandoned:
                            DecodeCountersigningSessionAbandonedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppPublishCountersigningSessionTriggered:
                            DecodePublishCountersigningSessionTriggeredReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppWasmHostFunctionsListed:
                            DecodeWasmHostFunctionsListedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppAgentInfoReturned:
                            {
                                var args = DecodeUnit<AgentInfoReturnedCallBackEventArgs>(response, dataReceivedEventArgs, HoloNETResponseType.AppAgentInfoReturned, "APP: AGENT INFO RETURNED");
                                try { args.AgentInfos = DeserializeResponseValue<List<string>>(response.data) ?? new List<string>(); }
                                catch (Exception ex) { HandleError(args, $"An unknown error occurred decoding app agent_info. Reason: {ex}"); }
                                RaiseAppEvent(args, OnAgentInfoReturnedCallBack, _taskCompletionAgentInfoReturnedCallBack, "AppAgentInfoReturned");
                            }
                            break;

                        case HoloNETResponseType.AppNetworkMetricsDumped:
                            {
                                var args = DecodeUnit<NetworkMetricsDumpedCallBackEventArgs>(response, dataReceivedEventArgs, HoloNETResponseType.AppNetworkMetricsDumped, "APP: NETWORK METRICS DUMPED");
                                try { args.NetworkMetricsDumpJSON = DeserializeResponseValueAsJson(response.data); }
                                catch (Exception ex) { HandleError(args, $"An unknown error occurred decoding app network metrics. Reason: {ex}"); }
                                RaiseAppEvent(args, OnNetworkMetricsDumpedCallBack, _taskCompletionNetworkMetricsDumpedCallBack, "AppNetworkMetricsDumped");
                            }
                            break;

                        case HoloNETResponseType.AppNetworkStatsDumped:
                            {
                                var args = DecodeUnit<NetworkStatsDumpedCallBackEventArgs>(response, dataReceivedEventArgs, HoloNETResponseType.AppNetworkStatsDumped, "APP: NETWORK STATS DUMPED");
                                try { args.NetworkStatsDumpJSON = DeserializeResponseValueAsJson(response.data); }
                                catch (Exception ex) { HandleError(args, $"An unknown error occurred decoding app network stats. Reason: {ex}"); }
                                RaiseAppEvent(args, OnNetworkStatsDumpedCallBack, _taskCompletionNetworkStatsDumpedCallBack, "AppNetworkStatsDumped");
                            }
                            break;

                        case HoloNETResponseType.AppEnabledViaAppInterface:
                            RaiseAppEvent(DecodeUnit<AppEnabledViaAppInterfaceCallBackEventArgs>(response, dataReceivedEventArgs, HoloNETResponseType.AppEnabledViaAppInterface, "APP: APP ENABLED"), OnAppEnabledCallBack, _taskCompletionAppEnabledCallBack, "AppEnabledViaAppInterface");
                            break;

                        case HoloNETResponseType.AppDirectSignalSent:
                            RaiseDirectSignalSentEvent(DecodeUnit<DirectSignalSentCallBackEventArgs>(response, dataReceivedEventArgs, HoloNETResponseType.AppDirectSignalSent, "APP: DIRECT SIGNAL SENT"));
                            break;

                        case HoloNETResponseType.AppDirectSignalCapabilityGranted:
                            RaiseDirectSignalCapabilityGrantedEvent(DecodeDirectSignalCapabilityGranted(response, dataReceivedEventArgs));
                            break;

                        case HoloNETResponseType.AppMemproofsProvided:
                            DecodeMemproofsProvidedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppPeerMetaInfoReturned:
                            DecodeAppPeerMetaInfoReturnedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AppOpTimingsDumped:
                            DecodeAppOpTimingsDumpedReceived(response, dataReceivedEventArgs);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in HoloNETClient.ProcessDataReceived method.";
                //HandleError(msg, ex); // could throw and skip completing the call
                ReportError(msg, ex);
            }

            return response;
        }

        protected override string ProcessErrorReceivedFromConductor(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
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

                    case HoloNETRequestType.AppCreateCloneCell:
                        RaiseCloneCellCreatedEvent(ProcessResponeError<CloneCellCreatedCallBackEventArgs>(response, dataReceivedEventArgs, "AppCreateCloneCell", msg));
                        break;

                    case HoloNETRequestType.AppEnableCloneCell:
                        RaiseCloneCellEnabledEvent(ProcessResponeError<CloneCellEnabledCallBackEventArgs>(response, dataReceivedEventArgs, "AppEnableCloneCell", msg));
                        break;

                    case HoloNETRequestType.AppDisableCloneCell:
                        RaiseCloneCellDisabledEvent(ProcessResponeError<CloneCellDisabledCallBackEventArgs>(response, dataReceivedEventArgs, "AppDisableCloneCell", msg));
                        break;

                    case HoloNETRequestType.AppGetCountersigningSessionState:
                        RaiseCountersigningSessionStateReturnedEvent(ProcessResponeError<CountersigningSessionStateReturnedCallBackEventArgs>(response, dataReceivedEventArgs, "AppGetCountersigningSessionState", msg));
                        break;

                    case HoloNETRequestType.AppAbandonCountersigningSession:
                        RaiseCountersigningSessionAbandonedEvent(ProcessResponeError<CountersigningSessionAbandonedCallBackEventArgs>(response, dataReceivedEventArgs, "AppAbandonCountersigningSession", msg));
                        break;

                    case HoloNETRequestType.AppPublishCountersigningSession:
                        RaisePublishCountersigningSessionTriggeredEvent(ProcessResponeError<PublishCountersigningSessionTriggeredCallBackEventArgs>(response, dataReceivedEventArgs, "AppPublishCountersigningSession", msg));
                        break;

                    case HoloNETRequestType.AppListWasmHostFunctions:
                        RaiseWasmHostFunctionsListedEvent(ProcessResponeError<WasmHostFunctionsListedCallBackEventArgs>(response, dataReceivedEventArgs, "AppListWasmHostFunctions", msg));
                        break;

                    case HoloNETRequestType.AppAgentInfo:
                        RaiseAppEvent(ProcessResponeError<AgentInfoReturnedCallBackEventArgs>(response, dataReceivedEventArgs, "AppAgentInfo", msg), OnAgentInfoReturnedCallBack, _taskCompletionAgentInfoReturnedCallBack, "AppAgentInfoReturned");
                        break;

                    case HoloNETRequestType.AppDumpNetworkMetrics:
                        RaiseAppEvent(ProcessResponeError<NetworkMetricsDumpedCallBackEventArgs>(response, dataReceivedEventArgs, "AppDumpNetworkMetrics", msg), OnNetworkMetricsDumpedCallBack, _taskCompletionNetworkMetricsDumpedCallBack, "AppNetworkMetricsDumped");
                        break;

                    case HoloNETRequestType.AppDumpNetworkStats:
                        RaiseAppEvent(ProcessResponeError<NetworkStatsDumpedCallBackEventArgs>(response, dataReceivedEventArgs, "AppDumpNetworkStats", msg), OnNetworkStatsDumpedCallBack, _taskCompletionNetworkStatsDumpedCallBack, "AppNetworkStatsDumped");
                        break;

                    case HoloNETRequestType.AppEnableApp:
                        RaiseAppEvent(ProcessResponeError<AppEnabledViaAppInterfaceCallBackEventArgs>(response, dataReceivedEventArgs, "AppEnableApp", msg), OnAppEnabledCallBack, _taskCompletionAppEnabledCallBack, "AppEnabledViaAppInterface");
                        break;

                    case HoloNETRequestType.AppSendDirectSignal:
                        RaiseDirectSignalSentEvent(ProcessResponeError<DirectSignalSentCallBackEventArgs>(response, dataReceivedEventArgs, "AppSendDirectSignal", msg));
                        break;

                    case HoloNETRequestType.AppGrantDirectSignalCapability:
                        RaiseDirectSignalCapabilityGrantedEvent(ProcessResponeError<DirectSignalCapabilityGrantedCallBackEventArgs>(response, dataReceivedEventArgs, "AppGrantDirectSignalCapability", msg));
                        break;

                    case HoloNETRequestType.AppProvideMemproofs:
                        RaiseMemproofsProvidedEvent(ProcessResponeError<MemproofsProvidedCallBackEventArgs>(response, dataReceivedEventArgs, "AppProvideMemproofs", msg));
                        break;

                    case HoloNETRequestType.AppPeerMetaInfo:
                        RaiseAppPeerMetaInfoReturnedEvent(ProcessResponeError<AppPeerMetaInfoReturnedCallBackEventArgs>(response, dataReceivedEventArgs, "AppPeerMetaInfo", msg));
                        break;

                    case HoloNETRequestType.AppDumpOpTimings:
                        RaiseAppOpTimingsDumpedEvent(ProcessResponeError<AppOpTimingsDumpedCallBackEventArgs>(response, dataReceivedEventArgs, "AppDumpOpTimings", msg));
                        break;
                }
            }

            return msg;
        }

        private void DecodeAppInfoDataReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            //AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs();
            // Created first so the request id is set even if decoding fails (a malformed reply left GetAppInfoAsync hanging).
            AppInfoCallBackEventArgs args = CreateHoloNETArgs<AppInfoCallBackEventArgs>(response, dataReceivedEventArgs);
            AppInfoResponse appInfoResponse = null;

            try
            {
                Logger.Log("APP INFO RESPONSE DATA DETECTED\n", LogType.Info);
                appInfoResponse = MessagePackSerializer.Deserialize<AppInfoResponse>(response.data, messagePackSerializerOptions);
                //args = CreateHoloNETArgs<AppInfoCallBackEventArgs>(response, dataReceivedEventArgs);

                if (appInfoResponse != null)
                {
                    appInfoResponse.data = ProcessAppInfo(appInfoResponse.data, args);
                    args.AppInfoResponse = appInfoResponse;
                    args.AppStatus = appInfoResponse.data.AppStatus;
                    args.AppStatusReason = appInfoResponse.data.AppStatusReason;
                    args.AppManifest = appInfoResponse.data.manifest;

                    if (response != null &&
                        _roleLookup.ContainsKey(response.id.ToString()) &&
                        !string.IsNullOrEmpty(_roleLookup[response.id.ToString()]) && appInfoResponse != null &&
                            appInfoResponse.data != null &&
                            appInfoResponse.data.cell_info != null &&
                            appInfoResponse.data.cell_info.ContainsKey(_roleLookup[response.id.ToString()]) &&
                            appInfoResponse.data.cell_info[_roleLookup[response.id.ToString()]] != null &&
                            appInfoResponse.data.cell_info[_roleLookup[response.id.ToString()]].Count > 0 &&
                            appInfoResponse.data.cell_info[_roleLookup[response.id.ToString()]][0] != null)
                        args.CellType = appInfoResponse.data.cell_info[_roleLookup[response.id.ToString()]][0].CellInfoType;

                    CachedAppInfo = appInfoResponse.data;
                    _roleLookup.Remove(response.id.ToString());
                }
                else
                {
                    args.Message = "Error occured in HoloNETClient.DecodeAppInfoDataReceived. appInfoResponse is null.";
                    args.IsError = true;
                    //HandleError(args.Message); // could throw and skip completing the call
                    ReportError(args.Message);
                }
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeAppInfoDataReceived. Reason: {ex}";
                args.IsError = true;
                args.Message = msg;
                //HandleError(msg, ex); // could throw and skip completing the call
                ReportError(msg, ex);
            }

            //If either the AgentPubKey or DnaHash is empty then attempt to get from the sandbox cmd.
            if (!args.IsError)
            {
                if (!string.IsNullOrEmpty(HoloNETDNA.AgentPubKey) && !string.IsNullOrEmpty(HoloNETDNA.DnaHash))
                    SetReadyForZomeCalls(response.id.ToString());

                else if (_automaticallyAttemptToGetFromSandboxIfConductorFails)
                {
                    //RetrieveAgentPubKeyAndDnaHashFromSandbox();
                    // Ran hc synchronously on the WebSocket receive thread, and with the default fallback back to the conductor
                    // the two could call each other forever. Run it in the background and don't bounce back.
                    string retrievalId = response.id.ToString();
                    FireAndForget(Task.Run(async () =>
                    {
                        AgentPubKeyDnaHash fromSandbox = await RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(_updateDnaHashAndAgentPubKey, false);
                        if (_taskCompletionAgentPubKeyAndDnaHashRetrieved.TryGetValue(retrievalId, out TaskCompletionSource<AgentPubKeyDnaHash> waiting))
                        {
                            _taskCompletionAgentPubKeyAndDnaHashRetrieved.Remove(retrievalId);
                            waiting.TrySetResult(fromSandbox);
                        }
                    }), "RetrieveAgentPubKeyAndDnaHashFromSandboxAsync");
                }
                else if (!string.IsNullOrEmpty(response.id.ToString()) && _taskCompletionAgentPubKeyAndDnaHashRetrieved.TryGetValue(response.id.ToString(), out TaskCompletionSource<AgentPubKeyDnaHash> noKeys))
                {
                    // app_info had no cell for this agent and there is no fallback: finish the retrieval rather than hang.
                    _taskCompletionAgentPubKeyAndDnaHashRetrieved.Remove(response.id.ToString());
                    RetrievingAgentPubKeyAndDnaHash = false;
                    noKeys.TrySetResult(null);
                }
            }

            RaiseAppInfoReceivedEvent(args);
        }

        private void DecodeSignalDataReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            SignalCallBackEventArgs signalCallBackEventArgs = new SignalCallBackEventArgs();

            try
            {
                Logger.Log("SIGNAL DATA DETECTED\n", LogType.Info);

                // Holochain 0.7.0 Signal: {"type": "app" | "app_direct" | "system", "value": {...}}.
                // The old {"App": [cell, data]} shape (with agent/dna swapped) no longer exists.
                signalCallBackEventArgs = CreateHoloNETArgs<SignalCallBackEventArgs>(response, dataReceivedEventArgs);
                string variant = ReadResponseType(response.data);

                if (variant == "app" || variant == "app_direct")
                {
                    SignalValue value = DeserializeResponseValue<SignalValue>(response.data);
                    Dictionary<string, object> signalDataDecoded = new Dictionary<string, object>();
                    string signalDataAsString = "";

                    if (value?.signal != null && value.signal.Length > 0)
                    {
                        // A direct signal is opaque bytes chosen by the sender and needn't be msgpack; decoding it as msgpack
                        // threw and turned the whole signal into an error. Fall back to the raw bytes.
                        try
                        {
                            object payload = MessagePackSerializer.Deserialize<object>(value.signal, messagePackSerializerOptions);

                            if (payload is Dictionary<object, object> map)
                            {
                                foreach (var kv in map)
                                    signalDataDecoded[kv.Key.ToString()] = kv.Value;
                            }
                            else
                                signalDataDecoded["value"] = payload;

                            signalDataAsString = MessagePackSerializer.ConvertToJson(value.signal);
                        }
                        catch (MessagePackSerializationException) when (variant == "app_direct")
                        {
                            signalDataDecoded["value"] = value.signal;
                            signalDataAsString = Convert.ToBase64String(value.signal);
                        }
                    }

                    signalCallBackEventArgs.DnaHash = value?.cell_id != null ? ConvertHoloHashToString(value.cell_id[0]) : "";
                    signalCallBackEventArgs.AgentPubKey = value?.cell_id != null ? ConvertHoloHashToString(value.cell_id[1]) : "";
                    signalCallBackEventArgs.RawSignalData = new SignalData { CellData = value?.cell_id, Data = value?.signal };
                    signalCallBackEventArgs.SignalData = signalDataDecoded;
                    signalCallBackEventArgs.SignalDataAsString = signalDataAsString;
                    signalCallBackEventArgs.SignalType = SignalType.App;
                    signalCallBackEventArgs.IsDirectSignal = variant == "app_direct";
                    signalCallBackEventArgs.FromAgentPubKey = value?.from_agent != null ? ConvertHoloHashToString(value.from_agent) : null;
                }
                else
                {
                    signalCallBackEventArgs.SignalType = SignalType.System;
                    // SystemSignal is adjacently tagged too: {"type": "system", "value": {"type": "<variant>", "value": ...}}.
                    byte[] system = ReadRawValue(response.data);
                    signalCallBackEventArgs.SystemSignalType = system == null ? null : ReadResponseType(system);
                    signalCallBackEventArgs.SignalDataAsString = DeserializeResponseValueAsJson(response.data);
                }
            }
            catch (Exception ex)
            {
                HandleError(signalCallBackEventArgs, "An unknown error occurred in HoloNETClient.DecodeSignalDataReceived.", ex);
            }

            // Ignore System signals — they carry no app-level data (mirrors the JS client behaviour).
            if (signalCallBackEventArgs.SignalType == SignalType.System && !signalCallBackEventArgs.IsError)
            {
                OnSystemSignalCallBack?.Invoke(this, signalCallBackEventArgs);
                return;
            }

            RaiseSignalReceivedEvent(signalCallBackEventArgs);
        }

        private void DecodeZomeDataReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            Logger.Log("ZOME RESPONSE DATA DETECTED\n", LogType.Info);
            //AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions); // outside any try: a malformed reply escaped the decoder and the zome call hung
            AppResponse appResponse = null;
            string id = response.id.ToString();
            ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs();

            zomeFunctionCallBackArgs = CreateHoloNETArgs<ZomeFunctionCallBackEventArgs>(response, dataReceivedEventArgs);
            zomeFunctionCallBackArgs.Zome = GetItemFromCache(id, _zomeLookup);
            zomeFunctionCallBackArgs.ZomeFunction = GetItemFromCache(id, _funcLookup);

            try
            {
                appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);
            }
            catch (Exception ex)
            {
                zomeFunctionCallBackArgs.IsError = true;
                zomeFunctionCallBackArgs.Message = $"The zome call response could not be decoded. Reason: {ex.Message}";
                Logger.Log(zomeFunctionCallBackArgs.Message, LogType.Error);
                RaiseZomeDataReceivedEvent(zomeFunctionCallBackArgs);
                return;
            }

            try
            {
                // ActionHash/EntryHash results (for example delete zome functions) are
                // MessagePack binary values, not record maps. Decode them before trying
                // the collection paths so a byte sequence is not mistaken for a list.
                object scalarResponse = MessagePackSerializer.Deserialize<object>(appResponse.data, messagePackSerializerOptions);
                zomeFunctionCallBackArgs.ZomeReturnValue = scalarResponse;

                // (), bool, numbers and strings are normal zome return types; they used to fall through every
                // collection path and be reported as "an unknown response".
                if (scalarResponse == null || scalarResponse is string || scalarResponse is bool || scalarResponse.GetType().IsPrimitive)
                {
                    RaiseZomeDataReceivedEvent(zomeFunctionCallBackArgs);
                    return;
                }

                if (scalarResponse is byte[] scalarHash)
                {
                    zomeFunctionCallBackArgs.ZomeReturnHash = ConvertHoloHashToString(scalarHash);
                    RaiseZomeDataReceivedEvent(zomeFunctionCallBackArgs);
                    return;
                }

                // Try single-record (dictionary) response first
                Dictionary<object, object> rawAppResponseData = null;
                List<object> rawAppResponseList = null;

                try
                {
                    rawAppResponseData = MessagePackSerializer.Deserialize<Dictionary<object, object>>(appResponse.data, messagePackSerializerOptions);
                }
                catch
                {
                    // Not a map — try list (zome functions returning Vec<Record> or similar)
                    rawAppResponseList = MessagePackSerializer.Deserialize<List<object>>(appResponse.data, messagePackSerializerOptions);
                }

                if (rawAppResponseList != null)
                {
                    // Multi-entry response (e.g. get_links, get_all_entries)
                    Dictionary<string, string> allKeyValuePairs = new Dictionary<string, string>();
                    string allKeyValuePairsAsString = "";

                    foreach (object item in rawAppResponseList)
                    {
                        Dictionary<object, object> itemDict = item as Dictionary<object, object>;
                        if (itemDict == null) continue;

                        Dictionary<string, object> itemAppData = new Dictionary<string, object>();
                        Dictionary<string, string> itemKeyValuePairs = new Dictionary<string, string>();
                        string itemKeyValuePairsAsString = "";
                        Record itemRecord = null;

                        (itemAppData, itemKeyValuePairs, itemKeyValuePairsAsString, itemRecord) = DecodeRawZomeData(itemDict, itemAppData, itemKeyValuePairs, itemKeyValuePairsAsString);

                        if (_entryDataObjectTypeLookup.ContainsKey(id) && _entryDataObjectTypeLookup[id] != null)
                            itemRecord.EntryDataObject = MapEntryDataObject(_entryDataObjectTypeLookup[id], itemKeyValuePairs);
                        else if (_entryDataObjectLookup.ContainsKey(id) && _entryDataObjectLookup[id] != null)
                            itemRecord.EntryDataObject = MapEntryDataObject(_entryDataObjectLookup[id], itemKeyValuePairs);

                        zomeFunctionCallBackArgs.Records.Add(itemRecord);

                        foreach (var kv in itemKeyValuePairs)
                            allKeyValuePairs[kv.Key] = kv.Value;
                        allKeyValuePairsAsString += itemKeyValuePairsAsString;
                    }

                    Logger.Log($"Decoded Data ({zomeFunctionCallBackArgs.Records.Count} records):\n{allKeyValuePairsAsString}", LogType.Info);
                    zomeFunctionCallBackArgs.KeyValuePair = allKeyValuePairs;
                    zomeFunctionCallBackArgs.KeyValuePairAsString = allKeyValuePairsAsString;
                }
                else if (rawAppResponseData != null)
                {
                    Dictionary<string, object> appResponseData = new Dictionary<string, object>();
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    string keyValuePairsAsString = "";
                    Record record = null;

                    (appResponseData, keyValuePairs, keyValuePairsAsString, record) = DecodeRawZomeData(rawAppResponseData, appResponseData, keyValuePairs, keyValuePairsAsString);

                    if (_entryDataObjectTypeLookup.ContainsKey(id) && _entryDataObjectTypeLookup[id] != null)
                        record.EntryDataObject = MapEntryDataObject(_entryDataObjectTypeLookup[id], keyValuePairs);
                    else if (_entryDataObjectLookup.ContainsKey(id) && _entryDataObjectLookup[id] != null)
                        record.EntryDataObject = MapEntryDataObject(_entryDataObjectLookup[id], keyValuePairs);

                    Logger.Log($"Decoded Data:\n{keyValuePairsAsString}", LogType.Info);

                    zomeFunctionCallBackArgs.RawZomeReturnData = rawAppResponseData;
                    zomeFunctionCallBackArgs.KeyValuePair = keyValuePairs;
                    zomeFunctionCallBackArgs.KeyValuePairAsString = keyValuePairsAsString;
                    zomeFunctionCallBackArgs.Records.Add(record);
                }
                else
                {
                    zomeFunctionCallBackArgs.Message = "Bad/Null Data Returned From The Holochain Conductor.";
                    zomeFunctionCallBackArgs.IsError = true;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // Final fallback: plain byte[] HoloHash return value
                    object rawAppResponseData = MessagePackSerializer.Deserialize<object>(appResponse.data, messagePackSerializerOptions);
                    byte[] holoHash = rawAppResponseData as byte[];

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
                        zomeFunctionCallBackArgs.Message = msg;
                        Logger.Log(msg, LogType.Error); // args carry the error; HandleError throws and skipped the raise below, hanging the caller
                    }
                }
                catch (Exception ex2)
                {
                    string msg = $"An unknown error occurred in HoloNETClient.DecodeZomeDataReceived. Reason: {ex2}";
                    zomeFunctionCallBackArgs.IsError = true;
                    zomeFunctionCallBackArgs.Message = msg;
                    Logger.Log(msg, LogType.Error);
                }
            }

            RaiseZomeDataReceivedEvent(zomeFunctionCallBackArgs);
        }

        // New in Holochain 0.7.0 - App API.

        private void DecodeCloneCellCreatedReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeCloneCellCreatedReceived. Reason: ";
            CloneCellCreatedCallBackEventArgs args = CreateHoloNETArgs<CloneCellCreatedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppCloneCellCreated;

            try
            {
                Logger.Log("APP: CLONE CELL CREATED\n", LogType.Info);
                ClonedCellResponse clonedCellResponse = DeserializeResponseValue<ClonedCellResponse>(response.data);

                if (clonedCellResponse != null)
                    args.ClonedCell = clonedCellResponse;
                else
                    HandleError(args, $"{errorMessage} clonedCellResponse failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseCloneCellCreatedEvent(args);
        }

        private void DecodeCloneCellEnabledReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeCloneCellEnabledReceived. Reason: ";
            CloneCellEnabledCallBackEventArgs args = CreateHoloNETArgs<CloneCellEnabledCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppCloneCellEnabled;

            try
            {
                Logger.Log("APP: CLONE CELL ENABLED\n", LogType.Info);
                ClonedCellResponse clonedCellResponse = DeserializeResponseValue<ClonedCellResponse>(response.data);

                if (clonedCellResponse != null)
                    args.ClonedCell = clonedCellResponse;
                else
                    HandleError(args, $"{errorMessage} clonedCellResponse failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseCloneCellEnabledEvent(args);
        }

        private void DecodeCloneCellDisabledReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeCloneCellDisabledReceived. Reason: ";
            CloneCellDisabledCallBackEventArgs args = CreateHoloNETArgs<CloneCellDisabledCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppCloneCellDisabled;

            try
            {
                Logger.Log("APP: CLONE CELL DISABLED\n", LogType.Info);
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseCloneCellDisabledEvent(args);
        }

        private void DecodeCountersigningSessionStateReturnedReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeCountersigningSessionStateReturnedReceived. Reason: ";
            CountersigningSessionStateReturnedCallBackEventArgs args = CreateHoloNETArgs<CountersigningSessionStateReturnedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppCountersigningSessionStateReturned;

            try
            {
                Logger.Log("APP: COUNTERSIGNING SESSION STATE RETURNED\n", LogType.Info);
                //CountersigningSessionStateResponse sessionStateResponse = DeserializeResponseValue<CountersigningSessionStateResponse>(response.data);
                //if (sessionStateResponse != null)
                //    args.SessionState = sessionStateResponse;
                //else
                //    HandleError(args, $"{errorMessage} sessionStateResponse failed to deserialize.");

                // The value is Option<CountersigningSessionState> itself (null = no session in progress), not a
                // {session_state} object, so "no session" was reported as an error.
                byte[] raw = ReadRawValue(response.data);
                args.SessionState = new CountersigningSessionStateResponse
                {
                    session_state = raw == null ? null : MessagePackSerializer.Deserialize<object>(raw, messagePackSerializerOptions)
                };
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseCountersigningSessionStateReturnedEvent(args);
        }

        private void DecodeCountersigningSessionAbandonedReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeCountersigningSessionAbandonedReceived. Reason: ";
            CountersigningSessionAbandonedCallBackEventArgs args = CreateHoloNETArgs<CountersigningSessionAbandonedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppCountersigningSessionAbandoned;

            try
            {
                Logger.Log("APP: COUNTERSIGNING SESSION ABANDONED\n", LogType.Info);
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseCountersigningSessionAbandonedEvent(args);
        }

        private void DecodePublishCountersigningSessionTriggeredReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodePublishCountersigningSessionTriggeredReceived. Reason: ";
            PublishCountersigningSessionTriggeredCallBackEventArgs args = CreateHoloNETArgs<PublishCountersigningSessionTriggeredCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppPublishCountersigningSessionTriggered;

            try
            {
                Logger.Log("APP: PUBLISH COUNTERSIGNING SESSION TRIGGERED\n", LogType.Info);
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaisePublishCountersigningSessionTriggeredEvent(args);
        }

        private T DecodeUnit<T>(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, HoloNETResponseType responseType, string logMessage) where T : HoloNETDataReceivedBaseEventArgs, new()
        {
            T args = CreateHoloNETArgs<T>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = responseType;
            Logger.Log(logMessage + "\n", LogType.Info);
            return args;
        }

        private DirectSignalCapabilityGrantedCallBackEventArgs DecodeDirectSignalCapabilityGranted(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            var args = DecodeUnit<DirectSignalCapabilityGrantedCallBackEventArgs>(response, dataReceivedEventArgs, HoloNETResponseType.AppDirectSignalCapabilityGranted, "APP: DIRECT SIGNAL CAPABILITY GRANTED");
            try
            {
                args.ActionHash = DeserializeResponseValue<byte[]>(response.data);
            }
            catch (Exception ex)
            {
                HandleError(args, $"An unknown error occurred in HoloNETClient.DecodeDirectSignalCapabilityGranted. Reason: {ex}");
            }
            return args;
        }

        /// <summary>Logs, raises <paramref name="handler"/> and completes the awaiting call for <paramref name="args"/>.Id.</summary>
        private void RaiseAppEvent<T>(T args, Delegate handler, Dictionary<string, TaskCompletionSource<T>> completions, string name) where T : HoloNETDataReceivedBaseEventArgs
        {
            LogEvent(name, args);
            handler?.DynamicInvoke(this, args);
            if (!string.IsNullOrEmpty(args.Id) && completions.TryGetValue(args.Id, out var completion))
            {
                completion.TrySetResult(args);
                completions.Remove(args.Id);
            }
        }

        private void RaiseDirectSignalSentEvent(DirectSignalSentCallBackEventArgs args)
        {
            LogEvent("AppDirectSignalSent", args);
            OnDirectSignalSentCallBack?.Invoke(this, args);
            if (!string.IsNullOrEmpty(args.Id) && _taskCompletionDirectSignalSentCallBack.TryGetValue(args.Id, out var completion))
            {
                completion.TrySetResult(args);
                _taskCompletionDirectSignalSentCallBack.Remove(args.Id);
            }
        }

        private void RaiseDirectSignalCapabilityGrantedEvent(DirectSignalCapabilityGrantedCallBackEventArgs args)
        {
            LogEvent("AppDirectSignalCapabilityGranted", args);
            OnDirectSignalCapabilityGrantedCallBack?.Invoke(this, args);
            if (!string.IsNullOrEmpty(args.Id) && _taskCompletionDirectSignalCapabilityGrantedCallBack.TryGetValue(args.Id, out var completion))
            {
                completion.TrySetResult(args);
                _taskCompletionDirectSignalCapabilityGrantedCallBack.Remove(args.Id);
            }
        }

        private void DecodeWasmHostFunctionsListedReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeWasmHostFunctionsListedReceived. Reason: ";
            WasmHostFunctionsListedCallBackEventArgs args = CreateHoloNETArgs<WasmHostFunctionsListedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppWasmHostFunctionsListed;

            try
            {
                Logger.Log("APP: WASM HOST FUNCTIONS LISTED\n", LogType.Info);
                List<string> wasmHostFunctions = DeserializeResponseValue<List<string>>(response.data);

                if (wasmHostFunctions != null)
                    args.WasmHostFunctions = wasmHostFunctions;
                else
                    HandleError(args, $"{errorMessage} wasmHostFunctions failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseWasmHostFunctionsListedEvent(args);
        }

        private void DecodeMemproofsProvidedReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeMemproofsProvidedReceived. Reason: ";
            MemproofsProvidedCallBackEventArgs args = CreateHoloNETArgs<MemproofsProvidedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppMemproofsProvided;

            try
            {
                Logger.Log("APP: MEMPROOFS PROVIDED\n", LogType.Info);
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseMemproofsProvidedEvent(args);
        }

        private void DecodeAppPeerMetaInfoReturnedReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppPeerMetaInfoReturnedReceived. Reason: ";
            AppPeerMetaInfoReturnedCallBackEventArgs args = CreateHoloNETArgs<AppPeerMetaInfoReturnedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppPeerMetaInfoReturned;

            try
            {
                Logger.Log("APP: PEER META INFO RETURNED\n", LogType.Info);
                //PeerMetaInfoResponse peerMetaInfoResponse = DeserializeResponseValue<PeerMetaInfoResponse>(response.data);
                PeerMetaInfoResponse peerMetaInfoResponse = DecodePeerMetaInfo(response.data);

                if (peerMetaInfoResponse != null)
                    args.PeerMetaInfo = peerMetaInfoResponse;
                else
                    HandleError(args, $"{errorMessage} peerMetaInfoResponse failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseAppPeerMetaInfoReturnedEvent(args);
        }

        private (Dictionary<string, object>, Dictionary<string, string> keyValuePair, string keyValuePairAsString, Record record) DecodeRawZomeData(Dictionary<object, object> rawAppResponseData, Dictionary<string, object> appResponseData, Dictionary<string, string> keyValuePair, string keyValuePairAsString, Record record = null)
        {
            string value = "";
            var options = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

            if (record == null)
                record = new Record();

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
                                        //decodedEntry[entryKey.ToString()] = (entry[entryKey]?.ToString() ?? "null"); // current-culture formatting
                                        decodedEntry[entryKey.ToString()] = ToInvariantString(entry[entryKey]);
                                        //keyValuePair[entryKey.ToString()] = (entry[entryKey]?.ToString() ?? "null"); // current-culture formatting
                                        keyValuePair[entryKey.ToString()] = ToInvariantString(entry[entryKey]);
                                        //keyValuePairAsString = string.Concat(keyValuePairAsString, entryKey.ToString(), "=", (entry[entryKey]?.ToString() ?? "null"), "\n"); // current-culture formatting
                                        keyValuePairAsString = string.Concat(keyValuePairAsString, entryKey.ToString(), "=", ToInvariantString(entry[entryKey]), "\n");
                                    }

                                    record.Bytes = bytes;
                                    record.BytesString = byteString;
                                    record.EntryKeyValuePairs = decodedEntry;
                                    appResponseData[key] = record;
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
                                (tempDict, keyValuePair, keyValuePairAsString, record) = DecodeRawZomeData(dict, tempDict, keyValuePair, keyValuePairAsString, record);
                                appResponseData[key] = tempDict;
                            }
                            else if (rawAppResponseData[key] != null)
                                //value = rawAppResponseData[key].ToString(); // current-culture formatting
                                value = ToInvariantString(rawAppResponseData[key]);
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
                                        record.ActionHash = value;
                                        break;

                                    case "entry_hash":
                                        record.EntryHash = value;
                                        break;

                                    case "prev_action":
                                        record.PreviousActionHash = value;
                                        break;

                                    case "signature":
                                        record.Signature = value;
                                        break;

                                    case "action_seq":
                                        record.ActionSequence = Convert.ToInt32(value);
                                        break;

                                    case "author":
                                        record.Author = value;
                                        break;

                                    case "original_action_address":
                                        record.OriginalActionAddress = value;
                                        break;

                                    case "original_entry_address":
                                        record.OriginalEntryAddress = value;
                                        break;

                                    case "timestamp":
                                        {
                                            record.Timestamp = Convert.ToInt64(value);
                                            long time = record.Timestamp / 1000; // Divide by 1,000 because we need milliseconds, not microseconds.
                                            //record.DateTime = DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime.AddHours(-5).AddMinutes(1); // fixed US-Eastern offset (+1 min): wrong everywhere else and during DST
                                            // Holochain timestamps are microseconds since the Unix epoch (UTC).
                                            record.DateTime = DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime;
                                        }
                                        break;

                                    case "type":
                                        record.Type = value;
                                        break;

                                    case "entry_type":
                                        record.EntryType = value;
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log($"Skipped a field in HoloNETClient.DecodeZomeReturnData: {ex.Message}", LogType.Warning); // one bad field must not abort the record (and hang the call)
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Skipped a field in HoloNETClient.DecodeZomeReturnData: {ex.Message}", LogType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.DecodeZomeReturnData method.", ex);
            }

            return (appResponseData, keyValuePair, keyValuePairAsString, record);
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

            // A retrieval waiting on this app_info (RetrieveAgentPubKeyAndDnaHashFromConductorAsync) was only completed on
            // success (SetReadyForZomeCalls), so an error left it waiting forever. Complete it with null.
            if (appInfoCallBackEventArgs.IsError && !string.IsNullOrEmpty(appInfoCallBackEventArgs.Id)
                && _taskCompletionAgentPubKeyAndDnaHashRetrieved.TryGetValue(appInfoCallBackEventArgs.Id, out TaskCompletionSource<AgentPubKeyDnaHash> retrieval))
            {
                _taskCompletionAgentPubKeyAndDnaHashRetrieved.Remove(appInfoCallBackEventArgs.Id);
                RetrievingAgentPubKeyAndDnaHash = false;
                retrieval.TrySetResult(null);
            }

            if (_taskCompletionAppInfoRetrieved != null && !string.IsNullOrEmpty(appInfoCallBackEventArgs.Id) && _taskCompletionAppInfoRetrieved.ContainsKey(appInfoCallBackEventArgs.Id))
            {
                _taskCompletionAppInfoRetrieved[appInfoCallBackEventArgs.Id].TrySetResult(appInfoCallBackEventArgs);
                _taskCompletionAppInfoRetrieved.Remove(appInfoCallBackEventArgs.Id);
            }
        }

        private void RaiseZomeDataReceivedEvent(ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs)
        {
            if (!string.IsNullOrEmpty(zomeFunctionCallBackArgs.Id))
                UntrackPendingCall(zomeFunctionCallBackArgs.Id);

            LogEvent("ZomeFunctionCallBack", zomeFunctionCallBackArgs, string.Concat("Zome: ", zomeFunctionCallBackArgs.Zome, ", Zome Function: ", zomeFunctionCallBackArgs.ZomeFunction, ", Raw Zome Return Data: ", zomeFunctionCallBackArgs.RawZomeReturnData, ", Zome Return Data: ", zomeFunctionCallBackArgs.ZomeReturnData, ", Zome Return Hash: ", zomeFunctionCallBackArgs.ZomeReturnHash));

            //if (_callbackLookup.ContainsKey(zomeFunctionCallBackArgs.Id) && _callbackLookup[zomeFunctionCallBackArgs.Id] != null)
            //    _callbackLookup[zomeFunctionCallBackArgs.Id].DynamicInvoke(this, zomeFunctionCallBackArgs);
            //if (_taskCompletionZomeCallBack.ContainsKey(zomeFunctionCallBackArgs.Id) && _taskCompletionZomeCallBack[zomeFunctionCallBackArgs.Id] != null)
            //{
            //    _taskCompletionZomeCallBack[zomeFunctionCallBackArgs.Id].TrySetResult(zomeFunctionCallBackArgs);
            //    _taskCompletionZomeCallBack.Remove(zomeFunctionCallBackArgs.Id);
            //}
            //OnZomeFunctionCallBack?.Invoke(this, zomeFunctionCallBackArgs);
            //// If the zome call requested for this to be cached then stick it in the cache.
            //if (_cacheZomeReturnDataLookup.ContainsKey(zomeFunctionCallBackArgs.Id) && _cacheZomeReturnDataLookup[zomeFunctionCallBackArgs.Id])
            //    _zomeReturnDataLookup[zomeFunctionCallBackArgs.Id] = zomeFunctionCallBackArgs;
            //_zomeLookup.Remove(zomeFunctionCallBackArgs.Id);
            //_funcLookup.Remove(zomeFunctionCallBackArgs.Id);
            //_callbackLookup.Remove(zomeFunctionCallBackArgs.Id);
            //_entryDataObjectTypeLookup.Remove(zomeFunctionCallBackArgs.Id);
            //_entryDataObjectLookup.Remove(zomeFunctionCallBackArgs.Id);
            //_cacheZomeReturnDataLookup.Remove(zomeFunctionCallBackArgs.Id);
            //_taskCompletionZomeCallBack.Remove(zomeFunctionCallBackArgs.Id);

            // Read and clear this call's state first, then complete it. The old order cleaned up after completing, so a
            // caller reusing the id (e.g. for cached results) had its new entries removed by the previous call's cleanup.
            string id = zomeFunctionCallBackArgs.Id ?? "";
            _callbackLookup.TryGetValue(id, out ZomeFunctionCallBack callback);
            _taskCompletionZomeCallBack.TryGetValue(id, out TaskCompletionSource<ZomeFunctionCallBackEventArgs> completion);
            bool cache = _cacheZomeReturnDataLookup.TryGetValue(id, out bool cacheFlag) && cacheFlag;

            _zomeLookup.Remove(id);
            _funcLookup.Remove(id);
            _callbackLookup.Remove(id);
            _entryDataObjectTypeLookup.Remove(id);
            _entryDataObjectLookup.Remove(id);
            _cacheZomeReturnDataLookup.Remove(id);
            if (completion != null && _taskCompletionZomeCallBack.TryGetValue(id, out var current) && current == completion)
                _taskCompletionZomeCallBack.Remove(id);

            // If the zome call requested for this to be cached then stick it in the cache.
            if (cache)
                _zomeReturnDataLookup[id] = zomeFunctionCallBackArgs;

            callback?.DynamicInvoke(this, zomeFunctionCallBackArgs);
            completion?.TrySetResult(zomeFunctionCallBackArgs);
            OnZomeFunctionCallBack?.Invoke(this, zomeFunctionCallBackArgs);
        }

        // New in Holochain 0.7.0 - App API.

        private void RaiseCloneCellCreatedEvent(CloneCellCreatedCallBackEventArgs cloneCellCreatedCallBackEventArgs)
        {
            LogEvent("AppCloneCellCreated", cloneCellCreatedCallBackEventArgs);
            OnCloneCellCreatedCallBack?.Invoke(this, cloneCellCreatedCallBackEventArgs);

            if (_taskCompletionCloneCellCreatedCallBack != null && !string.IsNullOrEmpty(cloneCellCreatedCallBackEventArgs.Id) && _taskCompletionCloneCellCreatedCallBack.ContainsKey(cloneCellCreatedCallBackEventArgs.Id))
            {
                _taskCompletionCloneCellCreatedCallBack[cloneCellCreatedCallBackEventArgs.Id].TrySetResult(cloneCellCreatedCallBackEventArgs);
                _taskCompletionCloneCellCreatedCallBack.Remove(cloneCellCreatedCallBackEventArgs.Id);
            }
        }

        private void RaiseCloneCellEnabledEvent(CloneCellEnabledCallBackEventArgs cloneCellEnabledCallBackEventArgs)
        {
            LogEvent("AppCloneCellEnabled", cloneCellEnabledCallBackEventArgs);
            OnCloneCellEnabledCallBack?.Invoke(this, cloneCellEnabledCallBackEventArgs);

            if (_taskCompletionCloneCellEnabledCallBack != null && !string.IsNullOrEmpty(cloneCellEnabledCallBackEventArgs.Id) && _taskCompletionCloneCellEnabledCallBack.ContainsKey(cloneCellEnabledCallBackEventArgs.Id))
            {
                _taskCompletionCloneCellEnabledCallBack[cloneCellEnabledCallBackEventArgs.Id].TrySetResult(cloneCellEnabledCallBackEventArgs);
                _taskCompletionCloneCellEnabledCallBack.Remove(cloneCellEnabledCallBackEventArgs.Id);
            }
        }

        private void RaiseCloneCellDisabledEvent(CloneCellDisabledCallBackEventArgs cloneCellDisabledCallBackEventArgs)
        {
            LogEvent("AppCloneCellDisabled", cloneCellDisabledCallBackEventArgs);
            OnCloneCellDisabledCallBack?.Invoke(this, cloneCellDisabledCallBackEventArgs);

            if (_taskCompletionCloneCellDisabledCallBack != null && !string.IsNullOrEmpty(cloneCellDisabledCallBackEventArgs.Id) && _taskCompletionCloneCellDisabledCallBack.ContainsKey(cloneCellDisabledCallBackEventArgs.Id))
            {
                _taskCompletionCloneCellDisabledCallBack[cloneCellDisabledCallBackEventArgs.Id].TrySetResult(cloneCellDisabledCallBackEventArgs);
                _taskCompletionCloneCellDisabledCallBack.Remove(cloneCellDisabledCallBackEventArgs.Id);
            }
        }

        private void RaiseCountersigningSessionStateReturnedEvent(CountersigningSessionStateReturnedCallBackEventArgs countersigningSessionStateReturnedCallBackEventArgs)
        {
            LogEvent("AppCountersigningSessionStateReturned", countersigningSessionStateReturnedCallBackEventArgs);
            OnCountersigningSessionStateReturnedCallBack?.Invoke(this, countersigningSessionStateReturnedCallBackEventArgs);

            if (_taskCompletionCountersigningSessionStateReturnedCallBack != null && !string.IsNullOrEmpty(countersigningSessionStateReturnedCallBackEventArgs.Id) && _taskCompletionCountersigningSessionStateReturnedCallBack.ContainsKey(countersigningSessionStateReturnedCallBackEventArgs.Id))
            {
                _taskCompletionCountersigningSessionStateReturnedCallBack[countersigningSessionStateReturnedCallBackEventArgs.Id].TrySetResult(countersigningSessionStateReturnedCallBackEventArgs);
                _taskCompletionCountersigningSessionStateReturnedCallBack.Remove(countersigningSessionStateReturnedCallBackEventArgs.Id);
            }
        }

        private void RaiseCountersigningSessionAbandonedEvent(CountersigningSessionAbandonedCallBackEventArgs countersigningSessionAbandonedCallBackEventArgs)
        {
            LogEvent("AppCountersigningSessionAbandoned", countersigningSessionAbandonedCallBackEventArgs);
            OnCountersigningSessionAbandonedCallBack?.Invoke(this, countersigningSessionAbandonedCallBackEventArgs);

            if (_taskCompletionCountersigningSessionAbandonedCallBack != null && !string.IsNullOrEmpty(countersigningSessionAbandonedCallBackEventArgs.Id) && _taskCompletionCountersigningSessionAbandonedCallBack.ContainsKey(countersigningSessionAbandonedCallBackEventArgs.Id))
            {
                _taskCompletionCountersigningSessionAbandonedCallBack[countersigningSessionAbandonedCallBackEventArgs.Id].TrySetResult(countersigningSessionAbandonedCallBackEventArgs);
                _taskCompletionCountersigningSessionAbandonedCallBack.Remove(countersigningSessionAbandonedCallBackEventArgs.Id);
            }
        }

        private void RaisePublishCountersigningSessionTriggeredEvent(PublishCountersigningSessionTriggeredCallBackEventArgs publishCountersigningSessionTriggeredCallBackEventArgs)
        {
            LogEvent("AppPublishCountersigningSessionTriggered", publishCountersigningSessionTriggeredCallBackEventArgs);
            OnPublishCountersigningSessionTriggeredCallBack?.Invoke(this, publishCountersigningSessionTriggeredCallBackEventArgs);

            if (_taskCompletionPublishCountersigningSessionTriggeredCallBack != null && !string.IsNullOrEmpty(publishCountersigningSessionTriggeredCallBackEventArgs.Id) && _taskCompletionPublishCountersigningSessionTriggeredCallBack.ContainsKey(publishCountersigningSessionTriggeredCallBackEventArgs.Id))
            {
                _taskCompletionPublishCountersigningSessionTriggeredCallBack[publishCountersigningSessionTriggeredCallBackEventArgs.Id].TrySetResult(publishCountersigningSessionTriggeredCallBackEventArgs);
                _taskCompletionPublishCountersigningSessionTriggeredCallBack.Remove(publishCountersigningSessionTriggeredCallBackEventArgs.Id);
            }
        }

        private void RaiseWasmHostFunctionsListedEvent(WasmHostFunctionsListedCallBackEventArgs wasmHostFunctionsListedCallBackEventArgs)
        {
            LogEvent("AppWasmHostFunctionsListed", wasmHostFunctionsListedCallBackEventArgs);
            OnWasmHostFunctionsListedCallBack?.Invoke(this, wasmHostFunctionsListedCallBackEventArgs);

            if (_taskCompletionWasmHostFunctionsListedCallBack != null && !string.IsNullOrEmpty(wasmHostFunctionsListedCallBackEventArgs.Id) && _taskCompletionWasmHostFunctionsListedCallBack.ContainsKey(wasmHostFunctionsListedCallBackEventArgs.Id))
            {
                _taskCompletionWasmHostFunctionsListedCallBack[wasmHostFunctionsListedCallBackEventArgs.Id].TrySetResult(wasmHostFunctionsListedCallBackEventArgs);
                _taskCompletionWasmHostFunctionsListedCallBack.Remove(wasmHostFunctionsListedCallBackEventArgs.Id);
            }
        }

        private void RaiseMemproofsProvidedEvent(MemproofsProvidedCallBackEventArgs memproofsProvidedCallBackEventArgs)
        {
            LogEvent("AppMemproofsProvided", memproofsProvidedCallBackEventArgs);
            OnMemproofsProvidedCallBack?.Invoke(this, memproofsProvidedCallBackEventArgs);

            if (_taskCompletionMemproofsProvidedCallBack != null && !string.IsNullOrEmpty(memproofsProvidedCallBackEventArgs.Id) && _taskCompletionMemproofsProvidedCallBack.ContainsKey(memproofsProvidedCallBackEventArgs.Id))
            {
                _taskCompletionMemproofsProvidedCallBack[memproofsProvidedCallBackEventArgs.Id].TrySetResult(memproofsProvidedCallBackEventArgs);
                _taskCompletionMemproofsProvidedCallBack.Remove(memproofsProvidedCallBackEventArgs.Id);
            }
        }

        private void RaiseAppPeerMetaInfoReturnedEvent(AppPeerMetaInfoReturnedCallBackEventArgs appPeerMetaInfoReturnedCallBackEventArgs)
        {
            LogEvent("AppPeerMetaInfoReturned", appPeerMetaInfoReturnedCallBackEventArgs);
            OnAppPeerMetaInfoReturnedCallBack?.Invoke(this, appPeerMetaInfoReturnedCallBackEventArgs);

            if (_taskCompletionAppPeerMetaInfoReturnedCallBack != null && !string.IsNullOrEmpty(appPeerMetaInfoReturnedCallBackEventArgs.Id) && _taskCompletionAppPeerMetaInfoReturnedCallBack.ContainsKey(appPeerMetaInfoReturnedCallBackEventArgs.Id))
            {
                _taskCompletionAppPeerMetaInfoReturnedCallBack[appPeerMetaInfoReturnedCallBackEventArgs.Id].TrySetResult(appPeerMetaInfoReturnedCallBackEventArgs);
                _taskCompletionAppPeerMetaInfoReturnedCallBack.Remove(appPeerMetaInfoReturnedCallBackEventArgs.Id);
            }
        }

        private void DecodeAppOpTimingsDumpedReceived(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppOpTimingsDumpedReceived. Reason: ";
            AppOpTimingsDumpedCallBackEventArgs args = CreateHoloNETArgs<AppOpTimingsDumpedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AppOpTimingsDumped;

            try
            {
                if (!args.IsError)
                {
                    OpTimingsDump dump = DeserializeResponseValue<OpTimingsDump>(response.data);
                    if (dump != null)
                        args.OpTimingsDump = dump;
                }
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseAppOpTimingsDumpedEvent(args);
        }

        private void RaiseAppOpTimingsDumpedEvent(AppOpTimingsDumpedCallBackEventArgs appOpTimingsDumpedCallBackEventArgs)
        {
            LogEvent("AppOpTimingsDumped", appOpTimingsDumpedCallBackEventArgs);
            OnAppOpTimingsDumpedCallBack?.Invoke(this, appOpTimingsDumpedCallBackEventArgs);

            if (_taskCompletionAppOpTimingsDumpedCallBack != null && !string.IsNullOrEmpty(appOpTimingsDumpedCallBackEventArgs.Id) && _taskCompletionAppOpTimingsDumpedCallBack.ContainsKey(appOpTimingsDumpedCallBackEventArgs.Id))
            {
                _taskCompletionAppOpTimingsDumpedCallBack[appOpTimingsDumpedCallBackEventArgs.Id].TrySetResult(appOpTimingsDumpedCallBackEventArgs);
                _taskCompletionAppOpTimingsDumpedCallBack.Remove(appOpTimingsDumpedCallBackEventArgs.Id);
            }
        }

        private void SetReadyForZomeCalls(string id)
        {
            RetrievingAgentPubKeyAndDnaHash = false;
            
            //TODO: Check this works...
            if (id == "-1")
                _taskCompletionAgentPubKeyAndDnaHashRetrieved[id] = new TaskCompletionSource<AgentPubKeyDnaHash>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (_taskCompletionAgentPubKeyAndDnaHashRetrieved != null && _taskCompletionAgentPubKeyAndDnaHashRetrieved.ContainsKey(id))
            {
                _taskCompletionAgentPubKeyAndDnaHashRetrieved[id].TrySetResult(new AgentPubKeyDnaHash() { AgentPubKey = HoloNETDNA.AgentPubKey, DnaHash = HoloNETDNA.DnaHash });
                _taskCompletionAgentPubKeyAndDnaHashRetrieved.Remove(id);
            }

            //_taskCompletionAgentPubKeyAndDnaHashRetrieved.TrySetResult(new AgentPubKeyDnaHash() { AgentPubKey = HoloNETDNA.AgentPubKey, DnaHash = HoloNETDNA.DnaHash });

            IsReadyForZomesCalls = true;
            ReadyForZomeCallsEventArgs eventArgs = new ReadyForZomeCallsEventArgs(EndPoint, HoloNETDNA.DnaHash, HoloNETDNA.AgentPubKey);
            OnReadyForZomeCalls?.Invoke(this, eventArgs);
            _taskCompletionReadyForZomeCalls.TrySetResult(eventArgs);
        }
    }
}
