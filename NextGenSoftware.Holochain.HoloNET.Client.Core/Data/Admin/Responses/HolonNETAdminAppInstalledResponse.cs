
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin;
using System.Collections.Generic;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public struct HolonNETAdminAppInstalledResponse
    {
        [Key("installed_app_id?")]
        public string installed_app_id { get; set; }

        [Key("agent_pub_key")]
        public string agent_pub_key { get; set; }

        [Key("cell_info")]
        public Dictionary<string, List<CellInfoType>> cell_info { get; set; }

        [Key("status")]
        public string status { get; set; }
    }
}

//export type AppInfo = {
//  agent_pub_key: AgentPubKey;
//  installed_app_id: InstalledAppId;
//  cell_info: Record<RoleName, Array<CellInfo>>;
//  status: InstalledAppInfoStatus;
//};