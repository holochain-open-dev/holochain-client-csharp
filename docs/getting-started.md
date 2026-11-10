# Getting started

**Fastest way:** start from a template. `dotnet new install NextGenSoftware.Holochain.HoloNET.Templates`, then `dotnet new holonet-console -n MyApp` (or `holonet-blazor`, `holonet-wpf`, `holonet-maui`, ...) gives you a project that already runs. See [Templates](templates.md) and the [5-minute tutorial](https://learn.holonet.oasisomniverse.one/tutorials/00-up-and-running-in-5-minutes). The rest of this page is for adding HoloNET to an app you already have.

## 1. Pick a package

| You want | Package |
|---|---|
| HoloNET to start its own Holochain conductor, on any OS (simplest) | `NextGenSoftware.Holochain.HoloNET.Client`: if no conductor is installed it downloads the official one the first time (`AutoDownloadConductor`, see [Where the conductor comes from](conductor.md)) |
| The same, but it must work offline or never download | `NextGenSoftware.Holochain.HoloNET.Client.Embedded` (bundles 0.7.1 and 0.7.0, Windows; `.Embedded.LinuxX64`, `.LinuxArm64`, `.MacX64` and `.MacArm64` for Linux and macOS) |
| The same, with a smaller download | `.Client.Embedded.Holochain071` or `.Client.Embedded.Holochain070` |
| To use a conductor you already run (`hc sandbox`, a launcher, a server) | `NextGenSoftware.Holochain.HoloNET.Client`, with `AutoStartHolochainConductor = false` |
| Entry models and collections on top | the matching `NextGenSoftware.Holochain.HoloNET.ORM*` variant |
| Just save and load data | `NextGenSoftware.Holochain.HoloNET.API` |
| Multiplayer networking | `NextGenSoftware.Holochain.HoloNET.HyperNET` |
| Everything in one reference (Client, ORM, HyperNET) | `NextGenSoftware.Holochain.HoloNET.HDK` |

```bash
dotnet add package NextGenSoftware.Holochain.HoloNET.Client.Embedded
```

Use one variant per app. They have the same API and namespaces; only the bundled conductor differs. Every library comes in the same four variants (none, `.Embedded`, `.Embedded.Holochain071`, `.Embedded.Holochain070`); see [Packages](packages.md) for the full list. `.Embedded` bundles the Windows conductors; Linux and macOS have their own offline packages (`.Embedded.LinuxX64`, `.LinuxArm64`, `.MacX64`, `.MacArm64`, each with `.Holochain071` / `.Holochain070` or both) for the Client, ORM, HyperNET and API. Or use the package with no suffix and HoloNET downloads the conductor.

## 2. Which Holochain version?

Both are supported and tested on every release.

- **0.7.1** is the default, and the one to use for new hApps.
- **0.7.0** is for hApps built against hdk 0.7.0. A hApp built for 0.7.1 also runs on a 0.7.0 conductor (the OASIS hApp is tested this way).

With a dual-version Embedded package, choose the version per client:

```csharp
var dna = new HoloNETDNA { EmbeddedHolochainVersion = EmbeddedHolochainVersion.V0_7_0 };
```

## 3. Install a hApp and call a zome function

The admin client does the whole Holochain 0.7 setup in one call:

1. Generates an agent key.
2. Installs the `.happ` (or keeps it if it's already installed).
3. Enables it.
4. Grants zome-call signing.
5. Attaches an app interface and issues the app authentication token.
6. Connects an app client.

```csharp
using NextGenSoftware.Holochain.HoloNET.Client;

// Embedded package: HoloNET starts the bundled conductor on connect.
var admin = new HoloNETClientAdmin(new HoloNETDNA { HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded });
await admin.ConnectAsync();

var app = await admin.InstallEnableSignAttachAndConnectToHappAsync("my-app", @"C:\hApps\my-app.happ", "my-role");
if (!app.IsAppConnected)
    throw new Exception(app.Message);

IHoloNETClientAppAgent client = app.HoloNETClientAppAgent;

ZomeFunctionCallBackEventArgs result = await client.CallZomeFunctionAsync("my_zome", "create_thing", new { name = "hello" });
if (result.IsError)
    Console.WriteLine(result.Message);
else
    Console.WriteLine(result.ZomeReturnHash); // the new record's ActionHash
```

On an Android phone use the Holochain runtime on the phone (the shared [Holochain Service Runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) or a built-in conductor, as Our World does through the OASIS Edge runtime ([Edge Runtime only](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Runtime-v1.2.0), [OGEngineClient + Edge](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-Edge-v2.1.0), [OGEngineClient + Edge + Unity](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Unity-v1.1.0), [OGEngineClient RemoteOnly](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-RemoteOnly-v2.1.0))); the `holonet-android` template wires it in ([details](templates#running-on-mobile-and-in-the-browser)). Android and iOS runtime libraries for apps: [Holochain 0.7 for OASIS: Android + iOS (oasis-holochain-0.7-v1.1.0)](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0) (the templates download the Android ones on the first build).

To use your own conductor (the non-Embedded package), point at its admin port and turn auto-start off:

```csharp
var admin = new HoloNETClientAdmin(new HoloNETDNA
{
    HolochainConductorAdminURI = "ws://localhost:65464",
    AutoStartHolochainConductor = false,
    AutoShutdownHolochainConductor = false
});
```

## 4. Go higher level

- **No Rust, just data:** see the [HoloNET API guide](api-guide.md).

  ```csharp
  await HoloNETAPI.SaveDataAsync("greeting", "hello");
  string greeting = (await HoloNETAPI.LoadDataAsync("greeting")).Result;
  ```

- **Your own entry types as C# classes:** see the [HoloNET ORM guide](orm-guide.md).
- **Multiplayer:** see the [HyperNET guide](hypernet-guide.md).

## 5. Things that trip people up on Holochain 0.7

- **ActionHash vs EntryHash.** Updates and deletes take the ActionHash of the record (`result.ZomeReturnHash` after a create). The ORM handles this for you.
- **App authentication tokens.** Since 0.6, every app-interface connection needs a token from the admin API. `InstallEnableSignAttachAndConnectToHappAsync` gets one. If you connect an app client yourself, call `admin.IssueAppAuthenticationTokenAsync` and set `HoloNETDNA.AppAuthenticationToken`.
- **Capability grants.** Holochain 0.7 rejects unrestricted grants for zome-call signing. HoloNET defaults to `CapGrantAccessType.Assigned`, which is correct.
- **Sync methods inside events.** Every method has a sync version, and they are safe to call from inside HoloNET's own event handlers. Prefer the `Async` versions anyway.

## 6. Logging

HoloNET logs to the console and to `%APPDATA%\NextGenSoftware\HoloNET\Logs`. Set this up with `HoloNETDNA.LogToConsole`, `LogToFile`, `LogPath`, `FileLoggingMode` and `ConsoleLoggingMode`, or pass your own `ILogProvider` to the client's constructor.
