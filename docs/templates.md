# Project templates

`dotnet new` templates for the .NET app types people use most, each with HoloNET already wired in. You pick the app type with the template, and the
HoloNET library with an option, and you get a project that builds and runs straight away.

```bash
dotnet new install NextGenSoftware.Holochain.HoloNET.Templates

dotnet new holonet-blazor -n MyApp                     # a Blazor app using the one-line HoloNET API
dotnet new holonet-wpf -n MyApp --HoloNET orm          # a WPF app using the HoloNET ORM
dotnet new holonet-webapi -n MyApp --HoloNET client    # a Web API using the HoloNET Client
```

## The templates

| Template | App type | Runs the HoloNET demo from | Bundled conductor |
|---|---|---|---|
| `holonet-console` | Console app | `Program.cs` | Yes (Windows) |
| `holonet-worker` | Worker service (background service) | `Worker.cs` | Yes (Windows) |
| `holonet-webapi` | ASP.NET Core Web API | `GET /holonet` | Yes (Windows) |
| `holonet-mvc` | ASP.NET Core MVC | the **HoloNET** menu item | Yes (Windows) |
| `holonet-razor` | ASP.NET Core Razor Pages | the **HoloNET** menu item | Yes (Windows) |
| `holonet-blazor` | Blazor Web App (server) | the **HoloNET** page, with a button | Yes (Windows) |
| `holonet-blazorwasm` | Blazor WebAssembly | the **HoloNET** page, with a button | No: connects to your conductor |
| `holonet-wpf` | WPF | a button on the main window | Yes (Windows) |
| `holonet-winforms` | Windows Forms | a button on the main form | Yes (Windows) |
| `holonet-winui` | WinUI 3 | a button on the main window | Yes (Windows) |
| `holonet-avalonia` | Avalonia (Windows, Linux, macOS) | a button on the main window | Yes (Windows) |
| `holonet-maui` | .NET MAUI | a button on the main page | Android: the runtime on the phone (shared, else built in); elsewhere connects to your conductor |
| `holonet-maui-blazor` | .NET MAUI Blazor Hybrid | the **HoloNET** page, with a button | Android: the runtime on the phone (shared, else built in); elsewhere connects to your conductor |
| `holonet-uno` | Uno Platform | a button on the main page | Android: the runtime on the phone (shared, else built in); elsewhere connects to your conductor |
| `holonet-android` | Android (native) | a button on the main activity | The runtime on the phone (shared, else built in); libraries download on first build |
| `holonet-ios` | iOS (native) | a button on the main screen | iOS: the conductor in the app, with the iOS bridge library (simulator-proven only); elsewhere connects to your conductor |
| `holonet-grpc` | ASP.NET Core gRPC service | `GET /holonet` endpoint in `Program.cs` | Yes (Windows) |
| `holonet-classlib` | Class library | `HoloNETService.cs` | Yes (Windows) |
| `holonet-xunit` | xUnit test project | `HoloNETTests.cs` | Yes (Windows) |
| `holonet-unity` | Unity (an `Assets` starter) | a `HoloNETBehaviour` component | Windows standalone builds |

Why some don't bundle a conductor: a browser can't host one, and on a phone the conductor comes from the runtime libraries rather than the template, so those apps connect to a
conductor you run on your computer (or on a server) instead when the libraries aren't there. See [Running on mobile and in the browser](#running-on-mobile-and-in-the-browser).

## The options

Every template has the same options.

| Option | Values | What it does |
|---|---|---|
| `--HoloNET` | `api` (default), `client`, `orm`, `hypernet` | Which HoloNET library the app uses. |
| `--Conductor` | `auto` (default), `embedded`, `external` | `embedded` bundles the Holochain conductor in the app (Windows) and starts and stops it for you. `external` uses a conductor you run yourself on `ws://localhost:7777`. `auto` is `embedded` on Windows; elsewhere HoloNET starts the Holochain on your computer and, the first time, downloads the official one (`AutoDownloadConductor`, see [Where the conductor comes from](conductor.md)). Mobile, browser and tablet templates always use `external`. |
| `--HolochainVersion` | `both` (default), `0.7.1`, `0.7.0` | With a bundled conductor: which Holochain version(s) to bundle. `both` lets you choose at run time with `HoloNETDNA.EmbeddedHolochainVersion`; one version gives a smaller app. |
| `--HoloNETVersion` | e.g. `4.6.0` | The HoloNET package version to use (defaults to the version the templates were released with). |

The options choose the NuGet package for you, so you never have to work out which one to add. For example `--HoloNET orm --Conductor embedded --HolochainVersion 0.7.1`
references `NextGenSoftware.Holochain.HoloNET.ORM.Embedded.Holochain071`. The ORM, HyperNET and API packages build on the Client, so you get the Client with them.
See [Packages](packages) for the whole list.

## The four libraries

All the HoloNET code in your new app is in one file, **`HoloNETDemo.cs`**. The screen, page or endpoint only calls `HoloNETDemo.RunAsync()` and shows the text it returns,
so you can read the HoloNET code on its own and replace it with yours.

### `--HoloNET api`: one line each (the default)

The simplest way to use Holochain: save, load and delete your data, share it with every agent, and hear when it changes. No Rust and no conductor plumbing.

```csharp
await HoloNETAPI.SaveDataAsync("greeting", "Hello, Holochain!");
string greeting = (await HoloNETAPI.LoadDataAsync("greeting")).Result;

await HoloNETAPI.SaveDataAsync("visits", 1);                          // numbers, bools, dates, Guids, enums
int visits = (await HoloNETAPI.LoadDataAsync<int>("visits")).Result;

await HoloNETAPI.SaveSharedDataAsync("leaderboard", "ada", "9001");   // shared with every agent in the app
```

Every call returns an `OASISResult` with `IsSuccess`, `Result` and `Message`; nothing throws. Guide: [HoloNET API](api-guide).

### `--HoloNET client`: full control

Connect to the conductor, install your own hApp, make zome calls and receive signals.

```csharp
var admin = new HoloNETClientAdmin(dna);
await admin.ConnectAsync("ws://localhost:7777");
var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, "my.happ", "my-role");
var result = await installed.HoloNETClientAppAgent.CallZomeFunctionAsync("my_zome", "my_function", new { name = "Ada" });
```

Guide: [HoloNET Client](client-guide).

### `--HoloNET orm`: your own classes

Save, load, change and delete your own C# classes. HoloNET tracks the changes and keeps the hashes.

```csharp
var ada = new Avatar(client) { Username = "ada", FirstName = "Ada" };
await ada.SaveAsync();                    // creates it
ada.FirstName = "Grace";                  // HoloNET notices the change
await ada.SaveAsync();                    // updates it
await ada.DeleteAsync();
```

Guide: [HoloNET ORM](orm-guide).

### `--HoloNET hypernet`: players talking to each other

Real-time messaging between players: zones, area of interest, reliable messages.

```csharp
var session = new HyperNETSession(client, "forest", "Alice", new HyperNETOptions());
session.OnMessage += (s, e) => Console.WriteLine($"{e.From.PlayerName}: {Encoding.UTF8.GetString(e.Payload)}");
await session.JoinAsync();
await session.SendAsync("chat", Encoding.UTF8.GetBytes("hello"), HyperNETTarget.Zone);
```

Guide: [HoloNET HyperNET](hypernet-guide).

## Running a new app

On Windows, with a bundled conductor (the default), just run it: `dotnet run`, or press F5 in your IDE. HoloNET starts Holochain when the app starts and stops it when the app ends.

With an external conductor, start one first. Each template includes the scripts:

```bash
./start-conductor.sh        # Git Bash, Linux, macOS (needs Holochain 0.7.x on your PATH)
```

```powershell
.\start-conductor.ps1       # PowerShell
```

Then run the app as usual.

## Which hApp does it use?

The demos install the OASIS hApp (`oasis.happ`), which has avatar entries, collections, a per-key data store (what the API uses) and HyperNET's functions.
The HoloNET API package copies it next to your app. To use your own hApp, set `HoloNETDemo.HappPath` to your `.happ` and change the zome and function names in `HoloNETDemo.cs`.
You can generate a hApp from your C# classes: see [Generating hApps](star-happ-generation).

## Running on mobile and in the browser

Browsers can't run a Holochain conductor, so the Blazor WebAssembly template always connects to one you run. Phones can, and the Android templates use that.

**Android (`holonet-android`, `holonet-maui`, `holonet-maui-blazor`, `holonet-uno`).** On Android the app starts the Holochain runtime on the phone, the same way Our World does through the OASIS Edge runtime:

1. If the phone has the **Holochain Service Runtime** app installed, the app asks it for access. The user approves a system notification once, and the app uses the phone's one shared conductor (every Holochain app on the phone shares it).
2. If the runtime isn't installed, isn't approved in time (60 seconds by default) or setup fails, the app **falls back to the conductor built into the app**.
3. Zome calls are signed by the conductor, because an app has no admin access to sign for itself. `HoloNETAndroidRuntime.cs` (in the template) wires this into HoloNET with `IZomeCallSigner`.

It works with every `--HoloNET` choice: the Client and ORM demos use the app connection directly, the API uses `HoloNETAPI.UseAppClientAsync` (new in 4.6.0), and the HyperNET demo joins a zone as the one player the runtime holds.

The runtime libraries (client, service and bridge `.aar` files and their dependencies, about 38 MB, arm64 only) are published as the [oasis-android-libs-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-android-libs-v1.0.0) release and the template downloads them into `runtime/android` on the first build (set `HoloNETDownloadRuntime=false` to skip). The bridge source is in [android-runtime](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/android-runtime) and the build is the `oasis-android-libraries` workflow in the runtime repository. They need Android 8.1 (API 27) or newer. If they are missing the app connects to a conductor you run.

**Status, so you know what is proven.** The shared-runtime path is proven by the OASIS team on an x86_64 emulator, not yet on a physical phone. The template's C# wrapper compiles for Android and the real libraries are packed into the APK, but it has **not been run on a device or emulator**. Treat it as ready to test, not as verified.

**iOS (`holonet-ios`, and the iOS targets of MAUI, MAUI Blazor and Uno).** An iPhone can't run a separate conductor process, so the conductor is linked into the app (Holochain 0.7 with the wasmi interpreter, because iOS forbids JIT). `HoloNETIosRuntime.cs` (in the template) starts it through `HoloNETIosBridge.xcframework`, the `holo_ios_*` C ABI merged with the conductor, published as [oasis-ios-bridge-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-bridge-v1.0.0): download and unzip it into `runtime/ios` (about 127 MB). It runs on the **iOS simulator** in the OASIS team's CI, but is **not yet proven on a physical iPhone**, and the template's iOS wrapper has **not been compiled or run by us on a Mac** (this machine has no Mac). Without the library the iOS app connects to a conductor you run.

Where the phone runtimes live:

- **Android and iOS source:** [NextGenSoftwareUK/android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) (our fork of Holochain's [android-service-runtime](https://github.com/holochain/android-service-runtime); our Holochain 0.7 work is on the [`oasis/holochain-0.7`](https://github.com/NextGenSoftwareUK/android-service-runtime/tree/oasis/holochain-0.7) branch).
- **Android libraries for apps:** [oasis-android-libs-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-android-libs-v1.0.0) (client, service, bridge and dependencies, built by the `oasis-android-libraries` workflow).
- **iOS bridge for .NET apps:** [oasis-ios-bridge-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-bridge-v1.0.0) (built by the `oasis-ios-bridge` workflow).
- **Android runtime app releases:** [oasis-holochain-0.7-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.0.0) (pre-release: arm64 and x86_64 APKs, build manifest, iOS xcframework, Swift bindings, SHA256SUMS) and [oasis-runtime-v0.2.0-holochain-0.7](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-runtime-v0.2.0-holochain-0.7).
- **iOS releases:** [oasis-ios-conductor-v0.7.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.7.1) and [oasis-ios-conductor-v0.1.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.1.1).
- **OASIS Edge runtime** (what Our World uses to run on phones): see the table below.

**OASIS Edge runtime and OGEngineClient releases** (all in [NextGenSoftwareUK/OASIS releases](https://github.com/NextGenSoftwareUK/OASIS/releases); the OASIS repo holds the Edge runtime and Unity bridge source, not the Android and iOS runtimes). Pick the one that matches your app:

| You want | Release | Archives |
|---|---|---|
| The Edge runtime only (a non-Unity .NET host; libraries, not an app) | [OASIS-Edge-Runtime-v1.2.0](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Runtime-v1.2.0) | Windows x64, Linux x64/ARM64, macOS x64/ARM64 |
| OGEngineClient + Edge (managed and native game client with the local/offline Edge stack) | [OGEngineClient-Edge-v2.1.0](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-Edge-v2.1.0) | the same five platforms |
| OGEngineClient + Edge + Unity (the Unity package, with the OGEngineClient Edge integration) | [OASIS-Edge-Unity-v1.1.0](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Unity-v1.1.0) | one Unity package zip |
| OGEngineClient only, hosted services, no local Edge | [OGEngineClient-RemoteOnly-v2.1.0](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-RemoteOnly-v2.1.0) | the same five platforms |
| Older OGEngineClient (Edge-enabled, one managed archive) | [OGEngineClient-v2.0.2](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-v2.0.2) | one managed zip |

How they fit together is in [Edge and game-client distributions](https://github.com/NextGenSoftwareUK/OASIS/blob/Development/Docs/WIP/Edge%20and%20Games/EDGE_DISTRIBUTIONS.md) and [Edge runtime releases](https://github.com/NextGenSoftwareUK/OASIS/blob/Development/Docs/WIP/Edge%20and%20Games/EDGE_RUNTIME_RELEASES.md). The Holochain layer is Edge Runtime, then HoloOASIS.Edge, then HoloOASIS.Unity, then HoloNET and the Holochain conductor; Edge does not include a conductor by itself, so on a phone the conductor comes from the Android or iOS runtime above.

Everywhere the runtime isn't used (iOS, the browser, or Android without the runtime libraries):

- Start a conductor on your development computer with the included script. It listens on `ws://localhost:7777`.
- Set `HoloNETDemo.AdminUri` to reach it. An Android emulator reaches your computer at `ws://10.0.2.2:7777`; a real device needs your computer's address on the network.
- The conductor installs the hApp from a path on **its own** computer, so set `HoloNETDemo.HappPath` to where `oasis.happ` (or your hApp) is on the conductor's computer.

## Unity

`holonet-unity` is an `Assets` starter rather than a complete Unity project (a Unity project can't be generated by `dotnet new`). Create a Unity project, copy in the `Assets` and
`Packages` folders, let [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) restore `Assets/packages.config`, add the `HoloNETBehaviour` component to a GameObject and press Play.
It uses the netstandard2.1 build of HoloNET that Unity supports. `Assets/packages.config` lists every package HoloNET needs, because NuGetForUnity restores only what is listed.
`templates/test_unity.py` tests it in a real Unity 2022.3 editor: it restores the packages, compiles the scripts and runs the demo against the bundled conductor.

## Updating and uninstalling

```bash
dotnet new install NextGenSoftware.Holochain.HoloNET.Templates     # installs, or updates to the latest
dotnet new uninstall NextGenSoftware.Holochain.HoloNET.Templates
dotnet new list holonet                                             # shows every HoloNET template
```

## How the templates are made

They are generated from the standard .NET templates (`dotnet new blazor`, `dotnet new wpf` and so on) plus the HoloNET wiring, by
[`templates/build_templates.py`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/templates/build_templates.py), so they stay current with new .NET
versions. [`templates/test_templates.py`](https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/templates/test_templates.py) creates a project from each template, builds
it against the HoloNET packages and, where it can, runs it and checks that the demo works.
