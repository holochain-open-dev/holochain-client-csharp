
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HolonNETAppInfoResponse
    {
        [Key("type")]
        public string type { get; set; }

        [Key("data")]
        public HoloNETDataAppInfoCall data { get; set; }
    }
}
