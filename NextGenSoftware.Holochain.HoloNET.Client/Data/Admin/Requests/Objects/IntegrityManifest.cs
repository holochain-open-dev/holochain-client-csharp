
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// The integrity-zomes container inside DnaManifest (Holochain 0.7.0).
    /// Mirrors holochain_types::dna::wasm::IntegrityManifest:
    ///
    /// pub struct IntegrityManifest {
    ///     pub zomes: Vec&lt;IntegrityZomeManifest&gt;,
    /// }
    ///
    /// Each individual integrity zome entry is represented by IntegrityZomeManifest.
    /// The per-zome fields (name, hash, bundled, path, url, dependencies, properties)
    /// that were previously on this class have been moved to IntegrityZomeManifest.
    ///
    // NOTE: the previous revision of this class modelled an individual integrity zome entry
    // (IntegrityZomeManifest) rather than the container — those fields are kept below as
    // commented-out reference but should not be used; use IntegrityZomeManifest instead.
    /// </summary>
    [MessagePackObject]
    public class IntegrityManifest
    {
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
        //[Key("properties")] public dynamic properties { get; set; }
    }
}
