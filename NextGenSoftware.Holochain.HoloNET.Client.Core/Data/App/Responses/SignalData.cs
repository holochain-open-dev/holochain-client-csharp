
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class SignalData
    {
        [Key(0)]
        public byte[][] CellData { get; set; }

        [Key(1)]
        public byte[] Data { get; set; }
    }
}