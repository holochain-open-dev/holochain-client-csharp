// Proves a computer with no Holochain installed can download, start and connect to a conductor with HoloNET (HoloNETDNA.AutoDownloadConductor).
//   dotnet run --project tools/ConductorDownloadSmoke [holochain version, default 0.7.1]
// It uses a throwaway cache folder and removes holochain from this process's PATH, so it always downloads. Exit code 0 = connected.
using System.Net;
using System.Net.Sockets;
using NextGenSoftware.Holochain.HoloNET.Client;

string version = args.Length > 0 ? args[0] : "0.7.1";
string folder = Path.Combine(Path.GetTempPath(), "holonet-smoke-" + Guid.NewGuid().ToString("N"));
Environment.SetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER", folder);
Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator)
    .Where(p => !File.Exists(Path.Combine(p, "holochain")) && !File.Exists(Path.Combine(p, "holochain.exe")))));

TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
int port = ((IPEndPoint)listener.LocalEndpoint).Port;
listener.Stop();
string uri = "ws://127.0.0.1:" + port;

Console.WriteLine($"{System.Runtime.InteropServices.RuntimeInformation.OSDescription} {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}: asking HoloNET for Holochain {version} (asset {HolochainConductorDownloader.AssetName("holochain")})");

HoloNETClientAdmin admin = new HoloNETClientAdmin(new HoloNETDNA
{
    HolochainConductorMode = HolochainConductorModeEnum.UseSystemGlobal,
    HolochainConductorAdminURI = uri,
    HolochainConductorConfigPath = Path.Combine(folder, "conductor-config.yaml"),
    AutoStartHolochainConductor = true,
    AutoShutdownHolochainConductor = true,
    AutoDownloadConductor = true,
    HolochainConductorVersion = version,
    SecondsToWaitForHolochainConductorToStart = 90,
    LogToConsole = true
});

int exitCode = 1;

try
{
    HoloNETConnectedEventArgs connected = await admin.ConnectAsync(uri);
    string path = HolochainConductorDownloader.CachedPath("holochain", version);
    Console.WriteLine($"downloaded: {File.Exists(path)} ({path}), connected: {connected.IsConnected} {connected.Message}");
    exitCode = connected.IsConnected && File.Exists(path) ? 0 : 1;
}
catch (Exception ex)
{
    Console.WriteLine("FAILED: " + ex);
}
finally
{
    await admin.DisconnectAsync();
}

Console.WriteLine(exitCode == 0 ? "OK: downloaded, started and connected." : "FAIL");
return exitCode;
