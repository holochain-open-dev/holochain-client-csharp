namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// NOT VERIFIED / NOT WIRED against any real Holochain 0.6.1 conductor-facing config.
    ///
    /// Researched holochain_conductor_api::config::conductor::NetworkConfig at
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs
    /// and there is NO client-configurable QUIC transport tuning surface (no version, max
    /// concurrent streams, congestion window, TLS settings, etc.). Low-level transport details
    /// (kitsune2/iroh) are either fixed internally or exposed only as the single opaque
    /// `advanced` JSON blob on NetworkConfig (see Kitsune2Config.AdvancedJson in this same
    /// directory; webrtc_config and signal_url were removed in Holochain 0.7.0) - not as a structured, per-field
    /// QUIC config object like this class previously fabricated (Version = "1",
    /// MaxConcurrentStreams, MaxStreamData, congestion window tuning, a nested TLSConfig with
    /// certificate file paths, etc.). None of those fields have a real Rust counterpart in the
    /// conductor-facing API.
    ///
    /// This class is kept as a placeholder only so existing references / the HoloNETDNA object
    /// graph keep compiling. Its fields below are illustrative/unverified placeholders, not real
    /// Holochain config, and are not sent anywhere. Do not rely on them changing conductor
    /// behaviour. If real QUIC-level tuning is ever exposed by Holochain, replace this file's
    /// contents entirely based on the verified Rust struct at that time.
    /// </summary>
    public class QUICConfig
    {
        /// <summary>
        /// Placeholder only - not backed by any real Holochain conductor config field.
        /// Kept false by default to make clear this has no effect.
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}
