
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class EnableAppResponse
    {
        [Key("app")]
        public AppInfo App { get; set; }

        [Key("errors")]
        //public [byte[][], string] Errors { get; set; } //errors: Array<[CellId, string]>;
        public object Errors { get; set; } //errors: Array<[CellId, string]>; //TODO: Need to find out what this contains and the correct data structure.
    }
}