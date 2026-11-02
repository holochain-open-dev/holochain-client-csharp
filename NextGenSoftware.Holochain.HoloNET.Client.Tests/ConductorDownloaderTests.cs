using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    public class ConductorDownloaderTests
    {
        [Fact]
        public void AssetNamesFollowHolochainsReleaseNaming()
        {
            string asset = HolochainConductorDownloader.AssetName("holochain");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.Equal("holochain-x86_64-pc-windows-msvc.exe", asset);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Assert.StartsWith("holochain-", asset);

            Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "hc.exe" : "hc", HolochainConductorDownloader.FileName("hc"));
        }

        [Fact]
        public void EveryDownloadableAssetHasAPinnedHash()
        {
            // The table ships inside the Client; the downloader refuses a file that does not match it.
            foreach (string version in new[] { "0.7.1", "0.7.0", "0.6.3", "0.5.3" })
                foreach (string asset in new[] { "holochain-x86_64-pc-windows-msvc.exe", "holochain-x86_64-unknown-linux-gnu", "holochain-aarch64-apple-darwin", "hc-aarch64-unknown-linux-gnu", "hc-x86_64-apple-darwin" })
                    Assert.Matches("^[0-9a-f]{64}$", HolochainConductorDownloader.KnownSha256(version, asset));

            Assert.Null(HolochainConductorDownloader.KnownSha256("9.9.9", "holochain-x86_64-unknown-linux-gnu"));
        }

        [Theory]
        [InlineData("../evil")]
        [InlineData("0.7.1/../../x")]
        [InlineData("")]
        public async Task ANonVersionIsRejectedBeforeAnythingIsWritten(string version)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => HolochainConductorDownloader.EnsureAsync("hc", version));
        }

        [Fact]
        public async Task AConductorAlreadyInTheFolderIsUsedWithoutDownloading()
        {
            string folder = Path.Combine(Path.GetTempPath(), "holonet-cond-" + Guid.NewGuid().ToString("N"));
            string previous = Environment.GetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER");
            Environment.SetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER", folder);

            try
            {
                string seeded = HolochainConductorDownloader.CachedPath("holochain", "0.7.1");
                Directory.CreateDirectory(Path.GetDirectoryName(seeded));
                File.WriteAllText(seeded, "an offline copy");

                Assert.Equal(seeded, await HolochainConductorDownloader.EnsureAsync("holochain", "0.7.1"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER", previous);
                Directory.Delete(folder, true);
            }
        }

        [SkippableFact]
        public async Task DownloadsAndChecksTheRealHcTool()
        {
            Skip.IfNot(Environment.GetEnvironmentVariable("HOLONET_TEST_DOWNLOAD") == "1", "Set HOLONET_TEST_DOWNLOAD=1 to download hc (about 8 MB) from the official Holochain release.");

            string folder = Path.Combine(Path.GetTempPath(), "holonet-cond-" + Guid.NewGuid().ToString("N"));
            string previous = Environment.GetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER");
            Environment.SetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER", folder);

            try
            {
                string path = await HolochainConductorDownloader.EnsureAsync("hc", "0.7.1");

                Assert.True(File.Exists(path));
                using (SHA256 sha = SHA256.Create())
                    Assert.Equal(HolochainConductorDownloader.KnownSha256("0.7.1", HolochainConductorDownloader.AssetName("hc")),
                        BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant());

                // A second call finds the cached copy.
                Assert.Equal(path, await HolochainConductorDownloader.EnsureAsync("hc", "0.7.1"));
                Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(path), "*.download"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER", previous);
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
            }
        }

        [Fact]
        public void EmbeddedPlatform_MatchesTheDownloadAssetForThisComputer()
        {
            string platform = HoloNETClientBase.CurrentEmbeddedPlatform();
            Assert.NotNull(platform);

            // win-x64 <-> x86_64-pc-windows-msvc, linux-arm64 <-> aarch64-unknown-linux-gnu, osx-x64 <-> x86_64-apple-darwin, and so on.
            string triple = HolochainConductorDownloader.PlatformTriple();
            string expected = platform switch
            {
                "win-x64" => "x86_64-pc-windows-msvc",
                "linux-x64" => "x86_64-unknown-linux-gnu",
                "linux-arm64" => "aarch64-unknown-linux-gnu",
                "osx-x64" => "x86_64-apple-darwin",
                "osx-arm64" => "aarch64-apple-darwin",
                _ => null
            };
            Assert.Equal(expected, triple);
        }

        [Fact]
        public void LightweightPackage_BundlesNoConductorForThisComputer()
        {
            // The tests build the lightweight Client: no Embedded package, so UseEmbedded falls back to the PATH / download on Linux and macOS.
            Assert.False(HoloNETClientBase.IsEmbeddedBundleForThisComputer());
        }
    }
}
