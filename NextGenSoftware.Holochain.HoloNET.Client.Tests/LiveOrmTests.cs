using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>An OASIS hApp avatar mapped with the HoloNET ORM (zome "oasis", *_entry_avatar functions).</summary>
    public class OrmAvatar : HoloNETEntryBase
    {
        public OrmAvatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", createHoloNETClientConnection: false, autoCallInitialize: false) { }

        public OrmAvatar(IHoloNETClientAppBase client) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

        [HolochainRustFieldName("id")] public string Id { get; set; }
        [HolochainRustFieldName("username")] public string Username { get; set; }
        [HolochainRustFieldName("email")] public string Email { get; set; }
        [HolochainRustFieldName("first_name")] public string FirstName { get; set; }
        [HolochainRustFieldName("karma")] public long Karma { get; set; }
    }

    /// <summary>The same avatar entry through HoloNETAuditEntryBase (adds id/version/created/modified/is_active fields).</summary>
    public class OrmAuditAvatar : HoloNETAuditEntryBase
    {
        public OrmAuditAvatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", createHoloNETClientConnection: false, autoCallInitialize: false) { }

        public OrmAuditAvatar(IHoloNETClientAppBase client) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

        [HolochainRustFieldName("username")] public string Username { get; set; }
        [HolochainRustFieldName("email")] public string Email { get; set; }
    }

    /// <summary>
    /// HoloNET ORM end to end against a real conductor (0.7.0 or 0.7.1) and the OASIS hApp: entries (create, load, update,
    /// delete, change tracking, audit fields) and collections (load, add, remove, save-all one by one and as a batch).
    /// Needs HOLONET_LIVE_ADMIN_URI and HOLONET_LIVE_HAPP_PATH, like the other live tests.
    /// </summary>
    [Collection(LiveConductor.Collection)]
    public class LiveOrmTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");
        private static readonly string HappPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH");

        private static async Task<T> Step<T>(string name, Task<T> task, int seconds = 60)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds))) != task)
                throw new TimeoutException($"'{name}' did not complete within {seconds}s.");
            return await task;
        }

        private static void RequireConductorAndHapp() =>
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath),
                "Needs HOLONET_LIVE_ADMIN_URI and HOLONET_LIVE_HAPP_PATH (see INTEGRATION_TESTING.md).");

        private static async Task WithApp(Func<IHoloNETClientAppAgent, Task> body)
        {
            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
            Assert.False((await admin.ConnectAsync(AdminUri)).IsError);
            string appId = "holonet-orm-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            try
            {
                var installed = await Step("install", admin.InstallEnableSignAttachAndConnectToHappAsync(appId, HappPath, "oasis"), 180);
                Assert.True(installed.IsAppConnected, "install/connect: " + installed.Message);
                try { await body(installed.HoloNETClientAppAgent); }
                finally { await installed.HoloNETClientAppAgent.DisconnectAsync(); }
            }
            finally
            {
                await admin.UninstallAppAsync(appId);
                await admin.DisconnectAsync();
            }
        }

        private static string NewName() => "orm-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        [SkippableFact]
        public async Task Live_Entry_CreateLoadUpdateDelete()
        {
            RequireConductorAndHapp();
            await WithApp(async client =>
            {
                string name = NewName();
                var avatar = new OrmAvatar(client) { Id = Guid.NewGuid().ToString(), Username = name, Email = name + "@example.com", FirstName = "Ada", Karma = 7 };

                var created = await Step("create", avatar.SaveAsync());
                Assert.False(created.IsError, "create: " + created.Message);
                Assert.False(string.IsNullOrEmpty(avatar.ActionHash), "ActionHash not set after create");
                Assert.False(string.IsNullOrEmpty(avatar.EntryHash), "EntryHash not set after create");
                Assert.Equal(ORM.Enums.HoloNETEntryState.NoChanges, avatar.State);
                Assert.False(avatar.HasEntryChanged());
                string createdAction = avatar.ActionHash;

                var loaded = new OrmAvatar(client);
                var load = await Step("load", loaded.LoadAsync(createdAction));
                Assert.False(load.IsError, "load: " + load.Message);
                Assert.Equal(name, loaded.Username);
                Assert.Equal("Ada", loaded.FirstName);
                Assert.Equal(7, loaded.Karma);
                Assert.Equal(createdAction, loaded.ActionHash);
                Assert.False(loaded.HasEntryChanged(), "freshly loaded entry reports changes");

                loaded.FirstName = "Grace";
                loaded.Karma = 8;
                Assert.True(loaded.HasEntryChanged(), "change not detected");
                var updated = await Step("update", loaded.SaveAsync());
                Assert.False(updated.IsError, "update: " + updated.Message);
                Assert.NotEqual(createdAction, loaded.ActionHash);
                Assert.Equal(createdAction, loaded.PreviousVersionActionHash);
                Assert.False(loaded.HasEntryChanged(), "entry still changed after save");

                var reloaded = new OrmAvatar(client);
                Assert.False((await Step("reload", reloaded.LoadAsync(loaded.ActionHash))).IsError);
                Assert.Equal("Grace", reloaded.FirstName);
                Assert.Equal(8, reloaded.Karma);
                Assert.Equal(name, reloaded.Username);

                // A second update chains on from the latest version.
                reloaded.Karma = 9;
                Assert.False((await Step("update 2", reloaded.SaveAsync())).IsError);

                var deleted = await Step("delete", reloaded.DeleteAsync());
                Assert.False(deleted.IsError, "delete: " + deleted.Message);
            });
        }

        [SkippableFact]
        public async Task Live_Entry_SyncMethods()
        {
            RequireConductorAndHapp();
            await WithApp(async client =>
            {
                await Task.Yield();
                string name = NewName();
                var avatar = new OrmAvatar(client) { Id = Guid.NewGuid().ToString(), Username = name, Email = name + "@example.com", FirstName = "Sync" };
                var created = await Task.Run(() => avatar.Save());
                Assert.False(created.IsError, "Save: " + created.Message);

                var loaded = new OrmAvatar(client) { ActionHash = avatar.ActionHash };
                var load = await Task.Run(() => loaded.Load());
                Assert.False(load.IsError, "Load: " + load.Message);
                Assert.Equal("Sync", loaded.FirstName);

                var deleted = await Task.Run(() => loaded.Delete());
                Assert.False(deleted.IsError, "Delete: " + deleted.Message);
            });
        }

        [SkippableFact]
        public async Task Live_Entry_LoadAndDeleteByCustomField()
        {
            RequireConductorAndHapp();
            await WithApp(async client =>
            {
                string name = NewName();
                var avatar = new OrmAvatar(client) { Id = Guid.NewGuid().ToString(), Username = name, Email = name + "@example.com", FirstName = "Custom" };
                Assert.False((await avatar.SaveAsync()).IsError);

                var byName = new OrmAvatar(client) { ZomeLoadEntryFunction = "get_avatar_by_username", ZomeDeleteEntryFunction = "delete_avatar_by_username" };
                var load = await Step("load by username", byName.LoadByCustomFieldAsync(name));
                Assert.False(load.IsError, "load by username: " + load.Message);
                Assert.Equal("Custom", byName.FirstName);
                Assert.Equal(avatar.ActionHash, byName.ActionHash);

                var deleted = await Step("delete by username", byName.DeleteByCustomFieldAsync(name));
                Assert.False(deleted.IsError, "delete by username: " + deleted.Message);
            });
        }

        [SkippableFact]
        public async Task Live_Entry_LoadOfUnknownHash_IsAnError()
        {
            RequireConductorAndHapp();
            await WithApp(async client =>
            {
                // A well-formed action hash nothing was ever written under: get returns None.
                var missing = new OrmAvatar(client);
                string unknown = client.ConvertHoloHashToString(new byte[] { 0x84, 0x29, 0x24 }.Concat(Enumerable.Repeat((byte)7, 36)).ToArray());
                var load = await Step("load unknown", missing.LoadAsync(unknown));
                Assert.True(load.IsError, "loading a hash that doesn't exist reported success");
            });
        }

        [SkippableFact]
        public async Task Live_AuditEntry_TracksIdVersionAndDates()
        {
            RequireConductorAndHapp();
            await WithApp(async client =>
            {
                string name = NewName();
                var avatar = new OrmAuditAvatar(client) { Username = name, Email = name + "@example.com" };
                var created = await Step("audit create", avatar.SaveAsync());
                Assert.False(created.IsError, "audit create: " + created.Message);
                Guid id = avatar.Id;
                Assert.NotEqual(Guid.Empty, id);
                Assert.Equal(1, avatar.Version);
                Assert.True(avatar.IsActive);
                Assert.NotEqual(DateTime.MinValue, avatar.CreatedDate);
                Assert.Equal(client.HoloNETDNA.AgentPubKey, avatar.CreatedBy);

                var loaded = new OrmAuditAvatar(client);
                Assert.False((await Step("audit load", loaded.LoadAsync(avatar.ActionHash))).IsError);
                Assert.Equal(id, loaded.Id);
                Assert.Equal(1, loaded.Version);
                Assert.Equal(avatar.CreatedBy, loaded.CreatedBy);
                Assert.Equal(avatar.CreatedDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), loaded.CreatedDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"));

                loaded.Email = "changed-" + name + "@example.com";
                var updated = await Step("audit update", loaded.SaveAsync());
                Assert.False(updated.IsError, "audit update: " + updated.Message);
                Assert.Equal(id, loaded.Id);   // the id identifies the entry across versions
                Assert.Equal(2, loaded.Version);
                Assert.NotEqual(DateTime.MinValue, loaded.ModifiedDate);
                Assert.Equal(client.HoloNETDNA.AgentPubKey, loaded.ModifiedBy);

                var deleted = await Step("audit delete", loaded.DeleteAsync());
                Assert.False(deleted.IsError, "audit delete: " + deleted.Message);
                Assert.NotEqual(DateTime.MinValue, loaded.DeletedDate);
            });
        }

        private static HoloNETCollection<OrmAvatar> NewCollection(IHoloNETClientAppAgent client) =>
            new HoloNETCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client, "batch_update_avatar_collection");

        private static OrmAvatar NewAvatar(string first)
        {
            string name = NewName();
            return new OrmAvatar { Id = Guid.NewGuid().ToString(), Username = name, Email = name + "@example.com", FirstName = first };
        }

        [SkippableFact]
        public async Task Live_Collection_AddLoadRemove()
        {
            RequireConductorAndHapp();
            await WithApp(async client =>
            {
                string anchor = "orm-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                var collection = NewCollection(client);

                var empty = await Step("load empty", collection.LoadCollectionAsync(anchor));
                Assert.False(empty.IsError, "load empty: " + empty.Message);
                Assert.Empty(collection);

                var a = NewAvatar("A");
                var b = NewAvatar("B");
                var addA = await Step("add A", collection.AddHoloNETEntryToCollectionAndSaveAsync(a, collectionAnchor: anchor));
                Assert.False(addA.IsError, "add A: " + addA.Message);
                var addB = await Step("add B", collection.AddHoloNETEntryToCollectionAndSaveAsync(b, collectionAnchor: anchor));
                Assert.False(addB.IsError, "add B: " + addB.Message);
                Assert.Equal(2, collection.Count);
                Assert.False(string.IsNullOrEmpty(a.ActionHash));

                var other = NewCollection(client);
                var loaded = await Step("load", other.LoadCollectionAsync(anchor));
                Assert.False(loaded.IsError, "load: " + loaded.Message);
                Assert.Equal(new[] { "A", "B" }, other.Select(e => e.FirstName).OrderBy(x => x).ToArray());
                Assert.All(other, e => Assert.False(string.IsNullOrEmpty(e.ActionHash)));
                Assert.All(other, e => Assert.False(e.HasEntryChanged()));

                var removed = await Step("remove A", other.RemoveHoloNETEntryFromCollectionAndSaveAsync(other.Single(e => e.FirstName == "A"), anchor));
                Assert.False(removed.IsError, "remove A: " + removed.Message);
                Assert.Single(other);

                var third = NewCollection(client);
                Assert.False((await Step("reload", third.LoadCollectionAsync(anchor))).IsError);
                Assert.Equal("B", Assert.Single(third).FirstName);
            });
        }

        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Live_Collection_SaveAllChanges(bool asOneBatch)
        {
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath), "needs a live conductor");
            await WithApp(async client =>
            {
                string anchor = "orm-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                var collection = NewCollection(client);
                Assert.False((await collection.LoadCollectionAsync(anchor)).IsError);

                collection.Add(NewAvatar("one"));
                collection.Add(NewAvatar("two"));
                var first = await Step("save all (adds)", collection.SaveAllChangesAsync(saveAsOneBatchOperation: asOneBatch, collectionAnchor: anchor));
                Assert.False(first.IsError, "save adds: " + first.Message + string.Join("; ", first.ErrorMessages));

                var reloaded = NewCollection(client);
                Assert.False((await reloaded.LoadCollectionAsync(anchor)).IsError);
                Assert.Equal(new[] { "one", "two" }, reloaded.Select(e => e.FirstName).OrderBy(x => x).ToArray());

                reloaded.Single(e => e.FirstName == "one").FirstName = "uno";
                reloaded.Remove(reloaded.Single(e => e.FirstName == "two"));
                reloaded.Add(NewAvatar("three"));
                var second = await Step("save all (mixed)", reloaded.SaveAllChangesAsync(saveAsOneBatchOperation: asOneBatch, collectionAnchor: anchor));
                Assert.False(second.IsError, "save mixed: " + second.Message + string.Join("; ", second.ErrorMessages));

                var final = NewCollection(client);
                Assert.False((await final.LoadCollectionAsync(anchor)).IsError);
                Assert.Equal(new[] { "three", "uno" }, final.Select(e => e.FirstName).OrderBy(x => x).ToArray());
            });
        }
    }
}
