
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Mirrors holochain_types::dna::dna_manifest::ZomeManifest (Holochain 0.7.0), used for both
    /// integrity and coordinator zomes. The Rust struct is deny_unknown_fields:
    ///
    /// pub struct ZomeManifest {
    ///     pub name: ZomeName,
    ///     pub hash: Option&lt;WasmHashB64&gt;,
    ///     pub path: String,
    ///     pub dependencies: Option&lt;Vec&lt;ZomeDependency&gt;&gt;,
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_types/src/dna/dna_manifest/dna_manifest_v0.rs
    /// </summary>
    [MessagePackObject]
    public class ZomeManifest
    {
        [Key("name")]
        public string name { get; set; }

        [Key("hash")]
        public string hash { get; set; }

        /// <summary>Path to the zome's .wasm, relative to the manifest. Required.</summary>
        [Key("path")]
        public string path { get; set; }

        [Key("dependencies")]
        public ZomeDependency[] dependencies { get; set; }

        // 0.7.0 replaced the bundled/path/url ZomeLocation with a single required `path`, and
        // ZomeManifest is deny_unknown_fields, so sending these keys makes the conductor reject the manifest.
        //[Key("bundled")] public string bundled { get; set; }
        //[Key("url")]     public string url { get; set; }
    }
}
