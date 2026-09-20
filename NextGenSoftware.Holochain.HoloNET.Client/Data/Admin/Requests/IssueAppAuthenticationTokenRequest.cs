using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::IssueAppAuthenticationTokenPayload,
    /// used as the payload for AdminRequest::IssueAppAuthenticationToken (Holochain 0.6.1):
    ///
    /// pub struct IssueAppAuthenticationTokenPayload {
    ///     pub installed_app_id: InstalledAppId,
    ///     #[serde(default = "default_expiry_seconds")]
    ///     pub expiry_seconds: u64,
    ///     #[serde(default = "default_single_use")]
    ///     pub single_use: bool,
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    /// </summary>
    [MessagePackObject]
    public class IssueAppAuthenticationTokenRequest
    {
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("expiry_seconds")]
        public ulong expiry_seconds { get; set; }

        [Key("single_use")]
        public bool single_use { get; set; }
    }
}
