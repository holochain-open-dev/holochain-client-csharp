using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Every app request, answered with a conductor error, must complete with IsError and the conductor's message.</summary>
    public class FakeConductorAppErrorTests
    {
        private static byte[][] Cell => new[] { Dna(), Agent() };

        /// <summary>An app client connected to <paramref name="fake"/>, authenticated and ready (agent key and DNA hash preset).</summary>
        internal static async Task<HoloNETClientAppAgent> ConnectApp(FakeConductor fake, string appId = "app")
        {
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 9, 9, 9 };
            var app = new HoloNETClientAppAgent(appId, null, dna);
            app.HoloNETDNA.AgentPubKey = app.ConvertHoloHashToString(Agent());
            app.HoloNETDNA.DnaHash = app.ConvertHoloHashToString(Dna());
            var connected = await app.ConnectAsync(fake.Uri);
            Assert.True(connected.IsConnected, connected.Message);
            return app;
        }

        public static readonly Dictionary<string, Func<HoloNETClientAppAgent, Task<HoloNETDataReceivedBaseEventArgs>>> Calls =
            new Dictionary<string, Func<HoloNETClientAppAgent, Task<HoloNETDataReceivedBaseEventArgs>>>
            {
                ["abandon_countersigning_session"] = async a => await a.AbandonCountersigningSessionAsync(new CellId(Dna(), Agent())),
                ["create_clone_cell"] = async a => await a.CreateCloneCellAsync("oasis"),
                ["disable_clone_cell"] = async a => await a.DisableCloneCellAsync(Cell),
                ["dump_op_timings"] = async a => await a.DumpOpTimingsAsync(Dna()),
                ["enable_clone_cell"] = async a => await a.EnableCloneCellAsync(Cell),
                ["app_info"] = async a => await a.GetAppInfoAsync(),
                ["peer_meta_info"] = async a => await a.GetAppPeerMetaInfoAsync("ws://peer"),
                ["get_countersigning_session_state"] = async a => await a.GetCountersigningSessionStateAsync(new CellId(Dna(), Agent())),
                ["grant_direct_signal_capability"] = async a => await a.GrantDirectSignalCapabilityAsync(Cell, "tag"),
                ["list_wasm_host_functions"] = async a => await a.ListWasmHostFunctionsAsync(),
                ["provide_memproofs"] = async a => await a.ProvideMemproofsAsync(new Dictionary<string, byte[]> { ["oasis"] = new byte[] { 1 } }),
                ["publish_countersigning_session"] = async a => await a.PublishCountersigningSessionAsync(new CellId(Dna(), Agent())),
                ["send_direct_signal"] = async a => await a.SendDirectSignalAsync(Dna(), new[] { Agent() }, new byte[] { 1, 2 }),
                ["agent_info"] = async a => await a.GetAgentInfoAsync(),
                ["dump_network_metrics"] = async a => await a.DumpNetworkMetricsAsync(),
                ["dump_network_stats"] = async a => await a.DumpNetworkStatsAsync(),
                ["enable_app"] = async a => await a.EnableAppAsync(),
            };

        public static IEnumerable<object[]> Names => Calls.Keys.Select(k => new object[] { k });

        [Theory]
        [MemberData(nameof(Names))]
        public async Task ConductorError_CompletesTheCallWithIsError(string name)
        {
            using var fake = new FakeConductor();
            var app = await ConnectApp(fake);
            fake.Respond = (type, value) => FakeConductor.Error("internal_error", "boom-" + name);
            var call = Calls[name](app);
            var done = await Task.WhenAny(call, Task.Delay(10000));
            Assert.True(done == call, name + " never completed after a conductor error");
            var result = await call;
            Assert.True(result.IsError, name + " did not report the error");
            Assert.Contains("boom-" + name, result.Message);
            await app.DisconnectAsync();
        }
    }
}
