
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Mirrors holochain_types::dna::dna_manifest::CoordinatorManifest (Holochain 0.7.0).
    /// deny_unknown_fields:
    ///
    /// pub struct CoordinatorManifest {
    ///     pub zomes: Vec&lt;ZomeManifest&gt;,
    /// }
    /// Coordinator zomes use the same ZomeManifest as integrity zomes (ZomeDependency objects).
    /// </summary>
    [MessagePackObject]
    public class CoordinatorManifest
    {
        [Key("zomes")]
        public ZomeManifest[] zomes { get; set; }
    }
}
