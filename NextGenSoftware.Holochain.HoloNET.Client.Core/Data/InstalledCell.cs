using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Core
{
    [MessagePackObject]
    public struct InstalledCell
    {
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }
      
        [Key("role_id")]
        public string role_id { get; set; }
    }
}