# HoloNET examples

Runnable, commented examples of everything in HoloNET, on Holochain 0.7.0 and 0.7.1. Every one of them is run against real conductors of both versions on each release (`scripts/run-live-tests.sh <version>`, or `PROJECTS=examples` for just these).

| Example | What it shows | Guide |
|---|---|---|
| [01-Client](01-Client/Program.cs) | Connecting; installing a hApp and connecting an app client in one call; zome calls; signals between agents; admin calls. | [Client guide](../docs/client-guide.md) |
| [02-ORM](02-ORM/Program.cs) | Entry models; create, load, update and delete; change tracking; audit entries; collections with batch saves. | [ORM guide](../docs/orm-guide.md) |
| [03-API](03-API/Program.cs) | HoloNET API, one line for everything: data, typed values, objects, listing, deletes, shared data, change notifications, messages, broadcast, sync versions. | [API guide](../docs/api-guide.md) |
| [04-HyperNET](04-HyperNET/Program.cs) | Three players in a zone: area of interest, positions, unreliable, zone-wide and reliable typed messages, ping, stats, changing zones. | [HyperNET guide](../docs/hypernet-guide.md) |
| [05-GenerateHapp](05-GenerateHapp/Program.cs) | Generating a complete Holochain 0.7 hApp (Rust and C#) from code, and the same hApp as a [WEB5 STAR OAPP](05-GenerateHapp/StarOAPP/README.md). | [STAR hApp generation](../docs/star-happ-generation.md) |
| [06-WebSite](06-WebSite/Program.cs) | A web site on Holochain: a live guestbook and leaderboard shared by every visitor, updated instantly in every browser. | [API guide: shared data](../docs/api-guide.md#shared-data) |

There are step-by-step tutorials built on these at **[learn.holonet.oasisomniverse.one](https://learn.holonet.oasisomniverse.one)**, and every guide and the full API reference are at **[docs.holonet.oasisomniverse.one](https://docs.holonet.oasisomniverse.one)**.

## Run them

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

1. **Start a conductor.** It runs in a throwaway sandbox on ws://localhost:7777, HoloNET's default.

   ```bash
   examples/start-conductor.sh          # Git Bash, Linux, macOS  (add 0.7.0 for Holochain 0.7.0)
   ```

   ```powershell
   .\examples\start-conductor.ps1       # PowerShell             (-Version 0.7.0)
   ```

   On Windows, these use the `hc` and `holochain` binaries bundled with HoloNET. Elsewhere, [install Holochain](https://developer.holochain.org/get-started) first.

2. **Run an example** in another terminal:

   ```bash
   dotnet run --project examples/03-API
   ```

   Or open `examples/HoloNET.Examples.slnx` in Visual Studio or Rider.

To point the examples at a conductor you already run, set `HOLONET_ADMIN_URI` (for example `ws://localhost:65464`). To use your own hApp, set `HOLONET_HAPP_PATH`. By default they use the OASIS hApp that HoloNET API ships, which has avatar and holon entries, ORM collections and HyperNET's zome functions.

## In your own app

The examples reference the HoloNET projects in this repo, so they always match the source. In your own app, use the NuGet packages instead:

```bash
dotnet add package NextGenSoftware.Holochain.HoloNET.Client.Embedded   # the Client, plus a bundled conductor HoloNET starts for you
dotnet add package NextGenSoftware.Holochain.HoloNET.ORM
dotnet add package NextGenSoftware.Holochain.HoloNET.API
dotnet add package NextGenSoftware.Holochain.HoloNET.HyperNET
```

With an `.Embedded` package, set `HoloNETDNA.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded` and you don't need to start a conductor at all. See [Getting started](../docs/getting-started.md).
