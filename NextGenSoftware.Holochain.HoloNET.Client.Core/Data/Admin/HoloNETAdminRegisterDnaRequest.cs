
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.AppManifest;

namespace NextGenSoftware.Holochain.HoloNET.Client
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