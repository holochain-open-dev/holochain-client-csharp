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
| `holonet-maui` | .NET MAUI | a button on the main page | No: connects to your conductor |
| `holonet-maui-blazor` | .NET MAUI Blazor Hybrid | the **HoloNET** page, with a button | No: connects to your conductor |
| `holonet-uno` | Uno Platform | a button on the main page | No: connects to your conductor |
| `holonet-android` | Android (native) | a button on the main activity | No: connects to your conductor |
| `holonet-ios` | iOS (native) | a button on the main screen | No: connects to your conductor |
| `holonet-unity` | Unity (an `Assets` starter) | a `HoloNETBehaviour` component | Windows standalone builds |

Why some can't bundle a conductor: Holochain's conductor is a desktop program. A browser, a phone or a tablet can't run it, so those apps connect to a
conductor you run on your computer (or on a server) instead. See [Running on mobile and in the browser](#running-on-mobile-and-in-the-browser).

## The options

Every template has the same options.

| Option | Values | What it does |
|---|---|---|
| `--HoloNET` | `api` (default), `client`, `orm`, `hypernet` | Which HoloNET library the app uses. |
| `--Conductor` | `auto` (default), `embedded`, `external` | `embedded` bundles the Holochain conductor in the app (Windows) and starts and stops it for you. `external` uses a conductor you run yourself on `ws://localhost:7777`. `auto` is `embedded` on Windows and `external` elsewhere. |
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

Browsers and phones can't run a Holochain conductor, so the Blazor WebAssembly, MAUI, MAUI Blazor, Uno, Android and iOS templates connect to one you run.

- Start a conductor on your development computer with the included script. It listens on `ws://localhost:7777`.
- Set `HoloNETDemo.AdminUri` to reach it. An Android emulator reaches your computer at `ws://10.0.2.2:7777`; a real device needs your computer's address on the network.
- The conductor installs the hApp from a path on **its own** computer, so set `HoloNETDemo.HappPath` to where `oasis.happ` (or your hApp) is on the conductor's computer.

## Unity

`holonet-unity` is an `Assets` starter rather than a complete Unity project (a Unity project can't be generated by `dotnet new`). Create a Unity project, copy in the `Assets` and
`Packages` folders, let [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) restore `Assets/packages.config`, add the `HoloNETBehaviour` component to a GameObject and press Play.
It uses the netstandard2.1 build of HoloNET that Unity supports. The HoloNET code is compiled against .NET Standard 2.1 in the template tests, but the starter has not been opened in the Unity editor.

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
