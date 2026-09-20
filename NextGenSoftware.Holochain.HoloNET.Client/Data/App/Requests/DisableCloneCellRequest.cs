using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_types::app::DisableCloneCellPayload, used as the payload for
    /// AppRequest::DisableCloneCell (Holochain 0.7.0):
    ///
    /// pub struct DisableCloneCellPayload {
    ///     pub clone_cell_id: CloneCellId,  // enum: CloneId(CloneId) | CellId(CellId)
    /// }
    /// https://docs.rs/holochain_types/0.7.0/holochain_types/app/struct.DisableCloneCellPayload.html
    /// </summary>
    [MessagePackObject]
    public class DisableCloneCellRequest
    {
        /// <summary>
        /// Either a clone id (string, e.g. "role_name.0") or a CellId (byte[][] / CellId tuple).
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
        [Key("clone_cell_id")]
        public dynamic clone_cell_id { get; set; }
    }
}
