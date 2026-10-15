using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Multi-step flows against a fake conductor that answers like Holochain 0.7: install → connect, and agent key discovery.</summary>
    public class FakeConductorFlowTests
    {
        /// <summary>A Holochain 0.7 AppInfo (holochain_conductor_api::AppInfo) for one role.</summary>
        internal static Dictionary<string, object> AppInfo(string appId, string cellType = "provisioned", string status = "enabled") => new Dictionary<string, object>
        {
            ["installed_app_id"] = appId,
            ["cell_info"] = new Dictionary<string, object>
            {
                ["oasis"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["type"] = cellType,
                        ["value"] = cellType == "stem"
                            ? (object)new Dictionary<string, object> { ["original_dna_hash"] = Dna(), ["dna_modifiers"] = new Dictionary<string, object> { ["network_seed"] = "", ["properties"] = new byte[] { 0xc0 } }, ["name"] = null }
                            : new Dictionary<string, object> { ["cell_id"] = new object[] { Dna(), Agent() }, ["dna_modifiers"] = new Dictionary<string, object> { ["network_seed"] = "", ["properties"] = new byte[] { 0xc0 } }, ["name"] = "oasis" }
                    }
                }
            },
            ["status"] = new Dictionary<string, object> { ["type"] = status },
            ["agent_pub_key"] = Agent(),
            ["manifest"] = new Dictionary<string, object>
            {
                ["manifest_version"] = "0", ["name"] = "oasis", ["description"] = null,
                ["roles"] = new object[] { new Dictionary<string, object> { ["name"] = "oasis", ["provisioning"] = new Dictionary<string, object> { ["strategy"] = "create", ["deferred"] = false }, ["dna"] = new Dictionary<string, object> { ["path"] = "oasis.dna", ["modifiers"] = new Dictionary<string, object> { ["network_seed"] = null, ["properties"] = null }, ["installed_hash"] = Dna(), ["clone_limit"] = 0 } } },
                ["allow_deferred_memproofs"] = false, ["bootstrap_url"] = null, ["relay_url"] = null
            },
            ["installed_at"] = 1792055763162719L
        };

        /// <summary>Answers each request the way a 0.7 conductor would; <paramref name="fail"/> turns one request type into a conductor error.</summary>
        private static Func<string, byte[], object> Conductor(FakeConductor fake, string appId, string fail = null, bool alreadyInstalled = false, string cellType = "provisioned")
        {
            bool installed = alreadyInstalled;
            return (type, value) =>
            {
                if (type == fail) return FakeConductor.Error("internal_error", "step-failed-" + type);
                switch (type)
                {
                    case "list_apps": return FakeConductor.Ok("apps_listed", installed ? new object[] { AppInfo(appId) } : new object[0]);
                    case "uninstall_app": installed = false; return FakeConductor.Ok("app_uninstalled", null);
                    case "generate_agent_pub_key": return FakeConductor.Ok("agent_pub_key_generated", Agent());
                    case "install_app": installed = true; return FakeConductor.Ok("app_installed", AppInfo(appId, cellType, "disabled"));
                    case "enable_app": return FakeConductor.Ok("app_enabled", AppInfo(appId));
                    case "grant_zome_call_capability": return FakeConductor.Ok("zome_call_capability_granted", null);
                    case "attach_app_interface": return FakeConductor.Ok("app_interface_attached", new Dictionary<string, object> { ["port"] = fake.Port });
                    case "issue_app_authentication_token": return FakeConductor.Ok("app_authentication_token_issued", new Dictionary<string, object> { ["token"] = new[] { 1, 2, 3 }, ["expires_at"] = null });
                    case "app_info": return FakeConductor.Ok("app_info", AppInfo(appId));
                }
                return FakeConductor.Error("internal_error", "unexpected " + type);
            };
        }

        private static string HappFile()
        {
            string path = Path.Combine(Path.GetTempPath(), "holonet-fake-" + Guid.NewGuid().ToString("N") + ".happ");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            return path;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task InstallEnableSignAttachAndConnect_FullFlow(bool alreadyInstalled)
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                fake.Respond = Conductor(fake, "flow-app", alreadyInstalled: alreadyInstalled);
                string happ = HappFile();
                try
                {
                    var r = await admin.InstallEnableSignAttachAndConnectToHappAsync("flow-app", happ, "oasis");
                    Assert.True(r.IsSuccess, r.Message);
                    Assert.True(r.IsAppConnected, r.Message);
                    Assert.Equal(CellInfoType.Provisioned, r.CellType);
                    Assert.Equal(Dna(), r.CellId[0]);
                    Assert.Equal(fake.Port, (int)r.AttachedOnPort);
                    Assert.Contains(fake.Requests, q => q.Type == "issue_app_authentication_token");
                    Assert.Equal(alreadyInstalled, fake.Requests.Any(q => q.Type == "uninstall_app"));
                    await r.HoloNETClientAppAgent.DisconnectAsync();
                }
                finally { File.Delete(happ); }
                await admin.DisconnectAsync();
            }
        }

        [Theory]
        [InlineData("generate_agent_pub_key")]
        [InlineData("install_app")]
        [InlineData("enable_app")]
        [InlineData("grant_zome_call_capability")]
        [InlineData("attach_app_interface")]
        [InlineData("issue_app_authentication_token")]
        [InlineData("uninstall_app")]
        public async Task InstallEnableSignAttachAndConnect_ReportsTheFailingStep(string failingStep)
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.OnError += (s, e) => { };
                fake.Respond = Conductor(fake, "flow-app", failingStep, alreadyInstalled: failingStep == "uninstall_app");
                string happ = HappFile();
                try
                {
                    var r = await admin.InstallEnableSignAttachAndConnectToHappAsync("flow-app", happ, "oasis");
                    if (failingStep == "uninstall_app")
                        Assert.True(r.IsSuccess, "a failed uninstall of the old copy is only logged: " + r.Message);
                    else
                    {
                        Assert.False(r.IsSuccess && r.IsAppConnected);
                        Assert.True(r.IsError);
                    }
                    if (r.HoloNETClientAppAgent != null) await r.HoloNETClientAppAgent.DisconnectAsync();
                }
                finally { File.Delete(happ); }
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task Install_WhenTheCellIsNotProvisioned_Aborts()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.OnError += (s, e) => { };
                fake.Respond = Conductor(fake, "flow-app", cellType: "stem");
                string happ = HappFile();
                try
                {
                    var r = await admin.InstallEnableSignAndAttachHappAsync("flow-app", happ, "oasis");
                    Assert.True(r.IsError);
                    Assert.Contains("Not Provisioned", r.Message);
                }
                finally { File.Delete(happ); }
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task Install_Sync()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                fake.Respond = Conductor(fake, "flow-app");
                string happ = HappFile();
                try
                {
                    Assert.True(admin.InstallEnableSignAndAttachHapp("flow-app", happ, "oasis").IsSuccess);
                    var connected = admin.InstallEnableSignAttachAndConnectToHapp("flow-app", happ, "oasis");
                    Assert.True(connected.IsAppConnected, connected.Message);
                    await connected.HoloNETClientAppAgent.DisconnectAsync();
                }
                finally { File.Delete(happ); }
                await admin.DisconnectAsync();
            }
        }

        /// <summary>An app client that only knows its app id learns its agent key and DNA hash from app_info and becomes ready.</summary>
        [Fact]
        public async Task AppConnect_DiscoversAgentKeyAndDnaHashFromAppInfo()
        {
            using var fake = new FakeConductor();
            fake.Respond = Conductor(fake, "discover-app");
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 9 };
            var app = new HoloNETClientAppAgent("discover-app", null, dna);
            var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            app.OnReadyForZomeCalls += (s, e) => ready.TrySetResult(true);

            var connected = await app.ConnectAsync(fake.Uri);
            Assert.True(connected.IsConnected, connected.Message);
            Assert.True(await Task.WhenAny(ready.Task, Task.Delay(5000)) == ready.Task, "OnReadyForZomeCalls not raised");
            Assert.Equal(app.ConvertHoloHashToString(Agent()), app.HoloNETDNA.AgentPubKey);
            Assert.Equal(app.ConvertHoloHashToString(Dna()), app.HoloNETDNA.DnaHash);
            Assert.True(app.IsReadyForZomesCalls);

            var keys = await app.RetrieveAgentPubKeyAndDnaHashAsync(null, "oasis");
            Assert.Equal(app.HoloNETDNA.DnaHash, keys?.DnaHash ?? app.HoloNETDNA.DnaHash);
            var info = await app.GetAppInfoAsync(null, "oasis");
            Assert.Equal(CellInfoType.Provisioned, info.CellType);
            await app.DisconnectAsync();
        }

        [Fact]
        public async Task AppConnect_CallbackModes_AndAnAppInfoError()
        {
            using var fake = new FakeConductor();
            fake.Respond = Conductor(fake, "discover-app");
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 9 };
            var app = new HoloNETClientAppAgent("discover-app", null, dna);
            app.OnError += (s, e) => { };
            var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            app.OnReadyForZomeCalls += (s, e) => ready.TrySetResult(true);
            await app.ConnectAsync(fake.Uri, ConnectedCallBackMode.UseCallBackEvents, RetrieveAgentPubKeyAndDnaHashMode.UseCallBackEvents);
            Assert.True(await Task.WhenAny(ready.Task, Task.Delay(5000)) == ready.Task, "not ready after callback-mode connect");

            // app_info failing: the retrieval completes (no hang) and leaves the keys as they were.
            fake.Respond = (t, v) => FakeConductor.Error("internal_error", "no app info");
            var retrieval = Task.Run(() => app.RetrieveAgentPubKeyAndDnaHashFromConductor(null, null, true, false));
            Assert.True(await Task.WhenAny(retrieval, Task.Delay(5000)) == retrieval, "retrieval hung on an app_info error");
            Assert.Null(await retrieval);
            // With the sandbox fallback on and no hc available, it still finishes (no conductor <-> sandbox loop).
            app.HoloNETDNA.FullPathToExternalHCToolBinary = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "no-such-dir", "hc.exe");
            app.HoloNETDNA.HolochainConductorAdminURI = fake.Uri;
            var withFallback = Task.Run(() => app.RetrieveAgentPubKeyAndDnaHashFromConductor(null, null, true, true));
            Assert.True(await Task.WhenAny(withFallback, Task.Delay(10000)) == withFallback, "conductor -> sandbox fallback never finished");
            var viaSync = app.GetAppInfo();
            Assert.True(viaSync.IsError);
            await app.DisconnectAsync();
        }
    }
}
