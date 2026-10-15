using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// HoloNET starting and stopping the conductor itself (AutoStartHolochainConductor / AutoShutdownHolochainConductor).
    /// These start and kill holochain/hc processes, so they only run when HOLONET_LIVE_AUTOSTART=1 and no other conductor
    /// is in use (run them on their own, not alongside the shared-conductor tests).
    /// HOLONET_LIVE_HOLOCHAIN_BIN / HOLONET_LIVE_HC_BIN point at the binaries (default: the bundled Client/Resources ones).
    /// </summary>
    [Collection(LiveConductor.Collection)]
    public class LiveConductorAutoStartTests
    {
        private static bool Enabled => Environment.GetEnvironmentVariable("HOLONET_LIVE_AUTOSTART") == "1";

        private static string Bundled(string exe)
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, "NextGenSoftware.Holochain.HoloNET.Client", "Resources", exe);
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }

        // Defaults to the copy embedded mode extracts, so Windows Firewall only ever sees one holochain.exe path per version.
        private static string HolochainBin => Environment.GetEnvironmentVariable("HOLONET_LIVE_HOLOCHAIN_BIN") ?? Extracted("0.7.1", "holochain.exe") ?? Bundled("holochain.exe");

        private static string Extracted(string version, string exe)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NextGenSoftware", "HoloNET", version, exe);
            return File.Exists(path) ? path : null;
        }
        private static string HcBin => Environment.GetEnvironmentVariable("HOLONET_LIVE_HC_BIN") ?? Bundled("hc.exe");

        private static int FreePort()
        {
            var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static bool IsListening(int port)
        {
            try
            {
                using var client = new TcpClient();
                return client.ConnectAsync("127.0.0.1", port).Wait(1000) && client.Connected;
            }
            catch { return false; }
        }

        private static int RunningConductors() =>
            Process.GetProcessesByName("holochain").Length + Process.GetProcessesByName("hc").Length;

        [SkippableFact]
        public Task Live_AutoStart_ProductionConductor_ThenAutoShutdown()
        {
            Skip.If(HolochainBin == null || !File.Exists(HolochainBin), "holochain.exe not found.");
            return AutoStartThenShutdown(dna =>
            {
                dna.HolochainConductorMode = HolochainConductorModeEnum.UseExternal;
                dna.FullPathToExternalHolochainConductorBinary = HolochainBin;
            }, null);
        }

        /// <summary>The Embedded package's bundled conductors (run with -p:Embedded=true and HOLONET_LIVE_EMBEDDED=1).</summary>
        [SkippableTheory]
        [InlineData(EmbeddedHolochainVersion.V0_7_1, "0.7.1")]
        [InlineData(EmbeddedHolochainVersion.V0_7_0, "0.7.0")]
        public Task Live_AutoStart_EmbeddedConductor_EachBundledVersion(EmbeddedHolochainVersion version, string expected)
        {
            Skip.IfNot(Environment.GetEnvironmentVariable("HOLONET_LIVE_EMBEDDED") == "1", "Build with -p:Embedded=true and set HOLONET_LIVE_EMBEDDED=1.");
            return AutoStartThenShutdown(dna =>
            {
                dna.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded;
                dna.EmbeddedHolochainVersion = version;
            }, expected);
        }

        private static async Task AutoStartThenShutdown(Action<HoloNETDNA> configure, string expectedVersion)
        {
            Skip.IfNot(Enabled, "Set HOLONET_LIVE_AUTOSTART=1 (and stop any other conductor) to run.");
            Skip.If(!OperatingSystem.IsWindows(), "Auto-start launches the conductor through PowerShell (Windows).");
            Skip.If(RunningConductors() > 0, "Another holochain/hc process is running; auto-shutdown would kill it.");

            int port = FreePort();
            string configDir = Path.Combine(Path.GetTempPath(), "HoloNET-autostart-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            var dna = new HoloNETDNA
            {
                AutoStartHolochainConductor = true,
                AutoShutdownHolochainConductor = true,
                HolochainConductorToUse = HolochainConductorEnum.HolochainProductionConductor,
                HolochainConductorConfigPath = Path.Combine(configDir, "conductor-config.yaml"),
                SecondsToWaitForHolochainConductorToStart = 8,
                ShowHolochainConductorWindow = false,
                HolochainConductorAdminURI = $"ws://localhost:{port}"
            };
            configure(dna);
            var admin = new HoloNETClientAdmin(dna);
            int powerShellsBefore = Process.GetProcessesByName("powershell").Length;

            bool started = false, stopped = false;
            admin.OnHolochainConductorStarted += (s, e) => started = true;
            admin.OnHolochainConductorsShutdownComplete += (s, e) => stopped = true;

            try
            {
                var connected = await admin.ConnectAsync($"ws://localhost:{port}");
                Assert.True(started, "OnHolochainConductorStarted was not raised");
                Assert.False(connected.IsError, "connect to auto-started conductor: " + connected.Message);
                Assert.True(connected.IsConnected, "not connected: " + connected.Message);

                var key = await admin.GenerateAgentPubKeyAsync();
                Assert.False(key.IsError, "generate_agent_pub_key on auto-started conductor: " + key.Message);

                if (expectedVersion != null)
                {
                    var running = Process.GetProcessesByName("holochain").Single();
                    string path = running.MainModule.FileName;
                    Assert.Contains(Path.DirectorySeparatorChar + expectedVersion + Path.DirectorySeparatorChar, path);
                }

                var disconnected = await admin.DisconnectAsync();
                Assert.False(disconnected.IsError, "disconnect: " + disconnected.Message);
                Assert.True(stopped, "OnHolochainConductorsShutdownComplete was not raised");

                for (int i = 0; i < 10 && IsListening(port); i++) await Task.Delay(500);
                Assert.False(IsListening(port), "conductor still listening after auto-shutdown");
                Assert.Empty(Process.GetProcessesByName("holochain"));
                Assert.True(Process.GetProcessesByName("powershell").Length <= powerShellsBefore, "the PowerShell window that hosted the conductor was left running");
            }
            finally
            {
                foreach (var p in Process.GetProcessesByName("holochain")) { try { p.Kill(); } catch { } }
                try { Directory.Delete(configDir, true); } catch { }
            }
        }
    }
}
