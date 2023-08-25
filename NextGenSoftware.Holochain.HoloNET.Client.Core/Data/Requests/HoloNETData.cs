
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETData
    {
        [Key("type")]
        public string type { get; set; }

        [Key("data")]
        public dynamic data { get; set; }
    }
}