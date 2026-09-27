
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    [MessagePackObject]
    public class DnaManifest
    {
        /// <summary>
        ///  Currently one "1" is supported
        /// </summary>
        [Key("manifest_version")]
        public string manifest_version { get; set; }

        /// <summary>
        /// The friendly "name" of a Holochain DNA.
        /// </summary>
        [Key("name")]
        public string name { get; set; }

        // NOTE (Holochain 0.7.0): network_seed and properties moved to IntegrityManifest.
        // Set them on DnaManifest.integrity.network_seed / .properties instead.
        // Kept here as commented-out reference for callers upgrading from pre-0.7.0.
        //[Key("network_seed")] public string network_seed { get; set; }
        //[Key("properties")]   public dynamic properties { get; set; }

        /// <summary>
        /// Integrity zomes container for this DNA (Holochain 0.7.0+). Single object wrapping
        /// an array of IntegrityZomeManifest entries (see IntegrityManifest.zomes).
        /// </summary>
        [Key("integrity")]
        public IntegrityManifest integrity { get; set; }

        /// <summary>
        /// Coordinator zomes container for this DNA (Holochain 0.7.0+). Single object wrapping
        /// an array of CoordinatorZomeManifest entries (see CoordinatorManifest.zomes).
        /// </summary>
        [Key("coordinator")]
        public CoordinatorManifest coordinator { get; set; }

        /// <summary>
        /// Legacy flat zomes array — retained for backwards compatibility and DNA bundles that
        /// predate the integrity/coordinator split. Prefer `integrity` + `coordinator` for 0.7.0+.
        /// </summary>
        [Key("zomes")]
        public ZomeManifest[] zomes { get; set; }
    }
}