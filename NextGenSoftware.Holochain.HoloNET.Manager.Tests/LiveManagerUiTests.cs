using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.Manager.Enums;
using NextGenSoftware.Holochain.HoloNET.Manager.Managers;
using NextGenSoftware.Holochain.HoloNET.Manager.Models;
using NextGenSoftware.Holochain.HoloNET.Manager.UserControls;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Tests
{
    /// <summary>
    /// A WPF Application on its own STA thread, as the Manager runs in the real app: its UI-thread work (RunOnUIThread) and
    /// the entry form (ucHoloNETEntry) both live on this dispatcher.
    /// </summary>
    internal static class Ui
    {
        private static Dispatcher _dispatcher;
        private static readonly object _lock = new object();

        private static Dispatcher Dispatcher
        {
            get
            {
                lock (_lock)
                {
                    if (_dispatcher == null)
                    {
                        var ready = new ManualResetEventSlim();
                        var thread = new Thread(() =>
                        {
                            new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                            _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                            ready.Set();
                            System.Windows.Threading.Dispatcher.Run();
                        });
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.IsBackground = true;
                        thread.Start();
                        ready.Wait();
                    }
                    return _dispatcher;
                }
            }
        }

        public static Task Run(Func<Task> body) => Dispatcher.InvokeAsync(body).Task.Unwrap();

        public static Task<T> Run<T>(Func<T> body) => Dispatcher.InvokeAsync(body).Task;
    }

    /// <summary>The Manager's form-driven operations (entry and collection, internal and shared connections) on the UI thread.</summary>
    [Collection(ManagerCollection.Name)]
    public class LiveManagerUiTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");

        private static async Task<T> Within<T>(string step, Task<T> task, int seconds = 120)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds))) != task)
                throw new TimeoutException($"'{step}' did not complete within {seconds}s.");
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

        private static async Task<HoloNETManager> Boot()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri), "Needs HOLONET_LIVE_ADMIN_URI (see INTEGRATION_TESTING.md).");
            var manager = new HoloNETManager();
            var listed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            manager.OnInstalledAppsChanged += (s, e) => listed.TrySetResult(true);
            manager.BootHoloNETManager();
            manager.HoloNETClientAdmin.HoloNETDNA.AutoStartHolochainConductor = false;
            manager.HoloNETClientAdmin.HoloNETDNA.AutoShutdownHolochainConductor = false;
            manager.HoloNETClientAdmin.HoloNETDNA.HolochainConductorAdminURI = AdminUri;
            manager.ShowDetailedLogMessages = true;
            await manager.ConnectAdminAsync();
            await Within("app listing", listed.Task, 60);
            return manager;
        }

        private static ucHoloNETEntry Form(string first, string last, string email) => new ucHoloNETEntry { FirstName = first, LastName = last, Email = email, DOB = "10/12/1815" };

        [SkippableFact]
        public async Task EntryDemo_SaveLoadDelete_FromTheForm()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri), "needs a live conductor");
            await Ui.Run(async () =>
            {
                var manager = await Boot();
                try
                {
                    HoloNETEntryUIManager.CurrentHoloNETEntryUI = Form("Ada", "Lovelace", "ada@example.com");
                    var saved = await Within("save from form", manager.SaveHoloNETEntryAsync());
                    Assert.False(saved.IsError, saved.Message);
                    Assert.Equal(manager.HoloNETEntry.ActionHash, HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarActionHash);

                    // Load again: the demo app is already initialised, so this goes through CheckIfDemoAppReadyAsync.
                    var loaded = await Within("load", manager.LoadHoloNETEntryAsync());
                    Assert.False(loaded.IsError, loaded.Message);
                    Assert.Equal("Lovelace", manager.HoloNETEntry.LastName);
                    Assert.NotEqual("", manager.GetEntryInfo(loaded));

                    // The demo app disabled behind the manager's back: it re-enables it.
                    await manager.HoloNETClientAdmin.DisableAppAsync(manager.HoloNETEntryDemoAppId);
                    Assert.False((await Within("load after disable", manager.LoadHoloNETEntryAsync())).IsError);

                    // The connection dropped: it reconnects.
                    await manager.HoloNETEntry.HoloNETClient.DisconnectAsync();
                    Assert.False((await Within("load after disconnect", manager.LoadHoloNETEntryAsync())).IsError);

                    var deleted = await Within("delete", manager.DeleteHoloNETEntryAsync());
                    Assert.False(deleted.IsError, deleted.Message);

                    // Uninstalled behind the manager's back: it closes and starts again.
                    await manager.HoloNETClientAdmin.UninstallAppAsync(manager.HoloNETEntryDemoAppId);
                    HoloNETEntryDNAManager.HoloNETEntryDNA.AvatarActionHash = null;
                    var afterUninstall = await Within("load after uninstall", manager.LoadHoloNETEntryAsync());
                    Assert.True(afterUninstall.IsWarning, afterUninstall.Message); // nothing saved for the new install yet
                }
                finally
                {
                    if (manager.HoloNETEntry != null)
                        await manager.CloseConnection(true);
                    await manager.HoloNETClientAdmin.UninstallAppAsync(manager.HoloNETEntryDemoAppId);
                    await manager.HoloNETClientAdmin.DisconnectAsync();
                    HoloNETEntryUIManager.CurrentHoloNETEntryUI = null;
                }
            });
        }

        [SkippableFact]
        public async Task CollectionDemo_ReinitialisesWhenItsAppChanges()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri), "needs a live conductor");
            await Ui.Run(async () =>
            {
                var manager = await Boot();
                try
                {
                    Assert.False((await Within("load", manager.LoadCollectionAsync())).IsError);
                    manager.HoloNETEntries.Add(new Avatar { FirstName = "C", LastName = "D", Email = "c@example.com" });
                    Assert.False((await Within("save", manager.SaveCollectionAsync())).IsError);

                    await manager.HoloNETClientAdmin.DisableAppAsync(manager.HoloNETCollectionDemoAppId);
                    Assert.False((await Within("load after disable", manager.LoadCollectionAsync())).IsError);
                    await manager.HoloNETEntries.HoloNETClient.DisconnectAsync();
                    Assert.False((await Within("load after disconnect", manager.LoadCollectionAsync())).IsError);
                    Assert.Contains(manager.HoloNETEntries, a => a.FirstName == "C");

                    await manager.HoloNETClientAdmin.UninstallAppAsync(manager.HoloNETCollectionDemoAppId);
                    var fresh = await Within("load after uninstall", manager.LoadCollectionAsync());
                    Assert.False(fresh.IsError, fresh.Message);
                    // (Not necessarily empty: collection links live on the DHT, shared by every install of the DNA.)
                }
                finally
                {
                    if (manager.HoloNETEntries != null)
                        await manager.CloseConnection(false);
                    await manager.HoloNETClientAdmin.UninstallAppAsync(manager.HoloNETCollectionDemoAppId);
                    await manager.HoloNETClientAdmin.DisconnectAsync();
                }
            });
        }

        [SkippableFact]
        public async Task SharedConnection_FormOperationsRunWhenTheClientConnects()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri), "needs a live conductor");
            await Ui.Run(async () =>
            {
                var manager = await Boot();
                string appId = "mgr-ui-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                try
                {
                    // As InitHoloNETEntryAsync does: the manager's own install/enable/grant handlers stand aside meanwhile.
                    manager.InitHoloNETEntryDemo = true;
                    var init = await Within("init app", manager.InitDemoApp(appId, manager.HoloNETEntryDemoHappPath));
                    manager.InitHoloNETEntryDemo = false;
                    Assert.True(init.IsSuccess, init.Message);
                    manager.ListHapps();
                    await Until("listed", () => manager.InstalledApps.Any(a => a.Name == appId && a.Status.StartsWith("Running")));
                    manager.CurrentApp = manager.InstalledApps.First(a => a.Name == appId);

                    // Ask for an operation before there's a client: it runs once the new client connects.
                    HoloNETEntryUIManager.CurrentHoloNETEntryUI = Form("Shared", "Form", "shared@example.com");
                    manager.ClientOperation = ClientOperation.SaveHoloNETEntryShared;
                    Assert.Equal(ConnectToAppAgentClientResponseType.GrantingZomeCapabilities, manager.ConnectToAppAgentClient().ResponseType);
                    await Until("shared save ran", () => manager.ClientOperation == ClientOperation.None && manager.HoloNETEntryShared.ContainsKey(appId) && !string.IsNullOrEmpty(manager.HoloNETEntryShared[appId].ActionHash), 60);
                    IHoloNETClientAppAgent client = manager.GetClient(manager.CurrentApp.DnaHash, manager.CurrentApp.AgentPubKey, appId);
                    Assert.NotNull(client);
                    manager.UpdateNumerOfClientConnections();
                    Assert.True(manager.NumberOfClientConnections >= 1);

                    var load = await Within("shared load", manager.LoadHoloNETEntrySharedAsync(client));
                    Assert.False(load.IsError, load.Message);
                    Assert.Equal("Form", manager.HoloNETEntryShared[appId].LastName);

                    // The shared collection: add from the form, update, remove, reload.
                    var empty = await Within("shared collection load", manager.LoadCollectionAsync(client));
                    Assert.False(empty.IsError, empty.Message);
                    var added = await Within("add", manager.AddHoloNETEntryToCollectionAsync(client, "One", "Two", DateTime.Today, "one@example.com"));
                    Assert.False(added.IsError, added.Message);
                    var collection = manager.HoloNETEntriesShared[appId];
                    Assert.Single(collection);
                    manager.CurrentAvatar = collection[0];
                    collection[0].LastName = "Updated";
                    var updated = await Within("update", manager.UpdateHoloNETEntryInCollectionAsync(client));
                    Assert.False(updated.IsError, updated.Message);
                    var removed = await Within("remove", manager.RemoveHoloNETEntryFromCollectionAsync(client, collection[0]));
                    Assert.False(removed.IsError, removed.Message);
                    Assert.Empty(collection);

                    // Connected already: other operations run straight away through ProcessClientOperationAsync.
                    foreach (var op in new[] { ClientOperation.LoadHoloNETEntryShared, ClientOperation.LoadHoloNETCollectionShared, ClientOperation.AddHoloNETEntryToCollectionShared, ClientOperation.CallZomeFunction })
                    {
                        manager.ClientOperation = op;
                        await Within(op.ToString(), manager.ProcessClientOperationAsync(client).ContinueWith(_ => true));
                        Assert.Equal(ClientOperation.None, manager.ClientOperation);
                    }

                    Assert.Equal(ConnectToAppAgentClientResponseType.Connected, manager.ConnectToAppAgentClient().ResponseType);
                    Assert.True(await manager.DisconnectAsync(manager.CurrentApp));
                    await Until("disconnected", () => client.State != System.Net.WebSockets.WebSocketState.Open, 30);
                    // A pooled client that isn't open is reconnected on request.
                    Assert.Equal(ConnectToAppAgentClientResponseType.Connecting, manager.ConnectToAppAgentClient().ResponseType);
                    await Until("reconnected", () => client.State == System.Net.WebSockets.WebSocketState.Open, 30);

                    // Reboot: closes everything and reconnects the admin client.
                    var rebooted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    manager.OnHoloNETManagerBooted += s => rebooted.TrySetResult(true);
                    manager.Reboot();
                    await Within("reboot", rebooted.Task, 60);
                    Assert.Equal(System.Net.WebSockets.WebSocketState.Open, manager.HoloNETClientAdmin.State);
                }
                finally
                {
                    HoloNETEntryUIManager.CurrentHoloNETEntryUI = null;
                    foreach (var c in manager.HoloNETClientAppAgentClients.ToList())
                        try { await c.DisconnectAsync(); } catch { }
                    await manager.HoloNETClientAdmin.UninstallAppAsync(appId);
                    manager.ShutdownHoloNETManager();
                }
            });
        }

        [SkippableFact]
        public async Task AdminQueries_AreLogged()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri), "needs a live conductor");
            await Ui.Run(async () =>
            {
                var manager = await Boot();
                var log = new System.Collections.Generic.List<string>();
                manager.OnLogMessage += (s, e) => { lock (log) log.Add(e.Message); };
                try
                {
                    manager.InitHoloNETEntryDemo = true;
                    var init = await manager.InitDemoApp("mgr-q-" + Guid.NewGuid().ToString("N").Substring(0, 6), manager.HoloNETEntryDemoHappPath);
                    manager.InitHoloNETEntryDemo = false;
                    Assert.True(init.IsSuccess, init.Message);
                    await manager.HoloNETClientAdmin.ListDnasAsync();
                    await manager.HoloNETClientAdmin.ListCellIdsAsync();
                    await manager.HoloNETClientAdmin.ListInterfacesAsync();
                    await manager.HoloNETClientAdmin.GetDnaDefinitionAsync(manager.HoloNETClientAdmin.ConvertHoloHashToBytes(init.DnaHash));
                    await Until("logged", () => { lock (log) return log.Any(l => l.Contains("DNA's Listed")) && log.Any(l => l.Contains("Cell ID's Listed")) && log.Any(l => l.Contains("App Interfaces Listed")) && log.Any(l => l.Contains("DNA Defintion Returned")); }, 30);
                    await manager.HoloNETClientAdmin.DisableAppAsync(init.AppInstalledResult.InstalledAppId ?? manager.InstalledApps.Last().Name);
                    await manager.HoloNETClientAdmin.UninstallAppAsync(init.AppInstalledResult.InstalledAppId ?? manager.InstalledApps.Last().Name);
                    await Until("disabled/uninstalled logged", () => { lock (log) return log.Any(l => l.Contains("Disabled")) && log.Any(l => l.Contains("Uninstalled")); }, 30);
                }
                finally
                {
                    await manager.HoloNETClientAdmin.DisconnectAsync();
                }
            });
        }
    }
}
