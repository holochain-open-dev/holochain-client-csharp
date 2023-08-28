
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    [MessagePackObject]
    public class HoloNETAdminAddAgentInfoRequest
    {
        [Key("agent_infos")]
        public AgentInfo[] agent_infos { get; set; }
        //public AgentInfoInner[] agent_infos { get; set; }
    }
}

// https://github.com/holochain/holochain-client-js/blob/main/src/api/admin/types.ts
// export type AgentInfoSigned = unknown; //TODO: What is AgentInfoSigned? Need to ask...
// export type AddAgentInfoRequest = { agent_infos: Array<AgentInfoSigned> };


