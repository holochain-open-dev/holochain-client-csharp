
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class HoloNETAdminAttachAppInterfaceRequest
    {
        [Key("port")]
        public int port { get; set; }
    }
}