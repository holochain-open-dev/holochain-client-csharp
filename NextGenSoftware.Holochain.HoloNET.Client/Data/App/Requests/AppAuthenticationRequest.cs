using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.App.Requests
{
    [MessagePackObject]
    public sealed class AppAuthenticationRequest
    {
        // Rust Vec<u8> without serde_bytes: an array of integers on the wire (confirmed live, 0.7.0).
        [Key("token")]
        [MessagePackFormatter(typeof(ByteArrayAsIntArrayFormatter))]
        public byte[] Token { get; set; }
    }

    [MessagePackObject]
    public sealed class AppAuthenticationEnvelope
    {
        [Key("type")]
        public string Type { get; set; } = "authenticate";

        [Key("data")]
        public byte[] Data { get; set; }
    }
}
