using System;
using System.IO;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientBase
    {
        /// <summary>
        /// Writes an embedded Holochain binary to disk when it is missing or differs in size from the
        /// bundled copy. Previously the file was only written when missing, so a conductor extracted by
        /// an older HoloNET release was used forever after an upgrade.
        /// </summary>
        protected static void ExtractEmbeddedBinary(string path, byte[] bytes)
        {
            if (File.Exists(path) && new FileInfo(path).Length == bytes.Length)
                return;

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(path, bytes);
        }

        /// <summary>Folder name for a bundled release, e.g. "0.7.1".</summary>
        public static string EmbeddedHolochainVersionString(EmbeddedHolochainVersion version) =>
            version == EmbeddedHolochainVersion.V0_7_0 ? "0.7.0" : "0.7.1";

        /// <summary>
        /// Where a bundled binary is extracted: %APPDATA%/NextGenSoftware/HoloNET/&lt;version&gt;/&lt;exe&gt;, one folder per
        /// release so switching versions doesn't overwrite the other.
        /// </summary>
        protected string EmbeddedBinaryPath(string exe) =>
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NextGenSoftware", "HoloNET",
                EmbeddedHolochainVersionString(EffectiveEmbeddedHolochainVersion()), exe);

        private static string EmbeddedResourceName(string exe, EmbeddedHolochainVersion version) =>
            $"HoloNET.Embedded.{exe}-{EmbeddedHolochainVersionString(version)}.exe";

        /// <summary>
        /// True when this build of HoloNET carries that conductor version: the Embedded package carries 0.7.1 and 0.7.0,
        /// Embedded.Holochain071 / Embedded.Holochain070 only one, and the lightweight package none.
        /// </summary>
        public static bool IsEmbeddedHolochainVersionBundled(EmbeddedHolochainVersion version) =>
            typeof(HoloNETClientBase).Assembly.GetManifestResourceInfo(EmbeddedResourceName("holochain", version)) != null;

        /// <summary>
        /// HoloNETDNA.EmbeddedHolochainVersion if this build bundles it, otherwise the version it does bundle (a
        /// single-version package asked for the other one gets the one it has, with a warning in the log).
        /// </summary>
        protected EmbeddedHolochainVersion EffectiveEmbeddedHolochainVersion()
        {
            EmbeddedHolochainVersion requested = HoloNETDNA.EmbeddedHolochainVersion;

            if (IsEmbeddedHolochainVersionBundled(requested))
                return requested;

            EmbeddedHolochainVersion other = requested == EmbeddedHolochainVersion.V0_7_0 ? EmbeddedHolochainVersion.V0_7_1 : EmbeddedHolochainVersion.V0_7_0;

            if (IsEmbeddedHolochainVersionBundled(other))
            {
                Logger.Log($"HoloNETDNA.EmbeddedHolochainVersion is {EmbeddedHolochainVersionString(requested)} but this HoloNET package only bundles Holochain {EmbeddedHolochainVersionString(other)}, so that is used.", NextGenSoftware.Logging.LogType.Warning);
                return other;
            }

            return requested;
        }

        private static byte[] ReadEmbeddedResource(string name)
        {
            using (Stream stream = typeof(HoloNETClientBase).Assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new InvalidOperationException($"This HoloNET package doesn't bundle {name}: use NextGenSoftware.Holochain.HoloNET.Client.Embedded (both versions) or the Embedded package for that version, or point HoloNET at your own conductor.");

                using (MemoryStream memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }

#if EMBEDDED
        // Were Properties.Resources.holochain / holochain_0_7_0 / hc / hc_0_7_0 (a .resx that always bundled both versions).
        //protected byte[] EmbeddedHolochain() =>
        //    HoloNETDNA.EmbeddedHolochainVersion == EmbeddedHolochainVersion.V0_7_0 ? Properties.Resources.holochain_0_7_0 : Properties.Resources.holochain;
        //protected byte[] EmbeddedHc() =>
        //    HoloNETDNA.EmbeddedHolochainVersion == EmbeddedHolochainVersion.V0_7_0 ? Properties.Resources.hc_0_7_0 : Properties.Resources.hc;
        protected byte[] EmbeddedHolochain() => ReadEmbeddedResource(EmbeddedResourceName("holochain", EffectiveEmbeddedHolochainVersion()));

        protected byte[] EmbeddedHc() => ReadEmbeddedResource(EmbeddedResourceName("hc", EffectiveEmbeddedHolochainVersion()));
#endif
    }
}
