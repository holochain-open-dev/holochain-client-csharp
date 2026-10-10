using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// The "value" of holochain_types::signal::Signal::App / AppDirect (Holochain 0.7.0).
    /// Signal is #[serde(tag = "type", content = "value", rename_all = "snake_case")]:
    ///   {"type": "app", "value": {"cell_id": [dna, agent], "zome_name": "...", "signal": bytes}}
    ///   {"type": "app_direct", "value": {"cell_id": [dna, agent], "signal": bytes}}
    ///   {"type": "system", "value": {...}}
    /// "signal" holds the zome's own msgpack-encoded payload.
    /// </summary>
    [MessagePackObject]
    public class SignalValue
    {
        /// <summary>CellId tuple: [0] = DnaHash, [1] = AgentPubKey.</summary>
        [Key("cell_id")]
        public byte[][] cell_id { get; set; }

        [Key("zome_name")]
        public string zome_name { get; set; }

        // AppSignal (ExternIO) is msgpack bin; AppDirect's Vec<u8> is an int array. The formatter reads both.
        [Key("signal")]
        [MessagePackFormatter(typeof(ByteArrayAsIntArrayFormatter))]
        public byte[] signal { get; set; }
    }
}
