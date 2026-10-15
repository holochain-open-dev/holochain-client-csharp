using System.Collections.Generic;
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Pagination cursor for dump_full_state (holochain_conductor_api::state_dump::DhtOpsCursor, Holochain 0.7.0 and 0.7.1):
    /// pass the previous page's <see cref="FullIntegrationStateDump.dht_ops_cursor"/> to get the next page.
    /// </summary>
    [MessagePackObject]
    public class DhtOpsCursor
    {
        /// <summary>Microsecond received timestamp of the last op selected.</summary>
        [Key("when_received")]
        public long when_received { get; set; }

        /// <summary>Hash of the last op selected (tie-breaks ops sharing a timestamp).</summary>
        [Key("hash")]
        public byte[] hash { get; set; }
    }

    /// <summary>
    /// Pagination cursor for dump_state (holochain_state_types::SourceChainCursor, an externally tagged enum):
    /// start after a sequence number or after an action.
    /// </summary>
    public static class SourceChainCursor
    {
        public static object Sequence(uint actionSeq) => new Dictionary<string, object> { ["Sequence"] = actionSeq };

        public static object ActionHash(byte[] actionHash) => new Dictionary<string, object> { ["ActionHash"] = actionHash };
    }
}
