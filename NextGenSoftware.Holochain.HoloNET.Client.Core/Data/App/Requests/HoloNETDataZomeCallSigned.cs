using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETDataZomeCallSigned : HoloNETDataZomeCall
    {
        [Key("signature")]
        public byte[] signature { get; set; }
    }
}