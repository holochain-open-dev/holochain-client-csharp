using System;
using System.Text;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using MessagePack;
using NextGenSoftware.WebSocket;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETClient
    {
        private bool _getAgentPubKeyAndDnaHashFromConductor;
        private bool _updateDnaHashAndAgentPubKey = true;
        //private TaskCompletionSource<GetInstancesCallBackEventArgs> _taskCompletionSourceGetInstance = new TaskCompletionSource<GetInstancesCallBackEventArgs>();
        private Dictionary<string, string> _instanceLookup = new Dictionary<string, string>();
        private Dictionary<string, string> _zomeLookup = new Dictionary<string, string>();
        private Dictionary<string, string> _funcLookup = new Dictionary<string, string>();
        private Dictionary<string, ZomeFunctionCallBack> _callbackLookup = new Dictionary<string, ZomeFunctionCallBack>();
        private Dictionary<string, ZomeFunctionCallBackEventArgs> _zomeReturnDataLookup = new Dictionary<string, ZomeFunctionCallBackEventArgs>();
        private Dictionary<string, bool> _cacheZomeReturnDataLookup = new Dictionary<string, bool>();
        private int _currentId = 0;
        private HoloNETConfig _config = null;

        //Events
        public delegate void Connected(object sender, ConnectedEventArgs e);
        public event Connected OnConnected;

        public delegate void Disconnected(object sender, DisconnectedEventArgs e);
        public event Disconnected OnDisconnected;

        public delegate void DataReceived(object sender, HoloNETDataReceivedEventArgs e);
        public event DataReceived OnDataReceived;

        public delegate void ZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e);
        public event ZomeFunctionCallBack OnZomeFunctionCallBack;

        public delegate void SignalsCallBack(object sender, SignalsCallBackEventArgs e);
        public event SignalsCallBack OnSignalsCallBack;

        public delegate void ConductorDebugCallBack(object sender, ConductorDebugCallBackEventArgs e);
        public event ConductorDebugCallBack OnConductorDebugCallBack;

        public delegate void AppInfoCallBack(object sender, AppInfoCallBackEventArgs e);
        public event AppInfoCallBack OnAppInfoCallBack;

        public delegate void ReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e);
        public event ReadyForZomeCalls OnReadyForZomeCalls;

        public delegate void Error(object sender, HoloNETErrorEventArgs e);
        public event Error OnError;

        // Properties
        public WebSocket.WebSocket WebSocket { get; set; }

        public HoloNETConfig Config
        {
            get
            {
                if (_config == null)
                    _config = new HoloNETConfig();

                return _config;
            }
        }

        public WebSocketState State
        {
            get
            {
                return WebSocket.State;
            }
        }

        public string EndPoint
        {
            get
            {
                if (WebSocket != null)
                    return WebSocket.EndPoint;

                return "";
            }
        }

        public ILogger Logger { get; set; }

        public HoloNETClient(string holochainConductorURI)
        {
            Logger = new ConsoleLogger();
            Init(holochainConductorURI);
        }

        public HoloNETClient(string holochainConductorURI, ILogger logger)
        {
            Logger = logger;
            Init(holochainConductorURI);
        }

        private void Init(string holochainConductorURI)
        {
            //HolochainVersion = version;
            WebSocket = new WebSocket.WebSocket(holochainConductorURI, Logger);

            //TODO: Impplemnt IDispoasable to unsubscribe event handlers to prevent memory leaks... 
            WebSocket.OnConnected += WebSocket_OnConnected;
            WebSocket.OnDataReceived += WebSocket_OnDataReceived;
            WebSocket.OnDisconnected += WebSocket_OnDisconnected;
            WebSocket.OnError += WebSocket_OnError;
        }

        private void WebSocket_OnError(object sender, WebSocketErrorEventArgs e)
        {
            HandleError(e.Reason, e.ErrorDetails);
        }

        private void WebSocket_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            ShutDownConductors();
            OnDisconnected?.Invoke(this, new DisconnectedEventArgs { EndPoint = e.EndPoint, Reason = e.Reason });
        }

        private void WebSocket_OnDataReceived(object sender, WebSocket.DataReceivedEventArgs e)
        {
            try
            {
                OnDataReceived?.Invoke(this, new HoloNETDataReceivedEventArgs { EndPoint = e.EndPoint, RawJSONData = e.RawJSONData, RawBinaryData = e.RawBinaryData, WebSocketResult = e.WebSocketResult });

                StringBuilder sb = new StringBuilder();
                sb.Append(Encoding.UTF8.GetString(e.RawBinaryData, 0, e.RawBinaryData.Length));
                string rawData = sb.ToString();
                Logger.Log("Raw Data Received: " + rawData, LogType.Debug);

                var options = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
                HoloNETResponse response = MessagePackSerializer.Deserialize<HoloNETResponse>(e.RawBinaryData, options);

                Logger.Log("RSM Response", LogType.Debug);
                Logger.Log($"Id: {response.id}", LogType.Debug);
                Logger.Log($"Type: {response.type}", LogType.Debug);

                if (rawData.Substring(32, 8) == "app_info")
                {
                    Logger.Log("\nAPP INFO RESPONSE DATA DETECTED\n", LogType.Debug);
                    HolonNETAppInfoResponse appInfoResponse = MessagePackSerializer.Deserialize<HolonNETAppInfoResponse>(response.data, options);

                    string dnHash = ConvertHoloHashToString(appInfoResponse.data.cell_data[0].cell_id[0]);
                    string agentPubKey = ConvertHoloHashToString(appInfoResponse.data.cell_data[0].cell_id[1]);

                    if (_updateDnaHashAndAgentPubKey)
                    {
                        Config.AgentPubKey = agentPubKey;
                        Config.DnaHash = dnHash;
                    }

                    AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs(response.id.ToString(), EndPoint, true, e.RawBinaryData, e.RawJSONData, dnHash, agentPubKey, appInfoResponse.data.installed_app_id, appInfoResponse, e.WebSocketResult);
                    Logger.Log(string.Concat("Id: ", args.Id, ", Is Call Successful: ", args.IsCallSuccessful ? "True" : "False", ", AgentPubKey: ", args.AgentPubKey, ", DnaHash: ", args.DnaHash, ", Installed App Id: ", args.InstalledAppId, ", Raw Binary Data: ",  e.RawBinaryData, ", Raw JSON Data: ", args.RawJSONData), LogType.Info);
                    OnAppInfoCallBack?.Invoke(this, args);

                    //If either the AgentPubKey or DnaHash is empty then attempt to get from the sandbox cmd.
                    if (string.IsNullOrEmpty(Config.AgentPubKey) || string.IsNullOrEmpty(Config.DnaHash))
                        GetAgentPubKeyAndDnaHashFromSandbox();
                    else
                        OnReadyForZomeCalls?.Invoke(this, new ReadyForZomeCallsEventArgs(dnHash, agentPubKey));
                }
                else
                {
                    HolonNETAppResponse appResponse = MessagePackSerializer.Deserialize<HolonNETAppResponse>(response.data, options);
                    Dictionary<object, object> rawAppResponseData = MessagePackSerializer.Deserialize<Dictionary<object, object>>(appResponse.data, options);
                    Dictionary<string, object> appResponseData = new Dictionary<string, object>();

                    string data = "";
                    string value = "";
                    foreach (string key in rawAppResponseData.Keys)
                    {
                        byte[] bytes = rawAppResponseData[key] as byte[];

                        if (bytes != null)
                            value = ConvertHoloHashToString(bytes);
                        else
                            value = rawAppResponseData[key].ToString();

                        data = string.Concat(data, key, "=", value, "\n");
                        appResponseData[key] = value;
                    }
                    Logger.Log("\nZOME RESPONSE DATA DETECTED\n", LogType.Debug);
                    Logger.Log($"Decoded Data:\n{data}", LogType.Debug);

                    string id = response.id.ToString();
                    ZomeFunctionCallBackEventArgs args = new ZomeFunctionCallBackEventArgs(id, e.EndPoint, GetItemFromCache(id, _instanceLookup), GetItemFromCache(id, _zomeLookup), GetItemFromCache(id, _funcLookup), true, rawData, rawAppResponseData, appResponseData, e.RawBinaryData, e.RawJSONData, e.WebSocketResult);
                    Logger.Log(string.Concat("Id: ", args.Id, ", Instance: ", args.Instance, ", Zome: ", args.Zome, ", Zome Function: ", args.ZomeFunction, ", Is Zome Call Successful: ", args.IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", args.RawZomeReturnData, ", Zome Return Data: ", args.ZomeReturnData, ", Raw Binary Data: ", e.RawBinaryData, ", Raw JSON Data: ", args.RawJSONData), LogType.Info);

                    if (_callbackLookup.ContainsKey(id) && _callbackLookup[id] != null)
                        _callbackLookup[id].DynamicInvoke(this, args);

                    OnZomeFunctionCallBack?.Invoke(this, args);

                    // If the zome call requested for this to be cached then stick it in cache.
                    if (_cacheZomeReturnDataLookup[id])
                        _zomeReturnDataLookup[id] = args;

                    _instanceLookup.Remove(id);
                    _zomeLookup.Remove(id);
                    _funcLookup.Remove(id);
                    _callbackLookup.Remove(id);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClientBase.WebSocket_OnDataReceived.", ex);
            }
        }

        private void WebSocket_OnConnected(object sender, ConnectedEventArgs e)
        {
            OnConnected?.Invoke(this, new ConnectedEventArgs { EndPoint = e.EndPoint });

            if (_getAgentPubKeyAndDnaHashFromConductor)
                GetAgentPubKeyAndDnaHashFromConductor();
        }

        public async Task Connect(bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = false)
        {
            try
            {
                _getAgentPubKeyAndDnaHashFromConductor = getAgentPubKeyAndDnaHashFromConductor;

                if (Logger == null)
                    throw new HoloNETException("ERROR: No Logger Has Been Specified! Please set a Logger with the Logger Property.");

                if (WebSocket.State != WebSocketState.Connecting && WebSocket.State != WebSocketState.Open && WebSocket.State != WebSocketState.Aborted)
                {
                    if (Config.AutoStartConductor && !string.IsNullOrEmpty(Config.FullPathToHolochainAppDNA))
                    {
                        DirectoryInfo info = new DirectoryInfo(Config.FullPathToHolochainAppDNA);
                        Logger.Log("Starting Holochain Conductor...", LogType.Info);

                        Process pProcess = new Process();
                        pProcess.StartInfo.WorkingDirectory = @"C:\holochain-holonix-v0.0.80-9-g6a1542d";
                        pProcess.StartInfo.FileName = "wsl";
                        // pProcess.StartInfo.Arguments = "run";
                        pProcess.StartInfo.UseShellExecute = false;
                        pProcess.StartInfo.RedirectStandardOutput = false;
                        pProcess.StartInfo.RedirectStandardInput = true;
                        pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        pProcess.StartInfo.CreateNoWindow = false;
                        pProcess.Start();

                        await Task.Delay(Config.SecondsToWaitForHolochainConductorToStart); // Give the conductor 5 seconds to start up...

                        // pProcess.StandardInput.WriteLine("nix-shell https://holochain.love");
                        //pProcess.StandardInput.WriteLine("nix-shell holochain-holonix-6a1542d");
                        pProcess.StandardInput.WriteLine("nix-shell C:\\holochain-holonix-v0.0.80-9-g6a1542d\\holochain-holonix-6a1542d");

                        await Task.Delay(Config.SecondsToWaitForHolochainConductorToStart); // Give the conductor 5 seconds to start up...

                        /*
                        //If no path to the conductor has been given then default to the current working directory.
                        if (string.IsNullOrEmpty(Config.FullPathToExternalHolochainConductor))
                            Config.FullPathToExternalHolochainConductor = string.Concat(Directory.GetCurrentDirectory(), "\\hc.exe"); //default to the current path

                        if (Config.SecondsToWaitForHolochainConductorToStart == 0)
                            Config.SecondsToWaitForHolochainConductorToStart = SecondsToWaitForConductorToStartDefault;

                        FileInfo conductorInfo = new FileInfo(Config.FullPathToExternalHolochainConductor);


                        //Make sure the condctor is not already running
                        if (!Process.GetProcesses().Any(x => x.ProcessName == conductorInfo.Name))
                        {
                            DirectoryInfo info = new DirectoryInfo(Config.FullPathToHolochainAppDNA);
                            Logger.Log("Starting Holochain Conductor...", LogType.Info);

                            Process pProcess = new Process();
                            pProcess.StartInfo.WorkingDirectory = info.Parent.Parent.FullName;
                            pProcess.StartInfo.FileName = Config.FullPathToExternalHolochainConductor;
                            pProcess.StartInfo.Arguments = "run";
                            pProcess.StartInfo.UseShellExecute = true;
                            pProcess.StartInfo.RedirectStandardOutput = false;
                            pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                            pProcess.StartInfo.CreateNoWindow = false;
                            pProcess.Start();

                            await Task.Delay(Config.SecondsToWaitForHolochainConductorToStart); // Give the conductor 5 seconds to start up...
                        }*/
                    }

                    Logger.Log(string.Concat("Connecting to ", WebSocket.EndPoint, "..."), LogType.Info);
                    await WebSocket.Connect();

                    if (getAgentPubKeyAndDnaHashFromSandbox)
                    {
                        AgentPubKeyDnaHash agentPubKeyDnaHash = await GetAgentPubKeyAndDnaHashFromSandbox();

                        if (agentPubKeyDnaHash != null)
                        {
                            Config.AgentPubKey = agentPubKeyDnaHash.AgentPubKey;
                            Config.DnaHash = agentPubKeyDnaHash.DnaHash;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(string.Concat("Error occured connecting to ", WebSocket.EndPoint), e);
            }
        }

        public async Task<AgentPubKeyDnaHash> GetAgentPubKeyAndDnaHashFromSandbox(bool updateConfig = true)
        {
            try
            {
                Process pProcess = new Process();
                pProcess.StartInfo.WorkingDirectory = Config.FullPathToHapp;
                pProcess.StartInfo.FileName = "hc";
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

                if (updateConfig)
                {
                    Config.AgentPubKey = agentPubKey;
                    Config.DnaHash = dnaHash;
                }

                if (!string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                    OnReadyForZomeCalls?.Invoke(this, new ReadyForZomeCallsEventArgs(dnaHash, agentPubKey));

                return new AgentPubKeyDnaHash() { DnaHash = dnaHash, AgentPubKey = agentPubKey };
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClientBase.GetAgentPubKeyAndDnaHashFromhApp getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        public async Task GetAgentPubKeyAndDnaHashFromConductor(bool updateConfig = true)
        {
            try
            {
                _updateDnaHashAndAgentPubKey = updateConfig;

                HoloNETData holoNETData = new HoloNETData()
                {
                    type = "app_info",
                    data = new HoloNETDataAppInfoCall()
                    {
                        installed_app_id = "test-app"
                    }
                };

                await SendHoloNETRequest("1", holoNETData);
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClientBase.GetAgentPubKeyAndDnaHash.", ex);
            }
        }

        public async Task SendHoloNETRequest(string id, HoloNETData holoNETData)
        {
            try
            {
                HoloNETRequest request = new HoloNETRequest()
                {
                    id = Convert.ToUInt64(id),
                    type = "Request",
                    data = MessagePackSerializer.Serialize(holoNETData)
                };

                if (WebSocket.State == WebSocketState.Open)
                    await WebSocket.SendRawDataAsync(MessagePackSerializer.Serialize(request)); //This is the fastest and most popular .NET MessagePack Serializer.
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClientBase.SendHoloNETRequest.", ex);
            }
        }

        public async Task Disconnect()
        {
            await WebSocket.Disconnect();
        }

        public async Task CallZomeFunctionAsync(string instanceId, string zome, string function, object paramsObject, bool cachReturnData = false)
        {
            _currentId++;
            await CallZomeFunctionAsync(_currentId.ToString(), instanceId, zome, function, null, paramsObject, true, cachReturnData);
        }

        public async Task CallZomeFunctionAsync(string id, string instanceId, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false)
        {
            await CallZomeFunctionAsync(id, instanceId, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData);
        }

        public async Task CallZomeFunctionAsync(string instanceId, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false)
        {
            _currentId++;
            await CallZomeFunctionAsync(_currentId.ToString(), instanceId, zome, function, callback, paramsObject, true, cachReturnData);
        }

        public async Task CallZomeFunctionAsync(string id, string instanceId, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false)
        {
            _cacheZomeReturnDataLookup[id] = cachReturnData;

            try
            {
                if (WebSocket.State == WebSocketState.Closed || WebSocket.State == WebSocketState.None)
                    await Connect();

                if (cachReturnData)
                {
                    if (_zomeReturnDataLookup.ContainsKey(id))
                    {
                        Logger.Log("Caching Enabled so returning data from cach...", LogType.Warn);
                        Logger.Log(string.Concat("Id: ", _zomeReturnDataLookup[id].Id, ", Instance: ", _zomeReturnDataLookup[id].Instance, ", Zome: ", _zomeReturnDataLookup[id].Zome, ", Zome Function: ", _zomeReturnDataLookup[id].ZomeFunction, ", Is Zome Call Successful: ", _zomeReturnDataLookup[id].IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", _zomeReturnDataLookup[id].RawZomeReturnData, ", Zome Return Data: ", _zomeReturnDataLookup[id].ZomeReturnData, ", JSON Raw Data: ", _zomeReturnDataLookup[id].RawJSONData), LogType.Info);

                        if (callback != null)
                            callback.DynamicInvoke(this, _zomeReturnDataLookup[id]);

                        OnZomeFunctionCallBack?.Invoke(this, _zomeReturnDataLookup[id]);
                        return;
                    }
                }

                if (matchIdToInstanceZomeFuncInCallback)
                {
                    _instanceLookup[id] = instanceId;
                    _zomeLookup[id] = zome;
                    _funcLookup[id] = function;
                }

                if (callback != null)
                    _callbackLookup[id] = callback;


                //switch (HolochainVersion)
                //{
                //    case HolochainVersion.Redux:
                //        {
                //            if (WebSocket.State == WebSocketState.Open)
                //            {
                //                await WebSocket.SendRawDataAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                //                    new
                //                    {
                //                        jsonrpc = "2.0",
                //                        id,
                //                        method = "call",
                //                        @params = new { instance_id = instanceId, zome, function, args = paramsObject }
                //                    }
                //                )));
                //            }
                //        }
                //        break;

                //    case HolochainVersion.RSM:
                //        {
                            HoloNETData holoNETData = new HoloNETData()
                            {
                                type = "zome_call",
                                data = new HoloNETDataZomeCall()
                                {
                                    cell_id = new byte[2][] { ConvertHoloHashToBytes(Config.DnaHash), ConvertHoloHashToBytes(Config.AgentPubKey) },
                                    fn_name = function,
                                    zome_name = zome,
                                    payload = MessagePackSerializer.Serialize(paramsObject),
                                    provenance = ConvertHoloHashToBytes(Config.AgentPubKey),
                                    cap = null
                                }
                            };

                            await SendHoloNETRequest(id, holoNETData);
                        //}
                        //break;
               // }
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClientBase.CallZomeFunctionAsync", ex);
            }
        }

        public void ClearCache()
        {
            _zomeReturnDataLookup.Clear();
            _cacheZomeReturnDataLookup.Clear();
        }

        public byte[] ConvertHoloHashToBytes(string hash)
        {
            return Convert.FromBase64String(hash.Replace('-', '+').Replace('_', '/').Substring(1, hash.Length - 1)); //also remove the u prefix.
        }

        public string ConvertHoloHashToString(byte[] bytes)
        {
            return string.Concat("u", Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_"));
        }

        private string GetItemFromCache(string id, Dictionary<string, string> cache)
        {
            return cache.ContainsKey(id) ? cache[id] : null;
        }

        private void HandleError(string message, Exception exception)
        {
            message = string.Concat(message, exception != null ? $". Error Details: {exception}" : "");
            Logger.Log(message, LogType.Error);

            OnError?.Invoke(this, new HoloNETErrorEventArgs { EndPoint = WebSocket.EndPoint, Reason = message, ErrorDetails = exception });

            switch (Config.ErrorHandlingBehaviour)
            {
                case ErrorHandlingBehaviour.AlwaysThrowExceptionOnError:
                    throw new HoloNETException(message, exception, WebSocket.EndPoint);

                case ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent:
                    {
                        if (OnError == null)
                            throw new HoloNETException(message, exception, WebSocket.EndPoint);
                    }
                    break;
            }
        }

        private void ShutDownConductors()
        {
            // Close any conductors down if necessary.
            if (Config.AutoShutdownConductor)
            {
                FileInfo conductorInfo = new FileInfo(Config.FullPathToExternalHolochainConductor);
                foreach (Process process in Process.GetProcessesByName(conductorInfo.Name))
                    process.Kill();
            }
        }
    }
}