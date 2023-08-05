using MessagePack;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETDataAppInfoCall
    {
        [Key("installed_app_id")]
        public string installed_app_id { get; set; }

        [Key("cell_data")]
        //public InstalledCell[] cell_data { get; set; }
        public Dictionary<string, List<CellInfoType>> cell_data { get; set; }

        [Key("agent_pub_key")]
        public byte[] agent_pub_key { get; set; }

        [Key("status")]
        public InstalledAppInfoStatusType status { get; set; }

        [Key("manifest")]
        public AppManifestType manifest { get; set; }
}