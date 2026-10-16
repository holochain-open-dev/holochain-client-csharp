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
            }, BundledVersion(version, expected));
        }

        /// <summary>The folder a run should use: the requested version, or (single-version Embedded package) the one it bundles.</summary>
        private static string BundledVersion(EmbeddedHolochainVersion version, string expected) =>
            HoloNETClientBase.IsEmbeddedHolochainVersionBundled(version) ? expected : (expected == "0.7.1" ? "0.7.0" : "0.7.1");

        private static string TestHapp(string version, string variant = null)
        {
            string tag = version.Replace(".", "");
            string folder = "happ-" + tag + (variant == null ? "" : "-" + variant);
            string file = variant == null ? "oasis.happ" : "oasis-" + variant + ".happ";
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, ".holochain-test", folder, file);
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }

        /// <summary>hc sandbox (HcDevTool) mode with each bundled release: hc generates a sandbox for the hApp and runs it.</summary>
        [SkippableTheory]
        [InlineData(EmbeddedHolochainVersion.V0_7_1, "0.7.1")]
        [InlineData(EmbeddedHolochainVersion.V0_7_0, "0.7.0")]
        public Task Live_AutoStart_HcDevTool_EachBundledVersion(EmbeddedHolochainVersion version, string expected)
        {
            Skip.IfNot(Environment.GetEnvironmentVariable("HOLONET_LIVE_EMBEDDED") == "1", "Build with -p:Embedded=true and set HOLONET_LIVE_EMBEDDED=1.");
            expected = BundledVersion(version, expected);
            string happ = TestHapp(expected);
            Skip.If(happ == null, "No " + expected + " test hApp in .holochain-test.");
            string root = Path.Combine(Path.GetTempPath(), "HoloNET-hc-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(root);
            return AutoStartThenShutdown(dna =>
            {
                dna.HolochainConductorToUse = HolochainConductorEnum.HcDevTool;
                dna.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded;
                dna.EmbeddedHolochainVersion = version;
                dna.FullPathToRootHappFolder = root;
                dna.FullPathToCompiledHappFolder = Path.GetDirectoryName(happ); // hc generate finds the .happ in this folder
                dna.SecondsToWaitForHolochainConductorToStart = 90; // upper bound; HoloNET connects as soon as the port is up
            }, expected);
        }

        private static HoloNETDNA ExternalDna(int port, string configDir) => new HoloNETDNA
        {
            AutoStartHolochainConductor = true,
            AutoShutdownHolochainConductor = true,
            HolochainConductorToUse = HolochainConductorEnum.HolochainProductionConductor,
            HolochainConductorMode = HolochainConductorModeEnum.UseExternal,
            FullPathToExternalHolochainConductorBinary = HolochainBin,
            HolochainConductorConfigPath = Path.Combine(configDir, "conductor-config.yaml"),
            SecondsToWaitForHolochainConductorToStart = 8,
            OnlyAllowOneHolochainConductorToRunAtATime = false,
            HolochainConductorAdminURI = "ws://localhost:" + port
        };

        /// <summary>Shutting down the current conductor must leave conductors this client did not start alone.</summary>
        [SkippableFact]
        public async Task Live_AutoShutdown_CurrentOnly_LeavesOtherConductorsRunning()
        {
            Skip.IfNot(Enabled, "Set HOLONET_LIVE_AUTOSTART=1 (and stop any other conductor) to run.");
            Skip.If(!OperatingSystem.IsWindows(), "Auto-start launches the conductor through PowerShell (Windows).");
            Skip.If(HolochainBin == null, "holochain.exe not found.");
            Skip.If(RunningConductors() > 0, "Another holochain/hc process is running.");

            int portA = FreePort(), portB = FreePort();
            string dirA = Path.Combine(Path.GetTempPath(), "HoloNET-a-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            string dirB = Path.Combine(Path.GetTempPath(), "HoloNET-b-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            var a = new HoloNETClientAdmin(ExternalDna(portA, dirA));
            var b = new HoloNETClientAdmin(ExternalDna(portB, dirB));
            try
            {
                Assert.True((await a.ConnectAsync("ws://localhost:" + portA)).IsConnected);
                Assert.True((await b.ConnectAsync("ws://localhost:" + portB)).IsConnected);
                Assert.Equal(2, Process.GetProcessesByName("holochain").Length);

                var bDown = await b.DisconnectAsync(DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode.ShutdownCurrentConductorOnly);
                Assert.Equal(1, bDown.HolochainConductorsShutdownResult.NumberOfHolochainExeInstancesShutdown);
                for (int i = 0; i < 10 && IsListening(portB); i++) await Task.Delay(500);
                Assert.False(IsListening(portB), "conductor B still running");

                // A's conductor survived and still answers.
                Assert.True(IsListening(portA), "shutting down B killed conductor A");
                var key = await a.GenerateAgentPubKeyAsync();
                Assert.False(key.IsError, "conductor A after B shut down: " + key.Message);

                await a.DisconnectAsync();
                for (int i = 0; i < 10 && IsListening(portA); i++) await Task.Delay(500);
                Assert.Empty(Process.GetProcessesByName("holochain"));
            }
            finally
            {
                foreach (var p in Process.GetProcessesByName("holochain")) { try { p.Kill(); } catch { } }
                try { Directory.Delete(dirA, true); } catch { }
                try { Directory.Delete(dirB, true); } catch { }
            }
        }

        /// <summary>UseSystemGlobal: holochain / hc found on the PATH (here the extracted 0.7.1 copies, so no new firewall prompt).</summary>
        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public Task Live_AutoStart_FromThePath(bool hc)
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NextGenSoftware", "HoloNET", "0.7.1");
            Skip.IfNot(File.Exists(Path.Combine(dir, "holochain.exe")) && File.Exists(Path.Combine(dir, "hc.exe")), "Run the embedded tests first so 0.7.1 is extracted.");
            string happ = TestHapp("0.7.1");
            Skip.If(hc && happ == null, "No 0.7.1 test hApp.");
            string path = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", dir + Path.PathSeparator + path);
            string root = Path.Combine(Path.GetTempPath(), "HoloNET-path-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(root);
            return AutoStartThenShutdown(dna =>
            {
                dna.HolochainConductorMode = HolochainConductorModeEnum.UseSystemGlobal;
                if (hc)
                {
                    dna.HolochainConductorToUse = HolochainConductorEnum.HcDevTool;
                    dna.FullPathToRootHappFolder = root;
                    dna.FullPathToCompiledHappFolder = Path.GetDirectoryName(happ);
                    dna.SecondsToWaitForHolochainConductorToStart = 90;
                }
            }, "0.7.1").ContinueWith(t => { Environment.SetEnvironmentVariable("PATH", path); t.GetAwaiter().GetResult(); });
        }

        /// <summary>ShutdownAllConductors stops every holochain/hc process (and HoloNET's own host), and nothing else.</summary>
        [SkippableFact]
        public async Task Live_AutoShutdown_AllConductors_StopsEveryConductor()
        {
            Skip.IfNot(Enabled, "Set HOLONET_LIVE_AUTOSTART=1 (and stop any other conductor) to run.");
            Skip.If(!OperatingSystem.IsWindows(), "Auto-start launches the conductor through PowerShell (Windows).");
            Skip.If(HolochainBin == null, "holochain.exe not found.");
            Skip.If(RunningConductors() > 0, "Another holochain/hc process is running.");

            int portA = FreePort(), portB = FreePort();
            string dirA = Path.Combine(Path.GetTempPath(), "HoloNET-all-a-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            string dirB = Path.Combine(Path.GetTempPath(), "HoloNET-all-b-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            var a = new HoloNETClientAdmin(ExternalDna(portA, dirA));
            var dnaB = ExternalDna(portB, dirB);
            dnaB.ShutDownALLHolochainConductors = true;
            var b = new HoloNETClientAdmin(dnaB);
            int powerShellsBefore = Process.GetProcessesByName("powershell").Length;
            try
            {
                Assert.True((await a.ConnectAsync("ws://localhost:" + portA)).IsConnected);
                Assert.True((await b.ConnectAsync("ws://localhost:" + portB)).IsConnected);
                Assert.Equal(2, Process.GetProcessesByName("holochain").Length);

                // B's DNA says "shut down all": both conductors go.
                var bDown = await b.DisconnectAsync();
                Assert.Equal(2, bDown.HolochainConductorsShutdownResult.NumberOfHolochainExeInstancesShutdown);
                for (int i = 0; i < 10 && (IsListening(portA) || IsListening(portB)); i++) await Task.Delay(500);
                Assert.Empty(Process.GetProcessesByName("holochain"));

                // The explicit mode does the same (nothing left to stop now).
                Assert.Equal(0, (await a.ShutDownHolochainConductorsAsync(ShutdownHolochainConductorsMode.ShutdownAllConductors)).NumberOfHolochainExeInstancesShutdown);
                Assert.Equal(0, a.ShutDownHolochainConductors(ShutdownHolochainConductorsMode.ShutdownAllConductors).NumberOfHolochainExeInstancesShutdown);
                a.OnError += (s, e) => { };
                await a.DisconnectAsync(DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode.DoNotShutdownAnyConductors);
                for (int i = 0; i < 10 && Process.GetProcessesByName("powershell").Length > powerShellsBefore + 1; i++) await Task.Delay(300);
            }
            finally
            {
                foreach (var p in Process.GetProcessesByName("holochain")) { try { p.Kill(); } catch { } }
                try { Directory.Delete(dirA, true); } catch { }
                try { Directory.Delete(dirB, true); } catch { }
            }
        }

        /// <summary>OnlyAllowOneHolochainConductorToRunAtATime: a second client reuses the running conductor instead of starting another.</summary>
        [SkippableFact]
        public async Task Live_OnlyAllowOneConductor_SecondClientReusesTheRunningOne()
        {
            Skip.IfNot(Enabled, "Set HOLONET_LIVE_AUTOSTART=1 (and stop any other conductor) to run.");
            Skip.If(!OperatingSystem.IsWindows(), "Auto-start launches the conductor through PowerShell (Windows).");
            Skip.If(HolochainBin == null, "holochain.exe not found.");
            Skip.If(RunningConductors() > 0, "Another holochain/hc process is running.");

            int port = FreePort();
            string dir = Path.Combine(Path.GetTempPath(), "HoloNET-one-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            var first = new HoloNETClientAdmin(ExternalDna(port, dir));
            var secondDna = ExternalDna(port, dir);
            secondDna.OnlyAllowOneHolochainConductorToRunAtATime = true;
            secondDna.AutoShutdownHolochainConductor = false;
            var second = new HoloNETClientAdmin(secondDna);
            bool secondStarted = false;
            second.OnHolochainConductorStarted += (s, e) => secondStarted = true;
            try
            {
                Assert.True((await first.ConnectAsync("ws://localhost:" + port)).IsConnected);
                var connected = await second.ConnectAsync("ws://localhost:" + port);
                Assert.True(connected.IsConnected, "second client: " + connected.Message);
                Assert.False(secondStarted, "second client started another conductor");
                Assert.Single(Process.GetProcessesByName("holochain"));

                await second.DisconnectAsync();
                Assert.True(IsListening(port), "second client's disconnect stopped the shared conductor");
                await first.DisconnectAsync();
                for (int i = 0; i < 10 && IsListening(port); i++) await Task.Delay(500);
                Assert.Empty(Process.GetProcessesByName("holochain"));
            }
            finally
            {
                foreach (var p in Process.GetProcessesByName("holochain")) { try { p.Kill(); } catch { } }
                try { Directory.Delete(dir, true); } catch { }
            }
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
                // The host is stopped with the conductor but can take a moment to exit (this flaked under load).
                for (int i = 0; i < 10 && Process.GetProcessesByName("powershell").Length > powerShellsBefore; i++) await Task.Delay(500);
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
