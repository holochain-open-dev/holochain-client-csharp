using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_types::app::ClonedCell, returned by AppResponse::CloneCellCreated and
    /// AppResponse::CloneCellEnabled (Holochain 0.7.0).
    ///
    /// Verified against holochain_conductor_api/src/app_interface.rs at holochain-0.7.0:
    ///   pub struct ClonedCell {
    ///     pub cell_id: CellId,
    ///     pub clone_id: CloneId,
    ///     pub original_dna_hash: DnaHash,
    ///     pub dna_modifiers: DnaModifiers,
    ///     pub name: String,
    ///     pub enabled: bool,
    ///   }
    /// All six fields match. dna_modifiers is kept as dynamic since DnaModifiers contains
    /// optional sub-fields (network_seed, properties, origin_time, quantum_time).
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
