using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class ZomeCallSigned : ZomeCall
    {
        [Key("signature")]
        public byte[] signature { get; set; }
    }
}