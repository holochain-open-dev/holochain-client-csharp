
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class ListAppsResponse
    {
        [Key("type")]
        public string type { get; set; }

        // AdminRequest/AdminResponse/AppRequest/AppResponse are #[serde(tag = "type", content = "value")]
        // (holochain 0.6.1 and 0.7.0). "data" was the pre-0.4 content key and is ignored by the conductor.
        [Key("value")]
        public AppInfo[] Apps { get; set; }
    }
}