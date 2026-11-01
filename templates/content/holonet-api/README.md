# HoloNETApp

A HoloNET API app: one line to save, load and delete data on [Holochain](https://holochain.org), share it with every agent,
get told when it changes, and message other agents.

## Run it

```bash
dotnet run
```

If you chose a bundled conductor (the default on Windows) the app starts and stops Holochain for you.
Otherwise start a conductor first, on `ws://localhost:7777`, in another terminal:

```bash
./start-conductor.sh        # Git Bash, Linux, macOS (needs Holochain on your PATH on Linux/macOS)
.\start-conductor.ps1       # PowerShell on Windows
```

To bundle the conductor in the app instead (Windows), create the project with `dotnet new holonet-api --Conductor embedded`.

## Learn more

- [HoloNET API guide](https://docs.holonet.oasisomniverse.one/api-guide): every call.
- [Learn HoloNET](https://learn.holonet.oasisomniverse.one): step-by-step tutorials.
- [Examples](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/examples): the Client, ORM, API, HyperNET, hApp generation and a live web site.
- [HoloNET](https://holonet.oasisomniverse.one) · [NuGet](https://www.nuget.org/profiles/NextGenSoftware)
