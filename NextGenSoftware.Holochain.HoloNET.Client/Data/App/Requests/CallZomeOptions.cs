namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Per-call options for CallZomeFunctionAsync.
    ///
    /// NOTE: Unlike the other classes in this folder, this is NOT a verified 1:1 mirror of a
    /// Rust struct - a thorough search of the holochain_zome_types / holochain_conductor_api /
    /// holochain_types crates at tag holochain-0.7.0 did not turn up a `CallZomeOptions` struct
    /// on the wire protocol. The wire-level request (see <see cref="ZomeCallParamsSigned"/>) has
    /// no equivalent "options" field today. This class instead lets HoloNET callers configure
    /// client-side behaviour (currently: a per-call request timeout) without having to fall back
    /// to the global <see cref="NetworkConfig.RequestTimeoutS"/> default. If a genuine
    /// CallZomeOptions wire struct is added to Holochain in a future release, this class can be
    /// extended/mapped to it then.
    /// </summary>
    public class CallZomeOptions
    {
        /// <summary>
        /// Per-call override for how long (in seconds) HoloNET should wait for a response to
        /// this zome call before timing out. If null, falls back to
        /// HoloNETDNA.NetworkConfig.RequestTimeoutS (see NetworkConfig).
        /// </summary>
        public int? TimeoutSeconds { get; set; }

        public static CallZomeOptions Default => new CallZomeOptions();
    }
}
