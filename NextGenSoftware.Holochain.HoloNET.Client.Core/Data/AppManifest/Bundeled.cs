
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.AppManifest
{
    [MessagePackObject]
    public struct Bundeled
    {
        [Key("modifiers")]
        public Modifiers modifiers { get; set; }

        [Key("installed_hash")]
        public object installed_hash { get; set; }

        [Key("_version")]
        public object _version { get; set; }

        [Key("clone_limit")]
        public object clone_limit { get; set; }
    }
}