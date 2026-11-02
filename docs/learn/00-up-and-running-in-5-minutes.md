# Up and running in 5 minutes

**You'll build:** a working .NET app that saves and loads data on Holochain, from one command, in the kind of app you already build (web, desktop, mobile, game).
**Time:** 5 minutes. **You need:** the [.NET 10 SDK](https://dotnet.microsoft.com/download). That's all: on Windows the app bundles the conductor, and on Linux and macOS HoloNET downloads the official one the first time it runs (see [Where the conductor comes from](https://docs.holonet.oasisomniverse.one/conductor)). Mobile and browser templates connect to a conductor you run.

HoloNET has project templates for the popular .NET app types, each with HoloNET already wired in. You choose the app type, and which HoloNET library it uses.

## 1. Install the templates

```bash
dotnet new install NextGenSoftware.Holochain.HoloNET.Templates
```

## 2. Create an app

Pick the app type you want:

```bash
dotnet new holonet-console -n MyApp       # console
dotnet new holonet-blazor -n MyApp        # Blazor web app
dotnet new holonet-wpf -n MyApp           # WPF desktop app
dotnet new holonet-maui -n MyApp          # .NET MAUI (mobile and desktop)
```

The full list is in [Templates](https://docs.holonet.oasisomniverse.one/templates): console, worker service, Web API, MVC, Razor Pages, Blazor (server and WebAssembly), WPF, WinForms,
WinUI, Avalonia, MAUI, MAUI Blazor, Uno, Android, iOS and Unity.

## 3. Run it

```bash
cd MyApp
dotnet run
```

On Windows the app bundles the Holochain conductor, starts it when your app starts and stops it when it ends. A console app prints something like this:

```text
greeting = Hello, Holochain!
visits = 1
ada's score = 9001
missing key: IsSuccess = False
```

For a web or desktop template, open the **HoloNET** page or press the **Run the HoloNET demo** button.

**A mobile or browser template, or a project created with `--Conductor external`?** Those use a conductor you run yourself. Start one in another terminal with the script that came with your project (it needs Holochain 0.7.x on your PATH):

```bash
./start-conductor.sh
```

## 4. Read the one file

Everything HoloNET does in your app is in **`HoloNETDemo.cs`**. With the default library (the one-line API) it's this:

```csharp
await HoloNETAPI.SaveDataAsync("greeting", "Hello, Holochain!");
string greeting = (await HoloNETAPI.LoadDataAsync("greeting")).Result;
```

Replace the demo with your own code, and you have an app on Holochain.

## 5. Try a different library

Create the same app again with another `--HoloNET` choice:

```bash
dotnet new holonet-console -n MyOrmApp --HoloNET orm          # save your own C# classes
dotnet new holonet-console -n MyClientApp --HoloNET client    # full control: install a hApp, zome calls, signals
dotnet new holonet-console -n MyGameApp --HoloNET hypernet    # player-to-player messaging
```

## Where next

- [A template for every app](07-a-template-for-every-app.md): how each app type calls HoloNET, with the code.
- [Save data in one line](02-save-data-in-one-line.md): the API in depth.
- [Model your data with the ORM](03-model-your-data.md) and [Multiplayer with HyperNET](05-multiplayer-with-hypernet.md).
- Every package and which one to pick: [Packages](https://docs.holonet.oasisomniverse.one/packages).
