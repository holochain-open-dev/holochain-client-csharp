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
        public async Task Live_ListApps_WithAwaitingMemproofsFilter_IsAccepted()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            AppResponse response = await SendAdminAsync("list_apps", new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.ListAppsRequest { status_filter = AppStatusFilter.AwaitingMemproofs.ToWireValue() });

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
        public async Task Live_InstallApp_PayloadIsParsedByConductor()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            // The repo's oasis.happ predates manifest_version "0", so installation must fail on the
            // bundle itself. That proves the request shape was accepted. The old shape failed earlier,
            // because the required `source` field was missing.
            string happ = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory,
                "../../../../HoloNET-Manager/NextGenSoftware.Holochain.HoloNET.Manager/OASIS_hAPP/oasis.happ"));
            if (!System.IO.File.Exists(happ)) return;

            var request = new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.InstallAppRequest
            {
                source = NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.InstallAppRequest.SourceFromPath(happ),
                installed_app_id = "holonet-live-test"
            };

            AppResponse response = await SendAdminAsync("install_app", request);
            string error = MessagePackSerializer.SerializeToJson(response.data);

            Assert.Equal("error", response.type);
            Assert.Contains("AppBundleError", error);
            Assert.DoesNotContain("missing field", error);
        }

        [Fact]
        public async Task Live_AttachAppInterface_ReturnsPort()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            AppResponse response = await SendAdminAsync("attach_app_interface", new NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.AttachAppInterfaceRequest());

            Assert.Equal("app_interface_attached", response.type);
            Assert.Contains("\"port\":", MessagePackSerializer.SerializeToJson(response.data));
        }

        [Fact]
        public async Task Live_HoloNETClientAdmin_InfoCalls_Decode()
        {
            if (string.IsNullOrEmpty(AdminUri)) return;

            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });

            try
            {
                await admin.ConnectAsync(AdminUri);

                var results = new (string Name, HoloNETDataReceivedBaseEventArgs Result)[]
                {
                    ("list_cell_ids", await admin.ListCellIdsAsync()),
                    ("storage_info", await admin.GetStorageInfoAsync()),
                    ("agent_info", await admin.GetAgentInfoAsync()),
                    ("dump_network_stats", await admin.DumpNetworkStatsAsync()),
                    ("dump_network_metrics", await admin.DumpNetworkMetricsAsync()),
                };

                foreach (var (name, result) in results)
                    Assert.False(result.IsError, $"{name}: {result.Message}");
            }
            finally
            {
                await admin.DisconnectAsync();
            }
        }

        /// <summary>
        /// Full app flow against a 0.7.0 hApp: install → enable → sign → attach → connect → zome call.
        /// Requires HOLONET_LIVE_HAPP_PATH (e.g. C:\Source\OASIS-Holochain-hApp\workdir\oasis.happ).
        /// </summary>
        [Fact]
        public async Task Live_InstallEnableSignAttachConnect_AndCallZome()
        {
            string happPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH");
            if (string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(happPath)) return;

            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });

            try
            {
                Assert.False((await admin.ConnectAsync(AdminUri)).IsError);

                string appId = "holonet-live-" + Guid.NewGuid().ToString("N").Substring(0, 8);
                var result = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, happPath, "oasis");

                Assert.True(result.IsAppInstalled, "install: " + result.Message);
                Assert.True(result.IsAppEnabled, "enable: " + result.Message);
                Assert.True(result.IsAppSigned, "sign: " + result.Message);
                Assert.True(result.IsAppAttached, "attach: " + result.Message);
                Assert.True(result.IsAppConnected, "connect: " + result.Message);
                Assert.Equal(CellInfoType.Provisioned, result.CellType);
                Assert.NotNull(result.CellId);

                var zome = await result.HoloNETClientAppAgent.CallZomeFunctionAsync("oasis", "get_all_avatars", null);
                Assert.False(zome.IsError, "zome call: " + zome.Message);

                await result.HoloNETClientAppAgent.DisconnectAsync();
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
