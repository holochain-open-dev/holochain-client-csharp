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

        // Adjacently tagged: {"type": "all"} or {"type": "listed", "value": [[zome, fn], ...]}.
        // Was typed as GrantedFunctions, whose single "functions" key never matches the wire shape.
        [Key("functions")]
        public System.Collections.Generic.Dictionary<string, object> functions { get; set; }

        // Holochain 0.7.1 renamed this type DesensitizedCapGrant { tag, constraint, capability }:
        // `access` became `constraint` (same fields), and `functions` moved into
        // capability = {"type": "zome_call", "value": {"functions": ...}} or {"type": "direct_signal"}.
        // Both shapes decode here; read AccessInfo / Functions / CapabilityType rather than the raw fields.

        /// <summary>Holochain 0.7.1+: who may use the grant (0.7.0 sends this as <see cref="access"/>).</summary>
        [Key("constraint")]
        public CapAccessInfo constraint { get; set; }

        /// <summary>Holochain 0.7.1+: what the grant allows, adjacently tagged ("zome_call" or "direct_signal").</summary>
        [Key("capability")]
        public System.Collections.Generic.Dictionary<string, object> capability { get; set; }

        /// <summary>Access info on either version.</summary>
        [IgnoreMember]
        public CapAccessInfo AccessInfo => access ?? constraint;

        /// <summary>Granted functions on either version (null for a direct-signal grant).</summary>
        [IgnoreMember]
        public System.Collections.Generic.Dictionary<string, object> Functions
        {
            get
            {
                if (functions != null) return functions;
                if (capability != null && capability.TryGetValue("value", out object value) &&
                    value is System.Collections.Generic.IDictionary<object, object> zomeCall &&
                    zomeCall.TryGetValue("functions", out object f) && f is System.Collections.Generic.IDictionary<object, object> map)
                {
                    var result = new System.Collections.Generic.Dictionary<string, object>();
                    foreach (var kv in map) result[kv.Key.ToString()] = kv.Value;
                    return result;
                }
                return null;
            }
        }

        /// <summary>"zome_call" or "direct_signal" (0.7.1+); "zome_call" for 0.7.0 grants.</summary>
        [IgnoreMember]
        public string CapabilityType => capability != null && capability.TryGetValue("type", out object type) ? type as string : "zome_call";
    }
}
