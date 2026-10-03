
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

        // Adjacently-tagged CapAccess: build with CapAccess.Unrestricted/Transferable/Assigned.
        [Key("access")]
        public dynamic access { get; set; }

        // Adjacently-tagged GrantedFunctions: {"type": "all"} or {"type": "listed", "value": [[zome, fn], ...]}
        [Key("functions")]
        public Dictionary<string, object> functions { get; set; }
    }
}