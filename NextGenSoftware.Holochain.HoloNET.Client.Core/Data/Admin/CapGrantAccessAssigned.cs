
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class CapGrantAccessAssigned
    {
        [Key("secret")]
        public byte[] secret { get; set; }

        [Key("assignees")]
        public byte[] assignees { get; set; }
    }
}