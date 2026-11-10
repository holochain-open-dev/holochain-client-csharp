# Your first zome call

**You'll build:** a .NET console app that connects to Holochain, installs a hApp, writes an entry, reads it back, and receives a signal from another agent.
**Time:** 15 minutes. **You need:** the [.NET 10 SDK](https://dotnet.microsoft.com/download) and a clone of [holochain-client-csharp](https://github.com/holochain-open-dev/holochain-client-csharp).
**Finished code:** [`examples/01-Client`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/examples/01-Client/Program.cs).

## 1. Start a conductor

Holochain apps run on a **conductor**, the Holochain runtime on your machine. The examples folder has a script that starts a throwaway one, using the binaries bundled with HoloNET:

```bash
examples/start-conductor.sh            # or, in PowerShell:  .\examples\start-conductor.ps1
```

Leave it running. Its admin interface is on `ws://localhost:7777`, HoloNET's default.

> In your own apps you can skip this step. Reference `NextGenSoftware.Holochain.HoloNET.Client.Embedded`, set `HolochainConductorMode = UseEmbedded`, and HoloNET starts a conductor for you.

## 2. Create the project

```bash
dotnet new console -n MyFirstHapp
cd MyFirstHapp
dotnet add package NextGenSoftware.Holochain.HoloNET.Client
```

You'll also need a `.happ` file: the OASIS hApp works well. It's `oasis.happ` in the [HoloNET API package](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.API), or in `HoloNET-API/NextGenSoftware.Holochain.HoloNET.API/` in the repo. Copy it next to your project.

## 3. Connect, and install the hApp in one call

```csharp
using NextGenSoftware.Holochain.HoloNET.Client;

var admin = new HoloNETClientAdmin(new HoloNETDNA
{
    HolochainConductorAdminURI = "ws://localhost:7777",
    AutoStartHolochainConductor = false,      // we started it ourselves
    AutoShutdownHolochainConductor = false
});
await admin.ConnectAsync();

var app = await admin.InstallEnableSignAttachAndConnectToHappAsync("my-first-app", "oasis.happ", "oasis");
if (!app.IsAppConnected) throw new Exception(app.Message);
var client = app.HoloNETClientAppAgent;
Console.WriteLine("My agent: " + app.AgentPubKey);
```

That one call does everything Holochain 0.7.0 and 0.7.1 need:
1. Generates an agent key.
2. Installs the hApp.
3. Enables it.
4. Sets up signing and a capability grant.
5. Attaches an app interface and issues an authentication token.
6. Connects an app client.

## 4. Write an entry, and read it back

```csharp
var created = await client.CallZomeFunctionAsync("oasis", "create_entry_avatar", new Dictionary<string, object>
{
    ["id"] = Guid.NewGuid().ToString(), ["username"] = "ada", ["email"] = "ada@example.com"
});
string actionHash = created.Records[0].ActionHash;

var read = await client.CallZomeFunctionAsync("oasis", "get_entry_avatar", client.ConvertHoloHashToBytes(actionHash));
Console.WriteLine("Read back: " + read.Records[0].EntryKeyValuePairs["username"]);
```

- **The parameters.** The dictionary's keys must match the fields of the Rust struct in the zome.
- **The hash.** An entry is identified by its **ActionHash**. You use it to read, update and delete it.
- **Errors.** Zome errors don't throw. Check `created.IsError` and `created.Message`.

## 5. Run it

```bash
dotnet run
```

## What just happened

- **No server.** Your app talked to your own conductor.
- **Where the data lives.** The avatar entry went onto your agent's **source chain** (its own signed history), and was published to the hApp's **DHT**, which every agent in the app shares.
- **Signed calls.** HoloNET signed every zome call with your agent's key.

## Next

- [Save data in one line](02-save-data-in-one-line.md) with HoloNET API, without zome calls at all.
- The [Client guide](https://docs.holonet.oasisomniverse.one/client-guide) covers signals, the admin API, clone cells and more.
