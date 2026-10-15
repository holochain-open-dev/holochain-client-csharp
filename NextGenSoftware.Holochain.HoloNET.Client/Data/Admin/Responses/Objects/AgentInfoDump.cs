
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [MessagePackObject]
    public class AgentInfoDump
    {
        // Holochain 0.7: kitsune2 AgentId / SpaceId serialise as base64 strings; kitsune_space reused the "kitsune_agent" key.
        //[Key("kitsune_agent")]
        //public KitsuneAgent kitsune_agent { get; set; }

        //[Key("kitsune_agent")]
        //public KitsuneSpace kitsune_space { get; set; }

        [Key("kitsune_agent")]
        public string kitsune_agent { get; set; }

        [Key("kitsune_space")]
        public string kitsune_space { get; set; }

        [Key("dump")]
        public string dump { get; set; }
    }
}