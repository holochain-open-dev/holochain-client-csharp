using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.API;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>HoloNET API against a real conductor: every hApp overload initialising on first use, caching, reconnecting,
    /// undecodable stored data, and failures to connect or install.</summary>
    [Collection(LiveConductor.Collection)]
    public class LiveHoloNETAPIEdgeTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");
        private static readonly string HappPath = Environment.GetEnvironmentVariable("HOLONET_LIVE_HAPP_PATH");

        private static void Point(string adminUri)
        {
            var dna = HoloNETAPI.HoloNETClientAdmin.HoloNETDNA;
            dna.HolochainConductorAdminURI = adminUri;
            dna.AutoStartHolochainConductor = false;
            dna.AutoShutdownHolochainConductor = false;
        }

        private static async Task Uninstall(string appId)
        {
            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
            await admin.ConnectAsync(AdminUri);
            await admin.UninstallAppAsync(appId);
            await admin.DisconnectAsync();
        }

        [SkippableFact]
        public async Task Live_HappOverloads_CacheReconnectAndBadStoredData()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath), "needs a live conductor");
            string appId = "holonet-api2-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string dnaPath = Path.Combine(Path.GetTempPath(), appId + ".json");
            Point(AdminUri);
            HoloNETAPIDNAManager.HoloNETAPIDNAPath = dnaPath;
            // This test covers the single-entry store (HoloNET API <= 4.4.2, and StorageMode "SingleEntry"): its entry hash,
            // corrupt stored fields, refusing to overwrite. Per-key storage has its own tests (LiveHoloNETAPIPerKeyTests).
            HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA { StoreId = appId, StorageMode = "SingleEntry" };
            HoloNETAPIDNAManager.SaveDNA();

            try
            {
                // Every hApp overload initialises HoloNET itself when it isn't yet.
                var happ = new HoloNETAPIHapp(appId, HappPath, "oasis");
                Assert.True((await HoloNETAPI.SaveDataAsync(happ, "k", "v")).IsSuccess, HoloNETAPI.LastError);
                await HoloNETAPI.ShutdownAsync();
                Assert.True((await HoloNETAPI.SaveObjectAsync(happ, "o", new ApiTestObject { FirstName = "F", Age = 3 })).IsSuccess, HoloNETAPI.LastError);
                await HoloNETAPI.ShutdownAsync();
                Assert.True((await HoloNETAPI.SaveAllDataAsync(happ)).IsSuccess);
                await HoloNETAPI.ShutdownAsync();
                Assert.True((await HoloNETAPI.SaveAllObjectsAsync(happ)).IsSuccess);
                await HoloNETAPI.ShutdownAsync();
                Assert.Equal("v", (await HoloNETAPI.LoadDataAsync(happ, "k")).Result);
                await HoloNETAPI.ShutdownAsync();
                Assert.Equal("v", (await HoloNETAPI.LoadAllDataAsync(happ)).Result["k"]);
                await HoloNETAPI.ShutdownAsync();
                Assert.True((await HoloNETAPI.LoadAllObjectsAsync(happ)).Result.ContainsKey("o"));
                await HoloNETAPI.ShutdownAsync();
                Assert.Equal(3, (await HoloNETAPI.LoadObjectAsync<ApiTestObject>(happ, "o")).Result.Age);
                await HoloNETAPI.ShutdownAsync();
                Assert.True(await HoloNETAPI.InitHoloNETAsync(happ));
                Assert.Equal("oasis", new HoloNETAPIHapp().RoleName);

                // Cached values: a string, an object saved in this session (already a T), and one that converts.
                Assert.Equal("v", (await HoloNETAPI.LoadDataAsync("k")).Result);
                var person = new ApiTestObject { FirstName = "Cached" };
                Assert.True((await HoloNETAPI.SaveObjectAsync("p", person)).IsSuccess);
                Assert.Same(person, (await HoloNETAPI.LoadObjectAsync<ApiTestObject>("p")).Result);
                Assert.True((await HoloNETAPI.SaveObjectAsync("n", 42)).IsSuccess);
                Assert.Equal(42L, (await HoloNETAPI.LoadObjectAsync<long>("n")).Result);

                // InitHoloNETAsync when already connected, and when the app connection dropped (it reconnects).
                Assert.True(await HoloNETAPI.InitHoloNETAsync(happ));
                await HoloNETAPI.HoloNETClient.DisconnectAsync();
                Assert.True(await HoloNETAPI.InitHoloNETAsync(appId, HappPath, "oasis"), HoloNETAPI.LastError);
                Assert.Equal(System.Net.WebSockets.WebSocketState.Open, HoloNETAPI.HoloNETClient.State);

                // Losing the remembered hash: the next load finds the entry by StoreId and remembers it again.
                string hash = HoloNETAPI.HoloNETAPIDNA.EntryHash;
                HoloNETAPI.HoloNETAPIDNA.EntryHash = null;
                Assert.True((await HoloNETAPI.LoadAllDataAsync()).IsSuccess);
                Assert.Equal(hash, HoloNETAPI.HoloNETAPIDNA.EntryHash);

                // Stored fields that aren't valid JSON are reported, not thrown.
                var corrupt = await HoloNETAPI.HoloNETClient.CallZomeFunctionAsync("oasis", "update_entry_avatar", new Dictionary<string, object>
                {
                    ["original_action_hash"] = HoloNETAPI.HoloNETClient.ConvertHoloHashToBytes(hash),
                    ["updated_entry"] = new Dictionary<string, object> { ["id"] = appId, ["username"] = appId, ["email"] = "", ["holonet_api_data"] = "{not json", ["holonet_api_objects"] = "[1" }
                });
                Assert.False(corrupt.IsError, corrupt.Message);
                var badData = await HoloNETAPI.LoadAllDataAsync();
                Assert.False(badData.IsSuccess);
                Assert.Contains("decoding the stored data", badData.Message);
                var badObjects = await HoloNETAPI.LoadAllObjectsAsync();
                Assert.False(badObjects.IsSuccess);
                Assert.Contains("decoding the stored objects", badObjects.Message);
                Assert.False((await HoloNETAPI.LoadDataAsync("k", useCache: false)).IsSuccess);
                Assert.False((await HoloNETAPI.LoadObjectAsync<ApiTestObject>("o", useCache: false)).IsSuccess);

                // After a restart neither half is loaded, and the stored data can't be read, so saving refuses rather than
                // overwriting it.
                await HoloNETAPI.ShutdownAsync();
                await HoloNETAPI.InitHoloNETAsync(appId, HappPath, "oasis");
                var refused = await HoloNETAPI.SaveDataAsync("k2", "v2");
                Assert.False(refused.IsSuccess);
                Assert.Contains("loading the stored data first", refused.Message);
                HoloNETAPI.Data.Clear();
                var refusedObjects = await HoloNETAPI.LoadAllDataAsync("oasis", "get_avatar_by_id");
                Assert.False(refusedObjects.IsSuccess);
            }
            finally
            {
                await HoloNETAPI.ShutdownAsync();
                await Uninstall(appId);
                HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA();
                try { File.Delete(dnaPath); } catch { }
            }
        }

        [SkippableFact]
        public async Task Live_InitFailures_AreReported()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(HappPath), "needs a live conductor");
            var errors = new List<string>();
            EventHandler<string> onError = (s, e) => errors.Add(e);
            HoloNETAPI.OnError += onError;
            HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA { StoreId = "never-" + Guid.NewGuid().ToString("N") };

            try
            {
                // An hApp that isn't there: the install fails.
                Point(AdminUri);
                await HoloNETAPI.ShutdownAsync();
                Assert.False(await HoloNETAPI.InitHoloNETAsync("holonet-api-missing", Path.Combine(Path.GetTempPath(), "no-such-" + Guid.NewGuid().ToString("N") + ".happ"), "oasis"));
                Assert.Contains("InstallEnableSignAttachAndConnectToHappAsync", HoloNETAPI.LastError);
                await HoloNETAPI.ShutdownAsync();

                // No conductor at the admin URI: nothing can be loaded or saved, and each call says why.
                Point("ws://127.0.0.1:1");
                Assert.False(await HoloNETAPI.InitHoloNETAsync());
                var load = await HoloNETAPI.LoadAllDataAsync();
                Assert.False(load.IsSuccess);
                Assert.Contains("could not be initialised", load.Message);
                var save = await HoloNETAPI.SaveAllDataAsync();
                Assert.False(save.IsSuccess);
                Assert.Contains("could not be initialised", save.Message);
                Assert.NotEmpty(errors);

                // A subscriber that throws can't break the API.
                HoloNETAPI.OnError += (s, e) => throw new InvalidOperationException("bad subscriber");
                Assert.False((await HoloNETAPI.LoadAllObjectsAsync()).IsSuccess);
            }
            finally
            {
                HoloNETAPI.OnError -= onError;
                Point(AdminUri);
                await HoloNETAPI.ShutdownAsync();
                HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA();
            }
        }

        [Fact]
        public void DnaManager_LoadAndSaveEdgeCases()
        {
            Assert.Throws<ArgumentNullException>(() => HoloNETAPIDNAManager.LoadDNA(""));
            Assert.Null(HoloNETAPIDNAManager.LoadDNA(Path.Combine(Path.GetTempPath(), "missing-" + Guid.NewGuid().ToString("N") + ".json")));
            Assert.False(HoloNETAPIDNAManager.SaveDNA("", new HoloNETAPIDNA()));
            Assert.False(HoloNETAPIDNAManager.SaveDNA(Path.Combine(Path.GetTempPath(), "x.json"), null));

            string path = Path.Combine(Path.GetTempPath(), "holonet-apidna-" + Guid.NewGuid().ToString("N"), "dna.json");
            Assert.True(HoloNETAPIDNAManager.SaveDNA(path, new HoloNETAPIDNA { EntryHash = "h", StoreId = "s" }));
            var loaded = HoloNETAPIDNAManager.LoadDNA(path);
            Assert.Equal("h", loaded.EntryHash);
            Assert.Equal("s", loaded.StoreId);
            Assert.True(HoloNETAPIDNAManager.IsLoaded);
            Assert.True(HoloNETAPIDNAManager.SaveDNA());
            Directory.Delete(Path.GetDirectoryName(path), true);
            HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA();
        }
    }
}
