namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// NOT VERIFIED / NOT WIRED against any real Holochain 0.7.0 conductor-facing config.
    ///
    /// Researched holochain_conductor_api's config module
    /// (https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor.rs
    /// and https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/config/conductor/paths.rs)
    /// and found only a `WASM_DIRECTORY` constant ("wasm") - the name of the subdirectory under
    /// the conductor's data_root_path where compiled wasm is cached on disk. There is no
    /// conductor-facing struct exposing WASM compilation/optimization/profiling/security/
    /// performance tuning to a remote client. The wasmer/witness compilation cache, JIT/AOT
    /// behaviour, sandboxing, etc. are entirely internal to the conductor process and not
    /// exposed over the admin/app websocket API that HoloNET talks to.
    ///
    /// A previous, unverified pass on this file invented an entire fabricated object graph
    /// (OptimizationLevel, EnableJIT/EnableAOT, congestion-style tuning, a WASMSecurityConfig with
    /// allowed/blocked imports, a WASMProfilingConfig, etc.) with no basis in the real API. That
    /// has been removed.
    ///
    /// This class is kept as a placeholder only so existing references / the HoloNETDNA object
    /// graph keep compiling. It is not sent anywhere and has no effect on conductor behaviour.
    /// </summary>
    public class WASMConfig
    {
        /// <summary>
        /// Placeholder only - not backed by any real Holochain conductor config field.
        /// Kept false by default to make clear this has no effect.
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}
