
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.AppManifest
{
    [MessagePackObject]
    public struct Modifiers
    {
        [Key("network_seed")]
        public string network_seed { get; set; }

        [Key("properties")]
        public string properties { get; set; }

        [Key("origin_time")]
        public string origin_time { get; set; }

        [Key("quantum_time")]
        public object quantum_time { get; set; }
    }
}