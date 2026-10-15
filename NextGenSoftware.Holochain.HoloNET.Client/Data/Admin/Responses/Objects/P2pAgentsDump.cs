
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class P2pAgentsDump
    {
        [Key("this_agent_info")]
        public AgentInfoDump this_agent_info { get; set; }

        // Holochain 0.7 P2pAgentsDump { this_agent_info, this_dna: Option<(DnaHash, SpaceId)>, this_agent: Option<(AgentPubKey, AgentId)>, peers }.
        // this_dna and peers reused the keys "this_agent_info" / "this_agent", so the typed view never deserialised.
        //[Key("this_agent_info")]
        //public byte[] this_dna { get; set; } //Option<(DnaHash, KitsuneSpace)>
        //[Key("this_agent")]
        //public byte[] this_agent { get; set; } //Option<(AgentPubKey, KitsuneAgent)>,
        //[Key("this_agent")]
        //public AgentInfoDump[] peers { get; set; }

        /// <summary>[DnaHash bytes, kitsune space id (base64 string)], or null.</summary>
        [Key("this_dna")]
        public object[] this_dna { get; set; }

        /// <summary>[AgentPubKey bytes, kitsune agent id (base64 string)], or null.</summary>
        [Key("this_agent")]
        public object[] this_agent { get; set; }

        [Key("peers")]
        public AgentInfoDump[] peers { get; set; }
    }
}