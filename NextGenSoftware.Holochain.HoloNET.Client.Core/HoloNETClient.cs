﻿using System;
using System.Text;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using MessagePack;
using NextGenSoftware.WebSocket;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Properties;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETClient
    {
        private bool _getAgentPubKeyAndDnaHashFromConductor;
        private bool _updateDnaHashAndAgentPubKey = true;
        private Dictionary<string, string> _zomeLookup = new Dictionary<string, string>();
        private Dictionary<string, string> _funcLookup = new Dictionary<string, string>();
        private Dictionary<string, ZomeFunctionCallBack> _callbackLookup = new Dictionary<string, ZomeFunctionCallBack>();
        private Dictionary<string, ZomeFunctionCallBackEventArgs> _zomeReturnDataLookup = new Dictionary<string, ZomeFunctionCallBackEventArgs>();
        private Dictionary<string, bool> _cacheZomeReturnDataLookup = new Dictionary<string, bool>();
        private int _currentId = 0;
        private HoloNETConfig _config = null;
        private Process _conductorProcess = null;

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

        public HoloNETClient(string holochainConductorURI = "ws://localhost:8888", bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            Logger.Loggers.Add(new DefaultLogger(logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour));
            Init(holochainConductorURI);
        }

        public HoloNETClient(ILogger logger, string holochainConductorURI = "ws://localhost:8888")
        {
            Logger.Loggers.Add(logger);
            Init(holochainConductorURI);
        }

        public HoloNETClient(IEnumerable<ILogger> loggers, string holochainConductorURI = "ws://localhost:8888")
        {
            Logger.Loggers = new List<ILogger>(loggers);
            Init(holochainConductorURI);
        }

        public async Task Connect(bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = false)
        {
            try
            {
                _getAgentPubKeyAndDnaHashFromConductor = getAgentPubKeyAndDnaHashFromConductor;

                if (Logger.Loggers.Count == 0)
                    throw new HoloNETException("ERROR: No Logger Has Been Specified! Please set a Logger with the Logger.Loggers Property.");

                if (WebSocket.State != WebSocketState.Connecting && WebSocket.State != WebSocketState.Open && WebSocket.State != WebSocketState.Aborted)
                {
                    if (Config.AutoStartHolochainConductor)
                        await StartConductor();

                    await WebSocket.Connect();

                    if (getAgentPubKeyAndDnaHashFromSandbox && !getAgentPubKeyAndDnaHashFromConductor)
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
                HandleError(string.Concat("Error occured in HoloNETClient.Connect method connecting to ", WebSocket.EndPoint), e);
            }
        }

        public async Task StartConductor()
        {
            try
            {
                // Was used when they were set to Content rather than Embedded.
                //string fullPathToEmbeddedHolochainConductorBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\holochain.exe");
                //string fullPathToEmbeddedHCToolBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\hc.exe");

                _conductorProcess = new Process();

                if (string.IsNullOrEmpty(Config.FullPathToExternalHolochainConductorBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && Config.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                    throw new ArgumentNullException("FullPathToExternalHolochainConductorBinary", "When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HolochainProductionConductor', FullPathToExternalHolochainConductorBinary cannot be empty.");

                if (string.IsNullOrEmpty(Config.FullPathToExternalHCToolBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    throw new ArgumentNullException("FullPathToExternalHCToolBinary", "When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HcDevTool', FullPathToExternalHCToolBinary cannot be empty.");

                if (!File.Exists(Config.FullPathToExternalHolochainConductorBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && Config.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HolochainProductionConductor', FullPathToExternalHolochainConductorBinary ({Config.FullPathToExternalHolochainConductorBinary}) must point to a valid file.");

                if (!File.Exists(Config.FullPathToExternalHCToolBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseExternal && Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseExternal' and HolochainConductorToUse is set to 'HcDevTool', FullPathToExternalHCToolBinary ({Config.FullPathToExternalHCToolBinary}) must point to a valid file.");

                //if (!File.Exists(fullPathToEmbeddedHolochainConductorBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseEmbedded && Config.HolochainConductorToUse == HolochainConductorEnum.HolochainProductionConductor)
                //    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseEmbedded' and HolochainConductorToUse is set to 'HolochainProductionConductor', you must ensure the holochain.exe is found here: {fullPathToEmbeddedHolochainConductorBinary}.");

                //if (!File.Exists(fullPathToEmbeddedHCToolBinary) && Config.HolochainConductorMode == HolochainConductorModeEnum.UseEmbedded && Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                //    throw new FileNotFoundException($"When HolochainConductorMode is set to 'UseEmbedded' and HolochainConductorToUse is set to 'HcDevTool', you must ensure the hc.exe is found here: {fullPathToEmbeddedHCToolBinary}.");

                if (!Directory.Exists(Config.FullPathToRootHappFolder))
                    throw new DirectoryNotFoundException($"The path for Config.FullPathToRootHappFolder ({Config.FullPathToRootHappFolder}) was not found.");

                if (!Directory.Exists(Config.FullPathToCompiledHappFolder))
                    throw new DirectoryNotFoundException($"The path for Config.FullPathToCompiledHappFolder ({Config.FullPathToCompiledHappFolder}) was not found.");


                if (Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                {
                    switch (Config.HolochainConductorMode)
                    {
                        case HolochainConductorModeEnum.UseExternal:
                            _conductorProcess.StartInfo.FileName = Config.FullPathToExternalHCToolBinary;
                            break;

                        case HolochainConductorModeEnum.UseEmbedded:
                            {
                                //throw new InvalidOperationException("You must install the Embedded version if you wish to use HolochainConductorMode.UseEmbedded.");

                                //_conductorProcess.StartInfo.FileName = fullPathToEmbeddedHCToolBinary;

                                string hcPath = Path.Combine(Directory.GetCurrentDirectory(), "hc.exe");

                                if (!File.Exists(hcPath))
                                {
                                    using (FileStream fsDst = new FileStream(hcPath, FileMode.CreateNew, FileAccess.Write))
                                    {
                                        byte[] bytes = Resources.hc;
                                        fsDst.Write(bytes, 0, bytes.Length);
                                    }
                                }

                                _conductorProcess.StartInfo.FileName = hcPath;
                            }
                            break;

                        case HolochainConductorModeEnum.UseSystemGlobal:
                            _conductorProcess.StartInfo.FileName = "hc.exe";
                            break;
                    }
                }
                else
                {
                    switch (Config.HolochainConductorMode)
                    {
                        case HolochainConductorModeEnum.UseExternal:
                            _conductorProcess.StartInfo.FileName = Config.FullPathToExternalHolochainConductorBinary;
                            break;

                        case HolochainConductorModeEnum.UseEmbedded:
                            {
                                //throw new InvalidOperationException("You must install the Embedded version if you wish to use HolochainConductorMode.UseEmbedded.");

                                //_conductorProcess.StartInfo.FileName = fullPathToEmbeddedHolochainConductorBinary;

                                string holochainPath = Path.Combine(Directory.GetCurrentDirectory(), "holochain.exe");

                                if (!File.Exists(holochainPath))
                                {
                                    using (FileStream fsDst = new FileStream(holochainPath, FileMode.CreateNew, FileAccess.Write))
                                    {
                                        byte[] bytes = Resources.holochain;
                                        fsDst.Write(bytes, 0, bytes.Length);
                                    }
                                }

                                _conductorProcess.StartInfo.FileName = holochainPath;
                            }
                            break;

                        case HolochainConductorModeEnum.UseSystemGlobal:
                            _conductorProcess.StartInfo.FileName = "holochain.exe";
                            break;
                    }
                }

                //Make sure the condctor is not already running
                if (!Config.OnlyAllowOneHolochainConductorToRunAtATime || (Config.OnlyAllowOneHolochainConductorToRunAtATime && !Process.GetProcesses().Any(x => x.ProcessName == _conductorProcess.StartInfo.FileName)))
                {
                    Logger.Log("Starting Holochain Conductor...", LogType.Info, true);
                    _conductorProcess.StartInfo.WorkingDirectory = Config.FullPathToRootHappFolder;

                    if (Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    {
                        //_conductorProcess.StartInfo.Arguments = "sandbox run 0";
                        _conductorProcess.StartInfo.Arguments = $"hc sandbox generate {Config.FullPathToCompiledHappFolder}";
                    }

                    _conductorProcess.StartInfo.UseShellExecute = true;
                    _conductorProcess.StartInfo.RedirectStandardOutput = false;

                    if (Config.ShowHolochainConductorWindow)
                    {
                        _conductorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        _conductorProcess.StartInfo.CreateNoWindow = false;
                    }
                    else
                    {
                        _conductorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        _conductorProcess.StartInfo.CreateNoWindow = true;
                    }

                    _conductorProcess.Start();

                    if (Config.HolochainConductorToUse == HolochainConductorEnum.HcDevTool)
                    {
                        await Task.Delay(3000);
                        _conductorProcess.Close();
                        _conductorProcess.StartInfo.Arguments = "sandbox run 0";
                        _conductorProcess.Start();
                    }

                    await Task.Delay(Config.SecondsToWaitForHolochainConductorToStart * 1000); // Give the conductor 7 (default) seconds to start up...
                }
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.StartConductor method.", ex);
            }
        }

        public async Task<AgentPubKeyDnaHash> GetAgentPubKeyAndDnaHashFromSandbox(bool updateConfig = true)
        {
            try
            {
                Logger.Log("Attempting To Retreive AgentPubKey & DnaHash From hc sandbox...", LogType.Info, true);

                if (string.IsNullOrEmpty(Config.FullPathToExternalHCToolBinary))
                    Config.FullPathToExternalHCToolBinary = string.Concat(Directory.GetCurrentDirectory(), "\\HolochainBinaries\\hc.exe"); //default to the current path

                Process pProcess = new Process();
                pProcess.StartInfo.WorkingDirectory = Config.FullPathToRootHappFolder;
                pProcess.StartInfo.FileName = "hc";
                //pProcess.StartInfo.FileName = Config.FullPathToExternalHCToolBinary; //TODO: Need to get this working later (think currently has a version conflict with keylairstone? But not urgent because AgentPubKey & DnaHash are retreived from Conductor anyway.
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

                Logger.Log("AgentPubKey & DnaHash successfully retreived from hc sandbox.", LogType.Info, false);

                if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                    OnReadyForZomeCalls?.Invoke(this, new ReadyForZomeCallsEventArgs(EndPoint, dnaHash, agentPubKey));

                return new AgentPubKeyDnaHash() { DnaHash = dnaHash, AgentPubKey = agentPubKey };
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.GetAgentPubKeyAndDnaHashFromSandbox method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        public async Task GetAgentPubKeyAndDnaHashFromConductor(bool updateConfig = true)
        {
            try
            {
                _updateDnaHashAndAgentPubKey = updateConfig;
                Logger.Log("Attempting To Retreive AgentPubKey & DnaHash from Holochain Conductor...", LogType.Info, true);

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
                HandleError("Error occured in HoloNETClient.GetAgentPubKeyAndDnaHashFromConductor method getting DnaHash & AgentPubKey from hApp.", ex);
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
                {
                    Logger.Log("Sending HoloNET Request to Holochain Conductor...", LogType.Info, true);
                    await WebSocket.SendRawDataAsync(MessagePackSerializer.Serialize(request)); //This is the fastest and most popular .NET MessagePack Serializer.
                    Logger.Log("HoloNET Request Successfully Sent To Holochain Conductor.", LogType.Info, false);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.SendHoloNETRequest method.", ex);
            }
        }

        public async Task Disconnect()
        {
            await WebSocket.Disconnect();
        }

        public async Task CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false)
        {
            _currentId++;
            await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData);
        }

        public async Task CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false)
        {
            await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData);
        }

        public async Task CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false)
        {
            _currentId++;
            await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData);
        }

        public async Task CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false)
        {
            _cacheZomeReturnDataLookup[id] = cachReturnData;

            try
            {
                if (WebSocket.State == WebSocketState.Closed || WebSocket.State == WebSocketState.None)
                    await Connect();

                Logger.Log($"Calling Zome Function {function} on Zome {zome} with Id {id} On Holochain Conductor...", LogType.Info, true);

                if (cachReturnData)
                {
                    if (_zomeReturnDataLookup.ContainsKey(id))
                    {
                        Logger.Log("Caching Enabled so returning data from cache...", LogType.Info);
                        Logger.Log(string.Concat("Id: ", _zomeReturnDataLookup[id].Id, ", Zome: ", _zomeReturnDataLookup[id].Zome, ", Zome Function: ", _zomeReturnDataLookup[id].ZomeFunction, ", Is Zome Call Successful: ", _zomeReturnDataLookup[id].IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", _zomeReturnDataLookup[id].RawZomeReturnData, ", Zome Return Data: ", _zomeReturnDataLookup[id].ZomeReturnData, ", JSON Raw Data: ", _zomeReturnDataLookup[id].RawJSONData), LogType.Info);

                        if (callback != null)
                            callback.DynamicInvoke(this, _zomeReturnDataLookup[id]);

                        OnZomeFunctionCallBack?.Invoke(this, _zomeReturnDataLookup[id]);
                        return;
                    }
                }

                if (matchIdToInstanceZomeFuncInCallback)
                {
                    _zomeLookup[id] = zome;
                    _funcLookup[id] = function;
                }

                if (callback != null)
                    _callbackLookup[id] = callback;

                if (paramsObject.GetType() == typeof(string))
                    paramsObject = ConvertHoloHashToBytes(paramsObject.ToString());

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
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.CallZomeFunctionAsync method.", ex);
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

        public async Task ShutDownAllConductors()
        {
            try
            {
                Logger.Log("Shutting Down All Holochain Conductors...", LogType.Info, true);

                foreach (Process process in Process.GetProcessesByName("hc"))
                {
                    if (Config.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                }

                //conductorInfo = new FileInfo(Config.FullPathToExternalHolochainConductorBinary);
                //parts = conductorInfo.Name.Split('.');

                foreach (Process process in Process.GetProcessesByName("holochain"))
                {
                    if (Config.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                }

                foreach (Process process in Process.GetProcessesByName("rustc"))
                {
                    if (Config.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                }

                Logger.Log("All Holochain Conductors Successfully Shutdown.", LogType.Info);
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.ShutDownAllConductors method", ex);
            }
        }

        private async Task ShutDownConductorsInternal()
        {
            try
            {
                // Close any conductors down if necessary.
                if (Config.AutoShutdownHolochainConductor)
                {
                    if (Config.ShutDownALLHolochainConductors)
                        await ShutDownAllConductors();

                    else if (_conductorProcess != null)
                    {
                        Logger.Log("Shutting Down Holochain Conductor...", LogType.Info, true);

                        if (Config.ShowHolochainConductorWindow)
                            _conductorProcess.CloseMainWindow();

                        _conductorProcess.Kill();
                        _conductorProcess.Close();

                        // _conductorProcess.WaitForExit();
                        _conductorProcess.Dispose();

                        Logger.Log("Holochain Conductor Successfully Shutdown.", LogType.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.ShutDownConductorsInternal method.", ex);
            }
        }
        private void Init(string holochainConductorURI)
        {
            try
            {
                WebSocket = new WebSocket.WebSocket(holochainConductorURI, Logger.Loggers);

                //TODO: Impplemnt IDispoasable to unsubscribe event handlers to prevent memory leaks... 
                WebSocket.OnConnected += WebSocket_OnConnected;
                WebSocket.OnDataReceived += WebSocket_OnDataReceived;
                WebSocket.OnDisconnected += WebSocket_OnDisconnected;
                WebSocket.OnError += WebSocket_OnError;
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.Init method.", ex);
            }
        }

        private void WebSocket_OnError(object sender, WebSocketErrorEventArgs e)
        {
            HandleError(e.Reason, e.ErrorDetails);
        }

        private void WebSocket_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            try
            {
                ShutDownConductorsInternal();
                OnDisconnected?.Invoke(this, new DisconnectedEventArgs { EndPoint = e.EndPoint, Reason = e.Reason });
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnDisconnected method.", ex);
            }
        }

        private void WebSocket_OnDataReceived(object sender, WebSocket.DataReceivedEventArgs e)
        {
            ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs = null;

            try
            {
                Logger.Log("DATA RECEIVED", LogType.Info);
                StringBuilder sb = new StringBuilder();
                sb.Append(Encoding.UTF8.GetString(e.RawBinaryData, 0, e.RawBinaryData.Length));
                string rawData = sb.ToString();
                Logger.Log("Raw Data Received: " + rawData, LogType.Debug);

                var options = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
                HoloNETResponse response = MessagePackSerializer.Deserialize<HoloNETResponse>(e.RawBinaryData, options);

                Logger.Log("RSM Response", LogType.Info);
                Logger.Log($"Id: {response.id}", LogType.Info);
                Logger.Log($"Type: {response.type}", LogType.Info);

                OnDataReceived?.Invoke(this, new HoloNETDataReceivedEventArgs(response.id.ToString(), EndPoint, true, e.RawBinaryData, e.RawJSONData, e.WebSocketResult, false));

                if (rawData.Substring(32, 8) == "app_info")
                {
                    Logger.Log("APP INFO RESPONSE DATA DETECTED\n", LogType.Info);
                    HolonNETAppInfoResponse appInfoResponse = MessagePackSerializer.Deserialize<HolonNETAppInfoResponse>(response.data, options);

                    string dnHash = ConvertHoloHashToString(appInfoResponse.data.cell_data[0].cell_id[0]);
                    string agentPubKey = ConvertHoloHashToString(appInfoResponse.data.cell_data[0].cell_id[1]);

                    if (_updateDnaHashAndAgentPubKey)
                    {
                        Config.AgentPubKey = agentPubKey;
                        Config.DnaHash = dnHash;
                    }

                    AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs(response.id.ToString(), EndPoint, true, e.RawBinaryData, e.RawJSONData, dnHash, agentPubKey, appInfoResponse.data.installed_app_id, appInfoResponse, e.WebSocketResult);
                    Logger.Log(string.Concat("Id: ", args.Id, ", Is Call Successful: ", args.IsCallSuccessful ? "True" : "False", ", AgentPubKey: ", args.AgentPubKey, ", DnaHash: ", args.DnaHash, ", Installed App Id: ", args.InstalledAppId, ", Raw Binary Data: ", e.RawBinaryData, ", Raw JSON Data: ", args.RawJSONData, "\n"), LogType.Info);
                    OnAppInfoCallBack?.Invoke(this, args);

                    //If either the AgentPubKey or DnaHash is empty then attempt to get from the sandbox cmd.
                    if (string.IsNullOrEmpty(Config.AgentPubKey) || string.IsNullOrEmpty(Config.DnaHash))
                        GetAgentPubKeyAndDnaHashFromSandbox();
                    else
                        OnReadyForZomeCalls?.Invoke(this, new ReadyForZomeCallsEventArgs(EndPoint, dnHash, agentPubKey));
                }
                else
                {
                    Logger.Log("ZOME RESPONSE DATA DETECTED\n", LogType.Info);
                    HolonNETAppResponse appResponse = MessagePackSerializer.Deserialize<HolonNETAppResponse>(response.data, options);
                    string id = response.id.ToString();

                    try
                    {
                        Dictionary<object, object> rawAppResponseData = MessagePackSerializer.Deserialize<Dictionary<object, object>>(appResponse.data, options);
                        Dictionary<string, object> appResponseData = new Dictionary<string, object>();
                        Dictionary<string, string> keyValuePair = new Dictionary<string, string>();
                        string keyValuePairAsString = "";
                        EntryData entryData = null;

                        (appResponseData, keyValuePair, keyValuePairAsString, entryData) = DecodeZomeReturnData(rawAppResponseData, appResponseData, keyValuePair, keyValuePairAsString);
                        //appResponseData["Entry"] = entryData;

                        Logger.Log($"Decoded Data:\n{keyValuePairAsString}", LogType.Info);
                        zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs(id, e.EndPoint, GetItemFromCache(id, _zomeLookup), GetItemFromCache(id, _funcLookup), true, rawData, rawAppResponseData, appResponseData, "", keyValuePair, keyValuePairAsString, entryData, e.RawBinaryData, e.RawJSONData, e.WebSocketResult);
                    }
                    catch (Exception ex)
                    {
                        object rawAppResponseData = MessagePackSerializer.Deserialize<object>(appResponse.data, options);
                        string hash = ConvertHoloHashToString((byte[])rawAppResponseData);

                        Logger.Log($"Decoded Data:\nHoloHash: {hash}", LogType.Info);
                        zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs(id, e.EndPoint, GetItemFromCache(id, _zomeLookup), GetItemFromCache(id, _funcLookup), true, rawData, null, null, hash, null, null, null, e.RawBinaryData, e.RawJSONData, e.WebSocketResult);
                    }

                    Logger.Log(string.Concat("Id: ", zomeFunctionCallBackArgs.Id, ", Zome: ", zomeFunctionCallBackArgs.Zome, ", Zome Function: ", zomeFunctionCallBackArgs.ZomeFunction, ", Is Zome Call Successful: ", zomeFunctionCallBackArgs.IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", zomeFunctionCallBackArgs.RawZomeReturnData, ", Zome Return Data: ", zomeFunctionCallBackArgs.ZomeReturnData, ", Zome Return Hash: ", zomeFunctionCallBackArgs.ZomeReturnHash, ", Raw Binary Data: ", e.RawBinaryData, ", Raw JSON Data: ", zomeFunctionCallBackArgs.RawJSONData), LogType.Info);

                    if (_callbackLookup.ContainsKey(id) && _callbackLookup[id] != null)
                        _callbackLookup[id].DynamicInvoke(this, zomeFunctionCallBackArgs);

                    OnZomeFunctionCallBack?.Invoke(this, zomeFunctionCallBackArgs);

                    // If the zome call requested for this to be cached then stick it in cache.
                    if (_cacheZomeReturnDataLookup[id])
                        _zomeReturnDataLookup[id] = zomeFunctionCallBackArgs;

                    _zomeLookup.Remove(id);
                    _funcLookup.Remove(id);
                    _callbackLookup.Remove(id);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnDataReceived method.", ex);
            }
        }

        private (Dictionary<string, object>, Dictionary<string, string> keyValuePair, string keyValuePairAsString, EntryData entry) DecodeZomeReturnData(Dictionary<object, object> rawAppResponseData, Dictionary<string, object> appResponseData, Dictionary<string, string> keyValuePair, string keyValuePairAsString)
        {
            string value = "";
            var options = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);
            EntryData entryData = null;

            foreach (string key in rawAppResponseData.Keys)
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

                            entryData = new EntryData() { Bytes = bytes, BytesString = byteString, Entry = decodedEntry };
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
                        (tempDict, keyValuePair, keyValuePairAsString, entryData) = DecodeZomeReturnData(dict, tempDict, keyValuePair, keyValuePairAsString);
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
                }
            }

            return (appResponseData, keyValuePair, keyValuePairAsString, entryData);
        }

        private void WebSocket_OnConnected(object sender, ConnectedEventArgs e)
        {
            try
            {
                OnConnected?.Invoke(this, new ConnectedEventArgs { EndPoint = e.EndPoint });

                //If the AgentPubKey & DnaHash have already been retreived from the hc sandbox command then raise the OnReadyForZomeCalls event.
                if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                    OnReadyForZomeCalls?.Invoke(this, new ReadyForZomeCallsEventArgs(EndPoint, Config.DnaHash, Config.AgentPubKey));

                //Otherwise, if the getAgentPubKeyAndDnaHashFromConductor param was set to true when calling the Connect method, retreive them now...
                else if (_getAgentPubKeyAndDnaHashFromConductor)
                    GetAgentPubKeyAndDnaHashFromConductor();
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnDataReceived method.", ex);
            }
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
    }
}