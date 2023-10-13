
using MessagePack;
using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class AttachAppInterfaceResponse
    {
        [Key("port")]
        public UInt16 Port { get; set; }
    }
}