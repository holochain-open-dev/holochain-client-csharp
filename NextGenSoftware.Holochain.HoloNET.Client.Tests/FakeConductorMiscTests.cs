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
    /// <summary>Exposes protected helpers for testing.</summary>
    public class TestableAdmin : HoloNETClientAdmin
    {
        public TestableAdmin(HoloNETDNA dna) : base(dna) { }
        public T Copy<T>(T source, T target) where T : HoloNETDataReceivedBaseEventArgs => CopyHoloNETArgs(source, target);
    }

    /// <summary>Fire-and-forget (void) methods, signing-credential edge cases and response-id matching, against <see cref="FakeConductor"/>.</summary>
    public class FakeConductorMiscTests
    {
        private static async Task WaitFor(Func<bool> condition, string what)
        {
            for (int i = 0; i < 100 && !condition(); i++) await Task.Delay(50);
            Assert.True(condition(), what);
        }

        [Fact]
        public async Task VoidMethods_SendTheirRequests()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                fake.Respond = (t, v) => t == "generate_agent_pub_key" ? FakeConductor.Ok("agent_pub_key_generated", Agent()) : FakeConductor.Ok("app_installed", FakeConductorFlowTests.AppInfo("void-app"));

                admin.GenerateAgentPubKey();
                await WaitFor(() => fake.Requests.Any(r => r.Type == "generate_agent_pub_key"), "GenerateAgentPubKey sent nothing");

                admin.InstallApp(admin.ConvertHoloHashToString(Agent()), "void-app", @"C:\some.happ");
                await WaitFor(() => fake.Requests.Any(r => r.Type == "install_app"), "InstallApp sent nothing");

                // Without an agent key the void InstallApp generates one first.
                admin.HoloNETDNA.AgentPubKey = null;
                admin.InstallApp(null, "void-app-2", @"C:\some.happ");
                await WaitFor(() => fake.Requests.Count(r => r.Type == "generate_agent_pub_key") >= 2, "InstallApp did not generate an agent key");

                var raw = new HoloNETData { type = "list_dnas", data = null };
                admin.SendHoloNETRequest(raw, HoloNETRequestType.AdminListDnas);
                await WaitFor(() => fake.Requests.Any(r => r.Type == "list_dnas"), "SendHoloNETRequest(HoloNETData) sent nothing");

                admin.SendHoloNETRequest(MessagePackSerializer.Serialize(new Dictionary<string, object> { ["type"] = "list_cell_ids" }), HoloNETRequestType.AdminListCellIds);
                await WaitFor(() => fake.Requests.Any(r => r.Type == "list_cell_ids"), "SendHoloNETRequest(bytes) sent nothing");
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task SigningCredentials_EdgeCases()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.OnError += (s, e) => { };
                fake.Respond = (t, v) => FakeConductor.Ok("zome_call_capability_granted", null);
                var cell = new[] { Dna(77), Agent(78) };

                Assert.Null(admin.GetCapGrantSecret(cell));
                Assert.True((await admin.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync((byte[][])null, CapGrantAccessType.Unrestricted, GrantedFunctionsType.All, null)).IsError);
                Assert.True((await admin.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(new[] { Dna(77) }, CapGrantAccessType.Unrestricted, GrantedFunctionsType.All, null)).IsError);
                Assert.True((await admin.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(cell, CapGrantAccessType.Assigned, GrantedFunctionsType.Listed, null)).IsError);

                foreach (var access in new[] { CapGrantAccessType.Unrestricted, CapGrantAccessType.Transferable, CapGrantAccessType.Assigned })
                {
                    var granted = await admin.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(cell, access, GrantedFunctionsType.Listed, new List<(string, string)> { ("oasis", "create_avatar") });
                    Assert.False(granted.IsError, access + ": " + granted.Message);
                }
                Assert.Equal(64, admin.GetCapGrantSecret(cell).Length);
                Assert.Equal(64, admin.GetCapGrantSecret(admin.ConvertHoloHashToString(cell[1]), admin.ConvertHoloHashToString(cell[0])).Length);
                await admin.DisconnectAsync();
            }
        }

        [Theory]
        [InlineData(EnforceRequestToResponseIdMatchingBehaviour.Ignore)]
        [InlineData(EnforceRequestToResponseIdMatchingBehaviour.Warn)]
        [InlineData(EnforceRequestToResponseIdMatchingBehaviour.Error)]
        public async Task ResponseWithAnUnknownId_FollowsTheMatchingBehaviour(EnforceRequestToResponseIdMatchingBehaviour behaviour)
        {
            using var fake = new FakeConductor();
            var dna = Dna(fake.Uri);
            dna.EnforceRequestToResponseIdMatchingBehaviour = behaviour;
            var admin = new HoloNETClientAdmin(dna);
            admin.OnError += (s, e) => { };
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            var received = new TaskCompletionSource<HoloNETDataReceivedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
            admin.OnDataReceived += (s, e) => received.TrySetResult(e);

            await fake.SendRawAsync(MessagePackSerializer.Serialize(new Dictionary<string, object>
            {
                ["id"] = 999999UL, ["type"] = "response",
                ["data"] = MessagePackSerializer.Serialize(FakeConductor.Ok("dnas_listed", new object[0]))
            }));
            Assert.True(await Task.WhenAny(received.Task, Task.Delay(5000)) == received.Task, "unsolicited response not surfaced");
            var e = received.Task.Result;
            Assert.Equal(behaviour == EnforceRequestToResponseIdMatchingBehaviour.Error, e.IsError);
            if (behaviour != EnforceRequestToResponseIdMatchingBehaviour.Ignore)
                Assert.Contains("does not match any pending request", e.Message);
            await admin.DisconnectAsync();
        }

        [Fact]
        public async Task GarbageFrame_IsReportedNotFatal()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            var errors = new List<string>();
            admin.OnError += (s, e) => { lock (errors) errors.Add(e.Reason); };
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            await fake.SendRawAsync(new byte[] { 0xc1, 0xc1, 0xc1 });
            await WaitFor(() => { lock (errors) return errors.Count > 0; }, "garbage frame not reported");
            // The connection still works afterwards.
            fake.Respond = (t, v) => FakeConductor.Ok("dnas_listed", new object[0]);
            Assert.False((await admin.ListDnasAsync()).IsError);
            await admin.DisconnectAsync();
        }

        [Fact]
        public void CopyHoloNETArgs_CopiesEveryField()
        {
            var admin = new TestableAdmin(Dna("ws://127.0.0.1:1"));
            var source = new AppsListedCallBackEventArgs { EndPoint = new Uri("ws://x:1"), Id = "5", IsError = true, Message = "m", RawBinaryData = new byte[] { 1 }, RawBinaryDataDecoded = "d", RawBinaryDataAsString = "s", RawJSONData = "{}", Exception = new Exception("e") };
            var target = admin.Copy(source, new AppsListedCallBackEventArgs());
            Assert.Equal("5", target.Id);
            Assert.True(target.IsError);
            Assert.Equal("m", target.Message);
            Assert.Equal("{}", target.RawJSONData);
            Assert.Same(source.Exception, target.Exception);
        }

        [Fact]
        public async Task Connect_WhileAlreadyConnecting_IsAWarning()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            admin.IsConnecting = true;
            var r = await admin.ConnectAsync(fake.Uri);
            Assert.True(r.IsWarning);
            Assert.False(r.IsConnected);
            admin.IsConnecting = false;
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            admin.IsDisconnecting = true;
            Assert.True((await admin.DisconnectAsync()).IsWarning);
            admin.IsDisconnecting = false;
            await admin.DisconnectAsync();
        }

        [Fact]
        public async Task AppsListed_WithABadAppInfo_IsAnError()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.OnError += (s, e) => { };
                var bad = FakeConductorFlowTests.AppInfo("bad");
                bad.Remove("agent_pub_key");
                fake.Respond = (t, v) => FakeConductor.Ok("apps_listed", new object[] { FakeConductorFlowTests.AppInfo("good"), bad });
                var r = await admin.ListAppsAsync(AppStatusFilter.All);
                Assert.True(r.IsError);
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task InstallFlow_UsesTheGivenLoggingFunction()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.OnError += (s, e) => { };
                fake.Respond = (t, v) => FakeConductor.Error("internal_error", "nope");
                var logged = new List<string>();
                string happ = System.IO.Path.GetTempFileName();
                try
                {
                    var r = await admin.InstallEnableSignAndAttachHappAsync("x", happ, "oasis", loggingFunction: (m, t) => logged.Add(m));
                    Assert.True(r.IsError);
                    Assert.NotEmpty(logged);
                }
                finally { System.IO.File.Delete(happ); }
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public void UnhandledExceptionHandler_ReportsWithoutThrowing_AndIsRemovedOnDispose()
        {
            var admin = new TestableAdmin(Dna("ws://127.0.0.1:1"));
            string reported = null;
            admin.OnError += (s, e) => reported = e.Reason;
            var handler = typeof(HoloNETClientBase).GetMethod("CurrentDomain_UnhandledException", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            handler.Invoke(admin, new object[] { this, new UnhandledExceptionEventArgs(new Exception("boom"), false) });
            Assert.Contains("boom", reported);
            admin.Dispose();
        }

        [Fact]
        public void LegacyAppInfoStatus_Struct()
        {
            Assert.Equal(AppInfoStatusEnum.Paused, new Data.App.Responses.Objects.AppInfoStatus { paused = 1 }.Status);
            Assert.Equal(AppInfoStatusEnum.Disabled, new Data.App.Responses.Objects.AppInfoStatus { disabled = 1 }.Status);
            Assert.Equal(AppInfoStatusEnum.Running, new Data.App.Responses.Objects.AppInfoStatus { running = 1 }.Status);
            Assert.Equal(AppInfoStatusEnum.None, new Data.App.Responses.Objects.AppInfoStatus().Status);
        }
    }
}
