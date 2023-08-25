
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETAdminDataCap
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }

        [Key("cap_grant")]
        public CapGrant cap_grant { get; set; }
    }
}