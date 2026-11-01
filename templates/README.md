# HoloNET project templates

```bash
dotnet new install NextGenSoftware.Holochain.HoloNET.Templates
```

| Command | What you get |
|---|---|
| `dotnet new holonet-api` | The simplest start: one-line save/load/delete, shared data, signals (HoloNET API). |
| `dotnet new holonet-client` | Full control: connect, install your hApp, zome calls, signals (HoloNET Client). |

Options: `--Conductor auto|embedded|external` (bundle Holochain in the app, Windows; or use one you start), `--HolochainVersion both|0.7.1|0.7.0`.

Docs: https://docs.holonet.oasisomniverse.one/templates · Learn: https://learn.holonet.oasisomniverse.one
