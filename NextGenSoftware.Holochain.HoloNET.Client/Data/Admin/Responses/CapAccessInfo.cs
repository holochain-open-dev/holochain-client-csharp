using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_zome_types::capability::CapAccessInfo (Holochain 0.7.0) — the
    /// desensitized (secret-free) summary of a CapAccess variant:
    ///
    /// pub struct CapAccessInfo {
    ///     pub access_type: String,                         // "Unrestricted" | "Transferable" | "Assigned"
    ///     pub assignees: Option&lt;BTreeSet&lt;AgentPubKey&gt;&gt;,    // Some only for Assigned variant
    /// }
    /// https://docs.rs/holochain_zome_types/0.7.0/holochain_zome_types/capability/struct.CapAccessInfo.html
    /// </summary>
    [MessagePackObject]
    public class CapAccessInfo
    {
        [Key("access_type")]
        public string access_type { get; set; }

        [Key("assignees")]
        public byte[][] assignees { get; set; }
    }
}
