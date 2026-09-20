using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors the Rust holochain_zome_types::cell::CellId tuple struct (Holochain 0.6.1):
    /// `pub struct CellId(DnaHash, AgentPubKey);`
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_zome_types/src/cell.rs
    /// A Rust tuple struct (de)serializes as a positional 2-element array, so this is
    /// MessagePack-encoded with positional integer keys (0 = dna_hash, 1 = agent_pub_key)
    /// rather than named map keys.
    /// </summary>
    [MessagePackObject]
    public class CellId
    {
        [Key(0)]
        public byte[] dna_hash { get; set; }

        [Key(1)]
        public byte[] agent_pub_key { get; set; }

        public CellId() { }

        public CellId(byte[] dnaHash, byte[] agentPubKey)
        {
            dna_hash = dnaHash;
            agent_pub_key = agentPubKey;
        }
    }
}