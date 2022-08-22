using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NextGenSoftware.Logging;

namespace NextGenSoftware.WebSocket
{
    public class WebSocket
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _cancellationToken;
        private WebSocketConfig _config = null;

        //Events
        public delegate void Connected(object sender, ConnectedEventArgs e);
        public event Connected OnConnected;

        public delegate void Disconnected(object sender, DisconnectedEventArgs e);
        public event Disconnected OnDisconnected;

        public delegate void DataReceived(object sender, DataReceivedEventArgs e);
        public event DataReceived OnDataReceived;

        public delegate void Error(object sender, WebSocketErrorEventArgs e);
        public event Error OnError;

        // Properties
        public string EndPoint { get; private set; }
        public ClientWebSocket ClientWebSocket { get; private set; } // Original HoloNET WebSocket (still works):
       // public UnityWebSocket UnityWebSocket { get; private set; } //Temporily using UnityWebSocket code until can find out why not working with RSM Conductor...
        
        public WebSocketConfig Config
        {
            get
            {
                if (_config == null)
                    _config = new WebSocketConfig();

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
                return ClientWebSocket.State;
                //return UnityWebSocket.ClientWebSocket.State;
            }
        }

        //public IWebSocketClientNET NetworkServiceProvider { get; set; }
        //public NetworkServiceProviderMode NetworkServiceProviderMode { get; set; }

        public WebSocket(string endPointURI, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "NextGenSoftwareWebSocket.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
        {
            EndPoint = endPointURI;
            Logging.Logging.Loggers.Add(new DefaultLogger(logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour));
            Init();
        }

        public WebSocket(string endPointURI, IEnumerable<ILogger> loggers)
        {
            EndPoint = endPointURI;
            Logging.Logging.Loggers = new List<ILogger>(loggers);
            Init();
        }

        public WebSocket(string endPointURI, ILogger logger)
        {
            Logging.Logging.Loggers.Add(logger);
            EndPoint = endPointURI;
            Init();
        }

        private void Init()
        {
            ClientWebSocket = new ClientWebSocket(); // The original built-in HoloNET WebSocket
            ClientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(Config.KeepAliveSeconds);

            //UnityWebSocket = new UnityWebSocket(endPointURI); //The Unity Web Socket code I ported wraps around the ClientWebSocket.
            //UnityWebSocket.OnOpen += UnityWebSocket_OnOpen;
            //UnityWebSocket.OnClose += UnityWebSocket_OnClose;

            //UnityWebSocket.OnError += UnityWebSocket_OnError;
            //UnityWebSocket.OnMessage += UnityWebSocket_OnMessage;


            _cancellationToken = _cancellationTokenSource.Token; //TODO: do something with this!
        }

        /*
        private void UnityWebSocket_OnMessage(byte[] data)
        {
            //OnDataReceived?.Invoke(this, new DataReceivedEventArgs { EndPoint = EndPoint, RawBinaryData = data });
            OnDataReceived?.Invoke(this, new DataReceivedEventArgs(EndPoint, true, data, null, null));
        }

        private void UnityWebSocket_OnError(string errorMsg)
        {
            OnError?.Invoke(this, new WebSocketErrorEventArgs() { EndPoint = EndPoint, Reason = errorMsg });
        }

        private void UnityWebSocket_OnClose(WebSocketCloseCode closeCode)
        {
            OnDisconnected?.Invoke(this, new DisconnectedEventArgs() { EndPoint = EndPoint, Reason = closeCode.ToString() });
        }

        private void UnityWebSocket_OnOpen()
        {
            OnConnected?.Invoke(this, new ConnectedEventArgs { EndPoint = EndPoint });
        }
        */

        //public async Task Connect()
        //{
        //    await UnityWebSocket.Connect();
        //    // await UnityWebSocket.Receive();
        //}

        // The original HoloNET Connect method (still works).
        public async Task Connect()
        {
            try
            {
                if (Logging.Logging.Loggers.Count == 0)
                    throw new WebSocketException("ERROR: No Logger Has Been Specified! Please set a Logger with the Logging.Logging.Loggers Property.");

                if (ClientWebSocket.State != WebSocketState.Connecting && ClientWebSocket.State != WebSocketState.Open && ClientWebSocket.State != WebSocketState.Aborted)
                {
                    Logging.Logging.Log(string.Concat("Connecting to ", EndPoint, "..."), LogType.Info, true);

                    await ClientWebSocket.ConnectAsync(new Uri(EndPoint), CancellationToken.None);
                    //NetworkServiceProvider.Connect(new Uri(EndPoint));
                    //TODO: need to be able to await this.

                    //if (NetworkServiceProvider.NetSocketState == NetSocketState.Open)
                    if (ClientWebSocket.State == WebSocketState.Open)
                    {
                        Logging.Logging.Log(string.Concat("Connected to ", EndPoint), LogType.Info);
                        OnConnected?.Invoke(this, new ConnectedEventArgs { EndPoint = EndPoint });
                        StartListen();
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(string.Concat("Error occured connecting to ", EndPoint), e);
            }
        }

        public async Task Disconnect()
        {
            try
            {
                if (Logging.Logging.Loggers.Count == 0)
                    throw new WebSocketException("ERROR: No Logger Has Been Specified! Please set a Logger with the Logging.Logging.Loggers Property.");

                if (ClientWebSocket != null && ClientWebSocket.State != WebSocketState.Connecting && ClientWebSocket.State != WebSocketState.Closed && ClientWebSocket.State != WebSocketState.Aborted && ClientWebSocket.State != WebSocketState.CloseSent && ClientWebSocket.State != WebSocketState.CloseReceived)
                {
                    Logging.Logging.Log(string.Concat("Disconnecting from ", EndPoint, "..."), LogType.Info, true);
                    await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client manually disconnected.", CancellationToken.None);

                    if (ClientWebSocket.State == WebSocketState.Closed)
                    {
                        Logging.Logging.Log(string.Concat("Disconnected from ", EndPoint), LogType.Info);
                        OnDisconnected?.Invoke(this, new DisconnectedEventArgs { EndPoint = EndPoint, Reason = "Disconnected Method Called." });
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(string.Concat("Error occured disconnecting from ", EndPoint), e);
            }
        }

        public async Task SendRawDataAsync(byte[] data)
        {
            string rawBytes = "";

            foreach (byte b in data)
                rawBytes = string.Concat(rawBytes, ", ", b.ToString());

            Logging.Logging.Log("Sending Raw Data...", LogType.Info);
            Logging.Logging.Log($"UTF8: {Encoding.UTF8.GetString(data, 0, data.Length)}", LogType.Debug);
            Logging.Logging.Log($"Bytes: {rawBytes}", LogType.Debug);
            //await UnityWebSocket.Send(data);

            // Original HoloNET code (still works):
            if (ClientWebSocket.State != WebSocketState.Open)
            {
                string msg = "Connection is not open!";
                HandleError(msg, new InvalidOperationException(msg));
            }

            int sendChunkSize = Config.SendChunkSize;
            var messagesCount = (int)Math.Ceiling((double)data.Length / sendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (sendChunkSize * i);
                var count = sendChunkSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > data.Length)
                    count = data.Length - offset;

                Logging.Logging.Log(string.Concat("Sending Data Packet ", (i + 1), " of ", messagesCount, "..."), LogType.Debug, true);
                //await ClientWebSocket.SendAsync(new ArraySegment<byte>(data, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken);
                await ClientWebSocket.SendAsync(new ArraySegment<byte>(data, offset, count), WebSocketMessageType.Binary, lastMessage, _cancellationToken);
            }

            Logging.Logging.Log("Sending Raw Data... Done!", LogType.Info);
        }

        private async Task StartListen()
        {
            var buffer = new byte[Config.ReceiveChunkSize];
            Logging.Logging.Log(string.Concat("Listening on ", EndPoint, "..."), LogType.Info, true);

            try
            {
                while (ClientWebSocket.State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();
                    List<byte> dataResponse = new List<byte>();

                    WebSocketReceiveResult result;
                    do
                    {
                        if (Config.NeverTimeOut)
                            result = await ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        else
                        {
                            using (var cts = new CancellationTokenSource((Config.TimeOutSeconds) * 1000))
                                result = await ClientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                        }

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            string msg = "Closing because received close message."; //TODO: Move all strings to constants at top or resources.strings
                            await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, CancellationToken.None);
                            OnDisconnected?.Invoke(this, new DisconnectedEventArgs { EndPoint = EndPoint, Reason = msg });
                            Logging.Logging.Log(msg, LogType.Info);

                            //AttemptReconnect(); //TODO: Not sure re-connect here?
                        }
                        else
                        {
                            stringResult.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                            Logging.Logging.Log(string.Concat("Received Data: ", stringResult), LogType.Info);
                            OnDataReceived?.Invoke(this, new DataReceivedEventArgs(null, EndPoint, true, buffer, stringResult.ToString(), result));
                        }
                    } while (!result.EndOfMessage);
                }
            }

            catch (TaskCanceledException ex)
            {
                string msg = string.Concat("Connection timed out after ", (Config.TimeOutSeconds), " seconds.");
                OnDisconnected?.Invoke(this, new DisconnectedEventArgs { EndPoint = EndPoint, Reason = msg });
                HandleError(msg, ex);
                await AttemptReconnect();
            }

            catch (Exception ex)
            {
                OnDisconnected?.Invoke(this, new DisconnectedEventArgs { EndPoint = EndPoint, Reason = string.Concat("Error occured: ", ex) });
                HandleError("Disconnected because an error occured.", ex);
                await AttemptReconnect();
            }

            finally
            {
                ClientWebSocket.Dispose();
            }
        }

        private async Task AttemptReconnect()
        {
            for (int i = 0; i < (Config.ReconnectionAttempts); i++)
            {
                if (ClientWebSocket.State == WebSocketState.Open)
                    break;

                Logging.Logging.Log(string.Concat("Attempting to reconnect... Attempt ", +i), LogType.Info, true);
                await Connect();
                await Task.Delay(Config.ReconnectionIntervalSeconds);
            }
        }

        private void HandleError(string message, Exception exception)
        {
            message = string.Concat(message, "\nError Details: ", exception != null ? exception.ToString() : "");
            Logging.Logging.Log(message, LogType.Error);

            OnError?.Invoke(this, new WebSocketErrorEventArgs { EndPoint = EndPoint, Reason = message, ErrorDetails = exception });

            switch (Config.ErrorHandlingBehaviour)
            {
                case ErrorHandlingBehaviour.AlwaysThrowExceptionOnError:
                    throw new WebSocketException(message, exception, this.EndPoint);

                case ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent:
                    {
                        if (OnError == null)
                            throw new WebSocketException(message, exception, this.EndPoint);
                    }
                    break;
            }
        }
    }
}