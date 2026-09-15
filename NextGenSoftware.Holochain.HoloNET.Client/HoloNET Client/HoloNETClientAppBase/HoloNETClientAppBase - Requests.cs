using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Generic;
using MessagePack;
using Blake2Fast;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using Sodium;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientAppBase : HoloNETClientBase, IHoloNETClientAppBase
    {
        /// <summary>
        /// This method will retrieve the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retrieving from the Conductor first. It will call RetrieveAgentPubKeyAndDnaHashFromConductor and RetrieveAgentPubKeyAndDnaHashFromSandboxAsync internally.
        /// </summary>
        /// <param name="installedAppId">If this is set then HoloNET will retreive the agentPubKey and DnaHash for this AppId. If it is not set it will retreive it for the InstalledAppId defined in the HoloNETDNA. This field is optional.</param>
        /// <param name="roleName">If this is set then HoloNET will look up the CellType and return it in the OnAppInfoCallBack event. This field is optional.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHash(string installedAppId = null, string roleName = null, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            return RetrieveAgentPubKeyAndDnaHashAsync(installedAppId, roleName, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved).Result;
        }

        /// <summary>
        /// This method will retrieve the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retrieving from the Conductor first. It will call RetrieveAgentPubKeyAndDnaHashFromConductor and RetrieveAgentPubKeyAndDnaHashFromSandboxAsync internally.
        /// </summary>
        /// <param name="installedAppId">If this is set then HoloNET will retreive the agentPubKey and DnaHash for this AppId. If it is not set it will retreive it for the InstalledAppId defined in the HoloNETDNA. This field is optional.</param>
        /// <param name="roleName">If this is set then HoloNET will look up the CellType and return it in the OnAppInfoCallBack event. This field is optional.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then raise the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromConductor">Set this to true for HoloNET to automatically retrieve the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashFromSandbox">Set this to true if you wish HoloNET to automatically retrieve the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashAsync(string installedAppId = null, string roleName = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool retrieveAgentPubKeyAndDnaHashFromConductor = true, bool retrieveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true)
        {
            if (RetrievingAgentPubKeyAndDnaHash)
                return null;

            RetrievingAgentPubKeyAndDnaHash = true;

            //Try to first get from the conductor.
            if (retrieveAgentPubKeyAndDnaHashFromConductor)
                return await RetrieveAgentPubKeyAndDnaHashFromConductorAsync(installedAppId, roleName,retrieveAgentPubKeyAndDnaHashMode, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails);

            else if (retrieveAgentPubKeyAndDnaHashFromSandbox)
            {
                AgentPubKeyDnaHash agentPubKeyDnaHash = await RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails);

                if (agentPubKeyDnaHash != null)
                    RetrievingAgentPubKeyAndDnaHash = false;
            }

            return new AgentPubKeyDnaHash() { AgentPubKey = HoloNETDNA.AgentPubKey, DnaHash = HoloNETDNA.DnaHash };
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default).
        /// </summary>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)
        {
            try
            {
                if (RetrievingAgentPubKeyAndDnaHash)
                    return null;

                RetrievingAgentPubKeyAndDnaHash = true;
                Logger.Log("Attempting To Retrieve AgentPubKey & DnaHash From hc sandbox...", LogType.Info, true);

                if (string.IsNullOrEmpty(HoloNETDNA.FullPathToExternalHCToolBinary))
                    HoloNETDNA.FullPathToExternalHCToolBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\hc.exe"); //default to the current path

                Process pProcess = new Process();
                pProcess.StartInfo.WorkingDirectory = HoloNETDNA.FullPathToRootHappFolder;
                pProcess.StartInfo.FileName = "hc";
                //pProcess.StartInfo.FileName = HoloNETDNA.FullPathToExternalHCToolBinary; //TODO: Need to get this working later (think currently has a version conflict with keylairstone? But not urgent because AgentPubKey & DnaHash are retrieved from Conductor anyway.
                pProcess.StartInfo.Arguments = "sandbox call list-cells";
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                pProcess.StartInfo.CreateNoWindow = false;
                pProcess.Start();

                string output = pProcess.StandardOutput.ReadToEnd();
                int dnaStart = output.IndexOf("DnaHash") + 8;
                int dnaEnd = output.IndexOf(")", dnaStart);
                int agentStart = output.IndexOf("AgentPubKey") + 12;
                int agentEnd = output.IndexOf(")", agentStart);

                string dnaHash = output.Substring(dnaStart, dnaEnd - dnaStart);
                string agentPubKey = output.Substring(agentStart, agentEnd - agentStart);

                if (!string.IsNullOrEmpty(dnaHash) && !string.IsNullOrEmpty(agentPubKey))
                {
                    if (updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved)
                    {
                        HoloNETDNA.AgentPubKey = agentPubKey;
                        HoloNETDNA.DnaHash = dnaHash;
                        HoloNETDNA.CellId = new byte[2][] { ConvertHoloHashToBytes(dnaHash), ConvertHoloHashToBytes(agentPubKey) };
                    }

                    Logger.Log("AgentPubKey & DnaHash successfully retrieved from hc sandbox.", LogType.Info, false);

                    if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(HoloNETDNA.AgentPubKey) && !string.IsNullOrEmpty(HoloNETDNA.DnaHash))
                        SetReadyForZomeCalls("-1");

                    return new AgentPubKeyDnaHash() { DnaHash = dnaHash, AgentPubKey = agentPubKey };
                }
                else if (automaticallyAttemptToRetrieveFromConductorIfSandBoxFails)
                    await RetrieveAgentPubKeyAndDnaHashFromConductorAsync();
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.retrieveAgentPubKeyAndDnaHashFromSandbox method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default).
        /// </summary>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromSandbox(bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)
        {
            return RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails).Result;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retrieved them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retrieve the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetrieveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromSandbox method. 
        /// </summary>
        /// <param name="installedAppId">If this is set then HoloNET will retreive the agentPubKey and DnaHash for this AppId. If it is not set it will retreive it for the InstalledAppId defined in the HoloNETDNA. This field is optional.</param>
        /// <param name="roleName">If this is set then HoloNET will look up the CellType and return it in the OnAppInfoCallBack event. This field is optional.</param>
        /// <param name="retrieveAgentPubKeyAndDnaHashMode">If set to `Wait` (default) it will await until it has finished retrieving the AgentPubKey & DnaHash before returning, otherwise it will return immediately and then raise the OnReadyForZomeCalls event once it has finished retrieving the DnaHash & AgentPubKey.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public async Task<AgentPubKeyDnaHash> RetrieveAgentPubKeyAndDnaHashFromConductorAsync(string installedAppId = null, string roleName = null, RetrieveAgentPubKeyAndDnaHashMode retrieveAgentPubKeyAndDnaHashMode = RetrieveAgentPubKeyAndDnaHashMode.Wait, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true)
        {
            try
            {
                //if (RetrievingAgentPubKeyAndDnaHash)
                //    return null;

                if (string.IsNullOrEmpty(installedAppId))
                {
                    if (!string.IsNullOrEmpty(HoloNETDNA.InstalledAppId))
                        installedAppId = HoloNETDNA.InstalledAppId;
                    else
                        installedAppId = "test-app";
                }

                _currentId++;
                _taskCompletionAgentPubKeyAndDnaHashRetrieved[_currentId.ToString()] = new TaskCompletionSource<AgentPubKeyDnaHash>();

                if (!string.IsNullOrEmpty(roleName))
                    _roleLookup[_currentId.ToString()] = roleName;

                _automaticallyAttemptToGetFromSandboxIfConductorFails = automaticallyAttemptToRetrieveFromSandBoxIfConductorFails;
                RetrievingAgentPubKeyAndDnaHash = true;
                _updateDnaHashAndAgentPubKey = updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved;
                Logger.Log("Attempting To Retrieve AgentPubKey & DnaHash from Holochain Conductor...", LogType.Info, true);

                HoloNETData holoNETData = new HoloNETData()
                {
                    type = "app_info",
                    data = new AppInfoRequest()
                    {
                        installed_app_id = installedAppId
                    }
                };

                await SendHoloNETRequestAsync(holoNETData, HoloNETRequestType.AppInfo, _currentId.ToString());

                if (retrieveAgentPubKeyAndDnaHashMode == RetrieveAgentPubKeyAndDnaHashMode.Wait)
                    return await _taskCompletionAgentPubKeyAndDnaHashRetrieved[_currentId.ToString()].Task;
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.retrieveAgentPubKeyAndDnaHashFromConductor method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the Connect method will automatically call this by default). Once it has retrieved them and the WebSocket has connceted to the Holochain Conductor it will raise the OnReadyForZomeCalls event. If it fails to retrieve the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToRetrieveFromSandBoxIfConductorFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromSandbox method. 
        /// </summary>
        /// <param name="installedAppId">If this is set then HoloNET will retreive the agentPubKey and DnaHash for this AppId. If it is not set it will retreive it for the InstalledAppId defined in the HoloNETDNA. This field is optional.</param>
        /// <param name="roleName">If this is set then HoloNET will look up the CellType and return it in the OnAppInfoCallBack event. This field is optional.</param>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromSandBoxIfConductorFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromConductor(string installedAppId = null, string roleName = null, bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromSandBoxIfConductorFails = true)
        {
            return RetrieveAgentPubKeyAndDnaHashFromConductorAsync(installedAppId, roleName, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails).Result;
        }

        /// <summary>
        /// This method gets the AppInfo from the Holochain Conductor.
        /// </summary>
        /// <param name="installedAppId">If this is set then HoloNET will retreive the agentPubKey and DnaHash for this AppId. If it is not set it will retreive it for the InstalledAppId defined in the HoloNETDNA. This field is optional.</param>
        /// <param name="roleName">If this is set then HoloNET will look up the CellType and return it in the OnAppInfoCallBack event. This field is optional.</param>
        /// <param name="conductorResponseCallBackMode">If set to `WaitForHolochainConductorResponse` (default) it will await until it has finished retrieving the AppInfo before returning, otherwise it will return immediately and then raise the OnAppInfoCallBack event once it has finished retrieving the AppInfo.</param>
        /// <returns>AppInfoCallBackEventArgs</returns>
        public async Task<AppInfoCallBackEventArgs> GetAppInfoAsync(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            try
            {
                if (string.IsNullOrEmpty(installedAppId))
                {
                    if (!string.IsNullOrEmpty(HoloNETDNA.InstalledAppId))
                        installedAppId = HoloNETDNA.InstalledAppId;
                    else
                        installedAppId = "test-app";
                }

                _currentId++;
                _taskCompletionAppInfoRetrieved[_currentId.ToString()] = new TaskCompletionSource<AppInfoCallBackEventArgs>();

                if (!string.IsNullOrEmpty(roleName))
                    _roleLookup[_currentId.ToString()] = roleName;

                RetrievingAgentPubKeyAndDnaHash = true;
               // _updateDnaHashAndAgentPubKey = updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved;
                Logger.Log("Attempting To Retrieve AppInfo from Holochain Conductor...", LogType.Info, true);

                HoloNETData holoNETData = new HoloNETData()
                {
                    type = "app_info",
                    data = new AppInfoRequest()
                    {
                        installed_app_id = installedAppId
                    }
                };

                await SendHoloNETRequestAsync(holoNETData, HoloNETRequestType.AppInfo, _currentId.ToString());

                if (conductorResponseCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
                    return await _taskCompletionAppInfoRetrieved[_currentId.ToString()].Task;
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.GetAppInfoAsync method getting the AppInfo from hApp.", ex);
            }

            return null;
        }

        /// <summary>
        /// This method gets the AppInfo from the Holochain Conductor.
        /// </summary>
        /// <param name="installedAppId">If this is set then HoloNET will retreive the agentPubKey and DnaHash for this AppId. If it is not set it will retreive it for the InstalledAppId defined in the HoloNETDNA. This field is optional.</param>
        /// <param name="roleName">If this is set then HoloNET will look up the CellType and return it in the OnAppInfoCallBack event. This field is optional.</param>
        /// <param name="conductorResponseCallBackMode">If set to `WaitForHolochainConductorResponse` (default) it will await until it has finished retrieving the AppInfo before returning, otherwise it will return immediately and then raise the OnAppInfoCallBack event once it has finished retrieving the AppInfo.</param>
        /// <returns>AppInfoCallBackEventArgs</returns>
        public async Task<AppInfoCallBackEventArgs> GetAppInfo(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return GetAppInfoAsync(installedAppId, roleName, ConductorResponseCallBackMode.UseCallBackEvents).Result;
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The id of the HoloNET request sent to the Holochain Conductor. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. If this id is not passed used by calling one of the alternative overloads then HoloNET will automatically generate a unique id.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return await CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return await CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.  
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.   
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.    
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.    
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.     
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.      
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <param name="callZomeOptions">Optional per-call options (e.g. a request timeout override). If null, defaults are used (see CallZomeOptions and HoloNETDNA.NetworkConfig.RequestTimeoutS).</param>
        /// <returns></returns>
        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, CallZomeOptions callZomeOptions = null)
        {
            callZomeOptions ??= CallZomeOptions.Default;
            try
            {
                _taskCompletionZomeCallBack[id] = new TaskCompletionSource<ZomeFunctionCallBackEventArgs>();

                //if (WebSocket.State == WebSocketState.Closed || WebSocket.State == WebSocketState.None)
                if (WebSocket.State != WebSocketState.Open && WebSocket.State != WebSocketState.Connecting)
                    await ConnectAsync();

                if (!IsReadyForZomesCalls)
                {
                    if (zomeResultCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
                        await WaitTillReadyForZomeCallsAsync();
                    else
                        return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "HoloNET is not ready to make zome calls yet, please wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                }

                if (string.IsNullOrEmpty(HoloNETDNA.DnaHash))
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "The DnaHash cannot be empty, please either set manually in the HoloNETDNA.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                //throw new InvalidOperationException("The DnaHash cannot be empty, please either set manually in the HoloNETDNA.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call.");

                if (string.IsNullOrEmpty(HoloNETDNA.AgentPubKey))
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "The AgentPubKey cannot be empty, please either set manually in the HoloNETDNA.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                //throw new InvalidOperationException("The AgentPubKey cannot be empty, please either set manually in the HoloNETDNA.AgentPubKey property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call.");

                Logger.Log($"Calling Zome Function {function} on Zome {zome} with Id {id} On Holochain Conductor...", LogType.Info, true);

                _cacheZomeReturnDataLookup[id] = cachReturnData;

                if (cachReturnData)
                {
                    if (_zomeReturnDataLookup.ContainsKey(id))
                    {
                        Logger.Log("Caching Enabled so returning data from cache...", LogType.Info);
                        Logger.Log(string.Concat("Id: ", _zomeReturnDataLookup[id].Id, ", Zome: ", _zomeReturnDataLookup[id].Zome, ", Zome Function: ", _zomeReturnDataLookup[id].ZomeFunction, ", Raw Zome Return Data: ", _zomeReturnDataLookup[id].RawZomeReturnData, ", Zome Return Data: ", _zomeReturnDataLookup[id].ZomeReturnData, ", JSON Raw Data: ", _zomeReturnDataLookup[id].RawJSONData), LogType.Info);
                        //Logger.Log(string.Concat("Id: ", _zomeReturnDataLookup[id].Id, ", Zome: ", _zomeReturnDataLookup[id].Zome, ", Zome Function: ", _zomeReturnDataLookup[id].ZomeFunction, ", Is Zome Call Successful: ", _zomeReturnDataLookup[id].IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", _zomeReturnDataLookup[id].RawZomeReturnData, ", Zome Return Data: ", _zomeReturnDataLookup[id].ZomeReturnData, ", JSON Raw Data: ", _zomeReturnDataLookup[id].RawJSONData), LogType.Info);

                        if (callback != null)
                            callback.DynamicInvoke(this, _zomeReturnDataLookup[id]);

                        OnZomeFunctionCallBack?.Invoke(this, _zomeReturnDataLookup[id]);
                        _taskCompletionZomeCallBack[id].SetResult(_zomeReturnDataLookup[id]);

                        Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                        _taskCompletionZomeCallBack.Remove(id);

                        return await returnValue;
                    }
                }

                if (matchIdToZomeFuncInCallback || HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour != EnforceRequestToResponseIdMatchingBehaviour.Ignore)
                {
                    _zomeLookup[id] = zome;
                    _funcLookup[id] = function;
                }

                if (callback != null)
                    _callbackLookup[id] = callback;

                //_responseMustHaveMatchingRequestLookup[id] = responseMustHaveMatchingRequest;

                try
                {
                    if (paramsObject != null && paramsObject.GetType() == typeof(string))
                    {
                        byte[] holoHash = null;
                        holoHash = ConvertHoloHashToBytes(paramsObject.ToString());

                        if (holoHash != null)
                            paramsObject = holoHash;
                    }
                }
                catch (Exception ex)
                {

                }

                string cellId = $"{HoloNETDNA.AgentPubKey}:{HoloNETDNA.DnaHash}";

                if (_signingCredentialsForCell.ContainsKey(cellId) && _signingCredentialsForCell[cellId] != null)
                {
                    ZomeCallUnsigned payload = new ZomeCallUnsigned()
                    {
                        cap_secret = _signingCredentialsForCell[cellId].CapSecret,
                        cell_id_agent_pub_key = ConvertHoloHashToBytes(HoloNETDNA.AgentPubKey),
                        cell_id_dna_hash = ConvertHoloHashToBytes(HoloNETDNA.DnaHash),
                        fn_name = function,
                        zome_name = zome,
                        payload = MessagePackSerializer.Serialize(paramsObject),
                        provenance = _signingCredentialsForCell[cellId].SigningKey, 
                        nonce = SodiumCore.GetRandomBytes(32), 
                        //expires_at = DateTime.Now.AddMinutes(5).Ticks / 10
                        expires_at = (DateTimeOffset.Now.ToUnixTimeMilliseconds() + 5 * 60 * 1000) * 1000
                    };

                    byte[] hash = new byte[32];
                    try
                    {
                        // Call into the `holochain_zome_types` crate to get a blake2b hash of the zomeCall
                        HolochainSerialisationWrapper.call_get_data_to_sign(hash, payload);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to get data to sign: " + e.ToString());
                    }

                    //byte[] hash = Blake2b.ComputeHash(MessagePackSerializer.Serialize(payload));
                    var sig = Sodium.PublicKeyAuth.SignDetached(hash, _signingCredentialsForCell[cellId].KeyPair.PrivateKey);

                    // Kept for backwards compatibility with any code that inspects the flattened
                    // ZomeCallSigned shape (e.g. via OnDataReceived/raw request introspection).
                    ZomeCallSigned signedPayload = new ZomeCallSigned()
                    {
                        cap_secret = payload.cap_secret,
                        cell_id = new CellId(payload.cell_id_dna_hash, payload.cell_id_agent_pub_key),
                        fn_name = payload.fn_name,
                        zome_name = payload.zome_name,
                        payload = payload.payload,
                        provenance = payload.provenance,
                        nonce = payload.nonce,
                        expires_at = payload.expires_at,
                        signature = sig[0..64]
                    };

                    // Holochain 0.6.1 wire shape for AppRequest::CallZome is
                    // ZomeCallParamsSigned { bytes: ExternIO, signature: Signature } where
                    // `bytes` is the holochain_serialized_bytes (msgpack struct-map) encoding of
                    // the unsigned ZomeCall/ZomeCallParams fields above (`signedPayload` minus
                    // the signature). See ZomeCallParamsSigned.cs for the source reference.
                    byte[] unsignedBytes = MessagePackSerializer.Serialize<ZomeCall>(signedPayload, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.None));

                    ZomeCallParamsSigned zomeCallParamsSigned = new ZomeCallParamsSigned(unsignedBytes, signedPayload.signature);

                    HoloNETData holoNETData = new HoloNETData()
                    {
                        type = "call_zome",
                        data = zomeCallParamsSigned
                    };

                    await SendHoloNETRequestAsync(holoNETData, HoloNETRequestType.ZomeCall, id);

                    if (zomeResultCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
                    {
                        Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                        int timeoutSeconds = callZomeOptions?.TimeoutSeconds ?? HoloNETDNA.NetworkConfig?.RequestTimeoutS ?? 60;

                        if (timeoutSeconds > 0)
                        {
                            Task delayTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
                            Task completedTask = await Task.WhenAny(returnValue, delayTask);

                            if (completedTask == delayTask)
                            {
                                string timeoutMsg = $"Zome call to {zome}.{function} (Id: {id}) timed out after {timeoutSeconds} seconds.";
                                HandleError(timeoutMsg, null);
                                _taskCompletionZomeCallBack.Remove(id);
                                return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = timeoutMsg };
                            }
                        }

                        return await returnValue;
                    }
                    else
                        return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, Message = "ZomeResultCallBackMode is set to UseCallBackEvents so please wait for OnZomeFunctionCallBack event for zome result." };
                }
                else
                {
                    string msg = $"Error occurred in HoloNETClient.CallZomeFunctionAsync method: Cannot sign zome call when no signing credentials have been authorized for the cell (AgentPubKey: {HoloNETDNA.AgentPubKey}, DnaHash: {HoloNETDNA.DnaHash}).";
                    HandleError(msg, null);
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, Message = msg, IsError = true };
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.CallZomeFunctionAsync method.", ex);
                return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = $"Error occurred in HoloNETClient.CallZomeFunctionAsync method. Details: {ex}", Exception = ex };
            }
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, bool cachReturnData = false, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The id of the HoloNET request sent to the Holochain Conductor. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. If this id is not passed used by calling one of the alternative overloads then HoloNET will automatically generate a unique id.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <param name="zomeResultCallBackMode">This is an optional param, where the caller can choose whether to wait for the Holochain Conductor response before returning to the caller or to return immediately once the request has been sent to the Holochain Conductor and then raise the OnDataReceived and then the OnZomeFunctionCallBack or OnSignalsCallBack events depending on the type of request sent to the Holochain Conductor.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ConductorResponseCallBackMode zomeResultCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor. 
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.  
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.    
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.     
        /// </summary>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null)
        {
            _currentId++;
            return CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, ConductorResponseCallBackMode.UseCallBackEvents);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectReturnedFromZome">This is an optional param, where the caller can pass in an instance of the dynamic data object they wish the entry data returned to be mapped to. This data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents).Result;
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <param name="entryDataObjectTypeReturnedFromZome">This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to. This newly created data object will then be returned in the ZomeFunctionCallBackEventArgs.Entry.EntryDataObject property.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return CallZomeFunction(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents);
        }

        /// <summary>
        /// This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the OnDataReceived event and then either the OnZomeFunctionCallBack or OnSignalCallBack event once it has received a response from the Holochain conductor.       
        /// </summary>
        /// <param name="id">The unique id you wish to assign for this call. This id is then returned in OnDataReceived, OnZomeFunctionCallBack and OnSignalCallBack events. There are overloads that omit this param, use these overloads if you wish HoloNET to auto-generate and manage the id's for you.</param>
        /// <param name="zome">The name of the zome you wish to target.</param>
        /// <param name="function">The name of the zome function you wish to call.</param>
        /// <param name="callback">A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event. (optional param).</param>
        /// <param name="paramsObject">A basic CLR object containing the params the zome function is expecting (optional param).</param>
        /// <param name="matchIdToZomeFuncInCallback">This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the zome and zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself. NOTE: If HoloNETDNA.EnforceRequestToResponseIdMatchingBehaviour is set to Warn or Error then this param will be ignored and the matching will always occur.</param>
        /// <param name="cachReturnData">This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.</param>
        /// <returns></returns>
        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback = true, bool cachReturnData = false)
        {
            return CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents).Result;
        }

        // New in Holochain 0.6.1 - App API.

        /// <summary>
        /// Create a new clone cell for the given role.
        /// </summary>
        /// <param name="roleName">The RoleName to create the clone cell for.</param>
        /// <param name="modifiers">The DNA modifiers to apply to the clone cell (network_seed/properties/origin_time/quantum_time - pass a plain object/dictionary with the modifier fields you need).</param>
        /// <param name="membraneProof">The optional membrane proof for the clone cell.</param>
        /// <param name="name">The optional human readable name for the clone cell.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCloneCellCreatedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<CloneCellCreatedCallBackEventArgs> CreateCloneCellAsync(string roleName, dynamic modifiers = null, byte[] membraneProof = null, string name = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppCreateCloneCell, "create_clone_cell", new CreateCloneCellRequest()
            {
                role_name = roleName,
                modifiers = modifiers,
                membrane_proof = membraneProof,
                name = name
            }, _taskCompletionCloneCellCreatedCallBack, "OnCloneCellCreatedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Create a new clone cell for the given role.
        /// </summary>
        /// <param name="roleName">The RoleName to create the clone cell for.</param>
        /// <param name="modifiers">The DNA modifiers to apply to the clone cell (network_seed/properties/origin_time/quantum_time - pass a plain object/dictionary with the modifier fields you need).</param>
        /// <param name="membraneProof">The optional membrane proof for the clone cell.</param>
        /// <param name="name">The optional human readable name for the clone cell.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CloneCellCreatedCallBackEventArgs CreateCloneCell(string roleName, dynamic modifiers = null, byte[] membraneProof = null, string name = null, string id = null)
        {
            return CreateCloneCellAsync(roleName, modifiers, membraneProof, name, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Enable a previously disabled clone cell.
        /// </summary>
        /// <param name="cloneCellId">Either the clone id (string, e.g. "role_name.0") or the CellId (byte[][]) of the clone cell to enable.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCloneCellEnabledCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<CloneCellEnabledCallBackEventArgs> EnableCloneCellAsync(dynamic cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppEnableCloneCell, "enable_clone_cell", new EnableCloneCellRequest()
            {
                clone_cell_id = cloneCellId
            }, _taskCompletionCloneCellEnabledCallBack, "OnCloneCellEnabledCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Enable a previously disabled clone cell.
        /// </summary>
        /// <param name="cloneCellId">Either the clone id (string, e.g. "role_name.0") or the CellId (byte[][]) of the clone cell to enable.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CloneCellEnabledCallBackEventArgs EnableCloneCell(dynamic cloneCellId, string id = null)
        {
            return EnableCloneCellAsync(cloneCellId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Disable a clone cell.
        /// </summary>
        /// <param name="cloneCellId">Either the clone id (string, e.g. "role_name.0") or the CellId (byte[][]) of the clone cell to disable.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCloneCellDisabledCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<CloneCellDisabledCallBackEventArgs> DisableCloneCellAsync(dynamic cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppDisableCloneCell, "disable_clone_cell", new DisableCloneCellRequest()
            {
                clone_cell_id = cloneCellId
            }, _taskCompletionCloneCellDisabledCallBack, "OnCloneCellDisabledCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Disable a clone cell.
        /// </summary>
        /// <param name="cloneCellId">Either the clone id (string, e.g. "role_name.0") or the CellId (byte[][]) of the clone cell to disable.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CloneCellDisabledCallBackEventArgs DisableCloneCell(dynamic cloneCellId, string id = null)
        {
            return DisableCloneCellAsync(cloneCellId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Get the state of a countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.6.1 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to get the countersigning session state for.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCountersigningSessionStateReturnedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<CountersigningSessionStateReturnedCallBackEventArgs> GetCountersigningSessionStateAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppGetCountersigningSessionState, "get_countersigning_session_state", new CountersigningCellIdRequest()
            {
                cell_id = cellId
            }, _taskCompletionCountersigningSessionStateReturnedCallBack, "OnCountersigningSessionStateReturnedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Get the state of a countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.6.1 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to get the countersigning session state for.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CountersigningSessionStateReturnedCallBackEventArgs GetCountersigningSessionState(CellId cellId, string id = null)
        {
            return GetCountersigningSessionStateAsync(cellId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Abandon an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.6.1 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to abandon the countersigning session for.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCountersigningSessionAbandonedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<CountersigningSessionAbandonedCallBackEventArgs> AbandonCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppAbandonCountersigningSession, "abandon_countersigning_session", new CountersigningCellIdRequest()
            {
                cell_id = cellId
            }, _taskCompletionCountersigningSessionAbandonedCallBack, "OnCountersigningSessionAbandonedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Abandon an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.6.1 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to abandon the countersigning session for.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CountersigningSessionAbandonedCallBackEventArgs AbandonCountersigningSession(CellId cellId, string id = null)
        {
            return AbandonCountersigningSessionAsync(cellId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Publish (force-finalise) an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.6.1 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to publish the countersigning session for.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnPublishCountersigningSessionTriggeredCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<PublishCountersigningSessionTriggeredCallBackEventArgs> PublishCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppPublishCountersigningSession, "publish_countersigning_session", new CountersigningCellIdRequest()
            {
                cell_id = cellId
            }, _taskCompletionPublishCountersigningSessionTriggeredCallBack, "OnPublishCountersigningSessionTriggeredCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Publish (force-finalise) an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.6.1 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to publish the countersigning session for.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public PublishCountersigningSessionTriggeredCallBackEventArgs PublishCountersigningSession(CellId cellId, string id = null)
        {
            return PublishCountersigningSessionAsync(cellId, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// List the names of the host functions available to WASM code running in this conductor.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnWasmHostFunctionsListedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<WasmHostFunctionsListedCallBackEventArgs> ListWasmHostFunctionsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppListWasmHostFunctions, "list_wasm_host_functions", null, _taskCompletionWasmHostFunctionsListedCallBack, "OnWasmHostFunctionsListedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// List the names of the host functions available to WASM code running in this conductor.
        /// </summary>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public WasmHostFunctionsListedCallBackEventArgs ListWasmHostFunctions(string id = null)
        {
            return ListWasmHostFunctionsAsync(ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Provide the membrane proofs for this app, used when the app was installed with `allow_deferred_memproofs` and the membrane proofs were not provided at installation time.
        /// </summary>
        /// <param name="membraneProofs">A role-name-keyed dictionary of raw membrane proof bytes, one per role that requires a membrane proof.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnMemproofsProvidedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<MemproofsProvidedCallBackEventArgs> ProvideMemproofsAsync(Dictionary<string, byte[]> membraneProofs, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppProvideMemproofs, "provide_memproofs", membraneProofs, _taskCompletionMemproofsProvidedCallBack, "OnMemproofsProvidedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Provide the membrane proofs for this app, used when the app was installed with `allow_deferred_memproofs` and the membrane proofs were not provided at installation time.
        /// </summary>
        /// <param name="membraneProofs">A role-name-keyed dictionary of raw membrane proof bytes, one per role that requires a membrane proof.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public MemproofsProvidedCallBackEventArgs ProvideMemproofs(Dictionary<string, byte[]> membraneProofs, string id = null)
        {
            return ProvideMemproofsAsync(membraneProofs, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Get meta info about a peer that is known via the networking layer (kitsune2), via the app interface.
        /// </summary>
        /// <param name="url">The peer's Url.</param>
        /// <param name="dnaHashes">Optionally restrict the returned info to these DnaHashes.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnAppPeerMetaInfoReturnedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<AppPeerMetaInfoReturnedCallBackEventArgs> GetAppPeerMetaInfoAsync(string url, List<byte[]> dnaHashes = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppPeerMetaInfo, "peer_meta_info", new PeerMetaInfoRequest()
            {
                url = url,
                dna_hashes = dnaHashes
            }, _taskCompletionAppPeerMetaInfoReturnedCallBack, "OnAppPeerMetaInfoReturnedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Get meta info about a peer that is known via the networking layer (kitsune2), via the app interface.
        /// </summary>
        /// <param name="url">The peer's Url.</param>
        /// <param name="dnaHashes">Optionally restrict the returned info to these DnaHashes.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public AppPeerMetaInfoReturnedCallBackEventArgs GetAppPeerMetaInfo(string url, List<byte[]> dnaHashes = null, string id = null)
        {
            return GetAppPeerMetaInfoAsync(url, dnaHashes, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        /// <summary>
        /// Dump DHT op timing information for a DNA via the app interface (new in Holochain 0.7.0).
        /// The app must be running a cell of this DNA. Results are paginated; pass the cursor from
        /// a previous call to retrieve the next page.
        /// </summary>
        /// <param name="dnaHash">The DNA hash whose DHT arc to dump op timings for.</param>
        /// <param name="cursor">Pagination cursor from a previous DumpOpTimings call, or null to start from the beginning.</param>
        /// <param name="limit">Maximum number of ops to return. Null means no limit.</param>
        /// <param name="conductorResponseCallBackMode">The Conductor Response CallBack Mode.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<AppOpTimingsDumpedCallBackEventArgs> DumpOpTimingsAsync(byte[] dnaHash, OpTimingsCursor cursor = null, uint? limit = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppDumpOpTimings, "dump_op_timings", new DumpOpTimingsRequest()
            {
                dna_hash = dnaHash,
                cursor = cursor,
                limit = limit
            }, _taskCompletionAppOpTimingsDumpedCallBack, "OnAppOpTimingsDumpedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Dump DHT op timing information for a DNA via the app interface (new in Holochain 0.7.0).
        /// </summary>
        /// <param name="dnaHash">The DNA hash whose DHT arc to dump op timings for.</param>
        /// <param name="cursor">Pagination cursor from a previous DumpOpTimings call, or null to start from the beginning.</param>
        /// <param name="limit">Maximum number of ops to return. Null means no limit.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public AppOpTimingsDumpedCallBackEventArgs DumpOpTimings(byte[] dnaHash, OpTimingsCursor cursor = null, uint? limit = null, string id = null)
        {
            return DumpOpTimingsAsync(dnaHash, cursor, limit, ConductorResponseCallBackMode.UseCallBackEvents, id).Result;
        }

        private async Task<T> CallFunctionAsync<T>(HoloNETRequestType requestType, string holochainConductorFunctionName, dynamic holoNETDataDetailed, Dictionary<string, TaskCompletionSource<T>> taskCompletionCallBack, string eventCallBackName, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null) where T : HoloNETDataReceivedBaseEventArgs, new()
        {
            HoloNETData holoNETData = new HoloNETData()
            {
                type = holochainConductorFunctionName,
                data = holoNETDataDetailed
            };

            if (string.IsNullOrEmpty(id))
                id = GetRequestId();

            taskCompletionCallBack[id] = new TaskCompletionSource<T> { };
            await SendHoloNETRequestAsync(holoNETData, requestType, id);

            if (conductorResponseCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
            {
                Task<T> returnValue = taskCompletionCallBack[id].Task;
                return await returnValue;
            }
            else
                return new T() { EndPoint = EndPoint, Id = id, Message = $"conductorResponseCallBackMode is set to UseCallBackEvents so please wait for {eventCallBackName} event for the result." };
        }
    }
}