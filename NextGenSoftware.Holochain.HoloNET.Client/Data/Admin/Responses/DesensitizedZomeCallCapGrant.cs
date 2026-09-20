using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_zome_types::capability::DesensitizedZomeCallCapGrant (Holochain 0.7.0)
    /// — the cap grant shape returned by the conductor with secrets removed:
    ///
    /// pub struct DesensitizedZomeCallCapGrant {
    ///     pub tag: String,
    ///     pub access: CapAccessInfo,
    ///     pub functions: GrantedFunctions,
    /// }
    /// https://docs.rs/holochain_zome_types/0.7.0/holochain_zome_types/capability/struct.DesensitizedZomeCallCapGrant.html
    /// </summary>
    [MessagePackObject]
    public class DesensitizedZomeCallCapGrant
    {
        [Key("tag")]
        public string tag { get; set; }

        [Key("access")]
        public CapAccessInfo access { get; set; }

        [Key("functions")]
        public GrantedFunctions functions { get; set; }
    }
}
