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
                EmbeddedHolochainVersionString(HoloNETDNA.EmbeddedHolochainVersion), exe);

#if EMBEDDED
        protected byte[] EmbeddedHolochain() =>
            HoloNETDNA.EmbeddedHolochainVersion == EmbeddedHolochainVersion.V0_7_0 ? Properties.Resources.holochain_0_7_0 : Properties.Resources.holochain;

        protected byte[] EmbeddedHc() =>
            HoloNETDNA.EmbeddedHolochainVersion == EmbeddedHolochainVersion.V0_7_0 ? Properties.Resources.hc_0_7_0 : Properties.Resources.hc;
#endif
    }
}
