
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETAdminDumpStateRequest
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }
    }
}