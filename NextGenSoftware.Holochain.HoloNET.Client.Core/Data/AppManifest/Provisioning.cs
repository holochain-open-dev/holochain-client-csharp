
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.AppManifest
{
    [MessagePackObject]
    public struct Provisioning
    {
        [Key("strategy")]
        public string strategy { get; set; }

        [Key("deferred")]
        public string deferred { get; set; }
    }

    //public enum Provisioning
    //{
    //    strategy,
    //    deferred
    //}
}