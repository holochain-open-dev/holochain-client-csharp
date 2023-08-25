
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class HoloNETAdminRegisterDnaRequest
    {
        [Key("modifiers")]
        public DnaModifiers modifiers { get; set; }

        [Key("DnaSource")]
        public string DnaSource { get; set; }
    }
}