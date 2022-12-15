
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETSignalResponse
    {
        //[Key("type")]
        //public string type { get; set; }

        //[Key("data")]
        //public HoloNETSignalData data { get; set; }

        [Key("App")]
        public byte[,] App { get; set; } = new byte[1,2];
        //public byte[][] App { get; set; } = new byte[2][];


        //[Key("App")]
        ////public byte[][] App { get; set; }
        ////public byte[] App { get; set; } //[[[], []], []]
        //public short[] App { get; set; } //[ [[], []], []]

        // public HoloNETSignalData[] HoloNETSignalData { get; set; }
    }
}