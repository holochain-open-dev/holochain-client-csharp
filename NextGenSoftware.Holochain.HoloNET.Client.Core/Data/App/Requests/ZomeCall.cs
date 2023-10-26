using MessagePack;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class ZomeCall
    {
        [Key(0)]
        public byte[] provenance { get; set; } //AgentPubKey

        [Key(1)]
        //public (byte[], byte[]) cell_id { get; set; }
        //public CellId cell_id { get; set; }
        public byte[][] cell_id { get; set; }

        [Key(2)]
        public string zome_name { get; set; }

        [Key(3)]
        public string fn_name { get; set; }

        [Key(4)]
        public byte[] cap_secret { get; set; }

        [Key(5)]
        public byte[] payload { get; set; }

        [Key(6)]
        public byte[] nonce { get; set; }

        [Key(7)]
        public long expires_at { get; set; }

        //[Key("provenance")]
        //public byte[] provenance { get; set; } //AgentPubKey

        //[Key("cell_id")]
        //public byte[][] cell_id { get; set; }

        //[Key("zome_name")]
        //public string zome_name { get; set; }

        //[Key("fn_name")]
        //public string fn_name { get; set; }

        //[Key("cap_secret")]
        //public byte[] cap_secret { get; set; }

        //[Key("payload")]
        //public byte[] payload { get; set; } 

        //[Key("nonce")]
        //public byte[] nonce { get; set; }

        //[Key("expires_at")]
        //public long expires_at { get; set; }
    }
}