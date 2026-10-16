#pragma warning disable CS0618 // exercises the obsolete (string hApp) overloads on purpose
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.API;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    public class ApiTestObject
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    /// <summary>
    /// HoloNET API (the static key/value + object store) end to end against a real conductor and the OASIS hApp, on
    /// Holochain 0.7.0 and 0.7.1: install, save, reload from the DHT, restart and find the same data again, errors.
    /// </summary>
    [Collection(LiveConductor.Collection)]
    public class LiveHoloNETAPITests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");
        private static readonly string HappPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH");

        private static async Task<T> Step<T>(string name, Task<T> task, int seconds = 120)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds))) != task)
                throw new TimeoutException($"'{name}' did not complete within {seconds}s.");
            return await task;
        }

        private static async Task SignalsBetweenTheApiAndAnotherAgent()
        {
            string peerId = "holonet-api-peer-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
            await admin.ConnectAsync(AdminUri);
            var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(peerId, HappPath, "oasis");
            Assert.True(installed.IsAppConnected, installed.Message);
            var peer = installed.HoloNETClientAppAgent;

            var apiGot = new System.Collections.Concurrent.ConcurrentQueue<HoloNETAPISignalEventArgs>();
            var peerGot = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();
            EventHandler<HoloNETAPISignalEventArgs> onSignal = (s, e) => apiGot.Enqueue(e);
            HoloNETAPI.OnSignalReceived += onSignal;
            peer.OnSignalCallBack += (s, e) => { if (e?.RawSignalData?.Data != null) peerGot.Enqueue(e.RawSignalData.Data); };

            try
            {
                // Peer -> API, through the same zome function the API uses.
                var sent = await peer.CallZomeFunctionAsync("oasis", "hypernet_send", new System.Collections.Generic.Dictionary<string, object>
                {
                    ["session"] = "holonet-api",
                    ["payload"] = System.Text.Encoding.UTF8.GetBytes("hello api"),
                    ["to"] = new[] { peer.ConvertHoloHashToBytes(HoloNETAPI.HoloNETClient.HoloNETDNA.AgentPubKey) }
                });
                Assert.False(sent.IsError, sent.Message);
                await Until(() => !apiGot.IsEmpty);
                Assert.True(apiGot.TryPeek(out var got));
                Assert.Equal("hello api", got.Message);
                Assert.Equal(peer.HoloNETDNA.AgentPubKey, got.FromAgentPubKey);
                Assert.Equal("hello api", HoloNETAPI.ReceiveSignal());

                // API -> peer.
                var reply = await HoloNETAPI.SendSignalAsync("hello peer", new[] { got.FromAgentPubKey });
                Assert.True(reply.IsSuccess, reply.Message);
                Assert.Equal(1, reply.Result);
                await Until(() => peerGot.Any(d => System.Text.Encoding.UTF8.GetString(d).Contains("hello peer")));

                // Nobody to send to: nothing sent, not an error.
                Assert.Equal(0, (await HoloNETAPI.SendSignalAsync("nobody", null)).Result);
            }
            finally
            {
                HoloNETAPI.OnSignalReceived -= onSignal;
                await peer.DisconnectAsync();
                await admin.UninstallAppAsync(peerId);
                await admin.DisconnectAsync();
            }
        }

        private static async Task Until(Func<bool> condition, int seconds = 30)
        {
            DateTime end = DateTime.UtcNow.AddSeconds(seconds);
            while (!condition())
            {
                if (DateTime.UtcNow > end)
                    throw new TimeoutException("Timed out waiting for a signal.");
                await Task.Delay(50);
            }
        }

        [SkippableFact]
        public async Task Live_SaveLoadRestart_KeepsDataAndObjects()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath), "Needs HOLONET_LIVE_ADMIN_URI and HOLONET_LIVE_HAPP_PATH (see INTEGRATION_TESTING.md).");

            string appId = "holonet-api-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string dnaPath = Path.Combine(Path.GetTempPath(), appId + ".json");
            var dna = HoloNETAPI.HoloNETClientAdmin.HoloNETDNA;
            dna.HolochainConductorAdminURI = AdminUri;
            dna.AutoStartHolochainConductor = false;
            dna.AutoShutdownHolochainConductor = false;
            HoloNETAPIDNAManager.HoloNETAPIDNAPath = dnaPath;
            // The single-entry store (HoloNET API <= 4.4.2, StorageMode "SingleEntry"); per-key storage: LiveHoloNETAPIPerKeyTests.
            HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA { StoreId = appId, StorageMode = "SingleEntry" };
            HoloNETAPIDNAManager.SaveDNA();
            var errors = new System.Collections.Generic.List<string>();
            EventHandler<string> onError = (s, e) => errors.Add(e);
            HoloNETAPI.OnError += onError;

            try
            {
                Assert.True(await Step("init", HoloNETAPI.InitHoloNETAsync(appId, HappPath, "oasis")), "init: " + HoloNETAPI.LastError);
                Assert.True(HoloNETAPI.IsInitialized);

                // Nothing saved yet: an empty store, not an error.
                var empty = await Step("load empty", HoloNETAPI.LoadAllDataAsync());
                Assert.True(empty.IsSuccess, empty.Message);
                Assert.Empty(empty.Result);

                var saved = await Step("save data", HoloNETAPI.SaveDataAsync("colour", "green"));
                Assert.True(saved.IsSuccess && saved.Result, saved.Message);
                Assert.False(string.IsNullOrEmpty(HoloNETAPI.HoloNETAPIDNA.EntryHash), "entry hash not remembered");
                string firstHash = HoloNETAPI.HoloNETAPIDNA.EntryHash;

                var savedObject = await Step("save object", HoloNETAPI.SaveObjectAsync("person", new ApiTestObject { FirstName = "Ada", LastName = "Lovelace", Age = 36 }));
                Assert.True(savedObject.IsSuccess, savedObject.Message);
                Assert.NotEqual(firstHash, HoloNETAPI.HoloNETAPIDNA.EntryHash); // the second save updated the same entry

                var savedAll = await Step("save all objects (hApp overload)", HoloNETAPI.SaveAllObjectsAsync(appId, HappPath, "oasis", "oasis"));
                Assert.True(savedAll.IsSuccess, savedAll.Message);

                // Read back from the DHT, not the cache.
                HoloNETAPI.Data.Clear();
                HoloNETAPI.Objects.Clear();
                var colour = await Step("load data", HoloNETAPI.LoadDataAsync("colour", useCache: false));
                Assert.True(colour.IsSuccess, colour.Message);
                Assert.Equal("green", colour.Result);
                var person = await Step("load object", HoloNETAPI.LoadObjectAsync<ApiTestObject>("person", useCache: false));
                Assert.True(person.IsSuccess, person.Message);
                Assert.Equal("Lovelace", person.Result.LastName);
                Assert.Equal(36, person.Result.Age);
                var cached = await HoloNETAPI.LoadObjectAsync<ApiTestObject>("person");
                Assert.True(cached.IsSuccess, cached.Message);

                var missing = await Step("load missing", HoloNETAPI.LoadDataAsync("nope", useCache: false));
                Assert.False(missing.IsSuccess);
                Assert.Contains("nope", missing.Message);
                var missingObject = await Step("load missing object", HoloNETAPI.LoadObjectAsync<ApiTestObject>("nobody", useCache: false));
                Assert.False(missingObject.IsSuccess);
                var wrongType = await Step("load as wrong type", HoloNETAPI.LoadObjectAsync<int>("person", useCache: false));
                Assert.False(wrongType.IsSuccess);

                // Restart: the installed app (and its agent) is reused rather than reinstalled, so the data is still there.
                await HoloNETAPI.ShutdownAsync();
                Assert.False(HoloNETAPI.IsInitialized);
                Assert.True(await Step("re-init", HoloNETAPI.InitHoloNETAsync(appId, HappPath, "oasis")), "re-init: " + HoloNETAPI.LastError);
                var again = await Step("load after restart", HoloNETAPI.LoadDataAsync(appId, HappPath, "oasis", "colour", false));
                Assert.True(again.IsSuccess, again.Message);
                Assert.Equal("green", again.Result);

                var updated = await Step("save after restart", HoloNETAPI.SaveDataAsync(appId, HappPath, "oasis", "colour", "blue", "oasis"));
                Assert.True(updated.IsSuccess, updated.Message);
                var all = await Step("load all", HoloNETAPI.LoadAllDataAsync(appId, HappPath, "oasis", null));
                Assert.Equal("blue", all.Result["colour"]);
                var allObjects = await Step("load all objects", HoloNETAPI.LoadAllObjectsAsync(appId, HappPath, "oasis", null));
                Assert.True(allObjects.IsSuccess, allObjects.Message);
                Assert.True(allObjects.Result.ContainsKey("person"));
                var objectByApp = await Step("load object (hApp overload)", HoloNETAPI.LoadObjectAsync<ApiTestObject>(appId, HappPath, "oasis", "person", false));
                Assert.Equal("Ada", objectByApp.Result.FirstName);
                Assert.True((await Step("save object (hApp overload)", HoloNETAPI.SaveObjectAsync(appId, HappPath, "oasis", "n", 5))).IsSuccess);
                Assert.True((await Step("save all data (hApp overload)", HoloNETAPI.SaveAllDataAsync(appId, HappPath, "oasis", "oasis"))).IsSuccess);

                // The DNA file was written and loads back.
                var reloaded = HoloNETAPIDNAManager.LoadDNA(dnaPath);
                Assert.NotNull(reloaded);
                Assert.Equal(HoloNETAPI.HoloNETAPIDNA.EntryHash, reloaded.EntryHash);

                // A zome error (unknown function) is reported, not swallowed.
                var bad = await Step("bad function", HoloNETAPI.LoadAllDataAsync("oasis", "no_such_function"));
                Assert.False(bad.IsSuccess);
                Assert.Contains(errors, e => e.Contains("no_such_function") || e.Contains("LoadAllDataAsync") || e.Contains("loading"));
                Assert.True(HoloNETAPI.SendSignal());
                Assert.Equal("", HoloNETAPI.ReceiveSignal());

                // Signals: a second agent (its own installed app) and the API agent message each other directly.
                await SignalsBetweenTheApiAndAnotherAgent();

                // Deletes, including straight after a restart (nothing loaded yet: a save would otherwise merge the stored
                // key back in), and they survive another restart.
                Assert.True((await Step("save k1", HoloNETAPI.SaveDataAsync("del-k1", "v1"))).IsSuccess);
                Assert.True((await Step("save k2", HoloNETAPI.SaveDataAsync("del-k2", "v2"))).IsSuccess);
                Assert.True((await Step("save o1", HoloNETAPI.SaveObjectAsync("del-o1", new ApiTestObject { FirstName = "Del" }))).IsSuccess);
                await HoloNETAPI.ShutdownAsync();
                var delK1 = await Step("delete k1 after restart", HoloNETAPI.DeleteDataAsync("del-k1"));
                Assert.True(delK1.IsSuccess, delK1.Message);
                Assert.True(delK1.Result);
                Assert.False((await Step("delete missing", HoloNETAPI.DeleteDataAsync("no-such-key"))).Result);
                Assert.True((await Step("delete o1", HoloNETAPI.DeleteObjectAsync("del-o1"))).Result);
                await HoloNETAPI.ShutdownAsync();
                var afterDelete = await Step("reload after delete", HoloNETAPI.LoadAllDataAsync());
                Assert.True(afterDelete.IsSuccess, afterDelete.Message);
                Assert.False(afterDelete.Result.ContainsKey("del-k1"));
                Assert.Equal("v2", afterDelete.Result["del-k2"]);
                Assert.False((await Step("reload objects after delete", HoloNETAPI.LoadAllObjectsAsync())).Result.ContainsKey("del-o1"));
                Assert.True((await Step("delete all data", HoloNETAPI.DeleteAllDataAsync())).Result);
                await HoloNETAPI.ShutdownAsync();
                Assert.Empty((await Step("reload after delete all", HoloNETAPI.LoadAllDataAsync())).Result);
                Assert.True((await Step("delete all objects", HoloNETAPI.DeleteAllObjectsAsync())).IsSuccess);
            }
            finally
            {
                HoloNETAPI.OnError -= onError;
                await HoloNETAPI.ShutdownAsync();
                var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
                await admin.ConnectAsync(AdminUri);
                await admin.UninstallAppAsync(appId);
                await admin.DisconnectAsync();
                HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA();
                try { File.Delete(dnaPath); } catch { }
            }
        }
    }
}
