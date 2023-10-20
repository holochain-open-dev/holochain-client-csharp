using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using NextGenSoftware.Holochain.HoloNET.Client.Data.App.Responses.Objects;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class AppInfo
    {
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("cell_info")]
        public Dictionary<string, List<CellInfo>> cell_info { get; set; }

        [Key("agent_pub_key")]
        public byte[] agent_pub_key { get; set; }

        [IgnoreMember]
        public string AgentPubKey { get; set; }

        [IgnoreMember]
        public string DnaHash { get; set; }

        [IgnoreMember]
        public byte[][] CellId { get; set; }

        [Key("status")]
        public AppInfoStatus status { get; set; }

        [Key("manifest")]
        public AppManifest manifest { get; set; }
    }
}