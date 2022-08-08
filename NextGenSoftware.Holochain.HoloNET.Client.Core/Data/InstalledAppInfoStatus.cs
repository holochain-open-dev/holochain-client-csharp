using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    //public struct InstalledAppInfoStatus
    //{
    //    [Key("status")]
    //    public string status { get; set; }
      
    //    [Key("reason")]
    //    public string reason { get; set; }
    //}
    public struct InstalledAppInfoStatus
    {
        [Key("running")]
        public object running { get; set; }
    }
}