# HoloNET API guide

HoloNET API is the quickest way to use Holochain from .NET: **one line** to save, load or delete data and objects, share data with everyone in your app, get told when it changes, and message other agents. You don't need to write any Rust or set up a conductor: it uses the bundled OASIS hApp (or yours), and HoloNET installs and connects it on first use. Every member is listed in the [API reference](api/API.md).

```bash
dotnet add package NextGenSoftware.Holochain.HoloNET.API
```

To have HoloNET start its own conductor, also add an Embedded Client package (`NextGenSoftware.Holochain.HoloNET.Client.Embedded`). Without one, it connects to the conductor on HoloNET's default admin port.

## Your data

```csharp
using NextGenSoftware.Holochain.HoloNET.API;

await HoloNETAPI.SaveDataAsync("theme", "dark");
string theme = (await HoloNETAPI.LoadDataAsync("theme")).Result;
await HoloNETAPI.DeleteDataAsync("theme");             // Result: true if it existed

// Any type: numbers, bools, dates, Guids, enums (stored in the invariant culture, so they load back the same anywhere).
await HoloNETAPI.SaveDataAsync("score", 9001);
int score = (await HoloNETAPI.LoadDataAsync<int>("score")).Result;

// What's there.
List<string> keys = (await HoloNETAPI.ListKeysAsync()).Result;
bool hasScore = (await HoloNETAPI.ExistsAsync("score")).Result;
```

Every call returns a `HoloNETAPIResult<T>` with `IsSuccess`, `Result` and `Message`. A missing key, an empty key or a value that won't convert is reported in `Message`; nothing throws.

## Objects

```csharp
public class Settings { public string Name { get; set; } public int Level { get; set; } }

await HoloNETAPI.SaveObjectAsync("settings", new Settings { Name = "Ada", Level = 3 });
Settings s = (await HoloNETAPI.LoadObjectAsync<Settings>("settings")).Result;
await HoloNETAPI.DeleteObjectAsync("settings");

List<string> objectKeys = (await HoloNETAPI.ListKeysAsync(objects: true)).Result;
```

Objects are stored as JSON.

## Working with everything at once

```csharp
var all = await HoloNETAPI.LoadAllDataAsync();    // fills HoloNETAPI.Data
HoloNETAPI.Data["volume"] = "7";
HoloNETAPI.Data.Remove("old");
await HoloNETAPI.SaveAllDataAsync();               // writes only what changed; removed keys are deleted

await HoloNETAPI.DeleteAllDataAsync();             // every value (objects are kept)
await HoloNETAPI.DeleteAllObjectsAsync();          // every object (values are kept)
```

`LoadAllObjectsAsync`, `HoloNETAPI.Objects` and `SaveAllObjectsAsync` do the same for objects.

## Shared data

Data in a **namespace** that every agent in the app can read and write: a leaderboard, a room's settings, a shared list.

```csharp
await HoloNETAPI.SaveSharedDataAsync("leaderboard", "high-score", "9001");
int best = (await HoloNETAPI.LoadSharedDataAsync<int>("leaderboard", "high-score")).Result;

await HoloNETAPI.SaveSharedObjectAsync("room-42", "settings", new RoomSettings { MaxPlayers = 8 });
var room = (await HoloNETAPI.LoadSharedObjectAsync<RoomSettings>("room-42", "settings")).Result;

var everything = (await HoloNETAPI.LoadAllSharedDataAsync("leaderboard")).Result;
var keys = (await HoloNETAPI.ListSharedKeysAsync("leaderboard")).Result;
await HoloNETAPI.DeleteSharedDataAsync("leaderboard", "high-score");
```

Shared loads always read from the DHT, because anyone may have changed the data.

Anyone in the app can write to a namespace. If only certain agents should be allowed to change something, enforce that with validation rules in your own hApp.

## Change notifications

Get told when someone else changes shared data:

```csharp
HoloNETAPI.OnDataChanged += (s, e) =>
    Console.WriteLine($"{e.ByAgentPubKey} {(e.IsDeleted ? "deleted" : "changed")} {e.Namespace}/{e.Key}");

await HoloNETAPI.SubscribeToChangesAsync("leaderboard");
// ... later
await HoloNETAPI.UnsubscribeFromChangesAsync("leaderboard");
```

Every shared save or delete tells the namespace's subscribers directly, as a remote signal. Notifications aren't guaranteed. If you can't afford to miss one, load the data again from time to time.

## Messages

```csharp
HoloNETAPI.OnSignalReceived += (s, e) => Console.WriteLine($"{e.FromAgentPubKey}: {e.Message}");

await HoloNETAPI.SendSignalAsync("hello", new[] { friendAgentPubKey });   // to specific agents
await HoloNETAPI.SendSignalToAllAsync("server restarting in 5 minutes");  // to everyone using the API in this app
string last = HoloNETAPI.ReceiveSignal();                                // the last message received
```

Messages go straight from agent to agent, and nothing is stored. Delivery isn't guaranteed. For acknowledged, retried messages, or for multiplayer, use [HyperNET](hypernet-guide.md).

## Without async

Every common call has a sync version, for Unity scripts, console apps and other code that doesn't use `async`:

```csharp
HoloNETAPI.SaveData("theme", "dark");
string theme = HoloNETAPI.LoadData("theme").Result;
int score = HoloNETAPI.LoadData<int>("score").Result;
HoloNETAPI.DeleteData("theme");
HoloNETAPI.SaveObject("settings", settings);
var s = HoloNETAPI.LoadObject<Settings>("settings").Result;
HoloNETAPI.SaveSharedData("leaderboard", "high-score", "9001");
HoloNETAPI.SendSignalToAll("hello");
bool exists = HoloNETAPI.Exists("theme").Result;
var keys = HoloNETAPI.ListKeys().Result;
```

## How it's stored

- **One entry per key.** Each key is its own entry: a holon in the OASIS hApp, with the id `<store>/<data|object>/<key>`. Saving or loading one key costs the same however big your store is.
- **Your store's id** is `HoloNETAPI.StoreId`: `HoloNETAPIDNA.StoreId` if you set one, otherwise one derived from your agent key. Shared data lives in `holonet-api-shared/<namespace>`.
- **Deletes keep history.** A delete saves a deleted marker as the key's newest version. Holochain keeps history, so earlier versions remain on the DHT.
- **Caching.** Loads of your own data are cached; pass `useCache: false` to read from the DHT again.
- **Upgrading from 4.4.2 or earlier.** Those versions kept the whole store in one entry. The first time 4.5 starts, it copies that store into per-key storage, once.

To keep the old one-entry store, set `HoloNETAPIDNA.StorageMode = "SingleEntry"`. Calls that name your own zome functions also use it, because they're meant for a custom hApp shaped like that store.

## Choosing the hApp

By default, the API uses the bundled OASIS hApp (`oasis.happ`, role `oasis`, installed app id `oasis`). To use a different one, pass a `HoloNETAPIHapp`:

```csharp
var happ = new HoloNETAPIHapp("my-app", @"C:\hApps\my.happ", "my-role");
await HoloNETAPI.InitHoloNETAsync(happ);
```

Whichever hApp you initialise with is remembered in `HoloNETAPIDNA.json`, so later one-line calls use it again, even after `ShutdownAsync` or a restart.

Your hApp needs the OASIS hApp's holon functions for storage: `create_entry_holon`, `update_entry_holon`, `get_holon_by_id` and `get_holons_for_parent_by_id`. For messages, shared-data notifications and broadcasts, it needs its `hypernet_*` functions. Both come from the OASIS hApp's coordinator zome, which you can copy.

## Errors

- Every result has `IsSuccess`, `Result` and `Message`, plus `ZomeCallResult` with the underlying zome call when there is one.
- Errors are also raised through `HoloNETAPI.OnError`, and kept in `HoloNETAPI.LastError`.

## Lifetime

The first call initialises everything: it connects, installs the hApp if needed, issues the app token, and joins the API's message zone. You can do this up front with `await HoloNETAPI.InitHoloNETAsync()`. Call `await HoloNETAPI.ShutdownAsync()` when you're done; the next call starts again cleanly.
