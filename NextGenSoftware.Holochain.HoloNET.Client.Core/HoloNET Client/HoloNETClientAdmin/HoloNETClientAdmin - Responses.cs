using System;
using MessagePack;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public partial class HoloNETClientAdmin : HoloNETClientBase//, IHoloNETClientAdmin
    {
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
                        case HoloNETResponseType.AdminAgentPubKeyGenerated:
                            DecodeAgentPubKeyGeneratedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppInstalled:
                            DecodeAppInstalledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppUninstalled:
                            DecodeAppUninstalledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppEnabled:
                            DecodeAppEnabledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppDisabled:
                            DecodeAppDisabledReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminZomeCallCapabilityGranted:
                            DecodeZomeCallCapabilityGrantedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppInterfaceAttached:
                            DecodeAppInterfaceAttachedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminDnaRegistered:
                            DecodeDnaRegisteredReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminDnaDefinitionReturned:
                            DecodeDnasListedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminAppsListed:
                            DecodeAppsListedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminDnasListed:
                            DecodeDnasListedReceived(response, dataReceivedEventArgs);
                            break;

                        case HoloNETResponseType.AdminCellIdsListed:
                            DecodeDnasListedReceived(response, dataReceivedEventArgs);
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

            if (response != null && response.id > 0 && _requestTypeLookup != null && _requestTypeLookup.ContainsKey(response.id.ToString()))
            {
                switch (_requestTypeLookup[response.id.ToString()])
                {
                    case HoloNETRequestType.AdminGenerateAgentPubKey:
                        RaiseAgentPubKeyGeneratedEvent(ProcessResponeError<AgentPubKeyGeneratedCallBackEventArgs>(response, dataReceivedEventArgs, "AdminGenerateAgentPubKey", msg));
                        break;

                    case HoloNETRequestType.AdminInstallApp:
                        RaiseAppInstalledEvent(ProcessResponeError<AppInstalledCallBackEventArgs>(response, dataReceivedEventArgs, "AdminInstallApp", msg));
                        break;

                    case HoloNETRequestType.AdminUninstallApp:
                        RaiseAppUninstalledEvent(ProcessResponeError<AppUninstalledCallBackEventArgs>(response, dataReceivedEventArgs, "AdminUninstallApp", msg));
                        break;

                    case HoloNETRequestType.AdminEnableApp:
                        RaiseAppEnabledEvent(ProcessResponeError<AppEnabledCallBackEventArgs>(response, dataReceivedEventArgs, "AdminEnableApp", msg));
                        break;

                    case HoloNETRequestType.AdminDisableApp:
                        RaiseAppDisabledEvent(ProcessResponeError<AppDisabledCallBackEventArgs>(response, dataReceivedEventArgs, "AdminDisableApp", msg));
                        break;

                    case HoloNETRequestType.AdminGrantZomeCallCapability:
                        RaiseZomeCallCapabilityGrantedEvent(ProcessResponeError<ZomeCallCapabilityGrantedCallBackEventArgs>(response, dataReceivedEventArgs, "AdminGrantZomeCallCapability", msg));
                        break;

                    case HoloNETRequestType.AdminAttachAppInterface:
                        RaiseAppInterfaceAttachedEvent(ProcessResponeError<AppInterfaceAttachedCallBackEventArgs>(response, dataReceivedEventArgs, "AdminGrantZomeCallCapability", msg));
                        break;

                    case HoloNETRequestType.AdminRegisterDna:
                        RaiseDnaRegisteredEvent(ProcessResponeError<DnaRegisteredCallBackEventArgs>(response, dataReceivedEventArgs, "AdminRegisterDna", msg));
                        break;

                    case HoloNETRequestType.AdminListApps:
                        RaiseAppsListedEvent(ProcessResponeError<AppsListedCallBackEventArgs>(response, dataReceivedEventArgs, "AdminListApps", msg));
                        break;

                    case HoloNETRequestType.AdminListDnas:
                        RaiseDnasListedEvent(ProcessResponeError<DnasListedCallBackEventArgs>(response, dataReceivedEventArgs, "AdminListDnas", msg));
                        break;

                    case HoloNETRequestType.AdminListCellIds:
                        RaiseCellIdsListedEvent(ProcessResponeError<CellIdsListedCallBackEventArgs>(response, dataReceivedEventArgs, "AdminListCellIds", msg));
                        break;
                }
            }

            return msg;
        }

        private void DecodeAgentPubKeyGeneratedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            AgentPubKeyGeneratedCallBackEventArgs args = CreateHoloNETArgs<AgentPubKeyGeneratedCallBackEventArgs>(response, dataReceivedEventArgs);

            try
            {
                Logger.Log("ADMIN AGENT PUB KEY GENERATED DATA DETECTED\n", LogType.Info);
                AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);
                args.AgentPubKey = ConvertHoloHashToString(appResponse.data);
                args.AppResponse = appResponse;

                Logger.Log($"AGENT PUB KEY GENERATED: {args.AgentPubKey}\n", LogType.Info);

                if (_updateDnaHashAndAgentPubKey)
                    HoloNETDNA.AgentPubKey = args.AgentPubKey;
            }
            catch (Exception ex)
            {
                HandleError(args, $"An unknown error occurred in HoloNETClient.DecodeAgentPubKeyGeneratedReceived. Reason: {ex}");
            }

            RaiseAgentPubKeyGeneratedEvent(args);
        }

        private void DecodeAppInstalledReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            AppInstalledCallBackEventArgs args = new AppInstalledCallBackEventArgs();
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppInstalledReceived. Reason: ";

            try
            {
                Logger.Log("ADMIN APP INSTALLED DATA DETECTED\n", LogType.Info);

                AppInfoResponse appInfoResponse = MessagePackSerializer.Deserialize<AppInfoResponse>(response.data, messagePackSerializerOptions);
                args = CreateHoloNETArgs<AppInstalledCallBackEventArgs>(response, dataReceivedEventArgs);

                if (appInfoResponse != null)
                {
                    appInfoResponse.data = ProcessAppInfo(appInfoResponse.data, args);
                    args.AppInfoResponse = appInfoResponse;
                }
                else
                    HandleError(args, $"{errorMessage} appInfoResponse is null.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseAppInstalledEvent(args);
        }

        private void DecodeAppUninstalledReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            AppUninstalledCallBackEventArgs args = new AppUninstalledCallBackEventArgs();
            args.HoloNETResponseType = HoloNETResponseType.AdminAppUninstalled;

            try
            {
                Logger.Log("ADMIN APP UNINSTALLED DATA DETECTED\n", LogType.Info);
                args = CreateHoloNETArgs<AppUninstalledCallBackEventArgs>(response, dataReceivedEventArgs);

                if (_uninstallingAppLookup != null && _uninstallingAppLookup.ContainsKey(response.id.ToString()))
                {
                    args.InstalledAppId = _uninstallingAppLookup[response.id.ToString()];
                    _uninstallingAppLookup.Remove(response.id.ToString());
                }
            }
            catch (Exception ex)
            {
                HandleError(args, $"An unknown error occurred in HoloNETClient.DecodeAppUninstalledReceived. Reason: {ex}");
            }

            RaiseAppUninstalledEvent(args);
        }

        private void DecodeAppEnabledReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppEnabledReceived. Reason: ";
            AppEnabledCallBackEventArgs args = CreateHoloNETArgs<AppEnabledCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppEnabled;

            try
            {
                Logger.Log("ADMIN APP ENABLED DATA DETECTED\n", LogType.Info);
                EnableAppResponse enableAppResponse = MessagePackSerializer.Deserialize<EnableAppResponse>(response.data, messagePackSerializerOptions);

                if (enableAppResponse != null)
                {
                    enableAppResponse.data.app = ProcessAppInfo(enableAppResponse.data.app, args);
                    args.AppInfoResponse = new AppInfoResponse() { data = enableAppResponse.data.app };
                    args.Errors = enableAppResponse.data.errors; //TODO: Need to find out what this contains and the correct data structure.
                }
                else
                {
                    HandleError(args, $"{errorMessage} An error occurred deserialzing EnableAppResponse from the Holochain Conductor.");
                }
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseAppEnabledEvent(args);
        }

        private void DecodeAppDisabledReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppEnabledReceived. Reason: ";
            AppDisabledCallBackEventArgs args = CreateHoloNETArgs<AppDisabledCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppDisabled;

            try
            {
                Logger.Log("ADMIN APP DISABLED DATA DETECTED\n", LogType.Info);

                if (_disablingAppLookup != null && _disablingAppLookup.ContainsKey(response.id.ToString()))
                {
                    args.InstalledAppId = _disablingAppLookup[response.id.ToString()];
                    _disablingAppLookup.Remove(response.id.ToString());
                }
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseAppDisabledEvent(args);
        }

        private void DecodeZomeCallCapabilityGrantedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeZomeCallCapabilityGrantedReceived. Reason: ";
            ZomeCallCapabilityGrantedCallBackEventArgs args = CreateHoloNETArgs<ZomeCallCapabilityGrantedCallBackEventArgs>(response, dataReceivedEventArgs);

            try
            {
                Logger.Log("ADMIN ZOME CALL CAPABILITY GRANTED\n", LogType.Info);
                args.HoloNETResponseType = HoloNETResponseType.AdminZomeCallCapabilityGranted;
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseZomeCallCapabilityGrantedEvent(args);
        }

        private void DecodeAppInterfaceAttachedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppInterfaceAttachedReceived. Reason: ";
            AppInterfaceAttachedCallBackEventArgs args = CreateHoloNETArgs<AppInterfaceAttachedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppInterfaceAttached;

            try
            {
                Logger.Log("ADMIN APP INTERFACE ATTACHED\n", LogType.Info);
                //object attachAppInterfaceResponse = MessagePackSerializer.Deserialize<object>(response.data, messagePackSerializerOptions);
                AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);

                if (appResponse != null)
                {
                    args.Port = Convert.ToUInt16(appResponse.data["port"]);

                    //AttachAppInterfaceResponse attachAppInterfaceResponse = MessagePackSerializer.Deserialize<AttachAppInterfaceResponse>(appResponse.data, messagePackSerializerOptions);
                    //attachAppInterfaceResponse.Port = attachAppInterfaceResponse.Port;
                }
                else
                    HandleError(args, $"{errorMessage} Error occured in HoloNETClient.DecodeAppInterfaceAttachedReceived. attachAppInterfaceResponse is null.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseAppInterfaceAttachedEvent(args);
        }

        private void DecodeDnaRegisteredReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeDnaRegisteredReceived. Reason: ";
            DnaRegisteredCallBackEventArgs args = CreateHoloNETArgs<DnaRegisteredCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminDnaRegistered;

            try
            {
                Logger.Log("ADMIN DNA REGISTERED\n", LogType.Info);
                byte[] responseData = MessagePackSerializer.Deserialize<byte[]>(response.data, messagePackSerializerOptions);

                if (responseData != null)
                    args.HoloHash = responseData;
                else
                    HandleError(args, $"{errorMessage} ResponseData failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseDnaRegisteredEvent(args);
        }

        private void DecodeDnaDefinitionReturned(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeDnaDefinitionReturned. Reason: ";
            DnaDefinitionReturnedCallBackEventArgs args = CreateHoloNETArgs<DnaDefinitionReturnedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminDnaDefinitionReturned;

            try
            {
                Logger.Log("ADMIN DNA DEFINTION RETURNED\n", LogType.Info);
                DnaDefinitionResponse dnaDefinition = MessagePackSerializer.Deserialize<DnaDefinitionResponse>(response.data, messagePackSerializerOptions);

                if (dnaDefinition != null)
                    args.DnaDefinition = dnaDefinition;
                else
                    HandleError(args, $"{errorMessage} dnaDefinition failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseDnaDefinitionReturnedEvent(args);
        }

        private void DecodeAppsListedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeAppsListedReceived. Reason: ";
            AppsListedCallBackEventArgs args = CreateHoloNETArgs<AppsListedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppInterfaceAttached;

            try
            {
                Logger.Log("ADMIN APPS LISTED\n", LogType.Info);
                ListAppsResponse appResponse = MessagePackSerializer.Deserialize<ListAppsResponse>(response.data, messagePackSerializerOptions);

                if (appResponse != null)
                {
                    foreach (AppInfo appInfo in appResponse.Apps)
                    {
                        AppInfoCallBackEventArgs appInfoArgs = new AppInfoCallBackEventArgs();
                        AppInfo processedAppInfo = ProcessAppInfo(appInfo, appInfoArgs);

                        if (!appInfoArgs.IsError)
                            args.Apps.Add(processedAppInfo);
                        else
                        {
                            args.IsError = true;
                            args.Message = $"{args.Message} # {appInfoArgs.Message}";
                        }
                    }

                    if (args.IsError)
                        HandleError(args.Message);
                }
                else
                    HandleError(args, $"{errorMessage} appResponse failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseAppsListedEvent(args);
        }

        private void DecodeDnasListedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeDnasListedReceived. Reason: ";
            DnasListedCallBackEventArgs args = CreateHoloNETArgs<DnasListedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminAppInterfaceAttached;

            try
            {
                Logger.Log("ADMIN DNA's LISTED\n", LogType.Info);
                byte[][] dataResponse = MessagePackSerializer.Deserialize<byte[][]>(response.data, messagePackSerializerOptions);

                if (dataResponse != null)
                    args.Dnas = dataResponse;
                else
                    HandleError(args, $"{errorMessage} dataResponse failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseDnasListedEvent(args);
        }

        private void DecodeCellIdsListedReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string errorMessage = "An unknown error occurred in HoloNETClient.DecodeCellIdsListedReceived. Reason: ";
            CellIdsListedCallBackEventArgs args = CreateHoloNETArgs<CellIdsListedCallBackEventArgs>(response, dataReceivedEventArgs);
            args.HoloNETResponseType = HoloNETResponseType.AdminCellIdsListed;

            try
            {
                Logger.Log("ADMIN CELLID's LISTED\n", LogType.Info);
                byte[][][] dataResponse = MessagePackSerializer.Deserialize<byte[][][]>(response.data, messagePackSerializerOptions);

                if (dataResponse != null)
                    args.CellIds = dataResponse;
                else
                    HandleError(args, $"{errorMessage} dataResponse failed to deserialize.");
            }
            catch (Exception ex)
            {
                HandleError(args, $"{errorMessage} {ex}");
            }

            RaiseCellIdsListedEvent(args);
        }

        private void RaiseAgentPubKeyGeneratedEvent(AgentPubKeyGeneratedCallBackEventArgs adminAgentPubKeyGeneratedCallBackEventArgs)
        {
            LogEvent("AdminAgentPubKeyGenerated", adminAgentPubKeyGeneratedCallBackEventArgs);
            OnAgentPubKeyGeneratedCallBack?.Invoke(this, adminAgentPubKeyGeneratedCallBackEventArgs);

            if (_taskCompletionAgentPubKeyGeneratedCallBack != null && !string.IsNullOrEmpty(adminAgentPubKeyGeneratedCallBackEventArgs.Id) && _taskCompletionAgentPubKeyGeneratedCallBack.ContainsKey(adminAgentPubKeyGeneratedCallBackEventArgs.Id))
                _taskCompletionAgentPubKeyGeneratedCallBack[adminAgentPubKeyGeneratedCallBackEventArgs.Id].SetResult(adminAgentPubKeyGeneratedCallBackEventArgs);
        }

        private void RaiseAppInstalledEvent(AppInstalledCallBackEventArgs adminAppInstalledCallBackEventArgs)
        {
            LogEvent("AdminAppInstalled", adminAppInstalledCallBackEventArgs);
            OnAppInstalledCallBack?.Invoke(this, adminAppInstalledCallBackEventArgs);

            if (_taskCompletionAppInstalledCallBack != null && !string.IsNullOrEmpty(adminAppInstalledCallBackEventArgs.Id) && _taskCompletionAppInstalledCallBack.ContainsKey(adminAppInstalledCallBackEventArgs.Id))
                _taskCompletionAppInstalledCallBack[adminAppInstalledCallBackEventArgs.Id].SetResult(adminAppInstalledCallBackEventArgs);
        }

        private void RaiseAppUninstalledEvent(AppUninstalledCallBackEventArgs adminAppUninstalledCallBackEventArgs)
        {
            LogEvent("AdminAppUninstalled", adminAppUninstalledCallBackEventArgs);
            OnAppUninstalledCallBack?.Invoke(this, adminAppUninstalledCallBackEventArgs);

            if (_taskCompletionAppUninstalledCallBack != null && !string.IsNullOrEmpty(adminAppUninstalledCallBackEventArgs.Id) && _taskCompletionAppUninstalledCallBack.ContainsKey(adminAppUninstalledCallBackEventArgs.Id))
                _taskCompletionAppUninstalledCallBack[adminAppUninstalledCallBackEventArgs.Id].SetResult(adminAppUninstalledCallBackEventArgs);
        }

        private void RaiseAppEnabledEvent(AppEnabledCallBackEventArgs adminAppEnabledCallBackEventArgs)
        {
            LogEvent("AdminAppEnabled", adminAppEnabledCallBackEventArgs);
            OnAppEnabledCallBack?.Invoke(this, adminAppEnabledCallBackEventArgs);

            if (_taskCompletionAppEnabledCallBack != null && !string.IsNullOrEmpty(adminAppEnabledCallBackEventArgs.Id) && _taskCompletionAppEnabledCallBack.ContainsKey(adminAppEnabledCallBackEventArgs.Id))
                _taskCompletionAppEnabledCallBack[adminAppEnabledCallBackEventArgs.Id].SetResult(adminAppEnabledCallBackEventArgs);
        }

        private void RaiseAppDisabledEvent(AppDisabledCallBackEventArgs adminAppDisabledCallBackEventArgs)
        {
            LogEvent("AdminAppDisabled", adminAppDisabledCallBackEventArgs);
            OnAppDisabledCallBack?.Invoke(this, adminAppDisabledCallBackEventArgs);

            if (_taskCompletionAppDisabledCallBack != null && !string.IsNullOrEmpty(adminAppDisabledCallBackEventArgs.Id) && _taskCompletionAppDisabledCallBack.ContainsKey(adminAppDisabledCallBackEventArgs.Id))
                _taskCompletionAppDisabledCallBack[adminAppDisabledCallBackEventArgs.Id].SetResult(adminAppDisabledCallBackEventArgs);
        }

        private void RaiseZomeCallCapabilityGrantedEvent(ZomeCallCapabilityGrantedCallBackEventArgs adminZomeCallCapabilityGrantedCallBackEventArgs)
        {
            LogEvent("AdminZomeCallCapabilityGranted", adminZomeCallCapabilityGrantedCallBackEventArgs);
            OnZomeCallCapabilityGrantedCallBack?.Invoke(this, adminZomeCallCapabilityGrantedCallBackEventArgs);

            if (_taskCompletionZomeCapabilityGrantedCallBack != null && !string.IsNullOrEmpty(adminZomeCallCapabilityGrantedCallBackEventArgs.Id) && _taskCompletionZomeCapabilityGrantedCallBack.ContainsKey(adminZomeCallCapabilityGrantedCallBackEventArgs.Id))
                _taskCompletionZomeCapabilityGrantedCallBack[adminZomeCallCapabilityGrantedCallBackEventArgs.Id].SetResult(adminZomeCallCapabilityGrantedCallBackEventArgs);
        }

        private void RaiseAppInterfaceAttachedEvent(AppInterfaceAttachedCallBackEventArgs adminAppInterfaceAttachedCallBackEventArgs)
        {
            LogEvent("AdminAppInterfaceAttached", adminAppInterfaceAttachedCallBackEventArgs);
            OnAppInterfaceAttachedCallBack?.Invoke(this, adminAppInterfaceAttachedCallBackEventArgs);

            if (_taskCompletionAppInterfaceAttachedCallBack != null && !string.IsNullOrEmpty(adminAppInterfaceAttachedCallBackEventArgs.Id) && _taskCompletionAppInterfaceAttachedCallBack.ContainsKey(adminAppInterfaceAttachedCallBackEventArgs.Id))
                _taskCompletionAppInterfaceAttachedCallBack[adminAppInterfaceAttachedCallBackEventArgs.Id].SetResult(adminAppInterfaceAttachedCallBackEventArgs);
        }

        private void RaiseDnaRegisteredEvent(DnaRegisteredCallBackEventArgs dnaRegisteredCallBackEventArgs)
        {
            LogEvent("AdminDnaRegistered", dnaRegisteredCallBackEventArgs);
            OnDnaRegisteredCallBack?.Invoke(this, dnaRegisteredCallBackEventArgs);

            if (_taskCompletionDnaRegisteredCallBack != null && !string.IsNullOrEmpty(dnaRegisteredCallBackEventArgs.Id) && _taskCompletionDnaRegisteredCallBack.ContainsKey(dnaRegisteredCallBackEventArgs.Id))
                _taskCompletionDnaRegisteredCallBack[dnaRegisteredCallBackEventArgs.Id].SetResult(dnaRegisteredCallBackEventArgs);
        }

        private void RaiseDnaDefinitionReturnedEvent(DnaDefinitionReturnedCallBackEventArgs dnaDefinitionReturnedCallBackEventArgs)
        {
            LogEvent("AdminDnaDefintionReturned", dnaDefinitionReturnedCallBackEventArgs);
            OnDnaDefinitionReturnedCallBack?.Invoke(this, dnaDefinitionReturnedCallBackEventArgs);

            if (_taskCompletionDnaDefinitionReturnedCallBack != null && !string.IsNullOrEmpty(dnaDefinitionReturnedCallBackEventArgs.Id) && _taskCompletionDnaDefinitionReturnedCallBack.ContainsKey(dnaDefinitionReturnedCallBackEventArgs.Id))
                _taskCompletionDnaDefinitionReturnedCallBack[dnaDefinitionReturnedCallBackEventArgs.Id].SetResult(dnaDefinitionReturnedCallBackEventArgs);
        }

        private void RaiseAppsListedEvent(AppsListedCallBackEventArgs adminAppsListedCallBackEventArgs)
        {
            LogEvent("AdminAppsListed", adminAppsListedCallBackEventArgs);
            OnAppsListedCallBack?.Invoke(this, adminAppsListedCallBackEventArgs);

            if (_taskCompletionAppsListedCallBack != null && !string.IsNullOrEmpty(adminAppsListedCallBackEventArgs.Id) && _taskCompletionAppsListedCallBack.ContainsKey(adminAppsListedCallBackEventArgs.Id))
                _taskCompletionAppsListedCallBack[adminAppsListedCallBackEventArgs.Id].SetResult(adminAppsListedCallBackEventArgs);
        }

        private void RaiseDnasListedEvent(DnasListedCallBackEventArgs dnasListedCallBackEventArgs)
        {
            LogEvent("AdminDnasListed", dnasListedCallBackEventArgs);
            OnDnasListedCallBack?.Invoke(this, dnasListedCallBackEventArgs);

            if (_taskCompletionDnasListedCallBack != null && !string.IsNullOrEmpty(dnasListedCallBackEventArgs.Id) && _taskCompletionDnasListedCallBack.ContainsKey(dnasListedCallBackEventArgs.Id))
                _taskCompletionDnasListedCallBack[dnasListedCallBackEventArgs.Id].SetResult(dnasListedCallBackEventArgs);
        }

        private void RaiseCellIdsListedEvent(CellIdsListedCallBackEventArgs cellIdsListedCallBackEventArgs)
        {
            LogEvent("AdminCellIdsListed", cellIdsListedCallBackEventArgs);
            OnCellIdsListedCallBack?.Invoke(this, cellIdsListedCallBackEventArgs);

            if (_taskCompletionCellIdsListedCallBack != null && !string.IsNullOrEmpty(cellIdsListedCallBackEventArgs.Id) && _taskCompletionCellIdsListedCallBack.ContainsKey(cellIdsListedCallBackEventArgs.Id))
                _taskCompletionCellIdsListedCallBack[cellIdsListedCallBackEventArgs.Id].SetResult(cellIdsListedCallBackEventArgs);
        }
    }
}