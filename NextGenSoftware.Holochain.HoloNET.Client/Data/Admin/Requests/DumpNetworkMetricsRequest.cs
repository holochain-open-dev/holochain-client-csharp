using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests
{
    /// <summary>
    /// Mirrors AdminRequest::DumpNetworkMetrics { dna_hash: Option&lt;DnaHash&gt;, include_dht_summary: bool }
    /// (Holochain 0.7.0). Sending no payload is rejected with "Failed to deserialize request".
    /// </summary>
    [MessagePackObject]
    public class DumpNetworkMetricsRequest
    {
        /// <summary>Restrict to one DNA, or null for all DNAs.</summary>
        [Key("dna_hash")]
        public byte[] dna_hash { get; set; }

        [Key("include_dht_summary")]
        public bool include_dht_summary { get; set; }
    }
}
