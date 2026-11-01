// HoloNET API: one line to save, load and delete data on Holochain, share data with every agent, hear when it changes,
// and message other agents. No Rust, no conductor plumbing: the first call connects, installs the hApp and gets ready.
//
// Guide:      https://docs.holonet.oasisomniverse.one/api-guide
// Tutorials:  https://learn.holonet.oasisomniverse.one
// Examples:   https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/examples
using NextGenSoftware.Holochain.HoloNET.API;
using NextGenSoftware.Holochain.HoloNET.Client;

//#if (UseEmbedded)
// The bundled Holochain conductor starts and stops with your app (Windows). Quiet its log; remove the line to watch every message.
HoloNETAPI.Configure(new HoloNETDNA { LogToConsole = false });
//#else
// This app uses a conductor you start first (./start-conductor.sh, or .\start-conductor.ps1 in PowerShell), on ws://localhost:7777.
HoloNETAPI.UseConductor("ws://localhost:7777", logToConsole: false);
//#endif

// ---- Your data: one line each ----
await HoloNETAPI.SaveDataAsync("greeting", "Hello, Holochain!");
Console.WriteLine((await HoloNETAPI.LoadDataAsync("greeting")).Result);

await HoloNETAPI.SaveDataAsync("visits", 1);                                  // any type: numbers, bools, dates, Guids, enums
Console.WriteLine("visits = " + (await HoloNETAPI.LoadDataAsync<int>("visits")).Result);

await HoloNETAPI.SaveObjectAsync("profile", new Profile { Name = "Ada", Level = 3 });
Console.WriteLine("profile = " + (await HoloNETAPI.LoadObjectAsync<Profile>("profile")).Result.Name);

Console.WriteLine("keys: " + string.Join(", ", (await HoloNETAPI.ListKeysAsync()).Result));
await HoloNETAPI.DeleteDataAsync("visits");

// ---- Shared data: every agent in the app can read and write a namespace; subscribers hear about changes ----
HoloNETAPI.OnDataChanged += (s, e) => Console.WriteLine($"{e.ByAgentPubKey} changed {e.Namespace}/{e.Key}");
await HoloNETAPI.SubscribeToChangesAsync("leaderboard");
await HoloNETAPI.SaveSharedDataAsync("leaderboard", "ada", "9001");
Console.WriteLine("ada's score = " + (await HoloNETAPI.LoadSharedDataAsync<int>("leaderboard", "ada")).Result);

// ---- Every call returns IsSuccess / Result / Message; nothing throws ----
var missing = await HoloNETAPI.LoadDataAsync("no-such-key");
Console.WriteLine("missing key: IsSuccess = " + missing.IsSuccess + " (" + missing.Message + ")");

// Messages go straight from agent to agent:
//   await HoloNETAPI.SendSignalAsync("hi", new[] { friendAgentPubKey });   await HoloNETAPI.SendSignalToAllAsync("hi everyone");
// And there are sync versions of everything for Unity scripts and simple apps: HoloNETAPI.SaveData("key", "value");

await HoloNETAPI.ShutdownAsync();

public class Profile
{
    public string Name { get; set; }
    public int Level { get; set; }
}
