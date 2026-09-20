using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_types::app::ClonedCell, returned by AppResponse::CloneCellCreated and
    /// AppResponse::CloneCellEnabled (Holochain 0.7.0).
    ///
    /// NOTE: Unlike most other classes added in this upgrade, the exact field list of
    /// `ClonedCell` for 0.6.1 could NOT be conclusively re-verified via docs.rs/GitHub source at
    /// the time of writing (searches for the type did not return a definitive hit). The shape
    /// below is carried over from the struct's well-established historical definition
    /// (cell_id, clone_id, original_dna_hash, dna_modifiers, name, enabled) which has been
    /// stable across Holochain releases - but please treat this one class as best-effort/
    /// unverified for 0.6.1 specifically and double check against a live conductor response if
    /// correctness here is critical.
    /// </summary>
    [MessagePackObject]
    public class ClonedCellResponse
    {
        [Key("cell_id")]
        public CellId cell_id { get; set; }

        [Key("clone_id")]
        public string clone_id { get; set; }

        [Key("original_dna_hash")]
        public byte[] original_dna_hash { get; set; }

        [Key("dna_modifiers")]
        public dynamic dna_modifiers { get; set; }

        [Key("name")]
        public string name { get; set; }

        [Key("enabled")]
        public bool enabled { get; set; }
    }
}
