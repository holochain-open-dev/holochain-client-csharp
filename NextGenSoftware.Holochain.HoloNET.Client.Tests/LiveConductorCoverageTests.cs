using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Live tests for HoloNET paths the original live suite didn't reach: zome results that carry real Records,
    /// app_info, the synchronous zome call, the app lifecycle (interfaces, disable, uninstall) and DNA registration.
    ///
    /// Needs a Holochain 0.7.0 conductor (HOLONET_LIVE_ADMIN_URI) and the OASIS hApp (HOLONET_LIVE_HAPP_PATH);
    /// see INTEGRATION_TESTING.md. Otherwise every test is reported as skipped. Each test installs its own app and
    /// uninstalls it, so the sandbox doesn't fill up.
    /// </summary>
    [Collection(LiveConductor.Collection)]
    public class LiveConductorCoverageTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");
        private static readonly string HappPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH");

        /// <summary>Fails with the step name instead of hanging the run when a call never completes.</summary>
        private static async Task<T> Step<T>(string name, Task<T> task, int seconds = 60)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds))) != task)
                throw new TimeoutException($"'{name}' did not complete within {seconds}s (HoloNET never completed the request).");
            return await task;
        }

        private static void RequireConductorAndHapp() =>
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath),
                "Needs HOLONET_LIVE_ADMIN_URI and HOLONET_LIVE_HAPP_PATH (see INTEGRATION_TESTING.md).");

        private static HoloNETClientAdmin NewAdmin() => new HoloNETClientAdmin(new HoloNETDNA
        {
            AutoStartHolochainConductor = false,
            AutoShutdownHolochainConductor = false,
            HolochainConductorAdminURI = AdminUri
        });

        /// <summary>Installs the OASIS hApp under a fresh id, runs <paramref name="body"/>, then uninstalls it.</summary>
        private static async Task WithInstalledApp(Func<HoloNETClientAdmin, string, InstallEnableSignAttachAndConnectToHappEventArgs, Task> body)
        {
            var admin = NewAdmin();
            Assert.False((await admin.ConnectAsync(AdminUri)).IsError);
            string appId = "holonet-cov-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            try
            {
                var installed = await Step("install/enable/sign/attach/connect", admin.InstallEnableSignAttachAndConnectToHappAsync(appId, HappPath, "oasis"), 180);
                Assert.True(installed.IsAppConnected, "install/connect: " + installed.Message);
                try
                {
                    await body(admin, appId, installed);
                }
                finally
                {
                    await installed.HoloNETClientAppAgent.DisconnectAsync();
                }
            }
            finally
            {
                await admin.UninstallAppAsync(appId);
                await admin.DisconnectAsync();
            }
        }

        private static string Snip(string text, string marker, int length)
        {
            int i = text.IndexOf(marker, StringComparison.Ordinal);
            return i < 0 ? "(no " + marker + ")" : text.Substring(i, Math.Min(length, text.Length - i));
        }

        private static Dictionary<string, object> NewAvatar(string username) => new Dictionary<string, object>
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["username"] = username,
            ["email"] = username + "@example.com",
            ["fields"] = new Dictionary<string, object> { ["first_name"] = "Live" }
        };

        [SkippableFact]
        public async Task Live_ZomeCallsReturningRecords_DecodeEntryFields()
        {
            RequireConductorAndHapp();
            await WithInstalledApp(async (admin, appId, app) =>
            {
                string username = "cov-" + Guid.NewGuid().ToString("N").Substring(0, 8);

                // create_avatar returns the new Record: HoloNET must decode its entry down to fields.
                var created = await Step("create_avatar", app.HoloNETClientAppAgent.CallZomeFunctionAsync("oasis", "create_avatar", NewAvatar(username)));
                Assert.False(created.IsError, "create_avatar: " + created.Message);
                Assert.Single(created.Records);
                Assert.Equal(username, created.KeyValuePair["username"]);

                var loaded = await Step("get_avatar_by_username", app.HoloNETClientAppAgent.CallZomeFunctionAsync("oasis", "get_avatar_by_username", username));
                Assert.False(loaded.IsError, "get_avatar_by_username: " + loaded.Message);
                Assert.Equal(username, loaded.KeyValuePair["username"]);

                // The synchronous API must not deadlock (run off the test's context, as a game loop would).
                var sync = await Step("sync CallZomeFunction", Task.Run(() => app.HoloNETClientAppAgent.CallZomeFunction("oasis", "get_avatar_by_username", username)));
                Assert.False(sync.IsError, "sync CallZomeFunction: " + sync.Message);
                Assert.Equal(username, sync.KeyValuePair["username"]);
            });
        }

        [SkippableFact]
        public async Task Live_AppInfo_DescribesTheInstalledApp()
        {
            RequireConductorAndHapp();
            await WithInstalledApp(async (admin, appId, app) =>
            {
                var info = await Step("app_info", app.HoloNETClientAppAgent.GetAppInfoAsync());
                Assert.False(info.IsError, "app_info: " + info.Message);
                Assert.NotNull(info.AppInfoResponse);
                Assert.Equal(appId, info.AppInfoResponse.data.installed_app_id);
                Assert.Contains("oasis", info.AppInfoResponse.data.cell_info.Keys);
            });
        }

        [SkippableFact]
        public async Task Live_AppLifecycle_ListInterfaces_Disable_Uninstall()
        {
            RequireConductorAndHapp();
            await WithInstalledApp(async (admin, appId, app) =>
            {
                var interfaces = await Step("list_app_interfaces", admin.ListInterfacesAsync());
                Assert.False(interfaces.IsError, "list_app_interfaces: " + interfaces.Message);
                Assert.NotEmpty(interfaces.WebSocketPorts);

                // Holochain 0.7 status {"type": "enabled"} decodes as Running.
                var running = (await admin.ListAppsAsync(AppStatusFilter.Enabled)).Apps.Single(a => a.installed_app_id == appId);
                Assert.Equal(AppInfoStatusEnum.Running, ((AppInfo)running).AppStatus);

                var disabled = await Step("disable_app", admin.DisableAppAsync(appId));
                Assert.False(disabled.IsError, "disable_app: " + disabled.Message);
                var disabledApps = await admin.ListAppsAsync(AppStatusFilter.Disabled);
                Assert.Contains(disabledApps.Apps, a => a.installed_app_id == appId);
                var disabledApp = (AppInfo)disabledApps.Apps.Single(a => a.installed_app_id == appId);
                Assert.Equal(AppInfoStatusEnum.Disabled, disabledApp.AppStatus);
                Assert.Equal("User", disabledApp.AppStatusReason);

                var uninstalled = await Step("uninstall_app", admin.UninstallAppAsync(appId));
                Assert.False(uninstalled.IsError, "uninstall_app: " + uninstalled.Message);
                var enabledApps = await admin.ListAppsAsync(AppStatusFilter.Enabled);
                var disabledAfter = await admin.ListAppsAsync(AppStatusFilter.Disabled);
                Assert.DoesNotContain(enabledApps.Apps.Concat(disabledAfter.Apps), a => a.installed_app_id == appId);
            });
        }

        /// <summary>
        /// Holochain 0.7.0 has no register_dna admin request (HoloNET marks RegisterDna obsolete). The call must come back
        /// as a clean error, not hang or throw.
        /// </summary>
        [SkippableFact]
        public async Task Live_ObsoleteRegisterDna_FailsCleanly()
        {
            RequireConductorAndHapp();
            var admin = NewAdmin();
            Assert.False((await admin.ConnectAsync(AdminUri)).IsError);
            try
            {
#pragma warning disable CS0618 // exercising the obsolete API on purpose
                var registered = await Step("register_dna", admin.RegisterDnaAsync(HappPath, network_seed: Guid.NewGuid().ToString()));
#pragma warning restore CS0618
                Assert.True(registered.IsError, "register_dna should fail on Holochain 0.7.0");
            }
            finally
            {
                await admin.DisconnectAsync();
            }
        }

        [SkippableFact]
        public async Task Live_AgentInfo_GetThenAddBack()
        {
            RequireConductorAndHapp();
            await WithInstalledApp(async (admin, appId, app) =>
            {
                // The conductor publishes an agent info for the new cell shortly after install.
                List<string> infos = null;
                for (int attempt = 0; attempt < 15 && (infos == null || infos.Count == 0); attempt++)
                {
                    var got = await Step("agent_info", admin.GetAgentInfoAsync());
                    Assert.False(got.IsError, "agent_info: " + got.Message);
                    infos = got.AgentInfos;
                    if (infos.Count == 0) await Task.Delay(1000);
                }
                Assert.NotEmpty(infos);
                Assert.All(infos, info => Assert.StartsWith("{", info.TrimStart()));

                // Adding our own (already known) infos back must be accepted.
                var added = await Step("add_agent_info", admin.AddAgentInfoAsync(infos));
                Assert.False(added.IsError, "add_agent_info: " + added.Message);
            });
        }

        /// <summary>Requests HoloNET added for full 0.7 coverage: admin dump_conductor_state; app agent_info, network metrics/stats, enable_app.</summary>
        [SkippableFact]
        public async Task Live_ConductorState_AndAppInterfaceAgentInfoMetricsStatsEnable()
        {
            RequireConductorAndHapp();
            await WithInstalledApp(async (admin, appId, installed) =>
            {
                var app = (HoloNETClientAppBase)installed.HoloNETClientAppAgent;

                var conductorState = await Step("dump_conductor_state", admin.DumpConductorStateAsync());
                Assert.False(conductorState.IsError, "dump_conductor_state: " + conductorState.Message);
                Assert.False(string.IsNullOrEmpty(conductorState.DumpedStateJSON));

                AgentInfoReturnedCallBackEventArgs infos = null;
                for (int attempt = 0; attempt < 15 && (infos == null || infos.AgentInfos.Count == 0); attempt++)
                {
                    infos = await Step("app agent_info", app.GetAgentInfoAsync(new[] { installed.CellId[0] }));
                    Assert.False(infos.IsError, "app agent_info: " + infos.Message);
                    if (infos.AgentInfos.Count == 0) await Task.Delay(1000);
                }
                Assert.NotEmpty(infos.AgentInfos);

                var metrics = await Step("app dump_network_metrics", app.DumpNetworkMetricsAsync(installed.CellId[0], true));
                Assert.False(metrics.IsError, "app dump_network_metrics: " + metrics.Message);
                Assert.False(string.IsNullOrEmpty(metrics.NetworkMetricsDumpJSON));

                var stats = await Step("app dump_network_stats", app.DumpNetworkStatsAsync());
                Assert.False(stats.IsError, "app dump_network_stats: " + stats.Message);
                Assert.False(string.IsNullOrEmpty(stats.NetworkStatsDumpJSON));

                var adminMetrics = await Step("admin dump_network_metrics(dna)", admin.DumpNetworkMetricsAsync(installed.CellId[0], true));
                Assert.False(adminMetrics.IsError, "admin dump_network_metrics(dna): " + adminMetrics.Message);

                // The app is already enabled; enabling it again through its own interface must succeed.
                var enabled = await Step("app enable_app", app.EnableAppAsync());
                Assert.False(enabled.IsError, "app enable_app: " + enabled.Message);

                var dnaDef = await Step("get_dna_definition", admin.GetDnaDefinitionAsync(installed.CellId));
                Assert.False(dnaDef.IsError, dnaDef.Message);
                Assert.Contains(dnaDef.DnaDefinition.CoordinatorZomes, z => z.Dependencies.Count > 0 && z.wasm_hash != null);

                var fullState = await Step("dump_full_state", admin.DumpFullStateAsync(installed.CellId));
                Assert.False(fullState.IsError, fullState.Message);
                Assert.True(fullState.DumpedState != null, "typed full state dump did not map: " + Snip(fullState.DumpedStateJSON, "\"source_chain_dump\"", 1200) + " ... " + Snip(fullState.DumpedStateJSON, "\"integration_dump\"", 900));
                Assert.NotEmpty(((FullStateDumpedResponse)fullState.DumpedState).source_chain_dump.records);

                // Paging (0.7): one DHT op per page, then the next page from the returned cursor.
                var page1 = await Step("dump_full_state limit 1", admin.DumpFullStateAsync(installed.CellId, (DhtOpsCursor)null, 1));
                Assert.False(page1.IsError, page1.Message);
                var dump1 = (FullStateDumpedResponse)page1.DumpedState;
                Assert.True(dump1.integration_dump.integrated.Length <= 1);
                Assert.NotNull(dump1.integration_dump.dht_ops_cursor);
                var page2 = await Step("dump_full_state page 2", admin.DumpFullStateAsync(installed.CellId, dump1.integration_dump.dht_ops_cursor, 1));
                Assert.False(page2.IsError, page2.Message);

                var chainPage = await Step("dump_state from seq 0, limit 2", admin.DumpStateAsync(installed.CellId, SourceChainCursor.Sequence(0), 2));
                Assert.False(chainPage.IsError, chainPage.Message);
                Assert.False(string.IsNullOrEmpty(chainPage.DumpedStateJSON));
            });
        }

        /// <summary>RetrieveAgentPubKeyAndDnaHashFromSandbox via `hc client call` (Holochain 0.7 removed `hc sandbox call`).</summary>
        [SkippableFact]
        public async Task Live_RetrieveAgentPubKeyAndDnaHash_FromHcClient()
        {
            RequireConductorAndHapp();
            string hc = Environment.GetEnvironmentVariable("HOLONET_LIVE_HC_BIN");
            Skip.If(string.IsNullOrEmpty(hc), "Set HOLONET_LIVE_HC_BIN to the hc.exe matching the conductor.");
            await WithInstalledApp(async (admin, appId, installed) =>
            {
                var app = (HoloNETClientAppBase)installed.HoloNETClientAppAgent;
                app.HoloNETDNA.InstalledAppId = appId;
                app.HoloNETDNA.HolochainConductorAdminURI = AdminUri;
                app.HoloNETDNA.FullPathToExternalHCToolBinary = hc;

                var keys = await Step("hc client list-apps", app.RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(true, false));
                Assert.NotNull(keys);
                Assert.Equal(app.ConvertHoloHashToString(installed.CellId[0]), keys.DnaHash);
                Assert.Equal(app.ConvertHoloHashToString(installed.CellId[1]), keys.AgentPubKey);

                // The retrieval flag is reset, so a second call works too.
                Assert.NotNull(await Step("again", app.RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(true, false)));

                // An app id hc doesn't know falls back to list-cells (any installed cell), run from the hApp folder.
                app.HoloNETDNA.InstalledAppId = "no-such-app";
                app.HoloNETDNA.FullPathToRootHappFolder = System.IO.Path.GetTempPath();
                var anyCell = await Step("hc client list-cells", app.RetrieveAgentPubKeyAndDnaHashFromSandboxAsync(false, false));
                Assert.NotNull(anyCell);
                Assert.StartsWith("uhC0k", anyCell.DnaHash);
            });
        }

        [SkippableFact]
        public async Task Live_DirectSignal_GrantThenSendBetweenTwoAgents()
        {
            RequireConductorAndHapp();
            await WithInstalledApp(async (admin, recipientId, recipient) =>
            {
                var recipientApp = (HoloNETClientAppBase)recipient.HoloNETClientAppAgent;
                var received = new TaskCompletionSource<SignalCallBackEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                recipientApp.OnSignalCallBack += (s, e) => received.TrySetResult(e);

                // 0.7.1+: the recipient must allow direct signals. 0.7.0 has no such request and must fail cleanly.
                var grant = await Step("grant_direct_signal_capability", recipientApp.GrantDirectSignalCapabilityAsync(recipient.CellId, "holonet-direct-signal"));
                bool v071 = !grant.IsError;
                if (v071)
                    Assert.Equal(39, grant.ActionHash.Length);
                else
                    Assert.False(string.IsNullOrEmpty(grant.Message));

                var adminGrant = await Step("admin grant_direct_signal_capability", admin.GrantDirectSignalCapabilityAsync(recipient.CellId, "holonet-direct-signal-admin"));
                Assert.Equal(v071, !adminGrant.IsError);

                await WithInstalledApp(async (admin2, senderId, sender) =>
                {
                    var senderApp = (HoloNETClientAppBase)sender.HoloNETClientAppAgent;
                    byte[] payload = { 1, 2, 3, 250 };
                    var sent = await Step("send_direct_signal", senderApp.SendDirectSignalAsync(sender.CellId[0], new[] { recipient.CellId[1] }, payload));
                    Assert.False(sent.IsError, "send_direct_signal: " + sent.Message);
                    Assert.Equal(HoloNETResponseType.AppDirectSignalSent, sent.HoloNETResponseType);

                    if (v071)
                    {
                        var done = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(30)));
                        Assert.True(done == received.Task, "recipient did not receive the direct signal");
                        Assert.False(received.Task.Result.IsError, "signal decode: " + received.Task.Result.Message);
                        Assert.True(received.Task.Result.IsDirectSignal);
                        Assert.Equal(senderApp.ConvertHoloHashToString(sender.CellId[1]), received.Task.Result.FromAgentPubKey);
                        Assert.Equal(payload, received.Task.Result.RawSignalData.Data);
                    }
                });
            });
        }

        [SkippableFact]
        public async Task Live_AddAdminInterfaces_OpensANewAdminPort()
        {
            RequireConductorAndHapp();
            // Let the OS choose a bindable port: random numeric ports can be in
            // Windows' excluded/reserved ranges even when no listener exists.
            var reservation = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            reservation.Start();
            ushort port;
            try { port = checked((ushort)((System.Net.IPEndPoint)reservation.LocalEndpoint).Port); }
            finally { reservation.Stop(); }
            var admin = NewAdmin();
            Assert.False((await admin.ConnectAsync(AdminUri)).IsError);
            try
            {
                var added = await Step("add_admin_interfaces", admin.AddAdminInterfacesAsync(new[] { port }));
                Assert.False(added.IsError, "add_admin_interfaces: " + added.Message);

                // The new port is a working admin interface.
                var second = NewAdmin();
                Assert.False((await second.ConnectAsync($"ws://localhost:{port}")).IsError);
                var key = await Step("generate_agent_pub_key on new port", second.GenerateAgentPubKeyAsync());
                Assert.False(key.IsError, "generate_agent_pub_key: " + key.Message);
                await second.DisconnectAsync();
            }
            finally
            {
                await admin.DisconnectAsync();
            }
        }

        [SkippableFact]
        public async Task Live_ListCapabilityGrants_ThenRevokeTheSigningGrant()
        {
            RequireConductorAndHapp();
            await WithInstalledApp(async (admin, appId, app) =>
            {
                var listed = await Step("list_capability_grants", admin.ListCapabilityGrantsAsync(appId));
                Assert.False(listed.IsError, "list_capability_grants: " + listed.Message);
                var cell = Assert.Single(listed.CapGrants);
                string seen = string.Join(", ", (cell.cap_grants ?? Array.Empty<CapGrantInfo>()).Select(g => $"{g.cap_grant?.tag ?? "<null>"}:{g.cap_grant?.AccessInfo?.access_type}"));
                Assert.True(cell.cap_grants?.Any(g => g.cap_grant?.tag == "zome-call-signing-key") == true, "grants seen: [" + seen + "]");
                var grant = cell.cap_grants.First(g => g.cap_grant?.tag == "zome-call-signing-key");
                Assert.Null(grant.revoked_at);

                var revoked = await Step("revoke_zome_call_capability", admin.RevokeZomeCallCapabilityAsync(grant.action_hash, cell.cell_id));
                Assert.False(revoked.IsError, "revoke_zome_call_capability: " + revoked.Message);

                var after = await Step("list_capability_grants (include revoked)", admin.ListCapabilityGrantsAsync(appId, includeRevoked: true));
                Assert.NotNull(after.CapGrants.Single().cap_grants.Single(g => g.cap_grant?.tag == "zome-call-signing-key").revoked_at);

                // With the grant revoked, calls signed with it are refused.
                try
                {
                    var call = await Step("zome call after revoke", app.HoloNETClientAppAgent.CallZomeFunctionAsync("oasis", "get_all_avatars", null));
                    Assert.True(call.IsError, "a call signed with a revoked grant should fail");
                }
                catch (HoloNETException) { /* HoloNET throws when nothing subscribes to OnError */ }
            });
        }

        /// <summary>
        /// Synchronous methods wait for the conductor and return its result (they used to return an empty placeholder
        /// straight away). Called directly on the test thread, which has a synchronisation context, so this also checks
        /// they don't deadlock.
        /// </summary>
        [SkippableFact]
        public async Task Live_SyncMethods_ReturnTheConductorsResult()
        {
            RequireConductorAndHapp();
            await WithInstalledApp((admin, appId, app) =>
            {
                var sync = NewAdmin();
                var connected = sync.Connect(AdminUri);
                Assert.True(connected.IsConnected, "sync Connect: " + connected.Message);

                var apps = sync.ListApps(AppStatusFilter.Enabled);
                Assert.False(apps.IsError, "sync ListApps: " + apps.Message);
                Assert.Contains(apps.Apps, a => a.installed_app_id == appId);

                var dnas = sync.ListDnas();
                Assert.False(dnas.IsError, "sync ListDnas: " + dnas.Message);
                Assert.NotEmpty(dnas.Dnas);

                sync.Disconnect();
                return Task.CompletedTask;
            });
        }
    }
}
