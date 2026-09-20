using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AppAuthenticationTokenIssued
    /// (Holochain 0.6.1):
    ///
    /// pub struct AppAuthenticationTokenIssued {
    ///     pub token: AppAuthenticationToken, // Vec&lt;u8&gt;
    ///     pub expires_at: Option&lt;Timestamp&gt;,
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    /// </summary>
    [MessagePackObject]
    public class AppAuthenticationTokenIssuedResponse
    {
        [Key("token")]
        public byte[] token { get; set; }

        [Key("expires_at")]
        public long? expires_at { get; set; }
    }
}
