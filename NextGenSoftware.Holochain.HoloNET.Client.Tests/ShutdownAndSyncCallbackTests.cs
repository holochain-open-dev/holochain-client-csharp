using System;
using System.Threading.Tasks;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Shutting down an already disconnected client, and sync calls made from inside HoloNET events.</summary>
    public class ShutdownAndSyncCallbackTests
    {
        [Fact]
        public async Task ShutdownAfterDisconnect_Completes()
        {
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            await app.DisconnectAsync();

            // Used to wait forever for an OnDisconnected that had already happened.
            var shutdown = app.ShutdownHoloNETAsync();
            Assert.True(await Task.WhenAny(shutdown, Task.Delay(5000)) == shutdown, "ShutdownHoloNETAsync never completed");
            Assert.NotNull(await shutdown);
            Assert.NotNull(app.ShutdownHoloNET());
        }

        [Fact]
        public async Task SyncCallFromInsideAnEvent_DoesNotDeadlock()
        {
            using var fake = new FakeConductor();
            fake.Respond = (t, v) => t == "list_apps" ? FakeConductor.Ok("apps_listed", new object[0]) : FakeConductor.Ok("agent_pub_key_generated", Agent());
            var (_, admin) = (fake, new HoloNETClientAdmin(Dna(fake.Uri)));
            var listed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            admin.OnAppsListedCallBack += (s, e) => listed.TrySetResult(true);

            // The HoloNET Manager pattern: chain the next request from inside the previous one's event with a sync call.
            admin.OnAgentPubKeyGeneratedCallBack += (s, e) => admin.ListApps(AppStatusFilter.All);
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            admin.GenerateAgentPubKey();

            Assert.True(await Task.WhenAny(listed.Task, Task.Delay(5000)) == listed.Task, "sync call inside the event deadlocked");
            // Outside an event the sync call still waits for (and returns) the conductor's reply.
            Assert.NotNull(admin.ListApps(AppStatusFilter.All).Apps);
            await admin.DisconnectAsync();
        }
    }
}
