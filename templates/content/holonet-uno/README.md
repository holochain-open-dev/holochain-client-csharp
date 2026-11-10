# HoloNETApp

A Uno Platform app with [HoloNET](https://holonet.oasisomniverse.one) wired in.

Everything HoloNET does in this app is in **`HoloNETDemo.cs`**: read it, then replace it with your own code. Each screen / page /
endpoint just calls `HoloNETDemo.RunAsync()` and shows the text it returns.

## Run it

On Android (and iOS, see below) this app uses the Holochain runtime on the phone (see "Holochain on the phone (Android)" below). Everywhere else it does not bundle
a conductor, so it connects to one you run on your development machine
(`ws://localhost:7777`). Start it with `./start-conductor.sh` (Git Bash, Linux, macOS) or `.\start-conductor.ps1` (PowerShell), and set
`HoloNETDemo.AdminUri` (an Android emulator reaches your machine at `ws://10.0.2.2:7777`). The conductor installs the hApp from a path on
*its* machine: set `HoloNETDemo.HappPath` to where `oasis.happ` (or your own `.happ`) is on the conductor's computer.

Then run the app as usual for this app type (`dotnet run`, or press F5 in your IDE).

## Holochain on the phone (Android)

On Android the app starts the Holochain runtime **on the phone**, the same way Our World does (OASIS Edge):

1. If the phone has the **Holochain Service Runtime** app installed, the app asks it for access and shows nothing more than a system
   notification the user approves once. The app then uses the phone's one shared conductor (every Holochain app on the phone shares it).
2. If the runtime is not installed, is not approved in time (60 seconds by default), or setup fails, the app **falls back to the conductor built into
   the app** (a foreground service).
3. The runtime libraries are downloaded into `runtime/android` on the first build (see `runtime/android/README.md`). If they are missing (for example you
   built with `-p:HoloNETDownloadRuntime=false`), the app connects to a conductor you run instead (`HoloNETDemo.AdminUri`).

Zome calls are signed by the conductor, because an app has no admin access to grant itself signing credentials. It works with every `--HoloNET`
choice: the Client and ORM demos use the app connection directly, the API uses `HoloNETAPI.UseAppClientAsync`, and the HyperNET demo joins a zone as the
one player the runtime holds.

**Status: not yet run on a physical phone.** The shared-runtime path is proven by the OASIS team on an x86_64 emulator, not on a physical phone, and this
template's wrapper has been compiled and packed with the real libraries for Android, but not run on a device or emulator. Please test it on yours and tell us what you find.

To try it: install the runtime on the emulator or phone (`adb install android-service-runtime-0.2.0-holochain-0.7-x86_64-debug.apk` for an x86_64
emulator, the arm64 APK for a phone), build and run the app, and approve the notification.

Runtime source and releases:

- Source: [android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) (a fork of [holochain/android-service-runtime](https://github.com/holochain/android-service-runtime); our Holochain 0.7 work is on the [`oasis/holochain-0.7`](https://github.com/NextGenSoftwareUK/android-service-runtime/tree/oasis/holochain-0.7) branch)
- Holochain 0.7 for OASIS: Android + iOS, one release with everything ([oasis-holochain-0.7-v1.1.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0)): the runtime apps (APKs), the Android libraries the templates download, the iOS conductor and the iOS bridge, with checksums and notes on what is proven
- OASIS Edge runtime (what Our World uses), by product: [Edge Runtime only](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Runtime-v1.2.0), [OGEngineClient + Edge](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-Edge-v2.1.0), [OGEngineClient + Edge + Unity](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Unity-v1.1.0), [OGEngineClient RemoteOnly](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-RemoteOnly-v2.1.0), all in the [OASIS releases](https://github.com/NextGenSoftwareUK/OASIS/releases)
- The bridge this template calls: [android-runtime](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/android-runtime) in the HoloNET repo

## Holochain on iOS

An iPhone cannot run a separate conductor process, so the conductor is linked into the app (Holochain 0.7 with the wasmi interpreter, because iOS forbids
JIT). On iOS the app starts it with `HoloNETIosRuntime.cs`, which calls `HoloNETIosBridge.xcframework`: put that in `runtime/ios` (see `runtime/ios/README.md`).
Without it the app connects to a conductor you run (`HoloNETDemo.AdminUri`).

**Status:** the conductor runs on the **iOS simulator** in the OASIS team's CI (it starts, installs the OASIS hApp and signs a zome call). It is **not yet proven
on a physical iPhone**, and this template's wrapper has **not been compiled or run by us on a Mac**. Treat it as ready to test. Runtime source and releases:

- Source: [android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) (a fork of [holochain/android-service-runtime](https://github.com/holochain/android-service-runtime); our Holochain 0.7 work is on the [`oasis/holochain-0.7`](https://github.com/NextGenSoftwareUK/android-service-runtime/tree/oasis/holochain-0.7) branch)
- Holochain 0.7 for OASIS: Android + iOS, one release with everything ([oasis-holochain-0.7-v1.1.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0)): the runtime apps (APKs), the Android libraries the templates download, the iOS conductor and the iOS bridge, with checksums and notes on what is proven
- OASIS Edge runtime (what Our World uses), by product: [Edge Runtime only](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Runtime-v1.2.0), [OGEngineClient + Edge](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-Edge-v2.1.0), [OGEngineClient + Edge + Unity](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Unity-v1.1.0), [OGEngineClient RemoteOnly](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-RemoteOnly-v2.1.0), all in the [OASIS releases](https://github.com/NextGenSoftwareUK/OASIS/releases)
- The bridge this template calls: [android-runtime](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/android-runtime) in the HoloNET repo

## Switch the HoloNET library

Created with `dotnet new holonet-uno --HoloNET api` (the default). The other choices:

| `--HoloNET` | What it is |
|---|---|
| `api` | One line to save, load and delete data, share it with every agent, and hear when it changes. Simplest. |
| `client` | Full control of the conductor: install a hApp, make zome calls, receive signals. |
| `orm` | Save, load and change your own C# classes (includes the Client). |
| `hypernet` | Real-time player-to-player messaging: zones, area of interest, reliable messages (includes the Client). |

## Learn more

- [Documentation](https://docs.holonet.oasisomniverse.one) and [tutorials](https://learn.holonet.oasisomniverse.one)
- [Examples](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/examples)
- [All HoloNET packages and templates](https://docs.holonet.oasisomniverse.one/packages)
