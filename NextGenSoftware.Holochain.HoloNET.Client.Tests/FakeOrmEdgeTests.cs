using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Holochain.HoloNET.ORM.Enums;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>ORM edge cases: lazy initialisation, forwarded client errors, stop-on-first-error saves, closed objects.</summary>
    public class FakeOrmEdgeTests
    {
        /// <summary>An app client for the fake conductor that is configured (keys, token) but not connected yet.</summary>
        private static HoloNETClientAppAgent Unconnected(FakeConductor fake)
        {
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 9, 9, 9 };
            var app = new HoloNETClientAppAgent("app", null, dna);
            app.HoloNETDNA.AgentPubKey = app.ConvertHoloHashToString(Agent());
            app.HoloNETDNA.DnaHash = app.ConvertHoloHashToString(Dna());
            return app;
        }

        private static async Task<(FakeConductor, FakeOrmHapp, HoloNETClientAppAgent)> Signed()
        {
            var (fake, connected) = await FakeConductorZomeCallTests.Ready(); // grants signing credentials for the cell
            await connected.DisconnectAsync();
            var happ = new FakeOrmHapp();
            happ.Attach(fake);
            return (fake, happ, Unconnected(fake));
        }

        private static OrmAvatar Bare(string first) => new OrmAvatar { Id = Guid.NewGuid().ToString(), Username = "e-" + first, Email = first + "@example.com", FirstName = first };

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Collection_ConnectsItsClientOnFirstUse(bool observable)
        {
            var (fake, happ, client) = await Signed();
            using (fake)
            {
                dynamic c = observable
                    ? new HoloNETObservableCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client, "", autoCallInitialize: false)
                    : (object)new HoloNETCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client, "", autoCallInitialize: false);
                var initialised = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (observable) ((HoloNETObservableCollection<OrmAvatar>)c).OnInitialized += (s, e) => initialised.TrySetResult(true);
                else ((HoloNETCollection<OrmAvatar>)c).OnInitialized += (s, e) => initialised.TrySetResult(true);

                Assert.False((bool)c.IsInitialized);
                HoloNETCollectionLoadedResult<OrmAvatar> load = await c.LoadCollectionAsync("lazy");
                Assert.False(load.IsError, load.Message);
                Assert.True(await Task.WhenAny(initialised.Task, Task.Delay(5000)) == initialised.Task, "OnInitialized not raised");
                Assert.True((bool)c.IsInitialized);
                await client.DisconnectAsync();
            }
        }

        [Fact]
        public async Task Entry_ConnectsItsClientOnFirstUseAndForwardsClientErrors()
        {
            var (fake, happ, client) = await Signed();
            using (fake)
            {
                var avatar = new OrmAvatar(client) { Id = Guid.NewGuid().ToString(), Username = "lazy", Email = "l@example.com" };
                var initialised = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                avatar.OnInitialized += (s, e) => initialised.TrySetResult(true);
                var saved = await avatar.SaveAsync();
                Assert.False(saved.IsError, saved.Message);
                Assert.True(await Task.WhenAny(initialised.Task, Task.Delay(5000)) == initialised.Task, "OnInitialized not raised");

                // An error the client reports reaches the entry's OnError.
                var forwarded = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                avatar.OnError += (s, e) => forwarded.TrySetResult(e.Reason);
                var list = new HoloNETCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client);
                var listError = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                list.OnError += (s, e) => listError.TrySetResult(e.Reason);
                var obs = new HoloNETObservableCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client);
                var obsError = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                obs.OnError += (s, e) => obsError.TrySetResult(e.Reason);
                await fake.SendRawAsync(new byte[] { 0xc1 }); // not MessagePack: the client reports a decode error
                foreach (var t in new[] { forwarded.Task, listError.Task, obsError.Task })
                    Assert.True(await Task.WhenAny(t, Task.Delay(5000)) == t, "client error not forwarded");
                await client.DisconnectAsync();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Collection_StopsAtTheFirstErrorWhenAsked(bool observable)
        {
            var (fake, happ, client) = await Signed();
            using (fake)
            {
                dynamic c = observable
                    ? new HoloNETObservableCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client)
                    : (object)new HoloNETCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client);
                var list = (IList<OrmAvatar>)c;
                await c.LoadCollectionAsync("stop");
                list.Add(Bare("a")); list.Add(Bare("b")); list.Add(Bare("c"));
                Assert.False(((HoloNETCollectionSavedResult)await c.SaveAllChangesAsync(saveAsOneBatchOperation: false, collectionAnchor: "stop")).IsError);

                list.Remove(list[0]); list.Remove(list[0]);
                happ.Failing.Add("remove_avatar_from_collection");
                HoloNETCollectionSavedResult removes = await c.SaveAllChangesAsync(saveAsOneBatchOperation: false, continueOnError: false, collectionAnchor: "stop");
                Assert.True(removes.IsError);
                Assert.Single(removes.EntiesSaveErrors);   // stopped after the first failure
                happ.Failing.Clear();
                Assert.False(((HoloNETCollectionSavedResult)await c.SaveAllChangesAsync(saveAsOneBatchOperation: false, collectionAnchor: "stop")).IsError);

                list.Add(Bare("d"));
                Assert.False(((HoloNETCollectionSavedResult)await c.SaveAllChangesAsync(saveAsOneBatchOperation: false, collectionAnchor: "stop")).IsError);
                foreach (var e in list) e.FirstName += "!";
                happ.Failing.Add("update_entry_avatar");
                HoloNETCollectionSavedResult updates = await c.SaveAllChangesAsync(saveAsOneBatchOperation: false, continueOnError: false, collectionAnchor: "stop");
                Assert.True(updates.IsError);
                Assert.Single(updates.EntiesSaveErrors);
                await client.DisconnectAsync();
            }
        }

        [Fact]
        public async Task ObservableCollection_OfATypeWithoutParameterlessConstructor_ReportsIt()
        {
            var (fake, happ, client) = await Signed();
            using (fake)
            {
                var c = new HoloNETObservableCollection<FakeOrmTests.NoDefaultCtor>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client);
                var r = await c.LoadCollectionAsync("nd");
                Assert.True(r.IsError);
                Assert.Contains("parameterless constructor", r.Message);
                await client.DisconnectAsync();
            }
        }

        [Fact]
        public void Diff_KeepsTheUpdatedFlagWhenAnEntryIsAddedOrRemoved()
        {
            var original = Bare("o");
            original.ActionHash = "uhCkkoriginal";
            var added = Bare("n");
            original.State = HoloNETEntryState.Updated;
            added.State = HoloNETEntryState.Updated;
            var a = new List<OrmAvatar>(); var r = new List<OrmAvatar>();
            Assert.True(HoloNETCollectionCore.Diff(new[] { added }, new[] { original }, a, r));
            Assert.Equal(HoloNETEntryState.UpdatedAndAddedToCollection, added.State);
            Assert.Equal(HoloNETEntryState.UpdatedAndRemovedFromCollections, original.State);
            Assert.Null(HoloNETCollectionCore.AttachLoadedEntry<OrmAvatar>(new Record(), null));
        }

        [Theory]
        [InlineData(ErrorHandlingBehaviour.NeverThrowExceptions, false)]
        [InlineData(ErrorHandlingBehaviour.OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent, true)]
        [InlineData(ErrorHandlingBehaviour.AlwaysThrowExceptionOnError, true)]
        public async Task ClosedCollection_ReportsErrorsPerErrorHandlingBehaviour(ErrorHandlingBehaviour behaviour, bool throws)
        {
            var (fake, happ, client) = await Signed();
            using (fake)
            {
                client.HoloNETDNA.ErrorHandlingBehaviour = behaviour;
                foreach (bool observable in new[] { false, true })
                {
                    dynamic c = observable
                        ? new HoloNETObservableCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client, autoCallInitialize: false)
                        : (object)new HoloNETCollection<OrmAvatar>("oasis", "get_avatar_collection", "add_avatar_to_collection", "remove_avatar_from_collection", client, autoCallInitialize: false);
                    // A subscriber that throws: the exception is caught inside the collection and goes through HandleError.
                    if (observable) ((HoloNETObservableCollection<OrmAvatar>)c).OnCollectionLoaded += (s, e) => throw new InvalidOperationException("subscriber failed");
                    else ((HoloNETCollection<OrmAvatar>)c).OnCollectionLoaded += (s, e) => throw new InvalidOperationException("subscriber failed");
                    if (throws)
                        await Assert.ThrowsAsync<HoloNETException>(async () => await c.LoadCollectionAsync("x"));
                    else
                    {
                        HoloNETCollectionLoadedResult<OrmAvatar> r = await c.LoadCollectionAsync("x");
                        Assert.True(r.IsError);
                        Assert.Contains("subscriber failed", r.Message);
                    }

                    // With no client at all the error is still reported (it used to throw a NullReferenceException).
                    c.HoloNETClient = null;
                    HoloNETCollectionLoadedResult<OrmAvatar> orphan = await c.LoadCollectionAsync("x");
                    Assert.True(orphan.IsError);
                }
            }
        }
    }
}
