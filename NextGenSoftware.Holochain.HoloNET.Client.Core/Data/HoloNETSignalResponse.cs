
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETSignalResponse
    {
        [Key("App")]
        public byte[][] App { get; set; }
       // public HoloNETSignalData[] HoloNETSignalData { get; set; }
    }
}