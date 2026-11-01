// HoloNET Client: connect to a Holochain conductor, install your hApp, call a zome function and listen for signals.
// Run:  dotnet run -- path/to/your.happ <role-name> <zome> <function>
//
// Guide:      https://docs.holonet.oasisomniverse.one/client-guide
// Tutorials:  https://learn.holonet.oasisomniverse.one  (tutorial 1 walks through this file)
using NextGenSoftware.Holochain.HoloNET.Client;

string happPath = args.Length > 0 ? args[0] : "my.happ";
string role = args.Length > 1 ? args[1] : "my-role";
string zome = args.Length > 2 ? args[2] : "my_zome";
string function = args.Length > 3 ? args[3] : "my_function";

if (!File.Exists(happPath))
{
    Console.WriteLine($"Pass the path to a .happ file: dotnet run -- path/to/your.happ <role-name> <zome> <function>   ('{happPath}' was not found)");
    return;
}

// HoloNETDNA holds every setting. Defaults: the bundled conductor on Windows (started and stopped for you), otherwise the one on ws://localhost:7777.
var dna = new HoloNETDNA { LogToConsole = false };
//#if (!UseEmbedded)
dna.AutoStartHolochainConductor = false;   // you start the conductor (start-conductor script)
dna.AutoShutdownHolochainConductor = false;
//#endif

var admin = new HoloNETClientAdmin(dna);
var connected = await admin.ConnectAsync("ws://localhost:7777");
if (!connected.IsConnected) { Console.WriteLine("Could not connect: " + connected.Message); return; }

// One call: generate an agent key, install the hApp, enable it, create signing credentials, attach an app interface and connect an app client.
string appId = "my-app-" + Guid.NewGuid().ToString("N")[..8];
var installed = await admin.InstallEnableSignAttachAndConnectToHappAsync(appId, happPath, role);
if (!installed.IsAppConnected) { Console.WriteLine("Could not install the hApp: " + installed.Message); return; }
var client = installed.HoloNETClientAppAgent;
Console.WriteLine($"Agent {installed.AgentPubKey} is running {Path.GetFileName(happPath)}");

// Signals emitted by your zome arrive here.
client.OnSignalCallBack += (s, e) => Console.WriteLine($"Signal received ({e.RawSignalData.Data.Length} bytes)");

// Zome calls: parameters are serialised with MessagePack using their field names, so they must match your Rust struct.
var result = await client.CallZomeFunctionAsync(zome, function, new Dictionary<string, object>());
Console.WriteLine(result.IsError ? "Zome call failed: " + result.Message : "Zome call returned " + result.Records.Count + " record(s)");

await client.DisconnectAsync();
await admin.UninstallAppAsync(appId);
await admin.DisconnectAsync();
