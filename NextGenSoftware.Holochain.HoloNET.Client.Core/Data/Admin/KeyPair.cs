
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class KeyPair
    {
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKey { get; set; }
    }
}