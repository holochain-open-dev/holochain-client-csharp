﻿
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public struct CloneId
    {
        [Key("role_name")]
        public string role_name { get; set; }
    }
}