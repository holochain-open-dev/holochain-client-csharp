using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Talks to a real Holochain conductor using HoloNET's own wire envelope classes.
    /// Runs only when HOLONET_LIVE_ADMIN_URI is set (e.g. ws://localhost:65464); otherwise each
    /// test returns immediately. See INTEGRATION_TESTING.md for how to start a 0.7.0 sandbox.
    /// </summary>
    public class LiveConductorTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");
        private static readonly MessagePackSerializerOptions Options =
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        private static async Task<AppResponse> SendAdminAsync(string type, object value)
        {
            using var socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Origin", "http://localhost");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            await socket.ConnectAsync(new Uri(AdminUri), cts.Token);

            var inner = new HoloNETData { type = type, data = value };
            var request = new HoloNETRequest { id = 1, type = "request", data = MessagePackSerializer.Serialize(inner, Options) };
            await socket.SendAsync(MessagePackSerializer.Serialize(request, Options), WebSocketMessageType.Binary, true, cts.Token);

            var buffer = new byte[1 << 20];
            int total = 0;
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer, total, buffer.Length - total), cts.Token);
                total += result.Count;
            } while (!result.EndOfMessage);

            var response = MessagePackSerializer.Deserialize<HoloNETResponse>(new ReadOnlyMemory<byte>(buffer, 0, total), Options);
            Assert.Equal("response", response.type);
            return MessagePackSerializer.Deserialize<AppResponse>(response.data, Options);
        }

        [Fact]
        public async Task Live_GenerateAgentPubKey_ReturnsAgentKey()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            AppResponse response = await SendAdminAsync("generate_agent_pub_key", null);

            Assert.Equal("agent_pub_key_generated", response.type);
            byte[] key = Assert.IsType<byte[]>(response.data);
            Assert.Equal(39, key.Length);
        }

        [Fact]
        public async Task Live_ListApps_ReturnsAppsListed()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            AppResponse response = await SendAdminAsync("list_apps", new Dictionary<string, object> { { "status_filter", null } });

            Assert.Equal("apps_listed", response.type);
        }

        [Fact]
        public async Task Live_OldDataContentKey_IsRejectedByConductor()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            // The envelope HoloNET sent before the "value" fix.
            using var socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Origin", "http://localhost");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            await socket.ConnectAsync(new Uri(AdminUri), cts.Token);

            var inner = new Dictionary<string, object> { { "type", "list_apps" }, { "data", new Dictionary<string, object> { { "status_filter", null } } } };
            var request = new HoloNETRequest { id = 2, type = "request", data = MessagePackSerializer.Serialize(inner, Options) };
            await socket.SendAsync(MessagePackSerializer.Serialize(request, Options), WebSocketMessageType.Binary, true, cts.Token);

            var buffer = new byte[1 << 16];
            var received = await socket.ReceiveAsync(buffer, cts.Token);
            var response = MessagePackSerializer.Deserialize<HoloNETResponse>(new ReadOnlyMemory<byte>(buffer, 0, received.Count), Options);
            var appResponse = MessagePackSerializer.Deserialize<AppResponse>(response.data, Options);

            Assert.Equal("error", appResponse.type);
        }

        [Fact]
        public async Task Live_HoloNETClientAdmin_EndToEnd()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            var admin = new HoloNETClientAdmin(new HoloNETDNA
            {
                AutoStartHolochainConductor = false,
                AutoShutdownHolochainConductor = false,
                HolochainConductorAdminURI = AdminUri
            });

            try
            {
                var connected = await admin.ConnectAsync(AdminUri);
                Assert.False(connected.IsError, connected.Message);

                var key = await admin.GenerateAgentPubKeyAsync();
                Assert.False(key.IsError, key.Message);
                Assert.False(string.IsNullOrEmpty(key.AgentPubKey));

                var apps = await admin.ListAppsAsync(AppStatusFilter.Enabled);
                Assert.False(apps.IsError, apps.Message);

                var dnas = await admin.ListDnasAsync();
                Assert.False(dnas.IsError, dnas.Message);
            }
            finally
            {
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task Live_ListApps_WithEnabledFilter_IsAccepted()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            AppResponse response = await SendAdminAsync("list_apps", new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.ListAppsRequest { status_filter = AppStatusFilter.Enabled });

            Assert.Equal("apps_listed", response.type);
        }

        [Fact]
        public async Task Live_ConductorError_IsSurfacedByHoloNETClientAdmin()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });

            try
            {
                await admin.ConnectAsync(AdminUri);
                var result = await admin.EnableAppAsync("no-such-app-" + Guid.NewGuid());
                Assert.True(result.IsError, "Enabling a non-existent app must surface the conductor error.");
            }
            finally
            {
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task Live_ListDnas_ReturnsDnasListed()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            AppResponse response = await SendAdminAsync("list_dnas", null);

            Assert.Equal("dnas_listed", response.type);
        }
    }
}
