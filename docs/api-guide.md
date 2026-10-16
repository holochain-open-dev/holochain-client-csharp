# HoloNET API guide

HoloNET API is the quickest way to put data on Holochain from .NET: static methods for a key/value store, an object store and direct messages. You don't need to write any Rust or set up a conductor; it uses the bundled OASIS hApp. Every member is listed in the [API reference](api/API.md).

```bash
dotnet add package NextGenSoftware.Holochain.HoloNET.API
```

To have HoloNET start its own conductor, also add an Embedded Client package (`NextGenSoftware.Holochain.HoloNET.Client.Embedded`). Without one, it connects to the conductor on HoloNET's default admin port.

## Key/value data

```csharp
using NextGenSoftware.Holochain.HoloNET.API;

await HoloNETAPI.SaveDataAsync("theme", "dark");

HoloNETAPIResult<string> theme = await HoloNETAPI.LoadDataAsync("theme");
if (theme.IsSuccess)
    Console.WriteLine(theme.Result); // "dark"

// Work with the whole store:
HoloNETAPI.Data["volume"] = "7";
await HoloNETAPI.SaveAllDataAsync();
var all = await HoloNETAPI.LoadAllDataAsync();  // all.Result is a Dictionary<string, string>
```

## Objects

```csharp
public class Settings { public string Name { get; set; } public int Level { get; set; } }

await HoloNETAPI.SaveObjectAsync("settings", new Settings { Name = "Ada", Level = 3 });
HoloNETAPIResult<Settings> s = await HoloNETAPI.LoadObjectAsync<Settings>("settings");
```

Objects are stored as JSON.

## Deleting

```csharp
await HoloNETAPI.DeleteDataAsync("theme");      // Result: true if the key existed
await HoloNETAPI.DeleteObjectAsync("settings");
await HoloNETAPI.DeleteAllDataAsync();          // every value (objects are kept)
await HoloNETAPI.DeleteAllObjectsAsync();       // every object (values are kept)
```

A delete saves a new version of the store without the key. Earlier versions stay on your source chain, as Holochain keeps history.

## How it's stored

- **One entry per agent.** The whole store is one entry on your agent's source chain, with the data and the objects in two fields.
- **Its id.** The entry's id is `HoloNETAPI.StoreId`: `HoloNETAPIDNA.StoreId` if you set one, otherwise one derived from your agent key.
- **Saving.** A save writes a new version of that entry, with what you've changed merged into what's already stored.
- **Surviving restarts.** Your data is still there after a restart, because the API reinstalls nothing and finds the same agent and entry.
- **Caching.** Loads are cached; pass `useCache: false` to read from the DHT again.

## Choosing the hApp

By default, the API uses the bundled OASIS hApp (`oasis.happ`, role `oasis`, installed app id `oasis`). To use a different one, pass a `HoloNETAPIHapp`:

```csharp
var happ = new HoloNETAPIHapp("my-app", @"C:\hApps\my.happ", "my-role");
await HoloNETAPI.SaveDataAsync(happ, "theme", "dark");
```

Your zome needs the same functions and fields as the OASIS avatar entry:

- **Functions:** `create_entry_avatar`, `update_entry_avatar` and `get_avatar_by_id`.
- **Fields:** `holonet_api_data` and `holonet_api_objects`.

The function names are optional parameters on every method, so you can point them at your own functions.

Whichever hApp you initialise with is remembered in `HoloNETAPIDNA.json`, so later one-line calls use it again, even after `ShutdownAsync` or a restart.

The older overloads that take the hApp as three strings (`hAppId, hAppPath, hAppRole`) still work, but they're obsolete. With three leading strings, C# could silently pick a different overload, for example treating your app id as the zome name.

## Direct messages

Send a text message straight to other agents. It goes as a Holochain remote signal, peer to peer, and nothing is stored.

```csharp
HoloNETAPI.OnSignalReceived += (s, e) =>
    Console.WriteLine($"{e.FromAgentPubKey}: {e.Message}");

var sent = await HoloNETAPI.SendSignalAsync("hello", new[] { friendAgentPubKey });
// sent.Result == number of agents it was sent to

string last = HoloNETAPI.ReceiveSignal(); // the last message received
```

Delivery isn't guaranteed. For acknowledged, retried messages, or for multiplayer, use [HyperNET](hypernet-guide.md).

## Errors

- **Results.** Every call returns a `HoloNETAPIResult<T>` with `IsSuccess`, `Result` and `Message`, plus `ZomeCallResult` with the underlying zome call.
- **Events.** Errors are also raised through `HoloNETAPI.OnError`, and kept in `HoloNETAPI.LastError`.

## Lifetime

The first call initialises everything: it connects, installs the hApp if needed, and issues the app token. You can do this up front with `await HoloNETAPI.InitHoloNETAsync()`. Call `await HoloNETAPI.ShutdownAsync()` when you're done; the next call starts again cleanly.
