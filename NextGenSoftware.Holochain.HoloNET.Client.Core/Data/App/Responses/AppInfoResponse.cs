
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class AppInfoResponse
    {
        [Key("type")]
        public string type { get; set; }

        [Key("data")]
        public AppInfo data { get; set; }
    }
}
