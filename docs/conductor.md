# Where the conductor comes from

HoloNET talks to a Holochain **conductor**. When `AutoStartHolochainConductor` is on (the default) HoloNET starts one for you, and it finds the program to start in this order:

| # | Source | When it is used |
|---|---|---|
| 1 | **Your own conductor** | `AutoStartHolochainConductor = false` and `HolochainConductorAdminURI` pointing at a conductor you run (`hc sandbox`, a launcher, a server). HoloNET starts nothing. |
| 2 | **The conductor bundled in an Embedded package** | `HolochainConductorMode = UseEmbedded` (the default when you installed a `.Embedded` package for your computer). Works offline; nothing is downloaded. |
| 3 | **A conductor you point at** | `HolochainConductorMode = UseExternal` and `FullPathToExternalHolochainConductorBinary` (and `FullPathToExternalHCToolBinary` for `hc`). |
| 4 | **The conductor on the PATH, or the one HoloNET downloads** | `HolochainConductorMode = UseSystemGlobal` (the default everywhere else). If `holochain` is on the PATH it is used; if not, and `AutoDownloadConductor` is on (the default), HoloNET downloads it. |

So with the plain `NextGenSoftware.Holochain.HoloNET.Client` package, an app on a computer with no Holochain installed just works: the first run downloads the conductor, every later run reuses it.

## AutoDownloadConductor

| Setting | Default | What it does |
|---|---|---|
| `AutoDownloadConductor` | `true` | Download `holochain` and `hc` when HoloNET needs a conductor and cannot find one. Set it to `false` on machines that must never download (then use an Embedded package, `UseExternal`, or put the files in the cache folder yourself). |
| `HolochainConductorVersion` | `""` | The Holochain release to download, for example `"0.7.1"`. Empty uses `EmbeddedHolochainVersion` (0.7.1). Setting it also makes HoloNET prefer that exact version over whatever is on the PATH. |
| `AllowUnverifiedConductorDownload` | `false` | Only for a release that has neither a SHA-256 in HoloNET's table nor a digest published on GitHub. Leave it off. |

```csharp
// Download Holochain 0.7.0 the first time, then reuse it. Nothing else to install.
var admin = new HoloNETClientAdmin(new HoloNETDNA { HolochainConductorVersion = "0.7.0" });

// A machine that must not download:
var offline = new HoloNETClientAdmin(new HoloNETDNA { AutoDownloadConductor = false });
```

### What is downloaded, and how it is checked

- Only the official **stable** releases at <https://github.com/holochain/holochain/releases> are used (never the `unstable` builds), for the computer's own operating system and processor: Windows x64, Linux x64 and arm64, macOS Intel and Apple Silicon.
- Every file is checked against its **SHA-256** before it is used. HoloNET ships a table of the hashes of every release from 0.5.3 to the release it was built with (`scripts/update_conductor_hashes.py` rebuilds it from the digests GitHub publishes); a release newer than the table is checked against the digest GitHub publishes for it. A file that does not match is deleted and the start fails. Nothing is ever run unchecked unless you set `AllowUnverifiedConductorDownload`.
- Files are cached in `%APPDATA%/NextGenSoftware/HoloNET/<version>/` (`~/.config/NextGenSoftware/HoloNET/<version>/` on Linux and macOS), the same folder an Embedded conductor is unpacked to. Set the `HOLONET_CONDUCTOR_FOLDER` environment variable to use another folder, laid out as `<folder>/<version>/holochain` and `hc`.
- HoloNET speaks the Holochain **0.7** conductor API. Other releases download fine, but only 0.7.0 and 0.7.1 are tested.

### Offline machines

Copy `holochain` and `hc` for the version you need into the cache folder (or the folder named by `HOLONET_CONDUCTOR_FOLDER`) on the offline machine: HoloNET finds them and downloads nothing. Or install an **Embedded** package, which carries the conductor inside it.

### Servers and containers

On Linux (a Docker image, Railway, a VM) the plain Client package is all you need. To keep container start-up fast, run your app once during the image build, or mount the cache folder on a persistent volume, so the 50 MB download is not repeated on every start. The conductor is started directly (no PowerShell), and stopped when the app disconnects.

### Phones

Android apps use the Holochain runtime on the phone instead of a conductor HoloNET starts: the shared Holochain Service Runtime app if the phone has it (the user approves access once), otherwise a conductor built into the app. This is what Our World does through the OASIS Edge runtime. The app gets a local app-interface port and a token (no admin connection) and the conductor signs zome calls, through `HoloNETClientAppBase.ZomeCallSigner`. The `holonet-android`, `holonet-maui`, `holonet-maui-blazor` and `holonet-uno` templates wire this in: see [Templates](templates#running-on-mobile-and-in-the-browser), and the runtime source and releases at [android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) and the OASIS Edge releases ([Edge Runtime only](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Runtime-v1.2.0), [OGEngineClient + Edge](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-Edge-v2.1.0), [OGEngineClient + Edge + Unity](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Unity-v1.1.0), [OGEngineClient RemoteOnly](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-RemoteOnly-v2.1.0)). It is ready to test, not yet proven on a physical phone. An iOS conductor exists ([releases](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.7.1)) and runs on the simulator, not yet on a physical iPhone. Android and iOS runtime libraries for apps: [Holochain 0.7 for OASIS: Android + iOS (oasis-holochain-0.7-v1.1.0)](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0) (the templates download the Android ones on the first build).

## Embedded packages

`NextGenSoftware.Holochain.HoloNET.Client.Embedded` (and the `.Holochain071` / `.Holochain070` single-version packages) carry the conductor inside the DLL, for apps that must work offline or must not download anything at run time (Unity games, installers). There are Embedded packages for every system Holochain publishes: Windows x64 (`.Embedded`), Linux x64 (`.Embedded.LinuxX64`), Linux arm64 (`.Embedded.LinuxArm64`), macOS Intel (`.Embedded.MacX64`) and macOS Apple Silicon (`.Embedded.MacArm64`), each with both Holochain versions or just `.Holochain071` / `.Holochain070`, for the Client, ORM, HyperNET and API. Each package holds one system and one processor, so none is bigger than the Windows one. The conductor is unpacked on first use into the same cache folder the download uses (and made executable on Linux and macOS). A package for a different system than the one it runs on is noticed and HoloNET falls back to the PATH and `AutoDownloadConductor`. See [Packages](packages.md) for the full list.
