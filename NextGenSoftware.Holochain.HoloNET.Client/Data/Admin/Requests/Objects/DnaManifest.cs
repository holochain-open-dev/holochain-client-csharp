
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Mirrors holochain_types::dna::DnaManifest (Holochain 0.7.0), an internally tagged enum:
    /// #[serde(tag = "manifest_version")] enum DnaManifest { #[serde(rename = "0")] V0(DnaManifestV0) }
    ///
    /// DnaManifestV0 (deny_unknown_fields) {
    ///     name: String,
    ///     integrity: IntegrityManifest,
    ///     coordinator: CoordinatorManifest,   // #[serde(default)]
    ///     lineage: Vec&lt;DnaHashB64&gt;,          // only with the unstable-migration feature
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_types/src/dna/dna_manifest.rs
    /// </summary>
    [MessagePackObject]
    public class DnaManifest
    {
        /// <summary>The manifest version tag. Must be "0" for Holochain 0.7.0.</summary>
        [Key("manifest_version")]
        public string manifest_version { get; set; } = "0";

        /// <summary>The friendly "name" of a Holochain DNA.</summary>
        [Key("name")]
        public string name { get; set; }

        [Key("integrity")]
        public IntegrityManifest integrity { get; set; }

        [Key("coordinator")]
        public CoordinatorManifest coordinator { get; set; }

        // Removed from DnaManifest in 0.7.0: network_seed and properties live on IntegrityManifest,
        // and the flat zomes list is replaced by integrity/coordinator. DnaManifestV0 is
        // deny_unknown_fields, so sending any of these keys makes the conductor reject the manifest.
        //[Key("network_seed")] public string network_seed { get; set; }
        //[Key("properties")]   public dynamic properties { get; set; }
        //[Key("zomes")]        public ZomeManifest[] zomes { get; set; }
    }
}
