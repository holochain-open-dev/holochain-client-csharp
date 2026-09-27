
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Mirrors holochain_types::dna::wasm::IntegrityManifest (Holochain 0.7.0).
    /// In 0.7.0 the DNA manifest splits zomes into integrity and coordinator zomes.
    /// Integrity zomes carry the entry/link type definitions and optionally their
    /// own properties (Option&lt;YamlProperties&gt;); coordinator zomes may depend on them.
    ///
    /// pub struct IntegrityManifest {
    ///     pub name: ZomeName,
    ///     pub dylib: Option&lt;PathBuf&gt;,
    ///     pub hash: Option&lt;WasmHash&gt;,
    ///     pub dependencies: Option&lt;Vec&lt;ZomeDependency&gt;&gt;,
    ///     // ZomeLocation (bundled | path | url)
    ///     pub bundled: Option&lt;String&gt;,
    ///     pub path: Option&lt;PathBuf&gt;,
    ///     pub url: Option&lt;Url2&gt;,
    ///     pub properties: Option&lt;YamlProperties&gt;,
    /// }
    /// </summary>
    [MessagePackObject]
    public class IntegrityManifest
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
        /// Arbitrary YAML properties for this integrity zome. In Holochain 0.7.0 this lives on
        /// IntegrityManifest (not the top-level DnaManifest). Kept as dynamic because
        /// YamlProperties is essentially serde_yaml::Value — any JSON/YAML-compatible object.
        /// </summary>
        [Key("properties")]
        public dynamic properties { get; set; }
    }
}
