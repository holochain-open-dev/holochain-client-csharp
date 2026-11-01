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


def p_ios(d):
    pass  # filled in below once the stock iOS files are inspected


# key, dotnet new args, patch fn, display name, classifications, forced external (no bundled conductor), project file TFM fix
TYPES = [
    ('console', ['console'], p_console, 'Console app', ['Console'], False),
    ('worker', ['worker'], p_worker, 'Worker Service (background service)', ['Service', 'Worker'], False),
    ('webapi', ['webapi'], p_webapi, 'ASP.NET Core Web API', ['Web', 'API'], False),
    ('mvc', ['mvc'], p_mvc, 'ASP.NET Core MVC web app', ['Web', 'MVC'], False),
    ('razor', ['razor'], p_razor, 'ASP.NET Core Razor Pages web app', ['Web', 'Razor'], False),
    ('blazor', ['blazor'], p_blazor, 'Blazor Web App (server)', ['Web', 'Blazor'], False),
    ('blazorwasm', ['blazorwasm'], p_blazorwasm, 'Blazor WebAssembly app', ['Web', 'Blazor', 'WebAssembly'], True),
    ('wpf', ['wpf'], p_wpf, 'WPF app', ['Desktop', 'WPF'], False),
    ('winforms', ['winforms'], p_winforms, 'Windows Forms app', ['Desktop', 'WinForms'], False),
    ('maui', ['maui'], p_maui, '.NET MAUI app', ['MAUI', 'Mobile', 'Desktop'], True),
    ('maui-blazor', ['maui-blazor'], p_mauiblazor, '.NET MAUI Blazor Hybrid app', ['MAUI', 'Blazor', 'Mobile'], True),
    ('android', ['android'], p_android, 'Android app', ['Android', 'Mobile'], True),
]


def package_block(forced_external):
    """The HoloNET package references for the generated csproj. HOLONET_* tokens are replaced by the template engine."""
    return '''
  <ItemGroup>
    <!-- HoloNET: the library chosen with the HoloNET option (api, client, orm or hypernet). HOLONET_PKG_SUFFIX is .Embedded* when the Holochain conductor
         is bundled in the app (the Conductor option, Windows only) and empty when you run your own conductor. -->
    <PackageReference Include="NextGenSoftware.Holochain.HoloNET.HOLONET_LIBRARY_IDHOLONET_PKG_SUFFIX" Version="HOLONET_VERSION" />
    <!--#if (!UseApi)-->
    <!-- Only for the OASIS hApp (oasis.happ) the demo installs: HoloNET API copies it next to your app. Use your own .happ and drop this. -->
    <PackageReference Include="NextGenSoftware.Holochain.HoloNET.APIHOLONET_PKG_SUFFIX" Version="HOLONET_VERSION" />
    <!--#endif-->
  </ItemGroup>

'''


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
    if forced_external:
        sym['UseEmbedded'] = {'type': 'computed', 'value': 'false'}
        # (no Conductor / HolochainVersion choice: this app type cannot host a conductor, so it connects to one you run)
        del sym['HolochainVersion']
    else:
        sym['Conductor'] = {
            'type': 'parameter', 'datatype': 'choice', 'defaultValue': 'auto',
            'description': 'Where the Holochain conductor comes from.',
            'choices': [
                {'choice': 'auto', 'description': 'Bundled in the app on Windows; otherwise one you start (start-conductor script included).'},
                {'choice': 'embedded', 'description': 'Bundle the conductor in the app (Windows only).'},
                {'choice': 'external', 'description': 'Use a conductor you run yourself on ws://localhost:7777 (start-conductor script included).'}]}
        sym['UseEmbedded'] = {'type': 'computed', 'value': '(Conductor == "embedded" || (Conductor == "auto" && OS == "Windows_NT"))'}
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


def readme(key, name, forced_external):
    cond = ('This app type cannot host a Holochain conductor, so it connects to one you run on your development machine\n'
            '(`ws://localhost:7777`). Start it with `./start-conductor.sh` (Git Bash, Linux, macOS) or `.\\start-conductor.ps1` (PowerShell), and set\n'
            '`HoloNETDemo.AdminUri` (an Android emulator reaches your machine at `ws://10.0.2.2:7777`). The conductor installs the hApp from a path on\n'
            "*its* machine: set `HoloNETDemo.HappPath` to where `oasis.happ` (or your own `.happ`) is on the conductor's computer."
            if forced_external else
            'On Windows the default is a bundled conductor (the app starts and stops Holochain itself). Otherwise start one first, in another terminal:\n\n'
            '```bash\n./start-conductor.sh        # Git Bash, Linux, macOS (needs Holochain 0.7.x on your PATH)\n.\\start-conductor.ps1       # PowerShell\n```\n\n'
            'Choose with `--Conductor auto|embedded|external` and `--HolochainVersion both|0.7.1|0.7.0` when you create the project.')
    return f'''# HoloNETApp

A {name} with [HoloNET](https://holonet.oasisomniverse.one) wired in.

Everything HoloNET does in this app is in **`HoloNETDemo.cs`**: read it, then replace it with your own code. Each screen / page /
endpoint just calls `HoloNETDemo.RunAsync()` and shows the text it returns.

## Run it

{cond}

Then run the app as usual for this app type (`dotnet run`, or press F5 in your IDE).

## Switch the HoloNET library

Created with `dotnet new holonet-{key} --HoloNET api` (the default). The other choices:

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


def build(key, base_args, patch, name, classes, forced):
    out = os.path.join(CONTENT, 'holonet-' + key)
    if os.path.isdir(out):
        shutil.rmtree(out)
    with tempfile.TemporaryDirectory(prefix='holonet-tpl-') as tmp:
        r = subprocess.run(['dotnet', 'new'] + base_args + ['-n', 'HoloNETApp', '-o', tmp, '--force'], capture_output=True, text=True)
        if r.returncode != 0:
            raise SystemExit(f'dotnet new {" ".join(base_args)} failed:\n{r.stdout}{r.stderr}')
        shutil.copytree(tmp, out, ignore=shutil.ignore_patterns('bin', 'obj', '*.user', '.vs'))
    csproj = os.path.join(out, 'HoloNETApp.csproj')
    s = rd(csproj)
    s = s.replace('<Nullable>enable</Nullable>', '<Nullable>disable</Nullable>')
    i = s.rindex('</Project>')
    wr(csproj, s[:i].rstrip('\n') + '\n' + package_block(forced) + s[i:])
    patch(out)
    demo = rd(os.path.join(SHARED, 'HoloNETDemo.cs')).replace('HOLONET_LIB_TEXT', 'HOLONET_LIBRARY_NAME')
    if forced:
        demo = demo.replace('//#if (!UseEmbedded)\n            dna.HolochainConductorAdminURI', '//#if (true)\n            dna.HolochainConductorAdminURI')
    wr(os.path.join(out, 'HoloNETDemo.cs'), demo)
    wr(os.path.join(out, 'README.md'), readme(key, name, forced))
    for f in ('start-conductor.sh', 'start-conductor.ps1', '.gitignore'):
        shutil.copy(os.path.join(SHARED, f), os.path.join(out, f))
    cfg = os.path.join(out, '.template.config')
    wr(os.path.join(cfg, 'template.json'), json.dumps(template_json(key, name, classes, forced), indent=2) + '\n')
    print('built holonet-' + key)


if __name__ == '__main__':
    want = set(sys.argv[1:])
    for t in TYPES:
        if not want or t[0] in want:
            build(*t)
