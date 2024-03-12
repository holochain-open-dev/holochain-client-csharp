
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
   // [MessagePackObject]
    public struct Record
    {
        [Key("action_hashed")]
        public byte[] ActionHash { get; set; }

        [Key("entry")]
        public byte[] Entry { get; set; }
    }
}