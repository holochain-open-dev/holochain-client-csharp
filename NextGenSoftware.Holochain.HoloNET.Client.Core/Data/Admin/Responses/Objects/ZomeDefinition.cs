
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class ZomeDefinition
    {
        [Key("ZomeName")]
        public string ZomeName { get; set; }
        
        [Key("wasm_hash")]
        public byte[] wasm_hash { get; set; }

        [Key("dependencies")]
        public string[] dependencies { get; set; } //ZomeName[]
    }
}