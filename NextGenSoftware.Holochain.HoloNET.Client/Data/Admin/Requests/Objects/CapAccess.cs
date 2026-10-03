using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Builds the wire form of holochain_integrity_types::capability::CapAccess.
    /// Rust: #[serde(tag = "type", content = "value", rename_all = "snake_case")]
    /// Wire:
    ///   {"type": "unrestricted"}
    ///   {"type": "transferable", "value": {"secret": bytes}}
    ///   {"type": "assigned",     "value": {"secret": bytes, "assignees": [agent_pub_key, ...]}}
    /// Verified against holochain-0.7.0 (and 0.6.1, which uses the same tagging).
    /// Supersedes CapGrantAccessUnrestricted/Transferable/Assigned, which emitted the
    /// externally-tagged {"Unrestricted": ...} form the conductor does not accept.
    /// </summary>
    public static class CapAccess
    {
        public static Dictionary<string, object> Unrestricted() =>
            new Dictionary<string, object> { { "type", "unrestricted" } };

        public static Dictionary<string, object> Transferable(byte[] secret) =>
            new Dictionary<string, object>
            {
                { "type", "transferable" },
                { "value", new Dictionary<string, object> { { "secret", secret } } }
            };

        public static Dictionary<string, object> Assigned(byte[] secret, byte[][] assignees) =>
            new Dictionary<string, object>
            {
                { "type", "assigned" },
                { "value", new Dictionary<string, object> { { "secret", secret }, { "assignees", assignees } } }
            };
    }
}
