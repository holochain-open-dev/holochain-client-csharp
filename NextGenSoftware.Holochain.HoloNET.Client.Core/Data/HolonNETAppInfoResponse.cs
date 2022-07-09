
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Core
{
    [MessagePackObject]
    public struct HolonNETAppInfoResponse
    {
        [Key("type")]
        public string type { get; set; }

        [Key("data")]
        public HoloNETDataAppInfoCall data { get; set; }
    }
}
