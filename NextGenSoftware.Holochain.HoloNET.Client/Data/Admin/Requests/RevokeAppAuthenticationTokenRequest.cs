using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AdminRequest::RevokeAppAuthenticationToken
    /// (Holochain 0.6.1). The payload is the raw token itself:
    ///
    /// `pub type AppAuthenticationToken = Vec&lt;u8&gt;;`
    /// RevokeAppAuthenticationToken(AppAuthenticationToken)
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    /// </summary>
    [MessagePackObject]
    public class RevokeAppAuthenticationTokenRequest
    {
        [Key("token")]
        public byte[] token { get; set; }
    }
}
