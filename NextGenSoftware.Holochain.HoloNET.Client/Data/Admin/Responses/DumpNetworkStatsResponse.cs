using System.Collections.Generic;
using Newtonsoft.Json;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Typed representation of the JSON returned by the Holochain admin `dump_network_stats`
    /// call (AdminResponse::NetworkStatsDumped(String) - the conductor returns a JSON-encoded
    /// string here, not a msgpack object, hence this is deserialized with Newtonsoft.Json from
    /// NetworkStatsDumpedCallBackEventArgs.NetworkStatsDumpJSON rather than via MessagePack).
    ///
    /// As of Holochain 0.7.0 this wraps kitsune2_api::transport::TransportStats per connection.
    /// https://docs.rs/kitsune2_api/latest/kitsune2_api/struct.TransportStats.html
    /// https://docs.rs/kitsune2_api/latest/kitsune2_api/struct.TransportConnectionStats.html
    /// </summary>
    public class DumpNetworkStatsResponse
    {
        /// <summary>
        /// The networking backend that is in use (e.g. "tx5", "mem", "webrtc").
        /// </summary>
        [JsonProperty("backend")]
        public string Backend { get; set; }

        /// <summary>
        /// The list of peer urls that this Kitsune2 instance can currently be reached at.
        /// </summary>
        [JsonProperty("peer_urls")]
        public List<string> PeerUrls { get; set; }

        /// <summary>
        /// The list of current transport connections and their stats.
        /// </summary>
        [JsonProperty("connections")]
        public List<TransportConnectionStats> Connections { get; set; }
    }

    /// <summary>
    /// Mirrors kitsune2_api/0.7.0).
    /// </summary>
    public class TransportConnectionStats
    {
        /// <summary>
        /// The public key of the remote peer.
        /// </summary>
        [JsonProperty("pub_key")]
        public string PubKey { get; set; }

        /// <summary>
        /// The message count sent on this connection.
        /// </summary>
        [JsonProperty("send_message_count")]
        public ulong SendMessageCount { get; set; }

        /// <summary>
        /// The bytes sent on this connection.
        /// </summary>
        [JsonProperty("send_bytes")]
        public ulong SendBytes { get; set; }

        /// <summary>
        /// The message count received on this connection.
        /// </summary>
        [JsonProperty("recv_message_count")]
        public ulong RecvMessageCount { get; set; }

        /// <summary>
        /// The bytes received on this connection.
        /// </summary>
        [JsonProperty("recv_bytes")]
        public ulong RecvBytes { get; set; }

        /// <summary>
        /// UNIX epoch timestamp in seconds when this connection was opened.
        /// </summary>
        [JsonProperty("opened_at_s")]
        public ulong OpenedAtS { get; set; }

        /// <summary>
        /// True if this connection has successfully upgraded to a direct peer connection.
        /// </summary>
        [JsonProperty("is_direct")]
        public bool IsDirect { get; set; }
    }
}
