using System;
using System.IO;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>IsConductorConfigReusable: a conductor config is reused only when the same conductor version wrote it and its data folder exists.</summary>
    public class ConductorConfigReuseTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "hn_cfgtest_" + Guid.NewGuid().ToString("N").Substring(0, 8));
        private readonly string _config;
        private readonly string _dataRoot;

        public ConductorConfigReuseTests()
        {
            _dataRoot = Path.Combine(_dir, "data");
            Directory.CreateDirectory(_dataRoot);
            _config = Path.Combine(_dir, "conductor-config.yml");
            File.WriteAllText(_config, $"tracing_override: null\ndata_root_path: {_dataRoot}\nadmin_interfaces: null\n");
            File.WriteAllText(HoloNETClientBase.ConductorConfigVersionMarkerPath(_config), "holochain 0.7.1\n");
        }

        public void Dispose() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }

        [Fact]
        public void SameVersionAndDataRoot_IsReused() => Assert.True(HoloNETClientBase.IsConductorConfigReusable(_config, "holochain 0.7.1"));

        [Fact]
        public void OtherConductorVersion_IsNotReused() => Assert.False(HoloNETClientBase.IsConductorConfigReusable(_config, "holochain 0.7.0"));

        [Fact]
        public void NoVersionMarker_IsNotReused()
        {
            File.Delete(HoloNETClientBase.ConductorConfigVersionMarkerPath(_config));
            Assert.False(HoloNETClientBase.IsConductorConfigReusable(_config, "holochain 0.7.1"));
        }

        [Fact]
        public void DeletedDataRoot_IsNotReused()
        {
            Directory.Delete(_dataRoot, true);
            Assert.False(HoloNETClientBase.IsConductorConfigReusable(_config, "holochain 0.7.1"));
        }

        [Fact]
        public void MissingConfigOrUnknownVersion_IsNotReused()
        {
            Assert.False(HoloNETClientBase.IsConductorConfigReusable(Path.Combine(_dir, "none.yml"), "holochain 0.7.1"));
            Assert.False(HoloNETClientBase.IsConductorConfigReusable(_config, null));
        }

        [Theory]
        [InlineData("data_root_path: C:\\tmp\\x\r\nkeystore:\n", "C:\\tmp\\x")]
        [InlineData("data_root_path: '/tmp/x'\n", "/tmp/x")]
        [InlineData("data_root_path: null\n", null)]
        [InlineData("keystore: null\n", null)]
        public void DataRootPath_IsRead(string config, string expected) => Assert.Equal(expected, HoloNETClientBase.GetConductorConfigDataRootPath(config));
    }
}
