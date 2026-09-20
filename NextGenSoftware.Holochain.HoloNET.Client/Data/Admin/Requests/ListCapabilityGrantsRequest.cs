using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AdminRequest::ListCapabilityGrants
    /// (Holochain 0.7.0):
    ///
    /// ListCapabilityGrants {
    ///     installed_app_id: String,
    ///     include_revoked: bool,
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    /// </summary>
    [MessagePackObject]
    public class ListCapabilityGrantsRequest
    {
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("include_revoked")]
        public bool include_revoked { get; set; }
    }
}
