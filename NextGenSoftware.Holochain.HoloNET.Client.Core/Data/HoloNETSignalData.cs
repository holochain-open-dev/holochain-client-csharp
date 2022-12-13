
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETSignalData
    {
        [Key("App")]
        public byte[] App { get; set; }
    }
}