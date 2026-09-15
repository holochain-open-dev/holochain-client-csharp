using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::admin_interface::AdminRequest::DumpOpTimings
    /// and holochain_conductor_api::app_interface::AppRequest::DumpOpTimings.
    /// New in Holochain 0.7.0. Reports, for each DHT op the conductor holds for a DNA,
    /// when the op was received, when it was integrated, or when validation was abandoned.
    /// Results are paginated; pass the cursor from a previous response to continue.
    /// https://github.com/holochain/holochain/blob/main-0.7/crates/holochain_conductor_api/src/admin_interface.rs
    /// </summary>
    [MessagePackObject]
    public class DumpOpTimingsRequest
    {
        /// <summary>
        /// The DNA whose DHT arc to dump op timings for.
        /// For the app API, the app must be running a cell of this DNA.
        /// </summary>
        [Key("dna_hash")]
        public byte[] dna_hash { get; set; }

        /// <summary>
        /// Pagination cursor from a previous DumpOpTimings call; only ops ordered strictly
        /// after this cursor are returned. Null starts from the beginning.
        /// </summary>
        [Key("cursor")]
        public OpTimingsCursor cursor { get; set; }

        /// <summary>
        /// Maximum number of ops to return. Must be greater than zero if set.
        /// </summary>
        [Key("limit")]
        public uint? limit { get; set; }
    }
}
