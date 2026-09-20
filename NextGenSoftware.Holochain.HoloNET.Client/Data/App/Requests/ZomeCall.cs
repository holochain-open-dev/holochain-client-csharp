using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors the Rust holochain_zome_types::zome_io::ZomeCallParams struct (Holochain 0.6.1):
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_zome_types/src/zome_io.rs
    ///
    /// pub struct ZomeCallParams {
    ///     pub provenance: AgentPubKey,
    ///     pub cell_id: CellId,
    ///     pub zome_name: ZomeName,
    ///     pub fn_name: FunctionName,
    ///     pub cap_secret: Option&lt;CapSecret&gt;,
    ///     pub payload: ExternIO,
    ///     pub nonce: Nonce256Bits,
    ///     pub expires_at: Timestamp,
    /// }
    ///
    /// CellId itself is a Rust tuple struct `CellId(DnaHash, AgentPubKey)`, which (de)serializes
    /// as a 2-element array, hence the use of the dedicated <see cref="CellId"/> type below
    /// (which is itself MessagePack-encoded with positional keys 0/1 to match the tuple shape)
    /// rather than the previous flattened byte[][] / split dna_hash+agent_pub_key fields.
    /// </summary>
    [MessagePackObject]
    public class ZomeCall
    {
        [Key("provenance")]
        public byte[] provenance { get; set; } //AgentPubKey

        [Key("cell_id")]
        public CellId cell_id { get; set; }

        [Key("zome_name")]
        public string zome_name { get; set; }

        [Key("fn_name")]
        public string fn_name { get; set; }

        [Key("cap_secret")]
        public byte[] cap_secret { get; set; }

        [Key("payload")]
        public byte[] payload { get; set; }

        [Key("nonce")]
        public byte[] nonce { get; set; }

        [Key("expires_at")]
        public long expires_at { get; set; }
    }
}