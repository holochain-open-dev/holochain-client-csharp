
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.App.Responses.Objects
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
    public struct AppInfoStatus
    {
        [Key("Paused")]
        public object Paused { get; set; }

        [Key("Disabled")]
        public object Disabled { get; set; }

        [Key("Running")]
        public object Running { get; set; }

        [IgnoreMember]
        public AppInfoStatusEnum Status
        {
            get
            {
                if (Paused != null)
                    return AppInfoStatusEnum.Paused;

                else if (Disabled != null)
                    return AppInfoStatusEnum.Disabled;

                else if (Running != null)
                    return AppInfoStatusEnum.Running;

                else
                    return AppInfoStatusEnum.None;
            }
        }
    }
}