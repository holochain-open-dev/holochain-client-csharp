using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Connect overloads / failures, and conductor-start branches that resolve paths without launching anything.</summary>
    public class ConnectAndStartBranchTests
    {
        private static readonly Type[] Clients = { typeof(HoloNETClientAdmin), typeof(HoloNETClientAppAgent), typeof(HoloNETClientApp) };

        public static IEnumerable<object[]> ConnectOverloads() => Clients.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == "Connect" || m.Name == "ConnectAsync")
            .Select(m => new object[] { t.Name, string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) + "|" + m.Name + "|" + m.DeclaringType.Name }));

        private static HoloNETClientBase NewClient(string type, string uri)
        {
            var dna = Dna(uri);
            dna.HolochainConductorAppAgentURI = uri;
            return type == nameof(HoloNETClientAdmin) ? new HoloNETClientAdmin(dna)
                 : type == nameof(HoloNETClientAppAgent) ? new HoloNETClientAppAgent("app", null, dna)
                 : (HoloNETClientBase)new HoloNETClientApp(dna);
        }

        /// <summary>Every public Connect / ConnectAsync overload connects (keys preset, so no retrieval is needed).</summary>
        [Theory]
        [MemberData(nameof(ConnectOverloads))]
        public async Task EveryConnectOverload_Connects(string clientType, string signature)
        {
            using var fake = new FakeConductor();
            fake.Respond = (t, v) => FakeConductor.Ok("app_info", FakeConductorFlowTests.AppInfo("app"));
            var client = NewClient(clientType, fake.Uri);
            client.OnError += (s, e) => { };
            client.HoloNETDNA.AgentPubKey = client.ConvertHoloHashToString(Agent());
            client.HoloNETDNA.DnaHash = client.ConvertHoloHashToString(Dna());
            var parts = signature.Split('|');
            var method = Clients.Single(t => t.Name == clientType).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == parts[1] && m.DeclaringType.Name == parts[2] && string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name)) == parts[0]);
            object[] args = method.GetParameters().Select<ParameterInfo, object>(p =>
                p.ParameterType == typeof(Uri) ? new Uri(fake.Uri)
                : p.ParameterType == typeof(string) ? (p.Name.Contains("App") ? "app" : fake.Uri)
                : p.ParameterType == typeof(ConnectedCallBackMode) ? ConnectedCallBackMode.WaitForHolochainConductorToConnect
                : p.ParameterType == typeof(RetrieveAgentPubKeyAndDnaHashMode) ? RetrieveAgentPubKeyAndDnaHashMode.DoNotRetreive
                : p.ParameterType == typeof(bool) ? false
                : p.HasDefaultValue ? p.DefaultValue : null).ToArray();

            object r = method.Invoke(client, args);
            var result = r is Task<HoloNETConnectedEventArgs> task ? await task : (HoloNETConnectedEventArgs)r;
            Assert.True(result.IsConnected, signature + ": " + result.Message);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task Connect_NoUriAnywhere_NoLogProviders_InvalidUri()
        {
            var admin = new HoloNETClientAdmin(Dna(""));
            admin.OnError += (s, e) => { };
            admin.EndPoint = null;
            Assert.True((await admin.ConnectAsync("")).IsError, "no URI");

            using var fake = new FakeConductor();
            var silent = new HoloNETClientAdmin(Dna(fake.Uri));
            silent.OnError += (s, e) => { };
            silent.Logger.LogProviders.Clear();
            Assert.True((await silent.ConnectAsync(fake.Uri)).IsError, "no log providers");

            var bad = new HoloNETClientAdmin(Dna(fake.Uri));
            bad.OnError += (s, e) => { };
            Assert.True((await bad.ConnectAsync("not a uri")).IsError, "invalid URI");

            // No URI passed: the client's own EndPoint is used.
            var byEndPoint = new HoloNETClientAdmin(Dna(fake.Uri));
            byEndPoint.EndPoint = new Uri(fake.Uri);
            var viaEndPoint = await byEndPoint.ConnectAsync("");
            Assert.True(viaEndPoint.IsConnected, viaEndPoint.Message);
            await byEndPoint.DisconnectAsync();
        }

        /// <summary>
        /// External hc with "only one conductor": when a process with the hc file's name is already running (here the test
        /// host itself) HoloNET resolves the holochain to use and stops before launching anything.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExternalHc_ResolvesItsHolochain_ThenReusesTheRunningOne(bool explicitHolochain)
        {
            string running = Process.GetCurrentProcess().MainModule.FileName;
            string dir = Path.Combine(Path.GetTempPath(), "holonet-hc-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string hc = Path.Combine(dir, Path.GetFileName(running));
                File.WriteAllBytes(hc, new byte[] { 0 });
                File.WriteAllBytes(Path.Combine(dir, "holochain.exe"), new byte[] { 0 });

                var dna = Dna("ws://127.0.0.1:1");
                dna.HolochainConductorToUse = HolochainConductorEnum.HcDevTool;
                dna.HolochainConductorMode = HolochainConductorModeEnum.UseExternal;
                dna.FullPathToExternalHCToolBinary = hc;
                dna.FullPathToExternalHolochainConductorBinary = explicitHolochain ? Path.Combine(dir, "holochain.exe") : "";
                dna.OnlyAllowOneHolochainConductorToRunAtATime = true;
                var admin = new HoloNETClientAdmin(dna);
                bool started = false;
                admin.OnHolochainConductorStarted += (s, e) => started = true;
                await admin.StartHolochainConductorAsync();
                Assert.False(started, "a second conductor was launched");
            }
            finally { try { Directory.Delete(dir, true); } catch { } }
        }

        /// <summary>A "holochain" that can't create a config: the error is reported and nothing is launched (no stray PowerShell).</summary>
        [Fact]
        public async Task ConfigCreationFails_NothingIsLaunched()
        {
            string dir = Path.Combine(Path.GetTempPath(), "holonet-cfg-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                // hc.exe answers --create-config with an error and no "Created config at" line; it never listens.
                string notHolochain = Path.Combine(dir, "not-holochain.exe");
                File.Copy(Path.Combine(Path.GetDirectoryName(typeof(HoloNETClientBase).Assembly.Location), "..", "..", "..", "..", "NextGenSoftware.Holochain.HoloNET.Client", "Resources", "hc.exe"), notHolochain);
                var dna = Dna("ws://127.0.0.1:1");
                dna.HolochainConductorToUse = HolochainConductorEnum.HolochainProductionConductor;
                dna.HolochainConductorMode = HolochainConductorModeEnum.UseExternal;
                dna.FullPathToExternalHolochainConductorBinary = notHolochain;
                dna.HolochainConductorConfigPath = Path.Combine(dir, "conductor-config.yaml");
                var admin = new HoloNETClientAdmin(dna);
                var errors = new List<string>();
                admin.OnError += (s, e) => errors.Add(e.Reason);
                int powerShells = Process.GetProcessesByName("powershell").Length;
                bool started = false;
                admin.OnHolochainConductorStarted += (s, e) => started = true;

                await admin.StartHolochainConductorAsync();
                Assert.False(started);
                Assert.Contains(errors, e => e.Contains("Created config at") || e.Contains("no conductor config"));
                Assert.True(Process.GetProcessesByName("powershell").Length <= powerShells, "a PowerShell host was started without a config");
            }
            finally { try { Directory.Delete(dir, true); } catch { } }
        }
    }
}
