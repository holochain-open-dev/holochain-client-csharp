# A template for every app

**You'll learn:** how each kind of .NET app calls HoloNET, so you can drop HoloNET into the app you already have.
**Time:** 10 minutes. **You need:** [Up and running in 5 minutes](00-up-and-running-in-5-minutes.md) done once.
**The templates:** [`templates/`](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/templates) · [Templates reference](https://docs.holonet.oasisomniverse.one/templates).

Every template keeps its HoloNET code in `HoloNETDemo.cs` and calls `HoloNETDemos.HoloNETDemo.RunAsync()` from the place that fits the app type. That one call is the only line that differs,
so here is each app type's wiring. To add HoloNET to an existing app, copy `HoloNETDemo.cs` into it, add the package the template's `.csproj` uses, and make the same call.

## Console and background services

```csharp
// holonet-console: Program.cs
Console.WriteLine(await HoloNETDemos.HoloNETDemo.RunAsync());
```

```csharp
// holonet-worker: Worker.cs. Runs once when the service starts, then idles.
logger.LogInformation("HoloNET demo:\n{result}", await HoloNETDemos.HoloNETDemo.RunAsync());
```

## Web: Web API, MVC, Razor Pages, Blazor

```csharp
// holonet-webapi: GET /holonet
app.MapGet("/holonet", async () => Results.Text(await HoloNETDemos.HoloNETDemo.RunAsync()));
```

```csharp
// holonet-mvc: Controllers/HoloNETController.cs
public async Task<IActionResult> Index() => View((object)await HoloNETDemos.HoloNETDemo.RunAsync());
```

```csharp
// holonet-razor: Pages/HoloNET.cshtml.cs
public async Task OnGetAsync() => Result = await HoloNETDemos.HoloNETDemo.RunAsync();
```

```razor
@* holonet-blazor and holonet-blazorwasm: Pages/HoloNET.razor *@
<button class="btn btn-primary" @onclick="Run">Run the HoloNET demo</button>
<pre>@output</pre>

@code {
    private string output = "";
    private async Task Run() => output = await HoloNETDemos.HoloNETDemo.RunAsync();
}
```

Blazor WebAssembly runs in the browser, which can't host a conductor, so it connects to one you run (see the templates reference).

## Desktop: WPF, WinForms, WinUI, Avalonia

```csharp
// holonet-wpf, holonet-winui and holonet-avalonia: a button's click handler
private async void RunButton_Click(object sender, RoutedEventArgs e)
{
    RunButton.IsEnabled = false;
    Output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
    RunButton.IsEnabled = true;
}
```

```csharp
// holonet-winforms: Form1.cs
runButton.Click += async (s, e) => output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
```

HoloNET is `async` all the way through, so the window stays responsive while Holochain works.

## Mobile and cross-platform: MAUI, MAUI Blazor, Uno, Android, iOS

```csharp
// holonet-maui: MainPage.xaml.cs
private async void OnRunClicked(object? sender, EventArgs e) => OutputLabel.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
```

```csharp
// holonet-android: MainActivity.cs
button.Click += async (s, e) => output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
```

```csharp
// holonet-ios: SceneDelegate.cs
button.TouchUpInside += async (s, e) => output.Text = await HoloNETDemos.HoloNETDemo.RunAsync ();
```

On Android the templates (`holonet-android`, `holonet-maui`, `holonet-maui-blazor`, `holonet-uno`) use the Holochain runtime on the phone, the way Our World does through the OASIS Edge runtime: the app asks the phone's shared **Holochain Service Runtime** for access (the user approves a notification once) and, if it isn't installed or isn't approved in time, **falls back to a conductor built into the app**. It works with every `--HoloNET` choice, and the runtime libraries ([oasis-android-libs-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-android-libs-v1.0.0)) download on the first build. This is ready to test rather than verified: the shared runtime is proven on an emulator, and the template's wrapper builds with the real libraries but hasn't been run on a device.

An iPhone can't run a separate conductor, so the iOS runtime links one into the app: download [oasis-ios-bridge-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-bridge-v1.0.0) into `runtime/ios` and `holonet-ios` uses it. It runs on the simulator but isn't proven on a physical iPhone yet, and we haven't compiled the iOS wrapper on a Mac. Without it the app connects to a conductor you run. Set `HoloNETDemo.AdminUri` to reach it (an Android emulator uses `ws://10.0.2.2:7777`) and `HoloNETDemo.HappPath` to where the hApp is on the conductor's computer.

The runtimes: [android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) (source, Android releases [oasis-holochain-0.7-v1.0.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.0.0), iOS releases [oasis-ios-conductor-v0.7.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.7.1)) and the OASIS Edge releases: [Edge Runtime only](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Runtime-v1.2.0), [OGEngineClient + Edge](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-Edge-v2.1.0), [OGEngineClient + Edge + Unity](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Unity-v1.1.0) and [OGEngineClient RemoteOnly](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-RemoteOnly-v2.1.0). The full details are in the [templates guide](https://docs.holonet.oasisomniverse.one/templates#running-on-mobile-and-in-the-browser).

## Games: Unity

Create a Unity project, copy in the `Assets` folder from `dotnet new holonet-unity`, let NuGetForUnity restore the packages and add the `HoloNETBehaviour` component:

```csharp
// holonet-unity: Assets/HoloNET/HoloNETBehaviour.cs
private async void Run() => output = await HoloNETDemos.HoloNETDemo.RunAsync();
```

For a multiplayer game, create it with `--HoloNET hypernet`: players join a zone, send each other messages and get an area of interest ([Multiplayer with HyperNET](05-multiplayer-with-hypernet.md)).

## What the demo does for each library

| `--HoloNET` | What `RunAsync()` does |
|---|---|
| `api` | Saves and loads a string and a number, shares a score with every agent, and shows a missing key returning `IsSuccess = false` instead of throwing. |
| `client` | Connects to the conductor, installs the hApp, creates an avatar with a zome call, and uninstalls. |
| `orm` | Saves an `Avatar` class, loads it back, changes it (HoloNET notices), saves again and deletes it. |
| `hypernet` | Two players join a zone; one sends a message and the other receives it. |

## Next

- [Save data in one line](02-save-data-in-one-line.md), [Model your data with the ORM](03-model-your-data.md), [Multiplayer with HyperNET](05-multiplayer-with-hypernet.md).
- [Generate a hApp](06-generate-a-happ.md) when you want your own zomes instead of the OASIS hApp.
