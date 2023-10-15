
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    [MessagePackObject]
    public class CapGrantAccessTransferable
    {
        [Key("secret")]
        public byte[] secret { get; set; }
    }
}