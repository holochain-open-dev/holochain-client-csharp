# Holochain runtime libraries for Android

Put the Holochain Android libraries (`.aar` files) in this folder. The project includes every `*.aar` here when it builds for Android
(`HoloNETApp.csproj`), and `HoloNETAndroidRuntime.cs` finds the bridge class in them.

You need three libraries, built from the runtime source (see the links in the main README.md):

| File | What it is |
|---|---|
| `holochain-client.aar` | Client for the Holochain Service Runtime (the shared conductor on the phone), with the Holochain 0.7 parcelers |
| `holochain-service.aar` | The Holochain conductor as an Android service: used for the built-in conductor when the phone has no shared runtime |
| `holonet-android-bridge.aar` | The Kotlin bridge that picks shared or built-in and signs zome calls (source: `android-runtime/` in the HoloNET repo) |

The conductor library is large (about 100 MB, arm64 only), so these are not shipped in the template. Without them the app still builds,
`HoloNETAndroidRuntime.IsBridgeAvailable` is false, and the demo connects to a conductor you run (`HoloNETDemo.AdminUri`).
