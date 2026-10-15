using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.ORM.Enums;
using NextGenSoftware.Utilities.ExtentionMethods;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>An entry using every property type the ORM converts, plus a field that is never sent.</summary>
    public class OrmTypedEntry : HoloNETEntryBase
    {
        public OrmTypedEntry() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", createHoloNETClientConnection: false, autoCallInitialize: false) { }
        public OrmTypedEntry(IHoloNETClientAppBase client) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }

        [HolochainRustFieldName("id")] public string Id { get; set; } = Guid.NewGuid().ToString();
        [HolochainRustFieldName("username")] public string Username { get; set; } = "typed";
        [HolochainRustFieldName("email")] public string Email { get; set; } = "typed@example.com";
        [HolochainRustFieldName("guid")] public Guid Guid { get; set; } = Guid.Parse("6f1c7e2a-1111-4222-8333-944455556666");
        [HolochainRustFieldName("born")] public DateTime Born { get; set; } = new DateTime(1815, 12, 10, 8, 30, 0, DateTimeKind.Utc);
        [HolochainRustFieldName("nickname")] public string Nickname { get; set; }
        [HolochainRustFieldName("secret", false)] public string Secret { get; set; } = "never sent";
    }

    /// <summary>HoloNET ORM entries and collections against <see cref="FakeOrmHapp"/> (no conductor needed).</summary>
    public class FakeOrmTests
    {
        private static async Task WithHapp(Func<FakeOrmHapp, HoloNETClientAppAgent, Task> body)
        {
            var (fake, app) = await FakeConductorZomeCallTests.Ready();
            using (fake)
            {
                var happ = new FakeOrmHapp();
                happ.Attach(fake);
                try { await body(happ, app); }
                finally { await app.DisconnectAsync(); }
            }
        }

        private static OrmAvatar NewAvatar(IHoloNETClientAppBase client, string first = "Ada") =>
            new OrmAvatar(client) { Id = Guid.NewGuid().ToString(), Username = "u-" + Guid.NewGuid().ToString("N").Substring(0, 6), Email = "e@example.com", FirstName = first, Karma = 1 };

        private static (string Fn, object Args) LastCall(FakeOrmHapp happ) => happ.Calls.Last();

        [Fact]
        public async Task Entry_CrudRaisesEventsAndTracksHashes()
        {
            await WithHapp(async (happ, client) =>
            {
                var avatar = NewAvatar(client);
                var events = new List<string>();
                avatar.OnSaved += (s, e) => events.Add("saved");
                avatar.OnLoaded += (s, e) => events.Add("loaded");
                avatar.OnDeleted += (s, e) => events.Add("deleted");

                Assert.False((await avatar.SaveAsync()).IsError);
                Assert.Equal("create_entry_avatar", LastCall(happ).Fn);
                Assert.Equal(client.ConvertHoloHashToString(FakeConductorAdminTests.Action(10)), avatar.ActionHash);
                Assert.Null(avatar.PreviousVersionActionHash);

                avatar.FirstName = "Grace";
                Assert.False((await avatar.SaveAsync()).IsError);
                var (fn, args) = LastCall(happ);
                Assert.Equal("update_entry_avatar", fn);
                Assert.Equal(10, FakeOrmHapp.Id(((Dictionary<object, object>)args)["original_action_hash"]));
                Assert.Equal(client.ConvertHoloHashToString(FakeConductorAdminTests.Action(10)), avatar.PreviousVersionActionHash);
                Assert.Equal(client.ConvertHoloHashToString(FakeConductorAdminTests.Action(11)), avatar.ActionHash);
                Assert.Equal("Update", avatar.Record.Type);

                var loaded = new OrmAvatar(client) { ActionHash = avatar.ActionHash };
                Assert.False((await loaded.LoadAsync()).IsError);
                Assert.Equal("Grace", loaded.FirstName);

                Assert.False((await avatar.LoadAsync()).IsError);
                Assert.False((await avatar.DeleteAsync()).IsError);
                Assert.Equal("delete_entry_avatar", LastCall(happ).Fn);
                Assert.Equal(new[] { "saved", "saved", "loaded", "deleted" }, events);
            });
        }

        [Fact]
        public async Task Entry_SyncWrappersAndExplicitHashes()
        {
            await WithHapp(async (happ, client) =>
            {
                await Task.Yield();
                var avatar = NewAvatar(client);
                Assert.False(await Task.Run(() => avatar.Save().IsError));
                string hash = avatar.ActionHash;

                var loaded = new OrmAvatar(client);
                Assert.False(await Task.Run(() => loaded.Load(hash).IsError));
                Assert.False(await Task.Run(() => loaded.Load().IsError));
                var byField = new OrmAvatar(client) { ZomeLoadEntryFunction = "get_avatar_by_username" };
                Assert.False(await Task.Run(() => byField.LoadByCustomField(avatar.Username).IsError));
                Assert.Equal(hash, byField.ActionHash);
                Assert.False(await Task.Run(() => loaded.Delete(hash).IsError));

                var other = NewAvatar(client);
                Assert.False(await Task.Run(() => other.Save().IsError));
                var byName = new OrmAvatar(client) { ZomeDeleteEntryFunction = "delete_avatar_by_username" };
                Assert.False(await Task.Run(() => byName.DeleteByCustomField(other.Username).IsError));
                Assert.True(await Task.Run(() => new OrmAvatar(client).Delete().IsError), "Delete with no hash should fail");
            });
        }

        [Fact]
        public async Task Entry_CustomDataIsSentWithTheLoadDeleteAndSaveParams()
        {
            await WithHapp(async (happ, client) =>
            {
                var avatar = NewAvatar(client);
                await avatar.SaveAsync(new Dictionary<string, string> { ["tag"] = "x" });
                Assert.Equal("x", ((Dictionary<object, object>)LastCall(happ).Args)["tag"]);

                // With custom data and a key, load/delete send { key: value, ...custom } (the fake rejects it: it takes a bare hash).
                var load = await new OrmAvatar(client).LoadByCustomFieldAsync(avatar.Username, "username", new Dictionary<string, string> { ["extra"] = "1" });
                var sent = (Dictionary<object, object>)LastCall(happ).Args;
                Assert.Equal(avatar.Username, sent["username"]);
                Assert.Equal("1", sent["extra"]);
                Assert.True(load.IsError);

                // Without a key the value goes on its own.
                var bare = await new OrmAvatar(client).LoadByCustomFieldAsync(avatar.Username, "", new Dictionary<string, string> { ["ignored"] = "1" });
                Assert.Equal(avatar.Username, LastCall(happ).Args);

                var deleted = await new OrmAvatar(client).DeleteAsync(avatar.ActionHash, new Dictionary<string, string> { ["why"] = "test" });
                Assert.Equal("test", ((Dictionary<object, object>)LastCall(happ).Args)["why"]);
                Assert.True(deleted.IsError);
            });
        }

        [Fact]
        public async Task Entry_ErrorsFromTheZomeComeBackAsIsError()
        {
            await WithHapp(async (happ, client) =>
            {
                var avatar = NewAvatar(client);
                happ.Failing.Add("create_entry_avatar");
                var created = await avatar.SaveAsync();
                Assert.True(created.IsError);
                Assert.Contains("fake create_entry_avatar failed", created.Message);
                Assert.True(string.IsNullOrEmpty(avatar.ActionHash));
                happ.Failing.Clear();

                Assert.False((await avatar.SaveAsync()).IsError);
                happ.Failing.Add("update_entry_avatar");
                avatar.FirstName = "x";
                Assert.True((await avatar.SaveAsync()).IsError);

                happ.Failing.Add("get_entry_avatar");
                Assert.True((await avatar.LoadAsync()).IsError);
                happ.Failing.Add("delete_entry_avatar");
                Assert.True((await avatar.DeleteAsync()).IsError);
            });
        }

        [Fact]
        public async Task Entry_LoadOfMissingOrEmptyHash_IsAnError()
        {
            await WithHapp(async (happ, client) =>
            {
                var missing = await new OrmAvatar(client).LoadAsync(client.ConvertHoloHashToString(FakeConductorAdminTests.Action(200)));
                Assert.True(missing.IsError);
                Assert.Contains("No entry was found", missing.Message);

                var byField = await new OrmAvatar(client).LoadByCustomFieldAsync("nobody");
                Assert.True(byField.IsError);

                var empty = await new OrmAvatar(client).LoadAsync("");
                Assert.True(empty.IsError);
                Assert.Contains("is null", empty.Message);
            });
        }

        [Fact]
        public async Task Entry_WithoutAClient_FailsCleanly()
        {
            await Task.Yield();
            var orphan = new OrmAvatar { ActionHash = "uhCkkAAAA" };
            var load = await orphan.LoadAsync();
            Assert.True(load.IsError);
            Assert.Contains("HoloNETClient is null", load.Message);
            Assert.False(orphan.IsInitialized);

            var save = await orphan.SaveAsync();
            Assert.True(save.IsError);

            var closed = await orphan.CloseAsync();
            Assert.Null(closed);
            Assert.Null(orphan.Close());
        }

        [Fact]
        public async Task Entry_BuildDynamicParamsObject_ConvertsTypesAndHonoursDisabledFields()
        {
            await WithHapp(async (happ, client) =>
            {
                await Task.Yield();
                var entry = new OrmTypedEntry(client);
                IDictionary<string, object> p = entry.BuildDynamicParamsObject(new Dictionary<string, string> { ["custom"] = "c" });
                Assert.Equal("6f1c7e2a-1111-4222-8333-944455556666", p["guid"]);
                Assert.Equal("1815-12-10T08:30:00.0000000Z", p["born"]);
                Assert.Equal("", p["nickname"]);          // null strings are sent as ""
                Assert.Equal("c", p["custom"]);
                Assert.False(p.ContainsKey("secret"));      // [HolochainRustFieldName("secret", false)]

                IDictionary<string, object> without = entry.BuildDynamicParamsObject(holochainFieldsIsEnabledKeyValuePairs: new Dictionary<string, bool> { ["Email"] = false }, cachePropertyInfos: false);
                Assert.False(without.ContainsKey("email"));
                HoloNETEntryBase.ClearCache();
            });
        }

        [Fact]
        public async Task Entry_SaveWithOwnParamsObject()
        {
            await WithHapp(async (happ, client) =>
            {
                var avatar = NewAvatar(client);
                Assert.False((await avatar.SaveAsync()).IsError);

                dynamic raw = new ExpandoObject();
                ExpandoObjectHelpers.AddProperty(raw, "original_action_hash", client.ConvertHoloHashToBytes(avatar.ActionHash));
                ExpandoObjectHelpers.AddProperty(raw, "updated_entry", avatar.BuildDynamicParamsObject());
                var updated = await avatar.SaveAsync((object)raw, false);
                Assert.False(updated.IsError, updated.Message);
                Assert.Equal("update_entry_avatar", LastCall(happ).Fn);

                dynamic named = avatar.BuildDynamicParamsObject();
                var renamed = await Task.Run(() => (ZomeFunctionCallBackEventArgs)avatar.Save((object)named, true, "updated_entry", "original_action_hash"));
                Assert.False(renamed.IsError, renamed.Message);
            });
        }

        [Fact]
        public async Task Entry_ChangeTrackingStates()
        {
            await WithHapp(async (happ, client) =>
            {
                var avatar = NewAvatar(client);
                await avatar.SaveAsync();
                Assert.False(avatar.HasEntryChanged());
                Assert.False(avatar.HasEntryChanged(new Dictionary<string, bool> { ["FirstName"] = false }, false));

                avatar.State = HoloNETEntryState.AddedToCollection;
                avatar.IsChanged = true;
                Assert.Equal(HoloNETEntryState.UpdatedAndAddedToCollection, avatar.State);
                avatar.State = HoloNETEntryState.RemovedFromCollection;
                avatar.IsChanged = true;
                Assert.Equal(HoloNETEntryState.UpdatedAndRemovedFromCollections, avatar.State);
                avatar.State = HoloNETEntryState.NoChanges;
                avatar.IsChanged = false;
                Assert.Equal(HoloNETEntryState.NoChanges, avatar.State);

                avatar.Karma = 99;
                Assert.True(avatar.HasEntryChanged());
                Assert.Equal(HoloNETEntryState.Updated, avatar.State);
                await avatar.SaveAsync();
                Assert.Equal(HoloNETEntryState.NoChanges, avatar.State);
                Assert.False(avatar.IsChanged);

                avatar.State = HoloNETEntryState.UpdatedAndAddedToCollection;
                await avatar.LoadAsync();
                Assert.Equal(HoloNETEntryState.AddedToCollection, avatar.State);
                avatar.State = HoloNETEntryState.UpdatedAndRemovedFromCollections;
                await avatar.LoadAsync();
                Assert.Equal(HoloNETEntryState.RemovedFromCollection, avatar.State);
                Assert.NotNull(avatar.OrginalEntry);
                Assert.NotEmpty(avatar.OrginalKeyValuePairs);
            });
        }

        [Fact]
        public async Task Entry_InitializeAndWaitWithAConnectedClient()
        {
            await WithHapp(async (happ, client) =>
            {
                var avatar = new OrmAvatar(client);
                Assert.True(avatar.IsInitialized);
                await avatar.InitializeAsync();
                avatar.Initialize();
                var ready = await avatar.WaitTillHoloNETInitializedAsync();
                Assert.NotNull(ready);
                Assert.False(avatar.IsInitializing);

                // A shared client is not the entry's to close.
                Assert.Null(await avatar.CloseAsync());
                Assert.Same(client, avatar.HoloNETClient);
            });
        }

        [Fact]
        public async Task Entry_ErrorHandlingBehaviour()
        {
            await WithHapp(async (happ, client) =>
            {
                var avatar = new OrmAvatar(client);
                var errors = new List<string>();
                avatar.OnError += (s, e) => errors.Add(e.Reason);
                client.HoloNETDNA.ErrorHandlingBehaviour = ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent;

                // A params object HoloNET can't serialize: the save fails inside SaveAsync and is reported, not thrown.
                var bad = await avatar.SaveAsync((object)new Func<int>(() => 1));
                Assert.True(bad.IsError);

                var thrower = new OrmAvatar(client);
                client.HoloNETDNA.ErrorHandlingBehaviour = ErrorHandlingBehaviour.AlwaysThrowExceptionOnError;
                await Assert.ThrowsAnyAsync<Exception>(() => thrower.SaveAsync((object)new Func<int>(() => 1)));
                client.HoloNETDNA.ErrorHandlingBehaviour = ErrorHandlingBehaviour.NeverThrowExceptions;
            });
        }

        [Fact]
        public async Task AuditEntry_FlagsAndSyncOverloads()
        {
            await WithHapp(async (happ, client) =>
            {
                await Task.Yield();
                var audit = new OrmAuditAvatar(client) { Username = "audit", Email = "a@example.com", IsVersionTrackingEnabled = false, IsGenerateUniqueGuidIdEnabled = false, IsActiveFlagEnabled = false, IsAuditAgentCreateModifyDeleteFieldsEnabled = false };
                Assert.False((await audit.SaveAsync()).IsError);
                var sent = (Dictionary<object, object>)LastCall(happ).Args;
                Assert.False(sent.ContainsKey("version"));
                Assert.False(sent.ContainsKey("is_active"));
                Assert.False(sent.ContainsKey("created_by"));
                Assert.Equal(Guid.Empty, audit.Id);

                var tracked = new OrmAuditAvatar(client) { Username = "tracked", Email = "t@example.com" };
                Assert.False(await Task.Run(() => tracked.Save().IsError));
                Assert.Single(tracked.AuditEntries.Where(a => a.Type == HoloNETAuditEntryType.Create));
                tracked.Email = "changed@example.com";
                Assert.False(await Task.Run(() => tracked.Save().IsError));
                Assert.Equal(2, tracked.Version);

                var loaded = new OrmAuditAvatar(client);
                Assert.False(await Task.Run(() => loaded.Load(tracked.ActionHash).IsError));
                Assert.False(await Task.Run(() => loaded.Load(0).IsError));
                Assert.False((await loaded.LoadAsync(0)).IsError);
                loaded.ZomeLoadEntryFunction = "get_avatar_by_username";
                Assert.False(await Task.Run(() => loaded.LoadByCustomField("tracked").IsError));
                Assert.False((await loaded.LoadByCustomFieldAsync("tracked")).IsError);
                loaded.ZomeLoadEntryFunction = "get_entry_avatar";

                dynamic p = new ExpandoObject();
                ExpandoObjectHelpers.AddProperty(p, "username", "raw");
                ExpandoObjectHelpers.AddProperty(p, "email", "raw@example.com");
                var raw = new OrmAuditAvatar(client);
                Assert.False(((ZomeFunctionCallBackEventArgs)await raw.SaveAsync((object)p)).IsError);
                var rawSent = (Dictionary<object, object>)LastCall(happ).Args;
                Assert.True(rawSent.ContainsKey("created_date") && rawSent.ContainsKey("version") && rawSent.ContainsKey("is_active"));
                dynamic p2 = new ExpandoObject();
                ExpandoObjectHelpers.AddProperty(p2, "username", "raw2");
                ExpandoObjectHelpers.AddProperty(p2, "email", "raw2@example.com");
                Assert.False(await Task.Run(() => ((ZomeFunctionCallBackEventArgs)new OrmAuditAvatar(client).Save((object)p2)).IsError));

                Assert.False(await Task.Run(() => tracked.Delete().IsError));
                Assert.Equal(client.HoloNETDNA.AgentPubKey, tracked.DeletedBy);
                var other = new OrmAuditAvatar(client) { Username = "byname", Email = "b@example.com", ZomeDeleteEntryFunction = "delete_avatar_by_username" };
                Assert.False((await other.SaveAsync()).IsError);
                Assert.False(await Task.Run(() => other.DeleteByCustomField("byname").IsError));
                var another = new OrmAuditAvatar(client) { Username = "byname2", Email = "b@example.com", ZomeDeleteEntryFunction = "delete_avatar_by_username" };
                Assert.False((await another.SaveAsync()).IsError);
                Assert.False((await another.DeleteByCustomFieldAsync("byname2")).IsError);
                Assert.False(await Task.Run(() => loaded.Delete(raw.ActionHash).IsError));
            });
        }

        private static HoloNETCollection<OrmAvatar> NewCollection(IHoloNETClientAppBase client, string batch = "batch_update_avatar_collection") =>
            new HoloNETCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client, batch);

        private static HoloNETObservableCollection<OrmAvatar> NewObservable(IHoloNETClientAppBase client, string batch = "batch_update_avatar_collection") =>
            new HoloNETObservableCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client, batch);

        private static OrmAvatar Bare(string first) => new OrmAvatar { Id = Guid.NewGuid().ToString(), Username = "c-" + first, Email = first + "@example.com", FirstName = first };

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Collection_SaveAllChanges(bool observable, bool batch)
        {
            await WithHapp(async (happ, client) =>
            {
                IList<OrmAvatar> Make() => observable ? NewObservable(client) : NewCollection(client);
                async Task<HoloNETCollectionLoadedResult<OrmAvatar>> Load(IList<OrmAvatar> c) => observable ? await ((HoloNETObservableCollection<OrmAvatar>)c).LoadCollectionAsync("a") : await ((HoloNETCollection<OrmAvatar>)c).LoadCollectionAsync("a");
                async Task<HoloNETCollectionSavedResult> Save(IList<OrmAvatar> c) => observable ? await ((HoloNETObservableCollection<OrmAvatar>)c).SaveAllChangesAsync(saveAsOneBatchOperation: batch, collectionAnchor: "a") : await ((HoloNETCollection<OrmAvatar>)c).SaveAllChangesAsync(saveAsOneBatchOperation: batch, collectionAnchor: "a");

                var c1 = Make();
                Assert.False((await Load(c1)).IsError);
                c1.Add(Bare("one"));
                c1.Add(Bare("two"));
                var s1 = await Save(c1);
                Assert.False(s1.IsError, s1.Message);
                Assert.Equal(2, s1.EntiesAdded.Count);
                Assert.All(c1, e => Assert.False(string.IsNullOrEmpty(e.ActionHash)));

                var c2 = Make();
                Assert.False((await Load(c2)).IsError);
                Assert.Equal(2, c2.Count);
                c2.Single(e => e.FirstName == "one").FirstName = "uno";
                c2.Remove(c2.Single(e => e.FirstName == "two"));
                var unsaved = Bare("ghost");
                c2.Add(unsaved);
                c2.Remove(unsaved);                      // added and removed before saving: nothing to send
                c2.Add(Bare("three"));
                var s2 = await Save(c2);
                Assert.False(s2.IsError, s2.Message);
                Assert.Single(s2.EntiesAdded);
                Assert.Single(s2.EntiesRemoved);
                Assert.Single(s2.EntiesSaved);
                Assert.Equal("uno", s2.EntiesSaved.Cast<OrmAvatar>().Single().FirstName);

                var c3 = Make();
                Assert.False((await Load(c3)).IsError);
                Assert.Equal(new[] { "three", "uno" }, c3.Select(e => e.FirstName).OrderBy(x => x).ToArray());

                // Nothing changed: nothing to send.
                var s3 = await Save(c3);
                Assert.False(s3.IsError);
                Assert.Empty(s3.EntiesAdded.Concat(s3.EntiesRemoved).Concat(s3.EntiesSaved));

                // A reload replaces the contents rather than appending.
                Assert.False((await Load(c3)).IsError);
                Assert.Equal(2, c3.Count);
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Collection_ErrorsAreReported(bool observable)
        {
            await WithHapp(async (happ, client) =>
            {
                dynamic c = observable ? NewObservable(client) : NewCollection(client);
                ((IList<OrmAvatar>)c).Add(Bare("x"));
                happ.Failing.Add("batch_update_avatar_collection");
                HoloNETCollectionSavedResult batch = await c.SaveAllChangesAsync(collectionAnchor: "e");
                Assert.True(batch.IsError);
                Assert.Single(batch.EntiesSaveErrors);
                Assert.Contains("fake batch_update_avatar_collection failed", batch.Message);

                happ.Failing.Add("create_entry_avatar");
                HoloNETCollectionSavedResult one = await c.SaveAllChangesAsync(saveAsOneBatchOperation: false, continueOnError: false, collectionAnchor: "e");
                Assert.True(one.IsError);
                Assert.Contains("fake create_entry_avatar failed", one.Message);
                happ.Failing.Clear();

                happ.Failing.Add("add_avatar_to_collection");
                ZomeFunctionCallBackEventArgs add = await c.AddHoloNETEntryToCollectionAndSaveAsync(Bare("y"), true, "e");
                Assert.True(add.IsError);
                ZomeFunctionCallBackEventArgs notSaved = await c.AddHoloNETEntryToCollectionAndSaveAsync(Bare("z"), false, "e");
                Assert.True(notSaved.IsError);
                Assert.Contains("has not been saved", notSaved.Message);
                happ.Failing.Clear();

                var saved = Bare("w");
                Assert.False(((ZomeFunctionCallBackEventArgs)await c.AddHoloNETEntryToCollectionAndSaveAsync(saved, true, "e")).IsError);
                happ.Failing.Add("remove_avatar_from_collection");
                ZomeFunctionCallBackEventArgs remove = await c.RemoveHoloNETEntryFromCollectionAndSaveAsync(saved, "e");
                Assert.True(remove.IsError);
                Assert.Contains(saved, (IEnumerable<OrmAvatar>)c);
                ZomeFunctionCallBackEventArgs localOnly = await c.RemoveHoloNETEntryFromCollectionAndSaveAsync(Bare("never"), "e");
                Assert.False(localOnly.IsError);

                happ.Failing.Add("get_avatar_collection");
                HoloNETCollectionLoadedResult<OrmAvatar> load = await c.LoadCollectionAsync("e");
                Assert.True(load.IsError);
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Collection_SyncWrappersEventsAndNoBatchFunction(bool observable)
        {
            await WithHapp(async (happ, client) =>
            {
                await Task.Yield();
                dynamic c = observable ? NewObservable(client, "") : NewCollection(client, "");
                var events = new List<string>();
                if (observable)
                {
                    var o = (HoloNETObservableCollection<OrmAvatar>)c;
                    o.OnCollectionLoaded += (s, e) => events.Add("loaded");
                    o.OnCollectionSaved += (s, e) => events.Add("saved");
                    o.OnHoloNETEntryAddedToCollection += (s, e) => events.Add("added");
                    o.OnHoloNETEntryRemovedFromCollection += (s, e) => events.Add("removed");
                }
                else
                {
                    var l = (HoloNETCollection<OrmAvatar>)c;
                    l.OnCollectionLoaded += (s, e) => events.Add("loaded");
                    l.OnCollectionSaved += (s, e) => events.Add("saved");
                    l.OnHoloNETEntryAddedToCollection += (s, e) => events.Add("added");
                    l.OnHoloNETEntryRemovedFromCollection += (s, e) => events.Add("removed");
                }

                Assert.False(await Task.Run(() => ((HoloNETCollectionLoadedResult<OrmAvatar>)c.LoadCollection("s")).IsError));
                var a = Bare("a");
                Assert.False(await Task.Run(() => ((ZomeFunctionCallBackEventArgs)c.AddHoloNETEntryToCollectionAndSave(a, true, "s")).IsError));
                Assert.False(await Task.Run(() => ((ZomeFunctionCallBackEventArgs)c.RemoveHoloNETEntryFromCollectionAndSave(a, "s")).IsError));
                ((IList<OrmAvatar>)c).Add(Bare("b"));
                c.UpdateEnriesState();
                Assert.True((bool)c.IsChanges);
                // No batch function configured: SaveAllChanges falls back to one call per change.
                Assert.False(await Task.Run(() => ((HoloNETCollectionSavedResult)c.SaveAllChanges(collectionAnchor: "s")).IsError));
                Assert.Equal(new[] { "loaded", "added", "removed", "added", "saved" }, events);
                Assert.True((bool)c.IsInitialized);
                await c.InitializeAsync();
                c.Initialize();
                Assert.NotNull(await c.WaitTillHoloNETInitializedAsync());
                Assert.Null(await c.CloseAsync());
                Assert.Null(c.Close());
            });
        }

        [Fact]
        public async Task Collection_LoadWithCustomDataSendsAMap()
        {
            await WithHapp(async (happ, client) =>
            {
                var c = NewCollection(client);
                var r = await c.LoadCollectionAsync("anchor", new Dictionary<string, string> { ["filter"] = "f" });
                var sent = (Dictionary<object, object>)LastCall(happ).Args;
                Assert.Equal("anchor", sent["collection_anchor"]);
                Assert.Equal("f", sent["filter"]);
                Assert.True(r.IsError); // the fake (like the real hApp) takes a bare anchor string
            });
        }

        /// <summary>An entry type with no parameterless constructor can't be created for loaded records: say so.</summary>
        public class NoDefaultCtor : HoloNETEntryBase
        {
            public NoDefaultCtor(IHoloNETClientAppBase client) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", client) { }
            [HolochainRustFieldName("username")] public string Username { get; set; }
        }

        [Fact]
        public async Task Collection_OfATypeWithoutParameterlessConstructor_ReportsIt()
        {
            await WithHapp(async (happ, client) =>
            {
                var seed = NewCollection(client);
                Assert.False((await seed.AddHoloNETEntryToCollectionAndSaveAsync(Bare("q"), true, "nd")).IsError);

                var c = new HoloNETCollection<NoDefaultCtor>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client);
                var r = await c.LoadCollectionAsync("nd");
                Assert.True(r.IsError);
                Assert.Contains("parameterless constructor", r.Message);
            });
        }
    }
}
