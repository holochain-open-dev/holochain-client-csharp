
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// The integrity-zomes container inside DnaManifest (Holochain 0.7.0).
    /// Mirrors holochain_types::dna::wasm::IntegrityManifest:
    ///
    /// pub struct IntegrityManifest {
    ///     pub network_seed: Option&lt;NetworkSeed&gt;,
    ///     pub origin_time: HumanTimestamp,
    ///     pub quantum_time: Option&lt;Duration&gt;,
    ///     pub properties: Option&lt;YamlProperties&gt;,
    ///     pub zomes: Vec&lt;IntegrityZomeManifest&gt;,
    /// }
    ///
    /// Note: in 0.7.0, network_seed, origin_time, quantum_time and properties belong on this
    /// container, NOT on the top-level DnaManifest (they were moved here from DnaManifest).
    /// Each individual integrity zome entry is represented by IntegrityZomeManifest.
    ///
    // NOTE: an earlier revision of this class modelled an individual integrity zome entry
    // rather than the container — those per-zome fields are kept below as commented-out
    // reference but should not be used; use IntegrityZomeManifest instead.
    /// </summary>
    [MessagePackObject]
    public class IntegrityManifest
    {
        /// <summary>
        /// A network seed for uniquifying this DNA. Moved here from DnaManifest in 0.7.0.
        /// </summary>
        [Key("network_seed")]
        public string network_seed { get; set; }

        /// <summary>
        /// The time at which this DNA's integrity was first established on the network.
        /// Mirrors HumanTimestamp (RFC3339 string). Defaults to the Holochain epoch if null.
        /// </summary>
        [Key("origin_time")]
        public string origin_time { get; set; }

        /// <summary>
        /// Override for the quantum time resolution used by the DHT arc algorithm.
        /// Mirrors Option&lt;Duration&gt; — kept as dynamic (Duration is seconds + nanos struct).
        /// </summary>
        [Key("quantum_time")]
        public dynamic quantum_time { get; set; }

        /// <summary>
        /// Arbitrary YAML properties for this DNA's integrity layer. Moved here from
        /// DnaManifest in 0.7.0. Kept as dynamic (YamlProperties = serde_yaml::Value).
        /// </summary>
        [Key("properties")]
        public dynamic properties { get; set; }

        /// <summary>The list of integrity zomes for this DNA.</summary>
        [Key("zomes")]
        public IntegrityZomeManifest[] zomes { get; set; }

        // ── Fields from the previous (incorrect) revision — moved to IntegrityZomeManifest ──
        // These were per-zome entry fields, not container fields. Kept as reference only.
        //[Key("name")]      public string name { get; set; }
        //[Key("dylib")]     public string dylib { get; set; }
        //[Key("hash")]      public string hash { get; set; }
        //[Key("dependencies")] public ZomeDependency[] dependencies { get; set; }
        //[Key("bundled")]   public string bundled { get; set; }
        //[Key("path")]      public string path { get; set; }
        //[Key("url")]       public string url { get; set; }
    }
}
