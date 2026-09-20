using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_zome_types::capability::CapGrantInfo (Holochain 0.7.0):
    ///
    /// pub struct CapGrantInfo {
    ///     pub cap_grant: DesensitizedZomeCallCapGrant,
    ///     pub action_hash: ActionHash,
    ///     pub created_at: Timestamp,
    ///     pub revoked_at: Option&lt;Timestamp&gt;,
    /// }
    /// https://docs.rs/holochain_zome_types/0.7.0/holochain_zome_types/capability/struct.CapGrantInfo.html
    ///
    /// NOTE: `DesensitizedZomeCallCapGrant` itself was not verified field-by-field against the
    /// Rust source (could not locate the authoritative definition at time of writing); it is
    /// represented here as `dynamic` so the rest of the structure can still round-trip via
    /// MessagePack without guessing at its internal shape.
    /// </summary>
    [MessagePackObject]
    public class CapGrantInfo
    {
        [Key("cap_grant")]
        public dynamic cap_grant { get; set; }

        [Key("action_hash")]
        public byte[] action_hash { get; set; }

        [Key("created_at")]
        public long created_at { get; set; }

        [Key("revoked_at")]
        public long? revoked_at { get; set; }
    }

    /// <summary>
    /// Mirrors holochain_conductor_api::AppCapGrantInfo (Holochain 0.7.0), a tuple struct:
    /// `pub struct AppCapGrantInfo(pub Vec&lt;(CellId, Vec&lt;CapGrantInfo&gt;)&gt;);`
    /// https://docs.rs/holochain_zome_types/0.7.0/holochain_zome_types/capability/struct.AppCapGrantInfo.html
    /// (Vec is used instead of a map because tuple keys are problematic with msgpack encoding.)
    /// </summary>
    [MessagePackObject]
    public class CellCapGrantInfo
    {
        [Key(0)]
        public CellId cell_id { get; set; }

        [Key(1)]
        public CapGrantInfo[] cap_grants { get; set; }
    }
}
