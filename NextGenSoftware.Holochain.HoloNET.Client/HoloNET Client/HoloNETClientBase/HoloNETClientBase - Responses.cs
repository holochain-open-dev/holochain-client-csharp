using System;
using System.Linq;
using MessagePack;
using NextGenSoftware.Logging;
using NextGenSoftware.Utilities;
using NextGenSoftware.Utilities.ExtentionMethods;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientBase : IHoloNETClientBase
    {
        protected virtual IHoloNETResponse ProcessDataReceived(WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string rawBinaryDataAsString = "";
            string rawBinaryDataDecoded = "";
            IHoloNETResponse response = null;

            try
            {
                rawBinaryDataAsString = DataHelper.ConvertBinaryDataToString(dataReceivedEventArgs.RawBinaryData);
                rawBinaryDataDecoded = DataHelper.DecodeBinaryDataAsUTF8(dataReceivedEventArgs.RawBinaryData);

                response = DecodeDataReceived(dataReceivedEventArgs.RawBinaryData, dataReceivedEventArgs);

                if (response != null && !response.IsError)
                {
                    switch (response.HoloNETResponseType)
                    {
                        case HoloNETResponseType.Error:
                            ProcessErrorReceivedFromConductor(response, dataReceivedEventArgs);
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

        protected virtual string ProcessErrorReceivedFromConductor(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string msg = $"Error in HoloNETClient.ProcessErrorReceivedFromConductor method. Error received from Holochain Conductor: ";

            if (response != null)
            {
                try
                {
                    AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);

                    if (appResponse != null)
                        msg = $"'{msg}{appResponse.type}: {appResponse.data["type"]}: {appResponse.data["data"]}.'";
                    else
                        msg = $"'{msg}{dataReceivedEventArgs.RawBinaryDataDecoded}'";
                }
                catch (Exception ex)
                {
                    msg = $"'{msg}{dataReceivedEventArgs.RawBinaryDataDecoded}'";
                }
            }
            else
                msg = $"'{msg}{dataReceivedEventArgs.RawBinaryDataDecoded}'";

            HandleError(msg, null);
            return msg;
        }

        protected virtual T ProcessResponeError<T>(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string responseErrorType, string msg) where T : HoloNETDataReceivedBaseEventArgs, new()
        {
            Logger.Log($"{responseErrorType} ERROR", LogType.Error);
            T args = CreateHoloNETArgs<T>(response, dataReceivedEventArgs);
            args.IsError = true;
            //args.IsCallSuccessful = false;
            args.Message = msg;
            return args;
        }

        protected virtual IHoloNETResponse DecodeDataReceived(byte[] rawBinaryData, WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            IHoloNETResponse response = null;
            HoloNETDataReceivedEventArgs holoNETDataReceivedEventArgs = new HoloNETDataReceivedEventArgs();

            try
            {
                string id = "";
                string rawBinaryDataAfterMessagePackDecodeAsString = "";
                string rawBinaryDataAfterMessagePackDecodeDecoded = "";

                byte[] data = rawBinaryData.ToArray();
                rawBinaryData.CopyTo(data, 0);


                response = MessagePackSerializer.Deserialize<HoloNETResponse>(data, messagePackSerializerOptions);
                AppResponse appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, messagePackSerializerOptions);

                id = response.id.ToString();
                rawBinaryDataAfterMessagePackDecodeAsString = DataHelper.ConvertBinaryDataToString(response.data);
                rawBinaryDataAfterMessagePackDecodeDecoded = DataHelper.DecodeBinaryDataAsUTF8(response.data);

                Logger.Log($"Id: {response.id} Type: {response.type}: {response.type} Internal Type: {appResponse.type}", LogType.Info);
                //Logger.Log(string.Concat("Raw Data Bytes Received After MessagePack Decode: ", rawBinaryDataAfterMessagePackDecodeAsString), LogType.Debug);
                //Logger.Log(string.Concat("Raw Data Bytes Decoded After MessagePack Decode: ", rawBinaryDataAfterMessagePackDecodeDecoded), LogType.Debug);

                switch (appResponse.type)
                {
                    case "zome-response":
                        response.HoloNETResponseType = HoloNETResponseType.ZomeResponse;
                        break;

                    case "signal":
                        response.HoloNETResponseType = HoloNETResponseType.Signal;
                        break;

                    case "app_info":
                        response.HoloNETResponseType = HoloNETResponseType.AppInfo;
                        break;

                    case "agent_pub_key_generated":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAgentPubKeyGenerated;
                        break;

                    case "app_installed":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppInstalled;
                        break;

                    case "app_uninstalled":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppUninstalled;
                        break;

                    case "app_enabled":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppEnabled;
                        break;

                    case "app_disabled":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppDisabled;
                        break;

                    case "zome_call_capability_granted":
                        response.HoloNETResponseType = HoloNETResponseType.AdminZomeCallCapabilityGranted;
                        break;

                    case "app_interface_attached":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppInterfaceAttached;
                        break;

                    case "apps_listed":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppsListed;
                        break;

                    case "dnas_listed":
                        response.HoloNETResponseType = HoloNETResponseType.AdminDnasListed;
                        break;

                    case "cell_ids_listed":
                        response.HoloNETResponseType = HoloNETResponseType.AdminCellIdsListed;
                        break;

                    case "app_interfaces_listed":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppInterfacesListed;
                        break;

                    case "dna_registered":
                        response.HoloNETResponseType = HoloNETResponseType.AdminDnaRegistered;
                        break;

                    case "dna_definition_returned":
                        response.HoloNETResponseType = HoloNETResponseType.AdminDnaDefinitionReturned;
                        break;

                    case "agent_info":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAgentInfoReturned;
                        break;

                    case "agent_info_added":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAgentInfoAdded;
                        break;

                    case "coordinators_updated":
                        response.HoloNETResponseType = HoloNETResponseType.AdminCoordinatorsUpdated;
                        break;

                    case "clone_cell_deleted":
                        response.HoloNETResponseType = HoloNETResponseType.AdminCloneCellDeleted;
                        break;

                    case "state_dumped":
                        response.HoloNETResponseType = HoloNETResponseType.AdminStateDumped;
                        break;

                    case "full_state_dumped":
                        response.HoloNETResponseType = HoloNETResponseType.AdminFullStateDumped;
                        break;

                    case "network_metrics_dumped":
                        response.HoloNETResponseType = HoloNETResponseType.AdminNetworkMetricsDumped;
                        break;

                    case "network_stats_dumped":
                        response.HoloNETResponseType = HoloNETResponseType.AdminNetworkStatsDumped;
                        break;

                    case "network_storage_info":
                        response.HoloNETResponseType = HoloNETResponseType.AdminStorageInfoReturned;
                        break;

                    case "records_grafted":
                        response.HoloNETResponseType = HoloNETResponseType.AdminRecordsGrafted;
                        break;

                    case "admin_interfaces_added":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAdminInterfacesAdded;
                        break;

                    // New in Holochain 0.6.1 - Admin API.

                    case "zome_call_capability_revoked":
                        response.HoloNETResponseType = HoloNETResponseType.AdminZomeCallCapabilityRevoked;
                        break;

                    case "capability_grants_info":
                        response.HoloNETResponseType = HoloNETResponseType.AdminCapabilityGrantsInfoReturned;
                        break;

                    case "app_authentication_token_issued":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppAuthenticationTokenIssued;
                        break;

                    case "app_authentication_token_revoked":
                        response.HoloNETResponseType = HoloNETResponseType.AdminAppAuthenticationTokenRevoked;
                        break;

                    case "compatible_cells":
                        response.HoloNETResponseType = HoloNETResponseType.AdminCompatibleCellsReturned;
                        break;

                    // peer_meta_info is shared between Admin and App interfaces - this client class
                    // (HoloNETClientBase) is shared by both Admin and App clients, so we route to the
                    // appropriate AdminPeerMetaInfoReturned/AppPeerMetaInfoReturned value based on which
                    // partial class is processing the response (Admin vs App-specific handling occurs
                    // in the derived classes' DecodeXReceived dispatch, both react to the response type
                    // set here - default to the Admin variant, App-side overrides as needed below).
                    case "peer_meta_info":
                        response.HoloNETResponseType = this is HoloNETClientAdmin ? HoloNETResponseType.AdminPeerMetaInfoReturned : HoloNETResponseType.AppPeerMetaInfoReturned;
                        break;

                    // New in Holochain 0.6.1 - App API.

                    case "clone_cell_created":
                        response.HoloNETResponseType = HoloNETResponseType.AppCloneCellCreated;
                        break;

                    case "clone_cell_enabled":
                        response.HoloNETResponseType = HoloNETResponseType.AppCloneCellEnabled;
                        break;

                    case "clone_cell_disabled":
                        response.HoloNETResponseType = HoloNETResponseType.AppCloneCellDisabled;
                        break;

                    case "countersigning_session_state":
                        response.HoloNETResponseType = HoloNETResponseType.AppCountersigningSessionStateReturned;
                        break;

                    case "countersigning_session_abandoned":
                        response.HoloNETResponseType = HoloNETResponseType.AppCountersigningSessionAbandoned;
                        break;

                    case "publish_countersigning_session_triggered":
                        response.HoloNETResponseType = HoloNETResponseType.AppPublishCountersigningSessionTriggered;
                        break;

                    case "list_wasm_host_functions":
                        response.HoloNETResponseType = HoloNETResponseType.AppWasmHostFunctionsListed;
                        break;

                    // AppResponse::Ok is a generic no-payload success response (wire type "ok").
                    // ProvideMemproofs is currently the only AppRequest wired through HoloNET that
                    // expects this generic Ok response back, so we map it directly to
                    // AppMemproofsProvided here. If other Ok-returning AppRequest methods are wired
                    // up in future, this mapping will need to become request-id aware instead.
                    case "ok":
                        response.HoloNETResponseType = HoloNETResponseType.AppMemproofsProvided;
                        break;

                    case "error":
                        response.HoloNETResponseType = HoloNETResponseType.Error;
                        break;
                }

                holoNETDataReceivedEventArgs = CreateHoloNETArgs<HoloNETDataReceivedEventArgs>(response, dataReceivedEventArgs);

                if (HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour != EnforceRequestToResponseIdMatchingBehaviour.Ignore
                    && !_pendingRequests.Contains(id))
                {
                    holoNETDataReceivedEventArgs.IsError = HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour == EnforceRequestToResponseIdMatchingBehaviour.Error;
                    //holoNETDataReceivedEventArgs.IsCallSuccessful = false;
                    holoNETDataReceivedEventArgs.Message = $"The id returned in the response ({id}) does not match any pending request.";

                    if (holoNETDataReceivedEventArgs.IsError)
                        holoNETDataReceivedEventArgs.Message = string.Concat(holoNETDataReceivedEventArgs.Message, " HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Error so Aborting Request.");
                }

                _pendingRequests.Remove(id);
                response.IsError = holoNETDataReceivedEventArgs.IsError;
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occurred in HoloNETClient.DecodeDataReceived. Reason: {ex}";
                holoNETDataReceivedEventArgs.IsError = true;
                holoNETDataReceivedEventArgs.Message = msg;
                //holoNETDataReceivedEventArgs.IsCallSuccessful = false;
                HandleError(msg, ex);
            }

            OnDataReceived?.Invoke(this, holoNETDataReceivedEventArgs);
            return response;
        }

        protected virtual AppInfo ProcessAppInfo(AppInfo appInfo, AppInfoCallBackEventArgs args, bool log = true)
        {
            if (appInfo != null)
            {
                string agentPubKey = ConvertHoloHashToString(appInfo.agent_pub_key);
                string agentPubKeyFromCell = "";
                string dnaHash = "";

                if (appInfo.cell_info != null)
                {
                    var first = appInfo.cell_info.First();

                    if (first.Value != null && first.Value.Count > 0 && first.Value[0].Provisioned != null && first.Value[0].Provisioned.cell_id != null && first.Value[0].Provisioned.cell_id.Length == 2 && first.Value[0].Provisioned.cell_id[0] != null && first.Value[0].Provisioned.cell_id[1] != null)
                    {
                        agentPubKeyFromCell = ConvertHoloHashToString(first.Value[0].Provisioned.cell_id[1]);
                        dnaHash = ConvertHoloHashToString(first.Value[0].Provisioned.cell_id[0]);
                        appInfo.CellId = first.Value[0].Provisioned.cell_id;
                        args.CellId = appInfo.CellId;

                        if (agentPubKeyFromCell != agentPubKey)
                        {
                            args.Message = $"Error occured in HoloNETClient.ProcessAppInfo. appInfoResponse.data.agent_pub_key and agentPubKey dervived from cell data do not match! appInfoResponse.data.agent_pub_key = {agentPubKey}, cellData = {agentPubKeyFromCell} ";
                            args.IsError = true;
                        }

                        //Conductor time is in microseconds so we need to multiple it by 10 to get the ticks needed to convert it to a DateTime.
                        //Also UNIX dateime (which hc uses) starts from 1970).
                        first.Value[0].Provisioned.dna_modifiers.OriginTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(first.Value[0].Provisioned.dna_modifiers.origin_time * 10);
                    }
                }

                if (_updateDnaHashAndAgentPubKey)
                {
                    if (!string.IsNullOrEmpty(agentPubKey))
                        HoloNETDNA.AgentPubKey = agentPubKey;

                    if (!string.IsNullOrEmpty(dnaHash))
                        HoloNETDNA.DnaHash = dnaHash;

                    if (args.CellId != null)
                        HoloNETDNA.CellId = args.CellId;
                }
                
                if (log)
                    Logger.Log($"hAPP {appInfo.installed_app_id}: AgentPubKey: {agentPubKey}, DnaHash: {dnaHash}", LogType.Info);

                args.AgentPubKey = agentPubKey;
                args.DnaHash = dnaHash;
                args.InstalledAppId = appInfo.installed_app_id;
                args.AppStatus = appInfo.AppStatus;
                args.AppStatusReason = appInfo.AppStatusReason;
                args.AppManifest = appInfo.manifest;

                if (appInfo.cell_info != null && appInfo.cell_info.Count > 0)
                {
                    var firstCells = appInfo.cell_info.First().Value;
                    if (firstCells != null && firstCells.Count > 0)
                        args.CellType = firstCells[0].CellInfoType;
                }

                appInfo.AgentPubKey = agentPubKey;
                appInfo.DnaHash = dnaHash;

                if (appInfo.manifest != null)
                {
                    if (appInfo.manifest.roles != null && appInfo.manifest.roles.Length > 0)
                    {
                        //string strategy = appInfo.manifest.roles[0].provisioning.strategy.ToPascalCase();

                        if (appInfo.manifest.roles[0].provisioning != null)
                            appInfo.manifest.roles[0].provisioning.StrategyType = (ProvisioningStrategyType)Enum.Parse(typeof(ProvisioningStrategyType), appInfo.manifest.roles[0].provisioning.strategy.ToPascalCase());
                    }
                }
            }
            else
            {
                args.Message = "Error occured in HoloNETClient.ProcessAppInfo. appInfo is null.";
                args.IsError = true;
            }

            return appInfo;
        }

        protected virtual T CreateHoloNETArgs<T>(IHoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs) where T : HoloNETDataReceivedBaseEventArgs, new()
        {
            return new T()
            {
                EndPoint = dataReceivedEventArgs != null ? dataReceivedEventArgs.EndPoint : null,
                Id = response != null ? response.id.ToString() : "",
                //IsCallSuccessful = true,
                RawBinaryData = dataReceivedEventArgs != null ? dataReceivedEventArgs.RawBinaryData : null,
                RawBinaryDataAsString = dataReceivedEventArgs != null ? dataReceivedEventArgs.RawBinaryDataAsString : "",
                RawBinaryDataDecoded = dataReceivedEventArgs != null ? dataReceivedEventArgs.RawBinaryDataDecoded : "",
                RawJSONData = dataReceivedEventArgs != null ? dataReceivedEventArgs.RawJSONData : "",
                WebSocketResult = dataReceivedEventArgs != null ? dataReceivedEventArgs.WebSocketResult : null
            };
        }

        protected virtual T1 CopyHoloNETArgs<T1>(T1 sourceArgs, T1 targetArgs) where T1 : HoloNETDataReceivedBaseEventArgs
        {
            targetArgs.EndPoint = sourceArgs.EndPoint;
            targetArgs.Exception = sourceArgs.Exception;
            targetArgs.Id = sourceArgs.Id;
            //targetArgs.IsCallSuccessful = sourceArgs.IsCallSuccessful;
            targetArgs.IsError = sourceArgs.IsError;
            targetArgs.Message = sourceArgs.Message;
            targetArgs.RawBinaryData = sourceArgs.RawBinaryData;
            targetArgs.RawBinaryDataDecoded = sourceArgs.RawBinaryDataDecoded;
            targetArgs.RawBinaryDataAsString = sourceArgs.RawBinaryDataAsString;
            targetArgs.RawJSONData = sourceArgs.RawJSONData;
            targetArgs.WebSocketResult = sourceArgs.WebSocketResult;

            return targetArgs;
        }
    }
}