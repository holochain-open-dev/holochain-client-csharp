
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    //[MessagePackObject]
    //public enum InstalledAppInfoStatusType
    //{
    //    Paused, //PausedAppReasonType
    //    Disabled,//DisabledAppReasonType
    //    Running
    //}


    //public struct InstalledAppInfoStatus
    //{
    //    [Key("status")]
    //    public string status { get; set; }

    //    [Key("reason")]
    //    public string reason { get; set; }
    //}

    [MessagePackObject]
    public struct InstalledAppInfoStatusType
    {
        [Key("running")]
        public object running { get; set; }
    }
}