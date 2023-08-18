﻿
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin
{
    [MessagePackObject]
    public struct RustDuration
    {
        [Key("secs")]
        public int secs { get; set; }

        [Key("nanos")]
        public int nanos { get; set; }
    }
}