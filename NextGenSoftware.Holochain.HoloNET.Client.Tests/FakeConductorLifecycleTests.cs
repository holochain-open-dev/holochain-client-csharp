using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Connect / disconnect / dispose behaviour and the error-handling modes, against <see cref="FakeConductor"/>.</summary>
    public class FakeConductorLifecycleTests
    {
        [Fact]
        public async Task Connect_WithoutAUri_ReportsAnError()
        {
            var admin = new HoloNETClientAdmin(Dna("ws://127.0.0.1:1"));
            admin.EndPoint = null;
            bool raised = false;
            admin.OnError += (s, e) => raised = true;
            var result = await admin.ConnectAsync("");
            Assert.True(result.IsError);
            Assert.True(raised);
        }

        [Fact]
        public async Task Connect_ToNothing_ReportsAnError()
        {
            var admin = new HoloNETClientAdmin(Dna("ws://127.0.0.1:1"));
            admin.OnError += (s, e) => { };
            var result = await admin.ConnectAsync("ws://127.0.0.1:1");
            Assert.False(result.IsConnected);
            Assert.True(result.IsError, "a refused connection must be reported as an error");
        }

        [Fact]
        public async Task Request_WhileDisconnected_FailsInsteadOfHanging()
        {
            var admin = new HoloNETClientAdmin(Dna("ws://127.0.0.1:1"));
            admin.OnError += (s, e) => { };
            var call = admin.ListAppsAsync(AppStatusFilter.All);
            Assert.True(await Task.WhenAny(call, Task.Delay(5000)) == call, "a request made while disconnected never completed");
            Assert.True((await call).IsError);
            var fireAndForget = await admin.ListAppsAsync(AppStatusFilter.All, ConductorResponseCallBackMode.UseCallBackEvents);
            Assert.True(fireAndForget.IsError);
        }

        [Fact]
        public async Task Request_WithANonNumericId_FailsClearly()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            var r = await admin.ListAppsAsync(AppStatusFilter.All, ConductorResponseCallBackMode.WaitForHolochainConductorResponse, "not-a-number");
            Assert.True(r.IsError);
            Assert.Contains("request id", r.Message);
            await admin.DisconnectAsync();
        }

        [Fact]
        public async Task Connect_UseCallBackEvents_ReturnsAtOnceThenRaisesOnConnected()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            var connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            admin.OnConnected += (s, e) => connected.TrySetResult(true);
            var result = await admin.ConnectAsync(fake.Uri, ConnectedCallBackMode.UseCallBackEvents);
            Assert.True(result.IsWarning);
            Assert.True(await Task.WhenAny(connected.Task, Task.Delay(5000)) == connected.Task, "OnConnected not raised");

            // A second connect while open is a no-op, not an error.
            Assert.False((await admin.ConnectAsync(fake.Uri)).IsError);
            await admin.DisconnectAsync();
        }

        [Fact]
        public async Task Connect_Sync_Disconnect_Sync()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            Assert.True(admin.Connect(fake.Uri).IsConnected);
            Assert.True(admin.Disconnect().IsDisconnected);
        }

        [Fact]
        public async Task Disconnect_WhenNotConnected_IsAWarning()
        {
            var admin = new HoloNETClientAdmin(Dna("ws://127.0.0.1:1"));
            var result = await admin.DisconnectAsync();
            Assert.True(result.IsWarning);
            Assert.False(result.IsError);
        }

        [Fact]
        public async Task Disconnect_UseCallBackEvents_RaisesOnDisconnected()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            var disconnected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            admin.OnDisconnected += (s, e) => disconnected.TrySetResult(true);
            await admin.DisconnectAsync(DisconnectedCallBackMode.UseCallBackEvents);
            Assert.True(await Task.WhenAny(disconnected.Task, Task.Delay(5000)) == disconnected.Task, "OnDisconnected not raised");
        }

        [Fact]
        public async Task ConductorDropsTheConnection_PendingCallsFailInsteadOfHanging()
        {
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            app.OnError += (s, e) => { };
            fake.Respond = (type, value) => null; // never answer
            var pending = app.ListWasmHostFunctionsAsync();
            await Task.Delay(200);
            await fake.DropClientsAsync();
            var done = await Task.WhenAny(pending, Task.Delay(10000));
            Assert.True(done == pending, "a call in flight when the connection dropped never completed");
            Assert.True((await pending).IsError);
        }

        /// <summary>A reply of a type the client doesn't route to the waiting call (e.g. renamed in a future Holochain) must fail it, not hang.</summary>
        [Fact]
        public async Task ReplyOfAnUnexpectedType_FailsTheCall()
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.OnError += (s, e) => { };
                fake.Respond = (t, v) => FakeConductor.Ok("some_future_response_type", new Dictionary<string, object>());
                var call = admin.GenerateAgentPubKeyAsync();
                Assert.True(await Task.WhenAny(call, Task.Delay(5000)) == call, "an unexpected reply left the call hanging");
                Assert.True((await call).IsError);
                Assert.Contains("unexpected", (await call).Message);

                fake.Respond = (t, v) => FakeConductor.Ok("full_state_dumped", new Dictionary<string, object>()); // a known type, but not this request's
                var mismatched = admin.ListDnasAsync();
                Assert.True(await Task.WhenAny(mismatched, Task.Delay(5000)) == mismatched, "a mismatched reply left the call hanging");
                Assert.True((await mismatched).IsError);
                await admin.DisconnectAsync();
            }
        }

        [Fact]
        public async Task ShutdownHoloNET_DisconnectsAndRaisesShutdownComplete()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            bool done = false;
            admin.OnHoloNETShutdownComplete += (s, e) => done = true;
            var result = await admin.ShutdownHoloNETAsync(DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode.DoNotShutdownAnyConductors);
            Assert.NotNull(result);
            Assert.True(done);
        }

        [Fact]
        public async Task ShutdownHoloNET_Sync()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            Assert.NotNull(admin.ShutdownHoloNET(ShutdownHolochainConductorsMode.DoNotShutdownAnyConductors));
        }

        [Fact]
        public async Task Dispose_IsIdempotent()
        {
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(Dna(fake.Uri));
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            admin.Dispose();
            admin.Dispose();
        }

        [Fact]
        public void GetCellId_FromDnaAndAgent_AndMissingValues()
        {
            var admin = new HoloNETClientAdmin(Dna("ws://127.0.0.1:1"));
            admin.OnError += (s, e) => { };
            Assert.Null(admin.GetCellId());
            admin.HoloNETDNA.AgentPubKey = admin.ConvertHoloHashToString(Agent());
            Assert.Null(admin.GetCellId());
            admin.HoloNETDNA.DnaHash = admin.ConvertHoloHashToString(Dna());
            var cell = admin.GetCellId();
            Assert.Equal(Dna(), cell[0]);
            Assert.Equal(Agent(), cell[1]);
            Assert.Equal(Dna(), admin.GetCellId(Dna(), Agent())[0]);
            Assert.Equal(Agent(), admin.GetCellId(admin.HoloNETDNA.DnaHash, admin.HoloNETDNA.AgentPubKey)[1]);
        }

        [Fact]
        public async Task GetCellIdAsync_MissingValues()
        {
            var admin = new HoloNETClientAdmin(Dna("ws://127.0.0.1:1"));
            admin.OnError += (s, e) => { };
            Assert.Null(await admin.GetCellIdAsync());
            admin.HoloNETDNA.AgentPubKey = admin.ConvertHoloHashToString(Agent());
            Assert.Null(await admin.GetCellIdAsync());
            admin.HoloNETDNA.DnaHash = admin.ConvertHoloHashToString(Dna());
            Assert.NotNull(await admin.GetCellIdAsync());
        }

        [Theory]
        [InlineData(ErrorHandlingBehaviour.AlwaysThrowExceptionOnError, false, true)]
        [InlineData(ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent, false, true)]
        [InlineData(ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent, true, false)]
        [InlineData(ErrorHandlingBehaviour.NeverThrowExceptions, false, false)]
        public void ErrorHandlingBehaviour_ControlsThrowing(ErrorHandlingBehaviour behaviour, bool subscribe, bool expectThrow)
        {
            var dna = Dna("ws://127.0.0.1:1");
            dna.ErrorHandlingBehaviour = behaviour;
            var admin = new HoloNETClientAdmin(dna);
            bool raised = false;
            if (subscribe) admin.OnError += (s, e) => raised = true;
            // GetCellId with nothing set goes through HandleError.
            if (expectThrow) Assert.Throws<HoloNETException>(() => admin.GetCellId());
            else Assert.Null(admin.GetCellId());
            Assert.Equal(subscribe, raised);
        }

        [Fact]
        public void ClearCache_ForgetsCachedState()
        {
            var app = new HoloNETClientAppAgent("app", null, Dna("ws://127.0.0.1:1"));
            app.ClearCache();
            app.ClearCache(true);
        }

        [Fact]
        public async Task CachedZomeResults_AreReturnedFromTheCacheOnTheSecondCall()
        {
            var (adminFake, admin) = await ConnectAdmin();
            using (adminFake)
            {
                adminFake.Respond = (t, v) => FakeConductor.Ok("zome_call_capability_granted", null);
                await admin.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(new[] { Dna(), Agent() }, CapGrantAccessType.Unrestricted, Data.Admin.Requests.Objects.GrantedFunctionsType.All, null);
                await admin.DisconnectAsync();
            }
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            int calls = 0;
            fake.Respond = (t, v) => { calls++; return FakeConductor.Ok("zome_called", MessagePack.MessagePackSerializer.Serialize(FakeConductorZomeCallTests.Record(FakeConductorZomeCallTests.EntryFields()))); };
            var first = await app.CallZomeFunctionAsync("987654", "oasis", "fn", new { x = 1 }, true, true, (Type)null);
            var second = await app.CallZomeFunctionAsync("987654", "oasis", "fn", new { x = 1 }, true, true, (Type)null);
            Assert.False(first.IsError, first.Message);
            Assert.False(second.IsError, second.Message);
            Assert.Equal(1, calls);
            app.ClearCache();
            await app.DisconnectAsync();
        }
    }
}
