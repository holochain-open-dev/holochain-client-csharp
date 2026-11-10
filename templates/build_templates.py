#!/usr/bin/env python3
"""Builds templates/content/holonet-<app type>/ : the stock .NET template for each app type (dotnet new <base>), with HoloNET wired in.

    python templates/build_templates.py            # every app type
    python templates/build_templates.py maui wpf   # just these

Each generated template has the same options:
    --HoloNET api|client|orm|hypernet        which HoloNET library the app uses (api = the one-line API, the default)
    --Conductor auto|embedded|external        bundle the Holochain conductor in the app (Windows) or use one you start
    --HolochainVersion both|0.7.1|0.7.0       with a bundled conductor, which Holochain version(s) to bundle
The wiring is one file, HoloNETDemo.cs (templates/shared/HoloNETDemo.cs), plus a small screen / page / endpoint per app type that
calls it. The content folders are generated: change this script or templates/shared, not the output.

The Android-capable types (android, maui, maui-blazor, uno) also get HoloNETAndroidRuntime.cs: on Android they start the Holochain runtime on the phone
(the shared Holochain Service Runtime when installed, otherwise the conductor built into the app) instead of connecting to a conductor you run.
"""
import io, json, os, re, shutil, subprocess, sys, tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
CONTENT = os.path.join(HERE, 'content')
SHARED = os.path.join(HERE, 'shared')
VERSION = json.load(io.open(os.path.join(HERE, '..', 'packages.json'), encoding='utf-8'))['version']


def rd(p):
    return io.open(p, encoding='utf-8-sig').read()


def wr(p, s):
    os.makedirs(os.path.dirname(p), exist_ok=True)
    io.open(p, 'w', encoding='utf-8', newline='\n').write(s)


def sub(path, old, new, count=1):
    s = rd(path)
    if old not in s:
        raise SystemExit(f'patch anchor not found in {path}:\n{old}')
    wr(path, s.replace(old, new, count))


def sub_re(path, pattern, repl, flags=re.S):
    s = rd(path)
    n, k = re.subn(pattern, repl, s, count=1, flags=flags)
    if not k:
        raise SystemExit(f'patch pattern not found in {path}:\n{pattern}')
    wr(path, n)


# App types that run on Android and can use the Holochain runtime on the phone (templates/shared/HoloNETAndroidRuntime.cs).
ANDROID_RUNTIME = {'android', 'maui', 'maui-blazor', 'uno'}
# ... and iOS (the iOS bridge is built by CI but not yet proven on a physical iPhone)
PHONE_RUNTIME = ANDROID_RUNTIME | {'ios'}
# App types that run on iOS: the iOS conductor runs on the simulator only so far, so they still connect to a conductor you run.
IOS_TYPES = {'ios', 'maui', 'maui-blazor', 'uno'}

DEMO_CALL = 'await HoloNETDemos.HoloNETDemo.RunAsync()'
RAZOR_PAGE = '''@page "/holonet"
%s
<PageTitle>HoloNET</PageTitle>

<h1>HoloNET</h1>
<p>This page runs the HoloNET demo in HoloNETDemo.cs (HoloNET HOLONET_LIBRARY_NAME).</p>

<button class="btn btn-primary" @onclick="Run" disabled="@busy">@(busy ? "Running..." : "Run the HoloNET demo")</button>
<pre class="mt-3">@output</pre>

@code {
    private bool busy;
    private string output = "";

    private async Task Run()
    {
        busy = true;
        output = "";
        output = await HoloNETDemos.HoloNETDemo.RunAsync();
        busy = false;
    }
}
'''
NAV_ITEM = '''<div class="nav-item px-3">
            <NavLink class="nav-link" href="holonet">
                <span class="bi bi-lightning-nav-menu" aria-hidden="true"></span> HoloNET
            </NavLink>
        </div>
        '''


def add_nav(path):
    s = rd(path)
    m = re.search(r'<div class="nav-item px-3">\s*<NavLink class="nav-link" href="weather">', s)
    if not m:
        raise SystemExit('nav anchor not found in ' + path)
    wr(path, s[:m.start()] + NAV_ITEM + s[m.start():])


# ---- per app type patches: each receives the generated stock project folder ----------------------------------------------------

def p_console(d):
    wr(f'{d}/Program.cs', '''// Runs the HoloNET demo (HoloNETDemo.cs) and prints what happened.
Console.WriteLine(await HoloNETDemos.HoloNETDemo.RunAsync());
''')


def p_worker(d):
    wr(f'{d}/Worker.cs', '''namespace HoloNETApp;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Runs the HoloNET demo (HoloNETDemo.cs) once when the service starts, then idles. Replace it with your own background work.
        logger.LogInformation("HoloNET demo:\\n{result}", await HoloNETDemos.HoloNETDemo.RunAsync());
        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }
}
''')


def p_webapi(d):
    sub(f'{d}/Program.cs', 'app.Run();', '''// GET /holonet runs the HoloNET demo (HoloNETDemo.cs) and returns what happened.
app.MapGet("/holonet", async () => Results.Text(await HoloNETDemos.HoloNETDemo.RunAsync()))
   .WithName("HoloNETDemo");

app.Run();''')


def p_mvc(d):
    wr(f'{d}/Controllers/HoloNETController.cs', '''using Microsoft.AspNetCore.Mvc;

namespace HoloNETApp.Controllers;

public class HoloNETController : Controller
{
    // Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened.
    public async Task<IActionResult> Index() => View((object)await HoloNETDemos.HoloNETDemo.RunAsync());
}
''')
    wr(f'{d}/Views/HoloNET/Index.cshtml', '''@model string
@{
    ViewData["Title"] = "HoloNET";
}
<h1>HoloNET</h1>
<p>The result of the HoloNET demo in HoloNETDemo.cs (HoloNET HOLONET_LIBRARY_NAME):</p>
<pre>@Model</pre>
''')
    s = rd(f'{d}/Views/Shared/_Layout.cshtml')
    m = re.search(r'<li class="nav-item">\s*<a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Privacy">', s)
    if not m:
        raise SystemExit('mvc nav anchor not found')
    wr(f'{d}/Views/Shared/_Layout.cshtml', s[:m.start()] + '''<li class="nav-item">
                            <a class="nav-link text-dark" asp-area="" asp-controller="HoloNET" asp-action="Index">HoloNET</a>
                        </li>
                        ''' + s[m.start():])


def p_razor(d):
    wr(f'{d}/Pages/HoloNET.cshtml', '''@page
@model HoloNETApp.Pages.HoloNETModel
@{
    ViewData["Title"] = "HoloNET";
}
<h1>HoloNET</h1>
<p>The result of the HoloNET demo in HoloNETDemo.cs (HoloNET HOLONET_LIBRARY_NAME):</p>
<pre>@Model.Result</pre>
''')
    wr(f'{d}/Pages/HoloNET.cshtml.cs', '''using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoloNETApp.Pages;

public class HoloNETModel : PageModel
{
    public string Result { get; private set; } = "";

    // Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened.
    public async Task OnGetAsync() => Result = await HoloNETDemos.HoloNETDemo.RunAsync();
}
''')
    s = rd(f'{d}/Pages/Shared/_Layout.cshtml')
    m = re.search(r'<li class="nav-item">\s*<a class="nav-link text-dark" asp-area="" asp-page="/Privacy">', s)
    if not m:
        raise SystemExit('razor nav anchor not found')
    wr(f'{d}/Pages/Shared/_Layout.cshtml', s[:m.start()] + '''<li class="nav-item">
                            <a class="nav-link text-dark" asp-area="" asp-page="/HoloNET">HoloNET</a>
                        </li>
                        ''' + s[m.start():])


def p_blazor(d):
    wr(f'{d}/Components/Pages/HoloNET.razor', RAZOR_PAGE % '@rendermode InteractiveServer')
    add_nav(f'{d}/Components/Layout/NavMenu.razor')


def p_blazorwasm(d):
    wr(f'{d}/Pages/HoloNET.razor', RAZOR_PAGE % '')
    add_nav(f'{d}/Layout/NavMenu.razor')


def p_wpf(d):
    sub_re(f'{d}/MainWindow.xaml', r'<Grid>.*</Grid>', '''<DockPanel Margin="16">
        <!-- Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened. -->
        <Button x:Name="RunButton" DockPanel.Dock="Top" Content="Run the HoloNET demo" Padding="12,6" HorizontalAlignment="Left" Click="RunButton_Click" />
        <TextBox x:Name="Output" Margin="0,12,0,0" IsReadOnly="True" TextWrapping="Wrap" VerticalScrollBarVisibility="Auto" FontFamily="Consolas" />
    </DockPanel>''')
    sub(f'{d}/MainWindow.xaml', 'Title="MainWindow"', 'Title="HoloNET"')
    wr(f'{d}/MainWindow.xaml.cs', '''using System.Windows;

namespace HoloNETApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        RunButton.IsEnabled = false;
        Output.Text = "Running...";
        Output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
        RunButton.IsEnabled = true;
    }
}
''')


def p_winforms(d):
    wr(f'{d}/Form1.cs', '''namespace HoloNETApp;

public partial class Form1 : Form
{
    private readonly Button runButton = new() { Text = "Run the HoloNET demo", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(8) };
    private readonly TextBox output = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };

    public Form1()
    {
        InitializeComponent();
        Text = "HoloNET";
        Controls.Add(output);
        Controls.Add(runButton);
        runButton.Click += async (s, e) =>
        {
            // Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened.
            runButton.Enabled = false;
            output.Text = "Running...";
            output.Text = (await HoloNETDemos.HoloNETDemo.RunAsync()).Replace("\\n", "\\r\\n");
            runButton.Enabled = true;
        };
    }
}
''')


def p_maui(d):
    wr(f'{d}/MainPage.xaml', '''<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="HoloNETApp.MainPage">

    <ScrollView>
        <VerticalStackLayout Padding="24" Spacing="16">
            <Label Text="HoloNET" Style="{StaticResource Headline}" SemanticProperties.HeadingLevel="Level1" />
            <Label Text="Runs the HoloNET demo in HoloNETDemo.cs (HoloNET HOLONET_LIBRARY_NAME)." />
            <Button x:Name="RunBtn" Text="Run the HoloNET demo" Clicked="OnRunClicked" HorizontalOptions="Fill" />
            <Label x:Name="OutputLabel" FontFamily="Consolas" />
        </VerticalStackLayout>
    </ScrollView>

</ContentPage>
''')
    wr(f'{d}/MainPage.xaml.cs', '''namespace HoloNETApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnRunClicked(object? sender, EventArgs e)
    {
        RunBtn.IsEnabled = false;
        OutputLabel.Text = "Running...";
        OutputLabel.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
        RunBtn.IsEnabled = true;
    }
}
''')


def p_mauiblazor(d):
    wr(f'{d}/Components/Pages/HoloNET.razor', RAZOR_PAGE % '')
    add_nav(f'{d}/Components/Layout/NavMenu.razor')


def p_android(d):
    wr(f'{d}/MainActivity.cs', '''using Android.Widget;

namespace HoloNETApp;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // A button that runs the HoloNET demo (HoloNETDemo.cs) and a text view for what happened.
        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        layout.SetPadding(32, 64, 32, 32);
        var button = new Button(this) { Text = "Run the HoloNET demo" };
        var output = new TextView(this);
        layout.AddView(button);
        layout.AddView(output);
        SetContentView(layout);

        button.Click += async (s, e) =>
        {
            button.Enabled = false;
            output.Text = "Running...";
            output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
            button.Enabled = true;
        };
    }
}
''')


def p_avalonia(d):
    sub_re(f'{d}/MainWindow.axaml', r'Title="HoloNETApp">.*</Window>', '''Title="HoloNET">
    <DockPanel Margin="16">
        <!-- Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened. -->
        <Button x:Name="RunButton" DockPanel.Dock="Top" Content="Run the HoloNET demo" Click="RunButton_Click" />
        <TextBox x:Name="Output" Margin="0,12,0,0" IsReadOnly="True" TextWrapping="Wrap" FontFamily="Consolas,Menlo,monospace" />
    </DockPanel>
</Window>''')
    wr(f'{d}/MainWindow.axaml.cs', '''using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HoloNETApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RunButton_Click(object? sender, RoutedEventArgs e)
    {
        RunButton.IsEnabled = false;
        Output.Text = "Running...";
        Output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
        RunButton.IsEnabled = true;
    }
}
''')


def p_uno(d):
    sub_re(f'{d}/MainPage.xaml', r'<ScrollViewer IsTabStop="True">.*</ScrollViewer>', '''<ScrollViewer IsTabStop="True">
    <StackPanel Padding="24" Spacing="16">
      <TextBlock Text="HoloNET" FontSize="28" />
      <!-- Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened. -->
      <Button x:Name="RunButton" Content="Run the HoloNET demo" Click="RunButton_Click" />
      <TextBlock x:Name="Output" FontFamily="Consolas" TextWrapping="Wrap" IsTextSelectionEnabled="True" />
    </StackPanel>
  </ScrollViewer>''')
    wr(f'{d}/MainPage.xaml.cs', '''namespace HoloNETApp;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        RunButton.IsEnabled = false;
        Output.Text = "Running...";
        Output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
        RunButton.IsEnabled = true;
    }
}
''')
    # Uno uses central package management: the version lives in Directory.Packages.props
    sub(os.path.join(os.path.dirname(d), 'Directory.Packages.props'), '<ItemGroup>\n  </ItemGroup>', '''<ItemGroup>
    <PackageVersion Include="NextGenSoftware.Holochain.HoloNET.HOLONET_LIBRARY_IDHOLONET_PKG_SUFFIX" Version="HOLONET_VERSION" />
    <!--#if (!UseApi)-->
    <PackageVersion Include="NextGenSoftware.Holochain.HoloNET.APIHOLONET_PKG_SUFFIX" Version="HOLONET_VERSION" />
    <!--#endif-->
  </ItemGroup>''')


def base_winui(d):
    wr(f'{d}/HoloNETApp.csproj', '''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <RootNamespace>HoloNETApp</RootNamespace>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <Platforms>x86;x64;ARM64</Platforms>
    <RuntimeIdentifiers>win-x86;win-x64;win-arm64</RuntimeIdentifiers>
    <RuntimeIdentifier Condition="'$(RuntimeIdentifier)' == ''">win-x64</RuntimeIdentifier>
    <UseWinUI>true</UseWinUI>
    <WindowsPackageType>None</WindowsPackageType>
    <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="2.6.4" />
    <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.28000.2957" />
  </ItemGroup>
</Project>
''')
    wr(f'{d}/app.manifest', '''<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="HoloNETApp.app" />
  <application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
      <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
    </windowsSettings>
  </application>
</assembly>
''')
    wr(f'{d}/App.xaml', '''<Application x:Class="HoloNETApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
''')
    wr(f'{d}/App.xaml.cs', '''using Microsoft.UI.Xaml;

namespace HoloNETApp;

public partial class App : Application
{
    private Window? window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = new MainWindow();
        window.Activate();
    }
}
''')
    wr(f'{d}/MainWindow.xaml', '''<Window x:Class="HoloNETApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="HoloNET">
    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <!-- Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened. -->
        <Button x:Name="RunButton" Content="Run the HoloNET demo" Click="RunButton_Click" />
        <ScrollViewer Grid.Row="1">
            <TextBlock x:Name="Output" FontFamily="Consolas" TextWrapping="Wrap" IsTextSelectionEnabled="True" />
        </ScrollViewer>
    </Grid>
</Window>
''')
    wr(f'{d}/MainWindow.xaml.cs', '''using Microsoft.UI.Xaml;

namespace HoloNETApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        RunButton.IsEnabled = false;
        Output.Text = "Running...";
        Output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
        RunButton.IsEnabled = true;
    }
}
''')


def p_winui(d):
    pass


# What NuGetForUnity must restore for the HoloNET libraries on .NET Standard 2.1 (the dependency closure, minus what Unity provides itself).
UNITY_DEPENDENCIES = [
    ('BetterConsoleTables', '1.1.3'), ('Colorful.Console', '1.2.15'), ('ConsoleTables', '2.6.2'), ('MessagePack', '2.5.303'), ('MessagePack.Annotations', '2.5.303'),
    ('Microsoft.Bcl.AsyncInterfaces', '8.0.0'), ('Microsoft.Extensions.Logging.Abstractions', '1.0.0'), ('Microsoft.NET.StringTools', '17.6.3'), ('NBitcoin', '9.0.3'),
    ('NextGenSoftware.CLI.Engine', '2.0.4'), ('NextGenSoftware.ErrorHandling', '2.0.4'), ('NextGenSoftware.Logging', '2.0.4'), ('NextGenSoftware.Utilities', '2.0.4'),
    ('NextGenSoftware.WebSocket', '2.0.4'), ('Nito.AsyncEx.Context', '5.1.2'), ('Nito.AsyncEx.Tasks', '5.1.2'), ('Nito.Disposables', '2.2.1'),
    ('Portable.BouncyCastle', '1.9.0'), ('ShellProgressBar', '5.2.0'), ('SshNet.Security.Cryptography', '1.3.0'), ('System.Buffers', '4.5.1'),
    ('System.Collections.Immutable', '6.0.0'), ('System.Memory', '4.5.5'), ('System.Numerics.Vectors', '4.4.0'), ('System.Runtime.CompilerServices.Unsafe', '6.0.0'),
    ('System.Text.Encodings.Web', '8.0.0'), ('System.Text.Json', '8.0.6'), ('System.Threading.Tasks.Extensions', '4.5.4'), ('Wcwidth', '1.0.0'), ('YamlDotNet', '13.7.1'),
]


def base_unity(d):
    # Not a dotnet project: a Unity "Assets" starter. NuGetForUnity installs the packages listed in packages.config.
    wr(f'{d}/Assets/packages.config', '''<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="NextGenSoftware.Holochain.HoloNET.HOLONET_LIBRARY_IDHOLONET_PKG_SUFFIX" version="HOLONET_VERSION" manuallyInstalled="true" />
  <!--#if (!UseApi)-->
  <package id="NextGenSoftware.Holochain.HoloNET.APIHOLONET_PKG_SUFFIX" version="HOLONET_VERSION" manuallyInstalled="true" />
  <!--#endif-->
  <!--#if (!UseClient)-->
  <package id="NextGenSoftware.Holochain.HoloNET.ClientHOLONET_PKG_SUFFIX" version="HOLONET_VERSION" />
  <!--#endif-->
  <!-- NuGetForUnity restores only what is listed here, so every dependency is listed (Newtonsoft.Json comes from Unity's own package). -->
''' + ''.join(f'  <package id="{i}" version="{v}" />\n' for i, v in UNITY_DEPENDENCIES) + '''</packages>
''')
    wr(f'{d}/Packages/manifest.json', '''{
  "dependencies": {
    "com.github-glitchenzo.nugetforunity": "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity"
  }
}
''')
    wr(f'{d}/Assets/HoloNET/HoloNETBehaviour.cs', '''using UnityEngine;

// Add this component to any GameObject. It shows a button that runs the HoloNET demo (HoloNETDemo.cs) and the text it returns.
public class HoloNETBehaviour : MonoBehaviour
{
    private string output = "";
    private bool busy;

    private async void Run()
    {
        busy = true;
        output = "Running...";
        // NuGetForUnity keeps the HoloNET API package's oasis.happ under Assets/Packages: use it.
        var happ = System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.dataPath, "Packages"), "oasis.happ", System.IO.SearchOption.AllDirectories);
        if (happ.Length > 0)
            HoloNETDemos.HoloNETDemo.HappPath = happ[0];
        output = await HoloNETDemos.HoloNETDemo.RunAsync();
        busy = false;
    }

    private void OnGUI()
    {
        GUI.enabled = !busy;
        if (GUI.Button(new Rect(20, 20, 260, 40), "Run the HoloNET demo"))
            Run();
        GUI.enabled = true;
        GUI.Label(new Rect(20, 70, Screen.width - 40, Screen.height - 90), output);
    }
}
''')


def p_unity(d):
    pass


def p_ios(d):
    sub_re(f'{d}/SceneDelegate.cs', r"// Create a 'UIViewController' with a single 'UILabel'.*?Window.RootViewController = vc;", '''// A button that runs the HoloNET demo (HoloNETDemo.cs) and a label for what happened.
			var vc = new UIViewController ();
			vc.View!.BackgroundColor = UIColor.SystemBackground;
			var button = UIButton.FromType (UIButtonType.System);
			button.SetTitle ("Run the HoloNET demo", UIControlState.Normal);
			button.Frame = new CoreGraphics.CGRect (20, 80, Window!.Frame.Width - 40, 44);
			var output = new UILabel (new CoreGraphics.CGRect (20, 140, Window.Frame.Width - 40, Window.Frame.Height - 180)) { Lines = 0, Font = UIFont.GetMonospacedSystemFont (13, UIFontWeight.Regular) };
			button.TouchUpInside += async (s, e) => {
				button.Enabled = false;
				output.Text = "Running...";
				output.Text = await HoloNETDemos.HoloNETDemo.RunAsync ();
				button.Enabled = true;
			};
			vc.View.AddSubview (button);
			vc.View.AddSubview (output);

			Window.RootViewController = vc;''')


def p_grpc(d):
    sub(f'{d}/Program.cs', 'app.Run();', '''// GET /holonet runs the HoloNET demo (HoloNETDemo.cs) and returns what happened. Your gRPC services (Services/) can call HoloNETDemo the same way.
app.MapGet("/holonet", async () => Results.Text(await HoloNETDemos.HoloNETDemo.RunAsync()));

app.Run();''')


def p_classlib(d):
    if os.path.exists(f'{d}/Class1.cs'):
        os.remove(f'{d}/Class1.cs')
    wr(f'{d}/HoloNETService.cs', '''namespace HoloNETApp;

/// <summary>Reference this library from any app to use HoloNET. Replace the demo call with your own HoloNET code (see HoloNETDemo.cs).</summary>
public static class HoloNETService
{
    /// <summary>Runs the HoloNET demo (HoloNETDemo.cs) and returns what happened.</summary>
    public static Task<string> RunDemoAsync() => HoloNETDemos.HoloNETDemo.RunAsync();
}
''')


def p_xunit(d):
    if os.path.exists(f'{d}/UnitTest1.cs'):
        os.remove(f'{d}/UnitTest1.cs')
    wr(f'{d}/HoloNETTests.cs', '''namespace HoloNETApp;

public class HoloNETTests
{
    // An integration test: it starts (or connects to) a conductor, so it needs Holochain available (see README.md).
    // Replace the demo with the HoloNET calls your own code makes.
    [Fact]
    public async Task HoloNETDemoRuns()
    {
        var result = await HoloNETDemos.HoloNETDemo.RunAsync();
        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
''')


# key, dotnet new args, patch fn, display name, classifications, forced external (no bundled conductor), project file TFM fix
TYPES = [
    ('console', ['console'], p_console, 'Console app', ['Console'], False),
    ('worker', ['worker'], p_worker, 'Worker Service (background service)', ['Service', 'Worker'], False),
    ('webapi', ['webapi'], p_webapi, 'ASP.NET Core Web API', ['Web', 'API'], False),
    ('grpc', ['grpc'], p_grpc, 'ASP.NET Core gRPC service', ['Web', 'gRPC', 'Service'], False),
    ('mvc', ['mvc'], p_mvc, 'ASP.NET Core MVC web app', ['Web', 'MVC'], False),
    ('razor', ['razor'], p_razor, 'ASP.NET Core Razor Pages web app', ['Web', 'Razor'], False),
    ('blazor', ['blazor'], p_blazor, 'Blazor Web App (server)', ['Web', 'Blazor'], False),
    ('blazorwasm', ['blazorwasm'], p_blazorwasm, 'Blazor WebAssembly app', ['Web', 'Blazor', 'WebAssembly'], True),
    ('classlib', ['classlib'], p_classlib, 'Class library', ['Library'], False),
    ('xunit', ['xunit'], p_xunit, 'xUnit test project', ['Test', 'xUnit'], False),
    ('wpf', ['wpf'], p_wpf, 'WPF app', ['Desktop', 'WPF'], False),
    ('winforms', ['winforms'], p_winforms, 'Windows Forms app', ['Desktop', 'WinForms'], False),
    ('maui', ['maui'], p_maui, '.NET MAUI app', ['MAUI', 'Mobile', 'Desktop'], True),
    ('maui-blazor', ['maui-blazor'], p_mauiblazor, '.NET MAUI Blazor Hybrid app', ['MAUI', 'Blazor', 'Mobile'], True),
    ('android', ['android'], p_android, 'Android app', ['Android', 'Mobile'], True),
    ('winui', base_winui, p_winui, 'WinUI 3 app', ['Desktop', 'WinUI'], False),
    ('unity', base_unity, p_unity, 'Unity project starter (Assets folder)', ['Unity', 'Game'], False, 'Assets/HoloNET', False, True),
    ('ios', ['ios'], p_ios, 'iOS app', ['iOS', 'Mobile'], True),
    ('avalonia', ['avalonia.app'], p_avalonia, 'Avalonia app', ['Desktop', 'Avalonia'], False),
    ('uno', ['unoapp'], p_uno, 'Uno Platform app', ['Uno', 'Mobile', 'Desktop', 'WebAssembly'], True, 'HoloNETApp', True),
]


def package_block(forced_external, cpm=False, android_runtime=False, ios_runtime=False, minimal_deps=False):
    """The HoloNET package references for the generated csproj. HOLONET_* tokens are replaced by the template engine."""
    ver = '' if cpm else ' Version="HOLONET_VERSION"'
    runtime = '''  <ItemGroup Condition="'$(TargetPlatformIdentifier)' == 'android'">
    <!-- The Holochain runtime for Android: put the bridge and runtime .aar files in runtime/android (see README.md). The hApp is packed as an Android asset
         because an Android app cannot read files next to it. -->
    <AndroidAarLibrary Include="runtime/android/*.aar" />
    <AndroidJavaLibrary Include="runtime/android/*.jar" />
    <AndroidAsset Include="$(NuGetPackageRoot)nextgensoftware.holochain.holonet.api/HOLONET_VERSION/content/oasis.happ" Link="oasis.happ"
                  Condition="Exists('$(NuGetPackageRoot)nextgensoftware.holochain.holonet.api/HOLONET_VERSION/content/oasis.happ')" />
  </ItemGroup>

  <!-- First build for Android: download the Holochain runtime libraries (about 38 MB) from the android-service-runtime release into runtime/android.
       Set HoloNETDownloadRuntime=false to skip it and put your own libraries there. -->
  <PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
    <!-- The Holochain Android runtime libraries need Android 8.1 (API 27) or newer. -->
    <SupportedOSPlatformVersion>27.0</SupportedOSPlatformVersion>
  </PropertyGroup>
  <PropertyGroup>
    <HoloNETRuntimeDeps Condition="'$(HoloNETRuntimeDeps)' == ''">MINIMAL_DEPS</HoloNETRuntimeDeps>
    <HoloNETRuntimeUrl Condition="'$(HoloNETRuntimeUrl)' == ''">https://github.com/NextGenSoftwareUK/android-service-runtime/releases/download/oasis-holochain-0.7-v1.1.0</HoloNETRuntimeUrl>
  </PropertyGroup>
  <Target Name="HoloNETFetchAndroidRuntime" BeforeTargets="ResolveAssemblyReferences;_ResolveAars;_GenerateJavaDesignerForComponent" Condition="'$(TargetPlatformIdentifier)' == 'android' And '$(HoloNETDownloadRuntime)' != 'false' And !Exists('$(MSBuildProjectDirectory)/runtime/android/holochain-service.aar')">
    <DownloadFile SourceUrl="$(HoloNETRuntimeUrl)/holochain-client.aar" DestinationFolder="$(MSBuildProjectDirectory)/runtime/android" SkipUnchangedFiles="true" />
    <DownloadFile SourceUrl="$(HoloNETRuntimeUrl)/holochain-service.aar" DestinationFolder="$(MSBuildProjectDirectory)/runtime/android" SkipUnchangedFiles="true" />
    <DownloadFile SourceUrl="$(HoloNETRuntimeUrl)/holonet-android-bridge.aar" DestinationFolder="$(MSBuildProjectDirectory)/runtime/android" SkipUnchangedFiles="true" />
    <DownloadFile SourceUrl="$(HoloNETRuntimeUrl)/holochain-android-dependencies.zip" DestinationFolder="$(MSBuildProjectDirectory)/runtime/android/dependencies-zip" SkipUnchangedFiles="true" />
    <Unzip SourceFiles="$(MSBuildProjectDirectory)/runtime/android/dependencies-zip/holochain-android-dependencies.zip" DestinationFolder="$(MSBuildProjectDirectory)/runtime/android" OverwriteReadOnlyFiles="true" />
    <!-- MAUI and Uno bring their own AndroidX, Material and Kotlin libraries: keep only the Holochain libraries and JNA, or the duplicates fail the build. -->
    <ItemGroup Condition="'$(HoloNETRuntimeDeps)' == 'minimal'">
      <HoloNETDropLib Include="$(MSBuildProjectDirectory)/runtime/android/*.aar;$(MSBuildProjectDirectory)/runtime/android/*.jar"
                      Exclude="$(MSBuildProjectDirectory)/runtime/android/holochain-*.aar;$(MSBuildProjectDirectory)/runtime/android/holonet-*.aar;$(MSBuildProjectDirectory)/runtime/android/jna-*.aar" />
    </ItemGroup>
    <Delete Files="@(HoloNETDropLib)" />
    <ItemGroup>
      <AndroidAarLibrary Include="$(MSBuildProjectDirectory)/runtime/android/*.aar" />
      <AndroidJavaLibrary Include="$(MSBuildProjectDirectory)/runtime/android/*.jar" />
    </ItemGroup>
  </Target>

''' if android_runtime else ''
    ios = '''  <PropertyGroup Condition="'$(TargetPlatformIdentifier)' == 'ios'">
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup Condition="'$(TargetPlatformIdentifier)' == 'ios'">
    <!-- The conductor for iOS: put HoloNETIosBridge.xcframework in runtime/ios (see runtime/ios/README.md). It is linked into the app. -->
    <NativeReference Include="runtime/ios/HoloNETIosBridge.xcframework" Kind="Static" ForceLoad="true" Condition="Exists('runtime/ios/HoloNETIosBridge.xcframework')"
                     Frameworks="Security SystemConfiguration Network CoreFoundation" LinkerFlags="-lresolv" />
    <BundleResource Include="$(NuGetPackageRoot)nextgensoftware.holochain.holonet.api/HOLONET_VERSION/content/oasis.happ" Link="oasis.happ"
                    Condition="Exists('$(NuGetPackageRoot)nextgensoftware.holochain.holonet.api/HOLONET_VERSION/content/oasis.happ')" />
  </ItemGroup>

''' if ios_runtime else ''
    return '''
  <ItemGroup>
    <!-- HoloNET: the library chosen with the HoloNET option (api, client, orm or hypernet). HOLONET_PKG_SUFFIX is .Embedded* when the Holochain conductor
         is bundled in the app (the Conductor option, Windows only) and empty when you run your own conductor. -->
    <PackageReference Include="NextGenSoftware.Holochain.HoloNET.HOLONET_LIBRARY_IDHOLONET_PKG_SUFFIX"%(ver)s />
    <!--#if (!UseApi)-->
    <!-- Only for the OASIS hApp (oasis.happ) the demo installs: HoloNET API copies it next to your app. Use your own .happ and drop this. -->
    <PackageReference Include="NextGenSoftware.Holochain.HoloNET.APIHOLONET_PKG_SUFFIX"%(ver)s />
    <!--#endif-->
  </ItemGroup>

''' % {'ver': ver} + runtime.replace('MINIMAL_DEPS', 'minimal' if minimal_deps else 'full') + ios


def template_json(key, name, classifications, forced_external):
    sym = {
        'HoloNET': {
            'type': 'parameter', 'datatype': 'choice', 'defaultValue': 'api',
            'description': 'Which HoloNET library the app uses.',
            'choices': [
                {'choice': 'api', 'description': 'HoloNET API: one line to save, load, delete, share and message (simplest).'},
                {'choice': 'client', 'description': 'HoloNET Client: full control of the conductor, zome calls and signals.'},
                {'choice': 'orm', 'description': 'HoloNET ORM: save and load your own C# classes (includes the Client).'},
                {'choice': 'hypernet', 'description': 'HoloNET HyperNET: real-time player-to-player messaging (includes the Client).'},
            ]},
        'HolochainVersion': {
            'type': 'parameter', 'datatype': 'choice', 'defaultValue': 'both',
            'description': 'With a bundled conductor: which Holochain version(s) to bundle.',
            'choices': [
                {'choice': 'both', 'description': 'Holochain 0.7.1 and 0.7.0 (pick at run time with HoloNETDNA.EmbeddedHolochainVersion).'},
                {'choice': '0.7.1', 'description': '0.7.1 only (smaller).'},
                {'choice': '0.7.0', 'description': '0.7.0 only (smaller).'}]},
        'HoloNETVersion': {'type': 'parameter', 'datatype': 'text', 'defaultValue': VERSION, 'description': 'The HoloNET package version.', 'replaces': 'HOLONET_VERSION'},
        'OS': {'type': 'bind', 'binding': 'env:OS'},
        'UseApi': {'type': 'computed', 'value': '(HoloNET == "api")'},
        'UseClient': {'type': 'computed', 'value': '(HoloNET == "client")'},
        'UseOrm': {'type': 'computed', 'value': '(HoloNET == "orm")'},
        'UseHyperNet': {'type': 'computed', 'value': '(HoloNET == "hypernet")'},
        'LibraryId': {'type': 'generated', 'generator': 'switch', 'replaces': 'HOLONET_LIBRARY_ID', 'parameters': {'evaluator': 'C++', 'datatype': 'string', 'cases': [
            {'condition': '(UseApi)', 'value': 'API'}, {'condition': '(UseClient)', 'value': 'Client'},
            {'condition': '(UseOrm)', 'value': 'ORM'}, {'condition': '(UseHyperNet)', 'value': 'HyperNET'}]}},
        'LibraryName': {'type': 'generated', 'generator': 'switch', 'replaces': 'HOLONET_LIBRARY_NAME', 'parameters': {'evaluator': 'C++', 'datatype': 'string', 'cases': [
            {'condition': '(UseApi)', 'value': 'API'}, {'condition': '(UseClient)', 'value': 'Client'},
            {'condition': '(UseOrm)', 'value': 'ORM'}, {'condition': '(UseHyperNet)', 'value': 'HyperNET'}]}},
        'PackageSuffix': {'type': 'generated', 'generator': 'switch', 'replaces': 'HOLONET_PKG_SUFFIX', 'parameters': {'evaluator': 'C++', 'datatype': 'string', 'cases': [
            {'condition': '(UseEmbedded && HolochainVersion == "0.7.1")', 'value': '.Embedded.Holochain071'},
            {'condition': '(UseEmbedded && HolochainVersion == "0.7.0")', 'value': '.Embedded.Holochain070'},
            {'condition': '(UseEmbedded)', 'value': '.Embedded'},
            {'condition': '(!UseEmbedded)', 'value': ''}]}},
    }
    sym['PhoneRuntime'] = {'type': 'computed', 'value': 'true' if key in PHONE_RUNTIME else 'false'}
    if forced_external:
        sym['UseEmbedded'] = {'type': 'computed', 'value': 'false'}
        # (no Conductor / HolochainVersion choice: this app type cannot host a conductor, so it connects to one you run)
        del sym['HolochainVersion']
    else:
        sym['Conductor'] = {
            'type': 'parameter', 'datatype': 'choice', 'defaultValue': 'auto',
            'description': 'Where the Holochain conductor comes from.',
            'choices': [
                {'choice': 'auto', 'description': 'Bundled in the app on Windows; elsewhere HoloNET starts the Holochain on this computer, downloading the official one the first time.'},
                {'choice': 'embedded', 'description': 'Bundle the conductor in the app (Windows only).'},
                {'choice': 'external', 'description': 'Use a conductor you run yourself on ws://localhost:7777 (start-conductor script included).'}]}
        sym['UseEmbedded'] = {'type': 'computed', 'value': '(Conductor == "embedded" || (Conductor == "auto" && OS == "Windows_NT"))'}
        sym['YouStartConductor'] = {'type': 'computed', 'value': '(Conductor == "external")'}
    # the symbol switch conditions reference UseEmbedded and HolochainVersion: forced-external apps have neither a choice nor a version
    if forced_external:
        for case in sym['PackageSuffix']['parameters']['cases']:
            case['condition'] = case['condition'].replace(' && HolochainVersion == "0.7.1"', ' && false').replace(' && HolochainVersion == "0.7.0"', ' && false')
    sources = [{'exclude': ['**/bin/**', '**/obj/**', '**/.vs/**', '**/*.user', '**/.sandbox/**'], 'modifiers': []}]
    if not forced_external:
        sources[0]['modifiers'].append({'condition': '(UseEmbedded)', 'exclude': ['start-conductor.sh', 'start-conductor.ps1']})
    return {
        '$schema': 'http://json.schemastore.org/template',
        'author': 'NextGen Software Ltd',
        'classifications': ['HoloNET', 'Holochain'] + classifications,
        'identity': 'NextGenSoftware.Holochain.HoloNET.Templates.' + key.title().replace('-', ''),
        'name': 'HoloNET ' + name,
        'description': f'A {name} with HoloNET already wired in: choose the HoloNET library (API, Client, ORM or HyperNET) and whether to bundle the Holochain conductor.',
        'shortName': 'holonet-' + key,
        'tags': {'language': 'C#', 'type': 'project'},
        'sourceName': 'HoloNETApp',
        'preferNameDirectory': True,
        'symbols': sym,
        'sources': sources,
        'postActions': [{'description': 'Restore the NuGet packages', 'actionId': '210D431B-A78B-4D2F-B762-4ED3E3EA9025', 'continueOnError': True}],
    }


def unity_readme():
    return r'''# HoloNETApp (Unity)

A Unity starter with [HoloNET](https://holonet.oasisomniverse.one) wired in (Unity 2021.3 or newer, which supports .NET Standard 2.1 and async/await).

This is an **Assets-only starter**, not a complete Unity project: create a new Unity project, then copy the `Assets` and `Packages` folders from here into it.
It is tested in the Unity 2022.3 editor: the packages restore through NuGetForUnity, the scripts compile, and the demo runs against the bundled conductor.

## Set up

1. Copy `Assets/` and `Packages/manifest.json` (merge the `dependencies` line if you already have a manifest) into your Unity project.
2. Let Unity import [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). In **NuGet > Restore Packages** it installs the HoloNET packages listed in `Assets/packages.config`.
3. Add the `HoloNETBehaviour` component (Assets/HoloNET) to any GameObject and press Play.

## The conductor

On Windows the default (`--Conductor embedded`) bundles the Holochain conductor: HoloNET starts and stops it for you, in the editor and in standalone builds.
On other platforms create the starter with `--Conductor external`, and run a conductor first (`./start-conductor.sh` or `.\start-conductor.ps1`; ws://localhost:7777).
`HoloNETBehaviour` finds `oasis.happ` (the HoloNET API package puts it under `Assets/Packages`); to use your own hApp set `HoloNETDemo.HappPath`.

## Learn more

- [Documentation](https://docs.holonet.oasisomniverse.one) and [tutorials](https://learn.holonet.oasisomniverse.one)
- [All HoloNET packages and templates](https://docs.holonet.oasisomniverse.one/packages)
'''


RUNTIME_LINKS = '''- Source: [android-service-runtime](https://github.com/NextGenSoftwareUK/android-service-runtime) (a fork of [holochain/android-service-runtime](https://github.com/holochain/android-service-runtime); our Holochain 0.7 work is on the [`oasis/holochain-0.7`](https://github.com/NextGenSoftwareUK/android-service-runtime/tree/oasis/holochain-0.7) branch)
- Holochain 0.7 for OASIS: Android + iOS, one release with everything ([oasis-holochain-0.7-v1.1.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0)): the runtime apps (APKs), the Android libraries the templates download, the iOS conductor and the iOS bridge, with checksums and notes on what is proven
- OASIS Edge runtime (what Our World uses), by product: [Edge Runtime only](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Runtime-v1.2.0), [OGEngineClient + Edge](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-Edge-v2.1.0), [OGEngineClient + Edge + Unity](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OASIS-Edge-Unity-v1.1.0), [OGEngineClient RemoteOnly](https://github.com/NextGenSoftwareUK/OASIS/releases/tag/OGEngineClient-RemoteOnly-v2.1.0), all in the [OASIS releases](https://github.com/NextGenSoftwareUK/OASIS/releases)
- The bridge this template calls: [android-runtime](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/android-runtime) in the HoloNET repo'''


def android_runtime_folder_readme():
    return '''# Holochain runtime libraries for Android

The first time you build for Android, the project downloads the Holochain runtime libraries from the
[oasis-holochain-0.7-v1.1.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0) release (about 38 MB) into this folder, and includes every `*.aar` and `*.jar` here
(`HoloNETApp.csproj`). `HoloNETAndroidRuntime.cs` then finds the bridge class in them.

| File | What it is |
|---|---|
| `holochain-client.aar` | Client for the Holochain Service Runtime (the shared conductor on the phone), with the Holochain 0.7.1 parcelers |
| `holochain-service.aar` | The Holochain conductor as an Android service (arm64 only): the built-in conductor used when the phone has no shared runtime |
| `holonet-android-bridge.aar` | The Kotlin bridge that picks shared or built-in and signs zome calls (source: `android-runtime/` in the HoloNET repo) |
| `holochain-android-dependencies.zip` | The bridge's runtime dependencies (unpacked here as `.jar` / `.aar` files) |

To use your own build instead, set `HoloNETDownloadRuntime=false` (`dotnet build -p:HoloNETDownloadRuntime=false`) and put your files here.
The libraries need Android 8.1 (API 27) or newer, so the template sets that as the minimum.
'''


ANDROID_SECTION = '''
## Holochain on the phone (Android)

On Android the app starts the Holochain runtime **on the phone**, the same way Our World does (OASIS Edge):

1. If the phone has the **Holochain Service Runtime** app installed, the app asks it for access and shows nothing more than a system
   notification the user approves once. The app then uses the phone's one shared conductor (every Holochain app on the phone shares it).
2. If the runtime is not installed, is not approved in time (60 seconds by default), or setup fails, the app **falls back to the conductor built into
   the app** (a foreground service).
3. The runtime libraries are downloaded into `runtime/android` on the first build (see `runtime/android/README.md`). If they are missing (for example you
   built with `-p:HoloNETDownloadRuntime=false`), the app connects to a conductor you run instead (`HoloNETDemo.AdminUri`).

Zome calls are signed by the conductor, because an app has no admin access to grant itself signing credentials. It works with every `--HoloNET`
choice: the Client and ORM demos use the app connection directly, the API uses `HoloNETAPI.UseAppClientAsync`, and the HyperNET demo joins a zone as the
one player the runtime holds.

**Status: not yet run on a physical phone.** The shared-runtime path is proven by the OASIS team on an x86_64 emulator, not on a physical phone, and this
template's wrapper has been compiled and packed with the real libraries for Android, but not run on a device or emulator. Please test it on yours and tell us what you find.

To try it: install the runtime on the emulator or phone (`adb install android-service-runtime-0.2.0-holochain-0.7-x86_64-debug.apk` for an x86_64
emulator, the arm64 APK for a phone), build and run the app, and approve the notification.

Runtime source and releases:

''' + RUNTIME_LINKS + '''
'''

IOS_SECTION = '''
## Holochain on iOS

An iPhone cannot run a separate conductor process, so the conductor is linked into the app (Holochain 0.7.1 with the wasmi interpreter, because iOS forbids
JIT). On iOS the app starts it with `HoloNETIosRuntime.cs`, which calls `HoloNETIosBridge.xcframework`: put that in `runtime/ios` (see `runtime/ios/README.md`).
Without it the app connects to a conductor you run (`HoloNETDemo.AdminUri`).

**Status:** the conductor runs on the **iOS simulator** in the OASIS team's CI (it starts, installs the OASIS hApp and signs a zome call). It is **not yet proven
on a physical iPhone**, and this template's wrapper has **not been compiled or run by us on a Mac**. Treat it as ready to test. Runtime source and releases:

''' + RUNTIME_LINKS + '''
'''


def ios_runtime_folder_readme():
    return '''# Holochain conductor for iOS

Put `HoloNETIosBridge.xcframework` in this folder. The project links it into the app (`HoloNETApp.csproj`, `NativeReference`) and `HoloNETIosRuntime.cs`
calls its C functions (`holo_ios_*`). It contains the Holochain 0.7.1 conductor (wasmi interpreter, device and Apple-silicon simulator) and the bridge.

Download `HoloNETIosBridge.xcframework.zip` (about 127 MB) from the [oasis-holochain-0.7-v1.1.0](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-holochain-0.7-v1.1.0) release and unzip it here. It is built by that repository's `oasis-ios-bridge` workflow, which compiles the Swift C-ABI bridge and merges it with the
[oasis-ios-conductor-v0.7.1](https://github.com/NextGenSoftwareUK/android-service-runtime/releases/tag/oasis-ios-conductor-v0.7.1) conductor.

The bridge build confirmed the `holo_ios_*` symbols are in the library for device and simulator; it has not been linked into a .NET app by us. The bridge build confirmed the `holo_ios_*` symbols are in the library for device and simulator; it has not been linked into a .NET app by us. Without it the app still builds, `HoloNETIosRuntime.IsBridgeAvailable` is false, and the demo connects to a conductor you run (`HoloNETDemo.AdminUri`).
The conductor is large (about 200 MB per slice before the linker strips it).
'''


def readme(key, name, forced_external):
    if key == 'unity':
        return unity_readme()
    if forced_external:
        if key == 'blazorwasm':
            why = 'Browsers cannot host a conductor, so this app connects to one you run on your development machine'
        elif key in ANDROID_RUNTIME:
            why = ('On Android (and iOS, see below) this app uses the Holochain runtime on the phone (see "Holochain on the phone (Android)" below). Everywhere else it does not bundle\n'
                   'a conductor, so it connects to one you run on your development machine')
        elif key == 'ios':
            why = ('On iOS this app can run the conductor inside the app (see "Holochain on iOS" below). Without the runtime library it does not bundle\n'
                   'a conductor, so it connects to one you run on your development machine')
        else:
            why = 'This app type does not bundle a Holochain conductor, so it connects to one you run on your development machine'
        cond = (why + '\n(`ws://localhost:7777`). Start it with `./start-conductor.sh` (Git Bash, Linux, macOS) or `.\\start-conductor.ps1` (PowerShell), and set\n'
                '`HoloNETDemo.AdminUri` (an Android emulator reaches your machine at `ws://10.0.2.2:7777`). The conductor installs the hApp from a path on\n'
                "*its* machine: set `HoloNETDemo.HappPath` to where `oasis.happ` (or your own `.happ`) is on the conductor's computer.")
    else:
        cond = (
            'On Windows the default is a bundled conductor (the app starts and stops Holochain itself). On Linux and macOS HoloNET starts the Holochain on your\n'
            'computer, and the first time it downloads the official one (checked against a SHA-256) into your user folder; nothing to install.\n'
            'To use a conductor you run yourself instead, create the project with `--Conductor external` and start one first, in another terminal:\n\n'
            '```bash\n./start-conductor.sh        # Git Bash, Linux, macOS (needs Holochain 0.7.x on your PATH)\n.\\start-conductor.ps1       # PowerShell\n```\n\n'
            'Choose with `--Conductor auto|embedded|external` and `--HolochainVersion both|0.7.1|0.7.0` when you create the project.')
    extra = ''
    if key in ANDROID_RUNTIME:
        extra += ANDROID_SECTION
    if key in IOS_TYPES:
        extra += IOS_SECTION
    default = 'client' if key == 'android' else 'api'
    return f'''# HoloNETApp

A {name} with [HoloNET](https://holonet.oasisomniverse.one) wired in.

Everything HoloNET does in this app is in **`HoloNETDemo.cs`**: read it, then replace it with your own code. Each screen / page /
endpoint just calls `HoloNETDemo.RunAsync()` and shows the text it returns.

## Run it

{cond}

Then run the app as usual for this app type (`dotnet run`, or press F5 in your IDE).
{extra}
## Switch the HoloNET library

Created with `dotnet new holonet-{key} --HoloNET {default}` (the default). The other choices:

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
'''


def build(key, base_args, patch, name, classes, forced, proj='', cpm=False, nocsproj=False):
    out = os.path.join(CONTENT, 'holonet-' + key)
    if os.path.isdir(out):
        shutil.rmtree(out)
    with tempfile.TemporaryDirectory(prefix='holonet-tpl-') as tmp:
        if callable(base_args):
            base_args(tmp)   # no stock `dotnet new` template for this app type (WinUI 3's ship with Visual Studio): a function writes the base project
        else:
            r = subprocess.run(['dotnet', 'new'] + base_args + ['-n', 'HoloNETApp', '-o', tmp, '--force'], capture_output=True, text=True)
            if r.returncode != 0:
                raise SystemExit(f'dotnet new {" ".join(base_args)} failed:\n{r.stdout}{r.stderr}')
        shutil.copytree(tmp, out, ignore=shutil.ignore_patterns('bin', 'obj', '*.user', '.vs'))
    root = out
    out = os.path.join(out, proj) if proj else out
    if not nocsproj:
        csproj = os.path.join(out, 'HoloNETApp.csproj')
        s = rd(csproj)
        s = s.replace('<Nullable>enable</Nullable>', '<Nullable>disable</Nullable>')
        i = s.rindex('</Project>')
        wr(csproj, s[:i].rstrip('\n') + '\n' + package_block(forced, cpm, key in ANDROID_RUNTIME, key in IOS_TYPES, key != 'android') + s[i:])
    patch(out)
    demo = rd(os.path.join(SHARED, 'HoloNETDemo.cs')).replace('HOLONET_LIB_TEXT', 'HOLONET_LIBRARY_NAME')
    if forced:
        demo = demo.replace('//#if (!UseEmbedded)\n            dna.HolochainConductorAdminURI', '//#if (true)\n            dna.HolochainConductorAdminURI').replace('//#if (YouStartConductor)', '//#if (true)')
    wr(os.path.join(out, 'HoloNETDemo.cs'), demo)
    if key in ANDROID_RUNTIME:
        wr(os.path.join(out, 'HoloNETAndroidRuntime.cs'), rd(os.path.join(SHARED, 'HoloNETAndroidRuntime.cs')))
        wr(os.path.join(out, 'runtime', 'android', 'README.md'), android_runtime_folder_readme())
    if key in IOS_TYPES:
        wr(os.path.join(out, 'HoloNETIosRuntime.cs'), rd(os.path.join(SHARED, 'HoloNETIosRuntime.cs')))
        wr(os.path.join(out, 'runtime', 'ios', 'README.md'), ios_runtime_folder_readme())
    wr(os.path.join(root, 'README.md'), readme(key, name, forced))
    for f in ('start-conductor.sh', 'start-conductor.ps1', '.gitignore'):
        shutil.copy(os.path.join(SHARED, f), os.path.join(root, f))
    cfg = os.path.join(root, '.template.config')
    wr(os.path.join(cfg, 'template.json'), json.dumps(template_json(key, name, classes, forced), indent=2) + '\n')
    print('built holonet-' + key)


if __name__ == '__main__':
    want = set(sys.argv[1:])
    for t in TYPES:
        if not want or t[0] in want:
            build(*t)
