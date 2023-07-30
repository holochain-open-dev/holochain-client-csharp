
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETResponse
    {
        [Key("id")]
        public ulong id { get; set; }

        [Key("type")]
        public string type { get; set; }

        [Key("data")]
        public byte[] data { get; set; }

        public bool IsError { get; set; }
    }
}
