
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// One coordinator zome entry inside CoordinatorManifest.zomes (Holochain 0.7.0).
    /// Mirrors holochain_types::dna::wasm::CoordinatorZomeManifest:
    ///
    /// pub struct CoordinatorZomeManifest {
    ///     pub name: ZomeName,
    ///     pub dylib: Option&lt;PathBuf&gt;,
    ///     pub hash: Option&lt;WasmHash&gt;,
    ///     pub dependencies: Option&lt;Vec&lt;ZomeName&gt;&gt;,  // plain strings, NOT ZomeDependency objects
    ///     pub bundled/path/url: (ZomeLocation),
    /// }
    /// Note: coordinator zome dependencies are plain ZomeName strings (not wrapped in ZomeDependency),
    /// unlike IntegrityZomeManifest which uses ZomeDependency objects.
    /// </summary>
    [MessagePackObject]
    public class CoordinatorZomeManifest
    {
        [Key("name")]
        public string name { get; set; }

        [Key("dylib")]
        public string dylib { get; set; }

        [Key("hash")]
        public string hash { get; set; }

        /// <summary>
        /// Names of integrity zomes this coordinator depends on.
        /// Wire format: plain strings (ZomeName), e.g. ["my_integrity_zome"].
        /// </summary>
        [Key("dependencies")]
        public string[] dependencies { get; set; }

        /// <summary>Expect file to be part of this bundle (ZomeLocation::Bundled).</summary>
        [Key("bundled")]
        public string bundled { get; set; }

        /// <summary>Get file from local filesystem (ZomeLocation::Path).</summary>
        [Key("path")]
        public string path { get; set; }

        /// <summary>Get file from URL (ZomeLocation::Url).</summary>
        [Key("url")]
        public string url { get; set; }
    }
}
