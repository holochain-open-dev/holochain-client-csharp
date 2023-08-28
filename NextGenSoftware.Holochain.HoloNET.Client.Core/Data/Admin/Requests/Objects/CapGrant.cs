
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    [MessagePackObject]
    public class CapGrant
    {
        [Key("tag")]
        public string tag { get; set; }

        [Key("cap_grant")]
        public dynamic cap_grant { get; set; }

        [Key("functions")]
        public GrantedFunctions functions { get; set; }

        [Key("access")]
        public CapGrantAccess access { get; set; }
    }
}