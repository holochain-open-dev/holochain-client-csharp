
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class HoloNETAdminAddAgentInfoRequest
    {
        [Key("agent_infos")]
        public object agent_infos { get; set; }
    }
}

// https://github.com/holochain/holochain-client-js/blob/main/src/api/admin/types.ts
// export type AgentInfoSigned = unknown; //TODO: What is AgentInfoSigned? Need to ask...
// export type AddAgentInfoRequest = { agent_infos: Array<AgentInfoSigned> };


