using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.API;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// HoloNET API 4.5 on a real conductor (0.7.0 / 0.7.1) and the OASIS hApp: per-key storage (one entry per key), typed
    /// loads, key listing, sync versions, migrating a single-entry store, shared data, change notifications and broadcasts
    /// (with a second agent on the other end).
    /// </summary>
    [Collection(LiveConductor.Collection)]
    public class LiveHoloNETAPIPerKeyTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");
        private static readonly string HappPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH");

        private static async Task<T> Step<T>(string name, Task<T> task, int seconds = 120)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds))) != task)
                throw new TimeoutException($"'{name}' did not complete within {seconds}s.");
            return await task;
        }

        private static async Task Until(string what, Func<bool> condition, int seconds = 30)
        {
            DateTime end = DateTime.UtcNow.AddSeconds(seconds);
            while (!condition())
            {
                if (DateTime.UtcNow > end) throw new TimeoutException("Timed out waiting for: " + what);
                await Task.Delay(50);
            }
        }

        private static void Point()
        {
            var dna = HoloNETAPI.HoloNETClientAdmin.HoloNETDNA;
            dna.HolochainConductorAdminURI = AdminUri;
            dna.AutoStartHolochainConductor = false;
            dna.AutoShutdownHolochainConductor = false;
        }

        /// <summary>A fresh API store (own app, own settings file), cleaned up afterwards.</summary>
        private static async Task WithApi(Func<string, HoloNETAPIHapp, Task> body, string storageMode = "PerKey")
        {
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath), "Needs HOLONET_LIVE_ADMIN_URI and HOLONET_LIVE_HAPP_PATH (see INTEGRATION_TESTING.md).");
            string appId = "holonet-api-pk-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string dnaPath = Path.Combine(Path.GetTempPath(), appId + ".json");
            Point();
            await HoloNETAPI.ShutdownAsync();
            HoloNETAPIDNAManager.HoloNETAPIDNAPath = dnaPath;
            HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA { StoreId = appId, StorageMode = storageMode };
            HoloNETAPIDNAManager.SaveDNA();
            var happ = new HoloNETAPIHapp(appId, HappPath, "oasis");

            try
            {
                Assert.True(await Step("init", HoloNETAPI.InitHoloNETAsync(happ)), HoloNETAPI.LastError);
                await body(appId, happ);
            }
            finally
            {
                await HoloNETAPI.ShutdownAsync();
                var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
                await admin.ConnectAsync(AdminUri);
                await admin.UninstallAppAsync(appId);
                await admin.DisconnectAsync();
                HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA();
                try { File.Delete(dnaPath); } catch { }
            }
        }

        public enum Colour { Red, Green }

        [SkippableFact]
        public async Task Live_PerKey_SaveLoadDeleteListTypedSync()
        {
            await WithApi(async (appId, happ) =>
            {
                Assert.True(HoloNETAPI.IsPerKeyStorage);

                // One line each; each key is its own entry.
                Assert.True((await Step("save a", HoloNETAPI.SaveDataAsync("a", "1"))).IsSuccess, HoloNETAPI.LastError);
                Assert.True((await Step("save b", HoloNETAPI.SaveDataAsync("b", "two"))).IsSuccess);
                Assert.True((await Step("save obj", HoloNETAPI.SaveObjectAsync("p", new ApiTestObject { FirstName = "Ada", Age = 36 }))).IsSuccess);
                Assert.True((await Step("overwrite a", HoloNETAPI.SaveDataAsync("a", "3"))).IsSuccess);

                // Typed saves and loads (invariant culture), enums, dates, Guids, nullable.
                var when = new DateTime(2026, 10, 16, 9, 30, 0, DateTimeKind.Utc);
                var id = Guid.NewGuid();
                Assert.True((await HoloNETAPI.SaveDataAsync("pi", 3.25)).IsSuccess);
                Assert.True((await HoloNETAPI.SaveDataAsync("on", true)).IsSuccess);
                Assert.True((await HoloNETAPI.SaveDataAsync("when", when.ToString("o"))).IsSuccess);
                Assert.True((await HoloNETAPI.SaveDataAsync("colour", Colour.Green)).IsSuccess);
                Assert.True((await HoloNETAPI.SaveDataAsync("id", id)).IsSuccess);

                // Restart: everything comes back from the DHT, one key at a time.
                await HoloNETAPI.ShutdownAsync();
                Assert.Equal("3", (await Step("load a", HoloNETAPI.LoadDataAsync("a"))).Result);
                Assert.Equal(3, (await HoloNETAPI.LoadDataAsync<int>("a")).Result);
                Assert.Equal(3.25, (await HoloNETAPI.LoadDataAsync<double>("pi")).Result);
                Assert.True((await HoloNETAPI.LoadDataAsync<bool>("on")).Result);
                Assert.Equal(when, (await HoloNETAPI.LoadDataAsync<DateTime>("when")).Result.ToUniversalTime());
                Assert.Equal(Colour.Green, (await HoloNETAPI.LoadDataAsync<Colour>("colour")).Result);
                Assert.Equal(id, (await HoloNETAPI.LoadDataAsync<Guid>("id")).Result);
                Assert.Equal(3, (await HoloNETAPI.LoadDataAsync<int?>("a")).Result);
                var notANumber = await HoloNETAPI.LoadDataAsync<int>("b");
                Assert.False(notANumber.IsSuccess);
                Assert.Contains("is not a Int32", notANumber.Message);
                Assert.Equal(36, (await HoloNETAPI.LoadObjectAsync<ApiTestObject>("p")).Result.Age);

                // Listing and existence.
                Assert.Equal(new[] { "a", "b", "colour", "id", "on", "pi", "when" }, (await HoloNETAPI.ListKeysAsync()).Result.ToArray());
                Assert.Equal(new[] { "p" }, (await HoloNETAPI.ListKeysAsync(objects: true)).Result.ToArray());
                Assert.True((await HoloNETAPI.ExistsAsync("b")).Result);
                Assert.False((await HoloNETAPI.ExistsAsync("nope")).Result);
                Assert.True((await HoloNETAPI.ExistsAsync("p", objects: true)).Result);

                // Missing key: a clear failure, not an exception.
                var missing = await HoloNETAPI.LoadDataAsync("nope", useCache: false);
                Assert.False(missing.IsSuccess);
                Assert.Contains("No data has been saved under the key 'nope'", missing.Message);

                // Deletes (one, then it stays deleted across a restart).
                Assert.True((await HoloNETAPI.DeleteDataAsync("b")).Result);
                Assert.False((await HoloNETAPI.DeleteDataAsync("b")).Result);
                Assert.True((await HoloNETAPI.DeleteObjectAsync("p")).Result);
                await HoloNETAPI.ShutdownAsync();
                Assert.False((await HoloNETAPI.ExistsAsync("b")).Result);
                Assert.False((await HoloNETAPI.LoadObjectAsync<ApiTestObject>("p", useCache: false)).IsSuccess);
                Assert.DoesNotContain("b", (await HoloNETAPI.ListKeysAsync()).Result);

                // SaveAll writes only what changed, and keys removed from Data become deletes.
                var all = await HoloNETAPI.LoadAllDataAsync();
                Assert.True(all.IsSuccess, all.Message);
                HoloNETAPI.Data["a"] = "4";
                HoloNETAPI.Data["new"] = "n";
                HoloNETAPI.Data.Remove("pi");
                var saveAll = await HoloNETAPI.SaveAllDataAsync();
                Assert.True(saveAll.IsSuccess, saveAll.Message);
                await HoloNETAPI.ShutdownAsync();
                var reloaded = (await HoloNETAPI.LoadAllDataAsync()).Result;
                Assert.Equal("4", reloaded["a"]);
                Assert.Equal("n", reloaded["new"]);
                Assert.False(reloaded.ContainsKey("pi"));
                Assert.Equal("Green", reloaded["colour"]);

                // Delete all values (objects kept), delete all objects.
                Assert.True((await HoloNETAPI.SaveObjectAsync("keep", new ApiTestObject { FirstName = "K" })).IsSuccess);
                Assert.True((await HoloNETAPI.DeleteAllDataAsync()).Result);
                Assert.Empty((await HoloNETAPI.ListKeysAsync()).Result);
                Assert.Equal(new[] { "keep" }, (await HoloNETAPI.ListKeysAsync(objects: true)).Result.ToArray());
                Assert.True((await HoloNETAPI.DeleteAllObjectsAsync()).Result);
                Assert.Empty((await HoloNETAPI.ListKeysAsync(objects: true)).Result);

                // Sync versions (no async needed).
                Assert.True(HoloNETAPI.SaveData("s", "sync").IsSuccess);
                Assert.Equal("sync", HoloNETAPI.LoadData("s", useCache: false).Result);
                Assert.Equal(7, HoloNETAPI.SaveData("seven", "7").IsSuccess ? HoloNETAPI.LoadData<int>("seven").Result : -1);
                Assert.True(HoloNETAPI.Exists("s").Result);
                Assert.Contains("s", HoloNETAPI.ListKeys().Result);
                Assert.True(HoloNETAPI.SaveObject("so", new ApiTestObject { FirstName = "S" }).IsSuccess);
                Assert.Equal("S", HoloNETAPI.LoadObject<ApiTestObject>("so", useCache: false).Result.FirstName);
                Assert.True(HoloNETAPI.DeleteObject("so").Result);
                Assert.True(HoloNETAPI.DeleteData("s").Result);
                Assert.False(HoloNETAPI.Exists("s").Result);

                // Empty key: refused.
                Assert.False((await HoloNETAPI.SaveDataAsync("", "x")).IsSuccess);
            });
        }

        [SkippableFact]
        public async Task Live_PerKey_MigratesASingleEntryStore()
        {
            await WithApi(async (appId, happ) =>
            {
                // Saved the old way (HoloNET API <= 4.4.2).
                Assert.False(HoloNETAPI.IsPerKeyStorage);
                Assert.True((await HoloNETAPI.SaveDataAsync("old", "value")).IsSuccess, HoloNETAPI.LastError);
                Assert.True((await HoloNETAPI.SaveObjectAsync("oldobj", new ApiTestObject { FirstName = "Old", Age = 9 })).IsSuccess);
                Assert.False(string.IsNullOrEmpty(HoloNETAPI.HoloNETAPIDNA.EntryHash));

                // Upgrade: switch to per-key; the next start copies the store over once.
                await HoloNETAPI.ShutdownAsync();
                HoloNETAPI.HoloNETAPIDNA.StorageMode = "PerKey";
                HoloNETAPIDNAManager.SaveDNA();
                Assert.True(await HoloNETAPI.InitHoloNETAsync(happ), HoloNETAPI.LastError);
                Assert.True(HoloNETAPI.HoloNETAPIDNA.MigratedToPerKey);
                Assert.Equal("value", (await HoloNETAPI.LoadDataAsync("old", useCache: false)).Result);
                Assert.Equal(9, (await HoloNETAPI.LoadObjectAsync<ApiTestObject>("oldobj", useCache: false)).Result.Age);
                Assert.Equal(new[] { "old" }, (await HoloNETAPI.ListKeysAsync()).Result.ToArray());
            }, storageMode: "SingleEntry");
        }

        [SkippableFact]
        public async Task Live_SharedData_ChangeNotifications_Broadcast()
        {
            await WithApi(async (appId, happ) =>
            {
                string ns = "board-" + Guid.NewGuid().ToString("N").Substring(0, 6);

                // A second agent (its own app), talking the same protocol with plain zome calls.
                string peerId = "holonet-api-pk-peer-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
                await admin.ConnectAsync(AdminUri);
                var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(peerId, HappPath, "oasis");
                Assert.True(installed.IsAppConnected, installed.Message);
                IHoloNETClientAppAgent peer = installed.HoloNETClientAppAgent;
                var peerSignals = new ConcurrentQueue<Dictionary<object, object>>();
                peer.OnSignalCallBack += (s, e) =>
                {
                    try { peerSignals.Enqueue(MessagePack.MessagePackSerializer.Deserialize<Dictionary<object, object>>(e.RawSignalData.Data)); } catch { }
                };

                var changes = new ConcurrentQueue<HoloNETAPIDataChangedEventArgs>();
                var messages = new ConcurrentQueue<HoloNETAPISignalEventArgs>();
                EventHandler<HoloNETAPIDataChangedEventArgs> onChanged = (s, e) => changes.Enqueue(e);
                EventHandler<HoloNETAPISignalEventArgs> onMessage = (s, e) => messages.Enqueue(e);
                HoloNETAPI.OnDataChanged += onChanged;
                HoloNETAPI.OnSignalReceived += onMessage;

                try
                {
                    // The peer subscribes to the namespace's changes and to broadcasts.
                    Assert.False((await peer.CallZomeFunctionAsync("oasis", "hypernet_join", new Dictionary<string, object> { ["session"] = "holonet-api-changes/" + ns, ["player_name"] = "peer" })).IsError);
                    Assert.False((await peer.CallZomeFunctionAsync("oasis", "hypernet_join", new Dictionary<string, object> { ["session"] = "holonet-api", ["player_name"] = "peer" })).IsError);

                    // Shared data: the API writes, the peer is told and can read it from the DHT.
                    Assert.True((await HoloNETAPI.SubscribeToChangesAsync(ns)).IsSuccess);
                    Assert.True((await Step("save shared", HoloNETAPI.SaveSharedDataAsync(ns, "high-score", "9001"))).IsSuccess, HoloNETAPI.LastError);
                    await Until("the peer is told", () => peerSignals.Any(sig => sig.TryGetValue("session", out object se) && se as string == "holonet-api-changes/" + ns));
                    var peerRead = await peer.CallZomeFunctionAsync("oasis", "get_holon_by_id", "holonet-api-shared/" + ns + "/data/high-score");
                    Assert.Equal("9001", peerRead.Records[0].EntryKeyValuePairs["holonet_api_value"].ToString());

                    // The peer changes it (same entry shape) and announces it: the API sees the change and the new value.
                    var update = await peer.CallZomeFunctionAsync("oasis", "update_entry_holon", new Dictionary<string, object>
                    {
                        ["original_action_hash"] = peer.ConvertHoloHashToBytes(peerRead.Records[0].ActionHash),
                        ["updated_entry"] = new Dictionary<string, object>
                        {
                            ["id"] = "holonet-api-shared/" + ns + "/data/high-score", ["parent_holon_id"] = "holonet-api-shared/" + ns + "/data",
                            ["holonet_api_key"] = "high-score", ["holonet_api_kind"] = "data", ["holonet_api_value"] = "9500", ["holonet_api_deleted"] = false
                        }
                    });
                    Assert.False(update.IsError, update.Message);
                    Assert.False((await peer.CallZomeFunctionAsync("oasis", "hypernet_send", new Dictionary<string, object>
                    {
                        ["session"] = "holonet-api-changes/" + ns,
                        ["payload"] = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object> { ["ns"] = ns, ["key"] = "high-score", ["object"] = false, ["deleted"] = false }),
                        ["to"] = null
                    })).IsError);
                    await Until("the API is told", () => changes.Any(c => c.Key == "high-score"));
                    var change = changes.First(c => c.Key == "high-score");
                    Assert.Equal(ns, change.Namespace);
                    Assert.Equal(peer.HoloNETDNA.AgentPubKey, change.ByAgentPubKey);
                    Assert.False(change.IsDeleted);
                    Assert.Equal(9500, (await HoloNETAPI.LoadSharedDataAsync<int>(ns, "high-score")).Result);

                    // Shared objects, listing, deleting.
                    Assert.True((await HoloNETAPI.SaveSharedObjectAsync(ns, "leader", new ApiTestObject { FirstName = "Ada", Age = 1 })).IsSuccess);
                    Assert.Equal("Ada", (await HoloNETAPI.LoadSharedObjectAsync<ApiTestObject>(ns, "leader")).Result.FirstName);
                    Assert.Equal(new[] { "high-score" }, (await HoloNETAPI.ListSharedKeysAsync(ns)).Result.ToArray());
                    Assert.Equal(new[] { "leader" }, (await HoloNETAPI.ListSharedKeysAsync(ns, objects: true)).Result.ToArray());
                    Assert.Equal("9500", (await HoloNETAPI.LoadAllSharedDataAsync(ns)).Result["high-score"]);
                    Assert.True((await HoloNETAPI.DeleteSharedDataAsync(ns, "high-score")).IsSuccess);
                    Assert.False((await HoloNETAPI.LoadSharedDataAsync(ns, "high-score")).IsSuccess);
                    Assert.True((await HoloNETAPI.DeleteSharedObjectAsync(ns, "leader")).IsSuccess);
                    Assert.Equal("v", HoloNETAPI.SaveSharedData(ns, "k", "v").IsSuccess ? HoloNETAPI.LoadSharedData(ns, "k").Result : null);
                    Assert.True(HoloNETAPI.DeleteSharedData(ns, "k").IsSuccess);

                    // Unsubscribed: no more OnDataChanged for this namespace.
                    Assert.True((await HoloNETAPI.UnsubscribeFromChangesAsync(ns)).IsSuccess);

                    // Broadcast: everyone in the API's zone (here, the peer).
                    var all = await Step("broadcast", HoloNETAPI.SendSignalToAllAsync("hello everyone"));
                    Assert.True(all.IsSuccess, all.Message);
                    Assert.True(all.Result >= 1, "sent to " + all.Result);
                    await Until("the peer gets the broadcast", () => peerSignals.Any(sig => sig.TryGetValue("payload", out object p) && p is byte[] b && System.Text.Encoding.UTF8.GetString(b) == "hello everyone"));
                    Assert.True(HoloNETAPI.SendSignalToAll("sync hello").IsSuccess);
                    Assert.True(HoloNETAPI.SendSignal("direct", new[] { peer.HoloNETDNA.AgentPubKey }).Result == 1);
                }
                finally
                {
                    HoloNETAPI.OnDataChanged -= onChanged;
                    HoloNETAPI.OnSignalReceived -= onMessage;
                    await peer.DisconnectAsync();
                    await admin.UninstallAppAsync(peerId);
                    await admin.DisconnectAsync();
                }
            });
        }
    }
}
