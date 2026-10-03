using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_types::app::DisableCloneCellPayload, used as the payload for
    /// AppRequest::DisableCloneCell (Holochain 0.7.0):
    ///
    /// pub struct DisableCloneCellPayload {
    ///     pub clone_cell_id: CloneCellId,  // enum: CloneId(CloneId) | DnaHash(DnaHash)
    /// }
    /// https://docs.rs/holochain_types/0.7.0/holochain_types/app/struct.DisableCloneCellPayload.html
    /// </summary>
    [MessagePackObject]
    public class DisableCloneCellRequest
    {
        /// <summary>
        /// The CloneCellId — either a clone id string (e.g. "role_name.0") or a CellId tuple.
        /// Wire (adjacently tagged): {"type": "clone_id", "value": "role.0"} or {"type": "dna_hash", "value": bytes}.
        /// Build with CloneCellId.FromCloneId / FromDnaHash.
        /// </summary>
        [Key("clone_cell_id")]
        public dynamic clone_cell_id { get; set; }
    }

    /// <summary>
    /// Mirrors AppRequest::EnableCloneCell's payload, EnableCloneCellPayload, which per the
    /// Holochain 0.7.0 docs is the same shape as DisableCloneCellPayload (a type alias / same
    /// CloneCellId wrapper).
    /// https://docs.rs/holochain_types/0.7.0/holochain_types/app/type.EnableCloneCellPayload.html
    /// </summary>
    [MessagePackObject]
    public class EnableCloneCellRequest
    {
        /// <summary>
        /// The CloneCellId — either a clone id string (e.g. "role_name.0") or a CellId tuple.
        /// Wire (adjacently tagged): {"type": "clone_id", "value": "role.0"} or {"type": "dna_hash", "value": bytes}.
        /// Build with CloneCellId.FromCloneId / FromDnaHash.
        /// </summary>
        [Key("clone_cell_id")]
        public dynamic clone_cell_id { get; set; }
    }
}
