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
        private sealed class CountingSigner : IZomeCallSigner
        {
            private readonly HoloNETClientAppAgent _client;
            public int Calls { get; private set; }
            public CountingSigner(HoloNETClientAppAgent client) => _client = client;

            public Task<ZomeCallParamsSigned> SignZomeCallAsync(ZomeCallToSign call)
            {
                Calls++;
                return Task.FromResult(_client.SignZomeCallWithAuthorizedCredentials(call));
            }
        }

        private sealed class DecliningSigner : IZomeCallSigner
        {
            public Task<ZomeCallParamsSigned> SignZomeCallAsync(ZomeCallToSign call) => Task.FromResult<ZomeCallParamsSigned>(null);
        }

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
                // Avatars may exist in the shared DNA; when they do, real 0.7.0 Records must decode down to entry fields.
                if (zome.Records.Count > 0)
                    Assert.Contains("username", zome.KeyValuePair.Keys);

                // Pluggable signer: the same call signed through IZomeCallSigner.
                var app = (HoloNETClientAppAgent)result.HoloNETClientAppAgent;
                var signer = new CountingSigner(app);
                app.ZomeCallSigner = signer;
                var signedExternally = await app.CallZomeFunctionAsync("oasis", "get_all_avatars", null);
                Assert.False(signedExternally.IsError, "zome call via IZomeCallSigner: " + signedExternally.Message);
                Assert.Equal(1, signer.Calls);

                // A signer that declines must fail the call (not hang, not fall back silently). Without an
                // OnError subscriber HoloNET reports errors by throwing, so accept either form.
                app.ZomeCallSigner = new DecliningSigner();
                try
                {
                    var declined = await app.CallZomeFunctionAsync("oasis", "get_all_avatars", null);
                    Assert.True(declined.IsError, "a declining signer should produce an error result");
                }
                catch (HoloNETException ex)
                {
                    Assert.Contains("ZomeCallSigner did not sign", ex.ToString());
                }
                app.ZomeCallSigner = null;

                await result.HoloNETClientAppAgent.DisconnectAsync();
            }
            finally
            {
                await admin.DisconnectAsync();
            }
        }

        /// <summary>
        /// Clone cells, countersigning state, WASM host functions and op timings.
        /// Requires HOLONET_LIVE_CLONE_HAPP_PATH: a 0.7.0 hApp with role "oasis", clone_limit &gt; 0
        /// (e.g. the OASIS hApp repacked with clone_limit: 3).
        /// </summary>
        [Fact]
        public async Task Live_AppApis_CloneCells_Countersigning_HostFns_OpTimings()
        {
            string happPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_CLONE_HAPP_PATH");
            if (string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(happPath)) return;

            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });

            try
            {
                await admin.ConnectAsync(AdminUri);
                string appId = "holonet-clone-" + Guid.NewGuid().ToString("N").Substring(0, 8);
                var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, happPath, "oasis");
                Assert.True(installed.IsAppConnected, installed.Message);
                var app = installed.HoloNETClientAppAgent;

                var created = await app.CreateCloneCellAsync("oasis", new Dictionary<string, object> { { "network_seed", Guid.NewGuid().ToString() } }, null, "test clone");
                Assert.False(created.IsError, "create clone: " + created.Message);
                Assert.NotNull(created.ClonedCell);
                string cloneId = created.ClonedCell.clone_id;
                Assert.False(string.IsNullOrEmpty(cloneId), "clone_id was not decoded");

                var disabled = await app.DisableCloneCellAsync(cloneId);
                Assert.False(disabled.IsError, "disable clone: " + disabled.Message);

                var enabled = await app.EnableCloneCellAsync(cloneId);
                Assert.False(enabled.IsError, "enable clone: " + enabled.Message);

                Assert.False((await app.DisableCloneCellAsync(cloneId)).IsError);
                var deleted = await admin.DeleteCloneCellAsync(appId, cloneId);
                Assert.False(deleted.IsError, "delete clone: " + deleted.Message);

                // The standard 0.7.0 conductor is built without unstable-countersigning, so it rejects
                // this request. It must come back as an error, not hang.
                var countersigning = await app.GetCountersigningSessionStateAsync(new CellId(installed.CellId[0], installed.CellId[1]));
                Assert.True(countersigning.IsError, "countersigning is feature-gated; expected the conductor to reject it");

                var hostFns = await app.ListWasmHostFunctionsAsync();
                Assert.False(hostFns.IsError, "host fns: " + hostFns.Message);

                var appTimings = await app.DumpOpTimingsAsync(installed.CellId[0]);
                Assert.False(appTimings.IsError, "app op timings: " + appTimings.Message);

                var adminTimings = await admin.DumpOpTimingsAsync(installed.CellId[0]);
                Assert.False(adminTimings.IsError, "admin op timings: " + adminTimings.Message);

                var dnaDef = await admin.GetDnaDefinitionAsync(installed.CellId);
                Assert.False(dnaDef.IsError, "dna definition: " + dnaDef.Message);

                var dnaDefByHash = await admin.GetDnaDefinitionAsync(installed.CellId[0]);
                Assert.False(dnaDefByHash.IsError, "dna definition by hash: " + dnaDefByHash.Message);

                // Feature-gated (unstable-migration) and absent from the standard 0.7.0 build: must error, not hang.
                var compatible = await admin.GetCompatibleCellsAsync(installed.CellId[0]);
                Assert.True(compatible.IsError, "get_compatible_cells is feature-gated; expected the conductor to reject it");

                var grants = await admin.ListCapabilityGrantsAsync(appId);
                Assert.False(grants.IsError, "capability grants: " + grants.Message);

                var state = await admin.DumpStateAsync(installed.CellId);
                Assert.False(state.IsError, "dump state: " + state.Message);

                var fullState = await admin.DumpFullStateAsync(installed.CellId);
                Assert.False(fullState.IsError, "dump full state: " + fullState.Message);

                var token = await admin.IssueAppAuthenticationTokenAsync(appId);
                Assert.False(token.IsError, "issue token: " + token.Message);
                var revoked = await admin.RevokeAppAuthenticationTokenAsync(token.TokenIssued.token);
                Assert.False(revoked.IsError, "revoke token: " + revoked.Message);

                await app.DisconnectAsync();
            }
            finally
            {
                await admin.DisconnectAsync();
            }
        }

        /// <summary>
        /// Deferred membrane proofs: install without proofs → connect → provide_memproofs → enable.
        /// Requires HOLONET_LIVE_MEMPROOFS_HAPP_PATH: a 0.7.0 hApp with allow_deferred_memproofs: true.
        /// </summary>
        [Fact]
        public async Task Live_DeferredMemproofs_ProvideThenEnable()
        {
            string happPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_MEMPROOFS_HAPP_PATH");
            if (string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(happPath)) return;

            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });

            try
            {
                await admin.ConnectAsync(AdminUri);
                string appId = "holonet-memproofs-" + Guid.NewGuid().ToString("N").Substring(0, 8);

                var installed = await admin.InstallAppAsync(appId, happPath);
                Assert.False(installed.IsError, "install: " + installed.Message);

                var beforeProofs = await admin.EnableAppAsync(appId);
                Assert.True(beforeProofs.IsError, "enabling before memproofs are provided should be rejected");

                var attached = await admin.AttachAppInterfaceAsync();
                Assert.False(attached.IsError, "attach: " + attached.Message);
                var token = await admin.IssueAppAuthenticationTokenAsync(appId);
                Assert.False(token.IsError, "token: " + token.Message);

                var app = new HoloNETClientAppAgent(appId, installed.AgentPubKey, new HoloNETDNA
                {
                    AutoStartHolochainConductor = false,
                    AutoShutdownHolochainConductor = false,
                    AgentPubKey = installed.AgentPubKey,
                    DnaHash = installed.DnaHash,
                    AppAuthenticationToken = token.TokenIssued.token
                });

                var connected = await app.ConnectAsync(appId, $"ws://127.0.0.1:{attached.Port}");
                Assert.False(connected.IsError, "connect: " + connected.Message);

                var provided = await app.ProvideMemproofsAsync(new Dictionary<string, byte[]> { { "oasis", new byte[] { 1, 2, 3 } } });
                Assert.False(provided.IsError, "provide memproofs: " + provided.Message);

                var enabled = await admin.EnableAppAsync(appId);
                Assert.False(enabled.IsError, "enable after memproofs: " + enabled.Message);

                await app.DisconnectAsync();
            }
            finally
            {
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task Live_InstallAppFromBytes_Installs()
        {
            string happPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH");
            if (string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(happPath)) return;

            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });

            try
            {
                await admin.ConnectAsync(AdminUri);
                var installed = await admin.InstallAppFromBytesAsync("holonet-bytes-" + Guid.NewGuid().ToString("N").Substring(0, 8), System.IO.File.ReadAllBytes(happPath));
                Assert.False(installed.IsError, "install from bytes: " + installed.Message);
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
