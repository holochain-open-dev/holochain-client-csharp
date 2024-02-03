
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class FullStateDumpedResponse
    {
        [Key("peer_dump")]
        public P2pAgentsDump peer_dump { get; set; }

        [Key("source_chain_dump")]
        public SourceChainJsonDump source_chain_dump { get; set; }

        [Key("integration_dump")]
        public FullIntegrationStateDump integration_dump { get; set; }
    }
}