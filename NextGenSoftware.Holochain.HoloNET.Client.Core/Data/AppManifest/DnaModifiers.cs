
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.AppManifest
{
    [MessagePackObject]
    public class DnaModifiers
    {
        [Key("network_seed")]
        public string network_seed { get; set; }

        [Key("properties")]
        public object properties { get; set; } //Could be object or byte[]?

        [Key("origin_time")]
        public long origin_time { get; set; } //RegisterDnaRequest doesn't need this.

        [IgnoreMember]
        public DateTime OriginTime { get; set; } //RegisterDnaRequest doesn't need this.

        [Key("quantum_time")]
        public RustDuration quantum_time { get; set; } //RegisterDnaRequest doesn't need this.
        //public object quantum_time { get; set; } //RegisterDnaRequest doesn't need this.
    }
}