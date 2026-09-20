using System.Collections.Generic;
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AdminResponse::PeerMetaInfo
    /// (also used for the equivalent AppResponse::PeerMetaInfo) in Holochain 0.7.0:
    ///
    /// PeerMetaInfo(BTreeMap&lt;DnaHash, BTreeMap&lt;String, PeerMetaInfo&gt;&gt;)
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    ///
    /// NOTE: The inner `kitsune2_api::PeerMetaInfo` struct's exact field shape could not be
    /// located/verified against an authoritative source at the time of writing, so each entry
    /// is represented as `dynamic` here rather than guessing its fields. Inspect RawJSONData /
    /// the raw msgpack payload if you need its exact contents until this can be confirmed.
    /// </summary>
    [MessagePackObject]
    public class PeerMetaInfoResponse
    {
        /// <summary>
        /// Outer key = hex/base64-ish string key for the DnaHash, inner dictionary keyed by
        /// peer Url string, value = opaque per-peer meta info (see NOTE above).
        /// </summary>
        [Key("dna_hash_peer_meta")]
        public Dictionary<string, Dictionary<string, dynamic>> dna_hash_peer_meta { get; set; }
    }
}
