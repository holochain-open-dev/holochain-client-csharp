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
    }
}
