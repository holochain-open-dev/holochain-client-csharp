
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.AppManifest
{
    //public enum AppManifestType
    //{
    //    V1, //PausedAppReasonType
    //    Disabled,//DisabledAppReasonType
    //    Running
    //}

    [MessagePackObject]
    public struct AppManifest
    {
        [Key("manifest_version")]
        public string manifest_version { get; set; }

        [Key("name")]
        public string name { get; set; }

        [Key("description")]
        public string description { get; set; }

        [Key("roles")]
        //public Roles roles { get; set; }
        public object roles { get; set; }
    }
}