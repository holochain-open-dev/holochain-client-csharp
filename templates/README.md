# HoloNET project templates

```bash
dotnet new install NextGenSoftware.Holochain.HoloNET.Templates
```

| Command | What you get |
|---|---|
| `dotnet new holonet-<app type>` | See the list below. |
| `dotnet new holonet-client` | Full control: connect, install your hApp, zome calls, signals (HoloNET Client). |

Options: `--Conductor auto|embedded|external` (bundle Holochain in the app, Windows; or use one you start), `--HolochainVersion both|0.7.1|0.7.0`.

Android (`holonet-android`, `holonet-maui`, `holonet-maui-blazor`, `holonet-uno`): the app starts the Holochain runtime on the phone (the shared Holochain Service Runtime if installed, else a built-in conductor) using `--HoloNET client` or `orm`; the runtime `.aar` libraries go in `runtime/android`. Ready to test, not yet run on a device. iOS still connects to a conductor you run. See [android-runtime](../android-runtime) and the docs.

Docs: https://docs.holonet.oasisomniverse.one/templates · Learn: https://learn.holonet.oasisomniverse.one
