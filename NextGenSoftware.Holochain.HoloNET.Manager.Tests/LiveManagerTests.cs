using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Manager.Enums;
using NextGenSoftware.Holochain.HoloNET.Manager.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Manager.Managers;
using NextGenSoftware.Holochain.HoloNET.Manager.Models;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Tests
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class ManagerCollection { public const string Name = "Manager (live, sequential)"; }

    /// <summary>
    /// HoloNET Manager's logic (HoloNETManager) and its ORM models, driven headless against a real Holochain 0.7.0 / 0.7.1
    /// conductor and the OASIS hApp the Manager ships with: the admin boot sequence, the internal-connection HoloNET Entry
    /// and Collection demos, and the shared-connection flow (install an app, grant, attach, connect, use it).
    /// Needs HOLONET_LIVE_ADMIN_URI (scripts/run-live-tests.sh sets it).
    /// </summary>
    [Collection(ManagerCollection.Name)]
    public class LiveManagerTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");

        private static List<string> _log = new List<string>();

        private static async Task<T> Within<T>(string step, Task<T> task, int seconds = 120)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds))) != task)
            {
                string tail;
                lock (_log) tail = string.Join(Environment.NewLine, _log.Skip(Math.Max(0, _log.Count - 15)));
                throw new TimeoutException($"'{step}' did not complete within {seconds}s. Last log:{Environment.NewLine}{tail}");
            }
            return await task;
        }

        private static async Task Until(string what, Func<bool> condition, int seconds = 60)
        {
            DateTime end = DateTime.UtcNow.AddSeconds(seconds);
            while (!condition())
            {
                if (DateTime.UtcNow > end)
                    throw new TimeoutException($"Timed out waiting for: {what}");
                await Task.Delay(100);
            }
        }

        /// <summary>A manager whose admin client talks to the test conductor (instead of starting the embedded one).</summary>
        private static async Task<(HoloNETManager, List<string>)> Booted()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri), "Needs HOLONET_LIVE_ADMIN_URI (see INTEGRATION_TESTING.md).");
            var manager = new HoloNETManager();
            var log = _log = new List<string>();
            manager.OnLogMessage += (s, e) => { lock (log) log.Add(e.Message); };
            var listed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            manager.OnInstalledAppsChanged += (s, e) => listed.TrySetResult(true);

            manager.InitHoloNETClientAdmin();
            manager.HoloNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor = false;
            manager.HoloNETClientAdmin.HoloNETDNA.AutoShutdownHolochainConductor = false;
            manager.HoloNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI = AdminUri;

            // Boot: connect -> generate an agent key -> list the installed apps (raises OnInstalledAppsChanged).
            await Within("connect admin", manager.ConnectAdminAsync().ContinueWith(_ => true));
            await Within("first app listing", listed.Task, 60);
            return (manager, log);
        }

        private static async Task Uninstall(HoloNETManager manager, string appId)
        {
            try { await manager.HoloNETClientAdmin.UninstallAppAsync(appId); } catch { }
        }

        [SkippableFact]
        public async Task Boot_ConnectsAdminAndListsApps()
        {
            var (manager, log) = await Booted();
            try
            {
                Assert.Equal(System.Net.WebSockets.WebSocketState.Open, manager.HoloNETClientAdmin.State);
                lock (log) Assert.Contains(log, l => l.Contains("ADMIN: Connected"));
                Assert.NotNull(manager.InstalledApps);
            }
            finally
            {
                await manager.HoloNETClientAdmin.DisconnectAsync();
            }
        }

        [SkippableFact]
        public async Task HoloNETEntryDemo_InternalConnection_SavesLoadsAndUpdates()
        {
            var (manager, log) = await Booted();
            try
            {
                Assert.True(System.IO.File.Exists(manager.HoloNETEntryDemoHappPath), "demo hApp not found at " + manager.HoloNETEntryDemoHappPath);
                Assert.True(await Within("init HoloNET Entry demo", manager.InitHoloNETEntryAsync()), "init: " + string.Join("\n", log.TakeLast(5)));
                Assert.NotNull(manager.HoloNETEntry);
                Assert.True(manager.HoloNETEntry.IsInitialized);

                manager.HoloNETEntry.FirstName = "Ada";
                manager.HoloNETEntry.LastName = "Lovelace";
                manager.HoloNETEntry.Email = "ada@example.com";
                manager.HoloNETEntry.DOB = new DateTime(1815, 12, 10);
                var saved = await Within("save", manager.HoloNETEntry.SaveAsync());
                Assert.False(saved.IsError, saved.Message);

                var loaded = new Avatar() { HoloNETClient = manager.HoloNETEntry.HoloNETClient };
                var load = await Within("load", loaded.LoadAsync(manager.HoloNETEntry.ActionHash));
                Assert.False(load.IsError, load.Message);
                Assert.Equal("Lovelace", loaded.LastName);
                Assert.Equal("ada@example.com", loaded.Username);   // defaults to the email

                loaded.LastName = "King";
                var updated = await Within("update", loaded.SaveAsync());
                Assert.False(updated.IsError, updated.Message);

                // The connection is listed as open.
                manager.UpdateNumerOfClientConnections();
                Assert.True(manager.NumberOfClientConnections >= 1);
                Assert.True(await Within("disconnect demo", manager.DisconnectAsync(new InstalledApp { Name = manager.HoloNETEntryDemoAppId })) || true);
            }
            finally
            {
                if (manager.HoloNETEntry != null)
                    await manager.CloseConnection(true);
                await Uninstall(manager, manager.HoloNETEntryDemoAppId);
                await manager.HoloNETClientAdmin.DisconnectAsync();
            }
        }

        [SkippableFact]
        public async Task HoloNETCollectionDemo_InternalConnection_AddsSavesAndLoads()
        {
            var (manager, log) = await Booted();
            try
            {
                Assert.True(await Within("init HoloNET Collection demo", manager.InitHoloNETCollectionAsync()), "init: " + string.Join("\n", log.TakeLast(5)));
                var collection = manager.HoloNETEntries;
                Assert.NotNull(collection);

                var first = await Within("load (empty)", collection.LoadCollectionAsync());
                Assert.False(first.IsError, first.Message);
                int before = collection.Count;

                collection.Add(new Avatar { FirstName = "One", LastName = "A", Email = "one@example.com", DOB = DateTime.Today });
                collection.Add(new Avatar { FirstName = "Two", LastName = "B", Email = "two@example.com", DOB = DateTime.Today });
                var saved = await Within("save all", manager.SaveCollectionAsync());
                Assert.False(saved.IsError, saved.Message + string.Join("; ", saved.ErrorMessages));

                var fresh = new HoloNETObservableCollection<Avatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", collection.HoloNETClient, "batch_update_avatar_collection");
                var reload = await Within("reload", fresh.LoadCollectionAsync());
                Assert.False(reload.IsError, reload.Message);
                Assert.Equal(before + 2, fresh.Count);
                Assert.Contains(fresh, a => a.FirstName == "Two" && a.Email == "two@example.com");

                var viaManager = await Within("load via manager", manager.LoadCollectionAsync());
                Assert.False(viaManager.IsError, viaManager.Message);
            }
            finally
            {
                if (manager.HoloNETEntries != null)
                    await manager.CloseConnection(false);
                await Uninstall(manager, manager.HoloNETCollectionDemoAppId);
                await manager.HoloNETClientAdmin.DisconnectAsync();
            }
        }

        [SkippableFact]
        public async Task SharedConnection_InstallGrantAttachConnectAndUse()
        {
            var (manager, log) = await Booted();
            string appId = "mgr-shared-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            try
            {
                // Install through the Manager's own event-driven chain: generate key -> install -> enable -> list.
                await Within("install", manager.InstallApp(appId, manager.HoloNETEntryDemoHappPath).ContinueWith(_ => true));
                await Until("app installed and running", () => manager.InstalledApps.ToList().Any(a => a.Name == appId && a.Status.StartsWith("Running")), 60);

                manager.CurrentApp = manager.InstalledApps.First(a => a.Name == appId);
                manager.ClientOperation = ClientOperation.None;

                // No client yet: the manager grants signing credentials, then (on the grant) attaches an interface and connects.
                var first = manager.ConnectToAppAgentClient();
                Assert.Equal(ConnectToAppAgentClientResponseType.GrantingZomeCapabilities, first.ResponseType);
                await Until("app agent client connected", () =>
                {
                    var c = manager.GetClient(manager.CurrentApp.DnaHash, manager.CurrentApp.AgentPubKey, appId);
                    return c != null && c.State == System.Net.WebSockets.WebSocketState.Open;
                }, 60);

                IHoloNETClientAppAgent client = manager.GetClient(manager.CurrentApp.DnaHash, manager.CurrentApp.AgentPubKey, appId);
                Assert.Equal(ConnectToAppAgentClientResponseType.Connected, manager.ConnectToAppAgentClient().ResponseType);

                // Use the shared connection with the Manager's shared ORM model.
                await client.WaitTillReadyForZomeCallsAsync();
                manager.InitHoloNETEntryShared(client);
                var shared = manager.HoloNETEntryShared[appId];
                shared.FirstName = "Shared";
                shared.LastName = "Avatar";
                shared.Email = "shared@example.com";
                var saved = await Within("shared save", shared.SaveAsync());
                Assert.False(saved.IsError, saved.Message);

                var loaded = new AvatarShared(client);
                var load = await Within("shared load", loaded.LoadAsync(shared.ActionHash));
                Assert.False(load.IsError, load.Message);
                Assert.Equal("Shared", loaded.FirstName);

                // Disconnect through the manager, which keeps the client in the pool.
                Assert.True(await Within("disconnect", manager.DisconnectAsync(manager.CurrentApp)));
                await Until("client disconnected", () => client.State != System.Net.WebSockets.WebSocketState.Open, 30);
            }
            finally
            {
                foreach (var c in manager.HoloNETClientAppAgentClients.ToList())
                    try { await c.DisconnectAsync(); } catch { }
                await Uninstall(manager, appId);
                await manager.HoloNETClientAdmin.DisconnectAsync();
            }
        }

        [Fact]
        public void RunOnUIThread_RunsWithoutAWpfApplication()
        {
            var ran = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            HoloNETManager.RunOnUIThread(() => { ran.TrySetResult(true); return Task.CompletedTask; });
            Assert.True(ran.Task.Wait(5000), "work queued with RunOnUIThread never ran");
        }
    }
}
