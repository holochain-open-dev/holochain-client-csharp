
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors holochain_types::app::InstallAppPayload (Holochain 0.7.0):
    ///
    /// pub struct InstallAppPayload {
    ///     pub source: AppBundleSource,              // {"type": "path" | "bytes", "value": ...}
    ///     pub agent_key: Option&lt;AgentPubKey&gt;,
    ///     pub installed_app_id: Option&lt;InstalledAppId&gt;,
    ///     pub network_seed: Option&lt;NetworkSeed&gt;,
    ///     pub roles_settings: Option&lt;HashMap&lt;RoleName, RoleSettings&gt;&gt;,
    ///     pub ignore_genesis_failure: bool,
    ///     pub restore_from_dht: bool,
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_types/src/app.rs
    /// </summary>
    [MessagePackObject]
    public class InstallAppRequest
    {
        /// <summary>Build with InstallAppRequest.SourceFromPath / SourceFromBytes.</summary>
        [Key("source")]
        public Dictionary<string, object> source { get; set; }

        [Key("agent_key")]
        public byte[] agent_key { get; set; }

        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("network_seed")]
        public string network_seed { get; set; }

        /// <summary>Per-role settings; build with InstallAppRequest.RolesSettingsFromMembraneProofs.</summary>
        [Key("roles_settings")]
        public Dictionary<string, object> roles_settings { get; set; }

        [Key("ignore_genesis_failure")]
        public bool ignore_genesis_failure { get; set; }

        [Key("restore_from_dht")]
        public bool restore_from_dht { get; set; }

        // Pre-0.5 shape. The conductor requires `source` and ignores these keys, so installs failed
        // with a missing-field error. Kept for reference; use source / roles_settings instead.
        //[Key("path")]            public string path { get; set; }
        //[Key("bundle")]          public AppBundle bundle { get; set; }
        //[Key("membrane_proofs")] public Dictionary<string, byte[]> membrane_proofs { get; set; }

        /// <summary>AppBundleSource::Path — a .happ path on the conductor's filesystem.</summary>
        public static Dictionary<string, object> SourceFromPath(string happPath) =>
            new Dictionary<string, object> { { "type", "path" }, { "value", happPath } };

        /// <summary>AppBundleSource::Bytes — the raw contents of a .happ file.</summary>
        public static Dictionary<string, object> SourceFromBytes(byte[] happBytes) =>
            new Dictionary<string, object> { { "type", "bytes" }, { "value", happBytes } };

        /// <summary>
        /// Maps role name → membrane proof to RoleSettings::Provisioned
        /// ({"type": "provisioned", "value": {"membrane_proof", "modifiers", "init_properties"}}).
        /// </summary>
        public static Dictionary<string, object> RolesSettingsFromMembraneProofs(Dictionary<string, byte[]> membraneProofs)
        {
            if (membraneProofs == null || membraneProofs.Count == 0)
                return null;

            var settings = new Dictionary<string, object>();
            foreach (var proof in membraneProofs)
            {
                settings[proof.Key] = new Dictionary<string, object>
                {
                    { "type", "provisioned" },
                    { "value", new Dictionary<string, object> { { "membrane_proof", proof.Value }, { "modifiers", null }, { "init_properties", null } } }
                };
            }

            return settings;
        }
    }
}
