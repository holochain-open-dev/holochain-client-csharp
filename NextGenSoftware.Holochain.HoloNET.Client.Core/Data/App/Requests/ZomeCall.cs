using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class ZomeCall
    {
        [Key("provenance")]
        public byte[] provenance { get; set; } //AgentPubKey = string

        [Key("cell_id")]
        public byte[][] cell_id { get; set; }
      
        [Key("zome_name")]
        public string zome_name { get; set; }

        [Key("fn_name")]
        public string fn_name { get; set; }

        [Key("cap_secret")]
        public byte[] cap_secret { get; set; }

        [Key("payload")]
        public byte[] payload { get; set; } 

        [Key("nonce")]
        public byte[] nonce { get; set; }

        [Key("expires_at")]
        public long expires_at { get; set; }
    }
}