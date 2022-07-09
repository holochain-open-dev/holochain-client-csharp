using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Core
{
    [MessagePackObject]
    public struct HoloNETDataZomeCall
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }
      
        [Key("zome_name")]
        public string zome_name { get; set; }

        [Key("fn_name")]
        public string fn_name { get; set; }

        [Key("payload")]
        public byte[] payload { get; set; } 

        [Key("cap")]
        public byte[] cap { get; set; } //CapSecret | null = string

        [Key("provenance")]
        public byte[] provenance { get; set; } //AgentPubKey = string
    }
}