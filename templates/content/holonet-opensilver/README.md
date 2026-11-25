# HoloNETApp

A OpenSilver app (Silverlight-style XAML in the browser) with [HoloNET](https://holonet.oasisomniverse.one) wired in.

Everything HoloNET does in this app is in **`HoloNETDemo.cs`**: read it, then replace it with your own code. Each screen / page /
endpoint just calls `HoloNETDemo.RunAsync()` and shows the text it returns.

## Run it

This app type does not bundle a Holochain conductor, so it connects to one you run on your development machine
(`ws://localhost:7777`). Start it with `./start-conductor.sh` (Git Bash, Linux, macOS) or `.\start-conductor.ps1` (PowerShell), and set
`HoloNETDemo.AdminUri` (an Android emulator reaches your machine at `ws://10.0.2.2:7777`). The conductor installs the hApp from a path on
*its* machine: set `HoloNETDemo.HappPath` to where `oasis.happ` (or your own `.happ`) is on the conductor's computer.

Run the browser head with `dotnet run --project HoloNETApp.Browser`, or the desktop simulator with `dotnet run --project HoloNETApp.Simulator` (Windows). Both share `HoloNETApp`. A browser cannot host a conductor, so the app connects to one you run. The browser head needs the WebAssembly build tools: `dotnet workload install wasm-tools`. Generating this template needs `dotnet new install OpenSilver.Templates`.

## Switch the HoloNET library

Created with `dotnet new holonet-opensilver --HoloNET api` (the default). The other choices:

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
