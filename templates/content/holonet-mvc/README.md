# HoloNETApp

A ASP.NET Core MVC web app with [HoloNET](https://holonet.oasisomniverse.one) wired in.

Everything HoloNET does in this app is in **`HoloNETDemo.cs`**: read it, then replace it with your own code. Each screen / page /
endpoint just calls `HoloNETDemo.RunAsync()` and shows the text it returns.

## Run it

On Windows the default is a bundled conductor (the app starts and stops Holochain itself). Otherwise start one first, in another terminal:

```bash
./start-conductor.sh        # Git Bash, Linux, macOS (needs Holochain 0.7.x on your PATH)
.\start-conductor.ps1       # PowerShell
```

Choose with `--Conductor auto|embedded|external` and `--HolochainVersion both|0.7.1|0.7.0` when you create the project.

Then run the app as usual for this app type (`dotnet run`, or press F5 in your IDE).

## Switch the HoloNET library

Created with `dotnet new holonet-mvc --HoloNET api` (the default). The other choices:

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
