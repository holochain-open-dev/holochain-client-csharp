
using MessagePack;
using System;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client.Core
{
    [MessagePackObject]
    //[Serializable]
    public class HoloNETResponse
    {
        [Key("id")]
        public ulong id { get; set; }

        [Key("type")]
        public string type { get; set; }

        [Key("data")]
        public byte[] data { get; set; }
    }

    [MessagePackObject]
    public struct HolonNETAppResponse
    {
        [Key("type")]
        public string type { get; set; }

        [Key("data")]
        public byte[] data { get; set; }
    }

    //[MessagePackObject]
}
