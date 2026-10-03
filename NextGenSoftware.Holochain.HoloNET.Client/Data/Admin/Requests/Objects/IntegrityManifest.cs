
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Mirrors holochain_types::dna::dna_manifest::IntegrityManifest (Holochain 0.7.0).
    /// deny_unknown_fields:
    ///
    /// pub struct IntegrityManifest {
    ///     pub network_seed: Option&lt;String&gt;,
    ///     pub properties: Option&lt;YamlProperties&gt;,
    ///     pub zomes: Vec&lt;ZomeManifest&gt;,
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_types/src/dna/dna_manifest/dna_manifest_v0.rs
    /// </summary>
    [MessagePackObject]
    public class IntegrityManifest
    {
        /// <summary>A network seed for uniquifying this DNA.</summary>
        [Key("network_seed")]
        public string network_seed { get; set; }

        /// <summary>Arbitrary DNA properties (YamlProperties = serde_yaml::Value).</summary>
        [Key("properties")]
        public dynamic properties { get; set; }

        [Key("zomes")]
        public ZomeManifest[] zomes { get; set; }

        // origin_time and quantum_time are not part of the 0.7.0 IntegrityManifest; they were
        // added here from memory without verification and would be rejected (deny_unknown_fields).
        //[Key("origin_time")]  public string origin_time { get; set; }
        //[Key("quantum_time")] public dynamic quantum_time { get; set; }
    }
}
