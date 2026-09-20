using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AdminRequest::RevokeZomeCallCapability
    /// (Holochain 0.7.0):
    ///
    /// RevokeZomeCallCapability {
    ///     action_hash: ActionHash,
    ///     cell_id: CellId,
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    /// </summary>
    [MessagePackObject]
    public class RevokeZomeCallCapabilityRequest
    {
        [Key("action_hash")]
        public byte[] action_hash { get; set; }

        [Key("cell_id")]
        public CellId cell_id { get; set; }
    }
}
