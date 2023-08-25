
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class HoloNETAdminGetAgentInfoRequest
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }
    }
}