using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// A client on a computer with no Holochain installed downloads the conductor, starts it and connects. Opt-in (it downloads about 55 MB from the official Holochain
    /// release): set HOLONET_TEST_DOWNLOAD=1. The Linux job in .github/workflows/conductor-download.yml runs it on a clean Ubuntu runner.
    /// </summary>
    public class AutoDownloadConductorTests
    {
        private static int FreePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        [SkippableFact]
        public async Task ANewMachineDownloadsStartsAndConnectsToTheConductor()
        {
            Skip.IfNot(Environment.GetEnvironmentVariable("HOLONET_TEST_DOWNLOAD") == "1", "Set HOLONET_TEST_DOWNLOAD=1 to download the conductor (about 55 MB).");

            string folder = Path.Combine(Path.GetTempPath(), "holonet-auto-" + Guid.NewGuid().ToString("N"));
            string previousFolder = Environment.GetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER");
            string previousPath = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER", folder);
            // No holochain on the PATH: this is a computer without Holochain.
            Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator.ToString(), (previousPath ?? "").Split(Path.PathSeparator).Where(p => !File.Exists(Path.Combine(p, "holochain")) && !File.Exists(Path.Combine(p, "holochain.exe")))));

            HoloNETClientAdmin admin = null;

            try
            {
                string uri = "ws://127.0.0.1:" + FreePort();
                admin = new HoloNETClientAdmin(new HoloNETDNA
                {
                    HolochainConductorMode = HolochainConductorModeEnum.UseSystemGlobal,
                    HolochainConductorAdminURI = uri,
                    HolochainConductorConfigPath = Path.Combine(folder, "conductor-config.yaml"),
                    AutoStartHolochainConductor = true,
                    AutoShutdownHolochainConductor = true,
                    AutoDownloadConductor = true,
                    HolochainConductorVersion = "0.7.1",
                    SecondsToWaitForHolochainConductorToStart = 60,
                    LogToConsole = false
                });

                HoloNETConnectedEventArgs connected = await admin.ConnectAsync(uri);

                Assert.True(connected.IsConnected, connected.Message);
                Assert.True(File.Exists(HolochainConductorDownloader.CachedPath("holochain", "0.7.1")), "the conductor was not downloaded");
            }
            finally
            {
                if (admin != null)
                    await admin.DisconnectAsync();

                Environment.SetEnvironmentVariable("PATH", previousPath);
                Environment.SetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER", previousFolder);
                try { if (Directory.Exists(folder)) Directory.Delete(folder, true); } catch (IOException) { }
            }
        }
    }
}
