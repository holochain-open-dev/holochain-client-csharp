# Holochain conductor for iOS

Put `HoloNETIosBridge.xcframework` in this folder. The project links it into the app (`HoloNETApp.csproj`, `NativeReference`) and `HoloNETIosRuntime.cs`
calls its C functions (`holo_ios_*`). It contains the Holochain 0.7 conductor (wasmi interpreter, device and Apple-silicon simulator) and the bridge.

Download `HoloNETIosBridge.xcframework.zip` (about 127 MB) from the [oasis-holochain-0.7-v1.1.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0) release and unzip it here. It is built by that repository's `oasis-ios-bridge` workflow, which compiles the Swift C-ABI bridge and merges it with the
[oasis-ios-conductor-v0.7.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.7.1) conductor.

The bridge build confirmed the `holo_ios_*` symbols are in the library for device and simulator; it has not been linked into a .NET app by us. The bridge build confirmed the `holo_ios_*` symbols are in the library for device and simulator; it has not been linked into a .NET app by us. Without it the app still builds, `HoloNETIosRuntime.IsBridgeAvailable` is false, and the demo connects to a conductor you run (`HoloNETDemo.AdminUri`).
The conductor is large (about 200 MB per slice before the linker strips it).
