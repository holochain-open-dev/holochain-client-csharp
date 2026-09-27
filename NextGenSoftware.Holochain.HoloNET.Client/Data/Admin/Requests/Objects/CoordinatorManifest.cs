
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// The coordinator-zomes container inside DnaManifest (Holochain 0.7.0).
    /// Mirrors holochain_types::dna::wasm::CoordinatorManifest:
    ///
    /// pub struct CoordinatorManifest {
    ///     pub zomes: Vec&lt;CoordinatorZomeManifest&gt;,
    /// }
    ///
    /// Each individual coordinator zome entry is represented by CoordinatorZomeManifest.
    /// Note: coordinator zome dependencies are plain ZomeName strings, unlike integrity
    /// zome dependencies which are ZomeDependency objects.
    ///
    // NOTE: previous revision used ZomeManifest[] (ZomeDependency[] for dependencies);
    // coordinator zomes actually use Vec&lt;ZomeName&gt; (plain strings). Updated to CoordinatorZomeManifest[].
    // [Key("zomes")] public ZomeManifest[] zomes { get; set; }  // ← old, incorrect type
    /// </summary>
    [MessagePackObject]
    public class CoordinatorManifest
    {
        [Key("zomes")]
        public CoordinatorZomeManifest[] zomes { get; set; }
    }
}