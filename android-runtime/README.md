# HoloNET Android runtime bridge

The Kotlin bridge the `dotnet new` templates for Android (`holonet-android`, `holonet-maui`, `holonet-maui-blazor`, `holonet-uno`) call over JNI
from `HoloNETAndroidRuntime.cs`. It is the same bridge OASIS Edge uses for Our World (copied from the OASIS repo; it has no Unity dependency).

What it does (`HolochainUnityBridge.kt`):

1. **Shared first.** If the phone has the Holochain Service Runtime app installed (`org.holochain.androidserviceruntime.app`), it asks that app
   for access. The runtime posts a notification and the user approves once. The app then uses the phone's one shared conductor.
2. **Embedded fallback.** If the runtime is missing, not approved within the timeout, or setup fails, it starts the conductor built into the app
   (`HolochainService` as a foreground service).
3. **Signing.** `signZomeCall` asks the conductor to sign with the agent key it holds, because an app has no admin access to grant itself signing
   credentials. `HoloNETAndroidRuntime.cs` plugs this into HoloNET as an `IZomeCallSigner`.

The class keeps the package name `one.oasisomniverse.holooasis.unity.HolochainUnityBridge` so the same compiled library works for Unity and .NET.

## Building the libraries

Published build: the [oasis-holochain-0.7-v1.1.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0) release has the three `.aar` files and the bridge's dependencies (built by the `oasis-android-libraries` workflow in the runtime repository, which also holds a copy of this bridge in `oasis/android-bridge`). The templates download it on the first build. The bridge compiles against the Holochain runtime libraries, which come from
[android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) (branch `oasis/holochain-0.7`):

- `client` and `service` for Holochain 0.7.1, built from that source with Rust and the Android NDK (the conductor library is about 100 MB unpacked, arm64 only).
- The workflow builds them, then the bridge, and publishes the release. Run it by pushing a tag `oasis-android-libs-v*` on that branch.

## Status

- Compiled as part of the OASIS Edge Unity package, and proven on an x86_64 emulator: with no runtime installed it falls back to the embedded conductor
  and a signed `call_zome` succeeds; with the old v0.1.5 runtime it reports `shared_unavailable` and falls back.
- **Not yet run on a physical phone**, and not yet run from a .NET (non-Unity) app: `HoloNETAndroidRuntime.cs` builds for Android with the real libraries (they are packed into the APK) but has not been run on a
  device or emulator.
- Android only. iOS has a separate conductor and bridge ([oasis-holochain-0.7-v1.1.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0)) that runs on the simulator and is not yet proven on a physical iPhone.
