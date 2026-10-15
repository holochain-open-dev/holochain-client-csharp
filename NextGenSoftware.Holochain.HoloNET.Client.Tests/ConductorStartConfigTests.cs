using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>StartHolochainConductorAsync rejects bad configuration before launching anything (no process is started).</summary>
    public class ConductorStartConfigTests
    {
        private static string Missing(string name) => Path.Combine(Path.GetTempPath(), "holonet-missing-" + Guid.NewGuid().ToString("N"), name);

        public static IEnumerable<object[]> BadConfigs()
        {
            yield return new object[] { "external holochain, no path", HolochainConductorEnum.HolochainProductionConductor, HolochainConductorModeEnum.UseExternal, "", "", "FullPathToExternalHolochainConductorBinary" };
            yield return new object[] { "external holochain, missing file", HolochainConductorEnum.HolochainProductionConductor, HolochainConductorModeEnum.UseExternal, Missing("holochain.exe"), "", "must point to a valid file" };
            yield return new object[] { "external hc, no path", HolochainConductorEnum.HcDevTool, HolochainConductorModeEnum.UseExternal, "", "", "FullPathToExternalHCToolBinary" };
            yield return new object[] { "external hc, missing file", HolochainConductorEnum.HcDevTool, HolochainConductorModeEnum.UseExternal, "", Missing("hc.exe"), "must point to a valid file" };
            yield return new object[] { "hc, missing root folder", HolochainConductorEnum.HcDevTool, HolochainConductorModeEnum.UseExternal, "", "root", "FullPathToRootHappFolder" };
            yield return new object[] { "hc, missing compiled folder", HolochainConductorEnum.HcDevTool, HolochainConductorModeEnum.UseExternal, "", "compiled", "FullPathToCompiledHappFolder" };
#if !EMBEDDED
            yield return new object[] { "embedded holochain without the Embedded package", HolochainConductorEnum.HolochainProductionConductor, HolochainConductorModeEnum.UseEmbedded, "", "", "Embedded NuGet package" };
            yield return new object[] { "embedded hc without the Embedded package", HolochainConductorEnum.HcDevTool, HolochainConductorModeEnum.UseEmbedded, "", "", "Embedded NuGet package" };
#endif
        }

        [Theory]
        [MemberData(nameof(BadConfigs))]
        public async Task BadConfiguration_IsReportedWithoutStartingAConductor(string scenario, HolochainConductorEnum conductor, HolochainConductorModeEnum mode, string holochainPath, string hcPath, string expected)
        {
            var dna = Dna("ws://127.0.0.1:1");
            dna.HolochainConductorToUse = conductor;
            dna.HolochainConductorMode = mode;
            dna.FullPathToExternalHolochainConductorBinary = holochainPath;
            string existingHc = typeof(ConductorStartConfigTests).Assembly.Location; // any existing file
            dna.FullPathToExternalHCToolBinary = hcPath == "root" || hcPath == "compiled" ? existingHc : hcPath;
            if (hcPath == "root") dna.FullPathToRootHappFolder = Missing("root");
            if (hcPath == "compiled") dna.FullPathToCompiledHappFolder = Missing("compiled");

            var admin = new HoloNETClientAdmin(dna);
            string reported = null;
            admin.OnError += (s, e) => reported = e.Reason;
            bool started = false;
            admin.OnHolochainConductorStarted += (s, e) => started = true;

            await admin.StartHolochainConductorAsync();
            Assert.False(started, scenario + ": a conductor was started");
            Assert.NotNull(reported);
            Assert.Contains(expected, reported);
        }

        [Fact]
        public void StartHolochainConductor_Sync_ReportsToo()
        {
            var dna = Dna("ws://127.0.0.1:1");
            dna.HolochainConductorMode = HolochainConductorModeEnum.UseExternal;
            dna.FullPathToExternalHolochainConductorBinary = "";
            var admin = new HoloNETClientAdmin(dna);
            string reported = null;
            admin.OnError += (s, e) => reported = e.Reason;
            admin.StartHolochainConductor();
            Assert.NotNull(reported);
        }
    }
}
