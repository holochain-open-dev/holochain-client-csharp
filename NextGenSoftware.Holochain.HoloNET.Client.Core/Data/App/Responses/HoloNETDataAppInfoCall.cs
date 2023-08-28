using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.AppManifest;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETDataAppInfoCall
    {
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("cell_info")]
        public Dictionary<string, List<CellInfoType>> cell_info { get; set; }
        //public object cell_info { get; set; }

        [Key("agent_pub_key")]
        public byte[] agent_pub_key { get; set; }

        [Key("status")]
        public InstalledAppInfoStatusType status { get; set; }
        //public object status { get; set; }

        [Key("manifest")]
        public AppManifest manifest { get; set; }
    }
}