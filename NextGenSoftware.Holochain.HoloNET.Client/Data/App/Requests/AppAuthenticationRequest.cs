using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.App.Requests
{
    [MessagePackObject]
    public sealed class AppAuthenticationRequest
    {
        [Key("token")]
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
