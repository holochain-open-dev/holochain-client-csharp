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

                var disabled = await Step("disable_app", admin.DisableAppAsync(appId));
                Assert.False(disabled.IsError, "disable_app: " + disabled.Message);
                var disabledApps = await admin.ListAppsAsync(AppStatusFilter.Disabled);
                Assert.Contains(disabledApps.Apps, a => a.installed_app_id == appId);

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

        [SkippableFact]
        public async Task Live_AddAdminInterfaces_OpensANewAdminPort()
        {
            RequireConductorAndHapp();
            ushort port = (ushort)new Random().Next(40000, 50000);
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
                string seen = string.Join(", ", (cell.cap_grants ?? Array.Empty<CapGrantInfo>()).Select(g => $"{g.cap_grant?.tag ?? "<null>"}:{g.cap_grant?.access?.GetType().Name}"));
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
    }
}
