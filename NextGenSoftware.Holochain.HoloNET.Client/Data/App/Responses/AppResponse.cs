
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class AppResponse : IAppResponse
    {
        [Key("type")]
        public string type { get; set; }

        // AdminRequest/AdminResponse/AppRequest/AppResponse are #[serde(tag = "type", content = "value")]
        // (holochain 0.6.1 and 0.7.0). "data" was the pre-0.4 content key and is ignored by the conductor.
        [Key("value")]
        //public byte[] data { get; set; }
        public dynamic data { get; set; }
    }
}
