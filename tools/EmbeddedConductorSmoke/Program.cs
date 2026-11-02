// Proves an offline Embedded package (HoloNET.Client.Embedded.LinuxX64 etc.) starts and connects to the conductor it carries, with nothing downloaded and no
// holochain on the PATH. Exit code 0 = connected from the bundled conductor.
using System.Net;
using System.Net.Sockets;
using NextGenSoftware.Holochain.HoloNET.Client;

string folder = Path.Combine(Path.GetTempPath(), "holonet-embedded-smoke-" + Guid.NewGuid().ToString("N"));
Environment.SetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER", folder);
Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator)
    .Where(p => !File.Exists(Path.Combine(p, "holochain")) && !File.Exists(Path.Combine(p, "holochain.exe")))));

TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
int port = ((IPEndPoint)listener.LocalEndpoint).Port;
listener.Stop();
string uri = "ws://127.0.0.1:" + port;

Console.WriteLine($"{System.Runtime.InteropServices.RuntimeInformation.OSDescription}: this computer is {HoloNETClientBase.CurrentEmbeddedPlatform()}, " +
    $"package is for this computer: {HoloNETClientBase.IsEmbeddedBundleForThisComputer()}, bundles 0.7.1: {HoloNETClientBase.IsEmbeddedHolochainVersionBundled(EmbeddedHolochainVersion.V0_7_1)}, " +
    $"0.7.0: {HoloNETClientBase.IsEmbeddedHolochainVersionBundled(EmbeddedHolochainVersion.V0_7_0)}");

if (!HoloNETClientBase.IsEmbeddedBundleForThisComputer())
{
    Console.WriteLine("FAIL: the package's conductors are not for this computer.");
    return 2;
}

// A package with only 0.7.0 gets 0.7.0 whatever this says (with a log warning); the both-versions package uses it.
HoloNETClientAdmin admin = new HoloNETClientAdmin(new HoloNETDNA
{
    HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded,
    HolochainConductorAdminURI = uri,
    HolochainConductorConfigPath = Path.Combine(folder, "conductor-config.yaml"),
    AutoStartHolochainConductor = true,
    AutoShutdownHolochainConductor = true,
    AutoDownloadConductor = false, // nothing may be downloaded: this is the offline package
    SecondsToWaitForHolochainConductorToStart = 90,
    LogToConsole = true
});

int exitCode = 1;

try
{
    HoloNETConnectedEventArgs connected = await admin.ConnectAsync(uri);
    string[] extracted = Directory.Exists(folder) ? Directory.GetFiles(folder, "holochain*", SearchOption.AllDirectories).Where(f => !f.EndsWith(".yaml")).ToArray() : Array.Empty<string>();
    Console.WriteLine($"extracted: {string.Join(", ", extracted)}; connected: {connected.IsConnected} {connected.Message}");
    exitCode = connected.IsConnected && extracted.Length > 0 ? 0 : 1;
}
catch (Exception ex)
{
    Console.WriteLine("FAILED: " + ex);
}
finally
{
    await admin.DisconnectAsync();
}

Console.WriteLine(exitCode == 0 ? "OK: started and connected from the bundled conductor." : "FAIL");
return exitCode;
