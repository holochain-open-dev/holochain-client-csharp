
using MessagePack;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Builds the wire form of holochain_integrity_types::capability::GrantedFunctions.
    /// Rust: #[serde(tag = "type", content = "value", rename_all = "snake_case")]
    /// Wire: {"type": "all"} or {"type": "listed", "value": [[zome, fn], ...]}
    /// Verified against holochain-0.7.0 (and 0.6.1, which uses the same tagging).
    /// A dictionary is used so the unit variant omits the "value" key entirely.
    /// </summary>
    [MessagePackObject]
    public class GrantedFunctions
    {
        [Key("functions")]
        public Dictionary<string, object> Functions { get; set; }

        public static GrantedFunctions All()
        {
            return new GrantedFunctions
            {
                // Previously {"All": null} (externally tagged) — the conductor expects adjacent tagging.
                Functions = new Dictionary<string, object> { { "type", "all" } }
            };
        }

        public static GrantedFunctions Listed(List<(string zome, string fn)> grants)
        {
            var list = new List<object[]>();
            foreach (var (zome, fn) in grants)
                list.Add(new object[] { zome, fn });

            return new GrantedFunctions
            {
                // Previously {"Listed": [...]} (externally tagged) — the conductor expects adjacent tagging.
                Functions = new Dictionary<string, object> { { "type", "listed" }, { "value", list } }
            };
        }
    }
}
