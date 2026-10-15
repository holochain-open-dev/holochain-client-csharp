
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class DumpStateRequest
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }

        /// <summary>Holochain 0.7 paging: a <see cref="SourceChainCursor"/> value, or null to start at the beginning.</summary>
        [Key("source_chain_cursor")]
        public object source_chain_cursor { get; set; }

        /// <summary>Maximum number of records per page (null = conductor default).</summary>
        [Key("limit")]
        public uint? limit { get; set; }
    }
}