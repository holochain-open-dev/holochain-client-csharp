
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.AppManifest
{
    [MessagePackObject]
    public struct DnaModifiers
    {
        [Key("network_seed")]
        public string network_seed { get; set; }

        [Key("properties")]
        public object properties { get; set; } //Could be object or byte[]?

        [Key("origin_time")]
        public string origin_time { get; set; } //RegisterDnaRequest doesn't need this.

        [Key("quantum_time")]
        public object quantum_time { get; set; } //RegisterDnaRequest doesn't need this.
    }
}