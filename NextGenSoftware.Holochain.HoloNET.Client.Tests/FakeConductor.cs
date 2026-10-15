using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// A loopback WebSocket server that speaks Holochain's wire envelope, so tests can drive every response,
    /// error and signal path of HoloNET deterministically (0.7.0 and 0.7.1 shapes) without a conductor.
    /// Requests arrive as {id, type: "request", data: {type, value}}; <see cref="Respond"/> returns the
    /// inner {type, value} reply (see <see cref="Ok"/> / <see cref="Error"/>), or null to send nothing.
    /// </summary>
    public sealed class FakeConductor : IDisposable
    {
        public static readonly MessagePackSerializerOptions Options =
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        private readonly TcpListener _listener = new TcpListener(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();
        private readonly ConcurrentBag<System.Net.WebSockets.WebSocket> _sockets = new ConcurrentBag<System.Net.WebSockets.WebSocket>();

        public int Port { get; }
        public string Uri => "ws://127.0.0.1:" + Port;

        /// <summary>(request type, raw msgpack of the request's "value" or null) → reply envelope or null.</summary>
        public Func<string, byte[], object> Respond { get; set; } = (type, value) => Ok("ok", null);

        public ConcurrentQueue<(string Type, byte[] Value)> Requests { get; } = new ConcurrentQueue<(string, byte[])>();
        public ConcurrentQueue<byte[]> OtherMessages { get; } = new ConcurrentQueue<byte[]>();

        /// <summary>Close the connection when a client sends "authenticate" (how Holochain rejects a bad app token).</summary>
        public bool RejectAuthentication { get; set; }

        public FakeConductor()
        {
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = AcceptLoop();
        }

        public static Dictionary<string, object> Ok(string type, object value)
        {
            var env = new Dictionary<string, object> { ["type"] = type };
            if (value != null) env["value"] = value;
            return env;
        }

        public static Dictionary<string, object> Error(string kind = "internal_error", string detail = "fake conductor error") =>
            new Dictionary<string, object> { ["type"] = "error", ["value"] = new Dictionary<string, object> { ["type"] = kind, ["value"] = detail } };

        /// <summary>Pushes a signal (outer type "signal") to every connected client.</summary>
        public async Task SendSignalAsync(object signalEnvelope)
        {
            byte[] frame = MessagePackSerializer.Serialize(new Dictionary<string, object>
            {
                ["type"] = "signal",
                ["data"] = MessagePackSerializer.Serialize(signalEnvelope, Options)
            }, Options);
            foreach (var ws in _sockets)
                if (ws.State == WebSocketState.Open)
                    await ws.SendAsync(new ArraySegment<byte>(frame), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>Sends raw bytes (for malformed-message tests).</summary>
        public async Task SendRawAsync(byte[] frame)
        {
            foreach (var ws in _sockets)
                if (ws.State == WebSocketState.Open)
                    await ws.SendAsync(new ArraySegment<byte>(frame), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>Closes every client connection from the server side.</summary>
        public async Task DropClientsAsync()
        {
            foreach (var ws in _sockets)
            {
                try { await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); } catch { }
            }
        }

        private async Task AcceptLoop()
        {
            while (!_stop.IsCancellationRequested)
            {
                TcpClient client;
                try { client = await _listener.AcceptTcpClientAsync(); }
                catch { return; }
                _ = Serve(client);
            }
        }

        private async Task Serve(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                string key = await ReadHandshakeKey(stream);
                string accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                byte[] reply = Encoding.ASCII.GetBytes("HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: " + accept + "\r\n\r\n");
                await stream.WriteAsync(reply, 0, reply.Length);

                var ws = System.Net.WebSockets.WebSocket.CreateFromStream(stream, new WebSocketCreationOptions { IsServer = true });
                _sockets.Add(ws);
                var buffer = new byte[1 << 16];
                while (ws.State == WebSocketState.Open && !_stop.IsCancellationRequested)
                {
                    using var message = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), _stop.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            try { await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); } catch { }
                            return;
                        }
                        message.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    await Handle(ws, message.ToArray());
                }
            }
            catch { }
        }

        private async Task Handle(System.Net.WebSockets.WebSocket ws, byte[] frame)
        {
            var outer = MessagePackSerializer.Deserialize<Dictionary<string, object>>(frame, Options);
            if (!outer.TryGetValue("type", out object t) || (string)t != "request")
            {
                OtherMessages.Enqueue(frame); // e.g. app "authenticate"
                if (RejectAuthentication)
                {
                    try { await ws.CloseOutputAsync(WebSocketCloseStatus.PolicyViolation, "bad token", CancellationToken.None); } catch { }
                }
                return;
            }

            byte[] inner = (byte[])outer["data"];
            string requestType = HoloNETClientBase.ReadResponseType(inner);
            byte[] value = RawValue(inner);
            Requests.Enqueue((requestType, value));

            object envelope = Respond(requestType, value);
            if (envelope == null)
                return;

            byte[] response = MessagePackSerializer.Serialize(new Dictionary<string, object>
            {
                ["id"] = Convert.ToUInt64(outer["id"]),
                ["type"] = "response",
                ["data"] = envelope is byte[] raw ? raw : MessagePackSerializer.Serialize(envelope, Options)
            }, Options);
            await ws.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>Raw msgpack of the envelope's "value" (null when absent).</summary>
        public static byte[] RawValue(byte[] envelope)
        {
            var reader = new MessagePackReader(envelope);
            int count = reader.ReadMapHeader();
            for (int i = 0; i < count; i++)
            {
                if (reader.ReadString() == "value")
                    return System.Buffers.BuffersExtensions.ToArray(reader.ReadRaw());
                reader.Skip();
            }
            return null;
        }

        /// <summary>Decodes a request value as a JSON-like object graph for assertions.</summary>
        public static T Decode<T>(byte[] value) => MessagePackSerializer.Deserialize<T>(value, Options);

        private static async Task<string> ReadHandshakeKey(Stream stream)
        {
            var sb = new StringBuilder();
            var one = new byte[1];
            while (!sb.ToString().EndsWith("\r\n\r\n"))
            {
                if (await stream.ReadAsync(one, 0, 1) == 0) throw new IOException("closed during handshake");
                sb.Append((char)one[0]);
            }
            foreach (string line in sb.ToString().Split("\r\n"))
                if (line.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase))
                    return line.Substring(line.IndexOf(':') + 1).Trim();
            throw new IOException("no Sec-WebSocket-Key");
        }

        public void Dispose()
        {
            _stop.Cancel();
            _listener.Stop();
            foreach (var ws in _sockets) { try { ws.Abort(); ws.Dispose(); } catch { } }
        }
    }
}
