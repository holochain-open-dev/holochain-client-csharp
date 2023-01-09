using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MessagePack;
using NextGenSoftware.WebSocket;
using NextGenSoftware.Logging;
using NextGenSoftware.Holochain.HoloNET.Client.Properties;
using NextGenSoftware.Utilities;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class HoloNETClient //: IDisposable //, IAsyncDisposable
    {
        private bool _shuttingDownHoloNET = false;
        private bool _getAgentPubKeyAndDnaHashFromConductor;
        private bool _automaticallyAttemptToGetFromSandboxIfConductorFails;
        private bool _updateDnaHashAndAgentPubKey = true;
        private Dictionary<string, string> _zomeLookup = new Dictionary<string, string>();
        private Dictionary<string, string> _funcLookup = new Dictionary<string, string>();
        private Dictionary<string, Type> _entryDataObjectTypeLookup = new Dictionary<string, Type>();
        private Dictionary<string, dynamic> _entryDataObjectLookup = new Dictionary<string, dynamic>();
        private Dictionary<string, ZomeFunctionCallBack> _callbackLookup = new Dictionary<string, ZomeFunctionCallBack>();
        private Dictionary<string, ZomeFunctionCallBackEventArgs> _zomeReturnDataLookup = new Dictionary<string, ZomeFunctionCallBackEventArgs>();
        private Dictionary<string, bool> _cacheZomeReturnDataLookup = new Dictionary<string, bool>();
        private Dictionary<string, TaskCompletionSource<ZomeFunctionCallBackEventArgs>> _taskCompletionZomeCallBack = new Dictionary<string, TaskCompletionSource<ZomeFunctionCallBackEventArgs>>();
        private TaskCompletionSource<AgentPubKeyDnaHash> _taskCompletionAgentPubKeyAndDnaHashRetreived = new TaskCompletionSource<AgentPubKeyDnaHash>();
        //private TaskCompletionSource<AgentPubKeyDnaHash> _taskCompletionAgentPubKeyAndDnaHashRetreivedFromConductor = new TaskCompletionSource<AgentPubKeyDnaHash>();
        private TaskCompletionSource<ReadyForZomeCallsEventArgs> _taskCompletionReadyForZomeCalls = new TaskCompletionSource<ReadyForZomeCallsEventArgs>();
        private TaskCompletionSource<DisconnectedEventArgs> _taskCompletionDisconnected = new TaskCompletionSource<DisconnectedEventArgs>();
        private TaskCompletionSource<HoloNETShutdownEventArgs> _taskCompletionHoloNETShutdown = new TaskCompletionSource<HoloNETShutdownEventArgs>();
        private int _currentId = 0;
        private HoloNETConfig _config = null;
        private Process _conductorProcess = null;
        private ShutdownHolochainConductorsMode _shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings;
        private MessagePackSerializerOptions messagePackSerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        //Events
        public delegate void Connected(object sender, ConnectedEventArgs e);
        public event Connected OnConnected;

        public delegate void Disconnected(object sender, DisconnectedEventArgs e);
        public event Disconnected OnDisconnected;

        public delegate void HolochainConductorsShutdownComplete(object sender, HolochainConductorsShutdownEventArgs e);
        public event HolochainConductorsShutdownComplete OnHolochainConductorsShutdownComplete;

        public delegate void HoloNETShutdownComplete(object sender, HoloNETShutdownEventArgs e);
        public event HoloNETShutdownComplete OnHoloNETShutdownComplete;

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
            set
            {
                _config = value;
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

        public bool IsReadyForZomesCalls { get; set; }
        public bool RetreivingAgentPubKeyAndDnaHash { get; private set; }

        public async Task<ReadyForZomeCallsEventArgs> WaitTillReadyForZomeCallsAsync()
        {
            return await _taskCompletionReadyForZomeCalls.Task;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectedCallBackMode"></param>
        /// <param name="retreiveAgentPubKeyAndDnaHashMode"></param>
        /// <param name="getAgentPubKeyAndDnaHashFromConductor"></param>
        /// <param name="getAgentPubKeyAndDnaHashFromSandbox"></param>
        /// <param name="automaticallyAttemptToGetFromConductorIfSandBoxFails"></param>
        /// <param name="automaticallyAttemptToGetFromSandBoxIfConductorFails"></param>
        /// <param name="updateConfig"></param>
        /// <returns></returns>
        public async Task ConnectAsync(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToGetFromConductorIfSandBoxFails = true, bool automaticallyAttemptToGetFromSandBoxIfConductorFails = true, bool updateConfig = true)
        {
            try
            {
                _getAgentPubKeyAndDnaHashFromConductor = getAgentPubKeyAndDnaHashFromConductor;

                if (Logger.Loggers.Count == 0)
                    throw new HoloNETException("ERROR: No Logger Has Been Specified! Please set a Logger with the Logger.Loggers Property.");

                if (WebSocket.State != WebSocketState.Connecting && WebSocket.State != WebSocketState.Open && WebSocket.State != WebSocketState.Aborted)
                {
                    if (Config.AutoStartHolochainConductor)
                        await StartConductorAsync();

                    if (connectedCallBackMode == ConnectedCallBackMode.WaitForHolochainConductorToConnect)
                        await WebSocket.Connect();
                    else
                    {
                        WebSocket.Connect();
                        await RetreiveAgentPubKeyAndDnaHashAsync(retreiveAgentPubKeyAndDnaHashMode, getAgentPubKeyAndDnaHashFromConductor, getAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToGetFromConductorIfSandBoxFails, automaticallyAttemptToGetFromSandBoxIfConductorFails, updateConfig);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(string.Concat("Error occured in HoloNETClient.Connect method connecting to ", WebSocket.EndPoint), e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getAgentPubKeyAndDnaHashFromConductor"></param>
        /// <param name="getAgentPubKeyAndDnaHashFromSandbox"></param>
        /// <param name="automaticallyAttemptToGetFromConductorIfSandBoxFails"></param>
        /// <param name="automaticallyAttemptToGetFromSandBoxIfConductorFails"></param>
        /// <param name="updateConfig"></param>
        public void Connect(bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToGetFromConductorIfSandBoxFails = true, bool automaticallyAttemptToGetFromSandBoxIfConductorFails = true, bool updateConfig = true)
        {
            ConnectAsync(ConnectedCallBackMode.UseCallBackEvents, RetreiveAgentPubKeyAndDnaHashMode.UseCallBackEvents, getAgentPubKeyAndDnaHashFromConductor, getAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToGetFromConductorIfSandBoxFails, automaticallyAttemptToGetFromSandBoxIfConductorFails, updateConfig);
        }

        public async Task StartConductorAsync()
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

        public void StartConductor()
        {
            StartConductorAsync();
        }

        public AgentPubKeyDnaHash RetreiveAgentPubKeyAndDnaHash(bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = false, bool automaticallyAttemptToGetFromConductorIfSandBoxFails = true, bool automaticallyAttemptToGetFromSandBoxIfConductorFails = true, bool updateConfig = true)
        {
            return RetreiveAgentPubKeyAndDnaHashAsync(RetreiveAgentPubKeyAndDnaHashMode.UseCallBackEvents, getAgentPubKeyAndDnaHashFromConductor, getAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToGetFromConductorIfSandBoxFails, automaticallyAttemptToGetFromSandBoxIfConductorFails, updateConfig).Result;
        }

        public async Task<AgentPubKeyDnaHash> RetreiveAgentPubKeyAndDnaHashAsync(RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToGetFromConductorIfSandBoxFails = true, bool automaticallyAttemptToGetFromSandBoxIfConductorFails = true, bool updateConfig = true)
        {
            if (RetreivingAgentPubKeyAndDnaHash)
                return null;

            RetreivingAgentPubKeyAndDnaHash = true;

            //Try to first get from the conductor.
            if (getAgentPubKeyAndDnaHashFromConductor)
                return await RetreiveAgentPubKeyAndDnaHashFromConductorAsync(retreiveAgentPubKeyAndDnaHashMode, updateConfig, automaticallyAttemptToGetFromSandBoxIfConductorFails);

            else if (getAgentPubKeyAndDnaHashFromSandbox)
            {
                AgentPubKeyDnaHash agentPubKeyDnaHash = await RetreiveAgentPubKeyAndDnaHashFromSandboxAsync(updateConfig, automaticallyAttemptToGetFromConductorIfSandBoxFails);

                if (agentPubKeyDnaHash != null)
                    RetreivingAgentPubKeyAndDnaHash = false;
            }

            return new AgentPubKeyDnaHash() { AgentPubKey = Config.AgentPubKey, DnaHash = Config.DnaHash };
        }

        public async Task<AgentPubKeyDnaHash> RetreiveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateConfig = true, bool automaticallyAttemptToGetFromConductorIfSandBoxFails = true)
        {
            try
            {
                if (RetreivingAgentPubKeyAndDnaHash)
                    return null;

                RetreivingAgentPubKeyAndDnaHash = true;
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

                if (!string.IsNullOrEmpty(dnaHash) && !string.IsNullOrEmpty(agentPubKey))
                {
                    if (updateConfig)
                    {
                        Config.AgentPubKey = agentPubKey;
                        Config.DnaHash = dnaHash;
                    }

                    Logger.Log("AgentPubKey & DnaHash successfully retreived from hc sandbox.", LogType.Info, false);

                    if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                        SetReadyForZomeCalls();

                    return new AgentPubKeyDnaHash() { DnaHash = dnaHash, AgentPubKey = agentPubKey };
                }
                else if (automaticallyAttemptToGetFromConductorIfSandBoxFails)
                    await RetreiveAgentPubKeyAndDnaHashFromConductorAsync();
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.GetAgentPubKeyAndDnaHashFromSandbox method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        public AgentPubKeyDnaHash RetreiveAgentPubKeyAndDnaHashFromSandbox(bool updateConfig = true, bool automaticallyAttemptToGetFromConductorIfSandBoxFails = true)
        {
            return RetreiveAgentPubKeyAndDnaHashFromSandboxAsync(updateConfig, automaticallyAttemptToGetFromConductorIfSandBoxFails).Result;
        }

        public async Task<AgentPubKeyDnaHash> RetreiveAgentPubKeyAndDnaHashFromConductorAsync(RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool updateConfig = true, bool automaticallyAttemptToGetFromSandboxIfConductorFails = true)
        {
            try
            {
                if (RetreivingAgentPubKeyAndDnaHash)
                    return null;

                _automaticallyAttemptToGetFromSandboxIfConductorFails = automaticallyAttemptToGetFromSandboxIfConductorFails;
                RetreivingAgentPubKeyAndDnaHash = true;
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

                await SendHoloNETRequestAsync(_currentId.ToString(), holoNETData);
                _currentId++;

                if (retreiveAgentPubKeyAndDnaHashMode == RetreiveAgentPubKeyAndDnaHashMode.Wait)
                    return await _taskCompletionAgentPubKeyAndDnaHashRetreived.Task;
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.GetAgentPubKeyAndDnaHashFromConductor method getting DnaHash & AgentPubKey from hApp.", ex);
            }

            return null;
        }

        public AgentPubKeyDnaHash RetreiveAgentPubKeyAndDnaHashFromConductor(bool updateConfig = true, bool automaticallyAttemptToGetFromSandboxIfConductorFails = true)
        {
            return RetreiveAgentPubKeyAndDnaHashFromConductorAsync(RetreiveAgentPubKeyAndDnaHashMode.UseCallBackEvents, updateConfig, automaticallyAttemptToGetFromSandboxIfConductorFails).Result;
        }

        public async Task SendHoloNETRequestAsync(string id, HoloNETData holoNETData)
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

        public void SendHoloNETRequest(string id, HoloNETData holoNETData)
        {
            SendHoloNETRequestAsync(id, holoNETData);
        }

        public async Task DisconnectAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            _shutdownHolochainConductorsMode = shutdownHolochainConductorsMode;
            await WebSocket.Disconnect();

            if (disconnectedCallBackMode == DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect)
                await _taskCompletionDisconnected.Task;
        }

        public void Disconnect(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            DisconnectAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, false, ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return await CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return await CallZomeFunctionAsync(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return await CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return await CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            try
            {
                _taskCompletionZomeCallBack[id] = new TaskCompletionSource<ZomeFunctionCallBackEventArgs>();

                if (WebSocket.State == WebSocketState.Closed || WebSocket.State == WebSocketState.None)
                    await ConnectAsync();

                await _taskCompletionReadyForZomeCalls.Task;

                if (string.IsNullOrEmpty(Config.DnaHash))
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "The DnaHash cannot be empty, please either set manually in the Config.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                    //throw new InvalidOperationException("The DnaHash cannot be empty, please either set manually in the Config.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call.");

                if (string.IsNullOrEmpty(Config.AgentPubKey))
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = "The AgentPubKey cannot be empty, please either set manually in the Config.DnaHash property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call." };
                    //throw new InvalidOperationException("The AgentPubKey cannot be empty, please either set manually in the Config.AgentPubKey property or wait till the ReadyForZomeCalls event is fired before attempting to make a zome call.");

                Logger.Log($"Calling Zome Function {function} on Zome {zome} with Id {id} On Holochain Conductor...", LogType.Info, true);

                _cacheZomeReturnDataLookup[id] = cachReturnData;

                if (cachReturnData)
                {
                    if (_zomeReturnDataLookup.ContainsKey(id))
                    {
                        Logger.Log("Caching Enabled so returning data from cache...", LogType.Info);
                        Logger.Log(string.Concat("Id: ", _zomeReturnDataLookup[id].Id, ", Zome: ", _zomeReturnDataLookup[id].Zome, ", Zome Function: ", _zomeReturnDataLookup[id].ZomeFunction, ", Is Zome Call Successful: ", _zomeReturnDataLookup[id].IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", _zomeReturnDataLookup[id].RawZomeReturnData, ", Zome Return Data: ", _zomeReturnDataLookup[id].ZomeReturnData, ", JSON Raw Data: ", _zomeReturnDataLookup[id].RawJSONData), LogType.Info);

                        if (callback != null)
                            callback.DynamicInvoke(this, _zomeReturnDataLookup[id]);

                        OnZomeFunctionCallBack?.Invoke(this, _zomeReturnDataLookup[id]);
                        _taskCompletionZomeCallBack[id].SetResult(_zomeReturnDataLookup[id]);

                        Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                        _taskCompletionZomeCallBack.Remove(id);

                        return await returnValue;
                    }
                }

                if (matchIdToInstanceZomeFuncInCallback)
                {
                    _zomeLookup[id] = zome;
                    _funcLookup[id] = function;
                }

                if (callback != null)
                    _callbackLookup[id] = callback;

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

                await SendHoloNETRequestAsync(id, holoNETData);

                if (zomeResultCallBackMode == ZomeResultCallBackMode.WaitForHolochainConductorResponse)
                {
                    Task<ZomeFunctionCallBackEventArgs> returnValue = _taskCompletionZomeCallBack[id].Task;
                    //_taskCompletionZomeCallBack.Remove(id);
                    return await returnValue;
                }
                else
                    return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, Message = "ZomeResultCallBackMode is set to UseCallBackEvents so please wait for OnZomeFunctionCallBack event for zome result." };
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.CallZomeFunctionAsync method.", ex);
                return new ZomeFunctionCallBackEventArgs() { EndPoint = EndPoint, Id = id, Zome = zome, ZomeFunction = function, IsError = true, Message = $"Error occured in HoloNETClient.CallZomeFunctionAsync method. Details: {ex}", Excception = ex };
            }
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, false, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, null, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, paramsObject, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunctionAsync(_currentId.ToString(), zome, function, paramsObject, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunction(id, zome, function, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunction(id, zome, function, null, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, false, ZomeResultCallBackMode.WaitForHolochainConductorResponse);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, false, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunctionAsync(_currentId.ToString(), zome, function, callback, paramsObject, true, false, entryDataObjectReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _currentId++;
            return CallZomeFunction(_currentId.ToString(), zome, function, callback, paramsObject, true, cachReturnData, entryDataObjectTypeReturnedFromZome, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectLookup[id] = entryDataObjectReturnedFromZome;
            return CallZomeFunction(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            _entryDataObjectTypeLookup[id] = entryDataObjectTypeReturnedFromZome;
            return CallZomeFunction(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode);
        }

        public ZomeFunctionCallBackEventArgs CallZomeFunction(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
        {
            return CallZomeFunctionAsync(id, zome, function, callback, paramsObject, matchIdToInstanceZomeFuncInCallback, cachReturnData, zomeResultCallBackMode).Result;
        }

        public void ClearCache()
        {
            _zomeReturnDataLookup.Clear();
            _cacheZomeReturnDataLookup.Clear();
        }

        public byte[] ConvertHoloHashToBytes(string hash)
        {
            return Convert.FromBase64String(hash.Replace('-', '+').Replace('_', '/').Substring(1, hash.Length - 1)); //also remove the u prefix.

            //Span<byte> output = null;
            //int bytesWritten = 0;

            //if (Convert.TryFromBase64String(hash.Replace('-', '+').Replace('_', '/').Substring(1, hash.Length - 1), output, out bytesWritten)) //also remove the u prefix.
            //    return output.ToArray();
            //else
            //    return null;
        }

        public string ConvertHoloHashToString(byte[] bytes)
        {
            return string.Concat("u", Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_"));
        }

        public async Task<dynamic> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs)
        {
            return await MapEntryDataObjectAsync(Activator.CreateInstance(entryDataObjectType), keyValuePairs);
        }

        public dynamic MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs)
        {
            return MapEntryDataObjectAsync(entryDataObjectType, keyValuePairs).Result;
        }

        public async Task<dynamic> MapEntryDataObjectAsync(dynamic entryDataObject, Dictionary<string, string> keyValuePairs)
        {
            try
            {
                if (keyValuePairs != null && entryDataObject != null)
                {
                    PropertyInfo[] props = entryDataObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

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

        public dynamic MapEntryDataObject(dynamic entryDataObject, Dictionary<string, string> keyValuePairs)
        {
            return MapEntryDataObjectAsync(entryDataObject, keyValuePairs).Result;
        }

        private async Task<HolochainConductorsShutdownEventArgs> ShutDownAllHolochainConductorsAsync()
        {
            HolochainConductorsShutdownEventArgs result = new HolochainConductorsShutdownEventArgs();
            result.AgentPubKey = Config.AgentPubKey;
            result.DnaHash = Config.DnaHash;
            result.EndPoint = EndPoint;

            try
            {
                Logger.Log("Shutting Down All Holochain Conductors...", LogType.Info, false);

                foreach (Process process in Process.GetProcessesByName("hc"))
                {
                    if (Config.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                    result.NumberOfHcExeInstancesShutdown++;
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
                    result.NumberOfHolochainExeInstancesShutdown++;
                }

                foreach (Process process in Process.GetProcessesByName("rustc"))
                {
                    if (Config.ShowHolochainConductorWindow)
                        process.CloseMainWindow();

                    process.Kill();
                    process.Close();

                    //process.WaitForExit();
                    process.Dispose();
                    result.NumberOfRustcExeInstancesShutdown++;
                }

                Logger.Log("All Holochain Conductors Successfully Shutdown.", LogType.Info);
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.ShutDownAllConductors method", ex);
            }

            return result;
        }

        ///// <summary>
        ///// Shutsdown all running Holochain Conductors.
        ///// </summary>
        //public HolochainConductorsShutdownEventArgs ShutDownAllHolochainConductors()
        //{
        //    return ShutDownAllHolochainConductorsAsync().Result;
        //}

        //public void Dispose()
        //{
        //    Shutdown();
        //}

        //public ValueTask DisposeAsync()
        //{
        //    if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
        //        await DisconnectAsync();

        //    Logger.Loggers.Clear();
        //}

        /// <summary>
        /// Will shutdown HoloNET and its current connetion to the Holochain Conductor.
        /// You can specify if HoloNET should wait until it has finished disconnecting and shutting down the conductors before returning to the caller or whether it should return immediately and then use the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events to notify the caller.
        /// It will also shutdown the current running Holochain Conductor or all conductors depending on the config/params passed in.
        /// </summary>
        /// <param name="disconnectedCallBackMode"></param>
        /// <param name="shutdownHolochainConductorsMode"></param>
        /// <returns></returns>
        public async Task<HoloNETShutdownEventArgs> ShutdownHoloNETAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            _shuttingDownHoloNET = true;

            if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
                await DisconnectAsync(disconnectedCallBackMode, shutdownHolochainConductorsMode);

            if (disconnectedCallBackMode == DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect)
                return await _taskCompletionHoloNETShutdown.Task;
            else
                return new HoloNETShutdownEventArgs(EndPoint, Config.DnaHash, Config.AgentPubKey, null);
        }

        /// <summary>
        /// Will shutdown HoloNET and its current connetion to the Holochain Conductor.
        /// Unlike the async version, this non async version will not wait until HoloNET disconnects & shutsdown any Holochain Conductors before it returns to the caller. It will later raise the Disconnected, HolochainConductorsShutdownComplete & HoloNETShutdownComplete events. If you wish to wait for HoloNET to disconnect and shutdown the conductors(s) before returning then please use ShutdownAsync instead.
        /// It will also shutdown the current running Holochain Conductor or all conductors depending on the config/params passed in.
        /// </summary>
        /// <param name="shutdownHolochainConductorsMode"></param>
        public HoloNETShutdownEventArgs ShutdownHoloNET(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            _shuttingDownHoloNET = true;

            if (WebSocket.State != WebSocketState.Closed || WebSocket.State != WebSocketState.CloseReceived || WebSocket.State != WebSocketState.CloseSent)
                Disconnect(DisconnectedCallBackMode.UseCallBackEvents, shutdownHolochainConductorsMode);

            return new HoloNETShutdownEventArgs(EndPoint, Config.DnaHash, Config.AgentPubKey, null);
        }

        //private async Task<HolochainConductorsShutdownEventArgs> ShutDownHolochainConductorsInternalAsync()
        //{
        //    HolochainConductorsShutdownEventArgs holochainConductorsShutdownEventArgs = new HolochainConductorsShutdownEventArgs();
        //    holochainConductorsShutdownEventArgs.AgentPubKey = Config.AgentPubKey;
        //    holochainConductorsShutdownEventArgs.DnaHash = Config.DnaHash;
        //    holochainConductorsShutdownEventArgs.EndPoint = EndPoint;

        //    try
        //    {
        //        // Close any conductors down if necessary.
        //        if ((Config.AutoShutdownHolochainConductor && _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.UseConfigSettings)
        //            || _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownCurrentConductorOnly
        //            || _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownAllConductors)
        //        {
        //            if ((Config.ShutDownALLHolochainConductors && _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.UseConfigSettings)
        //            || _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownAllConductors)
        //                holochainConductorsShutdownEventArgs = await ShutDownAllHolochainConductorsAsync();

        //            else if (_conductorProcess != null)
        //            {
        //                Logger.Log("Shutting Down Holochain Conductor...", LogType.Info, true);

        //                if (Config.ShowHolochainConductorWindow)
        //                    _conductorProcess.CloseMainWindow();

        //                _conductorProcess.Kill();
        //                _conductorProcess.Close();

        //                // _conductorProcess.WaitForExit();
        //                _conductorProcess.Dispose();

        //                if (_conductorProcess.StartInfo.FileName.Contains("hc.exe"))
        //                    holochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown = 1;

        //                else if (_conductorProcess.StartInfo.FileName.Contains("holochain.exe"))
        //                    holochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown = 1;

        //                Logger.Log("Holochain Conductor Successfully Shutdown.", LogType.Info);
        //            }
        //        }

        //        OnHolochainConductorsShutdownComplete?.Invoke(this, holochainConductorsShutdownEventArgs);

        //        if (_shuttingDownHoloNET)
        //        {
        //            Logger.Loggers.Clear();

        //            HoloNETShutdownEventArgs holoNETShutdownEventArgs = new HoloNETShutdownEventArgs(this.EndPoint, Config.DnaHash, Config.AgentPubKey, holochainConductorsShutdownEventArgs);
        //            OnHoloNETShutdownComplete?.Invoke(this, holoNETShutdownEventArgs);
        //            _taskCompletionHoloNETShutdown.SetResult(holoNETShutdownEventArgs);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleError("Error occured in HoloNETClient.ShutDownConductorsInternal method.", ex);
        //    }

        //    return holochainConductorsShutdownEventArgs;
        //}

        public async Task<HolochainConductorsShutdownEventArgs> ShutDownHolochainConductorsAsync(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
        {
            HolochainConductorsShutdownEventArgs holochainConductorsShutdownEventArgs = new HolochainConductorsShutdownEventArgs();
            holochainConductorsShutdownEventArgs.AgentPubKey = Config.AgentPubKey;
            holochainConductorsShutdownEventArgs.DnaHash = Config.DnaHash;
            holochainConductorsShutdownEventArgs.EndPoint = EndPoint;

            try
            {
                // Close any conductors down if necessary.
                if ((Config.AutoShutdownHolochainConductor && _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.UseConfigSettings)
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownCurrentConductorOnly
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownAllConductors)
                {
                    if ((Config.ShutDownALLHolochainConductors && _shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.UseConfigSettings)
                    || shutdownHolochainConductorsMode == ShutdownHolochainConductorsMode.ShutdownAllConductors)
                        holochainConductorsShutdownEventArgs = await ShutDownAllHolochainConductorsAsync();

                    else if (_conductorProcess != null)
                    {
                        Logger.Log("Shutting Down Holochain Conductor...", LogType.Info, true);

                        if (Config.ShowHolochainConductorWindow)
                            _conductorProcess.CloseMainWindow();

                        _conductorProcess.Kill();
                        _conductorProcess.Close();

                        // _conductorProcess.WaitForExit();
                        _conductorProcess.Dispose();

                        if (_conductorProcess.StartInfo.FileName.Contains("hc.exe"))
                            holochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown = 1;

                        else if (_conductorProcess.StartInfo.FileName.Contains("holochain.exe"))
                            holochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown = 1;

                        Logger.Log("Holochain Conductor Successfully Shutdown.", LogType.Info);
                    }
                }

                OnHolochainConductorsShutdownComplete?.Invoke(this, holochainConductorsShutdownEventArgs);

                if (_shuttingDownHoloNET)
                {
                    Logger.Loggers.Clear();

                    HoloNETShutdownEventArgs holoNETShutdownEventArgs = new HoloNETShutdownEventArgs(this.EndPoint, Config.DnaHash, Config.AgentPubKey, holochainConductorsShutdownEventArgs);
                    OnHoloNETShutdownComplete?.Invoke(this, holoNETShutdownEventArgs);
                    _taskCompletionHoloNETShutdown.SetResult(holoNETShutdownEventArgs);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error occured in HoloNETClient.ShutDownConductorsInternal method.", ex);
            }

            return holochainConductorsShutdownEventArgs;
        }

        public HolochainConductorsShutdownEventArgs ShutDownHolochainConductors(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode)
        {
            return ShutDownHolochainConductorsAsync(shutdownHolochainConductorsMode).Result;
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

        private void WebSocket_OnConnected(object sender, ConnectedEventArgs e)
        {
            try
            {
                OnConnected?.Invoke(this, new ConnectedEventArgs { EndPoint = e.EndPoint });

                //If the AgentPubKey & DnaHash have already been retreived from the hc sandbox command then raise the OnReadyForZomeCalls event.
                if (WebSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                    SetReadyForZomeCalls();

                //Otherwise, if the getAgentPubKeyAndDnaHashFromConductor param was set to true when calling the Connect method, retreive them now...
                else if (_getAgentPubKeyAndDnaHashFromConductor) //TODO: Might not need to do this now because it is called in the Connect method (need to test this).
                    RetreiveAgentPubKeyAndDnaHashFromConductorAsync();
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnConnected method.", ex);
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
                if (!_taskCompletionDisconnected.Task.IsCompleted)
                    _taskCompletionDisconnected.SetResult(e);

                OnDisconnected?.Invoke(this, e);

                //ShutDownHolochainConductorsInternalAsync();
                ShutDownHolochainConductorsAsync(_shutdownHolochainConductorsMode);
            }
            catch (Exception ex)
            {
                HandleError("Error in HoloNETClient.WebSocket_OnDisconnected method.", ex);
            }
        }

        private void WebSocket_OnDataReceived(object sender, WebSocket.DataReceivedEventArgs e)
        {
            ProcessDataReceived(e);
        }

        private void ProcessDataReceived(WebSocket.DataReceivedEventArgs dataReceivedEventArgs)
        {
            string rawBinaryDataAsString = "";
            string rawBinaryDataDecoded = "";
            string rawBinaryDataAfterMessagePackDecodeAsString = "";
            string rawBinaryDataAfterMessagePackDecodeDecoded = "";

            try
            {
                rawBinaryDataAsString = DataHelper.ConvertBinaryDataToString(dataReceivedEventArgs.RawBinaryData);
                rawBinaryDataDecoded = DataHelper.DecodeBinaryDataAsUTF8(dataReceivedEventArgs.RawBinaryData);

                Logger.Log("DATA RECEIVED", LogType.Info);
                Logger.Log("Raw Data Received: " + rawBinaryDataAsString, LogType.Debug);
                Logger.Log("Raw Data Decoded: " + rawBinaryDataDecoded, LogType.Debug);

                if (rawBinaryDataDecoded.Contains("error"))
                    ProcessErrorReceivedFromConductor(dataReceivedEventArgs, rawBinaryDataDecoded, rawBinaryDataAsString, rawBinaryDataAfterMessagePackDecodeAsString, rawBinaryDataAfterMessagePackDecodeDecoded);
                else
                {
                    HoloNETResponse response = DecodeDataReceived(dataReceivedEventArgs.RawBinaryData, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded);

                    if (rawBinaryDataDecoded.ToUpper().Contains("APP_INFO"))
                        DecodeAppInfoDataReceived(response, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded, rawBinaryDataAfterMessagePackDecodeAsString, rawBinaryDataAfterMessagePackDecodeDecoded);

                    else if (response.type.ToUpper() == "SIGNAL")
                        DecodeSignalDataReceived(response, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded, rawBinaryDataAfterMessagePackDecodeAsString, rawBinaryDataAfterMessagePackDecodeDecoded);
                    else
                        DecodeZomeDataReceived(response, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded, rawBinaryDataAfterMessagePackDecodeAsString, rawBinaryDataAfterMessagePackDecodeDecoded);
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in HoloNETClient.ProcessDataReceived method.";
                HandleError(msg, ex);
            }
        }

        private void ProcessErrorReceivedFromConductor(WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataDecoded, string rawBinaryDataAsString, string rawBinaryDataAfterMessagePackDecodeAsString, string rawBinaryDataAfterMessagePackDecodeDecoded)
        {
            string msg = $"Error in HoloNETClient.ProcessErrorReceivedFromConductor method. Error received from Holochain Conductor: {rawBinaryDataDecoded}";
            HandleError(msg, null);

            HoloNETResponse response = DecodeDataReceived(dataReceivedEventArgs.RawBinaryData, dataReceivedEventArgs, rawBinaryDataAsString, rawBinaryDataDecoded);

            if (rawBinaryDataDecoded.Contains("app_info"))
            {
                AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs()
                {
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    Id = response != null ? response.id.ToString() : null,
                    IsCallSuccessful = true,
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };

                RaiseAppInfoReceivedEvent(args);
            }
            else if (response != null)
            {
                if (response.type.ToUpper() == "SIGNAL")
                {
                    SignalsCallBackEventArgs signalsCallBackEventArgs = new SignalsCallBackEventArgs()
                    {
                        Id = response != null ? response.id.ToString() : null,
                        EndPoint = dataReceivedEventArgs.EndPoint,
                        IsCallSuccessful = false,
                        IsError = true,
                        Message = msg,
                        RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                        RawBinaryDataAsString = rawBinaryDataAsString,
                        RawBinaryDataDecoded = rawBinaryDataDecoded,
                        RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                        RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                        RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                        RawJSONData = dataReceivedEventArgs.RawJSONData,
                        WebSocketResult = dataReceivedEventArgs.WebSocketResult
                    };

                    RaiseSignalReceivedEvent(signalsCallBackEventArgs);
                }
            }
            else
            {
                string id = "";

                if (response != null)
                    id = response.id.ToString();

                ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs
                {
                    Id = id,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    IsError = true,
                    Message = msg,
                    Zome = GetItemFromCache(id, _zomeLookup),
                    ZomeFunction = GetItemFromCache(id, _funcLookup),
                    IsCallSuccessful = false,
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };

                RaiseZomeDataReceivedEvent(zomeFunctionCallBackArgs);
            }
        }

        private HoloNETResponse DecodeDataReceived(byte[] rawBinaryData, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataAsString, string rawBinaryDataDecoded)
        {
            HoloNETResponse response = null;
            HoloNETDataReceivedEventArgs holoNETDataReceivedEventArgs = new HoloNETDataReceivedEventArgs();

            try
            {
                string id = "";
                string rawBinaryDataAfterMessagePackDecodeAsString = "";
                string rawBinaryDataAfterMessagePackDecodeDecoded = "";

                response = MessagePackSerializer.Deserialize<HoloNETResponse>(rawBinaryData, messagePackSerializerOptions);

                id = response.id.ToString();
                rawBinaryDataAfterMessagePackDecodeAsString = DataHelper.ConvertBinaryDataToString(response.data);
                rawBinaryDataAfterMessagePackDecodeDecoded = DataHelper.DecodeBinaryDataAsUTF8(response.data);

                Logger.Log("Response", LogType.Info);
                Logger.Log($"Id: {response.id}", LogType.Info);
                Logger.Log($"Type: {response.type}", LogType.Info);
                Logger.Log(String.Concat("Raw Data Bytes Received After MessagePack Decode: ", rawBinaryDataAfterMessagePackDecodeAsString), LogType.Debug);
                Logger.Log(String.Concat("Raw Data Bytes Decoded After MessagePack Decode: ", rawBinaryDataAfterMessagePackDecodeDecoded), LogType.Debug);

                holoNETDataReceivedEventArgs = new HoloNETDataReceivedEventArgs()
                {
                    Id = id,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    IsCallSuccessful = true,
                    RawBinaryData = rawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occured in HoloNETClient.DecodeDataReceived. Reason: {ex}";
                holoNETDataReceivedEventArgs.IsError = true;
                holoNETDataReceivedEventArgs.Message = msg;
                HandleError(msg, ex);
            }

            OnDataReceived?.Invoke(this, holoNETDataReceivedEventArgs);
            return response;
        }

        private void DecodeAppInfoDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataAsString, string rawBinaryDataDecoded, string rawBinaryDataAfterMessagePackDecodeAsString, string rawBinaryDataAfterMessagePackDecodeDecoded)
        {
            AppInfoCallBackEventArgs args = new AppInfoCallBackEventArgs();

            try
            {
                Logger.Log("APP INFO RESPONSE DATA DETECTED\n", LogType.Info);
                HolonNETAppInfoResponse appInfoResponse = MessagePackSerializer.Deserialize<HolonNETAppInfoResponse>(response.data, messagePackSerializerOptions);

                string dnaHash = ConvertHoloHashToString(appInfoResponse.data.cell_data[0].cell_id[0]);
                string agentPubKey = ConvertHoloHashToString(appInfoResponse.data.cell_data[0].cell_id[1]);

                if (_updateDnaHashAndAgentPubKey)
                {
                    Config.AgentPubKey = agentPubKey;
                    Config.DnaHash = dnaHash;
                }

                Logger.Log($"AGENT PUB KEY RETURNED FROM CONDUCTOR: {Config.AgentPubKey}", LogType.Info);
                Logger.Log($"DNA HASH RETURNED FROM CONDUCTOR:: {Config.DnaHash}", LogType.Info);

                args = new AppInfoCallBackEventArgs()
                {
                    AgentPubKey = agentPubKey,
                    DnaHash = dnaHash,
                    AppInfo = appInfoResponse,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    Id = response.id.ToString(),
                    InstalledAppId = appInfoResponse.data.installed_app_id,
                    IsCallSuccessful = true,
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occured in HoloNETClient.DecodeAppInfoDataReceived. Reason: {ex}";
                args.IsError = true;
                args.Message = msg;
                HandleError(msg, ex);
            }

            //If either the AgentPubKey or DnaHash is empty then attempt to get from the sandbox cmd.
            if (!args.IsError)
            {
                if (!string.IsNullOrEmpty(Config.AgentPubKey) && !string.IsNullOrEmpty(Config.DnaHash))
                    SetReadyForZomeCalls();

                else if (_automaticallyAttemptToGetFromSandboxIfConductorFails)
                    RetreiveAgentPubKeyAndDnaHashFromSandbox();
            }

            RaiseAppInfoReceivedEvent(args);
        }

        private void DecodeSignalDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataAsString, string rawBinaryDataDecoded, string rawBinaryDataAfterMessagePackDecodeAsString, string rawBinaryDataAfterMessagePackDecodeDecoded)
        {
            SignalsCallBackEventArgs signalsCallBackEventArgs = new SignalsCallBackEventArgs();

            try
            {
                Logger.Log("SIGNAL DATA DETECTED\n", LogType.Info);

                HoloNETSignalResponse appResponse = MessagePackSerializer.Deserialize<HoloNETSignalResponse>(response.data, messagePackSerializerOptions);
                Dictionary<string, object> signalDataDecoded = new Dictionary<string, object>();
                SignalType signalType = SignalType.App;
                string agentPublicKey = "";
                string dnaHash = "";
                string signalDataAsString = "";

                if (appResponse != null)
                {
                    agentPublicKey = ConvertHoloHashToString(appResponse.App.CellData[0]);
                    dnaHash = ConvertHoloHashToString(appResponse.App.CellData[1]);
                    Dictionary<object, byte[]> signalData = MessagePackSerializer.Deserialize<Dictionary<object, byte[]>>(appResponse.App.SignalData, messagePackSerializerOptions);

                    foreach (object key in signalData.Keys)
                    {
                        signalDataDecoded[key.ToString()] = MessagePackSerializer.Deserialize<object>(signalData[key]);
                        signalDataAsString = string.Concat(signalDataAsString, key.ToString(), "=", signalDataDecoded[key.ToString()], ",");
                    }

                    signalDataAsString = signalDataAsString.Substring(0, signalDataAsString.Length - 1);
                }
                else
                    signalType = SignalType.System;

                signalsCallBackEventArgs = new SignalsCallBackEventArgs()
                {
                    Id = response != null ? response.id.ToString() : null,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    IsCallSuccessful = true,
                    AgentPubKey = agentPublicKey,
                    DnaHash = dnaHash,
                    RawSignalData = appResponse.App,
                    SignalData = signalDataDecoded,
                    SignalDataAsString = signalDataAsString,
                    SignalType = signalType, //TODO: Need to test for System SignalType... Not even sure if we want to raise this event for System signals? (the js client ignores them currently).
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };
            }
            catch (Exception ex)
            {
                string msg = $"An unknown error occured in HoloNETClient.DecodeSignalDataReceived. Reason: {ex}";
                signalsCallBackEventArgs.IsError = true;
                signalsCallBackEventArgs.Message = msg;
                HandleError(msg, ex);
            }

            RaiseSignalReceivedEvent(signalsCallBackEventArgs);
        }

        private void DecodeZomeDataReceived(HoloNETResponse response, WebSocket.DataReceivedEventArgs dataReceivedEventArgs, string rawBinaryDataAsString, string rawBinaryDataDecoded, string rawBinaryDataAfterMessagePackDecodeAsString, string rawBinaryDataAfterMessagePackDecodeDecoded)
        {
            Logger.Log("ZOME RESPONSE DATA DETECTED\n", LogType.Info);
            HolonNETAppResponse appResponse = MessagePackSerializer.Deserialize<HolonNETAppResponse>(response.data, messagePackSerializerOptions);
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


                Logger.Log($"Decoded Data:\n{keyValuePairsAsString}", LogType.Info);
                //Logger.Log($"appResponse.data as string: {DataHelper.ConvertBinaryDataToString(appResponse.data)}", LogType.Debug);
                //Logger.Log($"appResponse.data decoded: {DataHelper.DecodeBinaryData(appResponse.data)}", LogType.Debug);

                zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs
                {
                    Id = id,
                    EndPoint = dataReceivedEventArgs.EndPoint,
                    Zome = GetItemFromCache(id, _zomeLookup),
                    ZomeFunction = GetItemFromCache(id, _funcLookup),
                    IsCallSuccessful = true,
                    RawZomeReturnData = rawAppResponseData,
                    ZomeReturnData = appResponseData,
                    KeyValuePair = keyValuePairs,
                    KeyValuePairAsString = keyValuePairsAsString,
                    Entry = entryData,
                    RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                    RawBinaryDataAsString = rawBinaryDataAsString,
                    RawBinaryDataDecoded = rawBinaryDataDecoded,
                    RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                    RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                    RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                    RawJSONData = dataReceivedEventArgs.RawJSONData,
                    WebSocketResult = dataReceivedEventArgs.WebSocketResult
                };
            }
            catch (Exception ex)
            {
                try
                {
                    object rawAppResponseData = MessagePackSerializer.Deserialize<object>(appResponse.data, messagePackSerializerOptions);
                    byte[] holoHash = rawAppResponseData as byte[];

                    zomeFunctionCallBackArgs = new ZomeFunctionCallBackEventArgs
                    {
                        Id = id,
                        EndPoint = dataReceivedEventArgs.EndPoint,
                        Zome = GetItemFromCache(id, _zomeLookup),
                        ZomeFunction = GetItemFromCache(id, _funcLookup),
                        IsCallSuccessful = true,
                        RawBinaryData = dataReceivedEventArgs.RawBinaryData,
                        RawBinaryDataAsString = rawBinaryDataAsString,
                        RawBinaryDataDecoded = rawBinaryDataDecoded,
                        RawBinaryDataAfterMessagePackDecode = response != null ? response.data : null,
                        RawBinaryDataAfterMessagePackDecodeAsString = rawBinaryDataAfterMessagePackDecodeAsString,
                        RawBinaryDataAfterMessagePackDecodeDecoded = rawBinaryDataAfterMessagePackDecodeDecoded,
                        RawJSONData = dataReceivedEventArgs.RawJSONData,
                        WebSocketResult = dataReceivedEventArgs.WebSocketResult
                    };

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
                        zomeFunctionCallBackArgs.IsCallSuccessful = false;
                        zomeFunctionCallBackArgs.Message = msg;
                        HandleError(msg, null);
                    }
                }
                catch (Exception ex2)
                {
                    string msg = $"An unknown error occured in HoloNETClient.DecodeZomeDataReceived. Reason: {ex2}";
                    zomeFunctionCallBackArgs.IsError = true;
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
                                    entryData.Entry = decodedEntry;
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

        private void RaiseSignalReceivedEvent(SignalsCallBackEventArgs signalsCallBackEventArgs)
        {
            //TODO: Copy Test Harness logging into appropriate logs in this method for log/debug logtypes... (espcially decoded data)
            Logger.Log(string.Concat("Id: ", signalsCallBackEventArgs.Id, ", Is Call Successful: ", signalsCallBackEventArgs.IsCallSuccessful ? "True" : "False", ", AgentPubKey: ", signalsCallBackEventArgs.AgentPubKey, ", DnaHash: ", signalsCallBackEventArgs.DnaHash, " Raw Binary Data: ", signalsCallBackEventArgs.RawBinaryData, ", Raw JSON Data: ", signalsCallBackEventArgs.RawJSONData, ", Signal Type: ", Enum.GetName(typeof(SignalType), signalsCallBackEventArgs.SignalType), ", Signal Data: ", signalsCallBackEventArgs.SignalDataAsString, "\n"), LogType.Info);
            OnSignalsCallBack?.Invoke(this, signalsCallBackEventArgs);
        }

        private void RaiseAppInfoReceivedEvent(AppInfoCallBackEventArgs appInfoCallBackEventArgs)
        {
            Logger.Log(string.Concat("Id: ", appInfoCallBackEventArgs.Id, ", Is Call Successful: ", appInfoCallBackEventArgs.IsCallSuccessful ? "True" : "False", ", AgentPubKey: ", appInfoCallBackEventArgs.AgentPubKey, ", DnaHash: ", appInfoCallBackEventArgs.DnaHash, ", Installed App Id: ", appInfoCallBackEventArgs.InstalledAppId, ", Raw Binary Data: ", appInfoCallBackEventArgs.RawBinaryData, ", Raw JSON Data: ", appInfoCallBackEventArgs.RawJSONData, "\n"), LogType.Info);
            OnAppInfoCallBack?.Invoke(this, appInfoCallBackEventArgs);
        }

        private void RaiseZomeDataReceivedEvent(ZomeFunctionCallBackEventArgs zomeFunctionCallBackArgs)
        {
            Logger.Log(string.Concat("Id: ", zomeFunctionCallBackArgs.Id, ", Zome: ", zomeFunctionCallBackArgs.Zome, ", Zome Function: ", zomeFunctionCallBackArgs.ZomeFunction, ", Is Zome Call Successful: ", zomeFunctionCallBackArgs.IsCallSuccessful ? "True" : "False", ", Raw Zome Return Data: ", zomeFunctionCallBackArgs.RawZomeReturnData, ", Zome Return Data: ", zomeFunctionCallBackArgs.ZomeReturnData, ", Zome Return Hash: ", zomeFunctionCallBackArgs.ZomeReturnHash, ", Raw Binary Data: ", zomeFunctionCallBackArgs.RawBinaryData, ", Raw Binary Data As String: ", zomeFunctionCallBackArgs.RawBinaryDataAsString, "Raw Binary Data Decoded: ", zomeFunctionCallBackArgs.RawBinaryDataDecoded, ", Raw Binary Data After MessagePack Decode: ", zomeFunctionCallBackArgs.RawBinaryDataAfterMessagePackDecode, ",  Raw Binary Data After MessagePack Decode As String: ", zomeFunctionCallBackArgs.RawBinaryDataAfterMessagePackDecodeAsString, ", Raw Binary Data Decoded After MessagePack Decode: ", zomeFunctionCallBackArgs.RawBinaryDataAfterMessagePackDecodeDecoded, ", Raw JSON Data: ", zomeFunctionCallBackArgs.RawJSONData), LogType.Info);

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
            RetreivingAgentPubKeyAndDnaHash = false;
            _taskCompletionAgentPubKeyAndDnaHashRetreived.SetResult(new AgentPubKeyDnaHash() { AgentPubKey = Config.AgentPubKey, DnaHash = Config.DnaHash });
            
            IsReadyForZomesCalls = true;
            ReadyForZomeCallsEventArgs eventArgs = new ReadyForZomeCallsEventArgs(EndPoint, Config.DnaHash, Config.AgentPubKey);
            OnReadyForZomeCalls?.Invoke(this, eventArgs);
            _taskCompletionReadyForZomeCalls.SetResult(eventArgs);            
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