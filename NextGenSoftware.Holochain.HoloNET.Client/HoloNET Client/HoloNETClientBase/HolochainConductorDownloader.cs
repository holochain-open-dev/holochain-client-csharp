using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Finds, downloads and caches the Holochain conductor (holochain) and its dev tool (hc) for this computer's operating system and processor, so an app
    /// does not have to bundle one or install Holochain first. Only the official stable releases at https://github.com/holochain/holochain/releases are used
    /// (never the "unstable" builds), and every download is checked against a SHA-256: the table that ships inside HoloNET for the versions it knows, or the
    /// digest GitHub publishes for that release asset. Files are cached in %APPDATA%/NextGenSoftware/HoloNET/&lt;version&gt;/ (the folder an embedded conductor is
    /// extracted to), so a conductor you copied there in advance is used as is and nothing is downloaded (offline machines).
    /// </summary>
    public static class HolochainConductorDownloader
    {
        public const string ReleaseDownloadUrl = "https://github.com/holochain/holochain/releases/download";
        public const string ReleaseApiUrl = "https://api.github.com/repos/holochain/holochain/releases/tags";

        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private static JObject _knownHashes;

        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>The platform part of Holochain's release asset names for this computer, e.g. x86_64-unknown-linux-gnu. Throws if Holochain publishes no binary for it.</summary>
        public static string PlatformTriple()
        {
            bool arm = RuntimeInformation.OSArchitecture == Architecture.Arm64;
            bool x64 = RuntimeInformation.OSArchitecture == Architecture.X64;

            if (IsWindows && x64)
                return "x86_64-pc-windows-msvc";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && (x64 || arm))
                return arm ? "aarch64-unknown-linux-gnu" : "x86_64-unknown-linux-gnu";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && (x64 || arm))
                return arm ? "aarch64-apple-darwin" : "x86_64-apple-darwin";

            throw new PlatformNotSupportedException($"Holochain publishes no conductor for {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture}). Point HoloNETDNA.FullPathToExternalHolochainConductorBinary at one you built.");
        }

        /// <summary>"holochain" or "hc", with ".exe" on Windows.</summary>
        public static string FileName(string tool) => IsWindows ? tool + ".exe" : tool;

        /// <summary>The release asset name for a tool on this computer, e.g. holochain-x86_64-unknown-linux-gnu or hc-x86_64-pc-windows-msvc.exe.</summary>
        public static string AssetName(string tool) => $"{tool}-{PlatformTriple()}{(IsWindows ? ".exe" : "")}";

        /// <summary>%APPDATA%/NextGenSoftware/HoloNET/&lt;version&gt;, the same folder an embedded conductor is extracted to.</summary>
        public static string CacheFolder(string version) =>
            Path.Combine(CacheRoot(), version);

        /// <summary>
        /// %APPDATA%/NextGenSoftware/HoloNET, or the folder in the HOLONET_CONDUCTOR_FOLDER environment variable when it is set (for example a read-only folder on an
        /// offline machine that already holds the binaries, laid out as &lt;folder&gt;/&lt;version&gt;/holochain and hc).
        /// </summary>
        public static string CacheRoot()
        {
            string custom = Environment.GetEnvironmentVariable("HOLONET_CONDUCTOR_FOLDER");
            return string.IsNullOrWhiteSpace(custom) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NextGenSoftware", "HoloNET") : custom;
        }

        /// <summary>Where a cached copy of a tool lives.</summary>
        public static string CachedPath(string tool, string version) => Path.Combine(CacheFolder(version), FileName(tool));

        /// <summary>The SHA-256 (lower-case hex) HoloNET ships for a release asset, or null for a version it does not know.</summary>
        public static string KnownSha256(string version, string assetName)
        {
            if (_knownHashes == null)
            {
                using (Stream stream = typeof(HolochainConductorDownloader).Assembly.GetManifestResourceStream("HoloNET.ConductorHashes.json"))
                {
                    if (stream == null)
                        return null;

                    using (StreamReader reader = new StreamReader(stream))
                        _knownHashes = JObject.Parse(reader.ReadToEnd());
                }
            }

            return _knownHashes[version]?[assetName]?.ToString();
        }

        /// <summary>
        /// Returns the path of the tool ("holochain" or "hc") for that Holochain version: the cached copy if there is one, otherwise it is downloaded
        /// (and checked) first. Throws if it cannot be downloaded or does not match its SHA-256.
        /// </summary>
        /// <param name="allowUnverified">Only for versions that have neither a pinned hash nor a digest on GitHub; the download is then not checked.</param>
        public static async Task<string> EnsureAsync(string tool, string version, bool allowUnverified = false, Action<string> log = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(version) || version.IndexOfAny(new[] { '/', '\\', ':' }) >= 0 || version.Contains(".."))
                throw new ArgumentException($"'{version}' is not a Holochain version number (for example 0.7.1).", nameof(version));

            string path = CachedPath(tool, version);

            if (File.Exists(path))
                return path;

            await _lock.WaitAsync(cancellationToken);

            try
            {
                if (File.Exists(path))
                    return path;

                string asset = AssetName(tool);
                string url = $"{ReleaseDownloadUrl}/holochain-{version}/{asset}";
                Directory.CreateDirectory(CacheFolder(version));
                string temp = path + "." + Guid.NewGuid().ToString("N").Substring(0, 8) + ".download";

                using (HttpClient http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
                {
                    http.DefaultRequestHeaders.UserAgent.ParseAdd("HoloNET");
                    string expected = KnownSha256(version, asset) ?? await DigestFromGitHubAsync(http, version, asset, cancellationToken);

                    if (expected == null && !allowUnverified)
                        throw new InvalidOperationException($"HoloNET has no SHA-256 for {asset} in Holochain {version}, so it will not download it. Set HoloNETDNA.AllowUnverifiedConductorDownload to true to download it unchecked, or put the file at {path}.");

                    log?.Invoke($"Downloading Holochain {version} {asset} (first run on this computer; it is kept in {CacheFolder(version)})...");

                    try
                    {
                        string actual;

                        using (HttpResponseMessage response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                        {
                            if (!response.IsSuccessStatusCode)
                                throw new InvalidOperationException($"Could not download {url}: HTTP {(int)response.StatusCode}. Holochain {version} may not have a release binary for this platform; point HoloNETDNA.FullPathToExternalHolochainConductorBinary at one, or copy it to {path}.");

                            using (Stream source = await response.Content.ReadAsStreamAsync())
                            using (FileStream target = new FileStream(temp, FileMode.CreateNew, FileAccess.Write))
                            using (SHA256 sha = SHA256.Create())
                            {
                                byte[] buffer = new byte[81920];
                                int read;

                                while ((read = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                                {
                                    sha.TransformBlock(buffer, 0, read, null, 0);
                                    await target.WriteAsync(buffer, 0, read, cancellationToken);
                                }

                                sha.TransformFinalBlock(new byte[0], 0, 0);
                                actual = BitConverter.ToString(sha.Hash).Replace("-", "").ToLowerInvariant();
                            }
                        }

                        if (expected != null && !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException($"The downloaded {asset} ({version}) does not match its SHA-256 (expected {expected}, got {actual}), so it was discarded.");

                        MakeExecutable(temp);

                        if (File.Exists(path))
                            File.Delete(temp);   // another process finished first
                        else
                            File.Move(temp, path);
                    }
                    finally
                    {
                        if (File.Exists(temp))
                            File.Delete(temp);
                    }
                }

                log?.Invoke($"Holochain {version} {asset} is ready.");
                return path;
            }
            finally
            {
                _lock.Release();
            }
        }

        private static async Task<string> DigestFromGitHubAsync(HttpClient http, string version, string assetName, CancellationToken cancellationToken)
        {
            try
            {
                using (HttpResponseMessage response = await http.GetAsync($"{ReleaseApiUrl}/holochain-{version}", cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                        return null;

                    JObject release = JObject.Parse(await response.Content.ReadAsStringAsync());

                    foreach (JToken asset in release["assets"] ?? new JArray())
                    {
                        if (asset["name"]?.ToString() == assetName)
                        {
                            string digest = asset["digest"]?.ToString();
                            return digest != null && digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) ? digest.Substring(7).ToLowerInvariant() : null;
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
            }

            return null;
        }

        internal static void MakeExecutable(string path)
        {
            if (IsWindows)
                return;

            using (Process chmod = Process.Start(new ProcessStartInfo("chmod", $"755 \"{path}\"") { UseShellExecute = false, CreateNoWindow = true }))
                chmod.WaitForExit(10000);
        }
    }
}
