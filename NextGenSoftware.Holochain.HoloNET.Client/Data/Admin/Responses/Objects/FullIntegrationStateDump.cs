
using MessagePack;
using System;
using static System.Net.WebRequestMethods;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class FullIntegrationStateDump
    {
        [Key("validation_limbo")]
        //public DhtOp[] validation_limbo { get; set; } // pre-0.7 op shape; 0.7 DhtOp is the ChainOp/WarrantOp enum
        public object[] validation_limbo { get; set; }

        [Key("integration_limbo")]
        //public DhtOp[] integration_limbo { get; set; } // pre-0.7 op shape; 0.7 DhtOp is the ChainOp/WarrantOp enum
        public object[] integration_limbo { get; set; }

        [Key("integrated")]
        //public DhtOp[] integrated { get; set; } // pre-0.7 op shape; 0.7 DhtOp is the ChainOp/WarrantOp enum
        public object[] integrated { get; set; }

        [Key("dht_ops_cursor")]
        //public UInt64 dht_ops_cursor { get; set; }
        //public UInt64? dht_ops_cursor { get; set; } // Option<DhtOpsCursor>
        /// <summary>Cursor for the next page (pass to DumpFullStateAsync), or null when this was the last page.</summary>
        public DhtOpsCursor dht_ops_cursor { get; set; }
    }
}