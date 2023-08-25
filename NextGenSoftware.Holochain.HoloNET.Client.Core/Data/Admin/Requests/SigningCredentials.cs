
namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public class SigningCredentials
    {
        public byte[] CapSecret { get; set; }
        public KeyPair KeyPair { get; set; }
        public byte[] SigningKey { get; set; }
    }
}