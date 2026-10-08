using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AppAuthenticationTokenIssued
    /// (Holochain 0.7.0):
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
        // Rust Vec<u8> without serde_bytes: an array of integers on the wire (confirmed live, 0.7.0).
        [Key("token")]
        [MessagePackFormatter(typeof(ByteArrayAsIntArrayFormatter))]
        public byte[] token { get; set; }

        [Key("expires_at")]
        public long? expires_at { get; set; }
    }
}
