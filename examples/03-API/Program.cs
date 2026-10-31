// HoloNET example 03: HoloNET API - one line for everything
//
// HoloNET API is the simplest way in: static one-line calls to save / load / delete data and objects, share data with
// everyone in the app and hear when it changes, and message other agents. No Rust, no conductor plumbing: on first use it
// connects, installs the hApp and gets everything ready by itself.
//
// Run: start a conductor (examples/start-conductor.sh), then   dotnet run --project examples/03-API
// Docs: https://docs.holonet.oasisomniverse.one/api-guide  ·  API reference: docs/api/API.md

using System.Collections.Concurrent;
using HoloNET.Examples;
using NextGenSoftware.Holochain.HoloNET.API;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

// ---- Setup (only because the example uses a conductor you started; in an app with an .Embedded package, skip this) --
// Use the conductor you started, without console logging (true shows every message to and from the conductor).
HoloNETAPI.UseConductor(ExampleSetup.AdminUri, logToConsole: false);
// The API keeps its settings (which hApp, your store id) in HoloNETAPIDNA.json; give this run its own file and store.
string appId = ExampleSetup.UniqueAppId("example-api");
HoloNETAPIDNAManager.HoloNETAPIDNAPath = Path.Combine(Path.GetTempPath(), appId + ".json");
HoloNETAPIDNAManager.HoloNETAPIDNA = new HoloNETAPIDNA { StoreId = appId };
HoloNETAPIDNAManager.SaveDNA();
// Use the bundled OASIS hApp under this run's own app id (the API remembers it for every later call).
ExampleSetup.Check(await HoloNETAPI.InitHoloNETAsync(new HoloNETAPIHapp(appId, ExampleSetup.HappPath, "oasis")), "initialise: " + HoloNETAPI.LastError);

IHoloNETClientAppAgent otherPlayer = null;
var admin = new HoloNETClientAdmin(ExampleSetup.ConductorDNA());
string otherId = ExampleSetup.UniqueAppId("example-api-other");

try
{
    // ---- Your data: one line each ----------------------------------------------------------------------------------
    ExampleSetup.Title("Your data");
    await HoloNETAPI.SaveDataAsync("theme", "dark");
    Console.WriteLine("theme = " + (await HoloNETAPI.LoadDataAsync("theme")).Result);

    // Any type: numbers, bools, dates, Guids, enums (stored in the invariant culture).
    await HoloNETAPI.SaveDataAsync("score", 9001);
    await HoloNETAPI.SaveDataAsync("sound-on", true);
    int score = (await HoloNETAPI.LoadDataAsync<int>("score")).Result;
    bool soundOn = (await HoloNETAPI.LoadDataAsync<bool>("sound-on")).Result;
    Console.WriteLine($"score = {score} (an int), sound-on = {soundOn} (a bool)");

    // Results never throw: check IsSuccess and Message.
    var missing = await HoloNETAPI.LoadDataAsync("no-such-key");
    Console.WriteLine("missing key -> IsSuccess " + missing.IsSuccess + ": " + missing.Message);

    // What's there.
    Console.WriteLine("keys: " + string.Join(", ", (await HoloNETAPI.ListKeysAsync()).Result));
    Console.WriteLine("exists('theme')? " + (await HoloNETAPI.ExistsAsync("theme")).Result);

    // Delete.
    await HoloNETAPI.DeleteDataAsync("theme");
    Console.WriteLine("after delete, exists('theme')? " + (await HoloNETAPI.ExistsAsync("theme")).Result);

    // ---- Objects -------------------------------------------------------------------------------------------------
    ExampleSetup.Title("Objects");
    await HoloNETAPI.SaveObjectAsync("settings", new Settings { Name = "Ada", Level = 3 });
    Settings settings = (await HoloNETAPI.LoadObjectAsync<Settings>("settings", useCache: false)).Result;  // useCache: false reads the DHT again
    Console.WriteLine($"settings = {settings.Name}, level {settings.Level}");
    await HoloNETAPI.DeleteObjectAsync("settings");

    // ---- Everything at once ----------------------------------------------------------------------------------------
    ExampleSetup.Title("Everything at once");
    await HoloNETAPI.LoadAllDataAsync();         // fills HoloNETAPI.Data
    HoloNETAPI.Data["volume"] = "7";
    HoloNETAPI.Data.Remove("sound-on");
    await HoloNETAPI.SaveAllDataAsync();          // writes only what changed; removed keys are deleted
    Console.WriteLine("keys now: " + string.Join(", ", (await HoloNETAPI.ListKeysAsync()).Result));

    // ---- Another player (a second agent) for shared data and messages ---------------------------------------------
    // HoloNET API is one agent per app, so the "other player" here is a plain HoloNET client in its own installed app,
    // speaking the same protocol. In a real game it would be the API in someone else's copy of your app.
    ExampleSetup.Check((await admin.ConnectAsync(ExampleSetup.AdminUri)).IsConnected, "connect admin");
    var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(otherId, ExampleSetup.HappPath, "oasis");
    ExampleSetup.Check(installed.IsAppConnected, "install the other player: " + installed.Message);
    otherPlayer = installed.HoloNETClientAppAgent;
    var otherPlayerSignals = new ConcurrentQueue<SignalCallBackEventArgs>();
    otherPlayer.OnSignalCallBack += (s, e) => otherPlayerSignals.Enqueue(e);
    // It listens for messages and for changes to the "leaderboard" namespace (what SubscribeToChangesAsync does).
    await otherPlayer.CallZomeFunctionAsync("oasis", "hypernet_join", new Dictionary<string, object> { ["session"] = "holonet-api", ["player_name"] = "other" });
    await otherPlayer.CallZomeFunctionAsync("oasis", "hypernet_join", new Dictionary<string, object> { ["session"] = "holonet-api-changes/leaderboard", ["player_name"] = "other" });

    // ---- Shared data + change notifications ------------------------------------------------------------------------
    ExampleSetup.Title("Shared data and change notifications");
    HoloNETAPI.OnDataChanged += (s, e) => Console.WriteLine($"  OnDataChanged: {Short(e.ByAgentPubKey)} {(e.IsDeleted ? "deleted" : "changed")} {e.Namespace}/{e.Key}");
    await HoloNETAPI.SubscribeToChangesAsync("leaderboard");

    // Every agent in the app can read and write a namespace.
    await HoloNETAPI.SaveSharedDataAsync("leaderboard", "high-score", "9001");
    Console.WriteLine("high-score = " + (await HoloNETAPI.LoadSharedDataAsync<int>("leaderboard", "high-score")).Result);
    await WaitFor(() => otherPlayerSignals.Count > 0);
    Console.WriteLine($"the other player was told about the change ({otherPlayerSignals.Count} signal)");

    // The other player beats the score, and tells everyone subscribed (the API does both for you in SaveSharedDataAsync).
    await OtherPlayerSetsHighScore(otherPlayer, "9500");
    await Task.Delay(1500);   // give the notification a moment to arrive
    Console.WriteLine("high-score is now " + (await HoloNETAPI.LoadSharedDataAsync<int>("leaderboard", "high-score")).Result);

    await HoloNETAPI.SaveSharedObjectAsync("leaderboard", "champion", new Settings { Name = "Grace", Level = 99 });
    Console.WriteLine("champion = " + (await HoloNETAPI.LoadSharedObjectAsync<Settings>("leaderboard", "champion")).Result.Name);
    Console.WriteLine("shared keys: " + string.Join(", ", (await HoloNETAPI.ListSharedKeysAsync("leaderboard")).Result));
    await HoloNETAPI.UnsubscribeFromChangesAsync("leaderboard");

    // ---- Messages --------------------------------------------------------------------------------------------------
    ExampleSetup.Title("Messages");
    otherPlayerSignals.Clear();
    var direct = await HoloNETAPI.SendSignalAsync("hi, just you", new[] { otherPlayer.HoloNETDNA.AgentPubKey });
    var everyone = await HoloNETAPI.SendSignalToAllAsync("hi, everyone");
    await WaitFor(() => otherPlayerSignals.Count >= 2);
    Console.WriteLine($"direct message sent to {direct.Result}, broadcast sent to {everyone.Result}; the other player received {otherPlayerSignals.Count}");

    // ---- Without async ---------------------------------------------------------------------------------------------
    ExampleSetup.Title("Without async (Unity scripts, console apps)");
    HoloNETAPI.SaveData("sync-key", "sync value");
    Console.WriteLine("sync-key = " + HoloNETAPI.LoadData("sync-key").Result);
    HoloNETAPI.DeleteData("sync-key");
}
finally
{
    await HoloNETAPI.ShutdownAsync();
    if (otherPlayer != null) await otherPlayer.DisconnectAsync();
    if (admin.State == System.Net.WebSockets.WebSocketState.Open || (await admin.ConnectAsync(ExampleSetup.AdminUri)).IsConnected)
    {
        await admin.UninstallAppAsync(appId);
        await admin.UninstallAppAsync(otherId);
        await admin.DisconnectAsync();
    }
    try { File.Delete(HoloNETAPIDNAManager.HoloNETAPIDNAPath); } catch { }
    Console.WriteLine("Done.");
}

// The other player updating the shared entry and announcing it: exactly what HoloNETAPI.SaveSharedDataAsync does.
static async Task OtherPlayerSetsHighScore(IHoloNETClientAppAgent player, string value)
{
    string id = "holonet-api-shared/leaderboard/data/high-score";
    var current = await player.CallZomeFunctionAsync("oasis", "get_holon_by_id", id);
    await player.CallZomeFunctionAsync("oasis", "update_entry_holon", new Dictionary<string, object>
    {
        ["original_action_hash"] = player.ConvertHoloHashToBytes(current.Records[0].ActionHash),
        ["updated_entry"] = new Dictionary<string, object>
        {
            ["id"] = id, ["parent_holon_id"] = "holonet-api-shared/leaderboard/data",
            ["holonet_api_key"] = "high-score", ["holonet_api_kind"] = "data", ["holonet_api_value"] = value, ["holonet_api_deleted"] = false
        }
    });
    await player.CallZomeFunctionAsync("oasis", "hypernet_send", new Dictionary<string, object>
    {
        ["session"] = "holonet-api-changes/leaderboard",
        ["payload"] = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object> { ["ns"] = "leaderboard", ["key"] = "high-score", ["object"] = false, ["deleted"] = false }),
        ["to"] = null
    });
}

static async Task WaitFor(Func<bool> condition)
{
    for (int i = 0; i < 100 && !condition(); i++)
        await Task.Delay(100);
}

static string Short(string key) => string.IsNullOrEmpty(key) ? "-" : key.Substring(0, Math.Min(12, key.Length)) + "...";

public class Settings
{
    public string Name { get; set; }
    public int Level { get; set; }
}
