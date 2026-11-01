# HoloNET sandbox

The service behind [sandbox.holonet.oasisomniverse.one](https://sandbox.holonet.oasisomniverse.one): try HoloNET from a browser with nothing to install.

It is a small ASP.NET Core app (`HoloNET.Sandbox/`) that runs the HoloNET API, Client, ORM and HyperNET against a Holochain conductor. The page is a single static file,
`HoloNET.Sandbox/wwwroot/index.html`, which the service serves itself and which is also deployed as a static site
([HoloNETSandboxWebsite](https://github.com/NextGenSoftwareUK/HoloNETSandboxWebsite)); change it here and copy it there.

## What a visitor can do

| Feature | How it works |
|---|---|
| **Your data** | A private key-value store: `HoloNETAPI.SaveDataAsync`, `LoadDataAsync`, `DeleteDataAsync`. Each visitor gets a session id and their keys are prefixed with it, so visitors never see each other's data. |
| **Shared board** | `HoloNETAPI.SaveSharedDataAsync`: one namespace every visitor reads and writes. |
| **Run a demo** | One click runs the API, Client, ORM or HyperNET demo (the same code as the `dotnet new holonet-...` templates) and shows its output. |

## Safety

- **It never runs code a visitor sends.** The page can only call the fixed operations in `Program.cs`; a demo is chosen by name from a fixed list.
- **Limits:** keys are 1 to 40 letters, digits, dots, dashes or underscores; values are at most 500 characters; 25 keys per session; board messages are 280 characters and the latest 100 are kept.
- **Rate limits:** 120 requests a minute per address, 6 demo runs a minute per address, 2 demos at once, 90 seconds per demo, 500 sessions.
- **Clean-up:** a session's keys are deleted after 30 minutes idle.
- **It is public and shared:** nothing private belongs in it. The page says so.

## Run it on your computer

You need the .NET 10 SDK and a Holochain 0.7 conductor. On Windows the conductor binaries are in the HoloNET repository:

```bash
# 1. a conductor on ws://localhost:7777 (from the repository root)
./examples/start-conductor.sh            # Git Bash, Linux, macOS (needs hc and holochain on PATH outside Windows)
.\examples\start-conductor.ps1           # PowerShell

# 2. the sandbox, in another terminal
cd sandbox/HoloNET.Sandbox
dotnet run                               # http://localhost:5000
```

Open the address it prints. Settings, all optional:

| Environment variable | Default | What it is |
|---|---|---|
| `HOLONET_ADMIN_URI` | `ws://localhost:7777` | The conductor's admin interface. |
| `HOLONET_HAPP_PATH` | `oasis.happ` next to the app | The hApp the demos install. |
| `SANDBOX_ORIGINS` | `https://sandbox.holonet.oasisomniverse.one` | Web origins allowed to call the API from another site (localhost is always allowed). |
| `ASPNETCORE_URLS` | `http://localhost:5000` | Where it listens. |

## Hosting it

The static page needs nothing but static hosting (it is on Vercel). The **service** needs a machine that can run a Holochain conductor next to it:

- HoloNET's bundled conductors are Windows programs, so a Windows host can run `start-conductor.ps1` and the service side by side.
- On Linux, install Holochain 0.7.x (`hc` and `holochain`) and run `examples/start-conductor.sh`, then the service.
- Put the service behind HTTPS at `https://api.sandbox.holonet.oasisomniverse.one` (the page's default) and set `SANDBOX_ORIGINS` to the page's origin.

The hosted service is a deployment decision (a server, DNS and a small monthly cost). Until it is running, the page shows that the backend can't be reached and explains how to run it locally.
