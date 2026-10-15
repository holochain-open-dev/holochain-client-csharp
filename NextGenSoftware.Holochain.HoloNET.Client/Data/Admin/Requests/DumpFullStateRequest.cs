
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class DumpFullStateRequest
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }

        // DhtOpsCursor is a struct { when_received, hash } in 0.7.0 and 0.7.1, not a number.
        //[Key("dht_ops_cursor")]
        //public int? dht_ops_cursor { get; set; }

        [Key("dht_ops_cursor")]
        public DhtOpsCursor dht_ops_cursor { get; set; }

        /// <summary>Maximum number of DHT ops per page (null = conductor default).</summary>
        [Key("limit")]
        public uint? limit { get; set; }
    }
}