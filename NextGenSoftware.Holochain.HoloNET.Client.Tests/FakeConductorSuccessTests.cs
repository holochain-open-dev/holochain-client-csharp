using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Each response decoded from the exact Holochain 0.7 wire shape (from holochain_conductor_api / holochain_types 0.7.0
    /// and 0.7.1, which agree on everything except the 0.7.1 additions).
    /// </summary>
    public class FakeConductorSuccessTests
    {
        private static byte[][] Cell => new[] { Dna(), Agent() };
        private static object CellWire => new object[] { Dna(), Agent() };

        private static async Task<T> Admin<T>(string expectRequest, object reply, Func<HoloNETClientAdmin, Task<T>> call) where T : HoloNETDataReceivedBaseEventArgs
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                string seen = null;
                fake.Respond = (type, value) => { seen = type; return reply; };
                var result = await call(admin);
                Assert.Equal(expectRequest, seen);
                Assert.False(result.IsError, result.Message);
                await admin.DisconnectAsync();
                return result;
            }
        }

        private static async Task<T> App<T>(string expectRequest, object reply, Func<HoloNETClientAppAgent, Task<T>> call) where T : HoloNETDataReceivedBaseEventArgs
        {
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            string seen = null;
            fake.Respond = (type, value) => { seen = type; return reply; };
            var result = await call(app);
            Assert.Equal(expectRequest, seen);
            Assert.False(result.IsError, result.Message);
            await app.DisconnectAsync();
            return result;
        }

        // ---------------- admin

        [Fact]
        public async Task PeerMetaInfo_MapKeyedByDnaHash()
        {
            var reply = FakeConductor.Ok("peer_meta_info", new Dictionary<object, object>
            {
                [Dna()] = new Dictionary<string, object>
                {
                    ["gossip.last_sync"] = new Dictionary<string, object> { ["meta_value"] = "42", ["expires_at"] = 1792029232652253L }
                }
            });
            var r = await Admin("peer_meta_info", reply, a => a.GetPeerMetaInfoAsync("ws://peer"));
            var dna = Assert.Single(r.PeerMetaInfo.dna_hash_peer_meta);
            Assert.StartsWith("u", dna.Key);
            Assert.True(dna.Value.ContainsKey("gossip.last_sync"));
        }

        [Fact]
        public async Task CompatibleCells_SetOfAppAndCellIds()
        {
            var reply = FakeConductor.Ok("compatible_cells", new object[] { new object[] { "app", new object[] { CellWire } } });
            var r = await Admin("get_compatible_cells", reply, a => a.GetCompatibleCellsAsync(Dna()));
            var app = Assert.Single(r.CompatibleCells);
            Assert.Equal("app", app.installed_app_id);
            Assert.Single(app.cell_ids);
        }

        [Fact]
        public async Task DnaDefinition_ReadsWasmTaggedZomesAndLineage()
        {
            var zome = new Dictionary<string, object> { ["Wasm"] = new Dictionary<string, object> { ["wasm_hash"] = Hash(0x2a), ["dependencies"] = new[] { "oasis_integrity" } } };
            var reply = FakeConductor.Ok("dna_definition_returned", new Dictionary<string, object>
            {
                ["name"] = "oasis",
                ["modifiers"] = new Dictionary<string, object> { ["network_seed"] = "seed", ["properties"] = new byte[] { 0xc0 } },
                ["integrity_zomes"] = new object[] { new object[] { "oasis_integrity", new Dictionary<string, object> { ["Wasm"] = new Dictionary<string, object> { ["wasm_hash"] = Hash(0x2a), ["dependencies"] = new string[0] } } } },
                ["coordinator_zomes"] = new object[] { new object[] { "oasis", zome } },
                ["lineage"] = new object[0]
            });
            var r = await Admin("get_dna_definition", reply, a => a.GetDnaDefinitionAsync(Cell));
            Assert.Equal("oasis", r.DnaDefinition.Name);
            var coordinator = Assert.Single(r.DnaDefinition.CoordinatorZomes);
            Assert.Equal("oasis", coordinator.ZomeName);
            Assert.Equal("oasis_integrity", Assert.Single(coordinator.Dependencies));
            Assert.Equal(39, coordinator.wasm_hash.Length);
        }

        [Fact]
        public async Task FullStateDump_TypedViewMatches07()
        {
            var reply = FakeConductor.Ok("full_state_dumped", new Dictionary<string, object>
            {
                ["peer_dump"] = new Dictionary<string, object>
                {
                    ["this_agent_info"] = new Dictionary<string, object> { ["kitsune_agent"] = "QUdFTlQ", ["kitsune_space"] = "U1BBQ0U", ["dump"] = "{}" },
                    ["this_dna"] = new object[] { Dna(), "U1BBQ0U" },
                    ["this_agent"] = new object[] { Agent(), "QUdFTlQ" },
                    ["peers"] = new object[0]
                },
                ["source_chain_dump"] = new Dictionary<string, object>
                {
                    ["records"] = new object[] { new Dictionary<string, object> { ["signature"] = new byte[64], ["action_address"] = Action(), ["action"] = new Dictionary<string, object> { ["type"] = "Dna" }, ["entry"] = null } },
                    ["published_ops_count"] = 3
                },
                ["integration_dump"] = new Dictionary<string, object>
                {
                    ["validation_limbo"] = new object[0], ["integration_limbo"] = new object[0], ["integrated"] = new object[0], ["dht_ops_cursor"] = new Dictionary<string, object> { ["when_received"] = 7L, ["hash"] = Hash(0x24) }
                }
            });
            var r = await Admin("dump_full_state", reply, a => a.DumpFullStateAsync(Cell));
            Assert.NotNull(r.DumpedStateJSON);
            var dump = Assert.IsType<FullStateDumpedResponse>(r.DumpedState);
            Assert.Equal("U1BBQ0U", dump.peer_dump.this_agent_info.kitsune_space);
            Assert.Equal(3, dump.source_chain_dump.published_ops_count);
            Assert.Equal(64, Assert.Single(dump.source_chain_dump.records).signature.Length);
            Assert.Equal(7L, dump.integration_dump.dht_ops_cursor.when_received);
        }

        [Fact]
        public async Task StorageInfo_DnaBlobs()
        {
            var reply = FakeConductor.Ok("storage_info", new Dictionary<string, object>
            {
                ["blobs"] = new object[] { new Dictionary<string, object> { ["type"] = "dna", ["value"] = new Dictionary<string, object> { ["dht_data_size"] = 10, ["dht_data_size_on_disk"] = 20, ["dna_hash"] = Dna(), ["used_by"] = new[] { "app" } } } }
            });
            var r = await Admin("storage_info", reply, a => a.GetStorageInfoAsync());
            Assert.NotNull(r.StorageInfoResponse);
        }

        [Fact]
        public async Task Strings_And_Units()
        {
            Assert.Equal("{\"x\":1}", (await Admin("dump_state", FakeConductor.Ok("state_dumped", "{\"x\":1}"), a => a.DumpStateAsync(Cell))).DumpedStateJSON);
            Assert.Equal("{\"c\":1}", (await Admin("dump_conductor_state", FakeConductor.Ok("conductor_state_dumped", "{\"c\":1}"), a => a.DumpConductorStateAsync())).DumpedStateJSON);
            await Admin("update_coordinators", FakeConductor.Ok("coordinators_updated", null), a => a.UpdateCoordinatorsAsync(Dna(), new CoordinatorBundle()));
            await Admin("delete_clone_cell", FakeConductor.Ok("clone_cell_deleted", null), a => a.DeleteCloneCellAsync("app", Cell));
            await Admin("graft_records", FakeConductor.Ok("records_grafted", null), a => a.GraftRecordsAsync(Cell, false, new object[0]));
            await Admin("add_agent_info", FakeConductor.Ok("agent_info_added", null), a => a.AddAgentInfoAsync(new[] { "{}" }));
            await Admin("revoke_app_authentication_token", FakeConductor.Ok("app_authentication_token_revoked", null), a => a.RevokeAppAuthenticationTokenAsync(new byte[] { 1 }));
            Assert.Equal(39, (await Admin("register_dna", FakeConductor.Ok("dna_registered", Dna()), a => a.RegisterDnaAsync(Dna()))).HoloHash.Length);
            Assert.Equal(39, (await Admin("grant_direct_signal_capability", FakeConductor.Ok("direct_signal_capability_granted", Action()), a => a.GrantDirectSignalCapabilityAsync(Cell, "t"))).ActionHash.Length);
        }

        [Fact]
        public async Task NetworkMetricsAndStats_AsJson()
        {
            var stats = new Dictionary<string, object>
            {
                ["transport_stats"] = new Dictionary<string, object> { ["backend"] = "iroh", ["peer_urls"] = new[] { "ws://a" }, ["connections"] = new object[] { new Dictionary<string, object> { ["pub_key"] = "k", ["send_message_count"] = 1, ["send_bytes"] = 2, ["recv_message_count"] = 3, ["recv_bytes"] = 4, ["opened_at_s"] = 5, ["is_direct"] = true } } },
                ["blocked_message_counts"] = new Dictionary<string, object>()
            };
            Assert.Contains("iroh", (await Admin("dump_network_stats", FakeConductor.Ok("network_stats_dumped", stats), a => a.DumpNetworkStatsAsync())).NetworkStatsDumpJSON);
            Assert.Contains("iroh", (await App("dump_network_stats", FakeConductor.Ok("network_stats_dumped", stats), a => a.DumpNetworkStatsAsync())).NetworkStatsDumpJSON);

            var metrics = new Dictionary<object, object> { [Dna()] = new Dictionary<string, object> { ["fetch_state_summary"] = new Dictionary<string, object>(), ["gossip_state_summary"] = new Dictionary<string, object>(), ["local_agents"] = new object[0] } };
            Assert.Contains("fetch_state_summary", (await Admin("dump_network_metrics", FakeConductor.Ok("network_metrics_dumped", metrics), a => a.DumpNetworkMetricsAsync(Dna(), true))).NetworkMetricsDumpJSON);
            Assert.Contains("fetch_state_summary", (await App("dump_network_metrics", FakeConductor.Ok("network_metrics_dumped", metrics), a => a.DumpNetworkMetricsAsync(Dna(), true))).NetworkMetricsDumpJSON);
        }

        // ---------------- app

        [Fact]
        public async Task App_AgentInfo_EnableApp_HostFunctions()
        {
            var info = await App("agent_info", FakeConductor.Ok("agent_info", new[] { "{\"agent\":1}" }), a => a.GetAgentInfoAsync(new[] { Dna() }));
            Assert.Equal("{\"agent\":1}", Assert.Single(info.AgentInfos));
            await App("enable_app", FakeConductor.Ok("ok", null), a => a.EnableAppAsync());
            await App("send_direct_signal", FakeConductor.Ok("ok", null), a => a.SendDirectSignalAsync(Dna(), new[] { Agent() }, new byte[] { 1 }, new byte[64]));
            await App("provide_memproofs", FakeConductor.Ok("ok", null), a => a.ProvideMemproofsAsync(new Dictionary<string, byte[]> { ["oasis"] = new byte[] { 1 } }));
            var fns = await App("list_wasm_host_functions", FakeConductor.Ok("list_wasm_host_functions", new[] { "__hc__create_1" }), a => a.ListWasmHostFunctionsAsync());
            Assert.Equal("__hc__create_1", Assert.Single(fns.WasmHostFunctions));
            Assert.Equal(39, (await App("grant_direct_signal_capability", FakeConductor.Ok("direct_signal_capability_granted", Action()), a => a.GrantDirectSignalCapabilityAsync(Cell, "t"))).ActionHash.Length);
        }

        [Fact]
        public async Task App_PeerMetaInfo()
        {
            var reply = FakeConductor.Ok("peer_meta_info", new Dictionary<object, object> { [Dna()] = new Dictionary<string, object> { ["k"] = new Dictionary<string, object> { ["meta_value"] = 1, ["expires_at"] = null } } });
            var r = await App("peer_meta_info", reply, a => a.GetAppPeerMetaInfoAsync("ws://peer"));
            Assert.Single(r.PeerMetaInfo.dna_hash_peer_meta);
        }

        [Fact]
        public async Task App_CloneCells()
        {
            var cloned = new Dictionary<string, object>
            {
                ["cell_id"] = CellWire, ["clone_id"] = "oasis.0", ["original_dna_hash"] = Dna(9),
                ["dna_modifiers"] = new Dictionary<string, object> { ["network_seed"] = "s", ["properties"] = new byte[] { 0xc0 } },
                ["name"] = "clone", ["enabled"] = true
            };
            var created = await App("create_clone_cell", FakeConductor.Ok("clone_cell_created", cloned), a => a.CreateCloneCellAsync("oasis"));
            Assert.Equal("oasis.0", created.ClonedCell.clone_id);
            var enabled = await App("enable_clone_cell", FakeConductor.Ok("clone_cell_enabled", cloned), a => a.EnableCloneCellAsync(Cell));
            Assert.True(enabled.ClonedCell.enabled);
            await App("disable_clone_cell", FakeConductor.Ok("clone_cell_disabled", null), a => a.DisableCloneCellAsync(Cell));
        }

        [Fact]
        public async Task App_Countersigning()
        {
            var id = new CellId(Dna(), Agent());
            await App("get_countersigning_session_state", FakeConductor.Ok("countersigning_session_state", null), a => a.GetCountersigningSessionStateAsync(id));
            await App("abandon_countersigning_session", FakeConductor.Ok("countersigning_session_abandoned", null), a => a.AbandonCountersigningSessionAsync(id));
            await App("publish_countersigning_session", FakeConductor.Ok("publish_countersigning_session_triggered", null), a => a.PublishCountersigningSessionAsync(id));
        }
    }
}
