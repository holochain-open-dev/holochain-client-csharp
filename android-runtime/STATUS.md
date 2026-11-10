# Android and iOS runtime wiring: status

Published builds (NextGenSoftwareUK/android-service-runtime, branch `oasis/holochain-0.7`):
- Android libraries: https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-android-libs-v1.0.0 (`oasis-android-libraries` workflow)
- iOS bridge: https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-bridge-v1.0.0 (`oasis-ios-bridge` workflow)

Verified here: `holonet-android` and `holonet-maui` (Android target) build with the downloaded libraries and the APK contains the conductor and oasis.happ.
Not verified: running on any device or emulator; `holonet-maui-blazor` and `holonet-uno` builds (same mechanism, not built); the iOS wrapper (no Mac; the iOS bridge
workflow confirmed the `holo_ios_*` symbols are in the static library); anything on a physical phone.
HoloNET API gained `UseAppClientAsync` (HoloNET-API commit 79adbdb), published with the next package release (4.6.0).
