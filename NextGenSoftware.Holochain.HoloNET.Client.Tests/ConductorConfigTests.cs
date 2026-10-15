using System.Linq;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>SetConductorConfigAdminPort against the conductor-config.yaml Holochain 0.7.1 generates (and variants).</summary>
    public class ConductorConfigTests
    {
        // Exactly what holochain --create-config writes (0.7.1), paths shortened.
        private const string Generated =
            "tracing_override: null\n" +
            "wasm_backend: null\n" +
            "data_root_path: C:\\tmp\\x\n" +
            "keystore:\n" +
            "  type: lair_server_in_proc\n" +
            "  lair_root: C:\\tmp\\x\\ks\n" +
            "admin_interfaces:\n" +
            "- driver:\n" +
            "    type: websocket\n" +
            "    port: 0\n" +
            "    danger_bind_addr: null\n" +
            "    allowed_origins: '*'\n" +
            "network:\n" +
            "  bootstrap_url: https://dev-test-bootstrap2.holochain.org/\n" +
            "  request_timeout_s: 60\n" +
            "  report: none\n" +
            "  advanced: null\n" +
            "db_sync_level: Normal\n";

        [Fact]
        public void GeneratedConfig_OnlyTheAdminPortChanges()
        {
            string updated = HoloNETClientBase.SetConductorConfigAdminPort(Generated, 65000);
            Assert.Contains("    port: 65000", updated);
            Assert.Contains("  report: none", updated);
            Assert.Equal(Generated.Split('\n').Length, updated.Split('\n').Length);
        }

        [Theory]
        [InlineData("admin_interfaces: null")]
        [InlineData("admin_interfaces: []")]
        [InlineData("admin_interfaces: ~")]
        public void NullOrEmptyAdminInterfaces_GetAWebsocketInterface_AndReportIsUntouched(string emptyForm)
        {
            string config = Generated.Replace("admin_interfaces:\n- driver:\n    type: websocket\n    port: 0\n    danger_bind_addr: null\n    allowed_origins: '*'\n", emptyForm + "\n");
            Assert.DoesNotContain("port:", config.Replace("report:", ""));
            string updated = HoloNETClientBase.SetConductorConfigAdminPort(config, 65001);
            Assert.Contains("- driver:\n    type: websocket\n    port: 65001", updated);
            Assert.Contains("  report: none", updated);
        }

        [Fact]
        public void MissingAdminInterfaces_AreAppended()
        {
            string config = string.Join("\n", Generated.Split('\n').Where(l => !l.StartsWith("admin_interfaces") && !l.StartsWith("- driver") && !l.StartsWith("    ")));
            string updated = HoloNETClientBase.SetConductorConfigAdminPort(config, 65002);
            Assert.Contains("admin_interfaces:\n- driver:\n    type: websocket\n    port: 65002", updated);
        }

        [Fact]
        public void AdminBlockWithoutAPort_IsReplaced()
        {
            string config = Generated.Replace("    port: 0\n", "");
            string updated = HoloNETClientBase.SetConductorConfigAdminPort(config, 65003);
            Assert.Contains("    port: 65003", updated);
            Assert.Single(updated.Split('\n'), l => l == "admin_interfaces:");
        }

        [Fact]
        public void WindowsLineEndings_ArePreserved()
        {
            string updated = HoloNETClientBase.SetConductorConfigAdminPort(Generated.Replace("\n", "\r\n"), 65004);
            Assert.Contains("    port: 65004\r\n", updated);
            Assert.DoesNotContain("\r\r", updated);
        }
    }
}
