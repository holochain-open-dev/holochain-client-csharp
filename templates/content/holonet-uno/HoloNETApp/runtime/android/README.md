# Holochain runtime libraries for Android

The first time you build for Android, the project downloads the Holochain runtime libraries from the
[oasis-android-libs-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-android-libs-v1.0.0) release (about 38 MB) into this folder, and includes every `*.aar` and `*.jar` here
(`HoloNETApp.csproj`). `HoloNETAndroidRuntime.cs` then finds the bridge class in them.

| File | What it is |
|---|---|
| `holochain-client.aar` | Client for the Holochain Service Runtime (the shared conductor on the phone), with the Holochain 0.7 parcelers |
| `holochain-service.aar` | The Holochain conductor as an Android service (arm64 only): the built-in conductor used when the phone has no shared runtime |
| `holonet-android-bridge.aar` | The Kotlin bridge that picks shared or built-in and signs zome calls (source: `android-runtime/` in the HoloNET repo) |
| `holochain-android-dependencies.zip` | The bridge's runtime dependencies (unpacked here as `.jar` / `.aar` files) |

To use your own build instead, set `HoloNETDownloadRuntime=false` (`dotnet build -p:HoloNETDownloadRuntime=false`) and put your files here.
The libraries need Android 8.1 (API 27) or newer, so the template sets that as the minimum.
