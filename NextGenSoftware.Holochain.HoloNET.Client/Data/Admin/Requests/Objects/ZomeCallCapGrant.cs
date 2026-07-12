
using MessagePack;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    [MessagePackObject]
    public class ZomeCallCapGrant
    {
        [Key("tag")]
        public string tag { get; set; }

        //[Key("cap_grant")]
        //public dynamic cap_grant { get; set; }

        [Key("access")]
        //public CapGrantAccess access { get; set; }
        public dynamic access { get; set; }

        // Wire format: {"All": null} or {"Listed": [[zome, fn], ...]}
        // String keys are required — the conductor rejects integer enum keys.
        [Key("functions")]
        public Dictionary<string, object> functions { get; set; }
    }
}