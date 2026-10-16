using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Generic;
using MessagePack;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using Chaos.NaCl;

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
            //return Task.Run(() => RetrieveAgentPubKeyAndDnaHashAsync(installedAppId, roleName, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved)).Result; // returned before the conductor replied
            return Task.Run(() => RetrieveAgentPubKeyAndDnaHashAsync(installedAppId, roleName, RetrieveAgentPubKeyAndDnaHashMode.Wait, retrieveAgentPubKeyAndDnaHashFromConductor, retrieveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved)).GetAwaiter().GetResult();
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

                //if (string.IsNullOrEmpty(HoloNETDNA.FullPathToExternalHCToolBinary))
                //    HoloNETDNA.FullPathToExternalHCToolBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\hc.exe"); //default to the current path

                //Process pProcess = new Process();
                //pProcess.StartInfo.WorkingDirectory = HoloNETDNA.FullPathToRootHappFolder;
                //pProcess.StartInfo.FileName = "hc";
                ////pProcess.StartInfo.FileName = HoloNETDNA.FullPathToExternalHCToolBinary; //TODO: Need to get this working later (think currently has a version conflict with keylairstone? But not urgent because AgentPubKey & DnaHash are retrieved from Conductor anyway.
                //pProcess.StartInfo.Arguments = "sandbox call list-cells";
                //pProcess.StartInfo.UseShellExecute = false;
                //pProcess.StartInfo.RedirectStandardOutput = true;
                //pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                //pProcess.StartInfo.CreateNoWindow = false;
                //pProcess.Start();

                //string output = pProcess.StandardOutput.ReadToEnd();
                //int dnaStart = output.IndexOf("DnaHash") + 8;
                //int dnaEnd = output.IndexOf(")", dnaStart);
                //int agentStart = output.IndexOf("AgentPubKey") + 12;
                //int agentEnd = output.IndexOf(")", agentStart);

                //string dnaHash = output.Substring(dnaStart, dnaEnd - dnaStart);
                //string agentPubKey = output.Substring(agentStart, agentEnd - agentStart);

                // Holochain 0.7 removed `hc sandbox call`; admin calls are `hc client call --port <admin port> ...` and print JSON.
                (string dnaHash, string agentPubKey) = ReadCellFromHcClient();

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
                    //await RetrieveAgentPubKeyAndDnaHashFromConductorAsync();
                    // Its default fallback is back to the sandbox, so a failure on both sides looped forever.
                    return await RetrieveAgentPubKeyAndDnaHashFromConductorAsync(null, null, RetrieveAgentPubKeyAndDnaHashMode.Wait, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, false);
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.retrieveAgentPubKeyAndDnaHashFromSandbox method getting DnaHash & AgentPubKey from hApp.", ex);
            }
            finally
            {
                // Was never reset here, so after one failed attempt every later retrieval returned null.
                RetrievingAgentPubKeyAndDnaHash = false;
            }

            return null;
        }

        /// <summary>
        /// Reads the DNA hash and agent key with hc's client: `hc client call --port &lt;admin port&gt; list-apps` (the
        /// configured app's first provisioned cell), falling back to `list-cells`. Uses FullPathToExternalHCToolBinary when it
        /// exists, otherwise hc from the PATH. Returns nulls when hc fails or nothing is installed.
        /// </summary>
        protected virtual (string dnaHash, string agentPubKey) ReadCellFromHcClient()
        {
            string hc = !string.IsNullOrEmpty(HoloNETDNA.FullPathToExternalHCToolBinary) && File.Exists(HoloNETDNA.FullPathToExternalHCToolBinary) ? HoloNETDNA.FullPathToExternalHCToolBinary : "hc";
            int adminPort = new Uri(HoloNETDNA.HolochainConductorAdminURI).Port;

            string Run(string command)
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = hc;
                    process.StartInfo.Arguments = $"client call --port {adminPort} {command}";
                    if (!string.IsNullOrEmpty(HoloNETDNA.FullPathToRootHappFolder) && Directory.Exists(HoloNETDNA.FullPathToRootHappFolder))
                        process.StartInfo.WorkingDirectory = HoloNETDNA.FullPathToRootHappFolder;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(30000);
                    return output;
                }
            }

            string JsonPart(string output)
            {
                int start = output.IndexOfAny(new[] { '[', '{' });
                return start < 0 ? null : output.Substring(start);
            }

            if (!string.IsNullOrEmpty(HoloNETDNA.InstalledAppId))
            {
                string apps = JsonPart(Run("list-apps"));
                if (apps != null)
                {
                    foreach (var app in Newtonsoft.Json.Linq.JArray.Parse(apps))
                    {
                        if ((string)app["installed_app_id"] != HoloNETDNA.InstalledAppId)
                            continue;

                        foreach (var role in (app["cell_info"] as Newtonsoft.Json.Linq.JObject)?.Properties() ?? Enumerable.Empty<Newtonsoft.Json.Linq.JProperty>())
                            foreach (var cell in role.Value)
                                if ((string)cell["type"] == "provisioned")
                                    return ((string)cell["value"]["cell_id"]["dna_hash"], (string)cell["value"]["cell_id"]["agent_pub_key"]);
                    }
                }
            }

            string cells = JsonPart(Run("list-cells"));
            var first = cells == null ? null : Newtonsoft.Json.Linq.JArray.Parse(cells).FirstOrDefault();
            return first == null ? (null, null) : ((string)first["dna_hash"], (string)first["agent_pub_key"]);
        }

        /// <summary>
        /// This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retrieves them and the WebSocket has connected to the Holochain Conductor. If it fails to retrieve the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToRetrieveFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the RetrieveAgentPubKeyAndDnaHashFromConductor method to attempt to retrieve them directly from the conductor (default).
        /// </summary>
        /// <param name="updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved">Set this to true (default) to automatically update the HoloNETDNA property once it has retrieved the DnaHash & AgentPubKey.</param>
        /// <param name="automaticallyAttemptToRetrieveFromConductorIfSandBoxFails">If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.</param>
        /// <returns>The AgentPubKey and DnaHash</returns>
        public AgentPubKeyDnaHash RetrieveAgentPubKeyAndDnaHashFromSandbox(bool updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved = true, bool automaticallyAttemptToRetrieveFromConductorIfSandBoxFails = true)
        {
            //return Task.Run(() => RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails)).Result; // .Result wraps errors in AggregateException
            return Task.Run(() => RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromConductorIfSandBoxFails)).GetAwaiter().GetResult();
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

                //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
                string __requestId = GetRequestId();
                // Kept in a local: _currentId can move on (another call) while this one awaits, and the completion entry can
                // be removed by a fast reply before it is read.
                string requestId = __requestId;
                var retrieved = new TaskCompletionSource<AgentPubKeyDnaHash>(TaskCreationOptions.RunContinuationsAsynchronously);
                _taskCompletionAgentPubKeyAndDnaHashRetrieved[requestId] = retrieved;

                if (!string.IsNullOrEmpty(roleName))
                    _roleLookup[requestId] = roleName;

                _automaticallyAttemptToGetFromSandboxIfConductorFails = automaticallyAttemptToRetrieveFromSandBoxIfConductorFails;
                RetrievingAgentPubKeyAndDnaHash = true;
                _updateDnaHashAndAgentPubKey = updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved;
                Logger.Log("Attempting To Retrieve AgentPubKey & DnaHash from Holochain Conductor...", LogType.Info, true);

                HoloNETData holoNETData = new HoloNETData()
                {
                    type = "app_info",
                    // Holochain 0.7.0 AppRequest::AppInfo is a unit variant: the app is the one this interface is authenticated for.
                    // Sending the old { installed_app_id } object made the conductor reject the request ("Failed to deserialize request").
                    //data = new AppInfoRequest() { installed_app_id = installedAppId }
                    data = null
                };

                if (WebSocket.State != WebSocketState.Open)
                {
                    // Nothing can answer: report failure instead of waiting forever.
                    _taskCompletionAgentPubKeyAndDnaHashRetrieved.Remove(requestId);
                    RetrievingAgentPubKeyAndDnaHash = false;
                    return null;
                }

                await SendHoloNETRequestAsync(holoNETData, HoloNETRequestType.AppInfo, requestId);

                if (retrieveAgentPubKeyAndDnaHashMode == RetrieveAgentPubKeyAndDnaHashMode.Wait)
                    //return await _taskCompletionAgentPubKeyAndDnaHashRetrieved[_currentId.ToString()].Task;
                    return await retrieved.Task;
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
            //return Task.Run(() => RetrieveAgentPubKeyAndDnaHashFromConductorAsync(installedAppId, roleName, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails)).Result; // returned before the conductor replied
            return Task.Run(() => RetrieveAgentPubKeyAndDnaHashFromConductorAsync(installedAppId, roleName, RetrieveAgentPubKeyAndDnaHashMode.Wait, updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved, automaticallyAttemptToRetrieveFromSandBoxIfConductorFails)).GetAwaiter().GetResult();
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

                //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
                string __requestId = GetRequestId();
                string requestId = __requestId; // see RetrieveAgentPubKeyAndDnaHashFromConductorAsync
                var appInfoRetrieved = new TaskCompletionSource<AppInfoCallBackEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                _taskCompletionAppInfoRetrieved[requestId] = appInfoRetrieved;
                TrackPendingCall(requestId, _taskCompletionAppInfoRetrieved);

                if (!string.IsNullOrEmpty(roleName))
                    _roleLookup[requestId] = roleName;

                RetrievingAgentPubKeyAndDnaHash = true;
               // _updateDnaHashAndAgentPubKey = updateHoloNETDNAWithAgentPubKeyAndDnaHashOnceRetrieved;
                Logger.Log("Attempting To Retrieve AppInfo from Holochain Conductor...", LogType.Info, true);

                HoloNETData holoNETData = new HoloNETData()
                {
                    type = "app_info",
                    // Holochain 0.7.0 AppRequest::AppInfo is a unit variant: the app is the one this interface is authenticated for.
                    // Sending the old { installed_app_id } object made the conductor reject the request ("Failed to deserialize request").
                    //data = new AppInfoRequest() { installed_app_id = installedAppId }
                    data = null
                };

                //await SendHoloNETRequestAsync(holoNETData, HoloNETRequestType.AppInfo, _currentId.ToString());
                await SendTrackedRequestAsync(holoNETData, HoloNETRequestType.AppInfo, requestId);

                if (conductorResponseCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse || appInfoRetrieved.Task.IsCompleted)
                {
                    //return await _taskCompletionAppInfoRetrieved[_currentId.ToString()].Task;
                    AppInfoCallBackEventArgs appInfo = await appInfoRetrieved.Task;
                    UntrackPendingCall(requestId);
                    return appInfo;
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occurred in HoloNETClient.GetAppInfoAsync method getting the AppInfo from hApp.", ex);
                // Returned null, so callers reading .IsError got a NullReferenceException.
                return new AppInfoCallBackEventArgs { EndPoint = EndPoint, IsError = true, Message = $"Error occurred in HoloNETClient.GetAppInfoAsync method getting the AppInfo from hApp. Reason: {ex.Message}", Exception = ex };
            }

            //return null;
            return new AppInfoCallBackEventArgs { EndPoint = EndPoint, Message = "conductorResponseCallBackMode is set to UseCallBackEvents so please wait for the OnAppInfoCallBack event for the result." };
        }

        /// <summary>
        /// This method gets the AppInfo from the Holochain Conductor.
        /// </summary>
        /// <param name="installedAppId">If this is set then HoloNET will retreive the agentPubKey and DnaHash for this AppId. If it is not set it will retreive it for the InstalledAppId defined in the HoloNETDNA. This field is optional.</param>
        /// <param name="roleName">If this is set then HoloNET will look up the CellType and return it in the OnAppInfoCallBack event. This field is optional.</param>
        /// <param name="conductorResponseCallBackMode">If set to `WaitForHolochainConductorResponse` (default) it will await until it has finished retrieving the AppInfo before returning, otherwise it will return immediately and then raise the OnAppInfoCallBack event once it has finished retrieving the AppInfo.</param>
        /// <returns>AppInfoCallBackEventArgs</returns>
        //public async Task<AppInfoCallBackEventArgs> GetAppInfo(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse) // the sync variant returned a Task
        public AppInfoCallBackEventArgs GetAppInfo(string installedAppId = null, string roleName = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
        {
            //return Task.Run(() => GetAppInfoAsync(installedAppId, roleName, ConductorResponseCallBackMode.UseCallBackEvents)).Result; // returned before the conductor replied
            //return Task.Run(() => GetAppInfoAsync(installedAppId, roleName, ConductorResponseCallBackMode.WaitForHolochainConductorResponse)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => GetAppInfoAsync(installedAppId, roleName, mode));
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, null, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, callback, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, callback, paramsObject, true, false, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, callback, paramsObject, true, cachReturnData, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, callback, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return await CallZomeFunctionAsync(__requestId, zome, function, callback, paramsObject, true, cachReturnData, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
            callZomeOptions ??= CallZomeOptions.Default;
            try
            {
                _taskCompletionZomeCallBack[id] = new TaskCompletionSource<ZomeFunctionCallBackEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

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
                        _taskCompletionZomeCallBack[id].TrySetResult(_zomeReturnDataLookup[id]);

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

                        //if (holoHash != null)
                        //    paramsObject = holoHash;
                        // Only a real HoloHash string ("uhC...", 39 bytes, 0x84 xx 0x24 prefix) is sent as bytes. Any other string
                        // that happened to decode as base64 after a leading 'u' (e.g. a username) was being mangled into bytes.
                        if (holoHash != null && holoHash.Length == 39 && holoHash[0] == 0x84 && holoHash[2] == 0x24 && paramsObject.ToString().StartsWith("uhC"))
                            paramsObject = holoHash;
                    }
                }
                catch (Exception ex)
                {

                }

                string cellId = $"{HoloNETDNA.AgentPubKey}:{HoloNETDNA.DnaHash}";

                ZomeCallToSign callToSign = new ZomeCallToSign()
                {
                    CellIdDnaHash = ConvertHoloHashToBytes(HoloNETDNA.DnaHash),
                    CellIdAgentPubKey = ConvertHoloHashToBytes(HoloNETDNA.AgentPubKey),
                    ZomeName = zome,
                    FnName = function,
                    Payload = MessagePackSerializer.Serialize(paramsObject),
                    Nonce = GetCryptographicRandomBytes(32),
                    ExpiresAt = (DateTimeOffset.Now.ToUnixTimeMilliseconds() + 5 * 60 * 1000) * 1000
                };

                ZomeCallParamsSigned zomeCallParamsSigned = null;

                if (ZomeCallSigner != null)
                    zomeCallParamsSigned = await ZomeCallSigner.SignZomeCallAsync(callToSign);

                else
                    zomeCallParamsSigned = SignZomeCallWithAuthorizedCredentials(callToSign);

                if (zomeCallParamsSigned != null)
                {
                    HoloNETData holoNETData = new HoloNETData()
                    {
                        type = "call_zome",
                        data = zomeCallParamsSigned
                    };

                    // Taken before sending: a fast reply removes the entry from _taskCompletionZomeCallBack, so reading it after
                    // the send could throw KeyNotFoundException.
                    Task<ZomeFunctionCallBackEventArgs> zomeResultTask = _taskCompletionZomeCallBack[id].Task;

                    //await SendHoloNETRequestAsync(holoNETData, HoloNETRequestType.ZomeCall, id);
                    TrackPendingCall(id, _taskCompletionZomeCallBack);
                    await SendTrackedRequestAsync(holoNETData, HoloNETRequestType.ZomeCall, id);

                    if (zomeResultCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
                    {
                        //Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                        Task<ZomeFunctionCallBackEventArgs> returnValue = zomeResultTask;
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
                    string msg = ZomeCallSigner != null ? $"Error occurred in HoloNETClient.CallZomeFunctionAsync method: the configured ZomeCallSigner did not sign the call to {zome}.{function}." : $"Error occurred in HoloNETClient.CallZomeFunctionAsync method: Cannot sign zome call when no signing credentials have been authorized for the cell (AgentPubKey: {HoloNETDNA.AgentPubKey}, DnaHash: {HoloNETDNA.DnaHash}).";
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            //return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse); // bound the mode to a dynamic param
            return CallZomeFunctionSync(__requestId, zome, function, null, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            //return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, zomeResultCallBackMode); // bound the mode to a dynamic param
            return CallZomeFunctionSync(__requestId, zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            //return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode); // bound the mode to a dynamic param
            return CallZomeFunctionSync(__requestId, zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return CallZomeFunction(__requestId, zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            //return CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode); // returned the Task: a runtime binder error
            object entryDataObject = entryDataObjectReturnedFromZome;
            string id = __requestId;
            return Task.Run(() => CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObject, zomeResultCallBackMode)).GetAwaiter().GetResult();
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
            //return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode); // mode was dropped
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunctionSync(id, zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            //return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode); // bound the mode to a dynamic param
            return CallZomeFunctionSync(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            //return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents);
            return CallZomeFunctionSync(id, zome, function, null, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            return CallZomeFunction(__requestId, zome, function, callback, paramsObject, true, false, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            //return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents); // returned before the conductor replied
            return CallZomeFunction(__requestId, zome, function, callback, paramsObject, true, cachReturnData, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
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
            //_currentId++; // not atomic: two threads could get the same id (see GetRequestId)
            string __requestId = GetRequestId();
            //return CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, ConductorResponseCallBackMode.UseCallBackEvents); // returned the Task: a runtime binder error
            object entryDataObject = entryDataObjectReturnedFromZome;
            string id = __requestId;
            //return Task.Run(() => CallZomeFunctionAsync(id, zome, function, callback, paramsObject, true, false, entryDataObject, ConductorResponseCallBackMode.WaitForHolochainConductorResponse)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => CallZomeFunctionAsync(id, zome, function, callback, paramsObject, true, false, entryDataObject, mode));
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            //return Task.Run(() => CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents)).Result;
            return CallZomeFunctionSync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            //return CallZomeFunction(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents); // bound the mode to a dynamic param
            return CallZomeFunctionSync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
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
            //if (string.IsNullOrEmpty(id)) { _currentId++; id = _currentId.ToString(); } // a null id was used as a dictionary key (ArgumentNullException) // not atomic (see GetRequestId)
            if (string.IsNullOrEmpty(id)) { id = GetRequestId(); } // a null id was used as a dictionary key (ArgumentNullException)
            //return Task.Run(() => CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.UseCallBackEvents)).Result;
            return CallZomeFunctionSync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, ConductorResponseCallBackMode.WaitForHolochainConductorResponse);
        }

        /// <summary>
        /// Core of the synchronous CallZomeFunction overloads. Waits for the conductor's response unless the caller asked for
        /// <see cref="ConductorResponseCallBackMode.UseCallBackEvents"/>. Runs on the thread pool so a caller with a
        /// synchronisation context (UI thread, Unity main thread) can't deadlock on the websocket continuation.
        /// The callback and OnZomeFunctionCallBack events are raised either way.
        /// </summary>
        private ZomeFunctionCallBackEventArgs CallZomeFunctionSync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToZomeFuncInCallback, bool cachReturnData, ConductorResponseCallBackMode zomeResultCallBackMode)
        {
            return Task.Run(() => CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToZomeFuncInCallback, cachReturnData, zomeResultCallBackMode)).GetAwaiter().GetResult();
        }

        // New in Holochain 0.7.0 - App API.

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
            //return Task.Run(() => CreateCloneCellAsync(roleName, modifiers, membraneProof, name, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => CreateCloneCellAsync(roleName, modifiers, membraneProof, name, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // dynamic argument: returned the Task (runtime binder error)
            object modifiersObject = modifiers;
            //return Task.Run(() => CreateCloneCellAsync(roleName, modifiersObject, membraneProof, name, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => CreateCloneCellAsync(roleName, modifiersObject, membraneProof, name, mode, id));
        }

        /// <summary>
        /// Enable a previously disabled clone cell.
        /// </summary>
        /// <param name="cloneCellId">A clone id string (e.g. "role_name.0"), the clone DNA hash (byte[]), a CellId (byte[][], its DNA hash is used), or a CloneCellId.From* value for the clone cell to enable.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCloneCellEnabledCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<CloneCellEnabledCallBackEventArgs> EnableCloneCellAsync(dynamic cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppEnableCloneCell, "enable_clone_cell", new EnableCloneCellRequest()
            {
                clone_cell_id = CloneCellId.Normalize((object)cloneCellId)
            }, _taskCompletionCloneCellEnabledCallBack, "OnCloneCellEnabledCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Enable a previously disabled clone cell.
        /// </summary>
        /// <param name="cloneCellId">A clone id string (e.g. "role_name.0"), the clone DNA hash (byte[]), a CellId (byte[][], its DNA hash is used), or a CloneCellId.From* value for the clone cell to enable.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CloneCellEnabledCallBackEventArgs EnableCloneCell(dynamic cloneCellId, string id = null)
        {
            //return Task.Run(() => EnableCloneCellAsync(cloneCellId, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => EnableCloneCellAsync(cloneCellId, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // dynamic argument: returned the Task (runtime binder error)
            object cloneCellIdObject = cloneCellId;
            //return Task.Run(() => EnableCloneCellAsync(cloneCellIdObject, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => EnableCloneCellAsync(cloneCellIdObject, mode, id));
        }

        /// <summary>
        /// Disable a clone cell.
        /// </summary>
        /// <param name="cloneCellId">A clone id string (e.g. "role_name.0"), the clone DNA hash (byte[]), a CellId (byte[][], its DNA hash is used), or a CloneCellId.From* value for the clone cell to disable.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCloneCellDisabledCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public async Task<CloneCellDisabledCallBackEventArgs> DisableCloneCellAsync(dynamic cloneCellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppDisableCloneCell, "disable_clone_cell", new DisableCloneCellRequest()
            {
                clone_cell_id = CloneCellId.Normalize((object)cloneCellId)
            }, _taskCompletionCloneCellDisabledCallBack, "OnCloneCellDisabledCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Disable a clone cell.
        /// </summary>
        /// <param name="cloneCellId">A clone id string (e.g. "role_name.0"), the clone DNA hash (byte[]), a CellId (byte[][], its DNA hash is used), or a CloneCellId.From* value for the clone cell to disable.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CloneCellDisabledCallBackEventArgs DisableCloneCell(dynamic cloneCellId, string id = null)
        {
            //return Task.Run(() => DisableCloneCellAsync(cloneCellId, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => DisableCloneCellAsync(cloneCellId, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // dynamic argument: returned the Task (runtime binder error)
            object cloneCellIdObject = cloneCellId;
            //return Task.Run(() => DisableCloneCellAsync(cloneCellIdObject, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => DisableCloneCellAsync(cloneCellIdObject, mode, id));
        }

        /// <summary>
        /// Get the state of a countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to get the countersigning session state for.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCountersigningSessionStateReturnedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        // Only available when the conductor is built with the unstable-countersigning feature (not the
        // standard 0.7.0 release); otherwise the conductor answers "Failed to deserialize request".
        public async Task<CountersigningSessionStateReturnedCallBackEventArgs> GetCountersigningSessionStateAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppGetCountersigningSessionState, "get_countersigning_session_state", cellId /* tuple variant (Box<CellId>): the value is the CellId itself, not {cell_id} */, _taskCompletionCountersigningSessionStateReturnedCallBack, "OnCountersigningSessionStateReturnedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Get the state of a countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to get the countersigning session state for.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CountersigningSessionStateReturnedCallBackEventArgs GetCountersigningSessionState(CellId cellId, string id = null)
        {
            //return Task.Run(() => GetCountersigningSessionStateAsync(cellId, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => GetCountersigningSessionStateAsync(cellId, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => GetCountersigningSessionStateAsync(cellId, mode, id));
        }

        /// <summary>
        /// Abandon an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to abandon the countersigning session for.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnCountersigningSessionAbandonedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        // Only available when the conductor is built with the unstable-countersigning feature (not the
        // standard 0.7.0 release); otherwise the conductor answers "Failed to deserialize request".
        public async Task<CountersigningSessionAbandonedCallBackEventArgs> AbandonCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppAbandonCountersigningSession, "abandon_countersigning_session", cellId /* tuple variant (Box<CellId>): the value is the CellId itself, not {cell_id} */, _taskCompletionCountersigningSessionAbandonedCallBack, "OnCountersigningSessionAbandonedCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Abandon an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to abandon the countersigning session for.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public CountersigningSessionAbandonedCallBackEventArgs AbandonCountersigningSession(CellId cellId, string id = null)
        {
            //return Task.Run(() => AbandonCountersigningSessionAsync(cellId, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => AbandonCountersigningSessionAsync(cellId, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => AbandonCountersigningSessionAsync(cellId, mode, id));
        }

        /// <summary>
        /// Publish (force-finalise) an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to publish the countersigning session for.</param>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnPublishCountersigningSessionTriggeredCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        // Only available when the conductor is built with the unstable-countersigning feature (not the
        // standard 0.7.0 release); otherwise the conductor answers "Failed to deserialize request".
        public async Task<PublishCountersigningSessionTriggeredCallBackEventArgs> PublishCountersigningSessionAsync(CellId cellId, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppPublishCountersigningSession, "publish_countersigning_session", cellId /* tuple variant (Box<CellId>): the value is the CellId itself, not {cell_id} */, _taskCompletionPublishCountersigningSessionTriggeredCallBack, "OnPublishCountersigningSessionTriggeredCallBack", conductorResponseCallBackMode, id);
        }

        /// <summary>
        /// Publish (force-finalise) an in-progress countersigning session for the given cell. Gated behind the `unstable-countersigning` feature flag in Holochain 0.7.0 - may not be available on all conductor builds.
        /// </summary>
        /// <param name="cellId">The CellId to publish the countersigning session for.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        public PublishCountersigningSessionTriggeredCallBackEventArgs PublishCountersigningSession(CellId cellId, string id = null)
        {
            //return Task.Run(() => PublishCountersigningSessionAsync(cellId, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => PublishCountersigningSessionAsync(cellId, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => PublishCountersigningSessionAsync(cellId, mode, id));
        }

        /// <summary>
        /// List the names of the host functions available to WASM code running in this conductor.
        /// </summary>
        /// <param name="conductorResponseCallBackMode">The Concuctor Response CallBack Mode, set this to 'WaitForHolochainConductorResponse' if you want the function to wait for the Holochain Conductor response before returning that response or set it to 'UseCallBackEvents' to return from the function immediately and then raise the 'OnWasmHostFunctionsListedCallBack' event when the conductor responds.</param>
        /// <param name="id">The request id, leave null if you want HoloNET to manage this for you.</param>
        /// <returns></returns>
        /// <summary>
        /// The agent infos (encoded JSON strings) this conductor holds for this app's DNAs, or only for
        /// <paramref name="dnaHashes"/> (app interface agent_info, Holochain 0.7).
        /// </summary>
        public async Task<AgentInfoReturnedCallBackEventArgs> GetAgentInfoAsync(IEnumerable<byte[]> dnaHashes = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppAgentInfo, "agent_info",
                new Dictionary<string, object> { ["dna_hashes"] = dnaHashes?.ToArray() },
                _taskCompletionAgentInfoReturnedCallBack, "OnAgentInfoReturnedCallBack", conductorResponseCallBackMode, id);
        }

        public AgentInfoReturnedCallBackEventArgs GetAgentInfo(IEnumerable<byte[]> dnaHashes = null, string id = null)
        {
            //return Task.Run(() => GetAgentInfoAsync(dnaHashes, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => GetAgentInfoAsync(dnaHashes, mode, id));
        }

        /// <summary>Kitsune network metrics for this app's DNAs (or just <paramref name="dnaHash"/>) as JSON (app interface, Holochain 0.7).</summary>
        public async Task<NetworkMetricsDumpedCallBackEventArgs> DumpNetworkMetricsAsync(byte[] dnaHash = null, bool includeDhtSummary = false, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppDumpNetworkMetrics, "dump_network_metrics",
                new Dictionary<string, object> { ["dna_hash"] = dnaHash, ["include_dht_summary"] = includeDhtSummary },
                _taskCompletionNetworkMetricsDumpedCallBack, "OnNetworkMetricsDumpedCallBack", conductorResponseCallBackMode, id);
        }

        public NetworkMetricsDumpedCallBackEventArgs DumpNetworkMetrics(byte[] dnaHash = null, bool includeDhtSummary = false, string id = null)
        {
            //return Task.Run(() => DumpNetworkMetricsAsync(dnaHash, includeDhtSummary, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => DumpNetworkMetricsAsync(dnaHash, includeDhtSummary, mode, id));
        }

        /// <summary>Transport statistics from the networking backend as JSON (app interface, Holochain 0.7).</summary>
        public async Task<NetworkStatsDumpedCallBackEventArgs> DumpNetworkStatsAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppDumpNetworkStats, "dump_network_stats", null,
                _taskCompletionNetworkStatsDumpedCallBack, "OnNetworkStatsDumpedCallBack", conductorResponseCallBackMode, id);
        }

        public NetworkStatsDumpedCallBackEventArgs DumpNetworkStats(string id = null)
        {
            //return Task.Run(() => DumpNetworkStatsAsync(ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => DumpNetworkStatsAsync(mode, id));
        }

        /// <summary>
        /// Enables the app this connection is authenticated for (app interface enable_app, Holochain 0.7), e.g. once
        /// deferred memproofs have been provided.
        /// </summary>
        public async Task<AppEnabledViaAppInterfaceCallBackEventArgs> EnableAppAsync(ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppEnableApp, "enable_app", null,
                _taskCompletionAppEnabledCallBack, "OnAppEnabledCallBack", conductorResponseCallBackMode, id);
        }

        public AppEnabledViaAppInterfaceCallBackEventArgs EnableApp(string id = null)
        {
            //return Task.Run(() => EnableAppAsync(ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => EnableAppAsync(mode, id));
        }

        /// <summary>
        /// Sends <paramref name="signal"/> (opaque bytes) to <paramref name="agents"/> on the DNA <paramref name="dnaHash"/>
        /// (send_direct_signal). Best effort: the conductor confirms it was sent, not delivered. On Holochain 0.7.1+ a recipient
        /// only accepts it if it has a direct-signal grant for the sender (see <see cref="GrantDirectSignalCapabilityAsync"/>);
        /// pass that grant's secret as <paramref name="capSecret"/> when it has one. Recipients get it as an app signal.
        /// </summary>
        public async Task<DirectSignalSentCallBackEventArgs> SendDirectSignalAsync(byte[] dnaHash, IEnumerable<byte[]> agents, byte[] signal, byte[] capSecret = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            var request = new Dictionary<string, object>
            {
                ["dna_hash"] = dnaHash,
                ["agents"] = agents?.ToArray() ?? Array.Empty<byte[]>(),
                // Vec<u8> without serde_bytes: a msgpack array of integers, not bin.
                ["signal"] = (signal ?? Array.Empty<byte>()).Select(b => (int)b).ToArray()
            };
            // Holochain 0.7.1 field; omitted when unset so 0.7.0 conductors accept the request too.
            if (capSecret != null)
                request["cap_secret"] = capSecret;

            return await CallFunctionAsync(HoloNETRequestType.AppSendDirectSignal, "send_direct_signal", request,
                _taskCompletionDirectSignalSentCallBack, "OnDirectSignalSentCallBack", conductorResponseCallBackMode, id);
        }

        public DirectSignalSentCallBackEventArgs SendDirectSignal(byte[] dnaHash, IEnumerable<byte[]> agents, byte[] signal, byte[] capSecret = null, string id = null)
        {
            //return Task.Run(() => SendDirectSignalAsync(dnaHash, agents, signal, capSecret, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => SendDirectSignalAsync(dnaHash, agents, signal, capSecret, mode, id));
        }

        /// <summary>
        /// Holochain 0.7.1+: lets other agents send direct signals to <paramref name="cellId"/> (a cell of this app)
        /// (grant_direct_signal_capability). Holochain 0.7.0 doesn't have this request and returns an error.
        /// </summary>
        public async Task<DirectSignalCapabilityGrantedCallBackEventArgs> GrantDirectSignalCapabilityAsync(byte[][] cellId, string tag, CapGrantAccessType access = CapGrantAccessType.Unrestricted, byte[] secret = null, byte[][] assignees = null, ConductorResponseCallBackMode conductorResponseCallBackMode = ConductorResponseCallBackMode.WaitForHolochainConductorResponse, string id = null)
        {
            return await CallFunctionAsync(HoloNETRequestType.AppGrantDirectSignalCapability, "grant_direct_signal_capability",
                DirectSignalGrants.Request(cellId, tag, access, secret, assignees),
                _taskCompletionDirectSignalCapabilityGrantedCallBack, "OnDirectSignalCapabilityGrantedCallBack", conductorResponseCallBackMode, id);
        }

        public DirectSignalCapabilityGrantedCallBackEventArgs GrantDirectSignalCapability(byte[][] cellId, string tag, CapGrantAccessType access = CapGrantAccessType.Unrestricted, byte[] secret = null, byte[][] assignees = null, string id = null)
        {
            //return Task.Run(() => GrantDirectSignalCapabilityAsync(cellId, tag, access, secret, assignees, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => GrantDirectSignalCapabilityAsync(cellId, tag, access, secret, assignees, mode, id));
        }

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
            //return Task.Run(() => ListWasmHostFunctionsAsync(ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => ListWasmHostFunctionsAsync(ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => ListWasmHostFunctionsAsync(mode, id));
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
            //return Task.Run(() => ProvideMemproofsAsync(membraneProofs, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => ProvideMemproofsAsync(membraneProofs, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => ProvideMemproofsAsync(membraneProofs, mode, id));
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
            //return Task.Run(() => GetAppPeerMetaInfoAsync(url, dnaHashes, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => GetAppPeerMetaInfoAsync(url, dnaHashes, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => GetAppPeerMetaInfoAsync(url, dnaHashes, mode, id));
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
            //return Task.Run(() => DumpOpTimingsAsync(dnaHash, cursor, limit, ConductorResponseCallBackMode.UseCallBackEvents, id)).Result; // returned before the conductor replied
            //return Task.Run(() => DumpOpTimingsAsync(dnaHash, cursor, limit, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, id)).GetAwaiter().GetResult(); // deadlocked when called from inside a HoloNET event (see RunSync)
            return RunSync(mode => DumpOpTimingsAsync(dnaHash, cursor, limit, mode, id));
        }

        private static byte[] GetCryptographicRandomBytes(int length)
        {
            var bytes = new byte[length];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
            return bytes;
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

            taskCompletionCallBack[id] = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            Task<T> returnValue = taskCompletionCallBack[id].Task;
            TrackPendingCall(id, taskCompletionCallBack);
            //await SendHoloNETRequestAsync(holoNETData, requestType, id);
            await SendTrackedRequestAsync(holoNETData, requestType, id);

            if (conductorResponseCallBackMode == ConductorResponseCallBackMode.WaitForHolochainConductorResponse)
            {
                //Task<T> returnValue = taskCompletionCallBack[id].Task;
                T result = await returnValue;
                UntrackPendingCall(id);
                return result;
            }
            else if (returnValue.IsCompleted)
            {
                // Failed before it was sent (not connected / invalid id): report it rather than "please wait".
                UntrackPendingCall(id);
                return await returnValue;
            }
            else
                return new T() { EndPoint = EndPoint, Id = id, Message = $"conductorResponseCallBackMode is set to UseCallBackEvents so please wait for {eventCallBackName} event for the result." };
        }
    }
}
