
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETAdminAttachAppInterfaceRequest
    {
        [Key("port")]
        public int port { get; set; }
    }
}