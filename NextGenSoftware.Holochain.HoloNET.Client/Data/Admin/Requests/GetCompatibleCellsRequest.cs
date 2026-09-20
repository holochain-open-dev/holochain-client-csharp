using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AdminRequest::GetCompatibleCells
    /// (Holochain 0.7.0). The payload is a single DnaHash:
    ///
    /// GetCompatibleCells(DnaHash)
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    ///
    /// NOTE: This variant is gated behind the `unstable-migration` feature flag in Holochain
    /// 0.7.0 and so may not be available on all conductor builds.
    /// </summary>
    [MessagePackObject]
    public class GetCompatibleCellsRequest
    {
        [Key("dna_hash")]
        public byte[] dna_hash { get; set; }
    }
}
