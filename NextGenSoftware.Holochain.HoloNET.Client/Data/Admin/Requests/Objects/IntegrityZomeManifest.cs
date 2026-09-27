
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// One integrity zome entry inside IntegrityManifest.zomes (Holochain 0.7.0).
    /// Mirrors holochain_types::dna::wasm::IntegrityZomeManifest:
    ///
    /// pub struct IntegrityZomeManifest {
    ///     pub name: ZomeName,
    ///     pub dylib: Option&lt;PathBuf&gt;,
    ///     pub hash: Option&lt;WasmHash&gt;,
    ///     pub dependencies: Option&lt;Vec&lt;ZomeDependency&gt;&gt;,
    ///     pub bundled/path/url: (ZomeLocation),
    ///     pub properties: Option&lt;YamlProperties&gt;,
    /// }
    /// Note: dependencies here are ZomeDependency objects (name-wrapped), unlike
    /// CoordinatorZomeManifest which uses plain string ZomeName dependencies.
    /// </summary>
    [MessagePackObject]
    public class IntegrityZomeManifest
    {
        [Key("name")]
        public string name { get; set; }

        [Key("dylib")]
        public string dylib { get; set; }

        [Key("hash")]
        public string hash { get; set; }

        [Key("dependencies")]
        public ZomeDependency[] dependencies { get; set; }

        /// <summary>Expect file to be part of this bundle (ZomeLocation::Bundled).</summary>
        [Key("bundled")]
        public string bundled { get; set; }

        /// <summary>Get file from local filesystem (ZomeLocation::Path).</summary>
        [Key("path")]
        public string path { get; set; }

        /// <summary>Get file from URL (ZomeLocation::Url).</summary>
        [Key("url")]
        public string url { get; set; }

        /// <summary>
        /// Arbitrary YAML properties for this integrity zome (Option&lt;YamlProperties&gt;).
        /// Kept as dynamic because YamlProperties is serde_yaml::Value — any compatible object.
        /// </summary>
        [Key("properties")]
        public dynamic properties { get; set; }
    }
}
