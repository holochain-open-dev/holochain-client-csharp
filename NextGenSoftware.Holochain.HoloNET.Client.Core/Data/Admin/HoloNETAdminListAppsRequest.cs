
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETAdminListAppsRequest
    {
        [Key("status_filter")]
        public AppStatusFilter status_filter { get; set; }
    }
}