using System.Collections.Generic;
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors holochain_conductor_api::state_dump::OpTimingsDump.
    /// One page of op timings for a DNA, ordered by (when_received, op_hash).
    /// New in Holochain 0.7.0.
    /// </summary>
    [MessagePackObject]
    public class OpTimingsDump
    {
        /// <summary>
        /// The op timing records in this page.
        /// </summary>
        [Key("timings")]
        public List<OpTimingDump> timings { get; set; }

        /// <summary>
        /// Cursor to pass to the next DumpOpTimings call to get the following page.
        /// Null when there are no more ops.
        /// </summary>
        [Key("cursor")]
        public OpTimingsCursor cursor { get; set; }
    }

    /// <summary>
    /// Mirrors holochain_conductor_api::state_dump::OpTimingDump.
    /// Lifecycle timings for one DHT op in the conductor's current arc for a DNA.
    /// </summary>
    [MessagePackObject]
    public class OpTimingDump
    {
        /// <summary>The hash of this DHT op.</summary>
        [Key("op_hash")]
        public byte[] op_hash { get; set; }

        /// <summary>Microsecond timestamp when the op was first received.</summary>
        [Key("when_received")]
        public long when_received { get; set; }

        /// <summary>Microsecond timestamp when the op was integrated, or null if not yet integrated.</summary>
        [Key("when_integrated")]
        public long? when_integrated { get; set; }

        /// <summary>Microsecond timestamp when validation was abandoned, or null if not abandoned.</summary>
        [Key("abandoned_at")]
        public long? abandoned_at { get; set; }

        /// <summary>The validation outcome, or null if validation has not completed.</summary>
        [Key("validation_status")]
        public string validation_status { get; set; }

        /// <summary>Whether this op was validated locally (true), fetched from the network (false), or validation is pending (null).</summary>
        [Key("locally_validated")]
        public bool? locally_validated { get; set; }
    }

    /// <summary>
    /// Mirrors holochain_conductor_api::state_dump::OpTimingsCursor.
    /// Exclusive pagination marker for OpTimingsDump using microsecond timestamps
    /// and op hashes to break ties.
    /// </summary>
    [MessagePackObject]
    public class OpTimingsCursor
    {
        /// <summary>Microsecond timestamp of the last op in the previous page.</summary>
        [Key("when_received")]
        public long when_received { get; set; }

        /// <summary>Hash of the last op in the previous page.</summary>
        [Key("hash")]
        public byte[] hash { get; set; }
    }
}
