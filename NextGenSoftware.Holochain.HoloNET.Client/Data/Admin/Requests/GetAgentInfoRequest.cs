
using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors AdminRequest::AgentInfo { dna_hashes: Option&lt;Vec&lt;DnaHash&gt;&gt; } (Holochain 0.7.0).
    /// Null returns agent info for every DNA.
    /// </summary>
    [MessagePackObject]
    public class GetAgentInfoRequest
    {
        [Key("dna_hashes")]
        public byte[][] dna_hashes { get; set; }

        // Pre-0.5 field; the conductor ignores it, so the filter was silently dropped.
        //[Key("cell_id")] public byte[][] cell_id { get; set; }
    }
}
