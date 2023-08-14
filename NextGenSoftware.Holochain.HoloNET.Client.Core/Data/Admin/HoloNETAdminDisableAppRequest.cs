
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETAdminDisableAppRequest
    {
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }
    }
}