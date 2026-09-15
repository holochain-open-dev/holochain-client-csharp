namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors the Kitsune2-related fields of holochain_conductor_api::config::conductor::NetworkConfig
    /// from Holochain 0.7.0. In 0.7.0 the tx5/WebRTC transport was removed entirely; iroh (QUIC)
    /// is now the sole network backend. Consequently signal_url and webrtc_config fields were
    /// dropped from NetworkConfig, and relay_url is now the primary non-bootstrap server URL.
    ///
    /// Verified against:
    /// https://github.com/holochain/holochain/blob/main-0.7/crates/holochain_conductor_api/src/config/conductor.rs
    /// </summary>
    public class Kitsune2Config
    {
        /// <summary>
        /// The Kitsune2 bootstrap server to use for WAN discovery.
        /// Mirrors NetworkConfig.bootstrap_url: url2::Url2.
        /// </summary>
        public string BootstrapUrl { get; set; } = "https://dev-test-bootstrap2.holochain.org";

        /// <summary>
        /// The iroh relay server address. In Holochain 0.7.0 this replaced signal_url/WebRTC;
        /// the iroh bootstrap server now bundles a relay, so a separate relay binary is no longer
        /// needed. Mirrors NetworkConfig.relay_url: url2::Url2.
        /// </summary>
        public string RelayUrl { get; set; } = "https://use1-1.relay.n0.iroh-canary.iroh.link./";

        /// <summary>
        /// The target arc factor applied when receiving hints from kitsune2. Leave at 1 in normal
        /// operation; set to 0 for leecher nodes that do not contribute to gossip.
        /// Mirrors NetworkConfig.target_arc_factor: u32 (default 1).
        /// </summary>
        public uint TargetArcFactor { get; set; } = 1;

        /// <summary>
        /// Advanced escape hatch for directly configuring kitsune2 (raw JSON), bypassing the
        /// higher-level fields above. Mirrors NetworkConfig.advanced: Option&lt;serde_json::Value&gt;.
        /// The Rust docs explicitly warn: "use only if you know what you are doing".
        /// Not currently wired through to any websocket/conductor call by HoloNET.
        /// </summary>
        public string AdvancedJson { get; set; } = null;
    }
}
