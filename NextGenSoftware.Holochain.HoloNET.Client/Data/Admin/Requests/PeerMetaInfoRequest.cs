using System.Collections.Generic;
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AdminRequest::PeerMetaInfo
    /// (Holochain 0.7.0). Also used by the equivalent AppRequest::PeerMetaInfo variant on the
    /// app interface - same shape on both interfaces.
    ///
    /// PeerMetaInfo {
    ///     url: Url,
    ///     dna_hashes: Option&lt;Vec&lt;DnaHash&gt;&gt;,
    /// }
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/admin_interface.rs
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs
    /// </summary>
    [MessagePackObject]
    public class PeerMetaInfoRequest
    {
        [Key("url")]
        public string url { get; set; }

        [Key("dna_hashes")]
        public List<byte[]> dna_hashes { get; set; }
    }
}
