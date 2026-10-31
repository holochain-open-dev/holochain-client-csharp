# Save data in one line

**You'll build:** an app that saves, loads and deletes settings and objects on Holochain, with no Rust, no zome calls and no setup code.
**Time:** 10 minutes. **You need:** a running conductor (see [Your first zome call](01-first-zome-call.md), step 1).
**Finished code:** [`examples/03-API`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/examples/03-API/Program.cs).

## 1. Create the project

```bash
dotnet new console -n OneLiners
cd OneLiners
dotnet add package NextGenSoftware.Holochain.HoloNET.API
```

HoloNET API brings the OASIS hApp with it, and installs it on first use.

## 2. Point it at your conductor

```csharp
using NextGenSoftware.Holochain.HoloNET.API;

HoloNETAPI.UseConductor("ws://localhost:7777", logToConsole: false);
```

With an `.Embedded` Client package you'd skip this, and the API would start its own conductor.

## 3. One line each

```csharp
await HoloNETAPI.SaveDataAsync("theme", "dark");
string theme = (await HoloNETAPI.LoadDataAsync("theme")).Result;          // "dark"

await HoloNETAPI.SaveDataAsync("score", 9001);                             // any type
int score = (await HoloNETAPI.LoadDataAsync<int>("score")).Result;         // back as an int

await HoloNETAPI.SaveObjectAsync("player", new Player { Name = "Ada", Level = 3 });
Player p = (await HoloNETAPI.LoadObjectAsync<Player>("player")).Result;

var keys = (await HoloNETAPI.ListKeysAsync()).Result;                      // theme, score
await HoloNETAPI.DeleteDataAsync("theme");

public class Player { public string Name { get; set; } public int Level { get; set; } }
```

Run it twice. The second time, `LoadDataAsync("score")` returns 9001 straight away, because the data is on Holochain, not in memory.

## 4. Check results, not exceptions

Every call returns `IsSuccess`, `Result` and `Message`, and nothing throws:

```csharp
var missing = await HoloNETAPI.LoadDataAsync("nope");
if (!missing.IsSuccess) Console.WriteLine(missing.Message);   // "No data has been saved under the key 'nope'."
```

## 5. No async? No problem

Unity scripts and simple console apps can use the sync versions:

```csharp
HoloNETAPI.SaveData("volume", "7");
string volume = HoloNETAPI.LoadData("volume").Result;
```

## What just happened

- **Connecting.** On the first call, the API connected, installed the bundled OASIS hApp, and remembered it in `HoloNETAPIDNA.json`.
- **Storage.** Each key is its own entry on Holochain, so saving one key stays fast however many you have.
- **Privacy.** Your store belongs to your agent. To share data with everyone, use [shared data](04-a-live-site-on-holochain.md).

## Next

- [Model your data with the ORM](03-model-your-data.md): your own C# classes, mapped onto your own hApp.
- The [API guide](https://docs.holonet.oasisomniverse.one/api-guide) covers everything else: shared data, notifications and messages.
