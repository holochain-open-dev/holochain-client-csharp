namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors (the relevant part of) holochain_conductor_api::config::conductor::NetworkConfig
    /// from Holochain 0.6.1. In 0.5.x and earlier, request_timeout_s lived directly on
    /// ConductorConfig; in 0.6.1 it moved under ConductorConfig.network (NetworkConfig).
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs
    ///
    /// HoloNET uses this client-side to drive the default request/response timeout used for
    /// zome calls (see CallZomeOptions.TimeoutSeconds for a per-call override) and other
    /// websocket round-trips. It does not attempt to mirror every NetworkConfig field, only the
    /// one relevant to client request timeout behaviour.
    /// </summary>
    public class NetworkConfig
    {
        /// <summary>
        /// The amount of time, in seconds, to elapse before a request-response round trip
        /// times out. Mirrors Rust's `request_timeout_s: u64` (default 60 seconds in Holochain).
        /// </summary>
        public int RequestTimeoutS { get; set; } = 60;
    }
}
