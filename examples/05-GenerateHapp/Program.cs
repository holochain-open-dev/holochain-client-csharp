// HoloNET example 05: generate a complete Holochain 0.7 hApp (Rust + C#) from a description of your data
//
// Two ways, producing the same hApp:
//   * WEB5 STAR (the OASIS low/no-code generator) from a CelestialBody DNA folder - see StarOAPP/ and its README.md:
//       star light StarOAPP/LightRequest.json        (STAR fills its Holochain07 templates through HoloOASIS)
//   * HoloNET ORM's HolochainHappGenerator, from code or the same DNA folder - what this program does.
//
// Either way you get a Cargo workspace (integrity + coordinator zomes with create / get / update / delete / list for every
// entry and anchored collections), dna.yaml, happ.yaml, build.sh, and the matching HoloNET ORM C# models.
//
// Run:  dotnet run --project examples/05-GenerateHapp            (generates into examples/05-GenerateHapp/out)
//       add --build to also compile it (needs Rust with the wasm32-unknown-unknown target, and hc on the PATH)
// Docs: https://docs.holonet.oasisomniverse.one/star-happ-generation

using System.Diagnostics;
using HoloNET.Examples;
using NextGenSoftware.Holochain.HoloNET.ORM.Generation;

string here = FindExampleFolder();
string output = Path.Combine(Environment.GetEnvironmentVariable("HOLONET_EXAMPLE_SMOKE") == "1" ? Path.GetTempPath() : here, "out-" + Guid.NewGuid().ToString("N").Substring(0, 4));

// ---- 1. Describe the hApp in code ---------------------------------------------------------------------------------
ExampleSetup.Title("1. Describe the hApp in code");
var spec = new HappSpec
{
    Name = "space_game",                     // the hApp, its DNA and its role (snake_case)
    CSharpNamespace = "SpaceGame",           // the generated C# models' namespace
    HolochainVersion = HappHolochainVersion.V0_7_1,   // or V0_7_0 (hdk 0.7.0 / hdi 0.8.0)
    Zomes =
    {
        new HappZomeSpec("Game",
            new HappEntrySpec("Ship",
                new HappFieldSpec("Name", "string"),
                new HappFieldSpec("Speed", "double"),
                new HappFieldSpec("Crew", "int"),
                new HappFieldSpec("Launched", "DateTime"),   // Guid and DateTime are stored as strings
                new HappFieldSpec("IsDocked", "bool")),
            new HappEntrySpec("Planet",
                new HappFieldSpec("Name", "string"),
                new HappFieldSpec("Population", "long")))
    }
};
foreach (var zome in spec.Zomes)
    foreach (var entry in zome.Entries)
        Console.WriteLine($"  {zome.Name}.{entry.Name}: {string.Join(", ", entry.Fields.Select(f => f.Name + " (" + f.CSharpType + ")"))}");

// ---- 2. ...or read the same description from a STAR CelestialBody DNA folder ---------------------------------------
ExampleSetup.Title("2. The same, read from StarOAPP/CelestialBodyDNA (what STAR uses)");
HappSpec fromDna = HolochainHappGenerator.FromStarDnaFolder(Path.Combine(here, "StarOAPP", "CelestialBodyDNA"), "space_game", "SpaceGame");
foreach (var zome in fromDna.Zomes)
    foreach (var entry in zome.Entries)
        Console.WriteLine($"  {zome.Name}.{entry.Name}: {string.Join(", ", entry.Fields.Select(f => f.Name + " (" + f.CSharpType + ")"))}");

// ---- 3. Generate --------------------------------------------------------------------------------------------------
ExampleSetup.Title("3. Generate into " + output);
List<string> files = HolochainHappGenerator.Generate(spec, output);
foreach (string f in files.Select(f => Path.GetRelativePath(output, f)).OrderBy(f => f))
    Console.WriteLine("  " + f.Replace('\\', '/'));

string coordinator = File.ReadAllText(Path.Combine(output, "dnas", "space_game", "zomes", "coordinator", "game", "src", "lib.rs"));
Console.WriteLine("Coordinator functions: " + string.Join(", ", System.Text.RegularExpressions.Regex.Matches(coordinator, @"pub fn (\w+)\(").Select(m => m.Groups[1].Value)));

// ---- 4. Build it (optional) ---------------------------------------------------------------------------------------
if (args.Contains("--build"))
{
    ExampleSetup.Title("4. Build: ./build.sh (cargo, then hc dna pack + hc app pack)");
    var build = Process.Start(new ProcessStartInfo("bash", "build.sh") { WorkingDirectory = output, UseShellExecute = false });
    build.WaitForExit();
    ExampleSetup.Check(build.ExitCode == 0, "build.sh");
    Console.WriteLine("Built " + Path.Combine(output, "workdir", "space_game.happ"));
    Console.WriteLine("Install it with HoloNET (role 'space_game') and use CSharp/Ship.cs and CSharp/Planet.cs: see example 02.");
}
else
    Console.WriteLine("\n(Add --build to compile it: needs Rust + the wasm32-unknown-unknown target, and hc.)");

if (Environment.GetEnvironmentVariable("HOLONET_EXAMPLE_SMOKE") == "1")
    Directory.Delete(output, true);

// The example's own folder (where StarOAPP/ is), whether run with dotnet run or from bin/.
static string FindExampleFolder()
{
    string dir = AppContext.BaseDirectory;
    while (dir != null && !Directory.Exists(Path.Combine(dir, "StarOAPP")))
        dir = Path.GetDirectoryName(dir);
    return dir ?? Directory.GetCurrentDirectory();
}
