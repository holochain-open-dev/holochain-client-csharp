# HoloNETApp

A Uno Platform app with [HoloNET](https://holonet.oasisomniverse.one) wired in.

Everything HoloNET does in this app is in **`HoloNETDemo.cs`**: read it, then replace it with your own code. Each screen / page /
endpoint just calls `HoloNETDemo.RunAsync()` and shows the text it returns.

## Run it

On Android this app uses the Holochain runtime on the phone (see "Holochain on the phone (Android)" below). Everywhere else it does not bundle
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
3. If the bridge libraries are not in `runtime/android` (see `runtime/android/README.md`), the app connects to a conductor you run instead
   (`HoloNETDemo.AdminUri`).

Zome calls are signed by the conductor (`AndroidRuntimeZomeCallSigner`), because an app has no admin access to grant itself signing credentials.
The demo uses the runtime with `--HoloNET client` or `--HoloNET orm`. The HoloNET API and HyperNET libraries install the hApp through the conductor's
admin interface, which the phone's runtime does not expose to apps, so with those two the app still connects to a conductor you run.

**Status: not yet run on a physical phone.** The shared-runtime path is proven by the OASIS team on an x86_64 emulator, not on a physical phone, and this
template's wrapper has been compiled for Android but not run on a device or emulator. Please test it on yours and tell us what you find.

To try it: install the runtime on the emulator or phone (`adb install android-service-runtime-0.2.0-holochain-0.7-x86_64-debug.apk` for an x86_64
emulator, the arm64 APK for a phone), put the `.aar` files in `runtime/android`, run the app and approve the notification.

Runtime source and releases:

- Source: [android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) (a fork of [holochain/android-service-runtime](https://github.com/holochain/android-service-runtime); our Holochain 0.7 work is on the [`oasis/holochain-0.7`](https://github.com/NextGenSoftwareUK/android-service-runtime/tree/oasis/holochain-0.7) branch)
- Android releases: [oasis-holochain-0.7-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.0.0) (pre-release: arm64 and x86_64 runtime APKs, build manifest, SHA256SUMS) and [oasis-runtime-v0.2.0-holochain-0.7](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-runtime-v0.2.0-holochain-0.7)
- iOS releases: [oasis-ios-conductor-v0.7.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.7.1) and [oasis-ios-conductor-v0.1.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.1.1)
- OASIS Edge runtime (what Our World uses): [OASIS releases](https://github.com/NextGenSoftwareUK/OASIS/releases): `OASIS-Edge-Runtime-v1.2.0`, `OASIS-Edge-Unity-v1.1.0`, `OGEngineClient-Edge-v2.1.0`
- The bridge this template calls: [android-runtime](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/android-runtime) in the HoloNET repo

## Holochain on iOS

An iPhone cannot run a separate conductor process, so the iOS runtime links the conductor into the app. The OASIS team has built it
(`HolochainConductor.xcframework` with Swift bindings) and runs it on the **iOS simulator**: it starts, installs the OASIS hApp and signs a zome call.
It is **not yet proven on a physical iPhone**, and these templates do not wire it in: on iOS the app connects to a conductor you run
(`HoloNETDemo.AdminUri`). Runtime source and releases:

- Source: [android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) (a fork of [holochain/android-service-runtime](https://github.com/holochain/android-service-runtime); our Holochain 0.7 work is on the [`oasis/holochain-0.7`](https://github.com/NextGenSoftwareUK/android-service-runtime/tree/oasis/holochain-0.7) branch)
- Android releases: [oasis-holochain-0.7-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.0.0) (pre-release: arm64 and x86_64 runtime APKs, build manifest, SHA256SUMS) and [oasis-runtime-v0.2.0-holochain-0.7](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-runtime-v0.2.0-holochain-0.7)
- iOS releases: [oasis-ios-conductor-v0.7.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.7.1) and [oasis-ios-conductor-v0.1.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.1.1)
- OASIS Edge runtime (what Our World uses): [OASIS releases](https://github.com/NextGenSoftwareUK/OASIS/releases): `OASIS-Edge-Runtime-v1.2.0`, `OASIS-Edge-Unity-v1.1.0`, `OGEngineClient-Edge-v2.1.0`
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
