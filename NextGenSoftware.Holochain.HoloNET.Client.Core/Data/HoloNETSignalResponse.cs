
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETSignalResponse
    {
        [Key("App")]
        public HoloNETSignalData App { get; set; }
    }
}